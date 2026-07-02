using System.Collections;
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
    private Coroutine knockbackCoroutine;

    public bool IsKnockbackActive { get; private set; }

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
        if (IsKnockbackActive)
            return;

        HorizontalVelocity = Vector3.zero;

        if (characterController.isGrounded)
        {
            VerticalVelocity = groundedStickForce;
        }
    }

    public void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        if (distance <= 0f)
            return;

        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, distance, duration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float distance, float duration)
    {
        IsKnockbackActive = true;
        HorizontalVelocity = Vector3.zero;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;
        direction.Normalize();

        float safeDuration = Mathf.Max(duration, 0.05f);
        float moved = 0f;

        while (moved < distance)
        {
            float step = distance * (Time.deltaTime / safeDuration);
            step = Mathf.Min(step, distance - moved);
            characterController.Move(direction * step);
            moved += step;
            yield return null;
        }

        IsKnockbackActive = false;
        knockbackCoroutine = null;
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
