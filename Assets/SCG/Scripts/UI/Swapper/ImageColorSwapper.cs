using UnityEngine;
using UnityEngine.UI;

public class ImageColorSwapper : SwapperBase<Color>
{
    [SerializeField] private Image image;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if (!swapDictionary.TryGetValue(swapType, out var swapColor)) return;
        image.color = swapColor;
    }
}