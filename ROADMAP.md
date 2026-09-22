# ROADMAP.md â€” Lá»™ trÃ¬nh phÃ¡t triá»ƒn hoÃ n chá»‰nh ChronoDungeon

## Quy Æ°á»›c tráº¡ng thÃ¡i

- [x] DONE
- [~] IN PROGRESS / PARTIAL
- [ ] NOT STARTED
- [!] BLOCKED

Codex chá»‰ Ä‘Æ°á»£c cáº­p nháº­t tráº¡ng thÃ¡i sau khi verify project thá»±c táº¿.

KhÃ´ng mark milestone DONE chá»‰ vÃ¬ code Ä‘Ã£ tá»“n táº¡i.  
DONE nghÄ©a lÃ  feature Ä‘Ã£ compile, test vÃ  integrate mÃ  khÃ´ng cÃ²n blocking error Ä‘Ã£ biáº¿t.

---

# PHASE 0 â€” Foundation

## M0.1 Project / Scene Flow
**[x] DONE â€” foundation scene flow; scope clarified 2026-09-21.**

- [x] SplashScene
- [x] MainMenuScene
- [x] GameScene
- [x] Splash -> Main Menu
- [x] Play -> GameScene
- Settings implementation: **DEFER sang M9.3**, khÃ´ng pháº£i blocker cá»§a Room System.
- Final Exit/platform verification: **DEFER sang Phase 11 / final platform QA**, khÃ´ng pháº£i blocker cá»§a Room System.

## M0.2 Mobile Movement
- [x] Player Rigidbody2D movement
- [x] VirtualJoystick
- [x] Android-oriented landscape UI foundation
- [ ] final device touch verification
- [ ] final responsive control layout

## M0.3 Combat Foundation
- [x] PlayerAttack
- [x] reusable Health
- [x] Enemy HP
- [x] Enemy HealthBar
- [x] Player HP
- [x] Player HUD HealthBar
- [x] Enemy contact damage
- [x] Game Over
- [x] Restart
- [x] Main Menu return

## M0.4 Time Loop Foundation
- [x] Time Loop 20 giÃ¢y
- [x] Loop HUD
- [x] loop counter
- [x] Player reset
- [x] Enemy reset
- [x] resettable architecture
- [x] dead Enemy restoration
- [x] Game Over interaction vá»›i timer
- [x] khÃ´ng reload scene khi normal rewind

---

# PHASE 1 â€” Temporal Ghost Core

## M1.1 â€” Ghi vÃ  Replay chuyá»ƒn Ä‘á»™ng

**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

Má»¥c tiÃªu:  
Biáº¿n lá»‹ch sá»­ chuyá»ƒn Ä‘á»™ng cá»§a Player thÃ nh Ghost nhÃ¬n tháº¥y Ä‘Æ°á»£c sau rewind.

YÃªu cáº§u:
- [x] timeline data structure
- [x] Player timeline recorder
- [x] ghi time + position
- [x] ghi facing/rotation khi phÃ¹ há»£p
- [x] sample interval há»£p lÃ½
- [x] Ghost visual
- [x] Ghost playback
- [x] smooth interpolation
- [x] Ghost Ä‘á»™c láº­p vá»›i joystick
- [x] Ghost khÃ´ng nháº­n Player damage
- [x] Ghost playback pause cÃ¹ng gameplay
- [x] Ghost giá»¯ tráº¡ng thÃ¡i tá»›i háº¿t loop
- [x] khÃ´ng ghi láº¡i Ghost nhÆ° Player

Acceptance:
- Loop 1 record Player.
- Loop 2 spawn Ghost1 replay Loop1.
- Player hiá»‡n táº¡i váº«n Ä‘iá»u khiá»ƒn Ä‘á»™c láº­p.

Verification: 22 automated checks trong Play Mode qua Unity MCP; rewind tá»± nhiÃªn
20 giÃ¢y, 782 frame so sÃ¡nh playback (sai sá»‘ vá»‹ trÃ­ Ä‘o Ä‘Æ°á»£c = 0), pause/Game Over,
Player/Enemy HP reset, Enemy death restoration, attack/contact damage Ä‘á»u PASS.
Restart vá» Loop 1, recording má»›i, khÃ´ng Ghost; Main Menu return PASS qua API.
Console cuá»‘i khÃ´ng error/warning; Ä‘Ã£ exit Play Mode vÃ  save GameScene, khÃ´ng missing script.

Giá»›i háº¡n M1.1: má»™t Ghost cá»§a loop ngay trÆ°á»›c, visual primitive tÃ­m bÃ¡n trong suá»‘t;
ghi rotation vÃ  sprite flip hiá»‡n cÃ³ (Player chÆ°a cÃ³ directional facing system).
Action channel chá»‰ lÃ  cáº¥u trÃºc má»Ÿ rá»™ng, chÆ°a record/replay attack/dash/interaction.
ChÆ°a xÃ¡c nháº­n touch/multitouch Android hoáº·c Ä‘á»™ mÆ°á»£t cáº£m nháº­n trÃªn thiáº¿t bá»‹.
Chi tiáº¿t vÃ  manual checklist: [M1.1 review](Docs/Testing/M1.1/REVIEW.md).

## M1.2 â€” Multiple Ghosts

**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] Ghost1 Ä‘áº¡i diá»‡n Loop1
- [x] Ghost2 Ä‘áº¡i diá»‡n Loop2
- [x] Ghost3 Ä‘áº¡i diá»‡n Loop3
- [x] maxGhosts = 3
- [x] loáº¡i Ghost cÅ© nháº¥t khi vÆ°á»£t giá»›i háº¡n
- [x] Restart xÃ³a recordings/Ghosts
- [x] Game Over khÃ´ng lÃ m há»ng recording state

Acceptance:  
Test Ã­t nháº¥t 4 láº§n chuyá»ƒn loop vÃ  khÃ´ng bao giá» cÃ³ hÆ¡n 3 active Ghost.

Verification: 81 checks PASS qua Unity MCP/Play Mode, bá»‘n rewind tá»± nhiÃªn
20 giÃ¢y liÃªn tiáº¿p vÃ  má»™t rewind sau Restart. Loop 2/3/4 láº§n lÆ°á»£t giá»¯ lá»‹ch sá»­
[1], [1,2], [1,2,3]; Loop 5 giá»¯ [2,3,4]. Theo dÃµi 38.107 frame: tá»‘i Ä‘a 3 Ghost,
sai sá»‘ vá»‹ trÃ­ so vá»›i recording lÆ°u riÃªng = 0. Má»—i Ghost giá»¯ snapshot riÃªng,
khÃ´ng bá»‹ recorder ghi Ä‘Ã¨; slot cÅ© nháº¥t Ä‘Æ°á»£c tÃ¡i sá»­ dá»¥ng khi Ä‘á»§ ba Ghost.

PASS: joystick pointer giáº£ láº­p Ä‘iá»u khiá»ƒn Player Ä‘á»™c láº­p; Ghost khÃ´ng cÃ³ input,
Health/collider/Player tag; Enemy overlap Ghost khÃ´ng gÃ¢y Player damage;
pause/Game Over giá»¯ nguyÃªn timeline; combat, Enemy chase/contact vÃ  reset HP/dead
Enemy hoáº¡t Ä‘á»™ng. Restart tá»« Game Over vÃ  khi Ä‘ang chÆ¡i xÃ³a lá»‹ch sá»­; rewind Ä‘áº§u
sau Restart táº¡o Ä‘Ãºng má»™t Ghost má»›i; Main Menu khÃ´ng cÃ²n Ghost tá»“n dÆ°.

ÄÃ£ exit Play Mode, gá»¡ probe, save GameScene; reference Ä‘áº§y Ä‘á»§, khÃ´ng missing script.
Unity native Console cuá»‘i: 0 errors, 0 warnings. ChÆ°a xÃ¡c nháº­n touch/multitouch
hay performance Android. Chi tiáº¿t: [M1.2 review](Docs/Testing/M1.2/REVIEW.md).

## M1.3 â€” Action Timeline
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

Má»Ÿ rá»™ng timeline mÃ  khÃ´ng viáº¿t láº¡i movement system.

