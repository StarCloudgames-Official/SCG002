using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CharacterBehaviour : CachedMonoBehaviour
{
    [SerializeField] private Animator animator;

    private ClassTable currentClass;
    
    public async Awaitable Initialize(ClassTable dataTable)
    {
        currentClass = dataTable;

        animator.runtimeAnimatorController = await GetAnimator();
    }

    public void SetToGrid(CharacterGrid characterGrid)
    {
        CachedTransform.position = characterGrid.transform.position;
        characterGrid.SetCharacterBehaviour(this);
    }

    private async Awaitable<RuntimeAnimatorController> GetAnimator()
    {
        var path = $"{currentClass.classType}_{currentClass.spawnType}_Animator";
        var runtimeAnimator = await Addressables.LoadAssetAsync<RuntimeAnimatorController>(path);
        return runtimeAnimator;
    }
}