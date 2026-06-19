using UnityEngine;

public class EnemyHitReactAction : EnemyAction
{
    private EnemyHealth health;

    public override int Priority => 200;
    public override bool IsActive => health != null && health.IsHitStunned;
    public override bool BlocksOtherActions => true;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    public override void TickAction(float deltaTime)
    {
        if (health == null)
            return;

        if (!health.IsHitStunned)
            return;

        Controller.SetState(EnemyState.Hit);

        Controller.LockMovement();

        Motor.StopHorizontal();
        Anim.SetSpeed(0f);
    }
}
