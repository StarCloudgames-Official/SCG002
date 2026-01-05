using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIInGameMain : UIPanel
{
    [SerializeField] private ExtensionSlider killCountSlider;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text inGameCrystalCountText;
    [SerializeField] private TMP_Text luckyPointText;
    [SerializeField] private TMP_Text spawnCountText;
    [SerializeField] private TMP_Text spawnPriceText;
    [SerializeField] private UICharacterSellPopup characterSellPopup;
    [SerializeField] private UIClassEnhancePopup classEnhancePopup;
    [SerializeField] private UILuckyPopup luckyPopup;

    private InGameContext inGameContext;
    private Sequence timerWarningSequence;
    private int previousRemainingSeconds = -1;
    private int maxSpawnCount = -1;

    public override async Awaitable PreOpen(object param)
    {
        inGameContext = param as InGameContext;

        spawnPriceText.text = ConstantDataGetter.SpawnCrystalPrice.ToString();

        InitializeAndRegisterEvents();

        await Awaitable.NextFrameAsync();
    }

    public override async Awaitable Close(object param = null)
    {
        if (inGameContext?.StageManager != null)
        {
            UnregisterEvents();
        }

        StopTimerWarning();

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
        UpdateCrystalText(inGameContext.InGameCrystal);
        UpdateLuckyPointText(inGameContext.LuckyPoint);
        UpdateSpawnCountText(0);

        inGameContext.StageManager.OnKillCountChanged += UpdateKillCountSlider;
        inGameContext.StageManager.OnTimerChanged += UpdateTimerText;
        inGameContext.StageManager.OnStageChanged += UpdateStageText;
        inGameContext.InGameEvent.OnCrystalChange += UpdateCrystalText;
        inGameContext.InGameEvent.OnLuckyPointChange += UpdateLuckyPointText;
        inGameContext.InGameEvent.OnSpawnCountChanged += UpdateSpawnCountText;
    }

    private void UnregisterEvents()
    {
        inGameContext.StageManager.OnKillCountChanged -= UpdateKillCountSlider;
        inGameContext.StageManager.OnTimerChanged -= UpdateTimerText;
        inGameContext.StageManager.OnStageChanged -= UpdateStageText;
        inGameContext.InGameEvent.OnCrystalChange -= UpdateCrystalText;
        inGameContext.InGameEvent.OnLuckyPointChange -= UpdateLuckyPointText;
        inGameContext.InGameEvent.OnSpawnCountChanged -= UpdateSpawnCountText;
    }

    private void UpdateSpawnCountText(int currentCount)
    {
        if (maxSpawnCount == -1)
            maxSpawnCount = inGameContext.CharacterGridManager.TotalGridCount;

        spawnCountText.text = $"{currentCount}/{maxSpawnCount}";
    }

    private void UpdateStageText(int stageIndex)
    {
        stageText.text = $"STAGE {stageIndex + 1}";
    }

    private void UpdateCrystalText(int crystal)
    {
        inGameCrystalCountText.text = crystal.ToString();
    }

    private void UpdateLuckyPointText(int luckyPoint)
    {
        luckyPointText.text = luckyPoint.ToString();
    }

    private void UpdateKillCountSlider(int current, int max)
    {
        killCountSlider.AnimateTo(current, max, 0.05f, true).Forget();
    }

    private void UpdateTimerText(int remainingSeconds)
    {
        var dateTime = new DateTime().AddSeconds(remainingSeconds);
        timerText.text = dateTime.DateTimeToStringToMMSS();

        // 11초 -> 10초로 변할 때만 시작
        if (previousRemainingSeconds > 10 && remainingSeconds <= 10)
        {
            StartTimerWarning();
        }
        // 10초 -> 11초로 변할 때 중지 (타이머 리셋 상황 대비)
        else if (previousRemainingSeconds <= 10 && remainingSeconds > 10)
        {
            StopTimerWarning();
        }

        previousRemainingSeconds = remainingSeconds;
    }

    private void StartTimerWarning()
    {
        timerWarningSequence?.Kill();

        timerWarningSequence = DOTween.Sequence()
            .Append(timerText.DOColor(Color.red, 0.5f))
            .Join(timerText.transform.DOScale(1.2f, 0.5f))
            .Append(timerText.DOColor(Color.white, 0.5f))
            .Join(timerText.transform.DOScale(1f, 0.5f))
            .SetLoops(-1)
            .SetLink(gameObject);
    }

    private void StopTimerWarning()
    {
        timerWarningSequence?.Kill();
        timerText.color = Color.white;
        timerText.transform.localScale = Vector3.one;
    }

    public void OnClickSpawn()
    {
        inGameContext.SpawnManager.TrySpawnCharacter();
    }

    public void OnClickEnhance()
    {
        SetEnhancePopup().Forget();
    }

    private async Awaitable SetEnhancePopup()
    {
        await classEnhancePopup.Set();
        classEnhancePopup.gameObject.SetActive(true);
    }

    public void OnClickSell()
    {
        SetSellPopup().Forget();
    }

    private async Awaitable SetSellPopup()
    {
        await characterSellPopup.Set();
        characterSellPopup.gameObject.SetActive(true);
    }

    public void OnClickLucky()
    {
        SetLuckyPopup().Forget();
    }

    private async Awaitable SetLuckyPopup()
    {
        await luckyPopup.Set();
        luckyPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        timerWarningSequence?.Kill();
    }
}