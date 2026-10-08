using UnityEngine;

/// Smooth player-follow camera for a large authored zone. RoomCamera remains
/// responsible for the authored room-to-room framing outside this zone.
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class ZoneCameraController : MonoBehaviour
{
    [SerializeField] private WorldAreaManager areas;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private bool useGlobalZoneBounds = true;
    [SerializeField] private Vector2 globalBoundsCenter = new Vector2(10f, -3f);
    [SerializeField] private Vector2 globalBoundsSize = new Vector2(140f, 110f);
    [SerializeField, Min(.01f)] private float followSmoothTime = .16f;
    private Camera cameraComponent;
    private Vector3 velocity;
    private bool hasValidTarget;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        if (areas == null) areas = FindAnyObjectByType<WorldAreaManager>();
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        cameraComponent.orthographicSize = 8f;
    }

    private void LateUpdate()
    {
        FollowPlayer();
    }

    public void RefreshNow()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        // Zone 1 uses authored global bounds and must follow immediately even
        // when the legacy WorldAreaManager has not entered a temporal sub-area.
        // Sub-area gating remains for future area-local camera bounds.
        if (player == null || (areas != null && !areas.IsZoneActive && !useGlobalZoneBounds)) return;
        Bounds bounds = new Bounds(globalBoundsCenter, globalBoundsSize);
        if (!useGlobalZoneBounds && areas.CurrentSubArea != null && areas.CurrentSubArea.Bounds != null)
            bounds = areas.CurrentSubArea.Bounds.WorldBounds;
        float halfHeight = cameraComponent.orthographicSize;
        float halfWidth = halfHeight * cameraComponent.aspect;
        Vector3 target = player.transform.position;
        target.x = ClampToBounds(target.x, bounds.min.x, bounds.max.x, halfWidth, bounds.center.x);
        target.y = ClampToBounds(target.y, bounds.min.y, bounds.max.y, halfHeight, bounds.center.y);
        target.z = transform.position.z;
        if (!hasValidTarget)
        {
            transform.position = target;
            velocity = Vector3.zero;
            hasValidTarget = true;
            return;
        }
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, followSmoothTime);
    }

    private static float ClampToBounds(float value, float min, float max, float viewportHalfSize, float center)
    {
        float lower = min + viewportHalfSize;
        float upper = max - viewportHalfSize;
        return lower <= upper ? Mathf.Clamp(value, lower, upper) : center;
    }
}
