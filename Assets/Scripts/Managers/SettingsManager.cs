using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SettingsManager : MonoBehaviour
{
    private const string MasterKey = "ChronoDungeon.MasterVolume";
    private const string MusicKey = "ChronoDungeon.MusicVolume";
    private const string SfxKey = "ChronoDungeon.SfxVolume";
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public event Action SettingsChanged;

    private void Awake()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);
        Apply();
    }

    public void SetMasterVolume(float value) { MasterVolume = Mathf.Clamp01(value); Save(); Apply(); }
    public void SetMusicVolume(float value) { MusicVolume = Mathf.Clamp01(value); Save(); SettingsChanged?.Invoke(); }
    public void SetSfxVolume(float value) { SfxVolume = Mathf.Clamp01(value); Save(); SettingsChanged?.Invoke(); }

    public void Apply() { AudioListener.volume = MasterVolume; SettingsChanged?.Invoke(); }

    public void ResetDefaults()
    {
        MasterVolume = MusicVolume = SfxVolume = 1f;
        Save(); Apply();
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        PlayerPrefs.Save();
    }
}
