using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

/// Builds the saved, authored Tilemap presentation for Zone 1 only.
/// Gameplay objects are reused and kept under the Room so RoomManager remains
/// the owner of progression and the key/gate flow.
public static class GoldenZone01Builder
{
    private const string CaveAtlas = "Assets/ThirdParty/kenney/Environment/CavesDungeon/roguelikeDungeon_transparent.png";
    private const string RpgAtlas = "Assets/ThirdParty/kenney/Environment/RPG/roguelikeSheet_transparent.png";
    private const string CharacterAtlas = "Assets/ThirdParty/kenney/Characters/Roguelike/roguelikeChar_transparent.png";
    private const string ParticlePath = "Assets/ThirdParty/kenney/VFX/Particles/";
    private const string UiPath = "Assets/ThirdParty/kenney/UI/PixelUI/9-Slice/Colored/";
    private const string TileAssetFolder = "Assets/Art/GoldenZone/TileAssets";

    [MenuItem("ChronoDungeon/Build Golden Zone 01 - Ruined Entrance")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building the Golden Zone.");
        NormalizeKenneyImport(CaveAtlas);
        NormalizeKenneyImport(RpgAtlas);
        NormalizeKenneyImport(CharacterAtlas);

        RemoveLegacyZone01Presentation();
        DisableLegacyWorldPresentation();
        Room zone = FindZoneRoom();
        if (zone == null) throw new InvalidOperationException("Zone 1 Room was not found.");
        GameObject root = PrepareZoneRoot(zone);
        GameObject gameplay = PrepareGameplay(zone, root);
        Transform grid = PrepareChild(root.transform, "Grid");
        UnityEngine.Grid gridComponent = grid.GetComponent<UnityEngine.Grid>();
        if (gridComponent == null) gridComponent = grid.gameObject.AddComponent<UnityEngine.Grid>();
        gridComponent.cellSize = Vector3.one;
        Transform props = PrepareChild(root.transform, "Props");
        Transform vfx = PrepareChild(root.transform, "VFX");
        Transform zoneExit = PrepareChild(root.transform, "ZoneExit");

        ClearGeneratedTileAssets();
        // Use the stone-floor and dark-rock bands from the actual dungeon atlas.
        // The earlier 160-series band reads as a single brown slab at game-view scale.
        Sprite[] floorSprites = Sprites(CaveAtlas, 160, 161, 162, 163);
        Sprite[] wallSprites = Sprites(CaveAtlas, 64, 65, 66, 67);
        Sprite[] backgroundSprites = Sprites(CaveAtlas, 40, 41, 42, 43);
        TileBase[] floors = MakeTiles("ground", floorSprites, Tile.ColliderType.None);
        TileBase[] walls = MakeTiles("wall", wallSprites, Tile.ColliderType.Grid);
        TileBase[] backgrounds = MakeTiles("background", backgroundSprites, Tile.ColliderType.None);

        Tilemap background = PrepareTilemap(grid, "BackgroundTilemap", -30);
        Tilemap ground = PrepareTilemap(grid, "GroundTilemap", -20);
        Tilemap wall = PrepareTilemap(grid, "WallTilemap", -10);
        Tilemap decoration = PrepareTilemap(grid, "DecorationTilemap", 0);
        Tilemap foreground = PrepareTilemap(grid, "ForegroundTilemap", 20);
        ClearTilemap(background); ClearTilemap(ground); ClearTilemap(wall); ClearTilemap(decoration); ClearTilemap(foreground);
        ConfigureWallCollider(wall);
        BuildTileLayout(background, ground, wall, floors, walls, backgrounds);
        BuildProps(props, vfx, zoneExit);
        PrepareGameplayVisuals(zone, gameplay, props, zoneExit.gameObject);
        ConfigureCameraAndRoom(zone);
        ConfigureActors(zone);
        ConfigureZoneHud(zone);
        SaveScene();
        Debug.Log("Golden Zone 01 built: Ruined Entrance Tilemap, authored sub-areas, Kenney actor scale, and Zone HUD.");
    }

