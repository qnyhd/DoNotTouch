using UnityEngine;

public class EnemyIdleAction : EnemyAction
{
    public override int Priority => 0;

    public override void TickAction(float deltaTime)
    {
        if (Controller.HasActiveExclusiveAction(this))
            return;

        if (Controller.IsMovementLocked)
        {
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            Anim.SetStrafing(false);
            return;
        }

        if (!Sensor.HasTargetInDetectRange())
        {
            Controller.SetState(EnemyState.Idle);
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            Anim.SetStrafing(false);
            return;
        }

        if (Sensor.HasTargetInAttackRange())
        {
            Controller.SetState(EnemyState.Idle);
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            Anim.SetStrafing(false);
        }
    }
}