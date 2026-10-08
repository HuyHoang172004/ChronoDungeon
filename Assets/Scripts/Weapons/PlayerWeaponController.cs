using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private PlayerAttack attack;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private SpriteRenderer weaponSocketRenderer;
    [SerializeField] private WeaponDefinition startingWeapon;
    [Header("Unarmed fallback")]
    [SerializeField, Min(0f)] private float unarmedDamage = 15f;
    [SerializeField, Min(0.05f)] private float unarmedCooldown = 0.38f;
    [SerializeField, Min(0f)] private float unarmedRange = 1.15f;

    private WeaponDefinition currentWeapon;
    private float basicAttackReadyAt;
    private float skill1ReadyAt;
    private float skill2ReadyAt;

    public event Action<WeaponDefinition> OnWeaponChanged;
    public event Action<SkillDefinition, int> OnSkillUsed;

    public WeaponDefinition CurrentWeapon => currentWeapon;
    public bool HasWeapon => currentWeapon != null;
    public string CurrentWeaponName => currentWeapon != null ? currentWeapon.WeaponName : string.Empty;
    public Sprite CurrentWeaponIcon => currentWeapon != null ? currentWeapon.WeaponIcon : null;
    public float CurrentBaseDamage => currentWeapon != null ? currentWeapon.BaseDamage : attack != null ? attack.attackDamage : 0f;
    public float CurrentAttackCooldown => currentWeapon != null ? currentWeapon.AttackCooldown : attack != null ? attack.attackCooldown : 0f;
    public float CurrentAttackRange => currentWeapon != null ? currentWeapon.AttackRange : attack != null ? attack.attackRange : 0f;
    public float BasicAttackCooldownRemaining => Mathf.Max(0f, basicAttackReadyAt - Time.time);
    public bool IsSkill1Ready => CurrentWeapon != null && CurrentWeapon.Skill1 != null && Skill1CooldownRemaining <= 0f;
    public bool IsSkill2Ready => CurrentWeapon != null && CurrentWeapon.Skill2 != null && Skill2CooldownRemaining <= 0f;
    public float Skill1CooldownRemaining => Mathf.Max(0f, skill1ReadyAt - Time.time);
    public float Skill2CooldownRemaining => Mathf.Max(0f, skill2ReadyAt - Time.time);

    private void Awake()
    {
        ResolveReferences();
        if (startingWeapon != null) EquipWeapon(startingWeapon);
        else SetUnarmed();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Editor-only smoke-test input; mobile/UI input remains unchanged.
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) UseSkill1();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) UseSkill2();
    }
#endif

    private void ResolveReferences()
    {
        if (attack == null) attack = GetComponent<PlayerAttack>();
        if (weaponSocket == null)
        {
            Transform visual = transform.Find("PlayerVisual");
            if (visual != null) weaponSocket = visual.Find("WeaponSocket");
        }
        if (weaponSocketRenderer == null && weaponSocket != null)
            weaponSocketRenderer = weaponSocket.GetComponent<SpriteRenderer>();
    }

    private void ApplyHoldPose(WeaponDefinition weapon)
    {
        if (weaponSocket == null || weapon == null) return;
        weaponSocket.localPosition = weapon.HoldLocalPosition;
        weaponSocket.localRotation = Quaternion.Euler(0f, 0f, weapon.HoldRotationZ);
        weaponSocket.localScale = weapon.HoldLocalScale;
    }

    public bool EquipWeapon(WeaponDefinition weapon)
    {
        if (weapon == null) return false;
        ResolveReferences();
        currentWeapon = weapon;
        ApplyCurrentWeaponStats();
        ApplyHoldPose(weapon);

        // Equipping a weapon changes only the socket sprite. Its transform remains untouched.
        if (weaponSocketRenderer != null)
        {
            if (weapon.WeaponSprite != null) weaponSocketRenderer.sprite = weapon.WeaponSprite;
            weaponSocketRenderer.enabled = weaponSocketRenderer.sprite != null;
        }

        skill1ReadyAt = 0f;
        skill2ReadyAt = 0f;
        basicAttackReadyAt = 0f;
        OnWeaponChanged?.Invoke(currentWeapon);
        return true;
    }

    public void SetUnarmed()
    {
        ResolveReferences();
        currentWeapon = null;
        if (attack != null)
        {
            attack.attackDamage = unarmedDamage;
            attack.attackCooldown = unarmedCooldown;
            attack.attackRange = unarmedRange;
        }
        if (weaponSocketRenderer != null) weaponSocketRenderer.enabled = false;
        skill1ReadyAt = 0f;
        skill2ReadyAt = 0f;
        basicAttackReadyAt = 0f;
        OnWeaponChanged?.Invoke(null);
    }

    public void BasicAttack()
    {
        ResolveReferences();
        if (currentWeapon != null && currentWeapon.BasicAttackExecution != null)
        {
            if (Time.timeScale <= 0f || BasicAttackCooldownRemaining > 0f) return;
            if (currentWeapon.BasicAttackExecution.Execute(this, currentWeapon))
                basicAttackReadyAt = Time.time + Mathf.Max(0f, currentWeapon.AttackCooldown);
            return;
        }
        if (attack != null) attack.Attack();
    }

    public bool UseSkill1()
    {
        return UseSkill(currentWeapon != null ? currentWeapon.Skill1 : null, ref skill1ReadyAt, 1);
    }

    public bool UseSkill2()
    {
        return UseSkill(currentWeapon != null ? currentWeapon.Skill2 : null, ref skill2ReadyAt, 2);
    }

    public void ApplyCurrentWeaponStats()
    {
        if (attack == null || currentWeapon == null) return;
        attack.attackDamage = currentWeapon.BaseDamage;
        attack.attackCooldown = currentWeapon.AttackCooldown;
        attack.attackRange = currentWeapon.AttackRange;
    }

    private bool UseSkill(SkillDefinition skill, ref float readyAt, int slot)
    {
        if (skill == null || Time.timeScale <= 0f || Time.time < readyAt) return false;

        bool executed = skill.Execution == null || skill.Execution.Execute(this, skill);
        if (!executed) return false;

        readyAt = Time.time + Mathf.Max(0f, skill.Cooldown);
        OnSkillUsed?.Invoke(skill, slot);
        return true;
    }
}
