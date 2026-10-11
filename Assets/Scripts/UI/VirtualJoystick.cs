using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform joystickBG;
    public RectTransform joystickHandle;
    public PlayerMovement playerMovement;

    public float handleRange = 80f;

    private Vector2 inputDirection;

    // Read-only access for controllers that need to interpret an upward flick
    // differently from ordinary top-down movement (for example, side-view jump).
    public Vector2 InputDirection => inputDirection;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBG,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Vector2 radius = joystickBG.sizeDelta / 2f;

        inputDirection = new Vector2(
            localPoint.x / radius.x,
            localPoint.y / radius.y
        );

        inputDirection = Vector2.ClampMagnitude(inputDirection, 1f);

        joystickHandle.anchoredPosition =
            inputDirection * handleRange;

        if (playerMovement != null)
            playerMovement.SetMoveDirection(inputDirection);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetInput();
    }

    public void ResetInput()
    {
        inputDirection = Vector2.zero;

        joystickHandle.anchoredPosition = Vector2.zero;

        if (playerMovement != null)
            playerMovement.SetMoveDirection(Vector2.zero);
    }
}
