using UnityEngine;

// Authored room framing, independent of player motion or scene object names.
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class RoomCamera : MonoBehaviour
{
    [SerializeField] private RoomManager manager;
    private Room framedRoom;

    private void OnEnable()
    {
        if (manager == null) return;
        manager.ProgressChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (manager != null) manager.ProgressChanged -= Refresh;
    }

    private void Refresh()
    {
        if (manager.CurrentRoom == null || manager.CurrentRoom == framedRoom) return;
        framedRoom = manager.CurrentRoom;
        Vector3 center = framedRoom.CameraAnchor.position;
        center.z = transform.position.z;
        transform.position = center;
    }
}
