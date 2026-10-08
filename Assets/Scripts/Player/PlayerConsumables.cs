using System;
using UnityEngine;

/// <summary>
/// Owns the Player's limited-use recovery item.  The effect is percentage based
/// so it remains useful when maximum HP or Energy changes later.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerConsumables : MonoBehaviour
{
    [Header("Temporal Bean")]
    [SerializeField, Min(0)] private int temporalBeans;
    [SerializeField, Min(1)] private int maximumTemporalBeans = 9;
    [SerializeField, Range(0.01f, 1f)] private float healthRestorePercent = 0.35f;
    [SerializeField, Range(0.01f, 1f)] private float energyRestorePercent = 0.35f;
    [SerializeField, Min(0f)] private float useCooldown = 3f;

    private Health health;
    private PlayerResources resources;
    private float nextUseTime;

    public int TemporalBeans => temporalBeans;
    public int MaximumTemporalBeans => maximumTemporalBeans;
    public float CooldownRemaining => Mathf.Max(0f, nextUseTime - Time.time);
    public bool CanUseTemporalBean => temporalBeans > 0 && CooldownRemaining <= 0f &&
        health != null && !health.IsDead &&
        (health.currentHealth + 0.01f < health.maxHealth ||
         (resources != null && resources.Energy + 0.01f < resources.MaximumEnergy));

    public event Action<PlayerConsumables> BeansChanged;

    private void Awake()
    {
        health = GetComponent<Health>();
        resources = GetComponent<PlayerResources>();
        maximumTemporalBeans = Mathf.Max(1, maximumTemporalBeans);
        temporalBeans = Mathf.Clamp(temporalBeans, 0, maximumTemporalBeans);
    }

    public void AddTemporalBeans(int amount)
    {
        if (amount <= 0) return;
        int next = Mathf.Clamp(temporalBeans + amount, 0, maximumTemporalBeans);
        if (next == temporalBeans) return;
        temporalBeans = next;
        BeansChanged?.Invoke(this);
    }

    public bool UseTemporalBean()
    {
        if (!CanUseTemporalBean) return false;

        temporalBeans--;
        nextUseTime = Time.time + useCooldown;
        health.Heal(health.maxHealth * healthRestorePercent);
        if (resources != null)
            resources.RestoreEnergy(resources.MaximumEnergy * energyRestorePercent);
        BeansChanged?.Invoke(this);
        return true;
    }
}
