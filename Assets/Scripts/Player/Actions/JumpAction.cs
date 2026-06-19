using UnityEngine;

public class JumpAction : PlayerAction
{
    public float jumpForce = 7f;

    public override int Priority => 50;

    public override void TickAction(float deltaTime)
    {
        if (!Input.JumpPressed)
            return;

        if (!Motor.IsGrounded)
            return;

        if (Controller.HasActiveExclusiveAction(this))
            return;

        Motor.SetVerticalVelocity(jumpForce);
        Anim.TriggerJump();
    }
}
