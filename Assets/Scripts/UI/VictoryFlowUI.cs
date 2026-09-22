using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class VictoryFlowUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private GameObject panel;
    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        BuildPanel();
        if (gameManager != null) gameManager.VictoryTriggered += Show;
        panel.SetActive(false);
    }

    private void BuildPanel()
    {
        panel = new GameObject("Victory Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(.04f, .08f, .16f, .96f);
        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(60, 60, 80, 80); layout.spacing = 20f;
        layout.childControlWidth = true; layout.childControlHeight = true;
        var title = CreateText("TEMPORAL LOOP COMPLETE", 34, TextAnchor.MiddleCenter);
        title.color = new Color(.55f, .95f, 1f); title.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;
        var detail = CreateText("CHRONO GUARDIAN DEFEATED", 20, TextAnchor.MiddleCenter);
        detail.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;
        var buttonObject = new GameObject("Victory Main Menu", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        buttonObject.GetComponent<Image>().color = new Color(.12f, .35f, .5f, 1f);
        buttonObject.AddComponent<LayoutElement>().preferredHeight = 64f;
        var buttonText = CreateText("MAIN MENU", 22, TextAnchor.MiddleCenter);
        buttonText.transform.SetParent(buttonObject.transform, false);
        var buttonRect = buttonText.rectTransform; buttonRect.anchorMin = Vector2.zero; buttonRect.anchorMax = Vector2.one;
        buttonRect.offsetMin = Vector2.zero; buttonRect.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Button>().onClick.AddListener(() => gameManager.MainMenu());
    }

    private Text CreateText(string value, int size, TextAnchor alignment)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(panel.transform, false);
        var text = go.GetComponent<Text>(); text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size; text.alignment = alignment; text.color = Color.white;
        return text;
    }

    private void Show() { panel.SetActive(true); }

    private void OnDestroy()
    {
        if (gameManager != null) gameManager.VictoryTriggered -= Show;
    }
}
