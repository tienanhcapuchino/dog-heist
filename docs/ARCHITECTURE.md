# Kiến trúc code

## Các assembly và chiều phụ thuộc

```
DogHeist.UI ─────► DogHeist.Gameplay ─────► DogHeist.Core
DogHeist.AI ─────► DogHeist.Gameplay
DogHeist.Tests ──► DogHeist.Core, DogHeist.Gameplay
```

- **Core**: logic thuần không cần scene (máy trạng thái, kết quả ván, luật thành tích, lưu file). Không biết gì về nhân vật hay AI.
- **Gameplay**: luật chơi và nhân vật người chơi (input, di chuyển, tiếng ồn, ẩn nấp, tương tác, điều phối ván). Không biết gì về AI hay UI.
- **AI**: chó và chủ nhà do máy điều khiển. Đọc trạng thái của Gameplay, báo sự kiện qua `MatchEvents`.
- **UI**: chỉ đọc dữ liệu và nghe sự kiện, không chứa luật chơi.

Quy tắc: một assembly không bao giờ tham chiếu ngược chiều mũi tên. Khi cần "báo ngược lên", dùng sự kiện hoặc interface.

## Các mẫu thiết kế chính

**Tách input khỏi nhân vật.** `ThiefMotor` và `ThiefInteractor` chỉ đọc `ICharacterInput`. Hiện tại `LocalPlayerInput` cài đặt interface này bằng bàn phím và tay cầm. Khi lên multiplayer chỉ cần thêm một lớp input nhận dữ liệu từ mạng; code nhân vật không đổi.

**Máy trạng thái cho AI.** `DogAI` và `OwnerAI` dùng `StateMachine` trong Core. Mỗi trạng thái là một class nhỏ, chuyển trạng thái rõ ràng, dễ thêm trạng thái mới (ví dụ chủ nhà gọi hàng xóm).

**Kênh tiếng ồn.** Ai phát tiếng thì gọi `NoiseSystem.Emit`; `HearingSensor` của AI tự lọc tiếng nghe được theo khoảng cách và độ thính tai. Người phát không cần biết ai đang nghe.

**Sự kiện trận đấu.** `MatchEvents` (bị phát hiện, bị bắt, trốn thoát, kết thúc ván) giúp AI, Gameplay và UI không phải tham chiếu trực tiếp lẫn nhau. `MatchManager` là nơi duy nhất quyết định ván kết thúc và ghi thành tích.

**Dữ liệu tách khỏi code.** Mọi thông số cân bằng nằm trong `ThiefConfig`, `DogConfig`, `OwnerConfig` (ScriptableObject).

**Lưu thành tích qua interface.** `StatsService` chỉ biết `IStatsStorage`. Bản đầu dùng `JsonFileStatsStorage`; sau này có thể thêm bản lưu lên Steam hoặc server mà không đổi luật tính điểm.

## Luồng chính

**Trộm gây tiếng ồn:** `ThiefMotor` gọi `NoiseSystem.Emit`, `HearingSensor` của chó nhận được, `DogAI` chuyển sang `Alert` và sủa. Tiếng sủa lại là một tiếng ồn lớn; `HearingSensor` của chủ nhà nghe thấy, `OwnerAI` thức dậy và chuyển sang `Investigate`.

**Trộm bị bắt:** `VisionSensor` thấy trộm, `OwnerAI` chuyển sang `Chase` và báo `ThiefSpotted`. Khi tới đủ gần, nó báo `ThiefCaught`; `MatchManager` kết thúc ván, ghi thành tích và báo `MatchEnded`; `ResultScreenUI` hiện kết quả.

**Trộm thành công:** chó ăn đồ dụ nên `AllowPickup` bật, trộm bấm E để bế, đi vào `EscapeZone`, vùng này báo `ThiefEscaped`, và `MatchManager` kết thúc ván.

## Kế hoạch mở rộng lên multiplayer

Dự kiến dùng **Netcode for GameObjects** (giải pháp chính thức của Unity), mô hình server có quyền quyết định:

| Thành phần hiện tại | Thay đổi khi lên multiplayer |
|---|---|
| `ICharacterInput` | Thêm lớp input nhận từ client; server chạy `ThiefMotor` theo input đó (sau có thể thêm dự đoán phía client) |
| `MatchManager` | Chỉ chạy trên server; kết quả gửi về client bằng RPC hoặc NetworkVariable |
| `MatchEvents`, `NoiseSystem` | Chỉ server phát sự kiện gameplay; client chỉ nhận để hiển thị |
| `ThiefVisibility _thief` (một trộm) trong AI | Thay bằng danh sách trộm đang có trong ván |
| `PlayerRole` | Đã có sẵn; mỗi người chơi được gán vai khi vào ván |
| Vai chủ nhà | Viết `OwnerMotor` dùng `ICharacterInput`, tách hành vi của `OwnerAI` để người hoặc máy đều điều khiển được |
| `JsonFileStatsStorage` | Giữ cho chơi đơn; chơi online dùng Steam Stats hoặc server |

Nguyên tắc cần giữ từ bây giờ để việc chuyển đổi nhẹ nhàng: không đọc input trực tiếp trong code gameplay, không để UI thay đổi trạng thái game, và mọi quyết định thắng thua đi qua `MatchManager`.
