# Lộ trình phát triển

## M0: Khởi tạo (đang ở đây)

- [x] Cấu trúc repo, assembly, quy ước code
- [x] Bộ khung code: di chuyển, tiếng ồn, ẩn nấp, AI chó và chủ nhà, thành tích
- [ ] Tạo project Unity, cài package, tạo 3 file config
- [ ] Dựng màn greybox theo README và chơi thử được từ đầu đến cuối

## M1: Gameplay vui (prototype)

- [ ] Cân bằng thông số tiếng ồn, tầm nhìn, tốc độ cho tới khi một ván kéo dài 3 đến 5 phút
- [ ] Cho 3 đến 5 người chơi thử, ghi lại chỗ họ bối rối hoặc chán
- [ ] Thêm chỉ báo trên đầu AI (dấu ? khi nghi ngờ, dấu ! khi phát hiện)
- [ ] Âm thanh tạm: bước chân, chó sủa, chủ nhà la

## M2: Vertical slice

- [ ] Nhân vật và môi trường có đồ họa thật cho một màn hoàn chỉnh
- [ ] Animation đi, chạy, lom khom, bế chó
- [ ] Menu chính, menu tạm dừng, menu cài đặt (độ phân giải, âm lượng, đổi phím)
- [ ] Chuyển input sang file `.inputactions` để hỗ trợ đổi phím

## M3: Vai chủ nhà (chơi đơn)

- [ ] `OwnerMotor` cho người chơi điều khiển chủ nhà
- [ ] AI trộm (dùng lại `ICharacterInput` cho AI)
- [ ] Đồ nghề chủ nhà: camera, đèn cảm biến, chuông báo
- [ ] Thành tích số lần bắt được trộm

## M4: Đối kháng nhiều người

- [ ] Tích hợp Netcode for GameObjects, mô hình server quyết định
- [ ] Sảnh chờ, chọn vai, ghép trận
- [ ] Chế độ 1 chủ nhà đấu nhiều trộm

## M5: Phát hành

- [ ] Trang Steam "Coming Soon", trailer, bản demo cho Steam Next Fest
- [ ] Steam Achievements và Leaderboards
- [ ] Kiểm thử trên máy cấu hình yếu, tối ưu hiệu năng
