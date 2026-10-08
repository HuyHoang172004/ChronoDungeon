using UnityEngine;

[DisallowMultipleComponent]
public sealed class VoidArrowRuntime : MonoBehaviour
{
    private const int PoolSize = 10;
    private readonly VoidArrowProjectile[] pool = new VoidArrowProjectile[PoolSize];
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        for (int i = 0; i < pool.Length; i++)
        {
            GameObject arrow = new GameObject("Void Arrow", typeof(VoidArrowProjectile));
            arrow.transform.SetParent(transform, false);
            arrow.SetActive(false);
            pool[i] = arrow.GetComponent<VoidArrowProjectile>();
        }
    }

    public bool Fire(float damage, float range, float speed, float lifetime, Color color,
        float spreadDegrees, int targetsToHit)
    {
        Vector2 direction = movement != null ? movement.FacingDirection : Vector2.right;
        if (direction.sqrMagnitude < .001f) direction = Vector2.right;
        direction = Quaternion.Euler(0f, 0f, spreadDegrees) * direction.normalized;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].IsActive) continue;
            pool[i].Launch((Vector2)transform.position + direction * .45f, direction,
                damage, range, speed, lifetime, color, targetsToHit);
            return true;
        }
        return false;
    }
}