Event support báº¯t buá»™c:
- [x] Attack event
- [x] facing/attack direction
- [x] Ghost attack playback
- [x] Ghost cÃ³ thá»ƒ damage Enemy há»£p lá»‡
- [x] Ghost attack khÃ´ng áº£nh hÆ°á»Ÿng Player
- [x] attack timing khá»›p timeline Ä‘Ã£ record

Kiáº¿n trÃºc pháº£i sáºµn sÃ ng cho:
- [x] Dash event (extension point; chÆ°a implement mechanic)
- [x] Interaction event (extension point; chÆ°a implement mechanic)
- [x] Skill event (extension point; chÆ°a implement mechanic)

Acceptance:  
Player attack trong Loop1.  
Ghost1 láº·p láº¡i attack gáº§n Ä‘Ãºng timeline moment Ä‘Ã³ trong Loop2.

Verification: 31 checks PASS qua Unity MCP/Play Mode, bá»‘n rewind tá»± nhiÃªn 20 giÃ¢y.
9 attack replay Ä‘Ãºng má»™t láº§n/event, sai lá»‡ch thá»i Ä‘iá»ƒm tá»‘i Ä‘a 0,0304 giÃ¢y (trong má»™t frame).
Theo dÃµi 4.715 frame, sai sá»‘ movement replay = 0. Player/Ghost gÃ¢y Ä‘Ãºng 25 damage/Ä‘Ã²n,
Enemy hai collider khÃ´ng bá»‹ damage láº·p; Player vÃ  Health khÃ´ng Ä‘Æ°á»£c Ä‘Ã¡nh dáº¥u Enemy Ä‘á»u an toÃ n.
Hai Ghost gÃ¢y tá»•ng 75 damage tá»« ba event; ba Ghost vÃ  eviction giá»¯ Ä‘Ãºng lá»‹ch sá»­,
slot tÃ¡i sá»­ dá»¥ng khÃ´ng replay attack cÅ©. Pause, Game Over vÃ  Restart PASS.

ÄÃ£ thoÃ¡t Play Mode, save GameScene; Enemy cÃ³ EnemyAttackTarget, maxGhosts = 3,
khÃ´ng missing script/probe lÆ°u trong scene. Console cuá»‘i: 0 errors, 0 warnings.
Táº¡i thá»i Ä‘iá»ƒm M1.3 attack cÃ²n radial; hÆ°á»›ng Ä‘Ã£ Ä‘Æ°á»£c record Ä‘á»ƒ má»Ÿ rá»™ng. Directional hit shape/VFX
Ä‘Æ°á»£c hoÃ n táº¥t trong M3.1.
Action buffer giá»›i háº¡n 256 event/loop; overflow cáº£nh bÃ¡o má»™t láº§n vÃ  bá» event vÆ°á»£t giá»›i háº¡n.
ChÆ°a test touch/Android. Chi tiáº¿t: [M1.3 review](Docs/Testing/M1.3/REVIEW.md).

---

# PHASE 2 â€” First Complete Time Puzzle

## M2.1 â€” Pressure Switch
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] reusable PressureSwitch
- [x] Player activate Ä‘Æ°á»£c
- [x] Ghost tÆ°Æ¡ng thÃ­ch activate Ä‘Æ°á»£c
- [x] visual state active/inactive rÃµ
- [x] resettable khi rewind

Prefab PressureSwitch dÃ¹ng opt-in PressureSwitchActor, Ä‘áº¿m actor Ä‘á»™c láº­p vá»›i sá»‘ collider.
Player dÃ¹ng collider overlap; Ghost dÃ¹ng vá»‹ trÃ­ replay náº±m trong plate, khÃ´ng thÃªm physics collider.
CÃ³ IsActive/OccupantCount, C# StateChanged(bool) vÃ  Inspector UnityEvent<bool> Ä‘á»ƒ ná»‘i mechanism sau.
Rewind reset inactive/zero occupants ngay láº­p tá»©c, Ä‘Ã¡nh giÃ¡ láº¡i á»Ÿ frame gameplay tiáº¿p theo.

Verification: 38 checks PASS qua Unity MCP/Play Mode, hai rewind tá»± nhiÃªn 20 giÃ¢y,
14.641 frame theo dÃµi, sai sá»‘ Ghost movement = 0. Player hai collider Ä‘áº¿m má»™t actor;
Player + Ghost Ä‘áº¿m hai; má»™t actor rá»i thÃ¬ actor cÃ²n láº¡i váº«n giá»¯ switch, khÃ´ng phÃ¡t false event.
PASS: collider disable/exit, actor disable/destroy, switch disable/re-enable, Enemy bá»‹ loáº¡i,
reset khi Player/Ghost giá»¯ plate, Ghost attack Ä‘Ãºng 25 damage má»™t láº§n vÃ  khÃ´ng damage Player,
Enemy chase/contact, Game Over/freeze, Restart vÃ  C#/Inspector events Ä‘á»“ng nháº¥t (18 transitions).

ÄÃ£ kiá»ƒm tra hÃ¬nh áº£nh active/inactive, save prefab vÃ  GameScene, thoÃ¡t Play Mode;
khÃ´ng probe/missing script trong scene, maxGhosts = 3. Console cuá»‘i: 0 errors, 0 warnings.
ChÆ°a test mobile touch/Android. Khi record nÃªn Ä‘á»©ng rÃµ trÃªn plate: Ghost dÃ¹ng anchor point,
Player dÃ¹ng collider nÃªn ngÆ°á»¡ng kÃ­ch hoáº¡t á»Ÿ mÃ©p khÃ¡c nhau. ChÆ°a triá»ƒn khai Door/M2.2.
Chi tiáº¿t: [M2.1 review](Docs/Testing/M2.1/REVIEW.md).

## M2.2 â€” Door
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] reusable Door
- [x] locked/open state
- [x] opening animation hoáº·c visual feedback rÃµ
- [x] reset support khi cáº§n
- [x] khÃ´ng trap Player vÃ¬ collider/state mismatch

Door cÃ³ API `Open`, `Close`, `SetOpen(bool)`, `IsOpen`, `IsLocked`, `StateChanged(bool)` vÃ 
Inspector UnityEvent<bool>; khÃ´ng hard-code switch/cá»­a cá»¥ thá»ƒ. Collider blocking vÃ  visual panel
Ä‘Æ°á»£c cáº­p nháº­t cÃ¹ng má»™t state. Lá»‡nh Ä‘Ã³ng khi actor cÃ²n trong passage Ä‘Æ°á»£c trÃ¬ hoÃ£n an toÃ n tá»›i khi
clear, trÃ¡nh káº¹t Player/Enemy. Rewind tráº£ vá» tráº¡ng thÃ¡i authored ban Ä‘áº§u; prefab reusable.

Verification: 25 checks PASS qua Unity MCP/Play Mode, 8.204 frame, sai sá»‘ Ghost movement = 0.
PASS: cá»­a Ä‘Ã³ng cháº·n Rigidbody2D Player, má»Ÿ cho Ä‘i qua, event bool Ä‘iá»u khiá»ƒn Ä‘Ãºng, repeated command
khÃ´ng phÃ¡t event trÃ¹ng, Ä‘Ã³ng an toÃ n khi Player/Enemy cÃ²n trong passage, PressureSwitch ná»‘i event
má»Ÿ/Ä‘Ã³ng Door, visual/collider Ä‘á»“ng bá»™, rewind reset Door + switch, Ghost replay switch/attack,
combat, Game Over/freeze vÃ  Restart. Console cuá»‘i: 0 errors, 0 warnings.

ÄÃ£ save GameScene vÃ  `Assets/Prefabs/Door.prefab`, exit Play Mode; khÃ´ng probe/missing script
Ä‘Æ°á»£c lÆ°u trong scene. ChÆ°a test touch/Android. ChÆ°a triá»ƒn khai M2.3.
Chi tiáº¿t: [M2.2 review](Docs/Testing/M2.2/REVIEW.md).

