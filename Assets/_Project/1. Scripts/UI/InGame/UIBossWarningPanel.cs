using System.Collections;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossWarningPanel : UIPanel, IWaveStarter
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

    private MotionHandle warningMoveHandle;
    private MotionHandle glowFadeInHandle;
    private MotionHandle bossScaleUpHandle;
    private MotionHandle glowLoopHandle;
    private MotionHandle glowFadeOutHandle;
    private MotionHandle warningFadeOutHandle;
    private MotionHandle bossScaleDownHandle;

    public override UniTask PreOpen(object param)
    {
        var stageParam = param as WaveStarterParam;
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
        RunSequenceAsync(startX, endX).Forget();
    }

    private async UniTask RunSequenceAsync(float startX, float endX)
    {
        // Phase 1 (t=0): warningMove + glowFadeIn + bossScaleUp start simultaneously
        warningMoveHandle = LMotion.Create(startX, endX, TotalDuration)
            .WithEase(Ease.Linear)
            .BindToAnchoredPositionX(warningText)
            .AddTo(gameObject);

        glowFadeInHandle = LMotion.Create(0f, 1f, ScaleUpDuration)
            .WithEase(Ease.OutQuad)
            .BindToColorA(glowImage)
            .AddTo(gameObject);

        bossScaleUpHandle = LMotion.Create(Vector3.zero, Vector3.one, ScaleUpDuration)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(bossPanel)
            .AddTo(gameObject);

        // Phase 2 (t=0.5s): wait for glowFadeIn to complete, then start glow loop
        await glowFadeInHandle.ToUniTask();
        StartGlowLoop();

        // Phase 3 (t=3.5s): wait, then stop glow loop and start fade out + scale down
        await UniTask.Delay((int)(WaitDuration * 1000));
        StopGlowLoop();

        glowFadeOutHandle = LMotion.Create(glowImage.color.a, 0f, FadeOutDuration)
            .WithEase(Ease.OutQuad)
            .BindToColorA(glowImage)
            .AddTo(gameObject);

        warningFadeOutHandle = LMotion.Create(1f, 0f, FadeOutDuration)
            .WithEase(Ease.OutQuad)
            .BindToColorA(warningTextComponent)
            .AddTo(gameObject);

        bossScaleDownHandle = LMotion.Create(Vector3.one, Vector3.zero, ScaleDownDuration)
            .WithEase(Ease.InBack)
            .BindToLocalScale(bossPanel)
            .AddTo(gameObject);

        // Wait for the last phase to complete, then close
        await bossScaleDownHandle.ToUniTask();
        Close().Forget();
    }

    public IEnumerator StartStarterCoroutine()
    {
        StartStarter();
        yield return warningMoveHandle.ToYieldInstruction();
    }

    private void StartGlowLoop()
    {
        glowLoopHandle = LMotion.Create(1f, 0.3f, 1f)
            .WithLoops(-1, LoopType.Yoyo)
            .WithEase(Ease.InOutSine)
            .BindToColorA(glowImage)
            .AddTo(gameObject);
    }

    private void StopGlowLoop()
    {
        glowLoopHandle.TryCancel();
    }

    private void KillTweens()
    {
        warningMoveHandle.TryCancel();
        glowFadeInHandle.TryCancel();
        bossScaleUpHandle.TryCancel();
        glowLoopHandle.TryCancel();
        glowFadeOutHandle.TryCancel();
        warningFadeOutHandle.TryCancel();
        bossScaleDownHandle.TryCancel();
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}
