using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

/// Authored world replacement for GameScene.  This is intentionally an editor
/// rebuild command: the resulting hierarchy is saved into the scene and does
/// not depend on a runtime procedural room generator.
public static class FullWorldRebuild
{
    private const string WorldRootName = "ChronoWorld";
    private const string RpgPath = "Assets/ThirdParty/kenney/Environment/RPG/roguelikeSheet_transparent.png";
    private const string CavePath = "Assets/ThirdParty/kenney/Environment/CavesDungeon/roguelikeDungeon_transparent.png";
    private const string CharPath = "Assets/ThirdParty/kenney/Characters/Roguelike/roguelikeChar_transparent.png";
    private const string ParticlePath = "Assets/ThirdParty/kenney/VFX/Particles/magic_01.png";
    private static Sprite floorSprite;
    private static Sprite wallSprite;
    private static Sprite[] characterSprites;
    private static Sprite temporalSprite;
    private static Sprite opaqueSprite;
    private static Material unlitMaterial;

    private sealed class ZoneSpec
    {
        public string id, title, subtitle;
        public Vector2 position;
        public Color tint;
        public bool loop;
        public string[] areas;
        public ZoneSpec(string id, string title, string subtitle, Vector2 position, Color tint, bool loop, params string[] areas)
        { this.id = id; this.title = title; this.subtitle = subtitle; this.position = position; this.tint = tint; this.loop = loop; this.areas = areas; }
    }

    private static readonly ZoneSpec[] Specs =
    {
        new ZoneSpec("Zone01_RuinedEntrance", "RUINED ENTRANCE", "Broken Gate · Rune Key", new Vector2(-150f, 0f), new Color(.68f,.78f,.72f), false, "Broken Gate", "Ruined Passage", "Fallen Courtyard", "Side Ruins", "Rune Gate"),
        new ZoneSpec("Zone02_ForgottenBarracks", "FORGOTTEN BARRACKS", "Guard quarters · Captain Gate", new Vector2(-58f, 0f), new Color(.78f,.64f,.42f), false, "Barracks Entry", "Guard Hall", "Storage", "Training Yard", "Captain Gate"),
        new ZoneSpec("Zone03_OvergrownWilds", "OVERGROWN WILDS", "Roots consume the old dungeon", new Vector2(34f, 0f), new Color(.36f,.68f,.46f), false, "Root Passage", "Overgrown Hall", "Split Route", "Combat Clearing", "Rune Garden"),
        new ZoneSpec("Zone04_EchoLibrary", "ECHO LIBRARY", "The archive remembers you", new Vector2(106f, 52f), new Color(.28f,.82f,.92f), true, "Echo Entrance", "Archive Hall", "Ghost Switch Room", "Twin Mechanism", "Echo Gate"),
        new ZoneSpec("Zone05_AshForge", "ASH FORGE", "Heat, iron and waking embers", new Vector2(190f, 52f), new Color(.92f,.48f,.22f), false, "Forge Entry", "Furnace Walk", "Hazard Chamber", "Smelter Arena", "Reward Passage"),
        new ZoneSpec("Zone06_ShatteredMirrorPrison", "SHATTERED MIRROR PRISON", "Every reflection is a past self", new Vector2(274f, -2f), new Color(.65f,.34f,.92f), true, "Prison Entry", "Mirror Cells", "Broken Reflection", "Multi-Ghost Chamber", "Temporal Lock"),
        new ZoneSpec("Zone07_WardenApproach", "WARDEN APPROACH", "The guardian waits beyond the shrine", new Vector2(360f, -2f), new Color(.78f,.50f,.82f), false, "Warden Hall", "Guard Gauntlet", "Elite Chamber", "Final Shrine", "Boss Gate"),
        new ZoneSpec("Zone08_ChronoSanctum", "CHRONO SANCTUM", "The final time core", new Vector2(442f, 52f), new Color(.38f,.82f,1f), true, "Grand Entrance", "Boss Arena", "Temporal Core", "Guardian Phase", "Victory Exit")
    };

    [MenuItem("ChronoDungeon/Full World Rebuild")]
    public static void Rebuild()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before rebuilding the world.");
        NormalizeImportSettings();
        LoadSprites();