    private static void RemoveLegacyZone01Presentation()
    {
        GameObject world = GameObject.Find("ChronoWorld");
        if (world == null) return;
        var remove = new List<GameObject>();
        foreach (Transform child in world.transform)
            if (child.name == "Zone01_RuinedEntrance" && child.GetComponent<Room>() == null) remove.Add(child.gameObject);
        foreach (GameObject item in remove) UnityEngine.Object.DestroyImmediate(item);
    }

    private static void DisableLegacyWorldPresentation()
    {
        // Keep the old Zone 2-8 objects in the scene for later work, but make the
        // golden Zone 1 template the only visible authored world presentation.
        GameObject world = GameObject.Find("ChronoWorld");
        if (world != null)
        {
            foreach (Transform child in world.transform) child.gameObject.SetActive(false);
        }

        GameObject legacyLogic = GameObject.Find("LegacyGameplay_RoomLogic");
        if (legacyLogic == null) return;
        foreach (Transform room in legacyLogic.transform)
        {
            if (room.name == "Zone01_RuinedEntrance") continue;
            Transform content = room.Find("Content");
            if (content == null) continue;
            Transform authored = content.Find("Authored Layout");
            if (authored != null) authored.gameObject.SetActive(false);
            Transform oldArt = content.Find("Zone 1 Art");
            if (oldArt != null) oldArt.gameObject.SetActive(false);
            foreach (TextMesh text in content.GetComponentsInChildren<TextMesh>(true))
                text.gameObject.SetActive(false);
        }
    }

    private static Room FindZoneRoom()
    {
        foreach (Room room in UnityEngine.Object.FindObjectsByType<Room>(FindObjectsInactive.Include))
            if (room.DisplayName.Contains("RUINED ENTRANCE") || room.name == "01 Threshold" || room.name == "Zone01_RuinedEntrance") return room;
        return null;
    }

    private static GameObject PrepareZoneRoot(Room zone)
    {
        GameObject root = zone.gameObject;
        root.name = "Zone01_RuinedEntrance";
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root;
    }

    private static GameObject PrepareGameplay(Room zone, GameObject root)
    {
        GameObject gameplay = zone.Content;
        if (gameplay == null) gameplay = new GameObject("Gameplay");
        gameplay.name = "Gameplay";
        gameplay.transform.SetParent(root.transform, false);
        var builder = gameplay.GetComponent<Zone1LayoutBuilder>();
        if (builder != null) UnityEngine.Object.DestroyImmediate(builder);
        var dresser = gameplay.GetComponent<Zone1VisualDresser>();
        if (dresser != null) UnityEngine.Object.DestroyImmediate(dresser);

        Transform generated = gameplay.transform.Find("Zone 1 Environment");
        if (generated != null) UnityEngine.Object.DestroyImmediate(generated.gameObject);
        Transform authored = gameplay.transform.Find("Authored Layout");
        if (authored != null) authored.gameObject.SetActive(false);
        RemoveLegacyGameplayVisuals(gameplay.transform);
        PrepareChild(gameplay.transform, "Enemies");
        PrepareChild(gameplay.transform, "RuneKey");
        PrepareChild(gameplay.transform, "RuneGate");
        PrepareChild(gameplay.transform, "Triggers");

        MoveChildrenWithComponent(gameplay.transform, gameplay.transform.Find("Enemies"), typeof(EnemyFollow));
        MoveChildrenWithComponent(gameplay.transform, gameplay.transform.Find("RuneKey"), typeof(RuneKey));
        MoveChildrenWithComponent(gameplay.transform, gameplay.transform.Find("Triggers"), typeof(SubArea));
        MoveChildrenWithComponent(gameplay.transform, gameplay.transform.Find("Triggers"), typeof(SubAreaTransition));
        MoveChildrenWithComponent(gameplay.transform, gameplay.transform.Find("Triggers"), typeof(AreaBounds));

        RoomBoundary boundary = zone.GetComponentInChildren<RoomBoundary>(true);
        if (boundary != null)
        {
            Set(boundary, "width", 120f); Set(boundary, "height", 110f);
            Set(boundary, "centerOffset", new Vector2(0f, -4f)); Set(boundary, "exitOpening", 12f);
            boundary.Rebuild();
        }
        return gameplay;
    }

