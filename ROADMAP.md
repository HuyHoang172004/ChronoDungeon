# ROADMAP.md — Lộ trình phát triển hoàn chỉnh ChronoDungeon

## Quy ước trạng thái

- [x] DONE
- [~] IN PROGRESS / PARTIAL
- [ ] NOT STARTED
- [!] BLOCKED

Codex chỉ được cập nhật trạng thái sau khi verify project thực tế.

Không mark milestone DONE chỉ vì code đã tồn tại.  
DONE nghĩa là feature đã compile, test và integrate mà không còn blocking error đã biết.

---

# PHASE 0 — Foundation

## M0.1 Project / Scene Flow
**[x] DONE — foundation scene flow; scope clarified 2026-09-21.**

- [x] SplashScene
- [x] MainMenuScene
- [x] GameScene
- [x] Splash -> Main Menu
- [x] Play -> GameScene
- Settings implementation: **DEFER sang M9.3**, không phải blocker của Room System.
- Final Exit/platform verification: **DEFER sang Phase 11 / final platform QA**, không phải blocker của Room System.

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
- [x] Time Loop 20 giây
- [x] Loop HUD
- [x] loop counter
- [x] Player reset
- [x] Enemy reset
- [x] resettable architecture
- [x] dead Enemy restoration
- [x] Game Over interaction với timer
- [x] không reload scene khi normal rewind

---

# PHASE 1 — Temporal Ghost Core

## M1.1 — Ghi và Replay chuyển động

**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

Mục tiêu:  
Biến lịch sử chuyển động của Player thành Ghost nhìn thấy được sau rewind.

Yêu cầu:
- [x] timeline data structure
- [x] Player timeline recorder
- [x] ghi time + position
- [x] ghi facing/rotation khi phù hợp
- [x] sample interval hợp lý
- [x] Ghost visual
- [x] Ghost playback
- [x] smooth interpolation
- [x] Ghost độc lập với joystick
- [x] Ghost không nhận Player damage
- [x] Ghost playback pause cùng gameplay
- [x] Ghost giữ trạng thái tới hết loop
- [x] không ghi lại Ghost như Player

Acceptance:
- Loop 1 record Player.
- Loop 2 spawn Ghost1 replay Loop1.
- Player hiện tại vẫn điều khiển độc lập.

Verification: 22 automated checks trong Play Mode qua Unity MCP; rewind tự nhiên
20 giây, 782 frame so sánh playback (sai số vị trí đo được = 0), pause/Game Over,
Player/Enemy HP reset, Enemy death restoration, attack/contact damage đều PASS.
Restart về Loop 1, recording mới, không Ghost; Main Menu return PASS qua API.
Console cuối không error/warning; đã exit Play Mode và save GameScene, không missing script.

Giới hạn M1.1: một Ghost của loop ngay trước, visual primitive tím bán trong suốt;
ghi rotation và sprite flip hiện có (Player chưa có directional facing system).
Action channel chỉ là cấu trúc mở rộng, chưa record/replay attack/dash/interaction.
Chưa xác nhận touch/multitouch Android hoặc độ mượt cảm nhận trên thiết bị.
Chi tiết và manual checklist: [M1.1 review](Docs/Testing/M1.1/REVIEW.md).

## M1.2 — Multiple Ghosts

**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] Ghost1 đại diện Loop1
- [x] Ghost2 đại diện Loop2
- [x] Ghost3 đại diện Loop3
- [x] maxGhosts = 3
- [x] loại Ghost cũ nhất khi vượt giới hạn
- [x] Restart xóa recordings/Ghosts
- [x] Game Over không làm hỏng recording state

Acceptance:  
Test ít nhất 4 lần chuyển loop và không bao giờ có hơn 3 active Ghost.

Verification: 81 checks PASS qua Unity MCP/Play Mode, bốn rewind tự nhiên
20 giây liên tiếp và một rewind sau Restart. Loop 2/3/4 lần lượt giữ lịch sử
[1], [1,2], [1,2,3]; Loop 5 giữ [2,3,4]. Theo dõi 38.107 frame: tối đa 3 Ghost,
sai số vị trí so với recording lưu riêng = 0. Mỗi Ghost giữ snapshot riêng,
không bị recorder ghi đè; slot cũ nhất được tái sử dụng khi đủ ba Ghost.

PASS: joystick pointer giả lập điều khiển Player độc lập; Ghost không có input,
Health/collider/Player tag; Enemy overlap Ghost không gây Player damage;
pause/Game Over giữ nguyên timeline; combat, Enemy chase/contact và reset HP/dead
Enemy hoạt động. Restart từ Game Over và khi đang chơi xóa lịch sử; rewind đầu
sau Restart tạo đúng một Ghost mới; Main Menu không còn Ghost tồn dư.

