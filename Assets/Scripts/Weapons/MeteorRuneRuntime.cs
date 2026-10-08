using UnityEngine;

[DisallowMultipleComponent]
public sealed class MeteorRuneRuntime : MonoBehaviour
{
    private const int Segments = 24;
    private AttackResolver resolver;
    private PlayerMovement movement;
    private LineRenderer rune;
    private Material material;
    private SkillDefinition skill;
    private Vector2 target;
    private float delay;
    private float duration;
    private float elapsed;
    private float radius;
    private Color warningColor;
    private Color impactColor;
    private bool active;
    private bool damaged;

    private void Awake()
    {
        resolver = new AttackResolver();
        movement = GetComponent<PlayerMovement>();
    }

    public bool Cast(SkillDefinition definition, float castDelay, float visualLifetime,
        Color warning, Color impact)
    {
        if (active || definition == null) return false;
        skill = definition;
        Vector2 direction = movement != null ? movement.FacingDirection : Vector2.right;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        target = (Vector2)transform.position + direction.normalized * Mathf.Max(0.5f, definition.Range > 0f ? definition.Range : 3.5f);
        delay = Mathf.Max(0.1f, castDelay);
        duration = Mathf.Max(delay + 0.1f, visualLifetime);
        radius = definition.Radius > 0f ? definition.Radius : 1.6f;
        warningColor = warning;
        impactColor = impact;
        elapsed = 0f;
        damaged = false;
        active = true;
        EnsureVisual();
        rune.gameObject.SetActive(true);
        UpdateVisual();
        return true;
    }

    private void Update()
    {
        if (!active) return;
        elapsed += Time.deltaTime;
        if (!damaged && elapsed >= delay)
        {
            damaged = true;
            resolver.Execute(new AttackSnapshot
            {
                Position = target,
                Direction = Vector2.right,
                Range = radius,
                Damage = skill.Damage,
                ArcAngle = 360f
            });
        }
        UpdateVisual();
        if (elapsed < duration) return;
        active = false;
        rune.gameObject.SetActive(false);
    }

    private void EnsureVisual()
    {
        if (rune != null) return;
        GameObject visual = new GameObject("MeteorRuneVFX");
        visual.hideFlags = HideFlags.HideAndDontSave;
        rune = visual.AddComponent<LineRenderer>();
        rune.useWorldSpace = true;
        rune.loop = true;
        rune.positionCount = Segments;
        rune.startWidth = 0.1f;
        rune.endWidth = 0.05f;
        rune.sortingOrder = 24;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) { material = new Material(shader); rune.material = material; }
    }

    private void UpdateVisual()
    {
        float t = Mathf.Clamp01(elapsed / duration);
        float pulse = Mathf.Lerp(0.8f, 1.15f, Mathf.PingPong(elapsed * 3f, 1f));
        Color tint = elapsed < delay ? warningColor : impactColor;
        tint.a *= elapsed < delay ? 1f : 1f - Mathf.Clamp01((elapsed - delay) / Mathf.Max(0.01f, duration - delay));
        rune.startColor = tint;
        rune.endColor = tint;
        for (int i = 0; i < Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            Vector2 point = target + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * pulse;
            rune.SetPosition(i, new Vector3(point.x, point.y, transform.position.z - 0.21f));
        }
    }

    private void OnDestroy()
    {
        if (rune != null) Destroy(rune.gameObject);
        if (material != null) Destroy(material);
    }
}
