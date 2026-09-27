using UnityEngine;

/// Key-locked gate adapter. The Door remains the reusable blocker/visual;
/// this component only supplies the Zone objective requirement.
[RequireComponent(typeof(Door))]
public sealed class RuneGate : MonoBehaviour
{
    [SerializeField] private RuneKeyInventory inventory;
    [SerializeField] private SpriteRenderer runeMark;
    private Door door;

    private void Awake()
    {
        door = GetComponent<Door>();
        if (inventory == null) inventory = FindAnyObjectByType<RuneKeyInventory>();
        if (runeMark == null)
        {
            GameObject visual = new GameObject("Rune Gate Sigil");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            visual.transform.localScale = new Vector3(1.2f, .16f, 1f);
            runeMark = visual.AddComponent<SpriteRenderer>();
            runeMark.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
            runeMark.sortingOrder = 12;
        }
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.KeyChanged += OnKeyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.KeyChanged -= OnKeyChanged;
    }

    private void OnKeyChanged(bool _)
    {
        Refresh();
        Room room = GetComponentInParent<Room>();
        if (room != null) room.Evaluate();
    }
    private void Refresh()
    {
        bool unlocked = inventory != null && inventory.HasRuneKey;
        if (unlocked) door.Open();
        if (runeMark != null) runeMark.color = unlocked ? new Color(0.2f, 1f, 0.85f, 1f) : new Color(1f, 0.35f, 0.12f, 1f);
    }
}
