using System;
using TMPro;
using UnityEngine;

public class UIInGameMain : UIPanel
{
    [SerializeField] private ExtensionSlider killCountSlider;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text stageText;

    private bool canSpawn = true;
    private InGameContext inGameContext;

    private Action<int, int> onKillCountChanged;
    private Action<DateTime> onTimerChanged;

    public override async Awaitable PreOpen(object param)
    {
        inGameContext = param as InGameContext;

        //TODO : killCountSlider랑 timerText, stageText 다 StageManager에 이벤트로 연결해서 세팅하면 됨
        killCountSlider.SetValueImmediate(0, inGameContext.StageManager.CurrentStageData.monsterCount[inGameContext.StageManager.CurrentStageIndex]);

        var timerSeconds = inGameContext.StageManager.CurrentStageData.stageTimer[inGameContext.StageManager.CurrentStageIndex];
        var dateTime = new DateTime().AddSeconds(timerSeconds);
        timerText.text = dateTime.DateTimeToStringToMMSS();

        InitializeAndRegisterEvents();

        await Awaitable.NextFrameAsync();
    }

    public override async Awaitable Close(object param = null)
    {
        if (inGameContext?.StageManager != null)
        {
            UnregisterEvents();
        }

        base.Close().Forget();
    }

    private void InitializeAndRegisterEvents()
    {
        onKillCountChanged = (current, max) =>
        {
            killCountSlider.AnimateTo(current, max, 0.05f, true).Forget();
        };

        onTimerChanged = (dateTime) =>
        {
            timerText.text = dateTime.DateTimeToStringToMMSS();
        };

        inGameContext.StageManager.OnKillCountChanged += onKillCountChanged;
        inGameContext.StageManager.OnTimerChanged += onTimerChanged;
    }

    private void UnregisterEvents()
    {
        inGameContext.StageManager.OnKillCountChanged -= onKillCountChanged;
        inGameContext.StageManager.OnTimerChanged -= onTimerChanged;
    }

    public void OnClickSpawn()
    {
        if (!canSpawn)
            return;

        inGameContext.SpawnManager.TrySpawnCharacter(() => canSpawn = true).Forget();
    }

    public void OnClickEnhance()
    {
        
    }

    public void OnClickSell()
    {
        
    }
}