using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(700)]
[DisallowMultipleComponent]
public sealed class PlayerCombatHUD : MonoBehaviour
{
    [Header("Gameplay references")]
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerAttack attack;
    [SerializeField] private PlayerDash dash;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerResources playerResources;
    [SerializeField] private PlayerConsumables playerConsumables;

    [Header("Scene UI references")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button skill1Button;
    [SerializeField] private Button skill2Button;
    [SerializeField] private Button temporalBeanButton;
    [SerializeField] private TMP_Text temporalBeanCountText;
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image energyFill;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Image skill1Icon;
    [SerializeField] private Image skill2Icon;
    [SerializeField] private Image skill1CooldownOverlay;
    [SerializeField] private Image skill2CooldownOverlay;
    [SerializeField] private TMP_Text skill1CooldownText;
    [SerializeField] private TMP_Text skill2CooldownText;
    [SerializeField] private TMP_Text skill1Label;
    [SerializeField] private TMP_Text skill2Label;

    [Header("Player HUD references")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject temporalHUD;
    [SerializeField] private TMP_Text temporalTimeText;
    [SerializeField] private TMP_Text temporalLoopText;
    [SerializeField] private TMP_Text temporalGhostText;
    [SerializeField] private GameObject keyHUD;
    [SerializeField] private Image keyIcon;
    [SerializeField] private TMP_Text keyText;

    [Header("Context data references")]
    [SerializeField] private TimeLoopManager timeLoop;
    [SerializeField] private TemporalGhostManager ghostManager;
    [SerializeField] private RuneKeyInventory runeKeyInventory;
    [SerializeField] private ChronoGuardianHUD bossHUD;

    private bool wired;
    // These are authored scene icons. Weapon data may supply an icon later, but an
    // empty ScriptableObject reference must not erase the visual UI the designer set.
    private Sprite skill1FallbackIcon;
    private Sprite skill2FallbackIcon;

    private void Awake()
    {
        skill1FallbackIcon = skill1Icon != null ? skill1Icon.sprite : null;
        skill2FallbackIcon = skill2Icon != null ? skill2Icon.sprite : null;
    }

    private void OnEnable()
    {
        if (weaponController != null) weaponController.OnWeaponChanged += HandleWeaponChanged;
        if (playerHealth != null) playerHealth.Changed += HandleHealthChanged;
        if (playerResources != null)
        {
            playerResources.EnergyChanged += HandleEnergyChanged;
            playerResources.GoldChanged += HandleGoldChanged;
        }
        if (playerConsumables != null) playerConsumables.BeansChanged += HandleBeansChanged;
    }

    private void Start()
    {
        ResolveDataReferences();
        WireButtons();
        RefreshWeaponPresentation();
        RefreshHealth();
        RefreshResources();
        RefreshBeans();
        RefreshSkillCooldown(1);
        RefreshSkillCooldown(2);
        RefreshBeans();
        RefreshContextHUD();
    }

    private void Update()
    {
        RefreshSkillCooldown(1);
        RefreshSkillCooldown(2);
        RefreshContextHUD();
    }

    private void OnDisable()
    {
        if (weaponController != null) weaponController.OnWeaponChanged -= HandleWeaponChanged;
        if (playerHealth != null) playerHealth.Changed -= HandleHealthChanged;
        if (runeKeyInventory != null) runeKeyInventory.KeyChanged -= HandleKeyChanged;
        if (playerResources != null)
        {
            playerResources.EnergyChanged -= HandleEnergyChanged;
            playerResources.GoldChanged -= HandleGoldChanged;
        }
        if (playerConsumables != null) playerConsumables.BeansChanged -= HandleBeansChanged;
        wired = false;
    }

    private void ResolveDataReferences()
    {
        if (weaponController == null) weaponController = FindAnyObjectByType<PlayerWeaponController>();
        if (playerHealth == null && weaponController != null) playerHealth = weaponController.GetComponent<Health>();
        if (playerResources == null && weaponController != null)
            playerResources = weaponController.GetComponent<PlayerResources>();
        if (playerConsumables == null && weaponController != null)
            playerConsumables = weaponController.GetComponent<PlayerConsumables>();
        if (timeLoop == null) timeLoop = FindAnyObjectByType<TimeLoopManager>();
        if (ghostManager == null) ghostManager = FindAnyObjectByType<TemporalGhostManager>();
        if (runeKeyInventory == null && weaponController != null)
            runeKeyInventory = weaponController.GetComponent<RuneKeyInventory>();
        if (bossHUD == null) bossHUD = FindAnyObjectByType<ChronoGuardianHUD>();

        if (weaponController != null) weaponController.OnWeaponChanged -= HandleWeaponChanged;
        if (weaponController != null) weaponController.OnWeaponChanged += HandleWeaponChanged;
        if (playerHealth != null) playerHealth.Changed -= HandleHealthChanged;
        if (playerHealth != null) playerHealth.Changed += HandleHealthChanged;
        if (playerResources != null)
        {
            playerResources.EnergyChanged -= HandleEnergyChanged;
            playerResources.EnergyChanged += HandleEnergyChanged;
            playerResources.GoldChanged -= HandleGoldChanged;
            playerResources.GoldChanged += HandleGoldChanged;
        }
        if (playerConsumables != null)
        {
            playerConsumables.BeansChanged -= HandleBeansChanged;
            playerConsumables.BeansChanged += HandleBeansChanged;
        }
        if (runeKeyInventory != null) runeKeyInventory.KeyChanged -= HandleKeyChanged;
        if (runeKeyInventory != null) runeKeyInventory.KeyChanged += HandleKeyChanged;
    }

    private void WireButtons()
    {
        if (wired) return;

        if (attackButton != null && attackButton.onClick.GetPersistentEventCount() == 0)
        {
            attackButton.onClick.RemoveListener(OnAttackPressed);
            attackButton.onClick.AddListener(OnAttackPressed);
        }

        if (skill1Button != null)
        {
            skill1Button.onClick.RemoveListener(OnSkill1Pressed);
            skill1Button.onClick.AddListener(OnSkill1Pressed);
        }

        if (skill2Button != null)
        {
            skill2Button.onClick.RemoveListener(OnSkill2Pressed);
            skill2Button.onClick.AddListener(OnSkill2Pressed);
        }

        if (temporalBeanButton != null)
        {
            temporalBeanButton.onClick.RemoveListener(OnTemporalBeanPressed);
            temporalBeanButton.onClick.AddListener(OnTemporalBeanPressed);
        }

        wired = true;
    }

    private void OnAttackPressed()
    {
        if (weaponController != null) weaponController.BasicAttack();
        else if (attack != null) attack.Attack();
    }

    private void OnSkill1Pressed()
    {
        if (weaponController != null) weaponController.UseSkill1();
    }

    private void OnSkill2Pressed()
    {
        if (weaponController != null) weaponController.UseSkill2();
    }

    private void OnTemporalBeanPressed()
    {
        if (playerConsumables != null) playerConsumables.UseTemporalBean();
    }

    private void RefreshWeaponPresentation()
    {
        bool hasWeapon = weaponController != null && weaponController.CurrentWeapon != null;
        if (skill1Button != null) skill1Button.gameObject.SetActive(hasWeapon);
        if (skill2Button != null) skill2Button.gameObject.SetActive(hasWeapon);
        if (weaponIcon != null) weaponIcon.enabled = hasWeapon && weaponController.CurrentWeapon.WeaponIcon != null;
        if (weaponName != null) weaponName.gameObject.SetActive(hasWeapon);
        if (!hasWeapon) return;

        WeaponDefinition weapon = weaponController.CurrentWeapon;
        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon.WeaponIcon;
            weaponIcon.enabled = weapon.WeaponIcon != null;
        }
        if (weaponName != null)
            weaponName.text = string.IsNullOrEmpty(weapon.WeaponName) ? "WEAPON" : weapon.WeaponName.ToUpperInvariant();
        ApplySkillData(weapon.Skill1, skill1Icon, skill1FallbackIcon, skill1Label, "SKILL 1");
        ApplySkillData(weapon.Skill2, skill2Icon, skill2FallbackIcon, skill2Label, "SKILL 2");
    }

    private static void ApplySkillData(SkillDefinition skill, Image icon, Sprite authoredFallbackIcon, TMP_Text label, string fallbackLabel)
    {
        if (icon != null)
        {
            Sprite resolvedIcon = skill != null && skill.Icon != null ? skill.Icon : authoredFallbackIcon;
            icon.sprite = resolvedIcon;
            icon.enabled = resolvedIcon != null;
        }

        if (label != null)
            label.text = skill == null || string.IsNullOrEmpty(skill.SkillName)
                ? fallbackLabel
                : skill.SkillName.ToUpperInvariant();
    }

    private void RefreshSkillCooldown(int slot)
    {
        if (weaponController == null) return;

        SkillDefinition skill = weaponController.CurrentWeapon == null
            ? null
            : slot == 1 ? weaponController.CurrentWeapon.Skill1 : weaponController.CurrentWeapon.Skill2;
        float remaining = slot == 1
            ? weaponController.Skill1CooldownRemaining
            : weaponController.Skill2CooldownRemaining;
        float duration = skill == null ? 0f : skill.Cooldown;
        float normalized = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;

        Image overlay = slot == 1 ? skill1CooldownOverlay : skill2CooldownOverlay;
        TMP_Text text = slot == 1 ? skill1CooldownText : skill2CooldownText;
        Button button = slot == 1 ? skill1Button : skill2Button;

        if (overlay != null)
        {
            overlay.fillAmount = normalized;
            overlay.enabled = remaining > 0.01f;
        }

        if (text != null)
        {
            text.text = remaining > 0.01f ? Mathf.CeilToInt(remaining).ToString() : string.Empty;
            text.enabled = remaining > 0.01f;
        }

        if (button != null)
            button.interactable = remaining <= 0.01f && skill != null;
    }

    private void RefreshHealth()
    {
        if (playerHealth == null) return;

        float normalized = playerHealth.maxHealth <= 0f
            ? 0f
            : Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);

        if (hpFill != null) hpFill.fillAmount = normalized;
        if (hpText != null) hpText.text = string.Format("HP  {0:0} / {1:0}", playerHealth.currentHealth, playerHealth.maxHealth);
    }

