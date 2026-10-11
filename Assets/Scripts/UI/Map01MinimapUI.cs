using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Uses the authored map sprite and projects live actors onto it.</summary>
[DisallowMultipleComponent]
public sealed class Map01MinimapUI : MonoBehaviour
{
    private sealed class EnemyMarker
    {
        public EnemyFollow enemy;
        public Health health;
        public RectTransform rect;
    }

    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Vector2 worldCenter;
    [SerializeField] private Vector2 worldSize = Vector2.one;
    [SerializeField, Min(2f)] private float enemyMarkerSize = 7f;

    private readonly List<EnemyMarker> enemyMarkers = new List<EnemyMarker>();
    private float nextEnemyRefresh;
    private static Sprite markerSprite;
    private static Sprite runtimeMapSprite;
    private static Map01MinimapUI activeInstance;

    /// <summary>Replaces the initial Map 1 preview after the selected modular map is spawned.</summary>
    public static void ConfigureRuntimeMap(ModularMapDefinition definition)
    {
        if (definition == null || definition.walkableMask == null) return;
        Map01MinimapUI minimap = FindAnyObjectByType<Map01MinimapUI>();
        if (minimap == null || minimap.mapRect == null) return;
        Image image = minimap.mapRect.GetComponent<Image>();
        if (image == null) return;

        Texture2D mask = definition.walkableMask;
        runtimeMapSprite = Sprite.Create(mask, new Rect(0f, 0f, mask.width, mask.height),
            Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect);
        runtimeMapSprite.name = definition.zoneName + " Minimap";
        image.sprite = runtimeMapSprite;
        image.color = new Color(.42f, .72f, .82f, .98f);
        minimap.worldCenter = Vector2.zero;
        minimap.worldSize = definition.worldSize;
        float aspect = definition.worldSize.x / Mathf.Max(.001f, definition.worldSize.y);
        minimap.mapRect.sizeDelta = aspect >= 1f
            ? new Vector2(270f, 270f / aspect)
            : new Vector2(216f * aspect, 216f);
        minimap.RefreshEnemyMarkers();
    }

