using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageColorSwapper : SwapperBase<Color>
{
    [SerializeField] private Image image;
    
    public override void Swap(ISwapper.SwapType swapType)
    {
        if(!image)
            image = GetComponent<Image>();
        
        if (!swapDictionary.TryGetValue(swapType, out var swapColor)) return;
        image.color = swapColor;
    }
}