## M2.3 â€” Dual-Switch Puzzle
- **[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

Scenario báº¯t buá»™c:
- [x] Loop1 Player activate Switch A
- [x] rewind
- [x] Ghost1 activate Switch A
- [x] Player hiá»‡n táº¡i activate Switch B
- [x] Door má»Ÿ
- [x] Player Ä‘i qua Ä‘Æ°á»£c

Acceptance:  
Puzzle chá»‰ solve Ä‘Æ°á»£c khi dÃ¹ng Ã­t nháº¥t má»™t Ghost.

DualPressureDoor nháº­n hai PressureSwitch reference vÃ  má»™t Door reference; chá»‰ má»Ÿ khi cáº£ hai
switch active. KhÃ´ng hard-code scene object; mechanism cÃ³ thá»ƒ tÃ¡i sá»­ dá»¥ng vá»›i cáº·p input/output khÃ¡c.
Rewind reset cáº£ gate vÃ  Door vá» locked; PressureSwitch events tiáº¿p tá»¥c Ä‘iá»u khiá»ƒn state.

Verification: 16 checks PASS qua Unity MCP/Play Mode, 14.351 frame, sai sá»‘ Ghost movement = 0.
Loop 1 Player giá»¯ A khÃ´ng má»Ÿ cá»­a; Loop 2 Ghost giá»¯ A + Player giá»¯ B má»Ÿ cá»­a; rá»i B Ä‘Ã³ng cá»­a,
trá»Ÿ láº¡i B má»Ÿ láº¡i. Combat Player váº«n gÃ¢y Ä‘Ãºng 25 damage; rewind/reset vÃ  Restart PASS.
Console cuá»‘i: 0 errors, 0 warnings. ChÆ°a test touch/Android. ChÆ°a triá»ƒn khai M2.4.
Chi tiáº¿t: [M2.3 review](Docs/Testing/M2.3/REVIEW.md).

## M2.4 â€” Puzzle Feedback
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] switch SFX/VFX
- [x] door SFX/VFX
- [x] connection giá»¯a switch vÃ  door dá»… hiá»ƒu
- [x] basic tutorial hint cho temporal puzzle Ä‘áº§u tiÃªn

`PuzzleFeedback` lÃ  presentation-only adapter láº¯ng nghe PressureSwitch/Door events:
switch/door pulse, runtime tone feedback, LineRenderer ná»‘i hai switch vá»›i Door, vÃ  hint
"STAND ON BOTH TIME PLATES" / "ONE MORE SWITCH" / "TIME LINK COMPLETE". KhÃ´ng Ä‘á»•i puzzle logic.

Verification: 8 checks PASS qua Unity MCP/Play Mode, 257 frame; feedback adapter cÃ³ line/hint/audio,
inactive state Ä‘Ãºng, má»™t switch báº­t line + hint + pulse, hai switch hiá»ƒn thá»‹ solved connection vÃ 
Door-open feedback, release dá»n connection/hint, pause freeze vÃ  Restart cleanup PASS.
Console cuá»‘i: 0 errors, 0 warnings. ÄÃ£ save GameScene, exit Play Mode; chÆ°a test touch/Android.
Chi tiáº¿t: [M2.4 review](Docs/Testing/M2.4/REVIEW.md).

---

# PHASE 3 â€” Player Combat Polish

## M3.1 â€” Directional Combat
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] attack direction/facing
- [x] attack visual
- [x] hit feedback rÃµ
- [x] attack cooldown
- [x] ngÄƒn accidental multi-hit bug

`PlayerAttack` dÃ¹ng `PlayerMovement.FacingDirection`, ghi `ArcAngle` vÃ o `AttackSnapshot`
vÃ  dÃ¹ng cÃ¹ng payload cho Player/Ghost. `AttackResolver` lá»c má»¥c tiÃªu theo hÆ°á»›ng, váº«n deduplicate
Enemy cÃ³ nhiá»u collider. Cooldown máº·c Ä‘á»‹nh 0,35 giÃ¢y cháº·n swing trÃ¹ng; `AttackFeedback` hiá»ƒn thá»‹
slash line theo hÆ°á»›ng, cÃ²n `DamageFeedback` flash SpriteRenderer cá»§a Enemy khi trÃºng Ä‘Ã²n.

Verification: 10 checks PASS qua Unity MCP/Play Mode: hit phÃ­a trÆ°á»›c, khÃ´ng hit phÃ­a sau,
Ä‘á»•i hÆ°á»›ng, cooldown, slash visual, Enemy hit flash, timeline giá»¯ ArcAngle vÃ  pause giá»¯ nguyÃªn
combat/loop. Console cuá»‘i khÃ´ng cÃ³ error/warning má»›i; Ä‘Ã£ exit Play Mode vÃ  xÃ³a probe khá»i scene.
Regression M1.3 Ä‘Æ°á»£c giá»¯ nguyÃªn trong code path hiá»‡n táº¡i; touch/Android chÆ°a mÃ´ phá»ng Ä‘Ã¡ng tin cáº­y
qua MCP vÃ  cáº§n test thá»§ cÃ´ng trÃªn thiáº¿t bá»‹.

## M3.2 â€” Dash
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] Dash button
- [x] dash movement
- [x] cooldown
- [x] mobile-friendly behavior
- [x] visual feedback
- [x] Ghost timeline support náº¿u cáº§n

`PlayerDash` lÃ  component riÃªng, dÃ¹ng hÆ°á»›ng di chuyá»ƒn/facing hiá»‡n táº¡i, dash distance máº·c Ä‘á»‹nh
2,5 vÃ  cooldown 0,8 giÃ¢y. `DashButton` lÃ  UI adapter nháº­n pointer click trÃªn Canvas, tá»± hiá»ƒn thá»‹
tráº¡ng thÃ¡i ready/cooldown vÃ  táº¡o nhÃ£n DASH khi cháº¡y. `DashFeedback` hiá»ƒn thá»‹ vá»‡t cyan; Dash Ä‘Æ°á»£c
ghi vÃ o timeline vá»›i hÆ°á»›ng vÃ  distance payload. Ghost replay láº¡i action/feedback Ä‘Ãºng thá»i Ä‘iá»ƒm;
pose timeline giá»¯ quÃ£ng di chuyá»ƒn nÃªn khÃ´ng bá»‹ dash hai láº§n.

Verification: 11 checks PASS qua Unity MCP/Play Mode: movement theo hÆ°á»›ng, cooldown cháº·n láº·p,
cooldown há»“i phá»¥c, visual feedback, mobile button reference, pause freeze, natural rewind, Ghost
nháº­n timeline vÃ  replay Dash. Console cuá»‘i khÃ´ng cÃ³ error/warning má»›i; Ä‘Ã£ save GameScene, exit Play
Mode vÃ  xÃ³a probe khá»i scene. MCP khÃ´ng mÃ´ phá»ng touch Android Ä‘Ã¡ng tin cáº­y; cáº§n test tap Dash trÃªn
thiáº¿t bá»‹/emulator thá»§ cÃ´ng.

## M3.3 â€” Player Feedback
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] hit flash
- [x] damage feedback
- [x] attack SFX
- [x] damage SFX
- [x] death feedback
- [ ] optional restrained camera shake (deferred; khÃ´ng cáº§n cho acceptance)

`CombatFeedback` lÃ  component presentation dÃ¹ng chung cho Player vÃ  Enemy. `Health` phÃ¡t hit/death
feedback táº­p trung, `PlayerAttack` phÃ¡t attack SFX, cÃ²n `AttackResolver` giá»¯ hit flash/damage SFX
cho Enemy nhiá»u collider. Audio dÃ¹ng clip procedural ngáº¯n, khÃ´ng thÃªm package/asset dependency.

Verification: 10 checks PASS qua Unity MCP/Play Mode: Player/Enemy feedback component vÃ  audio
source, attack SFX, Enemy hit flash + damage SFX, Player contact damage + feedback, death feedback,
cooldown chá»‘ng duplicate vÃ  gameplay time váº«n hoáº¡t Ä‘á»™ng. Console cuá»‘i khÃ´ng cÃ³ error/warning má»›i;
Ä‘Ã£ save GameScene, exit Play Mode vÃ  xÃ³a probe khá»i scene. Touch/Android audio váº«n cáº§n test thá»§ cÃ´ng.

Acceptance:  
Combat responsive vÃ  dá»… hiá»ƒu mÃ  khÃ´ng cáº§n debug visual.

---

# PHASE 4 â€” Dungeon Room System

