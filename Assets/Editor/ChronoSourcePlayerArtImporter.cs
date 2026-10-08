#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// Imports the approved 4x4 source sheets as consistently named Sprite sub-assets and
/// wires the Base/Chrono Blade visual sets without touching gameplay transforms.
/// Rows in every approved sheet are Down, Left, Right, Up (top to bottom).
/// </summary>
public static class ChronoSourcePlayerArtImporter
{
    private const string BaseRoot = "Assets/ArtSource/Characters/Player/Base/";
    private const string BladeRoot = "Assets/ArtSource/Characters/Player/ChronoBlade/";
    private const string VisualRoot = "Assets/Data/PlayerVisuals/";
    // Source sheets are high-resolution four-direction contact sheets. 420 PPU preserves
    // the approved in-world body size once Player root physics scale is applied.
    private const float PlayerPixelsPerUnit = 420f;
    // Approved foot baseline: sprites render from their feet, never from the body centre.
    private const float FootPivotY = 0.08f;

    [MenuItem("Tools/ChronoDungeon/Art/Rebuild Approved Player Visual Sets")]
    public static void RebuildApprovedPlayerVisualSets()
    {
        Dictionary<string, Sprite[]> baseIdle = SliceAndLoad(BaseRoot + "CD_Player_Base_Idle_4Dir_4Frame_v02.png", "CD_BaseIdle");
        Dictionary<string, Sprite[]> baseWalk = SliceAndLoad(BaseRoot + "CD_Player_Base_Walk_4Dir_4Frame_v02.png", "CD_BaseWalk");
        Dictionary<string, Sprite[]> basePunch = SliceAndLoad(BaseRoot + "CD_Player_Base_Punch_4Dir_4Frame_v02.png", "CD_BasePunch");

        Dictionary<string, Sprite[]> bladeIdle = SliceAndLoad(BladeRoot + "CD_Player_ChronoBlade_Idle_4Dir_4Frame_v01.png", "CD_BladeIdle");
        Dictionary<string, Sprite[]> bladeWalk = SliceAndLoad(BladeRoot + "CD_Player_ChronoBlade_Walk_4Dir_4Frame_v01.png", "CD_BladeWalk");
        Dictionary<string, Sprite[]> bladeSlash = SliceAndLoad(BladeRoot + "CD_Player_ChronoBlade_Slash_4Dir_4Frame_v01.png", "CD_BladeSlash");
        Dictionary<string, Sprite[]> bladeBurst = SliceAndLoad(BladeRoot + "CD_Player_ChronoBlade_ChronoBurst_4Dir_4Frame_v01.png", "CD_BladeBurst");
        Dictionary<string, Sprite[]> bladeCleave = SliceAndLoad(BladeRoot + "CD_Player_ChronoBlade_TimeCleave_4Dir_4Frame_v01.png", "CD_BladeCleave");

        PlayerWeaponVisualSet baseSet = GetOrCreateVisualSet(VisualRoot + "BaseSourceVisualSet.asset");
        AssignVisualSet(baseSet, null, baseIdle, baseWalk, basePunch, null, null);

        WeaponDefinition chronoBlade = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Weapons/ChronoBlade.asset");
        if (chronoBlade == null)
            throw new InvalidOperationException("ChronoBlade.asset was not found.");

        PlayerWeaponVisualSet bladeSet = GetOrCreateVisualSet(VisualRoot + "ChronoBladeSourceVisualSet.asset");
        AssignVisualSet(bladeSet, chronoBlade, bladeIdle, bladeWalk, bladeSlash, bladeBurst, bladeCleave);
        WireCurrentGameScene(baseSet, bladeSet);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ChronoDungeon: approved Base Player and Chrono Blade source sheets were sliced and wired.");
    }

    private static Dictionary<string, Sprite[]> SliceAndLoad(string path, string prefix)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("Could not find TextureImporter for " + path);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = PlayerPixelsPerUnit;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (source == null)
            throw new InvalidOperationException("Could not load texture " + path);

        int width = source.width;
        int height = source.height;
        List<SpriteRect> spriteRects = new List<SpriteRect>(16);
        string[] directions = { "Down", "Left", "Right", "Up" };

