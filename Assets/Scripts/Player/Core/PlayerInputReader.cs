using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public PlayerInputData ReadInput()
    {
        PlayerInputData input = new PlayerInputData();

        input.Move = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        input.JumpPressed = Input.GetKeyDown(KeyCode.Space);
        input.DashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        input.AttackPressed = Input.GetMouseButtonDown(0);

        return input;
    }
}
