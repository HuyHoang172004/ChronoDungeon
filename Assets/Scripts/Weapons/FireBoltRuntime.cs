using UnityEngine;

[DisallowMultipleComponent]
public sealed class FireBoltRuntime : MonoBehaviour
{
    private const int PoolSize = 4;
    private FireBoltProjectile[] pool;
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        pool = new FireBoltProjectile[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject bolt = new GameObject("Fire Bolt", typeof(FireBoltProjectile));
            bolt.transform.SetParent(transform, false);
            bolt.SetActive(false);
            pool[i] = bolt.GetComponent<FireBoltProjectile>();
        }
    }

    public bool Cast(float damage, float range, float speed, float lifetime, Color color)
    {
        Vector2 direction = movement != null ? movement.FacingDirection : Vector2.right;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        direction.Normalize();
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].IsActive) continue;
            pool[i].Launch((Vector2)transform.position + direction * 0.4f, direction,
                damage, range, speed, lifetime, color);
            return true;
        }
        return false;
    }
}
