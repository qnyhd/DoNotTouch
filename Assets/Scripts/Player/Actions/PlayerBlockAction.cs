using UnityEngine;

public class PlayerBlockAction : PlayerAction, IBlockHandler
{
    private enum BlockState
    {
        None,
        BlockLoop,
        BlockCounter,
        GuardBreak
    }

    [Header("Block")]
    public float blockAngle = 180f;

    [Tooltip("成功格挡一次攻击时消耗的体力。注意：长按格挡和格挡移动不会消耗体力。")]
    public float blockStaminaCost = 3f;

    [Tooltip("至少多少体力才允许开始格挡。")]
    public float minStaminaToStartBlock = 0.5f;

    [Header("Block Recover")]
    [Tooltip("格挡状态下体力恢复倍率。1=正常恢复，0.5=一半速度，0=不恢复。")]
    public float blockingRecoverMultiplier = 0.35f;

    [Header("Block Move")]
    public float blockMoveSpeed = 1.8f;

    [Header("Block Counter")]
    public float blockCounterDuration = 0.65f;

    [Header("Guard Break")]
    public float guardBreakDuration = 0.9f;

    private BlockState state = BlockState.None;

    private float blockCounterTimer;
    private float guardBreakTimer;

    private CombatStamina stamina;
    private PlayerHealth health;

    public override int Priority => 110;

    public override bool IsActive
    {
        get
        {
            return state == BlockState.BlockLoop
                || state == BlockState.BlockCounter
                || state == BlockState.GuardBreak;
        }
    }

    public override bool BlocksOtherActions => true;

    // 只有 BlockLoop 才是真正格挡。
    public bool IsBlocking => state == BlockState.BlockLoop;

    public bool IsGuardBroken => state == BlockState.GuardBreak;

    private void Awake()
    {
        stamina = GetComponent<CombatStamina>();
        health = GetComponent<PlayerHealth>();
    }

    public override void TickAction(float deltaTime)
    {
        if (health != null && health.IsDead)
        {
            ForceEndBlock();
            return;
        }

        switch (state)
        {
            case BlockState.None:
                TickNone();
                break;

            case BlockState.BlockLoop:
                TickBlockLoop();
                break;

            case BlockState.BlockCounter:
                TickBlockCounter(deltaTime);
                break;

            case BlockState.GuardBreak:
                TickGuardBreak(deltaTime);
                break;
        }
    }

    private void TickNone()
    {
        ResetStaminaRecover();

        if (Input.BlockHeld && CanStartBlock())
        {
            StartBlockLoop();
        }
        else
        {
            Anim.SetBlocking(false);
            Anim.SetBlockMove(Vector2.zero);
        }
    }

    private void TickBlockLoop()
    {
        Controller.LockMovement();

        // 格挡状态：不消耗体力，只降低体力恢复速度
        SetBlockingStaminaRecover();

        Anim.SetBlocking(true);

        if (!Input.BlockHeld)
        {
            EndToIdle();
            return;
        }

        HandleBlockMovement();
    }

    private void TickBlockCounter(float deltaTime)
    {
        blockCounterTimer -= deltaTime;

        Controller.LockMovement();

        // BlockCounter 期间不移动，不使用格挡移动动画
        ResetStaminaRecover();

        Motor.SetHorizontalVelocity(Vector3.zero);
        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(Vector2.zero);

        bool shouldReturnToBlockLoop = Input.BlockHeld && CanStartBlockAfterCounter();

        Anim.SetBlocking(shouldReturnToBlockLoop);

        if (blockCounterTimer <= 0f)
        {
            if (shouldReturnToBlockLoop)
            {
                StartBlockLoop();
            }
            else
            {
                EndToIdle();
            }
        }
    }

    private void TickGuardBreak(float deltaTime)
    {
        guardBreakTimer -= deltaTime;

        Controller.LockMovement();

        ResetStaminaRecover();

        Motor.SetHorizontalVelocity(Vector3.zero);
        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(Vector2.zero);

        Anim.SetBlocking(false);

        if (guardBreakTimer <= 0f)
        {
            EndToIdle();
        }
    }

