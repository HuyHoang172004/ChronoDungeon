# M6.1 - Dual Pressure Switch review

## Status

Verified from the existing M2.3 implementation. No duplicate puzzle system was added.

## Behavior

`PressureSwitch` reconciles occupants from explicit `PressureSwitchActor` components. `DualPressureDoor`
opens its referenced `Door` only when both switches are active. `GhostPlayback` uses a replay anchor, so a
Ghost can hold switch A while the current Player activates switch B. Door visuals and collider synchronize
with the solved state, and the existing time-loop reset clears both switches, the gate, Door, and Ghost state.

## Verification evidence

- `DualPressurePuzzlePlayModeChecks`: **16/16 PASS**.
- Monitored frames: 14,351; worst Ghost position error: 0.
- Covered A-alone locked state, Ghost replay, B activation, open/close/reopen, rewind reset, combat regression,
and Restart cleanup.
- Existing M2.3 evidence: [playmode-results.txt](../M2.3/playmode-results.txt).

Unity MCP was unavailable during this checkpoint refresh (editor ping did not answer), so no new Play Mode run
was started in this turn. The prior recorded run is the current verification evidence. Touch/multitouch and
Android device testing remain manual follow-up.
