using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerAnimatorBridge))]
public class PlayerActionController : MonoBehaviour
{
    public PlayerInputData CurrentInput { get; private set; }

    public PlayerInputReader InputReader { get; private set; }
    public PlayerMotor Motor { get; private set; }
    public PlayerAnimatorBridge Anim { get; private set; }
    public PlayerHealth Health { get; private set; }

    public bool IsMovementLocked { get; private set; }

    private PlayerAction[] actions;

    private void Awake()
    {
        InputReader = GetComponent<PlayerInputReader>();
        Motor = GetComponent<PlayerMotor>();
        Anim = GetComponent<PlayerAnimatorBridge>();
        Health = GetComponent<PlayerHealth>();

        actions = GetComponents<PlayerAction>();

        Array.Sort(actions, (a, b) => b.Priority.CompareTo(a.Priority));

        foreach (PlayerAction action in actions)
        {
            action.Initialize(this);
        }
    }

    private void Update()
    {
        if (Health != null && Health.IsDead)
        {
            Motor.SetHorizontalVelocity(Vector3.zero);
            Anim.SetMove(Vector2.zero);
            Motor.Tick(Time.deltaTime);
            return;
        }

        CurrentInput = InputReader.ReadInput();

        IsMovementLocked = false;

        foreach (PlayerAction action in actions)
        {
            action.TickAction(Time.deltaTime);
        }

        Anim.SetGrounded(Motor.IsGrounded);
        Motor.Tick(Time.deltaTime);
    }

    public void LockMovement()
    {
        IsMovementLocked = true;
    }

    public bool HasActiveExclusiveAction(PlayerAction requester)
    {
        foreach (PlayerAction action in actions)
        {
            if (action == requester)
                continue;

            if (action.IsActive && action.BlocksOtherActions)
                return true;
        }

        return false;
    }
}