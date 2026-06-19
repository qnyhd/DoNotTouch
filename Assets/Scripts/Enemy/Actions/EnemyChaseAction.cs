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

        if (Sensor.HasTargetInAttackRange())
            return;

        Vector3 direction = Sensor.GetDirectionToTarget();

        if (direction.sqrMagnitude < 0.01f)
            return;

        Controller.SetState(EnemyState.Chase);

        Motor.SetHorizontalVelocity(direction * Motor.moveSpeed);
        Motor.RotateToDirection(direction);

        Anim.SetSpeed(1f);
    }
}
