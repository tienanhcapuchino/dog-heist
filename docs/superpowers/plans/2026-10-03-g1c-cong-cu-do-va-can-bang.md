# G1c: Công cụ đo và cân bằng — Kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mỗi ván greybox ghi một dòng nhật ký vào `match_log.jsonl`, mọi thông số cân bằng nằm trong ba config, HUD có đồng hồ ván; sau đó chạy vòng cân bằng 0 (mốc gốc).

**Architecture:** Core chứa logic thuần (`MatchLogBuilder`, `ConfigFingerprint`, bộ ghi file JSON Lines). Gameplay có `MatchLogRecorder` chỉ nghe sự kiện có sẵn và đọc trạng thái trộm; các hàm xử lý là `internal` để EditMode test gọi thẳng (EditMode không gọi `Awake`/`OnEnable`). Thông số còn nằm trên cảm biến và prefab chuyển vào config, AI truyền vào cảm biến. Công cụ greybox nối mọi thứ.

**Tech Stack:** Unity 6.3 LTS (6000.3.25f1), URP, C# 9, TextMeshPro, `JsonUtility`, Unity Test Framework (EditMode), PowerShell 5.1.

**Spec:** `docs/superpowers/specs/2026-10-03-g1c-can-bang-design.md`

## Global Constraints

