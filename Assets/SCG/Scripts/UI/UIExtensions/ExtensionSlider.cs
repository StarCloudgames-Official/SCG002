using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ExtensionSlider : CachedMonoBehaviour
{
    [SerializeField] private RectTransform fill;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private bool isPercentage;

    [Space]
    public UnityEvent<float> onValueChanged;

    private MotionHandle fillHandle;
    private int animateToken;
    private float originWidth;

    #region Unity

    private void Awake()
    {
        if (!fill) return;
        originWidth = fill.sizeDelta.x;
        if (originWidth <= 0f) originWidth = 1f;
    }

    private void OnDisable()
    {
        KillTween();
    }

    #endregion

    #region Public API

    public void SetValueImmediate(float current, float max, bool isInt = false)
    {
        var safeMax = Mathf.Max(0f, max);
        var clampedCurrent = Mathf.Clamp(current, 0f, safeMax);
        var normalized = safeMax <= 0f ? 0f : clampedCurrent / safeMax;

        UpdateFill(normalized);
        UpdateText(normalized, clampedCurrent, safeMax, isInt);

        onValueChanged?.Invoke(normalized);
    }

    public async UniTask AnimateTo(float targetCurrent, float max, float duration, bool isInt = false)
    {
        duration = Mathf.Max(0.0001f, duration);

        var token = ++animateToken;

        KillTween();

        var safeMax = Mathf.Max(0f, max);

        var startNormalized = GetCurrentNormalized();
        var startCurrent = safeMax * Mathf.Clamp01(startNormalized);
        var endCurrent = Mathf.Clamp(targetCurrent, 0f, safeMax);

        fillHandle = LMotion.Create(startCurrent, endCurrent, duration)
            .Bind(this, (value, self) =>
            {
                var normalized = safeMax <= 0f ? 0f : value / safeMax;

                self.UpdateFill(normalized);
                self.UpdateText(normalized, value, safeMax, isInt);

                self.onValueChanged?.Invoke(normalized);
            });

        while (fillHandle.IsActive() && fillHandle.IsPlaying() && token == animateToken) await UniTask.NextFrame();

        if (token != animateToken) return;

        var finalNormalized = safeMax <= 0f ? 0f : endCurrent / safeMax;

        UpdateFill(finalNormalized);
        UpdateText(finalNormalized, endCurrent, safeMax, isInt);

        onValueChanged?.Invoke(finalNormalized);

        KillTween();
    }

    #endregion

    #region Internal

    private float GetCurrentNormalized()
    {
        if (!fill) return 0f;
        return originWidth <= 0f ? 0f : Mathf.Clamp01(fill.sizeDelta.x / originWidth);
    }

    private void UpdateFill(float normalized)
    {
        if (!fill) return;

        normalized = Mathf.Clamp01(normalized);

        var size = fill.sizeDelta;
        size.x = originWidth * normalized;
        fill.sizeDelta = size;
    }

    private void UpdateText(float normalized, float current, float max, bool isInt = false)
    {
        if (!valueText) return;

        if (max <= 0f)
        {
            valueText.text = isPercentage ? "0%" : "0/0";
            return;
        }

        if (isPercentage)
        {
            var percentValue = normalized * 100f;
            valueText.text = ZString.Format("{0}%", FormatNumber(percentValue, isInt));
        }
        else
        {
            valueText.text = ZString.Format("{0}/{1}", FormatNumber(current, isInt), FormatNumber(max, isInt));
        }
    }

    private string FormatNumber(float value, bool isInt = false)
    {
        if (isInt) return ZString.Concat((int)value);
        return Mathf.Approximately(value % 1f, 0f) ? ZString.Concat((int)value) : ZString.Format("{0:0.##}", value);
    }
    
    private void KillTween()
    {
        fillHandle.TryCancel();
    }

    #endregion
}
