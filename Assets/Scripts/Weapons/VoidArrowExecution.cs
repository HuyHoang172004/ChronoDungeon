using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Void Arrow", fileName = "VoidArrowExecution")]
public sealed class VoidArrowExecution : BasicAttackExecutionDefinition
{
    [SerializeField, Min(1f)] private float speed = 11f;
    [SerializeField, Min(.1f)] private float lifetime = .65f;
    [SerializeField] private Color arrowColor = new Color(.55f, .18f, 1f, 1f);

    public override bool Execute(PlayerWeaponController controller, WeaponDefinition weapon)
    {
        if (controller == null || weapon == null) return false;
        VoidArrowRuntime runtime = controller.GetComponent<VoidArrowRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<VoidArrowRuntime>();
        return runtime.Fire(weapon.BaseDamage, weapon.AttackRange, speed, lifetime, arrowColor, 0f, 1);
    }
}
