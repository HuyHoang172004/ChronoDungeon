# M1.3 — Action Timeline

Status: DONE for M1.3 acceptance, verified 2026-09-21 in Unity 6000.6.0f1.

## Implementation

- `PlayerAttack` publishes accepted attack attempts, including misses. The recorder stamps them with the same loop clock used by movement playback.
- `AttackSnapshot` stores world position, facing direction, range and damage by value. Ghost damage uses the recorded attack origin even when a frame skips over an event. It does not track or re-aim at an Enemy.
- `AttackResolver` shares radial damage rules between Player and Ghost, reuses query/deduplication collections and applies damage once per Health per attack. Targets require an enabled `EnemyAttackTarget` beside Health. Player tag/PlayerMovement are explicitly excluded.
- `PlayerMovement` retains the last nonzero input direction and resets facing on rewind. Combat remains the existing radial attack; directional hit shapes and presentation belong to M3.1.
- `GhostPlayback` consumes ordered actions once per replay, resets its cursor on every loop/slot reuse and flushes pending endpoint events before world reset. Calling `Play` itself does not deal damage. Multiple Ghosts can each legitimately damage the same Enemy.
- Pose sampling/interpolation and the three-slot manager are unchanged. Existing `ActionKind`, direction and payload fields provide extension points for Dash, Interaction and Skill; those mechanics are not implemented in M1.3.
- The existing bounded action channel holds 256 events per loop. Overflow emits one warning per loop and drops further events; ordinary Player attacks still execute. No new attack cooldown is introduced.

## Files and scene

New: `Assets/Scripts/Player/AttackResolver.cs`, `Assets/Scripts/Enemy/EnemyAttackTarget.cs`, `Assets/Tests/ActionTimelinePlayModeChecks.cs`, their Unity-generated metadata, and this report.

Modified: `PlayerAttack.cs`, `PlayerMovement.cs`, `PlayerTimeline.cs`, `PlayerTimelineRecorder.cs`, `GhostPlayback.cs`, `Assets/Scenes/GameScene.unity`, `ROADMAP.md`.

Scene change through Unity MCP: add `EnemyAttackTarget` to the existing Enemy. No new saved GameObjects or prefabs; `maxGhosts` remains 3. Tests create their probe and extra collider/neutral target only in Play Mode.

## Verification

Final complete run: **31 checks PASS**, four natural 20-second rewinds, **4,715 monitored frames**, **9 replayed attacks**. Maximum measured movement error: **0**. Maximum replay delay: **0.03039742 seconds**, within one frame; no early attacks.

Verified exactly 25 damage per attack with two Enemy colliders, no Player/neutral damage, two independent Ghosts producing exactly 75 combined damage from three events, three-Ghost history and oldest-slot reuse without stale attacks. Live Player attack, pause, Game Over and Restart passed. Raw output: [playmode-results.txt](playmode-results.txt).

The initial script-only refresh did not import the new source files; a full asset refresh resolved the missing-type compile errors. An initial probe compared a paused Ghost against the newly advanced loop timestamp on the pause frame; the probe now skips movement interpolation comparisons while the clock is stopped. The complete test was rerun cleanly after that test correction; production movement code did not need a playback change.

Final editor state: outside Play Mode, GameScene saved via MCP, seven original roots, no missing scripts, all Ghost manager references present, maxGhosts = 3, no saved test probe. Native Unity Console: **0 errors, 0 warnings**, 32 informational logs. Source/document whitespace checks pass; Unity's scene serializer adds its standard trailing space on the new empty `m_Name` field, left untouched to avoid manual scene YAML edits. No Git commit or push.

To rerun: open GameScene, enter Play Mode, then create a runtime GameObject with `ActionTimelinePlayModeChecks` using MCP. The editor-only probe restarts the scene, runs four natural 20-second rewinds and checks attack damage/timing, multiple collider deduplication, explicit target filtering, pose replay, pause, Ghost eviction, Game Over and Restart. Enemy chase/contact are disabled temporarily to isolate attack damage. The probe is not part of the saved scene or Android build.

## Manual review

On Android, check joystick + Attack with two fingers and Restart touch behavior. Observe Enemy HP when a Ghost returns to a recorded attack location. Attack animation/VFX remain future combat polish, so HP changes currently provide the visible evidence. Device touch, Android performance and builds are not verified by this milestone.

Next milestone after review: M2.1 — Pressure Switch.
