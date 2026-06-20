using UnityEngine;

public class EnemyBlockAction : EnemyAction, IBlockHandler
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
    public float holdStaminaCostPerSecond = 0.8f;
    public float blockStaminaCost = 3f;
    public float minStaminaToStartBlock = 1f;

    [Header("AI Decision")]
    public float decisionInterval = 0.2f;
    public float blockChance = 0.75f;
    public float blockDetectDistance = 2.5f;

    [Header("Block Duration")]
    public float minBlockDuration = 0.5f;
    public float maxBlockDuration = 1.2f;

    [Header("Block Counter")]
    public float blockCounterDuration = 0.65f;

    [Header("Guard Break")]
    public float guardBreakDuration = 0.9f;

    private BlockState state = BlockState.None;

    private float blockTimer;
    private float decisionTimer;
    private float blockCounterTimer;
    private float guardBreakTimer;

    private CombatStamina stamina;
    private EnemyHealth health;

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
    public bool IsBlocking => state == BlockState.BlockLoop;

    public bool IsGuardBroken => state == BlockState.GuardBreak;

    private void Awake()
    {
        stamina = GetComponent<CombatStamina>();
        health = GetComponent<EnemyHealth>();
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
                TickNone(deltaTime);
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

    private void TickNone(float deltaTime)
    {
        Anim.SetBlocking(false);

        decisionTimer -= deltaTime;

        if (decisionTimer > 0f)
            return;

        decisionTimer = decisionInterval;

        if (ShouldEnemyStartBlock())
        {
            StartBlockLoop();
        }
    }

    private void TickBlockLoop(float deltaTime)
    {
        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();
        Anim.SetSpeed(0f);
        Anim.SetBlocking(true);

        RotateToTarget();

        bool hadEnough = stamina.Consume(holdStaminaCostPerSecond * deltaTime);

        if (!hadEnough || stamina.IsEmpty)
        {
            StartGuardBreak();
            return;
        }

        blockTimer -= deltaTime;

        if (blockTimer <= 0f)
        {
            EndToIdleOrChase();
        }
    }

    private void TickBlockCounter(float deltaTime)
    {
        blockCounterTimer -= deltaTime;

        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();
        Anim.SetSpeed(0f);

        RotateToTarget();

        bool shouldReturnToBlockLoop = ShouldReturnToBlockLoopAfterCounter();

        // 让 Animator 自己根据 Blocking 决定 BlockCounter 播完后回哪里。
        Anim.SetBlocking(shouldReturnToBlockLoop);

        if (blockCounterTimer <= 0f)
        {
            if (shouldReturnToBlockLoop)
            {
                ResumeBlockLoopAfterCounter();
            }
            else
            {
                EndToIdleOrChase();
            }
        }
    }

    private void TickGuardBreak(float deltaTime)
    {
        guardBreakTimer -= deltaTime;

        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();
        Anim.SetSpeed(0f);
        Anim.SetBlocking(false);

        if (guardBreakTimer <= 0f)
        {
            EndToIdleOrChase();
        }
    }

    private bool ShouldEnemyStartBlock()
    {
        if (stamina == null)
            return false;

        if (stamina.currentStamina < minStaminaToStartBlock)
            return false;

        if (Controller.IsInIdlePause)
            return false;

        if (Controller.CurrentState == EnemyState.Attack)
            return false;

        if (Controller.CurrentState == EnemyState.Backstep)
            return false;

        if (Controller.CurrentState == EnemyState.Hit)
            return false;

        if (Controller.CurrentState == EnemyState.Dead)
            return false;

        if (Controller.HasActiveExclusiveAction(this))
            return false;

        if (!Sensor.HasTarget())
            return false;

        if (Sensor.DistanceToTarget > blockDetectDistance)
            return false;

        AttackAction playerAttack = Sensor.Target.GetComponentInParent<AttackAction>();

        if (playerAttack == null)
            return false;

        if (!playerAttack.IsAttacking)
            return false;

        return Random.value <= blockChance;
    }

    private bool ShouldReturnToBlockLoopAfterCounter()
    {
        if (stamina == null)
            return false;

        if (stamina.currentStamina < minStaminaToStartBlock)
            return false;

        if (blockTimer <= 0f)
            return false;

        if (Controller.CurrentState == EnemyState.Dead)
            return false;

        return true;
    }

    private void StartBlockLoop()
    {
        state = BlockState.BlockLoop;

        float min = Mathf.Min(minBlockDuration, maxBlockDuration);
        float max = Mathf.Max(minBlockDuration, maxBlockDuration);

        blockTimer = Random.Range(min, max);

        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();

        RotateToTarget();

        Anim.SetSpeed(0f);
        Anim.SetBlocking(true);
    }

    private void ResumeBlockLoopAfterCounter()
    {
        state = BlockState.BlockLoop;

        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();

        RotateToTarget();

        Anim.SetSpeed(0f);
        Anim.SetBlocking(true);
    }

    private void StartBlockCounter()
    {
        state = BlockState.BlockCounter;
        blockCounterTimer = blockCounterDuration;

        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();

        RotateToTarget();

        Anim.SetSpeed(0f);
        Anim.TriggerBlockCounter();
    }

    private void StartGuardBreak()
    {
        state = BlockState.GuardBreak;
        guardBreakTimer = guardBreakDuration;

        if (stamina != null)
        {
            stamina.Consume(stamina.currentStamina);
        }

        Controller.SetState(EnemyState.Block);
        Controller.LockMovement();

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetBlocking(false);
        Anim.TriggerGuardBreak();
    }

    private void EndToIdleOrChase()
    {
        state = BlockState.None;

        Motor.ForceStop();
        Anim.SetSpeed(0f);
        Anim.SetBlocking(false);

        if (Sensor.HasTargetInDetectRange())
        {
            Controller.SetState(EnemyState.Chase);
        }
        else
        {
            Controller.SetState(EnemyState.Idle);
        }
    }

    private void ForceEndBlock()
    {
        state = BlockState.None;

        blockTimer = 0f;
        blockCounterTimer = 0f;
        guardBreakTimer = 0f;

        Anim.SetBlocking(false);
    }

    private void RotateToTarget()
    {
        Vector3 directionToTarget = Sensor.GetDirectionToTarget();

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Motor.RotateToDirection(directionToTarget);
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
        // 有且仅有 BlockLoop 期间受击才会触发 BlockCounter。
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