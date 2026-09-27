using UnityEngine;

/// Authored procedural dressing for Zone 1. The scene remains lightweight while
/// still presenting a readable dungeon: one continuous irregular floor network,
/// thick doorway walls, background depth, landmarks, and traversal obstacles.
[DisallowMultipleComponent]
public sealed class Zone1LayoutBuilder : MonoBehaviour
{
    private static Sprite whiteSprite;
    private static Material unlitMaterial;
    private Transform generated;

    private static readonly Color BackgroundDeep = new Color(0.012f, 0.018f, 0.035f, 1f);
    private static readonly Color BackgroundMid = new Color(0.035f, 0.065f, 0.095f, 1f);
    private static readonly Color StoneFloor = new Color(0.16f, 0.20f, 0.23f, 1f);
    private static readonly Color StoneFloorAlt = new Color(0.21f, 0.25f, 0.28f, 1f);
    private static readonly Color Wall = new Color(0.18f, 0.21f, 0.235f, 1f);
    private static readonly Color WallCap = new Color(0.29f, 0.34f, 0.36f, 1f);
    private static readonly Color Rubble = new Color(0.22f, 0.23f, 0.24f, 1f);
    private static readonly Color Rune = new Color(0.08f, 0.8f, 0.95f, 0.9f);
    private static readonly Color Moss = new Color(0.12f, 0.32f, 0.25f, 1f);

    private void OnEnable()
    {
        Build();
    }

    public void Build()
    {
        if (generated != null) return;
        generated = transform.Find("Zone 1 Environment");
        if (generated != null) return;

        generated = new GameObject("Zone 1 Environment").transform;
        generated.SetParent(transform, false);

        Transform background = Child(generated, "Background");
        Transform playable = Child(generated, "Playable");
        BuildBackground(background);
        BuildFloors(playable);
        BuildChambers(playable);
        BuildLandmarks(playable);
        BuildObstacles(playable);
    }

    public void Rebuild()
    {
        Transform old = transform.Find("Zone 1 Environment");
        if (old != null)
        {
            if (Application.isPlaying) Destroy(old.gameObject);
            else DestroyImmediate(old.gameObject);
        }
        generated = null;
        Build();
    }

    private static Transform Child(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child;
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        return item.transform;
    }

    private static void BuildBackground(Transform parent)
    {
        Make(parent, "Deep Dungeon Backdrop", new Vector3(3f, -3f, 0f), new Vector2(140f, 110f), BackgroundDeep, -20);
        Make(parent, "Distant Cliff North", new Vector3(5f, 48f, 0f), new Vector2(136f, 8f), BackgroundMid, -19);
        Make(parent, "Distant Cliff South", new Vector3(5f, -54f, 0f), new Vector2(136f, 10f), new Color(.018f, .04f, .055f, 1f), -19);
        Make(parent, "Distant Ruins West", new Vector3(-42f, 14f, 0f), new Vector2(28f, 5f), new Color(.07f, .12f, .15f, 1f), -18);
        Make(parent, "Distant Ruins North", new Vector3(20f, 51f, 0f), new Vector2(35f, 4f), new Color(.055f, .1f, .14f, 1f), -18);
        Make(parent, "Distant Ruins East", new Vector3(47f, 23f, 0f), new Vector2(32f, 5f), new Color(.06f, .12f, .14f, 1f), -18);

        for (int i = 0; i < 18; i++)
        {
            float x = -55f + i * 6.4f;
            Make(parent, "North Hanging Vine " + i, new Vector3(x, 28f + (i % 3) * .7f, 0f), new Vector2(.22f, 4.3f + (i % 2) * 1.6f), Moss, -16);
            Make(parent, "South Cliff Shadow " + i, new Vector3(x + 2.2f, -30f, 0f), new Vector2(4.2f, 2.1f), new Color(.025f, .06f, .07f, 1f), -17);
        }
        Make(parent, "Cyan Chasm Glow", new Vector3(3f, 27f, 0f), new Vector2(122f, .5f), new Color(.05f, .45f, .58f, .4f), -15);
        Make(parent, "Fog Bank North", new Vector3(-12f, 26f, 0f), new Vector2(48f, 2.2f), new Color(.16f, .27f, .3f, .18f), -14);
        Make(parent, "Fog Bank South", new Vector3(44f, -29f, 0f), new Vector2(42f, 2.4f), new Color(.12f, .24f, .3f, .18f), -14);
        for (int i = 0; i < 7; i++)
            Make(parent, "Background Crystal " + i, new Vector3(-4f + i * 11f, 23f + (i % 2) * 2f, 0f), new Vector2(.35f, 1.7f), new Color(.1f, .65f, .78f, .38f), -13);
    }

