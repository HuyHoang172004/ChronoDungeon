using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health), typeof(EnemyAttackTarget), typeof(BoxCollider2D))]
public sealed class ChronoGuardian : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField, Min(1f)] private float bossHealth = 500f;
    [SerializeField] private string introMessage = "CHRONO GUARDIAN AWAKENS";
    private Health health;
    private Vector3 initialPosition;
    private bool introShown;

    public Health Health => health;
    public float BossHealth => bossHealth;
    public string IntroMessage => introMessage;
    public bool IntroShown => introShown;
    public event Action<ChronoGuardian> IntroStarted;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.SetMaximumHealth(bossHealth, true);
        initialPosition = transform.position;
    }

    private void OnEnable()
    {
        if (health != null) health.RestoreToFullHealth();
        introShown = false;
        IntroStarted?.Invoke(this);
    }

    public void CaptureInitialState() => initialPosition = transform.position;

    public void ResetToInitialState()
    {
        transform.position = initialPosition;
        introShown = false;
        if (health != null) health.RestoreToFullHealth();
    }

    public void MarkIntroShown() => introShown = true;
}
