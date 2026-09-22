using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SettingsFlowUI : MonoBehaviour
{
    [SerializeField] private SettingsManager settings;
    private GameObject panel;
    private Slider master;
    private Slider music;
    private Slider sfx;
    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (settings == null) settings = FindAnyObjectByType<SettingsManager>();
        BuildPanel(); panel.SetActive(false);
    }

    public void Toggle() { if (panel != null) panel.SetActive(!panel.activeSelf); }
    public void Show() { panel.SetActive(true); }
    public void Hide() { panel.SetActive(false); }

    private void BuildPanel()
    {
        panel = new GameObject("Settings Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(transform, false);
        var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.15f, .15f); rect.anchorMax = new Vector2(.85f, .85f); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(.03f, .04f, .09f, .97f);
        var layout = panel.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(40, 40, 30, 30); layout.spacing = 10f;
        layout.childControlWidth = true; layout.childControlHeight = true;
        AddText("SETTINGS", 30, 48);
        master = AddSlider("MASTER VOLUME", settings.MasterVolume, settings.SetMasterVolume);
        music = AddSlider("MUSIC VOLUME", settings.MusicVolume, settings.SetMusicVolume);
        sfx = AddSlider("SFX VOLUME", settings.SfxVolume, settings.SetSfxVolume);
        AddButton("RESET DEFAULTS", () => { settings.ResetDefaults(); Refresh(); });
        AddButton("CLOSE", Hide);
    }

    private void AddText(string value, int size, float height)
    {
        var go = new GameObject(value, typeof(RectTransform), typeof(Text)); go.transform.SetParent(panel.transform, false);
        var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
        go.AddComponent<LayoutElement>().preferredHeight = height;
    }

    private Slider AddSlider(string label, float value, UnityEngine.Events.UnityAction<float> changed)
    {
        AddText(label, 18, 28); var go = new GameObject(label + " Slider", typeof(RectTransform), typeof(Slider)); go.transform.SetParent(panel.transform, false);
        var slider = go.GetComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; slider.value = value; slider.onValueChanged.AddListener(changed); go.AddComponent<LayoutElement>().preferredHeight = 34f; return slider;
    }

    private void AddButton(string label, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(panel.transform, false); go.GetComponent<Image>().color = new Color(.12f, .25f, .38f, 1f); go.AddComponent<LayoutElement>().preferredHeight = 48f; var text = AddButtonText(label, go.transform); go.GetComponent<Button>().onClick.AddListener(action);
    }
    private Text AddButtonText(string value, Transform parent) { var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var t = go.GetComponent<Text>(); t.text = value; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = 18; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; var r=t.rectTransform; r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero; return t; }
    private void Refresh() { master.value = settings.MasterVolume; music.value = settings.MusicVolume; sfx.value = settings.SfxVolume; }
}
