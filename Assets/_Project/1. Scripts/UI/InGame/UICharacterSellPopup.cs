using Cysharp.Threading.Tasks;
using UnityEngine;

public class UICharacterSellPopup : MonoBehaviour
{
    [SerializeField] private UICharacterSellItem[] sellItems;
    [SerializeField] private ListSwapper[] tapSwappers;

    public async UniTask Set(DataTableEnum.ClassType classType = DataTableEnum.ClassType.Warrior)
    {
        for (var i = 0; i < sellItems.Length; i++)
        {
            var targetSpawnType = (DataTableEnum.SpawnType)i + 1;
            sellItems[i].Set(classType, targetSpawnType);
        }

        var selectedTapIndex = (int)classType - 1;
        for (var i = 0; i < tapSwappers.Length; i++)
        {
            tapSwappers[i].Swap(i == selectedTapIndex ? ISwapper.SwapType.SwapType1 : ISwapper.SwapType.SwapType0);
        }

        await UniTask.NextFrame();
    }

    public void OnClickTap(int classType)
    {
        var targetClass = (DataTableEnum.ClassType)classType;
        Set(targetClass).Forget();
    }
}
