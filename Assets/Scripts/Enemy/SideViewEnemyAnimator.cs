using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Health))]
public sealed class SideViewEnemyAnimator : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int FacingRight = Animator.StringToHash("FacingRight");
    private static readonly int Attacking = Animator.StringToHash("Attacking");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Dead = Animator.StringToHash("Dead");

    private Animator animator;
    private Rigidbody2D body;
    private Health health;
    private EnemyContactDamage contactAttack;
    private Transform target;
    private float previousHealth;
    private bool deathApplied;
    private RigidbodyConstraints2D aliveConstraints;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        aliveConstraints=body.constraints;
        health = GetComponent<Health>();
        contactAttack = GetComponent<EnemyContactDamage>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
        previousHealth = health.currentHealth;
        health.Changed += OnHealthChanged;
        health.Died += OnDied;
    }

    private void OnDestroy()
    {
        if (health == null) return;
        health.Changed -= OnHealthChanged;
        health.Died -= OnDied;
    }

    private void Update()
    {
        if(health.IsDead) { if(!deathApplied) OnDied(health); return; }
        bool facingRight = target == null || target.position.x >= transform.position.x;
        animator.SetFloat(Speed, Mathf.Abs(body.linearVelocity.x));
        animator.SetBool(FacingRight, facingRight);
        animator.SetBool(Attacking, contactAttack != null && contactAttack.IsWindingUp);
        animator.SetBool(Dead, health.IsDead);
    }

    private void OnHealthChanged(Health value)
    {
        if(deathApplied && !value.IsDead) {
            deathApplied=false; body.constraints=aliveConstraints;
            var follow=GetComponent<EnemyFollow>(); if(follow!=null)follow.enabled=true;
            if(contactAttack!=null)contactAttack.enabled=true;
            foreach(var collider in GetComponents<Collider2D>())collider.enabled=true;
            animator.Rebind(); animator.Update(0);
        }
        if (!value.IsDead && value.currentHealth < previousHealth) animator.SetTrigger(Hit);
        previousHealth = value.currentHealth;
    }

    private void OnDied(Health _)
    {
        if(deathApplied)return;
        deathApplied=true;
        animator.ResetTrigger(Hit);
        animator.SetBool(Attacking,false);
        animator.SetFloat(Speed,0);
        animator.SetBool(Dead, true);
        animator.Play(animator.GetBool(FacingRight)?"DeathRight":"DeathLeft",0,0);
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        EnemyFollow follow = GetComponent<EnemyFollow>();
        if (follow != null) follow.enabled = false;
        if (contactAttack != null) contactAttack.enabled = false;
        foreach(var hitbox in GetComponents<Collider2D>())hitbox.enabled=false;
    }
}
