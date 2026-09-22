using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UpgradeChoiceUI : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private bool buildOnStart = true;

    private readonly List<UpgradeData> choices = new List<UpgradeData>();
    private readonly List<Button> choiceButtons = new List<Button>();
    private GameObject panel;
    private Text header;
    private bool wasPausedByChoice;

    public bool IsShowing => panel != null && panel.activeSelf;
    public IReadOnlyList<UpgradeData> CurrentChoices => choices;
    public int ChoiceButtonCount => choiceButtons.Count;

    private void Awake()
    {
        if (upgradeManager == null) upgradeManager = FindAnyObjectByType<UpgradeManager>();
        BuildPanel();
    }

    private void Start()
    {
        if (!buildOnStart) return;
        HideChoices();
    }

    private void BuildPanel()
    {
        if (panel != null) return;
        panel = new GameObject("Upgrade Choice Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.025f, 0.04f, 0.09f, 0.94f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(48, 48, 30, 30);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        header = CreateText("CHOOSE YOUR TEMPORAL UPGRADE", 30, TextAnchor.MiddleCenter);
        header.color = new Color(0.55f, 0.9f, 1f);
        var headerLayout = header.gameObject.AddComponent<LayoutElement>();
        headerLayout.minHeight = 60f;
        headerLayout.preferredHeight = 60f;

        choices.Clear();
        if (upgradeManager != null)
            for (int i = 0; i < upgradeManager.UpgradePool.Count && choices.Count < 3; i++)
                choices.Add(upgradeManager.UpgradePool[i]);

        for (int i = 0; i < choices.Count; i++)
        {
            UpgradeData data = choices[i];
            Button button = CreateChoiceButton(data, i);
            choiceButtons.Add(button);
        }
    }

    private Button CreateChoiceButton(UpgradeData data, int index)
    {
        var go = new GameObject("Upgrade Choice " + (index + 1), typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(panel.transform, false);
        go.GetComponent<Image>().color = new Color(0.08f, 0.14f, 0.24f, 1f);
        var layout = go.AddComponent<LayoutElement>();
        layout.minHeight = 108f;
        layout.preferredHeight = 108f;
        var group = go.AddComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(18, 18, 10, 10);
        group.spacing = 2f;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;
        var title = CreateText(data.title, 24, TextAnchor.MiddleLeft);
        title.color = Color.white;
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;
        var description = CreateText(data.description, 18, TextAnchor.MiddleLeft);
        description.color = new Color(0.7f, 0.85f, 0.95f);
        description.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;
        var button = go.GetComponent<Button>();
        int capturedIndex = index;
        button.onClick.AddListener(() => SelectChoice(capturedIndex));
        return button;
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
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    public void ShowChoices()
    {
        BuildPanel();
        if (upgradeManager == null) upgradeManager = FindAnyObjectByType<UpgradeManager>();
        panel.SetActive(true);
        wasPausedByChoice = Time.timeScale > 0f;
        Time.timeScale = 0f;
    }

    public void SelectChoice(int index)
    {
        if (!IsShowing || index < 0 || index >= choices.Count) return;
        if (upgradeManager != null) upgradeManager.ApplyUpgrade(choices[index]);
        HideChoices();
    }

    public void HideChoices()
    {
        if (panel != null) panel.SetActive(false);
        if (wasPausedByChoice) Time.timeScale = 1f;
        wasPausedByChoice = false;
    }

    private void OnDestroy()
    {
        if (wasPausedByChoice) Time.timeScale = 1f;
    }
}
