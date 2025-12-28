using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CharacterAttack : CachedMonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private ClassTable data;
    private CharacterBehaviour characterBehaviour;
    private InGameContext inGameContext;
    private MonsterSpawner monsterSpawner;
    private Animator animator;

    private List<MonsterBehaviour> monsters;
    private float currentAttackDelay;
    
    private static readonly int attacking = Animator.StringToHash("Attack");
    
    public void Initialize(CharacterBehaviour characterBehaviour, Animator animator)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        this.characterBehaviour = characterBehaviour;
        this.animator = animator;

        data = characterBehaviour.CurrentClass;
        inGameContext = InGameManager.Instance.InGameContext;
        monsterSpawner = inGameContext.StageManager.MonsterSpawner;

        currentAttackDelay = 0.0f;
    }

    private void Update()
    {
        if (!CanAttack())
            return;
        
        Attack();
    }

    private void Attack()
    {
        monsters = monsterSpawner.GetNearestMonster(CachedTransform.position, data.attackRange, data.attackMonsterCount);

        if (monsters == null || monsters.Count == 0)
        {
            if (monsters != null)
                ListPool<MonsterBehaviour>.Release(monsters);
            return;
        }

        animator.SetTrigger(attacking);
        currentAttackDelay = data.attackSpeed;

        FlipToMonster(monsters[0]);

        foreach (var monster in monsters)
        {
            var newProjectile = SCGObjectPoolingManager.Get<Projectile>();
            newProjectile.transform.position = CachedTransform.position;

            newProjectile.StartFlight(monster.CachedTransform, 5, () =>
            {

            });
        }

        ListPool<MonsterBehaviour>.Release(monsters);
    }

    private void FlipToMonster(MonsterBehaviour monster)
    {
        var direction = monster.CachedTransform.position.x - CachedTransform.position.x;
        spriteRenderer.flipX = direction < 0;
    }

    private bool CanAttack()
    {
        if (currentAttackDelay > 0.0f)
        {
            //TODO : 스탯매니저에서 공격속도받아와야됨
            currentAttackDelay -= Time.deltaTime;
        }
        
        return characterBehaviour.CanInteract && currentAttackDelay <= 0.0f;
    }
}