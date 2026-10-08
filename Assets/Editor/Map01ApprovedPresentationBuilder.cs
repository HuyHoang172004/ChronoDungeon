#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Rebuilds the authored Map 01 presentation hierarchy from the approved Pack 03 art.
/// The collision guide remains an editor reference only; generated EdgeCollider2D contours
/// follow its green walkable mask and are deliberately easy to refine by hand afterwards.
/// </summary>
public static class Map01ApprovedPresentationBuilder
{
    private const string MapRootPath = "ManualWorld/Map01_RuinedEntrance";
    private const string MasterPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/Master/CD_Map01_Master_Base_Composition_v01.png";
    private const string ForegroundPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/Foreground/CD_Map01_Foreground_Overlay_v01.png";
    private const string GuidePath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/Collision/CD_Map01_CollisionGuide_v01.png";
    // A broad authored map needs generous room scale on mobile. 30 PPU makes the
    // continuous world substantially larger without scaling Player transforms.
    private const float MapPixelsPerUnit = 30f;
    private const float MapCenterY = -3.5f;
    private const int GuideCellPixels = 8;

    [MenuItem("Tools/ChronoDungeon/Maps/Rebuild Map01 Approved Presentation")]
    public static void Rebuild()
    {
        GameObject mapRoot = GameObject.Find(MapRootPath);
        if (mapRoot == null) throw new InvalidOperationException("Map01_RuinedEntrance is not loaded.");

        Sprite master = PrepareSingleSprite(MasterPath);
        Sprite foreground = PrepareSingleSprite(ForegroundPath);
        Sprite guide = PrepareSingleSprite(GuidePath);

        GameObject backgroundRoot = GetOrCreateChild(mapRoot.transform, "BackgroundArt");
        SpriteRenderer masterRenderer = GetOrCreateSpriteRenderer(GetOrCreateChild(backgroundRoot.transform, "BaseMapArt"));
        ConfigureMapRenderer(masterRenderer, master, -100, false);

        GameObject foregroundRoot = GetOrCreateChild(mapRoot.transform, "ForegroundArt");
        SpriteRenderer foregroundRenderer = GetOrCreateSpriteRenderer(GetOrCreateChild(foregroundRoot.transform, "FrontWallsAndOccluders"));
        ConfigureMapRenderer(foregroundRenderer, foreground, 40, false);

        GameObject guideRoot = GetOrCreateChild(mapRoot.transform, "CollisionGuide_EditorOnly");
        SpriteRenderer guideRenderer = GetOrCreateSpriteRenderer(guideRoot);
        ConfigureMapRenderer(guideRenderer, guide, 500, true);
        guideRoot.SetActive(false);

        GetOrCreateChild(mapRoot.transform, "Props");
        GetOrCreateChild(mapRoot.transform, "Enemies");
        GetOrCreateChild(mapRoot.transform, "Loot");
        GetOrCreateChild(mapRoot.transform, "NPC");

        RebuildCollision(mapRoot.transform, AssetDatabase.LoadAssetAtPath<Texture2D>(GuidePath), master);
        FitCameraBounds(mapRoot.transform, master);
        NormalizePlayerPresentationScale();
        PlaceOpeningSpawn(mapRoot.transform);
        PlaceOpeningCamera(master);
        RepositionLegacyTestObjects();

        Transform legacy = mapRoot.transform.Find("GeneratedPresentation");
        if (legacy != null) legacy.gameObject.SetActive(false);
        Transform legacyMaster = mapRoot.transform.Find("Map01_MasterArt");
        if (legacyMaster != null) legacyMaster.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(mapRoot.scene);
        AssetDatabase.SaveAssets();
        Debug.Log("ChronoDungeon: rebuilt Map 01 approved master/foreground/collision presentation.");
    }

