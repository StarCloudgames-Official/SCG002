using UnityEngine;

public class ObjectSwapper : SwapperBase<GameObject>
{
    public override void Swap(ISwapper.SwapType swapType)
    {
        foreach (var data in swapDictionary)
        {
            data.Value.SetActive(data.Key == swapType);
        }
    }
}