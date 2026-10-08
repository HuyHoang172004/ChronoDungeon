# ChronoDungeon — Game Design Lock

Tài liệu này ghi các quyết định thiết kế đã được duyệt trong quá trình phát triển.  
Chỉ nội dung được người dùng xác nhận mới được thêm vào.

## 01. Core Identity & Gameplay Loop — Approved

### Game identity

**ChronoDungeon: Trapped Beyond Time** là game 2D top-down action roguelite cho mobile landscape. Người chơi chiến đấu, nhặt vũ khí và sử dụng Temporal Ghost từ vòng lặp trước để vượt qua puzzle và combat challenge.

Điểm khác biệt cốt lõi:

> Người chơi không chỉ mạnh hơn qua combat; họ phối hợp với chính hành động của mình trong quá khứ.

### Three experience layers

1. **Readable combat** — attack, dash, skill, loot và hồi phục phải vui, rõ và dễ điều khiển trên mobile ngay từ Map 1.
2. **Weapon/build choice** — mỗi weapon thay đổi lối đánh thực sự: kiếm cân bằng, trượng AoE, cung tầm xa.
3. **Time-loop depth** — Ghost được dạy dần, rồi trở thành công cụ bắt buộc cho puzzle và combat ở các map sau.

### Main gameplay loop

```text
Explore area
→ fight enemies / avoid hazards
→ collect gold, recovery, items and weapons
→ complete an objective or open a route
→ receive an upgrade or build choice
→ enter a harder encounter
→ defeat elite or boss
→ progress to the next map
```

### Temporal-area loop

```text
Perform actions
→ loop ends
→ Ghost replays the prior loop
→ current Player uses that Ghost support
→ solve puzzle, unlock route or complete combat objective
```

### Target pacing

- First-time map duration: approximately 10–18 minutes.
- Standard combat encounter: approximately 30–90 seconds.
- Ghost puzzle: approximately 1–4 minutes.
- Temporal loop duration: approximately 20 seconds.
- Eight-map full run target: approximately 90–150 minutes, with per-map checkpoints appropriate for a demo/mobile format.

### Locked scope decisions

1. **Action before time puzzle:** Map 1–2 focus on combat, loot and foundation; Map 3 introduces Ghost/time-loop gameplay.
2. The game has **three primary weapons only** until all three are complete.
3. The game uses **eight chapter-like maps**, each with its own visual identity and gameplay role.
4. The Player begins unarmed and obtains **Chrono Blade** by defeating an early Map 1 enemy.
5. Temporal Ghost is a required signature mechanic for the middle and late game, not decorative presentation.
6. Commercial-quality direction: low input complexity, clear visual readability, satisfying combat feedback, meaningful weapon/build choices and puzzles with understandable feedback.

---

## Pending Approval

## 02. Upgrade System — Approved

### Delivery model

- The Player chooses **one of three** upgrades after elite encounters, major treasure rewards or appropriate NPC rewards.
- Each upgrade must have an immediate and readable gameplay impact.
- Upgrades are not bought with Gold; Gold is reserved for shops, recovery items, chests and services.
- Each identical upgrade may stack at most three times.
- Run upgrades are lost when restarting a full run, but remain through checkpoints within the same run.

### Upgrade groups

| Group | Main stats |
|---|---|
| Survival | Max HP, healing, Temporal Bean efficiency, Energy capacity |
| Mobility | Move Speed, Dash Cooldown, Dash Distance, Dash invulnerability |
| Combat | Basic Damage, Attack Cooldown, Skill Damage, Skill Cooldown |
| Temporal | Loop Duration, Ghost Damage, Ghost duration and related synergy |

### Survival upgrades

- **Vital Core:** +20 Max HP and restore 20 HP.
- **Chrono Vitality:** +15% Max HP.
- **Emergency Reserve:** gain one Temporal Bean.
- **Efficient Bean:** Temporal Bean restores an additional 15% HP and Energy.
- **Energy Reservoir:** +25 Max Energy.
- **Temporal Recovery:** restore 25% HP when completing a combat room.

### Mobility upgrades

- **Swift Step:** +10% Move Speed.
- **Blink Circuit:** -15% Dash Cooldown.
- **Long Dash:** +15% Dash Distance.
- **Phase Dash:** +0.08 seconds Dash invulnerability.
- **Momentum:** the first Basic Attack after a Dash gains +20% damage.

### General combat upgrades

- **Sharpened Edge:** +15% Basic Attack Damage.
- **Rapid Rhythm:** -12% Attack Cooldown.
- **Arcane Force:** +15% Skill Damage.
- **Temporal Flow:** -12% Skill 1 and Skill 2 cooldown.
- **Executioner:** +20% damage against enemies below 30% HP.
- **Soul Siphon:** enemies gain a chance to drop a Health Shard.

### Weapon-specific upgrades

These only appear while the associated weapon is equipped.

**Chrono Blade**

- Temporal Slash width +15%.
- Chrono Burst range +20%.
- Time Cleave arc becomes wider.
- Every third attack gains bonus temporal damage.

**Ember Staff**

- Fire Bolt projectile speed and range increase.
- Flame Nova radius increases.
- Meteor Rune impact delay decreases.
- Fire Bolt applies a light burn effect.

**Void Bow**

- Arrow speed and range increase.
- Piercing Shot penetrates one additional enemy.
- Shadow Volley gains one additional arrow.
- Bonus critical chance or long-range damage.

### Temporal upgrades

Temporal upgrades begin to appear from Map 3 onward.

- **Extended Loop:** +3 seconds Loop Duration.
- **Echo Strength:** +20% Ghost Damage.
- **Persistent Echo:** Ghost duration improvement if required by the final implementation.
- **Temporal Recharge:** restore 15 Energy at the end of a loop.
- **Echo Resonance:** when a Ghost hits an enemy, the Player deals increased damage to that target briefly.

### Ghost limit decision

- Default and final initial scope: **maximum 3 Ghosts**.
- A fourth Ghost is not included in the initial build; it may only be considered after puzzle performance and balance are proven stable.

---

## 03. Player Progression, Economy & Checkpoints — Approved

### Base player resources

| Resource | Starting value | Role |
|---|---:|---|
| HP | 100 | Player health. |
| Energy | 100 | Resource for active weapon skills. |
| Gold | 0 | Run-only shop currency. |
| Temporal Bean | 0, maximum 9 | Active recovery consumable. |
| Rune Key | 0 | Objective/key item. |
| Ghost Limit | 3 | Maximum active Ghosts in Temporal Areas. |

### Energy rules

- Skill 1 costs approximately 20 Energy.
- Skill 2 costs approximately 35 Energy.
- Basic Attack and Dash are free in the initial scope.
- Energy recovery comes primarily from Energy Shards, Temporal Beans, NPC services and rewards. Passive recovery, if added, remains very slow and outside combat.

### Loot and reward sources

| Source | Reward target |
|---|---|
| Normal enemy | 1–4 Gold; low chance for HP/Energy shard. |
| Strong enemy | 4–8 Gold; chance for a Bean. |
| Elite | 15–25 Gold plus one upgrade choice. |
| Normal chest | 8–20 Gold or recovery loot. |
| Special chest | Rune Key, weapon or upgrade reward. |
| Boss | 40–60 Gold plus major upgrade/story reward. |

Weapon pickups never auto-equip; the Player confirms through the pickup UI.

### Recovery items

- **Health Shard:** restores 20–25 HP; crimson/red visual identity.
- **Energy Shard:** restores 25–30 Energy; cyan/blue visual identity.
- **Temporal Bean:** restores 35% HP and 35% Energy, is used manually, has a maximum carry count of 9 and costs approximately 20–30 Gold from shops.

### Gold and NPC shops

Gold is only retained during the current run. Proposed shop prices:

| Service | Gold cost |
|---|---:|
| Temporal Bean | 25 |
| Health Shard | 12 |
| Energy Shard | 12 |
| Full HP or Energy restoration | 35 |
| Minor random upgrade | 45–60 |
| Reroll three upgrade choices | 25 |
| Locked special chest | 20 or a Rune Key depending on chest type |

Planned shop/NPC locations: Map 2 Quartermaster, Map 3 Grove Keeper and Map 7 Chrono Merchant.

### Rune Key rules

- Rune Key is an objective item, not ordinary currency.
- It unlocks Rune Gates, relic chests or authored shortcuts.
- The Key HUD only appears when a map/objective needs it.
- Map-specific keys are consumed or reset after their authored objective.
- Map 1 teaches the basic key interaction; Ghost/key combinations begin from Map 3.

### Upgrade timing

The Player chooses one of three upgrades after elite encounters, bosses, major treasure rooms or special NPC rewards. Small combat rooms do not automatically grant upgrades. Target: 1–3 upgrade choices per map.

### Run definition

A **run** is one complete attempt from Map 1 until Victory against the Map 8 boss or a full reset. Within a run, the Player carries weapon, upgrades, Gold and Temporal Beans from map to map. A new run begins from the unarmed initial state with Gold and consumables reset.

### Chrono Anchor checkpoints and death

Chrono Anchors appear at the beginning of each map, before bosses and after large completion points where needed.

On death:

```text
Return to the nearest Chrono Anchor
→ restore HP/Energy to a safe amount (target: 70%)
→ reset unfinished enemies and puzzles in that checkpoint area
→ reset uncollected loot in that area
→ retain weapon and upgrades obtained before the checkpoint
→ lose Gold, Beans and Rune Keys obtained after the checkpoint
```

Boss deaths reset the boss encounter to the start of the fight; there are no mid-phase boss checkpoints.

The Game Over UI provides Retry from Anchor, Restart Current Map and Return to Main Menu.

### Map-to-map carryover

- Retained: equipped weapon, upgrades, Gold, Temporal Beans and Max HP/Energy improvements.
- Cleared or consumed as appropriate: map-specific Rune Keys and active Ghost/loop state.

---

## Pending Approval

## 04. Weapon & Combat Identity — Approved

### Shared combat rules

- Basic Attack and Dash do not consume Energy.
- Skill 1 costs 20 Energy; Skill 2 costs 35 Energy.
- Skills cannot cast with insufficient Energy or while their own cooldown is active.
- A cast hits an enemy at most once, except where a projectile/volley rule explicitly says otherwise.
- Player attacks never damage Player, Ghosts or non-enemy objects.
- All attacks resolve from the current facing direction.
- Combat visuals use four facing directions: Up, Down, Left and Right.
- Dash may cancel Basic Attack recovery but may not cancel the committed wind-up/strike of Skill 2.
- Initial final combat asset direction: weapon-bearing Player poses are full-body authored frames, not separately rotated weapons during attacks and skills.

### Unarmed opening state

| Stat | Value |
|---|---:|
| Basic attack | Punch Combo |
| Damage | 12 (third combo strike: 16) |
| Cooldown | 0.45 seconds |
| Range | 0.85 world units |
| Arc | 100 degrees |
| Skills | None |

Punch has a three-strike jab, hook and finishing straight-punch chain. The chain continues when the next input occurs within 0.55 seconds. The first Map 1 Ruin Husk has 45 HP and introduces the Chrono Blade weapon drop.

