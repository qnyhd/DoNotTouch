using UnityEngine;

/// <summary>
/// 冲刺专用体力槽，与格挡用的 CombatStamina 分开。
/// </summary>
public class DashStamina : CombatStamina
{
    private void Reset()
    {
        maxStamina = 4f;
        currentStamina = 4f;
        recoverDelayAfterConsume = 0f;
        recoverPerSecond = 0f;
        useIntervalRecover = true;
        recoverInterval = 2f;
        recoverAmountPerTick = 2f;
    }
}
