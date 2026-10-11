using System.Collections.Generic;
using UnityEngine;

/// <summary>Patrol, perception, A* pursuit, attack spacing and return behavior.</summary>
public enum EnemyMovementState { Idle, Patrol, Chase, Return }

public class EnemyFollow : MonoBehaviour, ITimeLoopResettable
{
    public Transform player;
    public float moveSpeed = 2.2f;
    public float detectionRange = 10f;
    [SerializeField, Min(0f)] private float stoppingDistance = .9f;
    [SerializeField, Min(1f)] private float patrolRadius = 22.5f;
    [SerializeField, Min(.1f)] private float repathInterval = .4f;
    [SerializeField, Min(.1f)] private float loseSightDelay = 2.5f;
    [SerializeField, Min(.1f)] private float patrolPause = 1.1f;

    private readonly List<Vector2> path = new List<Vector2>(128);
    private readonly RaycastHit2D[] sightHits = new RaycastHit2D[12];
    private readonly Collider2D[] nearbyHits = new Collider2D[12];
    private Rigidbody2D body;
    private Collider2D ownCollider;
    private Health health;
    private EnemyContactDamage attack;
    private EnemyAbilityBrain abilityBrain;
    private Map01NavigationGrid navigation;
    private Vector2 home, patrolTarget;
    private int pathIndex;
    private float nextRepath, lostPlayerAt, waitUntil, nextSense;
    private bool seesPlayer;
    private EnemyMovementState state;
    private Vector2 facingDirection = Vector2.down;

    public Vector2 FacingDirection => facingDirection;
    public bool IsMoving { get; private set; }
    public EnemyMovementState MovementState => state;

    public void ConfigureEncounter(PlayerMovement target, Map01NavigationGrid grid, float stopDistance)
    {
        player = target.transform;
        navigation = grid;
        stoppingDistance = Mathf.Max(.2f, stopDistance);
    }

    public void Configure(float speed, float senseRange, float newPatrolRadius = 22.5f)
    {
        moveSpeed = Mathf.Max(.1f, speed);
        detectionRange = Mathf.Max(1f, senseRange);
        patrolRadius = Mathf.Max(1f, newPatrolRadius);
        EnsureMovementBody();
        enabled = true;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        EnsureMovementBody();
        ownCollider = GetComponent<Collider2D>();
        health = GetComponent<Health>();
        attack = GetComponent<EnemyContactDamage>();
        abilityBrain = GetComponent<EnemyAbilityBrain>();
        if (navigation == null) navigation = FindAnyObjectByType<Map01NavigationGrid>();
        if (player == null)
        {
            GameObject target = GameObject.FindWithTag("Player");
            if (target != null) player = target.transform;
        }
        home = transform.position;
    }

    private void EnsureMovementBody()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (body == null) return;