### Chrono Blade — balanced melee

**Role:** approachable all-round melee weapon with temporal mid-range pressure.

| Ability | Damage | Energy | Cooldown | Range/shape |
|---|---:|---:|---:|---|
| Temporal Slash Combo | 25 / 25 / 34 | 0 | 0.35 sec | 2.0 range, 120 degree arc |
| Chrono Burst (Skill 1) | 40 | 20 | 5 sec | 3.5–4.0 directional wave |
| Time Cleave (Skill 2) | 60 | 35 | 7 sec | 2.6 range, 130 degree heavy arc |

The three Basic Attack strikes are a fast diagonal slash, return slash and temporal finishing cleave. Chrono Burst is a broad cyan/teal pulse with a light purple accent. Time Cleave has a committed wind-up, a large cyan crescent strike and recovery.

### Ember Staff — ranged area control

**Role:** safe range, crowd control and area damage.

| Ability | Damage | Energy | Cooldown | Range/shape |
|---|---:|---:|---:|---|
| Fire Bolt | 18 | 0 | 0.45 sec | 4.5 straight projectile |
| Flame Nova (Skill 1) | 35 | 20 | 6 sec | 2.2 radius around Player |
| Meteor Rune (Skill 2) | 55 | 35 | 9 sec | 3.5–4.0 target range; 1.6 impact radius |

Fire Bolt disappears on the first enemy hit. Flame Nova hits each enemy once around the Player. Meteor Rune places a warning rune, then strikes after about 0.5 seconds. Its visual language is orange/red with ash and warm impact effects.

### Void Bow — precision and range

**Role:** long-range safety, directional precision and line/formation control.

| Ability | Damage | Energy | Cooldown | Range/shape |
|---|---:|---:|---:|---|
| Void Arrow | 20 | 0 | 0.50 sec | 6.0 straight projectile |
| Piercing Shot (Skill 1) | 38 | 20 | 5 sec | 7.0 projectile, pierces 3 enemies |
| Shadow Volley (Skill 2) | 24 per arrow | 35 | 8 sec | 5.5 three-arrow front fan |

Shadow Volley may strike each enemy at most once in a cast, so its intended strength is wide group control rather than stacking three arrows into a single target. Void visuals use indigo/purple with restrained cyan edge light.

### Skill hierarchy decision

Skill 2 is always the stronger, less frequent and more committed ability:

- Higher direct damage or higher total multi-target value than Skill 1.
- Longer cooldown.
- Higher Energy cost.
- Larger/more decisive VFX and stronger combat impact.

Skill 1 remains the frequent tactical tool; Skill 2 is the high-impact response for groups, heavy enemies or a decisive opening.

### Weapon pickup/equip behaviour

- Player holds one active weapon.
- Pickup flow is prompt → comparison panel → Equip or Cancel.
- Equip changes current weapon, Basic Attack, Skill 1, Skill 2, HUD/Stats data and hold pose.
- Pickup never auto-equips.
- Weapon inventory, weapon wheel and dropping an old weapon are out of the initial scope.

### Weapon asset completeness requirement

Every weapon requires authored 4-direction assets for Idle, Walk, Basic Attack, Skill 1, Skill 2 and Hit; plus pickup art, weapon icon, two skill icons and VFX. All sheets share the base Player canvas, PPU, baseline and bottom-centre pivot. Player scale is never adjusted in-scene to compensate for sprite inconsistency.

### Weapon-specific upgrade direction

- Chrono Blade: wider slash, longer Burst, wider Cleave, empowered third strike and Burst/Blade synergy.
- Ember Staff: faster/longer Fire Bolt, larger Nova, faster Meteor Rune and optional light burn.
- Void Bow: longer range, additional pierce, additional volley arrow and long-range/weakpoint bonus.

---

## Pending Approval

## 05. Temporal Ghost, Puzzles & Cooperation Combat — Approved

### Core Ghost rule

Temporal Ghost is a replay of a prior Player loop, not autonomous AI and not a second directly controlled character. The Player plans actions in one loop, then uses the replaying Ghost in a later loop.

```text
Loop 1: Player performs an authored action sequence.
Loop 2: Ghost replays that sequence while the current Player completes a complementary action.
```

### Ghost capabilities

Ghost can replay movement, facing, Basic Attack, skills, Dash, pressure switches, levers, runes and selected authored interactions. Ghost attacks deal approximately 40–50% of the Player's damage: enough to support combat, distract or satisfy temporal mechanics while preserving the current Player as the primary actor.

Ghost cannot receive fresh joystick input, make autonomous decisions, select its own targets, collect loot/weapons, manage inventory, kill the Player or replace the current Player entirely.

### Puzzle grammar and escalation

1. **Dual Switch:** a Ghost holds switch A while Player holds switch B.
2. **Delayed Mechanism:** Ghost repeats a lever action so Player can use a temporary bridge/gate at the correct moment.
3. **Sequential Rune:** Ghosts and Player activate a timed A → B → C interaction sequence.
4. **Hazard Cooperation:** Ghost holds a valve or mechanism to disable a trap while Player crosses the hazard.
5. **Split Path:** a prior loop travels one branch while the current Player completes another branch in the same window.
6. **Combat + Puzzle:** Ghost distracts or attacks a guard while Player completes an objective.

### Ghost-required enemy patterns

- **Temporal Shield Knight:** frontal shield absorbs damage; Ghost draws the guard's attention while Player attacks the rear weak point.
- **Echo Binder:** two runic cores must be attacked in a near-simultaneous time window by Ghost and Player.
- **Paradox Beast:** only takes meaningful damage when a past and present attack connect within a short timing window.
- **Mirror Warden:** Ghost triggers a mirror rune so the real target can be exposed to Player damage.

### Ghost-required boss patterns

- **Thorn Matriarch (Map 3):** Ghost and Player hold/destroy paired root nodes to create boss openings.
- **Ash Warden (Map 5):** Ghost holds one cooling valve while Player handles the other; both active states remove boss protection.
- **Mirror Jailer (Map 6):** Ghost and Player activate opposite mirror anchors within a time window to break the shield.
- **Chrono Guardian (Map 8):** Phase 2 requires Ghost Rune A plus Player Rune B to break a temporal shield. Phase 3 combines multiple Ghost actions, temporal zones and a current Player objective to open decisive damage windows.

### Difficulty curve

| Map | Ghost challenge |
|---|---|
| Map 1 | No Ghost gameplay. |
| Map 2 | No Ghost gameplay; advanced combat/dash. |
| Map 3 | One Ghost; dual switch and simple timing. |
| Map 4 | One–two Ghosts; lever sequence and timed doors. |
| Map 5 | Ghost plus traps and combat pressure. |
| Map 6 | Two–three Ghosts; split path, mirror and simultaneous interactions. |
| Map 7 | Full-mechanic combat/puzzle gauntlet. |
| Map 8 | Ghost is mandatory for boss vulnerability windows. |

### Fairness requirements

- Puzzle rules may be challenging but are never hidden.
- Interactable objects use distinct colour/icon language.
- Ghost paths/actions are readable through timeline markers or temporal footprints.
- Loops reset quickly and clearly.
- A puzzle adds no more than one major new rule at a time.
- Bosses clearly signal shield states, damage windows and temporal objectives.
- Contextual hints may appear after repeated failure, but do not immediately solve the puzzle for the Player.

---

## Pending Approval

## 06. Hybrid Map Art Production — Approved

Each map uses a hybrid art workflow for high visual quality and practical manual level design:

```text
Large authored Base Map Art
+ separate Foreground Overlay
+ manually authored collision
+ separate props/interactives/gameplay objects
```

### Required map structure

```text
MapXX_Name
├── BackgroundArt
│   └── BaseMapArt
├── ForegroundArt
│   └── FrontWallsAndOccluders
├── Props
├── Enemies
├── Loot
├── NPC
├── Collision
├── Gameplay
└── CameraBounds
```

### Base Map Art rules

- The large Base Map Art contains walkable floor, rear walls, voids and static background decoration.
- It supports a continuous scrolling world; it is not limited to one camera screen.
- It never contains baked enemies, NPCs, Gold, recovery pickups, weapons, UI or other dynamic gameplay objects.
- Dynamic gates, chests, boss objects and traps remain separate assets so they can change state during play.

### Foreground Overlay rules

- Foreground contains front wall edges, near pillars, foliage and occluders that must render above Player.
- Base art renders below Player; foreground renders above Player as required by sorting.
- This prevents the visual problem of Player walking over walls while retaining a painted-map look.

### Collision rules

- Base art never defines movement collision.
- Manual PolygonCollider2D and/or BoxCollider2D shapes are authored along actual walls and void boundaries.
- A Collision Guide Image may be supplied for Editor-only reference, marking walkable surfaces, walls, voids, boss arena and trigger zones. It is hidden during play.

### Per-map asset package

Every map receives: Base Map Art, Foreground Overlay, Collision Guide, Props Sheet, Interactive Object Sheet, relevant Enemy Sheet, and appropriate loot/VFX assets.

### Reuse across maps

The same structure applies to all eight maps. Shared utility assets can be reused, while each map retains a distinctive base scene, prop set, interaction set and visual identity.

### Master Full-Map decision

- Each map primarily uses **one large authored Master Full Map image**, exactly like the current Map 1 `FullLayoutArt` workflow.
- Player traverses this continuous map while the camera follows; it is not a tile-painted map and does not teleport between screens.
- Manual PolygonCollider2D/BoxCollider2D shapes define walls, void boundaries and walkability.
- Optional Foreground Overlay and individual dynamic props/interactives remain separate only where sorting or gameplay state requires them.
- Tilemaps are optional repair/detail tools only, never a requirement for hand-painting the primary map.
- If a Master image exceeds practical Android texture memory, it may be split internally into seamless visual chunks. This is an invisible technical fallback, not a design change: the map remains one continuous painted world and requires no tile workflow from the level designer.

---

## Pending Approval

## 07. Map 1 — Ruined Entrance — Approved

### Role and pacing

Map 1 is the opening combat/loot chapter. It teaches movement, unarmed Punch Combo, loot, confirmed weapon pickup, Chrono Blade combat, Dash, a basic Rune Key interaction and the first upgrade choice. It has no Temporal Ghost or Time Loop gameplay.

- First-time target duration: 12–18 minutes.
- Replay target duration: 6–10 minutes.
- Includes one mini-boss, one simple Rune Key objective and one first upgrade reward.

### Visual identity

- Abandoned underground fortress: blue-grey broken stone, cracked paving, moss and black voids.
- Cyan rune light and cold cyan torches provide readable points of interest.
- Broken statues, columns and ancient clock/rune motifs foreshadow time corruption without activating loop gameplay.
- Production uses the approved Hybrid Map Art workflow: continuous Base Map Art, Foreground Overlay, manual collision and separate gameplay objects.

### Continuous world layout

