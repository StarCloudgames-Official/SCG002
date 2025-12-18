using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class SceneController
{
    public enum Scene
    {
        Title,
        Lobby,
        InGame
    }
    
    private static bool IsChangingScene = false;
    private static AsyncOperationHandle<SceneInstance>? currentSceneHandle;

    private static bool CanChangeScene()
    {
        if (IsChangingScene) return false;
        IsChangingScene = true;
        return true;
    }

    public static async Awaitable ChangeScene(Scene scene, bool isDirect = false)
    {
        if (!CanChangeScene()) return;

        if (!isDirect)
        {
            await UIManager.BlockUI();

            await LoadingFade.StartFadeIn();
            UIManager.CloseAllUI();
        }

        var previousHandle = currentSceneHandle;
        var previousSceneName = SceneManager.GetActiveScene().name;

        var temporaryScene = SceneManager.CreateScene("SceneController_Temporary");
        SceneManager.SetActiveScene(temporaryScene);

        await UnloadPreviousScene(previousHandle, previousSceneName);
        currentSceneHandle = null;

        var loadHandle = Addressables.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive);
        currentSceneHandle = loadHandle;

        SceneInstance newScene;
        try
        {
            newScene = await loadHandle.Task;
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: failed to load scene {scene}: {e}");
            IsChangingScene = false;
            return;
        }

        if (loadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"SceneController: scene load handle failed for {scene}");
            IsChangingScene = false;
            return;
        }

        SceneManager.SetActiveScene(newScene.Scene);
        await UnloadTemporaryScene(temporaryScene);

        await Resources.UnloadUnusedAssets();
        GC.Collect();

        StartSceneStarter(scene);
        
        IsChangingScene = false;
    }

    private static async Awaitable UnloadPreviousScene(AsyncOperationHandle<SceneInstance>? previousHandle, string previousSceneName)
    {
        if (previousHandle.HasValue && previousHandle.Value.IsValid())
        {
            var unloadHandle = Addressables.UnloadSceneAsync(previousHandle.Value);
            while (!unloadHandle.IsDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
        else if (!string.IsNullOrEmpty(previousSceneName))
        {
            var unload = SceneManager.UnloadSceneAsync(previousSceneName);
            if (unload != null)
            {
                while (!unload.isDone)
                {
                    await Awaitable.NextFrameAsync();
                }
            }
        }
    }

    private static async Awaitable UnloadTemporaryScene(UnityEngine.SceneManagement.Scene temporaryScene)
    {
        if (!temporaryScene.IsValid() || !temporaryScene.isLoaded) return;

        var unload = SceneManager.UnloadSceneAsync(temporaryScene);
        if (unload == null) return;

        while (!unload.isDone)
        {
            await Awaitable.NextFrameAsync();
        }
    }

    private static void StartSceneStarter(Scene scene)
    {
        var starterType = Type.GetType($"{scene.ToString()}SceneStarter");
        if (starterType == null)
        {
            Debug.LogError($"Can't find scene starter type");
            return;
        }
        
        var starter = Activator.CreateInstance(starterType) as SceneStarter;
        starter?.StartScene().Forget();
    }
}
