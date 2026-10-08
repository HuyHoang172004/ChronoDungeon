using System.Collections.Generic;
using UnityEngine;

/// <summary>Small pooled projectile used by every Void Bow action.</summary>
[DisallowMultipleComponent]
public sealed class VoidArrowProjectile : MonoBehaviour
{
    private readonly HashSet<Health> hitTargets = new HashSet<Health>();
    private readonly Collider2D[] overlapHits = new Collider2D[12];
    private LineRenderer trail;
    private Vector2 direction;
    private float damage;
    private float remainingRange;
    private float speed;
    private float expiresAt;
    private int remainingTargets;

    public bool IsActive => gameObject.activeSelf;

    private void Awake()
    {
        trail = gameObject.AddComponent<LineRenderer>();
        trail.useWorldSpace = true;
        trail.positionCount = 2;
        trail.startWidth = 0.11f;
        trail.endWidth = 0.025f;
        trail.numCapVertices = 3;
        trail.sortingOrder = 30;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) trail.material = new Material(shader);
    }

    public void Launch(Vector2 origin, Vector2 heading, float amount, float range,
        float velocity, float lifetime, Color color, int targetsToHit)
    {
        transform.position = origin;
        direction = heading.sqrMagnitude > .001f ? heading.normalized : Vector2.right;
        damage = amount;
        remainingRange = Mathf.Max(.1f, range);
        speed = Mathf.Max(.1f, velocity);
        expiresAt = Time.time + Mathf.Max(.05f, lifetime);
        remainingTargets = Mathf.Max(1, targetsToHit);
        hitTargets.Clear();
        trail.startColor = color;
        trail.endColor = new Color(color.r, color.g, color.b, .03f);
        gameObject.SetActive(true);
        DrawTrail();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (Time.time >= expiresAt || remainingRange <= 0f) { gameObject.SetActive(false); return; }
        float distance = Mathf.Min(speed * Time.deltaTime, remainingRange);
        transform.position += (Vector3)(direction * distance);
        remainingRange -= distance;
        HitTargetsAtPosition();
        DrawTrail();
        if (remainingTargets <= 0 || remainingRange <= 0f) gameObject.SetActive(false);
    }

    private void HitTargetsAtPosition()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, .15f, overlapHits);
        for (int i = 0; i < count && remainingTargets > 0; i++)
        {
            Collider2D hit = overlapHits[i];
            Health target = hit == null ? null : hit.GetComponentInParent<Health>();
            if (target == null || target.IsDead || target.GetComponent<PlayerMovement>() != null ||
                target.GetComponent<EnemyAttackTarget>() == null || !hitTargets.Add(target)) continue;
            target.TakeDamage(damage);
            remainingTargets--;
        }
    }

    private void DrawTrail()
    {
        Vector3 tip = transform.position;
        trail.SetPosition(0, tip);
        trail.SetPosition(1, tip - (Vector3)(direction * .38f));
    }

    private void OnDestroy()
    {
        if (trail != null && trail.material != null) Destroy(trail.material);
    }
}
