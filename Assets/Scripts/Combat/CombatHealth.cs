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

    public void TakeDamage(DamageInfo info)
    {
        if (IsDead)
            return;

        if (TryBlockDamage(info))
        {
            return;
        }

        currentHealth -= info.damage;

        Debug.Log($"{gameObject.name} took {info.damage} damage. HP = {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            IsDead = true;
            hitStunTimer = 0f;

            OnDeath(info.attacker);
            return;
        }

        hitStunTimer = hitStunDuration;
        OnHit(info.attacker);
    }

    // 兼容旧代码，如果还有地方调用 TakeDamage(2, attacker)，也不会报错
    public void TakeDamage(int damage, GameObject attacker)
    {
        DamageInfo info = new DamageInfo(
            damage,
            0f,
            attacker,
            transform.position,
            attacker != null ? transform.position - attacker.transform.position : -transform.forward,
            true
        );

        TakeDamage(info);
    }

    private bool TryBlockDamage(DamageInfo info)
    {
        if (!info.canBeBlocked)
            return false;

        IBlockHandler blockHandler = GetBlockHandler();

        if (blockHandler == null)
            return false;

        if (!blockHandler.IsBlocking)
            return false;

        Vector3 attackerPosition;

        if (info.attacker != null)
        {
            attackerPosition = info.attacker.transform.position;
        }
        else
        {
            attackerPosition = transform.position - info.attackDirection;
        }

        if (!blockHandler.CanBlockAttack(attackerPosition))
            return false;

        BlockResult result = blockHandler.TryBlock(info);

        return result == BlockResult.Blocked || result == BlockResult.GuardBroken;
    }

    private IBlockHandler GetBlockHandler()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IBlockHandler blockHandler)
            {
                return blockHandler;
            }
        }

        return null;
    }

    protected abstract void OnHit(GameObject attacker);

    protected abstract void OnDeath(GameObject attacker);
}