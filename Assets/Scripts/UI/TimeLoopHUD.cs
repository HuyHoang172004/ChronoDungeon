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
