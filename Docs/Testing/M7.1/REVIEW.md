# M7.1 Review

## Delivered

- Added reusable `UpgradeData` and `UpgradeType` definitions.
- Added `UpgradeManager` with Attack Damage, Movement Speed, Max HP, Dash Cooldown and Heal effects.
- Wired the manager to the GameScene player and added `PlayerDash` to the player setup.
- Rewind preserves upgrades during the current run; `GameManager.Restart()` resets baseline stats and stacks.

## Verification

- `UpgradeFrameworkPlayModeChecks`: **4/4 PASS**.
  - clean current-run start
  - four upgrade effects apply
  - loop rewind preserves upgrades
  - explicit new-run reset restores baseline
- Compile completed successfully.
- Final Console: 0 errors, 0 warnings.
- Scene saved; Play Mode exited.

## Manual follow-up

- M7.2 will add the 1-of-3 choice UI and mobile touch validation.
- Upgrade balance and Android performance remain to be tuned during progression integration.