Đã exit Play Mode, gỡ probe, save GameScene; reference đầy đủ, không missing script.
Unity native Console cuối: 0 errors, 0 warnings. Chưa xác nhận touch/multitouch
hay performance Android. Chi tiết: [M1.2 review](Docs/Testing/M1.2/REVIEW.md).

## M1.3 — Action Timeline
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

Mở rộng timeline mà không viết lại movement system.

Event support bắt buộc:
- [x] Attack event
- [x] facing/attack direction
- [x] Ghost attack playback
- [x] Ghost có thể damage Enemy hợp lệ
- [x] Ghost attack không ảnh hưởng Player
- [x] attack timing khớp timeline đã record

Kiến trúc phải sẵn sàng cho:
- [x] Dash event (extension point; chưa implement mechanic)
- [x] Interaction event (extension point; chưa implement mechanic)
- [x] Skill event (extension point; chưa implement mechanic)

Acceptance:  
Player attack trong Loop1.  
Ghost1 lặp lại attack gần đúng timeline moment đó trong Loop2.

Verification: 31 checks PASS qua Unity MCP/Play Mode, bốn rewind tự nhiên 20 giây.
9 attack replay đúng một lần/event, sai lệch thời điểm tối đa 0,0304 giây (trong một frame).
Theo dõi 4.715 frame, sai số movement replay = 0. Player/Ghost gây đúng 25 damage/đòn,
Enemy hai collider không bị damage lặp; Player và Health không được đánh dấu Enemy đều an toàn.
Hai Ghost gây tổng 75 damage từ ba event; ba Ghost và eviction giữ đúng lịch sử,
slot tái sử dụng không replay attack cũ. Pause, Game Over và Restart PASS.

Đã thoát Play Mode, save GameScene; Enemy có EnemyAttackTarget, maxGhosts = 3,
không missing script/probe lưu trong scene. Console cuối: 0 errors, 0 warnings.
Tại thời điểm M1.3 attack còn radial; hướng đã được record để mở rộng. Directional hit shape/VFX
được hoàn tất trong M3.1.
Action buffer giới hạn 256 event/loop; overflow cảnh báo một lần và bỏ event vượt giới hạn.
Chưa test touch/Android. Chi tiết: [M1.3 review](Docs/Testing/M1.3/REVIEW.md).

---

# PHASE 2 — First Complete Time Puzzle

## M2.1 — Pressure Switch
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] reusable PressureSwitch
- [x] Player activate được
- [x] Ghost tương thích activate được
- [x] visual state active/inactive rõ
- [x] resettable khi rewind

Prefab PressureSwitch dùng opt-in PressureSwitchActor, đếm actor độc lập với số collider.
Player dùng collider overlap; Ghost dùng vị trí replay nằm trong plate, không thêm physics collider.
Có IsActive/OccupantCount, C# StateChanged(bool) và Inspector UnityEvent<bool> để nối mechanism sau.
Rewind reset inactive/zero occupants ngay lập tức, đánh giá lại ở frame gameplay tiếp theo.

Verification: 38 checks PASS qua Unity MCP/Play Mode, hai rewind tự nhiên 20 giây,
14.641 frame theo dõi, sai số Ghost movement = 0. Player hai collider đếm một actor;
Player + Ghost đếm hai; một actor rời thì actor còn lại vẫn giữ switch, không phát false event.
PASS: collider disable/exit, actor disable/destroy, switch disable/re-enable, Enemy bị loại,
reset khi Player/Ghost giữ plate, Ghost attack đúng 25 damage một lần và không damage Player,
Enemy chase/contact, Game Over/freeze, Restart và C#/Inspector events đồng nhất (18 transitions).

Đã kiểm tra hình ảnh active/inactive, save prefab và GameScene, thoát Play Mode;
không probe/missing script trong scene, maxGhosts = 3. Console cuối: 0 errors, 0 warnings.
Chưa test mobile touch/Android. Khi record nên đứng rõ trên plate: Ghost dùng anchor point,
Player dùng collider nên ngưỡng kích hoạt ở mép khác nhau. Chưa triển khai Door/M2.2.
Chi tiết: [M2.1 review](Docs/Testing/M2.1/REVIEW.md).

## M2.2 — Door
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] reusable Door
- [x] locked/open state
- [x] opening animation hoặc visual feedback rõ
- [x] reset support khi cần
- [x] không trap Player vì collider/state mismatch

Door có API `Open`, `Close`, `SetOpen(bool)`, `IsOpen`, `IsLocked`, `StateChanged(bool)` và
Inspector UnityEvent<bool>; không hard-code switch/cửa cụ thể. Collider blocking và visual panel
được cập nhật cùng một state. Lệnh đóng khi actor còn trong passage được trì hoãn an toàn tới khi
clear, tránh kẹt Player/Enemy. Rewind trả về trạng thái authored ban đầu; prefab reusable.

