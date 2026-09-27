using UnityEngine;

// Builds the collision shell for one authored room. The east wall leaves the
// marked exit opening; all other edges keep movement inside the room footprint.
[DisallowMultipleComponent]
public sealed class RoomBoundary : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField, Min(2f)] private float width = 16.3f;
    [SerializeField, Min(2f)] private float height = 7.1f;
    [SerializeField] private Vector2 centerOffset;
    [SerializeField, Min(0.05f)] private float wallThickness = 0.35f;
    [SerializeField, Min(0.2f)] private float exitOpening = 2.25f;
    private Transform shell;

    public bool IsConfigured => content != null && shell != null &&
        shell.GetComponentsInChildren<BoxCollider2D>(true).Length == 5;

    private void Awake()
    {
        CreateShell();
    }

    public void Rebuild()
    {
        if (shell != null)
        {
            if (Application.isPlaying) Destroy(shell.gameObject);
            else DestroyImmediate(shell.gameObject);
            shell = null;
        }
        CreateShell();
    }

    private void CreateShell()
    {
        if (content == null) return;
        shell = content.transform.Find("Solid Room Boundaries");
        if (shell != null && ShellMatches()) return;
        if (shell != null)
        {
            if (Application.isPlaying) Destroy(shell.gameObject);
            else DestroyImmediate(shell.gameObject);
            shell = null;
        }
        shell = new GameObject("Solid Room Boundaries").transform;
        shell.SetParent(content.transform, false);
        Create("North Wall", centerOffset + new Vector2(0f, height * 0.5f), new Vector2(width, wallThickness));
        Create("South Wall", centerOffset + new Vector2(0f, -height * 0.5f), new Vector2(width, wallThickness));
        Create("West Wall", centerOffset + new Vector2(-width * 0.5f, 0f), new Vector2(wallThickness, height));
        float sideHeight = (height - exitOpening) * 0.5f;
        Create("East Upper Wall", centerOffset + new Vector2(width * 0.5f, (height + exitOpening) * 0.25f), new Vector2(wallThickness, sideHeight));
        Create("East Lower Wall", centerOffset + new Vector2(width * 0.5f, -(height + exitOpening) * 0.25f), new Vector2(wallThickness, sideHeight));
    }

    private void Create(string name, Vector2 localPosition, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(shell, false);
        wall.transform.localPosition = localPosition;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private bool ShellMatches()
    {
        Transform north = shell.Find("North Wall");
        Transform south = shell.Find("South Wall");
        Transform west = shell.Find("West Wall");
        if (north == null || south == null || west == null) return false;
        BoxCollider2D northCollider = north.GetComponent<BoxCollider2D>();
        BoxCollider2D southCollider = south.GetComponent<BoxCollider2D>();
        BoxCollider2D westCollider = west.GetComponent<BoxCollider2D>();
        return northCollider != null && southCollider != null && westCollider != null &&
            Mathf.Abs(northCollider.size.x - width) < .01f &&
            Mathf.Abs(southCollider.size.x - width) < .01f &&
            Mathf.Abs(westCollider.size.y - height) < .01f;
    }
}
