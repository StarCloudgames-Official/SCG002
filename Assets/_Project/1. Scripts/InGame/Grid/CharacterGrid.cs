using UnityEngine;

public class CharacterGrid : CachedMonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private CharacterBehaviour currentCharacterBehaviour;
    
    public float GetXSize => spriteRenderer.bounds.size.x;
    public float GetYSize => spriteRenderer.bounds.size.y;
    public bool IsEmpty => currentCharacterBehaviour == null;

    public void SetCharacterBehaviour(CharacterBehaviour behaviour)
    {
        currentCharacterBehaviour = behaviour;
    }
}