using TMPro;
using UnityEngine;

public class LocalizeTextSwapper : SwapperBase<string>
{
    [SerializeField] private TextMeshProUGUI targetText;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if (!swapDictionary.TryGetValue(swapType, out var swapKey)) return;
        targetText.text = swapKey.Localize();
    }
}