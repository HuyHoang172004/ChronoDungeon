# M4.1 — Room Architecture

Verified 2026-09-21 using Unity MCP, Unity 6000.6.0f1.

## Scope and behavior

- Two sequential encounters in the existing arena: defeat the guard, then solve the dual pressure switch puzzle.
- Room lifecycle: NotEntered → Active → Completed → Exited. Completion opens a dedicated room exit. The puzzle's original door remains independently controlled by DualPressureDoor.
- Room clear latches for the current loop so the Player can leave the plates and reach the exit. A natural rewind resets the active encounter and locks its exit again.
- Only the current Player can transition; inactive/stale rooms, uncleared rooms, paused gameplay and repeated completion cannot advance progression.
- Room entry preserves current HP, moves to the authored spawn, clears previous Ghosts and begins a fresh 20-second Loop 1 recording. Only Player and current-room resettable components participate in subsequent rewinds.
- Last exit sets RoomManager.IsComplete and stops the encounter timer. Victory presentation is deferred to its own milestone.
- M0.1 is documented as foundation DONE. Settings belongs to M9.3; final Exit/platform QA belongs to Phase 11. The temporary Settings/Exit edits from this session were removed; MainMenu.cs and MainMenuScene have no content diff.

## Changed files and scene

New scripts (plus Unity-generated .meta files):

- Assets/Scripts/Managers/Room.cs
- Assets/Scripts/Managers/RoomManager.cs
- Assets/Scripts/Environment/RoomExit.cs
- Assets/Scripts/UI/RoomHUD.cs
- Assets/Tests/RoomArchitecturePlayModeChecks.cs (Editor-only runtime probe)

Modified:

- Assets/Scripts/Managers/TimeLoopManager.cs: scoped BeginEncounter and EncounterStarted event; IsRunning respects enabled state.
- Assets/Scripts/Temporal/PlayerTimelineRecorder.cs: new recording on encounter entry.
- Assets/Scripts/Temporal/TemporalGhostManager.cs: clears history on encounter entry.
- Assets/Scenes/GameScene.unity: edited and saved through Unity MCP.
- ROADMAP.md: M0.1 scope clarification, M4.1 results, M4.4 dependency note, next milestone M4.2.

GameObjects/components:

- Managers gains RoomManager with serialized Player, loop and ordered Room references.
- Rooms/01 Combat Room and Rooms/02 Temporal Puzzle Room each have Room, Content, Player Spawn, Room Exit Door, Exit Trigger (RoomExit + trigger BoxCollider2D) and an EXIT label.
- Existing Enemy is reparented into combat Content. Existing switches, Temporal Door, Dual Pressure Door and feedback are reparented into puzzle Content. Their existing gameplay references are preserved.
- Canvas gains Room HUD (TMP text + RoomHUD). Position was adjusted after visual inspection to avoid the existing puzzle hint.
- Probe was injected only during Play Mode and is not saved in the scene.

## Verification

30 checks PASS. Full output: [playmode-results.txt](playmode-results.txt).

Covered combat clear/unlock, duplicate-completion protection, wrong actor/stale source rejection, pause, natural 20-second combat rewind, Enemy restoration, Ghost creation, physical exit trigger, HP preservation, new spawn/timer/timeline, old-room deactivation, second natural rewind, Ghost A + Player B puzzle completion, latched exit, final completion, Restart, Game Over, Restart after death and Main Menu cleanup.

Test's deprecated FindObjectsByType overload was replaced with the verified Unity 6.6 overload. Compile succeeded afterward. Additional Play Mode check confirmed transition and adjusted HUD rendering. Final Console: 0 errors, 0 warnings. A transient MCP WebSocket warning during domain reload did not recur in final Play Mode.

Final screenshot: [puzzle-room.png](puzzle-room.png). Scene saved and Editor left outside Play Mode. No commit/push.

## Manual review and limits

1. Play GameScene: defeat Enemy; verify cyan exit opens and walking through it enters the temporal puzzle.
2. Stand on left plate A until rewind; move Player to B while Ghost holds A. Verify exit unlocks, remains open when leaving B, and crossing it shows ENCOUNTERS COMPLETE.
3. Repeat with joystick + attack/dash on Android; confirm touch usability, hit feedback, readability and perceived transition quality. MCP checks used API/physics placement, not real multitouch.
4. Confirm Restart and Main Menu using the visible UI on device.

This milestone provides two encounters sharing an arena, not the 6–8-room dungeon (M4.2). Full walls/boundaries remain M4.3; the existing instantaneous dash can bypass geometry and is not fixed here. Existing mobile control layout, including the oversized Dash button visible in the screenshot, remains a separate UX limitation. No Android build/device test, final Victory UI, Settings or Exit implementation was added.

M4.4 must validate room/time-loop policy across the complete authored dungeon. Next proposed milestone: M4.2 — Dungeon Layout.
