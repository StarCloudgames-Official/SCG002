using System;
using System.Collections.Generic;
using Cysharp.Text;
using GUPS.AntiCheat.Protected.Storage.Prefs;
using UnityEngine;

public static class RedDotSystem
{
    private const string SaveKey = "RedDotSystem_Save";

    private static readonly Dictionary<string, List<RedDot>> redDots = new();
    private static readonly Dictionary<string, int> activeCounts = new();
    private static readonly Dictionary<string, List<string>> cachedSegments = new();
    
    private static readonly HashSet<string> rawOnPaths = new();

    private static bool loaded;

    #region Public Functions

    public static void Load()
    {
        if (loaded) return;
        
        loaded = true;

        if (!ProtectedPlayerPrefs.HasKey(SaveKey)) return;

        var json = ProtectedPlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<RedDotSaveData>(json);
        if (data?.onPaths == null) return;

        rawOnPaths.Clear();
        activeCounts.Clear();

        foreach (var path in data.onPaths)
        {
            if (string.IsNullOrEmpty(path)) continue;

            rawOnPaths.Add(path);
            ApplyToHierarchy(path, true);
        }

        RefreshAllViews();
    }

    public static void Save()
    {
        var data = new RedDotSaveData
        {
            onPaths = new List<string>(rawOnPaths)
        };

        var json = JsonUtility.ToJson(data);
        ProtectedPlayerPrefs.SetString(SaveKey, json);
        ProtectedPlayerPrefs.Save();
    }

    public static void ClearAll()
    {
        rawOnPaths.Clear();
        activeCounts.Clear();

        foreach (var kvp in redDots)
        {
            foreach (var dot in kvp.Value) dot.SetVisual(false);
        }

        ProtectedPlayerPrefs.DeleteKey(SaveKey);
    }

    public static void Set(string path, bool active)
    {
        if (string.IsNullOrEmpty(path)) return;

        var changed = active ? rawOnPaths.Add(path) : rawOnPaths.Remove(path);
        if (!changed) return;

        ApplyToHierarchy(path, active);
        Save();
    }

    public static bool GetIsOn(string path)
    {
        return activeCounts.TryGetValue(path, out var v) && v > 0;
    }

    #endregion

    #region Register

    internal static void Register(RedDot dot)
    {
        if (dot == null) return;
        
        var path = dot.Path;
        if (string.IsNullOrEmpty(path)) return;

        if (!redDots.TryGetValue(path, out var list))
        {
            list = new List<RedDot>();
            redDots[path] = list;
        }

        if (!list.Contains(dot)) list.Add(dot);

        dot.SetVisual(GetIsOn(path));
    }

    internal static void Unregister(RedDot dot)
    {
        if (dot == null) return;
        
        var path = dot.Path;
        if (string.IsNullOrEmpty(path)) return;

        if (!redDots.TryGetValue(path, out var list)) return;
        list.Remove(dot);
    }

    #endregion

    #region Private Functions

    private static void ApplyToHierarchy(string path, bool active)
    {
        var segments = GetPathSegments(path);
        if (segments == null || segments.Count == 0) return;

        var delta = active ? 1 : -1;

        foreach (var seg in segments)
        {
            var count = activeCounts.GetValueOrDefault(seg, 0);

            count += delta;
            if (count < 0) count = 0;

            activeCounts[seg] = count;
            var isOn = count > 0;

            if (!redDots.TryGetValue(seg, out var list)) continue;

            foreach (var dot in list)
            {
                if (dot) dot.SetVisual(isOn);
            }
        }
    }

    private static List<string> GetPathSegments(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (cachedSegments.TryGetValue(path, out var cached)) return cached;
            
        var result = new List<string>();

        if (!path.Contains("/"))
        {
            result.Add(path);
        }
        else
        {
            var parts = path.Split('/');
            var current = parts[0];
            
            result.Add(current);

            for (var i = 1; i < parts.Length; i++)
            {
                current = ZString.Format("{0}{1}", current, ("/" + parts[i]));
                result.Add(current);
            }
        }

        cachedSegments[path] = result;
        return result;
    }

    private static void RefreshAllViews()
    {
        foreach (var kvp in redDots)
        {
            var isOn = GetIsOn(kvp.Key);
            foreach (var dot in kvp.Value)
            {
                if (dot != null) dot.SetVisual(isOn);
            }
        }
    }

    [Serializable]
    private class RedDotSaveData
    {
        public List<string> onPaths;
    }

    #endregion
}