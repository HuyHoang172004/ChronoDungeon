using UnityEngine;

[DisallowMultipleComponent]
public sealed class TimeCleaveRuntime : MonoBehaviour
{
    private const int ArcSegments = 20;

    [Header("Optional authored presentation")]
    [SerializeField] private Sprite cleaveSprite;

    private PlayerWeaponController controller;
    private PlayerMovement movement;
    private PlayerVisualPresentation presentation;
    private AttackResolver damageResolver;
    private Transform playerVisual;
    private Animator playerAnimator;
    private Transform weaponSocket;
    private SpriteRenderer weaponRenderer;
    private GameObject visualRoot;
    private LineRenderer slashArc;
    private LineRenderer slashCore;
    private SpriteRenderer cleaveSpriteRenderer;
    private Material slashMaterial;
    private Material coreMaterial;

    private bool active;
    private bool damageApplied;
    private float elapsed;
    private float windupDuration;
    private float strikeDuration;
    private float recoveryDuration;
    private float windupDistance;
    private float lungeDistance;
    private float weaponSweepAngle;
    private float totalDuration;
    private float range;
    private float damage;
    private Vector2 direction;
    private float zPosition;
    private Color weaponGlowColor;
    private Color slashColor;
    private Color accentColor;
    private float animationResetAt = -1f;

    private Vector3 restVisualPosition;
    private Quaternion restVisualRotation;
    private Vector3 restVisualScale;
    private Vector3 restWeaponPosition;
    private Quaternion restWeaponRotation;
    private Vector3 restWeaponScale;
    private Color restWeaponColor = Color.white;

    public bool IsActive => active;

    private void Awake()
    {
        controller = GetComponent<PlayerWeaponController>();
        movement = GetComponent<PlayerMovement>();
        damageResolver = new AttackResolver();
        ResolveVisualReferences();
    }

    public bool Cast(SkillDefinition skill, float windup, float strike, float recovery,
        float backwardDistance, float forwardDistance, float sweepAngle,
        Color glow, Color burstColor, Color burstAccent)
    {
        if (skill == null || active || controller == null) return false;

        ResolveVisualReferences();
        if (movement != null) direction = movement.FacingDirection;
        else direction = Vector2.right;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        direction.Normalize();

        CaptureRestingTransforms();
        windupDuration = Mathf.Max(0.1f, windup);
        strikeDuration = Mathf.Max(0.1f, strike);
        recoveryDuration = Mathf.Max(0.1f, recovery);
        totalDuration = windupDuration + strikeDuration + recoveryDuration;
        windupDistance = Mathf.Max(0f, backwardDistance);
        lungeDistance = Mathf.Max(0f, forwardDistance);
        weaponSweepAngle = sweepAngle;
        range = skill.Range > 0f ? skill.Range : 2.6f;
        damage = skill.Damage;
        weaponGlowColor = glow;
        slashColor = burstColor;
        accentColor = burstAccent;
        elapsed = 0f;
        damageApplied = false;
        active = true;
        zPosition = transform.position.z - 0.14f;

        if (presentation != null) presentation.SetExternalPresentationLock(true);
        if (playerAnimator == null && playerVisual != null)
            playerAnimator = playerVisual.GetComponent<Animator>();
        if (playerAnimator != null) playerAnimator.SetTrigger("TimeCleave");
        animationResetAt = Time.time + totalDuration;
        EnsureVisual();
        visualRoot.SetActive(true);
        UpdatePresentation(0f);
        UpdateVisual(0f);
        return true;
    }

    private void Update()
    {
        if (animationResetAt >= 0f && Time.time >= animationResetAt)
        {
            if (playerAnimator != null)
            {
                string locomotionState = movement != null && movement.CurrentMoveDirection.sqrMagnitude > 0.0001f
                    ? "Walk"
                    : "Idle";
                playerAnimator.Play(locomotionState, 0, 0f);
            }
            animationResetAt = -1f;
        }

        if (!active) return;

        elapsed += Time.deltaTime;
        float strikeStart = windupDuration;
        float strikeEnd = strikeStart + strikeDuration;
        float normalized = Mathf.Clamp01(elapsed / totalDuration);

        UpdatePresentation(elapsed);
        UpdateVisual(normalized);

        if (!damageApplied && elapsed >= strikeStart)
        {
            damageApplied = true;
            damageResolver.Execute(new AttackSnapshot
            {
                Position = transform.position,
                Direction = direction,
                Range = range,
                Damage = damage,
                ArcAngle = weaponSweepAngle
            });
        }

        if (elapsed < totalDuration) return;

        active = false;
        RestoreRestingTransforms();
        if (presentation != null) presentation.SetExternalPresentationLock(false);
        if (visualRoot != null) visualRoot.SetActive(false);
    }

