#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SideViewBackgroundPresentationBuilder
{
    private static readonly string[] Scenes = {
        "Assets/Scenes/Map01_RuinedEntrance_SideView.unity",
        "Assets/Scenes/Map02_Grassland_SideView.unity",
        "Assets/Scenes/Map03_Barbarian_SideView.unity",
        "Assets/Scenes/Map04_FrozenCrystal_SideView.unity"
    };

    [MenuItem("Tools/ChronoDungeon/Maps/Apply Generated Backgrounds And Camera")]
    public static void Apply()
    {
        for (int i = 0; i < Scenes.Length; i++)
        {
            string assetPath = $"Assets/ArtSource/Maps/SideViewBackgrounds/Map{i + 1:00}_Background_v02.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();

            var scene = EditorSceneManager.OpenScene(Scenes[i], OpenSceneMode.Single);
            Camera camera = Camera.main;
            if (camera == null) throw new System.InvalidOperationException(Scenes[i] + ": Main Camera missing");
            camera.orthographicSize = 5.4f;

            Transform oldCameraBackground = camera.transform.Find("GeneratedMapBackground");
            if (oldCameraBackground != null) Object.DestroyImmediate(oldCameraBackground.gameObject);
            GameObject oldStrip = GameObject.Find("GeneratedMapBackgroundStrip");
            if (oldStrip != null) Object.DestroyImmediate(oldStrip);

            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            float minX = float.MaxValue, maxX = float.MinValue;
            foreach (Collider2D collider in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (collider.isTrigger) continue;
                string layer = LayerMask.LayerToName(collider.gameObject.layer);
                if (layer != "SideViewGround" && layer != "SideViewOneWay") continue;
                minX = Mathf.Min(minX, collider.bounds.min.x);
                maxX = Mathf.Max(maxX, collider.bounds.max.x);
            }
            if (minX == float.MaxValue) { minX = -90f; maxX = 90f; }

            var strip = new GameObject("GeneratedMapBackgroundStrip");
            float targetHeight = camera.orthographicSize * 2.7f;
            float scale = targetHeight / backgroundSprite.bounds.size.y;
            float panelWidth = backgroundSprite.bounds.size.x * scale;
            float left = minX - panelWidth * .65f;
            int panelCount = Mathf.CeilToInt((maxX - left + panelWidth * .65f) / panelWidth) + 1;
            for (int panelIndex = 0; panelIndex < panelCount; panelIndex++)
            {
                var panel = new GameObject($"BackgroundPanel_{panelIndex:00}");
                panel.transform.SetParent(strip.transform, false);
                panel.transform.position = new Vector3(left + panelWidth * (.5f + panelIndex), 2.2f, 8f);
                panel.transform.localScale = new Vector3(scale * 1.005f, scale, 1f);
                var renderer = panel.AddComponent<SpriteRenderer>();
                renderer.sprite = backgroundSprite;
                renderer.sortingOrder = -1000;
                renderer.flipX = panelIndex % 2 == 1;
                renderer.color = new Color(.72f, .78f, .86f, 1f);
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) throw new System.InvalidOperationException(Scenes[i] + ": Player missing");
            player.transform.localScale = Vector3.one * 2f;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene(Scenes[0], OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("ChronoDungeon: applied four generated backgrounds, player scale 2x, camera view 1.2x.");
    }
}
#endif
