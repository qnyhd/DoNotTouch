using UnityEngine;

public class DashAction : TimedPlayerAction
{
    public float dashSpeed = 10f;

    private Vector3 dashDirection;
    private PlayerHealth health;

    public override int Priority => 100;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    protected override bool ShouldStart()
    {
        if (health != null && health.IsDead)
            return false;

        return Input.DashPressed;
    }

    protected override void OnStart()
    {
        // 永远朝角色自己的正前方冲刺
        dashDirection = transform.forward;
        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.01f)
        {
            dashDirection = Vector3.forward;
        }

        dashDirection.Normalize();

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