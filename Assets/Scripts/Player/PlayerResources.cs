using System;
using UnityEngine;

/// <summary>
/// Runtime resource state owned by the Player.  It intentionally contains no
/// combat rules: skills, pickups and shops decide when to spend or grant a resource.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerResources : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField, Min(1f)] private float maximumEnergy = 100f;
    [SerializeField] private float energy = 100f;

    [Header("Gold")]
    [SerializeField, Min(0)] private int gold;

    public float MaximumEnergy => maximumEnergy;
    public float Energy => energy;
    public int Gold => gold;
    public event Action<PlayerResources> EnergyChanged;
    public event Action<PlayerResources> GoldChanged;

    private void Awake()
    {
        maximumEnergy = Mathf.Max(1f, maximumEnergy);
        energy = Mathf.Clamp(energy, 0f, maximumEnergy);
        gold = Mathf.Max(0, gold);
    }

    public bool TrySpendEnergy(float amount)
    {
        if (amount <= 0f) return true;
        if (energy + Mathf.Epsilon < amount) return false;
        energy = Mathf.Clamp(energy - amount, 0f, maximumEnergy);
        EnergyChanged?.Invoke(this);
        return true;
    }

    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f) return;
        float next = Mathf.Clamp(energy + amount, 0f, maximumEnergy);
        if (Mathf.Approximately(next, energy)) return;
        energy = next;
        EnergyChanged?.Invoke(this);
    }

    public void RestoreFullEnergy()
    {
        if (Mathf.Approximately(energy, maximumEnergy)) return;
        energy = maximumEnergy;
        EnergyChanged?.Invoke(this);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold = Mathf.Max(0, gold + amount);
        GoldChanged?.Invoke(this);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        GoldChanged?.Invoke(this);
        return true;
    }
}
