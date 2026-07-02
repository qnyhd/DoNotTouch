using UnityEngine;
using System.Collections;

/// <summary>
/// 起跳/落地时在主角脚边生成尘埃特效。
/// 推荐用 Jump Spawn Point / Land Spawn Point 空物体在 Scene 里拖位置。
/// </summary>
public class PlayerJumpLandDustVFX : MonoBehaviour
{
    [Header("References")]
    public GameObject jumpDustPrefab;
    public GameObject landDustPrefab;
    public PlayerMotor motor;
    public CharacterController characterController;

    [Header("Spawn Points (推荐)")]
    [Tooltip("拖一个空物体到脚底，在 Scene 里直接移动它来调位置")]
    public Transform jumpSpawnPoint;
    public Transform landSpawnPoint;

    [Header("Jump")]
    public float jumpSpawnDelay = 0f;
    public float jumpStartOffset = 0f;
    public float jumpPlaybackSpeed = 1f;

    [Header("Land")]
    public float landSpawnDelay = 0f;
    public float landStartOffset = 0f;
    public float landPlaybackSpeed = 1f;
    [Tooltip("离地至少多久后才算一次落地。")]
    public float minAirTimeForLandDust = 0.08f;

    [Header("Fallback Offset")]
    [Tooltip("没填 Spawn Point 时才用这里的数值")]
    public Vector3 jumpSpawnOffset = Vector3.zero;
    public float jumpFootYOffset = 0.02f;
    public Vector3 landSpawnOffset = Vector3.zero;
    public float landFootYOffset = 0.02f;

    [Header("Common")]
    public float destroyAfter = 2f;

