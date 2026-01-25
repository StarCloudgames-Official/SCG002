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

    public static async UniTask<RuntimeAnimatorController> GetAnimator(string path)
    {
        var runtimeAnimator = await Addressables.LoadAssetAsync<RuntimeAnimatorController>(path);
        return runtimeAnimator;
    }

    public static async UniTask<T> InstantiateAndGetComponent<T>(string path)
    {
        var handle = await Addressables.InstantiateAsync(path).Task;
        return handle.GetComponent<T>();
    }
    
    public static async UniTask<T> InstantiateAndGetComponent<T>(string path, Transform parent)
    {
        var handle = await Addressables.InstantiateAsync(path, parent).Task;
        return handle.GetComponent<T>();
    }
}
