# ChronoDungeon — Game Design Lock

Version: Side-View Rebuild

Đây là source of truth cho thiết kế hiện tại của ChronoDungeon: Trapped Beyond Time. File này thay thế hoàn toàn kế hoạch top-down/8-map trước đó.

## 1. Game identity

ChronoDungeon là game 2D side-scrolling action-platformer + time-puzzle roguelite cho Android landscape.

Player bị kẹt trong dungeon thời gian. Trong Temporal Chamber, hành động của Player được ghi lại trong một loop. Khi reset, Temporal Ghost phát lại đúng hành động đó. Player hiện tại phối hợp với Ghost để mở đường, giữ công tắc, vượt bẫy và tạo cửa sổ tấn công boss.

Ba trụ cột:

1. Combat dễ đọc, mạnh tay và phù hợp mobile.
2. Platform + Ghost puzzle: hành động quá khứ tạo điều kiện cho lượt hiện tại.
3. Progression gọn: weapon, loot, Gold, consumable và upgrade 1-trong-3.

Mục tiêu là một game ngắn nhưng hoàn chỉnh, chơi từ Main Menu đến Victory trong khoảng 55–85 phút ở lần đầu.

## 2. Scope đã khóa

- 4 map/chapter authored, side-scrolling liên tục.
- 3 weapon chính: Chrono Blade, Ember Staff, Void Bow.
- Player bắt đầu unarmed và nhận Chrono Blade trong Map 1.
- Tối đa 3 Temporal Ghost active.
- 4 boss chính: Gatebreaker Husk, Thorn Matriarch, Mirror Jailer, Chrono Guardian.
- Chrono Vanguard là mini-boss/elite boss ở Map 4, không phải map riêng.
- Không procedural map generation, inventory grid hoặc metaprogression trước khi full run ổn định.

## 3. Camera, movement và mobile controls

### Góc nhìn

- Camera side-view, follow Player chủ yếu theo trục X.
- Camera có thể dịch Y nhẹ ở khu nhiều tầng nhưng không snap giữa phòng.
- Map là world ngang rộng gồm background, midground, foreground/parallax và gameplay collider riêng.

### Movement

- Joystick kéo trái/phải: chạy trái/phải.
- Joystick flick/kéo lên rõ khi Player grounded: nhảy.
- Không tạo Jump button riêng.
- Jump chỉ kích hoạt khi input lên vượt ngưỡng rõ, mục tiêu Y >= 0.70, để tránh nhảy nhầm lúc chạy chéo.
- Không double-jump trong scope cơ bản.
- Player có gravity, ground check, coyote time nhẹ và jump buffer nhẹ.
- Down/drop-through one-way platform là optional, không bắt buộc ở Map 1.

### Combat controls

- Bên trái: VirtualJoystick hiện có.
- Bên phải: Attack lớn nhất; Dash, Skill 1, Skill 2 nhỏ hơn theo đường cong; Bean ở hàng dưới gần cụm combat.
- Không runtime rebuild visual UI; scene UI là source of truth.
- Q/E/F chỉ là editor debug input khi cần; mobile UI là input chính.

### Dash

- Dash ngang theo facing trái/phải, dùng được trên mặt đất và trên không theo balance.
- Dash không dùng Energy.
- Có cyan after-image/trail nhẹ, không che Player.

## 4. Time Loop và Temporal Ghost

Temporal loop chỉ tồn tại trong Temporal Chamber authored, không chạy toàn map.

Flow:

Enter chamber → 20-second loop begins → record actions
→ timer ends → Player returns to chamber start
→ Ghost replays prior loop → current Player cooperates
→ reach Stability Anchor → chamber locks completed state

Ghost replay: chạy trái/phải, nhảy, facing, dash, basic attack, skill và interaction/hold switch.

Rules:

- Tối đa 3 Ghost; Ghost cũ nhất bị thay khi tạo Ghost thứ tư.
- Ghost không nhận input trực tiếp, không autonomous AI và không gây Game Over cho Player.
- Normal enemy/boss HP không hồi khi loop reset.
- Door, switch, bridge, trap và temporary mechanism reset khi chamber chưa hoàn thành.
- Temporal Echo Enemy là ngoại lệ authored; có thể reset HP/vị trí nếu puzzle cần.
- Stability Anchor khóa route đã giải, dừng loop tại chamber đó và lưu tiến trình.

