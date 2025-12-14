using System;
using System.Collections.Generic;
using System.Data;
using StarCloudgamesLibrary;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class SpawnManager : Singleton<SpawnManager>
{
    private Dictionary<DataTableEnum.ClassType, Dictionary<DataTableEnum.SpawnType, int>> spawnedClassesCount;
    
    private Dictionary<DataTableEnum.SpawnType, float> inGameSpawnChances;
    private List<DataTableEnum.ClassType> classTypes;

    private readonly string characterPath = "Character";
    
    public override async Awaitable Initialize()
    {
        spawnedClassesCount = new Dictionary<DataTableEnum.ClassType, Dictionary<DataTableEnum.SpawnType, int>>();
        
        var dataTableList = DataTableManager.Instance.GetAllSpawnChanceTables();
        
        inGameSpawnChances = new Dictionary<DataTableEnum.SpawnType, float>();
        foreach (var dataTable in dataTableList)
        {
            inGameSpawnChances[dataTable.spawnType] = dataTable.drawChance;
        }
        
        classTypes = new List<DataTableEnum.ClassType>();
        foreach (DataTableEnum.ClassType classEnum in Enum.GetValues(typeof(DataTableEnum.ClassType)))
        {
            if(classEnum == DataTableEnum.ClassType.None)
                continue;
            
            classTypes.Add(classEnum);
        }

        await Awaitable.NextFrameAsync();
    }

    private DataTableEnum.SpawnType GetInGameSpawnType()
    {
        var chance = Random.value;
        var cumulativeChance = 0f;

        foreach (var dataTable in inGameSpawnChances)
        {
            cumulativeChance += dataTable.Value;
            if (!(chance <= cumulativeChance))
                continue;
            
            return dataTable.Key;
        }

        return DataTableEnum.SpawnType.None;
    }

    private DataTableEnum.ClassType GetInGameSpawnClassType()
    {
        return classTypes[Random.Range(0, classTypes.Count)];
    }

    public async Awaitable TryGetInGameSpawnType(Action onEnd)
    {
        //TODO : check can spawn. onEnd?.Invoke();
        
        var spawnType = GetInGameSpawnType();
        var classType = GetInGameSpawnClassType();
        var dataTable = DataTableManager.Instance.GetClassTable(classType, spawnType);

        IncreaseSpawnCount(dataTable);
        
        var newCharacter = await Addressables.InstantiateAsync(characterPath).Task;
        var characterBehaviour = newCharacter.GetComponent<CharacterBehaviour>();

        //TODO : 그리드 만들어서 그리드 할당 INitialize에 넘겨줘야됨
        characterBehaviour.Initialize(dataTable).Forget();
        onEnd?.Invoke();
    }

    private void IncreaseSpawnCount(ClassTable classTable)
    {
        var classType = classTable.classType;
        var spawnType = classTable.spawnType;

        if (!spawnedClassesCount.TryGetValue(classType, out var bySpawnType))
        {
            bySpawnType = new Dictionary<DataTableEnum.SpawnType, int>();
            spawnedClassesCount[classType] = bySpawnType;
        }

        if (bySpawnType.TryGetValue(spawnType, out var current))
            bySpawnType[spawnType] = current + 1;
        else
            bySpawnType[spawnType] = 1;
    }
}