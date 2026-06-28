using UnityEngine;

public class EnemyRangedRetreatAction : EnemyAction
{
    [Header("Retreat")]
    public float retreatStartDistance = 3.5f;
    public float retreatStopDistance = 5.2f;
    public float retreatSpeed = 1.8f;

    [Header("Attack While Retreating")]
    public float retreatAttackInterval = 2.0f;
    public float retreatPauseAfterAttackRequest = 0.2f;

    private bool isRetreating;
    private float retreatTimer;
    private float retreatPauseTimer;

    private EnemyHealth enemyHealth;

    public override int Priority => 130;
    public override bool IsActive => isRetreating;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public override void TickAction(float deltaTime)
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            StopRetreat();
            return;
        }

        if (!Sensor.HasTarget())
        {
            StopRetreat();
            return;
        }

        // 如果已经请求强制攻击，就不要继续后退，让投石脚本接管
        if (Controller.HasForcedRangedAttackRequest)
        {
            StopRetreat();
            return;
        }

        // 请求攻击后，短暂停顿，避免下一帧又马上进入后退
        if (retreatPauseTimer > 0f)
        {
            retreatPauseTimer -= deltaTime;
            return;
        }

        // 如果别的排他动作正在执行，例如投石、受击，就不要抢控制
        if (!isRetreating && Controller.HasActiveExclusiveAction(this))
            return;

        float distance = Sensor.DistanceToTarget;

        if (!isRetreating && distance <= retreatStartDistance)
        {
            StartRetreat();
        }

        if (!isRetreating)
            return;

        if (distance >= retreatStopDistance)
        {
            StopRetreat();
            return;
        }

        retreatTimer += deltaTime;

        // 重点：后退几秒后，强制攻击一次
        if (retreatTimer >= retreatAttackInterval)
        {
            RequestAttackOnce();
            return;
        }

        RetreatFromTarget();
    }

    private void StartRetreat()
    {
        isRetreating = true;
        retreatTimer = 0f;

        Controller.SetState(EnemyState.Backstep);

        Anim.SetSpeed(0f);
        Anim.SetRetreating(true);
    }

    private void RetreatFromTarget()
    {
        Controller.LockMovement();

        Vector3 directionToTarget = Sensor.GetDirectionToTarget();

        if (directionToTarget.sqrMagnitude < 0.01f)
        {
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            Anim.SetRetreating(false);
            return;
        }

        // 面向玩家
        Motor.RotateToDirection(directionToTarget);

        // 身体往后退
        Vector3 retreatDirection = -directionToTarget;
        retreatDirection.y = 0f;
        retreatDirection.Normalize();

        Motor.SetHorizontalVelocity(retreatDirection * retreatSpeed);

        // 不再用 Run 动画
        Anim.SetSpeed(0f);
        Anim.SetRetreating(true);
    }

    private void RequestAttackOnce()
    {
        isRetreating = false;
        retreatTimer = 0f;
        retreatPauseTimer = retreatPauseAfterAttackRequest;

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetRetreating(false);

        Controller.SetState(EnemyState.Idle);

        // 通知投石脚本：下一次允许强制攻击
        Controller.RequestForcedRangedAttack();
    }

    private void StopRetreat()
    {
        if (!isRetreating)
        {
            Anim.SetRetreating(false);
            return;
        }

        isRetreating = false;
        retreatTimer = 0f;

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetRetreating(false);

        Controller.SetState(EnemyState.Idle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatStartDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, retreatStopDistance);
    }
}