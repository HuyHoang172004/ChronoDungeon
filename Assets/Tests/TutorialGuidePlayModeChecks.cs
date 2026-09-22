#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class TutorialGuidePlayModeChecks : MonoBehaviour
{
    private int checks;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M9.5 FAIL: " + message);
        checks++;
        Debug.Log("M9.5 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var guide = FindAnyObjectByType<TutorialGuideUI>();
        var player = FindAnyObjectByType<PlayerMovement>();
        var attack = FindAnyObjectByType<PlayerAttack>();
        var dash = FindAnyObjectByType<PlayerDash>();
        Check(guide != null && player != null && attack != null && dash != null, "tutorial dependencies are available");
        Check(guide.CurrentStep == 0 && guide.IsVisible && guide.CurrentPrompt.Contains("JOYSTICK"), "movement prompt is visible at run start");
        player.SetMoveDirection(Vector2.right);
        yield return null;
        Check(guide.CurrentStep == 1 && guide.CurrentPrompt.Contains("ATTACK"), "movement advances to attack guidance");
        attack.Attack();
        yield return null;
        Check(guide.CurrentStep == 2 && guide.CurrentPrompt.Contains("DASH"), "attack advances to dash guidance");
        dash.Dash();
        yield return null;
        Check(guide.CurrentStep == 3 && guide.CurrentPrompt.Contains("REWINDS"), "dash advances to time-loop guidance");
        Debug.Log("M9.5 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
