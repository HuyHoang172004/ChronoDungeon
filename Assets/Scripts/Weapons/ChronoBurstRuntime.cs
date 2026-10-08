using UnityEngine;

[DisallowMultipleComponent]
public sealed class ChronoBurstRuntime : MonoBehaviour
{
    private const int RingSegments = 24;

    [Header("Optional authored presentation")]
    [SerializeField] private Sprite burstSprite;
    [SerializeField] private Sprite impactSprite;

    private PlayerWeaponController controller;
    private PlayerMovement movement;
    private AttackResolver damageResolver;
    private ChronoBurstCastPresentation castPresentation;
    private Animator playerAnimator;
    private Transform weaponSocket;
    private GameObject visualRoot;
    private LineRenderer ring;
    private LineRenderer core;
    private SpriteRenderer burstSpriteRenderer;
    private SpriteRenderer impactSpriteRenderer;
    private Material ringMaterial;
    private Material coreMaterial;

    private bool active;
    private float elapsed;
    private float duration;
    private float range;
    private float radius;
    private float startOffset;
    private Vector2 origin;
    private Vector2 direction;
    private float zPosition;
    private Color ringColor;
    private Color coreColor;
    private Color accentColor;
    private float animationResetAt = -1f;

    public bool IsActive => active;

    private void Awake()
    {
        controller = GetComponent<PlayerWeaponController>();
        movement = GetComponent<PlayerMovement>();
        damageResolver = new AttackResolver();
        castPresentation = GetComponent<ChronoBurstCastPresentation>();
        Transform visual = transform.Find("PlayerVisual");
        if (visual != null)
        {
            playerAnimator = visual.GetComponent<Animator>();
            weaponSocket = visual.Find("WeaponSocket");
        }
    }

    public bool Cast(SkillDefinition skill, float visualLifetime, float offset,
        Color burstRingColor, Color burstCoreColor, Color burstAccentColor)
    {
        if (skill == null || active || controller == null) return false;

        if (movement == null) movement = GetComponent<PlayerMovement>();
        direction = movement != null ? movement.FacingDirection : Vector2.right;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        direction.Normalize();

        duration = Mathf.Max(0.25f, visualLifetime);
        startOffset = Mathf.Max(0f, offset);
        range = skill.Range > 0f ? skill.Range : 3.5f;
        radius = skill.Radius > 0f ? skill.Radius : 0.8f;
        // The pulse should visibly leave the equipped blade, while gameplay hit
        // resolution remains anchored to the Player as before.
        if (weaponSocket == null)
        {
            Transform visual = transform.Find("PlayerVisual");
            if (visual != null) weaponSocket = visual.Find("WeaponSocket");
        }
        origin = weaponSocket != null ? (Vector2)weaponSocket.position : (Vector2)transform.position;
        origin += direction * startOffset;
        zPosition = transform.position.z - 0.12f;
        ringColor = burstRingColor;
        coreColor = burstCoreColor;
        accentColor = burstAccentColor;
        elapsed = 0f;
        active = true;

        // The existing resolver owns enemy filtering, shields, hit feedback and damage.
        // The full directional pulse is resolved once, so an enemy can only be hit once per cast.
        damageResolver.Execute(new AttackSnapshot
        {
            Position = transform.position,
            Direction = direction,
            Range = range + radius,
            Damage = skill.Damage,
            ArcAngle = 120f
        });

        EnsureVisual();
        visualRoot.SetActive(true);
        UpdateVisual(0f);
        if (castPresentation == null) castPresentation = GetComponent<ChronoBurstCastPresentation>();
        if (castPresentation == null) castPresentation = gameObject.AddComponent<ChronoBurstCastPresentation>();
        castPresentation.Play(direction, coreColor);
        if (playerAnimator == null)
        {
            Transform visual = transform.Find("PlayerVisual");
            if (visual != null) playerAnimator = visual.GetComponent<Animator>();
        }
        if (playerAnimator != null) playerAnimator.SetTrigger("ChronoBurst");
        animationResetAt = Time.time + 0.16f;
        return true;
    }

    private void Update()
    {
        if (animationResetAt >= 0f && Time.time >= animationResetAt)
        {
            if (playerAnimator != null)
                playerAnimator.Play(movement != null && movement.CurrentMoveDirection.sqrMagnitude > 0.0001f ? "Walk" : "Idle", 0, 0f);
            animationResetAt = -1f;
        }

        if (!active) return;

        elapsed += Time.deltaTime;
        float normalized = Mathf.Clamp01(elapsed / duration);
        UpdateVisual(normalized);
        if (normalized < 1f) return;

        active = false;
        if (visualRoot != null) visualRoot.SetActive(false);
    }

