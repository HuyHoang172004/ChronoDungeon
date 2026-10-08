using UnityEngine;

/// <summary>
/// Keeps the original Player body animator intact and only poses the equipped weapon.
/// The authored body sprite is never replaced or scaled by this component.
/// </summary>
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public sealed class PlayerWeaponPoseRig : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttack attack;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private SpriteRenderer weaponRenderer;
    [Header("Directional hand offsets, relative to the weapon hold pose")]
    [SerializeField] private Vector3 upOffset = new(0.02f, 0.05f, 0f);
    [SerializeField] private float upAngle = 90f;
    [SerializeField] private Vector3 downOffset = new(-0.54f, -0.05f, 0f);
    [SerializeField] private float downAngle = -90f;
    [Header("Action timing")]
    [SerializeField, Min(0.05f)] private float attackDuration = 0.24f;
    [SerializeField, Min(0.05f)] private float skill1Duration = 0.34f;
    [SerializeField, Min(0.05f)] private float skill2Duration = 0.50f;

    private Vector3 restPosition;
    private Quaternion restRotation;
    private Vector3 restScale;
    private float actionStartedAt = -1f;
    private float actionDuration;
    private int action;
    private bool activeWeapon;

    private void Awake()
    {
        if (movement == null) movement = GetComponentInParent<PlayerMovement>();
        if (attack == null) attack = GetComponentInParent<PlayerAttack>();
        if (weaponController == null) weaponController = GetComponentInParent<PlayerWeaponController>();
        if (weaponSocket == null) weaponSocket = transform.Find("WeaponSocket");
        if (weaponRenderer == null && weaponSocket != null) weaponRenderer = weaponSocket.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (attack != null) attack.AttackPerformed += StartAttack;
        if (weaponController != null)
        {
            weaponController.OnWeaponChanged += HandleWeaponChanged;
            weaponController.OnSkillUsed += StartSkill;
        }
    }

    private void Start() => HandleWeaponChanged(weaponController != null ? weaponController.CurrentWeapon : null);

    private void OnDisable()
    {
        if (attack != null) attack.AttackPerformed -= StartAttack;
        if (weaponController != null)
        {
            weaponController.OnWeaponChanged -= HandleWeaponChanged;
            weaponController.OnSkillUsed -= StartSkill;
        }
    }

    private void HandleWeaponChanged(WeaponDefinition weapon)
    {
        activeWeapon = weapon != null && weaponSocket != null && weaponRenderer != null && weaponRenderer.sprite != null;
        actionStartedAt = -1f;
        if (!activeWeapon) return;
        restPosition = weaponSocket.localPosition;
        restRotation = weaponSocket.localRotation;
        restScale = weaponSocket.localScale;
    }

    private void StartAttack(AttackSnapshot _)
    {
        if (!activeWeapon) return;
        action = 1;
        actionDuration = attackDuration;
        actionStartedAt = Time.time;
    }

    private void StartSkill(SkillDefinition _, int slot)
    {
        if (!activeWeapon) return;
        action = slot == 1 ? 2 : 3;
        actionDuration = slot == 1 ? skill1Duration : skill2Duration;
        actionStartedAt = Time.time;
    }

    private void LateUpdate()
    {
        if (!activeWeapon || weaponSocket == null || movement == null) return;
        Vector2 facing = movement.FacingDirection;
        if (facing.sqrMagnitude < 0.001f) facing = Vector2.right;

        Vector3 position = restPosition;
        float directionAngle = 0f;
        if (Mathf.Abs(facing.y) > Mathf.Abs(facing.x))
        {
            if (facing.y > 0f) { position += upOffset; directionAngle = upAngle; }
            else { position += downOffset; directionAngle = downAngle; }
        }

        float actionAngle = 0f;
        if (actionStartedAt >= 0f)
        {
            float t = Mathf.Clamp01((Time.time - actionStartedAt) / actionDuration);
            actionAngle = GetActionAngle(t);
            if (t >= 1f) actionStartedAt = -1f;
        }

        weaponSocket.localPosition = position;
        weaponSocket.localRotation = restRotation * Quaternion.Euler(0f, 0f, directionAngle + actionAngle);
        weaponSocket.localScale = restScale;
    }

    private float GetActionAngle(float t)
    {
        if (action == 1)
        {
            if (t < 0.25f) return Mathf.Lerp(0f, -30f, t / 0.25f);
            if (t < 0.68f) return Mathf.Lerp(-30f, 72f, (t - 0.25f) / 0.43f);
            return Mathf.Lerp(72f, 0f, (t - 0.68f) / 0.32f);
        }
        if (action == 2)
        {
            if (t < 0.30f) return Mathf.Lerp(0f, -18f, t / 0.30f);
            if (t < 0.72f) return Mathf.Lerp(-18f, 18f, (t - 0.30f) / 0.42f);
            return Mathf.Lerp(18f, 0f, (t - 0.72f) / 0.28f);
        }
        if (t < 0.32f) return Mathf.Lerp(0f, -48f, t / 0.32f);
        if (t < 0.65f) return Mathf.Lerp(-48f, 108f, (t - 0.32f) / 0.33f);
        return Mathf.Lerp(108f, 0f, (t - 0.65f) / 0.35f);
    }
}
