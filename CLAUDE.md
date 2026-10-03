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

- Chiều phụ thuộc assembly: `UI → Gameplay → Core`, `AI → Gameplay`, `Editor → tất cả`. Không bao giờ tham chiếu ngược; cần báo ngược thì dùng sự kiện hoặc interface. Không assembly runtime nào được tham chiếu `DogHeist.Editor` (namespace `DogHeist.EditorTools`).
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

- **Giai đoạn 0 xong (M0):** project Unity 6.3 LTS (6000.3.25f1, URP) chạy được; code biên dịch 0 lỗi trong Unity thật; 48 EditMode test đạt.
- Bộ khung code đầy đủ: di chuyển trộm, tiếng ồn, vùng sáng và chỗ nấp, đồ ăn dụ chó, AI chó Idle/Alert/EatLure/Calm/Carried, AI chủ nhà Sleeping/Investigate/Patrol/Chase, MatchManager, thành tích, HUD, màn kết quả.
- Công cụ `DogHeist > Tools > Build Greybox Level` (assembly `DogHeist.Editor`) dựng scene `Assets/_Project/Scenes/Level01_Neighborhood`, config, prefab, material, NavMesh, camera, HUD. Bố cục lấy từ `GreyboxLayout` (đo theo `ban_do_man_choi_trom_cho.png`).
- Đã chơi thử theo `docs/testing/M0-smoke-test.md`: tạm đạt, không lỗi đỏ. Nhân vật vẫn là khối capsule.
- **G1a xong (phản hồi cho người chơi, phần đầu của M1):** 88 EditMode test đạt; người dùng chơi thử phần F của checklist và xác nhận đạt.
  - Mức cảnh giác: `AwarenessLevel`, `IAwarenessSource` (Gameplay), `DogAI`/`OwnerAI` cài interface; dấu `Zzz`/`?`/`!`/`♥` trên đầu AI (`AwarenessIndicatorUI`, chữ và màu trong `AwarenessIndicatorStyle`).
  - HUD: con mắt lộ/ẩn (`VisibilityEyeUI`); rung camera khi chủ nhà phát hiện (`SpottedCameraShake`, Cinemachine Impulse).
  - Âm thanh tạm: `AudioCue`, `SoundLibrary` (10 cue trong `Assets/_Project/Audio/Cues/`), người phát tiếng trong `Gameplay/Audio`. File CC0 trong `Audio/SFX/`, nguồn ghi ở `docs/audio-sources.md`; `OwnerHuh`, `OwnerShout` còn trống.
  - Font Be Vietnam Pro (OFL) dạng động, LiberationSans dự phòng. *Company Name* = `tienanhcapuchino`, *Product Name* = `Dog Heist`.
- Mỗi task làm trên nhánh riêng `feature/<mô-tả-ngắn>` tạo từ `main`.

## Việc tiếp theo được đề xuất

1. G1c (M1): cân bằng thông số trong config cho ván chơi 3 đến 5 phút.
2. Thêm giọng chủ nhà ("Hửm?", "Trộm!") và thay tiếng chó vui tạm (`DogHappy`).
3. G1b (chiều sâu gameplay), G1d (chơi thử với 3 đến 5 người).
4. Đồ họa nhân vật để M2 (nguồn đề xuất: Quaternius CC0 cho người và chó, Mixamo cho lom khom và bế đồ).

## Kiểm tra

- Claude Code tự kiểm tra bằng `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1` (thêm `-CompileOnly` để chỉ biên dịch, `-Filter <tên>` để chạy một nhóm test). **Unity Editor phải đang đóng**; script trả mã 3 nếu Unity đang mở project. Nên đóng cả Visual Studio: khi nó mở `dog-heist.sln`, test greybox có thể lỗi chập chờn "Cannot open file ... .meta for write". Đường dẫn Unity.exe lấy từ biến môi trường `UNITY_EDITOR`.
- Việc cần nhìn tận mắt (chạy menu, bấm Play) thì nhờ người dùng làm trong Unity và gửi Console hoặc ảnh chụp.
- Không tự ý commit; làm xong thì hỏi người dùng trước khi commit, push hay tạo PR.
