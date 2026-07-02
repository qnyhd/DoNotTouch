using UnityEngine;

public class JumpAction : PlayerAction
{
    public float jumpForce = 7f;

    [Header("Dash Jump")]
    public bool allowJumpDuringDash = true;

    private PlayerHealth health;
    private DashAction dashAction;
    private JumpLevelPenaltyAction jumpPenalty;
    private PlayerJumpLandDustVFX jumpLandDustVfx;

    public override int Priority => 150;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        dashAction = GetComponent<DashAction>();
        jumpPenalty = GetComponent<JumpLevelPenaltyAction>();
        jumpLandDustVfx = GetComponent<PlayerJumpLandDustVFX>();
    }

    public override void TickAction(float deltaTime)
    {
        if (!Input.JumpPressed)
            return;

        if (health != null && health.IsDead)
            return;

        // ??????????????????????????????????
        if (!Motor.CanJump)
            return;

        if (!CanJumpNow())
            return;

        Motor.SetVerticalVelocity(jumpForce);

        Anim.TriggerJump();
        jumpLandDustVfx?.PlayJumpDust();
        jumpPenalty?.NotifyJumpStarted();
    }

    private bool CanJumpNow()
    {
        if (!Controller.HasActiveExclusiveAction(this))
            return true;

        // ?????????????
        if (allowJumpDuringDash && dashAction != null && dashAction.IsActive)
            return true;

        return false;
    }
}