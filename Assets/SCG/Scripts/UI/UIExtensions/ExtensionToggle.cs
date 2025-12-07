using UnityEngine;
using UnityEngine.EventSystems;
using Solo.MOST_IN_ONE;

public class ExtensionToggle : CachedMonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isOn;
    [SerializeField] private SwapperBehaviour swapper;

    [Header("Feedback")]
    [SerializeField] private SoundId soundId = SoundId.Click_Select;
    [SerializeField] private MOST_HapticFeedback.HapticTypes hapticType = MOST_HapticFeedback.HapticTypes.Selection;

    public bool IsOn => isOn;

    public void OnPointerClick(PointerEventData eventData)
    {
        isOn = !isOn;
        PlayFeedback();
        Swap();
    }

    private void PlayFeedback()
    {
        Haptic.PlayWithCoolDown(hapticType);
        SoundManager.PlaySFX(soundId);
    }

    private void Swap()
    {
        if (swapper != null)
        {
            swapper.Swap(isOn ? ISwapper.SwapType.SwapType0 : ISwapper.SwapType.SwapType1);
        }
    }
}
