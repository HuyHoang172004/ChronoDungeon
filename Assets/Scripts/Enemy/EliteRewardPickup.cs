using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
public sealed class EliteRewardPickup : MonoBehaviour
{
    [SerializeField, Min(1f)] private float healAmount = 35f;
    private SpriteRenderer visual;
    private Collider2D trigger;
    private bool consumed;

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    public bool IsConsumed => consumed;
    public void SetHealAmount(float amount) => healAmount = Mathf.Max(1f, amount);
    public void ResetReward()
    {
        consumed = false;
        gameObject.SetActive(true);
        if (visual != null) visual.enabled = true;
        if (trigger != null) trigger.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;
        Health target = other.GetComponentInParent<Health>();
        if (target == null || !target.CompareTag("Player")) return;
        target.Heal(healAmount);
        consumed = true;
        if (visual != null) visual.enabled = false;
        if (trigger != null) trigger.enabled = false;
        gameObject.SetActive(false);
    }
}
