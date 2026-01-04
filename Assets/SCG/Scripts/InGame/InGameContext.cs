using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameContext
{
    public InGameEnterInfo EnterInfo { get; set; }
    public CharacterGridManager CharacterGridManager { get; set; }
    public StageManager StageManager { get; set; }
    public SpawnManager SpawnManager { get; set; }
    public InGameEvents InGameEvent { get; set; }

    private int luckyPoint;

    public int LuckyPoint
    {
        get => luckyPoint;
        set
        {
            luckyPoint = value;
            InGameEvent.PublishLuckyPointChange(luckyPoint);
        }
    }

    private int inGameCrystal;

    public int InGameCrystal
    {
        get => inGameCrystal;
        set
        {
            inGameCrystal = value;
            InGameEvent.PublishCrystalChange(inGameCrystal);
        }
    }

    private Dictionary<DataTableEnum.ClassType, int> classEnhancements;
    public Dictionary<DataTableEnum.ClassType, int> ClassEnhancements => classEnhancements;

    public void Initialize(InGameEnterInfo enterInfo)
    {
        EnterInfo = enterInfo;
        InGameEvent = new InGameEvents();

        inGameCrystal = ConstantDataGetter.StartInGameSpawnCrystal;
        luckyPoint = 0;

        InitializeClassEnhancements();
    }

    private void InitializeClassEnhancements()
    {
        classEnhancements = new Dictionary<DataTableEnum.ClassType, int>();
        foreach (DataTableEnum.ClassType classType in Enum.GetValues(typeof(DataTableEnum.ClassType)))
        {
            if (classType == DataTableEnum.ClassType.None)
                continue;

            classEnhancements[classType] = 0;
        }
    }

    public bool CanUseInGameCrystal(int amount)
    {
        return InGameCrystal >= amount;
    }

    public void UseInGameCrystal(int amount)
    {
        InGameCrystal -= amount;
    }

    public bool CanUseLuckyPoint(int amount)
    {
        return LuckyPoint >= amount;
    }

    public void UseLuckyPoint(int amount)
    {
        LuckyPoint -= amount;
    }

    public int GetClassEnhanceLevel(DataTableEnum.ClassType classType)
    {
        return classEnhancements.GetValueOrDefault(classType, 0);
    }

    public void EnhanceClass(DataTableEnum.ClassType classType)
    {
        if (!classEnhancements.TryGetValue(classType, out var level))
            return;
        
        var nextLevel = level + 1;
        if(DataTableManager.Instance.GetClassEnhanceRatio(nextLevel) == null)
            return;

        classEnhancements[classType] = nextLevel;
        InGameEvent.PublishClassEnhancementChange(classType, nextLevel);
    }

    #region InGameEvent

    public class InGameEvents
    {
        public event Action<DataTableEnum.ClassType, DataTableEnum.SpawnType> OnSpawn;
        public event Action<int> OnCrystalChange;
        public event Action<int> OnLuckyPointChange;
        public event Action<int> OnSpawnCountChanged;
        public event Action<DataTableEnum.ClassType, int> OnClassEnhancementChange;

        public void PublishCrystalChange(int crystal)
        {
            OnCrystalChange?.Invoke(crystal);
        }

        public void PublishLuckyPointChange(int luckyPoint)
        {
            OnLuckyPointChange?.Invoke(luckyPoint);
        }

        public void PublishSpawn(DataTableEnum.ClassType classType, DataTableEnum.SpawnType spawnType)
        {
            OnSpawn?.Invoke(classType, spawnType);
        }

        public void PublishSpawnCountChanged(int count)
        {
            OnSpawnCountChanged?.Invoke(count);
        }

        public void PublishClassEnhancementChange(DataTableEnum.ClassType classType, int level)
        {
            OnClassEnhancementChange?.Invoke(classType, level);
        }
    }

    #endregion
}