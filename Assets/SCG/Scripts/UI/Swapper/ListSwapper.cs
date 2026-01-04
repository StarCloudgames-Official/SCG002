using UnityEngine;

public class ListSwapper : SwapperBehaviour
{
    [SerializeField] private SwapperBehaviour[] swappers;

    public override void Swap(ISwapper.SwapType swapType)
    {
        foreach (var swapper in swappers)
        {
            swapper.Swap(swapType);
        }
    }
}