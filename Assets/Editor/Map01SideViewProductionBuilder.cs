#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds Map 01 from side-view-only art and explicit gameplay geometry.
/// It intentionally never reads the legacy top-down master or collision mask.
/// Visual replacement therefore cannot silently alter player collision.
/// </summary>
public static class Map01SideViewProductionBuilder
{
    private const string ScenePath = "Assets/Scenes/Map01_RuinedEntrance_SideView.unity";
    private const string RootName = "Map01_SideViewProduction";
    private const string BackgroundPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_SideView_Background_v01.png";
    private const string TerrainLeftPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_Terrain_Left_v02.png";
    private const string TerrainMiddlePath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_Terrain_Middle_v02.png";
    private const string TerrainRightPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/SideViewProduction/CD_Map01_Terrain_Right_v02.png";
    private const float BackgroundPixelsPerUnit = 12f;
    private const float TerrainPixelsPerUnit = 36.2f;

    private readonly struct SolidSpec
    {
        public readonly string name;
        public readonly Vector2 center;
        public readonly Vector2 size;

        public SolidSpec(string name, float x, float y, float width, float height)
        {
            this.name = name;
            center = new Vector2(x, y);
            size = new Vector2(width, height);
        }
    }

    // Collision is deliberately authored as simple, reviewable geometry. Coordinates
    // are in world units and grouped by the five beats from MAP01_SIDE_VIEW_BLUEPRINT.
    private static readonly SolidSpec[] Solids =
    {
        new("01_EntranceUpper", -84f, 2.3f, 12f, 2.2f),
        new("01_LeftAqueduct", -57f, -1.7f, 54f, 2.2f),
        new("01_LeftBoundary", -90f, 4.5f, 1.6f, 14f),

        new("02_CentralAqueduct", 0f, -1.85f, 60f, 2.5f),

        new("03_BossApproach", 52f, -2.0f, 44f, 2.8f),
        new("03_BossStairLow", 75f, -0.7f, 4f, 1.2f),
        new("03_BossStairMid", 78f, 0.5f, 4f, 1.2f),
        new("03_BossPlatform", 84f, 1.9f, 12f, 2.2f),
        new("03_RightBoundary", 90f, 5.0f, 1.8f, 15f),
    };

    private static readonly SolidSpec[] OneWays =
    {
        new("02_Lift_01", 3f, 2.5f, 6f, 0.45f),
        new("02_Lift_02", 3f, 6.5f, 6f, 0.45f),
        new("02_Lift_03", 3f, 10.5f, 6f, 0.45f),
    };

    [MenuItem("Tools/ChronoDungeon/Maps/Rebuild Map01 Side-View Production")]
    public static void Rebuild()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        int groundLayer = RequireLayer("SideViewGround");
        int oneWayLayer = RequireLayer("SideViewOneWay");
        int triggerLayer = RequireLayer("SideViewTrigger");

