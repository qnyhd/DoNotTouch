using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    [Header("Path")]
    public float repathInterval = 0.2f;
    public float sampleTargetRadius = 2f;

    private NavMeshAgent agent;
    private float repathTimer;

    public bool IsReady
    {
        get
        {
            return agent != null && agent.enabled && agent.isOnNavMesh;
        }
    }

    public bool IsOnJumpLink
    {
        get
        {
            return IsReady && agent.isOnOffMeshLink;
        }
    }

    public OffMeshLinkData CurrentLinkData
    {
        get
        {
            return agent.currentOffMeshLinkData;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // 重点：
        // 正式敌人由 EnemyMotor 移动，NavMeshAgent 只负责算路
        agent.updatePosition = false;
        agent.updateRotation = false;

        // 正式敌人不要让 Agent 自己提前停
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;
    }

    private void LateUpdate()
    {
        if (!IsReady)
            return;

        // 让 NavMeshAgent 的内部位置跟随敌人的真实 Transform
        agent.nextPosition = transform.position;
    }

    public void SetDestination(Vector3 targetPosition)
    {
        if (!IsReady)
            return;

        repathTimer -= Time.deltaTime;

        if (repathTimer > 0f)
            return;

        repathTimer = repathInterval;

        Vector3 finalTarget = targetPosition;

        // 如果玩家位置不在 NavMesh 上，找附近最近的 NavMesh 点
        if (NavMesh.SamplePosition(
            targetPosition,
            out NavMeshHit hit,
            sampleTargetRadius,
            NavMesh.AllAreas))
        {
            finalTarget = hit.position;
        }

        agent.isStopped = false;
        agent.SetDestination(finalTarget);
    }

    public Vector3 GetPathDirection()
    {
        if (!IsReady)
            return Vector3.zero;

        if (agent.pathPending)
            return Vector3.zero;

        // 优先用 desiredVelocity，它包含 Agent 当前想走的路径方向
        Vector3 desired = agent.desiredVelocity;
        desired.y = 0f;

        if (desired.sqrMagnitude > 0.01f)
        {
            return desired.normalized;
        }

        // desiredVelocity 没有时，再用 steeringTarget
        if (agent.hasPath)
        {
            Vector3 point = agent.steeringTarget;
            Vector3 direction = point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                return direction.normalized;
            }
        }

        return Vector3.zero;
    }

    public void StopPath()
    {
        if (!IsReady)
            return;

        agent.isStopped = true;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    public void CompleteJumpLink()
    {
        if (!IsReady)
            return;

        if (agent.isOnOffMeshLink)
        {
            agent.CompleteOffMeshLink();
        }
    }
}