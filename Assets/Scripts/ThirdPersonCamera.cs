using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Distance")]
    public float distance = 5f;
    public float heightOffset = 1.4f;

    [Header("Mouse Control")]
    public float mouseSensitivity = 3f;
    public bool lockCursor = true;
    public bool invertY = false;

    [Header("Vertical Clamp")]
    public float minPitch = -25f;
    public float maxPitch = 60f;

    [Header("Character Rotation")]
    public bool rotateCharacterWithCamera = true;
    public float characterRotateSmooth = 15f;

    [Header("Smooth")]
    public float followSmooth = 12f;
    public float rotateSmooth = 15f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        HandleMouseInput();
        RotateCharacterWithCamera();
        FollowTarget();
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;

        if (invertY)
        {
            pitch += mouseY;
        }
        else
        {
            pitch -= mouseY;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void RotateCharacterWithCamera()
    {
        if (!rotateCharacterWithCamera)
            return;

        Quaternion targetRotation = Quaternion.Euler(0f, yaw, 0f);

        target.rotation = Quaternion.Slerp(
            target.rotation,
            targetRotation,
            characterRotateSmooth * Time.deltaTime
        );
    }

    private void FollowTarget()
    {
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 lookPoint = target.position + Vector3.up * heightOffset;

        Vector3 desiredPosition = lookPoint - cameraRotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmooth * Time.deltaTime
        );

        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotateSmooth * Time.deltaTime
        );
    }
}