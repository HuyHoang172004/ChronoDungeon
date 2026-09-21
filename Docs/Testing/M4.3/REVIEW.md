# M4.3 — Tilemap / Environment

Verified 2026-09-21 in Unity 6000.6.0f1 through Unity MCP. This milestone completes the authored room floor/wall blockout and physical containment for the eight-room layout.

## Implemented behavior

Each room Content now owns a `Solid Room Boundaries` shell with five BoxCollider2D segments:

- North wall
- South wall
- West wall
- East upper wall
- East lower wall

The east wall is split to keep the authored exit opening at the room's marked portal. The shell is serialized in GameScene and belongs to the room Content, so inactive rooms carry no active physics into the current encounter.

`RoomBoundary` exposes configuration for the shell and validates the five-segment shape. `PlayerDash` now casts the player's Rigidbody2D before moving and shortens the dash to the nearest solid hit. Normal Rigidbody2D movement collides with the same walls. The old shared `Environment` root is disabled in favor of per-room floors and trim created for M4.2; cyan temporal paths and amber hazard blockout preserve the visual language.

## Files and scene changes

New:

- [RoomBoundary.cs](../../../Assets/Scripts/Environment/RoomBoundary.cs) and Unity meta file.
- [RoomEnvironmentPlayModeChecks.cs](../../../Assets/Tests/RoomEnvironmentPlayModeChecks.cs) and Unity meta file.
- `Docs/Testing/M4.3/playmode-results.txt`.

Modified:

- [PlayerDash.cs](../../../Assets/Scripts/Player/PlayerDash.cs): Rigidbody2D cast and collision-safe dash distance.
- [GameScene.unity](../../../Assets/Scenes/GameScene.unity): eight RoomBoundary components and serialized five-segment shells.
- [ROADMAP.md](../../../ROADMAP.md): M4.3 result and next milestone M4.4.

No Settings, Exit/platform, trap, upgrade, elite or boss gameplay was added. Existing M4.1/M4.2 files remain in the worktree and were not removed.

## Verification

**76 checks PASS** in `RoomEnvironmentPlayModeChecks`; [full output](playmode-results.txt).

The test verifies eight rooms remain configured, each has five boundary segments and collision content, each room can be entered with its own active Content, west movement is contained, east dash is stopped outside the exit gap, the east opening remains clear, pause freezes the loop, Restart restores the first room and dash state, and Main Menu still unloads the environment. The test deliberately disables Enemy movement/contact to isolate environment behavior.

Final inspection found 8 rooms, 8 RoomBoundary components, no missing scripts and a clean saved scene. Play Mode final Console: **0 errors, 0 warnings**. `git diff --check` is clean for edited source/docs.

## Manual review and limits

1. Enter GameScene and walk around each room edge; confirm the Player cannot leave the floor through north, south or west walls.
2. Dash into an upper east wall and into the marked exit opening. The upper wall should stop the dash; the open portal should remain traversable when its Room exit is unlocked.
3. Test the full route with joystick, attack and dash on Android. MCP does not simulate real multitouch or device aspect ratios.

The environment is an authored SpriteRenderer blockout rather than a final Unity Tilemap asset. Decorative art, solid geometry polish and any moving/trap hazard are still separate work. Collision applies to room Content; the current per-room exit progression still controls whether a portal advances. The existing room-local Ghost/timeline behavior remains partially deferred to M4.4 for a full audit after environment integration.

Next proposed milestone: **M4.4 — Room + Time Loop Interaction**.
