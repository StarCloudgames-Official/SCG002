using System;
using UnityEngine;

public class InGameContext
{
    public InGameEnterInfo EnterInfo { get; set; }

    public InGameEvents InGameEvent;

    public void Initialize(InGameEnterInfo enterInfo)
    {
        EnterInfo = enterInfo;
        InGameEvent = new InGameEvents();
    }

    public class InGameEvents
    {
        public event Action<DataTableEnum.SpawnType> OnSpawn;

        public void PublishSpawn(DataTableEnum.SpawnType spawnType)
        {
            OnSpawn?.Invoke(spawnType);
        }
    }
}