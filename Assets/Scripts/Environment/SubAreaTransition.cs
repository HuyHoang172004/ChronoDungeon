using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class SubAreaTransition : MonoBehaviour
{
    [SerializeField] private WorldAreaManager manager;
    [SerializeField] private SubArea destination;
    private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other) => TryEnter(other);
    private void OnTriggerStay2D(Collider2D other) => TryEnter(other);

    private void TryEnter(Collider2D other)
    {
        if (manager == null || destination == null || other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<PlayerMovement>() != null)
            manager.EnterSubArea(destination);
    }
}
