using UnityEngine;

[DisallowMultipleComponent]
public sealed class SubArea : MonoBehaviour
{
    [SerializeField] private string displayName = "Sub-area";
    [SerializeField] private AreaBounds bounds;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private bool normalZone;
    [SerializeField] private Color accentColor = new Color(0.18f, 0.72f, 0.8f, 1f);

    public string DisplayName => displayName;
    public AreaBounds Bounds => bounds;
    public Transform CameraAnchor => cameraAnchor != null ? cameraAnchor : transform;
    public bool IsNormalZone => normalZone;
    public Color AccentColor => accentColor;
    public Vector2 CameraSize => bounds != null ? bounds.Size : new Vector2(16.3f, 7.1f);

    private void Reset()
    {
        bounds = GetComponent<AreaBounds>();
        if (bounds == null) bounds = gameObject.AddComponent<AreaBounds>();
        cameraAnchor = transform;
    }
}
