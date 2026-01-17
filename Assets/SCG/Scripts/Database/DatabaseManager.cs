using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using StarCloudgamesLibrary;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private Dictionary<Type, DatabaseContainerBase> containers;

    protected override void Awake()
    {
        base.Awake();
        RegisterContainersByReflection().Forget();
    }

    private async UniTask RegisterContainersByReflection()
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

            await instance.LoadLocalData();
            instance.Initialize();

            containers.Add(type, instance);
        }
    }

    public T GetContainer<T>() where T : DatabaseContainerBase
    {
        if (containers.TryGetValue(typeof(T), out var container)) return (T)container;
        return null;
    }

    public async UniTask LocalInitialize()
    {
        var containerTaskList = new List<UniTask>();

        foreach (var container in containers.Values)
        {
            containerTaskList.Add(container.LoadLocalData());
        }

        await UniTask.WhenAll(containerTaskList);

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

    public void AddReward(RewardData rewardData)
    {
        var assetContainer = GetContainer<UserAssetDatabaseContainer>();
        assetContainer.IncreaseAsset(rewardData.AssetType, rewardData.Amount);
    }

    public void AddRewardList(List<RewardData> rewardDatas)
    {
        foreach (var rewardData in rewardDatas)
        {
            AddReward(rewardData);
        }
    }
}
