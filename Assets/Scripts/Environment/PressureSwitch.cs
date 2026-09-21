using System;
using UnityEngine;
using UnityEngine.Events;

// Evaluate after GhostPlayback.LateUpdate so occupancy uses this frame's pose.
[DefaultExecutionOrder(200)]
[DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
public sealed class PressureSwitch : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private SpriteRenderer plate;
    [SerializeField] private SpriteRenderer indicator;
    [SerializeField] private Color inactiveColor = new Color(0.18f, 0.35f, 0.42f, 1f);
    [SerializeField] private Color activeColor = new Color(0.2f, 1f, 1f, 1f);
    [SerializeField] private UnityEvent<bool> onStateChanged = new UnityEvent<bool>();
    private BoxCollider2D area;
    private int resetFrame = -1;
    public bool IsActive { get; private set; }
    public int OccupantCount { get; private set; }
    public event Action<bool> StateChanged;

    private void Awake()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        UpdateVisual();
    }

    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void LateUpdate()
    {
        if (Time.timeScale <= 0f || Time.frameCount == resetFrame) return;
        int count = 0;
        if (area.enabled && (area.attachedRigidbody == null || area.attachedRigidbody.simulated))
            foreach (var actor in PressureSwitchActor.ActiveActors)
                if (actor != null && actor.gameObject.scene == gameObject.scene && actor.Overlaps(area)) count++;
        // Reconcile current geometry instead of accumulating trigger callbacks:
        // exits, disabled colliders, destroyed actors and teleports cannot leak counts.
        OccupantCount = count;
        SetState(count > 0);
    }

    public void CaptureInitialState() => ResetToInitialState();

    public void ResetToInitialState()
    {
        OccupantCount = 0;
        resetFrame = Time.frameCount;
        SetState(false);
        UpdateVisual();
    }

    private void OnDisable() => ResetToInitialState();

    private void SetState(bool active)
    {
        if (IsActive == active) return;
        IsActive = active;
        UpdateVisual();
        StateChanged?.Invoke(active);
        onStateChanged.Invoke(active);
    }

    private void UpdateVisual()
    {
        if (plate != null) plate.color = IsActive ? activeColor : inactiveColor;
        // A separate light also changes visibility: state is not color-only.
        if (indicator != null) indicator.enabled = IsActive;
    }
}
