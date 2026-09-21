using System.Collections.Generic;
using UnityEngine;

// Explicit opt-in: an Enemy or arbitrary trigger is never a switch occupant.
[DisallowMultipleComponent]
public sealed class PressureSwitchActor : MonoBehaviour
{
    internal static readonly HashSet<PressureSwitchActor> ActiveActors = new HashSet<PressureSwitchActor>();
    [SerializeField] private bool useReplayPoint;
    private Collider2D[] shapes;
    private Health health;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearRegistry() => ActiveActors.Clear();

    private void Awake()
    {
        health = GetComponent<Health>();
        RefreshColliders();
    }

    private void OnEnable() => ActiveActors.Add(this);
    private void OnDisable() => ActiveActors.Remove(this);

    // Call after deliberately adding/removing actor colliders at runtime.
    public void RefreshColliders() => shapes = GetComponentsInChildren<Collider2D>(true);

    // Ghosts remain non-physical: only switches query their replay anchor.
    public void UseReplayPoint() => useReplayPoint = true;

    internal bool Overlaps(Collider2D area)
    {
        if (!isActiveAndEnabled || (health != null && health.IsDead)) return false;
        if (useReplayPoint) return area.OverlapPoint(transform.position);
        if (shapes == null) return false;
        foreach (var shape in shapes)
        {
            if (shape == null || !shape.enabled || !shape.gameObject.activeInHierarchy ||
                (shape.attachedRigidbody != null && !shape.attachedRigidbody.simulated)) continue;
            // A nested opt-in actor owns its own colliders, not its parent's.
            if (shape.GetComponentInParent<PressureSwitchActor>() != this) continue;
            var distance = area.Distance(shape);
            if (distance.isValid && distance.distance <= 0f) return true;
        }
        return false;
    }
}
