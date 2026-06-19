using UnityEngine;

public class PlayerHitReactAction : PlayerAction
{
    private PlayerHealth health;

    public override int Priority => 200;
    public override bool IsActive => health != null && health.IsHitStunned;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    public override void TickAction(float deltaTime)
    {
        if (health == null)
            return;

        if (!health.IsHitStunned)
            return;

        Controller.LockMovement();

        Motor.SetHorizontalVelocity(Vector3.zero);
        Anim.SetMove(Vector2.zero);
    }
}