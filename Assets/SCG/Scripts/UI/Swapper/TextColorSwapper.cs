using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class TextColorSwapper : SwapperBase<Color>
{
    [SerializeField] private TMP_Text targetText;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if(!targetText)
            targetText = GetComponent<TMP_Text>();
        
        if (!swapDictionary.TryGetValue(swapType, out var swapColor)) return;
        targetText.color = swapColor;
    }
}