        body.simulated = true;
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        // FreezeRotation keeps a top-down sprite upright. Freezing X/Y here would
        // silently reject every MovePosition request made by the chase AI.
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.WakeUp();
    }

    private void OnEnable() => ResetBrain();

    private void FixedUpdate()
    {
        // Runtime tuning can attach the richer ability brain after this component's
        // Awake. Refresh the optional dependency so attack phases actually stop motion.
        if (abilityBrain == null) abilityBrain = GetComponent<EnemyAbilityBrain>();
        IsMoving = false;
        if (body == null || Time.timeScale <= 0f || (health != null && health.IsDead) ||
            (attack != null && attack.BlocksMovement) ||
            (abilityBrain != null && abilityBrain.BlocksMovement)) return;
        if (player == null)
        {
            GameObject target = GameObject.FindWithTag("Player");
            if (target == null) return;
            player = target.transform;
        }

        if (Time.time >= nextSense)
        {
            nextSense = Time.time + .16f;
            float distance = Vector2.Distance(body.position, player.position);
            // At close range the enemy must engage even if another enemy or a decorative
            // collider briefly blocks the linecast. Longer-range acquisition still needs LOS.
            seesPlayer = distance <= detectionRange &&
                (distance <= 3.5f || HasLineOfSight(player.position));
            if (seesPlayer)
            {
                state = EnemyMovementState.Chase;
                lostPlayerAt = Time.time + loseSightDelay;
            }
            else if (state == EnemyMovementState.Chase && Time.time >= lostPlayerAt)
            {
                state = EnemyMovementState.Return;
                path.Clear();
            }
        }

        switch (state)
        {
            case EnemyMovementState.Idle:
                if (Time.time >= waitUntil) BeginPatrol();
                break;
            case EnemyMovementState.Patrol:
                if (MoveToward(patrolTarget, .2f)) BeginIdle();
                break;
            case EnemyMovementState.Chase:
                float distance = Vector2.Distance(body.position, player.position);
                Vector2 playerDirection = (Vector2)player.position - body.position;
                if (playerDirection.sqrMagnitude > .001f) facingDirection = playerDirection.normalized;
                float desiredStop = attack != null
                    ? Mathf.Max(.2f, attack.AttackRange * .72f)
                    : stoppingDistance;
                if (distance <= desiredStop) { body.linearVelocity = Vector2.zero; return; }
                FollowPathTo(player.position, true);
                break;
            case EnemyMovementState.Return:
                if (MoveToward(home, .25f)) BeginIdle();
                break;
        }
    }

    private void BeginIdle()
    {
        state = EnemyMovementState.Idle;
        waitUntil = Time.time + patrolPause * Random.Range(.65f, 1.35f);
        path.Clear();
        body.linearVelocity = Vector2.zero;
    }

    private void BeginPatrol()
    {
        if (navigation != null && navigation.TryRandomWalkable(home, patrolRadius, out patrolTarget))
        {
            state = EnemyMovementState.Patrol;
            RebuildPath(patrolTarget);
        }
        else BeginIdle();
    }

    private bool MoveToward(Vector2 target, float arrival)
    {
        if (Vector2.Distance(body.position, target) <= arrival) return true;
        FollowPathTo(target, false);
        return false;
    }

    private void FollowPathTo(Vector2 target, bool movingTarget)
    {
        if (navigation == null)
        {
            MoveDirect(target);
            return;
        }
        if (Time.time >= nextRepath || path.Count == 0 || pathIndex >= path.Count)
            RebuildPath(target);
        // A stale/misaligned mask must not freeze every enemy. Physics collision still
        // prevents direct fallback movement from walking through authored walls.
        if (path.Count == 0 || pathIndex >= path.Count)
        {
            MoveDirect(target);
            return;
        }
        while (pathIndex < path.Count - 1 && Vector2.Distance(body.position, path[pathIndex]) < .22f) pathIndex++;
        MoveDirect(path[pathIndex]);
        if (!movingTarget && pathIndex == path.Count - 1 && Vector2.Distance(body.position, target) < .25f)
            body.linearVelocity = Vector2.zero;
    }

    private void RebuildPath(Vector2 target)
    {
        nextRepath = Time.time + repathInterval * Random.Range(.85f, 1.15f);
        pathIndex = 0;
        if (navigation == null || !navigation.FindPath(body.position, target, path)) path.Clear();
        if (path.Count > 1) pathIndex = 1;
    }

    private void MoveDirect(Vector2 target)
    {
        Vector2 delta = target - body.position;
        if (delta.sqrMagnitude < .001f) return;
        Vector2 desired = delta.normalized;
        Vector2 separation = Vector2.zero;
        int nearbyCount = Physics2D.OverlapCircleNonAlloc(body.position, .8f, nearbyHits);
        for (int i = 0; i < nearbyCount; i++)
        {
            EnemyFollow other = nearbyHits[i].GetComponentInParent<EnemyFollow>();
            if (other == null || other == this) continue;
            Vector2 away = body.position - (Vector2)other.transform.position;
            if (away.sqrMagnitude > .001f) separation += away.normalized * .22f;
        }
        Vector2 direction = (desired + separation).normalized;
        facingDirection = desired;
        IsMoving = true;
        body.MovePosition(body.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    private bool HasLineOfSight(Vector2 target)
    {
        Vector2 origin = body != null ? body.position : (Vector2)transform.position;
        int count = Physics2D.LinecastNonAlloc(origin, target, sightHits);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = sightHits[i].collider;
            if (hit == null || hit == ownCollider || hit.transform.IsChildOf(transform)) continue;
            if (hit.GetComponentInParent<PlayerMovement>() != null) return true;
            if (!hit.isTrigger) return false;
        }
        return true;
    }

    private void ResetBrain()
    {
        state = EnemyMovementState.Idle;
        path.Clear();
        pathIndex = 0;
        seesPlayer = false;
        IsMoving = false;
        waitUntil = Time.time + Random.Range(.2f, .8f);
        nextSense = nextRepath = 0f;
        if (body != null) body.linearVelocity = Vector2.zero;
    }

    public void CaptureInitialState() => home = transform.position;
    public void ResetToInitialState() => ResetBrain();
}

