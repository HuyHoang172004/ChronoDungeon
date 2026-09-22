using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    AttackDamage, MovementSpeed, MaxHealth, DashCooldown, Heal,
    LoopDuration, GhostDamage, TemporalAbility
}

[Serializable]
public sealed class UpgradeData
{
    public string id;
    public string title;
    [TextArea] public string description;
    public UpgradeType type;
    public float amount;
}

[DisallowMultipleComponent]
public sealed class UpgradeManager : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Health playerHealth;
    [SerializeField] private List<UpgradeData> upgradePool = new List<UpgradeData>();
    private readonly Dictionary<UpgradeType, int> stacks = new Dictionary<UpgradeType, int>();
    private float baseMoveSpeed;
    private float baseAttackDamage;
    private float baseDashCooldown;
    private float baseMaxHealth;
    private int appliedUpgradeCount;

    public int AppliedUpgradeCount => appliedUpgradeCount;
    public float CurrentAttackDamage => playerAttack == null ? 0f : playerAttack.attackDamage;
    public float CurrentMoveSpeed => playerMovement == null ? 0f : playerMovement.moveSpeed;
    public float CurrentDashCooldown => playerDash == null ? 0f : playerDash.DashCooldown;
    public float CurrentMaxHealth => playerHealth == null ? 0f : playerHealth.maxHealth;
    public IReadOnlyList<UpgradeData> UpgradePool => upgradePool;

    private void Awake()
    {
        if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerAttack == null) playerAttack = FindAnyObjectByType<PlayerAttack>();
        if (playerDash == null) playerDash = FindAnyObjectByType<PlayerDash>();
        if (playerHealth == null && playerMovement != null) playerHealth = playerMovement.GetComponent<Health>();
        EnsureDefaultPool();
        CaptureBaseline();
    }

    private void EnsureDefaultPool()
    {
        if (upgradePool != null && upgradePool.Count >= 8) return;
        upgradePool = new List<UpgradeData>
        {
            new UpgradeData { id = "attack_damage", title = "Temporal Edge", description = "+10 attack damage", type = UpgradeType.AttackDamage, amount = 10f },
            new UpgradeData { id = "movement_speed", title = "Swift Current", description = "+1 movement speed", type = UpgradeType.MovementSpeed, amount = 1f },
            new UpgradeData { id = "max_health", title = "Timeworn Vitality", description = "+20 maximum health", type = UpgradeType.MaxHealth, amount = 20f },
            new UpgradeData { id = "dash_cooldown", title = "Blink Rhythm", description = "-0.2s dash cooldown", type = UpgradeType.DashCooldown, amount = 0.2f },
            new UpgradeData { id = "heal", title = "Rewound Breath", description = "Restore 30 health", type = UpgradeType.Heal, amount = 30f },
            new UpgradeData { id = "loop_duration", title = "Stretched Moment", description = "+2 seconds loop duration", type = UpgradeType.LoopDuration, amount = 2f },
            new UpgradeData { id = "ghost_damage", title = "Echo Force", description = "+25% Temporal Ghost damage", type = UpgradeType.GhostDamage, amount = 0.25f },
            new UpgradeData { id = "temporal_ability", title = "Paradox Focus", description = "Improve the next temporal ability", type = UpgradeType.TemporalAbility, amount = 1f }
        };
    }

    private void CaptureBaseline()
    {
        baseMoveSpeed = playerMovement == null ? 0f : playerMovement.moveSpeed;
        baseAttackDamage = playerAttack == null ? 0f : playerAttack.attackDamage;
        baseDashCooldown = playerDash == null ? 0f : playerDash.DashCooldown;
        baseMaxHealth = playerHealth == null ? 0f : playerHealth.maxHealth;
    }

    public void ApplyUpgrade(UpgradeData data)
    {
        if (data == null) return;
        ApplyUpgrade(data.type, data.amount);
    }

    public void ApplyUpgrade(UpgradeType type, float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount)) return;
        switch (type)
        {
            case UpgradeType.AttackDamage:
                if (playerAttack != null) playerAttack.attackDamage += amount;
                break;
            case UpgradeType.MovementSpeed:
                if (playerMovement != null) playerMovement.moveSpeed = Mathf.Max(.1f, playerMovement.moveSpeed + amount);
                break;
            case UpgradeType.MaxHealth:
                if (playerHealth != null) playerHealth.SetMaximumHealth(playerHealth.maxHealth + amount, true);
                break;
            case UpgradeType.DashCooldown:
                if (playerDash != null) playerDash.SetDashCooldown(playerDash.DashCooldown - amount);
                break;
            case UpgradeType.Heal:
                if (playerHealth != null) playerHealth.Heal(Mathf.Max(0f, amount));
                break;
        }
        stacks[type] = GetStackCount(type) + 1;
        appliedUpgradeCount++;
    }

    public int GetStackCount(UpgradeType type) => stacks.TryGetValue(type, out int count) ? count : 0;

    // Loop rewind deliberately leaves run upgrades intact. New-run callers use this explicit reset.
    public void ResetRun()
    {
        if (playerMovement != null) playerMovement.moveSpeed = baseMoveSpeed;
        if (playerAttack != null) playerAttack.attackDamage = baseAttackDamage;
        if (playerDash != null) playerDash.SetDashCooldown(baseDashCooldown);
        if (playerHealth != null) playerHealth.SetMaximumHealth(baseMaxHealth, true);
        stacks.Clear();
        appliedUpgradeCount = 0;
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState() { }
}
