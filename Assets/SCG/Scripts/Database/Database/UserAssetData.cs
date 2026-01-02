using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class UserAssetData
{
    public Dictionary<DataTableEnum.AssetType, float> assets;
}

[AttributeExtensions.AutoRegisterDatabaseContainer]
public class UserAssetDatabaseContainer : DatabaseContainer<UserAssetData>
{
    protected override string PreferenceKey => "UserAssetDatabase";

    private Dictionary<DataTableEnum.AssetType, float> Assets => Data.assets;
    
    public override void Initialize()
    {
        if(Assets == null)
            Data.assets = new Dictionary<DataTableEnum.AssetType, float>();
    }

    public float GetAsset(DataTableEnum.AssetType assetType)
    {
        return Assets.GetValueOrDefault(assetType, 0);
    }

    public void IncreaseAsset(DataTableEnum.AssetType assetType, float value)
    {
        Assets.TryAdd(assetType, 0);
        Assets[assetType] += value;
        SetDirty(true);
    }

    public void DecreaseAsset(DataTableEnum.AssetType assetType, float value)
    {
        Assets.TryAdd(assetType, 0);
        Assets[assetType] -= value;

        if (Assets[assetType] < 0)
            Assets[assetType] = 0;

        SetDirty(true);
    }

    public bool CanUseAsset(DataTableEnum.AssetType assetType, float amount)
    {
        return GetAsset(assetType) >= amount;
    }
}