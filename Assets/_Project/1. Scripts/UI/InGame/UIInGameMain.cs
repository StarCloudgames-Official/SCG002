using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIInGameMain : UIPanel
{
    [SerializeField] private ExtensionSlider killCountSlider;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text stageText;

    private bool canSpawn = true;
    private InGameContext inGameContext;

    public override async Awaitable PreOpen(object param)
    {
        inGameContext = param as InGameContext;

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
        var stageManager = inGameContext.StageManager;
        var stageData = stageManager.CurrentStageData;
        var currentIndex = stageManager.CurrentStageIndex;
        var timerSeconds = Mathf.CeilToInt(stageData.stageTimer[currentIndex]);

        UpdateKillCountSlider(0, stageData.monsterCount[currentIndex]);
        UpdateTimerText(timerSeconds);
        UpdateStageText(currentIndex);

        inGameContext.StageManager.OnKillCountChanged += UpdateKillCountSlider;
        inGameContext.StageManager.OnTimerChanged += UpdateTimerText;
        inGameContext.StageManager.OnStageChanged += UpdateStageText;
    }

    private void UnregisterEvents()
    {
        inGameContext.StageManager.OnKillCountChanged -= UpdateKillCountSlider;
        inGameContext.StageManager.OnTimerChanged -= UpdateTimerText;
        inGameContext.StageManager.OnStageChanged -= UpdateStageText;
    }

    private void UpdateStageText(int stageIndex)
    {
        stageText.text = $"STAGE {stageIndex + 1}";
    }

    private void UpdateKillCountSlider(int current, int max)
    {
        killCountSlider.AnimateTo(current, max, 0.05f, true).Forget();
    }

    private void UpdateTimerText(int remainingSeconds)
    {
        var dateTime = new DateTime().AddSeconds(remainingSeconds);
        timerText.text = dateTime.DateTimeToStringToMMSS();
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