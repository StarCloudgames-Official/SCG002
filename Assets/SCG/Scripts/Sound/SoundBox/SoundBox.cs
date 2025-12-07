using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "SoundBox", menuName = "SCG/SoundBox")]
public class SoundBox : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField] private DefaultAsset[] folders;
    public DefaultAsset[] Folders => folders;
#endif

    [SerializeField] private SerializedDictionary<SoundId, AudioClip> clips = new();
    
    public AudioClip GetClip(SoundId id)
    {
        return clips != null && clips.TryGetValue(id, out var clip) ? clip : null;
    }

#if UNITY_EDITOR
    public void SetMapping(SerializedDictionary<SoundId, AudioClip> dict)
    {
        clips = dict;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}