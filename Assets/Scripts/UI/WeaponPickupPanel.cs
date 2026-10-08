using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class WeaponPickupPanel : MonoBehaviour
{
    [Header("Authored UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button promptButton;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text rangeText;
    [SerializeField] private TMP_Text skill1Text;
    [SerializeField] private TMP_Text skill2Text;
    [SerializeField] private CanvasGroup combatControlsGroup;

    private WeaponPickup currentPickup;
    private PlayerWeaponController currentController;

    private void Awake()
    {
        if (promptButton != null) promptButton.onClick.AddListener(OpenDetails);
        if (equipButton != null) equipButton.onClick.AddListener(ConfirmEquip);
        if (cancelButton != null) cancelButton.onClick.AddListener(Cancel);
        HideAll();
    }

    private void OnDestroy()
    {
        if (promptButton != null) promptButton.onClick.RemoveListener(OpenDetails);
        if (equipButton != null) equipButton.onClick.RemoveListener(ConfirmEquip);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(Cancel);
    }

    public void Show(WeaponPickup pickup, PlayerWeaponController controller)
    {
        if (pickup == null || controller == null || pickup.Weapon == null) return;
        currentPickup = pickup;
        currentController = controller;
        WeaponDefinition weapon = pickup.Weapon;
        if (promptText != null) promptText.text = "PICK UP - " + weapon.WeaponName;
        if (promptButton != null) promptButton.gameObject.SetActive(true);
        if (panel != null) panel.SetActive(false);
    }

    public void HideFor(WeaponPickup pickup)
    {
        if (pickup != currentPickup) return;
        HideAll();
    }

    private void OpenDetails()
    {
        if (currentPickup == null || currentPickup.Weapon == null) return;
        WeaponDefinition weapon = currentPickup.Weapon;
        if (promptButton != null) promptButton.gameObject.SetActive(false);
        Populate(weapon);
        if (panel != null) panel.SetActive(true);
        SetCombatControlsEnabled(false);
    }

    private void Populate(WeaponDefinition weapon)
    {
        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon.WeaponIcon;
            weaponIcon.enabled = weapon.WeaponIcon != null;
        }
        if (weaponName != null) weaponName.text = weapon.WeaponName;
        if (damageText != null) damageText.text = string.Format("Damage          {0:0.##}", weapon.BaseDamage);
        if (cooldownText != null) cooldownText.text = string.Format("Attack Cooldown  {0:0.##}s", weapon.AttackCooldown);
        if (rangeText != null) rangeText.text = string.Format("Attack Range     {0:0.##}", weapon.AttackRange);
        if (skill1Text != null) skill1Text.text = FormatSkill("Skill 1", weapon.Skill1);
        if (skill2Text != null) skill2Text.text = FormatSkill("Skill 2", weapon.Skill2);
    }

    private void ConfirmEquip()
    {
        if (currentPickup != null) currentPickup.Equip(currentController);
        HideAll();
    }

    private void Cancel() => HideAll();

    private void HideAll()
    {
        if (panel != null) panel.SetActive(false);
        if (promptButton != null) promptButton.gameObject.SetActive(false);
        SetCombatControlsEnabled(true);
        currentPickup = null;
        currentController = null;
    }

    private void SetCombatControlsEnabled(bool enabled)
    {
        if (combatControlsGroup == null) return;
        combatControlsGroup.interactable = enabled;
        combatControlsGroup.blocksRaycasts = enabled;
        combatControlsGroup.alpha = enabled ? 1f : 0.55f;
    }

    private static string FormatSkill(string label, SkillDefinition skill)
    {
        return skill == null ? label + "  -" : label + ": " + skill.SkillName;
    }
}
