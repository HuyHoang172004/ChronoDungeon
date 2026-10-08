using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PauseFlowUI : MonoBehaviour
{
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private PlayerStatsPanel statsPanel;
    private GameObject panel;
    private bool statsOverlayOpen;
    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (pauseManager == null) pauseManager = FindAnyObjectByType<PauseManager>();
        if (statsPanel == null) statsPanel = FindAnyObjectByType<PlayerStatsPanel>();
        BuildPanel();
        if (pauseManager != null) pauseManager.PauseChanged += Refresh;
        panel.SetActive(false);
    }

    public void TogglePause() => pauseManager.TogglePause();

    public void SetStatsOverlay(bool open)
    {
        if (pauseManager == null) pauseManager = FindAnyObjectByType<PauseManager>();
        statsOverlayOpen = open;
        Refresh(pauseManager != null && pauseManager.IsPaused);
    }

    private void BuildPanel()
    {
        panel = new GameObject("Pause Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(.02f, .03f, .07f, .95f);
        var layout = panel.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(80, 80, 70, 70); layout.spacing = 18f;
        layout.childControlWidth = true; layout.childControlHeight = true;
        var title = CreateText("PAUSED", 34, TextAnchor.MiddleCenter); title.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;
        AddButton("RESUME", () => pauseManager.Resume());
        AddButton("STATS", OpenStats);
        AddButton("RESTART RUN", () => pauseManager.RestartRun());
        AddButton("MAIN MENU", () => pauseManager.MainMenu());
    }

    private void OpenStats()
    {
        if (statsPanel == null) statsPanel = FindAnyObjectByType<PlayerStatsPanel>();
        if (statsPanel != null) statsPanel.Open();
    }

    private void AddButton(string label, UnityEngine.Events.UnityAction action)
    {
        var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        buttonObject.GetComponent<Image>().color = new Color(.12f, .25f, .38f, 1f);
        buttonObject.AddComponent<LayoutElement>().preferredHeight = 64f;
        var text = CreateText(label, 22, TextAnchor.MiddleCenter); text.transform.SetParent(buttonObject.transform, false);
        var rect = text.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Button>().onClick.AddListener(action);
    }

    private Text CreateText(string value, int size, TextAnchor alignment)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(panel.transform, false);
        var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size; text.alignment = alignment; text.color = Color.white; return text;
    }

    private void Refresh(bool paused)
    {
        if (panel != null) panel.SetActive(paused && !statsOverlayOpen);
    }
    private void OnDestroy() { if (pauseManager != null) pauseManager.PauseChanged -= Refresh; }
}
