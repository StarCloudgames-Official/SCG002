using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StageManager
{
    public StageDataTable CurrentStageData { get; private set; }
    public MonsterSpawner MonsterSpawner { get; private set; }

    public event Action<int, int> OnKillCountChanged;
    public event Action<int> OnTimerChanged;
    public event Action<int> OnWaveChanged;

    public int CurrentWaveIndex
    {
        get => currentWaveIndex;
        private set
        {
            currentWaveIndex = value;
            OnWaveChanged?.Invoke(currentWaveIndex);
        }
    }

    public int CurrentKillCount
    {
        get => currentKillCount;
        set
        {
            currentKillCount = value;
            OnKillCountChanged?.Invoke(currentKillCount, CurrentStageData.monsterCount[CurrentWaveIndex]);
        }
    }

    private int currentWaveIndex;
    private int currentKillCount;
    private bool isTimerRunning;
    private int lastDisplaySeconds;

    public async UniTask Initialize(StageDataTable stageData)
    {
        CurrentWaveIndex = 0;
        CurrentStageData = stageData;

        MonsterSpawner = new MonsterSpawner();
        await MonsterSpawner.Initialize();
    }

    public void Dispose()
    {
        OnKillCountChanged = null;
        OnTimerChanged = null;
        OnWaveChanged = null;
    }

    public void IncreaseKillCount()
    {
        CurrentKillCount++;

        var monsterCount = CurrentStageData.monsterCount[CurrentWaveIndex];

        if (CurrentKillCount >= monsterCount)
        {
            WaveClear().Forget();
        }
    }

    private async UniTask StartStarter()
    {
        var isBossStage = CurrentStageData.IsBossStage(CurrentWaveIndex);

        var starterParam = new WaveStarterParam();
        IWaveStarter starter = null;

        if (isBossStage)
        {
            starterParam.BossName = CurrentStageData.MonsterDataTables[CurrentWaveIndex].monsterName;

            starter = await UIManager.OpenUI<UIBossWarningPanel>(starterParam);
        }
        else
        {
            starterParam.WaveNumber = CurrentWaveIndex;
            starterParam.MaxWave = CurrentStageData.waveCount;

            starter = await UIManager.OpenUI<UIWaveStarter>(starterParam);
        }

        starter.StartStarter();

        await ((IUI)starter).WaitUntilClose();
    }

    public async UniTask StartWave()
    {
        CurrentKillCount = 0;

        await UIManager.BlockUI();
        await StartStarter();
        UIManager.RemoveBlocker();

        var currentMonsterData = CurrentStageData.MonsterDataTables[CurrentWaveIndex];
        var spawnCount = CurrentStageData.monsterCount[CurrentWaveIndex];
        var spawnDelay = CurrentStageData.spawnDelay[CurrentWaveIndex];

        MonsterSpawner.StartSpawn(currentMonsterData, spawnCount, spawnDelay).Forget();
        StartTimer().Forget();
    }

    private async UniTask StartTimer()
    {
        isTimerRunning = true;

        var remainingSeconds = CurrentStageData.waveTimer[CurrentWaveIndex];
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

    private async UniTask<bool> WaveClear()
    {
        StopTimer();

        CurrentWaveIndex++;
        var isCleared = CurrentWaveIndex >= CurrentStageData.waveCount;

        if (isCleared)
        {
            //TODO : Show Stage Clear Popup
        }
        else
        {
            //TODO await selection popup open and closed
            await UniTask.Delay(2000);
            StartWave().Forget();
        }

        return isCleared;
    }
}