    private void HandleBlockMovement()
    {
        Vector2 moveInput = Input.Move;

        if (!Input.HasValidMoveInput || Input.InvalidMoveInput)
        {
            Motor.SetHorizontalVelocity(Vector3.zero);

            Anim.SetMove(Vector2.zero);
            Anim.SetBlockMove(Vector2.zero);
            return;
        }

        Vector3 moveDirection = GetSelfRelativeDirection(moveInput);

        Motor.SetHorizontalVelocity(moveDirection * blockMoveSpeed);

        // 普通移动动画归零，格挡移动动画单独控制
        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(moveInput);
    }

    private Vector3 GetSelfRelativeDirection(Vector2 moveInput)
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

    private bool CanStartBlock()
    {
        if (stamina == null)
            return false;

        // 体力太低时不能新开始格挡，但格挡本身不会持续扣体力
        if (stamina.currentStamina < minStaminaToStartBlock)
            return false;

        if (!Motor.IsGrounded)
            return false;

        if (Controller.HasActiveExclusiveAction(this))
            return false;

        return true;
    }

    private bool CanStartBlockAfterCounter()
    {
        if (stamina == null)
            return false;

        if (stamina.currentStamina < minStaminaToStartBlock)
            return false;

        if (!Motor.IsGrounded)
            return false;

        return true;
    }

    private void StartBlockLoop()
    {
        state = BlockState.BlockLoop;

        SetBlockingStaminaRecover();

        Anim.SetMove(Vector2.zero);
        Anim.SetBlocking(true);
    }

    private void StartBlockCounter()
    {
        state = BlockState.BlockCounter;
        blockCounterTimer = blockCounterDuration;

        ResetStaminaRecover();

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(Vector2.zero);
        Anim.TriggerBlockCounter();
    }

    private void StartGuardBreak()
    {
        state = BlockState.GuardBreak;
        guardBreakTimer = guardBreakDuration;

        ResetStaminaRecover();

        if (stamina != null)
        {
            stamina.Consume(stamina.currentStamina);
        }

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(Vector2.zero);
        Anim.SetBlocking(false);
        Anim.TriggerGuardBreak();
    }

    private void EndToIdle()
    {
        state = BlockState.None;

        ResetStaminaRecover();

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(Vector2.zero);
        Anim.SetBlocking(false);
    }

    private void ForceEndBlock()
    {
        state = BlockState.None;
        blockCounterTimer = 0f;
        guardBreakTimer = 0f;

        ResetStaminaRecover();

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlockMove(Vector2.zero);
        Anim.SetBlocking(false);
    }

    private void SetBlockingStaminaRecover()
    {
        if (stamina != null)
        {
            stamina.SetRecoverMultiplier(blockingRecoverMultiplier);
        }
    }

    private void ResetStaminaRecover()
    {
        if (stamina != null)
        {
            stamina.ResetRecoverMultiplier();
        }
    }

    public bool CanBlockAttack(Vector3 attackerPosition)
    {
        if (state != BlockState.BlockLoop)
            return false;

        Vector3 directionToAttacker = attackerPosition - transform.position;
        directionToAttacker.y = 0f;

        if (directionToAttacker.sqrMagnitude < 0.01f)
            return true;

        directionToAttacker.Normalize();

        float angle = Vector3.Angle(transform.forward, directionToAttacker);

        return angle <= blockAngle * 0.5f;
    }

    public BlockResult TryBlock(DamageInfo info)
    {
        if (state != BlockState.BlockLoop)
            return BlockResult.NotBlocked;

        // 注意：
        // 只有真正挡住攻击时才消耗体力。
        // 长按格挡和格挡移动不会消耗体力。
        float cost = info.staminaDamage > 0f ? info.staminaDamage : blockStaminaCost;

        bool hadEnough = stamina.Consume(cost);

        if (!hadEnough || stamina.IsEmpty)
        {
            StartGuardBreak();
            return BlockResult.GuardBroken;
        }

        StartBlockCounter();
        return BlockResult.Blocked;
    }
}