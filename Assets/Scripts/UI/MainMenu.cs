using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string SelectedMapKey = "ChronoDungeon.SelectedMap";

    private static readonly string[] MapScenes =
    {
        "Map01_RuinedEntrance_SideView",
        "Map02_Grassland_SideView",
        "Map03_Barbarian_SideView",
        "Map04_FrozenCrystal_SideView"
    };

    public void SelectMap(int mapNumber)
    {
        int selected = Mathf.Clamp(mapNumber, 1, MapScenes.Length);
        PlayerPrefs.SetInt(SelectedMapKey, selected);
        PlayerPrefs.Save();
        Debug.Log($"ChronoDungeon: selected map {selected} ({MapScenes[selected - 1]}).");
    }

    public void PlayGame()
    {
        int selected = Mathf.Clamp(PlayerPrefs.GetInt(SelectedMapKey, 1), 1, MapScenes.Length);
        SceneManager.LoadScene(MapScenes[selected - 1]);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }
}