## M4.1 â€” Room Architecture
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] Room component
- [x] room lifecycle
- [x] enter room
- [x] lock room náº¿u cáº§n
- [x] clear condition
- [x] completion
- [x] unlock exit
- [x] transition sang room tiáº¿p theo

`Room` quáº£n lÃ½ NotEntered/Active/Completed/Exited vÃ  Ä‘iá»u kiá»‡n OnEntry/DefeatEnemies/SolvePuzzle;
`RoomManager` Ä‘iá»u phá»‘i sequence, `RoomExit` chá»‰ nháº­n Player, `RoomHUD` hiá»ƒn thá»‹ objective/exit.
GameScene cÃ³ hai encounter dÃ¹ng chung arena: combat dÃ¹ng Enemy hiá»‡n cÃ³, rá»“i dual-switch puzzle.
Má»—i encounter cÃ³ cá»­a thoÃ¡t riÃªng dÃ¹ng láº¡i `Door`; cá»­a puzzle gá»‘c váº«n do `DualPressureDoor` Ä‘iá»u khiá»ƒn.

Dependency trá»±c tiáº¿p: chuyá»ƒn phÃ²ng giá»¯ HP, Ä‘Æ°a Player tá»›i spawn má»›i, báº¯t Ä‘áº§u Loop 1 vÃ  timeline má»›i,
dá»n Ghost cÅ©; rewind chá»‰ reset Player vÃ  room hiá»‡n táº¡i. Clear Ä‘Æ°á»£c giá»¯ tá»›i rewind Ä‘á»ƒ Player Ä‘i tá»›i exit;
rewind má»Ÿ láº¡i encounter. Exit cuá»‘i phÃ¡t tráº¡ng thÃ¡i completion vÃ  dá»«ng timer, chÆ°a pháº£i Victory flow.

Verification: 30 checks PASS qua Unity MCP/Play Mode, gá»“m hai rewind tá»± nhiÃªn 20 giÃ¢y,
combat/lock/unlock, transition báº±ng physics trigger, pause, Ghost puzzle, room-local reset,
Restart, Game Over vÃ  Main Menu. ÄÃ£ sá»­a warning API deprecated trong test, compile láº¡i;
kiá»ƒm tra Play Mode bá»• sung xÃ¡c nháº­n HUD khÃ´ng Ä‘Ã¨ hint puzzle vÃ  transition váº«n hoáº¡t Ä‘á»™ng.
Console cuá»‘i: 0 errors, 0 warnings. ÄÃ£ exit Play Mode, save GameScene, khÃ´ng gáº¯n probe vÃ o scene.
ChÆ°a test touch/Android. Chi tiáº¿t: [M4.1 review](Docs/Testing/M4.1/REVIEW.md).

## M4.2 â€” Dungeon Layout
**[x] DONE â€” authored layout verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

TÃ¡m room riÃªng trong GameScene, authored thá»§ cÃ´ng vÃ  ná»‘i báº±ng RoomManager.
DONE á»Ÿ milestone nÃ y lÃ  layout/progression; khÃ´ng Ä‘á»“ng nghÄ©a Ä‘Ã£ hoÃ n thÃ nh trap, upgrade, elite hay boss gameplay.

Target:
- [x] Start Room â€” Threshold
- [x] Combat Room â€” Guard Hall (2 Enemy hiá»‡n cÃ³)
- [x] Time Puzzle Room â€” Echo Chamber (dual-switch + Ghost)
- [x] Trap Room â€” Pendulum Gallery (layout + 3 hazard sockets; trap mechanics á»Ÿ M6.4)
- [x] Upgrade/Treasure Room â€” Timewell Treasury (layout + reward socket; upgrade á»Ÿ Phase 7)
- [x] Combat/Puzzle Room thá»© hai â€” Split Bastion (3 Enemy hiá»‡n cÃ³)
- [x] Elite Room â€” Warden Antechamber (layout + 1 guard stand-in; elite behavior á»Ÿ M5.5)
- [x] Boss Room â€” Chrono Sanctum (arena + boss/mechanism sockets; boss á»Ÿ Phase 8)

Má»—i phÃ²ng cÃ³ spawn, camera anchor, exit, floor/border blockout vÃ  vá»‹ trÃ­ ná»™i dung riÃªng.
`RoomCamera` theo room hiá»‡n táº¡i qua event, khÃ´ng tÃ¬m object báº±ng tÃªn trong runtime.
Start/Trap/Treasure/Boss dÃ¹ng OnEntry Ä‘á»ƒ kiá»ƒm thá»­ tuyáº¿n layout; combat vÃ  puzzle váº«n yÃªu cáº§u clear.
Chá»‰ Content cá»§a room hiá»‡n táº¡i active. DashButton scale Ä‘Æ°á»£c sá»­a tá»« 2,5 vá» 1 Ä‘á»ƒ khÃ´ng che layout.

Verification: 153 checks PASS qua Unity MCP/Play Mode: Ä‘á»§ 8 role/footprint riÃªng, full progression
qua physics exit, spawn/camera Ä‘Ãºng phÃ²ng, content isolation, combat clear, hai rewind tá»± nhiÃªn,
Ghost puzzle á»Ÿ tá»a Ä‘á»™ má»›i, pause, HP preservation, Restart/Game Over/Main Menu.
ÄÃ£ inspect screenshot Guard Hall/Echo Chamber/Pendulum Gallery/Chrono Sanctum.
Console cuá»‘i: 0 errors, 0 warnings; khÃ´ng missing script/reference Ä‘Æ°á»£c kiá»ƒm tra.
ÄÃ£ exit Play Mode, save GameScene; chÆ°a test touch/Android.
Chi tiáº¿t, map vÃ  deferred content: [M4.2 review](Docs/Testing/M4.2/REVIEW.md).

KHÃ”NG Æ°u tiÃªn procedural generation.

## M4.3 â€” Tilemap / Environment
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] floor
- [x] walls
- [x] collision
- [x] room boundaries
- [x] visual theme consistency
- [x] Player khÃ´ng thoÃ¡t khá»i level geometry

Má»—i room cÃ³ authored floor/border blockout vÃ  `RoomBoundary` vá»›i nÄƒm BoxCollider2D: north,
south, west, east-upper/east-lower, giá»¯ khe exit á»Ÿ phÃ­a Ä‘Ã´ng. `PlayerDash` dÃ¹ng Rigidbody2D cast
Ä‘á»ƒ dá»«ng trÆ°á»›c collider thay vÃ¬ teleport xuyÃªn tÆ°á»ng. Main Camera váº«n Ä‘á»•i theo `RoomCamera`.
Environment cÅ© dÃ¹ng chung Ä‘Ã£ táº¯t; layout hiá»‡n táº¡i dÃ¹ng ná»n vÃ  trim theo tá»«ng room, mÃ u cyan cho
temporal path vÃ  amber cho hazard blockout.

Verification: 76 checks PASS qua Unity MCP/Play Mode trÃªn cáº£ 8 room: boundary/collider count,
movement bá»‹ giá»¯ á»Ÿ tÆ°á»ng tÃ¢y, dash bá»‹ cháº·n á»Ÿ tÆ°á»ng Ä‘Ã´ng phÃ­a trÃªn khe, khe exit cÃ²n má»Ÿ,
pause giá»¯ timer, Restart khÃ´i phá»¥c boundary/dash, Main Menu unload scene. Console cuá»‘i:
0 errors, 0 warnings; khÃ´ng missing script, 8 RoomBoundary/8 room; GameScene Ä‘Ã£ save vÃ  exit Play Mode.
ChÆ°a test touch/Android. Chi tiáº¿t: [M4.3 review](Docs/Testing/M4.3/REVIEW.md).

## M4.4 â€” Room + Time Loop Interaction
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-21.**

- [x] loop restart position behavior theo room
- [x] room-local reset behavior
- [x] Ghost lifecycle giá»¯a cÃ¡c room
- [x] timeline cÃ³ reset khi sang room hay khÃ´ng
- [x] ngÄƒn Ghost cÅ© phÃ¡ gameplay room má»›i

