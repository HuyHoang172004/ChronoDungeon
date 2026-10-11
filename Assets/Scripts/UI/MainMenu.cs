using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private static readonly string[] MapNames =
    {
        "MAP 1 - RUINED ENTRANCE", "MAP 2 - FORGOTTEN BARRACKS",
        "MAP 3 - OVERGROWN WILDS", "MAP 4 - ECHO LIBRARY",
        "MAP 5 - ASH FORGE", "MAP 6 - MIRROR PRISON",
        "MAP 7 - WARDEN APPROACH", "MAP 8 - CHRONO SANCTUM"
    };

    private TMP_Text selectedLabel;

    private void Start()
    {
        BuildMapSelector();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }

    private void BuildMapSelector()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        Button playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        if (canvas == null || playButton == null || GameObject.Find("Map Selection Panel") != null) return;

        RectTransform panel = new GameObject("Map Selection Panel", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup))
            .GetComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);
        panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f);
        panel.anchoredPosition = new Vector2(300f, -80f);
        panel.sizeDelta = new Vector2(800f, 245f);
        panel.GetComponent<Image>().color = new Color(.025f, .045f, .075f, .92f);
        GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(22, 22, 22, 22);
        grid.cellSize = new Vector2(180f, 82f);
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        int selected = MapSelectionRuntime.SelectedMap;
        for (int i = 0; i < MapNames.Length; i++)
        {
            int mapIndex = i;
            Button option = Instantiate(playButton, panel);
            option.name = "Map " + (i + 1) + " Button";
            option.onClick = new Button.ButtonClickedEvent();
            option.onClick.AddListener(() => SelectMap(mapIndex));
            TMP_Text text = option.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = "MAP " + (i + 1);
                text.fontSize = 25f;
            }
            ColorBlock colors = option.colors;
            colors.normalColor = i == selected ? new Color(.2f, .85f, 1f, 1f) : new Color(.42f, .48f, .58f, 1f);
            option.colors = colors;
        }

        selectedLabel = new GameObject("Selected Map Label", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        selectedLabel.rectTransform.SetParent(canvas.transform, false);
        selectedLabel.rectTransform.anchorMin = selectedLabel.rectTransform.anchorMax = new Vector2(.5f, .5f);
        selectedLabel.rectTransform.anchoredPosition = new Vector2(300f, -235f);
        selectedLabel.rectTransform.sizeDelta = new Vector2(820f, 55f);
        selectedLabel.alignment = TextAlignmentOptions.Center;
        selectedLabel.fontSize = 30f;
        selectedLabel.color = new Color(.35f, .9f, 1f, 1f);
        UpdateSelectedLabel();

        RectTransform playRect = playButton.GetComponent<RectTransform>();
        playRect.anchoredPosition = new Vector2(-430f, 50f);
        MoveMenuButton("SettingButton", new Vector2(-430f, -100f));
        MoveMenuButton("ExitButton", new Vector2(-430f, -250f));
    }

    private void SelectMap(int index)
    {
        MapSelectionRuntime.SelectedMap = index;
        Debug.Log("ChronoDungeon: selected map " + (index + 1) + ".");
        UpdateSelectedLabel();
        // Rebuild the selector colors on the next menu visit; the label gives immediate feedback.
    }

    private void UpdateSelectedLabel()
    {
        if (selectedLabel != null) selectedLabel.text = "SELECTED: " + MapNames[MapSelectionRuntime.SelectedMap];
    }

    private static void MoveMenuButton(string objectName, Vector2 position)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null && target.transform is RectTransform rect) rect.anchoredPosition = position;
    }
}
