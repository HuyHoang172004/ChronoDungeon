# AGENTS_UPGRADE.md — ChronoDungeon Major Quality Upgrade Plan

> **Mục đích:** Tài liệu này định nghĩa vòng nâng cấp lớn cho **ChronoDungeon – Trapped Beyond Time** sau khi các hệ thống gameplay nền tảng đã được xây dựng.
>
> **Ngôn ngữ:** Vietnamese  
> **Engine:** Unity 6.6  
> **Target:** Android Landscape  
> **Thể loại:** 2D top-down Action + Puzzle + Roguelite  
> **Toolchain:** Unity + Codex + Unity MCP + Git  
> **Trạng thái:** ACTIVE

---

# 0. Cách sử dụng tài liệu này

Agent phải đọc theo thứ tự:

1. `AGENTS.md`
2. `ROADMAP.md`
3. `AGENTS_UPGRADE.md`
4. Inspect project thực tế bằng Unity MCP
5. Chỉ sau đó mới sửa project.

`AGENTS.md` vẫn là bộ quy tắc kiến trúc, Git safety, testing và Unity MCP lâu dài.

`ROADMAP.md` vẫn ghi lại trạng thái milestone đã hoàn thành.

`AGENTS_UPGRADE.md` định nghĩa **hướng nâng cấp mới về world design, level design, đồ họa, UI, progression và game feel trước Final Deliverable**.

Nếu tài liệu và project thực tế khác nhau:

> **Project thực tế + serialized references + source code hiện tại là nguồn sự thật cuối cùng.**

Không được viết lại hệ thống đang ổn định chỉ vì tài liệu mô tả khác.

---

# 1. Mục tiêu nâng cấp

ChronoDungeon đã có các hệ thống gameplay cốt lõi. Vòng phát triển tiếp theo không nhằm nhồi thêm thật nhiều feature mới.

Mục tiêu là biến game từ:

> **"Một project Unity đã có đầy đủ chức năng."**

thành:

> **"Một game có cảm giác khám phá, chiến đấu, giải đố và tiến triển rõ ràng; đủ bắt mắt và có chất lượng trình bày tốt."**

Các mục tiêu bắt buộc:

- map phải rộng hơn đáng kể;
- không còn cảm giác mỗi khu chỉ là một hình chữ nhật nhỏ;
- người chơi có thể di chuyển qua nhiều sub-area liên tục;
- mỗi zone phải có bản sắc hình ảnh riêng;
- khu combat phải có nhịp đánh, enemy composition và objective;
- khu exploration phải có đường chính, ngách phụ, reward hoặc landmark;
- Time Loop không xuất hiện ở mọi nơi;
- chỉ Temporal Zone hoặc boss mechanic đặc biệt mới kích hoạt Loop/Ghost;
- UI/HUD phải gọn, không chồng chữ;
- art/environment/background phải được cải thiện ngay trong quá trình triển khai;
- Player/Ghost/Enemy/Boss phải dễ phân biệt;
- attack, dash, hit, death, rewind, Ghost spawn và reward phải có feedback rõ;
- upgrade phải cho người chơi thấy thay đổi chỉ số;
- độ khó tăng dần và run phải dài hơn bản hiện tại;
- Android/device QA có thể hoàn tất sau vòng nâng cấp này.

---

# 2. Những hệ thống hiện tại phải được bảo toàn

Trước khi sửa, agent phải inspect implementation thật.

Các foundation được coi là đã tồn tại và nên **reuse trước khi rewrite**:

## 2.1 Core gameplay
- Player movement
- Virtual joystick
- Directional attack
- Dash
- Health / damage / death
- Enemy damage
- Game Over / Restart
- Pause
- Main Menu
- Victory
- Settings
- Run state

## 2.2 Temporal systems
- Time Loop
- Timeline recorder
- Multiple Temporal Ghosts
- Ghost movement replay
- Ghost attack replay
- Ghost dash replay
- Reset architecture
- Room-local loop lifecycle
- Pressure switch
- Door
- Dual-switch Ghost puzzle
- Ghost interaction policy

## 2.3 Dungeon / progression
- Room lifecycle
- Room manager
- Room transition
- Enemy room clear
- Puzzle room clear
- Upgrade system
- Enemy roster
- Elite
- Chrono Guardian boss
- Tutorial flow

## 2.4 Presentation foundation
- HUD
- Player health
- Loop HUD
- Boss HUD
- Runtime feedback
- VFX/audio foundation
- Responsive UI foundation

Không tạo system trùng chức năng nếu có thể mở rộng sạch component hiện tại.

---

# 3. Design Vision mới

## 3.1 Fantasy

Player là một **Chrono Wanderer** bị mắc kẹt trong ChronoDungeon — một quần thể tàn tích nằm ngoài dòng thời gian.

Dungeon từng là một công trình khổng lồ dùng để điều khiển và lưu trữ thời gian. Sau một thảm họa, các khu vực bị:

- đóng băng thời gian;
- lặp lại;
- phân mảnh;
- tha hóa;
- lệch khỏi cùng một timeline.

Chrono Guardian là thực thể bảo vệ lõi thời gian.

