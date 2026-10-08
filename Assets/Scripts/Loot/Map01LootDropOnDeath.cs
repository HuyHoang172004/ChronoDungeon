using UnityEngine;

/// <summary>
/// Curated Map 01 reward bundle for the Ruin Husk encounter.  It is deliberately
/// small and one-shot: no per-frame work, no infinite spawning, and reusable
/// pickup behaviour is kept in WorldLootPickup.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public sealed class Map01LootDropOnDeath : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField, Min(0)] private int goldAmount = 8;
    [SerializeField, Min(0f)] private float healthAmount = 25f;
    [SerializeField, Min(0f)] private float energyAmount = 30f;
    [SerializeField, Min(0)] private int temporalBeanAmount = 1;

    [Header("Map 01 Loot Art")]
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite healthSprite;
    [SerializeField] private Sprite energySprite;
    [SerializeField] private Sprite temporalBeanSprite;
    [SerializeField] private int sortingOrder = 14;

    private Health health;
    private bool dropped;

    private void Awake() => health = GetComponent<Health>();

    private void OnEnable()
    {
        if (health != null) health.Died += DropRewards;
        dropped = false;
    }

    private void OnDisable()
    {
        if (health != null) health.Died -= DropRewards;
    }

    private void DropRewards(Health _)
    {
        if (dropped) return;
        dropped = true;

        Vector3 origin = transform.position;
        Spawn(WorldLootPickup.LootKind.Gold, goldAmount, goldSprite, origin + new Vector3(-0.42f, 0.12f, 0f), "Gold Drop");
        Spawn(WorldLootPickup.LootKind.Health, healthAmount, healthSprite, origin + new Vector3(0.42f, 0.12f, 0f), "Health Shard Drop");
        Spawn(WorldLootPickup.LootKind.Energy, energyAmount, energySprite, origin + new Vector3(-0.22f, 0.52f, 0f), "Energy Shard Drop");
        Spawn(WorldLootPickup.LootKind.TemporalBean, temporalBeanAmount, temporalBeanSprite, origin + new Vector3(0.28f, 0.52f, 0f), "Temporal Bean Drop");
    }

    private void Spawn(WorldLootPickup.LootKind kind, float amount, Sprite sprite, Vector3 position, string displayName)
    {
        if (amount <= 0f || sprite == null) return;

        GameObject pickupRoot = new GameObject(displayName);
        pickupRoot.transform.SetPositionAndRotation(position, Quaternion.identity);
        pickupRoot.transform.localScale = Vector3.one * 0.72f;

        SpriteRenderer renderer = pickupRoot.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        CircleCollider2D trigger = pickupRoot.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.48f;

        WorldLootPickup pickup = pickupRoot.AddComponent<WorldLootPickup>();
        pickup.Configure(kind, amount);
    }
}
