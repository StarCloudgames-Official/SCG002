using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class ExtensionButton : CachedMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private bool interactable = true;
    [SerializeField] private bool useAutoRepeat = false;

    [Header("Feedback")]
    [SerializeField] private SoundId soundId = SoundId.Click_Keyboard;
    [SerializeField] private MOST_HapticFeedback.HapticTypes hapticType = MOST_HapticFeedback.HapticTypes.Selection;

    [Space]
    public UnityEvent onClick;

    private Tween scaleTween;
    private Coroutine repeatCoroutine;
    private bool isPressed;
    private bool clickInvokedThisPress;
    
    private const float PressedScale = 0.9f;
    private const float PressDuration = 0.05f;
    private const float ReleaseDuration = 0.05f;
    private const Ease PressEase = Ease.OutQuad;
    private const Ease ReleaseEase = Ease.OutQuad;

    private const float InitialRepeatDelay = 0.3f;
    private const float StartRepeatInterval = 0.2f;
    private const float MinRepeatInterval = 0.05f;
    private const float IntervalDecrement = 0.01f;
    private const bool UseAcceleration = true;

    #region Properties

    public bool Interactable
    {
        get => interactable;
        set => interactable = value;
    }

    #endregion

    #region Pointer Handlers

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanInteract()) return;

        isPressed = true;
        clickInvokedThisPress = false;

        StartPressScaleTween();

        if (!useAutoRepeat) return;
        if (repeatCoroutine != null) StopCoroutine(repeatCoroutine);
        repeatCoroutine = StartCoroutine(RepeatRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;

        isPressed = false;

        StopRepeatRoutine();
        RestoreScale();
        PlayReleaseFeedback();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanInteract()) 
            return;

        if (!useAutoRepeat && clickInvokedThisPress) 
            return;
        
        clickInvokedThisPress = true;

        InvokeClick();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPressed) return;

        isPressed = false;

        StopRepeatRoutine();
        RestoreScale();
    }

    #endregion

    private void OnDisable()
    {
        isPressed = false;
        clickInvokedThisPress = false;
        StopRepeatRoutine();
        transform.localScale = Vector3.one;
        scaleTween?.Kill();
    }

    #region Interaction

    public void AddOnClickListener(UnityAction action)
    {
        onClick.AddListener(action);
    }

    private bool CanInteract()
    {
        return interactable && isActiveAndEnabled;
    }

    private void InvokeClick()
    {
        try
        {
            onClick?.Invoke();
        }
        catch (System.SystemException e)
        {
            Debug.LogException(e, this);
        }
    }

    #endregion

    #region Scale Tween

    private void StartPressScaleTween()
    {
        scaleTween?.Kill();
        scaleTween = CachedTransform
            .DOScale(Vector3.one * PressedScale, PressDuration)
            .SetEase(PressEase);
    }

    private void RestoreScale()
    {
        scaleTween?.Kill();
        scaleTween = CachedTransform
            .DOScale(Vector3.one, ReleaseDuration)
            .SetEase(ReleaseEase);
    }

    #endregion

    #region Feedback

    private void PlayReleaseFeedback()
    {
        Haptic.PlayWithCoolDown(hapticType);
        SoundManager.PlaySFX(soundId);
    }

    #endregion

    #region Auto Repeat

    private IEnumerator RepeatRoutine()
    {
        var elapsed = 0f;
        while (isPressed && elapsed < InitialRepeatDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!isPressed)
        {
            repeatCoroutine = null;
            yield break;
        }

        var currentInterval = StartRepeatInterval;

        InvokeClick();

        while (isPressed)
        {
            elapsed = 0f;
            while (isPressed && elapsed < currentInterval)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!isPressed) break;

            InvokeClick();

            if (UseAcceleration)
                currentInterval = Mathf.Max(MinRepeatInterval, currentInterval - IntervalDecrement);
        }

        repeatCoroutine = null;
    }

    private void StopRepeatRoutine()
    {
        if (repeatCoroutine == null) return;

        StopCoroutine(repeatCoroutine);
        repeatCoroutine = null;
    }

    #endregion
}
