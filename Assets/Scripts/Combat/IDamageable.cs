using UnityEngine;

public interface IDamageable
{
    bool IsDead { get; }

    void TakeDamage(int damage, GameObject attacker);
}