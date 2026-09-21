# AGENTS.md — Quy tắc phát triển ChronoDungeon

## 1. Thông tin project

Tên project: **ChronoDungeon – Trapped Beyond Time**

Engine: **Unity 6.6**  
Nền tảng: **Android**  
Hướng màn hình: **Landscape**  
Thể loại: **2D top-down Action + Puzzle + Roguelite**  
Input chính: **Mobile touch controls**  
Toolchain phát triển: **Codex + Unity MCP + Git**

File này là bộ quy tắc lâu dài dành cho mọi AI agent làm việc trong repository này.

Project phải được phát triển thành một game sinh viên hoàn chỉnh, có độ polish tốt, không phải tập hợp các prototype rời rạc.

Mục tiêu chất lượng:
- gameplay loop mạch lạc;
- visual feedback rõ ràng;
- mobile controls ổn định;
- UI dễ đọc;
- progression có ý nghĩa;
- presentation chỉn chu;
- không còn hành vi placeholder lộ liễu trong final build;
- không có compile/runtime error;
- core mechanic đủ dễ hiểu để trình bày và demo trước giảng viên.

Không tối ưu theo hướng nhồi thật nhiều feature.  
Ưu tiên một game nhỏ hơn nhưng **hoàn chỉnh, có chủ đích, ổn định và hấp dẫn**.

---

## 2. Tầm nhìn cốt lõi của game

Player bị mắc kẹt trong một dungeon bị nguyền rủa và lặp lại theo thời gian.

Mỗi loop kéo dài khoảng **20 giây**.

Khi một loop kết thúc:
1. world rewind;
2. Player trở về trạng thái đầu loop;
3. các Enemy và puzzle object có thể reset trở về trạng thái ban đầu;
4. một **Temporal Ghost** có thể replay hành động của Player từ loop trước;
5. Player hiện tại có thể hợp tác với chính phiên bản quá khứ của mình.

Bản sắc trung tâm của game là:

> **Giải combat và dungeon puzzle bằng cách hợp tác với chính hành động của mình trong quá khứ.**

Temporal Ghost không chỉ để trang trí.  
Nó phải trở thành một gameplay mechanic thiết yếu.

Ví dụ:
- Loop 1: Player đứng trên Switch A.
- Time rewind.
- Loop 2: Ghost 1 replay Loop 1 và giữ Switch A.
- Player hiện tại đứng trên Switch B.
- Door mở.

Game cuối cần liên tục tạo ra các tình huống khiến người chơi phải nghĩ:

> "Ở loop này mình nên làm gì để Ghost của mình có thể giúp ở loop tiếp theo?"

---

## 3. Các trụ cột thiết kế

Mỗi gameplay feature lớn nên hỗ trợ ít nhất một trong các trụ cột sau.

### 3.1 Time Cooperation
Hành động trong quá khứ của Player trở thành công cụ hữu ích trong loop tương lai.

### 3.2 Fast Readable Combat
Combat phải dễ hiểu trên mobile:
- movement;
- attack;
- dash;
- skill giới hạn;
- enemy telegraph rõ;
- hit feedback rõ.

### 3.3 Short Tactical Loops
Loop khoảng 20 giây phải tạo cảm giác cấp bách nhưng không gây khó chịu.

### 3.4 Puzzle + Combat Integration
Ghost nên hữu ích cho:
- pressure switch;
- trigger đồng thời;
- door;
- timed mechanism;
- distract hoặc damage Enemy;
- vượt hazard an toàn.

### 3.5 Roguelite Progression
Trong một dungeon run, Player nhận các lựa chọn upgrade để thay đổi build.

### 3.6 Mobile-Friendly Presentation
Game phải dễ nhìn và dễ điều khiển trên màn hình Android landscape.

---

## 4. Scope final mục tiêu

Scope cuối dự kiến:

