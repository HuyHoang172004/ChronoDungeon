# ChronoDungeon — Development Handoff

Tài liệu này dành cho agent tiếp theo tiếp quản project. Đây là bản tóm tắt hiện trạng kỹ thuật, không thay thế các tài liệu source of truth bên dưới.

## Thứ tự ưu tiên tài liệu

1. `AGENTS.md` — quy tắc làm việc, Unity MCP, testing và Git safety.
2. `Docs/GAME_DESIGN_LOCK.md` — source of truth cho thiết kế đã được duyệt: gameplay, map, combat, asset standard và UI rules.
3. File này — hiện trạng thực tế sau các thay đổi gần nhất và việc cần làm tiếp.
4. `ROADMAP.md` — checklist milestone cũ; phải đối chiếu với project thực tế trước khi tin trạng thái.
5. `Docs/CHRONODUNGEON_GAME_BLUEPRINT.md` — tài liệu tổng hợp để tham khảo ý tưởng.
6. `Docs/CHRONODUNGEON_IMPLEMENTATION_ROADMAP.md` — roadmap cũ hơn, dùng tham khảo dependency, không dùng làm trạng thái cuối cùng.

`AGENTS_UPGRADE.md` là tài liệu định hướng nâng cấp cũ. Không tự động làm theo nếu mâu thuẫn với `GAME_DESIGN_LOCK.md` hoặc scene/code hiện tại.

## Quy tắc tiếp quản bắt buộc

- Inspect project và GameScene bằng Unity MCP trước khi sửa.
- Đọc `AGENTS.md`, `ROADMAP.md`, file này và các phần liên quan trong `GAME_DESIGN_LOCK.md`.
- Không đọc/sửa `Library`, `Temp` hoặc generated IDE files.
- Không commit, push, reset, rebase hoặc sửa `.gitignore`.
- Không xóa gameplay core chỉ vì presentation cũ đang được thay.
- Mọi thay đổi scene phải compile, Play Mode test, kiểm tra Console, save scene và báo manual test còn thiếu.
- Không dùng Transform scale/offset để che lỗi sprite source nếu có thể sửa import/pivot/baseline ở source art.

## Core thiết kế đã khóa

- Game: 2D top-down action + puzzle + roguelite, Android landscape.
- Map 01 `Ruined Entrance` là map onboarding combat/loot, không có Time Loop/Ghost gameplay.
- Player bắt đầu unarmed; Chrono Blade đến từ enemy đầu Map 1.
- Map dùng một Master Full Map liên tục, camera follow, không procedural layout.
- Art pipeline map: Master Base Map + Foreground Overlay + Collision Guide editor-only + manual gameplay collision + props/interactives riêng.
- Player/player-weapon source art dùng chung canvas, pivot chân và baseline.
- Màu: cyan/teal cho temporal, violet cho corruption, orange/red cho danger, gold cho reward.

## Hiện trạng GameScene

Scene: `Assets/Scenes/GameScene.unity`

Core đang giữ:

- `Managers`: GameManager, TimeLoopManager, TemporalGhostManager, upgrades, RunState, Pause, Settings.
- `Main Camera` với MapCameraFollow.
- `Canvas`, EventSystem và UI/gameplay flow hiện có.
- `Player` với movement, Rigidbody2D, collider, attack, dash, Health, timeline, weapon/skill runtime và resources.

Map 01 hiện tại:

```text
ManualWorld
└── Map01_RuinedEntrance
    ├── BackgroundArt
    │   └── BaseMapArt
    ├── ForegroundArt
    │   └── FrontWallsAndOccluders
    ├── CollisionGuide_EditorOnly (inactive khi Play)
    ├── Props
    ├── Enemies
    ├── Loot
    ├── NPC
    ├── Collision
    │   ├── WalkableBoundary_00...
    │   └── MapPerimeter_00...03
    ├── Gameplay
    └── CameraBounds
```

Map art đang dùng:

