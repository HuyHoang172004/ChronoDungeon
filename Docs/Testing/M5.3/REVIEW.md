# M5.3 — Ash Archer

Verified 2026-09-22 in Unity 6000.6.0f1 via Unity MCP.

## Result

Split Bastion now contains two Ember Chasers and one Ash Archer. The Archer is the second normal
combat archetype: it creates distance, locks a readable firing direction, and launches finite
projectiles. Existing Health, EnemyAttackTarget, Room, TimeLoopActor and Ghost attack systems
remain the shared foundation.

| Setting | Value |
| --- | --- |
| HP | 50 |
| Arrow damage / speed | 15 / 7 |
| Attack range | 9 |
| Preferred distance | 3–5 |
| Wind-up | 0.75 seconds |
| Cooldown | 1.3 seconds |
| Projectile lifetime | 3 seconds |
| Pool capacity | 3 |

`EnemyArcher` retreats when Player is closer than 3 and approaches when farther than 5. It uses
line-of-sight checks and locks `LockedDirection` for the wind-up, so moving sideways can dodge.
`EnemyProjectile` uses a swept CircleCast each physics step, avoids tunnelling, stops at walls,
damages only the Player, and returns to the pool on impact, expiry, invalid target or reset.
`EnemyProjectilePool` creates three arrows once, rejects a fourth while full, and resets all slots
on rewind, disable and room transition. The aim line now shortens at the first blocking wall.

New files:

- `Assets/Scripts/Enemy/EnemyArcher.cs`
- `Assets/Scripts/Enemy/EnemyProjectile.cs`
- `Assets/Scripts/Enemy/EnemyProjectilePool.cs`
- `Assets/Prefabs/EnemyArrow.prefab`
- `Assets/Prefabs/AshArcher.prefab`
- `Assets/Tests/ArcherPlayModeChecks.cs`
- `Docs/Testing/M5.3/checks.txt`
- `Docs/Testing/M5.3/ash-archer.png`

Modified: `Assets/Scenes/GameScene.unity` (one Split Bastion Chaser replaced with Ash Archer,
Health=50, projectile/aim references, hood/face/eyes/bow visual); `ROADMAP.md`. Prior M5.1/M5.2
uncommitted changes remain in the working tree and are intentionally preserved.

## Verification

Compile passed after the final aim-line wall-shortening patch. The Editor-only probe ran in Play Mode
and logged 20 PASS checks:

- mixed roster and three-slot pool configured;
- visible wind-up, locked heading, one arrow launch, pause freeze and sidestep dodge;
- stationary Player takes exactly 15 damage and arrow returns to pool;
- retreat/approach distance management;
- fourth simultaneous arrow rejected without allocation;
- swept collision stops at wall and lifetime releases slot;
- in-flight arrows, aim state and Archer health reset on rewind;
- Player and Ghost attacks damage Archer;
- death clears actor-owned arrows;
- mixed room clears, opens exit, and transition leaves no arrows.

The existing M5.1 regression probe also passed 19 checks after the Archer integration. Final scene
inspection: `compiling=false`, `scriptCompilationFailed=false`, `missingScripts=0`, `probes=0`,
`dirty=false`, one Archer present and zero runtime projectile instances after Play Mode. Console
after the final run: 0 errors and 0 warnings. A camera screenshot was captured in Split Bastion
with aim line and arrow staged; MCP screenshot recursion did not recur in this capture.

Rewinds in the probe call the existing private rewind path directly to keep the milestone lean; no
new natural 20-second endurance run or Android build was performed.

## Manual review and limits

On Android, approach the Archer and read the aim line, sidestep during the 0.75-second warning,
then deliberately stay in the line to confirm damage. Test movement plus attack/dash touch input.
In Split Bastion, defeat the Archer and two Chasers, walk through the exit, then wait for a natural
rewind and confirm no arrows remain. Check that the arrow and aim line are visible at phone scale.

Direct Rigidbody2D movement is intentional for this authored room; there is no navigation around
complex obstacles. Touch, Android performance/build, audio balance and final difficulty tuning are
still unverified. Archer visuals are assembled from existing primitive sprites and are not final art.
Warden remains a stand-in; M5.4 will add the third normal archetype.

No commit or push. Stop at M5.3 for review. Next: M5.4 — Knight or Mage.