Verification: 25 checks PASS qua Unity MCP/Play Mode, 8.204 frame, sai số Ghost movement = 0.
PASS: cửa đóng chặn Rigidbody2D Player, mở cho đi qua, event bool điều khiển đúng, repeated command
không phát event trùng, đóng an toàn khi Player/Enemy còn trong passage, PressureSwitch nối event
mở/đóng Door, visual/collider đồng bộ, rewind reset Door + switch, Ghost replay switch/attack,
combat, Game Over/freeze và Restart. Console cuối: 0 errors, 0 warnings.

Đã save GameScene và `Assets/Prefabs/Door.prefab`, exit Play Mode; không probe/missing script
được lưu trong scene. Chưa test touch/Android. Chưa triển khai M2.3.
Chi tiết: [M2.2 review](Docs/Testing/M2.2/REVIEW.md).

## M2.3 — Dual-Switch Puzzle
- **[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

Scenario bắt buộc:
- [x] Loop1 Player activate Switch A
- [x] rewind
- [x] Ghost1 activate Switch A
- [x] Player hiện tại activate Switch B
- [x] Door mở
- [x] Player đi qua được

Acceptance:  
Puzzle chỉ solve được khi dùng ít nhất một Ghost.

DualPressureDoor nhận hai PressureSwitch reference và một Door reference; chỉ mở khi cả hai
switch active. Không hard-code scene object; mechanism có thể tái sử dụng với cặp input/output khác.
Rewind reset cả gate và Door về locked; PressureSwitch events tiếp tục điều khiển state.

Verification: 16 checks PASS qua Unity MCP/Play Mode, 14.351 frame, sai số Ghost movement = 0.
Loop 1 Player giữ A không mở cửa; Loop 2 Ghost giữ A + Player giữ B mở cửa; rời B đóng cửa,
trở lại B mở lại. Combat Player vẫn gây đúng 25 damage; rewind/reset và Restart PASS.
Console cuối: 0 errors, 0 warnings. Chưa test touch/Android. Chưa triển khai M2.4.
Chi tiết: [M2.3 review](Docs/Testing/M2.3/REVIEW.md).

## M2.4 — Puzzle Feedback
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] switch SFX/VFX
- [x] door SFX/VFX
- [x] connection giữa switch và door dễ hiểu
- [x] basic tutorial hint cho temporal puzzle đầu tiên

`PuzzleFeedback` là presentation-only adapter lắng nghe PressureSwitch/Door events:
switch/door pulse, runtime tone feedback, LineRenderer nối hai switch với Door, và hint
"STAND ON BOTH TIME PLATES" / "ONE MORE SWITCH" / "TIME LINK COMPLETE". Không đổi puzzle logic.

Verification: 8 checks PASS qua Unity MCP/Play Mode, 257 frame; feedback adapter có line/hint/audio,
inactive state đúng, một switch bật line + hint + pulse, hai switch hiển thị solved connection và
Door-open feedback, release dọn connection/hint, pause freeze và Restart cleanup PASS.
Console cuối: 0 errors, 0 warnings. Đã save GameScene, exit Play Mode; chưa test touch/Android.
Chi tiết: [M2.4 review](Docs/Testing/M2.4/REVIEW.md).

---

# PHASE 3 — Player Combat Polish

## M3.1 — Directional Combat
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] attack direction/facing
- [x] attack visual
- [x] hit feedback rõ
- [x] attack cooldown
- [x] ngăn accidental multi-hit bug

`PlayerAttack` dùng `PlayerMovement.FacingDirection`, ghi `ArcAngle` vào `AttackSnapshot`
và dùng cùng payload cho Player/Ghost. `AttackResolver` lọc mục tiêu theo hướng, vẫn deduplicate
Enemy có nhiều collider. Cooldown mặc định 0,35 giây chặn swing trùng; `AttackFeedback` hiển thị
slash line theo hướng, còn `DamageFeedback` flash SpriteRenderer của Enemy khi trúng đòn.

Verification: 10 checks PASS qua Unity MCP/Play Mode: hit phía trước, không hit phía sau,
đổi hướng, cooldown, slash visual, Enemy hit flash, timeline giữ ArcAngle và pause giữ nguyên
combat/loop. Console cuối không có error/warning mới; đã exit Play Mode và xóa probe khỏi scene.
Regression M1.3 được giữ nguyên trong code path hiện tại; touch/Android chưa mô phỏng đáng tin cậy
qua MCP và cần test thủ công trên thiết bị.

## M3.2 — Dash
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] Dash button
- [x] dash movement
- [x] cooldown
- [x] mobile-friendly behavior
- [x] visual feedback
- [x] Ghost timeline support nếu cần

`PlayerDash` là component riêng, dùng hướng di chuyển/facing hiện tại, dash distance mặc định
2,5 và cooldown 0,8 giây. `DashButton` là UI adapter nhận pointer click trên Canvas, tự hiển thị
trạng thái ready/cooldown và tạo nhãn DASH khi chạy. `DashFeedback` hiển thị vệt cyan; Dash được
ghi vào timeline với hướng và distance payload. Ghost replay lại action/feedback đúng thời điểm;
pose timeline giữ quãng di chuyển nên không bị dash hai lần.

