using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace StarCloudgamesLibrary
{
    public class IAPManager : Singleton<IAPManager>
    {
        private StoreController storeController;
        private bool eventsRegistered;
        private bool initialized;
        private bool initializing;

        // 테이블 / 매핑
        private readonly Dictionary<int, IAPDataTable> iapById = new();
        private readonly Dictionary<string, int> iapIdByProductId = new();
        private readonly Dictionary<int, string> productIdByIapId = new();

        // 런타임 상태
        private readonly Dictionary<int, Action<bool, string>> pendingCallbacks = new();
        private readonly Dictionary<string, Product> productById = new();
        private readonly HashSet<int> purchasingIds = new();

        public event Action<IAPDataTable> OnPurchaseSucceeded;
        public event Action<IAPDataTable, PurchaseFailureReason, string> OnPurchaseFailedEvent;
        public event Action ProductsUpdated;

        public bool Initialized() => initialized;

        #region Initialize & Lifecycle

        public override async UniTask Initialize()
        {
            await EnsureInitializedAsync();
        }

        /// <summary>
        /// 어디서 호출해도 한 번만 초기화되도록 보장.
        /// Purchase에서도 이걸 호출해서 자동 초기화 진행.
        /// </summary>
        private async UniTask EnsureInitializedAsync()
        {
            if (initialized)
                return;

            // 이미 누군가 초기화 진행 중이면 그게 끝날 때까지 기다림
            if (initializing)
            {
                await UniTask.WaitUntil(this, (self) => self.initialized || !self.initializing);
                return;
            }

            initializing = true;
            try
            {
                var dataTableManager = DataTableManager.Instance ?? DataTableManager.Create();

                // IAP 테이블 로드 대기
                IReadOnlyList<IAPDataTable> allIapTables = null;
                while (allIapTables == null)
                {
                    allIapTables = dataTableManager.GetAllIAPDataTables();
                    if (allIapTables == null)
                        await UniTask.NextFrame();
                }

#if UNITY_EDITOR
                // 에디터에서는 항상 FakeStore 사용
                UnityIAPServices.SetStoreAsDefault("fake");
#endif

                // UnityServices 초기화 (필요 시)
                try
                {
                    if (UnityServices.State != ServicesInitializationState.Initialized &&
                        UnityServices.State != ServicesInitializationState.Initializing)
                    {
                        await UnityServices.InitializeAsync();
                    }
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"IAPManager: UnityServices.InitializeAsync failed in editor, continue with FakeStore: {e}");
#else
                    Debug.LogError($"IAPManager: UnityServices.InitializeAsync failed: {e}");
                    return;
#endif
                }

                storeController = UnityIAPServices.StoreController();
                RegisterStoreEvents();

                try
                {
                    await storeController.Connect();
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"IAPManager: StoreController.Connect failed in editor: {e}");
#else
                    Debug.LogError($"IAPManager: StoreController.Connect failed: {e}");
#endif
                    return;
                }

                // 테이블 → ProductDefinition 빌드
                iapById.Clear();
                iapIdByProductId.Clear();
                productIdByIapId.Clear();
                productById.Clear();
                pendingCallbacks.Clear();
                purchasingIds.Clear();

                var productDefinitions = new List<ProductDefinition>();

                foreach (var iap in allIapTables)
                {
                    if (iap == null) continue;

                    string productId;
#if UNITY_IOS
                    productId = string.IsNullOrEmpty(iap.storeId_ios) ? iap.storeId_aos : iap.storeId_ios;
#else
                    productId = string.IsNullOrEmpty(iap.storeId_aos) ? iap.storeId_ios : iap.storeId_aos;
#endif
                    productId = productId?.Trim();

#if UNITY_EDITOR
                    // 에디터에서 ID가 비어 있으면 더미 ID라도 만들어서 FakeStore 테스트 가능하게
                    if (string.IsNullOrEmpty(productId))
                        productId = $"editor_iap_{iap.id}";
#endif

                    if (string.IsNullOrEmpty(productId))
                        continue;

                    var productType = iap.consumable ? ProductType.Consumable : ProductType.NonConsumable;
                    var def = new ProductDefinition(productId, productId, productType);
                    productDefinitions.Add(def);

                    iapById[iap.id] = iap;
                    productIdByIapId[iap.id] = productId;

                    if (!iapIdByProductId.ContainsKey(productId))
                        iapIdByProductId[productId] = iap.id;
                }

                if (productDefinitions.Count == 0)
                {
                    Debug.LogWarning("IAPManager: No productDefinitions created. IAP will be initialized but all purchases will fail.");
                    initialized = true;
                    return;
                }

                storeController.FetchProducts(productDefinitions);

                // OnPurchasesFetched / OnPurchasesFetchFailed / OnProductsFetchFailed 에서 initialized=true 세팅
                await UniTask.WaitUntil(this, (self) => self.initialized);
            }
            finally
            {
                initializing = false;
            }
        }

        private void OnDestroy()
        {
            UnregisterStoreEvents();
            storeController = null;
            productById.Clear();
            pendingCallbacks.Clear();
            purchasingIds.Clear();
            iapById.Clear();
            iapIdByProductId.Clear();
            productIdByIapId.Clear();
            initialized = false;
            initializing = false;
            eventsRegistered = false;
        }

        #endregion

        #region Public API

        public void Purchase(int iapId, Action<bool, string> onComplete = null)
        {
            // 비동기로 내부 처리
            PurchaseInternal(iapId, onComplete).Forget();
        }

        private async UniTask PurchaseInternal(int iapId, Action<bool, string> onComplete)
        {
            // 여기서 자동 초기화
            await EnsureInitializedAsync();

            if (!initialized)
            {
                onComplete?.Invoke(false, "IAP initialization failed");
                return;
            }

            if (purchasingIds.Contains(iapId))
            {
                onComplete?.Invoke(false, "Purchase already in progress");
                return;
            }

            if (!iapById.TryGetValue(iapId, out var iapData))
            {
                iapData = DataTableManager.Instance.GetIAPDataTable(iapId);
                if (iapData == null)
                {
                    onComplete?.Invoke(false, "IAP data not found");
                    return;
                }

                iapById[iapId] = iapData;
            }

            if (!productIdByIapId.TryGetValue(iapId, out var productId) || string.IsNullOrEmpty(productId))
            {
                onComplete?.Invoke(false, "ProductId not configured");
                return;
            }

            Product product = null;
            if (!productById.TryGetValue(productId, out product))
            {
                var products = storeController?.GetProducts();
                if (products != null && products.Count > 0)
                    product = products.FirstOrDefault(p => p.definition.id == productId);
            }

            if (product == null)
            {
                Debug.LogError($"IAPManager: Product not found for productId='{productId}' (iapId={iapId})");
                onComplete?.Invoke(false, "Product not found");
                return;
            }

            pendingCallbacks[iapId] = onComplete;
            purchasingIds.Add(iapId);
            storeController.PurchaseProduct(product);
        }

        public void RestorePurchases(Action<bool, string> onComplete = null)
        {
            if (storeController == null)
            {
                onComplete?.Invoke(false, "StoreController not initialized");
                return;
            }

#if UNITY_IOS
            storeController.RestoreTransactions((success, error) =>
            {
                onComplete?.Invoke(success, error);
            });
#else
            onComplete?.Invoke(false, "Restore not supported on this platform");
#endif
        }

        public ProductMetadata GetProductMetadataByIapId(int iapId)
        {
            // Initialize가 아직 안 됐을 수도 있으므로, 여기선 단순 조회만 하고 실패하면 null
            if (!productIdByIapId.TryGetValue(iapId, out var productId) || string.IsNullOrEmpty(productId))
            {
                if (!iapById.TryGetValue(iapId, out var iapData))
                {
                    iapData = DataTableManager.Instance.GetIAPDataTable(iapId);
                    if (iapData == null) return null;
                    iapById[iapId] = iapData;
                }

#if UNITY_IOS
                productId = string.IsNullOrEmpty(iapData.storeId_ios) ? iapData.storeId_aos : iapData.storeId_ios;
#else
                productId = string.IsNullOrEmpty(iapData.storeId_aos) ? iapData.storeId_ios : iapData.storeId_aos;
#endif
                productId = productId?.Trim();
                if (string.IsNullOrEmpty(productId)) return null;
            }

            if (productById.TryGetValue(productId, out var product))
                return product.metadata;

            var products = storeController?.GetProducts();
            var fallback = products?.FirstOrDefault(p => p.definition.id == productId);
            return fallback?.metadata;
        }

        #endregion

        #region StoreController Events

        private void OnPurchaseConfirmed(Order order)
        {
            // 필요하면 로그만
            Debug.Log($"IAPManager: Purchase confirmed. orderId={order.Info.TransactionID}, product={GetProductIdFromCart(order?.CartOrdered)}");
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription desc)
        {
            Debug.LogWarning($"IAPManager: Store disconnected: {desc.Message}");
            UnregisterStoreEvents();
            initialized = false;
        }

        private void OnProductsFetched(List<Product> products)
        {
            productById.Clear();

            if (products != null)
            {
                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    var id = p?.definition?.id;
                    if (!string.IsNullOrEmpty(id))
                    {
                        id = id.Trim();
                        productById[id] = p;
                    }
                }
            }

            // 기존 구매 내역 동기화
            storeController.FetchPurchases();

            ProductsUpdated?.Invoke();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failed)
        {
            Debug.LogError($"IAPManager: Products fetch failed: {failed.FailureReason}");
            // 더 이상 기다리지 않고 initialized=true 로 만들어준다.
            initialized = true;

            ProductsUpdated?.Invoke();
        }

        private void OnPurchasesFetched(Orders orders)
        {
            initialized = true;
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failed)
        {
            Debug.LogWarning($"IAPManager: Purchases fetch failed: {failed.FailureReason} {failed.Message}");
            initialized = true;
        }

        private void OnPurchasePending(PendingOrder order)
        {
            ValidateAndFinalizePurchase(order);
        }

        private void OnPurchaseFailedInternal(FailedOrder order)
        {
            var productId = GetProductIdFromCart(order.CartOrdered);
            int iapId = -1;
            if (!string.IsNullOrEmpty(productId))
            {
                productId = productId.Trim();
                if (iapIdByProductId.TryGetValue(productId, out var mappedId))
                    iapId = mappedId;
            }

            IAPDataTable data = (iapId != -1 && iapById.TryGetValue(iapId, out var found)) ? found : null;

            var reason = order.FailureReason;
            var message = order.Details;

            OnPurchaseFailedEvent?.Invoke(data, reason, message);

            if (iapId != -1 && pendingCallbacks.TryGetValue(iapId, out var cb))
            {
                cb?.Invoke(false, string.IsNullOrEmpty(message) ? reason.ToString() : message);
                pendingCallbacks.Remove(iapId);
            }
            if (iapId != -1) purchasingIds.Remove(iapId);
        }

        #endregion

        #region Purchase Flow / Validation

        private void ValidateAndFinalizePurchase(PendingOrder order)
        {
            var productId = GetProductIdFromCart(order.CartOrdered);
            int iapId = -1;
            if (!string.IsNullOrEmpty(productId))
            {
                productId = productId.Trim();
                if (iapIdByProductId.TryGetValue(productId, out var mappedId))
                    iapId = mappedId;
            }

#if UNITY_EDITOR
            // 에디터에서는 영수증 검증 없이 바로 성공 처리 (FakeStore)
            ConfirmAndNotifySuccess(order, iapId);
            return;
#else
            bool valid = false;
            string error = null;
#if UNITY_ANDROID
            try
            {
                var receipt = order.Info?.Receipt;
                if (string.IsNullOrEmpty(receipt))
                {
                    error = "Empty receipt";
                }
                else
                {
                    var isValid = TryValidateReceiptWithReflection(receipt, out var validateError);
                    valid = isValid;
                    error = validateError;
                }
            }
            catch (Exception ex)
            {
                valid = false;
                error = ex.Message;
            }
#else
            valid = true;
#endif
            if (valid)
            {
                ConfirmAndNotifySuccess(order, iapId);
            }
            else
            {
                NotifyFailure(iapId, PurchaseFailureReason.SignatureInvalid, error);
                storeController?.ConfirmPurchase(order);
            }
#endif
        }

        private void ConfirmAndNotifySuccess(PendingOrder order, int iapId)
        {
            IAPDataTable data = (iapId != -1 && iapById.TryGetValue(iapId, out var found)) ? found : null;
            OnPurchaseSucceeded?.Invoke(data);
            storeController?.ConfirmPurchase(order);

            if (iapId != -1 && pendingCallbacks.TryGetValue(iapId, out var cb))
            {
                cb?.Invoke(true, null);
                pendingCallbacks.Remove(iapId);
            }
            if (iapId != -1) purchasingIds.Remove(iapId);
        }

        private void NotifyFailure(int iapId, PurchaseFailureReason reason, string message)
        {
            IAPDataTable data = (iapId != -1 && iapById.TryGetValue(iapId, out var found)) ? found : null;
            OnPurchaseFailedEvent?.Invoke(data, reason, message);

            if (iapId != -1 && pendingCallbacks.TryGetValue(iapId, out var cb))
            {
                cb?.Invoke(false, string.IsNullOrEmpty(message) ? reason.ToString() : message);
                pendingCallbacks.Remove(iapId);
            }
            if (iapId != -1) purchasingIds.Remove(iapId);
        }

        #endregion

        #region Helpers

        private void RegisterStoreEvents()
        {
            if (storeController == null || eventsRegistered) return;

            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseFailed += OnPurchaseFailedInternal;
            storeController.OnPurchasesFetched += OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

            eventsRegistered = true;
        }

        private void UnregisterStoreEvents()
        {
            if (storeController == null || !eventsRegistered) return;

            storeController.OnStoreDisconnected -= OnStoreDisconnected;
            storeController.OnProductsFetched -= OnProductsFetched;
            storeController.OnProductsFetchFailed -= OnProductsFetchFailed;
            storeController.OnPurchasePending -= OnPurchasePending;
            storeController.OnPurchaseFailed -= OnPurchaseFailedInternal;
            storeController.OnPurchasesFetched -= OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
            storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;

            eventsRegistered = false;
            storeController = null;
        }

        private string GetProductIdFromCart(ICart cart)
        {
            if (cart == null) return null;
            var items = cart.Items();
            if (items == null || items.Count == 0) return null;
            var product = items[0]?.Product;
            var id = product?.definition?.id;
            return id?.Trim();
        }

        private bool TryValidateReceiptWithReflection(string receipt, out string error)
        {
            error = null;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var crossType = assemblies
                    .Select(a => a.GetType("UnityEngine.Purchasing.Security.CrossPlatformValidator"))
                    .FirstOrDefault(t => t != null);

                if (crossType == null)
                {
                    error = "CrossPlatformValidator not found";
                    return false;
                }

                var googleTangle = assemblies
                    .Select(a => a.GetType("UnityEngine.Purchasing.Security.GooglePlayTangle"))
                    .FirstOrDefault(t => t != null);

                if (googleTangle == null)
                {
                    error = "GooglePlayTangle not found";
                    return false;
                }

                var gm = googleTangle.GetMethod("Data", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var googleData = gm?.Invoke(null, null) as byte[];

                var ctor = crossType.GetConstructor(new[] { typeof(byte[]), typeof(byte[]), typeof(string) });
                if (ctor == null)
                {
                    error = "CrossPlatformValidator ctor not found";
                    return false;
                }

                var validator = ctor.Invoke(new object[] { googleData, null, Application.identifier });
                var validateMethod = crossType.GetMethod("Validate", new[] { typeof(string) });
                if (validateMethod == null)
                {
                    error = "Validate method not found";
                    return false;
                }

                try
                {
                    _ = validateMethod.Invoke(validator, new object[] { receipt });
                    return true;
                }
                catch (TargetInvocationException tie)
                {
                    var ex = tie.InnerException ?? tie;
                    error = ex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
#else
            return true;
#endif
        }

        #endregion
    }
}
