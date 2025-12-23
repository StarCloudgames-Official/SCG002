using UnityEngine;

public class StageManager
{
    private StageDataTable currentStageData;
    private MonsterSpawner monsterSpawner;
    
    private int currentStageIndex;
    
    public async Awaitable Initialize(StageDataTable stageData)
    {
        currentStageIndex = 0;
        currentStageData = stageData;
        
        monsterSpawner = new MonsterSpawner();
        await monsterSpawner.Initialize();
    }

    private async Awaitable StartStarter()
    {
        var isBossStage = currentStageData.IsBossStage(currentStageIndex);
        
        var starterParam = new StageStarterParam();
        IStageStarter starter = null;
        
        if (isBossStage)
        {
            starterParam.BossName = currentStageData.MonsterDataTables[currentStageIndex].monsterName;
            
            starter = await UIManager.OpenUI<UIBossWarningPanel>(starterParam);
        }
        else
        {
            starterParam.StageNumber = currentStageIndex;
            starterParam.MaxStage = currentStageData.stageCount;
            
            starter = await UIManager.OpenUI<UIStageStarter>(starterParam);
        }
        
        starter.StartStarter();

        await ((IUI)starter).WaitUntilClose();
    }

    public async Awaitable StartStage()
    {
        await UIManager.BlockUI();
        await StartStarter();
        UIManager.RemoveBlocker();
        
        var currentMonsterData = currentStageData.MonsterDataTables[currentStageIndex];
        var spawnCount = currentStageData.monsterCount[currentStageIndex];
        var spawnDelay = currentStageData.spawnDelay[currentStageIndex];

        monsterSpawner.StartSpawn(currentMonsterData, spawnCount, spawnDelay).Forget();
    }

    public async Awaitable<bool> StageClear()
    {
        currentStageIndex++;
        var isCleared = currentStageIndex >= currentStageData.stageCount;

        if (isCleared)
        {
            //TODO : Stage Clear Popup
        }
        else
        {
            //TODO await selection popup open and closed
            await Awaitable.WaitForSecondsAsync(2.0f);
            StartStage().Forget();
        }

        return isCleared;
    }
}