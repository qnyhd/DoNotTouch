using UnityEngine;

/// <summary>
/// 跳跃关违规惩罚：玩家跳跃落地后进入受控僵直，并显示眩晕特效。
/// 仅在本关需要时挂到主角上并勾选 Enabled。
/// </summary>
public class JumpLevelPenaltyAction : PlayerAction
{
    [Header("Penalty")]
    public bool penaltyEnabled = true;
    public float penaltyDuration = 1.5f;

    [Header("References")]
    public CharacterStunVFXController stunVfx;

    private bool waitingLand;
    private float penaltyTimer;
    private bool wasGroundedLastFrame;

    public bool IsInPenalty => penaltyTimer > 0f;

    public override int Priority => 190;
    public override bool IsActive => IsInPenalty;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        if (stunVfx == null)
            stunVfx = GetComponent<CharacterStunVFXController>();
    }

    public void NotifyJumpStarted()
    {
        if (!penaltyEnabled)
            return;

        waitingLand = true;
    }

    public override void TickAction(float deltaTime)
    {
        if (!penaltyEnabled)
            return;

        if (IsInPenalty)
        {
            penaltyTimer -= deltaTime;

            Controller.LockMovement();
            Motor.SetHorizontalVelocity(Vector3.zero);
            Motor.SetVerticalVelocity(0f);
            Anim.SetMove(Vector2.zero);

            if (penaltyTimer <= 0f)
                EndPenalty();

            wasGroundedLastFrame = Motor.IsGrounded;
            return;
        }

        if (waitingLand && Motor.IsGrounded && !wasGroundedLastFrame)
        {
            waitingLand = false;
            BeginPenalty();
        }

        wasGroundedLastFrame = Motor.IsGrounded;
    }

    private void BeginPenalty()
    {
        penaltyTimer = penaltyDuration;

        Controller.LockMovement();
        Motor.SetHorizontalVelocity(Vector3.zero);
        Motor.SetVerticalVelocity(0f);
        Anim.SetMove(Vector2.zero);

        if (stunVfx != null)
            stunVfx.ShowStun();
    }

    private void EndPenalty()
    {
        penaltyTimer = 0f;

        if (stunVfx != null)
            stunVfx.HideStun();
    }
}
