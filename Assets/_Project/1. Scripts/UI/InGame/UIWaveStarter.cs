using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIWaveStarter : UIPanel, IWaveStarter
{
    [SerializeField] private RectTransform wavePanel;
    [SerializeField] private TMP_Text waveText;

    private const float EnterDuration = 0.5f;
    private const float WaitDuration = 1f;
    private const float ExitDuration = 0.5f;

    private Sequence sequence;

    public override UniTask PreOpen(object param)
    {
        var waveParam = param as WaveStarterParam;
        waveText.text = $"WAVE {waveParam.WaveNumber + 1} / {waveParam.MaxWave}";
        return UniTask.CompletedTask;
    }

    public void StartStarter()
    {
        var halfScreen = Screen.width * 0.5f;
        var halfPanel = wavePanel.rect.width * 0.5f;
        var leftX = -(halfScreen + halfPanel);
        var rightX = halfScreen + halfPanel;

        wavePanel.gameObject.SetActive(true);
        wavePanel.anchoredPosition = new Vector2(leftX, wavePanel.anchoredPosition.y);

        sequence?.Kill();
        sequence = DOTween.Sequence()
            .Append(wavePanel.DOAnchorPosX(0, EnterDuration).SetEase(Ease.OutQuad))
            .AppendInterval(WaitDuration)
            .Append(wavePanel.DOAnchorPosX(rightX, ExitDuration).SetEase(Ease.InQuad))
            .OnComplete(() => Close().Forget())
            .SetLink(gameObject);
    }

    public IEnumerator StartStarterCoroutine()
    {
        StartStarter();
        yield return sequence.WaitForCompletion();
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }
}
