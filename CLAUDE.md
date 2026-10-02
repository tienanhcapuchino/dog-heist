# CLAUDE.md — Dog Heist

File này tóm tắt bối cảnh từ cuộc trò chuyện trên claude.ai, nơi dự án được lên ý tưởng và tạo bộ khung code. Claude Code tự đọc file này mỗi khi khởi động trong thư mục project.

## Giao tiếp

- Luôn trả lời người dùng bằng **tiếng Việt**.
- Comment trong code và tài liệu viết bằng tiếng Việt; tên class, method, biến viết bằng tiếng Anh.
- Người dùng đang học Unity: giải thích ngắn gọn lý do khi đưa ra lựa chọn kỹ thuật.

## Dự án

Game lén lút 3D góc nhìn thứ ba cho PC, Unity 6.3 LTS, C#, URP. Người chơi vào vai **trộm chó**: lẻn vào sân, ném đồ ăn dụ chó, bế chó ra khỏi cổng mà không bị chủ nhà bắt. Giọng điệu hài hước, không bạo lực, chó không bao giờ bị làm hại.

Các quyết định đã chốt:

- Bản đầu tiên: **chơi đơn, chỉ vai trộm**, đấu với chủ nhà và chó do AI điều khiển.
- Sau này: thêm vai chủ nhà, rồi đối kháng nhiều người (dự kiến Netcode for GameObjects, server có quyền quyết định).
- Thành tích: số lần trộm thành công không bị bắt (chính), trộm hoàn hảo, chuỗi thắng, số lần bị bắt; lưu JSON trong `persistentDataPath`.
- Phát hành mục tiêu: Steam.

Chi tiết:
- Thiết kế game: @docs/GDD.md
- Kiến trúc và kế hoạch multiplayer: @docs/ARCHITECTURE.md
- Lộ trình: @docs/ROADMAP.md
- Hướng dẫn cài đặt và dựng màn greybox: @README.md

## Quy tắc kiến trúc (bắt buộc giữ)

- Chiều phụ thuộc assembly: `UI → Gameplay → Core`, `AI → Gameplay`. Không bao giờ tham chiếu ngược; cần báo ngược thì dùng sự kiện hoặc interface.
- Code nhân vật chỉ đọc điều khiển qua `ICharacterInput`, không đọc bàn phím trực tiếp.
- Mọi quyết định thắng thua đi qua `MatchManager`; UI không được thay đổi trạng thái game.
- Thông số cân bằng đặt trong ScriptableObject config (`ThiefConfig`, `DogConfig`, `OwnerConfig`), không viết cứng.
- Logic không cần scene đặt trong Core và có EditMode test.
- Event tĩnh (`MatchEvents`, `NoiseSystem`, registry của `FoodLure`) phải có hàm reset gắn `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`.

## Quy ước code

- Chuẩn C# .NET: ngoặc nhọn xuống dòng (Allman), `PascalCase` cho type/method/property, `_camelCase` cho field private, `s_camelCase` cho field private static, interface bắt đầu bằng `I`.
- `[SerializeField] private` thay cho field public; config dùng `[field: SerializeField]` cho auto-property.
- Không nuốt lỗi bằng `catch {}` rỗng; bắt đúng loại exception và log có tiền tố (ví dụ `[Stats]`).
- Không dùng `GameObject.Find`; tham chiếu kéo thả trong Inspector.
- Unity hỗ trợ tối đa **C# 9**: không dùng cú pháp mới hơn (file-scoped namespace, record struct, raw string...).
- Khi có `using System;` thì gọi `UnityEngine.Random` đầy đủ để tránh trùng tên.

## Trạng thái hiện tại

- Đã xong: bộ khung code đầy đủ (di chuyển trộm, tiếng ồn, vùng sáng và chỗ nấp, đồ ăn dụ chó, AI chó Idle/Alert/EatLure/Calm/Carried, AI chủ nhà Sleeping/Investigate/Patrol/Chase, MatchManager, thành tích, HUD, màn kết quả, EditMode test).
- Code đã biên dịch thử ngoài Unity bằng .NET SDK với stub API Unity (0 lỗi) và test logic đạt, **nhưng chưa chạy trong Unity thật**.
- Chưa có scene, prefab, file config (phải tạo trong Unity Editor theo README).

## Việc tiếp theo được đề xuất

1. Sửa lỗi biên dịch nếu có khi mở project lần đầu trong Unity.
2. Viết Editor tool `DogHeist > Tools > Build Greybox Level` (assembly `DogHeist.Editor`, chỉ chạy trong Editor) tự dựng màn greybox thay cho 18 bước làm tay trong README.
3. Cân bằng thông số cho ván chơi dài khoảng 3 đến 5 phút.
4. Thêm chỉ báo "?" và "!" trên đầu AI.

## Kiểm tra

- Claude Code không mở được Unity Editor; người dùng chạy test qua *Window > General > Test Runner > EditMode*.
- Sau khi sửa code, nhắc người dùng quay lại Unity xem Console có lỗi không và dán lỗi vào để xử lý tiếp.
