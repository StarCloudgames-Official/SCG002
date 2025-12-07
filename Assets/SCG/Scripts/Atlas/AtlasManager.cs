using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public static class AtlasManager
{
    private static AtlasBox AtlasBox;
    
    private const string AddressableKey = "AtlasBox";

    public static void SetSprite(this Image targetImage, AtlasType atlasType, string spriteName)
    {
        var sprite = GetSprite(atlasType, spriteName);
        if (sprite == null) return;
        targetImage.sprite = sprite;
    }

    public static async Awaitable Initialize()
    {
        var handle = Addressables.LoadAssetAsync<AtlasBox>(AddressableKey);
        AtlasBox = await handle;
    }

    public static Sprite GetSprite(AtlasType atlasType, string spriteName)
    {
        return AtlasBox.GetSprite(atlasType, spriteName);
    }
}