        for (int rowFromTop = 0; rowFromTop < 4; rowFromTop++)
        {
            float yMin = height - ((rowFromTop + 1) * height / 4f);
            float yMax = height - (rowFromTop * height / 4f);
            for (int column = 0; column < 4; column++)
            {
                float xMin = column * width / 4f;
                float xMax = (column + 1) * width / 4f;
                spriteRects.Add(new SpriteRect
                {
                    name = prefix + "_" + directions[rowFromTop] + "_" + column,
                    rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax),
                    alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, FootPivotY),
                    spriteID = GUID.Generate()
                });
            }
        }

        SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(source);
        provider.InitSpriteEditorDataProvider();
        provider.SetSpriteRects(spriteRects.ToArray());
        provider.Apply();
        importer.SaveAndReimport();

        Dictionary<string, List<Sprite>> found = new Dictionary<string, List<Sprite>>();
        UnityEngine.Object[] importedAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (UnityEngine.Object asset in importedAssets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite == null) continue;
            foreach (string direction in directions)
            {
                string key = prefix + "_" + direction + "_";
                if (!sprite.name.StartsWith(key, StringComparison.Ordinal)) continue;
                if (!found.TryGetValue(direction, out List<Sprite> frames))
                {
                    frames = new List<Sprite>(4);
                    found.Add(direction, frames);
                }
                frames.Add(sprite);
                break;
            }
        }

        Dictionary<string, Sprite[]> result = new Dictionary<string, Sprite[]>();
        foreach (string direction in directions)
        {
            if (!found.TryGetValue(direction, out List<Sprite> frames) || frames.Count != 4)
            {
                List<string> names = new List<string>();
                foreach (UnityEngine.Object asset in importedAssets)
                    if (asset is Sprite) names.Add(asset.name);
                throw new InvalidOperationException("Expected four " + direction + " frames in " + path + ". Imported sprites: " + string.Join(", ", names));
            }
            frames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            result.Add(direction, frames.ToArray());
        }
        return result;
    }

    private static PlayerWeaponVisualSet GetOrCreateVisualSet(string path)
    {
        PlayerWeaponVisualSet set = AssetDatabase.LoadAssetAtPath<PlayerWeaponVisualSet>(path);
        if (set != null) return set;
        set = ScriptableObject.CreateInstance<PlayerWeaponVisualSet>();
        AssetDatabase.CreateAsset(set, path);
        return set;
    }

    private static void AssignVisualSet(
        PlayerWeaponVisualSet set,
        WeaponDefinition weapon,
        Dictionary<string, Sprite[]> idle,
        Dictionary<string, Sprite[]> walk,
        Dictionary<string, Sprite[]> attack,
        Dictionary<string, Sprite[]> skill1,
        Dictionary<string, Sprite[]> skill2)
    {
        SerializedObject serialized = new SerializedObject(set);
        serialized.FindProperty("weapon").objectReferenceValue = weapon;
        AssignState(serialized, "idle", idle);
        AssignState(serialized, "walk", walk);
        AssignState(serialized, "attack", attack);
        AssignState(serialized, "skill1", skill1);
        AssignState(serialized, "skill2", skill2);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(set);
    }

    private static void AssignState(SerializedObject serialized, string state, Dictionary<string, Sprite[]> frames)
    {
        SetDirection(serialized, state, "Down", frames);
        SetDirection(serialized, state, "Right", frames);
        SetDirection(serialized, state, "Up", frames);
        SetDirection(serialized, state, "Left", frames);
    }

    private static void SetDirection(SerializedObject serialized, string state, string direction, Dictionary<string, Sprite[]> source)
    {
        SerializedProperty sprite = serialized.FindProperty(state + direction);
        SerializedProperty frames = serialized.FindProperty(state + direction + "Frames");
        if (source == null || !source.TryGetValue(direction, out Sprite[] values))
        {
            sprite.objectReferenceValue = null;
            frames.arraySize = 0;
            return;
        }

        sprite.objectReferenceValue = values[0];
        frames.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            frames.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void WireCurrentGameScene(PlayerWeaponVisualSet baseSet, PlayerWeaponVisualSet bladeSet)
    {
        GameObject playerVisual = GameObject.Find("Player/PlayerVisual");
        if (playerVisual == null)
            throw new InvalidOperationException("Player/PlayerVisual was not found in the loaded scene.");

        PlayerWeaponVisualSetController controller = playerVisual.GetComponent<PlayerWeaponVisualSetController>();
        if (controller == null)
            throw new InvalidOperationException("PlayerWeaponVisualSetController is missing from Player/PlayerVisual.");

        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("unarmedVisualSet").objectReferenceValue = baseSet;
        SerializedProperty sets = serialized.FindProperty("visualSets");
        sets.arraySize = 1;
        sets.GetArrayElementAtIndex(0).objectReferenceValue = bladeSet;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(playerVisual.scene);
    }
}
#endif
