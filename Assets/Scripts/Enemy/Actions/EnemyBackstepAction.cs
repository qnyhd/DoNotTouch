using UnityEngine;

public class EnemyBackstepAction : TimedEnemyAction
{
    [Header("Backstep")]
    public float backstepSpeed = 3.5f;

    private Vector3 backstepDirection;

    public override int Priority => 120;
    public override bool BlocksOtherActions => true;

    protected override bool ShouldStart()
    {
        if (Controller.Health != null && Controller.Health.IsDead)
            return false;

        if (Controller.HasActiveExclusiveAction(this))
            return false;

        return Controller.ConsumeBackstepRequest();
    }

    protected override void OnStart()
    {
        Controller.SetState(EnemyState.Backstep);

        Vector3 directionToTarget = Sensor.GetDirectionToTarget();

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Motor.RotateToDirection(directionToTarget);
            backstepDirection = -directionToTarget;
        }
        else
        {
            backstepDirection = -transform.forward;
        }

        backstepDirection.y = 0f;
        backstepDirection.Normalize();

        Anim.SetSpeed(0f);
        Anim.TriggerBackstep();
    }

    protected override void OnActiveTick(float deltaTime)
    {
        Controller.LockMovement();

        Vector3 directionToTarget = Sensor.GetDirectionToTarget();

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Motor.RotateToDirection(directionToTarget);
        }

        Motor.SetHorizontalVelocity(backstepDirection * backstepSpeed);
        Anim.SetSpeed(0f);
    }

    protected override void OnEnd()
    {
        Motor.ForceStop();
        Anim.SetSpeed(0f);

        Controller.StartRandomIdlePauseAfterBackstep();

        Controller.SetState(EnemyState.Idle);
    }
}