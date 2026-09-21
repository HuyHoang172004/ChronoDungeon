using UnityEngine;

// Builds the collision shell for one authored room. The east wall leaves the
// marked exit opening; all other edges keep movement inside the room footprint.
[DisallowMultipleComponent]
public sealed class RoomBoundary : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField, Min(2f)] private float width = 16.3f;
    [SerializeField, Min(2f)] private float height = 7.1f;
    [SerializeField, Min(0.05f)] private float wallThickness = 0.35f;
    [SerializeField, Min(0.2f)] private float exitOpening = 2.25f;
    private Transform shell;

    public bool IsConfigured => content != null && shell != null &&
        shell.GetComponentsInChildren<BoxCollider2D>(true).Length == 5;

    private void Awake()
    {
        if (content == null) return;
        shell = content.transform.Find("Solid Room Boundaries");
        if (shell != null) return;
        shell = new GameObject("Solid Room Boundaries").transform;
        shell.SetParent(content.transform, false);
        Create("North Wall", new Vector2(0f, height * 0.5f), new Vector2(width, wallThickness));
        Create("South Wall", new Vector2(0f, -height * 0.5f), new Vector2(width, wallThickness));
        Create("West Wall", new Vector2(-width * 0.5f, 0f), new Vector2(wallThickness, height));
        float sideHeight = (height - exitOpening) * 0.5f;
        Create("East Upper Wall", new Vector2(width * 0.5f, (height + exitOpening) * 0.25f), new Vector2(wallThickness, sideHeight));
        Create("East Lower Wall", new Vector2(width * 0.5f, -(height + exitOpening) * 0.25f), new Vector2(wallThickness, sideHeight));
    }

    private void Create(string name, Vector2 localPosition, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(shell, false);
        wall.transform.localPosition = localPosition;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
}
