using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemRewardItem : CachedMonoBehaviour
{
    [SerializeField] private TMP_Text itemCountText;
    [SerializeField] private SwapperBehaviour swapper;
    [SerializeField] private Image icon;

    public void SetUp(RewardData rewardData)
    {
        CachedTransform.localScale = Vector3.zero;
        
        icon.SetSprite(AtlasType.UI_Main, ZString.Concat(rewardData.AssetType));
        itemCountText.text = ZString.Concat(rewardData.Amount);
        swapper.Swap(GetSwapType(rewardData.AssetType));
    }

    private ISwapper.SwapType GetSwapType(DataTableEnum.AssetType assetType)
    {
        switch (assetType)
        {
            case DataTableEnum.AssetType.Gold:
                return ISwapper.SwapType.SwapType0;
        }
        
        return ISwapper.SwapType.None;
    }

    public async UniTask StartProduce()
    {
        await LMotion.Create(Vector3.zero, Vector3.one, 0.3f).WithEase(Ease.OutBack).BindToLocalScale(CachedTransform).ToUniTask();
    }
}