- 1 dungeon run hoàn chỉnh;
- khoảng 6–8 room;
- combat room;
- time-puzzle room;
- trap room;
- treasure/upgrade room;
- elite encounter;
- final boss;
- 3 normal enemy archetype;
- 1 boss: **Chrono Guardian**;
- ít nhất 3 kiểu Temporal Ghost puzzle;
- tối đa 3 Temporal Ghost active;
- hệ thống 1-of-3 roguelite upgrade;
- UI flow hoàn chỉnh;
- sound/VFX feedback;
- Android build;
- Victory và Game Over flow.

Không mở rộng scope vượt quá mức này nếu các milestone bắt buộc chưa hoàn thành và ổn định.

---

## 5. Foundation đã triển khai

Trước khi thay đổi bất kỳ thứ gì, luôn inspect project thực tế vì section này có thể bị cũ theo thời gian.

Tại thời điểm file này được tạo, project đã có foundation sau.

### Scene flow
- SplashScene
- MainMenuScene
- GameScene

Flow mong đợi:

SplashScene -> MainMenuScene -> GameScene

### Gameplay foundation hiện có
- PlayerMovement
- VirtualJoystick
- PlayerAttack
- reusable Health component
- reusable HealthBar
- EnemyContactDamage
- GameManager
- Player HP
- Enemy HP
- Game Over
- Restart
- Main Menu
- Moving Fog

### Time Loop foundation hiện có
- ITimeLoopResettable
- TimeLoopManager
- TimeLoopActor
- TimeLoopHUD
- loop 20 giây
- world rewind không reload scene
- Player reset
- Enemy reset
- Enemy chết dùng deactivate/reactivate để hỗ trợ rewind
- loop counter
- timer pause cùng gameplay

Agent PHẢI inspect implementation hiện tại thay vì mặc định những chi tiết trên luôn chính xác.

---

## 6. Quy tắc kiến trúc

### 6.1 Inspect Before Editing

Trước mỗi milestone:
1. dùng Unity MCP inspect scene hiện tại;
2. đọc các script liên quan;
3. xác định dependency;
4. xác định system/component có thể tái sử dụng;
5. tránh tạo hệ thống trùng với thứ đã tồn tại.

Nếu chưa chắc cấu trúc project, hãy inspect.  
Không tự đoán.

### 6.2 Separation of Responsibilities

Ưu tiên component có trách nhiệm rõ ràng.

Ví dụ:
- Health quản lý HP.
- PlayerAttack quản lý attack của Player.
- EnemyFollow quản lý movement của Enemy.
- EnemyContactDamage quản lý contact damage.
- TimeLoopManager điều phối loop timing.
- Timeline recorder ghi lịch sử Player.
- Ghost playback component replay timeline.
- GameManager quản lý game-state transition.
- RoomManager quản lý room lifecycle.
- UpgradeManager quản lý lựa chọn upgrade.

Tránh tạo "God class".

### 6.3 Reuse Before Duplication

Trước khi tạo system mới:
- tìm xem đã có component dùng lại được chưa;
- mở rộng component hiện tại một cách sạch nếu phù hợp;
- không tạo hai system riêng cho Player và Enemy nếu một reusable system là đủ.

### 6.4 Time Loop Compatibility

Các gameplay object mới phải được thiết kế với rewind trong đầu.

Ví dụ:
- Enemy;
- Switch;
- Door;
- Trap;
- Projectile;
- destructible object;
- puzzle mechanism.

Dùng reset architecture hiện tại khi phù hợp.

### 6.5 Ghost Compatibility

Các hành động gameplay quan trọng của Player về sau phải có khả năng được biểu diễn trong timeline.

Timeline architecture nên mở rộng được cho:
- movement;
- facing;
- attack;
- dash;
- skill;
- interaction.

Không tạo kiến trúc movement-only khiến sau này phải viết lại toàn bộ recorder khi thêm event.

### 6.6 Tránh Hard-Coded Scene Lookup

Ưu tiên:
- serialized reference;
- interface;
- component reference;
- event.

Tránh lạm dụng GameObject.Find hoặc dependency bằng string nếu có thể dùng reference ổn định.

### 6.7 Mobile Performance

Mục tiêu là performance ổn định trên Android.

