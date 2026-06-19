using UnityEngine;

public abstract class CombatHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Attack")]
    public int attackDamage = 2;

    [Header("Hit Reaction")]
    public float hitStunDuration = 0.35f;

    private float hitStunTimer;

    public bool IsDead { get; private set; }

    public bool IsHitStunned
    {
        get
        {
            return hitStunTimer > 0f && !IsDead;
        }
    }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. HP = {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            IsDead = true;
            hitStunTimer = 0f;

            OnDeath(attacker);
            return;
        }

        hitStunTimer = hitStunDuration;
        OnHit(attacker);
    }

    protected abstract void OnHit(GameObject attacker);

    protected abstract void OnDeath(GameObject attacker);
}