using UnityEngine;
using System.Collections;

/// <summary>
/// 由攻击动画事件调用，在主角脚边生成砸地特效（跟随脚底高度，不射线打到下方平台）。
/// </summary>
public class HammerGroundHitVFX : MonoBehaviour
{
    [Header("References")]
    public GameObject groundHitPrefab;
    public PlayerMotor motor;
    public CharacterController characterController;

    [Header("Spawn")]
    public float forwardOffset = 1.2f;
    public float footYOffset = 0.02f;
    public float destroyAfter = 4f;

    [Header("Timing")]
    public float spawnDelay = 0f;
    public float playbackSpeed = 1f;
    public float startOffset = 0f;

    [Header("Condition")]
    public bool requireGrounded = false;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    public void PlayGroundHitVFX()
    {
        if (groundHitPrefab == null)
            return;

        if (requireGrounded && motor != null && !motor.IsGrounded)
            return;

        if (spawnDelay > 0f)
        {
            StartCoroutine(SpawnAfterDelay(spawnDelay));
            return;
        }

        SpawnAtFeet();
    }

    private IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (requireGrounded && motor != null && !motor.IsGrounded)
            yield break;

        SpawnAtFeet();
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

    private void SpawnAtFeet()
    {
        Vector3 foot = GetFootPosition();
        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.0001f)
            forward.Normalize();
        else
            forward = Vector3.forward;

        Vector3 spawnPoint = foot + forward * forwardOffset;
        spawnPoint.y = foot.y + footYOffset;

        GameObject fx = Instantiate(groundHitPrefab, spawnPoint, Quaternion.identity);
        ConfigureParticles(fx, playbackSpeed, startOffset);
        Destroy(fx, destroyAfter);
    }

    private void ConfigureParticles(GameObject root, float speed, float offset)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particle in particles)
        {
            ParticleSystem.MainModule main = particle.main;
            main.simulationSpeed = speed;

            if (offset > 0f)
            {
                particle.Simulate(offset, false, true, true);
                particle.Play();
            }
        }
    }
}