```text
Awakening Chamber
→ Fallen Passage
→ Ruined Courtyard
→ Broken Armory
→ Rune Gate Hall
→ Gatebreaker Arena
→ Exit Corridor to Map 2
```

The map is a continuous scrolling world with smooth Player-follow camera, not a series of teleports.

### Area flow

#### 1. Awakening Chamber

- Player spawns at the first Chrono Anchor, unarmed.
- Ruin Scout gives a short atmospheric introduction.
- Initial objective: **Find a way through the ruins**.
- No immediate combat or large tutorial banner.

#### 2. Fallen Passage

- First static Ruin Husk has 45 HP and teaches the Punch Combo.
- Its authored reward includes Chrono Blade, Gold and recovery pickups.
- Weapon pickup flow is NEW WEAPON panel → Equip or Cancel; never automatic.
- After equipping, the objective becomes **Test the Chrono Blade**.
- Two small Ruin Husk encounters let the Player test Basic Attack and skills.

#### 3. Ruined Courtyard

- First real combat arena, using broken columns and debris for readable space.
- Encounter progression: two Ruin Husk → Ruin Husk plus Cracked Archer → two Husk plus Archer.
- Teaches movement under pressure, Dash against projectiles, Chrono Burst at range and Time Cleave in close groups.
- Rewards: Gold, recovery loot and one small chest; the first Temporal Bean may be placed here intentionally.

#### 4. Broken Armory

- Military ruins with damaged armour racks, banners and weapons.
- Introduces the slow Heavy Warden with a large readable telegraphed attack.
- Heavy Warden guards the first Rune Key.
- Objective: **Find the Rune Key** → defeat Warden → collect key.
- Optional small relic chest/recovery reward is allowed.

#### 5. Rune Gate Hall

- Teaches the non-temporal key grammar: Rune Key → matching Rune Gate → interact → key consumed → gate opens.
- Key/gate colour language is cyan/gold and reflected by the temporary Key HUD.
- New objective: **Reach the gatekeeper**.
- A Chrono Anchor is placed before the mini-boss arena.

#### 6. Gatebreaker Arena

**Mini-boss: Gatebreaker Husk**

- Target health during initial balance: 300–400.
- Heavy Slam with orange ground telegraph.
- Short Charge toward Player.
- Three slow Stone Shard projectiles.
- After a missed Charge or completed Slam, the boss has a short punish window.
- Counterplay is readable Dash timing, basic attacks and committing Skill 2 during openings.
- No Ghost requirement in this first boss.

Boss reward: 20–30 Gold, one choice from three first upgrades, and the Exit Corridor opens.

First upgrade choices are intentionally simple:

- Vital Core: +20 Max HP.
- Sharpened Edge: +15% Basic Attack Damage.
- Swift Step: +10% Move Speed.

#### 7. Exit Corridor

- Low-combat transition corridor which visually introduces the military architecture of Map 2.
- Ruin Scout/voice line gives a concise forward hook.
- Final objective: **Enter the Forgotten Barracks**.

### Map 1 enemy scope

| Enemy | Role |
|---|---|
| Ruin Husk | Basic melee chaser. |
| Cracked Archer | Introductory ranged projectile enemy. |
| Heavy Warden | Slow heavy enemy with clear large telegraph. |
| Gatebreaker Husk | Map 1 mini-boss. |

### NPC scope

**Ruin Scout** is an atmospheric guide at the opening and exit. The NPC does not sell items and does not teach Time Loop early. Visual direction: spectral explorer silhouette, subdued cyan light and lantern/rune tablet; clearly distinct from Player Ghosts.

### Map 1 asset package

- Base Map Art, Foreground Overlay and Editor-only Collision Guide.
- Stone floor/wall/void presentation, ruins, broken pillars, statues, Rune Monolith, cyan torch and armory props.
- Chrono Anchor, Rune Gate, chest and Rune Key assets.
- Four-direction asset sets for Ruin Husk, Cracked Archer and Heavy Warden: Idle, Walk, Attack, Hit and Death.
- Gatebreaker Husk boss animation/VFX set.
- Loot visuals: Gold, Health Shard, Energy Shard, Temporal Bean and Rune Key.
- Enemy hit/death, chest opening, gate opening, Anchor activation and boss telegraph VFX.

---

## Pending Approval

## 08. Map 2 — Forgotten Barracks — Approved

### Role and pacing

Map 2 is a combat-first chapter that teaches Dash, target priority, risk/reward path selection and the first Gold shop. It deliberately has no Ghost or Time Loop gameplay so Player combat literacy is established before Map 3.

- First-time target duration: 12–18 minutes.
- Includes the first shop NPC, optional side-route reward, one mini-boss and one upgrade reward.

### Visual identity

- Abandoned military barracks: cold grey stone, rusted steel, torn banners, broken armour and weak warm torches.
- Training grounds, dormitory halls, armory and command room establish the former defensive line before the Sanctum.

### Continuous world layout

```text
Broken Gate
→ Training Yard
→ Dormitory Wing
→ Quartermaster Armory
→ Command Hall
→ Barracks Captain Arena
→ Sealed Wilds Passage
```

### Area flow

#### 1. Broken Gate

- Arrival from Map 1 with objective: **Reach the command hall**.
- Legion Husk encounter establishes that Player is now stronger with Chrono Blade.

#### 2. Training Yard

- Combat progression: two Legion Husk plus Rust Archer → Heavy Warden plus two Rust Archer → three Legion Husk plus Heavy Warden.
- Teaches Dash against projectiles, target priority, Chrono Burst at range and Time Cleave against close groups.
- Rewards include Gold, recovery loot and a small chest.

#### 3. Dormitory Wing

- Narrow barracks corridor with beds, damaged armour and military props.
- Main route is safer with less reward; optional Armory Cache route has extra combat and greater Gold/Bean reward.
- Optional Rune Key may unlock a relic chest or shortcut but is not mandatory for map completion.

#### 4. Quartermaster Armory

**Quartermaster Echo** is the first shop NPC. The shop is small and readable:

| Item/service | Price |
|---|---:|
| Temporal Bean | 25 Gold |
| Health Shard | 12 Gold |
| Energy Shard | 12 Gold |
| Full HP or Energy restoration | 35 Gold |
| Reroll next upgrade choice | 25 Gold |

#### 5. Command Hall

- Command table, tactical maps and commander statues create a pre-boss atmosphere.
- Final guard encounter and a Chrono Anchor precede the arena.
- Objective: **Defeat the Barracks Captain**.

#### 6. Barracks Captain Arena

**Mini-boss: Barracks Captain**

- Shield Bash with front telegraph.
- Command Charge with a readable ground path.
- Guard Stance reducing frontal damage; Player circles or waits for an opening.
- Rally Call summons two Legion Husk at controlled intervals.
- The encounter tests Player Dash timing, positioning and use of Chrono Blade skills.
- No Ghost requirement.

Reward: 25–35 Gold, one one-of-three upgrade choice and the passage to Map 3.

#### 7. Sealed Wilds Passage

- Vegetation and unstable cyan runes create a transition into Overgrown Wilds.
- The visual/lore hook foreshadows that memories and time no longer remain still.

### Enemy scope

| Enemy | Role |
|---|---|
| Legion Husk | Faster armored melee chaser. |
| Rust Archer | Introductory projectile enemy. |
| Heavy Warden | Slow heavy melee with large telegraph. |
| Barracks Captain | Shield/charge/minion mini-boss. |

### Asset package

- Barracks Base Map Art, Foreground Overlay and Collision Guide.
- Broken gate, banners, armour, weapons, training props, dormitory props, Quartermaster counter, command table and arena emblem.
- Quartermaster Echo idle asset and themed shop UI.
- Four-direction sets for Legion Husk, Rust Archer and Heavy Warden; boss set for Barracks Captain.
- Projectile, shield impact, charge telegraph and death VFX.
- Standard loot, optional Rune Key/relic chest, Chrono Anchor and exit gate assets.

---

## Pending Approval

## 09. Temporal Loop Resolution & Persistence — Approved

### Temporal Chamber scope

Loops only operate inside authored **Temporal Chambers**, not continuously across an entire map. Normal map areas do not reset. Each chamber has a Temporal Start Anchor and an authored 20-second loop.

```text
Enter Temporal Chamber
→ loop begins
→ complete the chamber and reach a Stability Anchor
→ chamber locks completed state and loop stops
→ continue to the next area
```

### On an incomplete loop reset

When the timer expires before chamber completion:

- Player returns to that chamber's Temporal Start Anchor.
- Doors, switches, levers, temporary bridges and traps return to their chamber-start state.
- A Ghost is created/replayed from the just-completed loop.
- Player begins the next loop from the start with the Ghost performing its recorded route.

### Persistent combat progress

| Entity/state | Behaviour on loop reset |
|---|---|
| Normal enemy | HP does not restore; a dead enemy remains dead within that chamber. Position/AI may reset only if still alive. |
| Boss / mini-boss | HP does not restore during the boss encounter. |
| Door / switch / lever / trap | Resets unless the chamber has been completed. |
| Player | Returns to Temporal Start Anchor. |
| Ghost | Replays the prior loop. |

This avoids invalidating Player combat progress. Ghost attacks can provide additional support in later loops, while Player remains the primary damage source.

### Temporal Echo Enemy exception

Later maps may use a clearly marked purple/cyan **Temporal Echo Enemy** whose HP/position does reset with the loop. This is an authored puzzle/combat exception, not the default enemy rule.

### Stability Anchor

When Player crosses a solved route and touches a **Stability Anchor** before the timer ends:

- The Temporal Chamber is permanently completed.
- The loop stops for that chamber.
- The solved route/door remains open.
- Player's progression is secured and the next checkpoint/area becomes active.

### Boss-loop rule

Boss HP is persistent. Temporal reset only restores encounter mechanics such as shields, runes, hazards and temporary states. Ghost cooperation opens damage windows; it never erases Player damage already dealt to the boss.

---

## 10. Map 3 — Overgrown Wilds — Approved

### Role and pacing

Map 3 is the first Time Loop and Ghost chapter. It teaches planning an action in one loop, then using its Ghost replay in the next loop. It also offers Ember Staff as the second weapon.

- First-time target duration: 15–22 minutes.
- One active Ghost maximum.
- Two authored temporal puzzle patterns, one light combat/puzzle chamber, a weapon shrine and one Ghost-required mini-boss.

### Visual identity

- Ancient ruins reclaimed by moss, roots, vines and shallow teal water.
- Dark stone and natural greens are contrasted by cyan temporal runes and restrained purple corruption.
- A living but dangerous atmosphere, distinct from the cold military stone of Map 2.

### Continuous world layout

```text
Wilds Threshold
→ Grove Keeper Sanctuary
→ First Temporal Grove
→ Sunken Relay Ruins
→ Ember Shrine
→ Thorn Matriarch Arena
→ Echo Library Passage
```

### Area flow

#### 1. Wilds Threshold

- Arrival from Map 2; first Vine Husk and Thorn Spitter encounters.
- Objective: **Find the source of the temporal roots**.
- Rune and root visuals foreshadow temporal activity, but no loop begins yet.

