using System;
using System.Collections.Generic;
using System.Reflection;
using StarCloudgamesLibrary;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private Dictionary<Type, DatabaseContainerBase> containers;

    protected override void Awake()
    {
        base.Awake();
        RegisterContainersByReflection();
    }

    private void RegisterContainersByReflection()
    {
        containers = new Dictionary<Type, DatabaseContainerBase>();

        var baseType = typeof(DatabaseContainerBase);
        var assembly = baseType.Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract) continue;
            if (!baseType.IsAssignableFrom(type)) continue;
            if (type.GetCustomAttribute<AttributeExtensions.AutoRegisterDatabaseContainerAttribute>() == null) continue;
            if (Activator.CreateInstance(type) is not DatabaseContainerBase instance) continue;

            instance.LoadLocalData();
            instance.Initialize();

            containers.Add(type, instance);
        }
    }

    public T GetContainer<T>() where T : DatabaseContainerBase
    {
        if (containers.TryGetValue(typeof(T), out var container)) return (T)container;
        return null;
    }

    public async Awaitable LocalInitialize()
    {
        var containerAwaitableList = new List<Awaitable>();
        
        foreach (var container in containers.Values)
        {
            containerAwaitableList.Add(container.LoadLocalData());
        }

        await containerAwaitableList.WhenAll();

        foreach (var container in containers.Values)
        {
            container.Initialize();
        }
    }

    private void LateUpdate()
    {
        foreach (var container in containers.Values)
        {
            if (container.IsDirty)
            {
                container.SaveToLocal();
            }
        }
    }

    public void AddRewardGroups(IReadOnlyList<RewardGroupDataTable> rewardGroups)
    {
        foreach (var rewardGroup in rewardGroups)
        {
            //TODO : Craete AssetDatabase and add assets
        }
    }
}