Tránh:
- Instantiate/Destroy quá thường xuyên trong gameplay loop;
- quá nhiều Update không cần thiết;
- allocation lớn mỗi frame;
- global search đắt đỏ trong runtime;
- particle không giới hạn;
- quá nhiều physics object không kiểm soát.

Dùng pooling khi thực sự có lợi, đặc biệt cho:
- projectile;
- VFX lặp lại;
- Enemy tái sử dụng nếu cần.

Không over-engineer pooling quá sớm cho object chỉ xuất hiện vài lần.

---

## 7. Visual Direction

Final presentation nên mang cảm giác dark-fantasy time dungeon thống nhất.

Gợi ý visual language:
- dungeon đá tối;
- time energy màu blue/cyan;
- danger của Enemy màu warm orange/red;
- Temporal Ghost bán trong suốt, sáng nhạt;
- temporal effect dùng purple hoặc cyan;
- interactive object phải tương phản rõ với background.

Game phải truyền đạt trực quan:
- thứ gì gây damage;
- thứ gì tương tác được;
- thứ gì thuộc time mechanic;
- trạng thái active/inactive;
- đâu là Ghost và đâu là Player hiện tại.

Không dành quá nhiều thời gian cho decorative art trước khi core gameplay ổn định.

Temporary shape được chấp nhận trong quá trình implement.  
Ở final milestone phải thay hoặc cải thiện các placeholder lộ liễu.

---

## 8. Mobile Controls

Final controls nên giữ đơn giản.

Expected controls:
- bên trái: VirtualJoystick;
- bên phải: Attack;
- bên phải: Dash;
- optional Skill/Interact button khi thật sự cần.

Controls phải:
- hỗ trợ movement + button input đồng thời;
- touch target đủ lớn;
- không che HUD quan trọng;
- hoạt động tốt ở landscape;
- phù hợp nhiều phone aspect ratio.

Touch behavior phải được test thủ công nếu Unity MCP không mô phỏng đáng tin cậy.

---

## 9. Combat Direction

Combat nên rõ ràng hơn là quá phức tạp.

### Player
Target final capabilities:
- movement;
- directional attack;
- dash;
- optional active skill;
- HP;
- upgrade modifier.

### Enemy archetype
Mục tiêu khoảng 3 archetype:

1. **Slime / Chaser**
   - tiếp cận Player;
   - contact hoặc melee attack đơn giản.

2. **Archer / Ranged Enemy**
   - giữ khoảng cách;
   - bắn projectile có telegraph rõ.

3. **Knight hoặc Mage**
   - Knight: charge/block/heavy melee;
   - HOẶC Mage: AoE/teleport/projectile.

Chọn loại tạo gameplay variety tốt hơn với chi phí development hợp lý.

### Elite
Tái sử dụng archetype hiện có nhưng tăng:
- HP;
- damage;
- speed;
- attack pattern;
- visual treatment.

---

## 10. Quy tắc Temporal Ghost

Temporal Ghost là signature system của game.

Final behavior mong muốn:
- record timeline của Player hiện tại;
- rewind;
- spawn Ghost đại diện loop vừa hoàn thành;
- Ghost replay lại hành động;
- tối đa 3 Ghost active;
- khi vượt giới hạn, loại Ghost cũ nhất.

Ghost cuối cùng cần replay:
- movement;
- facing;
- attack;
- interaction;
- dash nếu dash quan trọng với puzzle/combat.

Ghost KHÔNG được:
- nhận joystick input của Player;
- gây Player Game Over;
- hoạt động như autonomous AI;
- thay thế Player hiện tại.

Ghost interaction phải có chủ đích.

Ghost có thể tham gia các system được chọn như:
- pressure switch;
- attack Enemy;
- puzzle interaction.

Không để mọi trigger tự động phản ứng với Ghost.  
Dùng compatibility rõ ràng khi phù hợp.

---

## 11. Quy tắc Puzzle Design

Puzzle phải dạy Temporal Ghost mechanic theo mức độ tăng dần.

Final game cần ít nhất 3 puzzle pattern có ý nghĩa.

