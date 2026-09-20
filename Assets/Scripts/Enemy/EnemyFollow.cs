using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float detectionRange = 6f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(
            rb.position,
            player.position
        );

        if (distance <= detectionRange)
        {
            Vector2 direction =
                ((Vector2)player.position - rb.position).normalized;

            Vector2 newPosition =
                rb.position +
                direction * moveSpeed * Time.fixedDeltaTime;

            rb.MovePosition(newPosition);
        }
    }
}