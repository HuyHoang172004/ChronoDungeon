#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SideViewPlayModeStartScene
{
    static SideViewPlayModeStartScene()
    {
        EditorApplication.delayCall += () =>
        {
            SceneAsset menu = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenuScene.unity");
            if (menu != null) EditorSceneManager.playModeStartScene = menu;
        };
    }
}
#endif
