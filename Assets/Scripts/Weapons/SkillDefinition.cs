using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Skill Definition", fileName = "NewSkill")]
public sealed class SkillDefinition : ScriptableObject
{
    [SerializeField] private string skillName = "Skill";
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField, Min(0f)] private float cooldown = 1f;
    [SerializeField, Min(0f)] private float damage;
    [SerializeField, Min(0f)] private float range;
    [SerializeField, Min(0f)] private float radius;
    [SerializeField] private SkillExecutionDefinition execution;
    [SerializeField] private AudioClip sfx;
    [SerializeField] private GameObject vfxPrefab;

    public string SkillName => skillName;
    public string Description => description;
    public Sprite Icon => icon;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float Range => range;
    public float Radius => radius;
    public SkillExecutionDefinition Execution => execution;
    public AudioClip Sfx => sfx;
    public GameObject VfxPrefab => vfxPrefab;
}