    private static void BuildFloors(Transform parent)
    {
        Make(parent, "Entrance Hall Floor", new Vector3(-42f, 0f, 0f), new Vector2(30f, 20f), StoneFloor, -10);
        Make(parent, "Entrance Connecting Floor", new Vector3(-18f, 0f, 0f), new Vector2(20f, 8.5f), StoneFloorAlt, -10);
        Make(parent, "Fallen Courtyard Floor", new Vector3(10f, 0f, 0f), new Vector2(40f, 26f), StoneFloor, -10);
        Make(parent, "Side Ruins Stair Corridor Floor", new Vector3(20f, 20f, 0f), new Vector2(8f, 16f), StoneFloorAlt, -10);
        Make(parent, "Side Ruins Floor", new Vector3(20f, 38f, 0f), new Vector2(28f, 20f), StoneFloorAlt, -10);
        Make(parent, "East Bend Floor", new Vector3(36f, 0f, 0f), new Vector2(14f, 8.5f), StoneFloorAlt, -10);
        Make(parent, "Descending Passage Floor", new Vector3(42f, -20f, 0f), new Vector2(8.5f, 42f), StoneFloorAlt, -10);
        Make(parent, "Rune Gate Area Floor", new Vector3(42f, -48f, 0f), new Vector2(32f, 18f), StoneFloor, -10);

        for (int i = 0; i < 43; i++)
        {
            float x = -54f + i * 2.7f;
            float y = (i % 2 == 0) ? 7.7f : -7.7f;
            Make(parent, "Floor Stone Seam " + i, new Vector3(x, y, 0f), new Vector2(1.65f, .07f),
                new Color(.32f, .37f, .39f, .28f), -8);
        }
        for (int i = 0; i < 9; i++)
            Make(parent, "Courtyard Inlay " + i, new Vector3(-4f + i * 3.5f, 0f, 0f), new Vector2(.07f, 20f),
                new Color(.24f, .42f, .44f, .24f), -8);
        for (int i = 0; i < 5; i++)
            Make(parent, "Gate Stair Seam " + i, new Vector3(42f, -3f - i * 7f, 0f), new Vector2(6.4f, .12f), new Color(.32f, .37f, .39f, .3f), -8);
    }