Muốn thoát khỏi dungeon, Player phải:

1. khám phá các vùng bị đổ vỡ;
2. chiến đấu với sinh vật bị tha hóa;
3. mở khóa các cổng ancient/temporal;
4. học cách sử dụng chính bản thân trong quá khứ;
5. thu thập sức mạnh;
6. tiến vào Chrono Core;
7. đánh bại Chrono Guardian;
8. phá vòng lặp.

---

# 4. Core Gameplay Pillars

Mọi thay đổi lớn phải hỗ trợ ít nhất một pillar.

## 4.1 Exploration
Người chơi phải cảm thấy đang **đi qua một dungeon lớn**, không phải teleport giữa những box rời rạc.

## 4.2 Fast Readable Combat
Combat phải:
- dễ đọc;
- mượt;
- có telegraph;
- buộc người chơi di chuyển;
- có hit feedback;
- tránh spam vô nghĩa.

## 4.3 Selective Temporal Gameplay
Time Loop là signature mechanic nhưng **không được lạm dụng**.

Loop càng ít xuất hiện vô tội vạ thì lúc xuất hiện càng đặc biệt.

## 4.4 Meaningful Progression
Upgrade phải:
- thay đổi con số dễ thấy;
- thay đổi cách chơi;
- hoặc hỗ trợ rõ combat / mobility / temporal build.

## 4.5 Visual Readability
Người chơi phải nhìn nhanh và biết:
- Player ở đâu;
- Ghost ở đâu;
- enemy nào đang tấn công;
- projectile nào nguy hiểm;
- object nào tương tác được;
- cửa nào locked;
- puzzle nào liên quan temporal;
- objective hiện tại là gì.

---

# 5. World Structure mới

## 5.1 Nguyên tắc

Không còn coi:

> `1 zone = 1 rectangle`

Mỗi zone mới phải là:

> `1 zone = 2–4 sub-area / arena / corridor / pocket nối với nhau`

Target tổng thể:

- 8 main zones;
- khoảng 20–28 sub-area;
- đường chính rõ;
- có thể có 1–2 ngách phụ trong một số zone;
- transition tạo cảm giác đi sâu vào dungeon.

Không bắt buộc toàn bộ map nằm trong một camera.

Camera có thể:
- follow Player trong sub-area;
- clamp theo bounds;
- chuyển framing khi đi qua gate;
- hoặc chuyển sang segment kế tiếp.

---

# 6. World Flow đề xuất

```text
Zone 1 — Ruined Entrance
        ↓
Zone 2 — Forgotten Barracks
        ↓
Zone 3 — Overgrown Wilds
        ↓
Zone 4 — Echo Library [TEMPORAL]
        ↓
Zone 5 — Ash Forge
        ↓
Zone 6 — Shattered Mirror Prison [TEMPORAL]
        ↓
Zone 7 — Warden Approach
        ↓
Zone 8 — Chrono Sanctum [BOSS / MIXED TEMPORAL]
        ↓
Victory
```

---

# 7. Zone 1 — Ruined Entrance

## 7.1 Vai trò
- opening zone;
- onboarding;
- world introduction;
- combat đơn giản;
- first gate objective.

## 7.2 Loop
**OFF**

Không:
- timer;
- Ghost recording;
- Ghost spawn;
- Loop HUD.

## 7.3 Target sub-area
### Area 1A — Broken Gate
- Player spawn;
- environment showcase;
- movement tutorial ngắn;
- first interaction.

### Area 1B — Fallen Courtyard
- 2–4 melee enemies;
- attack/dash tutorial;
- basic obstacle.

### Area 1C — Rune Gate
- gate locked;
- tìm `Rune Key`;
- key nằm ở side pocket hoặc sau enemy pack;
- key mở cổng.

## 7.4 Visual
- ancient stone;
- collapsed architecture;
- vines;
- waterfall/chasm nếu phù hợp;
- torch;
- cyan ancient rune.

## 7.5 Acceptance
Zone 1 không được cảm giác như một tutorial box.

Người chơi phải:
- di chuyển qua nhiều đoạn;
- nhìn thấy world depth;
- chiến đấu;
- tìm key;
- mở gate;
- đi tiếp.

---

# 8. Zone 2 — Forgotten Barracks

## 8.1 Vai trò
Combat zone đầu tiên thực sự.

## 8.2 Loop
**OFF**

## 8.3 Target sub-area
### Area 2A — Barracks Yard
- melee enemy pack;
- 3–5 enemy tùy balancing.

### Area 2B — Watch Hall
- mix melee + Archer;
- pillar/cover/obstacle tạo đường đi.

### Area 2C — Captain Room
- multi-wave hoặc stronger encounter;
- clear mới mở exit.

### Optional reward pocket
- chest;
- first meaningful upgrade;
- consumable/heal nếu cần.

## 8.4 Combat goal
Không được spawn enemy ngẫu nhiên chỉ để đông.

Composition phải tạo pressure:
- melee áp sát;
- ranged ép di chuyển;
- Player cần attack + dash.