Puzzle grammar bắt buộc:

1. Ghost giữ Pressure Plate, Player dùng relay/cross route.
2. Ghost kích hoạt lever đúng thời điểm, Player chạy/nhảy qua bridge hoặc door window.
3. Ghost distract/attack enemy trong khi Player làm objective khác.
4. Hai hoặc ba Ghost giữ seal A/B, Player kích hoạt seal C.

## 5. Resources, loot, checkpoint và upgrade

### Player resources

| Resource | Start | Purpose |
|---|---:|---|
| HP | 100 | Health của Player. |
| Energy | 100 | Dùng Skill 1/2. |
| Gold | 0 | Currency trong run. |
| Temporal Bean | 0, max 9 | Hồi 35% HP và 35% Energy thủ công. |
| Rune Key | 0 | Key objective, chỉ hiện HUD khi cần. |
| Ghost limit | 3 | Giới hạn Ghost trong Temporal Chamber. |

### Loot

- Normal enemy: 1–4 Gold, cơ hội thấp Health/Energy Shard.
- Heavy/elite: Gold nhiều hơn, recovery rõ hơn, cơ hội Bean.
- Health Shard: hồi 20–25 HP; Energy Shard: hồi 25–30 Energy.
- Chest: Gold, recovery hoặc Rune Key/relic authored.
- Weapon pickup luôn là: prompt → comparison panel → Equip/Cancel; không auto-equip.

### Upgrade

Sau boss, elite lớn hoặc reward room, Player chọn 1 trong 3 upgrade:

- Survival: Max HP, heal, Bean efficiency, Max Energy.
- Mobility: Move Speed, Dash cooldown, Dash distance.
- Combat: Basic Damage, Attack cooldown, Skill Damage, Skill cooldown.
- Temporal: Loop duration, Ghost damage, Ghost synergy.

Mỗi identical upgrade stack tối đa 3 lần. Gold không dùng để mua upgrade thường; Gold dành cho shop/recovery/reroll.

### Checkpoint và death

- Chrono Anchor ở đầu map, trước boss và sau cột mốc lớn.
- Death trả Player về Anchor, hồi HP/Energy về mức an toàn khoảng 70%.
- Retain weapon, upgrade trước checkpoint và map progress đã lưu.
- Lose Gold/Bean/Key nhặt sau checkpoint theo checkpoint state.
- Boss death reset boss fight về Phase 1; không checkpoint giữa phase.

## 6. Weapon system

Weapon system giữ architecture ScriptableObject hiện có: WeaponDefinition → SkillDefinition → execution. PlayerWeaponController không hardcode theo weapon.

### Unarmed opening

| Ability | Value |
|---|---|
| Basic | Punch Combo |
| Damage | 12 / 12 / 16 |
| Cooldown | 0.45 sec |
| Range | 0.85 side-view units |
| Skills | None |

Punch chỉ là opening ngắn trước khi Chrono Blade drop; không cần mở rộng combo asset vượt mức hiện tại.

### Chrono Blade — balanced melee

| Ability | Damage | Energy | Cooldown | Side-view role |
|---|---:|---:|---:|---|
| Temporal Slash Combo | 25 / 25 / 34 | 0 | 0.35s | Chuỗi chém ngang phía trước. |
| Chrono Burst | 40 | 20 | 5s | Sóng cyan/teal bay ngang 3.5–4 units. |
| Time Cleave | 60 | 35 | 7s | Heavy arc rộng phía trước. |

### Ember Staff — range/AoE

| Ability | Damage | Energy | Cooldown | Side-view role |
|---|---:|---:|---:|---|
| Fire Bolt | 18 | 0 | 0.45s | Projectile ngang, range 4.5. |
| Flame Nova | 35 | 20 | 6s | AoE quanh Player. |
| Meteor Rune | 55 | 35 | 9s | Rune phía trước, delay khoảng 0.5s rồi impact. |

### Void Bow — precision/range

