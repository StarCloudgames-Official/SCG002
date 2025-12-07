using UnityEngine;
using UnityEngine.UI;

public class ImageSwapper : SwapperBase<Image>
{
    [SerializeField] private Image targetImage;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if (!swapDictionary.TryGetValue(swapType, out var swapImage)) return;
        targetImage.sprite = swapImage.sprite;
    }
}