    private bool wasGrounded = true;
    private float airTime;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (motor != null)
            wasGrounded = motor.IsGrounded;
    }

    private void Update()
    {
        if (motor == null)
            return;

        bool grounded = motor.IsGrounded;

        if (!grounded)
            airTime += Time.deltaTime;

        if (grounded && !wasGrounded && airTime >= minAirTimeForLandDust)
            PlayLandDust();

        if (grounded)
            airTime = 0f;

        wasGrounded = grounded;
    }

    public void PlayJumpDust()
    {
        SpawnDust(
            jumpDustPrefab,
            jumpSpawnPoint,
            jumpSpawnOffset,
            jumpFootYOffset,
            jumpSpawnDelay,
            jumpStartOffset,
            jumpPlaybackSpeed);
    }

    public void PlayLandDust()
    {
        SpawnDust(
            landDustPrefab,
            landSpawnPoint,
            landSpawnOffset,
            landFootYOffset,
            landSpawnDelay,
            landStartOffset,
            landPlaybackSpeed);
    }

    public Vector3 GetJumpSpawnPosition()
    {
        return GetSpawnPosition(jumpSpawnPoint, jumpSpawnOffset, jumpFootYOffset);
    }

    public Vector3 GetLandSpawnPosition()
    {
        return GetSpawnPosition(landSpawnPoint, landSpawnOffset, landFootYOffset);
    }

    [ContextMenu("Create Spawn Point Anchors")]
    private void CreateSpawnPointAnchors()
    {
        jumpSpawnPoint = CreateAnchor("JumpDustPoint", jumpSpawnPoint, new Vector3(0f, 0.05f, 0.1f));
        landSpawnPoint = CreateAnchor("LandDustPoint", landSpawnPoint, new Vector3(0f, 0.05f, 0.1f));
    }

    private Transform CreateAnchor(string name, Transform existing, Vector3 localPos)
    {
        if (existing != null)
            return existing;

        GameObject anchor = new GameObject(name);
        anchor.transform.SetParent(transform, false);
        anchor.transform.localPosition = localPos;
        return anchor.transform;
    }

    private void SpawnDust(
        GameObject prefab,
        Transform spawnPoint,
        Vector3 fallbackOffset,
        float footYOffset,
        float delay,
        float startOffset,
        float playbackSpeed)
    {
        if (prefab == null)
            return;

        if (delay > 0f)
        {
            StartCoroutine(SpawnAfterDelay(
                prefab,
                spawnPoint,
                fallbackOffset,
                footYOffset,
                delay,
                startOffset,
                playbackSpeed));
            return;
        }

        SpawnNow(prefab, spawnPoint, fallbackOffset, footYOffset, startOffset, playbackSpeed);
    }

    private IEnumerator SpawnAfterDelay(
        GameObject prefab,
        Transform spawnPoint,
        Vector3 fallbackOffset,
        float footYOffset,
        float delay,
        float startOffset,
        float playbackSpeed)
    {
        yield return new WaitForSeconds(delay);
        SpawnNow(prefab, spawnPoint, fallbackOffset, footYOffset, startOffset, playbackSpeed);
    }

    private void SpawnNow(
        GameObject prefab,
        Transform spawnPoint,
        Vector3 fallbackOffset,
        float footYOffset,
        float startOffset,
        float playbackSpeed)
    {
        Vector3 targetPosition = GetSpawnPosition(spawnPoint, fallbackOffset, footYOffset);
        Quaternion spawnRotation = GetSpawnRotation(spawnPoint);
        GameObject fx = Instantiate(prefab, targetPosition, spawnRotation);
        AlignParticleEmittersToWorldPoint(fx, targetPosition);
        ConfigureParticles(fx, playbackSpeed, startOffset);
        Destroy(fx, destroyAfter);
    }

    private Quaternion GetSpawnRotation(Transform spawnPoint)
    {
        if (spawnPoint != null)
            return spawnPoint.rotation;

        return transform.rotation;
    }

    /// <summary>
    /// 预制体里粒子常在子物体上，根节点位置不等于可见烟雾位置。
    /// 生成后把粒子发射点中心对齐到目标点（与 Gizmo 一致）。
    /// </summary>
    private static void AlignParticleEmittersToWorldPoint(GameObject fx, Vector3 targetWorldPoint)
    {
        ParticleSystem[] particles = fx.GetComponentsInChildren<ParticleSystem>(true);
        if (particles.Length == 0)
            return;

        Vector3 center = Vector3.zero;
        foreach (ParticleSystem particle in particles)
            center += particle.transform.position;

        center /= particles.Length;
        fx.transform.position += targetWorldPoint - center;
    }

    private static Vector3 EstimateParticleCenterOffset(GameObject prefab, Quaternion rotation)
    {
        if (prefab == null)
            return Vector3.zero;

        ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
        if (particles.Length == 0)
            return Vector3.zero;

        Transform root = prefab.transform;
        Vector3 sum = Vector3.zero;
        foreach (ParticleSystem particle in particles)
            sum += rotation * GetLocalOffsetInRootSpace(root, particle.transform);

        return sum / particles.Length;
    }

    private static Vector3 GetLocalOffsetInRootSpace(Transform root, Transform node)
    {
        Vector3 offset = Vector3.zero;
        Transform current = node;

        while (current != null && current != root)
        {
            offset = current.localPosition + current.localRotation * offset;
            current = current.parent;
        }

        return offset;
    }

    private Vector3 GetSpawnPosition(Transform spawnPoint, Vector3 fallbackOffset, float footYOffset)
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        Vector3 foot = GetFootPosition() + Vector3.up * footYOffset;
        Vector3 worldOffset =
            transform.right * fallbackOffset.x
            + Vector3.up * fallbackOffset.y
            + transform.forward * fallbackOffset.z;
        return foot + worldOffset;
    }

    private Vector3 GetFootPosition()
    {
        if (characterController != null)
        {
            return transform.position
                + characterController.center
                + Vector3.down * (characterController.height * 0.5f);
        }

        return transform.position;
    }

    private void ConfigureParticles(GameObject root, float speed, float offset)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particle in particles)
        {
            ParticleSystem.MainModule main = particle.main;
            main.simulationSpeed = speed;

            if (offset > 0f)
                particle.Simulate(offset, false, true, true);

            particle.Play(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 foot = GetFootPosition();

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(foot, 0.08f);

        DrawSpawnGizmo(
            GetJumpSpawnPosition(),
            GetSpawnRotation(jumpSpawnPoint),
            jumpDustPrefab,
            Color.green,
            foot);

        DrawSpawnGizmo(
            GetLandSpawnPosition(),
            GetSpawnRotation(landSpawnPoint),
            landDustPrefab,
            Color.cyan,
            foot);
    }

    private static void DrawSpawnGizmo(
        Vector3 targetPosition,
        Quaternion rotation,
        GameObject prefab,
        Color color,
        Vector3 foot)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(targetPosition, 0.12f);
        Gizmos.DrawLine(foot, targetPosition);

        if (prefab == null)
            return;

        Vector3 visualCenter = targetPosition + EstimateParticleCenterOffset(prefab, rotation);
        if ((visualCenter - targetPosition).sqrMagnitude <= 0.0001f)
            return;

        Gizmos.color = new Color(color.r, color.g, color.b, 0.45f);
        Gizmos.DrawWireSphere(visualCenter, 0.08f);
        Gizmos.DrawLine(targetPosition, visualCenter);
    }
}
