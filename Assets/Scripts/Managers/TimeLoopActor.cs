using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TimeLoopActor : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private UnityEvent onReset = new UnityEvent();
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool initialActive;
    private Rigidbody2D body;
    private Health health;

    public void CaptureInitialState()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialActive = gameObject.activeSelf;
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    public void ResetToInitialState()
    {
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        if (body != null)
        {
            body.position = initialPosition;
            body.rotation = initialRotation.eulerAngles.z;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        if (health != null) health.RestoreToFullHealth();
        onReset.Invoke();
        gameObject.SetActive(initialActive);
    }
}
