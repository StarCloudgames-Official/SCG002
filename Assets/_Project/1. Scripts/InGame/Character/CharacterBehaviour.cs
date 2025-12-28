using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterTouch))]
public class CharacterBehaviour : CachedMonoBehaviour
{
    [SerializeField] private Animator animator;

    public CharacterGrid CurrentGrid { get; set; }
    public ClassTable CurrentClass { get; private set; }
    
    private CharacterTouch characterTouch;

    public async Awaitable Initialize(ClassTable dataTable)
    {
        CurrentClass = dataTable;
        
        InitializeComponents();

        animator.runtimeAnimatorController = await GetAnimator();
    }

    private void InitializeComponents()
    {
        characterTouch = GetComponent<CharacterTouch>();
        characterTouch.Initialize(this);
    }

    public void SetToGrid(CharacterGrid characterGrid)
    {
        CurrentGrid = characterGrid;
        
        CachedTransform.position = characterGrid.transform.position;
        characterGrid.SetCharacterBehaviour(this);
    }

    private async Awaitable<RuntimeAnimatorController> GetAnimator()
    {
        var path = $"{CurrentClass.classType}_{CurrentClass.spawnType}_Animator";
        return await AddressableExtensions.GetAnimator(path);
    }
}