## 8.5 Visual
- banners;
- broken weapon rack;
- crates;
- armor props;
- stone floor;
- warm fire + cold temporal accents.

---

# 9. Zone 3 — Overgrown Wilds

## 9.1 Vai trò
Làm dungeon có cảm giác rộng và sống động hơn.

## 9.2 Loop
**OFF**

## 9.3 Sub-area
### Area 3A — Overgrown Passage
- path rộng hơn;
- vegetation;
- exploration.

### Area 3B — Split Path
- đường chính;
- side branch;
- reward.

### Area 3C — Beast Clearing
- Charge Beast / mobile enemies;
- địa hình ép dash.

### Area 3D — Rune Garden
- key/resource pickup;
- transition sang temporal zone.

## 9.4 Optional content
- secret chest;
- heal shrine;
- lore marker;
- temporary buff.

## 9.5 Visual
- roots;
- moss;
- glowing plant;
- mushrooms;
- broken ruins;
- fog;
- cyan/purple crystals.

---

# 10. Zone 4 — Echo Library

## 10.1 Vai trò
Temporal mechanic introduction ở scale lớn hơn.

## 10.2 Loop
**ON**

Khi enter:
- enable loop;
- capture initial temporal state;
- start timer;
- enable recorder;
- show Loop HUD;
- clear stale Ghost từ zone trước.

Khi exit:
- disable temporal mode;
- clear loop state theo policy;
- hide Loop HUD.

## 10.3 Sub-area
### Area 4A — Echo Entrance
- visual anomaly;
- giải thích ngắn vì sao loop active.

### Area 4B — Archive Switch
- Loop 1: Player đặt hành động;
- Loop 2: Ghost giữ mechanism;
- Player mở path.

### Area 4C — Twin Archive
- 2 mechanism;
- có thể cần timing;
- puzzle không chỉ đứng lên 2 plate quá đơn giản.

### Area 4D — Echo Gate
- final temporal puzzle;
- reward/exit.

## 10.4 Puzzle rule
Puzzle phải:
- visually understandable;
- có feedback;
- không soft-lock;
- reset chính xác;
- Ghost là bắt buộc.

---

# 11. Zone 5 — Ash Forge

## 11.1 Vai trò
Combat + hazard pressure.

## 11.2 Loop
**OFF**

## 11.3 Sub-area
### Area 5A — Forge Walkway
- environmental danger;
- moving or pulsing hazard.

### Area 5B — Furnace Floor
- Archer / Sentinel;
- hazard ép Player di chuyển.

### Area 5C — Smelter Arena
- multi-wave;
- stronger pack;
- gate clear condition.

### Reward
- combat-oriented upgrade;
- optional key / chest.

## 11.4 Visual
- lava orange;
- black metal;
- chain;
- furnace;
- spark;
- smoke;
- glowing hot floor.

---

# 12. Zone 6 — Shattered Mirror Prison

## 12.1 Vai trò
Temporal puzzle khó nhất trước boss.

## 12.2 Loop
**ON**

## 12.3 Sub-area
### Area 6A — Mirror Cells
- one Ghost setup.

### Area 6B — Broken Reflection
- timing + switch + movement.

### Area 6C — Multi-Ghost Chamber
- cần 2 Ghost hoặc nhiều timeline action.

### Area 6D — Temporal Lock
- puzzle climax;
- unlock final route.

## 12.4 Rule
Đây là zone duy nhất có thể dùng mechanic temporal phức tạp hơn đáng kể.

Không làm puzzle khó bằng cách che thông tin.

Khó vì:
- planning;
- timing;
- multiple action;
- hazard;
- route.

## 12.5 Visual
- mirror shards;
- semi-transparent platform;
- cyan lines;
- purple distortion;
- prison architecture;
- reflective VFX.

---

# 13. Zone 7 — Warden Approach

## 13.1 Vai trò
Final combat escalation.

## 13.2 Loop
**OFF**

## 13.3 Sub-area
### Area 7A — Hall of Guards
- mixed enemy pack.

### Area 7B — Warden Trial
- stronger wave / gauntlet.

### Area 7C — Elite Warden
- elite fight;
- attack patterns;
- telegraph.

### Area 7D — Final Shrine
- final upgrade;
- heal;
- stats view;
- boss warning.

## 13.4 Visual
- high pillars;
- statues;
- red/purple banners;
- ceremonial floor;
- large doors;
- darker mood.

---

# 14. Zone 8 — Chrono Sanctum

## 14.1 Vai trò
Final boss climax.

## 14.2 Loop
**MIXED**

### Phase 1
Loop OFF.

Focus:
- boss basic combat;
- learn attacks;
- dodge.

### Phase 2
Temporal anomaly activates.

Loop ON.

Focus:
- Ghost shield break;
- temporal objective;
- Player + past-self cooperation.

### Phase 3
Mixed finale.

Can include:
- faster pattern;
- temporary temporal pulse;
- Ghost disruption;
- vulnerability window.

## 14.3 Arena
Arena phải:
- lớn hơn normal combat room;
- không trống;
- có visual landmark;
- rõ attack telegraph;
- ít props gây cản tầm nhìn.

