using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public sealed class ModularMapTile
{
    public Sprite background;
    public Sprite ground;
    public Vector2 localPosition;
}

[Serializable]
public sealed class ModularMapColliderPath
{
    public Vector2[] points;
}

public static class ModularMapPresenter
{
    public static GameObject Build(ModularMapDefinition definition)
    {
        if (definition == null) return null;
        GameObject root = new GameObject(definition.zoneName + " Modular Runtime");
        Transform background = Child(root.transform, "Background");
        Transform ground = Child(root.transform, "WalkableGround");
        Transform collision = Child(root.transform, "Collision");
        if (definition.background != null)
        {
            GameObject backdrop = new GameObject("NonWalkableAbyss");
            backdrop.transform.SetParent(background, false);
            SpriteRenderer backdropRenderer = backdrop.AddComponent<SpriteRenderer>();
            backdropRenderer.sprite = definition.background;
            backdropRenderer.sortingOrder = -40;
            Vector2 sourceSize = definition.background.bounds.size;
            backdrop.transform.localScale = new Vector3(
                definition.worldSize.x / Mathf.Max(.001f, sourceSize.x),
                definition.worldSize.y / Mathf.Max(.001f, sourceSize.y), 1f);
        }
        foreach (ModularMapTile tile in definition.tiles)
        {
            AddTile(background, tile.background, tile.localPosition, -30);
            AddTile(ground, tile.ground, tile.localPosition, -20);
        }
        int pathIndex = 0;
        foreach (ModularMapColliderPath path in definition.colliderPaths)
        {
            if (path == null || path.points == null || path.points.Length < 3) continue;
            GameObject edgeObject = new GameObject("WalkableBoundary_" + pathIndex++.ToString("00"));
            edgeObject.transform.SetParent(collision, false);
            EdgeCollider2D edge = edgeObject.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = .045f;
            edge.points = path.points;
        }
        if (pathIndex == 0 && definition.walkableMask != null)
            WalkableMaskColliderBuilder.Build(definition.walkableMask, definition.worldSize, collision);
        Map01NavigationGrid navigation = root.AddComponent<Map01NavigationGrid>();
        navigation.Configure(definition.walkableMask, Vector2.zero, definition.worldSize);
        MapWalkablePlayerConstraint constraint = root.AddComponent<MapWalkablePlayerConstraint>();
        constraint.Configure(definition.walkableMask, definition.worldSize);
        Map01MinimapUI.ConfigureRuntimeMap(definition);
        return root;
    }

    public static Vector3 GetSpawn(ModularMapDefinition definition, float z)
    {
        Vector2 normalized = definition == null ? new Vector2(.12f, .5f) : definition.spawnNormalized;
        Vector2 size = definition == null ? Vector2.one : definition.worldSize;
        return new Vector3(Mathf.Lerp(-size.x * .5f, size.x * .5f, normalized.x),
            Mathf.Lerp(-size.y * .5f, size.y * .5f, normalized.y), z);
    }

    private static Transform Child(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static void AddTile(Transform parent, Sprite sprite, Vector2 position, int order)
    {
        if (sprite == null) return;
        GameObject tile = new GameObject(sprite.name);
        tile.transform.SetParent(parent, false);
        tile.transform.localPosition = position;
        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
    }
}

/// <summary>
/// Final runtime guard for the player. The same alpha mask that builds collision is
/// sampled after movement, so a gap in an EdgeCollider can never send the player
/// into transparent abyss art.
/// </summary>
[DisallowMultipleComponent]
public sealed class MapWalkablePlayerConstraint : MonoBehaviour
{
    private Texture2D mask;
    private Vector2 size;
    private PlayerMovement player;
    private Rigidbody2D body;
    private Vector2 lastValidPosition;
    private bool hasLastValidPosition;

    public void Configure(Texture2D walkableMask, Vector2 worldSize)
    {
        mask = walkableMask;
        size = worldSize;
    }

