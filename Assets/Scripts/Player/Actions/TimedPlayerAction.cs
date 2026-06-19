using UnityEngine;

public abstract class TimedPlayerAction : PlayerAction
{
    public float duration = 0.3f;
    public float cooldown = 0.2f;

    protected float timer;
    protected float cooldownTimer;

    public override bool IsActive => timer > 0f;

    public override void TickAction(float deltaTime)
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= deltaTime;
        }

        if (timer > 0f)
        {
            timer -= deltaTime;
            OnActiveTick(deltaTime);

            if (timer <= 0f)
            {
                OnEnd();
            }

            return;
        }

        if (ShouldStart() && cooldownTimer <= 0f && !Controller.HasActiveExclusiveAction(this))
        {
            timer = duration;
            cooldownTimer = cooldown;

            OnStart();
            OnActiveTick(deltaTime);
        }
    }

    protected abstract bool ShouldStart();

    protected virtual void OnStart() { }

    protected virtual void OnActiveTick(float deltaTime) { }

    protected virtual void OnEnd() { }
}
