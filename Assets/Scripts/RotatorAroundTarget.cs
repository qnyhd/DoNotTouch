using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class RotatorAroundTarget : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("要围绕旋转的中心物体（圆柱体）")]
    public Transform centerTarget;

    [Tooltip("旋转速度（度/秒）")]
    public float rotationSpeed = 90f;

    [Tooltip("旋转轴，默认是Y轴(0,1,0)")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("碰撞推力设置")]
    [Tooltip("推开主角的额外力度")]
    public float pushForce = 10f;

    private Rigidbody rb;

    void Start()
    {
        // 获取刚体并设置为运动学模式
        // 运动学刚体（Kinematic）可以通过代码移动，并且能推开其他非运动学刚体（如主角），同时自身不会被撞飞
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (centerTarget != null)
        {
            // 使用 Rigidbody 的 MovePosition 和 MoveRotation 进行物理移动
            // 这样可以保证碰撞体在移动时能正确地推开主角，而不会发生穿模
            Quaternion deltaRotation = Quaternion.AngleAxis(rotationSpeed * Time.fixedDeltaTime, rotationAxis);
            
            // 计算新位置：绕中心点旋转
            Vector3 direction = rb.position - centerTarget.position;
            Vector3 newPosition = centerTarget.position + (deltaRotation * direction);
            
            // 计算新旋转
            Quaternion newRotation = deltaRotation * rb.rotation;

            rb.MovePosition(newPosition);
            rb.MoveRotation(newRotation);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 当主角撞上去时，施加一个额外的推力将其推开
        // 假设主角带有 Rigidbody 并且标签为 "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // 计算推开的方向（从当前物体中心指向主角）
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                
                // 通常我们只在水平方向推开，避免把主角击飞到天上
                pushDirection.y = 0; 
                
                // 施加瞬间的冲击力
                playerRb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
            }
        }
    }
}
