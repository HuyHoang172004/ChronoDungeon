using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ChronoGuardianHUD : MonoBehaviour
{
    [SerializeField] private ChronoGuardian boss;
    private GameObject panel;
    private Text title;
    private Text intro;
    private Text status;
    private Image fill;

    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        BuildUI();
        if (boss == null) boss = FindAnyObjectByType<ChronoGuardian>();
        if (boss != null)
        {
            boss.IntroStarted += ShowIntro;
            var bossHealth = boss.GetComponent<Health>();
            if (bossHealth != null) bossHealth.Changed += RefreshHealth;
            var shield = boss.GetComponent<ChronoGuardianTemporalShield>();
            if (shield != null) shield.StateChanged += RefreshShield;
        }
        panel.SetActive(false);
    }

    private void BuildUI()
    {
        panel = new GameObject("Chrono Guardian HUD", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.2f, .82f);
        rect.anchorMax = new Vector2(.8f, .98f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(.03f, .04f, .1f, .92f);
        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 8, 8);
        layout.spacing = 2f;
        title = CreateText("CHRONO GUARDIAN", 24, TextAnchor.MiddleCenter);
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
        intro = CreateText("", 16, TextAnchor.MiddleCenter);
        intro.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;
        status = CreateText("", 15, TextAnchor.MiddleCenter);
        status.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
        var bar = new GameObject("Boss Health Bar", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(panel.transform, false);
        bar.GetComponent<Image>().color = new Color(.12f, .02f, .04f, 1f);
        bar.AddComponent<LayoutElement>().preferredHeight = 14f;
        var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(bar.transform, false);
        var fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill = fillObject.GetComponent<Image>();
        fill.color = new Color(0.65f, 0.15f, 0.95f, 1f);
    }

    private Text CreateText(string value, int size, TextAnchor alignment)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(panel.transform, false);
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private void ShowIntro(ChronoGuardian guardian)
    {
        panel.SetActive(true);
        intro.text = guardian.IntroMessage;
        guardian.MarkIntroShown();
        RefreshHealth(guardian.Health);
        RefreshShield(false);
    }

    private void RefreshHealth(Health current)
    {
        if (fill != null && current != null) fill.fillAmount = current.currentHealth / current.maxHealth;
    }

    private void RefreshShield(bool unlocked)
    {
        if (status == null) return;
        status.text = unlocked ? "TEMPORAL SHIELD BROKEN" : "SHIELD ACTIVE - GHOST ATTACK REQUIRED";
        status.color = unlocked ? new Color(.45f, 1f, 1f) : new Color(1f, .75f, .35f);
    }

    private void OnDestroy()
    {
        if (boss != null)
        {
            boss.IntroStarted -= ShowIntro;
            var bossHealth = boss.GetComponent<Health>();
            if (bossHealth != null) bossHealth.Changed -= RefreshHealth;
            var shield = boss.GetComponent<ChronoGuardianTemporalShield>();
            if (shield != null) shield.StateChanged -= RefreshShield;
        }
    }
}
