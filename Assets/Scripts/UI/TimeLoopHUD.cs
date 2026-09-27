using TMPro;
using UnityEngine;

public class TimeLoopHUD : MonoBehaviour
{
    [SerializeField] private TimeLoopManager loop;
    [SerializeField] private TMP_Text label;
    private GameManager gameManager;

    private void Awake()
    {
        if (loop == null) loop = FindAnyObjectByType<TimeLoopManager>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Start()
    {
        // Runtime-built tutorial/UI panels are created after the authored HUD. Keep the
        // loop readout visible in a stable top-left HUD slot during gameplay.
        transform.SetAsLastSibling();
        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(300f, -22f);
            rect.sizeDelta = new Vector2(260f, 78f);
        }
    }

    private void LateUpdate()
    {
        bool overlayOpen = Time.timeScale <= 0f ||
            (gameManager != null && (gameManager.IsGameOver || gameManager.IsVictory));
        bool temporalZone = loop != null && loop.enabled;
        if (label != null) label.enabled = !overlayOpen && temporalZone;
        if (!overlayOpen && temporalZone && loop != null && label != null)
            label.SetText("TIME: {0:1}\nLOOP: {1:0}", loop.remainingTime, loop.loopIndex);
    }
}
