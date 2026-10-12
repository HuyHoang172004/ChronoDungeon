#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SideViewEnemyAnimationBuilder
{
    private sealed class EnemySpec { public string Name; public float Health, Speed, Scale; }
    private sealed class MapSpec { public string Scene; public string[] Roster; }
    private const string Root = "Assets/Art/Enemies/SideView/Roster";
    private static readonly EnemySpec[] Enemies = {
        E("RuinGuardian",180,2,.86f), E("AshHusk",95,3,.82f),
        E("RustArcher",80,2.6f,.8f), E("ForgeKnight",220,1.7f,.9f),
        E("ThornSpitter",125,2.2f,.84f), E("MirrorWisp",105,2.5f,.78f),
        E("CrystalSentinel",190,1.9f,.87f)
    };
    private static readonly MapSpec[] Maps = {
        M("Assets/Scenes/Map01_RuinedEntrance_SideView.unity","RuinGuardian","AshHusk","RustArcher","CrystalSentinel"),
        M("Assets/Scenes/Map02_Grassland_SideView.unity","RustArcher","ThornSpitter","AshHusk","MirrorWisp"),
        M("Assets/Scenes/Map03_Barbarian_SideView.unity","ForgeKnight","AshHusk","RuinGuardian","ThornSpitter"),
        M("Assets/Scenes/Map04_FrozenCrystal_SideView.unity","CrystalSentinel","MirrorWisp","ForgeKnight","RustArcher")
    };

    [MenuItem("Tools/ChronoDungeon/Characters/Build Seven-Enemy Side-View Roster")]
    public static void Build()
    {
        var controllers = new Dictionary<string, AnimatorController>();
        foreach (EnemySpec e in Enemies) controllers[e.Name] = BuildController(e);
        foreach (MapSpec map in Maps) BuildMap(map, controllers);
        EditorSceneManager.OpenScene(Maps[0].Scene, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("ChronoDungeon: built seven shared side-view enemies and placed four enemies in each Map 1-4.");
    }

    private static AnimatorController BuildController(EnemySpec spec)
    {
        string frames = $"{Root}/{spec.Name}/Frames", output = $"{Root}/{spec.Name}/Animations";
        EnsureFolder(output);
        foreach(string state in new[]{"Idle","Run","Attack","Hit","Death"})
            SideViewSpriteCalibration.Calibrate(Enumerable.Range(0,4).Select(i=>$"{frames}/{state}/{spec.Name}_{state}_{i:00}.png").ToArray(),2f);
        Sprite[] idle=Load(frames,spec.Name,"Idle",0,4), run=Load(frames,spec.Name,"Run",0,4);
        Sprite[] attack=Load(frames,spec.Name,"Attack",0,4), hit=Load(frames,spec.Name,"Hit",0,4);
        Sprite[] death=Load(frames,spec.Name,"Death",0,4);
        if (new[]{idle.Length,run.Length,attack.Length,hit.Length,death.Length}.Sum()!=20)
            throw new InvalidOperationException(spec.Name+": incomplete frames");
        string path=$"{output}/{spec.Name}_SideView.controller"; AssetDatabase.DeleteAsset(path);
        AnimatorController c=AnimatorController.CreateAnimatorControllerAtPath(path);
        c.AddParameter("Speed",AnimatorControllerParameterType.Float);
        c.AddParameter("FacingRight",AnimatorControllerParameterType.Bool);
        c.AddParameter("Attacking",AnimatorControllerParameterType.Bool);
        c.AddParameter("Hit",AnimatorControllerParameterType.Trigger);
        c.AddParameter("Dead",AnimatorControllerParameterType.Bool);
        AnimatorStateMachine m=c.layers[0].stateMachine;
        AnimatorState ir=S(m,"IdleRight",Clip(output,spec.Name,"IdleRight",idle,7,false,true),200,0);
        AnimatorState rr=S(m,"RunRight",Clip(output,spec.Name,"RunRight",run,10,false,true),420,0);
        AnimatorState ar=S(m,"AttackRight",Clip(output,spec.Name,"AttackRight",attack,12,false,false),640,0);
        AnimatorState hr=S(m,"HitRight",Clip(output,spec.Name,"HitRight",hit,10,false,false),860,0);
        AnimatorState dr=S(m,"DeathRight",Clip(output,spec.Name,"DeathRight",death,7,false,false),1080,0);
        AnimatorState il=S(m,"IdleLeft",Clip(output,spec.Name,"IdleLeft",idle,7,true,true),200,190);
        AnimatorState rl=S(m,"RunLeft",Clip(output,spec.Name,"RunLeft",run,10,true,true),420,190);
        AnimatorState al=S(m,"AttackLeft",Clip(output,spec.Name,"AttackLeft",attack,12,true,false),640,190);
        AnimatorState hl=S(m,"HitLeft",Clip(output,spec.Name,"HitLeft",hit,10,true,false),860,190);
        AnimatorState dl=S(m,"DeathLeft",Clip(output,spec.Name,"DeathLeft",death,7,true,false),1080,190);
        m.defaultState=ir;
        Move(ir,rr,true); Move(rr,ir,false); Move(il,rl,true); Move(rl,il,false);
        Face(ir,il,false); Face(rr,rl,false); Face(il,ir,true); Face(rl,rr,true);
        Any(m,ar,"Attacking",true); Any(m,al,"Attacking",false);
        Any(m,hr,"Hit",true); Any(m,hl,"Hit",false);
        Any(m,dr,"Dead",true); Any(m,dl,"Dead",false);
        Exit(ar,ir); Exit(al,il); Exit(hr,ir); Exit(hl,il);
        return c;
    }

    private static void BuildMap(MapSpec map, Dictionary<string,AnimatorController> controllers)
    {
        var scene=EditorSceneManager.OpenScene(map.Scene,OpenSceneMode.Single);
        GameObject old=GameObject.Find("SideViewEnemyRoster"); if(old!=null) UnityEngine.Object.DestroyImmediate(old);
        GameObject legacy=GameObject.Find("RuinGuardian_SideView");
        if(legacy!=null && legacy.GetComponent<SideViewEnemyAnimator>()!=null) UnityEngine.Object.DestroyImmediate(legacy);
        GameObject root=new GameObject("SideViewEnemyRoster"); float[] xs={-55,-18,20,55};
        for(int i=0;i<4;i++) {
            EnemySpec spec=Enemies.First(e=>e.Name==map.Roster[i]);
            CreateEnemy(root.transform,spec,controllers[spec.Name],xs[i]);
        }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }

    private static void CreateEnemy(Transform parent, EnemySpec spec, AnimatorController controller, float x)
    {
        Sprite idle=AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/{spec.Name}/Frames/Idle/{spec.Name}_Idle_00.png");
        GameObject go=new GameObject(spec.Name); go.transform.SetParent(parent,false);
        go.transform.localScale=Vector3.one*spec.Scale;
        go.transform.position=new Vector3(x,idle.bounds.extents.y*spec.Scale+.04f,0);
        int layer=LayerMask.NameToLayer("Enemy"); go.layer=layer>=0?layer:0;
        SpriteRenderer sr=go.AddComponent<SpriteRenderer>(); sr.sprite=idle; sr.sortingOrder=12;
        Rigidbody2D rb=go.AddComponent<Rigidbody2D>(); rb.gravityScale=3; rb.freezeRotation=true;
        BoxCollider2D box=go.AddComponent<BoxCollider2D>();
        SideViewSpriteCalibration.FitBody(go,1.85f,.75f);
        Health hp=go.AddComponent<Health>(); hp.SetMaximumHealth(spec.Health,true);
        go.AddComponent<SideViewWorldHealthBar>();
        go.AddComponent<EnemyAttackTarget>();
        EnemyFollow follow=go.AddComponent<EnemyFollow>(); GameObject player=GameObject.FindWithTag("Player");
        if(player!=null) follow.player=player.transform; follow.moveSpeed=spec.Speed; follow.detectionRange=22;
        SerializedObject followData=new SerializedObject(follow);
        followData.FindProperty("stoppingDistance").floatValue=.8f; followData.ApplyModifiedPropertiesWithoutUndo();
        go.AddComponent<EnemyContactDamage>();
        Animator animator=go.AddComponent<Animator>(); animator.runtimeAnimatorController=controller;
        go.AddComponent<SideViewEnemyAnimator>();
        go.AddComponent<SideViewEnemyGrounding>();
    }

    private static void Prepare(string folder) {
        foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{folder}).Select(AssetDatabase.GUIDToAssetPath)) {
            TextureImporter i=AssetImporter.GetAtPath(path) as TextureImporter; if(i==null) continue;
            i.textureType=TextureImporterType.Sprite; i.spriteImportMode=SpriteImportMode.Single;
            i.spritePixelsPerUnit=100; i.alphaIsTransparency=true; i.mipmapEnabled=false;
            i.textureCompression=TextureImporterCompression.Uncompressed; i.SaveAndReimport();
        }
    }
    private static Sprite[] Load(string root,string enemy,string state,int start,int count)=>Enumerable.Range(start,count)
        .Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>($"{root}/{state}/{enemy}_{state}_{i:00}.png")).Where(s=>s!=null).ToArray();
    private static AnimationClip Clip(string output,string enemy,string state,Sprite[] sprites,float fps,bool flip,bool loop) {
        string path=$"{output}/{enemy}_{state}.anim"; AssetDatabase.DeleteAsset(path);
        var clip=new AnimationClip{name=enemy+"_"+state,frameRate=fps};
        var keys=sprites.Select((s,i)=>new ObjectReferenceKeyframe{time=i/fps,value=s}).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
        AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(SpriteRenderer),"m_FlipX"),AnimationCurve.Constant(0,sprites.Length/fps,flip?1:0));
        AnimationUtility.SetAnimationClipSettings(clip,new AnimationClipSettings{loopTime=loop}); AssetDatabase.CreateAsset(clip,path); return clip;
    }
    private static AnimatorState S(AnimatorStateMachine m,string n,Motion clip,float x,float y){var s=m.AddState(n,new Vector3(x,y));s.motion=clip;return s;}
    private static void Move(AnimatorState a,AnimatorState b,bool moving){var t=a.AddTransition(b);t.hasExitTime=false;t.duration=.05f;t.AddCondition(moving?AnimatorConditionMode.Greater:AnimatorConditionMode.Less,.05f,"Speed");}
    private static void Face(AnimatorState a,AnimatorState b,bool right){var t=a.AddTransition(b);t.hasExitTime=false;t.duration=.03f;t.AddCondition(right?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"FacingRight");}
    private static void Any(AnimatorStateMachine m,AnimatorState s,string p,bool right){var t=m.AddAnyStateTransition(s);t.hasExitTime=false;t.canTransitionToSelf=false;t.duration=0;t.AddCondition(AnimatorConditionMode.If,0,p);if(p!="Dead")t.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");t.AddCondition(right?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"FacingRight");}
    private static void Exit(AnimatorState a,AnimatorState b){var t=a.AddTransition(b);t.hasExitTime=true;t.exitTime=.95f;t.duration=.05f;}
    private static EnemySpec E(string n,float hp,float speed,float scale)=>new EnemySpec{Name=n,Health=hp,Speed=speed,Scale=scale};
    private static MapSpec M(string scene,params string[] roster)=>new MapSpec{Scene=scene,Roster=roster};
    private static void EnsureFolder(string path){if(AssetDatabase.IsValidFolder(path))return;string parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');EnsureFolder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
}
#endif
