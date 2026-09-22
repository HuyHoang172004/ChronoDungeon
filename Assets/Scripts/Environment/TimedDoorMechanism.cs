using UnityEngine;

// A short-lived door pulse driven by a replayable PressureSwitch.
[DisallowMultipleComponent]
public sealed class TimedDoorMechanism : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private PressureSwitch trigger;
    [SerializeField] private Door output;
    [SerializeField, Min(0.25f)] private float openDuration = 3f;
    [SerializeField] private SpriteRenderer timerIndicator;
    private float closesAt = -1f;

    public bool IsActive => closesAt > 0f && Time.time < closesAt;
    public float RemainingTime => IsActive ? Mathf.Max(0f, closesAt - Time.time) : 0f;
    public float OpenDuration => openDuration;

    private void OnEnable()
    {
        if (trigger != null) trigger.StateChanged += OnTriggerChanged;
        if (trigger != null && trigger.IsActive) Activate();
        UpdateIndicator();
    }

    private void OnDisable()
    {
        if (trigger != null) trigger.StateChanged -= OnTriggerChanged;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || closesAt < 0f) return;
        if (Time.time >= closesAt)
        {
            closesAt = -1f;
            if (output != null) output.Close();
        }
        UpdateIndicator();
    }

    private void OnTriggerChanged(bool active)
    {
        if (active) Activate();
    }

    public void Activate()
    {
        closesAt = Time.time + Mathf.Max(0.25f, openDuration);
        if (output != null) output.Open();
        UpdateIndicator();
    }

    public void CaptureInitialState() { }

    public void ResetToInitialState()
    {
        closesAt = -1f;
        if (output != null) output.Close();
        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        if (timerIndicator == null) return;
        timerIndicator.enabled = IsActive;
        if (IsActive) timerIndicator.color = new Color(0.35f, 0.9f, 1f, 0.35f + 0.65f * (RemainingTime / Mathf.Max(0.25f, openDuration)));
    }
}
