using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyMotor : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 720f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedStickForce = -2f;

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

    public void StopHorizontal()
    {
        HorizontalVelocity = Vector3.zero;
    }

    public void ForceStop()
    {
        HorizontalVelocity = Vector3.zero;

        if (characterController.isGrounded)
        {
            VerticalVelocity = groundedStickForce;
        }
    }

    public void RotateToDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
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
