#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

[InitializeOnLoad]
public static class CharacterAnimatorPipeline
{
    private const string SourceRoot = "Assets/ArtSource/Characters";
    private const string OutputRoot = "Assets/Generated/CharacterAnimations";
    private const string CatalogPath = "Assets/Resources/CharacterAnimationCatalog.asset";
    private const string PipelineVersionKey = "ChronoDungeon.CharacterAnimatorPipeline.v5";
    private const string PipelineSessionKey = "ChronoDungeon.CharacterAnimatorPipeline.v5.Running";
    private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };
    private static readonly string[] RequiredStates =
    {
        "Idle", "Move", "Patrol", "Chase", "Windup", "Attack", "Hit", "Stagger",
        "Knockback", "Recover", "Death"
    };

    private sealed class Sheet
    {
        public string Path;
        public string Action;
        public Sprite[][] Frames;
    }

    static CharacterAnimatorPipeline()
    {
        if (Application.isBatchMode || EditorPrefs.GetBool(PipelineVersionKey, false) ||
            SessionState.GetBool(PipelineSessionKey, false)) return;
        EditorApplication.delayCall += RebuildWhenEditorReady;
    }

    private static void RebuildWhenEditorReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += RebuildWhenEditorReady;
            return;
        }
        SessionState.SetBool(PipelineSessionKey, true);
        try
        {
            RebuildAll();
            EditorPrefs.SetBool(PipelineVersionKey, true);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/ChronoDungeon/Animation/Rebuild All Character Animators")]
    public static void RebuildAll()
    {
        EnsureFolder(OutputRoot);
        EnsureFolder("Assets/Resources");
        List<CharacterAnimationProfile> profiles = new List<CharacterAnimationProfile>();
        foreach (string folder in FindCharacterFolders())
        {
            CharacterAnimationProfile profile = BuildCharacter(folder);
            if (profile != null) profiles.Add(profile);
        }
        CharacterAnimationCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterAnimationCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<CharacterAnimationCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        SerializedObject serialized = new SerializedObject(catalog);
        SerializedProperty entries = serialized.FindProperty("profiles");
        entries.arraySize = profiles.Count;
        for (int i = 0; i < profiles.Count; i++) entries.GetArrayElementAtIndex(i).objectReferenceValue = profiles[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"ChronoDungeon: rebuilt {profiles.Count} character animation profiles.");
        ValidateAll();
    }

    [MenuItem("Tools/ChronoDungeon/Animation/Validate Character Animators")]
    public static void ValidateAll()
    {
        List<string> errors = new List<string>();
        string[] folders = FindCharacterFolders().ToArray();
        CharacterAnimationCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterAnimationCatalog>(CatalogPath);
        if (catalog == null) errors.Add("Missing CharacterAnimationCatalog.asset");
        else if (catalog.Profiles == null || catalog.Profiles.Length != folders.Length)
            errors.Add($"Catalog has {catalog.Profiles?.Length ?? 0} profiles; expected {folders.Length}.");

        foreach (string folder in folders)
        {
            string id = CharacterId(folder);
            CharacterAnimationProfile profile = catalog != null ? catalog.Find(id) : null;
            if (profile == null) { errors.Add(id + ": missing profile"); continue; }
            AnimatorController controller = profile.Controller as AnimatorController;
            if (controller == null) { errors.Add(id + ": missing AnimatorController"); continue; }
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (string state in RequiredStates)
            foreach (string direction in Directions)
                if (!machine.states.Any(s => s.state.name == state + "_" + direction))
                    errors.Add(id + ": missing " + state + "_" + direction);
        }

        if (errors.Count > 0)
            throw new InvalidOperationException("Character animation validation failed:\n" + string.Join("\n", errors));
        Debug.Log($"ChronoDungeon: character animation validation passed for {folders.Length} characters.");
    }

    [MenuItem("Tools/ChronoDungeon/Animation/Audit Production 3-4 Coverage")]
    public static void AuditProductionCoverage()
    {
        string[] folders = FindCharacterFolders().ToArray();
        List<string> errors = new List<string>();
        ValidateCategoryCount(folders, "/Player/", 4, "Player", errors);
        ValidateCategoryCount(folders, "/Enemies/", 20, "Enemies", errors);
        ValidateCategoryCount(folders, "/Bosses/", 8, "Bosses", errors);
        ValidateCategoryCount(folders, "/NPC/", 6, "NPC", errors);

        foreach (string folder in folders)
        {
            // Legacy sheets remain valid production sources.  A reviewed TopDownV2 sheet
            // wins per action during import, but characters that still use the legacy
            // art must not be reported as broken merely because they have no duplicate.
            HashSet<string> actions = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories)
                .Where(p => p.Contains("_4Dir_4Frame_", StringComparison.OrdinalIgnoreCase))
                .Select(p => NormalizeAction(ExtractAction(p) ?? string.Empty))
                .Where(a => !string.IsNullOrEmpty(a))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            bool isNpc = ToAssetPath(folder).IndexOf("/NPC/", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isNpc)
            {
                if (!actions.Any(a => a.Equals("Idle", StringComparison.OrdinalIgnoreCase) ||
                                     a.Equals("IdleTalk", StringComparison.OrdinalIgnoreCase) ||
                                     a.Equals("Chained", StringComparison.OrdinalIgnoreCase) ||
                                     a.Equals("Freed", StringComparison.OrdinalIgnoreCase)))
                    errors.Add(CharacterId(folder) + ": missing an NPC idle/presentation state");
                if (!actions.Any(a => a.IndexOf("Talk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      a.IndexOf("Interact", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      a.Equals("Freed", StringComparison.OrdinalIgnoreCase)))
                    errors.Add(CharacterId(folder) + ": missing an NPC interaction state");
                continue;
            }

            foreach (string required in new[] { "Idle", "Move", "Death" })
                if (!actions.Contains(required)) errors.Add(CharacterId(folder) + ": missing " + required);
            if (!actions.Any(a => a.Equals("Hit", StringComparison.OrdinalIgnoreCase) ||
                                  a.Equals("Stagger", StringComparison.OrdinalIgnoreCase) ||
                                  a.Equals("Knockback", StringComparison.OrdinalIgnoreCase)))
                errors.Add(CharacterId(folder) + ": missing hit reaction (Hit/Stagger/Knockback)");
            if (!actions.Any(IsOffensiveAction))
                errors.Add(CharacterId(folder) + ": missing a real offensive action");
        }

        if (errors.Count > 0)
            throw new InvalidOperationException("Production 3/4 animation coverage is incomplete (" +
                errors.Count + " issue(s)):\n" + string.Join("\n", errors));
        Debug.Log("ChronoDungeon: production 3/4 coverage passed for all 38 characters.");
    }

    private static void ValidateCategoryCount(string[] folders, string segment, int expected,
        string label, List<string> errors)
    {
        int actual = folders.Count(f => ToAssetPath(f).IndexOf(segment, StringComparison.OrdinalIgnoreCase) >= 0);
        if (actual != expected) errors.Add($"{label}: found {actual} character folders; expected {expected}.");
    }

    private static CharacterAnimationProfile BuildCharacter(string folder)
    {
        // A character may keep legacy sheets at its root while reviewed 3/4 replacements
        // live in TopDownV2. Pick one authoritative sheet per action before importing;
        // otherwise a legacy Idle can silently win because filesystem order is undefined.
        string[] sourcePaths = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories)
            .Select(ToAssetPath)
            .Where(p => p.Contains("_4Dir_4Frame_", StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => NormalizeAction(ExtractAction(p) ?? string.Empty), StringComparer.OrdinalIgnoreCase)
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .Select(g => g.OrderByDescending(SourcePreference).ThenByDescending(p => p, StringComparer.OrdinalIgnoreCase).First())
            .ToArray();
        if (sourcePaths.Length == 0) return null;
        string id = CharacterId(folder);
        string output = OutputRoot + "/" + id;
        EnsureFolder(output);
        List<Sheet> sheets = new List<Sheet>();
        foreach (string path in sourcePaths)
        {
            string action = ExtractAction(path);
            if (string.IsNullOrEmpty(action)) continue;
            sheets.Add(new Sheet { Path = path, Action = NormalizeAction(action), Frames = SliceAndLoad(path, id, NormalizeAction(action), DesiredHeight(folder)) });
        }
        if (sheets.Count == 0) return null;

        AnimatorController controller = RebuildController(output, id, sheets);
        string profilePath = output + "/" + id + "_Profile.asset";
        CharacterAnimationProfile profile = AssetDatabase.LoadAssetAtPath<CharacterAnimationProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<CharacterAnimationProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        SerializedObject serialized = new SerializedObject(profile);
        serialized.FindProperty("characterId").stringValue = id;
        serialized.FindProperty("controller").objectReferenceValue = controller;
        serialized.FindProperty("hasIndependentLeftFrames").boolValue = true;
        serialized.FindProperty("supportsEightDirections").boolValue = false;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static AnimatorController RebuildController(string output, string id, List<Sheet> sheets)
    {
        string path = output + "/" + id + ".controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states) machine.RemoveState(child.state);

        Dictionary<string, Sheet> byAction = sheets.GroupBy(s => s.Action).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        foreach (Sheet sheet in sheets.GroupBy(s => s.Action).Select(g => g.First()))
            AddDirectionalStates(machine, output, id, sheet.Action, sheet, IsLoop(sheet.Action));

        foreach (string required in RequiredStates)
        {
            if (byAction.ContainsKey(required)) continue;
            Sheet source = ChooseFallback(required, byAction);
            AddDirectionalStates(machine, output, id, required, source, IsLoop(required));
        }
        AnimatorState defaultState = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Idle_Down");
        if (defaultState != null) machine.defaultState = defaultState;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void AddDirectionalStates(AnimatorStateMachine machine, string output, string id,
        string stateName, Sheet source, bool loop)
    {
        for (int d = 0; d < Directions.Length; d++)
        {
            string fullName = stateName + "_" + Directions[d];
            AnimationClip clip = BuildClip(output, id, fullName, source.Frames[d], loop, FrameRate(stateName));
            AnimatorState state = machine.AddState(fullName);
            state.motion = clip;
        }
    }

    private static AnimationClip BuildClip(string output, string id, string stateName, Sprite[] frames, bool loop, float frameRate)
    {
        string path = output + "/" + id + "_" + stateName + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
        clip.frameRate = frameRate;
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip,
            new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys);
        string action = stateName.Substring(0, stateName.LastIndexOf('_'));
        float duration = Mathf.Max(1f / frameRate, frames.Length / frameRate);
        List<AnimationEvent> events = new List<AnimationEvent>();
        if (action == "Attack" || IsOffensiveAction(action))
            events.Add(new AnimationEvent { functionName = "OnAnimationImpact", time = duration * .56f });
        if (action.IndexOf("Cast", StringComparison.OrdinalIgnoreCase) >= 0 ||
            action.IndexOf("Bolt", StringComparison.OrdinalIgnoreCase) >= 0 ||
            action.IndexOf("Volley", StringComparison.OrdinalIgnoreCase) >= 0 ||
            action.IndexOf("Shot", StringComparison.OrdinalIgnoreCase) >= 0 ||
            action.IndexOf("Throw", StringComparison.OrdinalIgnoreCase) >= 0)
            events.Add(new AnimationEvent { functionName = "OnAnimationCastRelease", time = duration * .58f });
        if (action == "Move" || action == "Patrol" || action == "Chase")
        {
            events.Add(new AnimationEvent { functionName = "OnAnimationFootstep", time = duration * .25f });
            events.Add(new AnimationEvent { functionName = "OnAnimationFootstep", time = duration * .75f });
        }
        if (!loop)
            events.Add(new AnimationEvent { functionName = "OnAnimationCompleted", time = duration * .96f });
        AnimationUtility.SetAnimationEvents(clip, events.ToArray());
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static Sprite[][] SliceAndLoad(string path, string id, string action, float desiredWorldHeight)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (importer == null || texture == null) throw new InvalidOperationException("Cannot import " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        // Generated transparent sheets may be tightly cropped to dimensions that are
        // not divisible by four. Integer cells avoid sub-pixel SpriteRects and scale
        // shimmer; only the final 0-3 transparent border pixels remain unused.
        int cellWidth = texture.width / 4;
        int cellHeight = texture.height / 4;
        if (cellWidth <= 0 || cellHeight <= 0)
            throw new InvalidOperationException("Spritesheet is too small for a 4x4 grid: " + path);
        importer.spritePixelsPerUnit = cellHeight / desiredWorldHeight;
        SpriteRect[] rects = new SpriteRect[16];
        for (int d = 0; d < 4; d++)
        for (int f = 0; f < 4; f++)
            rects[d * 4 + f] = new SpriteRect
            {
                name = id + "_" + action + "_" + Directions[d] + "_" + f,
                rect = new Rect(f * cellWidth, (3 - d) * cellHeight, cellWidth, cellHeight),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(.5f, .08f),
                spriteID = GUID.Generate()
            };
        SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        Sprite[][] result = new Sprite[4][];
        for (int d = 0; d < 4; d++)
        {
            result[d] = new Sprite[4];
            for (int f = 0; f < 4; f++)
            {
                string name = id + "_" + action + "_" + Directions[d] + "_" + f;
                result[d][f] = assets.OfType<Sprite>().FirstOrDefault(s => s.name == name);
                if (result[d][f] == null) throw new InvalidOperationException("Missing sliced sprite " + name);
            }
        }
        return result;
    }

    private static Sheet ChooseFallback(string required, Dictionary<string, Sheet> sheets)
    {
        string[] choices = required switch
        {
            "Move" or "Patrol" or "Chase" => new[] { "Move", "MirrorBlink", "TemporalDash", "Idle" },
            "Windup" or "Recover" => new[] { "Attack", "Cast", "Idle" },
            "Hit" => new[] { "Hit", "Stagger", "Knockback", "Idle" },
            "Stagger" or "Knockback" => new[] { "Hit", "Stagger", "Knockback", "Idle" },
            "Attack" => new[] { "Attack", "Cast", "Idle" },
            "Death" => new[] { "Death", "Hit", "Idle" },
            _ => new[] { required, "Idle", "Move" }
        };
        foreach (string choice in choices) if (sheets.TryGetValue(choice, out Sheet sheet)) return sheet;
        if (required == "Attack" || required == "Windup" || required == "Recover")
        {
            Sheet offensive = sheets.FirstOrDefault(pair => IsOffensiveAction(pair.Key)).Value;
            if (offensive != null) return offensive;
        }
        return sheets.Values.First();
    }

    private static IEnumerable<string> FindCharacterFolders()
    {
        string absoluteRoot = Path.GetFullPath(SourceRoot);
        return Directory.GetDirectories(absoluteRoot, "*", SearchOption.AllDirectories)
            // Versioned sheet folders belong to their parent character; they are not
            // independent catalog entries.
            .Where(d => !new DirectoryInfo(d).Name.Equals("TopDownV2", StringComparison.OrdinalIgnoreCase))
            .Where(d => Directory.GetFiles(d, "*.png", SearchOption.TopDirectoryOnly)
                .Any(p => p.Contains("_4Dir_4Frame_", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase);
    }

    private static string CharacterId(string folder)
    {
        string category = new DirectoryInfo(folder).Parent?.Parent?.Name == "Characters"
            ? new DirectoryInfo(folder).Parent.Name
            : new DirectoryInfo(folder).Parent?.Parent?.Name;
        string map = new DirectoryInfo(folder).Parent?.Name;
        string name = new DirectoryInfo(folder).Name;
        return (category == "Player" ? "Player_" : category + "_" + map + "_") + name;
    }

    private static string ExtractAction(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        int marker = name.IndexOf("_4Dir_4Frame_", StringComparison.OrdinalIgnoreCase);
        if (marker < 0) return null;
        string prefix = name.Substring(0, marker);
        int split = prefix.LastIndexOf('_');
        return split >= 0 ? prefix.Substring(split + 1) : null;
    }

    private static string NormalizeAction(string action)
    {
        if (action.Equals("Walk", StringComparison.OrdinalIgnoreCase) ||
            action.Equals("Hover", StringComparison.OrdinalIgnoreCase)) return "Move";
        if (action.Equals("Attack", StringComparison.OrdinalIgnoreCase) ||
            action.Equals("Slash", StringComparison.OrdinalIgnoreCase) ||
            action.Equals("Punch", StringComparison.OrdinalIgnoreCase)) return "Attack";
        return action;
    }

    private static int SourcePreference(string path)
    {
        int score = path.IndexOf("/TopDownV2/", StringComparison.OrdinalIgnoreCase) >= 0 ? 1000 : 0;
        string fileName = Path.GetFileNameWithoutExtension(path);
        int versionMarker = fileName.LastIndexOf("_v", StringComparison.OrdinalIgnoreCase);
        if (versionMarker >= 0 && int.TryParse(fileName.Substring(versionMarker + 2), out int version))
            score += Mathf.Clamp(version, 0, 999);
        return score;
    }

    private static bool IsOffensiveAction(string action)
    {
        string[] tokens = { "Attack", "Slash", "Slam", "Bash", "Shot", "Bolt", "Volley", "Cleave", "Burst", "Throw", "Lash", "Nova", "Beam", "Pull", "Bind" };
        return tokens.Any(token => action.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static bool IsLoop(string state) => state == "Idle" || state == "Move" || state == "Patrol" || state == "Chase";
    private static float FrameRate(string state) => state == "Idle" ? 6f : state == "Move" || state == "Patrol" || state == "Chase" ? 9f : 11f;
    private static float DesiredHeight(string folder) => folder.Contains("Bosses") ? 3.4f : folder.Contains("Player") || folder.Contains("NPC") ? 2.2f : 2.5f;
    private static string ToAssetPath(string path) => path.Replace('\\', '/').Substring(Path.GetFullPath(".").Replace('\\', '/').Length + 1);

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
