using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health target;
    [SerializeField] private RectTransform fill;

    private void OnEnable()
    {
        if (target == null) target = GetComponentInParent<Health>();
        if (target == null) return;
        target.Changed += Refresh;
        Refresh(target);
    }

    private void OnDisable()
    {
        if (target != null) target.Changed -= Refresh;
    }

    private void Start()
    {
        if (target != null) Refresh(target);
    }

    private void Refresh(Health health)
    {
        if (fill == null) return;
        fill.anchorMax = new Vector2(health.currentHealth / health.maxHealth, 1f);
    }
}
