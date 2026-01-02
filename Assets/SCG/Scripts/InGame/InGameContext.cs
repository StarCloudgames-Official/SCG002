using System;
using UnityEngine;

public class InGameContext
{
    public InGameEnterInfo EnterInfo { get; set; }
    public CharacterGridManager CharacterGridManager { get; set; }
    public StageManager StageManager { get; set; }
    public SpawnManager SpawnManager { get; set; }
    public InGameEvents InGameEvent { get; set; }

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

    public void Initialize(InGameEnterInfo enterInfo)
    {
        EnterInfo = enterInfo;
        InGameEvent = new InGameEvents();

        inGameCrystal = ConstantDataGetter.StartInGameSpawnCrystal;
    }

    public bool CanUseInGameCrystal(int amount)
    {
        return InGameCrystal >= amount;
    }

    public void UseInGameCrystal(int amount)
    {
        InGameCrystal -= amount;
    }

    #region InGameEvent

    public class InGameEvents
    {
        public event Action<DataTableEnum.ClassType, DataTableEnum.SpawnType> OnSpawn;
        public event Action<int> OnCrystalChange;
        public event Action<int> OnSpawnCountChanged;

        public void PublishCrystalChange(int crystal)
        {
            OnCrystalChange?.Invoke(crystal);
        }

        public void PublishSpawn(DataTableEnum.ClassType classType, DataTableEnum.SpawnType spawnType)
        {
            OnSpawn?.Invoke(classType, spawnType);
        }

        public void PublishSpawnCountChanged(int count)
        {
            OnSpawnCountChanged?.Invoke(count);
        }
    }

    #endregion
}