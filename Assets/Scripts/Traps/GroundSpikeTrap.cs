using System.Collections.Generic;
using UnityEngine;

public class GroundSpikeTrap : MonoBehaviour
{
    private enum SpikeState
    {
        Hidden,
        Warning,
        Rising,
        Active,
        Finished
    }

    [Header("Warning Color")]
    public Color warningStartColor = new Color(1f, 0.35f, 0.35f, 0.25f);
    public Color warningEndColor = new Color(1f, 0f, 0f, 1f);

    [Header("Warning Scale")]
    public bool scaleWarningCircle = false;
    public float warningStartScale = 0.8f;
    public float warningEndScale = 1.0f;

    private Renderer warningRenderer;
    private Material warningMaterial;
    private Vector3 warningOriginalScale;

    [Header("References")]
    public Transform warningCircle;
    public Transform spikeVisual;

    [Header("Timing")]
    public float warningTime = 0.8f;
    public float riseTime = 0.12f;
    public float activeTime = 0.35f;

    [Header("Spike Position")]
    public float spikeHiddenLocalY = -1.2f;
    public float spikeShownLocalY = 0f;
    public float warningHeightOffset = 0.03f;

    [Header("Damage")]
    public int damage = 2;
    public float damageRadius = 0.8f;
    public LayerMask damageLayers = ~0;
    public bool requirePlayerGrounded = true;
    public bool canBeBlocked = false;

    [Header("Life")]
    public bool destroyAfterFinished = true;
    public float destroyDelay = 0.1f;

    private SpikeState state = SpikeState.Hidden;
    private float timer;
    private bool hasDamaged;

    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    public float TotalLifeTime
    {
        get
        {
            return warningTime + riseTime + activeTime + destroyDelay;
        }
    }

    private void Awake()
    {
        InitWarningMaterial();
        HideAll();
    }

    public void StartTrap(Vector3 groundPoint)
    {
        transform.position = groundPoint;

        timer = 0f;
        hasDamaged = false;
        damagedTargets.Clear();

        state = SpikeState.Warning;

        if (warningCircle != null)
        {
            warningCircle.gameObject.SetActive(true);
            warningCircle.localPosition = Vector3.up * warningHeightOffset;
            warningCircle.localScale = warningOriginalScale;
        }

        UpdateWarningVisual(0f);

        if (spikeVisual != null)
        {
            spikeVisual.gameObject.SetActive(true);
            spikeVisual.localPosition = new Vector3(
                spikeVisual.localPosition.x,
                spikeHiddenLocalY,
                spikeVisual.localPosition.z
            );
        }
    }

    private void Update()
    {
        switch (state)
        {
            case SpikeState.Warning:
                TickWarning();
                break;

            case SpikeState.Rising:
                TickRising();
                break;

            case SpikeState.Active:
                TickActive();
                break;
        }
    }

    private void TickWarning()
    {
        timer += Time.deltaTime;

        float t = warningTime <= 0f ? 1f : timer / warningTime;
        t = Mathf.Clamp01(t);

        UpdateWarningVisual(t);

        if (timer >= warningTime)
        {
            timer = 0f;
            state = SpikeState.Rising;
        }
    }

    private void TickRising()
    {
        timer += Time.deltaTime;

        float t = riseTime <= 0f ? 1f : timer / riseTime;
        t = Mathf.Clamp01(t);

        if (spikeVisual != null)
        {
            Vector3 localPos = spikeVisual.localPosition;
            localPos.y = Mathf.Lerp(spikeHiddenLocalY, spikeShownLocalY, t);
            spikeVisual.localPosition = localPos;
        }

        if (timer >= riseTime)
        {
            timer = 0f;
            state = SpikeState.Active;

            DealDamageOnce();
        }
    }

    private void TickActive()
    {
        timer += Time.deltaTime;

        if (timer >= activeTime)
        {
            FinishTrap();
        }
    }

    private void DealDamageOnce()
    {
        if (hasDamaged)
            return;

        hasDamaged = true;

        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            damageRadius,
            damageLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            if (requirePlayerGrounded && !IsTargetGrounded(col))
                continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damageable.IsDead)
                continue;

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);

            DamageInfo info = new DamageInfo(
                damage,
                0f,
                gameObject,
                col.ClosestPoint(transform.position),
                Vector3.up,
                canBeBlocked
            );

            damageable.TakeDamage(info);
        }
    }

    private bool IsTargetGrounded(Collider col)
    {
        PlayerMotor playerMotor = col.GetComponentInParent<PlayerMotor>();

        if (playerMotor != null)
            return playerMotor.IsGrounded;

        CharacterController controller = col.GetComponentInParent<CharacterController>();

        if (controller != null)
            return controller.isGrounded;

        return true;
    }

    private void FinishTrap()
    {
        state = SpikeState.Finished;

        if (warningCircle != null)
        {
            warningCircle.gameObject.SetActive(false);
        }

        if (destroyAfterFinished)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void HideAll()
    {
        state = SpikeState.Hidden;

        if (warningCircle != null)
        {
            warningCircle.gameObject.SetActive(false);
        }

        if (spikeVisual != null)
        {
            spikeVisual.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }

    private void InitWarningMaterial()
    {
        if (warningCircle == null)
            return;

        warningRenderer = warningCircle.GetComponent<Renderer>();

        if (warningRenderer != null)
        {
            // 重点：用 material 会生成材质实例，不会影响 Project 里的原材质
            warningMaterial = warningRenderer.material;
        }

        warningOriginalScale = warningCircle.localScale;
    }

    private void UpdateWarningVisual(float t)
    {
        Color currentColor = Color.Lerp(
            warningStartColor,
            warningEndColor,
            t
        );

        if (warningMaterial != null)
        {
            if (warningMaterial.HasProperty("_BaseColor"))
            {
                warningMaterial.SetColor("_BaseColor", currentColor);
            }
            else if (warningMaterial.HasProperty("_Color"))
            {
                warningMaterial.SetColor("_Color", currentColor);
            }
        }

        if (scaleWarningCircle && warningCircle != null)
        {
            float scale = Mathf.Lerp(
                warningStartScale,
                warningEndScale,
                t
            );

            warningCircle.localScale = warningOriginalScale * scale;
        }
    }
}