#### 2. Grove Keeper Sanctuary

- **Grove Keeper** explains Ghost as a record of Player actions, not a second independent character.
- Offers a small recovery shop as appropriate.
- Objective becomes **Awaken the Temporal Grove**.
- Chrono Anchor placed before the first chamber.

#### 3. First Temporal Grove

First authored chamber, using a 20-second loop and one Ghost.

**Puzzle A — Echo Relay**

- Rune Plate A must be held continuously.
- Rune Relay B is a button which latches active for six seconds.
- Gate opens while Ghost holds A and Player activates B.
- Player crosses the gate and reaches Stability Anchor before timer expiry.

Example solution:

```text
Loop 1: Player reaches and holds Plate A.
Loop 2: Ghost repeats/holds A; Player reaches Relay B, activates it,
then crosses the six-second gate window to Stability Anchor.
```

**Puzzle B — Delayed Root Bridge**

- Lever activation has a clear delay; Root Bridge grows after about three seconds and remains for about five seconds.
- In the recording loop, Player operates the lever.
- In the next loop Ghost repeats the lever timing while Player waits/runs to cross the bridge and touch Stability Anchor.

The intended solution timing has generous margin: route-to-mechanic is approximately 4–6 seconds and mechanism-to-Stability Anchor is approximately 4–8 seconds.

#### 4. Sunken Relay Ruins

- First light Ghost-plus-combat room.
- Ghost holds Rune Plate A while current Player deals with Root Brute and activates the complementary route.
- Normal enemy HP/death persists between loops, avoiding repeated grind.
- Rewards: Gold, Energy Shard and optional relic chest/reward.

#### 5. Ember Shrine

- Clear weapon pedestal holding Ember Staff, with warm fire/teal shrine lighting.
- Uses the standard confirmation pickup panel; Player may Equip or keep Chrono Blade.
- Shows Ember Staff metadata: Damage 18, Range 4.5, Flame Nova and Meteor Rune.
- Short safe test encounter permits Fire Bolt and Flame Nova use.

#### 6. Thorn Matriarch Arena

**Mini-boss: Thorn Matriarch**

- Visual: corrupted ancient root body, dark green vines, cyan temporal core and restrained red/purple thorns.
- Phase 1 attacks: Thorn Line, Root Slam and Seed Volley with clear telegraphs.
- Phase 2 creates a vine shield and paired Root Seals A/B.

Ghost-required shield break:

```text
Loop 1: Player records holding/activating Root Seal A.
Loop 2: Ghost performs Seal A while Player activates Seal B.
→ Both seals open a temporary shield-break window.
→ Player attacks the exposed temporal core.
```

- Boss HP persists through loops; only shield/rune/hazard states reset.
- Ghost opens the vulnerability condition but does not defeat the boss independently.

Reward: 30–40 Gold, one one-of-three upgrade choice, Grove Keeper lore update and passage to Map 4.

#### 7. Echo Library Passage

- Roots break into architectural library glyphs/books to transition visually toward Map 4.
- Objective: **Follow the echoes to the library**.

### Enemy scope

| Enemy | Role |
|---|---|
| Vine Husk | Readable slow melee chaser. |
| Thorn Spitter | Thorn projectile attacker. |
| Root Brute | Heavy slam/AoE enemy. |
| Thorn Matriarch | Ghost-required mini-boss. |

### Asset package

- Overgrown Wilds Base Map Art, Foreground roots/vines overlay and Collision Guide.
- Rune Plates, latching Relay B, temporal lever, Root Bridge, Rune Gate, Ember Shrine and Stability Anchor.
- Grove Keeper sprite/idle/talk pose and optional small shop panel.
- Four-direction Vine Husk, Thorn Spitter and Root Brute sets; Thorn Matriarch boss set.
- Temporal footprint/path markers, loop begin/end, active plate, bridge growth, root-seal links, boss shield/vulnerability VFX.

---

## Pending Approval

## 11. Map 4 — Echo Library — Approved

### Role and pacing

Map 4 expands Ghost gameplay from one Ghost to a maximum of two Ghosts. It teaches timing mechanisms, Ghost distraction in combat and multi-loop sequence planning.

- First-time target duration: 18–25 minutes.
- No new weapon; rewards are Gold, upgrades and story/lore.
- Includes Archivist Echo, two authored temporal puzzle patterns, one combat/puzzle room and Silent Curator mini-boss.

### Visual identity

- Vast ancient library with rotating shelves, broken stairs, floating books, dust and magical glyphs.
- Dark navy/indigo environment, cyan echoes, violet glyphs and restrained gold relic accents.

### Continuous world layout

```text
Archive Foyer
→ Rotating Stacks
→ Hall of Echoes
→ Tri-Index Vault
→ Astral Index Chamber
→ Silent Curator Arena
→ Ash Forge Descent
```

### Area flow

#### 1. Archive Foyer

- **Archivist Echo** gives objective: **Recover the Astral Index**.
- Establishes the library's theme: books store recorded actions, not merely text.
- Early Archive Wisp combat occurs before the temporal rooms.

#### 2. Rotating Stacks

**Delayed Shelf Rotation puzzle**

- A cyan lever rotates a large shelf after approximately two seconds, opening a route for six seconds.
- Loop 1 records Player reaching and activating the lever around the fifth second.
- Loop 2 Ghost repeats the timing while Player reaches the route, crosses it and touches Stability Anchor.
- The mechanism requires timing but has deliberate non-frame-perfect margin.

#### 3. Hall of Echoes

**Combat distraction puzzle**

- Archive Wisp, Living Tome and a Library Sentinel guard an Echo Plate/Rune Gate.
- Loop 1 records Player traversing the plate and attacking Sentinel.
- Loop 2 Ghost repeats this interaction, drawing Sentinel attention while current Player circles to activate the rear Rune Gate.
- Ghost supports positioning, rather than independently defeating the guard.

#### 4. Tri-Index Vault

**Two-Ghost sequential puzzle**

- Index A and Index B are continuous pressure seals.
- Index C is a six-second latching relay.
- Loop 1: Player holds A, creating Ghost 1.
- Loop 2: Ghost 1 holds A while Player holds B, creating Ghost 2.
- Loop 3: Ghost 1 holds A, Ghost 2 holds B, Player activates C, crosses the gate and reaches Stability Anchor.
- Beam, colour and countdown feedback make all puzzle rules visible.

#### 5. Astral Index Chamber

- Player retrieves the Astral Index and receives Gold, an upgrade choice and lore: Chrono Guardian imprisons memories of resistance.
- Chrono Anchor precedes the boss.
- Objective: **Defeat the Silent Curator**.

#### 6. Silent Curator Arena

**Mini-boss: The Silent Curator**

- Phase 1 attacks: Page Volley, Glyph Burst, Silence Beam and controlled Living Tome summons.
- Phase 2: Archive Lock shield requires Ghost-recorded Cyan Memory Index followed by current Player Purple Memory Index. This opens an approximately six-second stagger/damage window.
- Phase 3: combines previously introduced coloured glyph order with one Ghost support path; no wholly new rule is added.
- Boss HP remains persistent through loops; only shield/rune/hazard states reset.

Reward: 35–45 Gold, one upgrade choice, Astral Index lore and access to Map 5.

#### 7. Ash Forge Descent

- Burned glyphs, chains, ash and orange light transition the library toward Ash Forge.
- Objective: **Descend into the Ash Forge**.

### Enemy scope

| Enemy | Role |
|---|---|
| Archive Wisp | Light projectile mage. |
| Living Tome | Fast charge/zone enemy. |
| Library Sentinel | Heavy guard suitable for distraction mechanics. |
| The Silent Curator | Glyph/book/shield mini-boss. |

### Asset package

- Echo Library Base Map Art, shelf/pillar foreground overlay and Collision Guide.
- Rotating shelf, Echo Plate, Index Seals A/B/C, Vault Gate, Astral Index pedestal and glyph floor set.
- Archivist Echo idle/talk assets and concise lore presentation.
- Asset sets for Archive Wisp, Living Tome, Library Sentinel and Silent Curator.
- Shelf rotation, path markers, Index beams/countdown, Archive Lock shield, book projectile, glyph telegraph and boss stagger VFX.

---

## Pending Approval

## 12. Map 5 — Ash Forge — Approved

### Role and pacing

Map 5 is the first high-pressure Ghost chapter: combat, hazards and temporal machinery operate together. It has no primary NPC, retaining an isolated, dangerous forge atmosphere.

- First-time target duration: 18–25 minutes.
- Maximum two Ghosts.
- No new weapon.
- Includes hazard traversal, a two-Ghost mechanism puzzle, combat/hazard cooperation and Ash Warden mini-boss.

### Visual identity

- Underground forge with blackened stone, molten channels, iron chains, hammers, ash and hot pipes.
- Orange/red is the clear danger language; cyan marks temporal cooling machinery.

### Continuous world layout

```text
Cooling Tunnels
→ Conveyor Crucible
→ Chain Foundry
→ Molten Core
→ Ash Warden Arena
→ Mirror Prison Lift
```

### Area flow

#### 1. Cooling Tunnels

- Objective: **Restore the forge coolant flow**.
- Flame Jets use a readable pre-fire ground telegraph and cause non-one-shot damage.
- Ash Husk encounter introduces fighting while timing hazards; the first area does not begin a loop immediately.

#### 2. Conveyor Crucible

**Cooling Conveyor puzzle**

- Cooling Valve A must be held continuously, slowing the hot conveyor and disabling a Flame Jet window.
- Loop 1 records Player holding Valve A.
- Loop 2 Ghost holds A while current Player crosses conveyor, times remaining hazards and reaches Stability Anchor.
- Ghost creates a safe route but does not complete traversal for Player.

#### 3. Chain Foundry

**Two-Ghost machinery puzzle**

- Chain Brake A and B must be held to stop a giant hammer.
- Relay C latches a safe floor for seven seconds.
- Loop 1: Player holds A to create Ghost 1.
- Loop 2: Ghost 1 holds A; Player holds B to create Ghost 2.
- Loop 3: both Ghosts hold brakes; Player activates C, crosses and touches Stability Anchor.
- The first solution presentation avoids heavy combat, preserving readable puzzle learning.

#### 4. Molten Core

- Combat/hazard arena with Forge Knight, Ember Caster and magma geometry.
- Forge Knight's Heat Armor reduces frontal damage.
- Ghost activation/holding of Cooling Rune temporarily disables Heat Armor, allowing Player to attack the weak point.
- Player can still complete the combat without Ghost, but Ghost is tactically superior rather than a hard lock.

#### 5. Ash Warden Arena

**Mini-boss: Ash Warden**

- Phase 1: Hammer Slam, Molten Line, Chain Pull and Ember Burst.
- Phase 2: powerful Heat Shield requires Cooling Valve A and B simultaneously to expose core for roughly seven seconds.
- Loop 1 records Player holding Valve A.
- Loop 2 Ghost holds A while current Player activates B, then attacks exposed core.
- Phase 3 increases hazard/combat pace but adds no new Ghost rule.
- Boss HP persists through loops; shields, valves and hazards reset.

