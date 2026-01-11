using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizeTextSwapper : SwapperBase<string>
{
    [SerializeField] private TMP_Text targetText;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if(!targetText)
            targetText = GetComponent<TMP_Text>();
        
        if (!swapDictionary.TryGetValue(swapType, out var swapKey)) return;
        targetText.text = swapKey.Localize();
    }
}