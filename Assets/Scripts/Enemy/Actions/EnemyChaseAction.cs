using UnityEngine;

public class EnemyChaseAction : EnemyAction
{
    private EnemyNavigation navigation;

    public override int Priority => 10;

    private void Awake()
    {
        navigation = GetComponent<EnemyNavigation>();
    }

    public override void TickAction(float deltaTime)
    {
        if (Controller.IsMovementLocked)
            return;

        if (!Sensor.HasTargetInDetectRange())
        {
            StopChase();
            return;
        }

        if (Sensor.HasTargetInAttackRange())
        {
            StopChase();
            return;
        }

        Vector3 direction = Vector3.zero;

        if (navigation != null && navigation.IsReady)
        {
            navigation.SetDestination(Sensor.Target.position);
            direction = navigation.GetPathDirection();
        }

        // 如果 NavMesh 临时给不出方向，就退回原来的直线方向
        // 这样至少不会直接站着发呆
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Sensor.GetDirectionToTarget();
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            return;
        }

        Controller.SetState(EnemyState.Chase);

        Motor.SetHorizontalVelocity(direction * Motor.moveSpeed);
        Motor.RotateToDirection(direction);

        Anim.SetSpeed(1f);
    }

    private void StopChase()
    {
        if (navigation != null)
        {
            navigation.StopPath();
        }

        Motor.ForceStop();
        Anim.SetSpeed(0f);
    }
}