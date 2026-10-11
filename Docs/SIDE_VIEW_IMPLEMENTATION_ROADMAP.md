# ChronoDungeon — Side-View Implementation Roadmap

Tài liệu này là lộ trình triển khai kỹ thuật và asset cho bản Side-View Rebuild. Source of truth thiết kế là Docs/GAME_DESIGN_LOCK.md.

## Nguyên tắc

- Không xóa source asset top-down/4 hướng hiện có.
- Dùng Right-facing row hiện có làm prototype side-view và flipX cho hướng trái khi phù hợp.
- Chỉ tạo asset mới khi side-view thật sự thiếu pose hoặc silhouette không đọc được.
- Không dựng map hoàn chỉnh trước khi side-view movement, jump, camera và collision prototype ổn định.
- Mỗi phase phải compile, test Play Mode, kiểm Console và save trước khi sang phase sau.

## Asset reuse contract

### Map/environment

- Toàn bộ map phải thiết kế và dựng lại theo side-view: background/parallax, midground, platform, wall, bridge, foreground, collider và camera bounds.
- Map top-down cũ không dùng trực tiếp làm playable map side-view; chỉ giữ làm reference về palette, stone texture, rune, torch, prop hoặc composition mood.

### Character/gameplay assets

- Giữ source asset hiện có của Player Base, Chrono Blade, Ember Staff, Void Bow, normal enemy, NPC và boss.
- Với mọi character có left/right gameplay, dùng Right-facing row hiện có làm hướng chuẩn; Left dùng flipX nếu silhouette/weapon handedness cho phép.
- Up/Down rows không xóa; giữ trong ArtSource làm archive/reference, nhưng không dùng trong gameplay side-view.
- Không vẽ lại Idle, Run/Walk, Attack, Skill, Hit hoặc Death nếu Right-facing asset cũ đã đọc tốt ở góc ngang.
- Chỉ bổ sung hoặc chỉnh asset khi thật sự cần: Player/weapon Jump, Fall, Landing, Dash ngang; flying enemy air pose; boss/skill telegraph profile hoặc pose hiện tại không đọc đúng side-view.
- Ground enemy, NPC đứng yên và boss không cần Jump/Fall trừ khi thiết kế gameplay của chính đối tượng đó yêu cầu.

## Phase 0 — Safety and audit

Mục tiêu: chụp rõ hiện trạng trước khi chuyển đổi.

- Backup GameScene và các prefab Player quan trọng.
- Inspect Player, Rigidbody2D, collider, joystick, dash, attack, weapon system, TimeLoop/Ghost và camera.
- Lập bảng asset hiện có: Player Base, Chrono Blade, Ember Staff, Void Bow, enemy, boss, loot, VFX, NPC, UI.
- Đánh dấu asset nào dùng trực tiếp, asset nào dùng row Right + flipX, asset nào phải tạo side-view mới.
- Không xóa scene hoặc source asset ở phase này.

Gate: có backup, danh sách dependency và không có thay đổi gameplay.

## Phase 1 — Side-view technical prototype

Mục tiêu: chứng minh game điều khiển tốt ở góc nhìn ngang trước khi làm art/map.

- Tạo scene/prototype area riêng với sàn, tường, platform, hố và camera bounds đơn giản.
- Chuyển Player movement thành trái/phải + gravity + ground check.
- VirtualJoystick: trái/phải để chạy; flick/kéo lên vượt ngưỡng để nhảy.
- Thêm coyote time và jump buffer nhẹ.
- Chuyển Dash thành dash ngang.
- Camera follow ngang, có clamp và không block Player.
- Thiết lập layer collision cho Player, ground, one-way platform, enemy, projectile và trigger.
- Giữ WASD/editor fallback để test.

Gate:

- Joystick chạy/nhảy ổn, không nhảy nhầm khi chạy.
- Player không xuyên sàn/tường, không bị camera collider chặn.
- Dash, attack, Health và pause vẫn hoạt động.

### Phase 1 implementation status — 2026-10-11

- [x] Created an isolated `SideViewPrototype` scene; the existing `GameScene` remains the top-down reference scene.
- [x] Added side-view horizontal movement, gravity, ground check, joystick up-flick jump, coyote time, jump buffer, horizontal dash, and horizontal camera follow.
- [x] Added explicit side-view layers: Player, Ground, OneWay, Enemy, Projectile, and Trigger.
- [x] Verified grounded resting state and collision-safe dash against a dedicated prototype wall; Unity Console had no errors.
- [ ] Manual touch validation remains required: drag left/right, perform a deliberate up-flick to jump, and confirm normal horizontal motion does not trigger a jump.
- [ ] Do not replace `PlayerMovement`/`PlayerDash` in the production `GameScene` until this prototype is approved and the combat adapter is ready.

## Phase 2 — Four-map blueprint and environment bible

