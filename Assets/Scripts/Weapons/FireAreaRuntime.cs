using UnityEngine;

[DisallowMultipleComponent]
public sealed class FireAreaRuntime : MonoBehaviour
{
    private const int Segments = 28;
    private AttackResolver resolver;
    private LineRenderer ring;
    private Material material;
    private float elapsed;
    private float duration;
    private float radius;
    private float damage;
    private Color color;
    private bool active;

    private void Awake() => resolver = new AttackResolver();

    public bool Cast(float amount, float areaRadius, float visualDuration, Color fireColor)
    {
        if (active) return false;
        damage = amount;
        radius = Mathf.Max(0.1f, areaRadius);
        duration = Mathf.Max(0.1f, visualDuration);
        color = fireColor;
        elapsed = 0f;
        active = true;
        resolver.Execute(new AttackSnapshot
        {
            Position = transform.position,
            Direction = Vector2.right,
            Range = radius,
            Damage = damage,
            ArcAngle = 360f
        });
        EnsureVisual();
        ring.gameObject.SetActive(true);
        UpdateVisual();
        return true;
    }

    private void Update()
    {
        if (!active) return;
        elapsed += Time.deltaTime;
        UpdateVisual();
        if (elapsed < duration) return;
        active = false;
        ring.gameObject.SetActive(false);
    }

    private void EnsureVisual()
    {
        if (ring != null) return;
        GameObject visual = new GameObject("FlameNovaVFX");
        visual.hideFlags = HideFlags.HideAndDontSave;
        ring = visual.AddComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = Segments;
        ring.startWidth = 0.12f;
        ring.endWidth = 0.07f;
        ring.sortingOrder = 25;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) { material = new Material(shader); ring.material = material; }
    }

    private void UpdateVisual()
    {
        float t = Mathf.Clamp01(elapsed / duration);
        float visualRadius = radius * Mathf.Lerp(0.35f, 1f, t);
        Color tint = color;
        tint.a *= 1f - t;
        ring.startColor = tint;
        ring.endColor = tint;
        for (int i = 0; i < Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            Vector2 point = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * visualRadius;
            ring.SetPosition(i, new Vector3(point.x, point.y, transform.position.z - 0.2f));
        }
    }

    private void OnDestroy()
    {
        if (ring != null) Destroy(ring.gameObject);
        if (material != null) Destroy(material);
    }
}
