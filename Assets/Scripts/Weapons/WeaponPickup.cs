using UnityEngine;

[DisallowMultipleComponent]
public sealed class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponDefinition weapon;
    [SerializeField] private WeaponPickupPanel pickupPanel;
    private bool consumed;

    public WeaponDefinition Weapon => weapon;

    public void Configure(WeaponDefinition definition, WeaponPickupPanel panel = null)
    {
        weapon = definition;
        pickupPanel = panel;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = weapon != null ? weapon.WeaponSprite : null;
            renderer.enabled = renderer.sprite != null;
        }
    }

    private void Awake()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null && weapon != null && weapon.WeaponSprite != null)
        {
            renderer.sprite = weapon.WeaponSprite;
            renderer.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryOffer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryOffer(collision.collider);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || pickupPanel == null) return;
        PlayerWeaponController controller = other.GetComponentInParent<PlayerWeaponController>();
        if (controller != null) pickupPanel.HideFor(this);
    }

    private void TryOffer(Collider2D other)
    {
        if (consumed || weapon == null || other == null) return;
        PlayerWeaponController controller = other.GetComponentInParent<PlayerWeaponController>();
        if (controller == null) return;

        if (pickupPanel == null) pickupPanel = FindAnyObjectByType<WeaponPickupPanel>();
        if (pickupPanel != null) pickupPanel.Show(this, controller);
    }

    public bool Equip(PlayerWeaponController controller)
    {
        if (consumed || weapon == null || controller == null || !controller.EquipWeapon(weapon)) return false;
        consumed = true;
        gameObject.SetActive(false);
        return true;
    }
}
