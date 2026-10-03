# Checklist chơi thử Giai đoạn 0

Mở scene `Assets/_Project/Scenes/Level01_Neighborhood`, bấm Play. Ghi Đạt/Lỗi và ghi chú cho từng dòng.

## A. Điều khiển
- [ ] WASD đi, Shift chạy, giữ Ctrl lom khom; chuột xoay camera
- [ ] Thanh tiếng ồn góc dưới trái: lom khom gần như trống, đi bộ vừa, chạy gần đầy
- [ ] Chữ góc trên trái hiện "Đồ ăn dụ chó: 2" và "Trong bóng tối"

## B. Kết cục trốn thoát
- [ ] Lom khom chui qua lỗ hàng rào phía tây mà chó không sủa
- [ ] Ném đồ ăn (Q hoặc chuột trái) gần chó: chó chạy tới ăn
- [ ] Khi chó ăn xong, đứng gần thấy chữ "[E] Bế chó"; bấm E bế được
- [ ] Mang chó ra cổng phía nam vào ngõ: hiện "Trộm thành công!" hoặc "Trộm hoàn hảo..."
- [ ] Bấm "Chơi lại": màn tải lại, chơi tiếp được

## C. Kết cục bị bắt
- [ ] Chạy (Shift) gần chuồng chó: chó sủa
- [ ] Tiếng sủa đánh thức chủ nhà, chủ nhà đi về phía tiếng động
- [ ] Đứng trong vùng đèn hiên: chủ nhà thấy và đuổi
- [ ] Bị tóm: hiện "Bị bắt rồi!"

## D. Thành tích
- [ ] Sau mỗi ván, số liệu trên màn kết quả tăng đúng (thành công, hoàn hảo, chuỗi thắng, bị bắt)
- [ ] Tắt Play rồi bật lại: số liệu vẫn còn (file `player_stats.json` trong `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>`)

## E. Console
- [ ] Không có lỗi đỏ trong suốt các ván trên

## F. Phản hồi (G1a)
- [ ] Chủ nhà hiện `Zzz` → `?` → `!`; chó hiện `!` khi sủa và `♥` khi ăn
- [ ] Con mắt góc trên trái đổi khi vào bụi, vào tối, vào đèn
- [ ] Camera rung khi chủ nhà bắt đầu đuổi
- [ ] Nghe được các tiếng đã có file (xem `docs/audio-sources.md`)
- [ ] Chữ tiếng Việt hiện đúng dấu

## Nếu gặp vấn đề
- Chữ tiếng Việt hiện ô vuông: tạo font hỗ trợ tiếng Việt theo ghi chú cuối README.
- Chó sủa mà chủ nhà không dậy: tăng `BarkNoiseRadius` trong `DogConfig` (ví dụ 20), không sửa code.
- Chuột không xoay camera: chọn `FreeLook Camera`, ở component *Cinemachine Input Axis Controller* bấm menu ba chấm > *Reset*.
- Nhân vật AI đứng im: chọn `[Greybox]/Environment`, ở *NavMeshSurface* bấm *Bake*, hoặc chạy lại menu Build Greybox Level.

## Kết quả lần chơi thử đầu tiên (2026-10-02)

Người dùng đánh giá **tạm đạt**. Log Editor không có lỗi đỏ; `player_stats.json` ghi nhận một lần bị bắt (kết cục bị bắt chạy trọn vòng).

Vấn đề đã biết, để xử lý ở giai đoạn sau:
- Nhân vật vẫn là khối capsule, chưa có model và animation (M2; nguồn đề xuất: Quaternius CC0, Mixamo).
- *Company Name* và *Product Name* trong Player Settings còn là giá trị của project mẫu (`DefaultCompany`, `DogHeistTemplate`), nên thành tích lưu vào thư mục mang tên đó. Đổi trước khi có người chơi thật.
- Font TMP mặc định (LiberationSans) phải vẽ thêm ký tự tiếng Việt vào atlas fallback lúc chạy; nên thay bằng font hỗ trợ tiếng Việt.

## Kết quả chơi thử phần F (2026-10-03)

Người dùng xác nhận **đạt cả 5 dòng** phần F. Log Editor không có exception của game; chỉ cảnh báo `[Audio]` cho `OwnerHuh` và `OwnerShout` (chưa có file), mỗi cue một lần. Thành tích lưu ở thư mục mới `LocalLow\tienanhcapuchino\Dog Heist\`.

Đã xử lý từ lần trước: *Company Name*/*Product Name* đã đổi; font Be Vietnam Pro thay cho font mặc định.

Còn lại: giọng chủ nhà chưa có file; tiếng chó vui (`DogHappy`) đang tạm dùng tiếng rên dài.
