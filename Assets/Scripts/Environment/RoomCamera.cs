using UnityEngine;

// Authored room framing, independent of player motion or scene object names.
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class RoomCamera : MonoBehaviour
{
    [SerializeField] private RoomManager manager;
    [SerializeField, Min(1f)] private float viewSize = 2.8f;
    private Room framedRoom;
    private WorldAreaManager areaManager;
    private Camera cameraComponent;

    private void Awake()
    {
        // Migrate scenes authored with the previous camera distance.
        if (Mathf.Approximately(viewSize, 3.5f)) viewSize = 2.8f;
        cameraComponent = GetComponent<Camera>();
        ApplyViewSize();
    }

    private void OnEnable()
    {
        if (manager == null) return;
        manager.ProgressChanged += Refresh;
        areaManager = FindAnyObjectByType<WorldAreaManager>();
        if (areaManager != null) areaManager.AreaChanged += RefreshArea;
        Refresh();
    }

    private void OnDisable()
    {
        if (manager != null) manager.ProgressChanged -= Refresh;
        if (areaManager != null) areaManager.AreaChanged -= RefreshArea;
    }

    private void Refresh()
    {
        if (areaManager != null && areaManager.IsZoneActive) return;
        ApplyViewSize();
        if (manager.CurrentRoom == null || manager.CurrentRoom == framedRoom) return;
        framedRoom = manager.CurrentRoom;
        Vector3 center = framedRoom.CameraAnchor.position;
        center.z = transform.position.z;
        transform.position = center;
    }

    private void RefreshArea(SubArea area)
    {
        if (area == null) return;
        if (areaManager != null && areaManager.IsZoneActive) return;
        ApplyViewSize();
        Vector3 center = area.CameraAnchor.position;
        center.z = transform.position.z;
        transform.position = center;
    }

    private void ApplyViewSize()
    {
        if (cameraComponent == null) cameraComponent = GetComponent<Camera>();
        if (cameraComponent != null) cameraComponent.orthographicSize = viewSize;
    }
}