Mục tiêu: chốt toàn bộ layout trước khi sản xuất environment art hoặc dựng map hoàn chỉnh.

- Vẽ blueprint side-view cho cả 4 map: đường chính, đường phụ, platform, combat arena, Temporal Chamber, checkpoint, NPC/shop, chest, weapon reward, boss và exit.
- Chốt nhịp traversal/combat/puzzle cho mỗi map; không copy layout top-down cũ.
- Chốt environment kit cho mỗi map: background layers, midground stone/ruin, ground/platform edge, wall, bridge, foreground prop, torch/light và puzzle prop.
- Tạo Collision Guide riêng: walkable ground, wall, ceiling, one-way platform, void, boss arena, camera bounds và trigger zone.
- Chốt reuse matrix: asset cũ nào trở thành prop/reference, asset side-view nào cần mới.
- Review toàn bộ blueprint trước khi tạo master background hoặc paint environment.

Gate: cả 4 map đều có blueprint được duyệt; Map 1 có layout/collision guide đủ chi tiết để dựng vertical slice.

## Phase 3 — Player side-view asset pack

Mục tiêu: Player đọc rõ và nhất quán trong side-view.

Asset cần rà soát/tái dùng:

- Base Player Right-facing Idle, Walk và Punch hiện có.
- Chrono Blade Right-facing Idle, Walk, Basic Attack, Skill 1, Skill 2 và Hit.
- Ember Staff Right-facing Idle, Walk, Fire Bolt, Flame Nova, Meteor Rune và Hit.
- Void Bow Right-facing Idle, Walk, Void Arrow, Piercing Shot, Shadow Volley và Hit.

Asset cần tạo hoặc chỉnh nếu chưa có:

- Base Player: Jump, Fall, Landing, Dash ngang, Hit, Death.
- Mỗi weapon: Jump/Fall hold pose; chỉnh lại attack/skill chỉ khi pose 4 hướng cũ không hợp góc ngang.
- Shadow và after-image dùng chung.
- Player portrait/UI icon chỉ cập nhật nếu icon hiện tại không đọc tốt.

Integration:

- Canonical facing là Right; Left dùng flipX khi hợp.
- Giữ canvas 192×256, 128 PPU, bottom-center pivot, shared foot baseline.
- Không dùng scale/offset trên scene để che lỗi source sprite.
- Animator/state driver: Idle, Run, Jump, Fall, Landing, Attack, Skill1, Skill2, Dash, Hit, Death.

Gate: Player không crop chân, không trượt baseline, flip trái/phải đúng, attack/skill không đổi scale hoặc vị trí bất thường.

## Phase 4 — Weapon and combat conversion

Mục tiêu: cả ba weapon hoạt động theo chiều ngang với visual/hitbox đúng.

Chrono Blade:

- Temporal Slash Combo: hitbox/arc phía trước.
- Chrono Burst: wave cyan/teal ngang.
- Time Cleave: heavy arc ngang.
- Sửa VFX/weapon pose theo facing trái-phải.

Ember Staff:

- Fire Bolt bay ngang.
- Flame Nova AoE quanh Player.
- Meteor Rune đặt ở điểm phía trước Player.

Void Bow:

- Void Arrow bay ngang.
- Piercing Shot xuyên mục tiêu.
- Shadow Volley bắn fan ngang nhẹ.

Asset cần kiểm:

- Weapon pickup world visual, weapon icon, skill icon.
- Projectile, slash, burst, impact, cast, hit và cooldown VFX.
- Không tạo sprite mới nếu sprite/VFX hiện có đọc tốt ở side-view; ưu tiên sửa execution/hitbox trước.

Gate: equip/swap weapon, attack, skill, cooldown, damage, dash và HUD vẫn đúng; không duplicate combat system.

## Phase 5 — Enemy and boss side-view adaptation

Mục tiêu: tái sử dụng asset enemy/boss hiện có trước; chỉ tạo asset mới khi một pose hoặc telegraph thật sự thiếu cho góc nhìn ngang.

Mỗi normal enemy dùng lại Right-facing row hiện có và flipX cho trái khi phù hợp. Không cần tạo Jump/Fall cho ground enemy. Chỉ cần:

- Idle, Run/Walk, wind-up/telegraph, Attack, Hit, Death.
- Right-facing canonical frames; flipX trái nếu phù hợp.
- Shadow, HP feedback, projectile/hazard/telegraph nếu có.
- Collider gameplay tách khỏi visual.

Chỉ bổ sung asset nếu thiếu:

- Telegraph/hitbox visual không đọc được khi dùng side-view.
- Pose attack profile không phù hợp sau khi test.
- Airborne pose chỉ dành cho Wisp/flying enemy thực sự cần.
- Boss phase/VFX chưa có trong asset hiện tại.

Asset adaptation order:

