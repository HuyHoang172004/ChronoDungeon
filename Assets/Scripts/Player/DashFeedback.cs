using UnityEngine;

[DisallowMultipleComponent]
public sealed class DashFeedback : MonoBehaviour
{
    [SerializeField] private Color dashColor = new Color(0.2f, 0.9f, 1f, 0.85f);
    [SerializeField, Min(0.05f)] private float duration = 0.16f;
    [SerializeField, Range(3, 5)] private int afterImageCount = 4;
    [SerializeField, Min(0.15f)] private float afterImageLifetime = 0.2f;
    [SerializeField, Range(0.1f, 0.6f)] private float afterImageAlpha = 0.42f;
    [SerializeField, Min(0.01f)] private float trailWidth = 0.1f;
    [SerializeField, Min(0.05f)] private float burstDuration = 0.18f;
    private LineRenderer line;
    private LineRenderer burstLine;
    private Material material;
    private float until;
    private float burstElapsed = -1f;
    private Vector2 burstOrigin;
    private Transform sourceTransform;
    private SpriteRenderer sourceRenderer;
    private GameObject[] afterImageObjects;
    private SpriteRenderer[] afterImageRenderers;
    private float[] afterImageRemaining;
    private GameObject burstObject;
    private int nextAfterImage;

    private void Awake()
    {
        sourceTransform = transform.Find("PlayerVisual");
        if (sourceTransform != null) sourceRenderer = sourceTransform.GetComponent<SpriteRenderer>();

        var visual = new GameObject("Dash Trail");
        visual.transform.SetParent(transform, false);
        line = visual.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.widthMultiplier = trailWidth;
        line.numCapVertices = 3;
        line.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder - 1 : 0;
        material = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = material;
        line.startColor = new Color(dashColor.r, dashColor.g, dashColor.b, 0f);
        line.endColor = dashColor;
        line.enabled = false;

        CreateAfterImagePool();
        CreateBurstRing();
    }

    public void Play(Vector2 start, Vector2 direction, float distance)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, start + direction.normalized * distance);
        line.enabled = true;
        until = Time.time + duration;
        SpawnAfterImages(start, direction, distance);
        StartBurst(start);
    }

    public void Play(Vector2 direction)
    {
        Vector2 end = (Vector2)transform.position;
        Play(end - direction.normalized, direction, 1f);
    }

    private void Update()
    {
        if (line != null && line.enabled && Time.time >= until) line.enabled = false;
        UpdateAfterImages();
        UpdateBurstRing();
    }

    private void OnDisable()
    {
        if (line != null) line.enabled = false;
        if (burstLine != null) burstLine.enabled = false;
        burstElapsed = -1f;
        if (afterImageObjects == null) return;
        for (int i = 0; i < afterImageObjects.Length; i++)
            if (afterImageObjects[i] != null) afterImageObjects[i].SetActive(false);
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
        if (line != null) Destroy(line.gameObject);
        if (burstObject != null) Destroy(burstObject);
        if (afterImageObjects == null) return;
        for (int i = 0; i < afterImageObjects.Length; i++)
            if (afterImageObjects[i] != null) Destroy(afterImageObjects[i]);
    }

    private void CreateAfterImagePool()
    {
        int count = Mathf.Clamp(afterImageCount, 3, 5);
        afterImageObjects = new GameObject[count];
        afterImageRenderers = new SpriteRenderer[count];
        afterImageRemaining = new float[count];

        for (int i = 0; i < count; i++)
        {
            GameObject afterImage = new GameObject("Dash AfterImage " + (i + 1));
            afterImageObjects[i] = afterImage;
            afterImageRenderers[i] = afterImage.AddComponent<SpriteRenderer>();
            afterImageRenderers[i].sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder - 2 : -2;
            afterImage.SetActive(false);
        }
    }

    private void CreateBurstRing()
    {
        burstObject = new GameObject("Dash Burst");
        burstLine = burstObject.AddComponent<LineRenderer>();
        burstLine.positionCount = 25;
        burstLine.useWorldSpace = true;
        burstLine.widthMultiplier = 0.045f;
        burstLine.numCapVertices = 2;
        burstLine.sharedMaterial = material;
        burstLine.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder + 1 : 1;
        burstLine.enabled = false;
    }

    private void SpawnAfterImages(Vector2 start, Vector2 direction, float distance)
    {
        if (afterImageObjects == null || sourceRenderer == null || sourceRenderer.sprite == null) return;

        Vector2 end = start + direction.normalized * distance;
        for (int i = 0; i < afterImageObjects.Length; i++)
        {
            int index = nextAfterImage;
            nextAfterImage = (nextAfterImage + 1) % afterImageObjects.Length;
            float positionT = (i + 1f) / (afterImageObjects.Length + 1f);
            GameObject afterImage = afterImageObjects[index];
            SpriteRenderer renderer = afterImageRenderers[index];
            afterImage.transform.position = Vector2.Lerp(start, end, positionT);
            afterImage.transform.rotation = sourceTransform != null ? sourceTransform.rotation : Quaternion.identity;
            afterImage.transform.localScale = sourceTransform != null ? sourceTransform.lossyScale : Vector3.one;
            renderer.sprite = sourceRenderer.sprite;
            renderer.flipX = sourceRenderer.flipX;
            renderer.flipY = sourceRenderer.flipY;
            renderer.color = new Color(dashColor.r, dashColor.g, dashColor.b, afterImageAlpha);
            afterImage.SetActive(true);
            afterImageRemaining[index] = afterImageLifetime;
        }
    }

    private void UpdateAfterImages()
    {
        if (afterImageObjects == null) return;

        for (int i = 0; i < afterImageObjects.Length; i++)
        {
            if (!afterImageObjects[i].activeSelf) continue;
            afterImageRemaining[i] -= Time.deltaTime;
            if (afterImageRemaining[i] <= 0f)
            {
                afterImageObjects[i].SetActive(false);
                continue;
            }

            float alpha = afterImageAlpha * Mathf.Clamp01(afterImageRemaining[i] / afterImageLifetime);
            Color color = afterImageRenderers[i].color;
            color.a = alpha;
            afterImageRenderers[i].color = color;
        }
    }

    private void StartBurst(Vector2 origin)
    {
        if (burstLine == null) return;
        burstOrigin = origin;
        burstElapsed = 0f;
        burstLine.enabled = true;
        UpdateBurstRing();
    }

    private void UpdateBurstRing()
    {
        if (burstLine == null || burstElapsed < 0f) return;

        burstElapsed += Time.deltaTime;
        if (burstElapsed >= burstDuration)
        {
            burstLine.enabled = false;
            burstElapsed = -1f;
            return;
        }

        float normalized = Mathf.Clamp01(burstElapsed / burstDuration);
        float radius = Mathf.Lerp(0.12f, 0.5f, normalized);
        Color color = new Color(dashColor.r, dashColor.g, dashColor.b, dashColor.a * (1f - normalized));
        burstLine.startColor = color;
        burstLine.endColor = color;

        for (int i = 0; i < burstLine.positionCount; i++)
        {
            float angle = i / (burstLine.positionCount - 1f) * Mathf.PI * 2f;
            burstLine.SetPosition(i, burstOrigin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
    }
}