    public void Bind(PlayerMovement target)
    {
        player = target;
        body = player != null ? player.GetComponent<Rigidbody2D>() : null;
        if (player != null && IsWalkable(player.transform.position))
        {
            lastValidPosition = player.transform.position;
            hasLastValidPosition = true;
        }
    }

    private void FixedUpdate()
    {
        if (player == null || mask == null || !mask.isReadable) return;
        Vector2 position = body != null ? body.position : player.transform.position;
        if (IsWalkable(position))
        {
            lastValidPosition = position;
            hasLastValidPosition = true;
            return;
        }
        if (!hasLastValidPosition) return;
        if (body != null)
        {
            body.position = lastValidPosition;
            body.linearVelocity = Vector2.zero;
        }
        else player.transform.position = lastValidPosition;
    }

    private bool IsWalkable(Vector2 position)
    {
        if (size.x <= .001f || size.y <= .001f) return false;
        float u = position.x / size.x + .5f;
        float v = position.y / size.y + .5f;
        if (u <= 0f || u >= 1f || v <= 0f || v >= 1f) return false;
        int x = Mathf.Clamp(Mathf.FloorToInt(u * mask.width), 0, mask.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(v * mask.height), 0, mask.height - 1);
        return mask.GetPixel(x, y).a >= .5f;
    }
}

public static class WalkableMaskColliderBuilder
{
    private const int CellPixels = 8;
    private readonly struct Segment
    {
        public readonly Vector2Int a, b;
        public Segment(Vector2Int a, Vector2Int b) { this.a = a; this.b = b; }
    }

    public static void Build(Texture2D mask, Vector2 worldSize, Transform parent)
    {
        if (mask == null || !mask.isReadable) return;
        int width = Mathf.CeilToInt(mask.width / (float)CellPixels);
        int height = Mathf.CeilToInt(mask.height / (float)CellPixels);
        bool[,] cells = new bool[width, height];
        Color32[] pixels = mask.GetPixels32();
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            int px = Mathf.Min(mask.width - 1, x * CellPixels + CellPixels / 2);
            int py = Mathf.Min(mask.height - 1, y * CellPixels + CellPixels / 2);
            cells[x, y] = pixels[py * mask.width + px].a >= 128;
        }

        int created = 0;
        foreach (List<Vector2Int> loop in Trace(cells))
        {
            List<Vector2Int> simple = Simplify(loop);
            if (simple.Count < 3) continue;
            Vector2[] points = new Vector2[simple.Count + 1];
            for (int i = 0; i < simple.Count; i++)
                points[i] = new Vector2((simple[i].x / (float)width - .5f) * worldSize.x,
                    (simple[i].y / (float)height - .5f) * worldSize.y);
            points[points.Length - 1] = points[0];
            GameObject boundary = new GameObject("MaskBoundary_" + created++.ToString("00"));
            boundary.transform.SetParent(parent, false);
            EdgeCollider2D edge = boundary.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = .05f;
            edge.points = points;
        }
    }

