using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(SideViewPlayerController))]
public sealed class SideViewPlayerAnimator : MonoBehaviour
{
    private static readonly int HorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
    private static readonly int FacingRight = Animator.StringToHash("FacingRight");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Kick = Animator.StringToHash("Kick");
    private static readonly int SwordAttack = Animator.StringToHash("SwordAttack");
    private static readonly int SwordDownAttack = Animator.StringToHash("SwordDownAttack");
    private static readonly int Dead = Animator.StringToHash("Dead");

    private Animator animator;
    private Rigidbody2D body;
    private SideViewPlayerController controller;
    private Health health;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        controller = GetComponent<SideViewPlayerController>();
        health = GetComponent<Health>();
        if (health != null)
        {
            health.Died += OnDied;
        }
    }

    private void Update()
    {
        if(health!=null && health.IsDead)return;
        animator.SetFloat(HorizontalSpeed, Mathf.Abs(body.linearVelocity.x));
        animator.SetBool(FacingRight, controller.FacingRight);
        animator.SetBool(IsGrounded, controller.IsGrounded);
    }

    public void PlayAttack() => animator.SetTrigger(Attack);
    public void PlayKick() => animator.SetTrigger(Kick);
    public void PlaySwordAttack() => animator.SetTrigger(SwordAttack);
    public void PlaySwordDownAttack() => animator.SetTrigger(SwordDownAttack);
    public void SetDead(bool value) => animator.SetBool(Dead, value);

    private void OnDied(Health _)
    {
        animator.ResetTrigger(Attack);
        animator.ResetTrigger(Kick);
        animator.ResetTrigger(SwordAttack);
        animator.ResetTrigger(SwordDownAttack);
        animator.SetBool(Dead, true);
        animator.Play(controller.FacingRight?"DeathRight":"DeathLeft",0,0);
        animator.updateMode=AnimatorUpdateMode.UnscaledTime;
        body.linearVelocity=Vector2.zero;
        body.constraints=RigidbodyConstraints2D.FreezeAll;
        controller.enabled=false;
    }

    private void OnDestroy()
    {
        if (health == null) return;
        health.Died -= OnDied;
    }
}