    private static void RemoveLegacyGameplayVisuals(Transform gameplay)
    {
        var keep = new HashSet<string>(StringComparer.Ordinal) { "Enemies", "RuneKey", "RuneGate", "Triggers", "Solid Room Boundaries" };
        var preserveComponents = new[] { typeof(EnemyFollow), typeof(RuneKey), typeof(SubArea), typeof(SubAreaTransition), typeof(AreaBounds) };
        var remove = new List<GameObject>();
        foreach (Transform child in gameplay.GetComponentsInChildren<Transform>(true))
        {
            if (child == gameplay || !child.parent || child.parent != gameplay) continue;
            if (keep.Contains(child.name)) continue;
            if (child.name == "Exit Label" || child.name == "Objective_RUINED_ENTRANCE") { remove.Add(child.gameObject); continue; }
            bool preserve = false;
            foreach (Type type in preserveComponents) if (child.GetComponentInChildren(type, true) != null) { preserve = true; break; }
            if (!preserve && (child.GetComponentInChildren<SpriteRenderer>(true) != null || child.name.Contains("Environment") || child.name.Contains("Floor"))) remove.Add(child.gameObject);
        }
        foreach (GameObject go in remove) if (go != null) UnityEngine.Object.DestroyImmediate(go);
    }

    private static void PrepareGameplayVisuals(Room zone, GameObject gameplay, Transform props, GameObject zoneExit)
    {
        RuneKey key = gameplay.GetComponentInChildren<RuneKey>(true);
        if (key != null)
        {
            key.transform.position = new Vector3(20f, 38f, 0f);
            RemoveChildrenNamed(key.transform, "Rune Key Glow");
            CreateSprite(key.transform, "Rune Key", Sprite(CaveAtlas, 80), Vector3.zero, new Vector2(1.4f, 1.4f), 34, new Color(.25f, 1f, .86f, 1f));
            CreateSprite(key.transform, "Rune Key Aura", Sprite(ParticlePath + "magic_03.png"), Vector3.zero, new Vector2(1.8f, 1.8f), 33, new Color(.1f, .9f, 1f, .35f));
        }

        Door door = zone.ExitDoor;
        if (door == null)
        {
            GameObject doorObject = new GameObject("Room Exit Door", typeof(BoxCollider2D), typeof(Door), typeof(RuneGate));
            doorObject.transform.SetParent(gameplay.transform.Find("RuneGate"), false);
            door = doorObject.GetComponent<Door>();
            Set(zone, "exitDoor", door);
        }
        if (door != null)
        {
            door.transform.position = new Vector3(42f, -48f, 0f);
            BoxCollider2D blocker = door.GetComponent<BoxCollider2D>();
            if (blocker != null) blocker.size = new Vector2(2f, 6f);
            Set(door, "initiallyOpen", false);
            DisableDoorVisuals(door.transform);
            Transform runeGate = gameplay.transform.Find("RuneGate");
            door.transform.SetParent(runeGate, true);
            CreateGateArt(runeGate, zoneExit);
        }
        RoomExit exit = zone.GetComponentInChildren<RoomExit>(true);
        if (exit != null)
        {
            exit.transform.position = new Vector3(42f, -56f, 0f);
            BoxCollider2D trigger = exit.GetComponent<BoxCollider2D>();
            if (trigger != null) trigger.size = new Vector2(8f, 2.5f);
            exit.transform.SetParent(zoneExit.transform, true);
        }

        RuneKeyInventory inventory = FindAnyUnityObject<RuneKeyInventory>();
        if (key != null && inventory != null) Set(key, "inventory", inventory);
        if (door != null)
        {
            RuneGate gate = door.GetComponent<RuneGate>();
            if (gate == null) gate = door.gameObject.AddComponent<RuneGate>();
            if (inventory != null) Set(gate, "inventory", inventory);
        }
    }

