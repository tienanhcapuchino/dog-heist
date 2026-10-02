# G1a: Phản hồi cho người chơi — Thiết kế

Ngày: 2026-10-03. Thuộc Giai đoạn 1 (M1: Gameplay vui), phần đầu tiên trong bốn phần G1a → G1c → G1b → G1d.

## Mục đích

M1 hướng tới một ván greybox dài 3–5 phút, căng thẳng và buồn cười, người lạ chơi hiểu ngay không cần giải thích. Điều kiện hoàn thành M1: đa số người thử muốn chơi thêm ván nữa và tự tìm ra cách dụ chó bằng đồ ăn.

G1a làm trước vì thiếu phản hồi thì cân bằng (G1c) chỉ là mò mẫm: người chơi không biết chó và chủ nhà đang nghi ngờ hay đã phát hiện, cũng không biết mình đang lộ tới đâu.

## Đã chốt

- Dấu trên đầu AI theo phương án B: `Zzz`, `?`, `!`, `♥` (có thêm dấu cho trạng thái an toàn).
- Âm thanh theo phương án A: file CC0 người dùng tự tải; code đi qua ScriptableObject `AudioCue`.
- Kiến trúc theo cách 1: interface `IAwarenessSource` trong Gameplay; UI và âm thanh chỉ đọc interface và sự kiện.
- *Company Name* = `tienanhcapuchino`, *Product Name* = `Dog Heist`.

## Ràng buộc

- Chiều phụ thuộc: `UI → Gameplay → Core`, `AI → Gameplay`, `Editor → tất cả`. UI không tham chiếu AI.
- Logic chuyển trạng thái của AI giữ nguyên; chỉ thêm khai báo mức cảnh giác.
- Thông số cân bằng chỉ nằm trong `ThiefConfig`, `DogConfig`, `OwnerConfig`. Thông số trình bày (màu, độ mạnh rung, chữ) nằm trong ScriptableObject trình bày hoặc `[SerializeField]` trên component.
- C# 9, quy ước code của `CLAUDE.md`, log có tiền tố (`[Awareness]`, `[Audio]`, `[Greybox]`).
- Công cụ `DogHeist > Tools > Build Greybox Level` dựng được toàn bộ phần mới; chạy lại không ghi đè asset người dùng đã chỉnh.

## 1. Mức cảnh giác và dấu trên đầu AI

**Gameplay** (`Gameplay/Awareness/`):
- `enum AwarenessLevel { None, Sleeping, Suspicious, Alerted, Friendly }`
- `interface IAwarenessSource`:
  - `AwarenessLevel Awareness { get; }`
  - `event Action<AwarenessLevel> AwarenessChanged`
  - `Transform IndicatorAnchor { get; }` (điểm trên đầu để đặt dấu)

**AI:** `DogState` và `OwnerState` có thêm thuộc tính trừu tượng `AwarenessLevel Awareness`. Mỗi trạng thái khai báo:

| AI | Trạng thái | Mức | Dấu |
|---|---|---|---|
| Chủ nhà | Sleeping | `Sleeping` | `Zzz` (xám) |
| Chủ nhà | Investigate, Patrol | `Suspicious` | `?` (vàng) |
| Chủ nhà | Chase | `Alerted` | `!` (đỏ) |
| Chó | Idle | `None` | (không hiện) |
| Chó | Alert | `Alerted` | `!` (đỏ) |
| Chó | EatLure, Calm, Carried | `Friendly` | `♥` (hồng) |

`DogAI` và `OwnerAI` cài `IAwarenessSource`. Chúng nghe `StateMachine.StateChanged` (có sẵn trong Core) và chỉ phát `AwarenessChanged` khi mức thật sự đổi (Investigate → Patrol không phát lại).

**UI:** `AwarenessIndicatorUI` là chữ TextMeshPro 3D đặt tại `IndicatorAnchor`, luôn quay mặt về camera, đổi chữ và màu theo mức, "nảy" (phóng to rồi về cỡ cũ khoảng 0,2 giây) khi mức đổi. Chữ và màu từng mức nằm trong ScriptableObject `AwarenessIndicatorStyle`. Field tham chiếu nguồn có kiểu `MonoBehaviour` (Unity không serialize interface); lúc chạy, nếu component không cài `IAwarenessSource` thì log lỗi `[Awareness]` và tự tắt.

## 2. Con mắt trên HUD và rung camera

**`VisibilityEyeUI`** (UI) thay dòng chữ lộ/ẩn ở góc trên trái, chỉ đọc `ThiefVisibility`:
- Độ mở của mắt theo `Visibility01`: nấp trong bụi chỉ còn một khe, trong tối mở hé, dưới đèn (`IsInLight`) mở to và đổi màu vàng.
- Ghép từ sprite tròn có sẵn của Unity (`Knob`): lòng trắng là hình tròn kéo dẹt, con ngươi là hình tròn nhỏ màu tối; độ mở là độ cao lòng trắng. Không cần đồ họa riêng.
- Giữ chữ ngắn bên cạnh: "Đang nấp", "Trong tối", "Bị chiếu sáng".
- Độ mở tối thiểu, tối đa và màu là thông số trình bày.
- Hàm tính độ mở từ `Visibility01` là hàm thuần, có EditMode test.

**`SpottedCameraShake`** (UI) nghe `MatchEvents.ThiefSpotted` và phát một cú rung ngắn qua `CinemachineImpulseSource`; FreeLook Camera có `CinemachineImpulseListener`. Chỉ rung khi chủ nhà phát hiện, không rung khi chó sủa. `DogHeist.UI.asmdef` thêm tham chiếu `Unity.Cinemachine`. Độ mạnh và thời gian rung là `[SerializeField]` trên component.

