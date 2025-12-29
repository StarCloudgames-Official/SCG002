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
    
    private static readonly int death = Animator.StringToHash("Death");
    public bool IsDead => monsterHealth.IsDead();

    public async Awaitable Initialize(MonsterDataTable monsterData)
    {
        currentData = monsterData;
        
        animator.runtimeAnimatorController = await GetAnimatorController();
        
        InitializeComponents();

        monsterMovement.StartMovement();
    }

    private void InitializeComponents()
    {
        monsterMovement ??= GetComponent<MonsterMovement>();
        monsterHealth ??= GetComponent<MonsterHealth>();
        
        monsterMovement.Initialize(currentData, animator, spriteRenderer);
        monsterHealth.Initialize(currentData);
    }

    private async Awaitable<RuntimeAnimatorController> GetAnimatorController()
    {
        var path = ZString.Format(AddressableExtensions.MonsterAnimatorPath, currentData.monsterName);
        return await AddressableExtensions.GetAnimator(path);
    }
    
    public async Awaitable GetDamage(float damage)
    {
        var isDead = monsterHealth.GetDamage(damage);

        if (isDead)
        {
            monsterMovement.StopMovement();
            animator.SetTrigger(death);
            await animator.WaitCurrentStateCompleteAsync();
            await Awaitable.WaitForSecondsAsync(0.1f);
            
            SCGObjectPoolingManager.Release(this); 
        }
    }
}