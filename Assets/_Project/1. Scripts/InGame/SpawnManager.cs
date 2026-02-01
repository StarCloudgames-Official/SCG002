using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using StarCloudgamesLibrary;
using UnityEngine;
using UnityEngine.Pool;
using static DataTableEnum;
using Random = UnityEngine.Random;

public class SpawnManager
{
    private static readonly ClassType[] CachedClassTypes = (ClassType[])Enum.GetValues(typeof(ClassType));
    
    private Dictionary<ClassType, Dictionary<SpawnType, int>> spawnedClassesCount;
    private Dictionary<SpawnType, float> inGameSpawnChances;
    private List<ClassType> classTypes;

    private InGameContext inGameContext;
    private SCGObjectPooling<CharacterBehaviour> characterBehaviourPool;

    private int spawnCrystalPrice;

    #region Initialize

    public async UniTask Initialize()
    {
        inGameContext = InGameManager.Instance.InGameContext;
        spawnCrystalPrice = ConstantDataGetter.SpawnCrystalPrice;

        InitializeSpawnChances();

        characterBehaviourPool = await SCGObjectPoolingManager.GetOrCreatePoolAsync<CharacterBehaviour>(AddressableExtensions.CharacterPath, 30, InGameManager.Instance.CachedTransform);
    }

    private void InitializeSpawnChances()
    {
        spawnedClassesCount = new Dictionary<ClassType, Dictionary<SpawnType, int>>();

        var dataTableList = DataTableManager.Instance.GetAllSpawnChanceTables();

        inGameSpawnChances = new Dictionary<SpawnType, float>();
        foreach (var dataTable in dataTableList)
        {
            inGameSpawnChances[dataTable.spawnType] = dataTable.drawChance;
        }

        classTypes = new List<ClassType>();
        foreach (var classEnum in CachedClassTypes)
        {
            if(classEnum == ClassType.None)
                continue;

            classTypes.Add(classEnum);
        }
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

    public ClassType GetRandomClassType()
    {
        return classTypes[Random.Range(0, classTypes.Count)];
    }

    #endregion

    #region Spawn

    public bool CanSpawnCharacter()
    {
        return inGameContext.CharacterGridManager.GetRandomEmptyGrid() != null;
    }

    public void TrySpawnCharacterByCrystal()
    {
        if(!inGameContext.CanUseInGameCrystal(spawnCrystalPrice))
            return;
        if(!CanSpawnCharacter())
            return;

        inGameContext.UseInGameCrystal(spawnCrystalPrice);

        var spawnType = GetInGameSpawnType();
        var classType = GetRandomClassType();
        SpawnCharacter(classType, spawnType);
    }

    public void SpawnCharacter(ClassType classType, SpawnType spawnType)
    {
        var emptyGrid = inGameContext.CharacterGridManager.GetRandomEmptyGrid();
        var dataTable = DataTableManager.Instance.GetClassTable(classType, spawnType);

        var characterBehaviour = characterBehaviourPool.Get();
        characterBehaviour.Initialize(dataTable).Forget();
        characterBehaviour.SetToGrid(emptyGrid);

        IncreaseSpawnCount(classType, spawnType);
        InGameManager.Instance.InGameContext.InGameEvent.PublishSpawn(classType, spawnType);
        InGameManager.Instance.InGameContext.InGameEvent.PublishSpawnCountChanged(GetTotalSpawnCount());
    }

    #endregion

    #region Extension

    public int GetSpawnedCount(ClassType classType)
    {
        if (!spawnedClassesCount.TryGetValue(classType, out var bySpawnType))
            return 0;

        var result = 0;
        foreach (var classPair in bySpawnType)
        {
            result += classPair.Value;
        }

        return result;
    }

    public int GetSpawnedCount(ClassType classType, SpawnType spawnType)
    {
        if (!spawnedClassesCount.TryGetValue(classType, out var bySpawnType))
            return 0;

        return bySpawnType.GetValueOrDefault(spawnType, 0);
    }

    public int GetTotalSpawnCount()
    {
        var totalCount = 0;

        foreach (var classPair in spawnedClassesCount)
        {
            foreach (var spawnPair in classPair.Value)
            {
                totalCount += spawnPair.Value;
            }
        }

        return totalCount;
    }

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

    private void DecreaseSpawnCount(ClassType classType, SpawnType spawnType)
    {
        if (!spawnedClassesCount.TryGetValue(classType, out var bySpawnType))
            return;

        if (!bySpawnType.TryGetValue(spawnType, out var current))
            return;

        bySpawnType[spawnType] = Mathf.Max(0, current - 1);
    }

    #endregion

    #region Sell

    public bool SellCharacter(ClassType classType, SpawnType spawnType)
    {
        var activeCharacters = characterBehaviourPool.GetAllActive();
        CharacterBehaviour targetCharacter = null;

        foreach (var character in activeCharacters)
        {
            if (character.CurrentClass.classType == classType &&
                character.CurrentClass.spawnType == spawnType)
            {
                targetCharacter = character;
                break;
            }
        }

        ListPool<CharacterBehaviour>.Release(activeCharacters);

        if (targetCharacter == null)
            return false;

        targetCharacter.CurrentGrid.Clear();
        characterBehaviourPool.Release(targetCharacter);

        DecreaseSpawnCount(classType, spawnType);
        InGameManager.Instance.InGameContext.InGameEvent.PublishSpawnCountChanged(GetTotalSpawnCount());

        return true;
    }

    #endregion
}
