using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public PlayerInputData ReadInput()
    {
        PlayerInputData input = new PlayerInputData();

        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);
        bool a = Input.GetKey(KeyCode.A);
        bool d = Input.GetKey(KeyCode.D);

        input.MoveKeyCount = 0;

        if (w) input.MoveKeyCount++;
        if (s) input.MoveKeyCount++;
        if (a) input.MoveKeyCount++;
        if (d) input.MoveKeyCount++;

        bool oppositeForwardBack = w && s;
        bool oppositeLeftRight = a && d;
        bool tooManyKeys = input.MoveKeyCount > 2;

        input.InvalidMoveInput = oppositeForwardBack || oppositeLeftRight || tooManyKeys;

        if (input.InvalidMoveInput || input.MoveKeyCount == 0)
        {
            input.Move = Vector2.zero;
            input.HasValidMoveInput = false;
        }
        else
        {
            float x = 0f;
            float y = 0f;

            if (a) x -= 1f;
            if (d) x += 1f;
            if (w) y += 1f;
            if (s) y -= 1f;

            input.Move = new Vector2(x, y);

            if (input.Move.magnitude > 1f)
            {
                input.Move.Normalize();
            }

            input.HasValidMoveInput = input.Move.sqrMagnitude > 0.01f;
        }

        input.JumpPressed = Input.GetKeyDown(KeyCode.Space);
        input.DashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        input.AttackPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J);
        input.BlockHeld = Input.GetMouseButton(1);

        return input;
    }
}