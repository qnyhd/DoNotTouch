using UnityEngine;

public struct PlayerInputData
{
    public Vector2 Move;

    public bool HasValidMoveInput;
    public bool InvalidMoveInput;
    public int MoveKeyCount;

    public bool JumpPressed;
    public bool DashPressed;
    public bool AttackPressed;
    public bool BlockHeld;
}
