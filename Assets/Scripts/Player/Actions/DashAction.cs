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

        if (!Input.DashPressed)
            return false;

        // 没有按移动方向，不冲刺
        if (!Input.HasValidMoveInput)
            return false;

        // 3个键以上、W+S、A+D 这种非法输入，不冲刺
        if (Input.InvalidMoveInput)
            return false;

        return true;
    }

    protected override void OnStart()
    {
        dashDirection = GetSelfRelativeMoveDirection(Input.Move);

        if (dashDirection.sqrMagnitude < 0.01f)
        {
            timer = 0f;
            return;
        }

        Anim.TriggerDash();
    }

    protected override void OnActiveTick(float deltaTime)
    {
        Controller.LockMovement();

        Motor.SetHorizontalVelocity(dashDirection * dashSpeed);
    }

    protected override void OnEnd()
    {
        Motor.SetHorizontalVelocity(Vector3.zero);
    }

    private Vector3 GetSelfRelativeMoveDirection(Vector2 moveInput)
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * moveInput.y + right * moveInput.x;

        if (direction.sqrMagnitude < 0.01f)
            return Vector3.zero;

        return direction.normalized;
    }
}