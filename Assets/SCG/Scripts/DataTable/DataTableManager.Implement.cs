using System.Collections.Generic;

public partial class DataTableManager
{
    public IReadOnlyList<IAPDataTable> GetAllIAPDataTables()
    {
        return GetTable<IAPDataTable>();
    }

    public IAPDataTable GetIAPDataTable(int iapId)
    {
        var allIAPDataTables = GetAllIAPDataTables();
        foreach (var dataTable in allIAPDataTables)
        {
            if (dataTable.id == iapId)
                return dataTable;
        }

        return null;
    }

    public IReadOnlyList<RewardGroupDataTable> GetAllRewardGroupDataTables()
    {
        return GetTable<RewardGroupDataTable>();
    }

    public IReadOnlyList<RewardGroupDataTable> GetTargetRewardGroupRewards(int rewardGroupId)
    {
        var allData = GetAllRewardGroupDataTables();
        var targetGroups = new List<RewardGroupDataTable>();

        foreach (var data in allData)
        {
            if(data.rewardGroupId == rewardGroupId)
                targetGroups.Add(data);
        }
        
        return targetGroups;
    }

    public IReadOnlyList<SpawnChanceTable> GetAllSpawnChanceTables()
    {
        return GetTable<SpawnChanceTable>();
    }

    public ClassTable GetClassTable(DataTableEnum.ClassType classType, DataTableEnum.SpawnType spawnType)
    {
        var allData = GetTable<ClassTable>();

        foreach (var data in allData)
        {
            if(data.classType == classType && data.spawnType == spawnType)
                return data;
        }
        
        return null;
    }

    public StageDataTable GetStageDataTable(int stage, int floor)
    {
        var allData = GetTable<StageDataTable>();
        foreach (var data in allData)
        {
            if(data.stage == stage && data.floor == floor)
                return data;
        }
        return null;
    }

    public StageDataTable GetStageDataTable(int stageId)
    {
        var allData = GetTable<StageDataTable>();
        foreach (var data in allData)
        {
            if(data.id == stageId)
                return data;
        }
        return null;
    }

    public IReadOnlyList<MonsterDataTable> GetMonsterDataTables(int[] monsterIds)
    {
        var result = new List<MonsterDataTable>();
        var allData = GetTable<MonsterDataTable>();

        foreach (var data in allData)
        {
            foreach (var monsterId in monsterIds)
            {
                if (data.id == monsterId)
                {
                    result.Add(GetMonsterDataTable(monsterId));
                    break;
                }
            }
        }
        
        return result;
    }

    public MonsterDataTable GetMonsterDataTable(int monsterId)
    {
        var allData = GetTable<MonsterDataTable>();
        foreach (var data in allData)
        {
            if(data.id == monsterId)
                return data;
        }
        return null;
    }
}