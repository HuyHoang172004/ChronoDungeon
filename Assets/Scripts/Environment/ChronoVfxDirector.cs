using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Event-driven VFX pass for the time dungeon. One reusable particle system is shared by
/// combat, rewind, puzzle, progression and victory events to keep mobile allocations bounded.
/// </summary>
public sealed class ChronoVfxDirector : MonoBehaviour
{
    private readonly List<Health> boundHealth = new List<Health>(32);
    private readonly List<PlayerAttack> boundAttacks = new List<PlayerAttack>(4);
    private readonly List<GhostPlayback> boundGhosts = new List<GhostPlayback>(3);
    private readonly List<PressureSwitch> boundSwitches = new List<PressureSwitch>(8);
    private readonly List<Door> boundDoors = new List<Door>(8);
    private readonly List<ChronoGuardianPolish> boundBossPolish = new List<ChronoGuardianPolish>(2);
    private readonly Dictionary<Health, float> lastHealth = new Dictionary<Health, float>();
    private ParticleSystem burstSystem;
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private GameManager gameManager;
    private UpgradeChoiceUI upgradeUI;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene" &&
            FindAnyObjectByType<ChronoVfxDirector>() == null)
        {
            GameObject root = new GameObject("Chrono VFX Director");
            root.AddComponent<ChronoVfxDirector>();
        }
    }

    private void Awake()
    {
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        upgradeUI = FindAnyObjectByType<UpgradeChoiceUI>();
        CreateBurstSystem();
    }

    private void OnEnable()
    {
        if (loop != null) loop.LoopRewound += OnRewind;
        if (ghosts != null) ghosts.GhostSpawned += OnGhostSpawned;
        if (gameManager != null) gameManager.VictoryTriggered += OnVictory;
        if (upgradeUI != null) upgradeUI.UpgradeSelected += OnUpgradeSelected;
        RefreshBindings();
    }

    private void Update()
    {
        if (Time.frameCount % 30 == 0) RefreshBindings();
    }

    private void RefreshBindings()
    {
        BindHealth(FindObjectsByType<Health>(FindObjectsInactive.Include));
        BindAttacks(FindObjectsByType<PlayerAttack>(FindObjectsInactive.Include));
        BindGhosts(FindObjectsByType<GhostPlayback>(FindObjectsInactive.Include));
        BindSwitches(FindObjectsByType<PressureSwitch>(FindObjectsInactive.Include));
        BindDoors(FindObjectsByType<Door>(FindObjectsInactive.Include));
        BindBossPolish(FindObjectsByType<ChronoGuardianPolish>(FindObjectsInactive.Include));
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
            value.DeathFeedback += OnBossDeath;
        }
    }

    private void OnHealthChanged(Health health)
    {
        if (health == null) return;
        float previous = lastHealth.TryGetValue(health, out float value) ? value : health.currentHealth;
        lastHealth[health] = health.currentHealth;
        if (health.currentHealth >= previous) return;
        Burst(health.transform.position, health.IsDead ? new Color(1f, 0.15f, 0.08f) : Color.white,
            health.IsDead ? 20 : 7, health.IsDead ? 2.6f : 1.5f, health.IsDead ? 0.16f : 0.09f);
    }

    private void OnAttack(AttackSnapshot attack)
    {
        Vector2 direction = attack.Direction.sqrMagnitude > 0.001f ? attack.Direction.normalized : Vector2.right;
        Burst(attack.Position + direction * attack.Range * 0.62f, new Color(0.35f, 0.95f, 1f), 8, 1.7f, 0.075f);
    }

    private void OnGhostAction(PlayerTimeline.ActionEvent action)
    {
        if (action.Kind != PlayerTimeline.ActionKind.Attack) return;
        Burst(action.Attack.Position, new Color(0.7f, 0.45f, 1f), 8, 1.5f, 0.07f);
    }

    private void OnSwitchChanged(bool active)
    {
        if (!active) return;
        PressureSwitch source = FindNearestActiveSwitch();
        if (source != null) Burst(source.transform.position, new Color(0.25f, 1f, 0.85f), 12, 1.3f, 0.08f);
    }

    private void OnDoorChanged(bool open)
    {
        if (!open) return;
        Door source = FindNearestOpenDoor();
        if (source != null) Burst(source.transform.position, new Color(0.2f, 1f, 1f), 16, 1.8f, 0.1f);
    }

    private void OnBossPhase()
    {
        ChronoGuardian boss = FindAnyObjectByType<ChronoGuardian>();
        if (boss != null) Burst(boss.transform.position, new Color(0.85f, 0.28f, 1f), 28, 2.8f, 0.13f);
    }

    private void OnBossDeath()
    {
        ChronoGuardian boss = FindAnyObjectByType<ChronoGuardian>();
        if (boss != null) Burst(boss.transform.position, new Color(0.3f, 0.9f, 1f), 38, 3.4f, 0.18f);
    }

    private void OnRewind()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) Burst(player.transform.position, new Color(0.55f, 0.35f, 1f), 30, 2.2f, 0.12f);
    }

    private void OnGhostSpawned(GhostPlayback ghost)
    {
        if (ghost != null) Burst(ghost.transform.position, new Color(0.7f, 0.55f, 1f), 22, 2f, 0.1f);
    }

    private void OnUpgradeSelected(UpgradeData _)
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) Burst(player.transform.position, new Color(0.3f, 1f, 0.9f), 24, 2.4f, 0.11f);
    }

    private void OnVictory()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) Burst(player.transform.position, new Color(1f, 0.85f, 0.3f), 46, 3.8f, 0.15f);
    }

    private PressureSwitch FindNearestActiveSwitch()
    {
        for (int i = 0; i < boundSwitches.Count; i++)
            if (boundSwitches[i] != null && boundSwitches[i].IsActive) return boundSwitches[i];
        return null;
    }

    private Door FindNearestOpenDoor()
    {
        for (int i = 0; i < boundDoors.Count; i++)
            if (boundDoors[i] != null && boundDoors[i].IsOpen) return boundDoors[i];
        return null;
    }

    private void CreateBurstSystem()
    {
        GameObject visual = new GameObject("Temporal Burst Particles");
        visual.transform.SetParent(transform, false);
        burstSystem = visual.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = burstSystem.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 512;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startColor = Color.white;
        ParticleSystem.EmissionModule emission = burstSystem.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = burstSystem.shape;
        shape.enabled = false;
        ParticleSystemRenderer renderer = burstSystem.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Unlit") ?? Shader.Find("Sprites/Default");
        if (shader != null) renderer.sharedMaterial = new Material(shader);
    }

    private void Burst(Vector3 position, Color color, int count, float speed, float size)
    {
        if (burstSystem == null) return;
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i) / count;
            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed,
                startColor = color,
                startSize = size
            };
            burstSystem.Emit(emit, 1);
        }
    }

    private void OnDisable()
    {
        if (loop != null) loop.LoopRewound -= OnRewind;
        if (ghosts != null) ghosts.GhostSpawned -= OnGhostSpawned;
        if (gameManager != null) gameManager.VictoryTriggered -= OnVictory;
        if (upgradeUI != null) upgradeUI.UpgradeSelected -= OnUpgradeSelected;
        for (int i = 0; i < boundHealth.Count; i++) if (boundHealth[i] != null) boundHealth[i].Changed -= OnHealthChanged;
        for (int i = 0; i < boundAttacks.Count; i++) if (boundAttacks[i] != null) boundAttacks[i].AttackPerformed -= OnAttack;
        for (int i = 0; i < boundGhosts.Count; i++) if (boundGhosts[i] != null) boundGhosts[i].ActionReplayed -= OnGhostAction;
        for (int i = 0; i < boundSwitches.Count; i++) if (boundSwitches[i] != null) boundSwitches[i].StateChanged -= OnSwitchChanged;
        for (int i = 0; i < boundDoors.Count; i++) if (boundDoors[i] != null) boundDoors[i].StateChanged -= OnDoorChanged;
        for (int i = 0; i < boundBossPolish.Count; i++)
        {
            if (boundBossPolish[i] == null) continue;
            boundBossPolish[i].PhaseFeedback -= OnBossPhase;
            boundBossPolish[i].DeathFeedback -= OnBossDeath;
        }
    }
}