    private static void BuildChambers(Transform parent)
    {
        WallSegment(parent, "Entrance North Wall", new Vector3(-42f, 10f), new Vector2(30f, .8f));
        WallSegment(parent, "Entrance South Wall", new Vector3(-42f, -10f), new Vector2(30f, .8f));
        WallSegment(parent, "Entrance West Wall", new Vector3(-57f, 0f), new Vector2(.8f, 20f));
        WallSegment(parent, "Entrance East Upper Wall", new Vector3(-27f, 7f), new Vector2(.8f, 6f));
        WallSegment(parent, "Entrance East Lower Wall", new Vector3(-27f, -7f), new Vector2(.8f, 6f));

        WallSegment(parent, "Connecting North Wall", new Vector3(-18f, 4.4f), new Vector2(20f, .7f));
        WallSegment(parent, "Connecting South Wall", new Vector3(-18f, -4.4f), new Vector2(20f, .7f));
        WallSegment(parent, "Courtyard West Upper Wall", new Vector3(-10f, 8.5f), new Vector2(.8f, 9f));
        WallSegment(parent, "Courtyard West Lower Wall", new Vector3(-10f, -8.5f), new Vector2(.8f, 9f));
        WallSegment(parent, "Courtyard North West Wall", new Vector3(3f, 13f), new Vector2(26f, .8f));
        WallSegment(parent, "Courtyard North East Wall", new Vector3(27f, 13f), new Vector2(6f, .8f));
        WallSegment(parent, "Courtyard South Wall", new Vector3(10f, -13f), new Vector2(40f, .8f));
        WallSegment(parent, "Courtyard East Upper Wall", new Vector3(30f, 8.5f), new Vector2(.8f, 9f));
        WallSegment(parent, "Courtyard East Lower Wall", new Vector3(30f, -8.5f), new Vector2(.8f, 9f));

        WallSegment(parent, "Side Branch West Wall", new Vector3(16f, 20f), new Vector2(.8f, 16f));
        WallSegment(parent, "Side Branch East Wall", new Vector3(24f, 20f), new Vector2(.8f, 16f));
        WallSegment(parent, "Side Ruins North Wall", new Vector3(20f, 48f), new Vector2(28f, .8f));
        WallSegment(parent, "Side Ruins West Wall", new Vector3(6f, 38f), new Vector2(.8f, 20f));
        WallSegment(parent, "Side Ruins East Wall", new Vector3(34f, 38f), new Vector2(.8f, 20f));
        WallSegment(parent, "Side Ruins South West Wall", new Vector3(11f, 28f), new Vector2(10f, .8f));
        WallSegment(parent, "Side Ruins South East Wall", new Vector3(29f, 28f), new Vector2(10f, .8f));

        WallSegment(parent, "East Bend North Wall", new Vector3(36f, 4.4f), new Vector2(14f, .7f));
        WallSegment(parent, "East Bend South Wall", new Vector3(36f, -4.4f), new Vector2(14f, .7f));
        WallSegment(parent, "Descending West Wall", new Vector3(37.6f, -20f), new Vector2(.8f, 42f));
        WallSegment(parent, "Descending East Wall", new Vector3(46.4f, -20f), new Vector2(.8f, 42f));
        WallSegment(parent, "Gate North West Wall", new Vector3(31f, -39f), new Vector2(10f, .8f));
        WallSegment(parent, "Gate North East Wall", new Vector3(53f, -39f), new Vector2(10f, .8f));
        WallSegment(parent, "Gate South Wall", new Vector3(42f, -57f), new Vector2(32f, .8f));
        WallSegment(parent, "Gate West Wall", new Vector3(26f, -48f), new Vector2(.8f, 18f));
        WallSegment(parent, "Gate East Wall", new Vector3(58f, -48f), new Vector2(.8f, 18f));

        for (int i = 0; i < 12; i++)
        {
            float x = -53f + i * 9.5f;
            Make(parent, "North Wall Cap " + i, new Vector3(x, 13.45f, 0f), new Vector2(3.8f, .22f), WallCap, -5);
            Make(parent, "South Wall Cap " + i, new Vector3(x + 2f, -13.45f, 0f), new Vector2(3.5f, .22f), WallCap, -5);
        }
    }

    private static void BuildLandmarks(Transform parent)
    {
        Landmark(parent, "Broken Gate Landmark", new Vector3(-52f, 0f), 1.4f);
        Landmark(parent, "Courtyard Rune Landmark", new Vector3(10f, 10.5f), 1.1f);
        Landmark(parent, "Rune Key Shrine", new Vector3(20f, 38f), 1.5f);
        Landmark(parent, "Locked Rune Gate Landmark", new Vector3(42f, -48f), 1.65f);

        Torch(parent, "Entrance Torch North", new Vector3(-47f, 9.1f));
        Torch(parent, "Entrance Torch South", new Vector3(-35f, -9.1f));
        Torch(parent, "Courtyard Torch West", new Vector3(-4f, 11.9f));
        Torch(parent, "Courtyard Torch East", new Vector3(26f, -11.9f));
        Torch(parent, "Side Ruins Torch", new Vector3(29f, 46.5f));
        Torch(parent, "Gate Torch", new Vector3(52f, -40.2f));
    }

