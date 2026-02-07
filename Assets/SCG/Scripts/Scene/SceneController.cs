using System;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
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

    public static async UniTask ChangeScene(Scene scene, bool isDirect = false)
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
        
        var temporaryScene = CreateTemporaryScene();

        CleanUpScene();
        await UnloadPreviousScene(previousHandle, previousSceneName);
        if (!await LoadTargetScene(ZString.Concat(scene)))
            return;

        SceneManager.SetActiveScene(currentSceneHandle.Value.Result.Scene);
        await UnloadTemporaryScene(temporaryScene);

        await Resources.UnloadUnusedAssets();
        GC.Collect();

        StartSceneStarter(scene);

        IsChangingScene = false;
    }

    private static async UniTask<bool> LoadTargetScene(string sceneName)
    {
        var loadTargetSceneHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        currentSceneHandle = loadTargetSceneHandle;

        try
        {
            await loadTargetSceneHandle.Task;
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: failed to load scene {sceneName}: {e}");
            IsChangingScene = false;
            return false;
        }

        if (loadTargetSceneHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"SceneController: scene load handle failed for {sceneName}");
            IsChangingScene = false;
            return false;
        }

        return true;
    }

    private static async UniTask UnloadPreviousScene(AsyncOperationHandle<SceneInstance>? previousHandle, string previousSceneName)
    {
        if (previousHandle.HasValue && previousHandle.Value.IsValid())
        {
            var unloadHandle = Addressables.UnloadSceneAsync(previousHandle.Value);
            while (!unloadHandle.IsDone)
            {
                await UniTask.NextFrame();
            }
        }
        else if (!string.IsNullOrEmpty(previousSceneName))
        {
            var unload = SceneManager.UnloadSceneAsync(previousSceneName);
            if (unload != null)
            {
                while (!unload.isDone)
                {
                    await UniTask.NextFrame();
                }
            }
        }
    }

    private static UnityEngine.SceneManagement.Scene CreateTemporaryScene()
    {
        var temporaryScene = SceneManager.CreateScene("SceneController_Temporary");
        SceneManager.SetActiveScene(temporaryScene);
        return temporaryScene;
    }

    private static async UniTask UnloadTemporaryScene(UnityEngine.SceneManagement.Scene temporaryScene)
    {
        if (!temporaryScene.IsValid() || !temporaryScene.isLoaded) return;

        var unload = SceneManager.UnloadSceneAsync(temporaryScene);
        if (unload == null) return;

        while (!unload.isDone)
        {
            await UniTask.NextFrame();
        }
    }

    private static void CleanUpScene()
    {
        AddressableExtensions.ReleaseAllInstances();
        SCGObjectPoolingManager.ReleaseAllPools();
    }

    private static void StartSceneStarter(Scene scene)
    {
        var starter = scene switch
        {
            Scene.Title => new TitleSceneStarter(),
            Scene.Lobby => new LobbySceneStarter(),
            Scene.InGame => new InGameSceneStarter(),
            _ => (SceneStarter)null
        };

        if (starter == null)
        {
            Debug.LogError($"Can't find scene starter for {scene}");
            return;
        }

        starter.StartScene().Forget();
    }
}
