using System.Collections;
using UnityEngine;

public class MonsterBehaviour : CachedMonoBehaviour
{
    [SerializeField] private Animator animator;

    private MonsterDataTable currentData;

    private Vector3 targetPosition;
    private Coroutine moveCoroutine;
    private int currentPositionIndex;

    private const float ArrivalThreshold = 0.01f;

    public async Awaitable Initialize(MonsterDataTable monsterData)
    {
        currentData = monsterData;
        
        animator.runtimeAnimatorController = await AddressableExtensions.GetAnimator(currentData.animatorPath);

        targetPosition = MonsterPath.GetSpawnPosition(0);
        currentPositionIndex = 0;

        StartMovement();
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
            }
            else
            {
                var nextPos = MonsterPath.GetNextSpawnPosition(currentPositionIndex, out currentPositionIndex);
                targetPosition = nextPos;
            }

            yield return null;
        }
    }

    private void OnDisable()
    {
        StopMovement();
    }
}