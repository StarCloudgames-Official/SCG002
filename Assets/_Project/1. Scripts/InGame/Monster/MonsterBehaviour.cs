using System.Collections;
using Cysharp.Text;
using UnityEngine;

public class MonsterBehaviour : CachedMonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private MonsterDataTable currentData;

    private Vector3 targetPosition;
    private Coroutine moveCoroutine;
    private int currentPositionIndex;

    private const float ArrivalThreshold = 0.01f;
    private static readonly int running = Animator.StringToHash("Running");

    public async Awaitable Initialize(MonsterDataTable monsterData)
    {
        currentData = monsterData;
        
        animator.runtimeAnimatorController = await GetAnimatorController();

        targetPosition = MonsterPath.GetSpawnPosition(0);
        currentPositionIndex = 0;

        StartMovement();
    }

    private async Awaitable<RuntimeAnimatorController> GetAnimatorController()
    {
        var path = ZString.Format(AddressableExtensions.MonsterAnimatorPath, currentData.monsterName);
        return await AddressableExtensions.GetAnimator(path);
    }

    private void StartMovement()
    {
        StopMovement();
        moveCoroutine = StartCoroutine(MoveMonster());
    }

    private void StopMovement()
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