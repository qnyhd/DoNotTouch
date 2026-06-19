using UnityEngine;

public abstract class EnemyAction : MonoBehaviour
{
    protected EnemyActionController Controller { get; private set; }
    protected EnemyMotor Motor => Controller.Motor;
    protected EnemyAnimatorBridge Anim => Controller.Anim;
    protected EnemySensor Sensor => Controller.Sensor;

    public virtual int Priority => 0;
    public virtual bool IsActive => false;
    public virtual bool BlocksOtherActions => false;

    public virtual void Initialize(EnemyActionController controller)
    {
        Controller = controller;
    }

    public abstract void TickAction(float deltaTime);
}
