#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Pressing Play in the editor always follows the real player flow: menu first.</summary>
[InitializeOnLoad]
public static class PlayFromMainMenu
{
    private const string MainMenuPath = "Assets/Scenes/MainMenuScene.unity";

    static PlayFromMainMenu()
    {
        EditorApplication.delayCall += ConfigureStartScene;
    }

    private static void ConfigureStartScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        SceneAsset menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
        if (menu != null) EditorSceneManager.playModeStartScene = menu;
    }

    [MenuItem("ChronoDungeon/Diagnostics/Validate Current Map and Characters", true)]
    private static bool CanValidateCurrentGame()
    {
        return EditorApplication.isPlaying && !EditorApplication.isCompiling &&
            SceneManager.GetActiveScene().name == "GameScene";
    }

    [MenuItem("ChronoDungeon/Diagnostics/Validate Current Map and Characters")]
    public static void ValidateCurrentGame()
    {
        if (!CanValidateCurrentGame())
        {
            Debug.Log("ChronoDungeon: check not run. Press Unity Play, select a map in the game menu, " +
                "then press the game's Play button. Run this diagnostic after GameScene opens.");
            return;
        }

        int selected = MapSelectionRuntime.SelectedMap;
        if (selected > 0)
        {
            MapRuntimeCatalog catalog = Resources.Load<MapRuntimeCatalog>("MapRuntimeCatalog");
            Require(catalog != null && catalog.modularMaps != null && selected < catalog.modularMaps.Length,
                "Selected map catalog entry is missing.");
            ModularMapDefinition definition = catalog.modularMaps[selected];
            Require(definition != null && GameObject.Find(definition.zoneName + " Modular Runtime") != null,
                "The selected modular map has not been loaded.");
            Require(GameObject.Find("Map01_RuinedEntrance") == null, "Map 1 is still active over the selected map.");
        }

        PlayerMovement player = Object.FindAnyObjectByType<PlayerMovement>();
        Require(player != null, "No active player.");
        Transform visual = player.transform.Find("PlayerVisual");
        SpriteRenderer renderer = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
        Require(renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy &&
            renderer.sprite != null && renderer.color.a > .01f && renderer.bounds.size.y > .01f,
            "Player body is disabled, transparent, empty, or zero-sized.");
        Require(Camera.main != null && GeometryUtility.TestPlanesAABB(
            GeometryUtility.CalculateFrustumPlanes(Camera.main), renderer.bounds),
            "Player body is outside the camera view.");

        int enemies = 0;
        foreach (EnemyFollow enemy in Object.FindObjectsByType<EnemyFollow>())
        {
            enemies++;
            EnemyAnimationDriver driver = enemy.GetComponent<EnemyAnimationDriver>();
            Animator animator = enemy.GetComponent<Animator>();
            Require(driver != null && driver.enabled && animator != null && animator.enabled &&
                animator.runtimeAnimatorController != null, "Enemy animation is not connected: " + enemy.name);
            foreach (string direction in new[] { "Down", "Left", "Right", "Up" })
                Require(animator.HasState(0, Animator.StringToHash("Attack_" + direction)),
                    "Missing Attack_" + direction + " on " + enemy.name);
        }
        Debug.Log("ChronoDungeon diagnostics PASS: map " + (selected + 1) +
            ", visible player sprite " + renderer.sprite.name + ", " + enemies +
            " active enemies with attack states. This checks wiring; observe an actual attack separately.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.InvalidOperationException("ChronoDungeon diagnostics FAIL: " + message);
    }
}
#endif
