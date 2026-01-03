using UnityEngine;
using UnityEngine.AddressableAssets;

public static class LoadingFade
{
    private const string LoadingFadeCanvasPath = "Assets/_Project/Prefab/UI/Common/LoadingFadeCanvas.prefab";

    private static LoadingFadeCanvas LoadingFadeCanvas;
    
    public static async Awaitable StartFadeIn()
    {
        if (LoadingFadeCanvas) return;
        LoadingFadeCanvas = await AddressableExtensions.InstantiateAndGetComponent<LoadingFadeCanvas>(LoadingFadeCanvasPath);
        await LoadingFadeCanvas.StartFadeIn();
    }
    
    public static async Awaitable StartFadeOut()
    {
        if (!LoadingFadeCanvas) return;
        await LoadingFadeCanvas.StartFadeOut();
        
        Addressables.ReleaseInstance(LoadingFadeCanvas.gameObject);
        LoadingFadeCanvas = null;
    }
}