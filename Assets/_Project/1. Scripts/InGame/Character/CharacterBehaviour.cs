using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CharacterBehaviour : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private ClassTable currentClass;
    
    public async Awaitable Initialize(ClassTable dataTable)
    {
        currentClass = dataTable;

        animator.runtimeAnimatorController = await GetAnimator();
    }

    private async Awaitable<RuntimeAnimatorController> GetAnimator()
    {
        var path = $"{currentClass.classType}_{currentClass.spawnType}_Animator";
        var runtimeAnimator = await Addressables.LoadAssetAsync<RuntimeAnimatorController>(path);
        return runtimeAnimator;
    }
}