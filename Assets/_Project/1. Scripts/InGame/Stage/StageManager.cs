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

    public async Awaitable StartStage()
    {
        var currentMonsterData = currentStageData.MonsterDataTables[currentStageIndex];
        var isBossStage = currentStageData.IsBossStage(currentStageIndex);
        var spawnCount = currentStageData.monsterCount[currentStageIndex];
        
        //TODO : Start Spawn
        //spawn sequence : 3,2,1 카운트 UI 표시 해주고나서 스폰 시작하기
        //UIStageStarter 만들어서 보스면 페이드 추가하고 아니면 그냥 카운트 넣고 하면 될듯??
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