    private void RefreshResources()
    {
        if (playerResources == null) return;

        float normalized = playerResources.MaximumEnergy <= 0f
            ? 0f
            : Mathf.Clamp01(playerResources.Energy / playerResources.MaximumEnergy);
        if (energyFill != null) energyFill.fillAmount = normalized;
        if (energyText != null)
            energyText.text = string.Format("ENERGY  {0:0} / {1:0}", playerResources.Energy, playerResources.MaximumEnergy);
        if (goldText != null) goldText.text = string.Format("GOLD  {0}", playerResources.Gold);
    }

    private void RefreshBeans()
    {
        if (temporalBeanCountText != null)
            temporalBeanCountText.text = playerConsumables == null ? "x0" : "x" + playerConsumables.TemporalBeans;
        if (temporalBeanButton != null)
            temporalBeanButton.interactable = playerConsumables != null && playerConsumables.CanUseTemporalBean;
    }

    private void RefreshContextHUD()
    {
        bool bossVisible = bossHUD != null && bossHUD.IsVisible;
        bool temporalVisible = !bossVisible && timeLoop != null && timeLoop.enabled && timeLoop.IsRunning;

        if (temporalHUD != null) temporalHUD.SetActive(temporalVisible);
        if (objectivePanel != null) objectivePanel.SetActive(!bossVisible);
        if (keyHUD != null) keyHUD.SetActive(!bossVisible && runeKeyInventory != null && runeKeyInventory.HasRuneKey);

        if (!temporalVisible) return;
        if (temporalTimeText != null) temporalTimeText.text = string.Format("TIME  {0:0.0}", timeLoop.remainingTime);
        if (temporalLoopText != null) temporalLoopText.text = string.Format("LOOP  {0}", timeLoop.loopIndex);
        if (temporalGhostText != null)
        {
            int count = ghostManager == null ? 0 : ghostManager.ActiveGhostCount;
            int max = ghostManager == null ? 3 : ghostManager.MaxGhosts;
            temporalGhostText.text = string.Format("GHOST  {0}/{1}", count, max);
        }
    }

    private void HandleKeyChanged(bool _) => RefreshContextHUD();

    private void HandleWeaponChanged(WeaponDefinition _) { RefreshWeaponPresentation(); }
    private void HandleHealthChanged(Health _) { RefreshHealth(); }
    private void HandleEnergyChanged(PlayerResources _) { RefreshResources(); }
    private void HandleGoldChanged(PlayerResources _) { RefreshResources(); }
    private void HandleBeansChanged(PlayerConsumables _) { RefreshBeans(); }
}
