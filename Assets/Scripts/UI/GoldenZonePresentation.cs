using UnityEngine;

/// Keeps the Zone 1 presentation isolated from globally authored temporal UI.
/// Several legacy UI systems build their panels during Awake, so this guard runs
/// after those systems and hides only their presentation objects, not gameplay.
public sealed class GoldenZonePresentation : MonoBehaviour
{
    private void Awake()
    {
        ChronoGuardianHUD bossHud = GetComponent<ChronoGuardianHUD>();
        if (bossHud != null) bossHud.enabled = false;
        TutorialGuideUI tutorial = GetComponent<TutorialGuideUI>();
        if (tutorial != null) tutorial.enabled = false;
    }

    private void LateUpdate()
    {
        Transform root = transform.Find("Chrono Safe Area") ?? transform;
        SetInactive(FindDescendant(root, "Chrono Guardian HUD"));
        SetInactive(FindDescendant(root, "Tutorial Guide"));
        SetInactive(FindDescendant(root, "KenneyTopBar"));
        SetInactive(FindDescendant(root, "KenneyTemporalKey"));
        SetInactive(FindDescendant(root, "TimeLoopHUD"));
        SetInactive(FindDescendant(root, "Room HUD"));
    }

    private static void SetInactive(Transform target)
    {
        if (target != null && target.gameObject.activeSelf) target.gameObject.SetActive(false);
    }

    private static Transform FindDescendant(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            Transform found = FindDescendant(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