---

# 15. Contextual Time Loop Architecture

Không rewrite core TimeLoopManager nếu không cần.

Nên thêm policy/context layer.

Ví dụ:

```csharp
public enum TemporalMode
{
    Off,
    Puzzle,
    SpecialCombat,
    Boss
}
```

Zone/encounter có thể khai báo:

```csharp
TemporalMode temporalMode;
```

## 15.1 Off
- timer stopped;
- recorder stopped;
- no Ghost generation;
- Ghost history cleared hoặc suspended theo thiết kế;
- Loop HUD hidden.

## 15.2 Puzzle
- timer running;
- recorder active;
- Ghost active;
- puzzle resettable included.

## 15.3 Boss
- boss controls activation/deactivation;
- phase transitions phải safe;
- không để loop state leak sau Victory.

## 15.4 Required test
- enter normal → no loop;
- enter temporal → loop starts;
- exit temporal → timer disappears;
- no Ghost leak;
- restart state correct;
- boss phase transition correct.

---

# 16. Large Map / Camera / Exploration Implementation

## 16.1 Goal
Map phải cho cảm giác rộng nhưng vẫn mobile-friendly.

Không cần một giant map vô hạn.

Có thể dùng:
- connected authored sub-area;
- camera bounds;
- transition gates;
- additive scene nếu thật sự cần.

## 16.2 Preferred initial approach
Ưu tiên **mở rộng architecture hiện tại** trước.

Mỗi zone:
```text
ZoneRoot
├── Area_A
├── Area_B
├── Area_C
├── Secret_A
├── ZoneTransitions
└── Environment
```

Chỉ chuyển sang additive scene architecture nếu:
- GameScene quá nặng;
- authoring khó;
- reference management quá rối;
- profile cho thấy scene quá lớn.

Không migration architecture chỉ vì muốn "trông chuyên nghiệp".

## 16.3 Camera
Camera phải:
- follow Player;
- clamp trong current area;
- transition mượt khi sang area;
- không expose void/off-map;
- không rung/teleport khó chịu.

## 16.4 Area reveal
Có thể dùng:
- fog;
- darkness overlay;
- camera reveal;
- doorway transition;
- occluder;
- map/minimap reveal.

Không bắt buộc fog-of-war nếu scope lớn.

---

# 17. Keys / Gates / Objectives

Mỗi zone không nên chỉ:
> kill one enemy → exit.

Các objective có thể gồm:

## 17.1 Enemy Clear
- clear một group;
- clear wave;
- elite kill.

## 17.2 Key
- Rune Key;
- Forge Seal;
- Echo Sigil;
- Warden Crest.

## 17.3 Mechanism
- switch;
- pressure plate;
- timed door;
- temporal lock.

## 17.4 Exploration
- locate shrine;
- find side path;
- activate landmark.

## 17.5 Rule
Objective luôn cần:
- visual;
- UI feedback ngắn;
- clear completion state;
- no ambiguity.

---

# 18. Enemy Encounter Design

Không chỉ tăng enemy count.

Tăng difficulty bằng **composition**.

## 18.1 Corrupted Guard
Vai trò:
- basic melee;
- predictable attack;
- close pressure.

## 18.2 Ash Archer
Vai trò:
- ranged pressure;
- forces movement.

## 18.3 Charge Beast
Vai trò:
- fast gap closer;
- punishes standing still.

## 18.4 Clock Sentinel
Vai trò:
- projectile / area control;
- creates route pressure.

## 18.5 Elite Warden
Vai trò:
- high HP;
- multiple patterns;
- mini-boss.

## 18.6 Encounter examples

### Easy
```text
2 Guard
```

### Medium
```text
2 Guard
1 Archer
```

### Hard
```text
2 Guard
1 Archer
1 Beast
```

### Late
```text
1 Elite
1 Archer
1 Sentinel
```

Không spawn quá nhiều nếu readability kém hoặc performance tệ.

---

# 19. Difficulty Curve

Target cảm giác:

```text
Zone 1   ■
Zone 2   ■■
Zone 3   ■■
Zone 4   ■■■
Zone 5   ■■■■
Zone 6   ■■■■
Zone 7   ■■■■■
Zone 8   BOSS
```

Difficulty có thể tăng qua:
- enemy mix;
- encounter count;
- hazard;
- distance;
- objective;
- elite;
- attack pattern.

Không chỉ tăng HP sponge.

---

# 20. Player Visual Upgrade

Player final target:
- silhouette rõ;
- dark cloak / armor;
- cyan temporal accent;
- weapon readable;
- visual nhỏ vẫn phân biệt được.

## 20.1 Required states
- Idle
- Move
- Attack
- Dash
- Hurt
- Death

Không bắt buộc sprite-sheet animation phức tạp nếu runtime animation + good sprite đạt chất lượng.

## 20.2 Weapon
Primary:
**Chrono Blade**

Visual:
- cyan glow;
- clear slash;
- impact spark;
- after-image.

Optional secondary/special:
- Flame Rune;
- Chrono Burst.

Chỉ thêm nếu controls/UI vẫn sạch.

---

