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
- [x] SplashScene
- [x] MainMenuScene
- [x] GameScene
- [x] Splash -> Main Menu
- [x] Play -> GameScene
- [ ] hoàn thiện Settings behavior
- [ ] verify final Exit/platform behavior

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
- [ ] Ghost1 đại diện Loop1
- [ ] Ghost2 đại diện Loop2
- [ ] Ghost3 đại diện Loop3
- [ ] maxGhosts = 3
- [ ] loại Ghost cũ nhất khi vượt giới hạn
- [ ] Restart xóa recordings/Ghosts
- [ ] Game Over không làm hỏng recording state

Acceptance:  
Test ít nhất 4 lần chuyển loop và không bao giờ có hơn 3 active Ghost.

## M1.3 — Action Timeline
Mở rộng timeline mà không viết lại movement system.

Event support bắt buộc:
- [ ] Attack event
- [ ] facing/attack direction
- [ ] Ghost attack playback
- [ ] Ghost có thể damage Enemy hợp lệ
- [ ] Ghost attack không ảnh hưởng Player
- [ ] attack timing khớp timeline đã record

Kiến trúc phải sẵn sàng cho:
- [ ] Dash event
- [ ] Interaction event
- [ ] Skill event

Acceptance:  
Player attack trong Loop1.  
Ghost1 lặp lại attack gần đúng timeline moment đó trong Loop2.

---

# PHASE 2 — First Complete Time Puzzle

## M2.1 — Pressure Switch
- [ ] reusable PressureSwitch
- [ ] Player activate được
- [ ] Ghost tương thích activate được
- [ ] visual state active/inactive rõ
- [ ] resettable khi rewind

## M2.2 — Door
- [ ] reusable Door
- [ ] locked/open state
- [ ] opening animation hoặc visual feedback rõ
- [ ] reset support khi cần
- [ ] không trap Player vì collider/state mismatch

## M2.3 — Dual-Switch Puzzle
Scenario bắt buộc:
- [ ] Loop1 Player activate Switch A
- [ ] rewind
- [ ] Ghost1 activate Switch A
- [ ] Player hiện tại activate Switch B
- [ ] Door mở
- [ ] Player đi qua được

Acceptance:  
Puzzle chỉ solve được khi dùng ít nhất một Ghost.

## M2.4 — Puzzle Feedback
- [ ] switch SFX/VFX
- [ ] door SFX/VFX
- [ ] connection giữa switch và door dễ hiểu
- [ ] basic tutorial hint cho temporal puzzle đầu tiên

---

# PHASE 3 — Player Combat Polish

## M3.1 — Directional Combat
- [ ] attack direction/facing
- [ ] attack visual
- [ ] hit feedback rõ
- [ ] attack cooldown
- [ ] ngăn accidental multi-hit bug

## M3.2 — Dash
- [ ] Dash button
- [ ] dash movement
- [ ] cooldown
- [ ] mobile-friendly behavior
- [ ] visual feedback
- [ ] Ghost timeline support nếu cần

## M3.3 — Player Feedback
- [ ] hit flash
- [ ] damage feedback
- [ ] attack SFX
- [ ] damage SFX
- [ ] death feedback
- [ ] optional restrained camera shake

Acceptance:  
Combat responsive và dễ hiểu mà không cần debug visual.

---

# PHASE 4 — Dungeon Room System

## M4.1 — Room Architecture
- [ ] Room component
- [ ] room lifecycle
- [ ] enter room
- [ ] lock room nếu cần
- [ ] clear condition
- [ ] completion
- [ ] unlock exit
- [ ] transition sang room tiếp theo

## M4.2 — Dungeon Layout
Tạo authored dungeon khoảng 6–8 room.

Target:
- [ ] Start Room
- [ ] Combat Room
- [ ] Time Puzzle Room
- [ ] Trap Room
- [ ] Upgrade/Treasure Room
- [ ] Combat/Puzzle Room thứ hai
- [ ] Elite Room
- [ ] Boss Room

KHÔNG ưu tiên procedural generation.

## M4.3 — Tilemap / Environment
- [ ] floor
- [ ] walls
- [ ] collision
- [ ] room boundaries
- [ ] visual theme consistency
- [ ] Player không thoát khỏi level geometry

## M4.4 — Room + Time Loop Interaction
Phải quyết định rõ và implement:
- [ ] loop restart position behavior theo room
- [ ] room-local reset behavior
- [ ] Ghost lifecycle giữa các room
- [ ] timeline có reset khi sang room hay không
- [ ] ngăn Ghost cũ phá gameplay room mới

Recommended default:  
Mỗi major room là một local temporal encounter; khi vào room mới nên reset/clear Ghost history trừ khi gameplay testing chứng minh rule khác tốt hơn.

---

# PHASE 5 — Enemy Roster

## M5.1 — Enemy Base Improvements
- [ ] reusable damage/death behavior
- [ ] telegraphing
- [ ] loop reset compatibility
- [ ] room integration

## M5.2 — Enemy 1: Chaser / Slime
- [ ] chase
- [ ] contact/melee attack
- [ ] visual rõ
- [ ] tuned speed/damage

## M5.3 — Enemy 2: Archer
- [ ] ranged behavior
- [ ] distance management
- [ ] projectile
- [ ] projectile pooling hoặc reuse hiệu quả nếu cần
- [ ] projectile reset/cleanup khi rewind
- [ ] telegraphed attack

## M5.4 — Enemy 3: Knight HOẶC Mage
Chọn một dựa trên gameplay quality và development cost.

Knight option:
- [ ] charge
- [ ] heavy attack
- [ ] block/defense state

Mage option:
- [ ] ranged spell
- [ ] AoE marker
- [ ] teleport/reposition

## M5.5 — Elite Enemy
- [ ] reuse enemy foundation hiện có
- [ ] stronger stats/pattern
- [ ] distinctive visual
- [ ] meaningful reward

Acceptance:  
Ba normal enemy archetype tạo ra ba kiểu quyết định combat khác nhau rõ rệt.

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

Next milestone sau khi verify M1.1 ngày 2026-09-21:

**M1.2 — Multiple Ghosts**

M1.1 đã hoàn thành; dừng tại checkpoint review. Các hạng mục final Settings,
Exit/platform, responsive layout và device touch của Phase 0 vẫn chưa hoàn thành;
không thay đổi trạng thái của chúng trong milestone này.

Không tự skip sang milestone sau chỉ vì nó hấp dẫn hơn.  
Hoàn thành, test và integrate milestone chưa hoàn thành sớm nhất trước.
