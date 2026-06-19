using UnityEngine;

public class DashAction : TimedPlayerAction
{
    public float dashSpeed = 10f;

    private Vector3 dashDirection;

    public override int Priority => 100;
    public override bool BlocksOtherActions => true;

    protected override bool ShouldStart()
    {
        return Input.DashPressed;
    }

    protected override void OnStart()
    {
        dashDirection = Motor.GetCameraRelativeDirection(Input.Move);

        if (dashDirection.sqrMagnitude < 0.01f)
        {
            dashDirection = transform.forward;
        }

        Anim.TriggerDash();
    }

    protected override void OnActiveTick(float deltaTime)
    {
        Controller.LockMovement();

        Motor.SetHorizontalVelocity(dashDirection * dashSpeed);
        Motor.RotateToDirection(dashDirection);
    }

    protected override void OnEnd()
    {
        Motor.SetHorizontalVelocity(Vector3.zero);
    }
}
