using UnityEngine;

public class PlayerAnimationEventReceiver : MonoBehaviour
{
    public HammerGroundHitVFX hammerGroundHitVfx;

    private void Awake()
    {
        if (hammerGroundHitVfx == null)
            hammerGroundHitVfx = GetComponent<HammerGroundHitVFX>();
    }

    public void OnLand()
    {
        // 给 JumpLand 动画事件预留
    }

    // 在 HumanM@Attack2H02 砸地帧调用
    public void OnHammerGroundHit()
    {
        if (hammerGroundHitVfx != null)
            hammerGroundHitVfx.PlayGroundHitVFX();
    }
}
