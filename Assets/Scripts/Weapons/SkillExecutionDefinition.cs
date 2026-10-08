using UnityEngine;

public abstract class SkillExecutionDefinition : ScriptableObject
{
    public abstract bool Execute(PlayerWeaponController controller, SkillDefinition skill);
}
