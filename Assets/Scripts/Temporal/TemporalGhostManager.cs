using UnityEngine;

// M1.1: one reused visual ghost for the immediately preceding loop.
[DisallowMultipleComponent]
public sealed class TemporalGhostManager : MonoBehaviour
{
    [SerializeField] private TimeLoopManager loop;
    [SerializeField] private PlayerTimelineRecorder recorder;
    [SerializeField] private SpriteRenderer playerVisual;
    [SerializeField] private Color ghostTint = new Color(0.75f, 0.65f, 1f, 0.5f);
    private GhostPlayback ghost;
    private PlayerTimeline pending;
    public GhostPlayback ActiveGhost => ghost;

    private void OnEnable()
    {
        if (recorder != null) recorder.RecordingCompleted += QueueRecording;
        if (loop != null) loop.LoopRewound += StartPlayback;
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
    }

    private void QueueRecording(PlayerTimeline timeline) => pending = timeline;

    private void StartPlayback()
    {
        if (pending == null) return;
        if (ghost == null)
        {
            // Never clone the Player: no input, Health, recorder or colliders.
            GameObject instance = new GameObject("Temporal Ghost");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
            ghost = instance.AddComponent<GhostPlayback>();
            ghost.Initialize(loop, playerVisual, ghostTint);
        }
        ghost.Play(pending);
        pending = null;
    }

    private void OnDestroy()
    {
        if (ghost != null) Destroy(ghost.gameObject);
    }
}
