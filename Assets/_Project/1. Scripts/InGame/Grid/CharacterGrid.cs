using UnityEngine;

public class CharacterGrid : CachedMonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public CharacterBehaviour CurrentBehaviour { get; private set; }

    public float GetXSize => spriteRenderer.bounds.size.x;
    public float GetYSize => spriteRenderer.bounds.size.y;
    public bool IsEmpty => CurrentBehaviour == null;

    public void SetCharacterBehaviour(CharacterBehaviour behaviour)
    {
        CurrentBehaviour = behaviour;
    }

    public void Clear()
    {
        CurrentBehaviour = null;
    }
}