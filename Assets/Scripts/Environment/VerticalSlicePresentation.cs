using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Presentation-only layer for the two-room vertical slice. It never owns gameplay state.
[DefaultExecutionOrder(650)]
public sealed class VerticalSlicePresentation : MonoBehaviour
{
    [SerializeField] private Sprite kenneyHeroSprite;
    [SerializeField] private Sprite kenneyEnemySprite;
    [SerializeField] private Sprite kenneyDungeonTile;
    [SerializeField] private Sprite kenneyAttackButton;
    [SerializeField] private Sprite kenneyDashButton;
    private static readonly Color Void = new Color(0.012f, 0.024f, 0.075f, 1f);
    private static readonly Color Cliff = new Color(0.035f, 0.075f, 0.13f, 1f);
    private static readonly Color Stone = new Color(0.18f, 0.25f, 0.32f, 1f);
    private static readonly Color StoneLight = new Color(0.32f, 0.43f, 0.48f, 1f);
    private static readonly Color Cyan = new Color(0.05f, 0.9f, 1f, 1f);
    private static readonly Color Magenta = new Color(0.78f, 0.16f, 0.92f, 1f);
    private static readonly Color Orange = new Color(1f, 0.42f, 0.12f, 1f);
    private Sprite square;
    private Material material;

    private void Start()
    {
        FindArtSource();
        BuildWorldArt();
        TintAuthoredFloors();
        BuildActors();
        MuteLegacyRoom2Visuals();
        BuildHud();
    }

    private void TintAuthoredFloors()
    {
        foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
        {
            if (!renderer.gameObject.activeInHierarchy || !renderer.name.Contains("Floor")) continue;
            Transform t = renderer.transform;
            bool authored = false;
            while (t != null) { if (t.name == "Zone 1 Environment") { authored = true; break; } t = t.parent; }
            if (!authored) continue;
            renderer.color = renderer.name.Contains("Alt") || renderer.name.Contains("Connecting") || renderer.name.Contains("Side") ? new Color(.21f,.25f,.28f,1f) : new Color(.16f,.20f,.23f,1f);
        }
    }

    private void MuteLegacyRoom2Visuals()
    {
        Room room2 = null;
        foreach (Room room in FindObjectsByType<Room>(FindObjectsInactive.Include))
            if (room.DisplayName.Contains("GUARD")) { room2 = room; break; }
        if (room2 == null || room2.Content == null) return;
        foreach (SpriteRenderer renderer in room2.Content.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Transform t = renderer.transform;
            bool generatedActor = false;
            while (t != null && t != room2.Content.transform)
            {
                if (t.name == "Stylized Enemy Visual") { generatedActor = true; break; }
                t = t.parent;
            }
            if (!generatedActor) renderer.enabled = false;
        }
    }

