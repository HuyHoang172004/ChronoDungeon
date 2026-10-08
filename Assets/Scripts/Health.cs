using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    public enum DeathAction { None, Deactivate, Destroy }

    [SerializeField, Min(1f)] private float maximumHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private DeathAction deathAction = DeathAction.None;
    [SerializeField] private UnityEvent onDeath = new UnityEvent();
    private CombatFeedback feedback;

    public float maxHealth => maximumHealth;
    public float currentHealth => health;
    public bool IsDead => health <= 0f;
    public event Action<Health> Changed;
    public event Action<Health> Died;

    private void Awake()
    {
        feedback = GetComponent<CombatFeedback>();
        if (feedback == null) feedback = gameObject.AddComponent<CombatFeedback>();
        ValidateValues();
        health = maximumHealth;
    }

    private void OnValidate() => ValidateValues();

    private void ValidateValues()
    {
        if (float.IsNaN(maximumHealth) || float.IsInfinity(maximumHealth))
            maximumHealth = 100f;
        maximumHealth = Mathf.Max(1f, maximumHealth);
        health = float.IsNaN(health) ? maximumHealth : Mathf.Clamp(health, 0f, maximumHealth);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f || float.IsNaN(damage)) return;
        health = Mathf.Clamp(health - damage, 0f, maximumHealth);
        Changed?.Invoke(this);
        if (feedback != null) feedback.PlayHit();
        if (!IsDead) return;

        if (feedback != null) feedback.PlayDeath();
        Died?.Invoke(this);
        onDeath.Invoke();
        if (deathAction == DeathAction.Deactivate)
            gameObject.SetActive(false);
        else if (deathAction == DeathAction.Destroy)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    // Explicit lifecycle reset can revive an actor; ordinary healing cannot.
    public void RestoreToFullHealth()
    {
        ValidateValues();
        health = maximumHealth;
        Changed?.Invoke(this);
    }

    // Healing does not resurrect dead actors; respawning is a separate concern.
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f || float.IsNaN(amount)) return;
        health = Mathf.Clamp(health + amount, 0f, maximumHealth);
        Changed?.Invoke(this);
    }

    // Used by authored elite variants during scene setup; ordinary upgrades should use separate systems.
    public void SetMaximumHealth(float value, bool refill)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return;
        maximumHealth = Mathf.Max(1f, value);
        health = refill ? maximumHealth : Mathf.Clamp(health, 0f, maximumHealth);
        Changed?.Invoke(this);
    }
}
