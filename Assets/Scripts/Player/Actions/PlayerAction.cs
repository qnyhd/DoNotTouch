using UnityEngine;

public abstract class PlayerAction : MonoBehaviour
{
    protected PlayerActionController Controller { get; private set; }
    protected PlayerMotor Motor => Controller.Motor;
    protected PlayerAnimatorBridge Anim => Controller.Anim;
    protected PlayerInputData Input => Controller.CurrentInput;

    public virtual int Priority => 0;
    public virtual bool IsActive => false;
    public virtual bool BlocksOtherActions => false;

    public virtual void Initialize(PlayerActionController controller)
    {
        Controller = controller;
    }

    public abstract void TickAction(float deltaTime);
}