# 21. Ghost Visual Upgrade

Ghost phải nhìn giống Player nhưng khác ngay lập tức.

Target:
- cyan/blue;
- translucent;
- temporal trail;
- minor flicker;
- spawn dissolve;
- no confusion with current Player.

Ghost VFX không được quá sáng làm che puzzle.

---

# 22. Enemy Visual Upgrade

Mỗi archetype phải có:
- silhouette khác;
- color accent khác;
- weapon/attack readable.

Suggested:
- Guard: purple sword/shield;
- Archer: magenta ranged weapon;
- Beast: smoky dark purple;
- Sentinel: floating geometric body;
- Elite: orange/purple armor.

---

# 23. Boss Visual Upgrade

Chrono Guardian phải:
- lớn hơn enemy thường rõ rệt;
- central temporal core;
- phase change visible;
- shield state obvious;
- vulnerable state obvious;
- death sequence memorable.

Không chỉ scale Enemy lên.

---

# 24. Environment Art Direction

Phong cách:
> **Dark Fantasy Ancient Ruins + Temporal Magic**

Palette:
- deep navy;
- desaturated stone;
- cyan energy;
- purple temporal corruption;
- orange fire/danger.

## 24.1 Ancient Ruins kit
- floor tiles;
- wall;
- pillar;
- broken pillar;
- arch;
- statue;
- rubble;
- stairs;
- bridge.

## 24.2 Nature kit
- vines;
- grass;
- roots;
- mushroom;
- tree;
- moss;
- fog.

## 24.3 Temporal kit
- rune;
- crystal;
- crack;
- floating fragment;
- portal;
- timeline line;
- switch;
- pressure plate.

## 24.4 Forge kit
- chain;
- metal plate;
- furnace;
- gear;
- pipe;
- lava;
- smoke.

## 24.5 Boss kit
- giant rune;
- floating architecture;
- ritual floor;
- temporal core;
- shard orbit.

---

# 25. Asset Quality Rule

Từ giai đoạn này:

**Không chấp nhận raw primitive shape lộ rõ trong final visual**, trừ:
- collider invisible;
- debug object;
- intentionally stylized geometry phù hợp art style.

Nếu dùng external asset:
- license phải rõ;
- ưu tiên free/royalty-free hoặc user sở hữu;
- không lấy asset copyrighted không rõ quyền;
- không copy art thương mại từ game khác;
- ghi lại source/license trong `Docs/Assets/ASSET_SOURCES.md`.

Recommended sources:
- Unity Asset Store
- itch.io
- Kenney
- OpenGameArt

Không trộn quá nhiều style.

---

# 26. Background Quality

Mỗi zone cần ít nhất 3 lớp cảm giác chiều sâu:

1. playable floor;
2. structural/background layer;
3. atmosphere/detail layer.

Ví dụ:
```text
Foreground
- broken wall edge
- vines

Playable
- floor
- enemy
- props

Background
- cliff
- waterfall
- ruins
- fog
- crystal glow
```

Không để background chỉ là một màu phẳng.

---

# 27. Combat Feel

## 27.1 Attack
Pipeline mong muốn:
```text
input
→ anticipation nhỏ
→ slash visual
→ hit detection
→ hit spark
→ enemy flash
→ SFX
→ optional tiny hit-stop
```

## 27.2 Dash
- trail;
- after-image;
- cooldown readable;
- avoid wall penetration.

## 27.3 Hit
- flash;
- impact particle;
- short audio;
- readable knock/reaction nếu phù hợp.

## 27.4 Enemy death
- disable gameplay first;
- death visual;
- particle/smoke/shatter;
- optional loot;
- then reset-compatible inactive state.

Không phá Time Loop reset architecture.

---

# 28. Skill Expansion

Skill là optional extension, không được làm controls quá tải.

## 28.1 Flame Rune
- AoE temporal fire;
- long cooldown;
- orange/red VFX;
- damage group.

## 28.2 Chrono Burst
- short radius temporal shockwave;
- knockback/stagger;
- cyan/purple VFX.

Nếu skill chưa đủ polished:
- defer;
- không blocker cho upgrade plan.

---

# 29. Upgrade System nâng cấp

Upgrade phải có:
- icon;
- title;
- short description;
- old/new stat nếu phù hợp;
- visual rarity;
- immediate feedback.

## 29.1 Categories

### Combat
- Damage
- Attack Cooldown
- Attack Range
- Conditional effect

### Defense
- Max HP
- Heal
- Damage reduction nếu balance cho phép

### Mobility
- Move Speed
- Dash Cooldown
- Dash Distance

### Temporal
- Loop Duration
- Ghost Damage
- Ghost duration / capacity nếu architecture dùng được
- Temporal puzzle utility

---

# 30. Stats Panel

Player phải có cách xem current run stats.

Minimum:
- Max HP
- Current HP
- Attack Damage
- Attack Cooldown
- Move Speed
- Dash Cooldown
- Dash Distance

Temporal:
- Loop Duration
- Ghost limit
- Ghost damage modifier

Panel không cần inventory phức tạp.

---

# 31. Reward Flow

Sau major encounter:

