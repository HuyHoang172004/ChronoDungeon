using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChronoGuardian), typeof(SpriteRenderer))]
public sealed class ChronoGuardianPolish : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(.05f)] private float hitFlashDuration = .08f;
    [SerializeField, Min(.1f)] private float deathSequenceDuration = 1.2f;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private Color phaseColor = new Color(1f, .35f, .9f, 1f);
    [SerializeField] private Color deathColor = new Color(.25f, .15f, .45f, .45f);
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip phaseClip;
    [SerializeField] private AudioClip deathClip;
    private ChronoGuardian guardian;
    private Health health;
    private SpriteRenderer visual;
    private Color baseColor;
    private Coroutine flashRoutine;
    private bool phaseFeedbackShown;
    private bool deathStarted;
    private AudioSource audioSource;

    public bool DeathStarted => deathStarted;
    public event Action PhaseFeedback;
    public event Action DeathFeedback;

    private void Awake()
    {
        guardian = GetComponent<ChronoGuardian>();
        health = guardian.Health != null ? guardian.Health : GetComponent<Health>();
        visual = GetComponent<SpriteRenderer>();
        baseColor = visual.color;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        health.Changed += OnHealthChanged;
    }

    private void OnHealthChanged(Health changed)
    {
        if (changed.IsDead)
        {
            if (!deathStarted) StartCoroutine(DeathRoutine());
            return;
        }
        if (visual != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HitFlash());
        }
        PlayClip(hitClip);
        if (!phaseFeedbackShown && changed.currentHealth <= changed.maxHealth * .5f)
        {
            phaseFeedbackShown = true;
            if (visual != null) visual.color = phaseColor;
            PlayClip(phaseClip);
            PhaseFeedback?.Invoke();
        }
    }

    private IEnumerator HitFlash()
    {
        visual.color = hitColor;
        yield return new WaitForSecondsRealtime(hitFlashDuration);
        if (!deathStarted) visual.color = baseColor;
        flashRoutine = null;
    }

    private IEnumerator DeathRoutine()
    {
        deathStarted = true;
        DeathFeedback?.Invoke();
        PlayClip(deathClip);
        if (visual != null) visual.color = deathColor;
        foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
        yield return new WaitForSecondsRealtime(deathSequenceDuration);
        if (gameManager != null) gameManager.Victory();
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        StopAllCoroutines();
        deathStarted = false;
        phaseFeedbackShown = false;
        if (visual != null) visual.color = baseColor;
        foreach (var collider in GetComponents<Collider2D>()) collider.enabled = true;
    }

    private void OnDestroy()
    {
        if (health != null) health.Changed -= OnHealthChanged;
    }
}
