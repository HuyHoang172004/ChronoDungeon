using UnityEngine;

[DisallowMultipleComponent]
public sealed class DamageFeedback : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.08f;
    private SpriteRenderer[] sprites;
    private Color[] baseColors;
    private float until;

    private void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) baseColors[i] = sprites[i].color;
    }

    public void Play()
    {
        if (sprites == null) Awake();
        for (int i = 0; i < sprites.Length; i++) sprites[i].color = Color.white;
        until = Time.time + duration;
    }

    private void Update()
    {
        if (sprites == null || Time.time < until) return;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = baseColors[i];
    }

    private void OnDisable()
    {
        until = 0f;
        if (sprites == null) return;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = baseColors[i];
    }
}
