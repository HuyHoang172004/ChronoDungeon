using UnityEngine;

[DisallowMultipleComponent]
public sealed class ChronoGuardianArena : MonoBehaviour
{
    [SerializeField] private Vector2 arenaSize = new Vector2(14f, 8f);
    [SerializeField] private ChronoGuardian boss;

    public Vector2 ArenaSize => arenaSize;
    public ChronoGuardian Boss => boss;
    public bool IsConfigured => boss != null && arenaSize.x > 0f && arenaSize.y > 0f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.35f, 0.75f, 1f, 0.35f);
        Gizmos.DrawWireCube(transform.position, arenaSize);
    }
}
