using System;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIInGameMain : UIPanel
{
    [SerializeField] private ExtensionSlider killCountSlider;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text inGameCrystalCountText;
    [SerializeField] private TMP_Text luckyPointText;
    [SerializeField] private TMP_Text spawnCountText;
    [SerializeField] private TMP_Text spawnPriceText;
    [SerializeField] private UIDifficultyText difficultyText;
    [SerializeField] private UICharacterSellPopup characterSellPopup;
    [SerializeField] private UIClassEnhancePopup classEnhancePopup;
    [SerializeField] private UILuckyPopup luckyPopup;

    private InGameContext inGameContext;
    private MotionHandle timerWarningColorHandle;
    private MotionHandle timerWarningScaleHandle;
    private int previousRemainingSeconds = -1;
    private int maxSpawnCount = -1;

    public override void OnBackSpace()
    {
        if (luckyPopup.gameObject.activeSelf)
        {
            luckyPopup.gameObject.SetActive(false);
            return;
        }

        if (classEnhancePopup.gameObject.activeSelf)
        {
            classEnhancePopup.gameObject.SetActive(false);
            return;
        }

        if (characterSellPopup.gameObject.activeSelf)
        {
            characterSellPopup.gameObject.SetActive(false);
            return;
        }
    }

    public override async UniTask PreOpen(object param)
    {
        inGameContext = param as InGameContext;

        spawnPriceText.text = ZString.Concat(ConstantDataGetter.SpawnCrystalPrice);
        difficultyText.SetText(inGameContext.EnterInfo.Difficulty);

        InitializeAndRegisterEvents();

        await UniTask.NextFrame();
    }

    public override UniTask Close(object param = null)
    {
        if (inGameContext?.StageManager != null)
        {
            UnregisterEvents();
        }

        StopTimerWarning();

        base.Close().Forget();
        return UniTask.CompletedTask;
    }

    private void InitializeAndRegisterEvents()
    {
        var stageManager = inGameContext.StageManager;
        var stageData = stageManager.CurrentStageData;
        var currentIndex = stageManager.CurrentWaveIndex;
        var timerSeconds = Mathf.CeilToInt(stageData.waveTimer[currentIndex]);

        UpdateKillCountSlider(0, stageData.monsterCount[currentIndex]);
        UpdateTimerText(timerSeconds);
        UpdateWaveText(currentIndex);
        UpdateCrystalText(inGameContext.InGameCrystal);
        UpdateLuckyPointText(inGameContext.LuckyPoint);
        UpdateSpawnCountText(0);

        inGameContext.StageManager.OnKillCountChanged += UpdateKillCountSlider;
        inGameContext.StageManager.OnTimerChanged += UpdateTimerText;
        inGameContext.StageManager.OnWaveChanged += UpdateWaveText;
        inGameContext.InGameEvent.OnCrystalChange += UpdateCrystalText;
        inGameContext.InGameEvent.OnLuckyPointChange += UpdateLuckyPointText;
        inGameContext.InGameEvent.OnSpawnCountChanged += UpdateSpawnCountText;
    }

    private void UnregisterEvents()
    {
        inGameContext.StageManager.OnKillCountChanged -= UpdateKillCountSlider;
        inGameContext.StageManager.OnTimerChanged -= UpdateTimerText;
        inGameContext.StageManager.OnWaveChanged -= UpdateWaveText;
        inGameContext.InGameEvent.OnCrystalChange -= UpdateCrystalText;
        inGameContext.InGameEvent.OnLuckyPointChange -= UpdateLuckyPointText;
        inGameContext.InGameEvent.OnSpawnCountChanged -= UpdateSpawnCountText;
    }

    private void UpdateSpawnCountText(int currentCount)
    {
        if (maxSpawnCount == -1)
            maxSpawnCount = inGameContext.CharacterGridManager.TotalGridCount;

        spawnCountText.text = ZString.Format("{0}/{1}", currentCount, maxSpawnCount);
    }

    private void UpdateWaveText(int waveIndex)
    {
        waveText.text = ZString.Format("WAVE {0}", waveIndex + 1);
    }

    private void UpdateCrystalText(int crystal)
    {
        inGameCrystalCountText.text = ZString.Concat(crystal);
    }

    private void UpdateLuckyPointText(int luckyPoint)
    {
        luckyPointText.text = ZString.Concat(luckyPoint);
    }

    private void UpdateKillCountSlider(int current, int max)
    {
        killCountSlider.AnimateTo(current, max, 0.05f, true).Forget();
    }

    private void UpdateTimerText(int remainingSeconds)
    {
        var minutes = remainingSeconds / 60;
        var seconds = remainingSeconds % 60;
        timerText.text = ZString.Format("{0:00}:{1:00}", minutes, seconds);

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
        StopTimerWarning();

        timerWarningColorHandle = LMotion.Create(Color.white, Color.red, 0.5f)
            .WithLoops(-1, LoopType.Yoyo)
            .BindToColor(timerText)
            .AddTo(gameObject);

        timerWarningScaleHandle = LMotion.Create(Vector3.one, Vector3.one * 1.2f, 0.5f)
            .WithLoops(-1, LoopType.Yoyo)
            .BindToLocalScale(timerText.transform)
            .AddTo(gameObject);
    }

    private void StopTimerWarning()
    {
        timerWarningColorHandle.TryCancel();
        timerWarningScaleHandle.TryCancel();
        timerText.color = Color.white;
        timerText.transform.localScale = Vector3.one;
    }

    public void OnClickSpawn()
    {
        inGameContext.SpawnManager.TrySpawnCharacterByCrystal();
    }

    public void OnClickEnhance()
    {
        SetEnhancePopup().Forget();
    }

    private async UniTask SetEnhancePopup()
    {
        await classEnhancePopup.Set();
        classEnhancePopup.gameObject.SetActive(true);
    }

    public void OnClickSell()
    {
        SetSellPopup().Forget();
    }

    private async UniTask SetSellPopup()
    {
        await characterSellPopup.Set();
        characterSellPopup.gameObject.SetActive(true);
    }

    public void OnClickLucky()
    {
        SetLuckyPopup().Forget();
    }

    private async UniTask SetLuckyPopup()
    {
        await luckyPopup.Set();
        luckyPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        timerWarningColorHandle.TryCancel();
        timerWarningScaleHandle.TryCancel();
    }
}
