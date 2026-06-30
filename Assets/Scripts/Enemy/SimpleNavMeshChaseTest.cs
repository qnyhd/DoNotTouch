using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleNavMeshChaseTest : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public string playerTag = "Player";

    [Header("Chase")]
    public float detectRange = 12f;
    public float stopDistance = 1.5f;
    public float repathInterval = 0.2f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";

    private NavMeshAgent agent;
    private float repathTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        agent.stoppingDistance = stopDistance;

        FindPlayerIfNeeded();
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name} 不在 NavMesh 蓝色区域上，请把敌人放到烘焙出来的蓝色地面上。");
            StopMove();
            return;
        }

        FindPlayerIfNeeded();

        if (target == null)
        {
            StopMove();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > detectRange)
        {
            StopMove();
            return;
        }

        if (distance <= stopDistance)
        {
            StopMove();
            FaceTarget();
            return;
        }

        ChaseTarget();
    }

    private void FindPlayerIfNeeded()
    {
        if (target != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            target = player.transform;
        }
    }

    private void ChaseTarget()
    {
        agent.isStopped = false;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(target.position);
        }

        float moveSpeed = agent.velocity.magnitude;

        if (moveSpeed > 0.1f)
        {
            SetAnimSpeed(1f);
        }
        else
        {
            SetAnimSpeed(0f);
        }
    }

    private void StopMove()
    {
        if (agent == null)
            return;

        if (!agent.isStopped)
        {
            agent.isStopped = true;
        }

        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        SetAnimSpeed(0f);
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            agent.angularSpeed * Time.deltaTime
        );
    }

    private void SetAnimSpeed(float speed)
    {
        if (animator == null)
            return;

        animator.SetFloat(speedParameter, speed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}