using UnityEngine;

[DisallowMultipleComponent]
public sealed class FireBoltProjectile : MonoBehaviour
{
    private LineRenderer line;
    private Vector2 direction;
    private float damage;
    private float remainingDistance;
    private float speed;
    private float lifetime;
    private float elapsed;

    public bool IsActive => gameObject.activeSelf;

    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = 0.14f;
        line.endWidth = 0.04f;
        line.numCapVertices = 3;
        line.sortingOrder = 30;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) line.material = new Material(shader);
    }

    public void Launch(Vector2 origin, Vector2 heading, float amount, float range,
        float velocity, float duration, Color color)
    {
        transform.position = origin;
        direction = heading.normalized;
        damage = amount;
        remainingDistance = Mathf.Max(0.1f, range);
        speed = Mathf.Max(0.1f, velocity);
        lifetime = Mathf.Max(0.05f, duration);
        elapsed = 0f;
        line.startColor = color;
        line.endColor = new Color(color.r, color.g, color.b, 0.05f);
        gameObject.SetActive(true);
        DrawTrail();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        float step = Mathf.Min(Time.deltaTime, lifetime - elapsed);
        if (step <= 0f) { gameObject.SetActive(false); return; }
        float distance = speed * step;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.1f, direction, distance);
        if (hit.collider != null)
        {
            Health target = hit.collider.GetComponentInParent<Health>();
            if (target != null && !target.IsDead && !target.CompareTag("Player") &&
                target.GetComponent<PlayerMovement>() == null && target.GetComponent<EnemyAttackTarget>() != null)
            {
                target.TakeDamage(damage);
                gameObject.SetActive(false);
                return;
            }
        }
        transform.position += (Vector3)(direction * distance);
        remainingDistance -= distance;
        elapsed += step;
        DrawTrail();
        if (remainingDistance <= 0f || elapsed >= lifetime) gameObject.SetActive(false);
    }

    private void DrawTrail()
    {
        Vector3 end = transform.position;
        line.SetPosition(0, end);
        line.SetPosition(1, end - (Vector3)(direction * 0.3f));
    }
}