    private void ResolveVisualReferences()
    {
        if (playerVisual == null) playerVisual = transform.Find("PlayerVisual");
        if (playerVisual == null) return;
        if (playerAnimator == null) playerAnimator = playerVisual.GetComponent<Animator>();
        if (presentation == null) presentation = playerVisual.GetComponent<PlayerVisualPresentation>();
        if (weaponSocket == null) weaponSocket = playerVisual.Find("WeaponSocket");
        if (weaponRenderer == null && weaponSocket != null)
            weaponRenderer = weaponSocket.GetComponent<SpriteRenderer>();
    }

    private void CaptureRestingTransforms()
    {
        ResolveVisualReferences();
        if (playerVisual != null)
        {
            restVisualPosition = playerVisual.localPosition;
            restVisualRotation = playerVisual.localRotation;
            restVisualScale = playerVisual.localScale;
        }
        if (weaponSocket != null)
        {
            restWeaponPosition = weaponSocket.localPosition;
            restWeaponRotation = weaponSocket.localRotation;
            restWeaponScale = weaponSocket.localScale;
        }
        if (weaponRenderer != null) restWeaponColor = weaponRenderer.color;
    }

    private void RestoreRestingTransforms()
    {
        if (playerVisual != null)
        {
            playerVisual.localPosition = restVisualPosition;
            playerVisual.localRotation = restVisualRotation;
            playerVisual.localScale = restVisualScale;
        }
        if (weaponSocket != null)
        {
            weaponSocket.localPosition = restWeaponPosition;
            weaponSocket.localRotation = restWeaponRotation;
            weaponSocket.localScale = restWeaponScale;
        }
        if (weaponRenderer != null) weaponRenderer.color = restWeaponColor;
    }

    private void UpdatePresentation(float time)
    {
        if (playerVisual == null || weaponSocket == null) return;

        float bodyScale = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        Vector3 localDirection = new Vector3(direction.x, direction.y, 0f) / bodyScale;
        float leanSign = direction.x < -0.01f ? -1f : 1f;
        float bodyDistance;
        float bodyLean;
        float weaponAngle;
        float squash;

        if (time < windupDuration)
        {
            float t = EaseInOut(time / windupDuration);
            bodyDistance = Mathf.Lerp(0f, -windupDistance, t);
            bodyLean = Mathf.Lerp(0f, -4f * leanSign, t);
            weaponAngle = Mathf.Lerp(0f, -32f, t);
            squash = Mathf.Lerp(1f, 0.97f, t);
        }
        else if (time < windupDuration + strikeDuration)
        {
            float t = EaseOut((time - windupDuration) / strikeDuration);
            bodyDistance = Mathf.Lerp(-windupDistance, lungeDistance, t);
            bodyLean = Mathf.Lerp(-4f * leanSign, 6f * leanSign, t);
            weaponAngle = Mathf.Lerp(-32f, weaponSweepAngle - 32f, t);
            squash = Mathf.Lerp(0.97f, 1.04f, t);
        }
        else
        {
            float t = EaseOut((time - windupDuration - strikeDuration) / recoveryDuration);
            bodyDistance = Mathf.Lerp(lungeDistance, 0f, t);
            bodyLean = Mathf.Lerp(6f * leanSign, 0f, t);
            weaponAngle = Mathf.Lerp(weaponSweepAngle - 32f, 0f, t);
            squash = Mathf.Lerp(1.04f, 1f, t);
        }

        Vector3 scale = restVisualScale;
        scale.x = Mathf.Abs(restVisualScale.x) * squash * (direction.x < -0.01f ? -1f : 1f);
        scale.y = restVisualScale.y * (1f + (1f - squash) * 0.5f);
        playerVisual.localScale = scale;
        playerVisual.localPosition = restVisualPosition + localDirection * bodyDistance;
        playerVisual.localRotation = restVisualRotation * Quaternion.Euler(0f, 0f, bodyLean);
        weaponSocket.localPosition = restWeaponPosition;
        weaponSocket.localRotation = restWeaponRotation * Quaternion.Euler(0f, 0f, weaponAngle);
        weaponSocket.localScale = restWeaponScale;

        if (weaponRenderer != null)
        {
            float glowT = time < windupDuration ? time / windupDuration : 1f - Mathf.Clamp01((time - windupDuration) / strikeDuration);
            weaponRenderer.color = Color.Lerp(restWeaponColor, weaponGlowColor, Mathf.Clamp01(glowT) * 0.32f);
        }
    }

