using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class RoomExit : MonoBehaviour
{
    [SerializeField] private RoomManager manager;
    [SerializeField] private Room room;
    private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;
    private void OnTriggerEnter2D(Collider2D other) => TryEnter(other);
    private void OnTriggerStay2D(Collider2D other) => TryEnter(other);
    private void TryEnter(Collider2D other)
    {
        if (manager == null || room == null || other.attachedRigidbody == null) return;
        var actor = other.attachedRigidbody.GetComponent<PlayerMovement>();
        if (actor != null) manager.TryAdvance(room, actor);
    }
}
