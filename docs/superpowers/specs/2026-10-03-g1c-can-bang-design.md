# G1c: Cân bằng thông số — Thiết kế

Ngày: 2026-10-03. Thuộc Giai đoạn 1 (M1: Gameplay vui), phần thứ hai trong bốn phần G1a → G1c → G1b → G1d.

## Mục đích

M1 hướng tới một ván greybox dài 3–5 phút, căng thẳng và buồn cười. G1a đã cho người chơi đọc được tình huống. G1c chỉnh thông số trong config cho tới khi ván chơi đạt nhịp đó.

Hiện chưa có dữ liệu: người dùng chơi 3 ván, bị bắt cả 3, nhưng không biết diễn biến. Vì vậy G1c gồm hai nửa: **đo** (nhật ký ván chơi), rồi **chỉnh theo số đo** qua vài vòng.

## Đã chốt

- Cách đo: nhật ký ván chơi ghi file (cách 1). Không làm lớp hiển thị gỡ lỗi, không làm AI trộm tự chơi.
- Gom các thông số cân bằng còn nằm ngoài config vào config; thêm đồng hồ ván trên HUD.
- Mỗi vòng chỉnh tối đa 3 thông số, người dùng chơi 5 ván mỗi vòng.

## Ràng buộc

- Chiều phụ thuộc: `UI → Gameplay → Core`, `AI → Gameplay`, `Editor → tất cả`. Gameplay không tham chiếu AI; UI không tham chiếu AI.
- Không sửa logic chuyển trạng thái của AI hay luật chơi. Ngoại lệ duy nhất: cảm biến nhận giá trị từ config, `FoodLure.FindNearestUnclaimed` nhận khoảng cách tối đa, `ThiefInteractor` phát sự kiện khi ném đồ ăn.
- Thông số cân bằng chỉ nằm trong `ThiefConfig`, `DogConfig`, `OwnerConfig`.
- C# 9, quy ước code của `CLAUDE.md`, log có tiền tố `[MatchLog]`.
- Ghi nhật ký lỗi không bao giờ làm hỏng ván chơi.
- Công cụ `DogHeist > Tools > Build Greybox Level` dựng được toàn bộ phần mới; không ghi đè config đã có.

## 1. Nhật ký ván chơi: dữ liệu