        GameObject legacyRooms = GameObject.Find("Rooms");
        GameObject legacyEnvironment = GameObject.Find("Environment");
        StripPresentation(legacyRooms);
        StripPresentation(legacyEnvironment);
        if (legacyRooms != null) legacyRooms.name = "LegacyGameplay_RoomLogic";
        if (legacyEnvironment != null) legacyEnvironment.name = "LegacyGameplay_EnvironmentLogic";
        GameObject oldVerticalSlice = GameObject.Find("Chrono Vertical Slice World");
        if (oldVerticalSlice != null) oldVerticalSlice.SetActive(false);
        GameObject managers = GameObject.Find("Managers");
        if (managers != null)
        {
            VerticalSlicePresentation oldPresentation = managers.GetComponent<VerticalSlicePresentation>();
            if (oldPresentation != null) oldPresentation.enabled = false;
        }
        GameObject oldVisualDirection = GameObject.Find("Chrono Visual Direction");
        if (oldVisualDirection != null)
        {
            ChronoVisualReplacement oldPalette = oldVisualDirection.GetComponent<ChronoVisualReplacement>();
            if (oldPalette != null) oldPalette.enabled = false;
        }

        GameObject oldWorld = GameObject.Find(WorldRootName);
        if (oldWorld != null) UnityEngine.Object.DestroyImmediate(oldWorld);
        GameObject world = new GameObject(WorldRootName);
        Undo.RegisterCreatedObjectUndo(world, "Create ChronoWorld");

