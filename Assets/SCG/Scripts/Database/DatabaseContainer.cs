using System;
using System.Threading.Tasks;
using GUPS.AntiCheat.Protected.Storage.Prefs;
using MemoryPack;
using UnityEngine;

public abstract class DatabaseContainer<T> : DatabaseContainerBase
{
    protected T Data;

    public override void Initialize()
    {
    }

    public override async Awaitable LoadLocalData()
    {
        if (!ProtectedPlayerPrefs.HasKey(PreferenceKey)) return;

        var savedData = ProtectedPlayerPrefs.GetString(PreferenceKey);
        var base64 = Convert.FromBase64String(savedData);

        Data = await Task.Run(() => MemoryPackSerializer.Deserialize<T>(base64));
    }

    public override void SaveToLocal()
    {
        SetDirty(false);
        
        var serializedData = MemoryPackSerializer.Serialize(Data);
        var base64String = Convert.ToBase64String(serializedData);
        ProtectedPlayerPrefs.SetString(PreferenceKey, base64String);
    }
}