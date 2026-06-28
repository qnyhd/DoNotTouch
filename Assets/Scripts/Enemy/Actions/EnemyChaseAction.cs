using UnityEngine;

public class EnemyChaseAction : EnemyAction
{
    public override int Priority => 10;

    public override void TickAction(float deltaTime)
    {
        if (Controller.IsMovementLocked)
            return;

        if (!Sensor.HasTargetInDetectRange())
            return;

        // 进入攻击距离后，必须停下并把动画速度归零
        if (Sensor.HasTargetInAttackRange())
        {
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            return;
        }

        Vector3 direction = Sensor.GetDirectionToTarget();

        if (direction.sqrMagnitude < 0.01f)
        {
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            return;
        }

        Controller.SetState(EnemyState.Chase);

        Motor.SetHorizontalVelocity(direction * Motor.moveSpeed);
        Motor.RotateToDirection(direction);

        Anim.SetSpeed(1f);
    }
}