Mỗi ván kết thúc ghi **một dòng JSON** vào `match_log.jsonl` trong `Application.persistentDataPath` (hiện là `%USERPROFILE%\AppData\LocalLow\tienanhcapuchino\Dog Heist\`).

**Thông tin chung:** `endedAtUtc` (chuỗi ISO 8601), `outcome` (`ThiefEscaped` / `ThiefCaught`), `durationSeconds`, `configHash`.

**Mốc thời gian lần đầu** (giây kể từ đầu ván; `-1` nếu không xảy ra):

| Trường | Nguồn |
|---|---|
| `firstBarkSeconds` | `NoiseSystem.NoiseEmitted` với `NoiseSource.Dog` |
| `firstOwnerWakeSeconds` | `AwarenessChanged` của chủ nhà, mức rời `Sleeping` |
| `firstSpottedSeconds` | `MatchEvents.ThiefSpotted` |
| `firstLureThrownSeconds` | `ThiefInteractor.LureThrown` (sự kiện mới) |
| `firstDogLuredSeconds` | `AwarenessChanged` của chó, mức sang `Friendly` |
| `firstPickupSeconds` | `CarryableDog.PickedUp` |

**Bộ đếm:** `barkCount`, `ownerWakeCount`, `spottedCount`, `luresThrown`, `dogDropCount`.

**Thời gian theo trạng thái của trộm** (tổng giây, cộng mỗi khung hình): `secondsInLight` (`ThiefVisibility.IsInLight`), `secondsHidden` (`IsHidden`), `secondsCrouching` (`ThiefMotor.IsCrouching`), `secondsSprinting` (`IsSprinting`).

`ownerWakeCount` đếm mỗi lần mức của chủ nhà đi từ `Sleeping` sang mức khác. `barkCount` đếm mỗi tiếng sủa (mỗi lần `NoiseSource.Dog` phát tiếng).

Không ghi vị trí hay bản đồ nhiệt.

## 2. Nhật ký ván chơi: kiến trúc

**Core** (logic thuần, EditMode test):
- `MatchLogEntry`: lớp `[Serializable]` theo mẫu `PlayerStats` (field `[SerializeField] private` có tiền tố `_`, property chỉ đọc), nên khóa JSON là tên ở mục 1 thêm dấu `_` đầu (ví dụ `_barkCount`).
- `MatchLogBuilder`: `MarkFirst(milestone, seconds)` chỉ ghi lần đầu; `Increment(counter)`; `AddTime(state, seconds)`; `Build(outcome, durationSeconds, configHash, endedAtUtc)` trả `MatchLogEntry`. Mốc, bộ đếm, trạng thái là enum.
- `ConfigFingerprint.Compute(IEnumerable<string> parts)`: mã hex 8 ký tự, ổn định giữa các lần chạy (không dùng `string.GetHashCode`); cùng nội dung cho cùng mã, đổi bất kỳ ký tự nào thì đổi mã.
- `IMatchLogWriter` + `JsonLinesMatchLogWriter`: nối thêm một dòng vào file; tạo thư mục nếu chưa có. Bắt `IOException` và `UnauthorizedAccessException`, log `[MatchLog]` rồi bỏ qua.

**Gameplay**:
- `ThiefInteractor`: thêm `public event Action LureThrown`, phát sau khi ném thành công.
- `MatchLogRecorder` (MonoBehaviour, trên object `Match`): tham chiếu `MatchManager`, `ThiefMotor`, `ThiefVisibility`, `ThiefInteractor`, `CarryableDog`, nguồn cảnh giác của chó và chủ nhà (`MonoBehaviour` đổi bằng `AwarenessSourceResolver`), và `ScriptableObject[] _configs` (3 config). Nghe sự kiện, cộng thời gian trong `Update`, ghi khi nhận `MatchEnded` (thời lượng và kết quả lấy từ `MatchResult`). Mã config: `JsonUtility.ToJson` từng config rồi đưa vào `ConfigFingerprint`. Tham chiếu nào trống thì bỏ qua phần số liệu đó, không lỗi.
- Thời điểm của mốc lấy từ `MatchManager.ElapsedSeconds` (mục 3).

**Editor**: công cụ greybox gắn `MatchLogRecorder` lên object `Match` và nối mọi tham chiếu.

**Phân tích**: Claude đọc `match_log.jsonl` bằng script, gom theo `configHash`, lập bảng so với mục tiêu ở mục 4. Không làm màn hình thống kê trong game.

## 3. Gom thông số vào config và đồng hồ ván

| Thông số | Hiện ở | Chuyển vào | Giá trị mặc định |
|---|---|---|---|
| Tầm nhìn chủ nhà | `VisionSensor._range` | `OwnerConfig.VisionRange` | 12 |
| Góc nhìn chủ nhà | `VisionSensor._fieldOfView` | `OwnerConfig.VisionFieldOfView` | 110 |
| Độ thính tai của chó | `HearingSensor._sensitivity` | `DogConfig.HearingSensitivity` (mới) | 1 |
| Khoảng cách chó nhận ra đồ ăn | `FoodLure.AttractRadius` | `DogConfig.LureNoticeDistance` | 8 |
| Thời gian chó ăn | `FoodLure.EatDuration` | `DogConfig.EatDuration` | 4 |

- `VisionSensor.Configure(float range, float fieldOfView)`; `OwnerAI.Awake` gọi với giá trị config. `DogAI.Awake` đặt `HearingSensor.Sensitivity` từ config. Cảm biến không tham chiếu config.
- `FoodLure.FindNearestUnclaimed(Vector3 position, float maxDistance)`; `DogAI` truyền `LureNoticeDistance`. `DogEatLureState` dùng `DogConfig.EatDuration`. `FoodLure` bỏ hai field cũ.
- Giá trị mặc định bằng số cũ nên config đã có tự nhận đúng giá trị khi Unity nạp lại; game chơi y như trước.
- `MatchManager.ElapsedSeconds` (chỉ đọc): thời gian từ đầu ván, dừng khi ván kết thúc.
- `MatchTimerUI` (UI): hiện `phút:giây` ở giữa phía trên HUD; hàm định dạng thuần `FormatTime(seconds)` có test (ví dụ `0 → "0:00"`, `65.4 → "1:05"`, `600 → "10:00"`). Công cụ greybox tạo chữ này, dùng font tiếng Việt khi có.

## 4. Quy trình cân bằng

**Mục tiêu ban đầu** (có thể chỉnh sau vòng 0):

| Chỉ số | Mục tiêu |
|---|---|
| Thời lượng trung vị của ván thắng | 3–5 phút |
| Ván thua | Đa số dài hơn 60 giây |
| Chó sủa lần đầu khi chơi cẩn thận | Không sủa trong khoảng 30 giây đầu |
| Tỉ lệ thắng khi đã quen | Khoảng 40–60% |
| Cách thắng | Đa số ván thắng có dùng đồ ăn |

**Mỗi vòng:**
1. Vòng 0: người dùng chơi 5 ván với thông số hiện tại.
2. Claude đọc nhật ký, lập bảng so với mục tiêu, đề xuất tối đa 3 thay đổi config kèm lý do.
3. Người dùng duyệt; Claude chỉ sửa các file `.asset` trong `Assets/_Project/Settings/Configs/`; người dùng chơi tiếp 5 ván.
4. Lặp lại, dự kiến 3–4 vòng.

**Ghi chép:** `docs/balance/G1c-balance-log.md`, mỗi vòng có mã config, thay đổi và lý do, bảng kết quả, cảm nhận của người dùng.

**Nhánh và PR:** PR công cụ đo trước; mỗi vòng chỉnh một nhánh `feature/g1c-round-<n>` và một PR nhỏ (config + ghi chép). Khi chốt, giá trị mặc định trong code các config cập nhật theo số cuối cùng.

## Kiểm thử

**EditMode (Claude tự chạy):**
- `MatchLogBuilder`: mốc chỉ ghi lần đầu, mốc không xảy ra là `-1`; bộ đếm cộng đúng; thời gian cộng dồn; `Build` điền đủ trường.
- `ConfigFingerprint`: cùng đầu vào cho cùng mã, 8 ký tự hex; đổi một ký tự thì đổi mã.
- `JsonLinesMatchLogWriter`: hai lần ghi tạo hai dòng JSON đọc lại được; đường dẫn không ghi được thì log `[MatchLog]` và không ném lỗi.
- `MatchTimerUI.FormatTime`.
- `FoodLure.FindNearestUnclaimed` bỏ qua đồ ăn xa hơn `maxDistance`.
- `VisionSensor.Configure` đổi tầm nhìn dùng trong `CanSee`.
- Công cụ greybox: `Match` có `MatchLogRecorder` với đủ tham chiếu và 3 config; HUD có đồng hồ.
- Toàn bộ test cũ vẫn đạt.

**Chơi thử (người dùng):** vòng 0 gồm 5 ván; sau mỗi ván, file `match_log.jsonl` có thêm một dòng và đồng hồ hiện đúng thời lượng.

## Hoàn thành khi

1. Toàn bộ EditMode test đạt; công cụ đo đã merge.
2. Ở vòng cuối, thời lượng trung vị của ván thắng nằm trong 3–5 phút và các mục tiêu khác đạt hoặc gần đạt.
3. Người dùng xác nhận cảm giác "căng thẳng nhưng vui".

## Ngoài phạm vi

Lớp hiển thị gỡ lỗi (F1), AI trộm tự chơi, màn hình thống kê trong game, tính năng mới (G1b), giọng chủ nhà, chơi thử với người khác (G1d).
