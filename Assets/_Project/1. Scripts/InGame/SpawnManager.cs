using System;
using System.Collections.Generic;
using StarCloudgamesLibrary;
using UnityEngine;
using static DataTableEnum;
using Random = UnityEngine.Random;

public class SpawnManager : Singleton<SpawnManager>
{
    private Dictionary<ClassType, Dictionary<SpawnType, int>> spawnedClassesCount;
    private Dictionary<SpawnType, float> inGameSpawnChances;
    private List<ClassType> classTypes;
    
    private InGameContext inGameContext;

    private const string CharacterPath = "Character";

    #region Initialize

    public override async Awaitable Initialize()
    {
        inGameContext = InGameManager.Instance.InGameContext;
        
        spawnedClassesCount = new Dictionary<ClassType, Dictionary<SpawnType, int>>();
        
        var dataTableList = DataTableManager.Instance.GetAllSpawnChanceTables();
        
        inGameSpawnChances = new Dictionary<SpawnType, float>();
        foreach (var dataTable in dataTableList)
        {
            inGameSpawnChances[dataTable.spawnType] = dataTable.drawChance;
        }
        
        classTypes = new List<ClassType>();
        foreach (ClassType classEnum in Enum.GetValues(typeof(ClassType)))
        {
            if(classEnum == ClassType.None)
                continue;
            
            classTypes.Add(classEnum);
        }

        await Awaitable.NextFrameAsync();
    }

    #endregion

    #region Random Data

    private SpawnType GetInGameSpawnType()
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

        return SpawnType.Normal;
    }

    private ClassType GetInGameSpawnClassType()
    {
        return classTypes[Random.Range(0, classTypes.Count)];
    }

    #endregion

    #region Spawn

    public async Awaitable TrySpawnCharacter(Action onEnd)
    {
        //TODO : Check asset(gold) or something
        
        var emptyGrid = inGameContext.CharacterGridManager.GetRandomEmptyGrid();
        if (!emptyGrid)
        {
            onEnd?.Invoke();
            return;
        }

        var spawnType = GetInGameSpawnType();
        //var classType = GetInGameSpawnClassType();
        var classType = ClassType.Rogue; //애니 작업 끝나면 다시 제거해야 됨
        var dataTable = DataTableManager.Instance.GetClassTable(classType, spawnType);
        
        var characterBehaviour = await AddressableExtensions.InstantiateAndGetComponent<CharacterBehaviour>(CharacterPath);
        characterBehaviour.Initialize(dataTable).Forget();
        characterBehaviour.SetToGrid(emptyGrid);
        
        onEnd?.Invoke();
        
        IncreaseSpawnCount(classType, spawnType);
        InGameManager.Instance.InGameContext.InGameEvent.PublishSpawn(classType, spawnType);
    }

    #endregion

    #region Extension

    private void IncreaseSpawnCount(ClassType classType, SpawnType spawnType)
    {
        if (!spawnedClassesCount.TryGetValue(classType, out var bySpawnType))
        {
            bySpawnType = new Dictionary<SpawnType, int>();
            spawnedClassesCount[classType] = bySpawnType;
        }

        if (bySpawnType.TryGetValue(spawnType, out var current))
            bySpawnType[spawnType] = current + 1;
        else
            bySpawnType[spawnType] = 1;
    }

    #endregion
}