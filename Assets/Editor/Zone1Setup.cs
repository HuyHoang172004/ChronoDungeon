using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Zone1Setup
{
    public static string ApplyRework()
    {
        Room[] rooms = UnityEngine.Object.FindObjectsByType<Room>(FindObjectsInactive.Include);
        Room zone1 = Find(rooms, "THRESHOLD", "01 Threshold");
        Room zone2 = Find(rooms, "GUARD HALL");
        PlayerMovement player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
        RuneKeyInventory inventory = player.GetComponent<RuneKeyInventory>();
        if (inventory == null) inventory = player.gameObject.AddComponent<RuneKeyInventory>();

        RemoveAllComponentsUnder(zone2.transform, typeof(SubArea));
        RemoveAllComponentsUnder(zone2.transform, typeof(AreaBounds));
        RemoveIfPresent(FindDoor(zone2).GetComponent<RuneGate>());
        RemoveIfPresent(FindObjectUnder(zone2.transform, "Zone1 Rune Key"));
        GameObject zone1Content = Content(zone1);
        zone2.transform.position = new Vector3(24f, -56f, 0f);
        EditorUtility.SetDirty(zone2.transform);
        Transform oldRoom2Layout = zone2.transform.Find("Content/Authored Layout");
        if (oldRoom2Layout != null) oldRoom2Layout.gameObject.SetActive(false);
        RemoveAllComponentsUnder(zone1Content.transform, typeof(SubArea));
        RemoveAllComponentsUnder(zone1Content.transform, typeof(AreaBounds));
        RemoveAllComponentsUnder(zone1Content.transform, typeof(SubAreaTransition));
        Zone1LayoutBuilder layout = zone1Content.GetComponent<Zone1LayoutBuilder>();
        if (layout == null) layout = zone1Content.AddComponent<Zone1LayoutBuilder>();
        RemoveIfPresent(zone1Content.GetComponent<Zone1VisualDresser>());
        SetPlayerPresentation(player);

        RoomBoundary boundary = zone1.GetComponentInChildren<RoomBoundary>(true);
        Set(boundary, "width", 140f);
        Set(boundary, "height", 110f);
        Set(boundary, "centerOffset", new Vector2(10f, -3f));
        Set(boundary, "exitOpening", 8f);
        boundary.Rebuild();

        Door gateDoor = FindDoor(zone1);
        Set(gateDoor, "initiallyOpen", false);
        gateDoor.transform.localPosition = new Vector3(42f, -48f, 0f);
        RoomExit gateExit = zone1.GetComponentInChildren<RoomExit>(true);
        gateExit.transform.localPosition = new Vector3(42f, -56f, 0f);
        RuneGate runeGate = gateDoor.GetComponent<RuneGate>();
        if (runeGate == null) runeGate = gateDoor.gameObject.AddComponent<RuneGate>();
        Set(runeGate, "inventory", inventory);

        Set(zone1, "displayName", "ZONE 1 - RUINED ENTRANCE");
        Set(zone1, "clearCondition", 1);
        Set(zone1, "temporalZone", false);
        Set(zone1, "requiresRuneKeyForExit", true);
        Set(zone1, "runeKeyInventory", inventory);
        Set(zone1, "keepContentVisibleAfterLeave", true);
        Set(zone2, "temporalZone", true);
        Set(zone2, "requiresRuneKeyForExit", false);
        Set(zone2, "keepContentVisibleAfterLeave", false);
        zone1.SpawnPoint.localPosition = new Vector3(-47f, 0f, 0f);
        EditorUtility.SetDirty(zone1.SpawnPoint);

        List<Health> zoneEnemies = DuplicateCourtyardEnemies(zone2, zone1Content.transform);
        SetArray(zone1, "enemies", zoneEnemies.ToArray());

        GameObject key = FindObjectUnder(zone1Content.transform, "Zone1 Rune Key");
        if (key == null)
        {
            key = new GameObject("Zone1 Rune Key");
            key.transform.SetParent(zone1Content.transform, false);
        key.transform.localPosition = new Vector3(20f, 38f, 0f);
            key.AddComponent<CircleCollider2D>().radius = .6f;
            key.AddComponent<RuneKey>();
        }
        Set(key.GetComponent<RuneKey>(), "inventory", inventory);
        SetArray(key.GetComponent<RuneKey>(), "requiredEnemies", zoneEnemies.ToArray());

        SubArea a = MakeSubArea(zone1Content.transform, "1A Broken Gate - Entrance Hall", new Vector3(-42f, 0f, 0f), new Vector2(30f, 20f), new Color(.16f, .72f, .82f, 1f));
        SubArea corridor = MakeSubArea(zone1Content.transform, "1B Connecting Corridor", new Vector3(-18f, 0f, 0f), new Vector2(20f, 9f), new Color(.2f, .58f, .72f, 1f));
        SubArea b = MakeSubArea(zone1Content.transform, "1C Fallen Courtyard", new Vector3(10f, 0f, 0f), new Vector2(40f, 26f), new Color(.22f, .55f, .72f, 1f));
        SubArea c = MakeSubArea(zone1Content.transform, "1D Side Ruins - Rune Key Area", new Vector3(20f, 38f, 0f), new Vector2(28f, 20f), new Color(.25f, .85f, .78f, 1f));
        SubArea gateArea = MakeSubArea(zone1Content.transform, "1E Locked Rune Gate Approach", new Vector3(42f, -48f, 0f), new Vector2(32f, 18f), new Color(.15f, .75f, .9f, 1f));
        WorldAreaManager world = GameObject.Find("Managers").GetComponent<WorldAreaManager>();
        if (world == null) world = GameObject.Find("Managers").AddComponent<WorldAreaManager>();
        Set(world, "zoneRoom", zone1);
        Set(world, "connectedZoneRoom", zone2);
        Set(world, "player", player);
        Set(world, "initialSubArea", a);
        SetArray(world, "subAreas", new UnityEngine.Object[] { a, corridor, b, c, gateArea });
        CreateTransition(zone1Content.transform, "Transition Entrance-Corridor", new Vector3(-27f, 0f, 0f), new Vector2(2f, 7f), corridor, world);
        CreateTransition(zone1Content.transform, "Transition Corridor-Courtyard", new Vector3(-9f, 0f, 0f), new Vector2(2f, 7f), b, world);
        CreateTransition(zone1Content.transform, "Transition Courtyard-Side Ruins", new Vector3(20f, 14f, 0f), new Vector2(7f, 2f), c, world);
        CreateTransition(zone1Content.transform, "Transition Courtyard-Gate Bend", new Vector3(31f, 0f, 0f), new Vector2(2f, 7f), gateArea, world);

        ZoneCameraController zoneCamera = Camera.main.GetComponent<ZoneCameraController>();
        if (zoneCamera == null) zoneCamera = Camera.main.gameObject.AddComponent<ZoneCameraController>();
        Set(zoneCamera, "areas", world);
        Set(zoneCamera, "player", player);
        Set(zoneCamera, "useGlobalZoneBounds", true);
        Set(zoneCamera, "globalBoundsCenter", new Vector2(5f, -13f));
        Set(zoneCamera, "globalBoundsSize", new Vector2(150f, 130f));
        Camera.main.orthographicSize = 8f;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(.006f, .012f, .025f, 1f);

        layout.Rebuild();

        RoomBoundary room2Boundary = zone2.GetComponentInChildren<RoomBoundary>(true);
        if (room2Boundary != null)
        {
            Set(room2Boundary, "width", 72f);
            Set(room2Boundary, "height", 38f);
            Set(room2Boundary, "centerOffset", new Vector2(30f, 0f));
            Set(room2Boundary, "exitOpening", 8f);
            room2Boundary.Rebuild();
        }

        GameObject managers = GameObject.Find("Managers");
        VerticalSlicePresentation presentation = managers.GetComponent<VerticalSlicePresentation>();
        if (presentation == null) presentation = managers.AddComponent<VerticalSlicePresentation>();
        Set(presentation, "kenneyHeroSprite", LoadSprite("Assets/ThirdParty/Kenney/Characters/Roguelike/roguelikeChar_transparent.png", "roguelikeChar_transparent_0"));
        Set(presentation, "kenneyEnemySprite", LoadSprite("Assets/ThirdParty/Kenney/Characters/Roguelike/roguelikeChar_transparent.png", "roguelikeChar_transparent_39"));
        Set(presentation, "kenneyDungeonTile", LoadSprite("Assets/ThirdParty/Kenney/Environment/CavesDungeon/roguelikeDungeon_transparent.png", "roguelikeDungeon_transparent_160"));
        Set(presentation, "kenneyAttackButton", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/Kenney/UI/PixelUI/9-Slice/Colored/red.png"));
        Set(presentation, "kenneyDashButton", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/Kenney/UI/PixelUI/9-Slice/Colored/blue.png"));
        EditorUtility.SetDirty(presentation);

        EditorSceneManager.SaveOpenScenes();
        return "Zone 1 continuous world rebuilt: 140x110 global footprint, irregular L-layout, global camera bounds, " + zoneEnemies.Count + " courtyard enemies, key and gate.";
    }

    private static List<Health> DuplicateCourtyardEnemies(Room sourceRoom, Transform parent)
    {
        List<GameObject> oldClones = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child != null && child.name.StartsWith("Zone1 Courtyard Enemy", StringComparison.Ordinal)) oldClones.Add(child.gameObject);
        foreach (GameObject oldClone in oldClones) RemoveIfPresent(oldClone);
        List<GameObject> sources = new List<GameObject>();
        foreach (EnemyFollow enemy in sourceRoom.GetComponentsInChildren<EnemyFollow>(true)) sources.Add(enemy.gameObject);
        if (sources.Count == 0) throw new InvalidOperationException("No reusable enemy source found in Guard Hall.");
        Vector3[] positions = { new Vector3(-2f, 7f, 0f), new Vector3(5f, -7f, 0f), new Vector3(12f, 8f, 0f), new Vector3(20f, -6f, 0f), new Vector3(27f, 5f, 0f) };
        List<Health> result = new List<Health>();
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject clone = UnityEngine.Object.Instantiate(sources[i % sources.Count], parent);
            clone.name = "Zone1 Courtyard Enemy " + (i + 1);
            clone.transform.localPosition = positions[i];
            clone.SetActive(true);
            Health health = clone.GetComponent<Health>();
            if (health != null) result.Add(health);
        }
        return result;
    }

    private static SubArea MakeSubArea(Transform parent, string name, Vector3 position, Vector2 size, Color accent)
    {
        GameObject existing = FindObjectUnder(parent, name);
        if (existing != null) RemoveIfPresent(existing);
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = position;
        AreaBounds bounds = root.AddComponent<AreaBounds>();
        Set(bounds, "size", size);
        SubArea area = root.AddComponent<SubArea>();
        Set(area, "displayName", name);
        Set(area, "bounds", bounds);
        Set(area, "cameraAnchor", root.transform);
        Set(area, "normalZone", true);
        Set(area, "accentColor", accent);
        return area;
    }

    private static void CreateTransition(Transform parent, string name, Vector3 position, Vector2 size, SubArea destination, WorldAreaManager world)
    {
        RemoveIfPresent(FindObjectUnder(parent, name));
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = position;
        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = size;
        SubAreaTransition transition = root.AddComponent<SubAreaTransition>();
        Set(transition, "manager", world);
        Set(transition, "destination", destination);
    }

    private static Room Find(Room[] rooms, string displayName, string objectName = null)
    {
        foreach (Room room in rooms) if (room.DisplayName == displayName || (!string.IsNullOrEmpty(objectName) && room.name == objectName)) return room;
        throw new InvalidOperationException("Room not found: " + displayName);
    }

    private static GameObject Content(Room room) => (GameObject)typeof(Room).GetField("content", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(room);
    private static Door FindDoor(Room room) => (Door)typeof(Room).GetField("exitDoor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(room);

    private static GameObject FindObjectUnder(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child.gameObject;
        return null;
    }

    private static void RemoveAllComponentsUnder(Transform parent, Type componentType)
    {
        List<GameObject> roots = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child == parent) continue;
            if (child.GetComponent(componentType) != null && !roots.Contains(child.gameObject)) roots.Add(child.gameObject);
        }
        foreach (GameObject root in roots) RemoveIfPresent(root);
    }

    private static void SetArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty array = serialized.FindProperty(field);
        array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetPlayerPresentation(PlayerMovement player)
    {
        if (player == null) return;
        foreach (SpriteRenderer renderer in player.GetComponentsInChildren<SpriteRenderer>(true))
        {
            // Zone 1's procedural floor is deliberately above legacy room art.
            // Keep the player above the environment so it cannot look buried.
            renderer.sortingOrder = renderer.name == "ChronoVisualAccent" ? 41 : 40;
            EditorUtility.SetDirty(renderer);
        }
        EditorUtility.SetDirty(player);
    }

    private static Sprite LoadSprite(string path, string name)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if (asset is Sprite sprite && sprite.name == name) return sprite;
        return null;
    }

    private static void Set(UnityEngine.Object target, string field, object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null) return;
        if (value is string) property.stringValue = (string)value;
        else if (value is bool) property.boolValue = (bool)value;
        else if (value is int) property.intValue = (int)value;
        else if (value is float) property.floatValue = (float)value;
        else if (value is Vector2) property.vector2Value = (Vector2)value;
        else if (value is Color) property.colorValue = (Color)value;
        else if (value is UnityEngine.Object) property.objectReferenceValue = (UnityEngine.Object)value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void RemoveIfPresent(UnityEngine.Object target)
    {
        if (target != null) UnityEngine.Object.DestroyImmediate(target, true);
    }
}
