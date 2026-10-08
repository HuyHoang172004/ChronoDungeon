using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Chrono Burst", fileName = "ChronoBurstExecution")]
public sealed class ChronoBurstExecution : SkillExecutionDefinition
{
    [SerializeField, Min(0.25f)] private float visualDuration = 0.36f;
    [SerializeField, Min(0f)] private float startOffset = 0.35f;
    [SerializeField] private Color ringColor = new Color(0.12f, 0.95f, 1f, 0.86f);
    [SerializeField] private Color coreColor = new Color(0.72f, 1f, 1f, 0.95f);
    [SerializeField] private Color accentColor = new Color(0.55f, 0.2f, 1f, 0.45f);

    public override bool Execute(PlayerWeaponController controller, SkillDefinition skill)
    {
        if (controller == null || skill == null) return false;

        ChronoBurstRuntime runtime = controller.GetComponent<ChronoBurstRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<ChronoBurstRuntime>();

        return runtime.Cast(skill, visualDuration, startOffset, ringColor, coreColor, accentColor);
    }
}