Reward: 40–50 Gold, one upgrade choice and access to Mirror Prison Lift.

#### 6. Mirror Prison Lift

- Forge heat transitions into cold violet/cyan prison illumination and chain ambience.
- Objective: **Find the imprisoned echoes**.

### Enemy scope

| Enemy | Role |
|---|---|
| Ash Husk | Fire-tinted melee chaser. |
| Ember Caster | Fire projectile/zone enemy. |
| Forge Knight | Heavy enemy with Heat Armor. |
| Ash Warden | Cooling-valve/hazard mini-boss. |

### Asset package

- Ash Forge Base Map Art, chain/pipe foreground overlay and Collision Guide.
- Lava channels, Flame Jets, conveyors, Cooling Valves, Chain Brakes, hammer machinery, Cooling Runes, forge arena and prison lift.
- Ash Husk, Ember Caster, Forge Knight and Ash Warden asset sets.
- Flame telegraph, molten line, lava, cyan cooling, Heat Armor, hammer impact, chain-pull, core exposure and ash/death VFX.

---

## Pending Approval

## 13. Map 6 — Shattered Mirror Prison — Approved

### Role and pacing

Map 6 opens the full initial Ghost limit of three. It tests multi-loop planning, split paths, recorded facing/channel interactions and attack timing cooperation. It grants Void Bow, the third and final initial weapon.

- First-time target duration: 20–28 minutes.
- Maximum three Ghosts.
- Includes Prisoner Echo, three-Ghost puzzle, Echo Binder two-core combat, Void Bow and Mirror Jailer mini-boss.

### Visual identity

- Ancient fractured prison: dark iron, cold silver, chains, cells, shattered mirrors and cracked temporal crystals.
- Cyan, indigo and violet reflection light dominate; gold is reserved for seals/relic accents.

### Continuous world layout

```text
Prison Gate
→ Reflection Cells
→ Fractured Gallery
→ Triple Lock Chamber
→ Void Bow Reliquary
→ Mirror Jailer Arena
→ Warden Approach Bridge
```

### Area flow

#### 1. Prison Gate

- Arrival from Map 5 through prison lift.
- Objective: **Free the imprisoned echo**.
- Introduces Mirror Wisp and Prisoner Echo behind a temporal seal.

#### 2. Reflection Cells

**Recorded-facing Mirror Relay puzzle**

- Mirror Relay A must be channelled continuously from the correct facing direction.
- Mirror Relay B must be activated from the opposite route/direction.
- Loop 1 records Player channeling A.
- Loop 2 Ghost channels A while Player activates B, opens Prism Gate and reaches Stability Anchor.
- Mirror beams clearly show whether facing/channel state is valid.

#### 3. Fractured Gallery

**Three-route split-path puzzle**

- Left A and Right B are continuous seals; Center C is a seven-second latching relay.
- Loop 1: Player holds A, making Ghost 1.
- Loop 2: Ghost 1 holds A while Player holds B, making Ghost 2.
- Loop 3: both Ghosts hold A/B; Player activates C, crosses crystal bridge and reaches Stability Anchor.
- Route composition is visible but uses authored mirror obstruction for spatial challenge without hiding rules.

#### 4. Triple Lock Chamber

- Three Prison Lock Nodes must be simultaneously held to free Prisoner Echo.
- Each route uses an **Echo Binder**, an enemy with cyan and purple cores that must be struck within approximately two seconds.
- Ghost replays an attack/core interaction while Player hits the companion core, breaks protection and progresses node setup.
- This is the first explicit attack-timing cooperation puzzle.

**Prisoner Echo** provides lore: Chrono Guardian imprisoned Echoes to control the dungeon, and directs Player to Void Bow Reliquary.

#### 5. Void Bow Reliquary

- Clear purple/cyan relic room with standard confirmation pickup flow.
- Void Bow metadata: Damage 20, Range 6.0, Piercing Shot and Shadow Volley.
- Safe target/ranged-enemy encounter lets Player test the weapon.

#### 6. Mirror Jailer Arena

**Mini-boss: Mirror Jailer**

- Phase 1: Chain Lash, Prison Slam, Mirror Bolt and Cell Lock.
- Phase 2: Triple Mirror Lock shield uses continuous Anchor A/B plus seven-second latching Anchor C.
- Loop 1 records A; Loop 2 Ghost 1 holds A while Player records B; Loop 3 Ghost 1/2 hold A/B while Player activates C, breaking shield for damage window.
- Phase 3: Shattered Reflections creates distinguishable clone pressure; Ghost paths draw clone attention, giving Player a real-boss opening. No wholly new temporal rule is introduced.
- Boss HP persists; temporary anchors/shield/hazards reset with loops.

Reward: 45–55 Gold, one upgrade choice, Void Bow fallback if not collected and access to Map 7.

#### 7. Warden Approach Bridge

- Cold mirror/prison visual transitions to solemn ritual architecture.
- Objective: **Reach the Warden Approach**.

### Enemy scope

| Enemy | Role |
|---|---|
| Mirror Wisp | Short teleport/light projectile. |
| Prism Archer | Reflection/line projectile attacker. |
| Cell Guard | Heavy melee guard. |
| Echo Binder | Two-core timing enemy. |
| Mirror Jailer | Triple-anchor/clone mini-boss. |

### Asset package

- Mirror Prison Base Map Art, foreground iron bars/chains/mirror shards and Collision Guide.
- Prison cells, Mirror Relays, Prism Gate, Mirror Seals, crystal bridge, Prison Lock Nodes, Void Bow Reliquary, boss arena and approach bridge.
- Prisoner Echo chained/freed assets and prison-seal effects.
- Mirror Wisp, Prism Archer, Cell Guard, Echo Binder and Mirror Jailer asset sets.
- Mirror beams, channel/facing indicators, bridge formation, two-core timing, triple-anchor, chain/projectile/clone and shield-break VFX.

---

## Pending Approval

## 14. Map 7 — Warden Approach — Approved

### Role and pacing

Map 7 is the final-preparation/mastery chapter. It introduces no new Ghost rule; it tests the Player's full learned combat, routing, timing and three-Ghost toolkit before Chrono Sanctum.

- First-time target duration: 16–22 minutes.
- Maximum three Ghosts.
- No new weapon.
- Includes elite gauntlet, final Paradox puzzle, Chrono Merchant final shop and Chrono Vanguard mini-boss.

### Visual identity

- Solemn ceremonial path to the dungeon heart: black stone, Guardian statues, ritual gates and broad clean architecture.
- Cyan temporal runes, violet corruption and ancient gold accents make it distinct from Mirror Prison.

### Continuous world layout

```text
Approach Gate
→ Guardian Processional
→ Paradox Gauntlet
→ Chrono Merchant Shrine
→ Chrono Vanguard Arena
→ Sanctum Threshold
```

### Area flow

#### 1. Approach Gate

- Objective: **Reach the Chrono Sanctum**.
- Short elite Legion Husk/Prism Archer encounter establishes final-zone combat pressure without immediately starting a loop.

#### 2. Guardian Processional

- Four paced encounter waves: elite Legion Husk plus Prism Archer → Forge Knight plus Archive Wisp → Cell Guard plus Echo Binder → mixed elite wave.
- Breaks between waves allow loot collection and readable pacing.
- Rewards: Gold, recovery loot, Bean chance and a relic chest.

#### 3. Paradox Gauntlet

**Final authored mastery puzzle**

- Past Lane A and Echo Lane B are continuous channels; Present Lane C is a latching relay.
- Loop 1 records Player channeling A to make Ghost 1.
- Loop 2 Ghost 1 channels A while Player records B to make Ghost 2.
- Loop 3 Ghost 1/2 hold A/B while Player activates C, opens Central Gate, uses Mirror Relay with correct facing and reaches Stability Anchor.
- Ghosts may distract an Echo Binder guard, combining all established rules without adding a new one.

Reward: final pre-boss upgrade choice, Gold and relic chest.

#### 4. Chrono Merchant Shrine

**Chrono Merchant / Keeper** is the last shop. It sells no new weapon.

| Item/service | Price |
|---|---:|
| Temporal Bean | 25 Gold |
| Health Shard | 12 Gold |
| Energy Shard | 12 Gold |
| Full HP + Energy restore | 35 Gold |
| Reroll an unchosen upgrade reward | 25 Gold |

Player cannot return after leaving the shrine. A Chrono Anchor follows it.

#### 5. Chrono Vanguard Arena

**Mini-boss: Chrono Vanguard**

- Phase 1: Temporal Dash Slash, Guarded Cleave, Rune Spear Throw and anti-front-spam Echo Counter.
- Phase 2: Paradox Guard shield requires Ghost 1 Rune A, Ghost 2 Rune B and Player latching Rune C to create an approximately six-second damage window.
- Phase 3: faster patterns, shorter shield window and controlled Echo Binder pressure; no new temporal mechanic.
- Boss HP persists through loops.

Reward: 50–60 Gold, final upgrade choice and Chrono Sanctum Gate opens.

#### 6. Sanctum Threshold

- Quiet visual transition after the mini-boss, with final Chrono Anchor.
- Objective: **Defeat the Chrono Guardian**.
- Explicit Enter Sanctum interaction prevents an abrupt scene transition.

### Enemy scope

Map 7 uses elite variants rather than a large new enemy roster: Legion Husk, Prism Archer, Forge Knight, Archive Wisp, Cell Guard and Echo Binder. Elite treatment includes modest HP/damage/speed increase, one clear additional pattern and violet/gold visual treatment; it is not only inflated HP.

### Asset package

- Warden Approach Base Map Art, foreground statues/ritual columns and Collision Guide.
- Approach Gate, rune lanes, Central Gate, Chrono Merchant Shrine, Chrono Anchor, Vanguard arena and Sanctum Threshold.
- Chrono Merchant assets and final shop panel.
- Elite palette/aura/health-bar treatment for reused enemies.
- Chrono Vanguard full boss asset set, temporal spear/blade projectile, Guard shield, dash/counter telegraph and final-gate VFX.

---

## Pending Approval

## 15. Map 8 — Chrono Sanctum — Approved

### Role and pacing

Map 8 is the campaign finale. It introduces no wholly new mechanic; it pays off combat, weapon choice, resource management and the complete three-Ghost toolkit through Chrono Guardian.

- Preparation section target: 3–5 minutes.
- Boss target: 10–15 minutes on a first attempt.
- Total Map 8 target: 15–20 minutes.
- Maximum three Ghosts.
- No new weapon or shop.
- Final Chrono Anchor precedes the boss encounter.

### Visual identity

- Central time sanctum: a vast clean arena with clock-like rune floor, floating pillars, ancient mechanisms and sparse readable decoration.
- Dark cosmic stone, bright cyan temporal energy, violet paradox corruption and ancient gold detailing.

### Continuous world layout

```text
Sanctum Entrance
→ Hall of Still Moments
→ Chrono Reliquary
→ Chrono Guardian Arena
→ Victory Chamber
```

### Preparation flow

