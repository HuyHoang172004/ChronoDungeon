using System.Collections.Generic;
using UnityEngine;

// Oldest-to-newest history. Each slot owns a snapshot separate from the recorder.
[DisallowMultipleComponent]
public sealed class TemporalGhostManager : MonoBehaviour
{
    [SerializeField] private TimeLoopManager loop;
    [SerializeField] private PlayerTimelineRecorder recorder;
    [SerializeField] private SpriteRenderer playerVisual;
    [SerializeField] private Color ghostTint = new Color(0.75f, 0.65f, 1f, 0.5f);
    [SerializeField, Range(1, 3)] private int maxGhosts = 3;
    private readonly List<GhostPlayback> ghosts = new List<GhostPlayback>(3);
    private PlayerTimeline pending;
    private int capacity;
    public int MaxGhosts => capacity;
    public int ActiveGhostCount => ghosts.Count;
    public GhostPlayback GetGhost(int oldestFirstIndex) => ghosts[oldestFirstIndex];
    // Keep the M1.1 consumer API: this is the most recent recording's Ghost.
    public GhostPlayback ActiveGhost => ghosts.Count == 0 ? null : ghosts[ghosts.Count - 1];

    public void DisruptGhosts(float duration)
    {
        foreach (GhostPlayback ghost in ghosts)
            if (ghost != null) ghost.SetDisrupted(duration);
    }

    private void OnValidate() => maxGhosts = Mathf.Clamp(maxGhosts, 1, 3);
    private void Awake() => capacity = Mathf.Clamp(maxGhosts, 1, 3);

    private void OnEnable()
    {
        if (recorder != null) recorder.RecordingCompleted += QueueRecording;
        if (loop != null) loop.LoopRewound += StartPlayback;
        if (loop != null) loop.EncounterStarted += ClearHistory;
    }

    private void Start()
    {
        if (loop == null || recorder == null || playerVisual == null)
        {
            Debug.LogError("TemporalGhostManager requires loop, recorder and player visual references.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (recorder != null) recorder.RecordingCompleted -= QueueRecording;
        if (loop != null) loop.LoopRewound -= StartPlayback;
        if (loop != null) loop.EncounterStarted -= ClearHistory;
    }

    private void QueueRecording(PlayerTimeline timeline) => pending = timeline;

    private void StartPlayback()
    {
        if (pending == null) return;
        GhostPlayback newest;
        PlayerTimeline snapshot;
        if (ghosts.Count == capacity)
        {
            // Evict the oldest history before replacing it. No fourth object,
            // delayed Destroy overlap, or new buffer allocation at steady state.
            newest = ghosts[0];
            snapshot = newest.Timeline;
            newest.Play(null);
            ghosts.RemoveAt(0);
        }
        else
        {
            // Never clone the Player: no input, Health, recorder or colliders.
            GameObject instance = new GameObject("Temporal Ghost");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
            newest = instance.AddComponent<GhostPlayback>();
            newest.Initialize(loop, playerVisual, ghostTint);
            snapshot = new PlayerTimeline(pending.PoseCapacity, pending.ActionCapacity);
        }

        snapshot.CopyFrom(pending);
        newest.name = "Temporal Ghost - Loop " + snapshot.SourceLoop;
        newest.Play(snapshot);
        ghosts.Add(newest);
        pending = null;

        // Retained ghosts must replay from the beginning in every new loop too.
        foreach (GhostPlayback ghost in ghosts) ghost.Play(ghost.Timeline);
    }

    private void OnDestroy() => ClearHistory();

    public void ClearHistory()
    {
        pending = null;
        foreach (GhostPlayback ghost in ghosts)
        {
            if (ghost == null) continue;
            ghost.Play(null);
            ghost.gameObject.SetActive(false);
            Destroy(ghost.gameObject);
        }
        ghosts.Clear();
    }
}
