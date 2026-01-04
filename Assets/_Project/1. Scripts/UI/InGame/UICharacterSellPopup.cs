using System.Collections.Generic;
using UnityEngine;

public class UICharacterSellPopup : MonoBehaviour
{
    [SerializeField] private UICharacterSellItem[] sellItems;
    [SerializeField] private ListSwapper[] tapSwappers;

    public async Awaitable Set(DataTableEnum.ClassType classType = DataTableEnum.ClassType.Warrior)
    {
        for (var i = 0; i < sellItems.Length; i++)
        {
            var targetSpawnType = (DataTableEnum.SpawnType)i + 1;
            sellItems[i].Set(classType, targetSpawnType);
        }

        for (var i = 0; i < tapSwappers.Length; i++)
        {
            var targetSpawnType = (int)classType - 1;
            tapSwappers[i].Swap(i != targetSpawnType ? ISwapper.SwapType.SwapType0 : ISwapper.SwapType.SwapType1);
        }

        await Awaitable.NextFrameAsync();
    }

    public void OnClickTap(int classType)
    {
        var targetClass = (DataTableEnum.ClassType)classType;
        Set(targetClass).Forget();
    }
}