#### 1. Sanctum Entrance

- Objective: **Confront the Chrono Guardian**.
- No unnecessary combat gauntlet.
- A fixed Sanctum Restoration restores a portion of HP and Energy, preventing an unwinnable entry without replacing Map 7's shop preparation.

#### 2. Hall of Still Moments

- Three optional short murals/memory tablets explain Guardian's fall and the Ghosts' imprisoned memories.
- Final Chrono Anchor is placed before the Reliquary.

#### 3. Chrono Reliquary

- Compact confirmation panel shows equipped weapon, Beans, HP/Energy and upgrade summary.
- Explicit **Enter the Sanctum** interaction starts the boss; Player cannot return to Map 7 shop afterward.

### Chrono Guardian rules

- Initial balance target: 1,200–1,600 HP, tuned after complete combat/build playtests.
- Loop expiry never restores boss HP; it resets shields, runes, temporary hazards and temporal mechanics.
- If Player dies, the entire boss encounter resets to Phase 1; no mid-phase checkpoints.

### Phase 1 — The Guardian Awakens

Combat-only phase testing movement, Dash and weapon mastery.

| Attack | Read/counter |
|---|---|
| Chrono Cleave | Large frontal cone; move/dash sideways or behind. |
| Clockwork Bolt | Clear slow temporal projectile. |
| Time Fracture | Ground rune telegraph before detonation. |
| Rewind Charge | Charge leaves temporary dangerous trail. |
| Guardian Slam | Large ground shockwave. |

At approximately 70% HP, Guardian activates Epoch Shield.

### Phase 2 — Break the Epoch Shield

- Boss is nearly immune while shielded.
- Past Seal A is continuous; Present Seal B is a seven-second latching relay.
- Loop 1 records Player channeling Past Seal A, creating Ghost 1.
- Loop 2 Ghost 1 channels A while current Player activates B, breaking shield for an approximately seven-second damage window.
- Boss applies controlled pressure and may summon 1–2 light Temporal Echo minions, without obscuring the objective.

At approximately 35% HP, arena transitions into Paradox Convergence.

### Phase 3 — Paradox Convergence

- Past Seal A and Echo Seal B are continuous; Present Seal C is a latching relay.
- Loop 1 records A → Ghost 1.
- Loop 2 Ghost 1 holds A while Player records B → Ghost 2.
- Loop 3 Ghost 1/2 hold A/B while Player activates C, breaking Paradox Shield and exposing Guardian.
- New pressure is only intensified familiar language: Paradox Beam, telegraphed Echo Disruption zone, Fractured Clock ring and Final Cleave.
- Echo Disruption cannot unfairly invalidate a correctly planned route; it has clear warning and allows retry in a later loop.

### Victory Chamber

On Guardian defeat: core shatters, arena stabilises, imprisoned echoes are released and **THE LOOP HAS BEEN BROKEN** appears.

Victory UI shows weapon used, Gold collected, upgrades chosen, Ghosts used, completion time and death count. It offers **Play Again** and **Main Menu**. Online leaderboard/meta progression is outside the initial scope.

### Asset package

- Chrono Sanctum Base Map Art, floating-pillar/rune foreground overlay and Collision Guide.
- Sanctum gate, murals, Sanctum Restoration, Reliquary, rune dial floor, Past/Echo/Present Seals and Victory Chamber.
- Chrono Guardian full set: idle, movement, cleave, charge, slam, cast, shield, stagger, phase transition and death.
- Temporal Echo minion and themed boss HUD/shield icon.
- Rune telegraph, bolt, rewind trail, shields, seal beams, shield break, Paradox Beam, disruption zone, Fractured Clock, core shatter and victory release VFX.
- Victory and run-statistics UI assets.

---

## 16. Asset Bible & Production Standard — Approved

### Art direction

- 2D top-down dark-fantasy pixel art with a light 3/4 perspective.
- Crisp readable silhouettes for mobile landscape; no inconsistent character identity, random scene-scale fixes or cropped body parts.
- Cyan/teal represents temporal systems, violet represents paradox/corruption, orange/red represents danger/fire and gold represents reward.

### Player canonical standard

| Field | Approved standard |
|---|---|
| Master canvas | 192 × 256 px |
| World PPU | 128 |
| Pivot | bottom-center at shared foot baseline |
| Foot baseline | X=96, Y=24 from canvas bottom |
| Scene transform scale | (1, 1, 1) |
| Rotation | (0, 0, 0) |

Player identity remains fixed: dark/navy hair, dark navy/black coat, restrained cyan temporal trim, brown-dark belt, dark boots and a readable cloak silhouette. Every direction and action preserves proportions, hair, costume palette and foot baseline.

### Player animation target

| Animation | Frames per direction |
|---|---:|
| Idle | 4–6 |
| Walk | 6–8 |
| Punch strike 1/2 | 5–6 |
| Punch strike 3 | 6–8 |
| Hit | 3–4 |
| Death | 6–8 |
| Dash | 4–6 plus separate after-image VFX |

Idle is breathing/cloak motion without sliding feet; Walk has readable foot movement. No animation changes Player body scale or baseline.

### Weapon-bearing Player standard

- Chrono Blade, Ember Staff and Void Bow use the same Player canvas, PPU, pivot and baseline.
- Each weapon requires Idle, Walk, Basic Attack, Skill 1, Skill 2 and Hit in four directions, plus pickup art, weapon icon, two skill icons and VFX.
- Chrono Blade has three distinct Basic Attack strikes.
- Weapon combat frames are authored full-body Player-plus-weapon frames. Weapons are not separately rotated/positioned to fake combat poses.
- Target frame count: Idle 4–6, Walk 6–8, Basic strike 5–6, Skill 1 6–8, Skill 2 8–10 and Hit 3–4 per direction.

### Enemy and NPC standard

| Type | Master canvas |
|---|---|
| Normal small/ranged enemy | 192 × 256 px |
| Heavy enemy | 256 × 320 px |
| Mini-boss | 384 × 448 px |
| Chrono Guardian | 512 × 640 px |

- Enemy world PPU is 128 with bottom-center pivot and gameplay collider separate from visual art.
- Enemy minimum animation: Idle, Walk, Attack, Hit and Death. Bosses require clear wind-up/telegraph, attack, stagger and death states.
- NPCs use Player-equivalent scale and require Idle, Talk pose, optional gesture, shadow and optional UI portrait. They do not need combat sheets unless they fight.

### Environment and map standard

- Each map begins from one large Master Full Map image, then uses optional Foreground Overlay, dynamic props/interactives and manual collision.
- The Master map is a continuous camera-follow world; it is not a mandatory Tilemap.
- Base map art never bakes enemies, NPCs, loot, weapons, UI, dynamic gates, chests, bosses or active traps.
- Foreground Overlay is used only for objects that should render in front of Player, such as front wall edges, near pillars or foliage.
- Manual collider shapes define actual movement; the painted image never defines collision automatically.
- An Editor-only Collision Guide may mark walkable ground, walls, voids, arena and trigger boundaries.

### Prop and interactive source-size targets

| Asset | Source target |
|---|---:|
| Gold / shard / Bean / Rune Key | 96 × 96 px |
| Weapon pickup | 128 × 128 or 192 × 192 px |
| Chest / torch / small prop | 128 × 128 px |
| Lever | 128 × 192 px |
| Rune plate / switch | 192 × 192 px |
| Gate / Anchor / Mirror relay | 192–256 × 256–320 px |
| Large statue | 256 × 384 px |

Pickups need base art, optional separate glow/halo, optional shadow, subtle idle bob, pickup burst and HUD icon. Dynamic glow is not baked into a sprite if gameplay must change it.

### VFX and palette standard

| Use | Palette |
|---|---|
| Temporal / Ghost | cyan + teal + light purple |
| Chrono Blade | cyan core + purple accent |
| Ember Staff | orange/red + ash |
| Void Bow | indigo/violet + cyan edge |
| Danger | orange/red |
| Gold | gold |
| HP recovery | crimson/red |
| Energy recovery | cyan/blue |

Shared palette anchors: Void `#0A1018`, dark stone `#172636`, stone highlight `#607886`, moss `#75A864`, cyan `#17D9E8`, teal `#24BFB7`, purple `#8E5CFF`, orange `#FF7336`, red `#E43D50`, gold `#F6C84A`, light text `#F2E7CD`.

VFX targets: hit spark 96×96, slash 192×192, projectile 128×128, skill burst 256×256 and boss telegraph/impact 384–512×512. Effects must be readable without obscuring Player, enemy telegraphs or mobile controls.

### UI icon standard

- Weapon/skill/upgrade icons: 128 × 128 px.
- Item icons: 96 × 96 px.
- NPC/boss portraits: 256 × 256 px.
- Icons use strong silhouette, transparent background and no text baked into the image. Pixel UI frames remain separate.

### Mandatory production workflow

```text
Lock bible
→ make concept/contact sheet
→ approve silhouette, scale, palette and canvas
→ create complete sprite sheet
→ validate pivot/baseline/crop
→ preview animation
→ approve asset pack
→ integrate into Unity
→ test sorting, collider and gameplay
```

Asset-source corrections happen in the source art. Scene transforms are not used to compensate for inconsistent size, pose or cropping.

### Production order

```text
A. Base Player + Punch + Dash
B. Chrono Blade complete pack
C. Ember Staff complete pack
D. Void Bow complete pack
E. Common loot, item and UI icon pack
F. Map 1 environment and enemy pack
G. Map 2 environment and enemy pack
…
N. Chrono Guardian and Victory pack
```

Each asset pack is completed and approved before deep integration into its corresponding map.

---

## Pending Approval

## 17. Enemy Bible & Combat Balance — Approved

### Enemy count and production model

The designed campaign contains **21 normal/special enemy identities**, **6 elite variants** in Map 7 and **8 boss/mini-boss encounters**. Elite variants are not wholly new enemies: they reuse enemy foundations while adding a pattern, modest stat growth and gold/violet visual treatment.

Implementation reuses a small set of core movement/attack/telegraph controllers, but each listed identity receives distinct art, animation, VFX and readable combat pattern.

### Shared enemy rules

- No invisible continuous contact damage as the final combat model; attacks use readable wind-up/telegraph and recovery.
- Normal hit reaction target: 0.08–0.12 seconds; heavy enemies resist stagger; bosses only stagger in explicit windows.
- Normal enemies and bosses do not restore HP on loop expiry, except authored Temporal Echo Enemy challenges.
- All enemy packs require Idle, Walk, Wind-up/telegraph, Attack, Hit, Death, shadow and HP feedback.

### Stat-scale targets

| Stage | Normal HP | Normal damage | Heavy HP | Heavy damage |
|---|---:|---:|---:|---:|
| Map 1 | 45–60 | 8–10 | 120–150 | 16–18 |
| Map 2 | 60–80 | 10–12 | 150–190 | 18–20 |
| Map 3 | 75–95 | 12–14 | 180–220 | 20–22 |
| Map 4 | 90–110 | 14–16 | 220–260 | 22–24 |
| Map 5 | 110–130 | 16–18 | 260–310 | 24–26 |
| Map 6 | 125–150 | 18–20 | 300–350 | 26–28 |
| Map 7 elite | 150–190 | 20–24 | 360–430 | 28–32 |

