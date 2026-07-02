using UnityEngine;

/// <summary>
/// 关卡 debuff 眩晕的统一表现：摇头动画 + 头顶星星。
/// 各关卡的 PenaltyAction 在惩罚开始时调用 BeginStun，结束时调用 EndStun。
/// </summary>
public class PlayerStunPresentation : MonoBehaviour
{
    public CharacterStunVFXController stunVfx;

    private PlayerAnimatorBridge anim;

    private void Awake()
    {
        if (stunVfx == null)
            stunVfx = GetComponent<CharacterStunVFXController>();

        anim = GetComponent<PlayerAnimatorBridge>();
    }

    /// <summary>只显示星星，不播摇头（例如落地前提前出 VFX）。</summary>
    public void ShowVfxOnly()
    {
        stunVfx?.ShowStun();
    }

    /// <summary>落地/惩罚开始：摇头 + 星星。</summary>
    public void BeginStun()
    {
        anim?.SetStunned(true);
        stunVfx?.ShowStun();
    }

    /// <summary>惩罚结束。</summary>
    public void EndStun()
    {
        anim?.SetStunned(false);
        stunVfx?.HideStun();
    }
}