| Ability | Damage | Energy | Cooldown | Side-view role |
|---|---:|---:|---:|---|
| Void Arrow | 20 | 0 | 0.50s | Projectile ngang, range 6.0. |
| Piercing Shot | 38 | 20 | 5s | Pierce tối đa 3 enemy. |
| Shadow Volley | 24/arrow | 35 | 8s | Fan ngang nhẹ; một enemy chỉ hit một lần/cast. |

Weapon equip đổi visual/full-body visual set, rest pose, basic attack, skill definitions, stats và HUD data. Không reset transform thủ công của WeaponSocket legacy nếu còn dùng.

## 7. Campaign — 4 authored maps

### Map 1 — Ruined Entrance

Role: tutorial combat/loot/platforming, chưa có Ghost/Time Loop.

Visual: abandoned underground fortress; đá xanh-xám, cầu gãy, void, cyan torch/rune, cột vỡ và sàn nhiều tầng.

Flow:

Awakening Chamber → Fallen Passage → Ruined Courtyard
→ Broken Armory → Rune Gate Hall → Gatebreaker Arena → Map 2 exit

Teach: chạy, jump, punch, loot, weapon pickup confirm, Chrono Blade, Dash, basic Rune Key, first upgrade.

NPC: Ruin Scout mở đầu/lối ra; atmospheric guide, không shop, không giải thích Ghost sớm.

Enemies:

- Ruin Husk — basic melee chaser, claw slash.
- Cracked Archer — projectile ngang có aim telegraph.
- Armory Warden — slow heavy slam, shockwave/hazard ground.

Boss: Gatebreaker Husk

- 420–500 HP target.
- Heavy Slam, horizontal Charge, Stone Shard projectile.
- Sau Charge miss/Slam xong có punish window.
- Không Ghost requirement.

Reward: Chrono Blade, Gold/recovery, first upgrade; exit mở sang Map 2.

### Map 2 — Overgrown Wilds

Role: chapter đầu tiên có Temporal Ghost; nhận Ember Staff; gộp combat pressure của Barracks cũ với thiên nhiên/time corruption của Wilds.

Visual: ruined military outpost bị rễ cây nuốt chửng, vine bridge, teal water, moss, cyan rune, purple corruption. Quartermaster camp nằm trong guard post đổ nát; Grove Keeper ở sanctuary.

Flow:

Wilds Outpost → Quartermaster Camp → Grove Keeper Sanctuary
→ First Temporal Grove → Sunken Relay Ruins → Ember Shrine
→ Thorn Matriarch Arena → Echo Prison Gate

Teach: shop, one Ghost, 20-second loop, pressure plate, delayed bridge, Ghost + combat, Ember Staff.

NPC:

- Quartermaster Echo: shop đầu tiên.
- Grove Keeper: giải thích Ghost là recording, không phải clone AI.

Enemies:

- Vine Husk — melee, light root snare.
- Thorn Spitter — thorn shot/seed mine projectile.
- Root Brute — heavy root slam, spine wall/hazard.

Temporal chambers:

- Echo Relay: Ghost giữ Plate A, Player activate Relay B rồi vượt gate.
- Delayed Root Bridge: Ghost kéo lever theo timing, Player chạy/nhảy qua bridge.
- Light combat puzzle: Ghost giữ plate/distract, Player xử lý Root Brute và route.

Boss: Thorn Matriarch

- 700–800 HP target.
- Thorn Line, Root Slam, Seed Volley.
- Phase 2 shield chỉ mở khi Ghost giữ Root Seal A và Player kích hoạt Seal B.
- Boss HP persist giữa loop; chỉ shield/seal/hazard reset.

Reward: Ember Staff, Gold, upgrade, route sang Map 3.

### Map 3 — Echo Prison

Role: mastery 2–3 Ghost, platform timing, mirror/rune puzzles, Void Bow.

Visual: Echo Library bị vỡ nối với prison mirror complex; indigo shelves, floating books, chain, iron bars, shattered mirrors, ash/orange light cũ và cyan/violet beams.

Flow:

Archive Foyer → Rotating Stacks → Mirror Cellblock
→ Prism Relay Vault → Prisoner Sanctuary → Void Bow Reliquary
→ Mirror Jailer Arena → Sanctum Causeway

