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

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        bool ready = dash != null && dash.CanDash;
        if (image != null) image.color = ready ? readyColor : cooldownColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dash != null) dash.Dash();
    }
}