## 3. Âm thanh

**Dữ liệu** (`Gameplay/Audio/`):
- `AudioCue` (ScriptableObject): danh sách clip (chọn ngẫu nhiên, không lặp lại clip vừa phát), khoảng âm lượng, khoảng pitch, chế độ 2D/3D, khoảng cách nghe tối đa. Cue không có clip thì trả về `null`, không ném lỗi.
- `SoundLibrary` (ScriptableObject): tham chiếu tới mọi `AudioCue`.

**Bảng tiếng:**

| Cue | Khi nào | 2D/3D |
|---|---|---|
| `FootstepCrouch`, `FootstepWalk`, `FootstepSprint` | Trộm phát tiếng ồn bước chân; chọn theo `ThiefMotor` (lom khom / chạy / đi) | 3D |
| `DogBark` | Chó phát tiếng ồn | 3D |
| `DogHappy` (nhai hoặc rên vui) | Chó chuyển từ mức khác sang `Friendly` | 3D |
| `OwnerHuh` ("Hửm?") | Chủ nhà từ `Sleeping` sang `Suspicious` | 3D |
| `OwnerShout` ("Trộm!") | Chủ nhà sang `Alerted` | 3D |
| `LureLand` | Đồ ăn va chạm lần đầu | 3D |
| `StingerWin`, `StingerLose` | Ván kết thúc (thoát / bị bắt) | 2D |

**Người phát** (đều trong Gameplay, không sửa logic gameplay hay AI):
- `NoiseSoundPlayer` nghe `NoiseSystem.NoiseEmitted`: tiếng của trộm (`NoiseSource.Thief`) thì đọc `ThiefMotor` để chọn cue bước chân; tiếng của chó (`NoiseSource.Dog`) thì phát `DogBark`.
- `AwarenessSoundPlayer` gắn trên chó và chủ nhà, nghe `AwarenessChanged`, chọn cue theo cặp (mức cũ, mức mới). Luật chọn là hàm thuần, có test.
- `ImpactSound` gắn trên prefab đồ ăn, phát `LureLand` ở lần va chạm đầu tiên.
- `MatchStingerPlayer` nghe `MatchEvents.MatchEnded`, phát `StingerWin` hoặc `StingerLose`.

**File:** người dùng tải file CC0 (Kenney, Freesound lọc CC0) theo danh sách link Claude đưa trong kế hoạch, bỏ vào `Assets/_Project/Audio/SFX/`. Công cụ greybox tạo `SoundLibrary` và các `AudioCue` rỗng nếu chưa có, không ghi đè cue đã có clip. Cue rỗng thì im lặng và log một cảnh báo `[Audio]` cho mỗi cue (một lần).

## 4. Font tiếng Việt và tên game

- Người dùng tải **Be Vietnam Pro** (Google Fonts, giấy phép OFL) vào `Assets/_Project/Art/Fonts/`.
- Công cụ greybox tạo TMP font asset dạng động từ file đó và lưu thành asset; LiberationSans là font dự phòng (cho ký tự như `♥`). HUD, màn kết quả và dấu trên đầu AI dùng font này.
- Chưa có file font: công cụ cảnh báo `[Greybox]` và dùng font mặc định, không dừng.
- `ProjectSettings`: *Company Name* = `tienanhcapuchino`, *Product Name* = `Dog Heist`. Thư mục lưu thành tích đổi theo; số liệu thử nghiệm cũ bỏ lại.

## Kiểm thử

**EditMode (Claude tự chạy bằng `tools/run-unity-tests.ps1`):**
- Bảng trạng thái → mức cảnh giác của chó và chủ nhà.
- `AwarenessChanged` chỉ phát khi mức đổi.
- `AwarenessIndicatorStyle` trả đúng chữ và màu theo mức.
- Hàm tính độ mở mắt từ `Visibility01`.
- `AudioCue`: không lặp clip vừa phát, pitch và âm lượng trong khoảng, cue rỗng trả `null`.
- Chọn cue bước chân theo trạng thái di chuyển; chọn cue của chủ nhà và chó theo cặp mức.
- Công cụ greybox: gắn dấu trên đầu và nối nguồn; tạo con mắt; gắn Impulse Source và Listener; tạo `SoundLibrary` và cue rỗng, không ghi đè cue có sẵn; dùng font tiếng Việt khi có, cảnh báo khi không có.
- Toàn bộ test cũ vẫn đạt.

**Chơi thử (người dùng):** thêm phần **"F. Phản hồi"** vào checklist chơi thử:
- Chủ nhà hiện `Zzz` → `?` → `!`; chó hiện `!` khi sủa và `♥` khi ăn.
- Con mắt đổi khi vào bụi, vào tối, vào đèn.
- Camera rung khi chủ nhà bắt đầu đuổi.
- Nghe được các tiếng đã có file.
- Chữ tiếng Việt hiện đúng dấu.

## Hoàn thành khi

1. Toàn bộ EditMode test đạt.
2. Chạy lại menu *Build Greybox Level* dựng được màn có đủ phần mới.
3. Người dùng chơi thử phần F và xác nhận đạt.

## Ngoài phạm vi

Thanh nghi ngờ tăng dần (phương án C), nhạc nền, Audio Mixer và menu chỉnh âm lượng (M2), đồ họa nhân vật (M2), cân bằng thông số (G1c), tính năng chiều sâu (G1b), bản build và thử chơi với người khác (G1d).
