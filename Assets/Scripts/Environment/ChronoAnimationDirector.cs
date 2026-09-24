using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight animation pass for the visual accents created by ChronoVisualReplacement.
/// It deliberately animates only child renderers, never gameplay transforms or colliders.
/// </summary>
public sealed class ChronoAnimationDirector : MonoBehaviour
{
    private const string AccentName = "ChronoVisualAccent";
    private readonly List<AnimatedAccent> accents = new List<AnimatedAccent>(32);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene" &&
            FindAnyObjectByType<ChronoAnimationDirector>() == null)
        {
            GameObject root = new GameObject("Chrono Animation Director");
            root.AddComponent<ChronoAnimationDirector>();
        }
    }

    private void Start() => Refresh();

    private void Update()
    {
        if (Time.frameCount % 20 == 0) Refresh();

        float time = Time.unscaledTime;
        for (int i = accents.Count - 1; i >= 0; i--)
        {
            AnimatedAccent accent = accents[i];
            if (accent.renderer == null)
            {
                accents.RemoveAt(i);
                continue;
            }

            float amplitude = accent.IsBoss ? 0.11f : accent.IsGhost ? 0.12f : 0.06f;
            if (accent.player != null)
            {
                Rigidbody2D body = accent.player.GetComponent<Rigidbody2D>();
                if (body != null && body.linearVelocity.sqrMagnitude > 0.04f) amplitude = 0.1f;
            }
            float wave = Mathf.Sin(time * accent.Speed + accent.phase);
            float scale = 1f + wave * amplitude;
            accent.renderer.transform.localScale = accent.baseScale * scale;
            accent.renderer.transform.localPosition = accent.basePosition +
                Vector3.up * (wave * (accent.IsGhost ? 0.045f : 0.018f));

            if (accent.IsBoss)
                accent.renderer.transform.localRotation = Quaternion.Euler(0f, 0f, time * 18f);
            else if (accent.IsEnemy)
                accent.renderer.transform.localRotation = Quaternion.Euler(0f, 0f, wave * 5f);
            else
                accent.renderer.transform.localRotation = Quaternion.Euler(0f, 0f, wave * 2f);

            if (accent.door != null)
            {
                accent.renderer.enabled = accent.door.IsOpen;
                if (!accent.door.IsOpen) accent.renderer.transform.localScale = Vector3.zero;
            }
            else if (accent.switchTarget != null)
            {
                accent.renderer.enabled = true;
                float stateScale = accent.switchTarget.IsActive ? 1.08f : 0.82f;
                accent.renderer.transform.localScale *= stateScale;
            }
        }
    }

    private void Refresh()
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer == null || renderer.gameObject.name != AccentName || Contains(renderer)) continue;

            AnimatedAccent accent = new AnimatedAccent(renderer);
            accent.player = renderer.GetComponentInParent<PlayerMovement>();
            accent.ghost = renderer.GetComponentInParent<GhostPlayback>();
            accent.boss = renderer.GetComponentInParent<ChronoGuardian>();
            accent.archer = renderer.GetComponentInParent<EnemyArcher>();
            accent.knight = renderer.GetComponentInParent<EnemyKnight>();
            accent.chaser = renderer.GetComponentInParent<EnemyFollow>();
            accent.door = renderer.GetComponentInParent<Door>();
            accent.switchTarget = renderer.GetComponentInParent<PressureSwitch>();
            accents.Add(accent);
        }
    }

    private bool Contains(SpriteRenderer renderer)
    {
        for (int i = 0; i < accents.Count; i++)
            if (accents[i].renderer == renderer) return true;
        return false;
    }

    private sealed class AnimatedAccent
    {
        public readonly SpriteRenderer renderer;
        public readonly Vector3 baseScale;
        public readonly Vector3 basePosition;
        public readonly float phase;
        public PlayerMovement player;
        public GhostPlayback ghost;
        public ChronoGuardian boss;
        public EnemyArcher archer;
        public EnemyKnight knight;
        public EnemyFollow chaser;
        public Door door;
        public PressureSwitch switchTarget;

        public bool IsBoss => boss != null;
        public bool IsGhost => ghost != null;
        public bool IsEnemy => archer != null || knight != null || chaser != null;
        public float Speed => IsBoss ? 1.35f : IsGhost ? 2.1f : IsEnemy ? 3.2f : 2.4f;

        public AnimatedAccent(SpriteRenderer renderer)
        {
            this.renderer = renderer;
            baseScale = renderer.transform.localScale;
            basePosition = renderer.transform.localPosition;
            phase = renderer.transform.position.x * 1.37f + renderer.transform.position.y * 0.73f;
        }
    }
}
