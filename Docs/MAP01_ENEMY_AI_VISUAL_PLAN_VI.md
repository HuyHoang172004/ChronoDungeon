# Kế hoạch tiếp theo: AI quái và nâng độ nét Map 1

## Mục tiêu

Hoàn thiện Map 1 theo ba hướng: quái chủ động nhưng dễ đọc, tuần tra hợp lý trên mặt sàn, và hình ảnh rõ nét trên PC lẫn Android.

## Giai đoạn 1 — Chuẩn hóa chiến đấu hiện tại

- Giữ Player root ở scale 1 để tầm đánh dùng đúng world unit.
- Chỉ scale `PlayerVisual` và body collider; không scale `AttackResolver`, kỹ năng hoặc projectile range.
- Chuẩn hóa quái thường Map 1 ở 180 HP, miniboss dùng cấu hình riêng.
- Kiểm tra hit bằng gizmo cho đánh tay không, Chrono Blade và kỹ năng.
- Tách layer `Player`, `Enemy`, `WorldCollision`, `Pickup` để physics query không đánh nhầm mục tiêu.

Tiêu chí hoàn thành: hình chém, vùng gây damage và khoảng cách người chơi cảm nhận trùng nhau.

## Giai đoạn 2 — Quái tự động phát hiện và tấn công

- State machine tối thiểu: `Idle -> Patrol -> Alert -> Chase -> Attack -> Recover -> Return`.
- `detectionRange` chỉ chuyển sang Alert; quái cần line-of-sight trước khi Chase.
- Quái cận chiến dừng ngoài body collider của Player, quay đúng hướng rồi mới đánh.
- Attack có wind-up, hit frame và recovery; không gây damage ngay khi vừa chạm Player.
- Dùng cooldown riêng, giới hạn số quái cùng áp sát để tránh tất cả chồng lên nhau.
- Khi mất mục tiêu, quái tìm vị trí cuối cùng rồi quay về vùng tuần tra.

Tiêu chí hoàn thành: người chơi nhìn được tín hiệu, có thời gian né và không bị damage vô hình.

## Giai đoạn 3 — Tuần tra và tìm đường

- Bake NavMesh 2D từ chính alpha mặt sàn, co biên vào trong theo bán kính quái.
- Mỗi nhóm quái có `PatrolRoute` gồm 2–5 waypoint nằm trên NavMesh.
- Chọn waypoint tuần tự hoặc ngẫu nhiên có kiểm soát; chờ ngắn tại mỗi điểm.
- Khi Chase, cập nhật đường theo chu kỳ thay vì mỗi frame.
- Nếu không tìm được đường, quái quay về waypoint gần nhất, không đi thẳng xuyên vực.
- Dùng local avoidance/separation để quái không xếp thành một cục.

Tiêu chí hoàn thành: quái không xuyên tường, không rơi khỏi sàn và không mắc ở góc collider.

## Giai đoạn 4 — Thiết kế encounter Map 1

- Awakening Chamber: không có quái.
- Fallen Passage: 2 quái, dạy đánh tay không.
- Courtyard: 4–5 quái chia hai đợt.
- Broken Armory: 2 quái bảo vệ Chrono Blade/Rune Key.
- Rune Hall: 3 quái kết hợp cận chiến và đánh xa.
- Gatebreaker Arena: khóa cửa, miniboss cùng tối đa 2 lính phụ.
- Không để toàn bộ 12 quái cùng nhận biết Player từ đầu map.

Tiêu chí hoàn thành: nhịp độ tăng dần và mỗi encounter có khoảng nghỉ.

## Giai đoạn 5 — Làm game nét hơn

- Chốt một PPU thống nhất cho map và nhân vật; không dùng root scale để chữa kích thước asset.
- Tắt texture compression cho sprite chính; Android dùng ASTC chất lượng phù hợp.
- Không dùng mipmap cho sprite 2D và UI; kiểm tra bilinear/point theo phong cách art.
- Dùng Pixel Perfect Camera nếu asset hướng pixel-art; nếu là hand-painted thì giữ bilinear và camera ở vị trí ổn định theo pixel.
- Tránh phóng sprite vượt quá độ phân giải nguồn; tạo atlas 2K/4K phù hợp thay vì scale lớn trong scene.
- Kiểm tra URP 2D Renderer, render scale = 1, MSAA phù hợp và không để Dynamic Resolution làm mờ.
- Tách nền thung lũng, mặt sàn, foreground và VFX thành sorting layer rõ ràng.
- Test ở đúng độ phân giải landscape mục tiêu, đặc biệt 1920×1080 và màn Android phổ biến.

Tiêu chí hoàn thành: cạnh sprite không nhòe, UI không rung và hình giữ chất lượng khi camera di chuyển.

## Thứ tự triển khai đề xuất

1. Sửa và trực quan hóa hit range.
2. Thêm state machine đánh tự động.
3. Thêm NavMesh 2D và patrol route.
4. Chia encounter theo khu vực.
5. Tối ưu import texture, camera và URP.
6. Playtest toàn bộ Map 1 rồi mới nhân hệ thống sang Map 2.