/// <summary>Shared A* navigation grid generated from the same walkable mask as Map 01 collision.</summary>
[DisallowMultipleComponent]
public sealed class Map01NavigationGrid : MonoBehaviour
{
    [SerializeField] private Texture2D walkableMask;
    [SerializeField] private Vector2 worldCenter;
    [SerializeField] private Vector2 worldSize = Vector2.one;
    [SerializeField, Min(.25f)] private float cellSize = .75f;

    private int width, height, searchStamp;
    private bool[] walkable;
    private int[] visited, closed, cameFrom;
    private float[] gScore, fScore;
    private readonly List<int> heap = new List<int>(512);

    public void Configure(Texture2D mask, Vector2 center, Vector2 size)
    {
        walkableMask = mask;
        worldCenter = center;
        worldSize = size;
        Build();
    }

    private void Awake() => Build();

    private void Build()
    {
        if (walkableMask == null || !walkableMask.isReadable) return;
        width = Mathf.Max(1, Mathf.CeilToInt(worldSize.x / cellSize));
        height = Mathf.Max(1, Mathf.CeilToInt(worldSize.y / cellSize));
        int count = width * height;
        walkable = new bool[count];
        visited = new int[count]; closed = new int[count]; cameFrom = new int[count];
        gScore = new float[count]; fScore = new float[count];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (x + .5f) / width;
            float v = (y + .5f) / height;
            int px = Mathf.Clamp(Mathf.FloorToInt(u * walkableMask.width), 0, walkableMask.width - 1);
            int py = Mathf.Clamp(Mathf.FloorToInt(v * walkableMask.height), 0, walkableMask.height - 1);
            walkable[y * width + x] = walkableMask.GetPixel(px, py).a >= .5f;
        }
        ApplyNavigationClearance();
    }

    private void ApplyNavigationClearance()
    {
        // Leave one navigation cell around walls. The physics collider remains
        // authoritative, while A* no longer asks a large enemy to hug wall art.
        bool[] source = (bool[])walkable.Clone();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int index = y * width + x;
            if (!source[index]) { walkable[index] = false; continue; }
            bool safe = true;
            for (int oy = -1; oy <= 1 && safe; oy++)
            for (int ox = -1; ox <= 1; ox++)
            {
                int nx = x + ox, ny = y + oy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height || !source[ny * width + nx])
                {
                    safe = false;
                    break;
                }
            }
            walkable[index] = safe;
        }
    }

    public bool FindPath(Vector2 start, Vector2 goal, List<Vector2> result)
    {
        result.Clear();
        if (walkable == null) Build();
        if (walkable == null) return false;
        int startIndex = FindNearestWalkable(WorldToIndex(start));
        int goalIndex = FindNearestWalkable(WorldToIndex(goal));
        if (startIndex < 0 || goalIndex < 0) return false;
        if (++searchStamp == int.MaxValue)
        {
            System.Array.Clear(visited, 0, visited.Length);
            System.Array.Clear(closed, 0, closed.Length);
            searchStamp = 1;
        }
        heap.Clear();
        visited[startIndex] = searchStamp;
        cameFrom[startIndex] = -1;
        gScore[startIndex] = 0f;
        fScore[startIndex] = Heuristic(startIndex, goalIndex);
        HeapPush(startIndex);

        int guard = walkable.Length * 4;
        while (heap.Count > 0 && guard-- > 0)
        {
            int current = HeapPop();
            if (closed[current] == searchStamp) continue;
            if (current == goalIndex) { Reconstruct(current, result); return true; }
            closed[current] = searchStamp;
            int cx = current % width, cy = current / width;
            for (int oy = -1; oy <= 1; oy++)
            for (int ox = -1; ox <= 1; ox++)
            {
                if (ox == 0 && oy == 0) continue;
                int nx = cx + ox, ny = cy + oy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                int next = ny * width + nx;
                if (!walkable[next] || closed[next] == searchStamp) continue;
                if (ox != 0 && oy != 0 && (!walkable[cy * width + nx] || !walkable[ny * width + cx])) continue;
                float candidate = gScore[current] + (ox == 0 || oy == 0 ? 1f : 1.4142f);
                if (visited[next] == searchStamp && candidate >= gScore[next]) continue;
                visited[next] = searchStamp;
                cameFrom[next] = current;
                gScore[next] = candidate;
                fScore[next] = candidate + Heuristic(next, goalIndex);
                HeapPush(next);
            }
        }
        return false;
    }

    public bool TryRandomWalkable(Vector2 center, float radius, out Vector2 point)
    {
        if (walkable == null) Build();
        if (walkable == null) { point = center; return false; }
        for (int i = 0; i < 20; i++)
        {
            Vector2 candidate = center + Random.insideUnitCircle * radius;
            int index = WorldToIndex(candidate);
            if (index >= 0 && walkable[index]) { point = IndexToWorld(index); return true; }
        }
        point = center;
        return false;
    }

    private int WorldToIndex(Vector2 point)
    {
        Vector2 min = worldCenter - worldSize * .5f;
        int x = Mathf.FloorToInt((point.x - min.x) / cellSize);
        int y = Mathf.FloorToInt((point.y - min.y) / cellSize);
        return x < 0 || y < 0 || x >= width || y >= height ? -1 : y * width + x;
    }

    private Vector2 IndexToWorld(int index)
    {
        Vector2 min = worldCenter - worldSize * .5f;
        return min + new Vector2((index % width + .5f) * cellSize, (index / width + .5f) * cellSize);
    }

    private int FindNearestWalkable(int index)
    {
        if (index < 0) return -1;
        if (walkable[index]) return index;
        int cx = index % width, cy = index / width;
        for (int radius = 1; radius <= 5; radius++)
        for (int y = cy - radius; y <= cy + radius; y++)
        for (int x = cx - radius; x <= cx + radius; x++)
            if (x >= 0 && y >= 0 && x < width && y < height && walkable[y * width + x]) return y * width + x;
        return -1;
    }

    private float Heuristic(int a, int b)
    {
        int ax = a % width, ay = a / width, bx = b % width, by = b / width;
        int dx = Mathf.Abs(ax - bx), dy = Mathf.Abs(ay - by);
        return Mathf.Max(dx, dy) + .4142f * Mathf.Min(dx, dy);
    }

    private void Reconstruct(int current, List<Vector2> result)
    {
        while (current >= 0 && result.Count < walkable.Length)
        {
            result.Add(IndexToWorld(current));
            current = cameFrom[current];
        }
        result.Reverse();
    }

    private void HeapPush(int value)
    {
        heap.Add(value);
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (fScore[heap[parent]] <= fScore[value]) break;
            heap[i] = heap[parent]; i = parent;
        }
        heap[i] = value;
    }

    private int HeapPop()
    {
        int root = heap[0], last = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        if (heap.Count == 0) return root;
        int i = 0;
        while (true)
        {
            int left = i * 2 + 1;
            if (left >= heap.Count) break;
            int right = left + 1;
            int child = right < heap.Count && fScore[heap[right]] < fScore[heap[left]] ? right : left;
            if (fScore[last] <= fScore[heap[child]]) break;
            heap[i] = heap[child]; i = child;
        }
        heap[i] = last;
        return root;
    }
}
