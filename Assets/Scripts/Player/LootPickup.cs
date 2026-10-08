using UnityEngine;

/// <summary>
/// Small reusable world pickup for the first combat loop.  It deliberately has
/// no gameplay collider beyond its trigger and cleans itself up on a loop rewind.
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
public sealed class LootPickup : MonoBehaviour
{
    public enum Kind { Gold, Health, Energy, TemporalBean }

    [SerializeField] private Kind kind;
    [SerializeField, Min(1f)] private float amount = 1f;
    [SerializeField, Min(0f)] private float lifetime = 18f;

    private SpriteRenderer visual;
    private Collider2D trigger;
    private TimeLoopManager timeLoop;
    private PlayerMovement player;
    private float spawnedAt;
    private Vector3 basePosition;
    private bool collected;

    private static Sprite markerSprite;

    public static LootPickup Spawn(Vector3 position, Kind pickupKind, float pickupAmount)
    {
        GameObject root = new GameObject(pickupKind + " Loot");
        root.transform.position = position;
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = GetMarkerSprite();
        renderer.sortingOrder = 8;
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.28f;
        LootPickup pickup = root.AddComponent<LootPickup>();
        pickup.Configure(pickupKind, pickupAmount);
        return pickup;
    }

    public void Configure(Kind pickupKind, float pickupAmount)
    {
        kind = pickupKind;
        amount = Mathf.Max(1f, pickupAmount);
        ApplyVisual();
    }

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
        spawnedAt = Time.time;
        basePosition = transform.position;
        timeLoop = FindAnyObjectByType<TimeLoopManager>();
        player = FindAnyObjectByType<PlayerMovement>();
        if (timeLoop != null) timeLoop.LoopEnding += DespawnForLoop;
        ApplyVisual();
    }

    private void OnDestroy()
    {
        if (timeLoop != null) timeLoop.LoopEnding -= DespawnForLoop;
    }

    private void Update()
    {
        if (collected) return;
        transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * 4f + basePosition.x) * 0.06f);
        if (lifetime > 0f && Time.time - spawnedAt >= lifetime) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (collected || player == null) return;
        if (Vector2.SqrMagnitude((Vector2)player.transform.position - (Vector2)transform.position) <= 0.4f)
            Collect(player.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected || other == null) return;
        PlayerMovement playerMovement = other.GetComponentInParent<PlayerMovement>();
        if (playerMovement == null) return;
        Collect(playerMovement.gameObject);
    }

    private void Collect(GameObject playerObject)
    {
        if (collected || playerObject == null) return;
        bool granted = false;
        switch (kind)
        {
            case Kind.Gold:
                PlayerResources resources = playerObject.GetComponent<PlayerResources>();
                if (resources != null) { resources.AddGold(Mathf.RoundToInt(amount)); granted = true; }
                break;
            case Kind.Health:
                Health health = playerObject.GetComponent<Health>();
                if (health != null && !health.IsDead) { health.Heal(amount); granted = true; }
                break;
            case Kind.Energy:
                PlayerResources energy = playerObject.GetComponent<PlayerResources>();
                if (energy != null) { energy.RestoreEnergy(amount); granted = true; }
                break;
            case Kind.TemporalBean:
                PlayerConsumables consumables = playerObject.GetComponent<PlayerConsumables>();
                if (consumables != null) { consumables.AddTemporalBeans(Mathf.RoundToInt(amount)); granted = true; }
                break;
        }

        if (!granted) return;
        collected = true;
        Destroy(gameObject);
    }

    private void DespawnForLoop() => Destroy(gameObject);

    private void ApplyVisual()
    {
        if (visual == null) visual = GetComponent<SpriteRenderer>();
        if (visual == null) return;
        visual.sprite = GetMarkerSprite();
        visual.color = kind == Kind.Gold ? new Color(1f, 0.76f, 0.18f) :
            kind == Kind.Health ? new Color(1f, 0.2f, 0.28f) :
            kind == Kind.Energy ? new Color(0.08f, 0.82f, 1f) :
            new Color(0.3f, 1f, 0.72f);
    }

    private static Sprite GetMarkerSprite()
    {
        if (markerSprite != null) return markerSprite;
        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
            texture.SetPixel(x, y, distance <= 6.5f ? Color.white : Color.clear);
        }
        texture.Apply();
        markerSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        markerSprite.name = "RuntimeLootMarker";
        return markerSprite;
    }
}
