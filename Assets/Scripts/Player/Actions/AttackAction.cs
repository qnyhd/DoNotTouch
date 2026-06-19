using System.Collections.Generic;
using UnityEngine;

public class AttackAction : TimedPlayerAction
{
    [Header("Attack")]
    public bool stopMoveWhenAttack = true;

    public float hitTime = 0.25f;
    public float attackRange = 1.4f;
    public float attackRadius = 0.7f;

    public LayerMask hittableLayers = ~0;

    private bool hasHit;
    private PlayerHealth playerHealth;
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    public override int Priority => 90;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    protected override bool ShouldStart()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return false;

        return Input.AttackPressed;
    }

    protected override void OnStart()
    {
        hasHit = false;
        hitTargets.Clear();

        Anim.TriggerAttack();
    }

    protected override void OnActiveTick(float deltaTime)
    {
        if (stopMoveWhenAttack)
        {
            Controller.LockMovement();
            Motor.SetHorizontalVelocity(Vector3.zero);
        }

        float attackElapsedTime = duration - timer;

        if (!hasHit && attackElapsedTime >= hitTime)
        {
            hasHit = true;
            HitTargets();
        }
    }

    private void HitTargets()
    {
        Vector3 center = transform.position + transform.forward * attackRange;
        Collider[] colliders = Physics.OverlapSphere(center, attackRadius, hittableLayers);

        int damage = 2;

        if (playerHealth != null)
        {
            damage = playerHealth.attackDamage;
        }

        foreach (Collider col in colliders)
        {
            if (col.transform.root == transform.root)
                continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damageable.IsDead)
                continue;

            if (hitTargets.Contains(damageable))
                continue;

            hitTargets.Add(damageable);
            damageable.TakeDamage(damage, gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 center = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}