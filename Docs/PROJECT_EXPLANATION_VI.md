# ChronoDungeon — Giải thích project và cách chạy

## 1. Vì sao màn hình hiện tại chỉ có nền xanh?

Ảnh Unity đang mở **scene trống `Untitled`**, không phải scene của game.

Dấu hiệu:

- Thanh tiêu đề hiện `ChronoDungeon - Untitled`.
- Hierarchy chỉ có `Untitled` và `Main Camera`.
- Game view chỉ hiện màu nền xanh của Camera.

Vì không có Player, UI, Manager, map hay enemy trong scene `Untitled`, bấm Play ở đây sẽ **không chạy game ChronoDungeon**. Đây không phải bằng chứng game code bị mất hay game không thể chạy.

## 2. Cách mở và chạy đúng game

1. Nếu nút hình vuông màu xanh trên thanh công cụ đang bật, nhấn nó để **dừng Play Mode**.
2. Trong cửa sổ **Project**, mở `Assets > Scenes`.
3. Nhấp đúp file `GameScene.unity`.
4. Kiểm tra Hierarchy: scene đúng sẽ có các object như `Managers`, `Player`, `Canvas`, `ManualWorld` và `Map01_RuinedEntrance`.
5. Chờ Unity import/compile xong.
6. Mở tab **Console**; nếu có biểu tượng đỏ Error, đọc lỗi đầu tiên trước khi Play.
7. Nhấn nút tam giác **Play**.

Các scene trong project là:

```text
SplashScene → MainMenuScene → GameScene
```

`GameScene` là scene gameplay chính. Build Settings cũng đã đưa ba scene trên vào thứ tự build này.

## 3. Luồng màn hình và menu

### SplashScene

`SplashManager` chạy thanh loading giả lập trong khoảng 3 giây, sau đó gọi:

```csharp
SceneManager.LoadScene("MainMenuScene");
```

Thanh loading này dựa trên thời gian, **không phản ánh tiến độ tải asset thật**.

### MainMenuScene

Scene menu hiện có ba nút trực quan:

- **Play**: gọi `MainMenu.PlayGame()` để mở `GameScene`.
- **Settings**: có GameObject `SettingButton`, nhưng `Button.onClick` hiện đang trống và MainMenuScene không có `SettingsManager`/`SettingsFlowUI`. Nút này chưa hoạt động đầy đủ.
- **Exit**: gọi `Application.Quit()`. Trong Unity Editor, lệnh này không đóng cửa sổ Editor; nó chỉ có hiệu lực trong bản build.

### Menu trong lúc chơi

Trong `GameScene`, `PauseManager` đặt `Time.timeScale = 0` khi pause. `PauseFlowUI` cung cấp:

- Resume;
- Stats;
- Restart Run;
- Main Menu.

`SettingsFlowUI` trong GameScene có ba slider Master, Music và SFX, cùng Reset Defaults và Close. Giá trị được lưu bằng `PlayerPrefs`.

Lưu ý: `MasterVolume` được áp trực tiếp vào `AudioListener.volume`, còn Music/SFX mới chỉ được lưu và phát event; cần kiểm tra các Audio Director có thực sự đọc hai giá trị này khi mix âm thanh hay chưa.

### Màn hình kết thúc

- Player hết máu → `GameManager.GameOver()` → hiện Game Over và dừng thời gian.
- Boss cuối chết → `GameManager.Victory()` → hiện Victory và dừng thời gian.
- Restart/Replay reload lại `GameScene`.
- Main Menu quay về `MainMenuScene`.

## 4. Game này là gì?

**ChronoDungeon – Trapped Beyond Time** là game Unity 2D top-down cho Android ngang, pha trộn:

- action combat;
- puzzle;
- roguelite progression;
- time loop và Temporal Ghost.

Ý tưởng trung tâm: người chơi bị kẹt trong dungeon lặp thời gian. Hành động ở loop trước sẽ trở thành một Ghost ở loop sau. Người chơi hiện tại dùng Ghost đó để cùng giải puzzle hoặc đánh combat.

Ví dụ đơn giản:

```text
Loop 1: Player đứng lên Switch A.
Hết 20 giây: thế giới rewind.
Loop 2: Ghost lặp lại việc đứng lên Switch A.
         Player hiện tại đứng lên Switch B.
         Cửa mở vì hai switch cùng được kích hoạt.
```

## 5. Một vòng lặp thời gian hoạt động ra sao?

### Bước 1 — Vào một room

`RoomManager` kích hoạt room, đóng cửa ra, đặt Player về spawn point và bắt đầu encounter.

### Bước 2 — Chơi trong khoảng 20 giây

`TimeLoopManager` đếm ngược 20 giây. Trong lúc đó Player có thể di chuyển, đánh và dash.

