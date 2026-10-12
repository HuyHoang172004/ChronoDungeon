using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SideViewCameraBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = GetComponentInParent<Camera>();
        FitToCamera();
    }

    private void LateUpdate() => FitToCamera();

    private void FitToCamera()
    {
        if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null) return;
        float height = targetCamera.orthographicSize * 2.05f;
        float width = height * targetCamera.aspect;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float scale = Mathf.Max(width / spriteSize.x, height / spriteSize.y);
        transform.localScale = new Vector3(scale, scale, 1f);
        transform.localPosition = new Vector3(0f, 0f, 10f);
    }
}