        for (int i = 0; i < Specs.Length; i++) BuildZone(world.transform, Specs[i], i);
        BuildConnections(world.transform);
        BuildWorldBackdrop(world.transform);
        ReplaceCharacterPresentation();
        RebuildHUDArt();
        ConfigureCamera();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = world;
        Debug.Log("ChronoDungeon full world rebuild complete: 8 authored zones, connected route, Kenney presentation, camera bounds and HUD art saved.");
    }

    private static void NormalizeImportSettings()
    {
        foreach (string path in new[] { RpgPath, CavePath, CharPath })
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 16f;
            SerializedObject importerSerialized = new SerializedObject(importer);
            SerializedProperty meshType = importerSerialized.FindProperty("m_SpriteMeshType");
            if (meshType != null) meshType.intValue = 0; // SpriteMeshType.FullRect
            importerSerialized.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
        }
    }

    private static void LoadSprites()
    {
        Sprite[] cave = AssetDatabase.LoadAllAssetsAtPath(CavePath).OfType<Sprite>().ToArray();
        Sprite[] rpg = AssetDatabase.LoadAllAssetsAtPath(RpgPath).OfType<Sprite>().ToArray();
        Sprite[] chars = AssetDatabase.LoadAllAssetsAtPath(CharPath).OfType<Sprite>().ToArray();
        // These are the 18px floor and vertical wall tiles from the inspected
        // CavesDungeon atlas; using sub-assets keeps all eight zones Kenney art.
        floorSprite = FindSprite(rpg, 1416) ?? FindSprite(cave, 225) ?? cave.FirstOrDefault();
        wallSprite = FindSprite(rpg, 1543) ?? FindSprite(cave, 258) ?? cave.FirstOrDefault();
        characterSprites = new[] { FindSprite(chars, 0), FindSprite(chars, 39), FindSprite(chars, 78), FindSprite(chars, 117) }.Where(s => s != null).ToArray();
        temporalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ParticlePath);
        opaqueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
    }

    private static Sprite FindSprite(Sprite[] sprites, int suffix)
    { return sprites.FirstOrDefault(s => s.name.EndsWith("_" + suffix, StringComparison.Ordinal)); }

    private static void StripPresentation(GameObject root)
    {
        if (root == null) return;
        foreach (SpriteRenderer r in root.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = false;
        foreach (ParticleSystem p in root.GetComponentsInChildren<ParticleSystem>(true)) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private static void BuildZone(Transform world, ZoneSpec spec, int index)
    {
        GameObject zone = new GameObject(spec.id);
        zone.transform.SetParent(world, false);
        zone.transform.position = spec.position;
        GameObject background = Child(zone.transform, "Background");
        GameObject floor = Child(zone.transform, "Floor");
        GameObject walls = Child(zone.transform, "Walls");
        GameObject props = Child(zone.transform, "Props");
        GameObject gameplay = Child(zone.transform, "Gameplay");
        Child(zone.transform, "VFX");
        Child(zone.transform, "Transition");

        Tile(background.transform, "Abyss Scenery", Vector2.zero, new Vector2(78f, 58f), new Color(.025f,.045f,.07f,1f), -30);
        Color floorTint = Color.Lerp(new Color(.34f, .40f, .45f, 1f), spec.tint, .28f);
        Tile(floor.transform, "Main Hall Floor", Vector2.zero, new Vector2(54f, 34f), floorTint, -20);
        Tile(floor.transform, "North Side Area", new Vector2(-18f, 20f), new Vector2(24f, 16f), floorTint * .86f, -19);
        Tile(floor.transform, "South Hallway", new Vector2(22f, -19f), new Vector2(20f, 10f), floorTint * .76f, -19);

        // Thick, visible segments leave a clear doorway on the route-facing side.
        Wall(walls.transform, "North Wall", new Vector2(0f, 18f), new Vector2(54f, 1.5f));
        Wall(walls.transform, "South Wall West", new Vector2(-19f, -18f), new Vector2(16f, 1.5f));
        Wall(walls.transform, "South Wall East", new Vector2(20f, -18f), new Vector2(14f, 1.5f));
        Wall(walls.transform, "West Wall Upper", new Vector2(-27f, 9f), new Vector2(1.5f, 16f));
        Wall(walls.transform, "West Wall Lower", new Vector2(-27f, -10f), new Vector2(1.5f, 12f));
        Wall(walls.transform, "East Wall Upper", new Vector2(27f, 10f), new Vector2(1.5f, 14f));
        Wall(walls.transform, "East Wall Lower", new Vector2(27f, -12f), new Vector2(1.5f, 10f));
        Wall(walls.transform, "Side Area North", new Vector2(-18f, 28f), new Vector2(24f, 1.3f));
        Wall(walls.transform, "Side Area West", new Vector2(-30f, 20f), new Vector2(1.3f, 16f));
        Wall(walls.transform, "Side Area East", new Vector2(-6f, 23f), new Vector2(1.3f, 10f));
        Wall(walls.transform, "South Hall West", new Vector2(13f, -24f), new Vector2(1.3f, 10f));
        Wall(walls.transform, "South Hall East", new Vector2(31f, -24f), new Vector2(1.3f, 10f));

        for (int p = 0; p < 5; p++)
        {
            float x = -19f + p * 9.5f;
            Landmark(props.transform, "Landmark " + (p + 1), new Vector2(x, (p % 2 == 0) ? 11f : -10f), spec.tint, index + p);
        }
        if (index == 0 || index == 2 || index == 4) Prop(props.transform, "Overgrown Ruin", new Vector2(-18f, 23f), 0, new Color(.35f,.58f,.38f));
        if (index == 1 || index == 4 || index == 6) Prop(props.transform, "Forge Torch", new Vector2(20f, 11f), 1, new Color(1f,.34f,.08f));
        if (spec.loop) TemporalObelisk(props.transform, "Temporal Obelisk", new Vector2(0f, 0f), spec.tint);
        if (index == 7) BossCore(props.transform, "Central Temporal Core", new Vector2(0f, 0f));

        for (int a = 0; a < spec.areas.Length; a++)
        {
            GameObject area = new GameObject(string.Format("SubArea_{0:00}_{1}", a + 1, spec.areas[a]));
            area.transform.SetParent(gameplay.transform, false);
            area.transform.localPosition = new Vector3(-20f + (a % 3) * 18f, 10f - (a / 3) * 18f, 0f);
            BoxCollider2D bounds = area.AddComponent<BoxCollider2D>();
            bounds.isTrigger = true;
            bounds.size = new Vector2(16f, 12f);
        }
        GameObject title = new GameObject("ZoneTitle_" + spec.id);
        title.transform.SetParent(props.transform, false);
        title.transform.localPosition = new Vector3(0f, 15.7f, 0f);
        TextMesh text = title.AddComponent<TextMesh>();
        text.text = "ZONE " + (index + 1) + "  ·  " + spec.title;
        text.fontSize = 36;
        text.characterSize = .11f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = spec.loop ? new Color(.35f, .95f, 1f) : new Color(.82f, .78f, .58f);
        CreateObjectiveMarker(gameplay.transform, spec, index);
    }

    private static void BuildConnections(Transform world)
    {
        Transform connections = Child(world, "WorldConnections").transform;
        Vector2[] points = Specs.Select(s => s.position).ToArray();
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 a = points[i], b = points[i + 1];
            if (Mathf.Abs(a.y - b.y) < 4f) Passage(connections, "Zone" + (i + 1) + "_to_" + (i + 2) + "_Hallway", (a + b) * .5f, new Vector2(Mathf.Abs(b.x - a.x) + 18f, 10f), i);
            else
            {
                Passage(connections, "Zone" + (i + 1) + "_VerticalPassage", new Vector2(a.x + (b.x - a.x) * .45f, a.y), new Vector2(12f, 10f), i);
                Passage(connections, "Zone" + (i + 1) + "_Bridge", new Vector2(a.x + (b.x - a.x) * .75f, b.y), new Vector2(Mathf.Abs(b.x - a.x) * .55f + 18f, 10f), i);
            }
            GateVisual(connections, "Gate_" + (i + 1), Vector2.Lerp(a, b, .5f), Specs[i].loop || Specs[i + 1].loop);
        }
    }

    private static void BuildWorldBackdrop(Transform world)
    {
        if (opaqueSprite != null)
        {
            GameObject foundation = new GameObject("Deep Navy Foundation");
            foundation.transform.SetParent(world, false);
            foundation.transform.position = new Vector3(150f, 25f, 10f);
            foundation.transform.localScale = new Vector3(700f, 220f, 1f);
            SpriteRenderer foundationRenderer = foundation.AddComponent<SpriteRenderer>();
            foundationRenderer.sprite = opaqueSprite;
            foundationRenderer.sharedMaterial = UnlitMaterial();
            foundationRenderer.color = new Color(.012f, .026f, .05f, 1f);
            foundationRenderer.sortingOrder = -100;
        }
        Tile(world, "Distant Dungeon Expanse", new Vector2(150f, 25f), new Vector2(680f, 170f), new Color(.012f,.024f,.04f,1f), -50);
        Transform scenery = Child(world, "DistantScenery").transform;
        for (int i = 0; i < 28; i++)
        {
            float x = -185f + i * 24f;
            Prop(scenery, "Distant Ruin " + i, new Vector2(x, 88f + (i % 3) * 4f), 0, new Color(.12f,.18f,.22f,.75f));
        }
    }

    private static void Passage(Transform parent, string name, Vector2 position, Vector2 size, int index)
    { Tile(parent, name + " Floor", position, size, Specs[index].tint * .52f + Color.black * .2f, -18); }

    private static void GateVisual(Transform parent, string name, Vector2 position, bool temporal)
    {
        GameObject gate = new GameObject(name);
        gate.transform.SetParent(parent, false);
        gate.transform.position = position;
        SpriteRenderer r = gate.AddComponent<SpriteRenderer>();
        r.sprite = wallSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = temporal ? new Color(.22f,.9f,1f,.95f) : new Color(.72f,.48f,.24f,.95f);
        r.sortingOrder = -5;
        gate.transform.localScale = new Vector3(2.8f, 2.8f, 1f);
        if (temporal) AddTemporalGlow(gate.transform, "Gate Temporal Glow", 1.7f, new Color(.28f,.9f,1f,.65f));
    }

    private static GameObject Tile(Transform parent, string name, Vector2 position, Vector2 size, Color color, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = floorSprite;
        r.sharedMaterial = UnlitMaterial();
        r.drawMode = SpriteDrawMode.Simple;
        if (floorSprite != null && floorSprite.bounds.size.x > 0f && floorSprite.bounds.size.y > 0f)
            go.transform.localScale = new Vector3(size.x / floorSprite.bounds.size.x, size.y / floorSprite.bounds.size.y, 1f);
        r.color = color;
        r.sortingOrder = order;
        return go;
    }

    private static void Wall(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject go = Tile(parent, name, position, size, new Color(.36f,.43f,.48f,1f), -7);
        BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
    }

    private static void Landmark(Transform parent, string name, Vector2 pos, Color tint, int index)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = characterSprites.Length > 0 ? characterSprites[index % characterSprites.Length] : floorSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = Color.Lerp(Color.white, tint, .35f);
        r.sortingOrder = 2;
        go.transform.localScale = Vector3.one * 1.3f;
        AddTemporalGlow(go.transform, name + " Glow", 1.6f, new Color(tint.r, tint.g, tint.b, .22f));
    }

    private static void Prop(Transform parent, string name, Vector2 pos, int variant, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = characterSprites.Length > 0 ? characterSprites[variant % characterSprites.Length] : floorSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = color;
        r.sortingOrder = 1;
        go.transform.localScale = Vector3.one * 1.7f;
    }

    private static void TemporalObelisk(Transform parent, string name, Vector2 pos, Color tint)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = wallSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = new Color(.32f,.72f,.86f,.95f);
        r.sortingOrder = 3;
        go.transform.localScale = new Vector3(1.5f, 3.3f, 1f);
        AddTemporalGlow(go.transform, "Temporal Aura", 2.8f, new Color(tint.r, tint.g, tint.b, .35f));
    }

    private static void BossCore(Transform parent, string name, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = temporalSprite != null ? temporalSprite : wallSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = Color.white;
        r.sortingOrder = 6;
        go.transform.localScale = Vector3.one * 5f;
        AddTemporalGlow(go.transform, "Boss Core Aura", 5.5f, new Color(.38f,.45f,1f,.32f));
    }

    private static void AddTemporalGlow(Transform parent, string name, float size, Color color)
    {
        GameObject glow = new GameObject(name);
        glow.transform.SetParent(parent, false);
        glow.transform.localPosition = new Vector3(0f, 0f, .2f);
        SpriteRenderer r = glow.AddComponent<SpriteRenderer>();
        r.sprite = temporalSprite != null ? temporalSprite : floorSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = color;
        r.sortingOrder = -1;
        glow.transform.localScale = Vector3.one * size;
    }

    private static void CreateObjectiveMarker(Transform parent, ZoneSpec spec, int index)
    {
        GameObject marker = new GameObject("Objective_" + spec.title.Replace(" ", "_"));
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = new Vector3(19f, 3f, 0f);
        SpriteRenderer r = marker.AddComponent<SpriteRenderer>();
        r.sprite = temporalSprite != null ? temporalSprite : floorSprite;
        r.sharedMaterial = UnlitMaterial();
        r.color = spec.loop ? new Color(.25f,.9f,1f,.88f) : new Color(1f,.52f,.16f,.88f);
        r.sortingOrder = 4;
        marker.transform.localScale = Vector3.one * 1.4f;
        if (spec.loop) AddTemporalGlow(marker.transform, "Objective Temporal Trail", 1.8f, new Color(.28f,.9f,1f,.28f));
    }

    private static void ReplaceCharacterPresentation()
    {
        PlayerMovement player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            SpriteRenderer r = player.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault();
            if (r == null) r = player.gameObject.AddComponent<SpriteRenderer>();
            if (characterSprites.Length > 0) r.sprite = characterSprites[0];
            r.sharedMaterial = UnlitMaterial();
            r.color = Color.white;
            r.sortingOrder = 40;
            player.transform.position = Specs[0].position + new Vector2(-18f, 0f);
        }
        EnemyFollow[] enemies = UnityEngine.Object.FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include);
        for (int i = 0; i < enemies.Length; i++)
        {
            SpriteRenderer r = enemies[i].GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault();
            if (r == null) r = enemies[i].gameObject.AddComponent<SpriteRenderer>();
            if (characterSprites.Length > 0) r.sprite = characterSprites[(i + 1) % characterSprites.Length];
            r.sharedMaterial = UnlitMaterial();
            r.color = i % 3 == 1 ? new Color(1f,.55f,.35f) : Color.white;
            r.sortingOrder = 35;
        }
        foreach (GhostPlayback ghost in UnityEngine.Object.FindObjectsByType<GhostPlayback>(FindObjectsInactive.Include))
            foreach (SpriteRenderer r in ghost.GetComponentsInChildren<SpriteRenderer>(true)) { if (characterSprites.Length > 0) r.sprite = characterSprites[0]; r.sharedMaterial = UnlitMaterial(); r.color = new Color(.25f,.9f,1f,.52f); r.sortingOrder = 39; }
    }

    private static void RebuildHUDArt()
    {
        Canvas[] allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas candidate in allCanvases)
            foreach (Transform old in candidate.GetComponentsInChildren<Transform>(true).Where(t => t.name == "FullRebuildHUD").ToArray())
                UnityEngine.Object.DestroyImmediate(old.gameObject);
        foreach (TMP_Text label in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
            if (label.text != null && label.text.Contains("◆")) label.text = label.text.Replace("◆", "-");
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null) canvas = allCanvases.FirstOrDefault(c => c.name == "Canvas");
        if (canvas == null) return;
        foreach (Image legacyImage in canvas.GetComponentsInChildren<Image>(true))
            if (legacyImage.sprite != null) legacyImage.type = Image.Type.Simple;
        GameObject root = new GameObject("FullRebuildHUD", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        AddPanel(root.transform, "KenneyTopBar", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -104f), new Vector2(438f, -18f), "CHRONODUNGEON  //  WORLD MAP", new Color(.12f,.2f,.28f,.92f));
        AddPanel(root.transform, "KenneyObjective", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-360f, -104f), new Vector2(-18f, -18f), "OBJECTIVE  ·  REACH THE CHRONO SANCTUM", new Color(.16f,.11f,.22f,.94f));
        AddPanel(root.transform, "KenneyTemporalKey", new Vector2(.5f, 1f), new Vector2(.5f, 1f), new Vector2(-160f, -70f), new Vector2(160f, -18f), "TEMPORAL RESONANCE  -  0", new Color(.08f,.27f,.34f,.92f));
    }

    private static void AddPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 min, Vector2 max, string label, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.offsetMin = min; rt.offsetMax = max;
        Image image = go.GetComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/kenney/UI/PixelUI/9-Slice/Colored/blue.png");
        image.type = Image.Type.Simple;
        image.color = color;
        GameObject textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        RectTransform tr = textGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = new Vector2(18f, 6f); tr.offsetMax = new Vector2(-18f, -6f);
        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = label; text.fontSize = 18f; text.color = Color.white; text.alignment = TextAlignmentOptions.Center;
    }

    private static void ConfigureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.008f,.015f,.028f,1f);
        ZoneCameraController controller = cam.GetComponent<ZoneCameraController>();
        if (controller == null) controller = cam.gameObject.AddComponent<ZoneCameraController>();
        RoomCamera legacyRoomCamera = cam.GetComponent<RoomCamera>();
        if (legacyRoomCamera != null) legacyRoomCamera.enabled = false;
        GameObject managers = GameObject.Find("Managers");
        if (managers != null)
        {
            RoomManager legacyRoomManager = managers.GetComponent<RoomManager>();
            if (legacyRoomManager != null) legacyRoomManager.enabled = false;
        }
        SerializedObject serialized = new SerializedObject(controller);
        Set(serialized, "globalBoundsCenter", new Vector2(146f, 25f));
        Set(serialized, "globalBoundsSize", new Vector2(650f, 170f));
        Set(serialized, "useGlobalZoneBounds", true);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject Child(Transform parent, string name)
    { Transform found = parent.Find(name); if (found != null) return found.gameObject; GameObject go = new GameObject(name); go.transform.SetParent(parent, false); return go; }

    private static Material UnlitMaterial()
    {
        if (unlitMaterial != null) return unlitMaterial;
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        unlitMaterial = shader != null ? new Material(shader) { name = "ChronoWorld Kenney Unlit" } : null;
        return unlitMaterial;
    }

    private static void Set(SerializedObject so, string propertyName, object value)
    {
        SerializedProperty p = so.FindProperty(propertyName); if (p == null) return;
        if (value is Vector2 v) p.vector2Value = v; else if (value is bool b) p.boolValue = b;
    }
}