    private void EnsureVisual()
    {
        if (visualRoot != null) return;

        visualRoot = new GameObject("ChronoBurstVFX");
        visualRoot.hideFlags = HideFlags.HideAndDontSave;

        ring = visualRoot.AddComponent<LineRenderer>();
        GameObject coreObject = new GameObject("Core");
        coreObject.transform.SetParent(visualRoot.transform, false);
        core = coreObject.AddComponent<LineRenderer>();

        if (burstSprite != null)
        {
            GameObject spriteObject = new GameObject("BurstArt");
            spriteObject.transform.SetParent(visualRoot.transform, false);
            burstSpriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            burstSpriteRenderer.sprite = burstSprite;
            burstSpriteRenderer.sortingOrder = 22;
        }
        if (impactSprite != null)
        {
            GameObject impactObject = new GameObject("ImpactArt");
            impactObject.transform.SetParent(visualRoot.transform, false);
            impactSpriteRenderer = impactObject.AddComponent<SpriteRenderer>();
            impactSpriteRenderer.sprite = impactSprite;
            impactSpriteRenderer.sortingOrder = 19;
        }
        ConfigureLine(ring, 0.075f, RingSegments + 1, 20);
        ConfigureLine(core, 0.095f, 2, 21);

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            ringMaterial = new Material(shader);
            coreMaterial = new Material(shader);
            ring.material = ringMaterial;
            core.material = coreMaterial;
        }
    }

    private static void ConfigureLine(LineRenderer line, float width, int positions, int sortingOrder)
    {
        line.useWorldSpace = true;
        line.loop = false;
        line.positionCount = positions;
        line.startWidth = width;
        line.endWidth = width * 0.55f;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.sortingOrder = sortingOrder;
    }

    private void UpdateVisual(float normalized)
    {
        if (ring == null || core == null) return;

        float eased = 1f - Mathf.Pow(1f - normalized, 2f);
        // Use the moving weapon as the emitter, so the wave stays attached to the
        // cast pose/body lunge before it travels forward.
        Vector2 emitter = weaponSocket != null ? (Vector2)weaponSocket.position : origin;
        Vector2 center = emitter + direction * (range * eased);
        float ringRadius = Mathf.Lerp(0.12f, radius, eased);
        float alpha = Mathf.Clamp01(1f - normalized);

        Color ringTint = WithAlpha(ringColor, ringColor.a * alpha);
        Color coreTint = WithAlpha(coreColor, coreColor.a * alpha);
        Color accentTint = WithAlpha(accentColor, accentColor.a * alpha);
        ring.startColor = ringTint;
        ring.endColor = accentTint;
        core.startColor = coreTint;
        core.endColor = ringTint;

        for (int i = 0; i <= RingSegments; i++)
        {
            float angle = (i / (float)RingSegments) * Mathf.PI * 2f;
            Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius;
            ring.SetPosition(i, new Vector3(point.x, point.y, zPosition));
        }

        Vector2 streakStart = center - direction * Mathf.Lerp(0.42f, 0.12f, normalized);
        Vector2 streakEnd = center + direction * Mathf.Lerp(0.18f, 0.04f, normalized);
        core.SetPosition(0, new Vector3(streakStart.x, streakStart.y, zPosition - 0.01f));
        core.SetPosition(1, new Vector3(streakEnd.x, streakEnd.y, zPosition - 0.01f));

        if (burstSpriteRenderer != null)
        {
            burstSpriteRenderer.transform.position = new Vector3(center.x, center.y, zPosition - 0.02f);
            burstSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            burstSpriteRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.48f, 1f, eased);
            burstSpriteRenderer.color = WithAlpha(Color.white, alpha * 0.82f);
        }
        if (impactSpriteRenderer != null)
        {
            impactSpriteRenderer.transform.position = new Vector3(center.x, center.y, zPosition + 0.01f);
            impactSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            impactSpriteRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.35f, 0.85f, eased);
            impactSpriteRenderer.color = WithAlpha(Color.white, alpha * 0.58f);
        }
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private void OnDestroy()
    {
        if (visualRoot != null) Destroy(visualRoot);
        if (ringMaterial != null) Destroy(ringMaterial);
        if (coreMaterial != null) Destroy(coreMaterial);
    }
}
