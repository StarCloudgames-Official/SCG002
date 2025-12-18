using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressableExtensions
{
    public const string CharacterPath = "Character";
    public const string CharacterGridManagerPath = "CharacterGridManager";
    
    public static async Awaitable<T> InstantiateAndGetComponent<T>(string path)
    {
        var handle = await Addressables.InstantiateAsync(path).Task;
        return handle.GetComponent<T>();
    }
}