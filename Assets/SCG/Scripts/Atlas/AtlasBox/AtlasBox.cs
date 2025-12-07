using System;
using UnityEngine;
using UnityEngine.Rendering; // SerializedDictionary

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AtlasBox", menuName = "SCG/AtlasBox")]
public class AtlasBox : ScriptableObject
{
    [Serializable]
    public struct AtlasEntry
    {
        public AtlasType type;
        public SerializedDictionary<string, Sprite> sprites;
    }

#if UNITY_EDITOR
    [SerializeField] private DefaultAsset[] folders;
    public DefaultAsset[] Folders => folders;
#endif

    [SerializeField] private AtlasEntry[] atlasEntries;

    public Sprite GetSprite(AtlasType type, string spriteName)
    {
        if (type == AtlasType.None) return null;
        if (string.IsNullOrEmpty(spriteName)) return null;
        if (atlasEntries == null) return null;

        for (var i = 0; i < atlasEntries.Length; i++)
        {
            if (atlasEntries[i].type != type) continue;

            var cachedDictionary = atlasEntries[i].sprites;
            if (cachedDictionary != null && cachedDictionary.TryGetValue(spriteName, out var sprite))
                return sprite;

            break;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"[AtlasBox] Sprite not found. Atlas={type}, Name={spriteName}");
#endif
        return null;
    }

#if UNITY_EDITOR
    public void SetMapping(AtlasEntry[] entries)
    {
        atlasEntries = entries;
        EditorUtility.SetDirty(this);
    }
#endif
}