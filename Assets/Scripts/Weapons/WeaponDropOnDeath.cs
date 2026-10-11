using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Health))]
public sealed class WeaponDropOnDeath : MonoBehaviour
{
    [SerializeField] private WeaponDefinition weapon;
    private Health health;
    private bool dropped;

    private void Awake() => health = GetComponent<Health>();

    private void OnEnable()
    {
        if (health == null) health = GetComponent<Health>();
        if (health != null) health.Died += Drop;
        dropped = false;
    }

    private void OnDisable()
    {
        if (health != null) health.Died -= Drop;
    }

    private void Drop(Health _)
    {
        if (dropped || weapon == null) return;
        dropped = true;
        GameObject pickupRoot = new GameObject(weapon.WeaponName + " Pickup");
        pickupRoot.transform.position = transform.position;
        SpriteRenderer renderer = pickupRoot.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 12;
        renderer.sprite = weapon.WeaponSprite;
        // Readable at a glance without dwarfing the Player. The cyan presentation,
        // rather than raw size, is what makes this drop stand out.
        renderer.transform.localScale = Vector3.one * 1.18f;

        GameObject glowObject = new GameObject("ChronoGlow");
        glowObject.transform.SetParent(pickupRoot.transform, false);
        SpriteRenderer glow = glowObject.AddComponent<SpriteRenderer>();
        glow.sprite = weapon.WeaponSprite;
        glow.sortingOrder = 11;
        glow.color = new Color(0.05f, 0.92f, 1f, 0.28f);
        glow.transform.localScale = Vector3.one * 1.78f;

        CircleCollider2D trigger = pickupRoot.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.90f;
        WeaponPickup pickup = pickupRoot.AddComponent<WeaponPickup>();
        pickup.Configure(weapon);
        WeaponPickupWorldVisual presentation = pickupRoot.AddComponent<WeaponPickupWorldVisual>();
        presentation.Configure(renderer, glow, null);
    }
}
