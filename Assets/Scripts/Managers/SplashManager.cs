using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public RectTransform loadingBarFill;

    public float loadingTime = 3f;

    private float maxWidth = 680f;

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        float time = 0f;

        // Bắt đầu từ 0%
        loadingBarFill.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            0f
        );

        while (time < loadingTime)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / loadingTime);

            float currentWidth = maxWidth * progress;

            loadingBarFill.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                currentWidth
            );

            yield return null;
        }

        // Đảm bảo đạt đúng 100%
        loadingBarFill.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            maxWidth
        );

        SceneManager.LoadScene("MainMenuScene");
    }
}