    private static Sprite PrepareSingleSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing texture importer: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = MapPixelsPerUnit;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.isReadable = path == GuidePath;
        importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("Could not import sprite: " + path);
        return sprite;
    }

    private static GameObject GetOrCreateChild(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        if (found != null) return found.gameObject;
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static SpriteRenderer GetOrCreateSpriteRenderer(GameObject target)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        return renderer != null ? renderer : target.AddComponent<SpriteRenderer>();
    }

    private static void ConfigureMapRenderer(SpriteRenderer renderer, Sprite sprite, int sortingOrder, bool editorOnly)
    {
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.color = Color.white;
        renderer.transform.localPosition = new Vector3(0f, MapCenterY, 0f);
        renderer.transform.localRotation = Quaternion.identity;
        renderer.transform.localScale = Vector3.one;
        renderer.gameObject.tag = "Untagged";
        if (editorOnly) renderer.enabled = false;
    }

    private static void NormalizePlayerPresentationScale()
    {
        Transform playerVisual = GameObject.Find("Player/PlayerVisual")?.transform;
        if (playerVisual == null) throw new InvalidOperationException("Player/PlayerVisual was not found.");
        // 300 PPU source sheets at unit scale align with the 40 PPU map without changing
        // Player root physics, collider, movement or authored sprite baseline.
        playerVisual.localScale = Vector3.one;
        playerVisual.localPosition = Vector3.zero;
    }

    private static void PlaceOpeningSpawn(Transform mapRoot)
    {
        // Centre of the green Awakening Chamber in the approved collision guide.
        // Player physics is moved only because this is a rebuilt map coordinate space.
        Vector3 spawn = PixelToWorld(125f, 190f, 2172f, 724f);
        GameObject player = GameObject.Find("Player");
        if (player != null) player.transform.position = spawn;
        Transform marker = mapRoot.Find("Gameplay/PlayerSpawn");
        if (marker != null) marker.position = spawn;
    }

    private static void PlaceOpeningCamera(Sprite master)
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        float halfWidth = camera.orthographicSize * camera.aspect;
        float left = -master.bounds.extents.x;
        Vector3 spawn = PixelToWorld(125f, 190f, 2172f, 724f);
        camera.transform.position = new Vector3(Mathf.Max(left + halfWidth, spawn.x), spawn.y, camera.transform.position.z);
    }

    private static Vector3 PixelToWorld(float x, float yFromTop, float width, float height)
    {
        return new Vector3((x - width * 0.5f) / MapPixelsPerUnit,
            (height * 0.5f - yFromTop) / MapPixelsPerUnit + MapCenterY,
            0f);
    }

    private static void RepositionLegacyTestObjects()
    {
        // These are old vertical-slice test objects, not authored Map 01 placement.
        // Keep the first combat test reachable while the proper Ruin Husk roster is integrated.
        GameObject bearer = GameObject.Find("Map01_WeaponBearer");
        if (bearer != null) bearer.transform.position = new Vector3(-27.4f, 2.05f, 0f);

        // Design lock starts Map 01 unarmed and awards Chrono Blade from the early Husk.
        // The old Ember Staff test pickup belongs to a later weapon test and must not float
        // in the rebuilt entrance map.
        GameObject emberPickup = GameObject.Find("EmberStaffPickup");
        if (emberPickup != null) emberPickup.SetActive(false);
    }

    private static void FitCameraBounds(Transform mapRoot, Sprite master)
    {
        GameObject boundsRoot = GetOrCreateChild(mapRoot, "CameraBounds");
        BoxCollider2D bounds = boundsRoot.GetComponent<BoxCollider2D>();
        if (bounds == null) bounds = boundsRoot.AddComponent<BoxCollider2D>();
        boundsRoot.transform.localPosition = new Vector3(0f, MapCenterY, 0f);
        boundsRoot.transform.localRotation = Quaternion.identity;
        bounds.size = master.bounds.size;
        // Camera bounds must never block Player movement; Map01/Collision owns gameplay walls.
        bounds.isTrigger = true;

        Transform legacyBounds = boundsRoot.transform.Find("Map01CameraBounds");
        if (legacyBounds != null)
        {
            legacyBounds.localPosition = Vector3.zero;
            BoxCollider2D nested = legacyBounds.GetComponent<BoxCollider2D>();
            if (nested != null)
            {
                nested.size = master.bounds.size;
                nested.isTrigger = true;
            }
        }
    }

    private static void RebuildCollision(Transform mapRoot, Texture2D guideTexture, Sprite master)
    {
        if (guideTexture == null) throw new InvalidOperationException("Collision guide texture was not found.");

        Transform oldCollision = mapRoot.Find("Collision");
        if (oldCollision != null)
        {
            oldCollision.name = "LegacyCollisionBackup";
            oldCollision.gameObject.SetActive(false);
        }

        GameObject collisionRoot = GetOrCreateChild(mapRoot, "Collision");
        collisionRoot.SetActive(true);
        collisionRoot.transform.localPosition = new Vector3(0f, MapCenterY, 0f);
        collisionRoot.transform.localRotation = Quaternion.identity;

        for (int i = collisionRoot.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(collisionRoot.transform.GetChild(i).gameObject);

        bool[,] walkable = BuildWalkableMask(guideTexture, GuideCellPixels);
        List<List<Vector2Int>> loops = TraceBoundaryLoops(walkable);
        float cellWorld = GuideCellPixels / MapPixelsPerUnit;
        int width = walkable.GetLength(0);
        int height = walkable.GetLength(1);

        int created = 0;
        foreach (List<Vector2Int> loop in loops)
        {
            List<Vector2Int> simplified = SimplifyCollinear(loop);
            if (simplified.Count < 3) continue;
            GameObject edgeObject = new GameObject("WalkableBoundary_" + created.ToString("00"));
            edgeObject.transform.SetParent(collisionRoot.transform, false);
            EdgeCollider2D edge = edgeObject.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = 0.045f;
            Vector2[] points = new Vector2[simplified.Count + 1];
            for (int i = 0; i < simplified.Count; i++)
            {
                Vector2Int point = simplified[i];
                points[i] = new Vector2((point.x - width * 0.5f) * cellWorld, (point.y - height * 0.5f) * cellWorld);
            }
            points[points.Length - 1] = points[0];
            edge.points = points;
            created++;
        }

        if (created == 0) throw new InvalidOperationException("No collision boundaries were generated from the Map 01 collision guide.");
        CreateSafetyPerimeter(collisionRoot.transform, master.bounds.size);
    }

    private static void CreateSafetyPerimeter(Transform collisionRoot, Vector2 mapSize)
    {
        float halfWidth = mapSize.x * 0.5f;
        float halfHeight = mapSize.y * 0.5f;
        float padding = 0.10f;
        Vector2[][] sides =
        {
            new[] { new Vector2(-halfWidth - padding, -halfHeight - padding), new Vector2(halfWidth + padding, -halfHeight - padding) },
            new[] { new Vector2(halfWidth + padding, -halfHeight - padding), new Vector2(halfWidth + padding, halfHeight + padding) },
            new[] { new Vector2(halfWidth + padding, halfHeight + padding), new Vector2(-halfWidth - padding, halfHeight + padding) },
            new[] { new Vector2(-halfWidth - padding, halfHeight + padding), new Vector2(-halfWidth - padding, -halfHeight - padding) }
        };
        for (int i = 0; i < sides.Length; i++)
        {
            GameObject sideObject = new GameObject("MapPerimeter_" + i.ToString("00"));
            sideObject.transform.SetParent(collisionRoot, false);
            EdgeCollider2D edge = sideObject.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = 0.08f;
            edge.points = sides[i];
        }
    }

    private static bool[,] BuildWalkableMask(Texture2D guide, int cellPixels)
    {
        int width = Mathf.CeilToInt(guide.width / (float)cellPixels);
        int height = Mathf.CeilToInt(guide.height / (float)cellPixels);
        bool[,] mask = new bool[width, height];
        Color32[] pixels = guide.GetPixels32();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int px = Mathf.Min(guide.width - 1, x * cellPixels + cellPixels / 2);
            int py = Mathf.Min(guide.height - 1, y * cellPixels + cellPixels / 2);
            Color32 c = pixels[py * guide.width + px];
            // Guide green is the approved walkable language. The bright red wall trace is
            // deliberately excluded so collision is generated on the playable side.
            mask[x, y] = c.g > c.r * 1.18f && c.g > c.b * 1.15f && c.g > 70;
        }
        return mask;
    }

    private static List<List<Vector2Int>> TraceBoundaryLoops(bool[,] mask)
    {
        int width = mask.GetLength(0);
        int height = mask.GetLength(1);
        List<Segment> segments = new List<Segment>();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (!mask[x, y]) continue;
            if (y == 0 || !mask[x, y - 1]) segments.Add(new Segment(new Vector2Int(x + 1, y), new Vector2Int(x, y)));
            if (x == width - 1 || !mask[x + 1, y]) segments.Add(new Segment(new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y)));
            if (y == height - 1 || !mask[x, y + 1]) segments.Add(new Segment(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1)));
            if (x == 0 || !mask[x - 1, y]) segments.Add(new Segment(new Vector2Int(x, y), new Vector2Int(x, y + 1)));
        }

        Dictionary<Vector2Int, List<int>> touching = new Dictionary<Vector2Int, List<int>>();
        for (int i = 0; i < segments.Count; i++)
        {
            AddTouch(touching, segments[i].a, i);
            AddTouch(touching, segments[i].b, i);
        }

        bool[] used = new bool[segments.Count];
        List<List<Vector2Int>> loops = new List<List<Vector2Int>>();
        for (int seed = 0; seed < segments.Count; seed++)
        {
            if (used[seed]) continue;
            List<Vector2Int> loop = new List<Vector2Int>();
            Segment first = segments[seed];
            used[seed] = true;
            loop.Add(first.a);
            Vector2Int current = first.b;
            int guard = segments.Count + 1;
            while (guard-- > 0)
            {
                loop.Add(current);
                if (current == loop[0]) break;
                if (!touching.TryGetValue(current, out List<int> options)) break;
                int next = -1;
                for (int i = 0; i < options.Count; i++) if (!used[options[i]]) { next = options[i]; break; }
                if (next < 0) break;
                used[next] = true;
                Segment segment = segments[next];
                current = segment.a == current ? segment.b : segment.a;
            }
            if (loop.Count > 3 && loop[loop.Count - 1] == loop[0])
            {
                loop.RemoveAt(loop.Count - 1);
                loops.Add(loop);
            }
        }
        return loops;
    }

    private static void AddTouch(Dictionary<Vector2Int, List<int>> touching, Vector2Int point, int segment)
    {
        if (!touching.TryGetValue(point, out List<int> list))
        {
            list = new List<int>();
            touching.Add(point, list);
        }
        list.Add(segment);
    }

    private static List<Vector2Int> SimplifyCollinear(List<Vector2Int> points)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int previous = points[(i - 1 + points.Count) % points.Count];
            Vector2Int current = points[i];
            Vector2Int next = points[(i + 1) % points.Count];
            Vector2Int a = current - previous;
            Vector2Int b = next - current;
            if (a.x * b.y - a.y * b.x != 0) result.Add(current);
        }
        return result;
    }

    private readonly struct Segment
    {
        public readonly Vector2Int a;
        public readonly Vector2Int b;
        public Segment(Vector2Int start, Vector2Int end) { a = start; b = end; }
    }
}
#endif
