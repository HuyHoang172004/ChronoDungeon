#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>Runs the requested modular-map rebuild once after the current Unity editor compiles.</summary>
[InitializeOnLoad]
public static class Map01ModularAutoRebuild
{
    private const string SessionKey = "ChronoDungeon.Map01ActorAndEnemyAiRebuild.20261009.v4";

    static Map01ModularAutoRebuild()
    {
        if (Application.isBatchMode || SessionState.GetBool(SessionKey, false)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.isPlaying = false;
            return;
        }
        Schedule();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        Schedule();
    }

    private static void Schedule()
    {
        SessionState.SetBool(SessionKey, true);
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        try
        {
            Map01ApprovedPresentationBuilder.Rebuild();
            Debug.Log("ChronoDungeon: automatic modular Map 01 rebuild completed.");
        }
        catch (Exception exception)
        {
            SessionState.SetBool(SessionKey, false);
            Debug.LogException(exception);
        }
    }
}
#endif
