using UnityEngine;

/// <summary>
/// Attach to an enemy prefab or instance to give it a compact authored loot table.
/// Gold always drops; recovery and Bean drops use independent small chances.
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Health))]
public sealed class EnemyLootDrop : MonoBehaviour
{
    [Header("Guaranteed Gold")]
    [SerializeField, Min(1)] private int minimumGold = 2;
    [SerializeField, Min(1)] private int maximumGold = 5;

    [Header("Optional recovery")]
    [SerializeField, Range(0f, 1f)] private float healthDropChance = 0.18f;
    [SerializeField, Min(1f)] private float healthAmount = 20f;
    [SerializeField, Range(0f, 1f)] private float energyDropChance = 0.16f;
    [SerializeField, Min(1f)] private float energyAmount = 18f;
    [SerializeField, Range(0f, 1f)] private float beanDropChance = 0.05f;

    private Health health;
    private bool dropped;

    private void Awake() => health = GetComponent<Health>();

    private void OnEnable()
    {
        if (health == null) health = GetComponent<Health>();
        if (health != null) health.Died += DropLoot;
        dropped = false;
    }

    private void OnDisable()
    {
        if (health != null) health.Died -= DropLoot;
    }

    private void DropLoot(Health _)
    {
        if (dropped) return;
        dropped = true;
        int min = Mathf.Min(minimumGold, maximumGold);
        int max = Mathf.Max(minimumGold, maximumGold);
        LootPickup.Spawn(transform.position + RandomOffset(), LootPickup.Kind.Gold, Random.Range(min, max + 1));
        if (Random.value <= healthDropChance)
            LootPickup.Spawn(transform.position + RandomOffset(), LootPickup.Kind.Health, healthAmount);
        if (Random.value <= energyDropChance)
            LootPickup.Spawn(transform.position + RandomOffset(), LootPickup.Kind.Energy, energyAmount);
        if (Random.value <= beanDropChance)
            LootPickup.Spawn(transform.position + RandomOffset(), LootPickup.Kind.TemporalBean, 1f);
    }

    private static Vector3 RandomOffset()
    {
        Vector2 offset = Random.insideUnitCircle * 0.28f;
        return new Vector3(offset.x, offset.y, 0f);
    }
}
