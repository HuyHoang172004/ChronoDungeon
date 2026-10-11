using System;
using UnityEngine;

public enum EnemyAbilityKind { Melee, Area, Projectile, Dash, Guard, Teleport, Summon }

[Serializable]
public struct EnemyAbilitySpec
{
    public string abilityId;
    public EnemyAbilityKind kind;
    public CharacterAnimationState animationState;
    [Min(0f)] public float minimumRange;
    [Min(.1f)] public float maximumRange;
    [Min(0f)] public float damage;
    [Min(.1f)] public float windup;
    [Min(.1f)] public float activeDuration;
    [Min(.1f)] public float recovery;
    [Min(.1f)] public float cooldown;
    [Min(0f)] public float weight;
    public GameObject vfxPrefab;
}

[CreateAssetMenu(menuName = "ChronoDungeon/Enemies/Ability Profile", fileName = "EnemyAbilityProfile")]
public sealed class EnemyAbilityProfile : ScriptableObject
{
    [SerializeField] private string characterId;
    [SerializeField] private EnemyAbilitySpec[] abilities = Array.Empty<EnemyAbilitySpec>();
    public string CharacterId => characterId;
    public EnemyAbilitySpec[] Abilities => abilities;

    public static EnemyAbilityProfile CreateRuntime(GameObject owner)
    {
        string id = CharacterAnimationResolver.Resolve(owner)?.CharacterId;
        if (string.IsNullOrWhiteSpace(id)) id = owner != null ? owner.name : "Enemy";
        string key = id.ToLowerInvariant();
        bool ranged = key.Contains("archer") || key.Contains("wisp") || key.Contains("tome") ||
                      key.Contains("spitter") || key.Contains("mage") || key.Contains("curator") || key.Contains("caster");
        bool boss = key.Contains("bosses_") || key.Contains("guardian") || key.Contains("gatebreaker") ||
                    key.Contains("matriarch") || key.Contains("captain") || key.Contains("jailer");

        EnemyAbilityProfile runtime = CreateInstance<EnemyAbilityProfile>();
        runtime.hideFlags = HideFlags.DontSave;
        runtime.characterId = id;
        EnemyAbilitySpec primary = new EnemyAbilitySpec
        {
            abilityId = ranged ? "RangedPrimary" : "MeleePrimary",
            kind = ranged ? EnemyAbilityKind.Projectile : EnemyAbilityKind.Melee,
            animationState = ranged ? CharacterAnimationState.Cast : CharacterAnimationState.Attack,
            minimumRange = 0f,
            maximumRange = ranged ? 8.5f : (boss ? 2.35f : 1.45f),
            damage = boss ? 20f : 12f,
            windup = boss ? .55f : .38f,
            activeDuration = .22f,
            recovery = boss ? .75f : .5f,
            cooldown = boss ? 1.2f : 1.55f,
            weight = 1f
        };
        if (!boss)
        {
            runtime.abilities = new[] { primary };
            return runtime;
        }
        EnemyAbilitySpec area = new EnemyAbilitySpec
        {
            abilityId = "BossAreaSlam",
            kind = EnemyAbilityKind.Area,
            animationState = CharacterAnimationState.Attack,
            minimumRange = 0f,
            maximumRange = 3.5f,
            damage = 28f,
            windup = .9f,
            activeDuration = .3f,
            recovery = 1f,
            cooldown = 4.5f,
            weight = .45f
        };
        runtime.abilities = new[] { primary, area };
        return runtime;
    }
}
