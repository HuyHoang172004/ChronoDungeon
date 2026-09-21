using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMovement), typeof(SpriteRenderer))]
public sealed class PlayerTimelineRecorder : MonoBehaviour
{
    [SerializeField] private TimeLoopManager loop;
    [SerializeField, Range(0.02f, 0.2f)] private float sampleInterval = 0.05f;
    private SpriteRenderer visual;
    private PlayerTimeline recording, spare;
    private float nextSampleTime;
    public PlayerTimeline CurrentRecording => recording;
    public event Action<PlayerTimeline> RecordingCompleted;

    private void OnEnable()
    {
        if (loop == null) return;
        loop.LoopEnding += FinishRecording;
        loop.LoopRewound += BeginRecording;
    }

    private void Start()
    {
        if (loop == null)
        {
            Debug.LogError("PlayerTimelineRecorder requires a TimeLoopManager reference.", this);
            enabled = false;
            return;
        }
        visual = GetComponent<SpriteRenderer>();
        sampleInterval = Mathf.Clamp(sampleInterval, 0.02f, 0.2f);
        int capacity = Mathf.Clamp(Mathf.CeilToInt(loop.LoopDuration / sampleInterval) + 3, 3, 10000);
        recording = new PlayerTimeline(capacity);
        spare = new PlayerTimeline(capacity);
        BeginRecording();
    }

    private void OnDisable()
    {
        if (loop == null) return;
        loop.LoopEnding -= FinishRecording;
        loop.LoopRewound -= BeginRecording;
    }

    private void LateUpdate()
    {
        if (recording == null || !loop.IsRunning || loop.ElapsedTime < nextSampleTime) return;
        Capture(loop.ElapsedTime);
        // Do not fabricate historical poses after a slow frame.
        nextSampleTime = loop.ElapsedTime + sampleInterval;
    }

    private void BeginRecording()
    {
        if (recording == null) return;
        recording.Clear(loop.loopIndex);
        Capture(0f);
        nextSampleTime = sampleInterval;
    }

    private void Capture(float time)
    {
        recording.AddPose(new PlayerTimeline.Pose { Time = time, Position = transform.position,
            Rotation = transform.rotation, FlipX = visual.flipX, FlipY = visual.flipY });
    }

    private void FinishRecording()
    {
        if (recording == null) return;
        Capture(loop.LoopDuration);
        PlayerTimeline completed = recording;
        recording = spare;
        spare = completed;
        RecordingCompleted?.Invoke(completed);
    }
}
