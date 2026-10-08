using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Weapons/Executions/Shadow Volley", fileName = "ShadowVolleyExecution")]
public sealed class ShadowVolleyExecution : SkillExecutionDefinition
{
    [SerializeField, Min(1f)] private float speed = 10f;
    [SerializeField, Min(.1f)] private float lifetime = .7f;
    [SerializeField, Min(1)] private int arrowCount = 3;
    [SerializeField, Range(1f, 60f)] private float totalSpread = 28f;
    [SerializeField] private Color volleyColor = new Color(.75f, .18f, 1f, 1f);

    public override bool Execute(PlayerWeaponController controller, SkillDefinition skill)
    {
        if (controller == null || skill == null) return false;
        VoidArrowRuntime runtime = controller.GetComponent<VoidArrowRuntime>();
        if (runtime == null) runtime = controller.gameObject.AddComponent<VoidArrowRuntime>();
        int count = Mathf.Max(1, arrowCount);
        float range = skill.Range > 0f ? skill.Range : 4.8f;
        bool fired = false;
        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? .5f : (float)i / (count - 1);
            fired |= runtime.Fire(skill.Damage, range, speed, lifetime, volleyColor,
                Mathf.Lerp(-totalSpread * .5f, totalSpread * .5f, t), 1);
        }
        return fired;
    }
}
