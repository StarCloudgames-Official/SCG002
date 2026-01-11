using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageSwapper : SwapperBase<Image>
{
    [SerializeField] private Image targetImage;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if(!targetImage)
            targetImage = GetComponent<Image>();
        
        if (!swapDictionary.TryGetValue(swapType, out var swapImage)) return;
        targetImage.sprite = swapImage.sprite;
    }
}