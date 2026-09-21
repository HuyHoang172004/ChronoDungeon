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
    private PlayerAttack playerAttack;
    private bool reportedActionOverflow;
    public PlayerTimeline CurrentRecording => recording;
    public event Action<PlayerTimeline> RecordingCompleted;

    private void Awake() => playerAttack = GetComponent<PlayerAttack>();

    private void OnEnable()
    {
        if (playerAttack != null) playerAttack.AttackPerformed += RecordAttack;
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
        if (playerAttack != null) playerAttack.AttackPerformed -= RecordAttack;
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
        reportedActionOverflow = false;
        Capture(0f);
        nextSampleTime = sampleInterval;
    }

    private void Capture(float time)
    {
        recording.AddPose(new PlayerTimeline.Pose { Time = time, Position = transform.position,
            Rotation = transform.rotation, FlipX = visual.flipX, FlipY = visual.flipY });
    }

    private void RecordAttack(AttackSnapshot attack)
    {
        if (recording == null || loop == null || !loop.IsRunning) return;
        bool added = recording.TryAddAction(new PlayerTimeline.ActionEvent {
            Time = loop.ElapsedTime, Kind = PlayerTimeline.ActionKind.Attack,
            Direction = attack.Direction, Attack = attack });
        if (!added && !reportedActionOverflow)
        {
            reportedActionOverflow = true;
            Debug.LogWarning("Timeline action capacity reached; further actions are omitted this loop.", this);
        }
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
