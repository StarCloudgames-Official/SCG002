using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MemoryPack;
using StarCloudgamesLibrary;
using UnityEngine;

public partial class DataTableManager : Singleton<DataTableManager>
{
    private DataTableFile db;
    private readonly Dictionary<Type, object> cache = new();
    private readonly Dictionary<string, byte[]> tableMap = new();

    public override async UniTask Initialize()
    {
        await Load();
    }

    private async UniTask Load()
    {
        var request = Resources.LoadAsync<TextAsset>("DataTable");

        while (!request.isDone)
            await UniTask.NextFrame();

        var dataTable = request.asset as TextAsset;

        if (dataTable == null)
        {
            Debug.LogError("DataTableManager: Resources/DataTable(TextAsset)을 찾을 수 없습니다.");
            db = null;
            return;
        }

        db = MemoryPackSerializer.Deserialize<DataTableFile>(dataTable.bytes);

        if (db == null)
        {
            Debug.LogError("DataTableManager: DataTableFile 역직렬화에 실패했습니다.");
            return;
        }

        tableMap.Clear();
        foreach (var entry in db.Tables)
            tableMap[entry.TableName] = entry.Payload;

        await UniTask.NextFrame();
    }

    protected List<T> GetTable<T>()
    {
        var type = typeof(T);

        if (cache.TryGetValue(type, out var cached))
            return (List<T>)cached;

        string tableName = type.Name;

        if (!tableMap.TryGetValue(tableName, out var payload))
            return null;

        var list = MemoryPackSerializer.Deserialize<List<T>>(payload);
        cache[type] = list;
        return list;
    }
}
