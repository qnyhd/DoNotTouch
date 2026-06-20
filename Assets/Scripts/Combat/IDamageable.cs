using UnityEngine;

public interface IDamageable
{
    bool IsDead { get; }

    void TakeDamage(DamageInfo info);
}