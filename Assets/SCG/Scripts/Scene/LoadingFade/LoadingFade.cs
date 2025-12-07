using UnityEngine;
using UnityEngine.AddressableAssets;

public static class LoadingFade
{
    private const string LoadingFadeCanvasPath = "Assets/_Project/Prefab/UI/LoadingFadeCanvas.prefab";

    private static LoadingFadeCanvas LoadingFadeCanvas;
    
    public static async Awaitable StartFadeIn()
    {
        if (LoadingFadeCanvas) return;
        LoadingFadeCanvas = await GetLoadingFadeCanvas();
        await LoadingFadeCanvas.StartFadeIn();
    }
    
    public static async Awaitable StartFadeOut()
    {
        if (!LoadingFadeCanvas) return;
        await LoadingFadeCanvas.StartFadeOut();
        
        Addressables.ReleaseInstance(LoadingFadeCanvas.gameObject);
        LoadingFadeCanvas = null;
    }

    private static async Awaitable<LoadingFadeCanvas> GetLoadingFadeCanvas()
    {
        var handle = Addressables.InstantiateAsync(LoadingFadeCanvasPath);
        var gameObject = await handle.Task;
        return gameObject.GetComponent<LoadingFadeCanvas>();
    }
}