Policy Ä‘Ã£ chá»‘t: má»—i room lÃ  má»™t local temporal encounter. Khi room báº¯t Ä‘áº§u, `TimeLoopManager.BeginEncounter`
chá»¥p Player + current Room, reset Loop 1 vÃ  phÃ¡t `EncounterStarted`; `PlayerTimelineRecorder` báº¯t Ä‘áº§u
timeline má»›i cÃ²n `TemporalGhostManager` xÃ³a pending/active Ghost. Rewind trong room giá»¯ room hiá»‡n táº¡i,
tráº£ Player vá» spawn, khÃ´i phá»¥c enemy/puzzle resettable vÃ  táº¡o Ghost cá»§a loop vá»«a hoÃ n thÃ nh.
Transition chá»‰ báº­t Content room má»›i, táº¯t room cÅ©, giá»¯ HP, Ä‘Æ°a Player tá»›i spawn vÃ  khÃ´ng mang Ghost/timeline
cÅ© sang room má»›i. Room cÅ© khÃ´ng thá»ƒ advance láº¡i; Pause giá»¯ timer vÃ  Ghost state.

Verification: 20 checks PASS qua Unity MCP/Play Mode: combat action recording, room-local rewind,
Ghost snapshot/source loop, enemy health restore, independent Ghost actor, room transition/content isolation,
fresh Loop 1/timeline, stale Ghost rejection in puzzle, Ghost A + Player B cooperation, physical-exit hold,
pause, Restart and Main Menu. Console cuá»‘i 0 errors, 0 warnings; Ä‘Ã£ exit Play Mode.
Chi tiáº¿t: [M4.4 review](Docs/Testing/M4.4/REVIEW.md).

Recommended default:  
Má»—i major room lÃ  má»™t local temporal encounter; khi vÃ o room má»›i nÃªn reset/clear Ghost history trá»« khi gameplay testing chá»©ng minh rule khÃ¡c tá»‘t hÆ¡n.

---

# PHASE 5 â€” Enemy Roster

## M5.1 â€” Enemy Base Improvements
**[x] DONE â€” verified trong Unity 6000.6.0f1 ngÃ y 2026-09-22.**

- [x] reusable damage/death behavior
- [x] telegraphing
- [x] loop reset compatibility
- [x] room integration

TÃ¡i sá»­ dá»¥ng Health, EnemyAttackTarget, TimeLoopActor vÃ  Room; khÃ´ng thÃªm enemy base trÃ¹ng chá»©c nÄƒng.
EnemyContactDamage cÃ³ wind-up 0,45 giÃ¢y, vÃ²ng cáº£nh bÃ¡o amberâ†’red, dá»«ng chase khi chuáº©n bá»‹ Ä‘Ã¡nh,
kiá»ƒm tra láº¡i overlap á»Ÿ impact, 20 damage má»™t láº§n rá»“i cooldown 1 giÃ¢y. Query tÃ¡i sá»­ dá»¥ng buffer.
SÃ¡u Enemy hiá»‡n cÃ³ Ä‘Æ°á»£c ná»‘i EnemyAttackTelegraph qua serialized reference trong GameScene.
Rewind/death/disable há»§y Ä‘Ã²n Ä‘ang chá»; deathâ€“revive khÃ´i phá»¥c mÃ u/scale feedback.
Sá»­a dependency phÃ¡t hiá»‡n trong test: AttackFeedback/DashFeedback dÃ¹ng renderer riÃªng Ä‘á»ƒ Ghost replay
attack khÃ´ng cÃ²n lá»—i LineRenderer index out of bounds.

Verification lean: 19 checks M5.1 + 20 regression checks M4.4 PASS (wind-up, dash trÃ¡nh Ä‘Ã²n,
multi-collider damage, pause, live/dead rewind, Ghost damage, room clear/transition, puzzle,
Restart/Main Menu). Compile thÃ nh cÃ´ng; Console cuá»‘i 0 errors, 0 warnings; 6/6 reference há»£p lá»‡,
khÃ´ng missing script/probe trong scene. ÄÃ£ exit Play Mode vÃ  save GameScene.
MCP screenshot phÃ¡t sinh lá»—i ná»™i bá»™ ScreenshotUtility/recursive PlayerLoop; Ä‘Ã£ xÃ¡c Ä‘á»‹nh stack trace
vÃ  cháº¡y láº¡i 19 checks khÃ´ng chá»¥p áº£nh, PASS vá»›i Console sáº¡ch. ChÆ°a test touch/Android hoáº·c cÃ¢n báº±ng cuá»‘i.
Chi tiáº¿t, file thay Ä‘á»•i vÃ  manual checklist: [M5.1 review](Docs/Testing/M5.1/REVIEW.md).

## M5.2 â€” Enemy 1: Chaser / Slime
**[x] DONE â€” Ember Chaser verified trong Unity 6000.6.0f1 ngÃ y 2026-09-22.**

- [x] chase
- [x] contact/melee attack
- [x] visual rÃµ
- [x] tuned speed/damage

NÄƒm guard á»Ÿ Guard Hall/Split Bastion Ä‘Æ°á»£c cáº¥u hÃ¬nh thÃ nh Ember Chaser: 75 HP (3 Ä‘Ã²n Player
25 damage), speed 2,2 so vá»›i Player 5, detection 18, stopping distance 0,75. EnemyFollow clamp
bÆ°á»›c di chuyá»ƒn Ä‘á»ƒ khÃ´ng vÆ°á»£t khoáº£ng dá»«ng. Giá»¯ melee M5.1: 20 damage, wind-up 0,45 giÃ¢y,
cooldown 1 giÃ¢y, radius 0,65. Visual sprite ghÃ©p Ä‘á» cam cÃ³ viá»n tá»‘i/máº¯t/fang phÃ¢n biá»‡t vá»›i Player/Ghost.
CÃ³ prefab EmberChaser vá»›i HealthBar/telegraph reference ná»™i bá»™; cáº§n gÃ¡n Player khi Ä‘áº·t prefab.
Warden váº«n lÃ  stand-in, chÆ°a triá»ƒn khai elite. KhÃ´ng thÃªm AI/Health/reset system trÃ¹ng chá»©c nÄƒng.

Verification lean: 7 checks M5.2 + 19 regression M5.1 PASS qua MCP/Play Mode: chase/tá»‘c Ä‘á»™,
pause, khoáº£ng dá»«ng, 3-hit kill, wind-up/dash nÃ©/multi-collider damage/cooldown, deathâ€“revive,
Ghost damage, room clear/transition. Compile thÃ nh cÃ´ng; Console cuá»‘i 0 errors/warnings.
ÄÃ£ inspect hÃ¬nh áº£nh Guard Hall, prefab/reference, exit Play Mode vÃ  save GameScene.
ChÆ°a test touch/Android, final balancing; AI Ä‘uá»•i trá»±c tiáº¿p, chÆ°a pathfinding quanh váº­t cáº£n phá»©c táº¡p.
Chi tiáº¿t vÃ  manual checklist: [M5.2 review](Docs/Testing/M5.2/REVIEW.md).

## M5.3 â€” Enemy 2: Archer
**[x] DONE â€” Ash Archer verified trong Unity 6000.6.0f1 ngÃ y 2026-09-22.**

- [x] ranged behavior
- [x] distance management
- [x] projectile
- [x] projectile pooling hoáº·c reuse hiá»‡u quáº£ náº¿u cáº§n
- [x] projectile reset/cleanup khi rewind
- [x] telegraphed attack

Split Bastion dÃ¹ng roster há»—n há»£p: 2 Ember Chaser + 1 Ash Archer. Archer cÃ³ 50 HP, báº¯n mÅ©i tÃªn
15 damage á»Ÿ tá»‘c Ä‘á»™ 7, khoáº£ng Ä‘Ã¡nh 9, giá»¯ Player trong khoáº£ng 3â€“5 vÃ  di chuyá»ƒn trá»±c tiáº¿p,
khÃ´ng pathfinding. ÄÆ°á»ng ngáº¯m amberâ†’red khÃ³a hÆ°á»›ng trong 0,75 giÃ¢y; Player cÃ³ thá»ƒ sidestep.
`EnemyProjectilePool` giá»¯ tá»‘i Ä‘a 3 arrow, khÃ´ng Instantiate thÃªm trong combat. `EnemyProjectile`
dÃ¹ng CircleCast theo quÃ£ng Ä‘Æ°á»ng, cháº·n bá»Ÿi tÆ°á»ng, tá»± háº¿t háº¡n sau 3 giÃ¢y, chá»‰ damage Player.
Archer, pool vÃ  projectile Ä‘á»u reset khi rewind/disable/death; pool náº±m dÆ°á»›i room Content.
Visual Ash Archer dÃ¹ng hood/face/eyes/bow tá»« sprite hiá»‡n cÃ³; prefab Arrow vÃ  AshArcher Ä‘Æ°á»£c lÆ°u.

