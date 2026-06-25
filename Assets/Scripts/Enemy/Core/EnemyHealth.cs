using UnityEngine;

public class EnemyHealth : CombatHealth
{
    private EnemyAnimatorBridge anim;
    private EnemyMotor motor;
    private EnemyActionController controller;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<EnemyAnimatorBridge>();
        motor = GetComponent<EnemyMotor>();
        controller = GetComponent<EnemyActionController>();

        // ²»ÒªÐ´ËÀ maxHealth / attackDamage
    }

    protected override void OnHit(GameObject attacker)
    {
        if (controller != null)
        {
            controller.SetState(EnemyState.Hit);
        }

        if (motor != null)
        {
            motor.StopHorizontal();
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
            motor.StopHorizontal();
        }

        if (anim != null)
        {
            anim.TriggerDie();
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        Debug.Log($"{gameObject.name} Dead");
    }
}