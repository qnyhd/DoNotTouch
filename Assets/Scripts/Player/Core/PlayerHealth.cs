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

        // 不要在这里写死 maxHealth 和 attackDamage
        // currentHealth 已经在 CombatHealth 的 Awake 里根据 maxHealth 初始化了
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