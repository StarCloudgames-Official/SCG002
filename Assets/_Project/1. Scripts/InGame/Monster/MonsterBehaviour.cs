using System.Collections;
using Cysharp.Text;
using UnityEngine;

[RequireComponent(typeof(MonsterMovement))]
[RequireComponent(typeof(MonsterHealth))]
public class MonsterBehaviour : CachedMonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private MonsterDataTable currentData;
    private MonsterMovement monsterMovement;
    private MonsterHealth monsterHealth;

    public async Awaitable Initialize(MonsterDataTable monsterData)
    {
        currentData = monsterData;
        
        animator.runtimeAnimatorController = await GetAnimatorController();

        InitializeComponents();
        monsterMovement.StartMovement();
    }

    private void InitializeComponents()
    {
        monsterMovement = GetComponent<MonsterMovement>();
        monsterMovement.Initialize(currentData, animator, spriteRenderer);
        
        monsterHealth = GetComponent<MonsterHealth>();
        monsterHealth.Initialize(currentData);
    }

    private async Awaitable<RuntimeAnimatorController> GetAnimatorController()
    {
        var path = ZString.Format(AddressableExtensions.MonsterAnimatorPath, currentData.monsterName);
        return await AddressableExtensions.GetAnimator(path);
    }
    
    public void GetDamage(float damage)
    {
        
    }
}