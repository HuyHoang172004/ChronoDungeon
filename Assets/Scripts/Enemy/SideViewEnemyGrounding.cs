using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class SideViewEnemyGrounding : MonoBehaviour
{
    [SerializeField] private float castHeight = 12f;

    private void Start()
    {
        Collider2D body = GetComponent<Collider2D>();
        int mask = LayerMask.GetMask("SideViewGround", "SideViewOneWay");
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(transform.position.x, transform.position.y + castHeight * .5f),
            Vector2.down, castHeight, mask);
        if (hit.collider == null) return;
        float delta = hit.point.y - body.bounds.min.y + .025f;
        transform.position += Vector3.up * delta;
        Physics2D.SyncTransforms();
    }
}
