using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Flame Nova", fileName = "FlameNovaExecution")]
public sealed class FlameNovaExecution : SkillExecutionDefinition
{
    [SerializeField, Min(0.2f)] private float visualDuration = 0.35f;
    [SerializeField] private Color fireColor = new Color(1f, 0.22f, 0.04f, 0.9f);

    public override bool Execute(PlayerWeaponController controller, SkillDefinition skill)
    {
        if (controller == null || skill == null) return false;
        FireAreaRuntime runtime = controller.GetComponent<FireAreaRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<FireAreaRuntime>();
        return runtime.Cast(skill.Damage, skill.Radius > 0f ? skill.Radius : 2.2f, visualDuration, fireColor);
    }
}
