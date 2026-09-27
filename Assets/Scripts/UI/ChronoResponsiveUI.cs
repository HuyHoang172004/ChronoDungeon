using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Keeps authored UI readable across landscape aspect ratios and Android cutouts.
/// Existing anchors remain authoritative; the shared safe-area container only clips the
/// usable canvas region and the CanvasScaler keeps the 1920x1080 design scale stable.
/// </summary>
public sealed class ChronoResponsiveUI : MonoBehaviour
{
    private readonly List<Canvas> canvases = new List<Canvas>(4);
    private readonly List<RectTransform> safeRoots = new List<RectTransform>(4);
    private int lastWidth;
    private int lastHeight;
    private Rect lastSafeArea;

    public int WrappedCanvasCount => safeRoots.Count;
    public float ActiveAspectRatio => lastHeight > 0 ? (float)lastWidth / lastHeight : 0f;
    public Rect ActiveSafeArea => lastSafeArea;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if ((scene.name == "SplashScene" || scene.name == "MainMenuScene" || scene.name == "GameScene") &&
            FindAnyObjectByType<ChronoResponsiveUI>() == null)
        {
            GameObject root = new GameObject("Chrono Responsive UI");
            root.AddComponent<ChronoResponsiveUI>();
        }
    }

    private void Awake()
    {
        RefreshCanvasBindings();
        ApplyLayout(true);
    }

    private void LateUpdate()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight || Screen.safeArea != lastSafeArea)
            ApplyLayout(false);
    }

    private void RefreshCanvasBindings()
    {
        canvases.Clear();
        safeRoots.Clear();

        Canvas[] found = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < found.Length; i++)
        {
            Canvas canvas = found[i];
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform safeRoot = canvas.transform.Find("Chrono Safe Area") as RectTransform;
            if (safeRoot == null)
            {
                GameObject safeObject = new GameObject("Chrono Safe Area", typeof(RectTransform));
                safeRoot = safeObject.GetComponent<RectTransform>();
                safeRoot.SetParent(canvas.transform, false);

                List<Transform> existingChildren = new List<Transform>();
                for (int childIndex = 0; childIndex < canvas.transform.childCount; childIndex++)
                {
                    Transform child = canvas.transform.GetChild(childIndex);
                    if (child != safeRoot) existingChildren.Add(child);
                }

                for (int childIndex = 0; childIndex < existingChildren.Count; childIndex++)
                {
                    existingChildren[childIndex].SetParent(safeRoot, false);
                    existingChildren[childIndex].SetSiblingIndex(childIndex);
                }
            }

            safeRoot.anchorMin = Vector2.zero;
            safeRoot.anchorMax = Vector2.one;
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
            safeRoot.SetAsLastSibling();
            canvases.Add(canvas);
            safeRoots.Add(safeRoot);
        }
    }

    private void ApplyLayout(bool force)
    {
        if (canvases.Count == 0) RefreshCanvasBindings();

        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
        if (lastWidth <= 0 || lastHeight <= 0) return;

        Vector2 min = new Vector2(lastSafeArea.xMin / lastWidth, lastSafeArea.yMin / lastHeight);
        Vector2 max = new Vector2(lastSafeArea.xMax / lastWidth, lastSafeArea.yMax / lastHeight);
        for (int i = 0; i < safeRoots.Count; i++)
        {
            RectTransform safeRoot = safeRoots[i];
            if (safeRoot == null) continue;
            safeRoot.anchorMin = min;
            safeRoot.anchorMax = max;
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
        }
    }
}
