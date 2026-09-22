# M5.2 — Ember Chaser

Verified 2026-09-22 in Unity 6000.6.0f1 via Unity MCP.

## Result

Five existing normal guards (two Guard Hall, three Split Bastion) now use Ember Chaser tuning
and a red/orange body with dark outline, eyes and fangs assembled from existing sprite geometry.
This is the first normal melee archetype. Warden remains the unchanged elite stand-in.
Existing EnemyFollow, EnemyContactDamage, Health, HealthBar, EnemyAttackTarget and TimeLoopActor
provide behavior; no duplicate enemy system was added.

| Setting | Value | Purpose |
| --- | --- | --- |
| HP | 75 | Three baseline 25-damage Player attacks |
| Move speed | 2.2 | Pursuit pressure while Player at speed 5 can disengage |
| Detection range | 18 | Covers the authored combat room |
| Stopping distance | 0.75 | Clamp chase movement near target; physical colliders can stop earlier |
| Damage | 20 | Retained M5.1 damage; five unhealed strikes against 100 HP |
| Wind-up / cooldown | 0.45s / 1s | Retained warning window and strike cadence |
| Damage query radius | 0.65 | Retained collider-overlap check, re-evaluated on impact |

## Files and objects

New: `Assets/Prefabs/EmberChaser.prefab` and meta; `Assets/Tests/ChaserPlayModeChecks.cs` and meta;
this review, `checks.txt`, `ember-chaser.png`.

Modified this milestone: `Assets/Scripts/Enemy/EnemyFollow.cs` adds serialized stopping distance
and clamps movement to avoid overshoot; `Assets/Scenes/GameScene.unity` configures/renames five
enemies and adds eight SpriteRenderer children to each; `ROADMAP.md` marks M5.2 DONE.
Other uncommitted M5.1 work from the prior milestone is preserved.

Prefab is a reusable authored copy. Its Player scene reference is intentionally empty: assign
EnemyFollow.player when placing it, and register its Health with the room's enemy list.
Existing five scene actors retain their own authored instances and room references; they are
not claimed to be linked instances of this prefab. Prefab HealthBar and telegraph references
point internally. No per-frame visual allocations or repeated VFX spawning were added.

## Tests

Compile passed. Editor-only probes were attached only during Play Mode, never saved in scene.

- 7 M5.2 checks PASS: HP/speed relative to Player, visual child presence, actual chase displacement
  over 0.4 seconds, pause freeze, 0.75 stop without overshoot, two attacks leave 25 HP,
  third attack deactivates enemy. Stop-distance check temporarily isolates collision/attack;
  normal-contact behavior is covered by M5.1 regression.
- 19 existing M5.1 regression checks PASS on the tuned Chaser: warning before damage, chase
  stop during wind-up, pause, dash escape, one damage per strike with two colliders, cooldown,
  live rewind cancellation, Player attack, death/revive, clean visual restoration, room lock,
  Ghost replay damage, independent effect renderers, room clear and transition cleanup.
- Inspected camera screenshot in Guard Hall with one staged warning ring. Explicit camera
  capture avoided the previous MCP composited screenshot problem. Overlay HUD is excluded.
- Checked scene/prefab references, Warden unchanged, no missing scripts or saved probes.
  Final Console: 0 errors / 0 warnings; Play Mode exited and GameScene saved.

Logs: `checks.txt`. Rewind tests call the existing rewind path directly to keep testing lean;
this milestone did not repeat natural 20-second endurance tests or the full dungeon run.
A transient MCP WebSocket initialization warning after compile was inspected and cleared before
the test run; no gameplay warning/error occurred during the tests or explicit camera capture.

## Manual review and limitations

On Android, approach a Chaser, read the warning, move/dash away, then turn and attack. Confirm
the face/ring are distinguishable at phone scale and movement + attack/dash touch works together.
Kill both Guard Hall Chasers and walk through the exit; wait for a natural rewind to inspect
revival and Ghost cooperation. Try three Chasers in Split Bastion for encounter pressure.

Device touch/performance/build and final difficulty balancing remain unverified. Visuals are
simple authored shapes, not final animation/art. Pursuit is direct Rigidbody2D movement, without
navigation around complex obstacles. Prefab scene-target assignment is explicit/manual.

No commit/push. Stop at M5.2 for review; next milestone is M5.3 — Archer.
