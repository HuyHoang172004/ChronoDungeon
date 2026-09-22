#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class SettingsPlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message) { if (!ok) throw new System.Exception("M9.3 FAIL: " + message); checks++; Debug.Log("M9.3 PASS: " + message); }
    private IEnumerator Start()
    {
        yield return null; var settings=FindAnyObjectByType<SettingsManager>(); var ui=FindAnyObjectByType<SettingsFlowUI>();
        Check(settings!=null && ui!=null, "settings manager and UI are available");
        settings.SetMasterVolume(.4f); settings.SetMusicVolume(.6f); settings.SetSfxVolume(.8f);
        Check(Mathf.Approximately(settings.MasterVolume,.4f) && Mathf.Approximately(AudioListener.volume,.4f), "master volume applies immediately");
        Check(Mathf.Approximately(settings.MusicVolume,.6f) && Mathf.Approximately(settings.SfxVolume,.8f), "music and SFX values apply");
        ui.Show(); Check(ui.IsVisible, "settings panel can open"); ui.Hide(); Check(!ui.IsVisible, "settings panel can close");
        settings.ResetDefaults(); Check(Mathf.Approximately(settings.MasterVolume,1f) && Mathf.Approximately(AudioListener.volume,1f), "reset defaults restores audio values");
        Debug.Log("M9.3 ALL CHECKS PASSED; checks="+checks); Destroy(gameObject);
    }
    private void OnDestroy()=>Time.timeScale=1f;
}
#endif
