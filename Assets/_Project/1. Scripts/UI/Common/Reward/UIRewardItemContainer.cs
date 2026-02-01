using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIRewardItemContainer : MonoBehaviour
{
    [SerializeField] private Transform container;

    private readonly List<UIItemRewardItem> pooledItems = new();
    private GameObject cachedPrefab;

    public async UniTask SetUpContainer(List<RewardData> rewardDatas)
    {
        // 기존 아이템 비활성화
        foreach (var item in pooledItems)
        {
            item.gameObject.SetActive(false);
        }

        // 프리팹 캐싱
        if (cachedPrefab == null)
        {
            var handle = await Addressables.LoadAssetAsync<GameObject>(AddressableExtensions.UIRewardItemPath);
            cachedPrefab = handle;
        }

        for (var i = 0; i < rewardDatas.Count; i++)
        {
            UIItemRewardItem item;
            
            if (i < pooledItems.Count)
            {
                // 풀에서 재사용
                item = pooledItems[i];
                item.gameObject.SetActive(true);
            }
            else
            {
                // 새로 생성하고 풀에 추가
                item = Instantiate(cachedPrefab, container).GetComponent<UIItemRewardItem>();
                pooledItems.Add(item);
            }
            
            item.SetUp(rewardDatas[i]);
            await item.StartProduce();
        }
    }

    public void ClearContainer()
    {
        foreach (var item in pooledItems)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (cachedPrefab != null)
        {
            Addressables.Release(cachedPrefab);
            cachedPrefab = null;
        }
    }
}