Verification: 11 checks PASS qua Unity MCP/Play Mode: movement theo hướng, cooldown chặn lặp,
cooldown hồi phục, visual feedback, mobile button reference, pause freeze, natural rewind, Ghost
nhận timeline và replay Dash. Console cuối không có error/warning mới; đã save GameScene, exit Play
Mode và xóa probe khỏi scene. MCP không mô phỏng touch Android đáng tin cậy; cần test tap Dash trên
thiết bị/emulator thủ công.

## M3.3 — Player Feedback
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] hit flash
- [x] damage feedback
- [x] attack SFX
- [x] damage SFX
- [x] death feedback
- [ ] optional restrained camera shake (deferred; không cần cho acceptance)

`CombatFeedback` là component presentation dùng chung cho Player và Enemy. `Health` phát hit/death
feedback tập trung, `PlayerAttack` phát attack SFX, còn `AttackResolver` giữ hit flash/damage SFX
cho Enemy nhiều collider. Audio dùng clip procedural ngắn, không thêm package/asset dependency.

Verification: 10 checks PASS qua Unity MCP/Play Mode: Player/Enemy feedback component và audio
source, attack SFX, Enemy hit flash + damage SFX, Player contact damage + feedback, death feedback,
cooldown chống duplicate và gameplay time vẫn hoạt động. Console cuối không có error/warning mới;
đã save GameScene, exit Play Mode và xóa probe khỏi scene. Touch/Android audio vẫn cần test thủ công.

Acceptance:  
Combat responsive và dễ hiểu mà không cần debug visual.

---

# PHASE 4 — Dungeon Room System

## M4.1 — Room Architecture
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] Room component
- [x] room lifecycle
- [x] enter room
- [x] lock room nếu cần
- [x] clear condition
- [x] completion
- [x] unlock exit
- [x] transition sang room tiếp theo

`Room` quản lý NotEntered/Active/Completed/Exited và điều kiện OnEntry/DefeatEnemies/SolvePuzzle;
`RoomManager` điều phối sequence, `RoomExit` chỉ nhận Player, `RoomHUD` hiển thị objective/exit.
GameScene có hai encounter dùng chung arena: combat dùng Enemy hiện có, rồi dual-switch puzzle.
Mỗi encounter có cửa thoát riêng dùng lại `Door`; cửa puzzle gốc vẫn do `DualPressureDoor` điều khiển.

Dependency trực tiếp: chuyển phòng giữ HP, đưa Player tới spawn mới, bắt đầu Loop 1 và timeline mới,
dọn Ghost cũ; rewind chỉ reset Player và room hiện tại. Clear được giữ tới rewind để Player đi tới exit;
rewind mở lại encounter. Exit cuối phát trạng thái completion và dừng timer, chưa phải Victory flow.

Verification: 30 checks PASS qua Unity MCP/Play Mode, gồm hai rewind tự nhiên 20 giây,
combat/lock/unlock, transition bằng physics trigger, pause, Ghost puzzle, room-local reset,
Restart, Game Over và Main Menu. Đã sửa warning API deprecated trong test, compile lại;
kiểm tra Play Mode bổ sung xác nhận HUD không đè hint puzzle và transition vẫn hoạt động.
Console cuối: 0 errors, 0 warnings. Đã exit Play Mode, save GameScene, không gắn probe vào scene.
Chưa test touch/Android. Chi tiết: [M4.1 review](Docs/Testing/M4.1/REVIEW.md).

## M4.2 — Dungeon Layout
**[x] DONE — authored layout verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

Tám room riêng trong GameScene, authored thủ công và nối bằng RoomManager.
DONE ở milestone này là layout/progression; không đồng nghĩa đã hoàn thành trap, upgrade, elite hay boss gameplay.

Target:
- [x] Start Room — Threshold
- [x] Combat Room — Guard Hall (2 Enemy hiện có)
- [x] Time Puzzle Room — Echo Chamber (dual-switch + Ghost)
- [x] Trap Room — Pendulum Gallery (layout + 3 hazard sockets; trap mechanics ở M6.4)
- [x] Upgrade/Treasure Room — Timewell Treasury (layout + reward socket; upgrade ở Phase 7)
- [x] Combat/Puzzle Room thứ hai — Split Bastion (3 Enemy hiện có)
- [x] Elite Room — Warden Antechamber (layout + 1 guard stand-in; elite behavior ở M5.5)
- [x] Boss Room — Chrono Sanctum (arena + boss/mechanism sockets; boss ở Phase 8)

Mỗi phòng có spawn, camera anchor, exit, floor/border blockout và vị trí nội dung riêng.
`RoomCamera` theo room hiện tại qua event, không tìm object bằng tên trong runtime.
Start/Trap/Treasure/Boss dùng OnEntry để kiểm thử tuyến layout; combat và puzzle vẫn yêu cầu clear.
Chỉ Content của room hiện tại active. DashButton scale được sửa từ 2,5 về 1 để không che layout.