- Trả lời người dùng bằng tiếng Việt; comment tiếng Việt; tên class, method, biến tiếng Anh.
- Tối đa C# 9: không file-scoped namespace, không record struct, không raw string, không `init`.
- Allman; `PascalCase` type/method/property; `_camelCase` field private; `s_camelCase` field private static; interface bắt đầu bằng `I`; `[SerializeField] private`; config dùng `[field: SerializeField]`.
- Chiều phụ thuộc: `UI → Gameplay → Core`, `AI → Gameplay`, `Editor → tất cả`. Gameplay và UI không tham chiếu AI.
- Không sửa logic chuyển trạng thái của AI hay luật chơi (ngoại lệ: cảm biến nhận giá trị config, `FindNearestUnclaimed` nhận khoảng cách, `ThiefInteractor` phát `LureThrown`).
- Thông số cân bằng chỉ trong `ThiefConfig`/`DogConfig`/`OwnerConfig`; giá trị mặc định của thông số chuyển vào bằng đúng số cũ: `VisionRange = 12`, `VisionFieldOfView = 110`, `HearingSensitivity = 1`, `LureNoticeDistance = 8`, `EatDuration = 4`.
- Log có tiền tố `[MatchLog]`. Không `catch {}` rỗng. Ghi nhật ký lỗi không bao giờ làm hỏng ván.
- Có `using System;` thì gọi `UnityEngine.Random`/`UnityEngine.Object` đầy đủ.
- Kiểm tra bằng `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 [-Filter <tên>]` khi Unity **và Visual Studio đóng**.
- Mỗi task một nhánh `feature/g1c-<mô-tả>` tạo từ `origin/main`. **Không tự ý commit:** bước "Commit" nghĩa là hỏi người dùng. Commit message qua file (`git commit -F`), kết thúc bằng `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus

1. **Ván kết thúc hai lần hoặc `MatchEnded` phát lặp**: chỉ ghi đúng một dòng cho mỗi ván. → test ở Task 5.
2. **Thư mục nhật ký không ghi được** (đường dẫn trỏ vào trong một file, quyền ghi): log `[MatchLog]`, không ném lỗi, ván vẫn kết thúc bình thường. → test ở Task 2.
3. **Thiếu tham chiếu trên recorder** (chưa gán chó hoặc chủ nhà): vẫn ghi dòng nhật ký, các mốc liên quan là `-1`, không `NullReferenceException`. → test ở Task 5.
4. **Chủ nhà tỉnh rồi ngủ rồi tỉnh lại**: `ownerWakeCount` đếm mỗi lần rời `Sleeping`; đi tuần → đuổi → mất dấu không tính là tỉnh dậy thêm. → test ở Task 5.
5. **Mã config nhầm lẫn khi ghép chuỗi** (`["ab","c"]` và `["a","bc"]` cho cùng mã): phải khác mã, nếu không hai bộ thông số khác nhau bị gộp vào một vòng. → test ở Task 2.

---

## Cấu trúc file

| File | Trách nhiệm |
|---|---|
| `Scripts/Core/MatchLog/MatchLogTypes.cs` | enum `MatchMilestone`, `MatchCounter`, `ThiefStateTime` |
| `Scripts/Core/MatchLog/MatchLogEntry.cs` | dữ liệu một ván (serialize bằng `JsonUtility`) |
| `Scripts/Core/MatchLog/MatchLogBuilder.cs` | gom số liệu trong lúc chơi |
| `Scripts/Core/MatchLog/ConfigFingerprint.cs` | mã 8 ký tự hex cho bộ config |
| `Scripts/Core/MatchLog/IMatchLogWriter.cs`, `JsonLinesMatchLogWriter.cs` | ghi nối thêm vào `match_log.jsonl` |
| `Scripts/AI/Owner/OwnerConfig.cs`, `AI/Dog/DogConfig.cs` (sửa) | thông số chuyển vào |
| `Scripts/AI/Sensors/VisionSensor.cs`, `AI/Owner/OwnerAI.cs`, `AI/Dog/DogAI.cs`, `AI/Dog/DogStates.cs`, `Gameplay/Items/FoodLure.cs` (sửa) | dùng thông số từ config |
| `Scripts/Gameplay/Match/MatchManager.cs` (sửa) | `ElapsedSeconds` |
| `Scripts/UI/Hud/MatchTimerUI.cs` | đồng hồ ván |
| `Scripts/Gameplay/Thief/ThiefInteractor.cs` (sửa) | sự kiện `LureThrown` |
| `Scripts/Gameplay/Match/MatchLogRecorder.cs` | nghe sự kiện, ghi nhật ký |
| `Scripts/Editor/Greybox/GreyboxLevelBuilder.cs`, `GreyboxHudFactory.cs` (sửa) | nối recorder và đồng hồ |
| `docs/balance/G1c-balance-log.md` | ghi chép các vòng cân bằng |

---

### Task 1: Dữ liệu và bộ gom nhật ký (Core)

**Files:**
- Create: `Assets/_Project/Scripts/Core/MatchLog/MatchLogTypes.cs`, `MatchLogEntry.cs`, `MatchLogBuilder.cs`
- Test: `Assets/_Project/Tests/EditMode/MatchLogBuilderTests.cs`

**Interfaces:**
- Consumes: `DogHeist.Core.Match.MatchOutcome` (có sẵn).
- Produces (namespace `DogHeist.Core.MatchLog`):
  - `public enum MatchMilestone { FirstBark, OwnerWake, Spotted, LureThrown, DogLured, Pickup }`
  - `public enum MatchCounter { Bark, OwnerWake, Spotted, LureThrown, DogDrop }`
  - `public enum ThiefStateTime { InLight, Hidden, Crouching, Sprinting }`
  - `[Serializable] public sealed class MatchLogEntry` theo mẫu `PlayerStats`: field `[SerializeField] private` `_endedAtUtc` (string), `_outcome` (string, tên enum), `_durationSeconds`, `_configHash`, `_firstBarkSeconds`, `_firstOwnerWakeSeconds`, `_firstSpottedSeconds`, `_firstLureThrownSeconds`, `_firstDogLuredSeconds`, `_firstPickupSeconds` (float), `_barkCount`, `_ownerWakeCount`, `_spottedCount`, `_luresThrown`, `_dogDropCount` (int), `_secondsInLight`, `_secondsHidden`, `_secondsCrouching`, `_secondsSprinting` (float); property chỉ đọc PascalCase tương ứng (`EndedAtUtc`, `Outcome`, `DurationSeconds`, `ConfigHash`, `FirstBarkSeconds`, … `SecondsSprinting`); constructor `internal` không tham số để `MatchLogBuilder` điền qua setter `internal`.
  - `public sealed class MatchLogBuilder`:
    - `public const float NotReached = -1f;`
    - `public void MarkFirst(MatchMilestone milestone, float seconds)` — chỉ ghi lần đầu.
    - `public void Increment(MatchCounter counter)`
    - `public void AddTime(ThiefStateTime state, float seconds)` — bỏ qua `seconds <= 0`.
    - `public float GetFirst(MatchMilestone milestone)`, `public int GetCount(MatchCounter counter)` (cho test và recorder).
    - `public MatchLogEntry Build(MatchOutcome outcome, float durationSeconds, string configHash, DateTime endedAtUtc)` — `EndedAtUtc = endedAtUtc.ToString("o", CultureInfo.InvariantCulture)`, `Outcome = outcome.ToString()`.

- [ ] **Step 1: Viết test hỏng**

```csharp
using System;
using DogHeist.Core.Match;
using DogHeist.Core.MatchLog;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class MatchLogBuilderTests
    {
        [Test]
        public void MarkFirst_KeepsOnlyFirstTime()
        {
            var builder = new MatchLogBuilder();
            builder.MarkFirst(MatchMilestone.FirstBark, 12.5f);
            builder.MarkFirst(MatchMilestone.FirstBark, 40f);

            Assert.AreEqual(12.5f, builder.GetFirst(MatchMilestone.FirstBark));
        }

        [Test]
        public void Build_UnreachedMilestonesAreMinusOne()
        {
            var entry = new MatchLogBuilder().Build(MatchOutcome.ThiefCaught, 30f, "abcd1234", new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(-1f, entry.FirstBarkSeconds);
            Assert.AreEqual(-1f, entry.FirstPickupSeconds);
            Assert.AreEqual("ThiefCaught", entry.Outcome);
            Assert.AreEqual("abcd1234", entry.ConfigHash);
            Assert.AreEqual("2026-10-03T00:00:00.0000000Z", entry.EndedAtUtc);
        }

        [Test]
        public void Build_CopiesCountersAndTimes()
        {
            var builder = new MatchLogBuilder();
            builder.Increment(MatchCounter.Bark);
            builder.Increment(MatchCounter.Bark);
            builder.Increment(MatchCounter.LureThrown);
            builder.AddTime(ThiefStateTime.InLight, 1.5f);
            builder.AddTime(ThiefStateTime.InLight, 2f);
            builder.AddTime(ThiefStateTime.Hidden, -1f);
            builder.MarkFirst(MatchMilestone.Pickup, 90f);

            var entry = builder.Build(MatchOutcome.ThiefEscaped, 200f, "h", DateTime.UtcNow);

            Assert.AreEqual(2, entry.BarkCount);
            Assert.AreEqual(1, entry.LuresThrown);
            Assert.AreEqual(3.5f, entry.SecondsInLight, 1e-4f);
            Assert.AreEqual(0f, entry.SecondsHidden);
            Assert.AreEqual(90f, entry.FirstPickupSeconds);
            Assert.AreEqual(200f, entry.DurationSeconds);
        }

        [Test]
        public void Entry_SerializesWithUnderscoreKeys()
        {
            var builder = new MatchLogBuilder();
            builder.Increment(MatchCounter.Bark);
            var json = JsonUtility.ToJson(builder.Build(MatchOutcome.ThiefEscaped, 1f, "h", DateTime.UtcNow));

            StringAssert.Contains("\"_barkCount\":1", json);
            StringAssert.Contains("\"_outcome\":\"ThiefEscaped\"", json);
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter MatchLogBuilder`
Expected: lỗi biên dịch `The type or namespace name 'MatchLog' does not exist in the namespace 'DogHeist.Core'`.

- [ ] **Step 3: Viết `MatchLogTypes`, `MatchLogEntry`, `MatchLogBuilder`** theo Interfaces. Builder lưu mốc trong `float[]` khởi tạo `NotReached`, bộ đếm `int[]`, thời gian `float[]`, chỉ số theo giá trị enum.

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 4 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(core): thêm dữ liệu và bộ gom nhật ký ván chơi`

---

### Task 2: Mã config và bộ ghi file (Core)

**Files:**
- Create: `Assets/_Project/Scripts/Core/MatchLog/ConfigFingerprint.cs`, `IMatchLogWriter.cs`, `JsonLinesMatchLogWriter.cs`
- Test: `Assets/_Project/Tests/EditMode/MatchLogWriterTests.cs`

**Interfaces:**
- Consumes: `MatchLogEntry`, `MatchLogBuilder` (Task 1).
- Produces (namespace `DogHeist.Core.MatchLog`):
  - `public static class ConfigFingerprint { public static string Compute(IEnumerable<string> parts); }` — FNV-1a 32 bit trên byte UTF-8 của từng phần, sau mỗi phần băm thêm ký tự phân cách `'\u001f'`; trả `hash.ToString("x8")`. `parts` null coi như rỗng; phần tử null coi như chuỗi rỗng.
  - `public interface IMatchLogWriter { void Append(MatchLogEntry entry); }`
  - `public sealed class JsonLinesMatchLogWriter : IMatchLogWriter` — `public JsonLinesMatchLogWriter()` (dùng `Path.Combine(Application.persistentDataPath, "match_log.jsonl")`), `public JsonLinesMatchLogWriter(string filePath)` (ném `ArgumentException` khi rỗng), `public string FilePath { get; }`. `Append`: tạo thư mục cha nếu thiếu, `File.AppendAllText(path, JsonUtility.ToJson(entry) + "\n")`; bắt `IOException`/`UnauthorizedAccessException` → `Debug.LogError("[MatchLog] ...")`.

- [ ] **Step 1: Viết test hỏng** (`MatchLogWriterTests`, thư mục tạm trong `Path.GetTempPath()`, xóa ở `TearDown`)

```csharp
[Test]
public void Fingerprint_IsStableEightHex()
{
    var first = ConfigFingerprint.Compute(new[] { "{\"a\":1}", "{\"b\":2}" });
    var second = ConfigFingerprint.Compute(new[] { "{\"a\":1}", "{\"b\":2}" });

    Assert.AreEqual(first, second);
    StringAssert.IsMatch("^[0-9a-f]{8}$", first);
}

[Test]
public void Fingerprint_ChangesWhenAnyValueChanges() =>
    Assert.AreNotEqual(ConfigFingerprint.Compute(new[] { "{\"a\":1}" }), ConfigFingerprint.Compute(new[] { "{\"a\":2}" }));

[Test]
public void Fingerprint_PartBoundariesMatter() =>
    Assert.AreNotEqual(ConfigFingerprint.Compute(new[] { "ab", "c" }), ConfigFingerprint.Compute(new[] { "a", "bc" }));

[Test]
public void Append_TwiceWritesTwoJsonLines()
{
    var writer = new JsonLinesMatchLogWriter(Path.Combine(_tempDir, "sub", "match_log.jsonl"));
    writer.Append(new MatchLogBuilder().Build(MatchOutcome.ThiefCaught, 10f, "h1", DateTime.UtcNow));
    writer.Append(new MatchLogBuilder().Build(MatchOutcome.ThiefEscaped, 20f, "h2", DateTime.UtcNow));

    var lines = File.ReadAllLines(writer.FilePath);
    Assert.AreEqual(2, lines.Length);
    Assert.AreEqual(20f, JsonUtility.FromJson<MatchLogEntry>(lines[1]).DurationSeconds);
}

[Test]
public void Append_UnwritablePath_LogsAndDoesNotThrow()
{
    var blockingFile = Path.Combine(_tempDir, "not_a_folder");
    File.WriteAllText(blockingFile, "x");
    var writer = new JsonLinesMatchLogWriter(Path.Combine(blockingFile, "match_log.jsonl"));
    LogAssert.Expect(LogType.Error, new Regex(@"\[MatchLog\]"));

    Assert.DoesNotThrow(() => writer.Append(new MatchLogBuilder().Build(MatchOutcome.ThiefCaught, 1f, "h", DateTime.UtcNow)));
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng** — `-Filter MatchLogWriter`; Expected: lỗi biên dịch `The name 'ConfigFingerprint' does not exist in the current context`.

- [ ] **Step 3: Viết `ConfigFingerprint`, `IMatchLogWriter`, `JsonLinesMatchLogWriter`** theo Interfaces. `JsonUtility.FromJson` cần `MatchLogEntry` có constructor không tham số dùng được qua reflection (constructor `internal` là đủ).

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 5 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(core): thêm mã config và bộ ghi nhật ký JSON Lines`

---

### Task 3: Gom thông số cân bằng vào config

**Files:**
- Modify: `Assets/_Project/Scripts/AI/Owner/OwnerConfig.cs`, `AI/Dog/DogConfig.cs`, `AI/Sensors/VisionSensor.cs`, `AI/Owner/OwnerAI.cs`, `AI/Dog/DogAI.cs`, `AI/Dog/DogStates.cs`, `Gameplay/Items/FoodLure.cs`
- Test: `Assets/_Project/Tests/EditMode/BalanceConfigTests.cs`

**Interfaces:**
- Produces:
  - `OwnerConfig`: `[field: Header("Tầm nhìn")] VisionRange { get; private set; } = 12f` (`Min(0f)`), `VisionFieldOfView = 110f` (`Range(1f, 360f)`).
  - `DogConfig`: `HearingSensitivity = 1f` (`Min(0f)`, nhóm "Cảnh giác"), `LureNoticeDistance = 8f` (`Min(0f)`), `EatDuration = 4f` (`Min(0f)`, nhóm "Đồ ăn và trạng thái hiền").
  - `VisionSensor`: `public void Configure(float range, float fieldOfView)` (kẹp `range >= 0`, `fieldOfView` trong `[1, 360]`), `public float FieldOfView { get; }`; giữ `Range`. Field `_range`, `_fieldOfView` vẫn serialize để dùng khi không có AI gọi `Configure`.
  - `FoodLure.FindNearestUnclaimed(Vector3 position, float maxDistance)`; xóa `AttractRadius` và `EatDuration` khỏi `FoodLure`.
  - `OwnerAI.Awake`: `_vision?.Configure(_config.VisionRange, _config.VisionFieldOfView)` (sau kiểm tra config). `DogAI.Awake`: đặt `_hearing.Sensitivity = _config.HearingSensitivity` khi có `_hearing`; `FindAvailableLure()` truyền `_config.LureNoticeDistance`. `DogEatLureState` dùng `Dog.Config.EatDuration`.

- [ ] **Step 1: Viết test hỏng**

```csharp
[Test]
public void NewConfigs_DefaultToPreviousValues()
{
    var owner = ScriptableObject.CreateInstance<OwnerConfig>();
    var dog = ScriptableObject.CreateInstance<DogConfig>();
    try
    {
        Assert.AreEqual(12f, owner.VisionRange);
        Assert.AreEqual(110f, owner.VisionFieldOfView);
        Assert.AreEqual(1f, dog.HearingSensitivity);
        Assert.AreEqual(8f, dog.LureNoticeDistance);
        Assert.AreEqual(4f, dog.EatDuration);
    }
    finally
    {
        Object.DestroyImmediate(owner);
        Object.DestroyImmediate(dog);
    }
}

[Test]
public void VisionSensor_ConfigureOverridesRangeAndFov()
{
    var sensor = new GameObject("Eye").AddComponent<VisionSensor>();
    try
    {
        sensor.Configure(7f, 400f);
        Assert.AreEqual(7f, sensor.Range);
        Assert.AreEqual(360f, sensor.FieldOfView);
    }
    finally
    {
        Object.DestroyImmediate(sensor.gameObject);
    }
}

[Test]
public void FindNearestUnclaimed_IgnoresLuresBeyondMaxDistance()
{
    var near = new GameObject("Near").AddComponent<FoodLure>();
    var far = new GameObject("Far").AddComponent<FoodLure>();
    near.transform.position = new Vector3(3f, 0f, 0f);
    far.transform.position = new Vector3(10f, 0f, 0f);
    // EditMode không gọi OnEnable nên đăng ký thủ công qua hàm internal của FoodLure.
    FoodLure.RegisterForTests(near);
    FoodLure.RegisterForTests(far);
    try
    {
        Assert.AreSame(near, FoodLure.FindNearestUnclaimed(Vector3.zero, 5f));
        Assert.IsNull(FoodLure.FindNearestUnclaimed(new Vector3(-10f, 0f, 0f), 5f));
    }
    finally
    {
        FoodLure.UnregisterForTests(near);
        FoodLure.UnregisterForTests(far);
        Object.DestroyImmediate(near.gameObject);
        Object.DestroyImmediate(far.gameObject);
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng** — `-Filter BalanceConfig`; Expected: lỗi biên dịch `'OwnerConfig' does not contain a definition for 'VisionRange'`.

- [ ] **Step 3: Sửa config, `VisionSensor`, `FoodLure`, `OwnerAI`, `DogAI`, `DogStates`** theo Interfaces. Trong `FoodLure` thêm `internal static void RegisterForTests(FoodLure lure)` / `UnregisterForTests(FoodLure lure)` dùng cùng danh sách đăng ký mà `OnEnable`/`OnDisable` dùng (Gameplay đã có `InternalsVisibleTo` cho test).

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 3 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`. Mở file `Assets/_Project/Settings/Configs/*.asset` không cần sửa: Unity tự điền giá trị mặc định cho field mới khi nạp.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `refactor(ai): gom tầm nhìn, thính giác và thông số đồ ăn vào config`

---

### Task 4: Đồng hồ ván

**Files:**
- Modify: `Assets/_Project/Scripts/Gameplay/Match/MatchManager.cs`
- Create: `Assets/_Project/Scripts/UI/Hud/MatchTimerUI.cs`
- Test: `Assets/_Project/Tests/EditMode/MatchTimerTests.cs`

**Interfaces:**
- Produces:
  - `MatchManager.ElapsedSeconds { get; }` — `Playing`: `Time.time - _startTime`; `Ended`: thời lượng đã ghi lúc kết thúc (lưu vào field `_endedDuration` trong `EndMatch`, dùng chung với `MatchResult`); `NotStarted`: `0`.
  - `public sealed class MatchTimerUI : MonoBehaviour` (namespace `DogHeist.UI.Hud`): `[SerializeField] private MatchManager _match; [SerializeField] private TMP_Text _label;` `Update` đặt chữ khi giá trị giây nguyên đổi. `internal static string FormatTime(float seconds)` — số âm coi như 0; `phút:giây` hai chữ số cho giây, làm tròn xuống.

- [ ] **Step 1: Viết test hỏng**

```csharp
[TestCase(0f, "0:00")]
[TestCase(65.4f, "1:05")]
[TestCase(59.99f, "0:59")]
[TestCase(600f, "10:00")]
[TestCase(-3f, "0:00")]
public void FormatTime_MinutesAndSeconds(float seconds, string expected) =>
    Assert.AreEqual(expected, MatchTimerUI.FormatTime(seconds));
```

- [ ] **Step 2: Chạy test, xác nhận hỏng** — `-Filter MatchTimer`; Expected: lỗi biên dịch `The name 'MatchTimerUI' does not exist in the current context`.

- [ ] **Step 3: Thêm `ElapsedSeconds` và viết `MatchTimerUI`** theo Interfaces.

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 5 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(ui): thêm đồng hồ ván trên HUD`

---

### Task 5: Bộ ghi nhật ký trong ván (`MatchLogRecorder`)

**Files:**
- Modify: `Assets/_Project/Scripts/Gameplay/Thief/ThiefInteractor.cs` (thêm `public event Action LureThrown`, phát cuối `TryThrowLure` sau `LuresRemaining--`)
- Create: `Assets/_Project/Scripts/Gameplay/Match/MatchLogRecorder.cs`
- Test: `Assets/_Project/Tests/EditMode/MatchLogRecorderTests.cs`

**Interfaces:**
- Consumes: Task 1–2 (`MatchLogBuilder`, `ConfigFingerprint`, `IMatchLogWriter`, `JsonLinesMatchLogWriter`), Task 4 (`MatchManager.ElapsedSeconds`), `AwarenessSourceResolver`, `AwarenessLevel`, `NoiseEvent`, `MatchEvents`, `CarryableDog`.
- Produces: `public sealed class MatchLogRecorder : MonoBehaviour` (namespace `DogHeist.Gameplay.Match`) với field serialize `_match` (`MatchManager`), `_motor` (`ThiefMotor`), `_visibility` (`ThiefVisibility`), `_interactor` (`ThiefInteractor`), `_dog` (`CarryableDog`), `_dogAwareness`, `_ownerAwareness` (`MonoBehaviour`), `_configs` (`ScriptableObject[]`). Thành phần `internal` cho test:
  - `internal IMatchLogWriter Writer { get; set; }` — `Awake` gán `new JsonLinesMatchLogWriter()` nếu null.
  - `internal Func<float> Clock { get; set; }` — mặc định `() => _match != null ? _match.ElapsedSeconds : 0f`.
  - `internal void HandleNoise(NoiseEvent noise)` — `NoiseSource.Dog`: `MarkFirst(FirstBark)` + `Increment(Bark)`.
  - `internal void HandleOwnerAwareness(AwarenessLevel next)` — nếu mức trước là `Sleeping` và `next != Sleeping`: `MarkFirst(OwnerWake)` + `Increment(OwnerWake)`; luôn cập nhật mức trước. Mức trước khởi tạo bằng `Awareness` của nguồn khi bind (test đặt qua `internal void SetOwnerLevelForTests(AwarenessLevel level)`).
  - `internal void HandleDogAwareness(AwarenessLevel next)` — `next == Friendly`: `MarkFirst(DogLured)`.
  - `internal void HandleSpotted()`, `HandleLureThrown()`, `HandlePickedUp()`, `HandleDropped()` — mốc và bộ đếm tương ứng (`Pickup` chỉ là mốc; `Dropped` chỉ là bộ đếm `DogDrop`).
  - `internal void Sample(float deltaTime)` — cộng `InLight`/`Hidden` theo `_visibility`, `Crouching`/`Sprinting` theo `_motor`; tham chiếu null thì bỏ qua.
  - `internal void HandleMatchEnded(MatchResult result, PlayerStats stats)` — chỉ lần đầu: `Writer.Append(builder.Build(result.Outcome, result.DurationSeconds, ComputeConfigHash(), DateTime.UtcNow))`.
  - `OnEnable`/`OnDisable` đăng ký/hủy mọi sự kiện (nguồn nào null thì bỏ qua); `Update` gọi `Sample(Time.deltaTime)` khi `_match.State == MatchState.Playing`. `ComputeConfigHash()`: `JsonUtility.ToJson` từng phần tử khác null của `_configs` rồi `ConfigFingerprint.Compute`.

- [ ] **Step 1: Viết test hỏng** (một `FakeWriter : IMatchLogWriter` lưu `List<MatchLogEntry>`; recorder tạo bằng `new GameObject().AddComponent<MatchLogRecorder>()`, gán `Writer` và `Clock`)

```csharp
[Test]
public void MatchEnded_Twice_WritesOneLine()
{
    _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 42f), null);
    _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 42f), null);

    Assert.AreEqual(1, _writer.Entries.Count);
    Assert.AreEqual(42f, _writer.Entries[0].DurationSeconds);
}

