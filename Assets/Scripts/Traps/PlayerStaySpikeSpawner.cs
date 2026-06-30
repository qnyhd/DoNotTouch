using UnityEngine;

public class PlayerStaySpikeSpawner : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Spike")]
    public GroundSpikeTrap spikePrefab;

    [Header("Trigger Ground")]
    public LayerMask spikeGroundLayers;
    public float stayOnGroundTime = 1.5f;
    public bool resetTimerWhenJump = true;

    [Header("Cooldown")]
    public float cooldownAfterTrap = 0.5f;

    [Header("Ground Ray")]
    public float groundRayHeight = 4f;
    public float groundRayDistance = 10f;

    private float stayTimer;
    private float cooldownTimer;
    private GroundSpikeTrap currentTrap;

    private void Update()
    {
        FindPlayerIfNeeded();

        if (player == null || spikePrefab == null)
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 当前已经有地刺在预警 / 出刺，就先不生成新的
        if (currentTrap != null)
        {
            stayTimer = 0f;
            return;
        }

        if (cooldownTimer > 0f)
            return;

        bool playerGrounded = IsPlayerGrounded();

        // 玩家跳起来时，计时清零
        if (resetTimerWhenJump && !playerGrounded)
        {
            stayTimer = 0f;
            return;
        }

        // 检测玩家脚下是不是指定地形
        if (!TryGetSpikeGroundPoint(out Vector3 groundPoint))
        {
            stayTimer = 0f;
            return;
        }

        // 只要玩家一直在指定地形上，就累加时间
        stayTimer += Time.deltaTime;

        if (stayTimer >= stayOnGroundTime)
        {
            SpawnSpike(groundPoint);
        }
    }

    private void SpawnSpike(Vector3 groundPoint)
    {
        currentTrap = Instantiate(
            spikePrefab,
            groundPoint,
            Quaternion.identity
        );

        currentTrap.StartTrap(groundPoint);

        stayTimer = 0f;
        cooldownTimer = cooldownAfterTrap;
    }

    private bool TryGetSpikeGroundPoint(out Vector3 groundPoint)
    {
        groundPoint = player.position;

        Vector3 rayOrigin = player.position + Vector3.up * groundRayHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance,
            spikeGroundLayers,
            QueryTriggerInteraction.Ignore))
        {
            groundPoint = hit.point;
            return true;
        }

        return false;
    }

    private bool IsPlayerGrounded()
    {
        PlayerMotor playerMotor = player.GetComponent<PlayerMotor>();

        if (playerMotor != null)
            return playerMotor.IsGrounded;

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
            return controller.isGrounded;

        return true;
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
}