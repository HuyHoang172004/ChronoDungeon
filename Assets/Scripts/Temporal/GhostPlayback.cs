using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class GhostPlayback : MonoBehaviour
{
    private TimeLoopManager loop;
    private SpriteRenderer visual;
    private PressureSwitchActor switchActor;
    private readonly AttackResolver attacks = new AttackResolver();
    private int nextAction;
    public event System.Action<PlayerTimeline.ActionEvent> ActionReplayed;
    public PlayerTimeline Timeline { get; private set; }

    public void Initialize(TimeLoopManager clock, SpriteRenderer source, Color tint)
    {
        loop = clock;
        loop.LoopEnding += FinishActions;
        visual = GetComponent<SpriteRenderer>();
        visual.sprite = source.sprite;
        visual.sharedMaterial = source.sharedMaterial;
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder + 1;
        visual.color = tint;
        transform.localScale = source.transform.lossyScale;
        visual.enabled = false;
        switchActor = gameObject.AddComponent<PressureSwitchActor>();
        switchActor.UseReplayPoint();
        switchActor.enabled = false;
    }

    public void Play(PlayerTimeline timeline)
    {
        Timeline = timeline;
        nextAction = 0;
        visual.enabled = timeline != null && timeline.PoseCount > 0;
        if (switchActor != null) switchActor.enabled = visual.enabled;
        ApplyPose(0f);
    }

    private void LateUpdate()
    {
        if (loop == null || !loop.IsRunning) return;
        ApplyPose(loop.ElapsedTime);
        ReplayActions(loop.ElapsedTime);
    }

    // Flush the final frame before world reset, even if Update reached the
    // boundary before this Ghost's LateUpdate. Play only resets the cursor:
    // the manager may call it twice when adding a slot, so it must not attack.
    private void FinishActions() => ReplayActions(loop.LoopDuration);

    private void ReplayActions(float time)
    {
        if (Timeline == null) return;
        while (nextAction < Timeline.ActionCount && Timeline.GetAction(nextAction).Time <= time)
        {
            var action = Timeline.GetAction(nextAction++);
            switch (action.Kind)
            {
                case PlayerTimeline.ActionKind.Attack:
                    attacks.Execute(action.Attack);
                    break;
                // Future Dash/Interaction/Skill handlers use the same cursor.
            }
            ActionReplayed?.Invoke(action);
        }
    }

    private void OnDestroy()
    {
        if (loop != null) loop.LoopEnding -= FinishActions;
    }

    private void ApplyPose(float time)
    {
        if (Timeline == null || Timeline.PoseCount == 0) return;
        PlayerTimeline.Pose pose = Timeline.Evaluate(time);
        transform.SetPositionAndRotation(pose.Position, pose.Rotation);
        visual.flipX = pose.FlipX;
        visual.flipY = pose.FlipY;
    }
}
