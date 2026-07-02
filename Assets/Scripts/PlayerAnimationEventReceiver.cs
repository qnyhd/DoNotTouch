using UnityEngine;

public class PlayerAnimationEventReceiver : MonoBehaviour
{
    public HammerGroundHitVFX hammerGroundHitVfx;
    public HammerGroundSlam hammerGroundSlam;

    private void Awake()
    {
        if (hammerGroundHitVfx == null)
            hammerGroundHitVfx = GetComponent<HammerGroundHitVFX>();
        if (hammerGroundSlam == null)
            hammerGroundSlam = GetComponent<HammerGroundSlam>();
    }

    public void OnLand()
    {
        // 给 JumpLand 动画事件预留
    }

    // 在 HumanM@Attack2H02 砸地帧调用
    public void OnHammerGroundHit()
    {
        if (hammerGroundSlam != null)
        {
            hammerGroundSlam.PerformSlam();
            return;
        }

        if (hammerGroundHitVfx != null)
            hammerGroundHitVfx.PlayGroundHitVFX();
    }
}
