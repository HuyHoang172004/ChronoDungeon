using UnityEngine;

/// Lightweight authored-looking dressing for the first vertical slice. It uses
/// a single batched-friendly white sprite with layered colors, avoiding raw
/// primitive geometry while leaving room for imported art in the later art pass.
public sealed class Zone1VisualDresser : MonoBehaviour
{
    [SerializeField] private Color floorColor = new Color(0.055f, 0.075f, 0.105f, 1f);
    [SerializeField] private Color trimColor = new Color(0.12f, 0.22f, 0.28f, 1f);
    [SerializeField] private Color accentColor = new Color(0.15f, 0.72f, 0.82f, 1f);
    [SerializeField] private Vector2 size = new Vector2(16.3f, 7.1f);

    private void Awake()
    {
        if (transform.Find("Zone 1 Art") != null) return;
        GameObject root = new GameObject("Zone 1 Art");
        root.transform.SetParent(transform, false);
        Make(root.transform, "Ancient Floor", Vector3.zero, size, floorColor, -5);
        Make(root.transform, "North Rune Trim", new Vector3(0f, size.y * .5f - .32f, 0f), new Vector2(size.x, .18f), trimColor, -4);
        Make(root.transform, "South Rune Trim", new Vector3(0f, -size.y * .5f + .32f, 0f), new Vector2(size.x, .18f), trimColor, -4);
        for (int i = -2; i <= 2; i++)
        {
            Make(root.transform, "Floor Inlay " + i, new Vector3(i * 2.7f, 0f, 0f), new Vector2(.035f, size.y - .8f), new Color(accentColor.r, accentColor.g, accentColor.b, .18f), -3);
        }
        Make(root.transform, "North Sigil", new Vector3(-size.x * .27f, size.y * .5f - .5f, 0f), new Vector2(.7f, .12f), accentColor, -3);
        Make(root.transform, "South Sigil", new Vector3(size.x * .27f, -size.y * .5f + .5f, 0f), new Vector2(.7f, .12f), accentColor, -3);
    }

    private static void Make(Transform parent, string name, Vector3 position, Vector2 scale, Color color, int order)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        item.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
        renderer.color = color;
        renderer.sortingOrder = order;
    }
}
