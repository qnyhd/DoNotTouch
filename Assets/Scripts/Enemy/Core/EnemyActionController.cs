using System;
using UnityEngine;

[RequireComponent(typeof(EnemySensor))]
[RequireComponent(typeof(EnemyMotor))]
[RequireComponent(typeof(EnemyAnimatorBridge))]
public class EnemyActionController : MonoBehaviour
{
    public EnemyState CurrentState { get; private set; }

    public EnemySensor Sensor { get; private set; }
    public EnemyMotor Motor { get; private set; }
    public EnemyAnimatorBridge Anim { get; private set; }
    public EnemyHealth Health { get; private set; }

    public bool IsMovementLocked { get; private set; }

    [Header("Pause")]
    public float idlePauseAfterBackstep = 0.35f;

    private float idlePauseTimer;

    private EnemyAction[] actions;

    private bool backstepRequested;

    private void Awake()
    {
        Sensor = GetComponent<EnemySensor>();
        Motor = GetComponent<EnemyMotor>();
        Anim = GetComponent<EnemyAnimatorBridge>();
        Health = GetComponent<EnemyHealth>();

        actions = GetComponents<EnemyAction>();

        Array.Sort(actions, (a, b) => b.Priority.CompareTo(a.Priority));

        foreach (EnemyAction action in actions)
        {
            action.Initialize(this);
        }
    }

    private void Update()
    {
        if (Health != null && Health.IsDead)
        {
            Motor.ForceStop();
            Anim.SetSpeed(0f);
            Motor.Tick(Time.deltaTime);
            return;
        }

        Sensor.Tick();

        IsMovementLocked = false;

        if (idlePauseTimer > 0f)
        {
            idlePauseTimer -= Time.deltaTime;

            IsMovementLocked = true;
            CurrentState = EnemyState.Idle;

            Motor.ForceStop();
            Anim.SetSpeed(0f);

            Motor.Tick(Time.deltaTime);
            return;
        }

        foreach (EnemyAction action in actions)
        {
            action.TickAction(Time.deltaTime);
        }

        Motor.Tick(Time.deltaTime);
    }

    public void SetState(EnemyState state)
    {
        CurrentState = state;
    }

    public void LockMovement()
    {
        IsMovementLocked = true;
    }

    public void StartIdlePauseAfterBackstep()
    {
        idlePauseTimer = idlePauseAfterBackstep;
        IsMovementLocked = true;
        Motor.ForceStop();
        Anim.SetSpeed(0f);
    }

    public void RequestBackstep()
    {
        backstepRequested = true;
    }

    public bool ConsumeBackstepRequest()
    {
        if (!backstepRequested)
            return false;

        backstepRequested = false;
        return true;
    }

    public bool HasActiveExclusiveAction(EnemyAction requester)
    {
        foreach (EnemyAction action in actions)
        {
            if (action == requester)
                continue;

            if (action.IsActive && action.BlocksOtherActions)
                return true;
        }

        return false;
    }
}