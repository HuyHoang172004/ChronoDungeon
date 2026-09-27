using UnityEngine;

// Authored room framing, independent of player motion or scene object names.
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class RoomCamera : MonoBehaviour
{
    [SerializeField] private RoomManager manager;
    private Room framedRoom;
    private WorldAreaManager areaManager;

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
        Vector3 center = area.CameraAnchor.position;
        center.z = transform.position.z;
        transform.position = center;
        Camera camera = GetComponent<Camera>();
        if (camera != null) camera.orthographicSize = Mathf.Clamp(area.CameraSize.y * 0.55f, 3.8f, 8f);
    }
}
