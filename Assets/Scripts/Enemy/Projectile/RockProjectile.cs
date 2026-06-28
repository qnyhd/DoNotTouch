using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RockProjectile : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 2;
    public float damageRadius = 0.6f;
    public LayerMask damageLayers = ~0;

    [Header("Life")]
    public float maxLifeTime = 6f;
    public float destroyDelayAfterImpact = 0.05f;

    private Rigidbody rb;
    private Collider[] projectileColliders;

    private Transform ownerRoot;
    private GameObject owner;
    private GameObject targetCircle;

    private bool launched;
    private bool impacted;

    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileColliders = GetComponentsInChildren<Collider>();

        // 重点：没发射前不参与物理，不然场景里的 Rock 会自己掉下来砸人
        rb.isKinematic = true;
        rb.useGravity = false;

        foreach (Collider col in projectileColliders)
        {
            col.enabled = false;
        }
    }

    public void Launch(
        Vector3 startPosition,
        Vector3 targetPosition,
        GameObject attacker,
        int projectileDamage,
        float arcHeight,
        GameObject circleObject
    )
    {
        launched = true;
        impacted = false;
        damagedTargets.Clear();

        owner = attacker;
        ownerRoot = attacker != null ? attacker.transform.root : null;

        damage = projectileDamage;
        targetCircle = circleObject;

        transform.position = startPosition;

        foreach (Collider col in projectileColliders)
        {
            col.enabled = true;
        }

        IgnoreOwnerCollision();

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 velocity = CalculateBallisticVelocity(startPosition, targetPosition, arcHeight);

        // Unity 6 用 linearVelocity
        rb.linearVelocity = velocity;
        rb.angularVelocity = Random.insideUnitSphere * 8f;

        Destroy(gameObject, maxLifeTime);
    }

    private void IgnoreOwnerCollision()
    {
        if (ownerRoot == null)
            return;

        Collider[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider>();

        foreach (Collider rockCol in projectileColliders)
        {
            foreach (Collider ownerCol in ownerColliders)
            {
                if (rockCol != null && ownerCol != null)
                {
                    Physics.IgnoreCollision(rockCol, ownerCol, true);
                }
            }
        }
    }

    private Vector3 CalculateBallisticVelocity(Vector3 start, Vector3 target, float arcHeight)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 displacement = target - start;
        Vector3 displacementXZ = new Vector3(displacement.x, 0f, displacement.z);

        float heightDifference = target.y - start.y;

        float height = Mathf.Max(arcHeight, heightDifference + 0.5f);

        float velocityY = Mathf.Sqrt(2f * gravity * height);
        float timeUp = velocityY / gravity;

        float descendHeight = height - heightDifference;
        float timeDown = Mathf.Sqrt(2f * descendHeight / gravity);

        float totalTime = timeUp + timeDown;

        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + Vector3.up * velocityY;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!launched || impacted)
            return;

        if (IsOwner(collision.collider))
            return;

        ContactPoint contact = collision.GetContact(0);
        Impact(contact.point);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!launched || impacted)
            return;

        if (IsOwner(other))
            return;

        Impact(transform.position);
    }

    private bool IsOwner(Collider col)
    {
        if (col == null || ownerRoot == null)
            return false;

        return col.transform.root == ownerRoot;
    }

    private void Impact(Vector3 impactPoint)
    {
        impacted = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        TryDamageRadius(impactPoint);

        DestroyTargetCircle();

        Destroy(gameObject, destroyDelayAfterImpact);
    }

    private void TryDamageRadius(Vector3 impactPoint)
    {
        if (damageRadius <= 0f)
            return;

        Collider[] colliders = Physics.OverlapSphere(
            impactPoint,
            damageRadius,
            damageLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider col in colliders)
        {
            if (IsOwner(col))
                continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damageable.IsDead)
                continue;

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);

            Vector3 attackDirection = rb.linearVelocity.sqrMagnitude > 0.01f
                ? rb.linearVelocity.normalized
                : Vector3.down;

            DamageInfo info = new DamageInfo(
                damage,
                0f,
                owner,
                impactPoint,
                attackDirection,
                false
            );

            damageable.TakeDamage(info);
        }
    }

    private void DestroyTargetCircle()
    {
        if (targetCircle != null)
        {
            Destroy(targetCircle);
            targetCircle = null;
        }
    }

    private void OnDestroy()
    {
        DestroyTargetCircle();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}