1. Map 1: Ruin Husk, Cracked Archer, Armory Warden, Gatebreaker Husk.
2. Map 2: Vine Husk, Thorn Spitter, Root Brute, Thorn Matriarch.
3. Map 3: Archive Wisp, Living Tome, Mirror Wisp, Cell Guard, Mirror Jailer.
4. Map 4: elite palette variants, Temporal Echo Minion, Chrono Vanguard, Chrono Guardian.

AI conversion:

- Ground enemy: patrol, chase ngang, ledge safety, telegraph then attack.
- Archer/caster: giữ range ngang, projectile telegraph.
- Heavy: wind-up rõ, recovery rõ.
- Wisp/flying enemy: airborne movement controller riêng.

Gate: mỗi archetype test được độc lập với Player/weapon hiện có; không contact damage vô hình.

## Phase 6 — Common world, NPC and loot integration

Mục tiêu: tái sử dụng loot, UI icon, VFX, NPC và interactive asset hiện có; chỉ tạo bổ sung những asset side-view hoặc map-specific chưa tồn tại.

Ưu tiên giữ asset hiện có:

- Gold, Health Shard, Energy Shard, Temporal Bean, Rune Key, chest.
- Chrono Anchor, Stability Anchor, Rune Gate, pressure plate, lever, relay, bridge.
- Shared temporal VFX: loop start/reset, Ghost spawn/path, switch active, shield, seal beam, bridge grow.

NPC packs:

- Ruin Scout: Idle, Talk pose, portrait.
- Quartermaster Echo: Idle, Talk pose, portrait, shop prop.
- Grove Keeper: Idle, Talk/gesture, portrait.
- Archivist Prisoner: chained Idle, freed Talk pose, portrait.

All NPC dùng side-view Right + flipX, minimal animation, no combat set. Không tạo lại NPC nếu Idle/Talk hiện có vẫn đọc tốt; chỉ thêm/chỉnh profile pose khi cần.

Gate: pickup, shop, dialogue, key/gate, switch/door and anchor have readable visual feedback.

## Phase 7 — Map 1 side-view environment and vertical slice

Mục tiêu: hoàn thành một map side-view chơi được từ spawn đến exit.

Map 1 — Ruined Entrance:

- Far background, midground, gameplay platform/wall layer, foreground props/parallax. Đây là phần asset môi trường cần thiết kế mới theo side-view; map top-down cũ chỉ dùng làm reference/palette/prop source.
- Manual colliders cho ground, wall, ceiling, void và arena boundary.
- Ruin Scout, Chrono Blade drop/pickup, Rune Key, Rune Gate, chest, Chrono Anchor.
- Ruin Husk, Cracked Archer, Armory Warden và Gatebreaker Husk.
- Luồng: unarmed → punch first enemy → Blade drop → combat → key/gate → boss → upgrade → exit.

Gate:

- Full playthrough không cần Unity editor intervention.
- Collider không cho Player đi ra ngoài map hoặc bị chặn sớm.
- Boss, loot, upgrade, Game Over/retry và exit hoạt động.

## Phase 8 — Map 2 + Ember Staff + first Ghost

- Overgrown Wilds side-view environment.
- Quartermaster Echo shop và Grove Keeper tutorial.
- Một Ghost: Echo Relay, Delayed Root Bridge, combat/puzzle room.
- Ember Shrine và Ember Staff.
- Vine Husk, Thorn Spitter, Root Brute, Thorn Matriarch.

Gate: loop reset, Ghost replay jump/interaction, boss shield puzzle và Ember Staff hoạt động.

## Phase 9 — Map 3 + Void Bow + advanced Ghost

- Echo Prison side-view environment.
- Archivist Prisoner, Void Bow Reliquary.
- Two-Ghost and three-Ghost platform/mirror/relay puzzles.
- Archive Wisp, Living Tome, Mirror Wisp, Cell Guard, Mirror Jailer.

Gate: Ghost route deterministic đủ cho puzzle, không softlock, Mirror Jailer có damage window rõ.

## Phase 10 — Map 4 finale

- Warden Approach, elite gauntlet, Paradox Gauntlet, final Quartermaster shop.
- Chrono Vanguard mini-boss.
- Chrono Sanctum, Chrono Guardian ba phase, Victory Chamber.
- Game Over, retry from Anchor, Main Menu, Victory/replay flow.

Gate: một full run hoàn chỉnh từ Main Menu đến Victory.

## Phase 11 — Final polish and Android

- HUD responsive landscape, joystick touch test và combat buttons.
- SFX/music/ambience, hit feedback, boss telegraph, VFX pooling.
- Settings, volume/toggle, pause, stats, objective clarity.
- Android performance test, memory/GC check, resolution/safe area test.
- Full regression test sau mỗi major optimization.

Gate final: clean compile, no game-breaking runtime errors, Android build thành công và manual device playtest hoàn thành.
