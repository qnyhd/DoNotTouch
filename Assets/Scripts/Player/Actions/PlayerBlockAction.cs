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
    public float holdStaminaCostPerSecond = 1f;
    public float blockStaminaCost = 3f;
    public float minStaminaToStartBlock = 1f;

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

    // 只有 BlockLoop 才算真正格挡。
    // BlockCounter 和 GuardBreak 期间不算格挡。
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
                TickBlockLoop(deltaTime);
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
        if (Input.BlockHeld && CanStartBlock())
        {
            StartBlockLoop();
        }
        else
        {
            Anim.SetBlocking(false);
        }
    }

    private void TickBlockLoop(float deltaTime)
    {
        Controller.LockMovement();

        Motor.SetHorizontalVelocity(Vector3.zero);
        Anim.SetMove(Vector2.zero);
        Anim.SetBlocking(true);

        bool hadEnough = stamina.Consume(holdStaminaCostPerSecond * deltaTime);

        if (!hadEnough || stamina.IsEmpty)
        {
            StartGuardBreak();
            return;
        }

        if (!Input.BlockHeld)
        {
            EndToIdle();
        }
    }

    private void TickBlockCounter(float deltaTime)
    {
        blockCounterTimer -= deltaTime;

        Controller.LockMovement();

        Motor.SetHorizontalVelocity(Vector3.zero);
        Anim.SetMove(Vector2.zero);

        // 这里非常重要：
        // BlockCounter 播放期间，Blocking Bool 根据右键是否仍然按住决定。
        // 这样 Animator 可以在 BlockCounter 结束后自动选择回 BlockLoop 或 Idle。
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

        Motor.SetHorizontalVelocity(Vector3.zero);
        Anim.SetMove(Vector2.zero);

        // GuardBreak 必须直接回 Idle，不能回 BlockLoop。
        Anim.SetBlocking(false);

        if (guardBreakTimer <= 0f)
        {
            EndToIdle();
        }
    }

    private bool CanStartBlock()
    {
        if (stamina == null)
            return false;

        if (stamina.currentStamina < minStaminaToStartBlock)
            return false;

        if (!Motor.IsGrounded)
            return false;

        // 只能在 Idle / 移动时开始格挡。
        // 如果攻击、冲刺、受击、破防等排他动作正在执行，就不能开始格挡。
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

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlocking(true);
    }

    private void StartBlockCounter()
    {
        state = BlockState.BlockCounter;
        blockCounterTimer = blockCounterDuration;

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.TriggerBlockCounter();

        // 注意：这里不要直接固定 SetBlocking(false)。
        // 下一帧 TickBlockCounter 会根据右键是否还按住来设置 Blocking。
    }

    private void StartGuardBreak()
    {
        state = BlockState.GuardBreak;
        guardBreakTimer = guardBreakDuration;

        if (stamina != null)
        {
            stamina.Consume(stamina.currentStamina);
        }

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlocking(false);
        Anim.TriggerGuardBreak();
    }

    private void EndToIdle()
    {
        state = BlockState.None;

        Motor.SetHorizontalVelocity(Vector3.zero);

        Anim.SetMove(Vector2.zero);
        Anim.SetBlocking(false);
    }

    private void ForceEndBlock()
    {
        state = BlockState.None;
        blockCounterTimer = 0f;
        guardBreakTimer = 0f;

        Anim.SetBlocking(false);
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
        // 只有 BlockLoop 期间受击才可以变 BlockCounter。
        if (state != BlockState.BlockLoop)
            return BlockResult.NotBlocked;

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