[Test]
public void NoReferences_StillWritesWithUnreachedMilestones()
{
    _recorder.Sample(1f);
    _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 5f), null);

    Assert.AreEqual(-1f, _writer.Entries[0].FirstOwnerWakeSeconds);
    StringAssert.IsMatch("^[0-9a-f]{8}$", _writer.Entries[0].ConfigHash);
}

[Test]
public void OwnerWake_CountsEachLeaveFromSleeping()
{
    _recorder.SetOwnerLevelForTests(AwarenessLevel.Sleeping);
    _time = 10f; _recorder.HandleOwnerAwareness(AwarenessLevel.Suspicious);
    _recorder.HandleOwnerAwareness(AwarenessLevel.Alerted);
    _recorder.HandleOwnerAwareness(AwarenessLevel.Suspicious);
    _recorder.HandleOwnerAwareness(AwarenessLevel.Sleeping);
    _time = 50f; _recorder.HandleOwnerAwareness(AwarenessLevel.Alerted);
    _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 60f), null);

    Assert.AreEqual(2, _writer.Entries[0].OwnerWakeCount);
    Assert.AreEqual(10f, _writer.Entries[0].FirstOwnerWakeSeconds);
}

[Test]
public void DogBarks_CountedOnlyForDogNoise()
{
    _time = 7f;
    _recorder.HandleNoise(new NoiseEvent(Vector3.zero, 16f, NoiseSource.Dog, null));
    _recorder.HandleNoise(new NoiseEvent(Vector3.zero, 4f, NoiseSource.Thief, null));
    _recorder.HandleNoise(new NoiseEvent(Vector3.zero, 16f, NoiseSource.Dog, null));
    _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefEscaped, 100f), null);

    Assert.AreEqual(2, _writer.Entries[0].BarkCount);
    Assert.AreEqual(7f, _writer.Entries[0].FirstBarkSeconds);
}
```

(`Result(outcome, duration)` = `new MatchResult(PlayerRole.Thief, outcome, duration, false)`; `Clock = () => _time`.)

- [ ] **Step 2: Chạy test, xác nhận hỏng** — `-Filter MatchLogRecorder`; Expected: lỗi biên dịch `The type or namespace name 'MatchLogRecorder' could not be found`.

- [ ] **Step 3: Thêm `LureThrown` vào `ThiefInteractor` và viết `MatchLogRecorder`** theo Interfaces.

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 4 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(gameplay): ghi nhật ký diễn biến mỗi ván chơi`

