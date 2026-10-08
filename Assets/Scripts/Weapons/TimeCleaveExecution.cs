using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Time Cleave", fileName = "TimeCleaveExecution")]
public sealed class TimeCleaveExecution : SkillExecutionDefinition
{
    [SerializeField, Min(0.1f)] private float windupDuration = 0.15f;
    [SerializeField, Min(0.1f)] private float strikeDuration = 0.15f;
    [SerializeField, Min(0.1f)] private float recoveryDuration = 0.18f;
    [SerializeField, Min(0f)] private float windupDistance = 0.06f;
    [SerializeField, Min(0f)] private float lungeDistance = 0.12f;
    [SerializeField, Range(60f, 160f)] private float weaponSweepAngle = 125f;
    [SerializeField] private Color weaponGlowColor = new Color(0.35f, 1f, 1f, 1f);
    [SerializeField] private Color slashColor = new Color(0.12f, 0.95f, 1f, 0.9f);
    [SerializeField] private Color accentColor = new Color(0.55f, 0.2f, 1f, 0.5f);

    public override bool Execute(PlayerWeaponController controller, SkillDefinition skill)
    {
        if (controller == null || skill == null) return false;

        TimeCleaveRuntime runtime = controller.GetComponent<TimeCleaveRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<TimeCleaveRuntime>();

        return runtime.Cast(skill, windupDuration, strikeDuration, recoveryDuration,
            windupDistance, lungeDistance, weaponSweepAngle,
            weaponGlowColor, slashColor, accentColor);
    }
}
