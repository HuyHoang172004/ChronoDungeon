using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SideViewWorldHealthBar : MonoBehaviour
{
    private Health health;
    private RectTransform fill;
    private GameObject root;

    private void Awake()
    {
        health = GetComponent<Health>();
        Build();
        if (health != null)
        {
            health.Changed += Refresh;
            health.Died += OnDied;
            Refresh(health);
        }
    }

    private void Build()
    {
        root = new GameObject("EnemyHealthBar", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(140f, 16f);
        rect.localScale = Vector3.one * .01f;
        var sprite = GetComponentInChildren<SpriteRenderer>();
        float height = sprite != null && sprite.sprite != null ? sprite.sprite.bounds.size.y + .35f : 1.5f;
        rect.localPosition = new Vector3(0f, height, 0f);

        var background = new GameObject("Frame", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(root.transform, false);
        Stretch(background.GetComponent<RectTransform>(), 0f);
        background.GetComponent<Image>().color = new Color(.03f, .04f, .055f, .95f);

        var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(background.transform, false);
        fill = fillObject.GetComponent<RectTransform>();
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.pivot = new Vector2(0f, .5f);
        fill.offsetMin = new Vector2(3f, 3f);
        fill.offsetMax = new Vector2(-3f, -3f);
        fillObject.GetComponent<Image>().color = new Color(.14f, .88f, .38f, 1f);
    }

    private void Refresh(Health value)
    {
        if (fill == null) return;
        float ratio = value.maxHealth <= 0f ? 0f : Mathf.Clamp01(value.currentHealth / value.maxHealth);
        fill.anchorMax = new Vector2(ratio, 1f);
        fill.offsetMax = new Vector2(ratio >= .001f ? -3f : 0f, -3f);
    }

    private void OnDied(Health _) { if (root != null) root.SetActive(false); }

    private void OnDestroy()
    {
        if (health == null) return;
        health.Changed -= Refresh;
        health.Died -= OnDied;
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = Vector2.one * -inset;
    }
}
