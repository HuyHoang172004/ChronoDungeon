using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Weapon Definition", fileName = "NewWeapon")]
public sealed class WeaponDefinition : ScriptableObject
{
    [SerializeField] private string weaponName = "Weapon";
    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private Sprite weaponIcon;
    [SerializeField, Min(0f)] private float baseDamage = 25f;
    [SerializeField, Min(0f)] private float attackCooldown = 0.35f;
    [SerializeField, Min(0f)] private float attackRange = 2f;
    [SerializeField] private BasicAttackExecutionDefinition basicAttackExecution;
    [SerializeField] private Vector3 holdLocalPosition;
    [SerializeField] private float holdRotationZ;
    [SerializeField] private Vector3 holdLocalScale = Vector3.one;
    [SerializeField] private SkillDefinition skill1;
    [SerializeField] private SkillDefinition skill2;
    [SerializeField] private AudioClip basicAttackSfx;
    [SerializeField] private GameObject basicAttackVfxPrefab;

    public string WeaponName => weaponName;
    public Sprite WeaponSprite => weaponSprite;
    public Sprite WeaponIcon => weaponIcon;
    public float BaseDamage => baseDamage;
    public float AttackCooldown => attackCooldown;
    public float AttackRange => attackRange;
    public BasicAttackExecutionDefinition BasicAttackExecution => basicAttackExecution;
    public Vector3 HoldLocalPosition => holdLocalPosition;
    public float HoldRotationZ => holdRotationZ;
    public Vector3 HoldLocalScale => holdLocalScale;
    public SkillDefinition Skill1 => skill1;
    public SkillDefinition Skill2 => skill2;
    public AudioClip BasicAttackSfx => basicAttackSfx;
    public GameObject BasicAttackVfxPrefab => basicAttackVfxPrefab;
}