    private static List<List<Vector2Int>> Trace(bool[,] mask)
    {
        int width = mask.GetLength(0), height = mask.GetLength(1);
        List<Segment> segments = new();
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) if (mask[x, y])
        {
            if (y == 0 || !mask[x, y - 1]) segments.Add(new Segment(new Vector2Int(x + 1, y), new Vector2Int(x, y)));
            if (x == width - 1 || !mask[x + 1, y]) segments.Add(new Segment(new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y)));
            if (y == height - 1 || !mask[x, y + 1]) segments.Add(new Segment(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1)));
            if (x == 0 || !mask[x - 1, y]) segments.Add(new Segment(new Vector2Int(x, y), new Vector2Int(x, y + 1)));
        }
        Dictionary<Vector2Int, List<int>> touching = new();
        for (int i = 0; i < segments.Count; i++) { Add(touching, segments[i].a, i); Add(touching, segments[i].b, i); }
        bool[] used = new bool[segments.Count]; List<List<Vector2Int>> loops = new();
        for (int seed = 0; seed < segments.Count; seed++)
        {
            if (used[seed]) continue;
            List<Vector2Int> loop = new(); Segment first = segments[seed]; used[seed] = true; loop.Add(first.a);
            Vector2Int current = first.b; int guard = segments.Count + 1;
            while (guard-- > 0)
            {
                loop.Add(current); if (current == loop[0]) break;
                if (!touching.TryGetValue(current, out List<int> options)) break;
                int next = -1; for (int i = 0; i < options.Count; i++) if (!used[options[i]]) { next = options[i]; break; }
                if (next < 0) break; used[next] = true; Segment segment = segments[next];
                current = segment.a == current ? segment.b : segment.a;
            }
            if (loop.Count > 3 && loop[loop.Count - 1] == loop[0]) { loop.RemoveAt(loop.Count - 1); loops.Add(loop); }
        }
        return loops;
    }

    private static void Add(Dictionary<Vector2Int, List<int>> map, Vector2Int point, int index)
    { if (!map.TryGetValue(point, out List<int> list)) map.Add(point, list = new List<int>()); list.Add(index); }

    private static List<Vector2Int> Simplify(List<Vector2Int> points)
    {
        List<Vector2Int> result = new();
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int a = points[i] - points[(i - 1 + points.Count) % points.Count];
            Vector2Int b = points[(i + 1) % points.Count] - points[i];
            if (a.x * b.y - a.y * b.x != 0) result.Add(points[i]);
        }
        return result;
    }
}

/// <summary>Builds authored rosters on the selected map's walkable room tiles.</summary>
public static class ModularMapEncounters
{
    private static readonly string[][] Rosters =
    {
        new[] { "Enemies_Map02_LegionHusk", "Enemies_Map02_RustArcher", "Enemies_Map02_HeavyWarden", "Bosses_Map02_BarracksCaptain" },
        new[] { "Enemies_Map03_VineHusk", "Enemies_Map03_ThornSpitter", "Enemies_Map03_RootBrute", "Bosses_Map03_ThornMatriarch" },
        new[] { "Enemies_Map04_LivingTome", "Enemies_Map04_ArchiveWisp", "Enemies_Map04_LibrarySentinel", "Bosses_Map04_SilentCurator" },
        new[] { "Enemies_Map05_AshHusk", "Enemies_Map05_EmberCaster", "Enemies_Map05_ForgeKnight", "Bosses_Map05_AshWarden" },
        new[] { "Enemies_Map06_CellGuard", "Enemies_Map06_PrismArcher", "Enemies_Map06_EchoBinder", "Bosses_Map06_MirrorJailer" },
        // The approach reuses barracks guards; no separate Map 07 normal-enemy art exists.
        new[] { "Enemies_Map02_LegionHusk", "Enemies_Map02_RustArcher", "Enemies_Map02_HeavyWarden", "Bosses_Map07_ChronoVanguard" },
        new[] { "Enemies_Map08_TemporalEchoMinion", "Enemies_Map08_TemporalEchoMinion", "Enemies_Map06_CellGuard", "Bosses_Map08_ChronoGuardian" }
    };

    public static void Build(GameObject root, ModularMapDefinition map, int mapIndex, PlayerMovement player)
    {
        CharacterAnimationCatalog catalog = Resources.Load<CharacterAnimationCatalog>("CharacterAnimationCatalog");
        if (catalog == null || mapIndex < 1 || mapIndex > Rosters.Length)
            throw new InvalidOperationException("Missing encounter animation catalog for map " + (mapIndex + 1));
        string[] roster = Rosters[mapIndex - 1];
        foreach (string id in roster)
            if (catalog.Find(id)?.Controller == null)
                throw new InvalidOperationException("Encounter profile/controller missing: " + id);

        // Old scene actors belong to the old room layout, never to a newly built map.
        foreach (EnemyFollow enemy in UnityEngine.Object.FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include))
            enemy.gameObject.SetActive(false);
        RoomManager rooms = UnityEngine.Object.FindAnyObjectByType<RoomManager>();
        if (rooms != null) rooms.enabled = false;
        TimeLoopManager loop = UnityEngine.Object.FindAnyObjectByType<TimeLoopManager>();
        if (loop != null) loop.enabled = false;

