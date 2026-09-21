# M2.2 — Door

Status: DONE for M2.2 acceptance, verified 2026-09-21 in Unity 6000.6.0f1.

`Door` is a reusable prefab/component with no reference to a specific switch. `Open`, `Close`, and `SetOpen(bool)` are public; `StateChanged(bool)` and serialized `UnityEvent<bool>` allow PressureSwitch or another future mechanism to drive it. `IsOpen`, `IsLocked`, and `IsClosePending` expose state for room logic.

The BoxCollider2D is solid while locked and disabled while open. Closed panel visibility and status-light color are updated in the same state transition. A close request while a dynamic Rigidbody2D occupies the doorway is deferred until the passage clears, preventing collider/state desync or trapping an actor. Repeated state commands do not emit duplicate events. `ITimeLoopResettable` restores the authored initial state on rewind; the scene door starts locked.

The existing PressureSwitch Inspector event is wired to `Door.SetOpen(bool)` through Unity MCP. No door-specific code was added to PressureSwitch. Ghosts continue using their non-physical `PressureSwitchActor` anchor and do not need a collider to operate the existing event chain.

## Verification

**25 checks PASS**, 8,204 monitored frames, maximum Ghost movement error **0**, 12 Door state transitions. Passed: closed physical block, open passage, generic bool trigger, visual/collider synchronization, duplicate-command suppression, safe deferred close for Player and Enemy, PressureSwitch event connection, natural rewind reset, Ghost switch/attack replay, combat, Game Over freeze and Restart cleanup. Raw output: [playmode-results.txt](playmode-results.txt).

Unity compiled cleanly. Final console after stopping Play Mode: **0 errors, 0 warnings**. The earlier Unity MCP WebSocket message was transient during an uninitialized connection; it was absent from the final Console. The first Play Mode request was rejected by automatic approval because the usage quota was exhausted; a subsequent retry executed the complete test successfully.

Final state: GameScene saved via Unity MCP, outside Play Mode, reusable `Assets/Prefabs/Door.prefab` saved, PressureSwitch and Door references present, no test probe or missing script saved into the scene. No packages, `.gitignore`, commit or push changed.

Mobile touch and Android device testing remain manual because Unity MCP cannot reliably simulate multitouch. M2.3 Dual-Switch Puzzle was not started.

Next milestone after review: M2.3 — Dual-Switch Puzzle.
