using UnityEngine;

public class PlayerMovement : MonoBehaviour, ITimeLoopResettable
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    public Vector2 FacingDirection { get; private set; } = Vector2.right;
    private Vector2 initialFacing = Vector2.right;

    public void CaptureInitialState() => initialFacing = FacingDirection;
    public void ResetToInitialState() => FacingDirection = initialFacing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
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
