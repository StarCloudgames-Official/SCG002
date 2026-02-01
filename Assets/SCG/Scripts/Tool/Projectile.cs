using UnityEngine;

public class Projectile : CachedMonoBehaviour
{
    private Transform target;
    private MonsterBehaviour targetMonster;
    private float damage;
    private float speed;
    private bool isFlying;

    public void StartFlight(MonsterBehaviour monster, float speed, float damage)
    {
        targetMonster = monster;
        target = monster.CachedTransform;
        this.damage = damage;
        this.speed = speed;
        isFlying = true;
    }

    private void Update()
    {
        if (!isFlying)
            return;

        if (target == null || !target.gameObject.activeSelf)
        {
            isFlying = false;
            target = null;
            targetMonster = null;
            SCGObjectPoolingManager.Release(this);

            return;
        }

        var offset = target.position - CachedTransform.position;
        var sqrDistanceToTarget = offset.sqrMagnitude;
        var moveDistance = speed * Time.deltaTime;
        var sqrMoveDistance = moveDistance * moveDistance;

        if (sqrMoveDistance >= sqrDistanceToTarget)
        {
            CachedTransform.position = target.position;
            isFlying = false;

            if (targetMonster != null && !targetMonster.IsDead)
                targetMonster.GetDamage(damage);

            target = null;
            targetMonster = null;
            SCGObjectPoolingManager.Release(this);
        }
        else
        {
            var distance = Mathf.Sqrt(sqrDistanceToTarget);
            var direction = offset / distance;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            CachedTransform.rotation = Quaternion.Euler(0, 0, angle);

            CachedTransform.position += direction * moveDistance;
        }
    }
}
