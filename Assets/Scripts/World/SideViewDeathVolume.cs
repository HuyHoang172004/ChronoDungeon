using UnityEngine;

/// <summary>
/// Kills actors that fall below authored side-view collision. The volume is a
/// trigger and never behaves as an invisible floor.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class SideViewDeathVolume : MonoBehaviour
{
    [SerializeField] private LayerMask affectedLayers;

    private void Awake()
    {
        Collider2D volume = GetComponent<Collider2D>();
        volume.isTrigger = true;
        if (affectedLayers.value == 0)
            affectedLayers = LayerMask.GetMask("SideViewPlayer", "SideViewEnemy");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((affectedLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Health health = other.GetComponentInParent<Health>();
        if (health != null && !health.IsDead)
            health.TakeDamage(health.currentHealth);
    }
}
