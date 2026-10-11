# Modular Map Production Specification

## Mục tiêu

Map 02–08 được vẽ mới thành các phòng/module độc lập có độ phân giải cao. Không cắt,
phóng to hoặc tái sử dụng master composition cũ làm hình ảnh runtime.

## Hợp đồng hình ảnh

- Canvas module: hình vuông, nền alpha trong suốt.
- Góc nhìn: 3/4 cố định, đồng nhất với `CD_Map01_Modular3Q_Master_v01.png`.
- Tỉ lệ kiến trúc, nhân vật và texture phải giống nhau giữa mọi module.
- Hướng sáng chính và độ cao tường phải đồng nhất trong một map.
- Props lớn nằm sát tường; phần giữa phòng phải đủ rộng cho player, enemy và combat VFX.
- Không vẽ nhân vật, enemy, UI, chữ, vòng chọn hoặc nền đen vào module.

## Hợp đồng đầu nối

- Ký hiệu connector: `N`, `E`, `S`, `W`.
- Connector luôn nằm đúng tâm cạnh canvas tương ứng.
- Tất cả connector trong cùng bộ map có cùng bề rộng sàn, cao độ, độ dày tường và nhịp cột.
- Sàn connector chạm chính xác biên canvas, không có alpha gap, props hoặc VFX chắn miệng nối.
- Hai module chỉ được ghép khi hai connector đối diện có cùng chuẩn, ví dụ `E` nối `W`.
- Phòng kết thúc/boss chỉ có connector vào; không tạo connector ra thừa.

## Các lớp runtime

Mỗi module cuối cùng có các lớp cùng kích thước và pivot:

1. `Background`: vực/thung lũng/fog phía dưới, không tham gia collision.
2. `Ground`: mặt sàn có thể nhìn thấy; ảnh mới độ phân giải cao.
3. `WalkableMask`: trắng = được đi, alpha/đen = cấm; lùi khỏi tường dày đủ bán kính actor.
4. `Foreground`: tường/cột cao cần che player; sorting theo trục Y.
5. `Props`: vật cản và trang trí rời.
6. `Interactives`: cửa, rương, cơ chế level.

`WalkableMask` là nguồn duy nhất để sinh `EdgeCollider2D` và navigation grid. Không dò
collider từ màu của ảnh Ground.

## Map 02 hiện tại

- `CD_Map02_Room01_Entrance_WE_v02.png`: phòng vào, connector `W E`.
- `CD_Map02_Room02_Command_WES_v02.png`: phòng chỉ huy, connector `W E S`.
- `CD_Map02_Room03_Boss_N_v02.png`: đấu trường boss, chỉ có connector vào `N`.

Các module tiếp theo phải lấy hai file này làm reference trực tiếp để giữ đúng góc nhìn,
tỉ lệ và hình học connector.

