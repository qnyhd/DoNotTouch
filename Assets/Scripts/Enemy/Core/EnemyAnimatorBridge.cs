using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimatorBridge : MonoBehaviour
{
    private Animator animator;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int backstepHash = Animator.StringToHash("Backstep");
    private readonly int hitHash = Animator.StringToHash("Hit");
    private readonly int dieHash = Animator.StringToHash("Die");

    private readonly int blockingHash = Animator.StringToHash("Blocking");
    private readonly int blockCounterHash = Animator.StringToHash("BlockCounter");
    private readonly int guardBreakHash = Animator.StringToHash("GuardBreak");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat(speedHash, speed, 0.1f, Time.deltaTime);
    }

    public void SetBlocking(bool isBlocking)
    {
        animator.SetBool(blockingHash, isBlocking);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(attackHash);
    }

    public void TriggerBackstep()
    {
        animator.SetTrigger(backstepHash);
    }

    public void TriggerHit()
    {
        animator.SetTrigger(hitHash);
    }

    public void TriggerDie()
    {
        animator.SetTrigger(dieHash);
    }

    public void TriggerBlockCounter()
    {
        animator.SetTrigger(blockCounterHash);
    }

    public void TriggerGuardBreak()
    {
        animator.SetTrigger(guardBreakHash);
    }
}