using System;
using System.Collections.Generic;
using MemoryPack;

public partial class IAPDataTable
{
    [MemoryPackIgnore]
    private IReadOnlyList<RewardGroupDataTable> rewardGroupData;

    [MemoryPackIgnore]
    public IReadOnlyList<RewardGroupDataTable> RewardGroupData
    {
        get
        {
            if (rewardGroupData == null)
            {
                var manager = DataTableManager.Instance;
                if (manager != null)
                    rewardGroupData = manager.GetTargetRewardGroupRewards(rewardGroupId);
            }

            return rewardGroupData ?? Array.Empty<RewardGroupDataTable>();
        }
    }
}

public partial class StageDataTable
{
    [MemoryPackIgnore]
    private IReadOnlyList<MonsterDataTable> monsterDataTables;

    [MemoryPackIgnore]
    public IReadOnlyList<MonsterDataTable> MonsterDataTables
    {
        get
        {
            if (monsterDataTables == null)
            {
                var manager = DataTableManager.Instance;
                if (manager != null)
                    monsterDataTables = manager.GetMonsterDataTables(monsterIds);
            }
            
            return monsterDataTables ?? Array.Empty<MonsterDataTable>();
        }
    }

    public bool IsBossStage(int stageIndex)
    {
        var currentStageIndex = stageIndex + 1;
        foreach (var stage in bossWave)
        {
            if(stage ==  currentStageIndex)
                return true;
        }
        return false;
    }
}