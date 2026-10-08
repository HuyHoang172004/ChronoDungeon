using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerVisualPresentation : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttack attack;
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private GameObject attackSlashVfx;
    [SerializeField] private SpriteRenderer attackSlashRenderer;
    [Header("Unarmed punch sprites")]
    [SerializeField] private Sprite punchDownSprite;
    [SerializeField] private Sprite punchRightSprite;
    [SerializeField] private Sprite punchUpSprite;
    [SerializeField] private Sprite[] punchRightFrames;
    [SerializeField] private Sprite[] punchDownFrames;
    [SerializeField] private Sprite[] punchUpFrames;
    [SerializeField, Min(0f)] private float bobAmplitude = 0.025f;
    [SerializeField, Min(0f)] private float bobFrequency = 8f;
    [Header("Idle presentation")]
    [SerializeField, Min(0f)] private float idleBobAmplitude = 0f;
    [SerializeField, Min(0f)] private float idleBobFrequency = 1.5f;
    [SerializeField, Range(0f, 0.02f)] private float idleBreathScale = 0f;
    [SerializeField, Min(0.01f)] private float attackSwingDuration = 0.20f;
    [SerializeField, Range(0f, 120f)] private float attackSwingAngle = 90f;
    [SerializeField, Min(0.01f)] private float attackSlashDuration = 0.11f;
    [SerializeField, Min(0.01f)] private float attackWindupDuration = 0.05f;
    [SerializeField, Min(0.01f)] private float attackStrikeDuration = 0.08f;
    [SerializeField, Range(0.04f, 0.06f)] private float attackWindupDistance = 0.05f;
    [SerializeField, Range(0.08f, 0.12f)] private float attackStrikeDistance = 0.10f;
    [SerializeField, Range(3f, 5f)] private float attackWindupLeanAngle = 4f;
    [SerializeField, Range(5f, 7f)] private float attackStrikeLeanAngle = 6f;
    [SerializeField, Range(25f, 35f)] private float attackWindupWeaponAngle = 30f;
    [SerializeField, Range(0f, 0.05f)] private float attackSquashAmount = 0.04f;
    [Header("Directional weapon hand anchors")]
    [Tooltip("Relative to the equipped weapon's authored hold pose. Left uses the mirrored right-hand pose.")]
    [SerializeField] private Vector3 upHandOffset = new Vector3(0.02f, 0.05f, 0f);
    [SerializeField] private float upWeaponAngleOffset = 90f;
    [SerializeField] private Vector3 downHandOffset = new Vector3(-0.54f, -0.05f, 0f);
    [SerializeField] private float downWeaponAngleOffset = -90f;

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;
    private Vector3 baseWeaponSocketPosition;
    private Quaternion baseWeaponSocketRotation;
    private Vector3 baseWeaponSocketScale;
    private Vector3 baseAttackSlashScale;
    private Vector3 activeWeaponSocketPosition;
    private Quaternion activeWeaponSocketRotation;
    private Color baseAttackSlashColor = Color.white;
    private Vector2 attackDirection = Vector2.right;
    private float attackElapsed = -1f;
    private bool externalPresentationLocked;

    public bool IsExternalPresentationLocked => externalPresentationLocked;

    private float AttackRecoveryDuration
    {
        get { return Mathf.Max(0.01f, attackSwingDuration - attackWindupDuration - attackStrikeDuration); }
    }

    private void Awake()
    {
        if (movement == null) movement = GetComponentInParent<PlayerMovement>();
        if (attack == null) attack = GetComponentInParent<PlayerAttack>();
        if (visualRenderer == null) visualRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (weaponSocket == null) weaponSocket = transform.Find("WeaponSocket");
        if (attackSlashVfx == null)
        {
            Transform slash = transform.Find("AttackSlashVFX");
            if (slash != null) attackSlashVfx = slash.gameObject;
        }
        if (attackSlashRenderer == null && attackSlashVfx != null)
            attackSlashRenderer = attackSlashVfx.GetComponent<SpriteRenderer>();

        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
        baseLocalScale = transform.localScale;
        if (weaponSocket != null)
        {
            baseWeaponSocketPosition = weaponSocket.localPosition;
            baseWeaponSocketRotation = weaponSocket.localRotation;
            baseWeaponSocketScale = weaponSocket.localScale;
        }

        if (attackSlashRenderer != null) baseAttackSlashColor = attackSlashRenderer.color;
        if (attackSlashVfx != null) baseAttackSlashScale = attackSlashVfx.transform.localScale;
        CacheDirectionalWeaponRestPose(movement != null ? movement.FacingDirection : Vector2.right);
        RestoreRestingPresentation();
        SetAttackSlashVfxActive(false);
    }

    private void OnEnable()
    {
        if (attack == null) attack = GetComponentInParent<PlayerAttack>();
        if (weaponController == null) weaponController = GetComponentInParent<PlayerWeaponController>();
        if (attack != null) attack.AttackPerformed += HandleAttackPerformed;
        if (weaponController != null) weaponController.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDisable()
    {
        if (attack != null) attack.AttackPerformed -= HandleAttackPerformed;
        if (weaponController != null) weaponController.OnWeaponChanged -= HandleWeaponChanged;
        RestoreRestingPresentation();
        SetAttackSlashVfxActive(false);
        attackElapsed = -1f;
    }

    private void Start()
    {
        RefreshWeaponRestPose();
    }

    private void HandleWeaponChanged(WeaponDefinition _)
    {
        RefreshWeaponRestPose();
        RestoreRestingPresentation();
    }

    public void RefreshWeaponRestPose()
    {
        if (weaponSocket == null) return;
        baseWeaponSocketPosition = weaponSocket.localPosition;
        baseWeaponSocketRotation = weaponSocket.localRotation;
        baseWeaponSocketScale = weaponSocket.localScale;
        ApplyDirectionalWeaponRestPose(movement != null ? movement.FacingDirection : Vector2.right);
    }

    private void LateUpdate()
    {
        UpdateAnimatorParameters();
        if (externalPresentationLocked) return;

        if (attackElapsed < 0f)
        {
            UpdateFacingAndBob();
            return;
        }

        UpdateAttackPresentation();
    }

    private void UpdateAnimatorParameters()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (movement == null) movement = GetComponentInParent<PlayerMovement>();
        if (animator == null || movement == null) return;

        Vector2 direction = movement.CurrentMoveDirection;
        if (direction.sqrMagnitude <= 0.0001f)
            direction = movement.FacingDirection;

        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
        animator.SetFloat("Speed", movement.CurrentMoveDirection.magnitude);
        animator.SetBool("IsAttacking", attackElapsed >= 0f);
    }

    private void UpdateFacingAndBob()
    {
        if (movement == null || visualRenderer == null) return;

        Vector2 facing = movement.FacingDirection;
        float speed = movement.CurrentMoveDirection.magnitude;
        bool isMoving = speed > 0.01f;
        float verticalOffset;
        float visualScale;

        if (isMoving)
        {
            verticalOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude * speed;
            visualScale = 1f;
        }
        else
        {
            // One gentle, continuous breath cycle keeps the static idle frame alive
            // without moving it away from its authored rest pose or causing jitter.
            float breath = 0.5f + 0.5f * Mathf.Sin(Time.time * idleBobFrequency);
            verticalOffset = breath * idleBobAmplitude;
            visualScale = 1f - idleBreathScale * breath;
        }

        ApplyFacingScale(visualScale, facing.x < 0f);
        transform.localPosition = baseLocalPosition + Vector3.up * verticalOffset;
        transform.localRotation = baseLocalRotation;
        ApplyDirectionalWeaponRestPose(facing);
    }

    private void HandleAttackPerformed(AttackSnapshot attackSnapshot)
    {
        if (externalPresentationLocked) return;

        attackDirection = attackSnapshot.Direction.sqrMagnitude > 0.001f
            ? attackSnapshot.Direction.normalized
            : (movement != null ? movement.FacingDirection.normalized : Vector2.right);
        if (attackDirection.sqrMagnitude < 0.001f) attackDirection = Vector2.right;

        CacheDirectionalWeaponRestPose(attackDirection);
        RestoreRestingPresentation();
        SetAttackSlashVfxActive(false);
        attackElapsed = 0f;
    }

    private void UpdateAttackPresentation()
    {
        attackElapsed += Time.deltaTime;
        float windupEnd = attackWindupDuration;
        float strikeEnd = windupEnd + attackStrikeDuration;
        float recoveryEnd = strikeEnd + AttackRecoveryDuration;
        float bodyScale = GetParentScaleMagnitude();
        Vector3 localFacing = new Vector3(attackDirection.x, attackDirection.y, 0f) / bodyScale;
        float leanSign = attackDirection.x < -0.01f ? -1f : 1f;
        float bodyDistance;
        float bodyLean;
        float weaponAngle;
        float squash;

        if (attackElapsed < windupEnd)
        {
            float t = EaseInOut(attackElapsed / windupEnd);
            bodyDistance = Mathf.Lerp(0f, -attackWindupDistance, t);
            bodyLean = Mathf.Lerp(0f, -attackWindupLeanAngle * leanSign, t);
            weaponAngle = Mathf.Lerp(0f, -attackWindupWeaponAngle, t);
            squash = Mathf.Lerp(1f, 1f - attackSquashAmount * 0.35f, t);
        }
        else if (attackElapsed < strikeEnd)
        {
            float t = EaseOut((attackElapsed - windupEnd) / attackStrikeDuration);
            bodyDistance = Mathf.Lerp(-attackWindupDistance, attackStrikeDistance, t);
            bodyLean = Mathf.Lerp(-attackWindupLeanAngle * leanSign, attackStrikeLeanAngle * leanSign, t);
            weaponAngle = Mathf.Lerp(-attackWindupWeaponAngle, attackSwingAngle - attackWindupWeaponAngle, t);
            squash = Mathf.Lerp(1f - attackSquashAmount * 0.35f, 1f + attackSquashAmount, t);
        }
        else
        {
            float t = EaseOut(Mathf.Clamp01((attackElapsed - strikeEnd) / AttackRecoveryDuration));
            bodyDistance = Mathf.Lerp(attackStrikeDistance, 0f, t);
            bodyLean = Mathf.Lerp(attackStrikeLeanAngle * leanSign, 0f, t);
            weaponAngle = Mathf.Lerp(attackSwingAngle - attackWindupWeaponAngle, 0f, t);
            squash = Mathf.Lerp(1f + attackSquashAmount, 1f, t);
        }

        bool unarmed = !HasEquippedWeapon();
        if (unarmed)
        {
            // The authored punch frames contain their own anticipation and follow-through.
            // Do not add procedural lunge/squash, which would shift or resize the player.
            bodyDistance = 0f;
            bodyLean = 0f;
            squash = 1f;
            ApplyUnarmedPunchSprite();
        }

        ApplyFacingScale(squash, attackDirection.x < -0.01f);
        transform.localPosition = baseLocalPosition + localFacing * bodyDistance;
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, bodyLean);

        if (weaponSocket != null)
        {
            weaponSocket.localPosition = activeWeaponSocketPosition;
            weaponSocket.localRotation = activeWeaponSocketRotation * Quaternion.Euler(0f, 0f, weaponAngle);
            weaponSocket.localScale = baseWeaponSocketScale;
        }

        SpriteRenderer equippedWeaponRenderer = weaponSocket != null ? weaponSocket.GetComponent<SpriteRenderer>() : null;
        bool inSlashWindow = equippedWeaponRenderer != null && equippedWeaponRenderer.enabled &&
            attackElapsed >= windupEnd && attackElapsed < windupEnd + attackSlashDuration;
        if (inSlashWindow)
        {
            UpdateAttackSlashTransform();
            SetAttackSlashVfxActive(true);
            float slashT = Mathf.Clamp01((attackElapsed - windupEnd) / attackSlashDuration);
            SetAttackSlashAlpha(Mathf.Lerp(0.82f, 0f, slashT));
        }
        else
        {
            SetAttackSlashVfxActive(false);
        }

        if (attackElapsed >= recoveryEnd)
        {
            RestoreRestingPresentation();
            SetAttackSlashVfxActive(false);
            attackElapsed = -1f;
        }
    }

    private void RestoreRestingPresentation()
    {
        transform.localPosition = baseLocalPosition;
        transform.localRotation = baseLocalRotation;
        ApplyFacingScale(1f, movement != null && movement.FacingDirection.x < 0f);

        if (weaponSocket != null)
        {
            weaponSocket.localPosition = activeWeaponSocketPosition;
            weaponSocket.localRotation = activeWeaponSocketRotation;
            weaponSocket.localScale = baseWeaponSocketScale;
        }

        SetAttackSlashAlpha(baseAttackSlashColor.a);
    }

    private void ApplyFacingScale(float squash, bool facingLeft)
    {
        Vector3 scale = baseLocalScale;
        scale.x = Mathf.Abs(baseLocalScale.x) * squash * (facingLeft ? -1f : 1f);
        scale.y = baseLocalScale.y * (1f + (1f - squash) * 0.5f);
        transform.localScale = scale;
    }

    private void SetAttackSlashVfxActive(bool active)
    {
        if (attackSlashVfx != null && attackSlashVfx.activeSelf != active)
            attackSlashVfx.SetActive(active);

        if (!active) SetAttackSlashAlpha(baseAttackSlashColor.a);
    }

    private void SetAttackSlashAlpha(float alpha)
    {
        if (attackSlashRenderer == null) return;
        Color color = baseAttackSlashColor;
        color.a = alpha;
        attackSlashRenderer.color = color;
    }

    private void UpdateAttackSlashTransform()
    {
        if (attackSlashVfx == null || weaponSocket == null) return;

        Transform slashTransform = attackSlashVfx.transform;
        // The sword is pivoted at its hilt. Offset the slash ahead of that hilt in
        // socket space so the art follows the actual swing rather than a fixed point.
        Vector3 swingOffset = weaponSocket.localRotation * Vector3.right * 0.36f;
        slashTransform.localPosition = weaponSocket.localPosition + swingOffset;
        slashTransform.localRotation = weaponSocket.localRotation * Quaternion.Euler(0f, 0f, 18f);
        slashTransform.localScale = baseAttackSlashScale;
    }

    private bool HasEquippedWeapon()
    {
        SpriteRenderer renderer = weaponSocket != null ? weaponSocket.GetComponent<SpriteRenderer>() : null;
        return renderer != null && renderer.enabled && renderer.sprite != null;
    }

    private void ApplyUnarmedPunchSprite()
    {
        if (visualRenderer == null) return;
        Sprite punch = punchRightSprite;
        Sprite[] frames = punchRightFrames;
        if (Mathf.Abs(attackDirection.y) > Mathf.Abs(attackDirection.x))
        {
            bool up = attackDirection.y > 0f;
            punch = up ? punchUpSprite : punchDownSprite;
            frames = up ? punchUpFrames : punchDownFrames;
        }
        if (frames != null && frames.Length == 4)
        {
            float normalized = Mathf.Clamp01(attackElapsed / attackSwingDuration);
            punch = frames[Mathf.Min(3, Mathf.FloorToInt(normalized * 4f))];
        }
        if (punch != null) visualRenderer.sprite = punch;
    }

    private void ApplyDirectionalWeaponRestPose(Vector2 facing)
    {
        CacheDirectionalWeaponRestPose(facing);
        if (weaponSocket == null) return;
        weaponSocket.localPosition = activeWeaponSocketPosition;
        weaponSocket.localRotation = activeWeaponSocketRotation;
        weaponSocket.localScale = baseWeaponSocketScale;
    }

    private void CacheDirectionalWeaponRestPose(Vector2 facing)
    {
        activeWeaponSocketPosition = baseWeaponSocketPosition;
        activeWeaponSocketRotation = baseWeaponSocketRotation;

        if (facing.sqrMagnitude < 0.0001f) return;
        if (Mathf.Abs(facing.y) <= Mathf.Abs(facing.x)) return;

        if (facing.y > 0f)
        {
            // Back view: the hero's anatomical right hand is screen-right.
            activeWeaponSocketPosition += upHandOffset;
            activeWeaponSocketRotation *= Quaternion.Euler(0f, 0f, upWeaponAngleOffset);
        }
        else
        {
            // Front view: the hero's anatomical right hand is screen-left.
            activeWeaponSocketPosition += downHandOffset;
            activeWeaponSocketRotation *= Quaternion.Euler(0f, 0f, downWeaponAngleOffset);
        }
    }

    public void SetExternalPresentationLock(bool locked)
    {
        externalPresentationLocked = locked;
    }

    private float GetParentScaleMagnitude()
    {
        if (transform.parent == null) return 1f;
        return Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.x));
    }

    private static float EaseInOut(float value)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));
    }

    private static float EaseOut(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - (1f - value) * (1f - value);
    }
}
