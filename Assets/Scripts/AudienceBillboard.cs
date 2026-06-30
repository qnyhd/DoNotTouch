using UnityEngine;

/// <summary>
/// 让 2D 观众贴图部分朝向镜头，而不是完全 Billboard。
/// 把物体摆好初始朝向后，运行时会按 followStrength 轻微跟随相机。
/// </summary>
public class AudienceBillboard : MonoBehaviour
{
    [Tooltip("留空则使用 Main Camera")]
    public Transform cameraTransform;

    [Tooltip("跟随强度：0=不转，1=完全正对镜头。建议 0.2~0.4")]
    [Range(0f, 1f)]
    public float followStrength = 0.3f;

    private Quaternion baseRotation;

    private void Start()
    {
        baseRotation = transform.rotation;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 toCamera = cameraTransform.position - transform.position;
        toCamera.y = 0f;

        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        Quaternion faceCamera = Quaternion.LookRotation(toCamera, Vector3.up);
        transform.rotation = Quaternion.Slerp(baseRotation, faceCamera, followStrength);
    }
}
