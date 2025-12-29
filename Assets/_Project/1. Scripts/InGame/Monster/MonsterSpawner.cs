using SCG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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

    public List<MonsterBehaviour> GetNearestMonster(Vector3 targetPosition, float range, int count)
    {
        var allActivating = monsterPool.GetAllActive();
        var candidates = ListPool<(MonsterBehaviour monster, float sqrDistance)>.Get();

        var rangeSqr = range * range;

        foreach (var monster in allActivating)
        {
            if(monster.IsDead)
                continue;
            
            var offset = monster.transform.position - targetPosition;
            var sqrDistance = offset.sqrMagnitude;
            if (sqrDistance <= rangeSqr)
            {
                candidates.Add((monster, sqrDistance));
            }
        }

        var result = ListPool<MonsterBehaviour>.Get();
        var resultCount = Mathf.Min(count, candidates.Count);

        for (var i = 0; i < resultCount; i++)
        {
            var minIndex = -1;
            var minSqrDistance = float.MaxValue;

            for (var j = 0; j < candidates.Count; j++)
            {
                if (candidates[j].sqrDistance < minSqrDistance)
                {
                    minSqrDistance = candidates[j].sqrDistance;
                    minIndex = j;
                }
            }

            if (minIndex != -1)
            {
                result.Add(candidates[minIndex].monster);
                candidates[minIndex] = (candidates[minIndex].monster, float.MaxValue);
            }
        }

        ListPool<(MonsterBehaviour, float)>.Release(candidates);
        ListPool<MonsterBehaviour>.Release(allActivating);

        return result;
    }
}