        Transform actors = new GameObject("Map " + (mapIndex + 1) + " Encounters").transform;
        actors.SetParent(root.transform, false);
        Map01NavigationGrid grid = root.GetComponent<Map01NavigationGrid>();
        var occupied = new List<Vector2>();
        var route = new List<Vector2>();
        int count = 0, bosses = 0;
        foreach (ModularMapTile tile in map.tiles)
        {
            if (tile.ground == null) continue;
            bool bossRoom = tile.ground.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
            int slots = bossRoom ? 3 : 4;
            for (int i = 0; i < slots; i++)
            {
                bool boss = bossRoom && i == 0;
                float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                Vector2 wanted = tile.localPosition + (boss ? Vector2.zero : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.2f);
                if (!FindSpawn(map, grid, player.transform.position, wanted, occupied, route, out Vector2 position))
                {
                    Debug.LogError("No reachable enemy spawn near " + wanted + " in " + map.zoneName);
                    continue;
                }
                string id = boss ? roster[3] : roster[i % 3];
                Spawn(actors, catalog.Find(id), grid, player, position, mapIndex, boss);
                occupied.Add(position);
                count++;
                if (boss) bosses++;
            }
        }
        if (bosses != 1) Debug.LogError(map.zoneName + " requires one reachable boss; spawned " + bosses);
        Debug.Log("ChronoDungeon: " + map.zoneName + " spawned " + count + " enemies including " + bosses + " boss.");
    }

    private static bool FindSpawn(ModularMapDefinition map, Map01NavigationGrid grid, Vector2 start,
        Vector2 wanted, List<Vector2> occupied, List<Vector2> route, out Vector2 result)
    {
        for (int ring = 0; ring <= 12; ring++)
        for (int step = 0; step < (ring == 0 ? 1 : 16); step++)
        {
            float angle = step * Mathf.PI / 8f;
            Vector2 point = wanted + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring * .25f;
            if (Vector2.Distance(point, start) < 3f || occupied.Exists(p => Vector2.Distance(p, point) < 1f)) continue;
            bool clear = true;
            for (int sample = 0; sample < 9; sample++)
            {
                Vector2 offset = sample == 0 ? Vector2.zero :
                    new Vector2(Mathf.Cos(sample * Mathf.PI / 4f), Mathf.Sin(sample * Mathf.PI / 4f)) * .4f;
                Vector2 uv = (point + offset) / map.worldSize + Vector2.one * .5f;
                if (uv.x <= 0 || uv.y <= 0 || uv.x >= 1 || uv.y >= 1 || map.walkableMask == null ||
                    !map.walkableMask.isReadable || map.walkableMask.GetPixelBilinear(uv.x, uv.y).a < .5f)
                { clear = false; break; }
            }
            if (!clear || grid == null || !grid.FindPath(start, point, route)) continue;
            result = point;
            return true;
        }
        result = default;
        return false;
    }

    private static void Spawn(Transform parent, CharacterAnimationProfile profile, Map01NavigationGrid grid,
        PlayerMovement player, Vector2 position, int mapIndex, bool boss)
    {
        GameObject actor = new GameObject(profile.CharacterId);
        actor.SetActive(false);
        actor.transform.SetParent(parent, false);
        actor.transform.position = position;
        actor.transform.localScale = Vector3.one * .536f;
        SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 42;
        renderer.color = Color.white;
        actor.AddComponent<CharacterAnimationIdentity>().Configure(profile.CharacterId);
        Animator animator = actor.AddComponent<Animator>();
        animator.runtimeAnimatorController = profile.Controller;
        Rigidbody2D body = actor.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CircleCollider2D collider = actor.AddComponent<CircleCollider2D>();
        collider.radius = boss ? .55f : .32f;
        collider.offset = new Vector2(0, .12f);
        Health health = actor.AddComponent<Health>();
        health.SetMaximumHealth(boss ? 650f + mapIndex * 100f : 100f + mapIndex * 20f, true);
        EnemyFollow follow = actor.AddComponent<EnemyFollow>();
        bool ranged = profile.CharacterId.Contains("Archer") || profile.CharacterId.Contains("Caster") ||
            profile.CharacterId.Contains("Spitter") || profile.CharacterId.Contains("Wisp") || profile.CharacterId.Contains("Tome") ||
            profile.CharacterId.Contains("Curator") || profile.CharacterId.Contains("Mage");
        follow.ConfigureEncounter(player, grid, ranged ? 4f : .65f);
        follow.Configure(boss ? 1.3f : 1.8f, boss ? 7f : 6f, boss ? 2.5f : 4f);
        actor.AddComponent<EnemyAnimationDriver>().Configure(profile);
        if (ranged) actor.AddComponent<EnemyProjectilePool>().ConfigureRuntimeBolts();
        actor.AddComponent<EnemyAbilityBrain>();
        actor.AddComponent<EnemyReadabilityPresentation>();
        actor.AddComponent<EnemyLootDrop>();
        health.Died += _ =>
        {
            collider.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            UnityEngine.Object.Destroy(actor, 1.2f);
        };
        actor.SetActive(true);
    }
}

