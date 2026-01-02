using System.Collections.Generic;

public static class ConstantDataGetter
{
    private static readonly Dictionary<string, int> intCache = new();
    private static readonly Dictionary<string, float> floatCache = new();

    private static int GetInt(string key)
    {
        if (intCache.TryGetValue(key, out var cached))
            return cached;

        var value = DataTableManager.Instance.GetConstantDataTable(key).value;
        var parsed = int.Parse(value);
        intCache[key] = parsed;
        return parsed;
    }

    private static float GetFloat(string key)
    {
        if (floatCache.TryGetValue(key, out var cached))
            return cached;

        var value = DataTableManager.Instance.GetConstantDataTable(key).value;
        var parsed = float.Parse(value);
        floatCache[key] = parsed;
        return parsed;
    }

    public static int SpawnCrystalPrice => GetInt("SpawnCrystalPrice");
    public static int StartInGameSpawnCrystal => GetInt("StartInGameCrystal");
}