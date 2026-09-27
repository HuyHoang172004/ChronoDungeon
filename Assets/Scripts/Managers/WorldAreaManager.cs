using System;
using UnityEngine;

/// Coordinates authored sub-area navigation without replacing RoomManager.
[DefaultExecutionOrder(450)]
public sealed class WorldAreaManager : MonoBehaviour
{
    [SerializeField] private SubArea[] subAreas = Array.Empty<SubArea>();
    [SerializeField] private SubArea initialSubArea;
    [SerializeField] private Room zoneRoom;
    [SerializeField] private Room connectedZoneRoom;
    [SerializeField] private PlayerMovement player;
    private bool zoneActive;
    public SubArea CurrentSubArea { get; private set; }
    public event Action<SubArea> AreaChanged;

    private void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (initialSubArea != null) SetZoneActive(true);
    }

    private void Update()
    {
        if (!zoneActive || player == null || subAreas == null) return;
        if (CurrentSubArea != null && CurrentSubArea.Bounds != null && CurrentSubArea.Bounds.Contains(player.transform.position)) return;
        foreach (SubArea area in subAreas)
            if (area != null && area.Bounds != null && area.Bounds.Contains(player.transform.position))
            {
                EnterSubArea(area);
                return;
            }
    }

    public void EnterSubArea(SubArea area)
    {
        if (area == null || CurrentSubArea == area) return;
        CurrentSubArea = area;
        AreaChanged?.Invoke(area);
    }

    public bool IsZoneRoom(Room room) => room != null && (room == zoneRoom || room == connectedZoneRoom);
    public void SetZoneActive(bool active)
    {
        zoneActive = active;
        if (active && initialSubArea != null) EnterSubArea(initialSubArea);
        else if (!active) ClearSubArea();
    }

    public void ClearSubArea()
    {
        if (CurrentSubArea == null) return;
        CurrentSubArea = null;
        AreaChanged?.Invoke(null);
    }

    public bool IsZoneActive => zoneActive;

    public bool Contains(SubArea area)
    {
        if (area == null || subAreas == null) return false;
        foreach (SubArea candidate in subAreas) if (candidate == area) return true;
        return false;
    }
}