Verification lean: 20 checks Archer + 19 regression M5.1 PASS qua MCP/Play Mode (telegraph,
locked aim, projectile damage/nÃ©/pause, retreat/approach, pool cap, wall collision/lifetime,
rewind cleanup, Player/Ghost damage, death, mixed-room clear/transition). Compile sáº¡ch; Console
cuá»‘i 0 errors/warnings; 1 Archer, 0 runtime projectile, 0 missing script/probe, scene saved.
ÄÃ£ inspect screenshot Split Bastion. ChÆ°a test touch/Android, build/performance vÃ  cÃ¢n báº±ng cuá»‘i.
Chi tiáº¿t: [M5.3 review](Docs/Testing/M5.3/REVIEW.md).

## M5.4 â€” Enemy 3: Knight HOáº¶C Mage
**[x] DONE â€” Chrono Knight verified trong Unity 6000.6.0f1 ngÃ y 2026-09-22.**

Knight option:
- [x] charge
- [x] heavy attack
- [x] block/defense state

Mage option:
- [ ] ranged spell
- [ ] AoE marker
- [ ] teleport/reposition

## M5.5 - Elite Enemy
- [x] reuse existing enemy foundation
- [x] stronger stats/pattern
- [x] distinctive visual
- [x] meaningful reward

Acceptance:
Three normal enemy archetypes create clearly different combat decisions.

Chrono Knight in the Elite room is upgraded to an Elite Enemy: 220 HP, 50 charge damage, 0.5 s wind-up,
charge speed 8, 1.1 s block duration, and 2.3 s block interval. A gold aura identifies the elite. It keeps
Health/EnemyAttackTarget/Room/TimeLoopActor and rewind compatibility. On death it clears the room and spawns
a Temporal Elite Reward that heals 35 HP. The reward resets on rewind and can spawn again after a later death.
Saved prefabs: `ChronoElite`, `EliteRewardPickup`.

Verification lean: 8 M5.5 checks plus 19 M5.1 regression checks PASS through Unity MCP/Play Mode (tuning,
aura, elite death, room clear/exit, reward heal, rewind reset, reward reuse). Compile clean; final Console
has 0 errors and 0 warnings; scene saved and Play Mode exited. Touch/Android, performance, and final balance
remain manual follow-up. Details: [M5.5 review](Docs/Testing/M5.5/REVIEW.md).

---

# PHASE 6 â€” Puzzle Variety

Final game cáº§n Ã­t nháº¥t 3 temporal puzzle pattern cÃ³ Ã½ nghÄ©a.

## M6.1 - Puzzle Pattern A
- [x] Dual Pressure Switch

Acceptance:
A Ghost holds Pressure Switch A from the previous loop while the current Player activates Switch B. The Door
opens only when both are active. Leaving B closes it again; rewind resets both switches, gate and Door; Restart
clears Ghost and puzzle state.

M6.1 is verified by the existing `PressureSwitch`, `PressureSwitchActor`, `DualPressureDoor`, Ghost replay
anchor and GameScene setup. Evidence: 16/16 Play Mode checks PASS in
[M2.3 results](Docs/Testing/M2.3/playmode-results.txt); no duplicate system was created. Touch/multitouch
and Android device testing remain manual.

## M6.2 - Puzzle Pattern B
Timed mechanism:
- [x] timed door/lever
- [x] Ghost action at a recorded moment has meaningful effect
- [x] Current Player performs the complementary passage action

Implemented in `TimedDoorMechanism` and the authored `Timed Echo Gate` in Echo Chamber. The gate listens to
replayable Pressure Switch A, opens for 3 seconds, shows a cyan timer indicator, and resets closed on rewind.
The corrected Play Mode probe passed **5/5 checks**: closed start, Player trigger, rewind reset, Ghost replay
opening, and automatic timeout close. Existing M6.1 dual-switch behavior remains covered by its prior 16-check
PASS evidence. Touch/Android testing remains manual.

## M6.3 - Puzzle Pattern C
Combat cooperation:
- [x] Ghost attack/distract target
- [x] Player completes a second objective
- [x] Rewind resets cooperation state

Split Bastion now contains a `Combat Cooperation Objective` with distinct Ghost and Player targets. Damage
source is explicit in `AttackResolver`: Ghost attacks notify `CombatCooperationTarget` as Ghost damage while
live Player attacks notify it as Player damage. The objective indicator changes from orange to mint after both
roles contribute. The anchored Ghost target stays in place so the recorded attack remains readable; the other
Chaser remains mobile. All target/objective flags reset on rewind.

Verification lean: `CombatCooperationPlayModeChecks` **5/5 PASS** and M5.1 regression **19/19 PASS** through
Unity MCP frame-stepped Play Mode. Final Console: 0 errors, 0 warnings; scene saved and Play Mode exited.
Touch/Android and final encounter balance remain manual follow-up.

## M6.4 - Trap Mechanics
- [x] spikes
- [x] timed hazard
- [ ] projectile trap
- [ ] moving hazard

Pendulum Gallery now contains a rewind-compatible `TemporalSpikeTrap`. Player contact starts a 0.6 second
warning, then applies 20 damage during a short red spike pulse with cooldown. The idle/warning/active colors
and warning indicator make the hazard readable. Trap state and Player health reset correctly on rewind.

Verification lean: `TrapPlayModeChecks` **4/4 PASS** (idle setup, warning without damage, delayed damage,
rewind reset). Existing M5.1 regression evidence remains **19/19 PASS**. Compile clean; final Console 0 errors,
0 warnings; scene saved and Play Mode exited. Touch/Android and final hazard balance remain manual follow-up.

## M6.5 - Puzzle Tutorialization
- [x] introduce the mechanic in a safe first environment
- [x] visual hint
- [x] clear failure state
- [x] no text wall

Echo Chamber now has a compact world-space `Puzzle Tutorial Guide`. It starts with `STEP ON A TO BEGIN`,
explains the first-loop setup as `HOLD A - REWIND - FOLLOW YOUR GHOST`, then points to the complementary B
switch after Ghost replay. The solved state changes to `TIME LINK COMPLETE`; a small Ghost marker reinforces
that the past Player is the active helper. The guide resets with the puzzle and uses short messages only.

Verification lean: `PuzzleTutorialPlayModeChecks` **4/4 PASS** (safe entry, first-loop failure guidance,
Ghost-loop guidance, solved completion state). Compile clean; Console after stopping/clearing: 0 errors,
0 warnings; scene saved and Play Mode exited. Touch/Android and localization remain manual follow-up.

---

# PHASE 7 â€” Roguelite Progression

## M7.1 - Upgrade Framework
- [x] Upgrade data representation
- [x] apply upgrade
- [x] preserve upgrade for current run
- [x] reset correctly on new run

`UpgradeData` + `UpgradeType` provide reusable upgrade definitions. `UpgradeManager` applies Attack Damage,
Movement Speed, Max HP, Dash Cooldown and Heal effects. Rewind preserves current-run upgrades; `GameManager.Restart()`
resets the run baseline and clears stacks. GameScene now wires the reusable `PlayerDash` component.

Verification lean: `UpgradeFrameworkPlayModeChecks` **4/4 PASS** (clean start, four stat effects, rewind
persistence, explicit new-run reset). Compile clean; final Console 0 errors, 0 warnings; scene saved and
Play Mode exited. Mobile selection and balance remain follow-up work in M7.2/M7.3.
## M7.2 - 1-of-3 Upgrade UI
- [x] 3 choices
- [x] title
- [x] description
- [x] effect/value displayed clearly
- [x] mobile touch support
- [x] gameplay pauses during selection

