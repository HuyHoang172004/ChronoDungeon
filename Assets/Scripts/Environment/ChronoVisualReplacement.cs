using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime art-direction layer for the authored dungeon. It keeps gameplay components and
/// colliders untouched while replacing the flat prototype read with a small, consistent
/// silhouette language: cyan temporal rings, warm enemy accents, and a distinct boss crest.
/// The textures are generated once and reused, so this does not allocate per frame.
/// </summary>
public sealed class ChronoVisualReplacement : MonoBehaviour
{
    private const string MarkerName = "ChronoVisualAccent";
    private static Sprite ringSprite;
    private static Sprite diamondSprite;
    private readonly List<Accent> accents = new List<Accent>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneIsGameplay() && FindAnyObjectByType<ChronoVisualReplacement>() == null)
        {
            GameObject visuals = new GameObject("Chrono Visual Direction");
            visuals.AddComponent<ChronoVisualReplacement>();
        }
    }

    private static bool SceneIsGameplay()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene";
    }

    private void Awake()
    {
        BuildSprites();
    }

    private void Start()
    {
        RefreshVisuals();
    }

    private void Update()
    {
        // Room transitions and ghost spawns can add renderers after Start.
        if (Time.frameCount % 30 == 0)
        {
            RefreshVisuals();
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.6f) * 0.06f;
        for (int i = accents.Count - 1; i >= 0; i--)
        {
            Accent accent = accents[i];
            if (accent.renderer == null)
            {
                accents.RemoveAt(i);
                continue;
            }

            accent.renderer.transform.localScale = accent.baseScale * pulse;
            Color color = accent.baseColor;
            color.a = accent.baseAlpha * (0.84f + Mathf.Sin(Time.unscaledTime * 3.1f + i) * 0.12f);
            accent.renderer.color = color;
        }
    }

    private void RefreshVisuals()
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer source = renderers[i];
            if (source == null || source.gameObject.name == MarkerName || source.transform.Find(MarkerName) != null)
            {
                continue;
            }

            GameObject root = source.gameObject;
            Color color;
            Sprite sprite;
            float size;
            int order;
            if (root.GetComponent<PlayerMovement>() != null)
            {
                color = new Color(0.22f, 0.95f, 1f);
                sprite = ringSprite;
                size = 1.38f;
                order = source.sortingOrder - 1;
            }
            else if (root.GetComponent<GhostPlayback>() != null)
            {
                color = new Color(0.68f, 0.52f, 1f);
                sprite = ringSprite;
                size = 1.48f;
                order = source.sortingOrder - 1;
            }
            else if (root.GetComponent<ChronoGuardian>() != null)
            {
                color = new Color(0.24f, 0.88f, 1f);
                sprite = diamondSprite;
                size = 2.05f;
                order = source.sortingOrder + 1;
            }
            else if (root.GetComponent<EnemyArcher>() != null)
            {
                color = new Color(1f, 0.35f, 0.08f);
                sprite = diamondSprite;
                size = 1.25f;
                order = source.sortingOrder - 1;
            }
            else if (root.GetComponent<EnemyKnight>() != null)
            {
                color = new Color(0.72f, 0.36f, 1f);
                sprite = diamondSprite;
                size = 1.34f;
                order = source.sortingOrder - 1;
            }
            else if (root.GetComponent<EnemyFollow>() != null)
            {
                color = new Color(1f, 0.24f, 0.08f);
                sprite = ringSprite;
                size = 1.22f;
                order = source.sortingOrder - 1;
            }
            else if (root.GetComponentInParent<PressureSwitch>() != null)
            {
                color = new Color(0.32f, 1f, 0.84f);
                sprite = diamondSprite;
                size = 1.34f;
                order = source.sortingOrder + 1;
            }
            else if (root.GetComponentInParent<Door>() != null)
            {
                color = new Color(1f, 0.58f, 0.18f);
                sprite = ringSprite;
                size = 1.35f;
                order = source.sortingOrder + 1;
            }
            else
            {
                ApplyDungeonPalette(source);
                continue;
            }

            GameObject accentObject = new GameObject(MarkerName);
            accentObject.transform.SetParent(source.transform, false);
            accentObject.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            accentObject.transform.localScale = Vector3.one * size;
            SpriteRenderer accentRenderer = accentObject.AddComponent<SpriteRenderer>();
            accentRenderer.sprite = sprite;
            accentRenderer.color = color;
            accentRenderer.sortingLayerID = source.sortingLayerID;
            accentRenderer.sortingOrder = order;
            accents.Add(new Accent(accentRenderer, Vector3.one * size, color));
        }
    }

    private static void ApplyDungeonPalette(SpriteRenderer source)
    {
        string name = source.gameObject.name;
        if (name.Contains("Background") || name.Contains("Floor"))
        {
            source.color = new Color(0.08f, 0.12f, 0.18f, source.color.a);
        }
        else if (name.Contains("Wall") || name.Contains("Border") || name.Contains("Frame"))
        {
            source.color = new Color(0.16f, 0.2f, 0.28f, source.color.a);
        }
        else if (name.Contains("Column") || name.Contains("Grate") || name.Contains("Passage"))
        {
            source.color = new Color(0.25f, 0.29f, 0.38f, source.color.a);
        }
    }

    private static void BuildSprites()
    {
        if (ringSprite != null)
        {
            return;
        }

        ringSprite = Sprite.Create(CreateRingTexture(), new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 32f);
        ringSprite.name = "ChronoTemporalRing";
        diamondSprite = Sprite.Create(CreateDiamondTexture(), new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 32f);
        diamondSprite.name = "ChronoTemporalDiamond";
    }

    private static Texture2D CreateRingTexture()
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - 25f) / 3.5f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.55f));
            }
        }
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateDiamondTexture()
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distance = Mathf.Abs(x - 31.5f) + Mathf.Abs(y - 31.5f);
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - 24f) / 3f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.48f));
            }
        }
        texture.Apply();
        return texture;
    }

    private sealed class Accent
    {
        public readonly SpriteRenderer renderer;
        public readonly Vector3 baseScale;
        public readonly Color baseColor;
        public readonly float baseAlpha;

        public Accent(SpriteRenderer renderer, Vector3 baseScale, Color baseColor)
        {
            this.renderer = renderer;
            this.baseScale = baseScale;
            this.baseColor = baseColor;
            baseAlpha = baseColor.a;
        }
    }
}