Final numbers are tuned only after complete weapon/upgrade playtests.

### Core archetypes

- **Melee Chaser:** speed 2.2–2.6, range 1.0–1.2, 0.25–0.35 sec wind-up, 1.2–1.6 sec cooldown.
- **Ranged Attacker:** speed 1.5–1.9, preferred range 3.5–5.0, 0.40–0.55 sec aim cue, 1.6–2.2 sec projectile cooldown.
- **Heavy Guard:** speed 1.1–1.5, range 1.4–1.8, 0.65–0.90 sec wind-up, 2.0–2.8 sec cooldown.
- **Special/Puzzle Enemy:** limited authored use for Ghost/timing conditions.

### Normal and special roster

| # | Enemy | Map | Main pattern |
|---:|---|---:|---|
| 1 | Ruin Husk | 1 | Claw Slash melee. |
| 2 | Cracked Archer | 1 | Straight arrow. |
| 3 | Armory Warden | 1 | Heavy Slam. |
| 4 | Legion Husk | 2 | Slash and short Charge. |
| 5 | Rust Archer | 2 | Arrow Volley. |
| 6 | Heavy Warden | 2 | Heavy Cleave and small Ground Quake. |
| 7 | Vine Husk | 3 | Root Bite and light snare. |
| 8 | Thorn Spitter | 3 | Thorn Shot and Seed Mine. |
| 9 | Root Brute | 3 | Root Slam and Spine Wall. |
| 10 | Living Tome | 4 | Page Dash and Silence Circle. |
| 11 | Archive Wisp | 4 | Glyph Bolt and short Blink. |
| 12 | Library Sentinel | 4 | Archive Slam and Guard stance. |
| 13 | Temporal Shield Knight | 4–5 | Frontal shield; Ghost distracts while Player attacks rear weak point. |
| 14 | Ash Husk | 5 | Ember Swipe and short fire zone. |
| 15 | Ember Caster | 5 | Fire Orb and Flame Pool. |
| 16 | Forge Knight | 5 | Forge Slam and Heat Armor. |
| 17 | Mirror Wisp | 6 | Mirror Blink and Mirror Bolt. |
| 18 | Prism Archer | 6 | Prism/reflecting projectile. |
| 19 | Cell Guard | 6 | Chain Slam and Guard stance. |
| 20 | Echo Binder | 6–7 | Cyan/purple cores struck within a two-second Ghost/Player window. |
| 21 | Temporal Echo Minion | 8 | Rewind Blink and Echo Bolt; authored reset exception. |

### Elite rules

Elite variants gain approximately 40–60% HP, 20% damage, slight speed growth, one clear additional pattern, violet/gold aura/marker and improved Gold/recovery reward. They are never merely inflated-HP enemies.

### Boss roster

| # | Boss | Map | HP target | Primary pattern |
|---:|---|---:|---:|---|
| 1 | Gatebreaker Husk | 1 | 420–500 | Slam, Charge, Stone Shard. |
| 2 | Barracks Captain | 2 | 550–650 | Shield, Charge, minion command. |
| 3 | Thorn Matriarch | 3 | 700–800 | Root attacks and Twin Root Seal. |
| 4 | Silent Curator | 4 | 850–950 | Book/glyph attacks and Archive Lock. |
| 5 | Ash Warden | 5 | 1,000–1,150 | Forge hazards and Cooling Valve shield. |
| 6 | Mirror Jailer | 6 | 1,150–1,300 | Chains, mirrors, clones and Triple Lock. |
| 7 | Chrono Vanguard | 7 | 1,300–1,450 | Dash/guard mastery and Paradox Guard. |
| 8 | Chrono Guardian | 8 | 1,200–1,600 | Three-phase temporal finale. |

Boss damage target: light projectile 10–14, basic melee 14–20, heavy slam/charge 22–30, major telegraphed hit 30–40 and hazard tick 5–8. Player should survive roughly 3–5 significant mistakes depending on build and recovery.

### Loot targets

| Enemy type | Gold | Recovery chance |
|---|---:|---|
| Chaser | 1–3 | Low shard / very low Bean. |
| Ranged | 2–4 | Low shard / very low Bean. |
| Heavy | 4–8 | Medium shard / low Bean. |
| Elite | 15–25 | Guaranteed recovery / medium Bean chance. |
| Boss | 25–60 | Authored fixed reward. |

---

## Pending Approval

## 18. UI/UX Bible — Approved

### Technical rule

GameScene UI is authored in-scene and is the layout/visual source of truth. Runtime code updates data, icon, cooldown and visibility only; it never rebuilds buttons, hierarchy, sprites, anchors or manual layout.

### HUD layout

**Top-left Player HUD** is always ordered HP → Energy → Gold → Current Weapon. HP uses crimson fill and text `HP current / max`; Energy uses cyan/blue fill and `ENERGY current / max`; Gold is a compact coin row with no long bar; Weapon shows optional icon and name, or `UNARMED`.

Rune Key HUD is hidden by default and appears only for an active Key objective.

**Top-center context priority:** Boss HUD has highest priority; Temporal HUD (`TIME`, `LOOP`, `GHOST x/3`) appears only in Temporal Chambers when no Boss HUD is active; normal exploration needs neither.

**Top-right:** compact two-line Objective Panel. It updates with a small fade/slide and hides/compacts when boss presentation needs space.

### System/Settings button

- A clear gear **Settings button** is placed in the top-right Safe Area beneath/beside Objective HUD, without obstructing gameplay.
- Tapping it pauses gameplay and opens a unified **System Menu**; no accidental pause panel/UI overlap is permitted.
- System Menu options: Resume, Stats, Settings, Controls, Restart Current Map and Main Menu.
- Opening Stats from System Menu keeps gameplay paused; closing Stats returns to System Menu. Opening Stats directly in gameplay pauses only Stats and closing it resumes play.

### Settings sub-panel

The Settings sub-panel is reachable from System Menu and provides:

```text
MUSIC       on/off and volume
SFX         on/off and volume
MASTER      optional global volume
VIBRATION   on/off (when Android support is added)
CONTROLS    short mobile control reference
[BACK]
```

Settings are intended to persist locally when the save/settings system is implemented. Controls reference the joystick, Attack, Dash, Skill 1, Skill 2, Bean and interaction prompt; mobile rebinding is outside initial scope.

### Mobile controls

- Virtual Joystick anchors bottom-left in Chrono Safe Area: Base approximately 130×130 and Handle approximately 60×60, semi-transparent dark-blue/grey with cyan handle highlight.
- Combat cluster anchors bottom-right: Attack is largest at 96×96, Dash 72×72, Skill 1/Skill 2 76×76 and Bean approximately 80×80 on a lower separated row.
- Cluster composition is Skill 1 above, Dash left of Attack, Skill 2 lower/left of Attack and Bean lower/left as a distinct recovery button.
- Attack has orange/red accent. Dash uses cyan/blue. Skills use weapon-aware cyan/teal/purple or weapon-specific art.
- Editor WASD is testing support only; mobile joystick is primary input.

### Button and cooldown rules

- Attack/Dash labels hide when a readable icon exists.
- All skill buttons contain Icon, Radial 360 filled dark overlay and cooldown text.
- Overlay is 50–60% black/grey and uses button-compatible shape; Frame/Icon source sprites are not replaced by code.
- Ready state hides overlay/text; insufficient Energy dims skill state and prevents cast.
- Bean button displays a large icon and readable `xN` count, dims when unusable and uses a matching cooldown overlay.

### Interaction and modal panels

- World interaction prompts are Screen Space bottom-center, approximately 240×55, never use a pickup world position and hide when Player exits range.
- Weapon Pickup Panel anchors center, approximately 420×280: icon/name, damage, cooldown, range, Skill 1/2, Equip and Cancel.
- Shop, Weapon Pickup, Upgrade, Dialogue, Stats, System Menu, Game Over and Victory are centered modal panels that block/dim combat input beneath them.
- Upgrade panel presents exactly three large touch-friendly cards; choice is mandatory for mandatory reward events.
- Dialogue uses compact portrait/name/text and optional Trade/Leave choices, never long visual-novel blocks.

### Shop, Stats, Pause and end-state flows

- Shop shows Player Gold and approved consumable/service prices; unaffordable rows dim but retain readable price.
- Stats shows Player, Weapon, Skills and Temporal sections with aligned data columns and a visible top-right X button.
- The old independent Pause visual is replaced by System Menu flow to avoid Pause/Stats overlap.
- Game Over offers Retry from Anchor, Restart Current Map and Main Menu.
- Victory shows weapon used, Gold collected, upgrades chosen, Ghosts used, completion time and death count; actions are Play Again and Main Menu.

### Responsive/accessibility rules

- Canvas reference is 1920×1080 landscape with Scale With Screen Size and Chrono Safe Area.
- Corner-anchored HUD/control layout must remain valid on 16:9 and wide Android aspect ratios.
- State communication cannot rely only on colour: use icon, shape, text or animation too.
- High-contrast readable text, limited line length and no large tutorial banner in combat are required.

### UI priority

```text
Modal panel / System Menu
→ Boss HUD
→ Temporal HUD
→ Objective HUD
→ Player HUD and controls
```

---

## Pending Approval

## 19. NPC, Shop, Dialogue & Objective Bible — Approved

### Shared rules

- NPC dialogue is concise: maximum 2–3 lines, skippable and never repeated as a long tutorial.
- Puzzle hint appears only after approximately 2–3 loop failures and states a rule rather than giving a full solution.
- Shops do not sell weapons or direct upgrades in initial scope; weapons come from authored rewards/pedestals and upgrades come from reward choices.

### NPC roster

| NPC | Map | Role |
|---|---:|---|
| Ruin Scout | 1 | Opening story, Chrono Blade guidance, exit hook; no shop. |
| Quartermaster Echo | 2 | First recovery/reroll shop. |
| Grove Keeper | 3 | Time Loop/Ghost explanation, recovery shop and contextual hint. |
| Archivist Echo | 4 | Astral Index lore and sequence-puzzle hint; no shop. |
| Prisoner Echo | 6 | Echo lore and Void Bow direction; no shop. |
| Chrono Merchant/Keeper | 7 | Final recovery/reroll shop and final warning. |

Map 5 deliberately has no primary NPC, preserving isolated hazard/combat pacing. Map 8 has no shop/NPC dialogue; memory echoes and mural storytelling provide a quiet pre-boss payoff.

### Approved shop inventory

| Item/service | Price | Limit |
|---|---:|---|
| Temporal Bean | 25 Gold | Until carry cap of 9 |
| Health Shard | 12 Gold | Unlimited |
| Energy Shard | 12 Gold | Unlimited |
| Full HP or Energy restoration | 35 Gold | Once per shop visit |
| Reroll next upgrade reward | 25 Gold | Once per shop visit |