---

### Task 6: Dựng vào màn greybox

**Files:**
- Modify: `Assets/_Project/Scripts/Editor/Greybox/GreyboxLevelBuilder.cs`, `GreyboxHudFactory.cs`
- Test: `Assets/_Project/Tests/EditMode/GreyboxMatchLogTests.cs` (kế thừa `GreyboxSceneTestBase`)

**Interfaces:**
- Consumes: `MatchLogRecorder` (Task 5), `MatchTimerUI` (Task 4).
- Produces:
  - `GreyboxLevelBuilder.BuildInto`: thêm `MatchLogRecorder` lên object `Match`, nối `_match`, `_motor`, `_visibility`, `_interactor`, `_dog` (`CarryableDog` của chó), `_dogAwareness` (`DogAI`), `_ownerAwareness` (`OwnerAI`), `_configs` = `[ThiefConfig, DogConfig, OwnerConfig]` của `assets`.
  - `GreyboxHudFactory`: chữ `MatchTimerText` (TopCenter, vị trí `(0, -30)`, cỡ 36, căn giữa) và `MatchTimerUI` trên Canvas nối `_match`, `_label`; font theo `ApplyFont` có sẵn.

- [ ] **Step 1: Viết test hỏng**

```csharp
[Test]
public void BuildInto_AddsMatchLogRecorderWithConfigs()
{
    var result = Build();
    var recorder = result.Match.GetComponent<MatchLogRecorder>();

    Assert.IsNotNull(recorder);
    Assert.AreSame(result.Match, ReadReference(recorder, "_match"));
    Assert.AreSame(result.Owner, ReadReference(recorder, "_ownerAwareness"));
    Assert.AreSame(result.Dog, ReadReference(recorder, "_dogAwareness"));
    Assert.AreEqual(3, new SerializedObject(recorder).FindProperty("_configs").arraySize);
}

[Test]
public void BuildInto_AddsMatchTimer()
{
    var timer = Build().Root.GetComponentInChildren<MatchTimerUI>(true);

    Assert.IsNotNull(timer);
    Assert.IsNotNull(ReadReference(timer, "_label"));
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng** — `-Filter GreyboxMatchLog`; Expected: test hỏng ở `Assert.IsNotNull(recorder)` (biên dịch được vì Task 4–5 đã có kiểu).

- [ ] **Step 3: Sửa `GreyboxLevelBuilder` và `GreyboxHudFactory`** theo Interfaces.

- [ ] **Step 4: Chạy test, xác nhận đạt** — `-Filter Greybox`; Expected: mọi test Greybox đạt.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Người dùng chạy menu** *DogHeist > Tools > Build Greybox Level*, bấm Play một ván, đóng Unity. Claude kiểm tra: `match_log.jsonl` trong `%USERPROFILE%\AppData\LocalLow\tienanhcapuchino\Dog Heist\` có một dòng hợp lệ; `durationSeconds` khớp đồng hồ.

- [ ] **Step 7: Commit (hỏi người dùng trước)** — code, test, scene và prefab do menu sinh (khôi phục `BeVietnamPro SDF.asset` nếu chỉ đổi do chơi thử). Message: `feat(editor): dựng bộ ghi nhật ký và đồng hồ vào màn greybox`.

---

### Task 7: Vòng cân bằng 0 (mốc gốc)

**Files:**
- Create: `docs/balance/G1c-balance-log.md`
- Modify: `CLAUDE.md` (mục Kiểm tra: cách đọc nhật ký)

- [ ] **Step 1: Viết khung `docs/balance/G1c-balance-log.md`**: bảng mục tiêu (copy mục 4 của spec), mẫu một vòng (mã config, thay đổi và lý do, bảng kết quả, cảm nhận), mục "Vòng 0".

- [ ] **Step 2: Người dùng chơi 5 ván** với thông số hiện tại, không đổi config; chơi tự nhiên, cố gắng thắng.

- [ ] **Step 3: Phân tích** — đọc `match_log.jsonl` bằng PowerShell (`Get-Content | ConvertFrom-Json`), lọc theo `_configHash` hiện tại, tính: số ván thắng/thua, trung vị `_durationSeconds` theo kết quả, trung vị và số ván có `_firstBarkSeconds`, `_firstOwnerWakeSeconds`, tỉ lệ ván thắng có `_luresThrown > 0`, trung bình `_secondsInLight`, `_secondsCrouching`. Điền bảng "Vòng 0" và so với mục tiêu.

- [ ] **Step 4: Đề xuất vòng 1** — tối đa 3 thay đổi config kèm lý do dựa trên số đo; ghi vào file, chờ người dùng duyệt (thực hiện vòng 1 trên nhánh `feature/g1c-round-1` riêng, ngoài kế hoạch này).

- [ ] **Step 5: Cập nhật `CLAUDE.md`** mục Kiểm tra: vị trí `match_log.jsonl`, mỗi vòng lọc theo `_configHash`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `docs: ghi chép vòng cân bằng 0 cho G1c`.
