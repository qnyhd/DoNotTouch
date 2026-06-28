using UnityEngine;

public class EnemyRangedAttackAction : TimedEnemyAction
{
    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public GameObject targetCirclePrefab;

    [Header("Timing")]
    public float throwTime = 0.55f;

    [Header("Projectile")]
    public int damage = 2;
    public float arcHeight = 3f;
    public float targetCircleRadius = 1.2f;

    [Header("Range")]
    public float minShootDistance = 4f;

    [Header("Forced Attack")]
    public bool forcedAttackIgnoresCooldown = true;

    [Header("Ground")]
    public LayerMask groundLayers = ~0;
    public float groundRayHeight = 6f;
    public float groundRayDistance = 20f;

    private bool hasThrown;
    private bool isForcedAttack;

    private Vector3 lockedTargetPoint;
    private GameObject currentCircle;
    private EnemyHealth enemyHealth;

    public override int Priority => 100;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public override void TickAction(float deltaTime)
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= deltaTime;
        }

        if (timer > 0f)
        {
            timer -= deltaTime;

            OnActiveTick(deltaTime);

            if (timer <= 0f)
            {
                OnEnd();
            }

            return;
        }

        bool hasForcedRequest = Controller.HasForcedRangedAttackRequest;
        bool canNormalAttack = ShouldStartNormalAttack();

        if (!hasForcedRequest && !canNormalAttack)
            return;

        if (Controller.HasActiveExclusiveAction(this))
            return;

        if (!hasForcedRequest && cooldownTimer > 0f)
            return;

        if (hasForcedRequest && !forcedAttackIgnoresCooldown && cooldownTimer > 0f)
            return;

        isForcedAttack = Controller.ConsumeForcedRangedAttackRequest();

        timer = duration;
        cooldownTimer = cooldown;

        OnStart();
        OnActiveTick(deltaTime);
    }

    protected override bool ShouldStart()
    {
        return ShouldStartNormalAttack();
    }

    private bool ShouldStartNormalAttack()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
            return false;

        if (!Sensor.HasTarget())
            return false;

        if (!Sensor.HasTargetInAttackRange())
            return false;

        // 普通攻击：玩家太近时不投石
        if (Sensor.DistanceToTarget < minShootDistance)
            return false;

        return true;
    }

    protected override void OnStart()
    {
        hasThrown = false;

        ClearCurrentCircle();

        Controller.SetState(EnemyState.Attack);

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetRetreating(false);

        Vector3 direction = Sensor.GetDirectionToTarget();
        Motor.RotateToDirection(direction);

        lockedTargetPoint = GetGroundPointUnderTarget();

        SpawnTargetCircle(lockedTargetPoint);

        Anim.TriggerAttack();
    }

    protected override void OnActiveTick(float deltaTime)
    {
        Controller.LockMovement();

        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetRetreating(false);

        Vector3 direction = Sensor.GetDirectionToTarget();

        if (direction.sqrMagnitude > 0.01f)
        {
            Motor.RotateToDirection(direction);
        }

        float elapsed = duration - timer;

        if (!hasThrown && elapsed >= throwTime)
        {
            hasThrown = true;
            ThrowRock();
        }
    }

    protected override void OnEnd()
    {
        Motor.ForceStop();

        Anim.SetSpeed(0f);
        Anim.SetRetreating(false);

        if (!hasThrown)
        {
            ClearCurrentCircle();
        }

        isForcedAttack = false;
    }

    private Vector3 GetGroundPointUnderTarget()
    {
        if (Sensor.Target == null)
            return transform.position;

        Vector3 targetPosition = Sensor.Target.position;
        Vector3 rayOrigin = targetPosition + Vector3.up * groundRayHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return targetPosition;
    }

    private void SpawnTargetCircle(Vector3 point)
    {
        if (targetCirclePrefab == null)
            return;

        currentCircle = Instantiate(
            targetCirclePrefab,
            point + Vector3.up * 0.03f,
            Quaternion.identity
        );

        currentCircle.transform.localScale = new Vector3(
            targetCircleRadius,
            currentCircle.transform.localScale.y,
            targetCircleRadius
        );
    }

    private void ThrowRock()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"{gameObject.name} 没有设置 projectilePrefab");
            ClearCurrentCircle();
            return;
        }

        Vector3 startPosition = throwPoint != null
            ? throwPoint.position
            : transform.position + Vector3.up * 1.4f + transform.forward * 0.5f;

        GameObject rockObj = Instantiate(
            projectilePrefab,
            startPosition,
            Quaternion.identity
        );

        RockProjectile rock = rockObj.GetComponentInChildren<RockProjectile>();

        if (rock == null)
        {
            Debug.LogError("石头预制体上没有 RockProjectile 脚本");
            Destroy(rockObj);
            ClearCurrentCircle();
            return;
        }

        rock.damageRadius = targetCircleRadius * 0.5f;

        rock.Launch(
            startPosition,
            lockedTargetPoint + Vector3.up * 0.05f,
            gameObject,
            damage,
            arcHeight,
            currentCircle
        );

        currentCircle = null;
    }

    private void ClearCurrentCircle()
    {
        if (currentCircle != null)
        {
            Destroy(currentCircle);
            currentCircle = null;
        }
    }
}