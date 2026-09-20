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

    public float maxHealth => maximumHealth;
    public float currentHealth => health;
    public bool IsDead => health <= 0f;
    public event Action<Health> Changed;

    private void Awake()
    {
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
        if (!IsDead) return;

        onDeath.Invoke();
        if (deathAction == DeathAction.Deactivate)
            gameObject.SetActive(false);
        else if (deathAction == DeathAction.Destroy)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    // Healing does not resurrect dead actors; respawning is a separate concern.
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f || float.IsNaN(amount)) return;
        health = Mathf.Clamp(health + amount, 0f, maximumHealth);
        Changed?.Invoke(this);
    }
}
