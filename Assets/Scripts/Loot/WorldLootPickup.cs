using UnityEngine;

/// <summary>
/// A small reusable world pickup.  It owns only the collection effect; authored
/// enemies decide which rewards to spawn through Map01LootDropOnDeath.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class WorldLootPickup : MonoBehaviour
{
    public enum LootKind { Gold, Health, Energy, TemporalBean }

    [SerializeField] private LootKind kind;
    [SerializeField, Min(1f)] private float amount = 1f;
    [SerializeField] private bool collectOnTouch = true;

    private bool collected;

    public LootKind Kind => kind;
    public float Amount => amount;

    public void Configure(LootKind lootKind, float lootAmount)
    {
        kind = lootKind;
        amount = Mathf.Max(1f, lootAmount);
    }

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collectOnTouch)
            TryCollect(other.GetComponentInParent<PlayerMovement>()?.gameObject);
    }

    /// <summary>Public for authored interaction prompts and deterministic tests.</summary>
    public bool TryCollect(GameObject player)
    {
        if (collected || player == null) return false;

        switch (kind)
        {
            case LootKind.Gold:
            {
                PlayerResources resources = player.GetComponent<PlayerResources>();
                if (resources == null) return false;
                resources.AddGold(Mathf.RoundToInt(amount));
                break;
            }
            case LootKind.Health:
            {
                Health health = player.GetComponent<Health>();
                if (health == null || health.IsDead) return false;
                health.Heal(amount);
                break;
            }
            case LootKind.Energy:
            {
                PlayerResources resources = player.GetComponent<PlayerResources>();
                if (resources == null) return false;
                resources.RestoreEnergy(amount);
                break;
            }
            case LootKind.TemporalBean:
            {
                PlayerConsumables consumables = player.GetComponent<PlayerConsumables>();
                if (consumables == null) return false;
                consumables.AddTemporalBeans(Mathf.RoundToInt(amount));
                break;
            }
        }

        collected = true;
        gameObject.SetActive(false);
        return true;
    }
}