- `Assets/ArtSource/Maps/Map01_RuinedEntrance/Master/CD_Map01_Master_Base_Composition_v01.png`
- `Assets/ArtSource/Maps/Map01_RuinedEntrance/Foreground/CD_Map01_Foreground_Overlay_v01.png`
- `Assets/ArtSource/Maps/Map01_RuinedEntrance/Collision/CD_Map01_CollisionGuide_v01.png`

Map master hiện được import ở 30 PPU, kích thước khoảng `72.4 x 24.15` world units. Camera orthographic size hiện là `5.2`.

`CameraBounds` và `Map01CameraBounds` phải là trigger. Chúng chỉ dành cho camera, không được chặn Player.

Collision Map 01 hiện là first-pass được tạo từ vùng xanh của Collision Guide, gồm 27 contour và 4 perimeter edges. Đây chưa phải manual collision đã được người dùng test hết mọi đoạn; cần chạy WASD/joystick quanh từng khu để chỉnh các khe hoặc góc còn lệch.

Backup scene trước clean/rebuild gần nhất:

`Assets/Scenes/Backup/GameScene_BeforeCleanRebuild_20261008.unity`

## Player art hiện tại

- `Player/PlayerVisual` dùng `PlayerWeaponVisualSetController`.
- Base và Chrono Blade source sheets đã được slice thành sprite frame bằng:
  `Assets/Editor/ChronoSourcePlayerArtImporter.cs`
- Visual sets:
  - `Assets/Data/PlayerVisuals/BaseSourceVisualSet.asset`
  - `Assets/Data/PlayerVisuals/ChronoBladeSourceVisualSet.asset`
- Player source sprite import hiện dùng 420 PPU và custom bottom-center foot pivot.
- `PlayerVisual` local scale là `(1,1,1)`. Không thay Player root scale/collider để sửa visual.
- Visual bootstrap cũ `ChronoVisualReplacement` và `ChronoAnimationDirector` đã được chặn không tự sinh marker/ring presentation cũ.

## Những phần chưa hoàn tất

Ưu tiên gần nhất là hoàn thiện Map 01 vertical slice bằng asset mới:

1. Thay enemy prototype `Map01_WeaponBearer` bằng Ruin Husk source art và animation; sau đó thêm Cracked Archer, Heavy Warden và Gatebreaker theo đúng vị trí floor.
2. Gắn shadow, HP feedback, hit/death animation và loot drop mới.
3. Thêm Ruin Scout, Rune Gate, Chrono Anchor, chest, Rune Key và props Map 01 từ asset pack.
4. Manual test toàn bộ collision Map 01 bằng WASD và joystick; sửa đúng các đoạn Player có thể đi ra void hoặc bị chặn sớm.
5. Kiểm tra room objective, enemy clear, Chrono Blade drop/equip và exit sang Map 02.
6. Sau khi Map 01 ổn định mới thay sâu HUD/Menu bằng UI source art mới.

Không chuyển sang Map 02 cho đến khi Map 01 có thể chơi từ spawn đến exit ổn định.

## Quy trình test mỗi milestone

1. Inspect hierarchy/component/reference hiện tại.
2. Backup scene nếu có thao tác cleanup lớn.
3. Sửa source/scene.
4. Chờ Unity compile.
5. Clear và đọc Console.
6. Play Mode test feature mới.
7. Regression test Player movement, attack, dash, Health và scene flow.
8. Kiểm tra lại Console.
9. Exit Play Mode.
10. Save GameScene.
11. Báo file sửa, object sửa, test tự động/manual và limitation.

## Cảnh báo về trạng thái tài liệu

`ROADMAP.md` có các marker cũ đánh dấu nhiều presentation milestone đã hoàn thành trong khi source-art integration hiện tại vẫn còn dở dang. Không dùng marker đó để bỏ qua kiểm tra scene thực tế.

Khi hoàn tất một milestone mới, cập nhật `ROADMAP.md` trung thực và cập nhật file handoff này.
