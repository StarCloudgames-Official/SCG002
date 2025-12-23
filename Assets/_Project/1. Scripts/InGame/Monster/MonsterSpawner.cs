using SCG;
using System;
using System.Collections;
using UnityEngine;

public class MonsterSpawner
{
    private SCGObjectPooling<MonsterBehaviour> monsterPool;

    public async Awaitable Initialize()
    {
        monsterPool = await SCGObjectPoolingManager.GetOrCreatePoolAsync<MonsterBehaviour>(AddressableExtensions.MonsterPath, 30, InGameManager.Instance.CachedTransform);
    }

    public async Awaitable StartSpawn(MonsterDataTable monsterData, int targetSpawnCount, float spawnDelay)
    {
        for (var spawnedCount = 0; spawnedCount < targetSpawnCount; spawnedCount++)
        {
            var newMonster = monsterPool.Get();
            newMonster.transform.position = MonsterPath.SpawnPosition;
            
            await newMonster.Initialize(monsterData);
            await Awaitable.WaitForSecondsAsync(spawnDelay);
        }
    }
}