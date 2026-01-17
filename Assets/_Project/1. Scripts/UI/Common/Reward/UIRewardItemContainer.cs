using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIRewardItemContainer : MonoBehaviour
{
    [SerializeField] private Transform container;

    public async UniTask SetUpContainer(List<RewardData> rewardDatas)
    {
        var handle = await Addressables.LoadAssetAsync<GameObject>(AddressableExtensions.UIRewardItemPath);

        foreach (var rewardData in rewardDatas)
        {
            var item = Instantiate(handle, container).GetComponent<UIItemRewardItem>();
            item.SetUp(rewardData);
            await item.StartProduce();
        }
        
        Addressables.Release(handle);
    }
}
