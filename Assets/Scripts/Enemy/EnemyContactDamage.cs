using UnityEngine;

[DisallowMultipleComponent]
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damage = 20f;
    [SerializeField, Min(0.01f)] private float damageCooldown = 1f;
    [SerializeField, Min(0f)] private float contactRange = 0.65f;
    private float nextDamageTime;
    private Health ownHealth;

    private void Awake() => ownHealth = GetComponentInParent<Health>();
    private void OnEnable() => nextDamageTime = 0f;

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f || Time.time < nextDamageTime ||
            (ownHealth != null && ownHealth.IsDead)) return;

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, contactRange))
        {
            Health target = hit.GetComponentInParent<Health>();
            if (target == null || target == ownHealth || target.IsDead || !target.CompareTag("Player"))
                continue;

            nextDamageTime = Time.time + Mathf.Max(0.01f, damageCooldown);
            target.TakeDamage(damage);
            break;
        }
    }
}
