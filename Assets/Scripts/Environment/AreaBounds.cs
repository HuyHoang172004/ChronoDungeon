using UnityEngine;

/// Authored footprint for a reusable world sub-area. It is deliberately
/// independent from Room so exploration spaces can be used with or without
/// encounter progression.
[DisallowMultipleComponent]
public sealed class AreaBounds : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(16.3f, 7.1f);
    public Vector2 Size => size;
    public Bounds WorldBounds => new Bounds(transform.position, new Vector3(size.x, size.y, 1f));
    public bool Contains(Vector2 position) => WorldBounds.Contains(new Vector3(position.x, position.y, 0f));
}
