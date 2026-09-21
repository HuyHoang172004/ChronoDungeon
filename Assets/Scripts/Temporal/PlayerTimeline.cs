using UnityEngine;

// Separate pose and action channels let later milestones add actions without
// changing movement sampling. Buffers are bounded and reused between loops.
public sealed class PlayerTimeline
{
    public struct Pose
    {
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool FlipX, FlipY;
    }

    public enum ActionKind { Attack, Dash, Interaction, Skill }
    public struct ActionEvent
    {
        public float Time;
        public ActionKind Kind;
        public Vector2 Direction;
        public int Payload;
    }

    private readonly Pose[] poses;
    private readonly ActionEvent[] actions;
    public int PoseCount { get; private set; }
    public int ActionCount { get; private set; }
    public int SourceLoop { get; private set; }
    public int PoseCapacity => poses.Length;
    public int ActionCapacity => actions.Length;
    public Pose GetPose(int index) => poses[index];
    public ActionEvent GetAction(int index) => actions[index];

    public PlayerTimeline(int poseCapacity, int actionCapacity = 256)
    {
        poses = new Pose[Mathf.Max(2, poseCapacity)];
        actions = new ActionEvent[Mathf.Max(1, actionCapacity)];
    }

    public void Clear(int sourceLoop)
    {
        PoseCount = ActionCount = 0;
        SourceLoop = sourceLoop;
    }

    // Ghost-owned snapshots must not retain the recorder's rotating buffers.
    // Copy only at rewind; destination storage is reused after oldest eviction.
    public void CopyFrom(PlayerTimeline source)
    {
        if (source == null) throw new System.ArgumentNullException(nameof(source));
        if (source.PoseCount > poses.Length || source.ActionCount > actions.Length)
            throw new System.ArgumentException("Timeline snapshot capacity is too small.", nameof(source));
        System.Array.Copy(source.poses, poses, source.PoseCount);
        System.Array.Copy(source.actions, actions, source.ActionCount);
        PoseCount = source.PoseCount;
        ActionCount = source.ActionCount;
        SourceLoop = source.SourceLoop;
    }

    public void AddPose(Pose pose)
    {
        if (PoseCount > 0 && pose.Time <= poses[PoseCount - 1].Time)
        {
            if (pose.Time == poses[PoseCount - 1].Time) poses[PoseCount - 1] = pose;
            return;
        }
        if (PoseCount < poses.Length) poses[PoseCount++] = pose;
        else poses[PoseCount - 1] = pose; // Always preserve the final endpoint.
    }

    public bool TryAddAction(ActionEvent action)
    {
        if (ActionCount == actions.Length ||
            (ActionCount > 0 && action.Time < actions[ActionCount - 1].Time)) return false;
        actions[ActionCount++] = action;
        return true;
    }

    public Pose Evaluate(float time)
    {
        if (PoseCount == 0) return default;
        if (time <= poses[0].Time) return poses[0];
        if (time >= poses[PoseCount - 1].Time) return poses[PoseCount - 1];
        int low = 0, high = PoseCount - 1;
        while (high - low > 1)
        {
            int mid = (low + high) / 2;
            if (poses[mid].Time <= time) low = mid;
            else high = mid;
        }
        Pose a = poses[low], b = poses[high];
        float t = Mathf.InverseLerp(a.Time, b.Time, time);
        return new Pose { Time = time, Position = Vector3.Lerp(a.Position, b.Position, t),
            Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t), FlipX = a.FlipX, FlipY = a.FlipY };
    }
}