Verification: 153 checks PASS qua Unity MCP/Play Mode: đủ 8 role/footprint riêng, full progression
qua physics exit, spawn/camera đúng phòng, content isolation, combat clear, hai rewind tự nhiên,
Ghost puzzle ở tọa độ mới, pause, HP preservation, Restart/Game Over/Main Menu.
Đã inspect screenshot Guard Hall/Echo Chamber/Pendulum Gallery/Chrono Sanctum.
Console cuối: 0 errors, 0 warnings; không missing script/reference được kiểm tra.
Đã exit Play Mode, save GameScene; chưa test touch/Android.
Chi tiết, map và deferred content: [M4.2 review](Docs/Testing/M4.2/REVIEW.md).

KHÔNG ưu tiên procedural generation.

## M4.3 — Tilemap / Environment
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] floor
- [x] walls
- [x] collision
- [x] room boundaries
- [x] visual theme consistency
- [x] Player không thoát khỏi level geometry

Mỗi room có authored floor/border blockout và `RoomBoundary` với năm BoxCollider2D: north,
south, west, east-upper/east-lower, giữ khe exit ở phía đông. `PlayerDash` dùng Rigidbody2D cast
để dừng trước collider thay vì teleport xuyên tường. Main Camera vẫn đổi theo `RoomCamera`.
Environment cũ dùng chung đã tắt; layout hiện tại dùng nền và trim theo từng room, màu cyan cho
temporal path và amber cho hazard blockout.

Verification: 76 checks PASS qua Unity MCP/Play Mode trên cả 8 room: boundary/collider count,
movement bị giữ ở tường tây, dash bị chặn ở tường đông phía trên khe, khe exit còn mở,
pause giữ timer, Restart khôi phục boundary/dash, Main Menu unload scene. Console cuối:
0 errors, 0 warnings; không missing script, 8 RoomBoundary/8 room; GameScene đã save và exit Play Mode.
Chưa test touch/Android. Chi tiết: [M4.3 review](Docs/Testing/M4.3/REVIEW.md).

## M4.4 — Room + Time Loop Interaction
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-21.**

- [x] loop restart position behavior theo room
- [x] room-local reset behavior
- [x] Ghost lifecycle giữa các room
- [x] timeline có reset khi sang room hay không
- [x] ngăn Ghost cũ phá gameplay room mới

Policy đã chốt: mỗi room là một local temporal encounter. Khi room bắt đầu, `TimeLoopManager.BeginEncounter`
chụp Player + current Room, reset Loop 1 và phát `EncounterStarted`; `PlayerTimelineRecorder` bắt đầu
timeline mới còn `TemporalGhostManager` xóa pending/active Ghost. Rewind trong room giữ room hiện tại,
trả Player về spawn, khôi phục enemy/puzzle resettable và tạo Ghost của loop vừa hoàn thành.
Transition chỉ bật Content room mới, tắt room cũ, giữ HP, đưa Player tới spawn và không mang Ghost/timeline
cũ sang room mới. Room cũ không thể advance lại; Pause giữ timer và Ghost state.

Verification: 20 checks PASS qua Unity MCP/Play Mode: combat action recording, room-local rewind,
Ghost snapshot/source loop, enemy health restore, independent Ghost actor, room transition/content isolation,
fresh Loop 1/timeline, stale Ghost rejection in puzzle, Ghost A + Player B cooperation, physical-exit hold,
pause, Restart and Main Menu. Console cuối 0 errors, 0 warnings; đã exit Play Mode.
Chi tiết: [M4.4 review](Docs/Testing/M4.4/REVIEW.md).

Recommended default:  
Mỗi major room là một local temporal encounter; khi vào room mới nên reset/clear Ghost history trừ khi gameplay testing chứng minh rule khác tốt hơn.

---

# PHASE 5 — Enemy Roster

## M5.1 — Enemy Base Improvements
**[x] DONE — verified trong Unity 6000.6.0f1 ngày 2026-09-22.**

- [x] reusable damage/death behavior
- [x] telegraphing
- [x] loop reset compatibility
- [x] room integration

Tái sử dụng Health, EnemyAttackTarget, TimeLoopActor và Room; không thêm enemy base trùng chức năng.
EnemyContactDamage có wind-up 0,45 giây, vòng cảnh báo amber→red, dừng chase khi chuẩn bị đánh,
kiểm tra lại overlap ở impact, 20 damage một lần rồi cooldown 1 giây. Query tái sử dụng buffer.
Sáu Enemy hiện có được nối EnemyAttackTelegraph qua serialized reference trong GameScene.
Rewind/death/disable hủy đòn đang chờ; death–revive khôi phục màu/scale feedback.
Sửa dependency phát hiện trong test: AttackFeedback/DashFeedback dùng renderer riêng để Ghost replay
attack không còn lỗi LineRenderer index out of bounds.

