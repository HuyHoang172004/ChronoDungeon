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
    private bool sideViewMovement;
    private Collider2D bodyCollider;
    private int groundMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        attack = GetComponent<EnemyContactDamage>();
        sideViewMovement = GetComponent<SideViewEnemyAnimator>() != null;
        bodyCollider=GetComponent<Collider2D>();
        groundMask=LayerMask.GetMask("SideViewGround","SideViewOneWay");
        if (player == null)
        {
            GameObject target = GameObject.FindGameObjectWithTag("Player");
            if (target != null) player = target.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null || Time.timeScale <= 0f ||
            (health != null && health.IsDead) || (attack != null && attack.BlocksMovement))
        {
            if (sideViewMovement && rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector2 delta = (Vector2)player.position - rb.position;
        float distance = sideViewMovement ? Mathf.Abs(delta.x) : delta.magnitude;

        if (distance <= detectionRange && distance > stoppingDistance)
        {
            Vector2 direction = sideViewMovement
                ? new Vector2(Mathf.Sign(delta.x), 0f)
                : delta.normalized;

            if (sideViewMovement)
            {
                Bounds feet=bodyCollider.bounds;
                Vector2 probe=new Vector2(feet.center.x+direction.x*(feet.extents.x+.12f),feet.min.y+.25f);
                bool hasFloor=Physics2D.Raycast(probe,Vector2.down,.65f,groundMask).collider!=null;
                rb.linearVelocity = new Vector2(hasFloor ? direction.x * moveSpeed : 0, rb.linearVelocity.y);
            }
            else
                rb.MovePosition(rb.position + direction * Mathf.Min(moveSpeed * Time.fixedDeltaTime, distance - stoppingDistance));
        }
        else if (sideViewMovement)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
