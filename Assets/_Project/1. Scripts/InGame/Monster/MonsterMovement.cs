using System.Collections;
using UnityEngine;

public class MonsterMovement : CachedMonoBehaviour
{
    private Animator animator;
    private MonsterDataTable currentData;
    private SpriteRenderer spriteRenderer;

    private Vector3 targetPosition;
    private Coroutine moveCoroutine;
    private int currentPositionIndex;
    
    private const float ArrivalThreshold = 0.01f;
    private static readonly int running = Animator.StringToHash("Running");

    public void Initialize(MonsterDataTable monsterData, Animator animator, SpriteRenderer spriteRenderer)
    {
        this.currentData = monsterData;
        this.animator = animator;
        this.spriteRenderer = spriteRenderer;
        
        targetPosition = MonsterPath.GetSpawnPosition(0);
        currentPositionIndex = 0;
    }

    public void StartMovement()
    {
        StopMovement();
        moveCoroutine = StartCoroutine(MoveMonster());
    }

    public void StopMovement()
    {
        if (moveCoroutine == null)
            return;
        
        StopCoroutine(moveCoroutine);
        moveCoroutine = null;
    }

    private IEnumerator MoveMonster()
    {
        while (CachedGameObject.activeSelf)
        {
            var distance = Vector3.Distance(CachedTransform.position, targetPosition);

            if (distance > ArrivalThreshold)
            {
                var moveDistance = currentData.moveSpeed * Time.deltaTime;
                CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, targetPosition, moveDistance);
                animator.SetBool(running, true);
            }
            else
            {
                var nextPos = MonsterPath.GetNextSpawnPosition(currentPositionIndex, out currentPositionIndex);
                targetPosition = nextPos;
                CheckFlipSprite();
            }

            yield return null;
        }
    }

    private void CheckFlipSprite()
    {
        var currentX = CachedTransform.position.x;
        var targetX = targetPosition.x;
        
        spriteRenderer.flipX = currentX < targetX;
    }
    
    private void OnDisable()
    {
        StopMovement();
    }
}