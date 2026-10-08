using UnityEngine;

[DisallowMultipleComponent]
public sealed class ChronoBurstCastPresentation : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float castDuration = 0.16f;
    [SerializeField, Min(0f)] private float windupDistance = 0.035f;
    [SerializeField, Min(0f)] private float releaseDistance = 0.07f;
    [SerializeField, Range(0f, 35f)] private float windupWeaponAngle = 18f;
    [SerializeField, Range(0f, 35f)] private float releaseWeaponAngle = 14f;

    private Transform playerVisual;
    private Transform weaponSocket;
    private SpriteRenderer weaponRenderer;
    private PlayerVisualPresentation playerPresentation;
    private Vector3 baseVisualPosition;
    private Quaternion baseVisualRotation;
    private Vector3 baseVisualScale;
    private Vector3 baseSocketPosition;
    private Quaternion baseSocketRotation;
    private Vector3 baseSocketScale;
    private Color baseWeaponColor = Color.white;
    private Vector2 castDirection;
    private Color glowColor;
    private float elapsed = -1f;

    private void Awake()
    {
        ResolveReferences();
    }

    public void Play(Vector2 direction, Color weaponGlow)
    {
        ResolveReferences();
        if (playerVisual == null) return;

        Restore();
        castDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        glowColor = weaponGlow;
        baseVisualPosition = playerVisual.localPosition;
        baseVisualRotation = playerVisual.localRotation;
        baseVisualScale = playerVisual.localScale;

        if (weaponSocket != null)
        {
            baseSocketPosition = weaponSocket.localPosition;
            baseSocketRotation = weaponSocket.localRotation;
            baseSocketScale = weaponSocket.localScale;
        }

        if (weaponRenderer != null) baseWeaponColor = weaponRenderer.color;
        if (playerPresentation != null) playerPresentation.SetExternalPresentationLock(true);
        elapsed = 0f;
    }

    private void Update()
    {
        if (elapsed < 0f || playerVisual == null) return;

        elapsed += Time.deltaTime;
        float normalized = Mathf.Clamp01(elapsed / castDuration);
        float distance;
        float weaponAngle;
        float glow;

        if (normalized < 0.28f)
        {
            float phase = Ease(normalized / 0.28f);
            distance = Mathf.Lerp(0f, -windupDistance, phase);
            weaponAngle = Mathf.Lerp(0f, -windupWeaponAngle, phase);
            glow = Mathf.Lerp(0f, 0.45f, phase);
        }
        else if (normalized < 0.72f)
        {
            float phase = Ease((normalized - 0.28f) / 0.44f);
            distance = Mathf.Lerp(-windupDistance, releaseDistance, phase);
            weaponAngle = Mathf.Lerp(-windupWeaponAngle, releaseWeaponAngle, phase);
            glow = Mathf.Lerp(0.45f, 1f, phase);
        }
        else
        {
            float phase = Ease((normalized - 0.72f) / 0.28f);
            distance = Mathf.Lerp(releaseDistance, 0f, phase);
            weaponAngle = Mathf.Lerp(releaseWeaponAngle, 0f, phase);
            glow = Mathf.Lerp(1f, 0f, phase);
        }

        float parentScale = playerVisual.parent == null ? 1f : Mathf.Max(0.0001f, Mathf.Abs(playerVisual.parent.lossyScale.x));
        Vector3 localDirection = new Vector3(castDirection.x, castDirection.y, 0f) / parentScale;
        float lean = castDirection.x * Mathf.Lerp(-3f, 4f, normalized);
        playerVisual.localPosition = baseVisualPosition + localDirection * distance;
        playerVisual.localRotation = baseVisualRotation * Quaternion.Euler(0f, 0f, lean);
        playerVisual.localScale = baseVisualScale;

        if (weaponSocket != null)
        {
            weaponSocket.localPosition = baseSocketPosition;
            weaponSocket.localRotation = baseSocketRotation * Quaternion.Euler(0f, 0f, weaponAngle);
            weaponSocket.localScale = baseSocketScale;
        }

        if (weaponRenderer != null)
        {
            Color target = Color.Lerp(baseWeaponColor, glowColor, 0.35f);
            weaponRenderer.color = Color.Lerp(baseWeaponColor, target, glow);
        }

        if (normalized >= 1f) Restore();
    }

    private void ResolveReferences()
    {
        if (playerVisual == null) playerVisual = transform.Find("PlayerVisual");
        if (playerVisual == null) return;
        if (weaponSocket == null) weaponSocket = playerVisual.Find("WeaponSocket");
        if (weaponRenderer == null && weaponSocket != null) weaponRenderer = weaponSocket.GetComponent<SpriteRenderer>();
        if (playerPresentation == null) playerPresentation = playerVisual.GetComponent<PlayerVisualPresentation>();
    }

    private void Restore()
    {
        if (elapsed < 0f) return;
        if (playerVisual != null)
        {
            playerVisual.localPosition = baseVisualPosition;
            playerVisual.localRotation = baseVisualRotation;
            playerVisual.localScale = baseVisualScale;
        }
        if (weaponSocket != null)
        {
            weaponSocket.localPosition = baseSocketPosition;
            weaponSocket.localRotation = baseSocketRotation;
            weaponSocket.localScale = baseSocketScale;
        }
        if (weaponRenderer != null) weaponRenderer.color = baseWeaponColor;
        if (playerPresentation != null) playerPresentation.SetExternalPresentationLock(false);
        elapsed = -1f;
    }

    private void OnDisable()
    {
        Restore();
    }

    private static float Ease(float value)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));
    }
}
