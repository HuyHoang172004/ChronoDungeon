using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float detectionRange = 6f;
    [SerializeField, Min(0f)] private float stoppingDistance;

    private Rigidbody2D rb;
    private Health health;
    private EnemyContactDamage attack;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        attack = GetComponent<EnemyContactDamage>();
    }

    void FixedUpdate()
    {
        if (player == null || rb == null || Time.timeScale <= 0f ||
            (health != null && health.IsDead) || (attack != null && attack.BlocksMovement)) return;

        float distance = Vector2.Distance(
            rb.position,
            player.position
        );

        if (distance <= detectionRange && distance > stoppingDistance)
        {
            Vector2 direction =
                ((Vector2)player.position - rb.position).normalized;

            Vector2 newPosition =
                rb.position +
                direction * Mathf.Min(moveSpeed * Time.fixedDeltaTime, distance - stoppingDistance);

            rb.MovePosition(newPosition);
        }
    }
}