Teach: two Ghost sequence, then three-Ghost seal, mirror-facing relay, combat distraction và Void Bow.

NPC: Archivist Prisoner. Player giải cứu NPC khỏi mirror cell; NPC cung cấp lore, Rune Key hoặc mở đường đến Void Bow. Không shop mới.

Enemies:

- Archive Wisp — glyph bolt và short blink.
- Living Tome — page dash và silence/zone circle.
- Mirror Wisp — mirror blink, reflected bolt.
- Cell Guard — chain slam, guard stance.

Temporal chambers:

- Rotating shelf: Ghost pull lever đúng timing, Player chạy/nhảy qua opening.
- Prism Relay: Ghost 1 giữ A, Ghost 2 giữ B, Player align/facing relay C.
- Combat distraction: Ghost kéo aggro Cell Guard trong khi Player mở rear route.

Boss: Mirror Jailer

- 1,050–1,250 HP target.
- Chain Lash, Mirror Bolt, Prison Slam, short clone pattern.
- Mirror Lock shield cần Ghost A/B và Player relay C để tạo damage window.

Reward: Void Bow, upgrade, route to Chrono Sanctum.

### Map 4 — Chrono Sanctum

Role: final preparation và finale; không dạy rule mới, chỉ payoff toàn bộ combat + 3 Ghost grammar.

Visual: ceremonial black stone route, Guardian statues, temporal gates, clock rune floor, cosmic void, bright cyan energy, violet paradox corruption và gold details.

Flow:

Warden Approach → Elite Processional → Paradox Gauntlet
→ Quartermaster Shrine → Chrono Vanguard Arena
→ Sanctum Reliquary → Chrono Guardian Arena → Victory Chamber

NPC: Quartermaster Echo xuất hiện lần cuối ở Shrine; sau khi rời Shrine Player không quay lại shop được.

Enemies: elite variants tái sử dụng Vine Husk, Thorn Spitter, Archive Wisp, Mirror Wisp và Cell Guard; thêm Temporal Echo Minion chỉ trong boss finale.

Elite rule: +40–60% HP, +20% damage, một pattern thêm, violet/gold aura và reward tốt hơn; không chỉ tăng HP.

Paradox Gauntlet: Ghost 1 channel A, Ghost 2 channel B, Player latch C và dùng mirror relay đúng facing để mở Central Gate.

Mini-boss: Chrono Vanguard

- 850–1,000 HP target.
- Temporal Dash Slash, Guarded Cleave, Rune Spear Throw, Echo Counter.
- Paradox Guard cần Ghost A/B + Player C để mở window.

Final boss: Chrono Guardian

- 1,200–1,600 HP target, tune sau full playtest.
- Phase 1: Chrono Cleave, Clockwork Bolt, Time Fracture, Rewind Charge, Guardian Slam.
- Phase 2 at khoảng 70% HP: Epoch Shield; Ghost giữ Past Seal A, Player latch Present Seal B để break shield.
- Phase 3 at khoảng 35% HP: Ghost 1/2 giữ A/B, Player activate C để break Paradox Shield; thêm Paradox Beam, Echo Disruption, Fractured Clock ring và Final Cleave.
- Loop expiry không hồi boss HP.

Victory: core shatters, echoes released, THE LOOP HAS BEEN BROKEN, run stats, Play Again/Main Menu.

### 7A. Authoritative side-view map redesign

This subsection replaces every earlier room/zone flow for Maps 1–4 above. Those earlier labels remain only as narrative references; they must not constrain side-view scene layout, collider placement, camera bounds, or traversal.

#### Shared side-view level rules

- A map is one authored horizontal journey, not a chain of top-down rooms. Each chapter may rise, fall, loop visually, and use short optional ledges, but its critical route must read left-to-right on a mobile screen.
- Build from **playable ground and walls first**, then background, midground, foreground, props, NPCs, combat and puzzle content. Collision is never inferred from art.
- Every chapter has 3–5 distinct traversal beats, not a mandatory fixed number of zones. A beat earns its place by teaching movement, changing combat pressure, or presenting a puzzle decision.
- Branches are short reward lookouts, recovery niches or lore pockets that always return to the primary route. No route may soft-lock a player after a missed jump.
- Camera bounds track the entire authored route; combat arenas may temporarily narrow movement only after their encounter starts.

