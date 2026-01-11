using UnityEngine;

public class CharacterGrid : CachedMonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public CharacterBehaviour CurrentBehaviour { get; private set; }

    public float GetXSize
    {
        get
        {
            if (!isSizeCached) CacheSize();
            return cachedXSize;
        }
    }
    public float GetYSize
    {
        get
        {
            if (!isSizeCached) CacheSize();
            return cachedYSize;
        }
    }
    public bool IsEmpty => CurrentBehaviour == null;
        
    private float cachedXSize;
    private float cachedYSize;
    private bool isSizeCached;

    public void SetCharacterBehaviour(CharacterBehaviour behaviour)
    {
        CurrentBehaviour = behaviour;
    }

    public void Clear()
    {
        CurrentBehaviour = null;
    }

    private void CacheSize()
    {
        cachedXSize = spriteRenderer.bounds.size.x;
        cachedYSize = spriteRenderer.bounds.size.y;
        isSizeCached = true;
    }
}