GameScene Canvas now includes `UpgradeChoiceUI`. It builds three large touch-capable choices with title,
description and clear effect value. `ShowChoices()` pauses gameplay and selecting one applies the `UpgradeData`
then resumes gameplay. The panel is intentionally hidden until reward integration calls it.

Verification lean: `UpgradeChoiceUIPlayModeChecks` **7/7 PASS** (availability, three choices, hidden start,
pause, three buttons, button wiring, apply/resume). Compile clean; final Console 0 errors, 0 warnings; scene
saved and Play Mode exited. Android touch and visual balance remain manual follow-up.
## M7.3 - Upgrade Pool
Target meaningful upgrade definitions:
- [x] Attack Damage
- [x] Movement Speed
- [x] Max HP
- [x] Heal
- [x] Dash Cooldown
- [x] Loop Duration
- [x] Ghost Damage
- [x] Temporal/Ghost upgrade

`UpgradeManager` now owns a focused pool of 8 unique definitions with stable IDs, readable titles,
descriptions and values. The M7.2 choice UI consumes the first three pool entries, so future reward integration
can select from one shared source instead of duplicating definitions. Loop/Ghost/Temporal entries are defined
for their later system integrations; their application behavior remains in the relevant follow-up milestones.

Verification lean: `UpgradePoolPlayModeChecks` **12/12 PASS** (pool availability, 8 unique labeled entries,
all target categories, UI consumption). Compile clean; final Console 0 errors, 0 warnings; scene saved and
Play Mode exited. Balance and final reward selection remain follow-up work in M7.4.
## M7.4 - Reward Integration
- [x] reward after suitable room
- [x] treasure room
- [x] elite reward hook
- [x] upgrade choice preserves loop state

`UpgradeRewardManager` listens to room progression and opens the shared `UpgradeChoiceUI` once for Treasure,
Elite or CombatChallenge completion. Selection applies one pool entry, resumes gameplay and does not rewind or
replace the current loop state. Duplicate ProgressChanged notifications cannot reopen the same room reward.

Verification lean: `UpgradeRewardIntegrationPlayModeChecks` **5/5 PASS** (wiring, clean start, ineligible role,
treasure completion opens paused reward, selection closes/resumes). Compile clean; final Console 0 errors, 0
warnings; scene saved and Play Mode exited. Elite room end-to-end balance remains manual follow-up.
---

# PHASE 8 â€” Chrono Guardian Boss

## M8.1 - Boss Foundation
- [x] boss Health
- [x] boss HUD
- [x] boss reset/lifecycle rule
- [x] boss arena
- [x] boss intro

GameScene now contains the Chrono Guardian foundation in the authored boss room. `ChronoGuardian` owns a
500 HP resettable boss lifecycle and intro event, `ChronoGuardianHUD` displays the title, intro and health bar,
and `ChronoGuardianArena` marks the bounded boss arena. The boss remains inactive until its room content is
entered; later phase milestones will add attacks and vulnerability rules.

Verification lean: `ChronoGuardianFoundationPlayModeChecks` **5/5 PASS** (foundation presence, authored health,
arena wiring, intro HUD activation, full-health reset). Compile clean; final Console 0 errors, 0 warnings; scene
saved and Play Mode exited. Boss combat behavior remains follow-up work in M8.2-M8.4.
## M8.2 - Phase 1
- [x] readable normal attack
- [x] melee/ranged pattern
- [x] hazard telegraph

`ChronoGuardianPhase1` adds a close-range radial strike with a 0.8 second warning window, visible through the
reusable `EnemyAttackTelegraph`. The impact rechecks the Player at the end of the warning, deals 30 damage once,
and respects cooldown and death state. The component resets its windup, cooldown and attack count with the loop.

Verification lean: `ChronoGuardianPhase1PlayModeChecks` **5/5 PASS** (component, authored timing/radius, windup,
single impact, reset). Compile clean; final Console 0 errors, 0 warnings; scene saved and Play Mode exited.
The attack is intentionally a foundation pattern; Phase 2 will add temporal vulnerability and Ghost cooperation.
## M8.3 - Phase 2: Temporal Mechanic
- [x] boss shield or vulnerability mechanic
- [x] require Ghost cooperation
- [x] Player understands why Ghost is needed

`ChronoGuardianTemporalShield` starts with a visible shield that blocks current Player damage. A Ghost attack
breaks the shield, changes the boss visual to a vulnerable cyan state and opens the damage window. The HUD states
`SHIELD ACTIVE - GHOST ATTACK REQUIRED` and updates to `TEMPORAL SHIELD BROKEN`. Shield state resets on rewind.
`AttackResolver` now respects the shield for both Player and Ghost attacks.

Verification lean: `ChronoGuardianTemporalMechanicPlayModeChecks` **7/7 PASS** (shield present, Player blocked,
Player alone cannot open it, Ghost opens it, Player damage accepted, reset). Compile clean; final Console 0 errors,
0 warnings; scene saved and Play Mode exited. Multi-phase pressure remains follow-up work in M8.4.
## M8.4 - Phase 3: Finale
- [x] increase pressure
- [x] temporal hazard
- [x] fair Ghost disruption
- [x] clear final vulnerability window

`ChronoGuardianFinale` enters at 50% boss health and repeatedly telegraphs a temporal pulse. The pulse deals
one area hit, briefly disrupts active Ghost playback and opens a 2.5 second vulnerability window. The finale
state and hazard reset cleanly. Ghost disruption is visual and short, so it creates pressure without deleting
player history.

Verification lean: `ChronoGuardianFinalePlayModeChecks` **4/4 PASS** (phase entry, hazard windup, resolved hazard
and vulnerability window, reset). Compile clean; final Console 0 errors, 0 warnings; scene saved and Play Mode
exited. Boss death, VFX and audio polish remain follow-up work in M8.5.
## M8.5 - Boss Polish
- [x] hit feedback
- [x] phase transition feedback
- [x] SFX hook
- [x] VFX hook
- [x] death sequence
- [x] Victory trigger

`ChronoGuardianPolish` adds hit flash, phase color transition, optional audio hook points, a short unscaled death
sequence and collider shutdown. `GameManager.Victory()` now pauses the run, while `VictoryFlowUI` presents a clear
completion panel with a Main Menu button. The boss room remains the authored trigger point for the full sequence.

Verification lean: `ChronoGuardianPolishPlayModeChecks` **4/4 PASS** (foundation wiring, hit path, lethal death
sequence, paused Victory flow). Compile clean; final Console 0 errors, 0 warnings; scene saved and Play Mode exited.
Audio clips and final visual tuning remain manual presentation work.
---

# PHASE 9 â€” Complete Game Flow

## M9.1 â€” Run State
- [ ] Start new run
- [ ] room progression
- [ ] upgrade giá»¯ trong run
- [ ] Game Over reset Ä‘Ãºng
- [ ] Victory káº¿t thÃºc run

## M9.2 â€” Pause
- [ ] Pause button
- [ ] Resume
- [ ] Restart Run
- [ ] Main Menu
- [ ] pause khÃ´ng phÃ¡ Time Loop/Ghost timeline

## M9.3 â€” Settings
Settings implementation Ä‘Æ°á»£c DEFER tá»« M0.1 sang milestone nÃ y; chÆ°a triá»ƒn khai trong M4.1.

Tá»‘i thiá»ƒu:
- [ ] master volume
- [ ] music volume hoáº·c music toggle Ä‘Æ¡n giáº£n
- [ ] SFX volume hoáº·c SFX toggle Ä‘Æ¡n giáº£n

Optional náº¿u dá»…:
- [ ] vibration toggle
- [ ] quality setting

## M9.4 â€” Victory
- [ ] Victory screen
- [ ] replay/new run
- [ ] Main Menu
- [ ] optional run summary

## M9.5 â€” Tutorial
- [ ] movement
- [ ] attack
- [ ] time loop
- [ ] Ghost concept
- [ ] temporal puzzle Ä‘áº§u tiÃªn
- [ ] dash khi Ä‘Æ°á»£c giá»›i thiá»‡u

DÃ¹ng contextual guidance ngáº¯n gá»n.

---

# PHASE 10 â€” Art, Animation, Audio, VFX

Phase nÃ y cÃ³ thá»ƒ lÃ m nháº¹ tá»« sá»›m, nhÆ°ng final polish táº­p trung á»Ÿ Ä‘Ã¢y.