#### Map 1 — Ruined Entrance: side-view layout

**Purpose:** introduce horizontal movement, jump, unarmed punch, loot, Chrono Blade pickup, Rune Key/gate, first upgrade and a conventional boss. No Time Loop, Ghost or temporal puzzle is active here.

| Beat | Approx. X range | Traversal and gameplay purpose |
|---|---:|---|
| Rampart Breach | 0–28 | Safe spawn beneath a broken gatehouse. Ruin Scout gives a short prompt; flat floor, one low step and an optional high alcove teach jump without danger. |
| Broken Aqueduct | 28–65 | Collapsed bridge route with two short gaps, a recovery floor, raised archer perch and first Ruin Husk. The player learns that vertical positioning matters. |
| Sunken Mustering Yard | 65–106 | Broad combat yard with a lower floor, two side ledges and a breakable-looking armory balcony. Two Husk + one Cracked Archer encounter, chest/recovery branch, then Chrono Blade drop. |
| Runic Liftworks | 106–146 | Compact vertical ruin: three ascending platforms, Armory Warden, Rune Key on a high ledge, then a clear Rune Gate at the descent. It is a platform/combat test, not a time puzzle. |
| Gatebreaker Keep | 146–192 | Wide flat arena with left/right safety margins, closed boss gate after entry, Gatebreaker Husk, upgrade reward pedestal and a visibly lit exit arch to Map 2. |

**Map 1 collider contract:** solid ground on all critical platforms; left/right world walls; low ceiling only inside the boss keep; recovery ground under the first teaching gaps; no collider on camera bounds, markers, torches, chest or gate visual. The Rune Gate becomes a separate enabled solid only until its key condition is satisfied.

#### Map 2 — Overgrown Wilds: side-view layout

**Purpose:** first Temporal Ghost chapter, first shop, Ember Staff, one-Ghost pressure/lever timing and a boss shield window.

| Beat | Traversal and gameplay purpose |
|---|---|
| Overrun Outpost | A damaged stone causeway overtaken by roots. Quartermaster camp is a safe horizontal hub after the first enemy patrol. |
| Canopy Ravine | Alternating root bridges and rocky ledges above teal water. Thorn Spitter controls raised perches; short reward branches return to the main floor. |
| Temporal Rootspan | First 20-second chamber. Ghost holds a root plate while Player crosses a delayed bridge; failure resets only the chamber. |
| Bloomworks | Wider mixed combat/puzzle approach: Ghost distracts a Root Brute or keeps a vine door open while Player takes the upper route to Ember Shrine. |
| Thorn Cathedral | Open circular root arena for Thorn Matriarch. Phase 2 uses Ghost Root Seal A plus Player Seal B; exit is the revealed prison causeway. |

#### Map 3 — Echo Prison: side-view layout

**Purpose:** two- and three-Ghost mastery, precision ranged combat with Void Bow, mirrors and moving prison architecture.

| Beat | Traversal and gameplay purpose |
|---|---|
| Cellblock Descent | Horizontal cell galleries with doors, chains and short drop shafts. Archivist Prisoner is rescued through a readable side branch. |
| Rotating Archive | Stacked library platforms rotate/open on timed levers. One Ghost opens the first passage; vertical camera offset is allowed here. |
| Prism Engine | Central multi-level relay room. Ghost 1 keeps seal A, Ghost 2 keeps seal B, Player aligns/faces relay C to open a permanent route. |
| Mirror Causeway | Narrow suspended route with mirror blink enemies and bow-focused encounters. Optional cells hold recovery/lore. |
| Jailer Vault | Large mirrored arena with clear floor, two raised seal balconies and a center mirror lock. Mirror Jailer becomes vulnerable only during the authored Ghost/relay window. |

#### Map 4 — Chrono Sanctum: side-view layout

**Purpose:** finale that combines all learned traversal, combat and three-Ghost language without introducing new rules.

