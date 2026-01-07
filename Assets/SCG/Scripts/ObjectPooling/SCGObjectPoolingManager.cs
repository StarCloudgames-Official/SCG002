using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class SCGObjectPoolingManager
{
    private static readonly Dictionary<Type, object> pools = new();
    private static readonly Dictionary<Type, IDisposable> disposablePools = new();

    public static SCGObjectPooling<T> GetPool<T>() where T : Component
    {
        return pools.TryGetValue(typeof(T), out var pool) ? pool as SCGObjectPooling<T> : null;
    }

    public static bool HasPool<T>() where T : Component => pools.ContainsKey(typeof(T));

    #region 프리팹 기반 풀 생성

    public static SCGObjectPooling<T> CreatePool<T>(
        T prefab,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        Action<T> onGet = null,
        Action<T> onRelease = null) where T : Component
    {
        var type = typeof(T);

        if (pools.TryGetValue(type, out var existingPool))
            return existingPool as SCGObjectPooling<T>;

        var newPool = new SCGObjectPooling<T>(prefab, parent, defaultCapacity, maxSize, onGet, onRelease);

        pools[type] = newPool;
        disposablePools[type] = newPool;

        return newPool;
    }

    public static SCGObjectPooling<T> GetOrCreatePool<T>(
        T prefab,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        Action<T> onGet = null,
        Action<T> onRelease = null) where T : Component
    {
        return GetPool<T>() ?? CreatePool(prefab, parent, defaultCapacity, maxSize, onGet, onRelease);
    }

    #endregion

    #region Addressable 기반 비동기 풀 생성

    public static async UniTask<SCGObjectPooling<T>> CreatePoolAsync<T>(
        string addressableKey,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        Action<T> onGet = null,
        Action<T> onRelease = null) where T : Component
    {
        var type = typeof(T);

        if (pools.TryGetValue(type, out var existingPool))
            return existingPool as SCGObjectPooling<T>;

        var newPool = await SCGObjectPooling<T>.CreateAsync(
            addressableKey,
            parent,
            defaultCapacity,
            maxSize,
            onGet,
            onRelease
        );

        if (newPool == null)
        {
            Debug.LogError($"[SCGObjectPoolingManager] Failed to create pool for {typeof(T).Name} with key: {addressableKey}");
            return null;
        }

        pools[type] = newPool;
        disposablePools[type] = newPool;

        return newPool;
    }

    public static async UniTask<SCGObjectPooling<T>> GetOrCreatePoolAsync<T>(
        string addressableKey,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        Action<T> onGet = null,
        Action<T> onRelease = null) where T : Component
    {
        var existingPool = GetPool<T>();
        return existingPool ?? await CreatePoolAsync(addressableKey, parent, defaultCapacity, maxSize, onGet, onRelease);
    }

    public static async UniTask<SCGObjectPooling<T>> GetOrCreatePoolAsync<T>(
        string addressableKey,
        int preloadCount,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        Action<T> onGet = null,
        Action<T> onRelease = null) where T : Component
    {
        var existingPool = GetPool<T>();
        if (existingPool != null)
            return existingPool;

        var newPool = await CreatePoolAsync(addressableKey, parent, defaultCapacity, maxSize, onGet, onRelease);

        if (newPool == null || preloadCount <= 0)
            return newPool;

        var preloadList = new List<T>(preloadCount);
        for (var i = 0; i < preloadCount; i++)
        {
            preloadList.Add(newPool.Get());
        }

        foreach (var obj in preloadList)
        {
            newPool.Release(obj);
        }

        return newPool;
    }

    #endregion

    #region 풀에서 오브젝트 가져오기/반환

    public static T Get<T>() where T : Component => GetPool<T>()?.Get();

    public static void Release<T>(T element) where T : Component
    {
        if (element == null) return;
        GetPool<T>()?.Release(element);
    }

    #endregion

    #region 풀 해제

    public static void ReleasePool<T>() where T : Component
    {
        var type = typeof(T);

        if (disposablePools.TryGetValue(type, out var disposable))
        {
            disposable.Dispose();
            disposablePools.Remove(type);
        }

        pools.Remove(type);
    }

    public static void ReleaseAllPools()
    {
        foreach (var disposable in disposablePools.Values)
        {
            disposable.Dispose();
        }

        pools.Clear();
        disposablePools.Clear();
    }

    public static void ClearAllInactivePools()
    {
        foreach (var pool in pools.Values)
        {
            if (pool is IDisposable disposablePool)
            {
                var poolType = pool.GetType();
                poolType.GetMethod("Clear")?.Invoke(pool, null);
            }
        }
    }

    #endregion

    #region 유틸리티

    public static int PoolCount => pools.Count;

    public static IEnumerable<Type> RegisteredPoolTypes => pools.Keys;

    #endregion
}
