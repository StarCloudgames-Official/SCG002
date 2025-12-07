using StarCloudgamesLibrary;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ExtensionButton))]
public class IAPButtonExtension : MonoBehaviour
{
    [SerializeField] private int iapId;
    [SerializeField] private TextMeshProUGUI priceText;

    private ExtensionButton extensionButton;
    
    private void Awake()
    {
        InitializeButton();
    }

    private void OnEnable()
    {
        var manager = IAPManager.Instance;
        if (manager != null)
        {
            manager.ProductsUpdated += InitializePriceText;
        }

        InitializePriceText();
    }

    private void OnDisable()
    {
        var manager = IAPManager.Instance;
        if (manager != null)
        {
            manager.ProductsUpdated -= InitializePriceText;
        }
    }

    private void InitializePriceText()
    {
        var metaData = IAPManager.Instance.GetProductMetadataByIapId(iapId);
        if (metaData == null) return;
        
        priceText.text = metaData.localizedPriceString;
    }

    private void InitializeButton()
    {
        extensionButton = GetComponent<ExtensionButton>();
        extensionButton.AddOnClickListener(OnClickPurchase); 
    }

    public async void OnClickPurchase()
    {
        var uiLoadingOverPopup = await UIManager.OpenUI<UILoadingOverPopup>();
        
        IAPManager.Instance.Purchase(iapId, (purchaseSuccess, failReason) =>
        {
            uiLoadingOverPopup.Close().Forget();
            
            if (purchaseSuccess)
            {
                PurchaseSuccess().Forget();
            }
            else
            {
                Debug.Log(failReason);
            }
        });
    }

    private async Awaitable PurchaseSuccess()
    {
        var uiIAPPurchaseSuccessPopup = await UIManager.OpenUI<UIIAPPurchaseSuccessPopup>();
        await AwaitableExtensions.WhileAlive(uiIAPPurchaseSuccessPopup);
        
        var rewards = DataTableManager.Instance.GetIAPDataTable(iapId).RewardGroupData;
        DatabaseManager.Instance.AddRewardGroups(rewards);
        //TODO : 연출?
    }
}