| Beat | Traversal and gameplay purpose |
|---|---|
| Silent Processional | Monumental linear approach with elite patrols, clear checkpoints and a final Quartermaster shrine before the point of no return. |
| Paradox Bridge | A large multi-tier bridge over cosmic void. Ghosts channel seals A/B while Player completes latch C and a facing relay; failed loops reset only this authored chamber. |
| Vanguard Bastion | Compact combat keep with Chrono Vanguard mini-boss. Its Paradox Guard previews the final boss mechanic. |
| Sanctum Ascent | Calm pre-boss climb through clock statues and recovery alcoves; no new systems, only preparation and visual payoff. |
| Chrono Guardian Observatory | Broad, symmetric multi-level final arena: safe central floor, two seal balconies, visible clock ring hazards and space for Phase 1–3 telegraphs. Victory chamber opens to the right. |

## 8. NPC, shop và objective rules

Chỉ có 4 NPC identity chính:

| NPC | Role | Map appearance |
|---|---|---|
| Ruin Scout | Story guide, opening/exit hook; no shop. | 1, cameo 2/4 |
| Quartermaster Echo | Shop/recovery/reroll. | 2, 4 |
| Grove Keeper | Teaches loop/Ghost grammar. | 2 |
| Archivist Prisoner | Lore/key/route to Void Bow. | 3 |

Shop inventory:

| Item/service | Gold |
|---|---:|
| Temporal Bean | 25 |
| Health Shard | 12 |
| Energy Shard | 12 |
| Full HP or Energy restoration | 35 |
| Reroll next/unclaimed upgrade choice | 25 |

NPC interaction là portrait + 1–3 dialogue panels + objective update. Không có quest log phức tạp.

## 9. Enemy and boss production scope

Normal/special identity scope: 11, không phải 21 như bản cũ.

| Group | Identities |
|---|---|
| Map 1 | Ruin Husk, Cracked Archer, Armory Warden |
| Map 2 | Vine Husk, Thorn Spitter, Root Brute |
| Map 3 | Archive Wisp, Living Tome, Mirror Wisp, Cell Guard |
| Finale | Temporal Echo Minion; elite variants reuse existing identities |

Every enemy needs: Idle, Run/Walk, wind-up/telegraph, Attack, Hit, Death, shadow và HP feedback. Side-view enemy AI uses ground navigation/patrol/ledge safety; flying Wisp dùng airborne controller riêng.

Không có invisible continuous contact damage trong final build. Mọi damaging attack cần wind-up, readable hit area và recovery.

## 10. UI/UX

### Gameplay HUD

- Top-left: HP red bar + text, Energy cyan bar + text, Gold compact counter, current weapon name/icon.
- Key HUD hidden unless Rune Key objective active.
- Objective top-right, concise/contextual.
- Temporal HUD top-center chỉ trong active Temporal Chamber: time, loop count, Ghost count.
- Boss HUD top-center ưu tiên hơn Temporal/Objective text.
- Bottom-left joystick; bottom-right combat cluster.
- Bean hiển thị x0–x9, ở dưới/gần combat cluster.

### Panels

- Pause/System: Resume, Restart from Anchor, Main Menu, Settings.
- Settings: Music/SFX toggle/volume, vibration khi Android hỗ trợ, controls note.
- Stats panel: Player, Weapon, Skills, Temporal data; mở trực tiếp phải pause game nhưng không show PausePanel phía sau.
- Weapon pickup panel: centered screen-space UI, Equip/Cancel, không auto-equip.
- Upgrade panel: ba lựa chọn lớn, readable.
- Game Over và Victory flow là bắt buộc.

## 11. Side-view art và asset standards

### Existing asset preservation

- Không xóa asset 4-direction Player, weapon, enemy, UI, VFX, loot hoặc concept hiện có.
- Existing Right-facing row là side-view canonical direction ban đầu; dùng flipX cho trái khi silhouette/weapon handedness cho phép.
- Up/Down rows giữ làm archived source material, không dùng trong side-view gameplay.
- Top-down Master Map art giữ làm reference/archive; không stretch hoặc rotate thành side-view level.

### Player standard