```text
Clear encounter
↓
Reward feedback
↓
Chest / shrine / upgrade
↓
Choose / collect
↓
Stat feedback
↓
Continue
```

Không mở upgrade panel đột ngột giữa combat.

---

# 32. UI/HUD Redesign

## 32.1 Permanent HUD
Top-left:
- HP;
- compact resource/key.

Bottom-left:
- joystick.

Bottom-right:
- Attack;
- Dash;
- optional Skill.

## 32.2 Objective HUD
Top-right hoặc compact upper area:
- current objective;
- key count;
- short icon.

## 32.3 Temporal HUD
Chỉ hiện trong Temporal Zone:
- TIME;
- LOOP;
- Ghost count nếu cần.

Không hiện trong Normal Zone.

## 32.4 Boss HUD
- dedicated top bar;
- boss name;
- phase/status;
- không đè Loop text.

## 32.5 Room title
Khi enter:
- show 1–2 sec;
- fade.

Không giữ giant title trên gameplay lâu.

## 32.6 Tutorial
- contextual;
- one hint at a time;
- auto fade;
- no giant permanent text.

---

# 33. UI Layout Priority

Priority:
```text
Critical gameplay
> Objective
> Temporal
> Tutorial
> Flavor
```

Nếu overlap:
- lower priority UI phải hide/move/fade.

Không để:
- room title;
- boss intro;
- tutorial;
- shield warning;
- loop label

xuất hiện ở cùng một vị trí cùng lúc.

---

# 34. Minimap / Navigation

Optional but recommended nếu map mở rộng.

Minimum minimap:
- current sub-area;
- explored node;
- exit direction;
- key/objective icon.

Không cần real-time detailed map nếu tốn scope.

Có thể dùng node minimap.

---

# 35. Audio Direction

Music:
- exploration;
- combat;
- temporal;
- boss.

SFX:
- slash;
- hit;
- dash;
- enemy attack;
- enemy death;
- key pickup;
- gate open;
- upgrade;
- rewind;
- Ghost spawn;
- boss phase;
- Victory.

Audio mix phải tránh:
- SFX quá to;
- overlapping spam;
- music che combat feedback.

---

# 36. VFX Direction

Signature VFX cần ưu tiên:

1. Rewind
2. Ghost Spawn
3. Player Slash
4. Dash
5. Enemy Hit
6. Temporal Switch
7. Door Unlock
8. Boss Phase
9. Victory

Rewind + Ghost Spawn là identity mạnh nhất của game.

---

# 37. Roadmap nâng cấp mới

Không chạy U1–U9 một lần.

Mỗi milestone phải:
- implement;
- compile;
- smoke test;
- review;
- rồi mới tiếp tục.

---

# U1 — Large World Redesign

## U1.1 World Architecture
**[~] REWORKED — architecture/vertical slice implemented; pending manual traversal review.**

- [x] inspected current RoomManager, RoomExit, RoomCamera and TimeLoop integration;
- [x] reusable `SubArea`, `AreaBounds`, `WorldAreaManager` and `SubAreaTransition` foundation;
- [x] Room progression preserved, with normal-zone loop gating for authored rooms;
- [x] Zone 1 is one large 100x26 authored room; Room 2 / Guard Hall remains the separate temporal Zone 2;
- [x] Zone 1 sub-areas: Entrance Hall -> Connecting Corridor -> Fallen Courtyard -> Side Ruins / Rune Key Area -> Locked Rune Gate Approach;
- [x] combat encounter, Rune Key and locked Rune Gate;
- [x] Zone 1 timer/Ghost disabled; temporal rooms after Zone 1 remain enabled;
- [x] wall shell, deep background, stone variation, obstacles, torches, moss/vines and cyan rune landmarks applied across the five sub-areas;
- [x] viewport-aware camera clamping and delayed corridor-to-room bounds activation added; Zone 1 remains Normal with Loop/Ghost off.
- [x] player render order raised above procedural floor/wall dressing; camera clear color changed from blue skybox to dark dungeon color.

Manual follow-up remains: physical traversal/touch feel on an Android aspect ratio and final imported art replacement. The implementation is intentionally still marked [~] until that manual review is complete.

## U1.2 Zone 1 Expansion
- build 2–3 connected areas;
- first key/gate objective;
- visual art applied immediately.

## U1.3 Zone 2 Expansion
- combat progression;
- multiple enemy groups;
- reward.

## U1.4 Zone 3 Expansion
- branching exploration;
- optional reward;
- richer environment.

## U1.5 Zones 4–8 Layout Expansion
- expand remaining zone footprints;
- preserve their gameplay role;
- no need final polish yet, but no ugly raw box layout.

### U1 Acceptance
- Player traverses multiple connected sub-area.
- Camera and collisions work.
- No major regression.
- At least first 3 zones clearly feel larger.
- Existing progression still reaches later zones.

---

# U2 — Contextual Time Loop

## U2.1 Temporal Context
- implement zone-level temporal policy.

## U2.2 Normal Zone
- no Loop HUD;
- no recorder;
- no Ghost.

## U2.3 Temporal Zone
- Loop/Ghost start correctly.

