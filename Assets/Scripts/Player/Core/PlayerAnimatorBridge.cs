using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorBridge : MonoBehaviour
{
    private Animator animator;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveYHash = Animator.StringToHash("MoveY");

    private readonly int blockMoveXHash = Animator.StringToHash("BlockMoveX");
    private readonly int blockMoveYHash = Animator.StringToHash("BlockMoveY");
    private readonly int blockMoveSpeedHash = Animator.StringToHash("BlockMoveSpeed");

    private readonly int groundedHash = Animator.StringToHash("Grounded");
    private readonly int jumpHash = Animator.StringToHash("Jump");
    private readonly int dashHash = Animator.StringToHash("Dash");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int hitHash = Animator.StringToHash("Hit");
    private readonly int dieHash = Animator.StringToHash("Die");

    private readonly int blockingHash = Animator.StringToHash("Blocking");
    private readonly int blockCounterHash = Animator.StringToHash("BlockCounter");
    private readonly int guardBreakHash = Animator.StringToHash("GuardBreak");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMove(Vector2 moveInput)
    {
        float speed = Mathf.Clamp01(moveInput.magnitude);

        animator.SetFloat(speedHash, speed, 0.1f, Time.deltaTime);
        animator.SetFloat(moveXHash, moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat(moveYHash, moveInput.y, 0.1f, Time.deltaTime);
    }

    public void SetBlockMove(Vector2 moveInput)
    {
        float speed = Mathf.Clamp01(moveInput.magnitude);

        animator.SetFloat(blockMoveSpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetFloat(blockMoveXHash, moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat(blockMoveYHash, moveInput.y, 0.1f, Time.deltaTime);
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat(speedHash, speed, 0.1f, Time.deltaTime);
    }

    public void SetGrounded(bool isGrounded)
    {
        animator.SetBool(groundedHash, isGrounded);
    }

    public void SetBlocking(bool isBlocking)
    {
        animator.SetBool(blockingHash, isBlocking);
    }

    public void TriggerJump()
    {
        animator.SetTrigger(jumpHash);
    }

    public void TriggerDash()
    {
        animator.SetTrigger(dashHash);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(attackHash);
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