using System;
using UnityEngine;

public class Projectile : CachedMonoBehaviour
{
    private Transform target;
    private Action onArrived;
    private float speed;
    private bool isFlying;

    public void StartFlight(Transform target, float speed, Action onArrived)
    {
        this.target = target;
        this.onArrived = onArrived;
        this.speed = speed;
        isFlying = true;
    }

    private void Update()
    {
        if (!isFlying)
            return;

        if (!target || !target.gameObject.activeSelf)
        {
            isFlying = false;
            var callback = onArrived;
            onArrived = null;
            callback?.Invoke();
            return;
        }

        var offset = target.position - CachedTransform.position;
        var direction = offset.normalized;
        var moveDistance = speed * Time.deltaTime;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        CachedTransform.rotation = Quaternion.Euler(0, 0, angle);

        // sqrt 연산 최적화: SqrMagnitude 사용
        var sqrDistanceToTarget = offset.sqrMagnitude;
        var sqrMoveDistance = moveDistance * moveDistance;

        if (sqrMoveDistance >= sqrDistanceToTarget)
        {
            CachedTransform.position = target.position;
            isFlying = false;

            var callback = onArrived;
            onArrived = null;
            callback?.Invoke();

            SCGObjectPoolingManager.Release(this);
        }
        else
        {
            // 이동
            CachedTransform.position += direction * moveDistance;
        }
    }
}