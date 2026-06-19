using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public string playerTag = "Player";

    [Header("Range")]
    public float detectRange = 8f;
    public float attackRange = 1.5f;

    public Transform Target => target;

    public float DistanceToTarget
    {
        get
        {
            if (target == null)
                return Mathf.Infinity;

            return Vector3.Distance(transform.position, target.position);
        }
    }

    private void Awake()
    {
        FindPlayerIfNeeded();
    }

    public void Tick()
    {
        FindPlayerIfNeeded();
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

    public bool HasTarget()
    {
        return target != null;
    }

    public bool HasTargetInDetectRange()
    {
        return HasTarget() && DistanceToTarget <= detectRange;
    }

    public bool HasTargetInAttackRange()
    {
        return HasTarget() && DistanceToTarget <= attackRange;
    }

    public Vector3 GetDirectionToTarget()
    {
        if (target == null)
            return Vector3.zero;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return Vector3.zero;

        return direction.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