`PlayerTimelineRecorder` ghi lại:

- vị trí/hướng Player khoảng mỗi `0.05` giây;
- thời điểm và hướng Attack;
- thời điểm và hướng Dash.

### Bước 3 — Rewind

Khi hết giờ, `TimeLoopManager` gọi `ResetToInitialState()` trên các object hỗ trợ time loop. Player, enemy, switch, door, trap và room phù hợp sẽ quay về trạng thái đầu loop mà không cần reload scene.

### Bước 4 — Spawn Temporal Ghost

`TemporalGhostManager` copy bản ghi vừa hoàn thành sang `GhostPlayback`.

Ghost sẽ:

- hiện hình Player bán trong suốt;
- đi theo path đã ghi;
- replay Attack và Dash đã ghi;
- có thể tác động lên pressure switch;
- không có joystick, Health hoặc collider Player;
- không tự đưa Player đến Game Over.

Tối đa có **3 Ghost**. Khi có Ghost thứ tư, Ghost cũ nhất bị bỏ.

### Bước 5 — Hoàn thành room

Room được clear theo một trong các điều kiện:

- vào room là hoàn thành;
- giết hết enemy;
- giải puzzle;
- có Rune Key nếu exit yêu cầu.

Sau khi clear, `Door` mở để sang room tiếp theo.

## 6. Cấu trúc level/map

Cần phân biệt hai khái niệm:

- **Thiết kế mục tiêu** trong `GAME_DESIGN_LOCK.md`: chiến dịch gồm 8 map hoàn chỉnh.
- **Trạng thái thực tế**: `GameScene` đã có các root `Zone01` đến `Zone08` và nhiều hệ thống prototype, nhưng handoff xác nhận Map 01 mới là vertical slice đang được ưu tiên hoàn thiện. Không nên hiểu việc thấy đủ tên Zone trong Hierarchy là cả 8 map đã hoàn thành.

| Map | Vai trò thiết kế | Cơ chế/nội dung chính | Trạng thái nên hiểu |
|---|---|---|---|
| 1 — Ruined Entrance | Onboarding | Di chuyển, đánh tay không, nhặt Chrono Blade, Rune Key, upgrade đầu tiên; chưa dùng Ghost | Map có art/collision thực tế và đang được hoàn thiện |
| 2 — Forgotten Barracks | Combat training | Dash, ưu tiên mục tiêu, shop Gold, mini-boss | Có zone/prototype; chưa được handoff xác nhận hoàn thiện |
| 3 — Overgrown Wilds | Giới thiệu Time Loop | Ghost đầu tiên, dual switch/timed puzzle, Ember Staff | Cơ chế Ghost có code nền; content map cần kiểm chứng |
| 4 — Echo Library | Tăng độ khó puzzle | Sequence/recorded action, Astral Index, boss Silent Curator | Chủ yếu là thiết kế/prototype |
| 5 — Ash Forge | Hazard + temporal puzzle | Conveyor, valve, lửa, Ash Warden | Chủ yếu là thiết kế/prototype |
| 6 — Shattered Mirror Prison | Mở đủ 3 Ghost | Multi-loop planning, facing/timing, Void Bow | Chủ yếu là thiết kế/prototype |
| 7 — Warden Approach | Kiểm tra tổng hợp | Elite gauntlet, paradox puzzle, shop cuối | Chủ yếu là thiết kế/prototype |
| 8 — Chrono Sanctum | Finale | Chrono Guardian ba phase và Victory | Boss code nền đã có; full level cần kiểm chứng |

### Map 01 thực tế

Map 01 là một world liên tục, không dùng NavMesh. Player di chuyển bằng `Rigidbody2D`; biên đường đi dùng `Collider2D` thủ công.

Luồng thiết kế của Map 01:

```text
Awakening Chamber
→ Fallen Passage
→ Ruined Courtyard
→ Broken Armory
→ Rune Gate Hall
→ Gatebreaker Arena
→ Exit Corridor sang Map 02
```

Editor setup hiện chia vùng thành các sub-area như Entrance Hall, Connecting Corridor, Fallen Courtyard, Rune Key Area và Locked Rune Gate Approach. `WorldAreaManager` theo dõi Player đang ở sub-area nào để điều phối camera/presentation; đây không phải hệ thống pathfinding.

### Room và Zone

`RoomManager` quản lý progression theo thứ tự. Mỗi `Room` có:

- role: Start, Combat, TimePuzzle, Trap, Treasure, CombatChallenge, Elite hoặc Boss;
- spawn point;
- content bật/tắt;
- exit door;
- điều kiện clear;
- cờ có sử dụng temporal loop hay không.

Khi Player đi sang room mới, manager dừng dash/input cũ, cập nhật vị trí/camera, kích hoạt content và bắt đầu timeline mới nếu room là temporal zone.

