using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : MonoBehaviour, ITimeLoopResettable
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 joystickDirection;
    private Vector2 keyboardDirection;
    public Vector2 FacingDirection { get; private set; } = Vector2.right;
    public Vector2 CurrentMoveDirection => moveDirection;
    private Vector2 initialFacing = Vector2.right;

    public void CaptureInitialState() => initialFacing = FacingDirection;
    public void ResetToInitialState() => FacingDirection = initialFacing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (GetComponent<PlayerDash>() == null) gameObject.AddComponent<PlayerDash>();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        joystickDirection = direction;
        RecalculateMoveDirection();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current == null)
        {
            SetKeyboardMoveDirection(Vector2.zero);
            return;
        }

        Vector2 direction = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) direction.y += 1f;
        if (Keyboard.current.sKey.isPressed) direction.y -= 1f;
        if (Keyboard.current.dKey.isPressed) direction.x += 1f;
        if (Keyboard.current.aKey.isPressed) direction.x -= 1f;
        SetKeyboardMoveDirection(Vector2.ClampMagnitude(direction, 1f));
    }
#endif

    private void SetKeyboardMoveDirection(Vector2 direction)
    {
        keyboardDirection = direction;
        RecalculateMoveDirection();
    }

    private void RecalculateMoveDirection()
    {
        Vector2 selectedDirection = keyboardDirection.sqrMagnitude > 0f
            ? keyboardDirection
            : joystickDirection;
        moveDirection = selectedDirection.normalized;
        if (moveDirection.sqrMagnitude > 0f) FacingDirection = moveDirection;
    }

    void FixedUpdate()
    {
        rb.MovePosition(
            rb.position +
            moveDirection * moveSpeed * Time.fixedDeltaTime
        );
    }
}
