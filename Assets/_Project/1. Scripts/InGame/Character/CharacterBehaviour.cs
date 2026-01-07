using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CharacterTouch))]
[RequireComponent(typeof(CharacterAttack))]
public class CharacterBehaviour : CachedMonoBehaviour
{
    [SerializeField] private Animator animator;

    public CharacterGrid CurrentGrid { get; set; }
    public ClassTable CurrentClass { get; private set; }
    public bool CanInteract { get; set; }

    private CharacterTouch characterTouch;
    private CharacterAttack characterAttack;

    public async UniTask Initialize(ClassTable dataTable)
    {
        CurrentClass = dataTable;

        InitializeComponents();

        animator.runtimeAnimatorController = await GetAnimator();

        CanInteract = true;
    }

    private void InitializeComponents()
    {
        characterTouch = GetComponent<CharacterTouch>();
        characterTouch.Initialize(this);

        characterAttack = GetComponent<CharacterAttack>();
        characterAttack.Initialize(this, animator);
    }

    public void SetToGrid(CharacterGrid characterGrid)
    {
        CurrentGrid = characterGrid;

        CachedTransform.position = characterGrid.transform.position;
        characterGrid.SetCharacterBehaviour(this);
    }

    private async UniTask<RuntimeAnimatorController> GetAnimator()
    {
        var path = $"{CurrentClass.classType}_{CurrentClass.spawnType}_Animator";
        return await AddressableExtensions.GetAnimator(path);
    }
}
