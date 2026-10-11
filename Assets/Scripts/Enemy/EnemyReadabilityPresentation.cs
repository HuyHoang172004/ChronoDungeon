using UnityEngine;
using UnityEngine.SceneManagement;

/// Removes legacy enemy tinting and supplies a compact, segmented world-space HP bar.
[DisallowMultipleComponent]
public sealed class EnemyReadabilityPresentation : MonoBehaviour
{
    private const int SegmentCount = 6;
    private static Sprite pixelSprite;
    private Health health;
    private Transform barRoot;
    private SpriteRenderer[] segments;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoading()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") InstallForGameScene();
    }

    private static void InstallForGameScene()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;
        foreach (EnemyFollow enemy in FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include))
        {
            if (enemy == null) continue;
            if (enemy.GetComponent<EnemyAnimationDriver>() == null)
                enemy.gameObject.AddComponent<EnemyAnimationDriver>();
            if (enemy.GetComponent<EnemyReadabilityPresentation>() == null)
                enemy.gameObject.AddComponent<EnemyReadabilityPresentation>();
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();

        DisableLegacyPlaceholder("Ember Body", "Brow Left", "Brow Right", "Eye Left", "Eye Right",
            "Mouth", "Fang Left", "Fang Right", "Stylized Enemy Visual", "ChronoVisualAccent");

        // Retire the old single green rectangle.
        foreach (HealthBar legacyBar in GetComponentsInChildren<HealthBar>(true))
            legacyBar.gameObject.SetActive(false);

        // The red/orange enemy appearance came from an inherited renderer tint.
        // Restore the authored Ruin Husk sprite colours instead.
        SpriteRenderer authoredBody = GetComponent<SpriteRenderer>();
        if (authoredBody != null)
        {
            authoredBody.color = Color.white;
            authoredBody.sortingOrder = Mathf.Max(authoredBody.sortingOrder, 42);
        }

        CombatFeedback combatFeedback = GetComponent<CombatFeedback>();
        if (combatFeedback != null) combatFeedback.RecaptureBaseColors();
        DamageFeedback damageFeedback = GetComponent<DamageFeedback>();
        if (damageFeedback != null) damageFeedback.RecaptureBaseColors();

        BuildBar();
    }

    private void DisableLegacyPlaceholder(params string[] names)
    {
        Transform[] descendants = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < descendants.Length; i++)
        {
            if (descendants[i] == transform) continue;
            for (int n = 0; n < names.Length; n++)
                if (descendants[i].name.Equals(names[n], System.StringComparison.OrdinalIgnoreCase))
                {
                    descendants[i].gameObject.SetActive(false);
                    break;
                }
        }
    }

    private void OnEnable()
    {
        if (health != null) health.Changed += Refresh;
    }

    private void Start()
    {
        if (health == null) health = GetComponent<Health>();
        if (health != null)
        {
            health.Changed -= Refresh;
            health.Changed += Refresh;
            Refresh(health);
        }
    }

    private void OnDisable()
    {
        if (health != null) health.Changed -= Refresh;
    }

    private void LateUpdate()
    {
        if (barRoot == null) return;
        Vector3 scale = transform.lossyScale;
        barRoot.localScale = new Vector3(
            1f / Mathf.Max(.001f, Mathf.Abs(scale.x)),
            1f / Mathf.Max(.001f, Mathf.Abs(scale.y)), 1f);
        barRoot.rotation = Quaternion.identity;
    }

    private void BuildBar()
    {
        Transform old = transform.Find("Segmented Health Bar");
        if (old != null) Destroy(old.gameObject);

        barRoot = new GameObject("Segmented Health Bar").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = new Vector3(0f, 1.05f, 0f);

        CreateBlock("Border", Vector3.zero, new Vector2(1.34f, .19f), new Color(.015f, .02f, .03f, .98f), 198);
        CreateBlock("Track", Vector3.zero, new Vector2(1.25f, .105f), new Color(.08f, .1f, .12f, 1f), 199);

        segments = new SpriteRenderer[SegmentCount];
        const float segmentWidth = .18f;
        const float gap = .025f;
        float totalWidth = SegmentCount * segmentWidth + (SegmentCount - 1) * gap;
        for (int i = 0; i < SegmentCount; i++)
        {
            float x = -totalWidth * .5f + segmentWidth * .5f + i * (segmentWidth + gap);
            segments[i] = CreateBlock("HP Segment " + (i + 1), new Vector3(x, 0f, 0f),
                new Vector2(segmentWidth, .07f), new Color(.22f, .92f, .82f, 1f), 200);
        }
    }

    private SpriteRenderer CreateBlock(string objectName, Vector3 position, Vector2 size, Color color, int order)
    {
        GameObject block = new GameObject(objectName);
        block.transform.SetParent(barRoot, false);
        block.transform.localPosition = position;
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPixelSprite();
        renderer.color = color;
        renderer.sortingOrder = order;
        return renderer;
    }

    private void Refresh(Health value)
    {
        if (segments == null || value == null) return;
        float normalized = value.maxHealth <= 0f ? 0f : Mathf.Clamp01(value.currentHealth / value.maxHealth);
        int visible = Mathf.CeilToInt(normalized * SegmentCount);
        Color segmentColor = normalized > .5f
            ? new Color(.2f, .9f, .78f, 1f)
            : normalized > .25f ? new Color(1f, .72f, .18f, 1f) : new Color(1f, .3f, .2f, 1f);
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].enabled = i < visible && !value.IsDead;
            segments[i].color = segmentColor;
        }
        if (barRoot != null) barRoot.gameObject.SetActive(!value.IsDead);
    }

    private static Sprite GetPixelSprite()
    {
        if (pixelSprite != null) return pixelSprite;
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = "Enemy Health Bar Pixel",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        texture.Apply(false, true);
        pixelSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(.5f, .5f), 2f);
        pixelSprite.name = "Enemy Health Bar Sprite";
        pixelSprite.hideFlags = HideFlags.HideAndDontSave;
        return pixelSprite;
    }
}