public static class MapSelectionRuntime
{
    private const string PreferenceKey = "ChronoDungeon.SelectedMap";
    private static readonly string[] ZoneNames =
    {
        "Zone01_RuinedEntrance", "Zone02_ForgottenBarracks", "Zone03_OvergrownWilds",
        "Zone04_EchoLibrary", "Zone05_AshForge", "Zone06_ShatteredMirrorPrison",
        "Zone07_WardenApproach", "Zone08_ChronoSanctum"
    };

    private static readonly string[] MapAssetRelativePaths =
    {
        "ArtSource/Maps/Map01_RuinedEntrance/Master/CD_Map01_Modular3Q_Master_v01.png",
        "ArtSource/Maps/Map02_ForgottenBarracks/Master/CD_Map02_Master_Base_Composition_v01.png",
        "ArtSource/Maps/Map03_OvergrownWilds/Master/CD_Map03_Master_Base_Composition_v01.png",
        "ArtSource/Maps/Map04_EchoLibrary/Master/CD_Map04_Master_Base_Composition_v01.png",
        "ArtSource/Maps/Map05_AshForge/Master/CD_Map05_Master_Base_Composition_v01.png",
        "ArtSource/Maps/Map06_ShatteredMirrorPrison/Master/CD_Map06_Master_Base_Composition_v01.png",
        "ArtSource/Maps/Map07_WardenApproach/Master/CD_Map07_Master_Base_Composition_v01.png",
        "ArtSource/Maps/Map08_ChronoSanctum/Master/CD_Map08_Master_Base_Composition_v01.png"
    };

