using System;
using UnityEngine;

public class InGameContext
{
    public InGameEnterInfo EnterInfo { get; set; }

    public InGameEvents InGameEvent { get; set; }

    public void Initialize(InGameEnterInfo enterInfo)
    {
        EnterInfo = enterInfo;
        InGameEvent = new InGameEvents();
    }

    #region InGameEvent

    public class InGameEvents
    {
        public event Action<DataTableEnum.ClassType, DataTableEnum.SpawnType> OnSpawn;

        public void PublishSpawn(DataTableEnum.ClassType classType, DataTableEnum.SpawnType spawnType)
        {
            OnSpawn?.Invoke(classType, spawnType);
        }
    }

    #endregion
}