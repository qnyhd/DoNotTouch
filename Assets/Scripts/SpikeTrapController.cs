using System.Collections;
using UnityEngine;

public class SpikeTrapController : MonoBehaviour
{
    [Header("引用设置")]
    [Tooltip("主角的 Transform")]
    public Transform player;
    [Tooltip("主角的 Rigidbody（用于获取速度进行预判）")]
    public Rigidbody playerRb;
    [Tooltip("预警区域的物体（比如一个红色的面片）")]
    public GameObject warningArea;
    [Tooltip("地刺的父物体（包含一排地刺）")]
    public GameObject spikeRow;

    [Header("时间与节奏")]
    [Tooltip("触发间隔（冷却时间）")]
    public float cooldownTime = 3.0f;
    [Tooltip("预警持续时间")]
    public float warningTime = 1.0f;
    [Tooltip("地刺停留（攻击）时间")]
    public float attackTime = 1.0f;

    [Header("预判与攻击参数")]
    [Tooltip("预判时间（秒）：数值越大，预判位置越靠前")]
    public float predictionTime = 0.5f;
    [Tooltip("地刺升起的高度")]
    public float spikeRiseHeight = 2.0f;
    [Tooltip("地刺升起的速度")]
    public float spikeRiseSpeed = 15f;

    private Vector3 spikeInitialLocalPos;

    void Start()
    {
        // 记录地刺初始的相对位置（应该在地下）
        if (spikeRow != null)
        {
            spikeInitialLocalPos = spikeRow.transform.localPosition;
            spikeRow.SetActive(false);
        }

        if (warningArea != null)
        {
            warningArea.SetActive(false);
        }

        // 开启地刺循环逻辑
        StartCoroutine(TrapRoutine());
    }

    IEnumerator TrapRoutine()
    {
        while (true)
        {
            // 1. 等待冷却时间
            yield return new WaitForSeconds(cooldownTime);

            if (player == null) continue;

            // 2. 计算预判位置
            Vector3 targetPosition = player.position;
            Vector3 moveDirection = Vector3.zero;

            if (playerRb != null && playerRb.linearVelocity.magnitude > 0.1f)
            {
                // 如果主角在移动，根据速度和预判时间计算未来位置
                moveDirection = playerRb.linearVelocity;
                moveDirection.y = 0; // 忽略垂直方向的速度（比如跳跃）
                targetPosition += moveDirection * predictionTime;
            }

            // 保持地刺在地面高度（假设地面 Y=0，你可以根据实际地形调整）
            targetPosition.y = 0; 
            transform.position = targetPosition;

            // 让地刺排垂直于玩家的移动方向（形成横排封路的效果）
            if (moveDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            // 3. 预警阶段
            warningArea.SetActive(true);
            yield return new WaitForSeconds(warningTime);

            // 4. 攻击阶段（地刺升起）
            // 注意：这里不要把 warningArea 关掉，让预警一直显示到攻击结束
            // warningArea.SetActive(false); 
            spikeRow.SetActive(true);

            float t = 0;
            Vector3 targetSpikePos = spikeInitialLocalPos + Vector3.up * spikeRiseHeight;
            
            // 升起动画（平滑地从地下长出来）
            while (t < 1f)
            {
                // 降低这里的增长速度，让它长得更明显，而不是瞬间闪现
                // 原来是 t += Time.deltaTime * spikeRiseSpeed;
                // 我们改成用真实时间除以一个固定时长（比如0.2秒长完）
                t += Time.deltaTime * (spikeRiseSpeed * 0.5f); 
                spikeRow.transform.localPosition = Vector3.Lerp(spikeInitialLocalPos, targetSpikePos, t);
                yield return null;
            }

            // 保持攻击状态一段时间
            yield return new WaitForSeconds(attackTime);

            // 5. 收回阶段
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * (spikeRiseSpeed * 0.5f);
                spikeRow.transform.localPosition = Vector3.Lerp(targetSpikePos, spikeInitialLocalPos, t);
                yield return null;
            }
            
            spikeRow.SetActive(false);
            warningArea.SetActive(false); // 攻击彻底结束后再关闭预警
        }
    }
}