Verification lean: 19 checks M5.1 + 20 regression checks M4.4 PASS (wind-up, dash tránh đòn,
multi-collider damage, pause, live/dead rewind, Ghost damage, room clear/transition, puzzle,
Restart/Main Menu). Compile thành công; Console cuối 0 errors, 0 warnings; 6/6 reference hợp lệ,
không missing script/probe trong scene. Đã exit Play Mode và save GameScene.
MCP screenshot phát sinh lỗi nội bộ ScreenshotUtility/recursive PlayerLoop; đã xác định stack trace
và chạy lại 19 checks không chụp ảnh, PASS với Console sạch. Chưa test touch/Android hoặc cân bằng cuối.
Chi tiết, file thay đổi và manual checklist: [M5.1 review](Docs/Testing/M5.1/REVIEW.md).

## M5.2 — Enemy 1: Chaser / Slime
**[x] DONE — Ember Chaser verified trong Unity 6000.6.0f1 ngày 2026-09-22.**

- [x] chase
- [x] contact/melee attack
- [x] visual rõ
- [x] tuned speed/damage

Năm guard ở Guard Hall/Split Bastion được cấu hình thành Ember Chaser: 75 HP (3 đòn Player
25 damage), speed 2,2 so với Player 5, detection 18, stopping distance 0,75. EnemyFollow clamp
bước di chuyển để không vượt khoảng dừng. Giữ melee M5.1: 20 damage, wind-up 0,45 giây,
cooldown 1 giây, radius 0,65. Visual sprite ghép đỏ cam có viền tối/mắt/fang phân biệt với Player/Ghost.
Có prefab EmberChaser với HealthBar/telegraph reference nội bộ; cần gán Player khi đặt prefab.
Warden vẫn là stand-in, chưa triển khai elite. Không thêm AI/Health/reset system trùng chức năng.

Verification lean: 7 checks M5.2 + 19 regression M5.1 PASS qua MCP/Play Mode: chase/tốc độ,
pause, khoảng dừng, 3-hit kill, wind-up/dash né/multi-collider damage/cooldown, death–revive,
Ghost damage, room clear/transition. Compile thành công; Console cuối 0 errors/warnings.
Đã inspect hình ảnh Guard Hall, prefab/reference, exit Play Mode và save GameScene.
Chưa test touch/Android, final balancing; AI đuổi trực tiếp, chưa pathfinding quanh vật cản phức tạp.
Chi tiết và manual checklist: [M5.2 review](Docs/Testing/M5.2/REVIEW.md).

## M5.3 — Enemy 2: Archer
**[x] DONE — Ash Archer verified trong Unity 6000.6.0f1 ngày 2026-09-22.**

- [x] ranged behavior
- [x] distance management
- [x] projectile
- [x] projectile pooling hoặc reuse hiệu quả nếu cần
- [x] projectile reset/cleanup khi rewind
- [x] telegraphed attack

Split Bastion dùng roster hỗn hợp: 2 Ember Chaser + 1 Ash Archer. Archer có 50 HP, bắn mũi tên
15 damage ở tốc độ 7, khoảng đánh 9, giữ Player trong khoảng 3–5 và di chuyển trực tiếp,
không pathfinding. Đường ngắm amber→red khóa hướng trong 0,75 giây; Player có thể sidestep.
`EnemyProjectilePool` giữ tối đa 3 arrow, không Instantiate thêm trong combat. `EnemyProjectile`
dùng CircleCast theo quãng đường, chặn bởi tường, tự hết hạn sau 3 giây, chỉ damage Player.
Archer, pool và projectile đều reset khi rewind/disable/death; pool nằm dưới room Content.
Visual Ash Archer dùng hood/face/eyes/bow từ sprite hiện có; prefab Arrow và AshArcher được lưu.

Verification lean: 20 checks Archer + 19 regression M5.1 PASS qua MCP/Play Mode (telegraph,
locked aim, projectile damage/né/pause, retreat/approach, pool cap, wall collision/lifetime,
rewind cleanup, Player/Ghost damage, death, mixed-room clear/transition). Compile sạch; Console
cuối 0 errors/warnings; 1 Archer, 0 runtime projectile, 0 missing script/probe, scene saved.
Đã inspect screenshot Split Bastion. Chưa test touch/Android, build/performance và cân bằng cuối.
Chi tiết: [M5.3 review](Docs/Testing/M5.3/REVIEW.md).

## M5.4 — Enemy 3: Knight HOẶC Mage
**[x] DONE — Chrono Knight verified trong Unity 6000.6.0f1 ngày 2026-09-22.**

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

# PHASE 6 — Puzzle Variety

Final game cần ít nhất 3 temporal puzzle pattern có ý nghĩa.

## M6.1 — Puzzle Pattern A
- [ ] Dual Pressure Switch