## 7. Các hệ thống chính trong source

| Hệ thống | File/nhóm file | Vai trò |
|---|---|---|
| Quản lý game state | `Assets/Scripts/Managers/GameManager.cs` | Game Over, Restart, Main Menu, Victory. |
| Time loop | `Managers/TimeLoopManager.cs` và `ITimeLoopResettable.cs` | Đếm giờ, rewind, reset state. |
| Temporal Ghost | `Temporal/PlayerTimelineRecorder.cs`, `PlayerTimeline.cs`, `TemporalGhostManager.cs`, `GhostPlayback.cs` | Ghi loop và replay Ghost. |
| Room/dungeon | `Managers/Room.cs`, `RoomManager.cs` | Thứ tự room, clear condition, spawn và exit. |
| Player combat | `Player/PlayerMovement.cs`, `PlayerAttack.cs`, `PlayerDash.cs`, `Health.cs` | Di chuyển, đánh, dash, máu. |
| Enemy | `Enemy/EnemyFollow.cs`, `EnemyArcher.cs`, `EnemyKnight.cs` | Chaser, ranged archer, heavy knight. |
| Puzzle | `Environment/PressureSwitch.cs`, `DualPressureDoor.cs`, `TimedDoorMechanism.cs` | Switch, door kép, cơ chế thời gian. |
| Vũ khí/skill | `Weapons/PlayerWeaponController.cs`, `WeaponDefinition.cs`, `SkillDefinition.cs` | Trang bị, basic attack, skill cooldown. |
| Upgrade | `Progression/UpgradeManager.cs`, `UpgradeRewardManager.cs` | Chọn 1 trong 3 upgrade sau room phù hợp. |
| Boss | `Enemy/ChronoGuardian*.cs` | Boss, telegraph, shield temporal và finale. |
| UI | `UI/VirtualJoystick.cs`, `TimeLoopHUD.cs`, `UpgradeChoiceUI.cs`, `VictoryFlowUI.cs` | Mobile control, timer, lựa chọn upgrade, end screen. |

## 8. Combat, vũ khí và enemy

Player có:

- movement bằng joystick mobile; WASD chỉ để test trong Editor;
- basic directional attack;
- dash với cooldown;
- máu và resource;
- đổi vũ khí/skill thông qua `PlayerWeaponController`.

Data hiện có ba weapon asset:

1. **Chrono Blade** — melee temporal.
2. **Ember Staff** — ranged/fire area.
3. **Void Bow** — precision projectile/range.

Enemy hiện có:

- **Chaser**: tiếp cận Player;
- **Archer**: ngắm/telegraph rồi bắn projectile;
- **Knight**: chase, charge và block;
- **Elite**: biến thể mạnh hơn và có reward.

Chrono Guardian có nhiều component: Phase 1 slam telegraph, Temporal Shield và Finale. Finale kích hoạt khi máu boss xuống 50%, tạo hazard, làm Ghost bị disrupt ngắn hạn và mở cửa sổ boss vulnerable.

## 9. Những thiếu sót của game

Đây là các điểm đọc được trực tiếp từ source, không phải kết luận từ một lần Play Mode:

### Mức nghiêm trọng — ảnh hưởng trực tiếp gameplay

1. **Ghost chưa replay Skill hoặc Interaction.**
   - `PlayerTimeline.ActionKind` có `Interaction` và `Skill`.
   - `GhostPlayback` hiện chỉ xử lý `Attack` và `Dash`.
   - Vì vậy mục tiêu Ghost replay toàn bộ action quan trọng chưa đạt.

2. **Ba upgrade được khai báo nhưng chưa áp dụng gameplay.**
   - Upgrade pool có `LoopDuration`, `GhostDamage`, `TemporalAbility`.
   - `UpgradeManager.ApplyUpgrade()` mới thực hiện attack damage, movement speed, max health, dash cooldown và heal.
   - Cần nối ba upgrade temporal vào hệ thống tương ứng trước khi dùng trong bản demo cuối.

3. **Boss hiện hồi đầy HP khi rewind.**
   - `ChronoGuardian.ResetToInitialState()` gọi `RestoreToFullHealth()`.
   - Tài liệu thiết kế yêu cầu boss HP giữ qua loop, nên code hiện tại mâu thuẫn với thiết kế và cần quyết định/sửa rõ ràng.

### Mức cao — content và progression chưa khớp tài liệu

4. **Project đang thiên về Map 01 vertical slice.**
   - Tài liệu thiết kế mục tiêu 8 map, nhưng Build Settings hiện chỉ dùng `SplashScene`, `MainMenuScene`, `GameScene`.
   - GameScene có root Zone01–Zone08, nhưng handoff ghi ưu tiên hiện tại là hoàn thiện `Map01_RuinedEntrance` trước Map 02. Nhiều zone có thể là prototype chứ chưa phải level final.

