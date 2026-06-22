using UnityEngine;

public class EnemyStrafeAction : TimedEnemyAction
{
    [Header("Strafe")]
    public float strafeSpeed = 2.2f;
    public float minStrafeDuration = 0.5f;
    public float maxStrafeDuration = 1.2f;

    [Header("Early Stop")]
    public float stopStrafeDistance = 1.8f;

    private int strafeSide;
    private bool isLeft;

    public override int Priority => 115;
    public override bool BlocksOtherActions => true;

    protected override bool ShouldStart()
    {
        if (Controller.Health != null && Controller.Health.IsDead)
            return false;

        if (Controller.IsInIdlePause)
            return false;

        if (Controller.HasActiveExclusiveAction(this))
            return false;

        if (!Sensor.HasTargetInDetectRange())
            return false;

        bool hasRequest = Controller.ConsumeStrafeRequest(out bool force);

        if (!hasRequest)
            return false;

        return true;
    }

    protected override void OnStart()
    {
        duration = Random.Range(minStrafeDuration, maxStrafeDuration);
        timer = duration;

        isLeft = Random.value < 0.5f;
        strafeSide = isLeft ? -1 : 1;

        Controller.SetState(EnemyState.Strafe);

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetStrafeLeft(isLeft);
        Anim.SetStrafing(true);
    }

    protected override void OnActiveTick(float deltaTime)
    {
        Controller.LockMovement();

        if (!Sensor.HasTarget())
        {
            StopStrafeNow();
            return;
        }

        // 重点：
        // 敌人和玩家太近，立刻停止平移，StrafeX = false，回 Idle 初始形态
        if (Sensor.DistanceToTarget <= stopStrafeDistance)
        {
            StopStrafeNow();
            return;
        }

        Vector3 directionToTarget = Sensor.GetDirectionToTarget();

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Motor.RotateToDirection(directionToTarget);
        }

        Vector3 strafeDirection = transform.right * strafeSide;

        Motor.SetHorizontalVelocity(strafeDirection * strafeSpeed);

        Anim.SetSpeed(0f);
        Anim.SetStrafeLeft(isLeft);
        Anim.SetStrafing(true);
    }

    protected override void OnEnd()
    {
        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetStrafing(false);

        Controller.SetState(EnemyState.Idle);
    }

    private void StopStrafeNow()
    {
        timer = 0f;

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetStrafing(false);

        Controller.SetState(EnemyState.Idle);
    }
}