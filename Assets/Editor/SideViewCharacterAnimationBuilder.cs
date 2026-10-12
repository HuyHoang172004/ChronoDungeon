#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SideViewCharacterAnimationBuilder
{
    private const string IdleSheet = "Assets/Art/Characters/Player/Generated/Player_Idle_Right_v5.png";
    private const string RunSheet = "Assets/Art/Characters/Player/Generated/Player_Walk_Right_v2.png";
    private const string CombatAtlas = "Assets/ArtSource/Characters/Player/SideViewCombat/Player_Combat_StateAtlas_v02.png";
    private const string KickSheet = "Assets/ArtSource/Characters/Player/SideViewCombat/Player_Kick_Right_v01.png";
    private const string Output = "Assets/Art/Characters/Player/SideView/Animations";
    private const string CombatFrames = "Assets/Art/Characters/Player/SideView/CombatFrames";
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/Map01_RuinedEntrance_SideView.unity",
        "Assets/Scenes/Map02_Grassland_SideView.unity",
        "Assets/Scenes/Map03_Barbarian_SideView.unity",
        "Assets/Scenes/Map04_FrozenCrystal_SideView.unity"
    };

    [MenuItem("Tools/ChronoDungeon/Characters/Build Player Side-View Locomotion")]
    public static void Build()
    {
        EnsureFolder("Assets/Art/Characters/Player/SideView");
        EnsureFolder(Output);
        const string calibrated = "Assets/Art/Characters/Player/SideView/Calibrated";
        // HitLeft/HitRight were intentionally removed from the player state set.
        AssetDatabase.DeleteAsset(calibrated + "/Hit");
        AssetDatabase.DeleteAsset(Output + "/Player_HitRight.anim");
        AssetDatabase.DeleteAsset(Output + "/Player_HitLeft.anim");
        Sprite[] idle = SideViewSpriteCalibration.ImportSheet(IdleSheet, calibrated+"/Idle",4,1,0,.92f);
        Sprite[] run = SideViewSpriteCalibration.ImportSheet(RunSheet, calibrated+"/Run",4,1,0,.92f);
        // Keep every state on the approved original chibi character. The former
        // combat atlas used different realistic proportions and overlapping cells.
        Sprite[] jump = new[]{run[1]};
        Sprite[] attack = SideViewSpriteCalibration.ImportSheet(CombatAtlas,calibrated+"/Attack",4,4,0,.92f);
        Sprite[] kick = SideViewSpriteCalibration.ImportSheet(KickSheet,calibrated+"/Kick",4,1,0,.92f);
        Sprite[] swordAttack = SideViewSpriteCalibration.ImportSheet(CombatAtlas,calibrated+"/SwordAttack",4,4,1,.92f);
        Sprite[] swordDownAttack = SideViewSpriteCalibration.ImportSheet(CombatAtlas,calibrated+"/SwordDownAttack",4,4,2,.92f);
        Sprite[] death = SideViewSpriteCalibration.ImportSheet(CombatAtlas,calibrated+"/Death",4,4,3,.92f);
        AnimationClip idleRight = CreateClip("Player_IdleRight", idle, 6f, false, true);
        AnimationClip idleLeft = CreateClip("Player_IdleLeft", idle, 6f, true, true);
        AnimationClip runRight = CreateClip("Player_RunRight", run, 10f, false, true);
        AnimationClip runLeft = CreateClip("Player_RunLeft", run, 10f, true, true);
        AnimationClip jumpRight = CreateClip("Player_JumpRight", jump, 12f, false, false);
        AnimationClip jumpLeft = CreateClip("Player_JumpLeft", jump, 12f, true, false);
        AnimationClip attackRight = CreateClip("Player_AttackRight", attack, 14f, false, false);
        AnimationClip attackLeft = CreateClip("Player_AttackLeft", attack, 14f, true, false);
        AnimationClip kickRight = CreateClip("Player_KickRight", kick, 14f, false, false);
        AnimationClip kickLeft = CreateClip("Player_KickLeft", kick, 14f, true, false);
        AnimationClip swordAttackRight = CreateClip("Player_SwordAttackRight", swordAttack, 14f, false, false);
        AnimationClip swordAttackLeft = CreateClip("Player_SwordAttackLeft", swordAttack, 14f, true, false);
        AnimationClip swordDownAttackRight = CreateClip("Player_SwordDownAttackRight", swordDownAttack, 14f, false, false);
        AnimationClip swordDownAttackLeft = CreateClip("Player_SwordDownAttackLeft", swordDownAttack, 14f, true, false);
        AnimationClip deathRight = CreateClip("Player_DeathRight", death, 8f, false, false);
        AnimationClip deathLeft = CreateClip("Player_DeathLeft", death, 8f, true, false);

        string controllerPath = Output + "/Player_SideView.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("HorizontalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("FacingRight", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Kick", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("SwordAttack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("SwordDownAttack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idleR = State(machine, "IdleRight", idleRight, new Vector3(250, 0));
        AnimatorState runR = State(machine, "RunRight", runRight, new Vector3(500, 0));
        AnimatorState idleL = State(machine, "IdleLeft", idleLeft, new Vector3(250, 170));
        AnimatorState runL = State(machine, "RunLeft", runLeft, new Vector3(500, 170));
        AnimatorState jumpR = State(machine, "JumpRight", jumpRight, new Vector3(750, 0));
        AnimatorState jumpL = State(machine, "JumpLeft", jumpLeft, new Vector3(750, 170));
        AnimatorState attackR = State(machine, "AttackRight", attackRight, new Vector3(500, -170));
        AnimatorState attackL = State(machine, "AttackLeft", attackLeft, new Vector3(500, 340));
        AnimatorState kickR = State(machine, "KickRight", kickRight, new Vector3(750, -340));
        AnimatorState kickL = State(machine, "KickLeft", kickLeft, new Vector3(750, 510));
        AnimatorState swordAttackR = State(machine, "SwordAttackRight", swordAttackRight, new Vector3(750, -170));
        AnimatorState swordAttackL = State(machine, "SwordAttackLeft", swordAttackLeft, new Vector3(750, 340));
        AnimatorState swordDownAttackR = State(machine, "SwordDownAttackRight", swordDownAttackRight, new Vector3(1000, -170));
        AnimatorState swordDownAttackL = State(machine, "SwordDownAttackLeft", swordDownAttackLeft, new Vector3(1000, 340));
        AnimatorState deathR = State(machine, "DeathRight", deathRight, new Vector3(1250, -170));
        AnimatorState deathL = State(machine, "DeathLeft", deathLeft, new Vector3(1250, 340));
        machine.defaultState = idleR;
        Add(idleR, runR, "HorizontalSpeed", AnimatorConditionMode.Greater, .05f);
        Add(runR, idleR, "HorizontalSpeed", AnimatorConditionMode.Less, .05f);
        Add(idleL, runL, "HorizontalSpeed", AnimatorConditionMode.Greater, .05f);
        Add(runL, idleL, "HorizontalSpeed", AnimatorConditionMode.Less, .05f);
        Add(idleR, idleL, "FacingRight", AnimatorConditionMode.IfNot, 0f);
        Add(runR, runL, "FacingRight", AnimatorConditionMode.IfNot, 0f);
        Add(idleL, idleR, "FacingRight", AnimatorConditionMode.If, 0f);
        Add(runL, runR, "FacingRight", AnimatorConditionMode.If, 0f);
        AddGroundTransition(machine, jumpR, true);
        AddGroundTransition(machine, jumpL, false);
        Add(jumpR, idleR, "IsGrounded", AnimatorConditionMode.If, 0f);
        Add(jumpL, idleL, "IsGrounded", AnimatorConditionMode.If, 0f);
        AddActionTransition(machine, attackR, "Attack", true);
        AddActionTransition(machine, attackL, "Attack", false);
        AddActionTransition(machine, kickR, "Kick", true);
        AddActionTransition(machine, kickL, "Kick", false);
        AddActionTransition(machine, swordAttackR, "SwordAttack", true);
        AddActionTransition(machine, swordAttackL, "SwordAttack", false);
        AddActionTransition(machine, swordDownAttackR, "SwordDownAttack", true);
        AddActionTransition(machine, swordDownAttackL, "SwordDownAttack", false);
        AddActionTransition(machine, deathR, "Dead", true, true);
        AddActionTransition(machine, deathL, "Dead", false, true);
        AddExit(attackR, idleR);
        AddExit(attackL, idleL);
        AddExit(kickR, idleR);
        AddExit(kickL, idleL);
        AddExit(swordAttackR, idleR);
        AddExit(swordAttackL, idleL);
        AddExit(swordDownAttackR, idleR);
        AddExit(swordDownAttackL, idleL);

        foreach (string scenePath in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject player = null;
            try { player = GameObject.FindWithTag("Player"); } catch (UnityException) { }
            if (player == null) throw new InvalidOperationException(scenePath + ": Player missing.");
            player.transform.localScale = Vector3.one * 2f;
            Animator animator = player.GetComponent<Animator>();
            if (animator == null) animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            if (player.GetComponent<SideViewPlayerAnimator>() == null) player.AddComponent<SideViewPlayerAnimator>();
            SpriteRenderer renderer = player.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null) { renderer.flipX = false; renderer.sprite=idle[0]; }
            animator.applyRootMotion=false;
            SideViewSpriteCalibration.FitBody(player,.86f,.36f);
            Physics2D.SyncTransforms();
            var ground=Physics2D.Raycast(new Vector2(player.transform.position.x,player.transform.position.y+6),Vector2.down,12,LayerMask.GetMask("SideViewGround","SideViewOneWay"));
            if(ground.collider!=null) player.transform.position=new Vector3(player.transform.position.x,ground.point.y+.025f,player.transform.position.z);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene(Scenes[0], OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("ChronoDungeon: rebuilt player with 4-frame unarmed, sword, overhead sword and matching-outfit death states on Maps 1-4.");
    }

    private static Sprite[] LoadFrames(string path)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderBy(sprite => sprite.rect.x).ToArray();
        if (sprites.Length == 0) throw new InvalidOperationException("No frames: " + path);
        return sprites;
    }

    private static Sprite[] LoadFrameSet(string folder, int start, int count)
    {
        return Enumerable.Range(start, count)
            .Select(i => AssetDatabase.LoadAssetAtPath<Sprite>($"{CombatFrames}/{folder}/Player_{folder}_{i:00}.png"))
            .Where(sprite => sprite != null).ToArray();
    }

    private static void PrepareCombatFrames()
    {
        foreach (string path in AssetDatabase.FindAssets("t:Texture2D", new[] { CombatFrames })
                     .Select(AssetDatabase.GUIDToAssetPath))
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }
    }

    private static AnimationClip CreateClip(string name, Sprite[] frames, float fps, bool flipped, bool loop)
    {
        string path = Output + "/" + name + ".anim";
        AssetDatabase.DeleteAsset(path);
        var clip = new AnimationClip { name = name, frameRate = fps };
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var flipCurve = AnimationCurve.Constant(0f, frames.Length / fps, flipped ? 1f : 0f);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_FlipX"), flipCurve);
        AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = loop });
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorState State(AnimatorStateMachine machine, string name, Motion motion, Vector3 position)
    {
        AnimatorState state = machine.AddState(name, position);
        state.motion = motion;
        return state;
    }

    private static void Add(AnimatorState from, AnimatorState to, string parameter, AnimatorConditionMode mode, float threshold)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.canTransitionToSelf = false;
        transition.duration = .06f;
        transition.AddCondition(mode, threshold, parameter);
    }

    private static void AddGroundTransition(AnimatorStateMachine machine, AnimatorState to, bool facingRight)
    {
        AnimatorStateTransition transition = machine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.canTransitionToSelf = false;
        transition.duration = .04f;
        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
        transition.AddCondition(facingRight ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "FacingRight");
    }

    private static void AddActionTransition(AnimatorStateMachine machine, AnimatorState to, string parameter, bool facingRight, bool isBool = false)
    {
        AnimatorStateTransition transition = machine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.canTransitionToSelf = false;
        transition.duration = .03f;
        transition.AddCondition(isBool ? AnimatorConditionMode.If : AnimatorConditionMode.If, 0f, parameter);
        if(parameter!="Dead") transition.AddCondition(AnimatorConditionMode.IfNot,0f,"Dead");
        transition.AddCondition(facingRight ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "FacingRight");
    }

    private static void AddExit(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = .95f;
        transition.duration = .05f;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
#endif
