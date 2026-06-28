using UnityEngine;

public class EnemyHealth : CombatHealth
{
    [Header("Death Collision")]
    public bool disableCollisionOnDeath = true;
    public bool disableTriggerCollidersToo = false;

    private EnemyAnimatorBridge anim;
    private EnemyMotor motor;
    private EnemyActionController controller;

    private CharacterController characterController;
    private Collider[] colliders;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<EnemyAnimatorBridge>();
        motor = GetComponent<EnemyMotor>();
        controller = GetComponent<EnemyActionController>();

        characterController = GetComponent<CharacterController>();
        colliders = GetComponentsInChildren<Collider>();

        maxHealth = 10;
        currentHealth = maxHealth;
        attackDamage = 2;
    }

    protected override void OnHit(GameObject attacker)
    {
        if (controller != null)
        {
            controller.SetState(EnemyState.Hit);
        }

        if (motor != null)
        {
            motor.ForceStop();
        }

        if (anim != null)
        {
            anim.TriggerHit();
        }
    }

    protected override void OnDeath(GameObject attacker)
    {
        if (controller != null)
        {
            controller.SetState(EnemyState.Dead);
        }

        if (motor != null)
        {
            motor.ForceStop();
        }

        if (anim != null)
        {
            anim.TriggerDie();
        }

        DisableCollisionAfterDeath();

        if (controller != null)
        {
            controller.enabled = false;
        }

        Debug.Log($"{gameObject.name} Dead");
    }

    private void DisableCollisionAfterDeath()
    {
        if (!disableCollisionOnDeath)
            return;

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (colliders == null)
            return;

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            if (col.isTrigger && !disableTriggerCollidersToo)
                continue;

            col.enabled = false;
        }
    }
}