using UnityEngine;

/// <summary>Horizontal camera follow used only by the isolated side-view prototype.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class SideViewCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private BoxCollider2D bounds;
    [SerializeField] private Vector2 targetOffset = new Vector2(2f, 1f);
    [SerializeField, Min(0f)] private float smoothTime = 0.14f;

    private Camera cameraComponent;
    private Vector3 velocity;

    private void Awake() => cameraComponent = GetComponent<Camera>();

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x + targetOffset.x, target.position.y + targetOffset.y, transform.position.z);
        Vector3 next = smoothTime <= 0f ? desired : Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        if (bounds != null && cameraComponent != null)
        {
            Bounds area = bounds.bounds;
            float halfHeight = cameraComponent.orthographicSize;
            float halfWidth = halfHeight * cameraComponent.aspect;
            next.x = Mathf.Clamp(next.x, area.min.x + halfWidth, area.max.x - halfWidth);
            next.y = Mathf.Clamp(next.y, area.min.y + halfHeight, area.max.y - halfHeight);
        }

        transform.position = next;
    }
}
