using UnityEngine;

// Presentation-only circle, reusable by attacks with a radial warning area.
[DisallowMultipleComponent, RequireComponent(typeof(LineRenderer))]
public sealed class EnemyAttackTelegraph : MonoBehaviour
{
    [SerializeField] private Color warningColor = new Color(1f, 0.65f, 0.1f, 0.85f);
    [SerializeField] private Color impactColor = new Color(1f, 0.15f, 0.06f, 1f);
    private LineRenderer line;
    private const int Segments = 40;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = Segments;
        line.enabled = false;
    }

    public void Show(Vector3 center, float radius, float progress)
    {
        if (line == null) return;
        Color color = Color.Lerp(warningColor, impactColor, Mathf.Clamp01(progress));
        line.startColor = line.endColor = color;
        line.widthMultiplier = Mathf.Lerp(0.045f, 0.09f, Mathf.Clamp01(progress));
        for (int i = 0; i < Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
        line.enabled = true;
    }

    public void Hide() { if (line != null) line.enabled = false; }
    private void OnDisable() => Hide();
}