## M6.2 — Puzzle Pattern B
Timed mechanism:
- [ ] timed door/lever
- [ ] Ghost action đúng recorded moment có ý nghĩa
- [ ] Player hiện tại làm complementary action

## M6.3 — Puzzle Pattern C
Combat cooperation:
- [ ] Ghost attack/distract target
- [ ] Player thực hiện objective thứ hai
HOẶC
- [ ] simultaneous enemy/target interaction

## M6.4 — Trap Mechanics
Có thể gồm:
- [ ] spikes
- [ ] timed hazard
- [ ] projectile trap
- [ ] moving hazard

Ít nhất một trap phải tương tác hợp lý với rewind.

## M6.5 — Puzzle Tutorialization
- [ ] giới thiệu mechanic đầu tiên trong môi trường an toàn
- [ ] visual hint
- [ ] failure state dễ hiểu
- [ ] không cần text wall

---

# PHASE 7 — Roguelite Progression

## M7.1 — Upgrade Framework
- [ ] Upgrade data representation
- [ ] apply upgrade
- [ ] giữ hiệu lực trong current run
- [ ] reset đúng khi new run

## M7.2 — 1-of-3 Upgrade UI
- [ ] 3 choices
- [ ] title
- [ ] description
- [ ] effect/value hiển thị rõ
- [ ] mobile touch support
- [ ] gameplay pause trong lúc chọn

## M7.3 — Upgrade Pool
Target các upgrade có ý nghĩa:
- [ ] Attack Damage
- [ ] Movement Speed
- [ ] Max HP
- [ ] Heal
- [ ] Dash Cooldown
- [ ] Loop Duration
- [ ] Ghost Damage
- [ ] Temporal/Ghost upgrade

Target khoảng 8–12 upgrade definition có ý nghĩa, không tạo hàng chục lựa chọn filler.

## M7.4 — Reward Integration
- [ ] reward sau room phù hợp
- [ ] treasure room
- [ ] elite reward
- [ ] upgrade choice không phá loop state

---

# PHASE 8 — Chrono Guardian Boss

## M8.1 — Boss Foundation
- [ ] boss Health
- [ ] boss HUD
- [ ] boss reset/lifecycle rule
- [ ] boss arena
- [ ] boss intro

## M8.2 — Phase 1
- [ ] readable normal attack
- [ ] melee/ranged pattern
- [ ] hazard telegraph

## M8.3 — Phase 2: Temporal Mechanic
- [ ] boss shield hoặc vulnerability mechanic
- [ ] yêu cầu Ghost cooperation
- [ ] Player hiểu vì sao cần Ghost

## M8.4 — Phase 3: Finale
- [ ] tăng pressure
- [ ] temporal hazard
- [ ] boss có thể disrupt Ghost hoặc manipulate timeline một cách fair
- [ ] final vulnerability window rõ

## M8.5 — Boss Polish
- [ ] hit feedback
- [ ] phase transition feedback
- [ ] SFX
- [ ] VFX
- [ ] death sequence
- [ ] Victory trigger

Acceptance:  
Boss đánh được, dễ hiểu, và thể hiện rõ core mechanic của game.

---

# PHASE 9 — Complete Game Flow

## M9.1 — Run State
- [ ] Start new run
- [ ] room progression
- [ ] upgrade giữ trong run
- [ ] Game Over reset đúng
- [ ] Victory kết thúc run

## M9.2 — Pause
- [ ] Pause button
- [ ] Resume
- [ ] Restart Run
- [ ] Main Menu
- [ ] pause không phá Time Loop/Ghost timeline

## M9.3 — Settings
Settings implementation được DEFER từ M0.1 sang milestone này; chưa triển khai trong M4.1.

Tối thiểu:
- [ ] master volume
- [ ] music volume hoặc music toggle đơn giản
- [ ] SFX volume hoặc SFX toggle đơn giản

Optional nếu dễ:
- [ ] vibration toggle
- [ ] quality setting

## M9.4 — Victory
- [ ] Victory screen
- [ ] replay/new run
- [ ] Main Menu
- [ ] optional run summary

## M9.5 — Tutorial
- [ ] movement
- [ ] attack
- [ ] time loop
- [ ] Ghost concept
- [ ] temporal puzzle đầu tiên
- [ ] dash khi được giới thiệu

Dùng contextual guidance ngắn gọn.

---

# PHASE 10 — Art, Animation, Audio, VFX

Phase này có thể làm nhẹ từ sớm, nhưng final polish tập trung ở đây.

## M10.1 — Visual Replacement
- [ ] thay hoặc cải thiện rõ các primitive placeholder
- [ ] Player visual nhất quán
- [ ] Ghost visual
- [ ] Enemy visual
- [ ] dungeon tile/environment
- [ ] switch/door visual
- [ ] boss visual

Asset có thể đơn giản, nhưng final game phải trông có chủ đích.

## M10.2 — Animation
Ưu tiên:
- [ ] Player idle/move
- [ ] Player attack
- [ ] dash feedback
- [ ] Enemy
- [ ] boss
- [ ] door
- [ ] Ghost readability

