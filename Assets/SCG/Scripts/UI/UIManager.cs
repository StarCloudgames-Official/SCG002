using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;

public static class UIManager
{
    private static List<IUI> spawnedUIList = new();
    private static int blockerCount = 0;

    public static async Awaitable BlockUI()
    {
        blockerCount++;
        if (blockerCount == 1)
        {
            await OpenUI<UIBlocker>();
        }
    }

    public static void RemoveAllBlocker()
    {
        blockerCount = 0;
        RemoveBlocker();
    }

    public static void RemoveBlocker()
    {
        blockerCount--;
        if (blockerCount > 0)
            return;
        
        blockerCount = 0;
        
        if (TryGetSpawnedUI<UIBlocker>(out var blocker))
        {
            blocker.Close().Forget();
        }
    }

    public static async Awaitable<T> OpenUI<T>(object param = null) where T : Component, IUI
    {
        var ui = await GetUI<T>();
        ui.Open(param).Forget();
        return ui;
    }

    public static void RemoveUI(IUI ui)
    {
        if(spawnedUIList.Contains(ui))
            spawnedUIList.Remove(ui);
    }

    public static void CloseUI(IUI ui)
    {
        if (spawnedUIList.Contains(ui))
        {
            var targetUI = GetUI(ui);
            targetUI.Close().Forget();
        }
    }

    public static IUI GetUI(IUI ui)
    {
        if (spawnedUIList.Contains(ui))
            return spawnedUIList[spawnedUIList.IndexOf(ui)];
        
        return null;
    }

    public static void CloseAllUI()
    {
        RemoveAllBlocker();
        
        var list = new List<IUI>(spawnedUIList);
        foreach(var ui in list) ui.Close().Forget();
    }

    public static bool IsActivating(IUI ui)
    {
        return spawnedUIList.Contains(ui);
    }

    public static async Awaitable<T> GetUI<T>() where T : Component, IUI
    {
        if(TryGetSpawnedUI<T>(out var spawnedUI)) return spawnedUI; 
        
        var parentCanvas = ResolveParent(typeof(T));

        var addressableKey = UIAddressableKeys.Get<T>();
        if (string.IsNullOrEmpty(addressableKey))
            throw new System.Exception($"[UIManager] Addressable key not found for {typeof(T).Name}. Run SCG/Tools/Generate/Generate UI Addressable Keys.");

        var ui = await AddressableExtensions.InstantiateAndGetComponent<T>(addressableKey);
        ui.transform.SetParent(parentCanvas);
        
        if (ui == null) throw new MissingComponentException($"{typeof(T).Name} component not found on prefab.");

        spawnedUIList.Add(ui);
        return ui;
    }

    private static bool TryGetSpawnedUI<T>(out T ui) where T : Component, IUI
    {
        foreach (var iui in spawnedUIList)
        {
            if (iui is T existing && existing != null)
            {
                ui = existing;
                return true;
            }
        }
        
        ui = null;
        return false;
    }
    
    private static Transform ResolveParent(System.Type uiComponent)
    {
        if (typeof(UIPanel).IsAssignableFrom(uiComponent) && ObjectRegister.TryGet<RectTransform>(ObjectRegister.RegisterType.UIPanel, out var panelObj))
            return panelObj;
        if (typeof(UIPopup).IsAssignableFrom(uiComponent) && ObjectRegister.TryGet<RectTransform>(ObjectRegister.RegisterType.UIPopup, out var popupObj))
            return popupObj;
        if (typeof(UIOverPopup).IsAssignableFrom(uiComponent) && ObjectRegister.TryGet<RectTransform>(ObjectRegister.RegisterType.UIOverPopup, out var overObj))
            return overObj;

        return null;
    }

    public static void BackspaceClose()
    {
        if(spawnedUIList.Count <= 0) return;

        spawnedUIList[^1].OnBackSpace();
    }
}