using UnityEngine;

public class EnemyMeleeAttackAction : TimedEnemyAction
{
    [Header("Attack")]
    public float hitTime = 0.35f;
    public float hitRange = 1.6f;

    [Header("Block")]
    public float staminaDamageOnBlock = 3f;

    [Header("Backstep After Attacks")]
    public int minAttacksBeforeBackstep = 1;
    public int maxAttacksBeforeBackstep = 3;

    private int attackCount;
    private int nextBackstepAttackCount;

    private bool hasHit;
    private EnemyHealth enemyHealth;

    public override int Priority => 100;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        ResetBackstepAttackCount();
    }

    protected override bool ShouldStart()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
            return false;

        if (Controller.CurrentState == EnemyState.Backstep)
            return false;

        if (Controller.CurrentState == EnemyState.Block)
            return false;

        if (Controller.IsInIdlePause)
            return false;

        return Sensor.HasTargetInAttackRange();
    }

    protected override void OnStart()
    {
        hasHit = false;

        Controller.SetState(EnemyState.Attack);

        Motor.ForceStop();

        Vector3 direction = Sensor.GetDirectionToTarget();
        Motor.RotateToDirection(direction);

        Anim.SetSpeed(0f);
        Anim.TriggerAttack();
    }

    protected override void OnActiveTick(float deltaTime)
    {
        Controller.LockMovement();

        Motor.ForceStop();
        Anim.SetSpeed(0f);

        Vector3 direction = Sensor.GetDirectionToTarget();
        Motor.RotateToDirection(direction);

        float attackElapsedTime = duration - timer;

        if (!hasHit && attackElapsedTime >= hitTime)
        {
            hasHit = true;
            TryHitTarget();
        }
    }

    protected override void OnEnd()
    {
        Motor.ForceStop();
        Anim.SetSpeed(0f);

        attackCount++;

        if (attackCount >= nextBackstepAttackCount)
        {
            attackCount = 0;
            ResetBackstepAttackCount();

            Controller.RequestBackstep();
        }
    }

    private void ResetBackstepAttackCount()
    {
        if (maxAttacksBeforeBackstep < minAttacksBeforeBackstep)
        {
            maxAttacksBeforeBackstep = minAttacksBeforeBackstep;
        }

        nextBackstepAttackCount = Random.Range(
            minAttacksBeforeBackstep,
            maxAttacksBeforeBackstep + 1
        );
    }

    private void TryHitTarget()
    {
        Transform target = Sensor.Target;

        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > hitRange)
            return;

        IDamageable damageable = target.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        if (damageable.IsDead)
            return;

        int damage = 2;

        if (enemyHealth != null)
        {
            damage = enemyHealth.attackDamage;
        }

        Vector3 attackDirection = target.position - transform.position;
        attackDirection.y = 0f;

        if (attackDirection.sqrMagnitude < 0.01f)
        {
            attackDirection = transform.forward;
        }

        DamageInfo info = new DamageInfo(
            damage,
            staminaDamageOnBlock,
            gameObject,
            target.position,
            attackDirection,
            true
        );

        damageable.TakeDamage(info);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
}