## M10.3 — VFX
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

## M10.4 — Audio
- [ ] menu music
- [ ] dungeon music
- [ ] boss music hoặc intensified variant
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

# PHASE 11 — Android & UX Polish

## M11.1 — Responsive UI
Test các landscape ratio phổ biến:
- [ ] 16:9
- [ ] wider phone ratio
- [ ] safe positioning
- [ ] không clipped HUD
- [ ] không overlapping controls

## M11.2 — Touch Testing
Test thủ công:
- [ ] joystick
- [ ] move + attack đồng thời
- [ ] move + dash
- [ ] rapid button presses
- [ ] Game Over button
- [ ] Pause UI
- [ ] Upgrade selection

## M11.3 — Performance
- [ ] profiler sanity check
- [ ] không có per-frame allocation lộ liễu từ system mới
- [ ] Ghost recording memory có giới hạn
- [ ] max Ghost count được giữ
- [ ] projectile/VFX được kiểm soát
- [ ] target frame rate ổn định trên Android device dự kiến

## M11.4 — Android Build
- [ ] final Exit/platform behavior verification (DEFER từ M0.1; kết hợp final platform QA)
- [ ] Android build thành công
- [ ] install lên real device
- [ ] launch thành công
- [ ] scene flow hoạt động
- [ ] touch hoạt động
- [ ] audio hoạt động
- [ ] app pause/resume behavior được kiểm tra

---

# PHASE 12 — Final Balancing and QA

## M12.1 — Full Run Testing
Chạy nhiều full run:

Main Menu  
-> Dungeon  
-> Rooms  
-> Upgrades  
-> Elite  
-> Boss  
-> Victory

Đồng thời test failure flow:

Main Menu  
-> Dungeon  
-> Game Over  
-> Restart  
-> Main Menu

## M12.2 — Difficulty
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
đủ thử thách để thể hiện mechanic nhưng không gây khó chịu khi presentation.

## M12.3 — Bug Checklist
- [ ] không NullReferenceException
- [ ] không MissingReferenceException
- [ ] không broken serialized reference
- [ ] không Ghost duplication sau Restart
- [ ] timer không tiếp tục khi pause/Game Over
- [ ] dead Enemy reset đúng
- [ ] không puzzle soft-lock
- [ ] không door collider desync
- [ ] Player không bị kẹt sau rewind
- [ ] không upgrade stacking bug
- [ ] không boss phase soft-lock

## M12.4 — Presentation Polish
- [ ] font nhất quán
- [ ] màu sắc nhất quán
- [ ] button style nhất quán
- [ ] HUD dễ đọc
- [ ] transition sạch
- [ ] feedback rõ
- [ ] không còn debug text chỉ dành cho development
- [ ] không còn visible placeholder label

---

# PHASE 13 — Final Deliverable

## M13.1 — Final Build
- [ ] clean Android APK/AAB theo yêu cầu
- [ ] final version number
- [ ] final icon
- [ ] final app/game title
- [ ] test install

## M13.2 — Demo Readiness
Chuẩn bị một demo path ổn định thể hiện:
- [ ] movement/combat
- [ ] rewind
- [ ] Ghost replay
- [ ] Ghost puzzle
- [ ] upgrade
- [ ] enemy variety
- [ ] boss
- [ ] Victory

Evaluator phải hiểu được unique mechanic của game nhanh chóng.

## M13.3 — Completion Definition

ChronoDungeon chỉ được coi là DONE khi:

- [ ] full run chơi được từ launch tới Victory;
- [ ] core Temporal Ghost mechanic là thiết yếu;
- [ ] có ít nhất 3 temporal puzzle pattern;
- [ ] combat có enemy variety rõ;
- [ ] upgrade progression hoạt động;
- [ ] boss hoàn chỉnh;
- [ ] UI/audio/VFX khiến game có cảm giác intentional;
- [ ] Android build hoạt động;
- [ ] final manual QA hoàn thành;
- [ ] không còn known game-breaking bug.

---

# NEXT MILESTONE

Codex phải inspect project trước khi tin marker này.

Next milestone after M5.5 verification on 2026-09-22:

**M6.1 - Puzzle Pattern A: Dual Pressure Switch**

M5.5 is complete; stop at the review checkpoint. M6.1 will verify and polish the two-switch Temporal Ghost
puzzle, although part of the room/puzzle foundation already exists from M2.3.
M0.1 là foundation DONE: Settings defer sang M9.3, final Exit/platform verification defer sang
Phase 11/final platform QA theo scope clarification của user. Các hạng mục device touch và
responsive layout của M0.2 vẫn cần final QA; không chặn milestone phát triển chính Room System.

Không tự skip sang milestone sau chỉ vì nó hấp dẫn hơn.  
Hoàn thành, test và integrate milestone chưa hoàn thành sớm nhất trước.
