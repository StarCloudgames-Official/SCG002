using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIStageStarter : UIPanel, IStageStarter
{
    [SerializeField] private RectTransform stagePanel;
    [SerializeField] private TMP_Text stageText;
    
    private const float EnterDuration = 0.5f;
    private const float WaitDuration = 1f;
    private const float ExitDuration = 0.5f;

    private Sequence sequence;

    public override async Awaitable PreOpen(object param)
    {
        var stageParam = param as StageStarterParam;
        stageText.text = $"STAGE {stageParam.StageNumber} / {stageParam.MaxStage}";
    }

    public void StartStarter()
    {
        var halfScreen = Screen.width * 0.5f;
        var halfPanel = stagePanel.rect.width * 0.5f;
        var leftX = -(halfScreen + halfPanel);
        var rightX = halfScreen + halfPanel;
        
        stagePanel.gameObject.SetActive(true);
        stagePanel.anchoredPosition = new Vector2(leftX, stagePanel.anchoredPosition.y);
        
        sequence?.Kill();
        sequence = DOTween.Sequence()
            .Append(stagePanel.DOAnchorPosX(0, EnterDuration).SetEase(Ease.OutQuad))
            .AppendInterval(WaitDuration)
            .Append(stagePanel.DOAnchorPosX(rightX, ExitDuration).SetEase(Ease.InQuad))
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