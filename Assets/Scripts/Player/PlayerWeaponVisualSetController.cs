using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerWeaponVisualSetController : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttack attack;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerVisualPresentation legacyPresentation;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer weaponSocketRenderer;
    [SerializeField] private Animator bodyAnimator;
    [Tooltip("Full-body visual set used before the player equips a weapon. Leave Weapon empty on this asset.")]
    [SerializeField] private PlayerWeaponVisualSet unarmedVisualSet;
    [SerializeField] private List<PlayerWeaponVisualSet> visualSets = new();
    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float attackPoseDuration = 0.24f;
    [SerializeField, Min(0.01f)] private float skill1PoseDuration = 0.34f;
    [SerializeField, Min(0.01f)] private float skill2PoseDuration = 0.50f;
    [SerializeField, Min(0f)] private float walkBobAmplitude = 0.018f;
    [SerializeField, Min(0f)] private float walkBobFrequency = 9f;
    [SerializeField, Min(0f)] private float idleBobAmplitude = 0.006f;
    [SerializeField, Min(0f)] private float idleBobFrequency = 1.5f;
    [SerializeField, Min(0.1f)] private float idleFramesPerSecond = 1.1f;
    [SerializeField, Min(0.1f)] private float walkFramesPerSecond = 5.5f;
    [Tooltip("Small presentation-only lift for baked full-body weapon art. Player physics and colliders are unchanged.")]
    [SerializeField] private float equippedVisualYOffset = 0f;
    [Tooltip("Approved Base and weapon sheets share one pixel-per-unit contract, so no scale correction is required.")]
    [SerializeField, Min(0.1f)] private float equippedVisualScaleMultiplier = 1f;

    private PlayerWeaponVisualSet activeSet;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;
    private float poseEndsAt;
    private float poseStartedAt;
    private int activePose;

    private void Awake()
    {
        ResolveReferences();
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
        baseLocalScale = transform.localScale;
    }

    private void OnEnable()
    {
        ResolveReferences();
        if (attack != null) attack.AttackPerformed += HandleAttack;
        if (weaponController != null)
        {
            weaponController.OnWeaponChanged += HandleWeaponChanged;
            weaponController.OnSkillUsed += HandleSkillUsed;
        }
    }

    private void Start()
    {
        ApplyWeaponSet(weaponController != null ? weaponController.CurrentWeapon : null);
    }

    private void OnDisable()
    {
        if (attack != null) attack.AttackPerformed -= HandleAttack;
        if (weaponController != null)
        {
            weaponController.OnWeaponChanged -= HandleWeaponChanged;
            weaponController.OnSkillUsed -= HandleSkillUsed;
        }
    }

    private void ResolveReferences()
    {
        if (movement == null) movement = GetComponentInParent<PlayerMovement>();
        if (attack == null) attack = GetComponentInParent<PlayerAttack>();
        if (weaponController == null) weaponController = GetComponentInParent<PlayerWeaponController>();
        if (legacyPresentation == null) legacyPresentation = GetComponent<PlayerVisualPresentation>();
        if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
        if (bodyAnimator == null) bodyAnimator = GetComponent<Animator>();
        if (weaponSocketRenderer == null)
        {
            Transform socket = transform.Find("WeaponSocket");
            if (socket != null) weaponSocketRenderer = socket.GetComponent<SpriteRenderer>();
        }
    }

    private void HandleWeaponChanged(WeaponDefinition weapon) => ApplyWeaponSet(weapon);

    private void ApplyWeaponSet(WeaponDefinition weapon)
    {
        activeSet = weapon == null ? unarmedVisualSet : null;
        for (int i = 0; weapon != null && i < visualSets.Count; i++)
        {
            if (visualSets[i] != null && visualSets[i].Weapon == weapon)
            {
                activeSet = visualSets[i];
                break;
            }
        }

        bool useAuthoredFullBodyArt = activeSet != null;
        if (legacyPresentation != null) legacyPresentation.SetExternalPresentationLock(useAuthoredFullBodyArt);
        // The legacy Animator owns the SpriteRenderer through its old animation clips.
        // Disable it only while this controller owns the full-body equipped art, otherwise it
        // overwrites the baked weapon frame after LateUpdate.
        if (bodyAnimator != null) bodyAnimator.enabled = !useAuthoredFullBodyArt;
        // The weapon controller owns the normal socket visibility. This visual set only
        // suppresses it while a full-body weapon sprite is active; in particular, do not
        // re-enable the old sword when the player switches back to unarmed.
        if (weaponSocketRenderer != null && useAuthoredFullBodyArt) weaponSocketRenderer.enabled = false;
        activePose = 0;
        poseEndsAt = 0f;
        poseStartedAt = 0f;
        if (useAuthoredFullBodyArt && bodyRenderer != null)
        {
            Vector2 facing = movement != null ? movement.FacingDirection : Vector2.right;
            if (facing.sqrMagnitude < 0.0001f) facing = Vector2.right;
            bodyRenderer.sprite = activeSet.GetIdle(facing);
            bodyRenderer.flipX = false;
        }
        // Base and equipped source sheets use the same body scale and pivot contract.
        // Only the equipped sheet needs its authored transparent-padding correction.
        bool equippedWeaponVisual = useAuthoredFullBodyArt && weapon != null;
        transform.localPosition = baseLocalPosition + (equippedWeaponVisual ? Vector3.up * equippedVisualYOffset : Vector3.zero);
        transform.localRotation = baseLocalRotation;
        float setScale = equippedWeaponVisual ? equippedVisualScaleMultiplier : 1f;
        transform.localScale = new Vector3(
            baseLocalScale.x * setScale,
            baseLocalScale.y * setScale,
            baseLocalScale.z);
    }

    private void HandleAttack(AttackSnapshot _)
    {
        if (activeSet == null) return;
        activePose = 1;
        poseStartedAt = Time.time;
        poseEndsAt = Time.time + attackPoseDuration;
        if (bodyRenderer != null && movement != null)
            bodyRenderer.sprite = activeSet.GetAttack(movement.FacingDirection);
    }

    private void HandleSkillUsed(SkillDefinition _, int slot)
    {
        if (activeSet == null) return;
        activePose = slot == 1 ? 2 : 3;
        poseStartedAt = Time.time;
        poseEndsAt = Time.time + (slot == 1 ? skill1PoseDuration : skill2PoseDuration);
        if (bodyRenderer != null && movement != null)
            bodyRenderer.sprite = slot == 1
                ? activeSet.GetSkill1(movement.FacingDirection)
                : activeSet.GetSkill2(movement.FacingDirection);
    }

    private void LateUpdate()
    {
        if (activeSet == null || bodyRenderer == null || movement == null) return;

        if (weaponSocketRenderer != null) weaponSocketRenderer.enabled = false;
        Vector2 facing = movement.FacingDirection;
        if (facing.sqrMagnitude < 0.0001f) facing = Vector2.right;
        bool moving = movement.CurrentMoveDirection.sqrMagnitude > 0.0001f;
        Sprite sprite = null;
        Sprite[] frames = null;

        if (Time.time < poseEndsAt)
        {
            if (activePose == 1)
            {
                frames = activeSet.GetAttackFrames(facing);
                sprite = GetFrame(frames, activeSet.GetAttack(facing), (Time.time - poseStartedAt) / attackPoseDuration);
            }
            else if (activePose == 2) sprite = GetFrame(activeSet.GetSkill1Frames(facing), activeSet.GetSkill1(facing), (Time.time - poseStartedAt) / skill1PoseDuration);
            else if (activePose == 3) sprite = GetFrame(activeSet.GetSkill2Frames(facing), activeSet.GetSkill2(facing), (Time.time - poseStartedAt) / skill2PoseDuration);
        }
        else
        {
            activePose = 0;
            frames = moving ? activeSet.GetWalkFrames(facing) : activeSet.GetIdleFrames(facing);
            Sprite fallback = moving ? activeSet.GetWalk(facing) : activeSet.GetIdle(facing);
            float animationRate = moving ? walkFramesPerSecond : idleFramesPerSecond;
            sprite = GetLoopFrame(frames, fallback, Time.time, animationRate);
        }

        // A missing authored pose gracefully falls back to the current idle frame;
        // it never reveals the separate socket sword again.
        if (sprite == null) sprite = activeSet.GetIdle(facing);
        if (sprite != null) bodyRenderer.sprite = sprite;
        bodyRenderer.flipX = false;

        // The equipped sprite set contains a still pose for each direction. A very small
        // vertical breathing motion keeps its idle readable without changing its size or pivot.
        float bob = moving
            ? Mathf.Sin(Time.time * walkBobFrequency) * walkBobAmplitude
            : Mathf.Sin(Time.time * idleBobFrequency) * idleBobAmplitude;
        bool equippedWeaponVisual = weaponController != null && weaponController.CurrentWeapon != null;
        transform.localPosition = baseLocalPosition + Vector3.up * ((equippedWeaponVisual ? equippedVisualYOffset : 0f) + bob);
        transform.localRotation = baseLocalRotation;
        float setScale = equippedWeaponVisual ? equippedVisualScaleMultiplier : 1f;
        transform.localScale = new Vector3(
            baseLocalScale.x * setScale,
            baseLocalScale.y * setScale,
            baseLocalScale.z);
    }

    private static Sprite GetFrame(Sprite[] frames, Sprite fallback, float progression)
    {
        if (frames == null || frames.Length == 0) return fallback;
        int index = Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(progression, 1f) * frames.Length), 0, frames.Length - 1);
        return frames[index] != null ? frames[index] : fallback;
    }

    private static Sprite GetLoopFrame(Sprite[] frames, Sprite fallback, float time, float framesPerSecond)
    {
        if (frames == null || frames.Length == 0) return fallback;
        int index = Mathf.FloorToInt(time * framesPerSecond) % frames.Length;
        return frames[index] != null ? frames[index] : fallback;
    }
}
