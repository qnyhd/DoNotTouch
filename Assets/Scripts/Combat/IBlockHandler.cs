using UnityEngine;

public enum BlockResult
{
    NotBlocked,
    Blocked,
    GuardBroken
}

public interface IBlockHandler
{
    bool IsBlocking { get; }
    bool IsGuardBroken { get; }

    bool CanBlockAttack(Vector3 attackerPosition);

    BlockResult TryBlock(DamageInfo info);
}
