using UnityEngine;

public class MoveAction : PlayerAction
{
    public float moveSpeed = 4f;

    [Header("Move Mode")]
    public bool useSelfDirection = true;

    public override int Priority => 0;

    public override void TickAction(float deltaTime)
    {
        if (Controller.IsMovementLocked)
        {
            Anim.SetMove(Vector2.zero);
            return;
        }

        Vector2 moveInput = Input.Move;

        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        float moveAmount = Mathf.Clamp01(moveInput.magnitude);

        Vector3 moveDirection;

        if (useSelfDirection)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = forward * moveInput.y + right * moveInput.x;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                moveDirection.Normalize();
            }
        }
        else
        {
            moveDirection = Motor.GetCameraRelativeDirection(moveInput);
        }

        Motor.SetHorizontalVelocity(moveDirection * moveSpeed * moveAmount);

        Anim.SetMove(moveInput);
    }
}