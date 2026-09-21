using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(300)]
[DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
public sealed class Door : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private bool initiallyOpen;
    [SerializeField] private bool resetOnRewind = true;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private SpriteRenderer statusLight;
    [SerializeField] private Color lockedColor = new Color(1f, 0.55f, 0.15f, 1f);
    [SerializeField] private Color openColor = new Color(0.2f, 1f, 1f, 1f);
    [SerializeField] private UnityEvent<bool> onStateChanged = new UnityEvent<bool>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>(8);
    private BoxCollider2D blocker;
    private bool requestedOpen;
    public bool IsOpen { get; private set; }
    public bool IsLocked => !IsOpen;
    public bool IsClosePending => IsOpen && !requestedOpen;
    public event Action<bool> StateChanged;

    private void Awake()
    {
        blocker = GetComponent<BoxCollider2D>();
        blocker.isTrigger = false;
        requestedOpen = initiallyOpen;
        ApplyState(initiallyOpen);
    }

    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    // Inspector dynamic bool events or any mechanism may control this API.
    // No knowledge of a specific switch, puzzle or scene is needed here.
    public void SetOpen(bool open)
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f) return;
        requestedOpen = open;
        ApplyRequest();
    }

    private void LateUpdate()
    {
        if (IsClosePending && Time.timeScale > 0f) ApplyRequest();
    }

    private void ApplyRequest()
    {
        // Keep both the open visual and the non-blocking collider state until
        // the passage is clear. Repeated close requests cannot crush an actor.
        ApplyState(requestedOpen || PassageOccupied());
    }

    private bool PassageOccupied()
    {
        // Closing is infrequent; synchronize teleport/rewind transforms before
        // querying the disabled blocker's intended world-space footprint.
        Physics2D.SyncTransforms();
        Vector3 scale = transform.lossyScale;
        Vector2 size = Vector2.Scale(blocker.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        var filter = new ContactFilter2D { useTriggers = false };
        Physics2D.OverlapBox(transform.TransformPoint(blocker.offset), size,
            transform.eulerAngles.z, filter, overlaps);
        bool occupied = false;
        foreach (var hit in overlaps)
        {
            if (hit == blocker || hit.transform.IsChildOf(transform)) continue;
            var body = hit.attachedRigidbody;
            if (body != null && body.simulated && body.bodyType != RigidbodyType2D.Static)
            { occupied = true; break; }
        }
        overlaps.Clear();
        return occupied;
    }

    private void ApplyState(bool open)
    {
        bool changed = IsOpen != open;
        IsOpen = open;
        blocker.enabled = !open;
        if (closedVisual != null) closedVisual.SetActive(!open);
        if (statusLight != null) statusLight.color = open ? openColor : lockedColor;
        if (!changed) return;
        StateChanged?.Invoke(open);
        onStateChanged.Invoke(open);
    }

    // Awake already applies the authored state; never capture a transient
    // mechanism command as the next loop's initial state.
    public void CaptureInitialState() { }

    public void ResetToInitialState()
    {
        if (!resetOnRewind) return;
        if (blocker == null) blocker = GetComponent<BoxCollider2D>();
        requestedOpen = initiallyOpen;
        ApplyRequest();
    }
}
