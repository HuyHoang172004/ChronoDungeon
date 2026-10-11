# Plan sửa quái, chiến đấu và hiệu ứng kỹ năng

## Hiện trạng

- `EnemyFollow` chỉ đuổi thẳng khi `player` đã được gán và người chơi nằm trong `detectionRange`.
- Không có trạng thái tuần tra, mất dấu, quay về vị trí ban đầu hoặc né vật cản.
- `EnemyContactDamage` xử lý đánh cận chiến độc lập. Một enemy clone thiếu component này sẽ chỉ đi theo mà không đánh.
- Hướng đi hiện tại là đường thẳng nên quái có thể mắc collider hoặc cố đi xuyên tường.
- Hiệu ứng kỹ năng đang dùng nhiều vòng tròn/line và màu phát sáng lớn, thiếu hình dạng thể hiện hướng, tầm và thời điểm gây sát thương.

## Giai đoạn 1 — Chuẩn hóa prefab quái

Tạo một prefab `RuinHusk` duy nhất có đủ:

- `Rigidbody2D`, collider chân và `Health`.
- `EnemyBrain` điều khiển state machine.
- `EnemyMotor2D` chỉ chịu trách nhiệm di chuyển.
- `EnemyMeleeAttack` chịu trách nhiệm wind-up, hit frame và cooldown.
- `EnemyReadabilityPresentation` cho sprite và thanh máu.
- Target tự tìm `PlayerMovement` nếu Inspector chưa được gán.

Các enemy trong map phải được sinh từ prefab này, không clone ngẫu nhiên một object trong scene.

## Giai đoạn 2 — State machine AI

Luồng đề xuất:

`Idle -> Patrol -> Alert -> Chase -> AttackWindup -> AttackRecover`

Nhánh phụ:

- Mất dấu: `Chase -> Search -> Return -> Patrol`.
- Trúng đòn mạnh: `Any -> Stunned`.
- Hết máu: `Any -> Dead`.

Mỗi state chỉ điều khiển một hành vi, tránh vừa di chuyển vừa wind-up tấn công.

## Giai đoạn 3 — Tuần tra và tìm đường

- Mỗi quái có 2–5 `PatrolPoint` hoặc chọn điểm ngẫu nhiên trong vùng đi được.
- Dừng 0.5–1.5 giây tại mỗi điểm, sau đó chọn điểm tiếp theo.
- Dùng grid walkable tạo từ collision mask cho A* 2D.
- Cập nhật đường đi khoảng 4–6 lần/giây, không tính lại mỗi frame.
- Dùng raycast ngắn để tránh quái chồng lên nhau và tránh góc collider.
- Khi ra ngoài vùng chiến đấu, quái quay về spawn thay vì đuổi vô hạn.

## Giai đoạn 4 — Tấn công tự động

- Khi vào `attackRange`, quái dừng di chuyển và quay về phía người chơi.
- Hiện telegraph nhỏ dưới vùng đánh trong thời gian wind-up.
- Chỉ gây sát thương đúng hit frame; người chơi rời vùng trước hit frame sẽ né được.
- Sau đòn đánh có recovery rõ ràng để người chơi phản công.
- Cận chiến thường: wind-up 0.35–0.5 giây, recovery 0.45–0.7 giây.
- Elite có thể có đòn lao hoặc đập đất nhưng vẫn dùng cùng state machine.

## Giai đoạn 5 — Làm lại VFX kỹ năng

- Tách `Telegraph`, `Cast`, `Hit` và `Aftermath` thành bốn phần.
- Telegraph mảnh, ít sáng và thể hiện chính xác vùng nguy hiểm.
- Cast bám theo tay/vũ khí, không dùng vòng tròn lớn quanh chân.
- Hit chỉ xuất hiện tại điểm va chạm, có flash ngắn và mảnh vỡ theo hướng lực.
- Aftermath mờ nhanh trong 0.15–0.35 giây, tránh che nhân vật và map.
- Dùng object pool cho projectile, impact và trail.
- Quy định sorting layer: `GroundVFX < Actor < WeaponVFX < Foreground`.
- Màu nhận diện: cyan cho thời gian, tím cho void, cam cho lửa; không trộn cả ba trong một kỹ năng.

## Giai đoạn 6 — Kiểm thử

- Quái tuần tra ít nhất 20 giây mà không mắc tường.
- Phát hiện và đuổi người chơi đúng bán kính.
- Mất dấu rồi quay về patrol.
- Mỗi wind-up chỉ gây sát thương một lần.
- Không đánh xuyên collider hoặc ngoài tầm.
- 12–20 quái hoạt động đồng thời không tạo spike CPU đáng kể.
- VFX không che sprite, thanh máu hoặc mép walkable.

## Thứ tự triển khai

1. Prefab quái chuẩn và tự gán target.
2. State machine cùng đòn đánh cận chiến.
3. Patrol và return-to-spawn.
4. A* trên collision mask.
5. Làm lại VFX ba kỹ năng đại diện.
6. Cân bằng tốc độ, tầm phát hiện, sát thương và cooldown.
