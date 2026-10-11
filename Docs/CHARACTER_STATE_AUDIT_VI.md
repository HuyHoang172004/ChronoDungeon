# Rà soát state nhân vật

## Cách pipeline chọn asset

Mỗi nhân vật được đọc tất cả sprite sheet có hậu tố `_4Dir_4Frame_` trong thư mục của nó. Nếu cùng một action có cả sprite mới và cũ, sheet trong `TopDownV2` được ưu tiên; nếu chưa có sheet mới, sheet legacy ở thư mục gốc được dùng trực tiếp. Vì vậy nhân vật chưa vẽ lại vẫn có controller và animation hoạt động, không bị thay bằng ảnh trống.

## Kết quả rà soát

- 4 nhân vật người chơi: có đủ Idle, Move, phản ứng trúng đòn, Death và kỹ năng tấn công.
- 20 quái thường: có Idle, Move, Death, phản ứng trúng đòn và ít nhất một action tấn công. Một số dùng tên kỹ năng riêng thay cho `Attack`, ví dụ `HeavyAttack`, `FireBolt`, `TwoCoreBind`, `PrismShot` và `MirrorBolt`; pipeline nhận các action này là tấn công.
- 8 boss: có Idle, Move, Death và kỹ năng chủ đạo. ChronoGuardian dùng `Stagger` thay cho `Hit`; pipeline dùng Stagger làm phản ứng trúng đòn. Đây là hợp lệ, không phải thiếu state.
- 6 NPC: không cần Attack/Death như quái. Chúng được kiểm tra theo state trình bày/tương tác: Idle, IdleTalk, Chained, Freed, Talk hoặc Interact.

## Các trường hợp đặc biệt đã xử lý

- `MirrorWisp` không có state Move độc lập, nhưng có `MirrorBlink`. Khi AI cần Move/Patrol/Chase, controller dùng MirrorBlink trước rồi mới rơi về Idle.
- `ChronoGuardian` không có Hit riêng; Hit/Stagger/Knockback sẽ dùng Stagger.
- `EchoBinder` dùng `TwoCoreBind`; action này đã được nhận là đòn tấn công và sẽ có animation impact.
- Các action bắn như Shot, Bolt, Volley và Throw có thêm mốc `OnAnimationCastRelease`, để projectile/VFX được gọi ở giữa animation thay vì xuất hiện ngay khi bấm.

## Điều còn cần kiểm tra trong Unity

Rà soát trên chỉ xác nhận source asset, controller và fallback. Cần bấm `Tools > ChronoDungeon > Animation > Rebuild All Character Animators`, sau đó `Validate Character Animators`, rồi vào Play Mode kiểm tra thực tế hướng quay, hitbox, timing và animator parameter của từng prefab. Việc build C# ngoài Unity không thay thế được bước này vì Unity mới import sprite, tạo sub-sprite và gắn controller.
