using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Piercing Shot", fileName = "PiercingShotExecution")]
public sealed class PiercingShotExecution : SkillExecutionDefinition
{
    [SerializeField, Min(1f)] private float speed = 13f;
    [SerializeField, Min(.1f)] private float lifetime = .75f;
    [SerializeField, Min(1)] private int pierceTargets = 3;
    [SerializeField] private Color shotColor = new Color(.25f, .9f, 1f, 1f);

    public override bool Execute(PlayerWeaponController controller, SkillDefinition skill)
    {
        if (controller == null || skill == null) return false;
        VoidArrowRuntime runtime = controller.GetComponent<VoidArrowRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<VoidArrowRuntime>();
        return runtime.Fire(skill.Damage, skill.Range > 0f ? skill.Range : 5.5f,
            speed, lifetime, shotColor, 0f, pierceTargets);
    }
}
