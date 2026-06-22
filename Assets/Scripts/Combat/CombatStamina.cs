using UnityEngine;

public class CombatStamina : MonoBehaviour
{
    [Header("Stamina")]
    public float maxStamina = 10f;
    public float currentStamina = 10f;

    [Header("Recover")]
    public float recoverPerSecond = 1.5f;
    public float recoverDelayAfterConsume = 0.8f;

    private float recoverDelayTimer;

    private float recoverMultiplier = 1f;

    public bool IsEmpty => currentStamina <= 0.01f;

    public float Normalized
    {
        get
        {
            if (maxStamina <= 0f)
                return 0f;

            return currentStamina / maxStamina;
        }
    }

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (recoverDelayTimer > 0f)
        {
            recoverDelayTimer -= Time.deltaTime;
            return;
        }

        Recover(Time.deltaTime);
    }

    public bool HasEnough(float amount)
    {
        return currentStamina >= amount;
    }

    public bool Consume(float amount)
    {
        if (amount <= 0f)
            return true;

        bool hadEnough = currentStamina >= amount;

        currentStamina -= amount;

        if (currentStamina < 0f)
        {
            currentStamina = 0f;
        }

        recoverDelayTimer = recoverDelayAfterConsume;

        return hadEnough;
    }

    public void SetRecoverMultiplier(float multiplier)
    {
        recoverMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetRecoverMultiplier()
    {
        recoverMultiplier = 1f;
    }

    private void Recover(float deltaTime)
    {
        if (currentStamina >= maxStamina)
            return;

        currentStamina += recoverPerSecond * recoverMultiplier * deltaTime;

        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
    }
}