    private void FindArtSource()
    {
        foreach (SpriteRenderer r in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
            if (r.sprite != null && r.sprite.name == "Square") { square = r.sprite; break; }
        if (square == null) square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f));
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        material = new Material(shader);
    }

    private GameObject Layer(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order, float rotation = 0f)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        go.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square; sr.sharedMaterial = material; sr.color = color; sr.sortingOrder = order;
        return go;
    }

    private GameObject SpriteLayer(Transform parent, string name, Vector2 pos, Vector2 scale, Sprite sprite, Color color, int order, float rotation = 0f)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        go.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite; sr.sharedMaterial = material; sr.color = color; sr.sortingOrder = order;
        return go;
    }

    private Transform Group(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) return t;
        GameObject go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform;
    }

    private void BuildWorldArt()
    {
        GameObject worldObject = GameObject.Find("Chrono Vertical Slice World");
        if (worldObject == null) worldObject = new GameObject("Chrono Vertical Slice World");
        worldObject.transform.position = Vector3.zero;
        worldObject.transform.rotation = Quaternion.identity;
        worldObject.transform.localScale = Vector3.one;
        Transform root = Group(worldObject.transform, "Chrono Vertical Slice Art");
        Layer(root, "Deep Abyss", new Vector2(5, -13), new Vector2(168, 142), Void, -100);
        Layer(root, "Cliff Shelf North", new Vector2(5, 49), new Vector2(156, 7), Cliff, -95, -2f);
        Layer(root, "Cliff Shelf West", new Vector2(-69, -12), new Vector2(8, 118), Cliff, -95, 3f);
        Layer(root, "Distant Ruins", new Vector2(-2, 30), new Vector2(120, 2), new Color(.08f,.17f,.22f), -90, 1f);
        Layer(root, "Mist Band", new Vector2(3, -39), new Vector2(152, 3), new Color(.11f,.32f,.39f,.24f), -85, -1f);
        MakeRuins(root);
        MakeKenneyDungeonTiles(root);
        MakeRoom1Accents(root);
        MakeRoom2Accents(root);
    }

    private void MakeKenneyDungeonTiles(Transform root)
    {
        if (kenneyDungeonTile == null) return;
        Transform g = Group(root, "Kenney Dungeon Tile Accents");
        int index = 0;
        for (int row=-1; row<=1; row++)
            for (int col=-4; col<=4; col++)
                SpriteLayer(g, "Kenney Stone Tile " + index++, new Vector2(-54 + col*3.2f, row*3.2f), new Vector2(5f,5f), kenneyDungeonTile, new Color(1f,1f,1f,.72f), 17, (row+col)%2==0 ? -1f : 1f);
        for (int row=-1; row<=1; row++)
            for (int col=0; col<=8; col++)
                SpriteLayer(g, "Kenney Courtyard Tile " + index++, new Vector2(-2 + col*3.2f, row*3.2f), new Vector2(5f,5f), kenneyDungeonTile, new Color(.9f,.96f,1f,.62f), 17, (row+col)%2==0 ? 1f : -1f);
    }

    private void MakeFloorMosaic(Transform root)
    {
        Transform g = Group(root, "Stone Floor Mosaic");
        int index = 0;
        for (int row = -1; row <= 1; row++)
            for (int col = -2; col <= 2; col++)
            {
                Vector2 p = new Vector2(-48f + col * 5.2f, row * 5.2f);
                Layer(g, "Entrance Slab " + index++, p, new Vector2(4.75f, 4.7f), (row + col) % 2 == 0 ? new Color(.19f,.23f,.26f,.92f) : new Color(.15f,.19f,.23f,.92f), 16, (row-col)*.5f);
            }
        for (int i=0;i<8;i++)
        {
            Vector2 p = new Vector2(-25f + i*5f, 0f);
            Layer(g, "Corridor Slab " + i, p, new Vector2(4.6f, 7.7f), i%2==0 ? new Color(.20f,.25f,.28f,.94f) : new Color(.16f,.21f,.25f,.94f), 16, i%2==0 ? 0f : 1f);
        }
        for (int i=0;i<6;i++)
        {
            Vector2 p = new Vector2(-3f + i*6f, 0f);
            Layer(g, "Courtyard Stone Plate " + i, p, new Vector2(5.6f, 11.5f), i%2==0 ? new Color(.18f,.23f,.27f,.88f) : new Color(.14f,.19f,.24f,.88f), 16, i%2==0 ? -.4f : .4f);
        }
    }

    private void MakeRuins(Transform root)
    {
        Vector2[] ruins = { new(-57, 34), new(-32, 29), new(56, 27), new(66, -38), new(-50, -43), new(58, -68) };
        for (int i = 0; i < ruins.Length; i++)
        {
            Layer(root, "Distant Ruin " + i, ruins[i], new Vector2(5 + i % 3, 10 + i % 2 * 4), new Color(.06f,.14f,.19f,.9f), -80, i * 7f);
            Layer(root, "Ruin Glow " + i, ruins[i] + new Vector2(0, 3), new Vector2(.5f, 4f), i % 2 == 0 ? Cyan * new Color(1,1,1,.18f) : Magenta * new Color(1,1,1,.14f), -78, 8f);
        }
    }

    private void MakeRoom1Accents(Transform root)
    {
        Transform g = Group(root, "Room 1 - Ruined Entrance Art");
        Vector2[] pillars = { new(-54, 7), new(-30, 7), new(-3, 10), new(20, 10), new(27, -10), new(35, -43), new(49, -43) };
        for (int i=0;i<pillars.Length;i++) MakePillar(g, pillars[i], i % 2 == 0 ? StoneLight : Stone, "Entrance Pillar " + i);
        Vector2[] braziers = { new(-50, 5), new(-12, 3), new(25, 8), new(47, -43) };
        for (int i=0;i<braziers.Length;i++) MakeBrazier(g, braziers[i], "Torch " + i);
        Vector2[] rubble = { new(-38, -5), new(-20, 3), new(4, -9), new(17, 15), new(28, -6), new(39, -30) };
        for (int i=0;i<rubble.Length;i++) Layer(g, "Rubble " + i, rubble[i], new Vector2(2.2f, .65f), new Color(.24f,.29f,.32f), 14, i*11f);
        MakeRoom2ExitArch(g);
    }

    private void MakeRoom2ExitArch(Transform parent)
    {
        Transform gate = Group(parent, "Passage to Room 2 - Temporal Shrine");
        Layer(gate, "Passage Opening", new Vector2(42f,-48f), new Vector2(6.8f,6.2f), new Color(.015f,.025f,.07f,.98f), 10);
        Layer(gate, "Arch Left", new Vector2(37.6f,-48f), new Vector2(1.2f,8.5f), new Color(.25f,.32f,.38f), 22, 1f);
        Layer(gate, "Arch Right", new Vector2(46.4f,-48f), new Vector2(1.2f,8.5f), new Color(.25f,.32f,.38f), 22, -1f);
        Layer(gate, "Arch Lintel", new Vector2(42f,-44f), new Vector2(10.2f,1.35f), new Color(.34f,.42f,.45f), 23, -1f);
        Layer(gate, "Gate Rune", new Vector2(42f,-48f), new Vector2(.22f,5.4f), new Color(1f,.25f,.12f,.92f), 25);
        Layer(gate, "Passage Cyan Edge", new Vector2(42f,-55.2f), new Vector2(7f,.18f), Cyan, 24);
        Layer(gate, "Room 2 Beacon", new Vector2(42f,-41.8f), new Vector2(1.8f,.18f), Magenta, 25);
        Door door = null;
        float best = float.MaxValue;
        foreach (Door candidate in FindObjectsByType<Door>(FindObjectsInactive.Include))
        {
            float distance = Vector2.Distance(candidate.transform.position, new Vector2(42f,-48f));
            if (distance < best) { best = distance; door = candidate; }
        }
        GatePresentation state = gate.gameObject.GetComponent<GatePresentation>();
        if (state == null) state = gate.gameObject.AddComponent<GatePresentation>();
        state.door = door;
    }

    private void MakeRoom2Accents(Transform root)
    {
        Transform g = Group(root, "Room 2 - Temporal Shrine Art");
        Layer(g, "Temporal Shrine Floor", new Vector2(54, -56), new Vector2(70, 34), new Color(.19f,.12f,.29f), -12);
        for (int i=0;i<8;i++) Layer(g, "Shrine Floor Panel " + i, new Vector2(21 + i*9, -56), new Vector2(.12f, 31f), new Color(.38f,.16f,.48f,.24f), -10, i%2==0 ? 0f : 1f);
        Vector2[] obelisks = { new(36,-60), new(55,-61), new(73,-60), new(46,-51), new(65,-51) };
        for (int i=0;i<obelisks.Length;i++)
        {
            Color c = i % 2 == 0 ? Cyan : Magenta;
            Layer(g, "Temporal Obelisk " + i, obelisks[i], new Vector2(1.2f, 4.3f), new Color(.08f,.12f,.18f), 16, i%2==0 ? 2 : -2);
            Layer(g, "Obelisk Core " + i, obelisks[i] + new Vector2(0,.1f), new Vector2(.22f, 2.8f), c * new Color(1,1,1,.9f), 18);
        }
        Layer(g, "Shrine Platform", new Vector2(55,-73), new Vector2(16, 2.2f), new Color(.17f,.09f,.25f), 12, 1f);
        Layer(g, "Shrine Core", new Vector2(55,-71.6f), new Vector2(2.5f, 2.5f), Magenta * new Color(1,1,1,.75f), 19, 45f);
        for (int i=0;i<7;i++) Layer(g, "Temporal Fracture " + i, new Vector2(32+i*7, -81 + (i%3)*4), new Vector2(3.5f,.12f), i%2==0 ? Cyan : Magenta, 13, 14f + i*9f);
    }

    private void MakePillar(Transform parent, Vector2 pos, Color color, string name)
    {
        Layer(parent, name + " shadow", pos + new Vector2(.4f,-.45f), new Vector2(3.6f, 1.2f), new Color(0,0,0,.34f), 11, 5f);
        Layer(parent, name, pos, new Vector2(1.8f, 3.4f), color, 15, 1f);
        Layer(parent, name + " cap", pos + new Vector2(0,1.5f), new Vector2(2.3f,.36f), StoneLight, 16, 1f);
        Layer(parent, name + " moss", pos + new Vector2(-.65f,.4f), new Vector2(.22f,1.6f), new Color(.18f,.5f,.36f), 17, -3f);
    }

    private void MakeBrazier(Transform parent, Vector2 pos, string name)
    {
        Layer(parent, name + " base", pos, new Vector2(1.6f,.65f), new Color(.1f,.12f,.15f), 16);
        Layer(parent, name + " flame glow", pos + new Vector2(0,.65f), new Vector2(2.2f,2.2f), new Color(1f,.23f,.04f,.12f), 13);
        Layer(parent, name + " flame", pos + new Vector2(0,.7f), new Vector2(.55f,1.35f), Orange, 19, 8f);
        Layer(parent, name + " cyan ember", pos + new Vector2(.32f,.8f), new Vector2(.16f,.38f), Cyan, 20, -12f);
    }

    private void BuildActors()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) BuildPlayer(player.transform);
        foreach (EnemyFollow enemy in FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include)) BuildEnemy(enemy.transform, enemy.transform.GetHashCode());
    }

    private void BuildPlayer(Transform player)
    {
        Transform g = Group(player, "Stylized Player Visual");
        Layer(g, "Shadow", new Vector2(0,-.55f), new Vector2(1.5f,.5f), new Color(0,0,0,.42f), 30);
        Layer(g, "Cloak", new Vector2(0,0), new Vector2(1.4f,1.9f), new Color(.04f,.16f,.22f), 40, 45f);
        Layer(g, "Cyan Aura", Vector2.zero, new Vector2(1.15f,1.7f), new Color(.04f,.65f,.78f,.25f), 39);
        Layer(g, "Core", new Vector2(0,.08f), new Vector2(.75f,1.25f), new Color(.2f,.78f,.82f), 43);
        Layer(g, "Visor", new Vector2(.2f,.43f), new Vector2(.62f,.22f), Color.white, 45, -8f);
        Layer(g, "Temporal Ribbon", new Vector2(-.48f,.12f), new Vector2(.2f,1.35f), Magenta, 44, 18f);
        if (kenneyHeroSprite != null) SpriteLayer(g, "Kenney Hero Sprite", new Vector2(0,.1f), new Vector2(8f,8f), kenneyHeroSprite, Color.white, 47);
        SpriteRenderer rootSprite = player.GetComponent<SpriteRenderer>(); if (rootSprite != null) rootSprite.color = new Color(1,1,1,0);
    }

    private void BuildEnemy(Transform enemy, int seed)
    {
        Transform g = Group(enemy, "Stylized Enemy Visual");
        float variant = Mathf.Abs(seed % 3);
        Color accent = variant == 0 ? Orange : (variant == 1 ? new Color(1,.15f,.1f) : Magenta);
        Layer(g, "Shadow", new Vector2(0,-.45f), new Vector2(1.5f,.45f), new Color(0,0,0,.45f), 30);
        Layer(g, "Corrupted Shell", Vector2.zero, new Vector2(1.55f,1.55f), new Color(.12f,.04f,.12f), 40, 45f);
        Layer(g, "Core", new Vector2(0,.05f), new Vector2(.85f,1.1f), new Color(.35f,.07f,.16f), 43);
        Layer(g, "Eye L", new Vector2(-.3f,.35f), new Vector2(.2f,.18f), accent, 46, 12f);
        Layer(g, "Eye R", new Vector2(.3f,.35f), new Vector2(.2f,.18f), accent, 46, -12f);
        Layer(g, "Shoulder L", new Vector2(-.65f,.05f), new Vector2(.42f,.65f), accent * new Color(1,1,1,.75f), 44, -18f);
        Layer(g, "Shoulder R", new Vector2(.65f,.05f), new Vector2(.42f,.65f), accent * new Color(1,1,1,.75f), 44, 18f);
        if (kenneyEnemySprite != null) SpriteLayer(g, "Kenney Enemy Sprite", new Vector2(0,.08f), new Vector2(8f,8f), kenneyEnemySprite, new Color(1f,.78f,.78f), 47);
        SpriteRenderer rootSprite = enemy.GetComponent<SpriteRenderer>(); if (rootSprite != null) rootSprite.color = new Color(1,1,1,0);
    }

    private void BuildHud()
    {
        Canvas canvas = null;
        foreach (Canvas candidate in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            if (candidate.renderMode != RenderMode.WorldSpace) { canvas = candidate; break; }
        if (canvas == null) return;
        foreach (TMP_Text text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            if (text.name == "Room HUD" || text.name == "Tutorial Text" || text.name == "TimeLoopHUD") text.gameObject.SetActive(false);
        }
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            if (t.name == "GameOverPanel") t.gameObject.SetActive(false);
        Transform root = canvas.transform.Find("Vertical Slice HUD");
        if (root == null) { GameObject go = new GameObject("Vertical Slice HUD", typeof(RectTransform)); go.transform.SetParent(canvas.transform, false); root = go.transform; }
        RectTransform r = root as RectTransform; r.anchorMin = new Vector2(0,1); r.anchorMax = new Vector2(0,1); r.pivot = new Vector2(0,1); r.anchoredPosition = new Vector2(34,-28); r.sizeDelta = new Vector2(410,90);
        if (root.Find("Frame") == null)
        {
            GameObject frame = new GameObject("Frame", typeof(RectTransform), typeof(Image)); frame.transform.SetParent(root,false); RectTransform fr=frame.GetComponent<RectTransform>(); fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero; frame.GetComponent<Image>().color=new Color(.015f,.05f,.09f,.88f);
            GameObject title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)); title.transform.SetParent(root,false); RectTransform tr=title.GetComponent<RectTransform>(); tr.anchorMin=new Vector2(0,1); tr.anchorMax=new Vector2(0,1); tr.pivot=new Vector2(0,1); tr.anchoredPosition=new Vector2(18,-12); tr.sizeDelta=new Vector2(360,30); TextMeshProUGUI tx=title.GetComponent<TextMeshProUGUI>(); tx.text="CHRONODUNGEON  //  RUINED ENTRANCE"; tx.fontSize=16; tx.color=Cyan; tx.fontStyle=FontStyles.Bold;
            GameObject sub = new GameObject("Objective", typeof(RectTransform), typeof(TextMeshProUGUI)); sub.transform.SetParent(root,false); RectTransform sr=sub.GetComponent<RectTransform>(); sr.anchorMin=new Vector2(0,1); sr.anchorMax=new Vector2(0,1); sr.pivot=new Vector2(0,1); sr.anchoredPosition=new Vector2(18,-47); sr.sizeDelta=new Vector2(365,26); TextMeshProUGUI st=sub.GetComponent<TextMeshProUGUI>(); st.text="Explore the ruined passage"; st.fontSize=15; st.color=new Color(.8f,.9f,.95f);
        }
        StyleButtons(canvas);
    }

    private void StyleButtons(Canvas canvas)
    {
        foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
        {
            string n=image.gameObject.name.ToLowerInvariant();
            if (n.Contains("attack")) { image.color=new Color(1f,.28f,.08f,.94f); if (kenneyAttackButton!=null) { image.sprite=kenneyAttackButton; image.type=Image.Type.Sliced; } StyleLabel(image.transform,"ATTACK",Color.white); }
            else if (n.Contains("dash")) { image.color=new Color(.1f,.65f,.95f,.94f); if (kenneyDashButton!=null) { image.sprite=kenneyDashButton; image.type=Image.Type.Sliced; } StyleLabel(image.transform,"DASH",Color.white); }
        }
    }

    private void StyleLabel(Transform parent, string text, Color color)
    {
        TextMeshProUGUI label=parent.GetComponentInChildren<TextMeshProUGUI>(true); if (label==null) return; label.text=text; label.color=color; label.fontStyle=FontStyles.Bold; label.fontSize=24;
    }
}

public sealed class GatePresentation : MonoBehaviour
{
    public Door door;
    private SpriteRenderer[] bars;
    private bool lastOpen;

    private void Start()
    {
        bars = GetComponentsInChildren<SpriteRenderer>(true);
        Apply();
    }

    private void Update()
    {
        if (door != null && door.IsOpen != lastOpen) Apply();
    }

    private void Apply()
    {
        lastOpen = door != null && door.IsOpen;
        foreach (SpriteRenderer bar in bars)
        {
            if (bar.name == "Gate Rune")
            {
                bar.color = lastOpen ? new Color(.2f,1f,.85f,.25f) : new Color(1f,.25f,.12f,.95f);
                bar.transform.localScale = new Vector3(lastOpen ? .04f : .22f, 5.4f, 1f);
            }
            else if (bar.name == "Passage Cyan Edge") bar.color = lastOpen ? new Color(.2f,1f,.85f) : new Color(.05f,.75f,1f,.8f);
        }
    }
}
