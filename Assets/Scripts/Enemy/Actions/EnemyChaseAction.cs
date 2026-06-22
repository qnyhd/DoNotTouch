using UnityEngine;

public class EnemyChaseAction : EnemyAction
{
    [Header("Random Strafe While Chasing")]
    public float strafeDecisionInterval = 0.8f;
    public float strafeChance = 0.35f;
    public float minDistanceToStrafe = 2.5f;
    public float maxDistanceToStrafe = 6f;

    private float strafeDecisionTimer;

    public override int Priority => 10;

    public override void TickAction(float deltaTime)
    {
        if (Controller.IsMovementLocked)
            return;

        if (Controller.IsInIdlePause)
            return;

        if (!Sensor.HasTargetInDetectRange())
            return;

        if (Sensor.HasTargetInAttackRange())
            return;

        TryRequestStrafe(deltaTime);

        Vector3 direction = Sensor.GetDirectionToTarget();

        if (direction.sqrMagnitude < 0.01f)
            return;

        Controller.SetState(EnemyState.Chase);

        Motor.SetHorizontalVelocity(direction * Motor.moveSpeed);
        Motor.RotateToDirection(direction);

        Anim.SetSpeed(1f);
        Anim.SetStrafing(false);
    }

    private void TryRequestStrafe(float deltaTime)
    {
        strafeDecisionTimer -= deltaTime;

        if (strafeDecisionTimer > 0f)
            return;

        strafeDecisionTimer = strafeDecisionInterval;

        float distance = Sensor.DistanceToTarget;

        // 太近不平移，避免贴脸时还横移
        if (distance < minDistanceToStrafe)
            return;

        // 太远也不平移，先追近玩家
        if (distance > maxDistanceToStrafe)
            return;

        if (Random.value > strafeChance)
            return;

        Controller.RequestStrafe(false);
    }
}