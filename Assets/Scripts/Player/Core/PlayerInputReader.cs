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
        input.AttackPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J);

        // 鼠标右键按住格挡
        input.BlockHeld = Input.GetMouseButton(1);

        return input;
    }
}