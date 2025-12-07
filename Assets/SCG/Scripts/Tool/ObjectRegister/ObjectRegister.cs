using System.Collections.Generic;
using UnityEngine;

public static class ObjectRegister
{
    public enum RegisterType
    {
        None,
        UIPanel,
        UIPopup,
        UIOverPopup
    }

    private static readonly Dictionary<RegisterType, object> registeredObjects = new();

    public static void Register(RegisterType type, object obj)
    {
        registeredObjects[type] = obj;
    }

    public static void Unregister(RegisterType type)
    {
        registeredObjects.Remove(type);
    }

    public static object Get(RegisterType type)
    {
        if (!registeredObjects.TryGetValue(type, out var obj)) return null;
        if (obj is not Object uo || uo != null) return obj;
        
        registeredObjects.Remove(type);
        return null;
    }

    public static T Get<T>(RegisterType type) where T : class
    {
        return Get(type) as T;
    }

    public static bool TryGet(RegisterType type, out object obj)
    {
        obj = Get(type);
        return obj != null;
    }

    public static bool TryGet<T>(RegisterType type, out T value) where T : class
    {
        value = Get(type) as T;
        return value != null;
    }

    public static void Clear()
    {
        registeredObjects.Clear();
    }
}