#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SideViewMapsProductionBuilder
{
    private const string TemplateScene = "Assets/Scenes/Map01_RuinedEntrance_SideView.unity";
    private const float PixelsPerUnit = 36.2f;
    private const int SampleStepPixels = 7;
    private const byte AlphaThreshold = 70;

    private sealed class MapSpec
    {
        public int Number;
        public string Theme;
        public string ScenePath;
        public string[] Tiles;
    }

    private static readonly MapSpec[] Maps =
    {
        new MapSpec
        {
            Number = 1, Theme = "RuinedEntrance",
            ScenePath = "Assets/Scenes/Map01_RuinedEntrance_SideView.unity",
            Tiles = new[]
            {
                "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_Straight_Left_v03.png",
                "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_Straight_Middle_v03.png",
                "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_Straight_Right_v03.png"
            }
        },
        new MapSpec
        {
            Number = 2, Theme = "Grassland",
            ScenePath = "Assets/Scenes/Map02_Grassland_SideView.unity",
            Tiles = new[]
            {
                "Assets/ArtSource/Maps/Map02_ForgottenBarracks/SideViewProduction/Map02_Grassland_Left_v01.png",
                "Assets/ArtSource/Maps/Map02_ForgottenBarracks/SideViewProduction/Map02_Grassland_Middle_v01.png",
                "Assets/ArtSource/Maps/Map02_ForgottenBarracks/SideViewProduction/Map02_Grassland_Right_v01.png"
            }
        },
        new MapSpec
        {
            Number = 3, Theme = "Barbarian",
            ScenePath = "Assets/Scenes/Map03_Barbarian_SideView.unity",
            Tiles = new[]
            {
                "Assets/ArtSource/Maps/Map03_OvergrownWilds/SideViewProduction/Map03_Barbarian_Left_v01.png",
                "Assets/ArtSource/Maps/Map03_OvergrownWilds/SideViewProduction/Map03_Barbarian_Middle_v01.png",
                "Assets/ArtSource/Maps/Map03_OvergrownWilds/SideViewProduction/Map03_Barbarian_Right_v01.png"
            }
        },
        new MapSpec
        {
            Number = 4, Theme = "FrozenCrystal",
            ScenePath = "Assets/Scenes/Map04_FrozenCrystal_SideView.unity",
            Tiles = new[]
            {
                "Assets/ArtSource/Maps/Map04_EchoLibrary/SideViewProduction/Map04_FrozenCrystal_Left_v01.png",
                "Assets/ArtSource/Maps/Map04_EchoLibrary/SideViewProduction/Map04_FrozenCrystal_Middle_v01.png",
                "Assets/ArtSource/Maps/Map04_EchoLibrary/SideViewProduction/Map04_FrozenCrystal_Right_v01.png"
            }
        }
    };

    [MenuItem("Tools/ChronoDungeon/Maps/Rebuild Side-View Maps 01-04")]
    public static void RebuildAll()
    {
        foreach (MapSpec spec in Maps) Build(spec);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Maps[0].ScenePath, OpenSceneMode.Single);
        Debug.Log("ChronoDungeon: rebuilt side-view Maps 01-04 with twelve modular tiles and traced platform collision.");
    }

    [MenuItem("Tools/ChronoDungeon/Maps/Validate Side-View Maps 01-04")]
    public static void ValidateAll()
    {
        foreach (MapSpec spec in Maps)
        {
            Scene scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find(RootName(spec));
            if (root == null) throw new InvalidOperationException($"Map {spec.Number}: production root missing.");
            int tileCount = root.transform.Find("Presentation")?.GetComponentsInChildren<SpriteRenderer>(true).Length ?? 0;
            int edgeCount = root.transform.Find("Collision/WalkableEdges")?.GetComponentsInChildren<EdgeCollider2D>(true).Length ?? 0;
            int deathCount = root.transform.Find("Collision/DeathVolumes")?.GetComponentsInChildren<SideViewDeathVolume>(true).Length ?? 0;
            if (tileCount != 3 || edgeCount < 3 || deathCount != 1)
                throw new InvalidOperationException($"Map {spec.Number}: invalid build: tiles={tileCount}, edges={edgeCount}, death={deathCount}.");
            if (!scene.IsValid()) throw new InvalidOperationException($"Map {spec.Number}: scene failed to open.");
            Debug.Log($"ChronoDungeon: Map {spec.Number} validation passed ({tileCount} tiles, {edgeCount} separated collider paths). ");
        }
        EditorSceneManager.OpenScene(Maps[0].ScenePath, OpenSceneMode.Single);
    }

    private static void Build(MapSpec spec)
    {
        if (spec.Number > 1)
        {
            Scene template = EditorSceneManager.OpenScene(TemplateScene, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(template, spec.ScenePath, true);
        }

        Scene scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
        RemoveOldMapRoots();

        int groundLayer = RequireLayer("SideViewGround");
        int triggerLayer = RequireLayer("SideViewTrigger");
        GameObject root = new GameObject(RootName(spec));
        GameObject presentation = Child(root.transform, "Presentation");
        GameObject collision = Child(root.transform, "Collision");
        GameObject edges = Child(collision.transform, "WalkableEdges");

        for (int tileIndex = 0; tileIndex < spec.Tiles.Length; tileIndex++)
            BuildTile(spec, tileIndex, presentation.transform, edges.transform, groundLayer);

        BuildDeathVolume(Child(collision.transform, "DeathVolumes").transform, triggerLayer);
        BuildCameraBounds(root.transform, triggerLayer);
        PlacePlayerAndCamera(root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildTile(MapSpec spec, int tileIndex, Transform presentation, Transform edges, int groundLayer)
    {
        string path = spec.Tiles[tileIndex];
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing side-view texture: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.isReadable = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(candidate => candidate.rect.width * candidate.rect.height).FirstOrDefault();
        if (texture == null || sprite == null) throw new InvalidOperationException("Failed to import tile: " + path);

        int baselineRow = FindBaseline(texture);
        float tileX = (tileIndex - 1) * 60f;
        // Texture2D pixel rows are bottom-up. Offset the sprite so the detected
        // route baseline lands on world Y=0.
        float tileY = (texture.height * 0.5f - baselineRow) / PixelsPerUnit;

        GameObject visual = Child(presentation, $"Tile_{tileIndex + 1}");
        visual.transform.localPosition = new Vector3(tileX, tileY, 0f);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 0;

        List<List<Vector2>> paths = TraceWalkablePaths(texture, baselineRow, tileIndex);
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].Count < 2) continue;
            GameObject edgeObject = Child(edges, $"Tile_{tileIndex + 1}_Surface_{i + 1:00}");
            edgeObject.layer = groundLayer;
            edgeObject.transform.localPosition = new Vector3(tileX, tileY, 0f);
            EdgeCollider2D edge = edgeObject.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = 0.035f;
            edge.points = paths[i].ToArray();
        }
    }

    private static int FindBaseline(Texture2D texture)
    {
        Color32[] pixels = texture.GetPixels32();
        int bestRow = Mathf.RoundToInt(texture.height * 0.4f);
        int routeStartX = Mathf.Min(450, texture.width / 4);
        int sampleCount = Mathf.CeilToInt((texture.width - routeStartX) / 5f);

        // The entry/exit gate contains large opaque arches. A transition-count
        // heuristic mistakes their roof for the floor. The playable foundation
        // is the highest row that stays opaque across most of the route after
        // the gate, so use broad horizontal coverage instead.
        for (int y = 1; y < texture.height - 1; y++)
        {
            int score = 0;
            for (int x = routeStartX; x < texture.width; x += 5)
                if (Solid(pixels, texture.width, x, y)) score++;
            if (score >= sampleCount * 0.55f) bestRow = y;
        }
        return bestRow;
    }

    private static List<List<Vector2>> TraceWalkablePaths(Texture2D texture, int baselineRow, int tileIndex)
    {
        Color32[] pixels = texture.GetPixels32();
        var result = new List<List<Vector2>>();
        var current = new List<Vector2>();
        int previousY = int.MinValue;
        for (int x = 0; x < texture.width; x += SampleStepPixels)
        {
            int y = FindSurfaceY(pixels, texture.width, texture.height, x, baselineRow, tileIndex);
            bool split = y < 0 || (previousY != int.MinValue && Mathf.Abs(y - previousY) > 125);
            if (split)
            {
                FinishPath(current, result);
                current = new List<Vector2>();
                previousY = int.MinValue;
                continue;
            }

            Vector2 point = new Vector2(
                (x - texture.width * 0.5f) / PixelsPerUnit,
                (y - texture.height * 0.5f + 2f) / PixelsPerUnit);
            current.Add(point);
            previousY = y;
        }
        FinishPath(current, result);
        return result;
    }

    private static int FindSurfaceY(Color32[] pixels, int width, int height, int x, int baseline, int tileIndex)
    {
        // Gate art is decorative. Force its footprint to the route baseline so
        // the collider never climbs the arch roof or side columns.
        if ((tileIndex == 0 && x < 450) || (tileIndex == 2 && x > width - 450))
            return HasBodyBelow(pixels, width, height, x, baseline) ? baseline : -1;

        int start = Mathf.Max(18, baseline - 120);
        int end = Mathf.Min(height - 2, baseline + 230);
        for (int y = end; y >= start; y--)
        {
            if (!Solid(pixels, width, x, y)) continue;
            if (HasBodyBelow(pixels, width, height, x, y)) return y;
        }
        return -1;
    }

    private static bool HasBodyBelow(Color32[] pixels, int width, int height, int x, int y)
    {
        int hits = 0;
        for (int offset = 1; offset <= 16 && y - offset >= 0; offset++)
            if (Solid(pixels, width, x, y - offset)) hits++;
        return hits >= 10;
    }

    private static bool Solid(Color32[] pixels, int width, int x, int y) => pixels[y * width + x].a >= AlphaThreshold;

    private static void FinishPath(List<Vector2> source, List<List<Vector2>> output)
    {
        if (source.Count < 3) return;
        var smoothed = new List<Vector2>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            int from = Mathf.Max(0, i - 2);
            int to = Mathf.Min(source.Count - 1, i + 2);
            float averageY = 0f;
            for (int j = from; j <= to; j++) averageY += source[j].y;
            averageY /= to - from + 1;
            smoothed.Add(new Vector2(source[i].x, averageY));
        }
        output.Add(Simplify(smoothed, 0.035f));
    }

    private static List<Vector2> Simplify(List<Vector2> points, float tolerance)
    {
        if (points.Count <= 2) return points;
        var result = new List<Vector2> { points[0] };
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 a = result[result.Count - 1];
            Vector2 b = points[i];
            Vector2 c = points[i + 1];
            float area = Mathf.Abs(Vector3.Cross(b - a, c - b).z);
            if (area > tolerance) result.Add(b);
        }
        result.Add(points[points.Count - 1]);
        return result;
    }

    private static void BuildDeathVolume(Transform parent, int layer)
    {
        GameObject death = Child(parent, "VoidBelowMap");
        death.layer = layer;
        death.transform.localPosition = new Vector3(0f, -14f, 0f);
        BoxCollider2D box = death.AddComponent<BoxCollider2D>();
        box.size = new Vector2(184f, 5f);
        box.isTrigger = true;
        death.AddComponent<SideViewDeathVolume>();
    }

    private static void BuildCameraBounds(Transform root, int layer)
    {
        GameObject boundsObject = Child(root, "CameraBounds");
        boundsObject.layer = layer;
        BoxCollider2D bounds = boundsObject.AddComponent<BoxCollider2D>();
        bounds.size = new Vector2(184f, 38f);
        bounds.offset = new Vector2(0f, 1f);
        bounds.isTrigger = true;
    }

    private static void PlacePlayerAndCamera(Transform root)
    {
        GameObject player = null;
        try { player = GameObject.FindWithTag("Player"); } catch (UnityException) { }
        if (player == null) player = GameObject.Find("SideViewPlayer");
        if (player != null)
        {
            BoxCollider2D box = player.GetComponent<BoxCollider2D>();
            if (box != null) box.offset = new Vector2(box.offset.x, box.size.y * 0.5f + 0.35f);
            // Spawn just beyond the decorative entrance gate. Its doorway is
            // transparent art, not a platform, so spawning inside it makes the
            // grounded character look as if they are floating.
            player.transform.position = new Vector3(-75f, 0.03f, 0f);
            Physics2D.SyncTransforms();
            Collider2D collider = player.GetComponent<Collider2D>();
            if (collider != null)
                player.transform.position += Vector3.up * (0.03f - collider.bounds.min.y);
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.transform.position = new Vector3(-75f, 2.2f, camera.transform.position.z);
        }
    }

    private static void RemoveOldMapRoots()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            string n = root.name;
            if (n.Contains("SideViewProduction", StringComparison.OrdinalIgnoreCase) ||
                n.Equals("ManualWorld", StringComparison.OrdinalIgnoreCase) ||
                n.StartsWith("Map0", StringComparison.OrdinalIgnoreCase))
                UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static string RootName(MapSpec spec) => $"Map{spec.Number:00}_{spec.Theme}_SideViewProduction";

    private static int RequireLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0) throw new InvalidOperationException("Missing required layer: " + layerName);
        return layer;
    }

    private static GameObject Child(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }
}
#endif
