using UnityEngine;

public class MovingFog : MonoBehaviour
{
    public float speed = 0.5f;

    public float rightLimit = 14f;
    public float leftReset = -14f;

    void Update()
    {
        transform.Translate(
            Vector3.right * speed * Time.deltaTime,
            Space.World
        );

        if (transform.position.x >= rightLimit)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = leftReset;

            transform.position = newPosition;
        }
    }
}