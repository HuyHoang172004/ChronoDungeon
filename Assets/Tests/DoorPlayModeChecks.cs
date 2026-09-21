#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(10000)]
public sealed class DoorPlayModeChecks : MonoBehaviour
{
    private Door door;
    private BoxCollider2D blocker;
    private GameObject panel;
    private PlayerMovement player;
    private EnemyFollow enemy;
    private PressureSwitch plate;
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private int checks, frames, transitions, replays;
    private float worstError;
    private bool monitor, background, resetSeen;

    private void Check(bool ok, string message)
    {
        if (!ok) { monitor = false; StopAllCoroutines(); throw new System.Exception("M2.2 FAIL: " + message); }
        checks++;
        Debug.Log("M2.2 PASS: " + message);
    }

    private void Bind()
    {
        door = FindAnyObjectByType<Door>();
        blocker = door.GetComponent<BoxCollider2D>();
        panel = door.transform.Find("Closed Panel").gameObject;
        player = FindAnyObjectByType<PlayerMovement>();
        enemy = FindAnyObjectByType<EnemyFollow>();
        enemy.enabled = false;
        enemy.GetComponent<EnemyContactDamage>().enabled = false;
        plate = FindAnyObjectByType<PressureSwitch>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
    }

    private void Place(Component target, Vector3 position)
    {
        target.transform.position = position;
        var rb = target.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = position; rb.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private void LateUpdate()
    {
        if (!monitor) return;
        if (blocker.enabled == door.IsOpen || panel.activeSelf == door.IsOpen || door.IsLocked == door.IsOpen)
            Check(false, "Collider/visual/state desynchronization");
        if (!loop.IsRunning) return;
        for (int i=0;i<ghosts.ActiveGhostCount;i++)
        {
            var g=ghosts.GetGhost(i);
            float error=Vector3.Distance(g.transform.position,g.Timeline.Evaluate(loop.ElapsedTime).Position);
            worstError=Mathf.Max(worstError,error);
            if(error>0.001f) Check(false,"Door changed Ghost movement replay");
        }
        frames++;
    }

    private IEnumerator At(float time)
    {
        while(loop.ElapsedTime<time) yield return null;
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        FindAnyObjectByType<GameManager>().Restart();
        yield return null;
        yield return null;
        Bind();
        monitor = true;
        door.StateChanged += value => transitions++;
        Check(door.IsLocked && blocker.enabled && panel.activeSelf, "Initial locked door has blocking collider and visible panel");
        Check(loop.LoopDuration==20 && ghosts.MaxGhosts==3,"Existing loop duration and maxGhosts retained");
        var closedColor=door.transform.Find("Status Light").GetComponent<SpriteRenderer>().color;
        Place(enemy,new Vector3(-1.5f,0,0));
        enemy.GetComponent<TimeLoopActor>().CaptureInitialState();
        Place(player,new Vector3(0,-1,0));
        player.SetMoveDirection(Vector2.right);
        yield return new WaitForSeconds(0.7f);
        player.SetMoveDirection(Vector2.zero);
        Check(player.transform.position.x>0.8f && player.transform.position.x<1.3f,
            "Closed Door physically blocks Rigidbody2D Player moving toward it");
        var signal=new UnityEvent<bool>();
        signal.AddListener(door.SetOpen);
        signal.Invoke(true);
        Check(door.IsOpen && !blocker.enabled && !panel.activeSelf,"Generic bool event opens collider and visual together");
        Check(door.transform.Find("Status Light").GetComponent<SpriteRenderer>().color!=closedColor,
            "Open status light differs from locked status");
        int before=transitions;
        signal.Invoke(true);
        Check(transitions==before,"Repeated open command does not duplicate state events");
        player.SetMoveDirection(Vector2.right);
        yield return new WaitForSeconds(0.65f);
        player.SetMoveDirection(Vector2.zero);
        Check(player.transform.position.x>3f,"Open Door allows physical Player passage");
        Place(player,door.transform.position);
        signal.Invoke(false);
        Check(door.IsOpen && door.IsClosePending && !blocker.enabled && !panel.activeSelf,
            "Close while Player occupies passage defers without trapping or visual mismatch");
        yield return new WaitForSeconds(0.2f);
        Check(door.IsOpen,"Door remains open while passage occupied");
        door.Open();
        Check(!door.IsClosePending,"New open signal cancels pending close");
        door.Close();
        player.SetMoveDirection(Vector2.right);
        yield return new WaitForSeconds(0.45f);
        player.SetMoveDirection(Vector2.zero);
        Check(door.IsLocked && !door.IsClosePending && player.transform.position.x>3,
            "Player can exit and pending Door closes automatically after clearance");
        door.Open();
        Place(enemy,door.transform.position);
        door.Close();
        Check(door.IsClosePending,"Enemy body also prevents unsafe closure");
        Place(enemy,new Vector3(-1.5f,0,0));
        yield return new WaitForSeconds(0.08f);
        Check(door.IsLocked,"Door closes after Enemy clears");
        Place(player,plate.transform.position);
        yield return new WaitForSeconds(0.12f);
        float switchTime=loop.ElapsedTime;
        Check(plate.IsActive && door.IsOpen,"Saved PressureSwitch Inspector event opens Door");
        yield return new WaitForSeconds(0.4f);
        Place(player,Vector3.zero);
        yield return new WaitForSeconds(0.12f);
        float leaveTime=loop.ElapsedTime;
        Check(!plate.IsActive && door.IsLocked,"Switch release closes clear Door");
        player.GetComponent<PlayerAttack>().Attack();
        float attackTime=loop.ElapsedTime;
        Check(enemy.GetComponent<Health>().currentHealth==75 && player.GetComponent<Health>().currentHealth==100,
            "Live attack still deals exactly 25 Enemy damage");
        loop.LoopRewound += () => resetSeen=door.IsLocked && blocker.enabled && panel.activeSelf && !plate.IsActive;
        yield return At(18f);
        Place(player,plate.transform.position);
        yield return new WaitForSeconds(0.12f);
        Check(door.IsOpen,"Door is open before natural rewind");
        float deadline=Time.realtimeSinceStartup+10;
        while(loop.loopIndex==1 && Time.realtimeSinceStartup<deadline) yield return null;
        Check(loop.loopIndex==2 && resetSeen,"Natural rewind restores configured locked Door and inactive switch");
        Check(ghosts.ActiveGhostCount==1,"Ghost history created normally");
        ghosts.ActiveGhost.ActionReplayed += action => replays++;
        yield return At(switchTime+0.1f);
        Check(plate.IsActive && door.IsOpen,"Replaying Ghost operates existing switch-to-Door event");
        yield return At(leaveTime+0.08f);
        Check(!plate.IsActive && door.IsLocked,"Ghost departure closes Door without collider interaction");
        yield return At(attackTime+0.2f);
        Check(replays==1 && enemy.GetComponent<Health>().currentHealth==75 && player.GetComponent<Health>().currentHealth==100,
            "Ghost attack remains once-only, exact damage, no Player damage");
        door.Open();
        Place(player,door.transform.position);
        door.Close();
        var hp=player.GetComponent<Health>();
        hp.TakeDamage(10000);
        var game=FindAnyObjectByType<GameManager>();
        Check(game.IsGameOver && Time.timeScale==0 && door.IsClosePending,"Game Over works while Door close is pending");
        float frozen=loop.ElapsedTime;
        int oldTransitions=transitions;
        Place(player,Vector3.zero);
        yield return new WaitForSecondsRealtime(0.25f);
        Check(door.IsOpen && transitions==oldTransitions && loop.ElapsedTime==frozen,
            "Game Over freezes pending Door, loop and visual state");
        monitor=false;
        var oldDoor=door;
        game.Restart();
        yield return null;
        yield return null;
        Bind();
        Check(oldDoor==null && door.IsLocked && !door.IsClosePending && !plate.IsActive &&
            ghosts.ActiveGhostCount==0 && loop.loopIndex==1 && Time.timeScale==1,
            "Restart restores clean Door/switch/history state");
        Application.runInBackground=background;
        Debug.Log("M2.2 ALL CHECKS PASSED; checks="+checks+"; frames="+frames+"; transitions="+transitions+"; worstPosition="+worstError);
    }

    private void OnDestroy() => Application.runInBackground=background;
}
#endif
