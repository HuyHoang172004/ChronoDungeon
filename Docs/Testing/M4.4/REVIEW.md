# M4.4 — Room + Time Loop Interaction

Verified 2026-09-21 in Unity 6000.6.0f1 through Unity MCP. This milestone locks the temporal policy for authored rooms and verifies it after the M4.3 environment collision work.

## Policy

Each authored room is a local temporal encounter:

1. `RoomManager` enters a room and moves the Player to that room's spawn.
2. `TimeLoopManager.BeginEncounter` clears its resettable list, captures the Player and current room, resets to Loop 1 and raises `EncounterStarted`.
3. `PlayerTimelineRecorder` clears and starts a new local timeline. `TemporalGhostManager` clears pending and active Ghost history.
4. A rewind records the current loop, resets only the current encounter, returns the Player to that room's spawn and spawns the Ghost for that completed loop.
5. A room transition disables old Content, enables new Content, preserves Player HP, starts another local Loop 1 and removes all old Ghost history.

This means an old Ghost cannot press a switch, attack an enemy or consume timeline state in a later room. The current room stays active until its physical exit is reached. Pause freezes both timer and Ghost state.

## Files and implementation

New:

- [RoomTimeLoopInteractionPlayModeChecks.cs](../../../Assets/Tests/RoomTimeLoopInteractionPlayModeChecks.cs) and Unity meta file.
- `Docs/Testing/M4.4/playmode-results.txt`.

Existing implementation verified and retained:

- [TimeLoopManager.cs](../../../Assets/Scripts/Managers/TimeLoopManager.cs): `BeginEncounter`, scoped resettable capture and `EncounterStarted`.
- [PlayerTimelineRecorder.cs](../../../Assets/Scripts/Temporal/PlayerTimelineRecorder.cs): local recording restart on `EncounterStarted`.
- [TemporalGhostManager.cs](../../../Assets/Scripts/Temporal/TemporalGhostManager.cs): pending/active Ghost cleanup on `EncounterStarted`.
- [RoomManager.cs](../../../Assets/Scripts/Managers/RoomManager.cs): room entry, spawn, Content isolation and transition ordering.
- [Room.cs](../../../Assets/Scripts/Managers/Room.cs): room-local reset and clear state.
- [GameScene.unity](../../../Assets/Scenes/GameScene.unity): eight-room authored route and collision boundaries.
- [ROADMAP.md](../../../ROADMAP.md): M4.4 result and next milestone.

No new scene GameObject was saved for the test probe. No Settings, enemy archetype, trap, upgrade or boss gameplay was added.

## Verification

**20 checks PASS** in `RoomTimeLoopInteractionPlayModeChecks`; [full output](playmode-results.txt).

The test records combat action, forces room-local rewind, confirms Loop 2/Ghost 1/source Loop 1, verifies Player spawn and enemy health restoration, then enters the puzzle room and confirms old Content is inactive, HP is preserved, Ghost count is zero and the new timeline is Loop 1. It verifies no stale Ghost pressure, then performs the new room's Ghost A + Player B puzzle, confirms the exit stays current until physical exit, tests pause, Restart and Main Menu cleanup.

Final Play Mode Console: **0 errors, 0 warnings**. Scene was left outside Play Mode. The probe was created at runtime only and was not saved into the scene.

## Manual review and limits

1. Run from Guard Hall into Echo Chamber. Make one action, wait for rewind, and confirm the Ghost appears only in that room.
2. Enter the next room and confirm the previous Ghost disappears, the loop counter/timeline restart at Loop 1, and the Player HP remains unchanged.
3. On Android, repeat with joystick + attack/dash while crossing room exits. MCP does not simulate real multitouch or device behavior.

The policy is fully verified across the existing combat/puzzle transition, but future Trap, Treasure, Elite and Boss systems must implement their own resettable state and be included in regression tests. Next proposed milestone: **M5.1 — Enemy Base Improvements**.
