using UnityEngine;

public struct DamageInfo
{
    public int damage;
    public float staminaDamage;
    public GameObject attacker;
    public Vector3 hitPoint;
    public Vector3 attackDirection;
    public bool canBeBlocked;
    public float knockbackDistance;

    public DamageInfo(
        int damage,
        float staminaDamage,
        GameObject attacker,
        Vector3 hitPoint,
        Vector3 attackDirection,
        bool canBeBlocked = true,
        float knockbackDistance = 0f
    )
    {
        this.damage = damage;
        this.staminaDamage = staminaDamage;
        this.attacker = attacker;
        this.hitPoint = hitPoint;
        this.attackDirection = attackDirection.sqrMagnitude > 0.01f
            ? attackDirection.normalized
            : Vector3.forward;
        this.canBeBlocked = canBeBlocked;
        this.knockbackDistance = knockbackDistance;
    }
}
