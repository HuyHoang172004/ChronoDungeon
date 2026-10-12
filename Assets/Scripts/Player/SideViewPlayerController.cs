using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Side-view player motor. Horizontal input moves the actor; upward joystick
/// flicks request up to three height-calibrated jumps before the next landing.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class SideViewPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VirtualJoystick virtualJoystick;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private SpriteRenderer visualRenderer;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 6f;
    [SerializeField, Min(0.1f)] private float acceleration = 40f;
    [SerializeField, Min(0.25f)] private float jumpHeightInBodyLengths = 1f;
    [SerializeField, Range(1, 3)] private int maximumJumpCount = 3;
    [SerializeField, Min(0.1f)] private float gravityMultiplier = 3f;
    [SerializeField, Range(0.2f, 1f)] private float jumpFlickThreshold = 0.7f;
    [SerializeField, Range(0.01f, 1f)] private float groundCheckRadius = 0.12f;
    [SerializeField, Range(0f, 0.25f)] private float coyoteTime = 0.1f;
    [SerializeField, Range(0f, 0.25f)] private float jumpBufferTime = 0.1f;

    [Header("Prototype Dash")]
    [SerializeField, Min(0.1f)] private float dashDistance = 2.25f;
    [SerializeField, Min(0.05f)] private float dashCooldown = 0.7f;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private float horizontalInput;
    private float nextDashTime;
    private float coyoteTimeRemaining;
    private float jumpBufferRemaining;
    private float verticalVelocity;
    private bool joystickJumpArmed = true;
    private bool wasGrounded;
    private int jumpsUsed;
    private readonly RaycastHit2D[] dashHits = new RaycastHit2D[8];
    private ContactFilter2D dashFilter;

    public bool IsGrounded { get; private set; }
    public bool FacingRight { get; private set; } = true;
    public Vector2 FacingDirection => FacingRight ? Vector2.right : Vector2.left;
    public float DashCooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);
    public int JumpsUsed => jumpsUsed;
    public int JumpsRemaining => Mathf.Max(0, maximumJumpCount - jumpsUsed);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        if (groundLayers.value == 0)
            groundLayers = LayerMask.GetMask("SideViewGround", "SideViewOneWay");
        dashFilter.useTriggers = false;
        dashFilter.SetLayerMask(groundLayers);
        // The legacy top-down Player Rigidbody can carry stale gravity/constraint
        // state into the side-view conversion. Keep vertical integration local so
        // jump and fall remain deterministic in the authored platform route.
        body.gravityScale = 0f;
        if (visualRenderer == null) visualRenderer = GetComponentInChildren<SpriteRenderer>();
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.SetParent(transform, false);
            groundCheck = check.transform;
        }

        // Match the check to the real collider bottom. The side-view sprites use
        // a feet pivot, so a fixed negative offset would sit below the platform.
        if (bodyCollider != null)
        {
            Vector3 bottomWorld = new Vector3(
                bodyCollider.bounds.center.x,
                bodyCollider.bounds.min.y,
                transform.position.z);
            groundCheck.position = bottomWorld;
        }
    }

    private void Update()
    {
        UpdateInput();
        IsGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers) != null;
        if (IsGrounded && !wasGrounded)
            jumpsUsed = 0;
        wasGrounded = IsGrounded;
        coyoteTimeRemaining = IsGrounded
            ? coyoteTime
            : Mathf.Max(0f, coyoteTimeRemaining - Time.deltaTime);
        jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - Time.deltaTime);

        Vector2 joystickInput = virtualJoystick != null ? virtualJoystick.InputDirection : Vector2.zero;
        bool joystickRequestsJump = joystickInput.y >= jumpFlickThreshold;
        if (joystickRequestsJump && joystickJumpArmed)
        {
            RequestJump();
            joystickJumpArmed = false;
        }
        else if (joystickInput.y < jumpFlickThreshold * 0.55f)
        {
            joystickJumpArmed = true;
        }
    }

    private void FixedUpdate()
    {
        // Read the held horizontal axis here as well as in Update. This keeps
        // mobile drag input continuous when Unity's UI event loop is throttled
        // or loses an Update tick during a camera/scene transition.
        float joystickX = virtualJoystick != null ? virtualJoystick.InputDirection.x : 0f;
        float keyboardX = 0f;
#if UNITY_EDITOR
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboardX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboardX += 1f;
        }
#endif
        horizontalInput = Mathf.Abs(keyboardX) > 0.01f ? keyboardX : joystickX;

        bool canUseGroundJump = coyoteTimeRemaining > 0f && jumpsUsed == 0;
        bool canUseAirJump = !IsGrounded && jumpsUsed < maximumJumpCount;
        if (jumpBufferRemaining > 0f && (canUseGroundJump || canUseAirJump))
        {
            float bodyHeight = bodyCollider != null ? bodyCollider.bounds.size.y : 1f;
            float gravity = Mathf.Abs(Physics2D.gravity.y * gravityMultiplier);
            float currentUpwardSpeed = Mathf.Max(0f, verticalVelocity);
            // Adding 2*g*h to v^2 raises the apex by approximately one body height
            // per press, even when the next jump is requested while still ascending.
            verticalVelocity = Mathf.Sqrt(
                currentUpwardSpeed * currentUpwardSpeed +
                2f * gravity * bodyHeight * jumpHeightInBodyLengths);
            jumpsUsed++;
            jumpBufferRemaining = 0f;
            coyoteTimeRemaining = 0f;
            IsGrounded = false;
        }

        if (IsGrounded && verticalVelocity < 0f)
            verticalVelocity = -1.5f;
        verticalVelocity += Physics2D.gravity.y * gravityMultiplier * Time.fixedDeltaTime;

        // Move the body from the authored input delta instead of inheriting the
        // legacy Rigidbody velocity. This prevents friction/damping or stale
        // top-down physics state from making horizontal movement slow to zero.
        float targetVelocityX = horizontalInput * moveSpeed;
        float acceleratedVelocityX = Mathf.MoveTowards(
            body.linearVelocity.x,
            targetVelocityX,
            acceleration * Time.fixedDeltaTime);
        Vector2 nextPosition = body.position + new Vector2(
            acceleratedVelocityX * Time.fixedDeltaTime,
            verticalVelocity * Time.fixedDeltaTime);
        body.MovePosition(nextPosition);
        body.linearVelocity = new Vector2(acceleratedVelocityX, verticalVelocity);

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            FacingRight = horizontalInput > 0f;
        }
    }

    public void TryJump()
    {
        RequestJump();
    }

    public void RequestJump()
    {
        if (!IsGrounded && jumpsUsed >= maximumJumpCount) return;
        jumpBufferRemaining = jumpBufferTime;
    }

    public void Dash()
    {
        if (DashCooldownRemaining > 0f) return;
        nextDashTime = Time.time + dashCooldown;
        float travel = dashDistance;
        int hitCount = body.Cast(FacingDirection, dashFilter, dashHits, dashDistance);
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = dashHits[i];
            if (hit.collider == null || hit.collider.isTrigger) continue;
            travel = Mathf.Min(travel, Mathf.Max(0f, hit.distance - 0.04f));
        }

        body.position += FacingDirection * travel;
        Physics2D.SyncTransforms();
    }

    private void UpdateInput()
    {
        float joystickX = virtualJoystick != null ? virtualJoystick.InputDirection.x : 0f;
        float keyboardX = 0f;

#if UNITY_EDITOR
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboardX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboardX += 1f;
            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) RequestJump();
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame) Dash();
        }
#endif

        horizontalInput = Mathf.Abs(keyboardX) > 0.01f ? keyboardX : joystickX;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
