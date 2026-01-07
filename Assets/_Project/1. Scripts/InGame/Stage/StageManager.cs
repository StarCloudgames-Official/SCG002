using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StageManager
{
    public StageDataTable CurrentStageData { get; private set; }
    public MonsterSpawner MonsterSpawner { get; private set; }

    public event Action<int, int> OnKillCountChanged;
    public event Action<int> OnTimerChanged;
    public event Action<int> OnStageChanged;

    public int CurrentStageIndex
    {
        get => currentStageIndex;
        private set
        {
            currentStageIndex = value;
            OnStageChanged?.Invoke(currentStageIndex);
        }
    }

    public int CurrentKillCount
    {
        get => currentKillCount;
        set
        {
            currentKillCount = value;
            OnKillCountChanged?.Invoke(currentKillCount, CurrentStageData.monsterCount[CurrentStageIndex]);
        }
    }

    private int currentStageIndex;
    private int currentKillCount;
    private bool isTimerRunning;
    private int lastDisplaySeconds;

    public async UniTask Initialize(StageDataTable stageData)
    {
        CurrentStageIndex = 0;
        CurrentStageData = stageData;

        MonsterSpawner = new MonsterSpawner();
        await MonsterSpawner.Initialize();
    }

    public void IncreaseKillCount()
    {
        CurrentKillCount++;

        var monsterCount = CurrentStageData.monsterCount[CurrentStageIndex];

        if (CurrentKillCount >= monsterCount)
        {
            StageClear().Forget();
        }
    }

    private async UniTask StartStarter()
    {
        var isBossStage = CurrentStageData.IsBossStage(CurrentStageIndex);

        var starterParam = new StageStarterParam();
        IStageStarter starter = null;

        if (isBossStage)
        {
            starterParam.BossName = CurrentStageData.MonsterDataTables[CurrentStageIndex].monsterName;

            starter = await UIManager.OpenUI<UIBossWarningPanel>(starterParam);
        }
        else
        {
            starterParam.StageNumber = CurrentStageIndex;
            starterParam.MaxStage = CurrentStageData.stageCount;

            starter = await UIManager.OpenUI<UIStageStarter>(starterParam);
        }

        starter.StartStarter();

        await ((IUI)starter).WaitUntilClose();
    }

    public async UniTask StartStage()
    {
        CurrentKillCount = 0;

        await UIManager.BlockUI();
        await StartStarter();
        UIManager.RemoveBlocker();

        var currentMonsterData = CurrentStageData.MonsterDataTables[CurrentStageIndex];
        var spawnCount = CurrentStageData.monsterCount[CurrentStageIndex];
        var spawnDelay = CurrentStageData.spawnDelay[CurrentStageIndex];

        MonsterSpawner.StartSpawn(currentMonsterData, spawnCount, spawnDelay).Forget();
        StartTimer().Forget();
    }

    private async UniTask StartTimer()
    {
        isTimerRunning = true;

        var remainingSeconds = CurrentStageData.stageTimer[CurrentStageIndex];
        lastDisplaySeconds = -1;

        while (isTimerRunning && remainingSeconds > 0)
        {
            var currentDisplaySeconds = Mathf.CeilToInt(remainingSeconds);

            if (currentDisplaySeconds != lastDisplaySeconds)
            {
                OnTimerChanged?.Invoke(currentDisplaySeconds);
                lastDisplaySeconds = currentDisplaySeconds;
            }

            await UniTask.NextFrame();
            remainingSeconds -= Time.deltaTime;
        }

        // Timer finished
        if (isTimerRunning && lastDisplaySeconds != 0)
        {
            OnTimerChanged?.Invoke(0);
            lastDisplaySeconds = 0;
        }

        if (isTimerRunning)
            StageFailed();
    }

    private void StopTimer()
    {
        isTimerRunning = false;
    }

    private void StageFailed()
    {
        StopTimer();
        //TODO : Show Stage Failed Popup
    }

    private async UniTask<bool> StageClear()
    {
        StopTimer();

        CurrentStageIndex++;
        var isCleared = CurrentStageIndex >= CurrentStageData.stageCount;

        if (isCleared)
        {
            //TODO : Show Stage Clear Popup
        }
        else
        {
            //TODO await selection popup open and closed
            await UniTask.Delay(2000);
            StartStage().Forget();
        }

        return isCleared;
    }
}
