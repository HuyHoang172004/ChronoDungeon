# M5.4 — Chrono Knight

Verified 2026-09-22 in Unity 6000.6.0f1 via Unity MCP.

## Result

The former Warden stand-in in Warden Antechamber is now the third combat archetype, Chrono Knight.
This gives the roster three different decisions: Ember Chaser pursues and melees, Ash Archer keeps
distance and fires a dodgeable arrow, and Chrono Knight commits to a telegraphed charge then uses
a short defensive block. The Elite room already used `DefeatEnemies`, so its authored exit now
requires defeating the Knight.

| Setting | Value |
| --- | --- |
| HP | 140 |
| Chase speed | 1.35 |
| Charge trigger / stop distance | 5.5 / 2.2 |
| Charge wind-up / speed / duration | 0.6s / 7 / 0.6s |
| Heavy charge damage | 35 |
| Block duration / interval | 0.8s / 3.2s |

`EnemyKnight` owns a small state machine: Chase, ChargeWindup, Charging and Blocking. Charge locks
direction and draws a red line. It uses a reusable CircleCast buffer and a distance confirmation
after Rigidbody collision resolution so a collider stopped just short still receives the authored
heavy hit. Blocking draws a blue ring and `AttackResolver` ignores Player/Ghost damage while calling
`NotifyBlockedAttack`; normal damage resumes after the window. Rewind, disable and death clear
charge/block visuals and restore health through the existing reset architecture.

## Files and scene

New:

- `Assets/Scripts/Enemy/EnemyKnight.cs`
- `Assets/Prefabs/ChronoKnight.prefab`
- `Assets/Tests/KnightPlayModeChecks.cs`
- `Docs/Testing/M5.4/checks.txt`

Modified:

- `Assets/Scripts/Player/AttackResolver.cs`: recognizes the opt-in blocking Knight before damage.
- `Assets/Scenes/GameScene.unity`: Warden stand-in renamed Chrono Knight, HP set to 140, old
  contact/follow components removed, Knight component/serialized Player target/charge line/block
  ring added, and blue armor/visor/crest/eye/blade children authored.
- `ROADMAP.md`: M5.4 DONE and next milestone M5.5.

Prior M5.1–M5.3 changes remain in the working tree and are intentionally preserved.

## Verification

Compile passed after the final charge collision patch. The Editor-only probe ran in Play Mode and
logged 14 PASS checks:

- Elite room configuration, shared EnemyAttackTarget and readable crest/blade visual;
- charge warning, charge start without immediate damage, 35-damage impact and charge completion;
- blue block state, blocked Player attack, block expiry and post-block damage;
- rewind clears state and restores HP; Ghost still spawns;
- Knight death deactivates and opens the Elite exit; room transition cleans old content.

The M5.1 regression probe passed 19 checks after the Knight integration, covering contact telegraph,
pause, dash escape, multi-collider damage, rewind/death restoration, Ghost damage, room clear and
transition. Final scene inspection: `compiling=false`, `scriptCompilationFailed=false`,
`missingScripts=0`, `probes=0`, `dirty=false`, one Knight present. Console after final run: 0 errors
and 0 warnings. Play Mode exited and GameScene saved.

Two probe-only timing issues were corrected during verification: the initial charge assertion was
too early for a 4-unit approach at speed 7, and the block probe allowed a block to start before its
close-range assertion. The game behavior was then re-run with corrected test timing. The final
collision fallback handles the authored Rigidbody stopping point; it does not add per-frame allocations.

## Manual review and limits

In Warden Antechamber, approach until the red charge line appears, sidestep/dash, then deliberately
stay in its path to confirm 35 damage. Approach again during the blue ring and attack; the hit should
be blocked. Attack after the ring disappears, defeat the Knight and walk through the exit. Wait for a
natural rewind and verify HP, charge and block visuals reset cleanly. Test move + attack + dash on
Android to judge whether the lines/ring remain readable.

Direct Rigidbody2D movement is used for authored rooms; there is no navigation around complex walls.
Touch, Android build/performance, audio balance and final difficulty tuning remain unverified. Visual
parts are assembled from existing sprites and are not final art. M5.5 still needs elite-specific
stats/pattern, stronger visual treatment and a meaningful reward.

No commit or push. Stop at M5.4 for review. Next: M5.5 — Elite Enemy.
