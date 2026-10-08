using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerStatsPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private PauseFlowUI pauseFlowUI;
    [SerializeField] private CanvasGroup combatControlsGroup;

    [Header("Data")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerDash dash;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private TimeLoopManager timeLoop;
    [SerializeField] private TemporalGhostManager ghostManager;

    [Header("UI fields")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text moveSpeedText;
    [SerializeField] private TMP_Text dashCooldownText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text weaponDamageText;
    [SerializeField] private TMP_Text weaponAttackCooldownText;
    [SerializeField] private TMP_Text weaponRangeText;
    [SerializeField] private TMP_Text skill1Text;
    [SerializeField] private TMP_Text skill2Text;
    [SerializeField] private TMP_Text ghostLimitText;
    [SerializeField] private TMP_Text loopDurationText;

    private PauseManager pauseManager;
    private bool pausedByStats;
    private bool openedFromPause;

    private void Awake()
    {
        ResolveReferences();
        if (openButton != null) openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (panel != null && panel.activeSelf) Refresh();
    }

    private void OnDestroy()
    {
        if (openButton != null) openButton.onClick.RemoveListener(Open);
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        ResolveReferences();
        if (panel == null) return;

        openedFromPause = pauseManager != null && pauseManager.IsPaused;
        if (pauseFlowUI != null) pauseFlowUI.SetStatsOverlay(true);

        if (pauseManager != null && !pauseManager.IsPaused)
        {
            pauseManager.Pause();
            pausedByStats = true;
        }

        panel.SetActive(true);
        SetCombatControlsEnabled(false);
        Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        SetCombatControlsEnabled(true);

        if (pausedByStats && pauseManager != null)
        {
            pausedByStats = false;
            pauseManager.Resume();
        }

        if (pauseFlowUI != null)
        {
            pauseFlowUI.SetStatsOverlay(false);
            if (!openedFromPause) openedFromPause = false;
        }
    }

    private void ResolveReferences()
    {
        if (pauseManager == null) pauseManager = FindAnyObjectByType<PauseManager>();
        if (playerHealth == null) playerHealth = FindAnyObjectByType<Health>();
        if (movement == null) movement = FindAnyObjectByType<PlayerMovement>();
        if (dash == null) dash = FindAnyObjectByType<PlayerDash>();
        if (weaponController == null) weaponController = FindAnyObjectByType<PlayerWeaponController>();
        if (timeLoop == null) timeLoop = FindAnyObjectByType<TimeLoopManager>();
        if (ghostManager == null) ghostManager = FindAnyObjectByType<TemporalGhostManager>();
        if (pauseFlowUI == null) pauseFlowUI = FindAnyObjectByType<PauseFlowUI>();
        if (combatControlsGroup == null)
        {
            GameObject controls = GameObject.Find("Canvas/Chrono Safe Area/MobileControls");
            if (controls != null) combatControlsGroup = controls.GetComponent<CanvasGroup>();
        }
    }

    private void SetCombatControlsEnabled(bool enabled)
    {
        if (combatControlsGroup == null) return;
        combatControlsGroup.interactable = enabled;
        combatControlsGroup.blocksRaycasts = enabled;
        combatControlsGroup.alpha = enabled ? 1f : 0.55f;
    }

    private void Refresh()
    {
        WeaponDefinition weapon = weaponController != null ? weaponController.CurrentWeapon : null;
        SkillDefinition skill1 = weapon != null ? weapon.Skill1 : null;
        SkillDefinition skill2 = weapon != null ? weapon.Skill2 : null;

        Set(hpText, playerHealth == null ? "HP  - / -" : string.Format("HP  {0:0} / {1:0}", playerHealth.currentHealth, playerHealth.maxHealth));
        Set(moveSpeedText, movement == null ? "Move Speed  -" : string.Format("Move Speed  {0:0.##}", movement.moveSpeed));
        Set(dashCooldownText, dash == null ? "Dash Cooldown  -" : string.Format("Dash Cooldown  {0:0.##}s", dash.DashCooldown));

        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon != null ? weapon.WeaponIcon : null;
            weaponIcon.enabled = weapon != null && weapon.WeaponIcon != null;
        }

        Set(weaponNameText, weapon == null ? "Weapon  -" : weapon.WeaponName);
        Set(weaponDamageText, weapon == null ? "Damage  -" : string.Format("Damage  {0:0.##}", weapon.BaseDamage));
        Set(weaponAttackCooldownText, weapon == null ? "Attack Cooldown  -" : string.Format("Attack Cooldown  {0:0.##}s", weapon.AttackCooldown));
        Set(weaponRangeText, weapon == null ? "Range  -" : string.Format("Range  {0:0.##}", weapon.AttackRange));

        SetSkill(skill1Text, "Skill 1", skill1);
        SetSkill(skill2Text, "Skill 2", skill2);

        Set(ghostLimitText, ghostManager == null ? "Ghost Limit  -" : string.Format("Ghost Limit  {0}", ghostManager.MaxGhosts));
        Set(loopDurationText, timeLoop == null ? "Loop Duration  -" : string.Format("Loop Duration  {0:0.##}s", timeLoop.LoopDuration));
    }

    private static void SetSkill(TMP_Text target, string fallback, SkillDefinition skill)
    {
        if (target == null) return;
        target.text = skill == null
            ? fallback + "  -"
            : string.Format("{0}\nDamage  {1:0.##}    Cooldown  {2:0.##}s", skill.SkillName, skill.Damage, skill.Cooldown);
    }

    private static void Set(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }
}
