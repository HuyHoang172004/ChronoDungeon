using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class RuneKey : MonoBehaviour
{
    [SerializeField] private RuneKeyInventory inventory;
    [SerializeField] private SpriteRenderer glow;
    [SerializeField] private Health[] requiredEnemies;
    private Collider2D pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;
        if (inventory == null) inventory = FindAnyObjectByType<RuneKeyInventory>();
        if (glow == null)
        {
            GameObject visual = new GameObject("Rune Key Glow");
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(.55f, .55f, 1f);
            glow = visual.AddComponent<SpriteRenderer>();
            glow.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
            glow.color = new Color(.15f, 1f, .85f, .95f);
            glow.sortingOrder = 10;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (inventory == null || other.attachedRigidbody == null ||
            other.attachedRigidbody.GetComponent<PlayerMovement>() == null) return;
        if (requiredEnemies != null)
            foreach (Health enemy in requiredEnemies) if (enemy != null && !enemy.IsDead) return;
        inventory.CollectRuneKey();
        if (glow != null) glow.enabled = false;
        pickupCollider.enabled = false;
    }
}
