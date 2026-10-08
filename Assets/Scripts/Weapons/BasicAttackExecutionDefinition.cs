using UnityEngine;

public abstract class BasicAttackExecutionDefinition : ScriptableObject
{
    public abstract bool Execute(PlayerWeaponController controller, WeaponDefinition weapon);
}