    private void EnsureVisual()
    {
        if (visualRoot != null) return;

        visualRoot = new GameObject("TimeCleaveVFX");
        visualRoot.hideFlags = HideFlags.HideAndDontSave;
        slashArc = visualRoot.AddComponent<LineRenderer>();
        GameObject coreObject = new GameObject("Core");
        coreObject.transform.SetParent(visualRoot.transform, false);
        slashCore = coreObject.AddComponent<LineRenderer>();

        if (cleaveSprite != null)
        {
            GameObject spriteObject = new GameObject("CleaveArt");
            spriteObject.transform.SetParent(visualRoot.transform, false);
            cleaveSpriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            cleaveSpriteRenderer.sprite = cleaveSprite;
            cleaveSpriteRenderer.sortingOrder = 24;

            // The authored crescent is the final presentation. Hide the old
            // procedural strokes, which otherwise read as a stray white line.
            slashArc.enabled = false;
            slashCore.enabled = false;
        }
        ConfigureLine(slashArc, 0.10f, ArcSegments + 1, 22);
        ConfigureLine(slashCore, 0.14f, 2, 23);

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            slashMaterial = new Material(shader);
            coreMaterial = new Material(shader);
            slashArc.material = slashMaterial;
            slashCore.material = coreMaterial;
        }
    }

    private static void ConfigureLine(LineRenderer line, float width, int positions, int sortingOrder)
    {
        line.useWorldSpace = true;
        line.positionCount = positions;
        line.startWidth = width;
        line.endWidth = width * 0.45f;
        line.numCapVertices = 3;
        line.numCornerVertices = 3;
        line.sortingOrder = sortingOrder;
    }

    private void UpdateVisual(float normalized)
    {
        if (slashArc == null || slashCore == null) return;

        float strikeStart = windupDuration / totalDuration;
        float strikeEnd = (windupDuration + strikeDuration) / totalDuration;
        float intensity = normalized < strikeStart
            ? Mathf.Lerp(0.12f, 0.35f, normalized / strikeStart)
            : normalized < strikeEnd
                ? Mathf.Lerp(0.35f, 1f, (normalized - strikeStart) / (strikeEnd - strikeStart))
                : Mathf.Lerp(1f, 0f, (normalized - strikeEnd) / (1f - strikeEnd));
        float radius = Mathf.Lerp(0.52f, 0.9f, Mathf.Clamp01(intensity));
        // Anchor the crescent to the live weapon socket rather than Player's root.
        // This keeps the large cleave visually locked to the blade through the swing.
        Vector2 emitter = weaponSocket != null ? (Vector2)weaponSocket.position : (Vector2)transform.position;
        Vector2 center = emitter + direction * Mathf.Lerp(0.20f, 0.62f, Mathf.Clamp01(intensity));
        float centerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float halfAngle = weaponSweepAngle * 0.5f;
        float alpha = Mathf.Clamp01(intensity) * 0.9f;
        slashArc.startColor = WithAlpha(slashColor, slashColor.a * alpha);
        slashArc.endColor = WithAlpha(accentColor, accentColor.a * alpha);
        slashCore.startColor = WithAlpha(Color.white, alpha);
        slashCore.endColor = WithAlpha(slashColor, slashColor.a * alpha);

        for (int i = 0; i <= ArcSegments; i++)
        {
            float angle = (centerAngle - halfAngle) + (weaponSweepAngle * i / ArcSegments);
            Vector2 point = center + DegreeDirection(angle) * radius;
            slashArc.SetPosition(i, new Vector3(point.x, point.y, zPosition));
        }

        Vector2 coreStart = center - direction * 0.42f;
        Vector2 coreEnd = center + direction * 0.42f;
        slashCore.SetPosition(0, new Vector3(coreStart.x, coreStart.y, zPosition - 0.01f));
        slashCore.SetPosition(1, new Vector3(coreEnd.x, coreEnd.y, zPosition - 0.01f));

        if (cleaveSpriteRenderer != null)
        {
            cleaveSpriteRenderer.transform.position = new Vector3(center.x, center.y, zPosition - 0.02f);
            cleaveSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, centerAngle);
            cleaveSpriteRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.35f, intensity);
            cleaveSpriteRenderer.color = WithAlpha(Color.white, alpha * 0.78f);
        }
    }

    private static Vector2 DegreeDirection(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private static float EaseInOut(float value) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));

    private static float EaseOut(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - (1f - value) * (1f - value);
    }

    private void OnDestroy()
    {
        if (presentation != null) presentation.SetExternalPresentationLock(false);
        RestoreRestingTransforms();
        if (visualRoot != null) Destroy(visualRoot);
        if (slashMaterial != null) Destroy(slashMaterial);
        if (coreMaterial != null) Destroy(coreMaterial);
    }
}