### Puzzle A — Dual Pressure Switch
Ghost giữ A trong khi Player activate B.

### Puzzle B — Timed Door
Player thực hiện hành động ở một loop để Ghost trigger mechanism đúng thời điểm ở loop tiếp theo.

### Puzzle C — Combat Cooperation
Ghost attack/distract một threat trong khi Player hiện tại làm objective khác hoặc attack target khác.

Optional:
- lever sequence;
- hazard timing;
- moving platform;
- projectile interception.

Mỗi puzzle phải hiểu được từ visual feedback.  
Tránh puzzle tối nghĩa buộc người chơi thử sai mà không có feedback.

---

## 12. Room và Dungeon Structure

Target flow:

Start Room  
-> Combat / Puzzle / Trap  
-> Upgrade / Treasure  
-> Combat / Puzzle  
-> Elite  
-> Final preparation  
-> Chrono Guardian  
-> Victory

Final run target: khoảng 6–8 room.

Room system nên hỗ trợ:
- enter room;
- room start;
- enemy/puzzle setup;
- room clear condition;
- door lock/unlock;
- room completion;
- transition sang room tiếp theo.

Không làm procedural generation nếu authored dungeon bắt buộc chưa hoàn thành.  
Một authored dungeon polished tốt hơn procedural generation yếu.

---

## 13. Roguelite Upgrade System

Implement hệ thống chọn 1 trong 3 upgrade.

Upgrade gợi ý:
- +attack damage;
- +movement speed;
- lower dash cooldown;
- +max HP;
- heal;
- +loop duration;
- stronger Ghost damage;
- +maximum Ghost count nếu cân bằng cho phép;
- Ghost attack bonus;
- temporal ability improvement.

Mỗi upgrade phải:
- có title rõ;
- có description ngắn;
- ảnh hưởng gameplay dễ nhận biết.

Tránh hàng chục upgrade ít tác động.  
Một pool nhỏ nhưng có ý nghĩa tốt hơn.

---

## 14. Boss Direction — Chrono Guardian

Final boss: **Chrono Guardian**

Boss phải thể hiện time mechanic của game thay vì chỉ là Enemy nhiều HP.

Cấu trúc gợi ý:

### Phase 1
Combat rõ ràng:
- melee/ranged attack;
- telegraphed hazard.

### Phase 2
Áp lực time mechanic:
- cần Ghost hỗ trợ;
- nhiều target/switch;
- boss shield chỉ vulnerable dưới điều kiện temporal.

### Phase 3
Finale áp lực cao:
- pattern nhanh hơn;
- temporal zone;
- boss có thể disrupt hoặc disable Ghost tạm thời;
- Player phải kết hợp hành động hiện tại với setup từ loop trước.

Mechanic phải fair và có telegraph rõ.

Một boss ngắn nhưng polished tốt hơn boss dài và bug.

---

## 15. UI Requirements

Final game nên có:

### Splash
- title;
- loading transition.

### Main Menu
- Play;
- Settings;
- Exit khi phù hợp nền tảng.

### Gameplay HUD
- Player HP;
- loop timer;
- loop number;
- Ghost count;
- Attack;
- Dash;
- optional Skill;
- room/puzzle feedback khi cần.

### Pause
- Resume;
- Restart Run;
- Main Menu;
- Settings.

### Upgrade UI
- 3 lựa chọn rõ ràng.

### Game Over
- Restart;
- Main Menu.

### Victory
- completion message;
- run statistics nếu có;
- Main Menu / Replay.

UI phải nhất quán và dễ đọc trên Android landscape.

---

## 16. Audio và Feedback Requirements

Trước khi tuyên bố game hoàn chỉnh, phải có feedback cho các hành động quan trọng:

- Player attack;
- Player hit;
- Enemy hit;
- Enemy death;
- dash;
- rewind;
- Ghost spawn;
- switch activation;
- door open;
- upgrade selection;
- boss attack;
- Victory;
- Game Over.

Có thể dùng:
- SFX đơn giản;
- screen/UI feedback;
- particle;
- hit flash;
- optional camera shake cho impact lớn.