    private static void BuildTileLayout(Tilemap background, Tilemap ground, Tilemap wall, TileBase[] floors, TileBase[] walls, TileBase[] backgrounds)
    {
        background.color = new Color(.42f, .52f, .58f, 1f);
        ground.color = new Color(.92f, .95f, .95f, 1f);
        wall.color = Color.white;
        decorationColor(background, new Color(.55f, .64f, .68f, 1f));

        for (int y = -64; y <= 54; y++)
            for (int x = -66; x <= 66; x++) background.SetTile(new Vector3Int(x, y, 0), backgrounds[(x * 7 + y * 3 & int.MaxValue) % backgrounds.Length]);

        var cells = new HashSet<Vector2Int>();
        AddRect(cells, -56, -9, 29, 19);
        AddRect(cells, -27, -3, 20, 7);
        AddRect(cells, -8, -12, 39, 25);
        AddRect(cells, 16, 13, 9, 16);
        AddRect(cells, 7, 28, 28, 21);
        AddRect(cells, 30, -3, 9, 7);
        AddRect(cells, 38, -39, 9, 36);
        AddRect(cells, 27, -56, 32, 18);

        foreach (Vector2Int cell in cells)
        {
            int index = (Mathf.Abs(cell.x * 13 + cell.y * 17) % floors.Length);
            ground.SetTile((Vector3Int)cell, floors[index]);
        }
        foreach (Vector2Int cell in cells)
        {
            bool edge = false;
            foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                if (!cells.Contains(cell + d)) { edge = true; break; }
            if (edge) wall.SetTile((Vector3Int)cell, walls[Mathf.Abs(cell.x + cell.y) % walls.Length]);
        }
    }

    private static void AddRect(HashSet<Vector2Int> cells, int x, int y, int width, int height)
    { for (int iy = y; iy < y + height; iy++) for (int ix = x; ix < x + width; ix++) cells.Add(new Vector2Int(ix, iy)); }

    private static void BuildProps(Transform props, Transform vfx, Transform zoneExit)
    {
        Sprite rubble = Sprite(CaveAtlas, 60);
        Sprite stone = Sprite(CaveAtlas, 80);
        Sprite pillar = Sprite(RpgAtlas, 280);
        Sprite torch = Sprite(ParticlePath + "flame_01.png");
        Vector2[] pillars = { new(-48, 6), new(-35, -6), new(-2, 8), new(10, -5), new(23, 6), new(21, 34), new(30, 43), new(49, -43) };
        for (int i = 0; i < pillars.Length; i++) CreateSprite(props, "Kenney Pillar " + i, pillar, pillars[i], new Vector2(1.8f, 1.8f), 4, new Color(.9f, .95f, .95f, 1f));
        Vector2[] rocks = { new(-50, -7), new(-13, 2), new(-4, -9), new(7, 7), new(18, 9), new(28, -7), new(12, 31), new(29, 31), new(51, -52) };
        for (int i = 0; i < rocks.Length; i++) CreateSprite(props, "Kenney Rubble " + i, i % 2 == 0 ? rubble : stone, rocks[i], new Vector2(1.25f, 1.25f), 5, new Color(.8f, .86f, .88f, 1f));
        Vector2[] torches = { new(-50, 7), new(-31, -7), new(-2, 10), new(24, 10), new(30, 45), new(49, -41) };
        for (int i = 0; i < torches.Length; i++)
        {
            CreateSprite(vfx, "Torch Flame " + i, torch, torches[i] + Vector2.up * .6f, new Vector2(.7f, .7f), 18, new Color(1f, .55f, .16f, .95f));
            CreateSprite(vfx, "Torch Glow " + i, Sprite(ParticlePath + "circle_03.png"), torches[i], new Vector2(2.2f, 2.2f), 16, new Color(1f, .28f, .08f, .16f));
        }
        CreateSprite(vfx, "Courtyard Rune", Sprite(ParticlePath + "magic_02.png"), new Vector2(10f, 9.5f), new Vector2(2.6f, 2.6f), 17, new Color(.2f, .85f, 1f, .42f));
        CreateSprite(vfx, "Side Ruins Rune", Sprite(ParticlePath + "symbol_01.png"), new Vector2(20f, 38f), new Vector2(2.4f, 2.4f), 17, new Color(.2f, .85f, 1f, .65f));
    }

