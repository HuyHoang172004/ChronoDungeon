using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class DashButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PlayerDash dash;
    [SerializeField] private Color readyColor = new Color(0.15f, 0.8f, 1f, 0.82f);
    [SerializeField] private Color cooldownColor = new Color(0.25f, 0.35f, 0.4f, 0.72f);
    private Image image;
    private TextMeshProUGUI label;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (dash == null) dash = FindAnyObjectByType<PlayerDash>();
        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-390f, 180f);
            rect.sizeDelta = new Vector2(180f, 180f);
        }
        label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
        {
            GameObject child = new GameObject("Dash Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            child.transform.SetParent(transform, false);
            label = child.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 34f;
            label.color = Color.white;
            label.text = "DASH";
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }
    }

    private void Update()
    {
        bool ready = dash != null && dash.CanDash;
        if (image != null) image.color = ready ? readyColor : cooldownColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dash == null) dash = FindAnyObjectByType<PlayerDash>();
        if (dash != null) dash.Dash();
    }
}
