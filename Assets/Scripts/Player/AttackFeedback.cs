using UnityEngine;

[DisallowMultipleComponent]
public sealed class AttackFeedback : MonoBehaviour
{
    [SerializeField] private Color slashColor = new Color(1f, 0.82f, 0.25f, 0.9f);
    [SerializeField, Min(0.01f)] private float duration = 0.1f;
    private LineRenderer line;
    private Material material;
    private float until;

    private void Awake()
    {
        var visual = new GameObject("Attack Slash");
        visual.transform.SetParent(transform, false);
        line = visual.AddComponent<LineRenderer>();
        line.positionCount = 3;
        line.useWorldSpace = true;
        line.widthMultiplier = 0.09f;
        line.numCapVertices = 2;
        material = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = material;
        line.startColor = slashColor;
        line.endColor = new Color(slashColor.r, slashColor.g, slashColor.b, 0f);
        line.enabled = false;
    }

    public void Play(AttackSnapshot attack)
    {
        Vector2 direction = attack.Direction.sqrMagnitude > 0.001f ? attack.Direction.normalized : Vector2.right;
        Vector2 side = new Vector2(-direction.y, direction.x) * attack.Range * 0.28f;
        Vector3 origin = attack.Position;
        Vector3 tip = origin + (Vector3)(direction * attack.Range);
        line.SetPosition(0, origin + (Vector3)side * 0.4f);
        line.SetPosition(1, tip);
        line.SetPosition(2, origin - (Vector3)side * 0.4f);
        line.enabled = true;
        until = Time.time + duration;
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
