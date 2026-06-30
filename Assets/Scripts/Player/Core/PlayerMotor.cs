using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    public float gravity = -20f;
    public float groundedStickForce = -2f;
    public float rotationSpeed = 720f;
    public float maxFallSpeed = 12f;

    private CharacterController characterController;
    private float lastContactAngle = 0f;
    private float tickContactAngle = 90f;
    private Vector3 lastSteepNormal = Vector3.up;

    public Vector3 HorizontalVelocity { get; private set; }
    public float VerticalVelocity { get; private set; }

    public bool IsGrounded =>
        characterController.isGrounded &&
        lastContactAngle <= characterController.slopeLimit;

    public bool CanJump => characterController.isGrounded;

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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float angle = Vector3.Angle(hit.normal, Vector3.up);
        tickContactAngle = Mathf.Min(tickContactAngle, angle);

        if (angle > characterController.slopeLimit)
            lastSteepNormal = hit.normal;
    }

    private Vector3 StripSteepSurfaceVelocity(Vector3 velocity, Vector3 surfaceNormal)
    {
        Vector3 uphill = Vector3.ProjectOnPlane(Vector3.up, surfaceNormal);
        if (uphill.sqrMagnitude > 0.01f)
        {
            uphill.Normalize();
            float uphillSpeed = Vector3.Dot(velocity, uphill);
            if (uphillSpeed > 0f)
                velocity -= uphill * uphillSpeed;
        }

        float intoSurface = Vector3.Dot(velocity, -surfaceNormal);
        if (intoSurface > 0f)
            velocity += surfaceNormal * intoSurface;

        return velocity;
    }

    public void Tick(float deltaTime)
    {
        tickContactAngle = 90f;

        bool onSteepSurface =
            characterController.isGrounded &&
            lastContactAngle > characterController.slopeLimit;

        if (IsGrounded && VerticalVelocity < 0f)
            VerticalVelocity = groundedStickForce;
        else
            VerticalVelocity += gravity * deltaTime;

        VerticalVelocity = Mathf.Max(VerticalVelocity, -maxFallSpeed);

        Vector3 move;

        if (onSteepSurface && VerticalVelocity <= 0f)
        {
            // 站不住的坡/墙：只沿表面滑，不吃方向键，避免贴墙蓄力
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, lastSteepNormal);
            if (slideDir.sqrMagnitude > 0.01f)
                move = slideDir.normalized * Mathf.Abs(VerticalVelocity);
            else
                move = Vector3.up * VerticalVelocity;
        }
        else
        {
            Vector3 horizontal = HorizontalVelocity;
            if (onSteepSurface)
                horizontal = StripSteepSurfaceVelocity(horizontal, lastSteepNormal);

            move = horizontal;
            move.y = VerticalVelocity;
        }

        characterController.Move(move * deltaTime);

        lastContactAngle = characterController.isGrounded ? tickContactAngle : 90f;
    }
}
