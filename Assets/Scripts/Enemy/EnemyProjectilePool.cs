using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyProjectilePool : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField, Range(1, 8)] private int capacity = 3;
    private EnemyProjectile[] slots;
    private GameObject root;
    private bool runtimeBolts;
    private static Sprite boltSprite;
    public void ConfigureRuntimeBolts() => runtimeBolts = true;
    public int Capacity => slots == null ? capacity : slots.Length;
    public int ActiveCount
    {
        get { int count = 0; if (slots != null) foreach (var p in slots) if (p != null && p.IsFlying) count++; return count; }
    }

    private void Awake()
    {
        if (projectilePrefab == null && !runtimeBolts) { Debug.LogError("Projectile pool requires a prefab.", this); enabled = false; return; }
        root = new GameObject(name + " Arrows");
        // Sibling under room Content: arrows do not inherit the shooter's movement/scale.
        root.transform.SetParent(transform.parent, false);
        slots = new EnemyProjectile[Mathf.Clamp(capacity, 1, 8)];
        for (int i = 0; i < slots.Length; i++)
        {
            if (projectilePrefab != null) slots[i] = Instantiate(projectilePrefab, root.transform);
            else
            {
                GameObject bolt = new GameObject("Enemy Bolt");
                bolt.SetActive(false);
                bolt.transform.SetParent(root.transform, false);
                SpriteRenderer visual = bolt.AddComponent<SpriteRenderer>();
                if (boltSprite == null)
                    boltSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 2, 2), Vector2.one * .5f, 2f);
                visual.sprite = boltSprite;
                visual.color = new Color(1f, .65f, .15f);
                visual.sortingOrder = 60;
                bolt.transform.localScale = new Vector3(.3f, .09f, 1f);
                slots[i] = bolt.AddComponent<EnemyProjectile>();
            }
            slots[i].Cancel();
        }
    }

    public bool TryFire(Vector2 origin, Vector2 direction, Health target, float speed, float damage, float lifetime)
    {
        if (!isActiveAndEnabled || slots == null || target == null || target.IsDead) return false;
        foreach (var p in slots)
        {
            if (p == null || p.IsFlying) continue;
            p.Launch(origin, direction, target, speed, damage, lifetime);
            return true;
        }
        return false; // Hard bound; never allocate extra projectiles during combat.
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        if (slots != null) foreach (var p in slots) if (p != null) p.Cancel();
    }
    private void OnDisable() => ResetToInitialState();
    private void OnDestroy() { if (root != null) Destroy(root); }
}