    private void Awake()
    {
        // Older scene rebuilds left several authored minimap roots in GameScene.
        // Keep exactly one at runtime even if a stale scene copy is opened.
        if (activeInstance != null && activeInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        activeInstance = this;
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        ConfigurePlayerMarker();
        RefreshEnemyMarkers();
    }

    private void OnDestroy()
    {
        if (activeInstance == this) activeInstance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoading()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") EnsureRuntimeMinimap();
    }

    private static void EnsureRuntimeMinimap()
    {
        if (SceneManager.GetActiveScene().name != "GameScene" || FindAnyObjectByType<Map01MinimapUI>() != null) return;
        Canvas canvas = FindAnyObjectByType<Canvas>();
        SpriteRenderer mapRenderer = FindMapRenderer();
        PlayerMovement runtimePlayer = FindAnyObjectByType<PlayerMovement>();
        if (canvas == null || mapRenderer == null || mapRenderer.sprite == null || runtimePlayer == null)
        {
            Debug.LogWarning("Map 01 minimap could not be created: Canvas, modular map art, or Player is missing.");
            return;
        }

        GameObject root = new GameObject("Map01 Minimap", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = rootRect.anchorMax = rootRect.pivot = Vector2.one;
        rootRect.anchoredPosition = new Vector2(-24f, -92f);
        rootRect.sizeDelta = new Vector2(286f, 228f);
        root.GetComponent<Image>().color = new Color(.01f, .025f, .055f, .94f);
        Outline outline = root.GetComponent<Outline>();
        outline.effectColor = new Color(.1f, .85f, 1f, .9f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject mapObject = new GameObject("Floor Map", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform runtimeMapRect = mapObject.GetComponent<RectTransform>();
        runtimeMapRect.SetParent(rootRect, false);
        runtimeMapRect.anchorMin = runtimeMapRect.anchorMax = runtimeMapRect.pivot = new Vector2(.5f, .5f);
        float aspect = mapRenderer.bounds.size.x / Mathf.Max(.001f, mapRenderer.bounds.size.y);
        runtimeMapRect.sizeDelta = new Vector2(270f, 270f / aspect);
        Image mapImage = mapObject.GetComponent<Image>();
        mapImage.sprite = mapRenderer.sprite;
        mapImage.color = new Color(.72f, .84f, .92f, .96f);
        mapImage.raycastTarget = false;

        Map01MinimapUI minimap = root.AddComponent<Map01MinimapUI>();
        minimap.mapRect = runtimeMapRect;
        minimap.playerMarker = CreateMarker("Player Marker", runtimeMapRect, 10f, new Color(.05f, .58f, 1f, 1f));
        minimap.player = runtimePlayer;
        minimap.worldCenter = mapRenderer.bounds.center;
        minimap.worldSize = mapRenderer.bounds.size;
        rootRect.SetAsLastSibling();
    }

    private void LateUpdate()
    {
        if (mapRect == null || playerMarker == null || player == null) return;
        PositionMarker(playerMarker, player.transform.position);
        if (Time.unscaledTime >= nextEnemyRefresh)
        {
            nextEnemyRefresh = Time.unscaledTime + 1f;
            RefreshEnemyMarkers();
        }
        for (int i = 0; i < enemyMarkers.Count; i++)
        {
            EnemyMarker marker = enemyMarkers[i];
            bool visible = marker.enemy != null && marker.enemy.gameObject.activeInHierarchy &&
                           (marker.health == null || !marker.health.IsDead);
            marker.rect.gameObject.SetActive(visible);
            if (visible) PositionMarker(marker.rect, marker.enemy.transform.position);
        }
    }

    private void ConfigurePlayerMarker()
    {
        if (playerMarker == null) return;
        Image image = playerMarker.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetMarkerSprite();
            image.color = new Color(.05f, .58f, 1f, 1f);
            image.raycastTarget = false;
        }
        playerMarker.localRotation = Quaternion.identity;
        playerMarker.sizeDelta = new Vector2(10f, 10f);
        playerMarker.SetAsLastSibling();
    }

    private void RefreshEnemyMarkers()
    {
        if (mapRect == null) return;
        EnemyFollow[] enemies = FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = enemyMarkers.Count - 1; i >= 0; i--)
        {
            if (enemyMarkers[i].enemy != null) continue;
            if (enemyMarkers[i].rect != null) Destroy(enemyMarkers[i].rect.gameObject);
            enemyMarkers.RemoveAt(i);
        }
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyFollow enemy = enemies[i];
            bool exists = false;
            for (int j = 0; j < enemyMarkers.Count; j++)
                if (enemyMarkers[j].enemy == enemy) { exists = true; break; }
            if (exists) continue;
            enemyMarkers.Add(new EnemyMarker
            {
                enemy = enemy,
                health = enemy.GetComponent<Health>(),
                rect = CreateMarker("Enemy Marker", mapRect, enemyMarkerSize, new Color(1f, .12f, .08f, 1f))
            });
        }
        if (playerMarker != null) playerMarker.SetAsLastSibling();
    }

    private void PositionMarker(RectTransform marker, Vector3 worldPosition)
    {
        Vector2 safeSize = new Vector2(Mathf.Max(.001f, worldSize.x), Mathf.Max(.001f, worldSize.y));
        Vector2 normalized = (Vector2)worldPosition - worldCenter;
        normalized = new Vector2(normalized.x / safeSize.x, normalized.y / safeSize.y) + Vector2.one * .5f;
        normalized.x = Mathf.Clamp01(normalized.x);
        normalized.y = Mathf.Clamp01(normalized.y);
        Rect rect = mapRect.rect;
        marker.anchoredPosition = new Vector2((normalized.x - .5f) * rect.width, (normalized.y - .5f) * rect.height);
    }

    private static RectTransform CreateMarker(string name, RectTransform parent, float size, Color color)
    {
        GameObject markerObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = markerObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(size, size);
        Image image = markerObject.GetComponent<Image>();
        image.sprite = GetMarkerSprite();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static Sprite GetMarkerSprite()
    {
        if (markerSprite != null) return markerSprite;
        const int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Runtime Minimap Dot",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[size * size];
        Vector2 center = Vector2.one * (size - 1) * .5f;
        float radius = size * .43f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            pixels[y * size + x] = Vector2.Distance(new Vector2(x, y), center) <= radius ? Color.white : Color.clear;
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        markerSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * .5f, size);
        markerSprite.name = "Runtime Minimap Dot";
        return markerSprite;
    }

    private static SpriteRenderer FindMapRenderer()
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i].name == "WalkableFloor" || renderers[i].name == "BaseMapArt") return renderers[i];
        return null;
    }
}
