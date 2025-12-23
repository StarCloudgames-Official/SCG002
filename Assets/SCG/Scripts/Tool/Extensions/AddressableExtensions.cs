using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressableExtensions
{
    public const string CharacterPath = "Character";
    public const string CharacterGridManagerPath = "CharacterGridManager";
    public const string MonsterPath = "Monster";
    
    public static async Awaitable<RuntimeAnimatorController> GetAnimator(string path)
    {
        var runtimeAnimator = await Addressables.LoadAssetAsync<RuntimeAnimatorController>(path);
        return runtimeAnimator;
    }
    
    public static async Awaitable<T> InstantiateAndGetComponent<T>(string path)
    {
        var handle = await Addressables.InstantiateAsync(path).Task;
        return handle.GetComponent<T>();
    }
}