using UnityEngine;

public class JumpAction : PlayerAction
{
    public float jumpForce = 7f;

    [Header("Dash Jump")]
    public bool allowJumpDuringDash = true;

    private PlayerHealth health;
    private DashAction dashAction;

    public override int Priority => 150;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        dashAction = GetComponent<DashAction>();
    }

    public override void TickAction(float deltaTime)
    {
        if (!Input.JumpPressed)
            return;

        if (health != null && health.IsDead)
            return;

        if (!Motor.IsGrounded)
            return;

        if (!CanJumpNow())
            return;

        Motor.SetVerticalVelocity(jumpForce);

        // 最终动画以跳跃为准
        Anim.TriggerJump();
        //Anim.TriggerJumpOverrideDash();
    }

    private bool CanJumpNow()
    {
        if (!Controller.HasActiveExclusiveAction(this))
            return true;

        // 允许冲刺期间跳跃
        if (allowJumpDuringDash && dashAction != null && dashAction.IsActive)
            return true;

        return false;
    }
}