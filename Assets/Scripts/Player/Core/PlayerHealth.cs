using UnityEngine;

public class PlayerHealth : CombatHealth
{
    private PlayerAnimatorBridge anim;
    private PlayerMotor motor;
    private PlayerActionController controller;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<PlayerAnimatorBridge>();
        motor = GetComponent<PlayerMotor>();
        controller = GetComponent<PlayerActionController>();

        maxHealth = 10;
        currentHealth = maxHealth;
        attackDamage = 2;
    }

    protected override void OnHit(GameObject attacker)
    {
        if (motor != null)
        {
            motor.SetHorizontalVelocity(Vector3.zero);
        }

        if (anim != null)
        {
            anim.TriggerHit();
        }
    }

    protected override void OnDeath(GameObject attacker)
    {
        if (motor != null)
        {
            motor.SetHorizontalVelocity(Vector3.zero);
        }

        if (anim != null)
        {
            anim.TriggerDie();
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        Debug.Log("Player Dead");
    }
}
