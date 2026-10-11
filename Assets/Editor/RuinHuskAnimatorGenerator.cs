#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

[InitializeOnLoad]
public static class RuinHuskAnimatorGenerator
{
    private const string SourceFolder = "Assets/ArtSource/Characters/Enemies/Map01/RuinHusk";
    private const string OutputFolder = "Assets/Resources/EnemyAnimations";
    private const string ControllerPath = OutputFolder + "/RuinHusk.controller";
    private const string SessionKey = "ChronoDungeon.RuinHuskAnimator.v2";
    private static readonly string[] Actions = { "Idle", "Walk", "Attack", "Hit", "Death" };
    private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };

    static RuinHuskAnimatorGenerator()
    {
        if (Application.isBatchMode || SessionState.GetBool(SessionKey, false)) return;
        SessionState.SetBool(SessionKey, true);
        EditorApplication.delayCall += GenerateWhenReady;
    }

    [MenuItem("Tools/ChronoDungeon/Enemies/Rebuild RuinHusk Animator")]
    public static void Generate()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(OutputFolder);
        foreach (string action in Actions) SliceSheet(action);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState state in machine.states) machine.RemoveState(state.state);

        foreach (string action in Actions)
        foreach (string direction in Directions)
        {
            string stateName = action + "_" + direction;
            AnimationClip clip = BuildClip(action, direction, action == "Idle" || action == "Walk");
            AnimatorState state = machine.AddState(stateName);
            state.motion = clip;
            if (action == "Idle" && direction == "Down") machine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("ChronoDungeon: rebuilt RuinHusk 4-direction Animator (Idle/Walk/Attack/Hit/Death).");
    }

    private static void GenerateWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetBool(SessionKey, false);
            return;
        }
        Generate();
    }

    private static void SliceSheet(string action)
    {
        string path = SourceFolder + "/CD_Enemy_RuinHusk_" + action + "_4Dir_4Frame_v01.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePixelsPerUnit = 100f;
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) { importer.SaveAndReimport(); texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); }
        float cellWidth = texture.width / 4f;
        float cellHeight = texture.height / 4f;
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] sprites = new SpriteRect[16];
        for (int direction = 0; direction < 4; direction++)
        for (int frame = 0; frame < 4; frame++)
        {
            int index = direction * 4 + frame;
            sprites[index] = new SpriteRect
            {
                name = action + "_" + Directions[direction] + "_" + frame,
                rect = new Rect(frame * cellWidth, (3 - direction) * cellHeight, cellWidth, cellHeight),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(.5f, .5f),
                spriteID = GUID.Generate()
            };
        }
        provider.SetSpriteRects(sprites);
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static AnimationClip BuildClip(string action, string direction, bool loop)
    {
        string clipPath = OutputFolder + "/RuinHusk_" + action + "_" + direction + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
        clip.frameRate = action == "Attack" ? 10f : 8f;
        string sheetPath = SourceFolder + "/CD_Enemy_RuinHusk_" + action + "_4Dir_4Frame_v01.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[4];
        for (int frame = 0; frame < 4; frame++)
        {
            string spriteName = action + "_" + direction + "_" + frame;
            Sprite sprite = null;
            foreach (Object asset in assets) if (asset is Sprite candidate && candidate.name == spriteName) { sprite = candidate; break; }
            frames[frame] = new ObjectReferenceKeyframe { time = frame / clip.frameRate, value = sprite };
        }
        EditorCurveBinding binding = new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
