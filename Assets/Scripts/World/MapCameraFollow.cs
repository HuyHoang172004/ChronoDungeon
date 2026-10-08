using UnityEngine;

[DisallowMultipleComponent]
public sealed class MapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private BoxCollider2D bounds;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;
    [SerializeField] private bool clampToBounds = true;

    private Camera cameraComponent;
    private Vector3 velocity;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        if (smoothTime <= 0f) transform.position = desired;
        else transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        if (clampToBounds && bounds != null && cameraComponent != null)
            ClampToBounds();
    }

    private void ClampToBounds()
    {
        Bounds area = bounds.bounds;
        float halfHeight = cameraComponent.orthographicSize;
        float halfWidth = halfHeight * cameraComponent.aspect;
        Vector3 position = transform.position;
        float minX = area.min.x + halfWidth;
        float maxX = area.max.x - halfWidth;
        float minY = area.min.y + halfHeight;
        float maxY = area.max.y - halfHeight;
        position.x = minX > maxX ? area.center.x : Mathf.Clamp(position.x, minX, maxX);
        position.y = minY > maxY ? area.center.y : Mathf.Clamp(position.y, minY, maxY);
        transform.position = position;
    }
}
