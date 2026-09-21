using TMPro;
using UnityEngine;

public sealed class RoomHUD : MonoBehaviour
{
    [SerializeField] private RoomManager manager;
    [SerializeField] private TMP_Text label;
    private void OnEnable() { manager.ProgressChanged += Refresh; Refresh(); }
    private void OnDisable() { if (manager != null) manager.ProgressChanged -= Refresh; }
    private void Refresh()
    {
        if (manager.CurrentRoom == null) { label.text = ""; return; }
        label.text = manager.IsComplete ? "ENCOUNTERS COMPLETE" :
            (manager.CurrentIndex + 1) + "/" + manager.RoomCount + "  " + manager.CurrentRoom.DisplayName +
            (manager.CurrentRoom.State == RoomState.Completed ? "  -  EXIT OPEN" : "");
    }
}
