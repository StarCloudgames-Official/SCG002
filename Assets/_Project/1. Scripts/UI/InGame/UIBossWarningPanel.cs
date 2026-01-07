using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossWarningPanel : UIPanel, IStageStarter
{
    [SerializeField] private Image glowImage;
    [SerializeField] private RectTransform warningText;
    [SerializeField] private RectTransform bossPanel;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text warningTextComponent;

    private const float TotalDuration = 4f;
    private const float ScaleUpDuration = 0.5f;
    private const float WaitDuration = 3f;
    private const float ScaleDownDuration = 0.5f;
    private const float FadeOutDuration = 0.5f;

    private Sequence sequence;
    private Tween glowLoopTween;

    public override UniTask PreOpen(object param)
    {
        var stageParam = param as StageStarterParam;
        bossNameText.text = stageParam.BossName;
        
        return UniTask.CompletedTask;
    }

    public void StartStarter()
    {
        var halfScreen = Screen.width * 0.5f;
        var halfWarning = warningText.rect.width * 0.5f;
        var startX = halfScreen + halfWarning;
        var endX = -(halfScreen + halfWarning);

        warningText.anchoredPosition = new Vector2(startX, warningText.anchoredPosition.y);

        KillTweens();

        sequence = DOTween.Sequence()
            .Insert(0, warningText.DOAnchorPosX(endX, TotalDuration).SetEase(Ease.Linear))
            .Insert(0, glowImage.DOFade(1f, ScaleUpDuration).SetEase(Ease.OutQuad))
            .Insert(0, bossPanel.DOScale(1f, ScaleUpDuration).SetEase(Ease.OutBack))
            .InsertCallback(ScaleUpDuration, StartGlowLoop)
            .Insert(ScaleUpDuration + WaitDuration, glowImage.DOFade(0f, FadeOutDuration).SetEase(Ease.OutQuad).OnStart(StopGlowLoop))
            .Insert(ScaleUpDuration + WaitDuration, warningTextComponent.DOFade(0f, FadeOutDuration).SetEase(Ease.OutQuad))
            .Insert(ScaleUpDuration + WaitDuration, bossPanel.DOScale(0f, ScaleDownDuration).SetEase(Ease.InBack))
            .OnComplete(() => Close().Forget())
            .SetLink(gameObject);
    }

    public IEnumerator StartStarterCoroutine()
    {
        StartStarter();
        yield return sequence.WaitForCompletion();
    }

    private void StartGlowLoop()
    {
        glowLoopTween = glowImage.DOFade(0.3f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopGlowLoop()
    {
        glowLoopTween?.Kill();
    }

    private void KillTweens()
    {
        sequence?.Kill();
        glowLoopTween?.Kill();
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}
