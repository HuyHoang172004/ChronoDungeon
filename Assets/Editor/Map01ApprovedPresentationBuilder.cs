#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rebuilds the authored Map 01 presentation hierarchy from the approved Pack 03 art.
/// The collision guide remains an editor reference only; generated EdgeCollider2D contours
/// follow its green walkable mask and are deliberately easy to refine by hand afterwards.
/// </summary>
public static class Map01ApprovedPresentationBuilder
{
    private const string MapRootPath = "ManualWorld/Map01_RuinedEntrance";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string FloorPath = Map01ModularCompositeGenerator.OutputMapPath;
    private const string CollisionPath = Map01ModularCompositeGenerator.OutputCollisionPath;
    private const string BackgroundPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/Master/CD_Map01_ValleyBackground_A_v02.png";
    private const float MapPixelsPerUnit = 30f;
    private const float MapCenterY = -3.5f;
    private const int CollisionCellPixels = 6;
    // Four sampled cells = 24 source pixels (0.8 world units). This keeps actor
    // feet on the visible paving instead of letting their centres enter thick
    // cliff faces, pillars and 3/4 wall artwork.
    private const int WalkableInsetCells = 4;
    private const float Map01PlayerScale = 1.05f;
    private const float Map01EnemyScale = 1.072f;
    private const float Map01CameraSize = 4.875f;
    private const float Map01EnemyPatrolRadius = 22.5f;
    private const float Map01EnemyHealth = 180f;

    [MenuItem("Tools/ChronoDungeon/Maps/Rebuild Map01 Approved Presentation")]
    public static void Rebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before rebuilding Map 01. Play Mode changes are discarded by Unity.");

        GameObject mapRoot = GameObject.Find(MapRootPath);
        if (mapRoot == null)
        {
            if (!System.IO.File.Exists(GameScenePath))
                throw new InvalidOperationException("GameScene is missing at " + GameScenePath);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            mapRoot = GameObject.Find(MapRootPath);
        }
        if (mapRoot == null) throw new InvalidOperationException("Map01_RuinedEntrance is not loaded.");

        Map01ModularCompositeGenerator.Generate();
        Sprite floor = PrepareSingleSprite(FloorPath);
        Sprite background = PrepareSingleSprite(BackgroundPath);

        GameObject backgroundRoot = GetOrCreateChild(mapRoot.transform, "BackgroundArt");
        SpriteRenderer valleyRenderer = GetOrCreateSpriteRenderer(GetOrCreateChild(backgroundRoot.transform, "ValleyBackground"));
        ConfigureMapRenderer(valleyRenderer, background, -200, false);
        SpriteRenderer floorRenderer = GetOrCreateSpriteRenderer(GetOrCreateChild(backgroundRoot.transform, "WalkableFloor"));
        ConfigureMapRenderer(floorRenderer, floor, -100, false);
        valleyRenderer.transform.localScale = new Vector3(
            floor.bounds.size.x / Mathf.Max(.001f, background.bounds.size.x),
            floor.bounds.size.y / Mathf.Max(.001f, background.bounds.size.y),
            1f);
        DisableLegacyMapArt(mapRoot.transform, valleyRenderer.gameObject, floorRenderer.gameObject);

        GameObject foregroundRoot = GetOrCreateChild(mapRoot.transform, "ForegroundArt");
        // Foreground is intentionally empty for the collision-first pass. It can be
        // authored later without changing the floor alpha or gameplay collision.
        foregroundRoot.SetActive(false);

        GetOrCreateChild(mapRoot.transform, "Props");
        GetOrCreateChild(mapRoot.transform, "Enemies");
        GetOrCreateChild(mapRoot.transform, "Loot");
        GetOrCreateChild(mapRoot.transform, "NPC");

