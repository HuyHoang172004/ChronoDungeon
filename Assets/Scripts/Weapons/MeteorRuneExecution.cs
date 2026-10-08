using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Meteor Rune", fileName = "MeteorRuneExecution")]
public sealed class MeteorRuneExecution : SkillExecutionDefinition
{
    [SerializeField, Min(0.1f)] private float delay = 0.5f;
    [SerializeField, Min(0.1f)] private float visualDuration = 0.8f;
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.05f, 0.72f);
    [SerializeField] private Color impactColor = new Color(1f, 0.08f, 0.02f, 0.95f);

    public override bool Execute(PlayerWeaponController controller, SkillDefinition skill)
    {
        if (controller == null || skill == null) return false;
        MeteorRuneRuntime runtime = controller.GetComponent<MeteorRuneRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<MeteorRuneRuntime>();
        return runtime.Cast(skill, delay, visualDuration, warningColor, impactColor);
    }
}
