using System.Collections;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

public class UIWaveStarter : UIPanel, IWaveStarter
{
    [SerializeField] private RectTransform wavePanel;
    [SerializeField] private TMP_Text waveText;

    private const float EnterDuration = 0.5f;
    private const float WaitDuration = 1f;
    private const float ExitDuration = 0.5f;

    private MotionHandle sequenceHandle;

    public override UniTask PreOpen(object param)
    {
        var waveParam = param as WaveStarterParam;
        waveText.text = ZString.Format("WAVE {0} / {1}", waveParam.WaveNumber + 1, waveParam.MaxWave);
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

        sequenceHandle.TryCancel();
        sequenceHandle = LSequence.Create()
            .Append(LMotion.Create(leftX, 0f, EnterDuration).WithEase(Ease.OutQuad).BindToAnchoredPositionX(wavePanel))
            .AppendInterval(WaitDuration)
            .Append(LMotion.Create(0f, rightX, ExitDuration).WithEase(Ease.InQuad).BindToAnchoredPositionX(wavePanel))
            .Run(builder => builder.WithOnComplete(() => Close().Forget()))
            .AddTo(gameObject);
    }

    public IEnumerator StartStarterCoroutine()
    {
        StartStarter();
        yield return sequenceHandle.ToYieldInstruction();
    }

    private void OnDestroy()
    {
        sequenceHandle.TryCancel();
    }
}