        Texture2D floorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FloorPath);
        Texture2D collisionTexture = PrepareReadableTexture(CollisionPath);
        RebuildCollision(mapRoot.transform, collisionTexture, floor);
        RebuildNavigation(mapRoot.transform, collisionTexture, floor);
        FitCameraBounds(mapRoot.transform, floor);
        ScaleMap01Actors(mapRoot.transform);
        RemoveActorGroundCircles(mapRoot.transform);
        RebuildMap01Enemies(mapRoot.transform, floorTexture);
        RebuildMinimap(floor);
        Vector3 openingSpawn = PlaceOpeningSpawn(mapRoot.transform, floorTexture);
        PlaceOpeningCamera(floor, openingSpawn);
        RepositionLegacyTestObjects();

        Transform legacy = mapRoot.transform.Find("GeneratedPresentation");
        if (legacy != null) legacy.gameObject.SetActive(false);
        Transform legacyMaster = mapRoot.transform.Find("Map01_MasterArt");
        if (legacyMaster != null) legacyMaster.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(mapRoot.scene);
        AssetDatabase.SaveAssets();
        bool sceneSaved = EditorSceneManager.SaveScene(mapRoot.scene);
        Debug.Log($"ChronoDungeon: Map 01 scene save result={sceneSaved}, path={mapRoot.scene.path}");
        Debug.Log("ChronoDungeon: rebuilt Map 01 approved master/foreground/collision presentation.");
    }

    [MenuItem("Tools/ChronoDungeon/Maps/Rebuild Map01 Approved Presentation", true)]
    private static bool CanRebuild()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
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
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = 4096;
        importer.isReadable = path == FloorPath;
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

    private static void ScaleMap01Actors(Transform mapRoot)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) throw new InvalidOperationException("Player was not found.");
        // Never scale the Player gameplay root: attack ranges are authored in world units.
        // Only the presentation is smaller, so physics queries and weapon reach stay correct.
        player.transform.localScale = Vector3.one;
        Transform playerVisual = player.transform.Find("PlayerVisual");
        if (playerVisual != null) playerVisual.localScale = Vector3.one * Map01PlayerScale;

        BoxCollider2D playerBody = player.GetComponent<BoxCollider2D>();
        if (playerBody != null)
        {
            playerBody.size = new Vector2(.42f, .34f);
            playerBody.offset = new Vector2(0f, .14f);
        }

        foreach (EnemyFollow enemy in mapRoot.GetComponentsInChildren<EnemyFollow>(true))
        {
            enemy.transform.localScale = Vector3.one * Map01EnemyScale;
            ConfigureEnemyBrain(enemy);
            NormalizeEnemyVisual(enemy.gameObject);
            Health health = enemy.GetComponent<Health>();
            if (health != null) health.SetMaximumHealth(Map01EnemyHealth, true);
        }

        // The current early weapon bearer is still a legacy Map 01 test actor outside
        // the authored Enemies container, so include it until placement is migrated.
        GameObject bearer = GameObject.Find("Map01_WeaponBearer");
        if (bearer != null)
        {
            bearer.transform.localScale = Vector3.one * Map01EnemyScale;
            EnemyFollow bearerBrain = bearer.GetComponent<EnemyFollow>();
            if (bearerBrain != null) ConfigureEnemyBrain(bearerBrain);
            NormalizeEnemyVisual(bearer);
            Health health = bearer.GetComponent<Health>();
            if (health != null) health.SetMaximumHealth(Map01EnemyHealth, true);
        }
    }

    private static void DisableLegacyMapArt(Transform mapRoot, GameObject valley, GameObject floor)
    {
        Transform background = mapRoot.Find("BackgroundArt");
        if (background != null)
            foreach (Transform child in background)
                if (child.gameObject != valley && child.gameObject != floor)
                    child.gameObject.SetActive(false);

        string[] legacyRoots = { "Map01_MasterArt", "GeneratedPresentation", "LegacyCollisionBackup" };
        for (int i = 0; i < legacyRoots.Length; i++)
        {
            Transform legacy = mapRoot.Find(legacyRoots[i]);
            if (legacy != null) legacy.gameObject.SetActive(false);
        }
    }

    private static void RemoveActorGroundCircles(Transform mapRoot)
    {
        GameObject player = GameObject.Find("Player");
        if (player != null) RemoveNamedDescendants(player.transform,
            "PlayerShadow", "Player Shadow", "Temporal Cyan Accent", "ChronoVisualAccent");

        foreach (EnemyFollow enemy in mapRoot.GetComponentsInChildren<EnemyFollow>(true))
            RemoveNamedDescendants(enemy.transform, "EnemyShadow", "Enemy Shadow", "ChronoVisualAccent");
    }

    private static void RebuildMap01Enemies(Transform mapRoot, Texture2D floorTexture)
    {
        GameObject enemiesRoot = GetOrCreateChild(mapRoot, "Enemies");
        Transform generated = enemiesRoot.transform.Find("GeneratedMap01Enemies");
        if (generated != null) UnityEngine.Object.DestroyImmediate(generated.gameObject);
        generated = new GameObject("GeneratedMap01Enemies").transform;
        generated.SetParent(enemiesRoot.transform, false);

        EnemyFollow source = null;
        foreach (EnemyFollow candidate in mapRoot.GetComponentsInChildren<EnemyFollow>(true))
        {
            if (!candidate.name.StartsWith("Map01 Generated Enemy", StringComparison.Ordinal))
            {
                source = candidate;
                break;
            }
        }
        if (source == null) source = UnityEngine.Object.FindAnyObjectByType<EnemyFollow>(FindObjectsInactive.Include);
        if (source == null) throw new InvalidOperationException("No EnemyFollow source exists for Map 01 enemy generation.");

        // Normalized locations target Map A's broad combat spaces, never its narrow links.
        Vector2[] normalizedSpawns =
        {
            new Vector2(.17f, .17f), new Vector2(.24f, .17f),
            new Vector2(.43f, .16f), new Vector2(.51f, .18f),
            new Vector2(.78f, .16f), new Vector2(.84f, .18f),
            new Vector2(.18f, .52f), new Vector2(.21f, .59f),
            new Vector2(.19f, .83f), new Vector2(.56f, .84f),
            new Vector2(.60f, .55f), new Vector2(.68f, .55f)
        };

        var roomEnemies = new List<Health>();
        Health sourceHealth = source.GetComponent<Health>();
        if (sourceHealth != null && source.transform.IsChildOf(mapRoot)) roomEnemies.Add(sourceHealth);
        for (int i = 0; i < normalizedSpawns.Length; i++)
        {
            Vector2 uv = normalizedSpawns[i];
            Vector3 position = PixelToWorld(
                floorTexture.width * uv.x,
                floorTexture.height * uv.y,
                floorTexture.width,
                floorTexture.height);
            GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, generated);
            clone.name = "Map01 Generated Enemy " + (i + 1).ToString("00");
            clone.transform.position = position;
            clone.transform.localScale = Vector3.one * Map01EnemyScale;
            clone.SetActive(true);
            EnemyFollow cloneBrain = clone.GetComponent<EnemyFollow>();
            if (cloneBrain != null) ConfigureEnemyBrain(cloneBrain);
            CharacterAnimationIdentity identity = clone.GetComponent<CharacterAnimationIdentity>();
            if (identity == null) identity = clone.AddComponent<CharacterAnimationIdentity>();
            CharacterAnimationIdentity sourceIdentity = source.GetComponent<CharacterAnimationIdentity>();
            identity.Configure(sourceIdentity != null && !string.IsNullOrWhiteSpace(sourceIdentity.CharacterId)
                ? sourceIdentity.CharacterId
                : CharacterAnimationResolver.GuessCharacterId(source.gameObject));
            NormalizeEnemyVisual(clone);
            RemoveNamedDescendants(clone.transform, "EnemyShadow", "Enemy Shadow", "ChronoVisualAccent");
            Health health = clone.GetComponent<Health>();
            if (health != null)
            {
                health.SetMaximumHealth(Map01EnemyHealth, true);
                roomEnemies.Add(health);
            }
        }

        Room mapRoom = mapRoot.GetComponentInParent<Room>();
        if (mapRoom != null)
        {
            SerializedObject serialized = new SerializedObject(mapRoom);
            SerializedProperty enemies = serialized.FindProperty("enemies");
            enemies.arraySize = roomEnemies.Count;
            for (int i = 0; i < roomEnemies.Count; i++)
                enemies.GetArrayElementAtIndex(i).objectReferenceValue = roomEnemies[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mapRoom);
        }
    }

    private static void RemoveNamedDescendants(Transform root, params string[] names)
    {
        var remove = new List<GameObject>();
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == root) continue;
            for (int i = 0; i < names.Length; i++)
                if (child.name.Equals(names[i], StringComparison.OrdinalIgnoreCase))
                {
                    remove.Add(child.gameObject);
                    break;
                }
        }
        foreach (GameObject target in remove) UnityEngine.Object.DestroyImmediate(target);
    }

    private static void NormalizeEnemyVisual(GameObject enemy)
    {
        SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.color = Color.white;
        if (enemy.GetComponent<EnemyReadabilityPresentation>() == null)
            enemy.AddComponent<EnemyReadabilityPresentation>();
    }

    private static void ConfigureEnemyBrain(EnemyFollow enemy)
    {
        enemy.Configure(2.2f, 10f, Map01EnemyPatrolRadius);
        EditorUtility.SetDirty(enemy);
    }

    private static void RebuildNavigation(Transform mapRoot, Texture2D collisionTexture, Sprite floor)
    {
        Map01NavigationGrid navigation = mapRoot.GetComponent<Map01NavigationGrid>();
        if (navigation == null) navigation = mapRoot.gameObject.AddComponent<Map01NavigationGrid>();
        navigation.Configure(collisionTexture, new Vector2(0f, MapCenterY), floor.bounds.size);
        EditorUtility.SetDirty(navigation);
    }

    private static void RebuildMinimap(Sprite floor)
    {
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) throw new InvalidOperationException("Canvas was not found for Map 01 minimap.");
        Transform parent = canvas.transform.Find("Chrono Safe Area") ?? canvas.transform;
        // Previous rebuilds created the panel under different Canvas/Safe Area parents,
        // so deleting only parent.Find(...) left stale panels visible in Play Mode.
        foreach (Map01MinimapUI oldMinimap in UnityEngine.Object.FindObjectsByType<Map01MinimapUI>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(oldMinimap.gameObject);

        GameObject root = new GameObject("Map01 Minimap", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = Vector2.one;
        rootRect.anchorMax = Vector2.one;
        rootRect.pivot = Vector2.one;
        rootRect.anchoredPosition = new Vector2(-24f, -92f);
        rootRect.sizeDelta = new Vector2(286f, 228f);
        Image panel = root.GetComponent<Image>();
        panel.color = new Color(.015f, .035f, .075f, .88f);

        GameObject mapObject = new GameObject("Floor Map", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform mapRect = mapObject.GetComponent<RectTransform>();
        mapRect.SetParent(rootRect, false);
        mapRect.anchorMin = new Vector2(.5f, .5f);
        mapRect.anchorMax = new Vector2(.5f, .5f);
        mapRect.pivot = new Vector2(.5f, .5f);
        mapRect.anchoredPosition = Vector2.zero;
        float aspect = floor.bounds.size.x / Mathf.Max(.001f, floor.bounds.size.y);
        mapRect.sizeDelta = new Vector2(270f, 270f / aspect);
        Image mapImage = mapObject.GetComponent<Image>();
        mapImage.sprite = floor;
        mapImage.preserveAspect = true;
        mapImage.color = new Color(.72f, .84f, .92f, .92f);
        mapImage.raycastTarget = false;

        GameObject markerObject = new GameObject("Player Marker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform markerRect = markerObject.GetComponent<RectTransform>();
        markerRect.SetParent(mapRect, false);
        markerRect.anchorMin = new Vector2(.5f, .5f);
        markerRect.anchorMax = markerRect.anchorMin;
        markerRect.pivot = new Vector2(.5f, .5f);
        markerRect.sizeDelta = new Vector2(15f, 15f);
        markerRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image markerImage = markerObject.GetComponent<Image>();
        markerImage.color = new Color(.05f, .58f, 1f, 1f);
        markerImage.raycastTarget = false;

        Map01MinimapUI minimap = root.AddComponent<Map01MinimapUI>();
        SerializedObject serialized = new SerializedObject(minimap);
        serialized.FindProperty("mapRect").objectReferenceValue = mapRect;
        serialized.FindProperty("playerMarker").objectReferenceValue = markerRect;
        serialized.FindProperty("player").objectReferenceValue = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        serialized.FindProperty("worldCenter").vector2Value = new Vector2(0f, MapCenterY);
        serialized.FindProperty("worldSize").vector2Value = floor.bounds.size;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(root);
    }

    private static Vector3 PlaceOpeningSpawn(Transform mapRoot, Texture2D floorTexture)
    {
        if (floorTexture == null) throw new InvalidOperationException("Map 01 floor texture was not found.");
        // Centre of the opening courtyard in modular Corner 1.
        Vector3 spawn = PixelToWorld(
            floorTexture.width * 0.175f,
            floorTexture.height * 0.165f,
            floorTexture.width,
            floorTexture.height);
        GameObject player = GameObject.Find("Player");
        if (player != null) player.transform.position = spawn;
        Transform marker = mapRoot.Find("Gameplay/PlayerSpawn");
        if (marker != null) marker.position = spawn;
        return spawn;
    }

    private static void PlaceOpeningCamera(Sprite master, Vector3 spawn)
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        camera.orthographic = true;
        camera.orthographicSize = Map01CameraSize;
        ZoneCameraController controller = camera.GetComponent<ZoneCameraController>();
        if (controller != null)
        {
            SerializedObject serialized = new SerializedObject(controller);
            SerializedProperty viewSize = serialized.FindProperty("viewSize");
            if (viewSize != null) viewSize.floatValue = Map01CameraSize;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }
        float halfWidth = camera.orthographicSize * camera.aspect;
        float left = -master.bounds.extents.x;
        camera.transform.position = new Vector3(Mathf.Max(left + halfWidth, spawn.x), spawn.y, camera.transform.position.z);
    }

    private static Vector3 PixelToWorld(float x, float yFromTop, float width, float height)
    {
        return new Vector3((x - width * 0.5f) / MapPixelsPerUnit,
            (height * 0.5f - yFromTop) / MapPixelsPerUnit + MapCenterY,
            0f);
    }

    private static Texture2D PrepareReadableTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing texture importer: " + path);
        importer.textureType = TextureImporterType.Default;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) throw new InvalidOperationException("Could not import texture: " + path);
        return texture;
    }

    private static void RepositionLegacyTestObjects()
    {
        // These are old vertical-slice test objects, not authored Map 01 placement.
        // Keep the first combat test reachable while the proper Ruin Husk roster is integrated.
        GameObject bearer = GameObject.Find("Map01_WeaponBearer");
        if (bearer != null) bearer.transform.position = new Vector3(-11.2f, -2.5f, 0f);

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

    private static void RebuildCollision(Transform mapRoot, Texture2D floorTexture, Sprite master)
    {
        if (floorTexture == null) throw new InvalidOperationException("Walkable floor texture was not found.");

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

        bool[,] walkable = BuildWalkableMask(floorTexture, CollisionCellPixels);
        walkable = ErodeWalkableMask(walkable, WalkableInsetCells);
        List<List<Vector2Int>> loops = TraceBoundaryLoops(walkable);
        float cellWorld = CollisionCellPixels / MapPixelsPerUnit;
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

    private static bool[,] BuildWalkableMask(Texture2D floor, int cellPixels)
    {
        int width = Mathf.CeilToInt(floor.width / (float)cellPixels);
        int height = Mathf.CeilToInt(floor.height / (float)cellPixels);
        bool[,] mask = new bool[width, height];
        Color32[] pixels = floor.GetPixels32();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int px = Mathf.Min(floor.width - 1, x * cellPixels + cellPixels / 2);
            int py = Mathf.Min(floor.height - 1, y * cellPixels + cellPixels / 2);
            // The visible floor alpha is now the single source of truth for traversal.
            // Transparent valley pixels are never walkable.
            mask[x, y] = pixels[py * floor.width + px].a >= 128;
        }
        return mask;
    }

    private static bool[,] ErodeWalkableMask(bool[,] source, int radius)
    {
        if (radius <= 0) return source;
        int width = source.GetLength(0);
        int height = source.GetLength(1);
        bool[,] result = new bool[width, height];
        int radiusSquared = radius * radius;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (!source[x, y]) continue;
            bool safe = true;
            for (int oy = -radius; oy <= radius && safe; oy++)
            for (int ox = -radius; ox <= radius; ox++)
            {
                if (ox * ox + oy * oy > radiusSquared) continue;
                int sx = x + ox, sy = y + oy;
                if (sx < 0 || sy < 0 || sx >= width || sy >= height || !source[sx, sy])
                {
                    safe = false;
                    break;
                }
            }
            result[x, y] = safe;
        }
        return result;
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
