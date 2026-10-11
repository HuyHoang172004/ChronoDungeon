using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Applies Map 01 readability and combat tuning even before an editor rebuild.</summary>
public static class Map01RuntimeTuning
{
    private const float CameraSize = 4.875f / 1.2f;
    // Keep the hero near the normal enemy scale. The previous 2.144 value made the
    // presentation more than twice its authored size every frame.
    private const float PlayerVisualScale = .525f;
    private const float EnemyScale = .536f;
    private const float EnemyPatrolRadius = 22.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoading()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") Apply();
    }

    private static void Apply()
    {
        if (SceneManager.GetActiveScene().name != "GameScene" || MapSelectionRuntime.SelectedMap != 0) return;

        ApplyCurrentValues();

        if (Object.FindAnyObjectByType<RuntimeVisualScaleTuning>() == null)
        {
            GameObject tuningObject = new GameObject("Runtime Visual Scale Tuning");
            tuningObject.hideFlags = HideFlags.DontSave;
            tuningObject.AddComponent<RuntimeVisualScaleTuning>();
        }

        if (Object.FindAnyObjectByType<Map01PlayerBootstrap>() == null)
        {
            GameObject bootstrap = new GameObject("Map01 Player Bootstrap");
            bootstrap.hideFlags = HideFlags.DontSave;
            bootstrap.AddComponent<Map01PlayerBootstrap>();
        }
    }

    public static void ApplyCurrentValues()
    {
        if (SceneManager.GetActiveScene().name != "GameScene" || MapSelectionRuntime.SelectedMap != 0) return;

        Camera camera = Camera.main;
        if (camera != null) camera.orthographicSize = CameraSize;

        PlayerMovement player = Object.FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            Transform visual = player.transform.Find("PlayerVisual");
            if (visual != null)
            {
                PlayerVisualPresentation presentation = visual.GetComponent<PlayerVisualPresentation>();
                if (presentation != null) presentation.SetPresentationScale(PlayerVisualScale);
                else visual.localScale = Vector3.one * PlayerVisualScale;

                PlayerWeaponVisualSetController visualSetController =
                    visual.GetComponent<PlayerWeaponVisualSetController>();
                if (visualSetController != null)
                    visualSetController.SetPresentationScale(PlayerVisualScale);
            }
        }

        foreach (EnemyFollow enemy in Object.FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include))
        {
            if (enemy == null) continue;
            enemy.transform.localScale = Vector3.one * EnemyScale;
            enemy.Configure(2.2f, 10f, EnemyPatrolRadius);

            EnemyContactDamage attack = enemy.GetComponent<EnemyContactDamage>();
            if (attack == null) attack = enemy.gameObject.AddComponent<EnemyContactDamage>();
            attack.Configure(20f, 1.15f, 1.15f, .38f);

            if (enemy.GetComponent<EnemyReadabilityPresentation>() == null)
                enemy.gameObject.AddComponent<EnemyReadabilityPresentation>();
            CharacterAnimationIdentity identity = enemy.GetComponent<CharacterAnimationIdentity>();
            if (identity == null) identity = enemy.gameObject.AddComponent<CharacterAnimationIdentity>();
            string resolvedId = CharacterAnimationResolver.GuessCharacterId(enemy.gameObject);
            if (!string.IsNullOrWhiteSpace(resolvedId) && identity.CharacterId != resolvedId)
                identity.Configure(resolvedId);
            EnemyAnimationDriver animationDriver = enemy.GetComponent<EnemyAnimationDriver>();
            if (animationDriver == null) animationDriver = enemy.gameObject.AddComponent<EnemyAnimationDriver>();
            CharacterAnimationProfile animationProfile = CharacterAnimationResolver.Resolve(enemy.gameObject);
            if (animationProfile != null && animationDriver.Profile != animationProfile)
                animationDriver.Configure(animationProfile);
            if (enemy.GetComponent<EnemyAbilityBrain>() == null)
                enemy.gameObject.AddComponent<EnemyAbilityBrain>();
            // AbilityBrain owns windup/impact/recovery now. Keep the legacy contact
            // attacker present for old prefabs, but prevent duplicate damage ticks.
            attack.enabled = false;
        }
    }

}

/// <summary>
/// Applies the authored opening spawn only after RoomManager has initialized, then makes
/// sure an enabled PlayerVisual is what the camera follows. This prevents the old scene
/// transform (-39, 29) from leaving the hero outside the visible Map 01 play area.
/// </summary>
public sealed class Map01PlayerBootstrap : MonoBehaviour
{
    private int framesRemaining = 3;

    private void Update()
    {
        if (--framesRemaining > 0) return;
        if (MapSelectionRuntime.SelectedMap != 0) { Destroy(gameObject); return; }
        PlayerMovement player = Object.FindAnyObjectByType<PlayerMovement>();
        GameObject spawn = GameObject.Find("PlayerSpawn");
        if (player != null && spawn != null)
        {
            Vector3 position = spawn.transform.position;
            position.z = player.transform.position.z;
            player.transform.position = position;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
            Transform visual = player.transform.Find("PlayerVisual");
            SpriteRenderer renderer = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
            if (renderer != null) { renderer.enabled = true; renderer.sortingOrder = 30; }

            Camera camera = Camera.main;
            if (camera != null)
            {
                RoomCamera roomCamera = camera.GetComponent<RoomCamera>();
                if (roomCamera != null) roomCamera.enabled = false;
                ZoneCameraController zone = camera.GetComponent<ZoneCameraController>();
                if (zone == null) zone = camera.gameObject.AddComponent<ZoneCameraController>();
                SpriteRenderer floor = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include)
                    .FirstOrDefault(value => value.name == "WalkableFloor");
                Vector2 size = floor != null ? floor.bounds.size : new Vector2(40f, 25f);
                zone.ConfigureRuntimeMap(player, size, 4.875f / 1.2f);
            }
        }
        Destroy(gameObject);
    }
}

[DisallowMultipleComponent]
public sealed class RuntimeVisualScaleTuning : MonoBehaviour
{
    private void LateUpdate() => Map01RuntimeTuning.ApplyCurrentValues();
}