## U2.4 Boss Mixed Mode
- temporal phase activation/deactivation.

### U2 Acceptance
Normal combat can be played indefinitely without timer rewind.
Temporal zone preserves original signature mechanic.

---

# U3 — UI/HUD Redesign

## U3.1 Global HUD layout
## U3.2 Contextual objective
## U3.3 Temporal HUD visibility
## U3.4 Boss HUD hierarchy
## U3.5 Tutorial popup cleanup
## U3.6 Upgrade/stats UI
## U3.7 Game Over/Victory consistency

### U3 Acceptance
No obvious text overlap in common 16:9 Game View.
Critical information is readable.

---

# U4 — Environment Art Pass

## U4.1 Ancient ruins
## U4.2 Overgrown zone
## U4.3 Echo temporal zone
## U4.4 Ash Forge
## U4.5 Mirror prison
## U4.6 Warden area
## U4.7 Chrono boss environment
## U4.8 lighting/background consistency

### U4 Acceptance
No major zone looks like raw rectangles/basic Unity primitives.

---

# U5 — Character Visual Pass

## U5.1 Player
## U5.2 Ghost
## U5.3 Guard/Archer/Beast/Sentinel
## U5.4 Elite
## U5.5 Chrono Guardian
## U5.6 animation/readability

---

# U6 — Encounter & Objective Pass

## U6.1 enemy composition
## U6.2 multi-group combat
## U6.3 key/gate
## U6.4 hazard objective
## U6.5 elite gauntlet
## U6.6 secret/reward pockets
## U6.7 objective UI feedback

---

# U7 — Upgrade & Reward Pass

## U7.1 upgrade card visual
## U7.2 stat panel
## U7.3 rarity
## U7.4 reward placement
## U7.5 upgrade feedback
## U7.6 final shrine

---

# U8 — Combat Feel Pass

## U8.1 attack impact
## U8.2 dash impact
## U8.3 enemy feedback
## U8.4 death feedback
## U8.5 projectile polish
## U8.6 temporal VFX
## U8.7 audio mix

---

# U9 — Difficulty / Pacing / Quality Pass

## U9.1 full run pacing
## U9.2 difficulty curve
## U9.3 zone duration
## U9.4 enemy tuning
## U9.5 upgrade tuning
## U9.6 boss tuning
## U9.7 UI final cleanup
## U9.8 visible placeholder cleanup
## U9.9 full Editor run

Sau U9 mới quay lại:
- Android real device;
- M11 deferred checks;
- M12 final manual QA;
- M13 Final Deliverable.

---

# 38. Execution Protocol cho Agent

Mỗi lần user nói:
> "Tiếp tục upgrade milestone"

Agent phải:

1. đọc `AGENTS.md`;
2. đọc `ROADMAP.md`;
3. đọc `AGENTS_UPGRADE.md`;
4. inspect Unity scene/project;
5. xác định milestone U sớm nhất chưa DONE;
6. inspect dependency;
7. plan ngắn;
8. implement milestone đó;
9. compile;
10. Play Mode smoke test;
11. regression test trực tiếp liên quan;
12. Console check;
13. save;
14. exit Play Mode;
15. update trạng thái upgrade;
16. STOP để user review.

Không tự chạy nhiều milestone lớn liên tiếp trừ khi user yêu cầu rõ.

---

# 39. Testing Policy

Do project đã có nhiều automated checks, vòng upgrade ưu tiên test lean.

Minimum:
- compile;
- no new runtime exception;
- happy path của feature mới;
- related regression;
- Console cuối;
- saved scene/prefab.

Không chạy stress test quá mức nếu gây crash Editor.

Manual test phải ghi rõ nếu MCP không xác nhận đáng tin:
- visual quality;
- touch;
- mobile feel;
- audio balance;
- difficulty feel.

---

# 40. Git Safety

Agent không:
- commit;
- push;
- reset;
- rebase;
- delete branch;
- sửa `.gitignore`

trừ khi user yêu cầu rõ.

Sau mỗi major U phase, user sẽ tự checkpoint Git.

---

# 41. External Asset Policy

Nếu cần asset bên ngoài:

1. ưu tiên asset có license rõ;
2. ghi source;
3. không tải asset thương mại trái phép;
4. không dùng artwork lấy trực tiếp từ game khác;
5. không trộn nhiều style không tương thích;
6. import vào đúng folder.

Create:
```text
Docs/Assets/ASSET_SOURCES.md
```

Format:
```text
Asset:
Source:
License:
Used in:
Modified:
```

Nếu agent không thể tải asset hợp lệ:
- tạo integration slot;
- dùng polished procedural/runtime visual tạm;
- báo user asset cần tải.

Không dùng raw square/circle làm final character/environment visual nếu có thể tránh.

---

# 42. Folder Organization

Recommended:

