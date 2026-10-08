using UnityEngine;

[DisallowMultipleComponent]
public sealed class WeaponPickupWorldVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private Transform halo;
    [SerializeField, Min(0f)] private float floatAmplitude = 0.10f;
    [SerializeField, Min(0f)] private float floatFrequency = 2.2f;
    [SerializeField, Min(0f)] private float pulseAmplitude = 0.12f;

    private Vector3 basePosition;
    private Vector3 baseGlowScale;

    public void Configure(SpriteRenderer weapon, SpriteRenderer glow, Transform ring)
    {
        weaponRenderer = weapon;
        glowRenderer = glow;
        halo = ring;
        basePosition = transform.localPosition;
        if (glowRenderer != null) baseGlowScale = glowRenderer.transform.localScale;
    }

    private void Awake()
    {
        basePosition = transform.localPosition;
        if (glowRenderer != null) baseGlowScale = glowRenderer.transform.localScale;
    }

    private void LateUpdate()
    {
        float wave = Mathf.Sin(Time.time * floatFrequency);
        transform.localPosition = basePosition + Vector3.up * (wave * floatAmplitude);
        float pulse = 1f + (0.5f + 0.5f * wave) * pulseAmplitude;
        if (glowRenderer != null) glowRenderer.transform.localScale = baseGlowScale * pulse;
        if (halo != null)
        {
            halo.localScale = Vector3.one * (1f + (0.5f + 0.5f * wave) * 0.18f);
            halo.Rotate(0f, 0f, 18f * Time.deltaTime);
        }
    }
}
