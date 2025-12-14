using System.Collections.Generic;
using UnityEngine;

public static class SpawnManager
{
    private static Dictionary<DataTableEnum.SpawnType, float> inGameSpawnChances;
    
    private static bool initialized;
    
    private static void Initialize()
    {
        if (initialized)
            return;

        var dataTableList = DataTableManager.Instance.GetAllSpawnChanceTables();
        
        inGameSpawnChances = new Dictionary<DataTableEnum.SpawnType, float>();
        foreach (var dataTable in dataTableList)
        {
            inGameSpawnChances[dataTable.spawnType] = dataTable.drawChance;
        }
        
        initialized = true;
    }

    public static bool TryGetInGameSpawnType(out DataTableEnum.SpawnType spawnType)
    {
        Initialize();

        var chance = Random.value;
        var cumulativeChance = 0f;

        foreach (var dataTable in inGameSpawnChances)
        {
            cumulativeChance += dataTable.Value;
            if (!(chance <= cumulativeChance))
                continue;
            
            spawnType = dataTable.Key;
            return true;
        }

        spawnType = DataTableEnum.SpawnType.None;
        return false;
    }
}