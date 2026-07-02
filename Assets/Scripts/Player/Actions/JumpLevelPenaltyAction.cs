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
    public PlayerStunPresentation stunPresentation;
    public CharacterStunVFXController stunVfx;

    [Header("Stun VFX")]
    [Tooltip("落地前多少米先显示星星。0 = 落地瞬间才出现；越大越早（容易显得过早）。")]
    public float stunShowLeadDistance = 0f;
    public LayerMask stunGroundLayers = ~0;
    public float stunGroundRayDistance = 3f;

    private bool waitingLand;
    private bool stunShownForJump;
    private float penaltyTimer;
    private bool wasGroundedLastFrame;
    private CharacterController characterController;

    public bool IsInPenalty => penaltyTimer > 0f;

    public override int Priority => 190;
    public override bool IsActive => IsInPenalty;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        if (stunPresentation == null)
            stunPresentation = GetComponent<PlayerStunPresentation>();

        if (stunVfx == null)
            stunVfx = GetComponent<CharacterStunVFXController>();

        characterController = GetComponent<CharacterController>();
    }

    public void NotifyJumpStarted()
    {
        if (!penaltyEnabled)
            return;

        waitingLand = true;
        stunShownForJump = false;
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

        if (waitingLand)
        {
            TryShowStunBeforeLand();

            if (Motor.IsGrounded && !wasGroundedLastFrame)
            {
                waitingLand = false;
                BeginPenalty();
            }
        }

        wasGroundedLastFrame = Motor.IsGrounded;
    }

    private void TryShowStunBeforeLand()
    {
        if (stunShownForJump || stunShowLeadDistance <= 0f)
            return;

        if (stunPresentation == null && stunVfx == null)
            return;

        if (Motor.IsGrounded)
            return;

        if (!TryGetDistanceToGround(out float distance))
            return;

        if (distance > stunShowLeadDistance)
            return;

        stunShownForJump = true;

        if (stunPresentation != null)
            stunPresentation.ShowVfxOnly();
        else
            stunVfx?.ShowStun();
    }

    private bool TryGetDistanceToGround(out float distance)
    {
        distance = 0f;

        Vector3 origin = transform.position;
        if (characterController != null)
        {
            origin = transform.position
                + characterController.center
                + Vector3.down * (characterController.height * 0.5f);
        }

        if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                stunGroundRayDistance,
                stunGroundLayers,
                QueryTriggerInteraction.Ignore))
        {
            distance = hit.distance;
            return true;
        }

        return false;
    }

    private void BeginPenalty()
    {
        penaltyTimer = penaltyDuration;

        Controller.LockMovement();
        Motor.SetHorizontalVelocity(Vector3.zero);
        Motor.SetVerticalVelocity(0f);
        Anim.SetMove(Vector2.zero);

        if (stunPresentation != null)
            stunPresentation.BeginStun();
        else if (stunVfx != null && !stunShownForJump)
        {
            Anim.SetStunned(true);
            stunVfx.ShowStun();
        }
        else
            Anim.SetStunned(true);

        stunShownForJump = false;
    }

    private void EndPenalty()
    {
        penaltyTimer = 0f;

        if (stunPresentation != null)
            stunPresentation.EndStun();
        else
        {
            Anim.SetStunned(false);
            stunVfx?.HideStun();
        }
    }
}
