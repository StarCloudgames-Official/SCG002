using UnityEngine;

public class StageManager
{
    private StageDataTable currentStageData;
    
    private int currentStageIndex;
    
    public void SetStageData(StageDataTable stageData)
    {
        currentStageIndex = 0;
        currentStageData = stageData;
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
        
        var currentMonsterData = currentStageData.MonsterDataTables[currentStageIndex];
        var spawnCount = currentStageData.monsterCount[currentStageIndex];
        
        await StartStarter();

        UIManager.RemoveBlocker();
        
        Debug.Log("Start Stage!");
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