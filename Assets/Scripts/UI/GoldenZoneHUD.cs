using TMPro;
using UnityEngine;

/// Zone 1 presentation HUD. It intentionally omits all temporal and boss data;
/// the first zone teaches navigation, combat, key pickup, and the gate.
public sealed class GoldenZoneHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveLabel;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private RuneKeyInventory inventory;
    [SerializeField] private Room zoneRoom;
    [SerializeField] private Health[] guards;
    private float nextRefresh;
    private string lastObjective;

    private void Awake()
    {
        if (objectiveLabel == null) objectiveLabel = GetComponentInChildren<TMP_Text>(true);
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (inventory == null && player != null) inventory = player.GetComponent<RuneKeyInventory>();
        if (zoneRoom == null)
        {
            foreach (Room room in FindObjectsByType<Room>(FindObjectsInactive.Include))
                if (room.DisplayName.Contains("RUINED ENTRANCE")) { zoneRoom = room; break; }
        }
        if (zoneRoom != null)
        {
            var enemyList = new System.Collections.Generic.List<Health>();
            foreach (EnemyFollow enemy in zoneRoom.GetComponentsInChildren<EnemyFollow>(true))
            {
                Health health = enemy.GetComponent<Health>();
                if (health != null) enemyList.Add(health);
            }
            guards = enemyList.ToArray();
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + .12f;
        Refresh();
    }

    private void Refresh()
    {
        if (objectiveLabel == null) return;
        string objective = "EXPLORE THE RUINED ENTRANCE";
        if (inventory != null && inventory.HasRuneKey)
            objective = "OPEN THE RUNE GATE";
        else if (AllGuardsDefeated())
            objective = "FIND THE RUNE KEY";
        else if (player != null && player.transform.position.x > -10f &&
                 player.transform.position.y > -14f && player.transform.position.y < 15f)
            objective = "DEFEAT THE CORRUPTED GUARDS";

        if (objective == lastObjective) return;
        lastObjective = objective;
        objectiveLabel.text = objective;
    }

    private bool AllGuardsDefeated()
    {
        if (guards == null || guards.Length == 0) return false;
        foreach (Health guard in guards) if (guard != null && !guard.IsDead) return false;
        return true;
    }
}