```text
Assets/
├── Art/
│   ├── Characters/
│   │   ├── Player/
│   │   ├── Ghost/
│   │   └── Enemies/
│   ├── Boss/
│   ├── Environment/
│   │   ├── Ruins/
│   │   ├── Nature/
│   │   ├── Temporal/
│   │   ├── Forge/
│   │   └── Sanctum/
│   ├── Props/
│   ├── UI/
│   └── VFX/
├── Audio/
│   ├── Music/
│   ├── Combat/
│   ├── Temporal/
│   └── UI/
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Environment/
│   ├── Puzzle/
│   └── UI/
├── Scripts/
└── Scenes/
```

Không bắt buộc migration toàn bộ folder cũ nếu rủi ro reference cao.

---

# 43. Performance Guardrails

Đẹp hơn không có nghĩa spam effect.

Avoid:
- unbounded particles;
- expensive per-frame searches;
- excessive transparent full-screen layers;
- hundreds of physics colliders vô nghĩa;
- Instantiate/Destroy continuous projectile;
- too many real-time lights.

Prefer:
- reuse;
- pooling cho projectile/VFX lặp;
- cached references;
- simple 2D lighting;
- particles có cap.

---

# 44. Definition of Done cho Major Upgrade

Vòng upgrade chỉ được coi là hoàn thành khi:

- map không còn cảm giác 8 box rời;
- Player đi qua nhiều sub-area;
- normal zone không bị rewind bởi loop;
- Temporal Zone dùng Ghost hợp lý;
- ít nhất 2 zone có exploration rõ;
- ít nhất 2 zone có combat encounter đa dạng;
- key/gate objective hoạt động;
- UI không còn overlap rõ ràng;
- Player/Ghost/Enemy/Boss visually readable;
- environment có art/background/props;
- attack/dash/hit/death có feedback tốt;
- upgrade có stat feedback;
- difficulty tăng dần;
- full Editor run có thể tới Victory;
- không có known game-breaking regression.

---

# 45. Final Product Goal

Target cảm giác cuối:

```text
Main Menu
    ↓
Enter Dungeon
    ↓
Explore
    ↓
Fight
    ↓
Find Key / Unlock Gate
    ↓
Discover Temporal Anomaly
    ↓
Use Ghost
    ↓
Solve Puzzle
    ↓
Obtain Upgrade
    ↓
Explore New Biome
    ↓
Fight Harder Enemy Mix
    ↓
Elite
    ↓
Final Shrine
    ↓
Chrono Guardian
    ↓
Temporal Boss Phase
    ↓
Victory
```

Người chơi phải cảm nhận được:

> "Mình đang đi sâu vào một dungeon thật, mạnh dần lên, gặp các khu vực khác nhau và chỉ ở những nơi thời gian bị phá vỡ thì mình mới cần hợp tác với bản thân trong quá khứ."

---

# 46. Immediate Next Milestone

## **U1.1 — World Architecture**

Agent phải bắt đầu bằng cách:

1. inspect `Room`, `RoomManager`, `RoomExit`, `RoomCamera`, TimeLoop integration;
2. inspect 8 zone hiện có trong GameScene;
3. thiết kế cấu trúc reusable sub-area;
4. không phá current progression;
5. triển khai foundation cho connected larger map;
6. sau đó mở rộng **Zone 1 — Ruined Entrance** thành 2–3 sub-area;
7. áp dụng visual/background tốt ngay từ đầu;
8. không implement U2/U3/U4 toàn bộ trong cùng task.

### U1.1 / first vertical slice success
Player phải có thể:

```text
Start
→ đi qua Broken Gate
→ chiến đấu ở Fallen Courtyard
→ tìm Rune Key
→ quay/mở Rune Gate
→ đi sang vùng kế
```

Không có Loop Timer ở vertical slice này.

Scene phải:
- compile;
- playable;
- saved;
- Console sạch;
- không commit/push.

---

# 47. Prompt khởi động đề xuất cho Codex

```text
Đọc AGENTS.md, ROADMAP.md và AGENTS_UPGRADE.md.

Đây là vòng Major Quality Upgrade của ChronoDungeon.
Tạm thời Android/device QA không phải ưu tiên.

Dùng Unity MCP inspect project hiện tại trước khi thay đổi.

Bắt đầu đúng milestone U1.1 — World Architecture theo AGENTS_UPGRADE.md.

Mục tiêu của vertical slice đầu tiên:
- biến Zone 1 từ một phòng đơn giản thành một khu vực lớn gồm 2–3 sub-area nối với nhau;
- Player có cảm giác khám phá và đi sâu vào dungeon;
- có combat encounter;
- có Rune Key;
- có locked Rune Gate;
- lấy key rồi mở gate để đi tiếp;
- Loop/Ghost phải OFF trong Zone 1;
- áp dụng background, props, ánh sáng và visual tốt ngay trong lúc triển khai, tránh primitive placeholder lộ liễu;
- giữ toàn bộ core systems hiện tại ổn định.

Không làm toàn bộ U2-U9 cùng lúc.
Không commit/push.

Sau khi U1.1 hoàn thành:
- compile;
- smoke test;
- Console check;
- save scene/prefab;
- exit Play Mode;
- báo cáo file/GameObject thay đổi, test đã chạy, known limitation;
- dừng để tôi review.
```

---

**END OF AGENTS_UPGRADE.md**
