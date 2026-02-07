using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressableExtensions
{
    public const string CharacterPath = "Character";
    public const string CharacterGridManagerPath = "CharacterGridManager";
    public const string MonsterPath = "Monster";
    public const string MonsterAnimatorPath = "Assets/_Project/Animation/Monster/Animator/{0}.controller";
    public const string UIRewardItemPath = "Assets/_Project/Prefab/UI/Common/UIItemRewardItem.prefab";
    public const string MapPrefabPath = "Assets/_Project/Prefab/Map/{0}.prefab";

    private static readonly List<GameObject> trackedInstances = new();

    public static async UniTask<RuntimeAnimatorController> GetAnimator(string path)
    {
        var runtimeAnimator = await Addressables.LoadAssetAsync<RuntimeAnimatorController>(path);
        return runtimeAnimator;
    }

    public static async UniTask<T> InstantiateAndGetComponent<T>(string path, bool tracked = true)
    {
        var go = await Addressables.InstantiateAsync(path).Task;
        if (tracked) trackedInstances.Add(go);
        return go.GetComponent<T>();
    }

    public static async UniTask<T> InstantiateAndGetComponent<T>(string path, Transform parent, bool tracked = true)
    {
        var go = await Addressables.InstantiateAsync(path, parent).Task;
        if (tracked) trackedInstances.Add(go);
        return go.GetComponent<T>();
    }

    public static async UniTask<GameObject> InstantiateTracked(string path)
    {
        var go = await Addressables.InstantiateAsync(path).Task;
        trackedInstances.Add(go);
        return go;
    }

    public static void ReleaseAllInstances()
    {
        foreach (var go in trackedInstances)
        {
            if (go != null)
                Addressables.ReleaseInstance(go);
        }
        trackedInstances.Clear();
    }
}