    private static void CreateGateArt(Transform gate, GameObject zoneExit)
    {
        Sprite arch = Sprite(CaveAtlas, 220);
        CreateSprite(zoneExit.transform, "Rune Gate Left Pillar", arch, new Vector3(37.5f, -48f, 0f), new Vector2(2.2f, 5.5f), 8, Color.white);
        CreateSprite(zoneExit.transform, "Rune Gate Right Pillar", arch, new Vector3(46.5f, -48f, 0f), new Vector2(2.2f, 5.5f), 8, Color.white);
        CreateSprite(zoneExit.transform, "Rune Gate Lintel", arch, new Vector3(42f, -44.8f, 0f), new Vector2(8.8f, 2.1f), 8, Color.white);
        CreateSprite(zoneExit.transform, "Rune Gate Sigil", Sprite(ParticlePath + "magic_04.png"), new Vector3(42f, -48f, 0f), new Vector2(3f, 3f), 10, new Color(.12f, 1f, .9f, .8f));
    }

    private static void ConfigureCameraAndRoom(Room zone)
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.orthographic = true; camera.orthographicSize = 9.5f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.02f, .035f, .05f, 1f);
            ZoneCameraController controller = camera.GetComponent<ZoneCameraController>();
            if (controller == null) controller = camera.gameObject.AddComponent<ZoneCameraController>();
            Set(controller, "areas", FindAnyUnityObject<WorldAreaManager>());
            Set(controller, "player", FindAnyUnityObject<PlayerMovement>());
            Set(controller, "useGlobalZoneBounds", true);
            Set(controller, "globalBoundsCenter", new Vector2(0f, -4f));
            Set(controller, "globalBoundsSize", new Vector2(126f, 112f));
        }
        if (zone.SpawnPoint != null) zone.SpawnPoint.position = new Vector3(-47f, 0f, 0f);
        if (zone.ExitDoor != null) zone.ExitDoor.transform.position = new Vector3(42f, -48f, 0f);
    }

    private static void ConfigureActors(Room zone)
    {
        Sprite playerSprite = Sprite(CharacterAtlas, 0);
        PlayerMovement player = FindAnyUnityObject<PlayerMovement>();
        if (player != null)
        {
            player.transform.position = new Vector3(-47f, 0f, 0f);
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sprite = playerSprite; sr.color = Color.white; sr.sortingOrder = 30; }
            player.transform.localScale = Vector3.one * 1.35f;
            RemoveChildrenByNames(player.transform, "Ember Body", "Brow Left", "Brow Right", "Eye Left", "Eye Right", "Mouth", "Fang Left", "Fang Right", "Stylized Player Visual");
            CreateSprite(player.transform, "Player Shadow", Sprite(ParticlePath + "circle_02.png"), new Vector3(0f, -.48f, 0f), new Vector2(1.15f, .36f), 25, new Color(0f, 0f, 0f, .48f));
            CreateSprite(player.transform, "Temporal Cyan Accent", Sprite(ParticlePath + "magic_01.png"), new Vector3(0f, .05f, 0f), new Vector2(1.35f, 1.35f), 29, new Color(.1f, .85f, 1f, .18f));
        }
        int variant = 0;
        foreach (EnemyFollow enemy in zone.GetComponentsInChildren<EnemyFollow>(true))
        {
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // The atlas rows 39/78 are equipment fragments, not readable
                // character bodies. Use the full Kenney character frame and tint
                // variants so enemies remain legible against the dungeon floor.
                sr.sprite = Sprite(CharacterAtlas, 0);
                sr.color = variant % 2 == 0 ? new Color(1f, .48f, .28f, 1f) : new Color(.78f, .86f, 1f, 1f);
                sr.sortingOrder = 30;
                sr.enabled = true;
            }
            enemy.transform.localScale = Vector3.one * 1.35f;
            RemoveChildrenByNames(enemy.transform, "Ember Body", "Brow Left", "Brow Right", "Eye Left", "Eye Right", "Mouth", "Fang Left", "Fang Right", "Stylized Enemy Visual");
            CreateSprite(enemy.transform, "Enemy Shadow", Sprite(ParticlePath + "circle_02.png"), new Vector3(0f, -.42f, 0f), new Vector2(.95f, .3f), 25, new Color(0f, 0f, 0f, .5f));
            variant++;
        }
    }

    private static void ConfigureZoneHud(Room zone)
    {
        GameObject canvasGo = null;
        foreach (Canvas candidate in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (candidate.name == "Canvas") { canvasGo = candidate.gameObject; break; }
        }
        if (canvasGo == null) return;
        canvasGo.SetActive(true);
        canvasGo.transform.localScale = Vector3.one;
        Transform uiRoot = canvasGo.transform.Find("Chrono Safe Area") ?? canvasGo.transform;
        foreach (TimeLoopHUD loopHud in UnityEngine.Object.FindObjectsByType<TimeLoopHUD>(FindObjectsInactive.Include)) loopHud.gameObject.SetActive(false);
        foreach (RoomHUD roomHud in UnityEngine.Object.FindObjectsByType<RoomHUD>(FindObjectsInactive.Include)) roomHud.gameObject.SetActive(false);
        foreach (ChronoGuardianHUD bossHud in UnityEngine.Object.FindObjectsByType<ChronoGuardianHUD>(FindObjectsInactive.Include))
        {
            bossHud.enabled = false;
        }
        Transform bossPanel = uiRoot.Find("Chrono Guardian HUD"); if (bossPanel != null) bossPanel.gameObject.SetActive(false);
        VerticalSlicePresentation presentation = FindAnyUnityObject<VerticalSlicePresentation>();
        if (presentation != null) presentation.enabled = false;
        Transform temporalKey = FindDescendant(uiRoot, "KenneyTemporalKey"); if (temporalKey != null) temporalKey.gameObject.SetActive(false);
        Transform oldFull = FindDescendant(uiRoot, "FullRebuildHUD"); if (oldFull != null) oldFull.gameObject.SetActive(true);
        Transform gameOver = FindDescendant(uiRoot, "GameOverPanel"); if (gameOver != null) gameOver.gameObject.SetActive(false);
        Transform tutorial = FindDescendant(uiRoot, "Tutorial Guide"); if (tutorial != null) tutorial.gameObject.SetActive(false);
        Transform upgrade = FindDescendant(uiRoot, "Upgrade Choice Panel"); if (upgrade != null) upgrade.gameObject.SetActive(false);
        Transform victory = FindDescendant(uiRoot, "Victory Panel"); if (victory != null) victory.gameObject.SetActive(false);
        GoldenZonePresentation presentationGuard = canvasGo.GetComponent<GoldenZonePresentation>();
        if (presentationGuard == null) presentationGuard = canvasGo.AddComponent<GoldenZonePresentation>();

        StyleHealth(FindDescendant(uiRoot, "PlayerHealthBar"));
        StyleJoystick(FindDescendant(uiRoot, "JoystickBG"), FindDescendant(uiRoot, "JoystickHandle"));
        StyleButton(FindDescendant(uiRoot, "AttackButton"), UiPath + "red.png", "ATTACK", new Vector2(1f, 0f), new Vector2(-54f, 54f), new Vector2(150f, 150f));
        StyleButton(FindDescendant(uiRoot, "DashButton"), UiPath + "blue.png", "DASH", new Vector2(1f, 0f), new Vector2(-218f, 54f), new Vector2(150f, 150f));
        Transform objective = FindDescendant(uiRoot, "KenneyObjective");
        if (objective != null)
        {
            RectTransform rect = objective as RectTransform; Anchor(rect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-42f, -42f), new Vector2(540f, 78f));
            Image image = objective.GetComponent<Image>(); if (image != null) { image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiPath + "blue.png"); image.type = Image.Type.Sliced; image.color = new Color(.05f, .18f, .25f, .92f); }
            TMP_Text label = objective.GetComponentInChildren<TMP_Text>(true);
            if (label != null) { label.text = "EXPLORE THE RUINED ENTRANCE"; label.fontSize = 22f; label.color = new Color(.78f, .96f, 1f); label.alignment = TextAlignmentOptions.Center; }
            GoldenZoneHUD hud = objective.GetComponent<GoldenZoneHUD>(); if (hud == null) hud = objective.gameObject.AddComponent<GoldenZoneHUD>();
            Set(hud, "objectiveLabel", label); Set(hud, "player", FindAnyUnityObject<PlayerMovement>()); Set(hud, "inventory", FindAnyUnityObject<RuneKeyInventory>()); Set(hud, "zoneRoom", zone);
        }
        Transform title = FindDescendant(uiRoot, "KenneyTopBar"); if (title != null) title.gameObject.SetActive(false);
        Transform health = FindDescendant(uiRoot, "PlayerHealthBar");
        if (health != null)
        {
            TMP_Text tag = health.GetComponentInChildren<TMP_Text>(true);
            if (tag == null) tag = CreateLabel(health, "HP", new Vector2(20f, -6f), new Vector2(60f, 28f), 18f);
            tag.text = "HP"; tag.color = new Color(.8f, .95f, 1f); tag.fontStyle = FontStyles.Bold;
        }
    }

    private static void StyleHealth(Transform health)
    {
        if (health == null) return;
        RectTransform rect = health as RectTransform; Anchor(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -42f), new Vector2(330f, 42f));
        Image background = health.Find("Background")?.GetComponent<Image>(); if (background != null) background.color = new Color(.04f, .08f, .11f, .9f);
        Image fill = health.Find("Fill")?.GetComponent<Image>(); if (fill != null) fill.color = new Color(.15f, .85f, .7f, 1f);
    }

    private static void StyleJoystick(Transform bg, Transform handle)
    {
        if (bg == null || handle == null) return;
        RectTransform r = bg as RectTransform; Anchor(r, Vector2.zero, Vector2.zero, new Vector2(.5f, .5f), new Vector2(138f, 138f), new Vector2(190f, 190f));
        RectTransform h = handle as RectTransform; Anchor(h, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(82f, 82f));
        Image bgImage = bg.GetComponent<Image>(); if (bgImage != null) { bgImage.sprite = Sprite(ParticlePath + "circle_03.png"); bgImage.color = new Color(.08f, .45f, .55f, .42f); }
        Image handleImage = handle.GetComponent<Image>(); if (handleImage != null) { handleImage.sprite = Sprite(ParticlePath + "circle_02.png"); handleImage.color = new Color(.35f, .9f, 1f, .78f); }
        VirtualJoystick joystick = bg.GetComponent<VirtualJoystick>(); if (joystick != null) { joystick.joystickBG = r; joystick.joystickHandle = h; joystick.playerMovement = FindAnyUnityObject<PlayerMovement>(); joystick.handleRange = 54f; }
    }

    private static void StyleButton(Transform button, string spritePath, string labelText, Vector2 anchor, Vector2 position, Vector2 size)
    {
        if (button == null) return;
        RectTransform r = button as RectTransform; Anchor(r, anchor, anchor, new Vector2(1f, 0f), position, size);
        Image image = button.GetComponent<Image>(); if (image != null) { image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath); image.type = Image.Type.Sliced; image.color = Color.white; }
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true); if (label == null) label = CreateLabel(button, labelText, Vector2.zero, size, 22f); else { label.text = labelText; label.fontSize = 22f; }
        label.alignment = TextAlignmentOptions.Center; label.color = Color.white; label.fontStyle = FontStyles.Bold;
    }

    private static TMP_Text CreateLabel(Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>(); Anchor(r, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), position, size);
        TMP_Text label = go.GetComponent<TMP_Text>(); label.text = text; label.fontSize = fontSize; label.alignment = TextAlignmentOptions.Center; return label;
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = pivot; rect.anchoredPosition = pos; rect.sizeDelta = size; }

    private static Transform PrepareChild(Transform parent, string name)
    { Transform t = parent.Find(name); if (t != null) return t; var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }

    private static Transform FindDescendant(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            Transform found = FindDescendant(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private static void MoveChildrenWithComponent(Transform parent, Transform destination, Type type)
    {
        if (destination == null) return;
        var list = new List<Transform>(); foreach (Transform child in parent) if (child != destination && child.GetComponent(type) != null) list.Add(child);
        foreach (Transform child in list) child.SetParent(destination, true);
    }

    private static Tilemap PrepareTilemap(Transform parent, string name, int sortingOrder)
    {
        Transform t = PrepareChild(parent, name); Tilemap map = t.GetComponent<Tilemap>(); if (map == null) map = t.gameObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = t.GetComponent<TilemapRenderer>(); if (renderer == null) renderer = t.gameObject.AddComponent<TilemapRenderer>(); renderer.sortingOrder = sortingOrder; renderer.mode = TilemapRenderer.Mode.Individual;
        return map;
    }

    private static void ConfigureWallCollider(Tilemap wall)
    {
        TilemapCollider2D collider = wall.GetComponent<TilemapCollider2D>(); if (collider == null) collider = wall.gameObject.AddComponent<TilemapCollider2D>();
        collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        Rigidbody2D body = wall.GetComponent<Rigidbody2D>(); if (body == null) body = wall.gameObject.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D composite = wall.GetComponent<CompositeCollider2D>(); if (composite == null) composite = wall.gameObject.AddComponent<CompositeCollider2D>(); composite.geometryType = CompositeCollider2D.GeometryType.Polygons; composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
    }

    private static void ClearTilemap(Tilemap map) { if (map != null) map.ClearAllTiles(); }

    private static TileBase[] MakeTiles(string prefix, Sprite[] sprites, Tile.ColliderType colliderType)
    {
        var result = new TileBase[sprites.Length]; for (int i = 0; i < sprites.Length; i++)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>(); tile.name = prefix + "_" + i; tile.sprite = sprites[i]; tile.colliderType = colliderType;
            AssetDatabase.CreateAsset(tile, TileAssetFolder + "/" + tile.name + ".asset"); result[i] = tile;
        }
        AssetDatabase.SaveAssets(); return result;
    }

    private static void ClearGeneratedTileAssets()
    {
        EnsureFolder("Assets/Art"); EnsureFolder("Assets/Art/GoldenZone"); EnsureFolder(TileAssetFolder);
        foreach (string path in AssetDatabase.FindAssets("t:Tile", new[] { TileAssetFolder })) AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(path));
    }

    private static void EnsureFolder(string path)
    { if (!AssetDatabase.IsValidFolder(path)) { string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/'); string name = System.IO.Path.GetFileName(path); AssetDatabase.CreateFolder(parent, name); } }

    private static Sprite[] Sprites(string path, params int[] ids) { var result = new Sprite[ids.Length]; for (int i = 0; i < ids.Length; i++) result[i] = Sprite(path, ids[i]); return result; }
    private static Sprite Sprite(string path, int id) { return Sprite(path, "_" + id); }
    private static Sprite Sprite(string path)
    {
        Sprite single = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (single != null) return single;
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if (asset is Sprite sprite) return sprite;
        return null;
    }

    private static Sprite Sprite(string path, string suffix)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) if (asset is Sprite sprite && sprite.name.EndsWith(suffix, StringComparison.Ordinal)) return sprite;
        return null;
    }

    private static void NormalizeKenneyImport(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter; if (importer == null) return;
        importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.spritePixelsPerUnit = 16; importer.mipmapEnabled = false; importer.alphaIsTransparency = true; AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    private static GameObject CreateSprite(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector2 scale, int order, Color color)
    {
        Transform existing = parent.Find(name); if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        GameObject go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = localPosition; go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order; renderer.maskInteraction = SpriteMaskInteraction.None; return go;
    }

    private static void DisableDoorVisuals(Transform root) { foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled = false; }
    private static void RemoveChildrenNamed(Transform root, string name) { Transform child = root.Find(name); if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject); }
    private static void RemoveChildrenByNames(Transform root, params string[] names) { foreach (string name in names) RemoveChildrenNamed(root, name); }

    private static void decorationColor(Tilemap map, Color color) { if (map != null) map.color = color; }
    private static T FindAnyUnityObject<T>() where T : UnityEngine.Object { return UnityEngine.Object.FindAnyObjectByType<T>(); }
    private static void SaveScene() { EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets(); }

    private static void Set(UnityEngine.Object target, string field, object value)
    {
        if (target == null) return; SerializedObject serialized = new SerializedObject(target); SerializedProperty property = serialized.FindProperty(field); if (property == null) return;
        if (value is string s) property.stringValue = s; else if (value is bool b) property.boolValue = b; else if (value is int i) property.intValue = i; else if (value is float f) property.floatValue = f; else if (value is Vector2 v2) property.vector2Value = v2; else if (value is Color c) property.colorValue = c; else if (value is UnityEngine.Object obj) property.objectReferenceValue = obj;
        serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target);
    }
}
