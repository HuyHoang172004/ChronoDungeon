using UnityEngine;

[DisallowMultipleComponent]
public sealed class DashFeedback : MonoBehaviour
{
    [SerializeField] private Color dashColor = new Color(0.2f, 0.9f, 1f, 0.85f);
    [SerializeField, Min(0.05f)] private float duration = 0.16f;
    private LineRenderer line;
    private Material material;
    private float until;

    private void Awake()
    {
        var visual = new GameObject("Dash Trail");
        visual.transform.SetParent(transform, false);
        line = visual.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.widthMultiplier = 0.16f;
        line.numCapVertices = 3;
        material = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = material;
        line.startColor = new Color(dashColor.r, dashColor.g, dashColor.b, 0f);
        line.endColor = dashColor;
        line.enabled = false;
    }

    public void Play(Vector2 start, Vector2 direction, float distance)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, start + direction.normalized * distance);
        line.enabled = true;
        until = Time.time + duration;
    }

    public void Play(Vector2 direction)
    {
        Vector2 end = (Vector2)transform.position;
        Play(end - direction.normalized, direction, 1f);
    }

    private void Update()
    {
        if (line != null && line.enabled && Time.time >= until) line.enabled = false;
    }

    private void OnDisable() { if (line != null) line.enabled = false; }
    private void OnDestroy()
    {
        if (material != null) Destroy(material);
        if (line != null) Destroy(line.gameObject);
    }
}