    public static int SelectedMap
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PreferenceKey, 0), 0, ZoneNames.Length - 1);
        set
        {
            PlayerPrefs.SetInt(PreferenceKey, Mathf.Clamp(value, 0, ZoneNames.Length - 1));
            PlayerPrefs.Save();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoading()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GameScene" || SelectedMap == 0) return;
        GameObject runner = new GameObject("Selected Map Spawn");
        runner.AddComponent<MapSelectionSpawnRunner>();
    }

    private sealed class MapSelectionSpawnRunner : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // RoomManager initializes at execution order 500; wait until its spawn is applied.
            yield return null;
            yield return new WaitForFixedUpdate();
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            int selected = SelectedMap;
            Debug.Log("ChronoDungeon: loading selected map index " + selected + " (" + ZoneNames[selected] + ").");
            MapRuntimeCatalog catalog = Resources.Load<MapRuntimeCatalog>("MapRuntimeCatalog");
            ModularMapDefinition modularMap = catalog != null && catalog.modularMaps != null &&
                selected < catalog.modularMaps.Length ? catalog.modularMaps[selected] : null;
            // Maps 2-8 are modular-only. Never fall back to an obsolete master PNG:
            // that made a failed catalog rebuild look like a successful new map.
            if (selected != 0)
            {
                GameObject map01 = GameObject.Find("Map01_RuinedEntrance");
                if (map01 != null) map01.SetActive(false);
            }

            if (player != null && modularMap != null)
            {
                GameObject modularRoot = ModularMapPresenter.Build(modularMap);
                Vector3 destination = ModularMapPresenter.GetSpawn(modularMap, player.transform.position.z);
                MapWalkablePlayerConstraint constraint = modularRoot != null
                    ? modularRoot.GetComponent<MapWalkablePlayerConstraint>() : null;
                ApplyRuntimePlayerScale(player);
                player.transform.position = destination;
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    body.position = destination;
                    body.linearVelocity = Vector2.zero;
                }
                if (constraint != null) constraint.Bind(player);
                ConfigureRuntimeCamera(player, modularMap.worldSize);
                ModularMapEncounters.Build(modularRoot, modularMap, selected, player);
                Debug.Log("ChronoDungeon: ready map " + (selected + 1) + " " + modularMap.zoneName +
                    "; player=" + player.transform.position + "; tiles=" + modularMap.tiles.Count);
            }
            else Debug.LogError("Selected map " + ZoneNames[selected] +
                " has no valid modular definition. Run ChronoDungeon > Rebuild Modular Maps; " +
                "the retired master image is intentionally not used as a fallback.");

            Destroy(gameObject);
        }

        private static void ConfigureRuntimeCamera(PlayerMovement player, Vector2 mapSize)
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            RoomCamera roomCamera = camera.GetComponent<RoomCamera>();
            if (roomCamera != null) roomCamera.enabled = false;
            MapCameraFollow legacyFollow = camera.GetComponent<MapCameraFollow>();
            if (legacyFollow != null) legacyFollow.enabled = false;
            ZoneCameraController zoneCamera = camera.GetComponent<ZoneCameraController>();
            if (zoneCamera == null) zoneCamera = camera.gameObject.AddComponent<ZoneCameraController>();
            zoneCamera.enabled = true;
            zoneCamera.ConfigureRuntimeMap(player, mapSize, 4.5f / 1.2f);
        }

        private static void ApplyRuntimePlayerScale(PlayerMovement player)
        {
            Transform visual = player.transform.Find("PlayerVisual");
            if (visual == null) return;
            const float mapScale = .525f;
            PlayerVisualPresentation presentation = visual.GetComponent<PlayerVisualPresentation>();
            if (presentation != null) presentation.SetPresentationScale(mapScale);
            else visual.localScale = Vector3.one * mapScale;
            PlayerWeaponVisualSetController weaponVisual = visual.GetComponent<PlayerWeaponVisualSetController>();
            if (weaponVisual != null) weaponVisual.SetPresentationScale(mapScale);
        }

        private static Sprite LoadAuthoredMapSprite(int index)
        {
            if (index < 0 || index >= MapAssetRelativePaths.Length) return null;
            string path = Path.Combine(Application.dataPath,
                MapAssetRelativePaths[index].Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) return null;

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false)
            {
                name = ZoneNames[index] + " Runtime Texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            if (!texture.LoadImage(bytes, false))
            {
                Destroy(texture);
                return null;
            }
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
        }
    }
}