## M10.1 â€” Visual Replacement
- [ ] thay hoáº·c cáº£i thiá»‡n rÃµ cÃ¡c primitive placeholder
- [ ] Player visual nháº¥t quÃ¡n
- [ ] Ghost visual
- [ ] Enemy visual
- [ ] dungeon tile/environment
- [ ] switch/door visual
- [ ] boss visual

Asset cÃ³ thá»ƒ Ä‘Æ¡n giáº£n, nhÆ°ng final game pháº£i trÃ´ng cÃ³ chá»§ Ä‘Ã­ch.

## M10.2 â€” Animation
Æ¯u tiÃªn:
- [ ] Player idle/move
- [ ] Player attack
- [ ] dash feedback
- [ ] Enemy
- [ ] boss
- [ ] door
- [ ] Ghost readability

## M10.3 â€” VFX
- [ ] attack impact
- [ ] damage
- [ ] enemy death
- [ ] rewind
- [ ] Ghost spawn
- [ ] switch activation
- [ ] door open
- [ ] upgrade
- [ ] boss phase
- [ ] Victory

## M10.4 â€” Audio
- [ ] menu music
- [ ] dungeon music
- [ ] boss music hoáº·c intensified variant
- [ ] attack
- [ ] hit
- [ ] damage
- [ ] enemy death
- [ ] rewind
- [ ] Ghost spawn
- [ ] UI button
- [ ] door/switch
- [ ] Game Over
- [ ] Victory

---

# PHASE 11 â€” Android & UX Polish

## M11.1 â€” Responsive UI
Test cÃ¡c landscape ratio phá»• biáº¿n:
- [ ] 16:9
- [ ] wider phone ratio
- [ ] safe positioning
- [ ] khÃ´ng clipped HUD
- [ ] khÃ´ng overlapping controls

## M11.2 â€” Touch Testing
Test thá»§ cÃ´ng:
- [ ] joystick
- [ ] move + attack Ä‘á»“ng thá»i
- [ ] move + dash
- [ ] rapid button presses
- [ ] Game Over button
- [ ] Pause UI
- [ ] Upgrade selection

## M11.3 â€” Performance
- [ ] profiler sanity check
- [ ] khÃ´ng cÃ³ per-frame allocation lá»™ liá»…u tá»« system má»›i
- [ ] Ghost recording memory cÃ³ giá»›i háº¡n
- [ ] max Ghost count Ä‘Æ°á»£c giá»¯
- [ ] projectile/VFX Ä‘Æ°á»£c kiá»ƒm soÃ¡t
- [ ] target frame rate á»•n Ä‘á»‹nh trÃªn Android device dá»± kiáº¿n

## M11.4 â€” Android Build
- [ ] final Exit/platform behavior verification (DEFER tá»« M0.1; káº¿t há»£p final platform QA)
- [ ] Android build thÃ nh cÃ´ng
- [ ] install lÃªn real device
- [ ] launch thÃ nh cÃ´ng
- [ ] scene flow hoáº¡t Ä‘á»™ng
- [ ] touch hoáº¡t Ä‘á»™ng
- [ ] audio hoáº¡t Ä‘á»™ng
- [ ] app pause/resume behavior Ä‘Æ°á»£c kiá»ƒm tra

---

# PHASE 12 â€” Final Balancing and QA

## M12.1 â€” Full Run Testing
Cháº¡y nhiá»u full run:

Main Menu  
-> Dungeon  
-> Rooms  
-> Upgrades  
-> Elite  
-> Boss  
-> Victory

Äá»“ng thá»i test failure flow:

Main Menu  
-> Dungeon  
-> Game Over  
-> Restart  
-> Main Menu

## M12.2 â€” Difficulty
Tune:
- [ ] Player damage
- [ ] Enemy HP
- [ ] Enemy damage
- [ ] Enemy speed
- [ ] loop duration
- [ ] puzzle timing
- [ ] Ghost effectiveness
- [ ] boss HP
- [ ] boss attack timing

Goal:  
Ä‘á»§ thá»­ thÃ¡ch Ä‘á»ƒ thá»ƒ hiá»‡n mechanic nhÆ°ng khÃ´ng gÃ¢y khÃ³ chá»‹u khi presentation.

## M12.3 â€” Bug Checklist
- [ ] khÃ´ng NullReferenceException
- [ ] khÃ´ng MissingReferenceException
- [ ] khÃ´ng broken serialized reference
- [ ] khÃ´ng Ghost duplication sau Restart
- [ ] timer khÃ´ng tiáº¿p tá»¥c khi pause/Game Over
- [ ] dead Enemy reset Ä‘Ãºng
- [ ] khÃ´ng puzzle soft-lock
- [ ] khÃ´ng door collider desync
- [ ] Player khÃ´ng bá»‹ káº¹t sau rewind
- [ ] khÃ´ng upgrade stacking bug
- [ ] khÃ´ng boss phase soft-lock

## M12.4 â€” Presentation Polish
- [ ] font nháº¥t quÃ¡n
- [ ] mÃ u sáº¯c nháº¥t quÃ¡n
- [ ] button style nháº¥t quÃ¡n
- [ ] HUD dá»… Ä‘á»c
- [ ] transition sáº¡ch
- [ ] feedback rÃµ
- [ ] khÃ´ng cÃ²n debug text chá»‰ dÃ nh cho development
- [ ] khÃ´ng cÃ²n visible placeholder label

---

# PHASE 13 â€” Final Deliverable

## M13.1 â€” Final Build
- [ ] clean Android APK/AAB theo yÃªu cáº§u
- [ ] final version number
- [ ] final icon
- [ ] final app/game title
- [ ] test install

## M13.2 â€” Demo Readiness
Chuáº©n bá»‹ má»™t demo path á»•n Ä‘á»‹nh thá»ƒ hiá»‡n:
- [ ] movement/combat
- [ ] rewind
- [ ] Ghost replay
- [ ] Ghost puzzle
- [ ] upgrade
- [ ] enemy variety
- [ ] boss
- [ ] Victory

Evaluator pháº£i hiá»ƒu Ä‘Æ°á»£c unique mechanic cá»§a game nhanh chÃ³ng.

## M13.3 â€” Completion Definition

ChronoDungeon chá»‰ Ä‘Æ°á»£c coi lÃ  DONE khi:

- [ ] full run chÆ¡i Ä‘Æ°á»£c tá»« launch tá»›i Victory;
- [ ] core Temporal Ghost mechanic lÃ  thiáº¿t yáº¿u;
- [ ] cÃ³ Ã­t nháº¥t 3 temporal puzzle pattern;
- [ ] combat cÃ³ enemy variety rÃµ;
- [ ] upgrade progression hoáº¡t Ä‘á»™ng;
- [ ] boss hoÃ n chá»‰nh;
- [ ] UI/audio/VFX khiáº¿n game cÃ³ cáº£m giÃ¡c intentional;
- [ ] Android build hoáº¡t Ä‘á»™ng;
- [ ] final manual QA hoÃ n thÃ nh;
- [ ] khÃ´ng cÃ²n known game-breaking bug.

---

# NEXT MILESTONE

Codex pháº£i inspect project trÆ°á»›c khi tin marker nÃ y.

Next milestone after M8.5 verification on 2026-09-22:

**M9.1 - Run State**

M8.5 is complete; stop at the review checkpoint. M9.1 will connect start, progression, upgrade persistence, Game Over and Victory into one run state.
M0.1 lÃ  foundation DONE: Settings defer sang M9.3, final Exit/platform verification defer sang
Phase 11/final platform QA theo scope clarification cá»§a user. CÃ¡c háº¡ng má»¥c device touch vÃ 
responsive layout cá»§a M0.2 váº«n cáº§n final QA; khÃ´ng cháº·n milestone phÃ¡t triá»ƒn chÃ­nh Room System.

KhÃ´ng tá»± skip sang milestone sau chá»‰ vÃ¬ nÃ³ háº¥p dáº«n hÆ¡n.  
HoÃ n thÃ nh, test vÃ  integrate milestone chÆ°a hoÃ n thÃ nh sá»›m nháº¥t trÆ°á»›c.