- Shared canvas: 192×256 px, 128 PPU, bottom-center foot pivot và shared baseline.
- Scene root scale (1,1,1); không che inconsistency bằng scene scale/offset.
- Base Player states: Idle, Run, Jump, Fall, Landing, Punch, Dash, Hit, Death.
- Weapon states: Idle, Run, Jump/Fall hold pose, Basic Attack, Skill 1, Skill 2, Hit.
- Existing 4-direction sheets cấp Idle/Walk/Attack/Skill prototype; side-view art mới ưu tiên Jump/Fall/Landing và pose profile còn thiếu.

### Environment workflow

Far background/parallax
→ midground master environment
→ gameplay platforms, walls, one-way platforms
→ foreground occluders/props
→ manual Box/Polygon/Edge Collider2D
→ dynamic enemies, loot, NPCs, doors and puzzles

- Art không tự định nghĩa collision.
- Ground, wall, ceiling, slope, void dùng collider manual riêng.
- One-way platform optional, cần visual language rõ.
- Background không bake enemy, loot, NPC, boss hoặc UI.
- Foreground không che Player trong lúc jump/combat quan trọng.

### Palette

- Temporal/Ghost: cyan, teal, light purple.
- Corruption/paradox: violet/purple.
- Danger/fire: orange/red.
- Reward: gold.
- Dungeon base: dark navy/blue-grey stone, controlled contrast cho mobile.

## 12. Technical architecture rules

- Giữ reusable Health, damage, weapon/skill definition, loot, upgrade, TimeLoopManager, recorder, Ghost playback, UI data binding và checkpoint data khi tương thích.
- Thay movement top-down hai trục bằng side-view movement controller; không hack gravity vào code cũ.
- Player, Ghost và ground enemy dùng Rigidbody2D với intentional layer/collision matrix.
- Ghost playback phải record/replay jump và grounded-safe motion đủ deterministic cho authored chamber.
- Camera bounds không được block Player.
- Map collision thuộc hierarchy Map scene/level, không thuộc Camera/UI.
- Pool projectile/VFX/audio tần suất cao khi cần; không over-engineer static props.
- Runtime UI chỉ update values; không rebuild hierarchy/style/layout.

## 13. Asset production order

1. Pack 00 — Side-view technical migration: side camera, gravity, ground check, joystick flick jump, collision layers.
2. Pack 01 — Base Player side-view completion: verify right-facing frames; add Jump/Fall/Landing; punch/dash polish.
3. Pack 02 — Chrono Blade side-view integration: reuse canonical frames, add missing air/hold poses, tune VFX.
4. Pack 03 — Map 1 vertical slice: Ruined Entrance background/platform/collision, 3 enemies, Gatebreaker, Scout.
5. Pack 04 — Map 2 + Ember Staff + first Ghost: Wilds, Quartermaster, Grove Keeper, Temporal Chamber, Thorn Matriarch.
6. Pack 05 — Map 3 + Void Bow + 2–3 Ghost puzzles: Echo Prison, Archivist Prisoner, Mirror Jailer.
7. Pack 06 — Map 4 finale: elite variants, final shop, Vanguard, Chrono Guardian, Victory.
8. Pack 07 — polish: UI, SFX/music, VFX, performance, Android build và full-run test.

## 14. Testing gates

Mỗi milestone cần:

1. Compile không có error mới.
2. Console check trước/sau Play Mode.
3. Movement test bằng joystick và WASD/editor fallback.
4. Ground, wall, ledge, jump và dash collision regression.
5. Attack/skill/weapon regression.
6. Ghost playback regression trong Temporal Chamber liên quan.
7. UI touch/raycast check trên landscape aspect ratios.
8. Save scene/prefab và báo manual Android test còn thiếu.

Game chỉ complete khi fresh run đi được Main Menu → Map 1 → Map 4 → Victory, gồm Game Over/retry, 3 weapon, Temporal Chamber, boss phases và Android touch control.

## 15. Non-negotiable scope rules

- Không quay lại kế hoạch 8 map trước khi 4-map build ổn định/polished.
- Không thêm weapon thứ tư.
- Không procedural level, inventory grid, multiplayer, online leaderboard hoặc meta-progression trước full run.
- Không xóa source asset cũ chỉ vì chưa dùng trong side-view.
- Không dùng transform hack để che pivot/crop/scale/baseline sai.
- Giữ gameplay systems modular/data-driven; không hardcode weapon-specific branch vào PlayerWeaponController.