        GameObject previous = GameObject.Find(RootName);
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);

        GameObject root = new GameObject(RootName);
        BuildPresentation(root.transform);

        GameObject collision = Child(root.transform, "Collision");
        BuildSolids(Child(collision.transform, "SolidGround").transform, Solids, groundLayer, false);
        BuildSolids(Child(collision.transform, "OneWayPlatforms").transform, OneWays, oneWayLayer, true);
        BuildDeathVolume(Child(collision.transform, "DeathVolumes").transform, triggerLayer);

        GameObject beats = Child(root.transform, "MapBeats");
        CreateBeat(beats.transform, "01_RampartBreach", -96f, -55f);
        CreateBeat(beats.transform, "02_BrokenAqueduct", -55f, -24f);
        CreateBeat(beats.transform, "03_SunkenMusteringYard", -24f, 22f);
        CreateBeat(beats.transform, "04_RunicLiftworks", 22f, 42f);
        CreateBeat(beats.transform, "05_GatebreakerKeep", 42f, 96f);

        BuildGameplayMarkers(root.transform);
        BuildCameraBounds(root.transform, triggerLayer);
        PlacePlayerAndCamera();
        DisableLegacyTopDownPresentation();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("ChronoDungeon: rebuilt Map 01 with side-view-only art and explicit modular collision.");
    }

    [MenuItem("Tools/ChronoDungeon/Maps/Validate Map01 Side-View Production")]
    public static void Validate()
    {
        GameObject root = GameObject.Find(RootName);
        if (root == null) throw new InvalidOperationException("Run Rebuild Map01 Side-View Production first.");
        int solids = root.transform.Find("Collision/SolidGround")?.GetComponentsInChildren<BoxCollider2D>().Length ?? 0;
        int oneWays = root.transform.Find("Collision/OneWayPlatforms")?.GetComponentsInChildren<BoxCollider2D>().Length ?? 0;
        if (solids != Solids.Length || oneWays != OneWays.Length)
            throw new InvalidOperationException($"Map01 collider count mismatch. Solid {solids}/{Solids.Length}, one-way {oneWays}/{OneWays.Length}.");
        Transform terrainRoot = root.transform.Find("Presentation/GameplayTerrain");
        SpriteRenderer[] terrainTiles = terrainRoot != null
            ? terrainRoot.GetComponentsInChildren<SpriteRenderer>(true)
            : Array.Empty<SpriteRenderer>();
        if (terrainTiles.Length != 3 || terrainTiles.Any(tile => tile.sprite == null || tile.sprite.rect.width < 2000f))
            throw new InvalidOperationException("Map01 must contain three full-resolution terrain tiles.");
        Debug.Log($"ChronoDungeon: Map01 side-view validation passed ({solids} solids, {oneWays} one-way platforms).");
    }

    private static void BuildPresentation(Transform root)
    {
        GameObject presentation = Child(root, "Presentation");
        CreateArtLayer(presentation.transform, "Background", BackgroundPath, -100, BackgroundPixelsPerUnit, Vector3.zero);
        GameObject terrain = Child(presentation.transform, "GameplayTerrain");
        CreateArtLayer(terrain.transform, "Terrain_Left", TerrainLeftPath, 0, TerrainPixelsPerUnit, new Vector3(-60f, 1.78f, 0f));
        CreateArtLayer(terrain.transform, "Terrain_Middle", TerrainMiddlePath, 0, TerrainPixelsPerUnit, new Vector3(0f, 1.28f, 0f));
        CreateArtLayer(terrain.transform, "Terrain_Right", TerrainRightPath, 0, TerrainPixelsPerUnit, new Vector3(60f, 2.66f, 0f));
        Child(presentation.transform, "Foreground");
    }

    private static void CreateArtLayer(Transform parent, string name, string path, int order, float pixelsPerUnit, Vector3 localPosition)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing Map01 side-view texture: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        // Some source-art import workflows leave sprite-sheet rectangles behind.
        // Force the serialized mode as well so this panorama cannot silently return
        // to Multiple and leave the scene's SpriteRenderer referencing no full image.
        SerializedObject serializedImporter = new SerializedObject(importer);
        SerializedProperty spriteMode = serializedImporter.FindProperty("m_SpriteMode");
        if (spriteMode != null) spriteMode.intValue = (int)SpriteImportMode.Single;
        serializedImporter.ApplyModifiedPropertiesWithoutUndo();
        importer.SaveAndReimport();

        GameObject layer = Child(parent, name);
        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(candidate => candidate.rect.width * candidate.rect.height)
            .FirstOrDefault();
        if (renderer.sprite == null)
            throw new InvalidOperationException("No sprite was imported from " + path);
        renderer.sortingOrder = order;
        layer.transform.localPosition = localPosition;
    }

    private static void BuildSolids(Transform parent, IReadOnlyList<SolidSpec> specs, int layer, bool oneWay)
    {
        foreach (SolidSpec spec in specs)
        {
            GameObject item = Child(parent, spec.name);
            item.layer = layer;
            item.transform.localPosition = spec.center;
            BoxCollider2D box = item.AddComponent<BoxCollider2D>();
            box.size = spec.size;
            box.edgeRadius = 0.04f;
            if (!oneWay) continue;
            PlatformEffector2D effector = item.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
            box.usedByEffector = true;
        }
    }

    private static void BuildDeathVolume(Transform parent, int layer)
    {
        GameObject death = Child(parent, "VoidBelowMap");
        death.layer = layer;
        death.transform.localPosition = new Vector3(0f, -25f, 0f);
        BoxCollider2D box = death.AddComponent<BoxCollider2D>();
        box.size = new Vector2(192f, 8f);
        box.isTrigger = true;
        death.AddComponent<SideViewDeathVolume>();
    }

    private static void BuildCameraBounds(Transform root, int layer)
    {
        GameObject boundsObject = Child(root, "CameraBounds");
        boundsObject.layer = layer;
        BoxCollider2D bounds = boundsObject.AddComponent<BoxCollider2D>();
        bounds.size = new Vector2(192f, 48f);
        bounds.offset = new Vector2(0f, -1f);
        bounds.isTrigger = true;
    }

    private static void BuildGameplayMarkers(Transform root)
    {
        GameObject markers = Child(root, "GameplayMarkers");
        Marker(markers.transform, "PlayerSpawn", -84f, 3.43f);
        Marker(markers.transform, "EnemySpawn_FirstHusk", -55f, -0.57f);
        Marker(markers.transform, "ChronoBladeDrop", -9f, -0.57f);
        Marker(markers.transform, "RuneKey", 3f, 10.9f);
        Marker(markers.transform, "Checkpoint", 43f, -0.57f);
        Marker(markers.transform, "GatebreakerSpawn", 65f, -0.57f);
        Marker(markers.transform, "ExitToMap02", 86f, 3.03f);
    }

    private static void Marker(Transform parent, string name, float x, float y)
    {
        GameObject marker = Child(parent, name);
        marker.transform.localPosition = new Vector3(x, y, 0f);
    }

    private static void PlacePlayerAndCamera()
    {
        GameObject player = null;
        try
        {
            player = GameObject.FindWithTag("Player");
        }
        catch (UnityException)
        {
            // Keep the builder usable even if the Player tag was removed from the project.
        }

        if (player == null) player = GameObject.Find("SideViewPlayer");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            // Character sprites use a bottom/feet pivot. Keep the physics body
            // above that pivot so the visible boots (the sprite also has a small
            // transparent strip below them) rest on the authored surface.
            if (playerCollider is BoxCollider2D playerBox)
                playerBox.offset = new Vector2(playerBox.offset.x, playerBox.size.y * 0.5f + 0.35f);

            const float awakeningFloorTop = 3.4f;
            float localColliderBottom = playerCollider != null
                ? player.transform.InverseTransformPoint(playerCollider.bounds.min).y
                : 0f;
            player.transform.position = new Vector3(
                -84f,
                awakeningFloorTop - localColliderBottom + 0.03f,
                0f);
            Physics2D.SyncTransforms();
        }

        Camera camera = Camera.main;
        if (camera == null) return;
        camera.orthographic = true;
        camera.transform.position = new Vector3(-84f, 4.5f, camera.transform.position.z);
    }

    private static void CreateBeat(Transform parent, string name, float left, float right)
    {
        GameObject beat = Child(parent, name);
        beat.transform.localPosition = new Vector3((left + right) * 0.5f, 0f, 0f);
        BoxCollider2D volume = beat.AddComponent<BoxCollider2D>();
        volume.size = new Vector2(right - left, 48f);
        volume.isTrigger = true;
        volume.enabled = false; // Editor layout marker until room lifecycle is wired.
    }

    private static void DisableLegacyTopDownPresentation()
    {
        GameObject legacy = GameObject.Find("ManualWorld/Map01_RuinedEntrance");
        if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy);
        GameObject oldRoot = GameObject.Find("Map01_RuinedEntrance");
        if (oldRoot != null && oldRoot.name != RootName) UnityEngine.Object.DestroyImmediate(oldRoot);
    }

    private static int RequireLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0) throw new InvalidOperationException("Missing required layer: " + layerName);
        return layer;
    }

    private static GameObject Child(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }
}
#endif
