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
    
    private InGameContext inGameContext;
    
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
        inGameContext ??= InGameManager.Instance.InGameContext;
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
    
    public void GetDamage(float damage)
    {
        var isDead = monsterHealth.GetDamage(damage);

        if (isDead)
        {
            Dead().Forget();
        }
    }

    private async Awaitable Dead()
    {
        inGameContext.StageManager.IncreaseKillCount();
        inGameContext.InGameCrystal += currentData.dropCrystal;

        var dropChance = Random.value;
        if (dropChance <= ConstantDataGetter.LuckyPointDropChance)
        {
            inGameContext.LuckyPoint += 1;
        }

        monsterMovement.StopMovement();
        animator.SetTrigger(death);
        await animator.WaitCurrentStateCompleteAsync();
        await Awaitable.WaitForSecondsAsync(0.1f);
            
        SCGObjectPoolingManager.Release(this); 
    }
}