Grove Keeper shop omits reroll. Chrono Merchant's restoration is Full HP + Energy for 35 Gold. Unaffordable shop rows remain readable but dim.

### Canonical dialogue direction

- **Ruin Scout:** “The ruins remember violence. Find the blade they took from you.” / “Good. Let its light cut a path forward.”
- **Quartermaster:** “Gold still carries weight here. Spend it before time takes it from you.”
- **Grove Keeper:** “Time is not reversing for you. It is remembering what you do.” Hint: “Your echo will repeat the path you leave behind.”
- **Archivist:** “These books do not preserve words. They preserve action.” Hint: “One memory can hold one seal. Three memories can open a history.”
- **Prisoner Echo:** “You do not summon us from the past. You give us one more chance to act.”
- **Chrono Merchant:** “Three echoes follow you. The last step is yours alone.”

### Approved objective flow

| Map | Objective progression |
|---|---|
| 1 | Find a way through the ruins → Find the Rune Key → Defeat the Gatebreaker → Enter the Forgotten Barracks |
| 2 | Reach the command hall → Defeat the Barracks Captain → Enter the Overgrown Wilds |
| 3 | Find the source of the temporal roots → Awaken the Temporal Grove → Defeat Thorn Matriarch → Follow the echoes to the library |
| 4 | Recover the Astral Index → Defeat the Silent Curator → Descend into the Ash Forge |
| 5 | Restore the forge coolant flow → Defeat the Ash Warden → Find the imprisoned echoes |
| 6 | Free the imprisoned echo → Defeat the Mirror Jailer → Reach the Warden Approach |
| 7 | Reach the Chrono Sanctum → Defeat the Chrono Vanguard → Defeat the Chrono Guardian |
| 8 | Confront the Chrono Guardian → The Loop Has Been Broken |

Objectives use clear action verbs, maximum two lines and do not replace contextual puzzle hints with long instructions.

---

## Pending Approval

## 20. Audio & Music Bible — Approved

### Direction and rules

- Sound is dark-fantasy, readable and mobile-friendly: cold ancient dungeon, glassy/reversed temporal cues and warm danger/fire cues.
- Every significant Player, enemy, boss, loot, interaction, UI and loop event has a purposeful SFX.
- Combat telegraphs include audible cues. Music never masks essential gameplay signals.
- No voice acting in initial scope; NPC dialogue uses concise text with optional short character blips.

### Map music direction

| Map | Direction |
|---|---|
| 1 | Cold ruin ambience, light piano/rune bell and void wind. |
| 2 | Slow military percussion and dark strings. |
| 3 | Nature ambience, dark flute/harp and cyan shimmer. |
| 4 | Broken piano, page flutter and quiet glyph choir. |
| 5 | Metallic percussion, forge hammer, hot bass and fire crackle. |
| 6 | Glass chime, reversed pad, echo and chain ambience. |
| 7 | Processional choir and slow ritual drums. |
| 8 | Large temporal theme with Guardian phase variations. |

Mini-bosses use map music plus a boss-percussion/motif layer. Chrono Guardian has a dedicated three-phase track: ancient/stable phase, temporal-arpeggio shield phase, and high-pressure choir/percussion finale; Victory uses a resolved temporal variant.

### Combat/interaction sound identity

- Chrono Blade: cyan blade whoosh, temporal crack, cyan pulse and heavy Cleave impact.
- Ember Staff: ember ignition, fire projectile hiss, Nova burst, meteor descent/impact.
- Void Bow: bow draw/release, void trail, piercing snap and multi-arrow volley.
- Ghost/loop: cyan hum, ticking warning, reverse reset, layered Ghost spawn, lower-volume replay attacks, Stability Anchor resolve chime and shield-break crack.
- Recovery/loot: crimson heal chime, cyan energy pickup, Gold jingle, Bean magical pop and weapon-specific equip sting.
- Enemies require wind-up, attack, hit, death and special-mechanism cues; Bosses also require intro, shield, break, stagger, phase and death cues.
- UI uses restrained click, deny, panel, purchase, upgrade, objective, pause, game-over and victory cues.

### Audio mixer and settings

```text
MASTER
├── MUSIC
├── AMBIENCE
├── SFX
└── UI
```

Default mix: Master 100%, Music 65%, SFX 80%, UI 75%; Music/SFX toggles on; Vibration on when Android supports it. Settings persist locally when save/settings implementation is added.

Pause/System Menu stops gameplay SFX with time flow, ducks Music approximately 30–40% and leaves UI audible.

### Mobile constraints

- Target maximum approximately 16 simultaneous gameplay voices.
- Reuse/pool AudioSources; never spawn one per hit/projectile.
- One main ambience source per map, dedicated boss source where needed and clear predominantly 2D mix.

### Audio asset target

- Eight map ambience/music loops.
- One multi-phase Chrono Guardian track and one Victory variant.
- Approximately 50–70 purposeful reusable short SFX plus 15–20 boss/special-mechanic cues.

---

## Pending Approval

## 21. Detailed Asset Production Manifest — Approved

### Folder, naming and version rules

- Approved Unity-ready art lives under `Assets/Art`; references/contact sheets live under `Assets/ArtSource`.
- Category roots: Characters, Enemies, NPC, Weapons, Items, Maps, VFX, UI and Audio.
- Filename standard: `CD_[Category]_[Subject]_[Action]_[Direction]_[FrameCount]_v01.ext`.
- Approved art is never overwritten. Corrections create `v02`, are reviewed, then replace Unity references deliberately.

### Sprite-sheet convention

- Four-direction sheets always use rows: **Down → Left → Right → Up**.
- Frames always progress left-to-right.
- Player/weapon sheets use the approved 192×256 cell, shared 128 PPU and foot baseline.
- A 4-direction 8-frame Player walk sheet is therefore 1536×1024.

### Required character packs

- Base Player: Idle, Walk, Punch 01/02/03, Dash, Hit and Death.
- Chrono Blade: Idle, Walk, Slash 01/02/03, Chrono Burst, Time Cleave and Hit; pickup/icon/skill icons/VFX/SFX.
- Ember Staff: Idle, Walk, Fire Bolt, Flame Nova, Meteor Rune and Hit; pickup/icon/skill icons/VFX/SFX.
- Void Bow: Idle, Walk, Void Arrow, Piercing Shot, Shadow Volley and Hit; pickup/icon/skill icons/VFX/SFX.

### Required common packs

- Gold, Health Shard, Energy Shard, Temporal Bean, Rune Key, chest, Chrono Anchor and Stability Anchor.
- Each pickup has base art, optional separate glow/shadow, pickup VFX, HUD icon and SFX.
- Common VFX includes hit, heal, energy, Gold, enemy death, Dash/after-image, loop start/reset, Ghost spawn/path, Stability Anchor and shield states.

### Map/enemy/NPC/audio manifest

- Each map contains Master Base Map, optional Foreground, Collision Guide, props, interactives, map VFX and ambience/audio.
- Master Map naming: `CD_MapXX_Master_Base_v01.png`, optional foreground and Collision Guide counterparts.
- Enemy pack per identity contains Idle, Walk, Attack, Hit, Death, shadow and any required projectile/hazard/telegraph.
- NPC pack contains Idle, Talk, required pose/prop and portrait.
- Audio naming begins `CD_SFX_` or `CD_Music_` and follows the approved Audio Bible.

### Production order

```text
Pack 00 — Technical Art Setup
Pack 01 — Base Player
Pack 02 — Chrono Blade
Pack 03 — Map 1 Core
Pack 04 — Ember Staff + Maps 2–3
Pack 05 — Void Bow + Maps 4–6
Pack 06 — Maps 7–8 Finale
Pack 07 — UI/Audio Final Polish
```

### Mandatory approval gates

1. Contact sheet: approve identity, palette, silhouette and map composition.
2. Sprite sheets: verify canvas, baseline/pivot, crop, direction and frame alignment.
3. Unity preview: verify import, sorting, animation, collider and camera scale.
4. Gameplay test: verify readability, input/mobile UI and clean Console.
5. Pack lock: approved assets are versioned and never silently overwritten.

### Consistency guarantee rule

Sprite consistency is enforced through shared source canvas, shared PPU, shared baseline, locked direction order, contact-sheet review and five approval gates. Asset integration is forbidden until these checks pass; Unity transform scale/offset is never used as a substitute for correcting inconsistent source art.

---

## Pending Approval

## 22. Technical Implementation Roadmap — Approved

### Scene and map structure

- `GameScene` remains the persistent gameplay shell: Player, Camera, Canvas/UI, EventSystem, Managers, persistent Audio and VFX/pool roots.
- Each authored chapter becomes its own additive map scene: `Map01_RuinedEntrance` through `Map08_ChronoSanctum`.
- A map is unloaded and the next map is loaded at a completed exit. Player/HUD/managers persist, so the journey feels continuous while Android memory remains controlled.
- Each map remains a continuous Master Full Map image with manual collider workflow; additive scenes do not imply Tilemap workflow.
- Current Map 1 remains where it is until Pack 03 is complete, then can be migrated safely to its own map scene.

### Checkpoint/save rules

- Chrono Anchor saves current map, anchor ID, Player HP/Max HP, Energy/Max Energy, Gold, Beans, Rune Key state, equipped weapon, upgrade stacks, objective/map completion, defeated enemies, opened chests and solved Stability Anchors.
- Ghost runtime timelines are not saved on application reload. Incomplete Temporal Chambers reset on reload; completed Stability Anchors remain complete.
- Retry From Anchor restores checkpoint state and safe 70% HP/Energy. Weapon/upgrades before checkpoint remain; Gold/Bean/Key collected afterward are lost.
- Loop expiry retains normal enemy/boss HP but resets authored temporary puzzle mechanisms. Player death resets an entire boss encounter to Phase 1.

### Data and integration rules

- Balance/content lives in ScriptableObject definitions: Weapon, Skill, Upgrade, Enemy, Boss, Map, Shop, Dialogue and Loot data.
- Gameplay drives animation; animation visual state does not create a duplicate damage system.
- Asset import follows approved canvas/PPU/pivot rules; pixel assets are Point-filtered and master maps are tested with Android compression/size limits.
- Pool high-frequency projectiles/VFX/audio sources; do not pool one-time NPC/map props prematurely.

### Test and milestone sequence

Every milestone requires compile, clean new-console check, Play Mode test, relevant Player regression, state/transition test when applicable, save and eventual Android device test.

```text
Milestone 0 — Technical Setup
Milestone 1 — Base Player
Milestone 2 — Chrono Blade
Milestone 3 — Map 1 Vertical Slice
Milestone 4 — Map 2 + Map 3 / Ember Staff / first Ghost
Milestone 5 — Maps 4 + 5
Milestone 6 — Map 6 / Void Bow / three Ghosts
Milestone 7 — Maps 7 + 8 / Chrono Guardian / Victory
Milestone 8 — UI/Audio/Performance/Android full-run polish
```

---

## Pending Approval

- No remaining core design decision. Next action is production planning and Pack 00 execution after a final design-lock summary/review.
