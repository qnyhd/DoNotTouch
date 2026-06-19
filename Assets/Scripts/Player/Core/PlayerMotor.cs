using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    public float gravity = -20f;
    public float groundedStickForce = -2f;
    public float rotationSpeed = 720f;

    private CharacterController characterController;

    public Vector3 HorizontalVelocity { get; private set; }
    public float VerticalVelocity { get; private set; }

    public bool IsGrounded => characterController.isGrounded;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetHorizontalVelocity(Vector3 velocity)
    {
        HorizontalVelocity = velocity;
    }

    public void SetVerticalVelocity(float velocity)
    {
        VerticalVelocity = velocity;
    }

    public void RotateToDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public Vector3 GetCameraRelativeDirection(Vector2 moveInput)
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        if (direction.sqrMagnitude < 0.01f)
            return Vector3.zero;

        if (Camera.main == null)
            return direction.normalized;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 finalDirection = camForward * moveInput.y + camRight * moveInput.x;
        return finalDirection.normalized;
    }

    public void Tick(float deltaTime)
    {
        if (characterController.isGrounded && VerticalVelocity < 0f)
        {
            VerticalVelocity = groundedStickForce;
        }
        else
        {
            VerticalVelocity += gravity * deltaTime;
        }

        Vector3 finalVelocity = HorizontalVelocity;
        finalVelocity.y = VerticalVelocity;

        characterController.Move(finalVelocity * deltaTime);
    }
}