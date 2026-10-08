using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Fire Bolt", fileName = "FireBoltExecution")]
public sealed class FireBoltExecution : BasicAttackExecutionDefinition
{
    [SerializeField, Min(1f)] private float speed = 8f;
    [SerializeField, Min(0.1f)] private float lifetime = 0.7f;
    [SerializeField] private Color boltColor = new Color(1f, 0.25f, 0.05f, 1f);

    public override bool Execute(PlayerWeaponController controller, WeaponDefinition weapon)
    {
        if (controller == null || weapon == null) return false;
        FireBoltRuntime runtime = controller.GetComponent<FireBoltRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<FireBoltRuntime>();
        return runtime.Cast(weapon.BaseDamage, weapon.AttackRange, speed, lifetime, boltColor);
    }
}
