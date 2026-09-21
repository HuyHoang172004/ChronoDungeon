# M2.3 — Dual-Switch Puzzle

Status: DONE for M2.3 acceptance, verified 2026-09-21 in Unity 6000.6.0f1.

`DualPressureDoor` is a small reusable mechanism adapter. It receives two `PressureSwitch` references and one `Door` reference; it opens the output only when both inputs are active. It has no scene-name lookup or hard-coded switch/door identity. Existing PressureSwitch occupancy, Ghost anchor playback, Door collider/visual synchronization and TimeLoop reset remain responsible for their own state.

GameScene now contains a second PressureSwitch prefab instance (`Pressure Switch B`) and a `Dual Pressure Door` mechanism reference. The previous direct first-switch-to-Door listener was removed, so A alone cannot open the Door.

## Verification

**16 checks PASS**, 14,351 monitored frames, maximum Ghost movement error **0**. Loop 1 Player held A alone and the Door stayed locked; after rewind Ghost 1 held A while the current Player activated B and the Door opened. Leaving B closed it while Ghost A remained; returning reopened it. Rewind reset both switches, the dual gate and Door. Player combat remained valid. Restart cleared puzzle, Door and Ghost state. Raw output: [playmode-results.txt](playmode-results.txt).

Unity compiled cleanly. Final Console after stopping Play Mode: **0 errors, 0 warnings**. GameScene was saved outside Play Mode with no test probe saved. No package, `.gitignore`, commit or push changes.

Touch/multitouch and Android device passage still need manual testing because Unity MCP does not simulate them reliably. M2.4 Puzzle Feedback was not started.

Next milestone after review: M2.4 — Puzzle Feedback.