    private static void BuildObstacles(Transform parent)
    {
        Pillar(parent, "Courtyard Pillar A", new Vector3(0f, 3.5f));
        Pillar(parent, "Courtyard Pillar B", new Vector3(11f, -3.5f));
        Pillar(parent, "Courtyard Pillar C", new Vector3(23f, 4.5f));
        RubblePile(parent, "Courtyard Rubble West", new Vector3(-6f, -8f), new Vector2(2.8f, 1.6f));
        RubblePile(parent, "Courtyard Rubble East", new Vector3(25f, 8f), new Vector2(3.2f, 1.4f));
        RubblePile(parent, "Side Ruins Rubble", new Vector3(10f, 42f), new Vector2(2.6f, 1.7f));
        RubblePile(parent, "Gate Rubble", new Vector3(52f, -52f), new Vector2(3f, 1.5f));
        Make(parent, "Broken Bridge Slab", new Vector3(-18f, 0f), new Vector2(4.5f, .55f), WallCap, -4);
        Make(parent, "Side Ruins Stair 1", new Vector3(20f, 14f), new Vector2(6f, .25f), WallCap, -4);
        Make(parent, "Side Ruins Stair 2", new Vector3(20f, 18f), new Vector2(6f, .25f), WallCap, -4);
        Make(parent, "Descending Stair 1", new Vector3(42f, -8f), new Vector2(6.5f, .25f), WallCap, -4);
        Make(parent, "Descending Stair 2", new Vector3(42f, -14f), new Vector2(6.5f, .25f), WallCap, -4);
    }

    private static void WallSegment(Transform parent, string name, Vector3 position, Vector2 size)
    {
        GameObject item = Make(parent, name, position, size, Wall, -4);
        BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
    }

    private static void Pillar(Transform parent, string name, Vector3 position)
    {
        GameObject item = Make(parent, name, position, new Vector2(1.25f, 1.25f), WallCap, -3);
        GameObject shadow = Make(parent, name + " Shadow", position + new Vector3(.18f, -.2f), new Vector2(1.55f, 1.55f), new Color(.02f, .03f, .04f, .65f), -5);
        BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        shadow.transform.SetSiblingIndex(0);
    }

    private static void RubblePile(Transform parent, string name, Vector3 position, Vector2 size)
    {
        for (int i = 0; i < 3; i++)
        {
            float t = i / 2f;
            GameObject item = Make(parent, name + " Stone " + i, position + new Vector3((t - .5f) * size.x, (i % 2) * .22f, 0f),
                new Vector2(size.x * (.55f + .18f * i), size.y * (.55f + .15f * i)), Rubble, -3);
            BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }
    }

    private static void Torch(Transform parent, string name, Vector3 position)
    {
        Make(parent, name + " Bracket", position, new Vector2(.22f, .65f), WallCap, -2);
        Make(parent, name + " Flame", position + new Vector3(0f, .45f), new Vector2(.32f, .5f), new Color(1f, .48f, .12f, .9f), -1);
        Make(parent, name + " Light", position, new Vector2(1.8f, 1.8f), new Color(1f, .3f, .08f, .08f), -6);
    }

    private static void Landmark(Transform parent, string name, Vector3 position, float size)
    {
        Make(parent, name + " Base", position, new Vector2(size * 1.8f, .22f), new Color(.06f, .25f, .3f, 1f), -2);
        Make(parent, name + " Rune", position + new Vector3(0f, .2f), new Vector2(size, .16f), Rune, -1);
        Make(parent, name + " Rune Vertical", position + new Vector3(0f, -.22f), new Vector2(.16f, size * .75f), Rune, -1);
        Make(parent, name + " Glow", position, new Vector2(size * 2.5f, size * 2.5f), new Color(.05f, .65f, .9f, .08f), -7);
    }

    private static GameObject Make(Transform parent, string name, Vector3 position, Vector2 scale, Color color, int sortingOrder)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        item.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = WhiteSprite();
        renderer.sharedMaterial = UnlitMaterial();
        renderer.color = color;
        // The authored scene contains an older backdrop at sorting order 0.
        // Keep the new world stack above it so the reworked Zone 1 is visible
        // without deleting the reusable room presentation objects.
        renderer.sortingOrder = sortingOrder + 25;
        return item;
    }

    private static Sprite WhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;
        // Reuse the scene's known-good 2D square sprite. Creating a sprite
        // directly from Texture2D.whiteTexture is not rendered reliably by
        // this project's URP 2D setup after the world is moved to large
        // negative/positive coordinates.
        foreach (SpriteRenderer source in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
            if (source.sprite != null && source.sprite.name == "Square")
            {
                whiteSprite = source.sprite;
                return whiteSprite;
            }
        whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
        whiteSprite.name = "Procedural White Pixel";
        return whiteSprite;
    }

    private static Material UnlitMaterial()
    {
        if (unlitMaterial != null) return unlitMaterial;
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            unlitMaterial = new Material(shader);
            unlitMaterial.name = "Zone1 Procedural Unlit Material";
        }
        return unlitMaterial;
    }
}
