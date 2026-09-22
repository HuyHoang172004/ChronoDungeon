# M5.1 — Enemy Base Improvements

Verified 2026-09-22, Unity 6000.6.0f1, Unity MCP / Editor Play Mode.

## Behavior and scope

Existing Health owns damage/death; EnemyAttackTarget opts into Player/Ghost damage;
TimeLoopActor restores health, position and activation; Room owns completion. These are reused.
EnemyContactDamage now winds up for 0.45 seconds with an amber-to-red warning circle.
EnemyFollow stops during wind-up. Impact rechecks Player collider overlap in radius 0.65;
escaping via movement/dash avoids damage. A strike deals 20 damage once even with multiple
Player colliders, then waits 1 second before another wind-up. Queries reuse a List buffer.
Death/disable/rewind cancel pending attacks and clear the warning. Hit/death feedback restores
original color/scale on disable so revived enemies do not retain the death flash.

The warning is a presentation-only component with a serialized shared material. No VFX objects
are spawned per strike. All six existing guards use it; Warden remains a stand-in, not an elite.
No M5.2 archetype art or balancing work is claimed.

## Changed files and scene

New:
- `Assets/Scripts/Enemy/EnemyAttackTelegraph.cs` (+ Unity meta).
- `Assets/Materials/EnemyTelegraph.mat` (+ material/folder meta).
- `Assets/Tests/EnemyBasePlayModeChecks.cs` (+ Unity meta), Editor-only runtime probe.
- This review, `checks.txt`, and `telegraph.png`.

Modified:
- `Assets/Scripts/Enemy/EnemyContactDamage.cs`: wind-up, impact/cooldown, reset interface, query reuse.
- `Assets/Scripts/Enemy/EnemyFollow.cs`: health and wind-up movement guards.
- `Assets/Scripts/Player/CombatFeedback.cs`, `DamageFeedback.cs`: clear stale visuals on disable.
- `Assets/Scripts/Player/AttackFeedback.cs`, `DashFeedback.cs`: independently owned child renderers,
  disable cleanup and material teardown. Regression exposed their previous shared renderer conflict.
- `Assets/Scenes/GameScene.unity`: six child `Attack Telegraph` GameObjects, each with
  LineRenderer + EnemyAttackTelegraph; six EnemyContactDamage serialized references/wind-up values.
  Room, Health and TimeLoopActor references/behavior retained.
- `ROADMAP.md`: M5.1 DONE; next milestone M5.2.

## Lean verification

Compile passed. `EnemyBasePlayModeChecks` was attached via MCP only during Play Mode:
19 checks PASS for proximity warning before damage, chase stop, pause, dash escape, two-collider
deduplication, cooldown, live rewind cancellation, Ghost spawn, directional Player damage,
death cancellation, room clear/unlock, revival HP/color/scale, combat relock, Ghost damage,
independent Ghost renderers and transition cleanup.

Existing `RoomTimeLoopInteractionPlayModeChecks`: 20 checks PASS for room-local recording/rewind,
Ghost history and health restoration, room transition isolation, fresh timeline, dual-switch
Ghost cooperation, pause, Restart and Main Menu. Full messages are in `checks.txt`.
Rewinds were invoked directly to keep this pass lean; no new full 20-second-loop endurance run.
The screenshot stages a 65% wind-up in paused Guard Hall to inspect circle geometry/color;
it is a camera capture without the overlay HUD, not proof of mobile readability.

Issues encountered and resolved/isolated:
- Initial scripts-only compile missed the new source; full asset refresh/import resolved it.
- Ghost attack raised LineRenderer index-out-of-bounds because dash reused the same renderer
  with two vertices while attack required three. Fixed with separate child renderers; retested.
- MCP screenshot emitted five recursive PlayerLoop errors. All stack traces pointed to
  `MCPForUnity.Runtime.Helpers.ScreenshotUtility.CaptureCompositedAfterFrame`, line 197, through
  the MCP screenshot command. After documenting and clearing these, a fresh Play Mode run of
  all 19 M5.1 checks passed without screenshot capture. Final Console: 0 errors / 0 warnings.
- `git diff --check` reports only Unity-generated empty `m_Name: ` trailing spaces in scene YAML;
  scene was saved by Unity MCP, not manually reformatted.
- MCP briefly disconnected during the final post-Play save and logged a WebSocket initialization
  warning. Reconnected and retried scene save successfully; cleared this documented transport warning.

Final scene inspection: 6 enemies, 6 telegraphs, 6 valid serialized telegraph references,
0 missing scripts, 0 saved test probes. Exited Play Mode and saved GameScene. No commit/push.

## Manual review and limits

- On Android, move + attack/dash simultaneously and judge whether 0.45-second warning is readable.
- Step fully outside the circle (including the Player collider) or dash away before impact;
  staying in contact should cost 20 HP per completed attack.
- Kill a guard, wait for the natural rewind, and verify clean revival/warning; clear both guards
  and walk through the exit to Echo Chamber.
- Check attack/dash visual and audio feel for Player and Ghost on the device.

Touch, final difficulty tuning, Android performance/build and final art remain unverified.
Death still uses immediate deactivation; a longer death animation/audio tail is future polish.
MCP screenshot recursion is a tooling limitation; package/generated files were not modified.
Next: M5.2 — Enemy 1: Chaser / Slime, only after this checkpoint is reviewed.
