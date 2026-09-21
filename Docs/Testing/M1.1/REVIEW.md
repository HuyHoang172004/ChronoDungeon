# M1.1 review — 2026-09-21

Status: DONE for M1.1 acceptance in Unity Editor 6000.6.0f1, GameScene.
Android touch verification remains manual. No Git commit or push performed.

## Implementation

- `PlayerTimeline`: bounded, reusable pose and action buffers; binary-search pose evaluation, position/rotation interpolation and final-pose hold.
- `PlayerTimelineRecorder`: Player-only component; samples scaled loop time, world position/rotation and sprite flips every approximately 0.05 seconds. Slow frames record actual timestamps rather than fabricating historical poses. Two buffers of 403 poses each for the current 20-second configuration, plus bounded action storage. No per-frame allocation in recording/playback.
- `GhostPlayback`: reuses Player sprite/material with translucent purple tint; evaluates the shared clock. No input, Health, collider, Rigidbody or recorder.
- `TemporalGhostManager`: lazily creates one visual Ghost after the first rewind, reuses it for the previous loop's recording. Multiple Ghost history is deferred to M1.2.
- `TimeLoopManager`: exposes elapsed/duration/running state and invokes `LoopEnding` before actor reset. Existing reset flow and `LoopRewound` remain in use.
- Action event types are extension points only; gameplay action recording/playback belongs to M1.3.

## Scene changes through Unity MCP

- `GameObjects/Player`: added `PlayerTimelineRecorder`, linked `Managers/TimeLoopManager`, sample interval 0.05.
- `Managers`: added `TemporalGhostManager`, linked loop, recorder and Player SpriteRenderer; tint RGBA (0.75, 0.65, 1, 0.5).
- Runtime-only `Temporal Ghost`: Transform, SpriteRenderer, GhostPlayback.
- Editor test probe was removed after testing. GameScene saved in Edit Mode. No prefab changes; no missing scripts found.

## Verification

`Assets/Tests/TemporalGhostPlayModeChecks.cs` is an editor-only integration probe, excluded from player builds by `UNITY_EDITOR`. It is not attached to the saved scene.
To repeat: open saved GameScene in Edit Mode, create a temporary GameObject with this component, enter Play Mode with Game View focused/background execution enabled, and wait for the pass summary. The probe temporarily disables EnemyFollow for the safe recording path, then restores it to test combat. Exit Play Mode and remove the temporary object without saving it into the scene.

22 checks passed:

1. Loop 1 recording starts without Ghost.
2. Position and rotation midpoint interpolation.
3. Final pose and flip held beyond last sample timestamp.
4. Actual PlayerMovement follows a nontrivial path.
5. Pause freezes timer and recording.
6. Enemy death deactivates actor.
7. Natural 20-second rewind without scene reload.
8. Player pose and HP restoration.
9. Dead Enemy pose, HP and activation restoration.
10. Loop 2 Ghost uses Loop 1 recording.
11. Last recorded pose precedes rewind teleport.
12. Ghost has no damage/input/recording/collision components.
13. Ghost sprite and translucent tint exist.
14. Pose count stays within configured capacity.
15. Player moves independently while Ghost replays.
16. Pause freezes Ghost and shared clock.
17. 782 frame comparisons against recorded timeline: maximum position error 0.
18. Ghost holds stationary recorded endpoint.
19. Player attack damages Enemy, not Player.
20. Enemy contact damages Player.
21. Player death triggers Game Over.
22. Game Over freezes clock, recording and Ghost.

Further MCP checks: GameManager.Restart reloads GameScene at Loop 1 with fresh recording, no Ghost and timeScale 1. GameManager.MainMenu returns to MainMenuScene. These invoked game APIs, not physical touchscreen buttons.

Compilation succeeded. Final Console reads after Play Mode tests and scene navigation contained zero errors/warnings. An earlier MCP WebSocket reconnect warning occurred during refresh; it was a tooling warning, and subsequent calls/tests succeeded. Some MCP component-inspection/add calls failed with invocation errors; configuration was completed through MCP `execute_code` using Unity Editor APIs and verified in the saved scene diff.

Screenshot: [Game Over with frozen Ghost](game-over-ghost.png). This confirms the purple Ghost is visible separately from the cyan Player; final art remains outside this milestone.

`git diff --check` reported only two trailing spaces on Unity-generated empty `m_Name:` scene fields. Scene YAML was not hand-edited to normalize Unity serialization.

## Manual review

1. Move with the joystick along a visible path, release, wait for rewind. In Loop 2 move elsewhere and observe the translucent purple Ghost retrace the old path.
2. Repeat with direction changes and stops; judge smoothness/readability on the target Android device.
3. Test movement + Attack simultaneously using real touches; verify Ghost does not follow current joystick input or take damage.
4. Use the visible Game Over Restart/Main Menu buttons. API navigation passed; physical touch hit targets remain unverified.

Current Player has no directional-facing mechanic; timeline preserves transform rotation and sprite flips. No Ghost attack, puzzle interaction, dash, final art or multiple-Ghost history is claimed. Next milestone: M1.2, only after user requests continuation.
