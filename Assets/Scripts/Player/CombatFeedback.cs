using UnityEngine;

[DisallowMultipleComponent]
public sealed class CombatFeedback : MonoBehaviour
{
    [SerializeField] private Color hitColor = new Color(1f, 0.28f, 0.22f, 1f);
    [SerializeField] private Color deathColor = new Color(1f, 0.08f, 0.08f, 1f);
    [SerializeField, Min(0.01f)] private float hitDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float deathDuration = 0.2f;
    private SpriteRenderer[] sprites;
    private Color[] baseColors;
    private float flashUntil;
    private Color flashColor;
    private Vector3 baseScale;
    private AudioSource audioSource;
    private AudioClip attackClip;
    private AudioClip hitClip;
    private AudioClip deathClip;

    private void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) baseColors[i] = sprites[i].color;
        baseScale = transform.localScale;
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        attackClip = MakeTone("PlayerAttackFeedback", 680f, 0.06f, 0.1f);
        hitClip = MakeTone("DamageFeedback", 190f, 0.08f, 0.14f);
        deathClip = MakeTone("DeathFeedback", 95f, 0.22f, 0.2f);
    }

    public void PlayAttack()
    {
        if (audioSource != null && attackClip != null) audioSource.PlayOneShot(attackClip);
    }

    public void PlayHit()
    {
        DamageFeedback existing = GetComponent<DamageFeedback>();
        if (existing != null) existing.Play();
        else Flash(hitColor, hitDuration, 1.08f);
        if (audioSource != null && hitClip != null) audioSource.PlayOneShot(hitClip);
    }

    public void PlayDeath()
    {
        Flash(deathColor, deathDuration, 1.16f);
        if (audioSource != null && deathClip != null) audioSource.PlayOneShot(deathClip);
    }

    private void Flash(Color color, float duration, float scale)
    {
        flashColor = color;
        flashUntil = Time.time + duration;
        transform.localScale = baseScale * scale;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = flashColor;
    }

    private void Update()
    {
        if (Time.time < flashUntil) return;
        transform.localScale = baseScale;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = baseColors[i];
    }

    private void OnDisable()
    {
        flashUntil = 0f;
        transform.localScale = baseScale;
        if (sprites == null) return;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = baseColors[i];
    }

    private static AudioClip MakeTone(string name, float frequency, float duration, float volume)
    {
        int rate = 22050;
        int samples = Mathf.CeilToInt(rate * duration);
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float envelope = 1f - i / (float)samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * envelope * volume;
        }
        clip.SetData(data, 0);
        return clip;
    }
}
