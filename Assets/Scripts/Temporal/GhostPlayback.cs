using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class GhostPlayback : MonoBehaviour
{
    private TimeLoopManager loop;
    private SpriteRenderer visual;
    public PlayerTimeline Timeline { get; private set; }

    public void Initialize(TimeLoopManager clock, SpriteRenderer source, Color tint)
    {
        loop = clock;
        visual = GetComponent<SpriteRenderer>();
        visual.sprite = source.sprite;
        visual.sharedMaterial = source.sharedMaterial;
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder + 1;
        visual.color = tint;
        transform.localScale = source.transform.lossyScale;
        visual.enabled = false;
    }

    public void Play(PlayerTimeline timeline)
    {
        Timeline = timeline;
        visual.enabled = timeline != null && timeline.PoseCount > 0;
        ApplyPose(0f);
    }

    private void LateUpdate()
    {
        if (loop != null && loop.IsRunning) ApplyPose(loop.ElapsedTime);
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
