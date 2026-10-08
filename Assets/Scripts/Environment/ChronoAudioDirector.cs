using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Central lightweight audio routing for the authored run. Clips are short procedural tones,
/// so the milestone remains self-contained and does not add large binary assets to Android.
/// </summary>
public sealed class ChronoAudioDirector : MonoBehaviour
{
    private readonly List<Health> boundHealth = new List<Health>(32);
    private readonly List<PlayerAttack> boundAttacks = new List<PlayerAttack>(4);
    private readonly List<PlayerWeaponController> boundWeapons = new List<PlayerWeaponController>(4);
    private readonly List<GhostPlayback> boundGhosts = new List<GhostPlayback>(3);
    private readonly List<PressureSwitch> boundSwitches = new List<PressureSwitch>(8);
    private readonly List<Door> boundDoors = new List<Door>(8);
    private readonly List<ChronoGuardianPolish> boundBossPolish = new List<ChronoGuardianPolish>(2);
    private readonly List<Button> boundButtons = new List<Button>(24);
    private readonly Dictionary<Health, float> lastHealth = new Dictionary<Health, float>();
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioClip menuMusic;
    private AudioClip dungeonMusic;
    private AudioClip bossMusic;
    private AudioClip attackClip;
    private AudioClip skillClip;
    private AudioClip hitClip;
    private AudioClip deathClip;
    private AudioClip rewindClip;
    private AudioClip ghostClip;
    private AudioClip switchClip;
    private AudioClip doorClip;
    private AudioClip upgradeClip;
    private AudioClip bossClip;
    private AudioClip victoryClip;
    private AudioClip gameOverClip;
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private GameManager gameManager;
    private UpgradeChoiceUI upgradeUI;
    private SettingsManager settings;
    private ChronoGuardian boss;
    private bool bossMusicActive;
    private bool isMenu;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if ((scene.name == "GameScene" || scene.name == "MainMenuScene") &&
            FindAnyObjectByType<ChronoAudioDirector>() == null)
        {
            GameObject root = new GameObject("Chrono Audio Director");
            root.AddComponent<ChronoAudioDirector>();
        }
    }

    private void Awake()
    {
        isMenu = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenuScene";
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        upgradeUI = FindAnyObjectByType<UpgradeChoiceUI>();
        settings = FindAnyObjectByType<SettingsManager>();
        ChronoGuardian[] bosses = FindObjectsByType<ChronoGuardian>(FindObjectsInactive.Include);
        boss = bosses.Length > 0 ? bosses[0] : null;

        CreateClips();
        musicSource = CreateSource("Music", true);
        sfxSource = CreateSource("SFX", false);
        PlayMusic(isMenu ? menuMusic : dungeonMusic);
    }

    private void OnEnable()
    {
        if (loop != null) loop.LoopRewound += OnRewind;
        if (ghosts != null) ghosts.GhostSpawned += OnGhostSpawned;
        if (gameManager != null)
        {
            gameManager.GameOverTriggered += OnGameOver;
            gameManager.VictoryTriggered += OnVictory;
        }
        if (upgradeUI != null) upgradeUI.UpgradeSelected += OnUpgradeSelected;
        if (settings != null) settings.SettingsChanged += RefreshVolume;
        RefreshBindings();
        RefreshVolume();
    }

    private void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            RefreshBindings();
            RefreshBossMusic();
        }
    }

    private void RefreshBossMusic()
    {
        if (isMenu || boss == null) return;
        bool shouldUseBossMusic = boss.gameObject.activeInHierarchy && boss.Health != null && !boss.Health.IsDead;
        if (shouldUseBossMusic == bossMusicActive) return;
        bossMusicActive = shouldUseBossMusic;
        PlayMusic(bossMusicActive ? bossMusic : dungeonMusic);
    }

    private void RefreshBindings()
    {
        BindHealth(FindObjectsByType<Health>(FindObjectsInactive.Include));
        BindAttacks(FindObjectsByType<PlayerAttack>(FindObjectsInactive.Include));
        BindWeapons(FindObjectsByType<PlayerWeaponController>(FindObjectsInactive.Include));
        BindGhosts(FindObjectsByType<GhostPlayback>(FindObjectsInactive.Include));
        BindSwitches(FindObjectsByType<PressureSwitch>(FindObjectsInactive.Include));
        BindDoors(FindObjectsByType<Door>(FindObjectsInactive.Include));
        BindBossPolish(FindObjectsByType<ChronoGuardianPolish>(FindObjectsInactive.Include));
        BindButtons(FindObjectsByType<Button>(FindObjectsInactive.Include));
    }

    private void BindHealth(Health[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            Health value = values[i];
            if (value == null || boundHealth.Contains(value)) continue;
            boundHealth.Add(value);
            lastHealth[value] = value.currentHealth;
            value.Changed += OnHealthChanged;
        }
    }

    private void BindAttacks(PlayerAttack[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            PlayerAttack value = values[i];
            if (value == null || boundAttacks.Contains(value)) continue;
            boundAttacks.Add(value);
            value.AttackPerformed += OnAttack;
        }
    }

    private void BindWeapons(PlayerWeaponController[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            PlayerWeaponController value = values[i];
            if (value == null || boundWeapons.Contains(value)) continue;
            boundWeapons.Add(value);
            value.OnSkillUsed += OnSkillUsed;
        }
    }

    private void BindGhosts(GhostPlayback[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            GhostPlayback value = values[i];
            if (value == null || boundGhosts.Contains(value)) continue;
            boundGhosts.Add(value);
            value.ActionReplayed += OnGhostAction;
        }
    }

    private void BindSwitches(PressureSwitch[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            PressureSwitch value = values[i];
            if (value == null || boundSwitches.Contains(value)) continue;
            boundSwitches.Add(value);
            value.StateChanged += OnSwitchChanged;
        }
    }

    private void BindDoors(Door[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            Door value = values[i];
            if (value == null || boundDoors.Contains(value)) continue;
            boundDoors.Add(value);
            value.StateChanged += OnDoorChanged;
        }
    }

    private void BindBossPolish(ChronoGuardianPolish[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            ChronoGuardianPolish value = values[i];
            if (value == null || boundBossPolish.Contains(value)) continue;
            boundBossPolish.Add(value);
            value.PhaseFeedback += OnBossPhase;
        }
    }

    private void BindButtons(Button[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            Button value = values[i];
            if (value == null || boundButtons.Contains(value)) continue;
            boundButtons.Add(value);
            value.onClick.AddListener(PlayUiClick);
        }
    }

    private void OnHealthChanged(Health health)
    {
        if (health == null) return;
        float previous = lastHealth.TryGetValue(health, out float value) ? value : health.currentHealth;
        lastHealth[health] = health.currentHealth;
        if (health.currentHealth >= previous) return;
        Play(health.IsDead ? deathClip : hitClip);
    }

    private void OnAttack(AttackSnapshot _) => Play(attackClip);
    private void OnSkillUsed(SkillDefinition _, int __) => Play(skillClip);

    private void OnGhostAction(PlayerTimeline.ActionEvent action)
    {
        if (action.Kind == PlayerTimeline.ActionKind.Attack) Play(attackClip);
    }

    private void OnSwitchChanged(bool active)
    {
        if (active) Play(switchClip);
    }

    private void OnDoorChanged(bool open)
    {
        if (open) Play(doorClip);
    }

    private void OnRewind() => Play(rewindClip);
    private void OnGhostSpawned(GhostPlayback _) => Play(ghostClip);
    private void OnUpgradeSelected(UpgradeData _) => Play(upgradeClip);
    private void OnBossPhase() => Play(bossClip);
    private void OnGameOver() => Play(gameOverClip);
    private void OnVictory() => Play(victoryClip);

    private void PlayUiClick() => Play(switchClip);

    private void Play(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    private AudioSource CreateSource(string sourceName, bool music)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = music;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        return source;
    }

    private void RefreshVolume()
    {
        float musicVolume = settings == null ? 1f : settings.MusicVolume;
        float sfxVolume = settings == null ? 1f : settings.SfxVolume;
        if (musicSource != null) musicSource.volume = musicVolume * 0.18f;
        if (sfxSource != null) sfxSource.volume = sfxVolume * 0.45f;
    }

    private void CreateClips()
    {
        menuMusic = CreateMusic("MenuMusic", new[] { 261.63f, 329.63f, 392f }, 0.16f);
        dungeonMusic = CreateMusic("DungeonMusic", new[] { 110f, 138.59f, 164.81f }, 0.18f);
        bossMusic = CreateMusic("BossMusic", new[] { 73.42f, 92.5f, 110f }, 0.26f);
        attackClip = CreateSweep("AttackSfx", 720f, 420f, 0.07f, 0.2f);
        skillClip = CreateSweep("SkillSfx", 340f, 980f, 0.18f, 0.22f);
        hitClip = CreateSweep("HitSfx", 180f, 90f, 0.09f, 0.24f);
        deathClip = CreateSweep("DeathSfx", 240f, 55f, 0.3f, 0.24f);
        rewindClip = CreateSweep("RewindSfx", 160f, 720f, 0.42f, 0.25f);
        ghostClip = CreateSweep("GhostSpawnSfx", 420f, 920f, 0.22f, 0.2f);
        switchClip = CreateSweep("UiSwitchSfx", 520f, 780f, 0.1f, 0.18f);
        doorClip = CreateSweep("DoorSfx", 280f, 620f, 0.2f, 0.2f);
        upgradeClip = CreateSweep("UpgradeSfx", 380f, 980f, 0.32f, 0.22f);
        bossClip = CreateSweep("BossPhaseSfx", 120f, 42f, 0.5f, 0.24f);
        victoryClip = CreateSweep("VictorySfx", 440f, 880f, 0.6f, 0.2f);
        gameOverClip = CreateSweep("GameOverSfx", 220f, 55f, 0.55f, 0.22f);
    }

    private AudioClip CreateMusic(string name, float[] chord, float volume)
    {
        const int rate = 22050;
        int samples = rate * 6;
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            int note = Mathf.FloorToInt(t * 1.5f) % chord.Length;
            float beat = Mathf.Sin(t * Mathf.PI * 3f) * 0.5f + 0.5f;
            float tone = Mathf.Sin(2f * Mathf.PI * chord[note] * t) * 0.55f;
            float fifth = Mathf.Sin(2f * Mathf.PI * chord[(note + 1) % chord.Length] * 2f * t) * 0.18f;
            float bass = Mathf.Sin(2f * Mathf.PI * chord[note] * 0.5f * t) * 0.22f;
            data[i] = (tone + fifth + bass) * (0.55f + beat * 0.45f) * volume;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSweep(string name, float startFrequency, float endFrequency, float duration, float volume)
    {
        const int rate = 22050;
        int samples = Mathf.CeilToInt(rate * duration);
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float normalized = i / (float)samples;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, normalized);
            float envelope = Mathf.Sin(Mathf.PI * normalized);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * envelope * volume;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private void OnDisable()
    {
        if (loop != null) loop.LoopRewound -= OnRewind;
        if (ghosts != null) ghosts.GhostSpawned -= OnGhostSpawned;
        if (gameManager != null)
        {
            gameManager.GameOverTriggered -= OnGameOver;
            gameManager.VictoryTriggered -= OnVictory;
        }
        if (upgradeUI != null) upgradeUI.UpgradeSelected -= OnUpgradeSelected;
        if (settings != null) settings.SettingsChanged -= RefreshVolume;
        for (int i = 0; i < boundHealth.Count; i++) if (boundHealth[i] != null) boundHealth[i].Changed -= OnHealthChanged;
        for (int i = 0; i < boundAttacks.Count; i++) if (boundAttacks[i] != null) boundAttacks[i].AttackPerformed -= OnAttack;
        for (int i = 0; i < boundWeapons.Count; i++) if (boundWeapons[i] != null) boundWeapons[i].OnSkillUsed -= OnSkillUsed;
        for (int i = 0; i < boundGhosts.Count; i++) if (boundGhosts[i] != null) boundGhosts[i].ActionReplayed -= OnGhostAction;
        for (int i = 0; i < boundSwitches.Count; i++) if (boundSwitches[i] != null) boundSwitches[i].StateChanged -= OnSwitchChanged;
        for (int i = 0; i < boundDoors.Count; i++) if (boundDoors[i] != null) boundDoors[i].StateChanged -= OnDoorChanged;
        for (int i = 0; i < boundBossPolish.Count; i++) if (boundBossPolish[i] != null) boundBossPolish[i].PhaseFeedback -= OnBossPhase;
        for (int i = 0; i < boundButtons.Count; i++) if (boundButtons[i] != null) boundButtons[i].onClick.RemoveListener(PlayUiClick);
    }
}
