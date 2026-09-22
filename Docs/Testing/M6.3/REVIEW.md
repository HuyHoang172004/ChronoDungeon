# M6.3 - Combat Cooperation review

## Implemented

- `CombatCooperationTarget` tracks whether a target was damaged by a Ghost or by the current Player.
- `CombatCooperationObjective` requires both roles and updates an orange/mint indicator.
- `AttackResolver` now carries an explicit `fromGhost` source flag; the existing Player path remains unchanged.
- Ghost playback passes `fromGhost=true`.
- Split Bastion uses Ember Chaser 1 as a stationary Ghost target and Ember Chaser 2 as the mobile Player target.

## Verification

- `CombatCooperationPlayModeChecks`: **5/5 PASS**.
- Covered distinct targets, Player-only incomplete state, Ghost replay damage, complementary Player damage,
and rewind reset of both role flags.
- Regression `EnemyBasePlayModeChecks`: **19/19 PASS**.
- Compile clean; final Console: **0 errors, 0 warnings**.
- Scene saved, test runner removed, Play Mode exited.

Touch/Android and final combat balance remain manual follow-up.