Không lạm dụng camera shake hoặc particle.

---

## 17. Quy tắc Testing

Mỗi milestone phải được test trước khi coi là hoàn thành.

Minimum workflow:
1. save script;
2. chờ Unity compile;
3. inspect Console;
4. fix compile error;
5. enter Play Mode;
6. test feature mới;
7. regression test các feature hiện có có khả năng bị ảnh hưởng;
8. inspect Console lần nữa;
9. exit Play Mode;
10. save scene/prefab.

Không được tuyên bố đã test touch interaction nếu MCP không test đáng tin cậy.  
Phải liệt kê rõ phần user cần test thủ công.

Milestone không được coi là hoàn thành nếu còn:
- compile error;
- runtime exception do feature mới;
- broken reference;
- existing gameplay bị hỏng rõ ràng.

Warning do implementation mới tạo ra phải được kiểm tra.

---

## 18. Git Safety Rules

Agent KHÔNG được:
- commit;
- push;
- reset;
- rebase;
- delete branch;
- sửa .gitignore

trừ khi user yêu cầu rõ.

User tự xử lý Git checkpoint sau khi review milestone.

Không sửa:
- Library;
- Temp;
- generated IDE files.

---

## 19. Quy tắc Unity MCP

Dùng Unity MCP cho:
- scene inspection;
- hierarchy inspection;
- GameObject creation;
- component changes;
- serialized reference;
- scene saving;
- Play Mode;
- Console inspection.

Việc sửa C# source bằng workspace file editing là chấp nhận được.

Không chỉnh trực tiếp Unity scene YAML khi có thể thực hiện an toàn bằng Unity MCP.

Nếu Unity MCP không khả dụng:
- không được giả vờ đã test scene;
- phải báo blocker.

---

## 20. Milestone Execution Protocol

Khi user yêu cầu "tiếp tục phát triển" hoặc "tiếp tục milestone tiếp theo":

1. Đọc AGENTS.md.
2. Đọc ROADMAP.md.
3. Dùng Unity MCP inspect project thực tế.
4. Xác định milestone sớm nhất chưa DONE.
5. Kiểm tra xem implementation hiện tại đã đáp ứng một phần milestone chưa.
6. Lập implementation plan ngắn.
7. Chỉ implement milestone đó và dependency trực tiếp cần thiết.
8. Compile.
9. Test.
10. Fix error.
11. Regression test.
12. Cập nhật ROADMAP.md trung thực.
13. Dừng sau milestone.
14. Báo cáo:
   - file tạo mới;
   - file sửa;
   - GameObject/component thay đổi;
   - test đã chạy;
   - kết quả test;
   - manual test cần user thực hiện;
   - known limitation;
   - milestone tiếp theo được đề xuất.

KHÔNG tự động chuyển sang major milestone tiếp theo nếu user chưa yêu cầu.

Checkpoint này là có chủ đích để đảm bảo reliability, Git safety, quota management và review.

---

## 21. Quality Gate trước khi Final Completion

Không được tuyên bố game hoàn chỉnh cho đến khi tất cả điều kiện sau đạt:

- core Time Loop hoạt động ổn định;
- Temporal Ghost replay được các hành động bắt buộc;
- có ít nhất 3 time-based puzzle pattern có ý nghĩa;
- combat có ít nhất 3 normal enemy archetype;
- room progression hoạt động;
- upgrade selection hoạt động;
- Chrono Guardian boss hoàn chỉnh;
- Victory và Game Over flow hoạt động;
- Android touch controls usable;
- UI phù hợp common landscape aspect ratio;
- có audio/VFX feedback;
- không compile error;
- không còn known game-breaking runtime error;
- full run có thể chơi từ Main Menu tới Victory;
- Restart và Replay hoạt động;
- Android build thành công;
- final manual playtest hoàn thành.

Mục tiêu cuối không phải là:

> "Tất cả feature đã tồn tại."

Mà là:

> **ChronoDungeon tạo cảm giác như một game hoàn chỉnh, có chủ đích từ lúc mở game cho tới Victory.**
