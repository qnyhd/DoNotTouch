using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 气垫锤砸地：在锤击落点画范围圆，并把圆内敌人沿径向击退。
/// 由动画事件 OnHammerGroundHit 触发，与 HammerGroundHitVFX 使用同一落点。
/// </summary>
public class HammerGroundSlam : MonoBehaviour
{
    [Header("References")]
    public HammerGroundHitVFX groundHitVfx;
    public AttackAction attackAction;
    public PlayerHealth playerHealth;
    public CharacterController characterController;

    [Header("Slam Area")]
    public float slamRadius = 1.1f;
    public LayerMask hittableLayers = ~0;
    public float maxTargetHeightOffset = 1.5f;

    [Header("Knockback")]
    [Tooltip("击退距离 = 主角身宽 × 这个倍数。2 = 两个身位，3 = 三个身位。")]
    public float knockbackBodyWidths = 2.5f;

    [Header("Damage")]
    public bool dealDamage = true;
    public float staminaDamageOnBlock = 6f;
    public bool canBeBlocked = false;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void Awake()
    {
        if (groundHitVfx == null)
            groundHitVfx = GetComponent<HammerGroundHitVFX>();
        if (attackAction == null)
            attackAction = GetComponent<AttackAction>();
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    public void PerformSlam()
    {
        if (!TryGetImpactPoint(out Vector3 impactPoint))
            return;

        groundHitVfx?.PlayGroundHitVFX();
        HitTargetsInRadius(impactPoint);
        attackAction?.NotifyHammerSlamHit();
    }

    public bool TryGetImpactPoint(out Vector3 impactPoint)
    {
        if (groundHitVfx != null)
            return groundHitVfx.TryGetImpactPoint(out impactPoint);

        impactPoint = transform.position;
        return true;
    }

    private void HitTargetsInRadius(Vector3 impactPoint)
    {
        hitTargets.Clear();

        Collider[] colliders = Physics.OverlapSphere(
            impactPoint,
            slamRadius,
            hittableLayers,
            QueryTriggerInteraction.Ignore);

        int damage = playerHealth != null ? playerHealth.attackDamage : 2;
        float knockbackDistance = GetKnockbackDistance();

        foreach (Collider col in colliders)
        {
            if (col.transform.root == transform.root)
                continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();

            if (damageable == null || damageable.IsDead)
                continue;

            if (hitTargets.Contains(damageable))
                continue;

            Vector3 enemyPoint = col.transform.position;
            if (Mathf.Abs(enemyPoint.y - impactPoint.y) > maxTargetHeightOffset)
                continue;

            Vector3 flatOffset = enemyPoint - impactPoint;
            flatOffset.y = 0f;

            if (flatOffset.sqrMagnitude > slamRadius * slamRadius)
                continue;

            hitTargets.Add(damageable);

            Vector3 knockbackDirection = flatOffset.sqrMagnitude > 0.0001f
                ? flatOffset.normalized
                : transform.forward;

            DamageInfo info = new DamageInfo(
                dealDamage ? damage : 0,
                staminaDamageOnBlock,
                gameObject,
                impactPoint,
                knockbackDirection,
                canBeBlocked,
                knockbackDistance);

            damageable.TakeDamage(info);
        }
    }

    private float GetKnockbackDistance()
    {
        float bodyWidth = 0.6f;

        if (characterController != null)
            bodyWidth = characterController.radius * 2f;

        return bodyWidth * knockbackBodyWidths;
    }

    private void OnDrawGizmosSelected()
    {
        if (!TryGetImpactPoint(out Vector3 impactPoint))
            return;

        Gizmos.color = new Color(1f, 0.55f, 0f, 0.9f);
        Gizmos.DrawWireSphere(impactPoint, slamRadius);
    }
}