5. **Nút Settings ở Main Menu chưa được nối.**
   - `SettingButton` tồn tại nhưng danh sách `m_OnClick` trống.
   - `SettingsFlowUI` chỉ được gắn trong GameScene, không có trong MainMenuScene.

6. **Checkpoint/save theo thiết kế chưa thấy implementation hoàn chỉnh.**
   - Có `RunStateManager`, nhưng chưa thấy hệ thống lưu đầy đủ map, anchor, weapon, upgrade, enemy/chest/puzzle state như Game Design Lock yêu cầu.

7. **Ghost limit chưa tiến triển theo map.**
   - Design nói Map 3 bắt đầu một Ghost và Map 6 mở ba Ghost.
   - `TemporalGhostManager` hiện cấu hình tối đa 3 ngay từ đầu; cần cơ chế unlock/limit theo progression nếu muốn đúng thiết kế.

### Mức trung bình — UI, âm thanh và độ hoàn thiện

8. **UI Pause/Settings/Victory được dựng bằng code runtime.**
   - Điều này mâu thuẫn với UI Bible yêu cầu layout authored trong scene.
   - Khó chỉnh responsive, sprite, anchor và polish trực tiếp trong Unity Editor.

9. **Splash loading không phải loading thật.**
    - Thanh chạy đủ 3 giây rồi đổi scene, kể cả asset đã tải xong sớm hoặc chưa sẵn sàng.

10. **Settings âm thanh chưa chắc điều khiển đúng từng nhóm.**
    - Master tác động `AudioListener.volume`.
    - Music và SFX được lưu nhưng cần nối vào AudioMixer hoặc Audio Director tương ứng.

11. **Collision Map 01 vẫn cần kiểm thử thủ công.**
    - Đây là game physics 2D, không phải NavMesh.
    - Handoff nói collision mới là first-pass; có nguy cơ Player đi ra void hoặc bị chặn sớm ở góc/khe.

12. **Nhiều dependency dùng `FindAnyObjectByType`.**
    - Tiện cho prototype nhưng làm dependency ẩn, khó kiểm soát khi scene lớn hoặc additive scene.
    - Nên chuyển các reference quan trọng sang serialized reference/event/service rõ ràng.

### Chưa được xác minh

13. **Chưa khẳng định project sạch lỗi sau package repair.**
   - `manifest.json` vừa được sửa để loại package Unity không tồn tại.
   - Cần mở `GameScene`, chờ compile và kiểm tra Console trước khi kết luận game chạy ổn.

14. **Chưa có bằng chứng Android device test mới.**
    - Chạy trong Editor không chứng minh joystick đa chạm, Safe Area, aspect ratio, hiệu năng và Android build đều ổn.

## 10. Đánh giá tổng thể

Project có một ý tưởng đủ khác biệt để làm đồ án: dùng chính hành động quá khứ làm đồng đội. Kiến trúc time loop, timeline và Ghost được tách component khá rõ; combat, room, upgrade và boss đều đã có foundation.

Điểm yếu lớn nhất hiện không phải “thiếu thật nhiều feature”, mà là **khoảng cách giữa tài liệu thiết kế rất lớn và mức hoàn thiện thực tế**. Hướng hợp lý là khóa Map 01 thành vertical slice thật ổn định, sau đó hoàn thiện một temporal map đại diện và boss finale, thay vì coi các Zone placeholder là tám map đã xong.

Thứ tự ưu tiên đề xuất:

1. Compile sạch và kiểm tra Console.
2. Test collision/flow Map 01 từ spawn đến exit.
3. Sửa upgrade không có tác dụng và nút Settings Main Menu.
4. Quyết định quy tắc boss HP qua rewind.
5. Hoàn thiện Ghost Skill/Interaction nếu puzzle cần chúng.
6. Test Android thật trước khi mở rộng content.

## 11. Checklist test đầu tiên

Sau khi mở đúng `GameScene`:

- [ ] Console không có lỗi đỏ mới.
- [ ] Player xuất hiện trên Map 01.
- [ ] Di chuyển được bằng WASD trong Editor.
- [ ] Attack và Dash hoạt động.
- [ ] Time HUD chạy và rewind sau khoảng 20 giây.
- [ ] Ghost xuất hiện sau loop đầu và di chuyển lại theo Player cũ.
- [ ] Switch/Door phản hồi Player hoặc Ghost.
- [ ] Game Over/Restart/Main Menu không bị lỗi.

Nếu GameScene vẫn không chạy, hãy gửi ảnh **Console đang mở**, ưu tiên dòng Error đỏ đầu tiên. Ảnh đó mới cho biết lỗi compile/runtime cụ thể cần sửa.
