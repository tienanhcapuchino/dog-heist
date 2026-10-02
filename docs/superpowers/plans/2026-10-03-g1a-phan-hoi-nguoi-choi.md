# G1a: Phản hồi cho người chơi — Kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Người chơi luôn đọc được tình huống: dấu `Zzz`/`?`/`!`/`♥` trên đầu AI, con mắt lộ/ẩn trên HUD, rung camera khi bị phát hiện, âm thanh tạm, chữ tiếng Việt đúng dấu; tất cả dựng tự động bằng menu *Build Greybox Level*.

**Architecture:** Gameplay định nghĩa `AwarenessLevel`, `IAwarenessSource` và `AwarenessTracker` (lớp thuần, chỉ phát sự kiện khi mức đổi). AI cài interface: mỗi lớp trạng thái tự khai báo mức. UI (`AwarenessIndicatorUI`, `VisibilityEyeUI`, `SpottedCameraShake`) và âm thanh (`Gameplay/Audio`) chỉ đọc interface và sự kiện. Logic dễ test được tách thành hàm hoặc lớp thuần vì EditMode test không gọi `Awake`/`OnEnable`.

**Tech Stack:** Unity 6.3 LTS (6000.3.25f1), URP, C# 9, TextMeshPro, Cinemachine 3.1 (Impulse), Unity Test Framework (EditMode), PowerShell 5.1.

**Spec:** `docs/superpowers/specs/2026-10-03-g1a-phan-hoi-nguoi-choi-design.md`

## Global Constraints

- Trả lời người dùng bằng tiếng Việt; comment tiếng Việt; tên class, method, biến tiếng Anh.
- Tối đa C# 9: không file-scoped namespace, không record struct, không raw string, không `init`.
- Allman; `PascalCase` type/method/property; `_camelCase` field private; `s_camelCase` field private static; interface bắt đầu bằng `I`; `[SerializeField] private`.
- Chiều phụ thuộc: `UI → Gameplay → Core`, `AI → Gameplay`, `Editor → tất cả`. UI không tham chiếu AI. Không assembly runtime nào tham chiếu `DogHeist.Editor`.
- Logic chuyển trạng thái của AI giữ nguyên; chỉ thêm khai báo mức cảnh giác và phát sự kiện.
- Thông số cân bằng chỉ trong `ThiefConfig`/`DogConfig`/`OwnerConfig`. Thông số trình bày nằm trong `AwarenessIndicatorStyle`, `AudioCue` hoặc `[SerializeField]` trên component.
- Log có tiền tố: `[Awareness]`, `[Audio]`, `[Greybox]`. Không `catch {}` rỗng, không `GameObject.Find`.
- Sự kiện tĩnh mới (nếu có) phải có reset `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`.
- Công cụ greybox không ghi đè asset đã tồn tại (config, cue đã có clip, style, font asset).
- Kiểm tra bằng `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 [-Filter <tên>]` khi Unity **đóng**.
- **Không tự ý commit:** bước "Commit" nghĩa là hỏi người dùng, chỉ commit khi được đồng ý. Mỗi commit kết thúc bằng `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus

1. **Nguồn cảnh giác gán sai** (kéo nhầm component không cài `IAwarenessSource`, hoặc để trống): dấu phải log lỗi `[Awareness]` và tắt, không văng `NullReferenceException` mỗi frame. → test ở Task 2.
2. **Cue chưa có file âm thanh**: phát nhiều lần vẫn im lặng và chỉ cảnh báo `[Audio]` **một lần** cho mỗi cue, không làm ngập Console theo từng bước chân. → test ở Task 4.
3. **Chạy lại công cụ sau khi người dùng đã kéo clip vào cue hoặc chỉnh style**: clip và chỉnh sửa phải còn nguyên. → test ở Task 6.
4. **Chưa tải font tiếng Việt**: công cụ vẫn dựng xong màn, chỉ cảnh báo; khi có font, `♥` vẫn hiện nhờ font dự phòng. → test ở Task 6.
5. **Chuyển mức không nên phát tiếng**: chủ nhà mất dấu (`Alerted → Suspicious`) không được phát "Hửm?"; chó `Friendly → Friendly` (ăn → hiền → được bế) không phát lại tiếng vui; khởi động (`None → Sleeping`) không phát gì. → test ở Task 5.

---

## Cấu trúc file

| File | Trách nhiệm |
|---|---|
| `Scripts/Gameplay/Awareness/AwarenessLevel.cs` | enum mức cảnh giác |
| `Scripts/Gameplay/Awareness/IAwarenessSource.cs` | interface nguồn cảnh giác |
| `Scripts/Gameplay/Awareness/AwarenessTracker.cs` | lớp thuần giữ mức hiện tại, phát sự kiện khi đổi |
| `Scripts/Gameplay/Awareness/AwarenessSourceResolver.cs` | đổi `MonoBehaviour` serialize thành `IAwarenessSource`, log lỗi khi sai |
| `Scripts/AI/AssemblyInfo.cs` | `InternalsVisibleTo` cho test |
| `Scripts/AI/Dog/DogStates.cs`, `DogAI.cs` | khai báo mức theo trạng thái, cài interface |
| `Scripts/AI/Owner/OwnerStates.cs`, `OwnerAI.cs` | như trên |
| `Scripts/UI/Awareness/AwarenessIndicatorStyle.cs` | ScriptableObject chữ và màu theo mức |
| `Scripts/UI/Awareness/AwarenessIndicatorUI.cs` | dấu 3D trên đầu AI |
| `Scripts/UI/Hud/VisibilityEyeUI.cs` | con mắt lộ/ẩn |
| `Scripts/UI/Feedback/SpottedCameraShake.cs` | rung camera khi bị phát hiện |
| `Scripts/Gameplay/Audio/AudioCue.cs`, `CuePlayback.cs`, `AudioCuePlayer.cs` | dữ liệu một loại tiếng, chọn clip, phát |
| `Scripts/Gameplay/Audio/SoundLibrary.cs`, `SoundRules.cs` | kho cue, luật chọn cue |
| `Scripts/Gameplay/Audio/NoiseSoundPlayer.cs`, `AwarenessSoundPlayer.cs`, `ImpactSound.cs`, `MatchStingerPlayer.cs` | người phát tiếng |
| `Scripts/Editor/Greybox/GreyboxAssets.cs` (sửa), `GreyboxFeedbackFactory.cs` (mới), các factory hiện có (sửa) | dựng tự động |
| `docs/testing/M0-smoke-test.md` (sửa), `docs/audio-sources.md` (mới) | checklist F, nguồn tải âm thanh và font |

---

### Task 1: Mức cảnh giác trong Gameplay và AI

**Files:**
- Create: `Assets/_Project/Scripts/Gameplay/Awareness/AwarenessLevel.cs`, `IAwarenessSource.cs`, `AwarenessTracker.cs`
- Create: `Assets/_Project/Scripts/AI/AssemblyInfo.cs` (`[assembly: InternalsVisibleTo("DogHeist.Tests.EditMode")]`)
- Modify: `Assets/_Project/Scripts/AI/Dog/DogStates.cs`, `DogAI.cs`, `Assets/_Project/Scripts/AI/Owner/OwnerStates.cs`, `OwnerAI.cs`
- Test: `Assets/_Project/Tests/EditMode/AwarenessTests.cs`

**Interfaces:**
- Produces (namespace `DogHeist.Gameplay.Awareness`):
  - `public enum AwarenessLevel { None = 0, Sleeping = 1, Suspicious = 2, Alerted = 3, Friendly = 4 }`
  - `public interface IAwarenessSource { AwarenessLevel Awareness { get; } event Action<AwarenessLevel> AwarenessChanged; Transform IndicatorAnchor { get; } }`
  - `public sealed class AwarenessTracker { AwarenessLevel Current { get; } event Action<AwarenessLevel> Changed; void Set(AwarenessLevel level); }` — khởi tạo `Current = None`; `Set` chỉ phát `Changed` khi mức khác `Current`.
  - `DogState`/`OwnerState`: `public abstract AwarenessLevel Awareness { get; }` (bảng trong spec).
  - `DogAI`, `OwnerAI` cài `IAwarenessSource`; thêm `[SerializeField] private Transform _indicatorAnchor;` (`IndicatorAnchor` trả `_indicatorAnchor` hoặc `transform` khi trống).

- [ ] **Step 1: Viết test hỏng**

```csharp
using System.Collections.Generic;
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Awareness;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class AwarenessTests
    {
        [Test]
        public void Tracker_RaisesOnlyWhenLevelChanges()
        {
            var tracker = new AwarenessTracker();
            var raised = new List<AwarenessLevel>();
            tracker.Changed += raised.Add;

            tracker.Set(AwarenessLevel.None);
            tracker.Set(AwarenessLevel.Suspicious);
            tracker.Set(AwarenessLevel.Suspicious);
            tracker.Set(AwarenessLevel.Alerted);

            CollectionAssert.AreEqual(new[] { AwarenessLevel.Suspicious, AwarenessLevel.Alerted }, raised);
            Assert.AreEqual(AwarenessLevel.Alerted, tracker.Current);
        }

        [Test]
        public void DogStates_DeclareSpecLevels()
        {
            var dog = new GameObject("Dog").AddComponent<DogAI>();
            try
            {
                Assert.AreEqual(AwarenessLevel.None, new DogIdleState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Alerted, new DogAlertState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Friendly, new DogEatLureState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Friendly, new DogCalmState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Friendly, new DogCarriedState(dog).Awareness);
            }
            finally
            {
                Object.DestroyImmediate(dog.gameObject);
            }
        }

        [Test]
        public void OwnerStates_DeclareSpecLevels()
        {
            var owner = new GameObject("Owner").AddComponent<OwnerAI>();
            try
            {
                Assert.AreEqual(AwarenessLevel.Sleeping, new OwnerSleepingState(owner).Awareness);
                Assert.AreEqual(AwarenessLevel.Suspicious, new OwnerInvestigateState(owner).Awareness);
                Assert.AreEqual(AwarenessLevel.Suspicious, new OwnerPatrolState(owner).Awareness);
                Assert.AreEqual(AwarenessLevel.Alerted, new OwnerChaseState(owner).Awareness);
            }
            finally
            {
                Object.DestroyImmediate(owner.gameObject);
            }
        }

        [Test]
        public void AiIndicatorAnchor_FallsBackToOwnTransform()
        {
            var dog = new GameObject("Dog").AddComponent<DogAI>();
            try
            {
                Assert.AreSame(dog.transform, ((IAwarenessSource)dog).IndicatorAnchor);
            }
            finally
            {
                Object.DestroyImmediate(dog.gameObject);
            }
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter Awareness`
Expected: lỗi biên dịch `The type or namespace name 'Awareness' does not exist in the namespace 'DogHeist.Gameplay'`.

- [ ] **Step 3: Viết `AwarenessLevel`, `IAwarenessSource`, `AwarenessTracker`** theo Interfaces.

- [ ] **Step 4: Thêm `Awareness` cho các lớp trạng thái** (`abstract` trên `DogState`/`OwnerState`, `override` trên từng trạng thái theo bảng spec) và tạo `AI/AssemblyInfo.cs`.

- [ ] **Step 5: Cài `IAwarenessSource` cho `DogAI` và `OwnerAI`**

Mỗi lớp có `private readonly AwarenessTracker _awareness = new();`. Trong `Awake` (sau khi tạo state) đăng ký `_stateMachine.StateChanged += (_, next) => _awareness.Set(((DogState)next).Awareness);` (tương tự `OwnerState`). `AwarenessChanged` chuyển tiếp `add`/`remove` sang `_awareness.Changed`. Không đổi logic chuyển trạng thái.

- [ ] **Step 6: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter Awareness`
Expected: `Đạt 4 | Lỗi 0`.

- [ ] **Step 7: Chạy toàn bộ test**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1`
Expected: `Lỗi 0` (48 test cũ + 4 mới).

- [ ] **Step 8: Commit (hỏi người dùng trước)** — `feat(gameplay): thêm mức cảnh giác cho chó và chủ nhà`

---

### Task 2: Dấu trên đầu AI

**Files:**
- Create: `Assets/_Project/Scripts/Gameplay/Awareness/AwarenessSourceResolver.cs`
- Create: `Assets/_Project/Scripts/UI/Awareness/AwarenessIndicatorStyle.cs`, `AwarenessIndicatorUI.cs`
- Test: `Assets/_Project/Tests/EditMode/AwarenessIndicatorTests.cs`

**Interfaces:**
- Consumes: `AwarenessLevel`, `IAwarenessSource` (Task 1).
- Produces:
  - `public static class AwarenessSourceResolver { public static IAwarenessSource Resolve(MonoBehaviour candidate, Object context); }` — trả `null` và `Debug.LogError("[Awareness] ...", context)` khi `candidate` null hoặc không cài interface.
  - `[CreateAssetMenu(menuName = "DogHeist/Awareness Indicator Style")] public sealed class AwarenessIndicatorStyle : ScriptableObject` với `public bool TryGet(AwarenessLevel level, out string text, out Color color)`; trả `false` cho `None`. Giá trị mặc định: `Sleeping` = `"Zzz"` xám `(0.75, 0.75, 0.8)`, `Suspicious` = `"?"` vàng `(1, 0.85, 0.2)`, `Alerted` = `"!"` đỏ `(1, 0.25, 0.2)`, `Friendly` = `"♥"` hồng `(1, 0.45, 0.65)`.
  - `public sealed class AwarenessIndicatorUI : MonoBehaviour` (namespace `DogHeist.UI.Awareness`), `[RequireComponent(typeof(TextMeshPro))]`, field `[SerializeField] private MonoBehaviour _source; [SerializeField] private AwarenessIndicatorStyle _style; [SerializeField, Min(0f)] private float _popScale = 1.4f; [SerializeField, Min(0.01f)] private float _popDuration = 0.2f;`
    - `internal bool Bind(IAwarenessSource source)` — đăng ký sự kiện và áp mức hiện tại; trả `false` khi `source == null`. Lấy `TextMeshPro` bằng `GetComponent` khi cần, không dựa vào `Awake` (EditMode test không gọi `Awake`).
    - `internal void Apply(AwarenessLevel level)` — bật/tắt `TextMeshPro`, đặt chữ và màu, bắt đầu "nảy".
    - `OnEnable`: `Bind(AwarenessSourceResolver.Resolve(_source, this))`; thất bại thì `enabled = false`. `OnDisable`: hủy đăng ký. `LateUpdate`: đặt vị trí theo `IndicatorAnchor`, quay về `Camera.main` (bỏ qua khi không có camera), thu "nảy" về 1.

- [ ] **Step 1: Viết test hỏng**

```csharp
using System;
using DogHeist.Gameplay.Awareness;
using DogHeist.UI.Awareness;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DogHeist.Tests.EditMode
{
    public sealed class AwarenessIndicatorTests
    {
        private sealed class FakeSource : IAwarenessSource
        {
            public AwarenessLevel Awareness { get; set; }
            public event Action<AwarenessLevel> AwarenessChanged;
            public Transform IndicatorAnchor { get; set; }
            public void Raise(AwarenessLevel level) { Awareness = level; AwarenessChanged?.Invoke(level); }
        }

        private GameObject _host;
        private AwarenessIndicatorStyle _style;

        [SetUp]
        public void Create()
        {
            _host = new GameObject("Indicator");
            _style = ScriptableObject.CreateInstance<AwarenessIndicatorStyle>();
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_style);
        }

        [TestCase(AwarenessLevel.Sleeping, "Zzz")]
        [TestCase(AwarenessLevel.Suspicious, "?")]
        [TestCase(AwarenessLevel.Alerted, "!")]
        [TestCase(AwarenessLevel.Friendly, "♥")]
        public void Style_DefaultTexts(AwarenessLevel level, string expected)
        {
            Assert.IsTrue(_style.TryGet(level, out var text, out _));
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void Style_NoneIsHidden() => Assert.IsFalse(_style.TryGet(AwarenessLevel.None, out _, out _));

        [Test]
        public void Indicator_FollowsSourceChanges()
        {
            var indicator = CreateIndicator();
            var source = new FakeSource { Awareness = AwarenessLevel.Sleeping, IndicatorAnchor = _host.transform };

            Assert.IsTrue(indicator.Bind(source));
            var label = _host.GetComponent<TextMeshPro>();
            Assert.AreEqual("Zzz", label.text);

            source.Raise(AwarenessLevel.Alerted);
            Assert.AreEqual("!", label.text);

            source.Raise(AwarenessLevel.None);
            Assert.IsFalse(label.enabled);
        }

        [Test]
        public void Resolver_ComponentWithoutInterface_LogsAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[Awareness\]"));

            Assert.IsNull(AwarenessSourceResolver.Resolve(CreateIndicator(), _host));
        }

        [Test]
        public void Resolver_Null_LogsAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[Awareness\]"));

            Assert.IsNull(AwarenessSourceResolver.Resolve(null, _host));
        }

        private AwarenessIndicatorUI CreateIndicator()
        {
            var indicator = _host.AddComponent<AwarenessIndicatorUI>();
            var serialized = new UnityEditor.SerializedObject(indicator);
            serialized.FindProperty("_style").objectReferenceValue = _style;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return indicator;
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter AwarenessIndicator`
Expected: lỗi biên dịch `The type or namespace name 'Awareness' does not exist in the namespace 'DogHeist.UI'`.

- [ ] **Step 3: Viết `AwarenessSourceResolver`, `AwarenessIndicatorStyle`, `AwarenessIndicatorUI`** theo Interfaces. Style lưu bốn mục (chữ + màu) dạng field `[SerializeField]` có giá trị mặc định ở trên.

- [ ] **Step 4: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter AwarenessIndicator`
Expected: `Đạt 8 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(ui): thêm dấu cảnh giác trên đầu AI`

---

### Task 3: Con mắt lộ/ẩn và rung camera

**Files:**
- Create: `Assets/_Project/Scripts/UI/Hud/VisibilityEyeUI.cs`
- Create: `Assets/_Project/Scripts/UI/Feedback/SpottedCameraShake.cs`
- Modify: `Assets/_Project/Scripts/UI/DogHeist.UI.asmdef` (thêm `"Unity.Cinemachine"` vào `references`)
- Test: `Assets/_Project/Tests/EditMode/VisibilityEyeTests.cs`

**Interfaces:**
- Consumes: `ThiefVisibility.Visibility01`, `IsInLight` (có sẵn); `MatchEvents.ThiefSpotted` (có sẵn).
- Produces:
  - `public sealed class VisibilityEyeUI : MonoBehaviour` (namespace `DogHeist.UI.Hud`): `[SerializeField] private ThiefVisibility _visibility; [SerializeField] private RectTransform _eyeWhite; [SerializeField] private Image _eyeWhiteImage; [SerializeField, Range(0f, 1f)] private float _minOpenness = 0.08f; [SerializeField, Range(0f, 1f)] private float _maxOpenness = 1f; [SerializeField] private Color _darkColor = Color.white; [SerializeField] private Color _litColor = new Color(1f, 0.85f, 0.2f);`
    - `internal static float ComputeOpenness(float visibility01, float minOpenness, float maxOpenness)` — `Lerp(min, max, Clamp01(visibility01))`.
    - `Update`: đặt `localScale.y` của `_eyeWhite` = độ mở; màu `_litColor` khi `IsInLight`, ngược lại `_darkColor`.
  - `public sealed class SpottedCameraShake : MonoBehaviour` (namespace `DogHeist.UI.Feedback`): `[SerializeField] private CinemachineImpulseSource _impulse; [SerializeField, Min(0f)] private float _force = 0.6f;` — `OnEnable` đăng ký `MatchEvents.ThiefSpotted`, `OnDisable` hủy; handler gọi `_impulse.GenerateImpulseWithForce(_force)` (bỏ qua khi `_impulse` null).

- [ ] **Step 1: Viết test hỏng**

```csharp
using DogHeist.UI.Hud;
using NUnit.Framework;

namespace DogHeist.Tests.EditMode
{
    public sealed class VisibilityEyeTests
    {
        [TestCase(0f, 0.08f)]
        [TestCase(1f, 1f)]
        [TestCase(0.5f, 0.54f)]
        [TestCase(-3f, 0.08f)]
        [TestCase(7f, 1f)]
        public void ComputeOpenness_MapsVisibilityIntoRange(float visibility, float expected)
        {
            Assert.AreEqual(expected, VisibilityEyeUI.ComputeOpenness(visibility, 0.08f, 1f), 1e-4f);
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter VisibilityEye`
Expected: lỗi biên dịch `The type or namespace name 'VisibilityEyeUI' could not be found`.

- [ ] **Step 3: Viết `VisibilityEyeUI`, `SpottedCameraShake`, thêm `Unity.Cinemachine` vào UI asmdef.**

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 5 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(ui): thêm con mắt lộ/ẩn và rung camera khi bị phát hiện`

---

### Task 4: Dữ liệu âm thanh (`AudioCue`, `SoundLibrary`)

**Files:**
- Create: `Assets/_Project/Scripts/Gameplay/Audio/AudioCue.cs`, `CuePlayback.cs`, `AudioCuePlayer.cs`, `SoundLibrary.cs`
- Test: `Assets/_Project/Tests/EditMode/AudioCueTests.cs`

**Interfaces:**
- Produces (namespace `DogHeist.Gameplay.Audio`):
  - `public readonly struct CuePlayback { public CuePlayback(AudioClip clip, float volume, float pitch); AudioClip Clip; float Volume; float Pitch; }`
  - `[CreateAssetMenu(menuName = "DogHeist/Audio Cue")] public sealed class AudioCue : ScriptableObject`:
    - field: `[SerializeField] private AudioClip[] _clips = Array.Empty<AudioClip>(); [SerializeField] private Vector2 _volumeRange = new Vector2(0.9f, 1f); [SerializeField] private Vector2 _pitchRange = new Vector2(0.95f, 1.05f); [SerializeField, Range(0f, 1f)] private float _spatialBlend = 1f; [SerializeField, Min(1f)] private float _maxDistance = 25f;`
    - `public bool HasClips { get; }`, `public float SpatialBlend { get; }`, `public float MaxDistance { get; }`
    - `internal void SetClips(AudioClip[] clips)` (cho test và công cụ greybox)
    - `public CuePlayback Prepare(Func<int, int> pickIndex = null, Func<float> random01 = null)` — mặc định dùng `UnityEngine.Random`. Không có clip: trả `default` và `Debug.LogWarning("[Audio] Cue '<name>' chưa có clip ...")` **chỉ lần đầu** (field `[NonSerialized] bool _warnedEmpty`). Có từ hai clip trở lên: nếu chỉ số chọn trùng lần trước thì lấy chỉ số kế tiếp (`(i + 1) % n`).
  - `public static class AudioCuePlayer { public static void Play(AudioCue cue, AudioSource source); }` — bỏ qua khi `cue` hoặc `source` null hoặc `Prepare` trả clip null; đặt `spatialBlend`, `maxDistance`, `pitch` rồi `PlayOneShot(clip, volume)`.
  - `[CreateAssetMenu(menuName = "DogHeist/Sound Library")] public sealed class SoundLibrary : ScriptableObject` với các property `AudioCue` (`[field: SerializeField]`): `FootstepCrouch`, `FootstepWalk`, `FootstepSprint`, `DogBark`, `DogHappy`, `OwnerHuh`, `OwnerShout`, `LureLand`, `StingerWin`, `StingerLose`; và `internal void Assign(string cueName, AudioCue cue)` cho công cụ greybox (ném `ArgumentException("[Audio] ...")` khi tên sai).

- [ ] **Step 1: Viết test hỏng**

```csharp
using System.Text.RegularExpressions;
using DogHeist.Gameplay.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DogHeist.Tests.EditMode
{
    public sealed class AudioCueTests
    {
        private AudioCue _cue;

        [SetUp]
        public void Create() => _cue = ScriptableObject.CreateInstance<AudioCue>();

        [TearDown]
        public void Destroy() => Object.DestroyImmediate(_cue);

        [Test]
        public void Prepare_NeverRepeatsPreviousClip()
        {
            var a = AudioClip.Create("a", 100, 1, 44100, false);
            var b = AudioClip.Create("b", 100, 1, 44100, false);
            _cue.SetClips(new[] { a, b });

            var first = _cue.Prepare(_ => 0, () => 0.5f).Clip;
            var second = _cue.Prepare(_ => 0, () => 0.5f).Clip;

            Assert.AreSame(a, first);
            Assert.AreSame(b, second);
        }

        [Test]
        public void Prepare_VolumeAndPitchStayInRange()
        {
            _cue.SetClips(new[] { AudioClip.Create("a", 100, 1, 44100, false) });

            var low = _cue.Prepare(_ => 0, () => 0f);
            var high = _cue.Prepare(_ => 0, () => 1f);

            Assert.AreEqual(0.9f, low.Volume, 1e-4f);
            Assert.AreEqual(0.95f, low.Pitch, 1e-4f);
            Assert.AreEqual(1f, high.Volume, 1e-4f);
            Assert.AreEqual(1.05f, high.Pitch, 1e-4f);
        }

        [Test]
        public void Prepare_EmptyCue_ReturnsNoClipAndWarnsOnce()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Audio\]"));

            Assert.IsNull(_cue.Prepare().Clip);
            Assert.IsNull(_cue.Prepare().Clip);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter AudioCue`
Expected: lỗi biên dịch `The type or namespace name 'Audio' does not exist in the namespace 'DogHeist.Gameplay'`.

- [ ] **Step 3: Viết `CuePlayback`, `AudioCue`, `AudioCuePlayer`, `SoundLibrary`** theo Interfaces. Âm lượng = `Lerp(_volumeRange.x, _volumeRange.y, random01())`, pitch tương tự.

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 3 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(audio): thêm AudioCue và SoundLibrary`

---

### Task 5: Người phát tiếng

**Files:**
- Create: `Assets/_Project/Scripts/Gameplay/Audio/SoundRules.cs`, `NoiseSoundPlayer.cs`, `AwarenessSoundPlayer.cs`, `ImpactSound.cs`, `MatchStingerPlayer.cs`
- Test: `Assets/_Project/Tests/EditMode/SoundRulesTests.cs`

**Interfaces:**
- Consumes: `SoundLibrary`, `AudioCue`, `AudioCuePlayer.Play` (Task 4); `IAwarenessSource`, `AwarenessSourceResolver` (Task 1–2); `NoiseSystem.NoiseEmitted`, `NoiseEvent.Emitter`, `ThiefMotor.IsCrouching/IsSprinting`, `MatchEvents.MatchEnded`, `MatchResult.LocalPlayerWon` (có sẵn).
- Produces (namespace `DogHeist.Gameplay.Audio`):
  - `public enum AwarenessVoice { Owner, Dog }`
  - `public static class SoundRules`:
    - `public static AudioCue Footstep(SoundLibrary library, bool crouching, bool sprinting)` — lom khom ưu tiên trước chạy; còn lại là đi.
    - `public static AudioCue ForAwarenessChange(SoundLibrary library, AwarenessVoice voice, AwarenessLevel previous, AwarenessLevel next)` — Owner: `Sleeping → Suspicious` = `OwnerHuh`; mọi mức khác `Alerted` → `Alerted` = `OwnerShout`. Dog: mức khác `Friendly` → `Friendly` = `DogHappy`. Còn lại `null`.
  - `NoiseSoundPlayer : MonoBehaviour`, `[RequireComponent(typeof(AudioSource))]`: `[SerializeField] private SoundLibrary _library; [SerializeField] private NoiseSoundKind _kind;` với `public enum NoiseSoundKind { Footsteps, Bark }`. Nghe `NoiseSystem.NoiseEmitted`, chỉ xử lý khi `noise.Emitter == gameObject`. `Footsteps` đọc `ThiefMotor` trên cùng object.
  - `AwarenessSoundPlayer : MonoBehaviour`, `[RequireComponent(typeof(AudioSource))]`: `[SerializeField] private MonoBehaviour _source; [SerializeField] private SoundLibrary _library; [SerializeField] private AwarenessVoice _voice;` — nhớ mức trước (khởi tạo bằng mức hiện tại khi bind), gọi `SoundRules.ForAwarenessChange`.
  - `ImpactSound : MonoBehaviour`, `[RequireComponent(typeof(AudioSource))]`: `[SerializeField] private SoundLibrary _library;` — `OnCollisionEnter` lần đầu phát `LureLand`.
  - `MatchStingerPlayer : MonoBehaviour`, `[RequireComponent(typeof(AudioSource))]`: `[SerializeField] private SoundLibrary _library;` — `LocalPlayerWon` → `StingerWin`, ngược lại `StingerLose`.

- [ ] **Step 1: Viết test hỏng**

```csharp
using DogHeist.Gameplay.Audio;
using DogHeist.Gameplay.Awareness;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class SoundRulesTests
    {
        private SoundLibrary _library;

        [SetUp]
        public void Create()
        {
            _library = ScriptableObject.CreateInstance<SoundLibrary>();
            foreach (var name in new[] { "FootstepCrouch", "FootstepWalk", "FootstepSprint", "DogHappy", "OwnerHuh", "OwnerShout" })
            {
                var cue = ScriptableObject.CreateInstance<AudioCue>();
                cue.name = name;
                _library.Assign(name, cue);
            }
        }

        [TestCase(false, false, "FootstepWalk")]
        [TestCase(true, false, "FootstepCrouch")]
        [TestCase(false, true, "FootstepSprint")]
        [TestCase(true, true, "FootstepCrouch")]
        public void Footstep_PicksByMovement(bool crouching, bool sprinting, string expected)
        {
            Assert.AreEqual(expected, SoundRules.Footstep(_library, crouching, sprinting).name);
        }

        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Sleeping, AwarenessLevel.Suspicious, "OwnerHuh")]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Suspicious, AwarenessLevel.Alerted, "OwnerShout")]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Sleeping, AwarenessLevel.Alerted, "OwnerShout")]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.Alerted, AwarenessLevel.Friendly, "DogHappy")]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.None, AwarenessLevel.Friendly, "DogHappy")]
        public void AwarenessChange_PlaysExpectedCue(AwarenessVoice voice, AwarenessLevel from, AwarenessLevel to, string expected)
        {
            Assert.AreEqual(expected, SoundRules.ForAwarenessChange(_library, voice, from, to).name);
        }

        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Alerted, AwarenessLevel.Suspicious)]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.None, AwarenessLevel.Sleeping)]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Suspicious, AwarenessLevel.Sleeping)]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.Friendly, AwarenessLevel.Friendly)]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.None, AwarenessLevel.Alerted)]
        public void AwarenessChange_SilentCases(AwarenessVoice voice, AwarenessLevel from, AwarenessLevel to)
        {
            Assert.IsNull(SoundRules.ForAwarenessChange(_library, voice, from, to));
        }
    }
}
```

(Chó sủa đã có tiếng từ `NoiseSoundPlayer`, nên `None → Alerted` của chó im lặng ở đây để không phát hai lần.)

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter SoundRules`
Expected: lỗi biên dịch `The name 'SoundRules' does not exist in the current context`.

- [ ] **Step 3: Viết `SoundRules` và bốn component phát tiếng** theo Interfaces.

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 14 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(audio): phát tiếng bước chân, chó, chủ nhà, đồ ăn và kết thúc ván`

---

### Task 6: Asset cho công cụ greybox (cue, style, font, prefab đồ ăn)

**Files:**
- Modify: `Assets/_Project/Scripts/Editor/Greybox/GreyboxAssets.cs`
- Test: `Assets/_Project/Tests/EditMode/GreyboxFeedbackAssetsTests.cs`

**Interfaces:**
- Consumes: `AudioCue.SetClips/HasClips`, `SoundLibrary.Assign` (Task 4); `AwarenessIndicatorStyle` (Task 2); `ImpactSound` (Task 5).
- Produces — `GreyboxAssets` thêm:
  - `public SoundLibrary Sounds { get; private set; }` — `{root}/Audio/SoundLibrary.asset`; mười cue rỗng ở `{root}/Audio/Cues/<TênCue>.asset` (tên như property của `SoundLibrary`). Cue đã tồn tại thì giữ nguyên. Cue `StingerWin`/`StingerLose` tạo với `_spatialBlend = 0`.
  - `public AwarenessIndicatorStyle IndicatorStyle { get; private set; }` — `{root}/Settings/AwarenessIndicatorStyle.asset`, không ghi đè.
  - `public TMP_FontAsset UiFont { get; private set; }` — nếu có `{root}/Art/Fonts/BeVietnamPro-Regular.ttf`: tạo (hoặc nạp) `{root}/Art/Fonts/BeVietnamPro SDF.asset` bằng `TMP_FontAsset.CreateFontAsset(font)` (động), lưu atlas và material làm sub-asset, thêm `TMP_Settings.defaultFontAsset` (LiberationSans SDF) vào `fallbackFontAssetTable`. Không có file: `UiFont = null` và `Debug.LogWarning("[Greybox] Chưa có font tiếng Việt ...")`.
  - Prefab `FoodLure`: nếu chưa có `ImpactSound` thì thêm `AudioSource` (`playOnAwake = false`, `spatialBlend = 1`) và `ImpactSound` trỏ `Sounds` (cả prefab mới lẫn prefab đã có).
  - Không cần sửa asmdef: `DogHeist.Editor.asmdef` đã tham chiếu Gameplay, UI và `Unity.TextMeshPro`.

- [ ] **Step 1: Viết test hỏng**

```csharp
using System.Linq;
using System.Text.RegularExpressions;
using DogHeist.EditorTools.Greybox;
using DogHeist.Gameplay.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxFeedbackAssetsTests
    {
        private const string TempRoot = "Assets/_Project/Tests/_GreyboxFeedbackTemp";
        private const string LiberationTtf = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";

        [TearDown]
        public void DeleteTemp() => AssetDatabase.DeleteAsset(TempRoot);

        [Test]
        public void LoadOrCreate_CreatesEmptyCuesAndStyle()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Greybox\].*font"));

            var assets = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.IsNotNull(assets.Sounds.FootstepWalk);
            Assert.IsNotNull(assets.Sounds.StingerLose);
            Assert.IsFalse(assets.Sounds.DogBark.HasClips);
            Assert.AreEqual(0f, assets.Sounds.StingerWin.SpatialBlend);
            Assert.IsNotNull(assets.IndicatorStyle);
            Assert.IsNotNull(assets.LurePrefab.GetComponent<ImpactSound>());
        }

        [Test]
        public void LoadOrCreate_KeepsClipsUserAssigned()
        {
            var first = GreyboxAssets.LoadOrCreate(TempRoot);
            var clip = AudioClip.Create("bark", 100, 1, 44100, false);
            AssetDatabase.AddObjectToAsset(clip, first.Sounds.DogBark);
            first.Sounds.DogBark.SetClips(new[] { clip });
            EditorUtility.SetDirty(first.Sounds.DogBark);
            AssetDatabase.SaveAssets();

            var second = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.AreSame(first.Sounds.DogBark, second.Sounds.DogBark);
            Assert.IsTrue(second.Sounds.DogBark.HasClips);
        }

        [Test]
        public void LoadOrCreate_WithVietnameseFont_CreatesFontAssetWithFallback()
        {
            GreyboxAssets.EnsureFolder($"{TempRoot}/Art/Fonts");
            Assert.IsTrue(AssetDatabase.CopyAsset(LiberationTtf, $"{TempRoot}/Art/Fonts/BeVietnamPro-Regular.ttf"));

            var assets = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.IsNotNull(assets.UiFont);
            Assert.IsTrue(assets.UiFont.fallbackFontAssetTable.Any(f => f != null));
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxFeedbackAssets`
Expected: lỗi biên dịch `'GreyboxAssets' does not contain a definition for 'Sounds'`.

- [ ] **Step 3: Mở rộng `GreyboxAssets.LoadOrCreate`** theo Interfaces (giữ nguyên chữ ký cũ).

- [ ] **Step 4: Chạy test, xác nhận đạt** — Expected: `Đạt 3 | Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`. (Cảnh báo font trong các test greybox cũ không làm test hỏng: Unity Test Framework chỉ đánh hỏng test khi có log lỗi hoặc exception không được chờ trước.)

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(editor): tạo cue âm thanh, style dấu cảnh giác và font tiếng Việt cho greybox`

---

### Task 7: Dựng phần phản hồi vào màn greybox

**Files:**
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxFeedbackFactory.cs`
- Modify: `GreyboxCharacterFactory.cs`, `GreyboxCameraFactory.cs`, `GreyboxHudFactory.cs`, `GreyboxLevelBuilder.cs` (cùng thư mục)
- Test: `Assets/_Project/Tests/EditMode/GreyboxFeedbackTests.cs`

**Interfaces:**
- Consumes: mọi thứ từ Task 1–6; `SerializedWiring.Assign/AssignArray`, `GreyboxPrimitives.CreateEmpty` (có sẵn).
- Produces:
  - `internal static class GreyboxFeedbackFactory`:
    - `public static AwarenessIndicatorUI CreateIndicator(Component aiSource, Transform anchor, GreyboxAssets assets)` — object con `AwarenessIndicator` có `TextMeshPro` (cỡ chữ 6, căn giữa, font `assets.UiFont` nếu có), nối `_source`, `_style`.
    - `public static void AddVoice(Component aiSource, AwarenessVoice voice, GreyboxAssets assets)` — `AudioSource` (`playOnAwake = false`) + `AwarenessSoundPlayer` nối `_source`, `_library`, `_voice`.
    - `public static void AddNoiseSound(GameObject emitter, NoiseSoundKind kind, GreyboxAssets assets)`.
  - Chó: anchor con `IndicatorAnchor` tại `(0, 1.0, 0)`; chủ nhà: `(0, 2.3, 0)`; nối `_indicatorAnchor`. Chó: `AddVoice(Dog)` + `AddNoiseSound(Bark)`. Chủ nhà: `AddVoice(Owner)`. Trộm: `AddNoiseSound(Footsteps)`.
  - Camera: `CinemachineImpulseSource` trên object `Main Camera`, `CinemachineImpulseListener` trên `FreeLook Camera`, `SpottedCameraShake` nối `_impulse`.
  - `GreyboxHudFactory.Create(Transform parent, ThiefMotor thief, MatchManager match, GreyboxAssets assets)` (thêm tham số `assets`); `SpottedCameraShake` và `CinemachineImpulseSource` đặt trên object `Main Camera` do `GreyboxCameraFactory.Create` tạo.
  - HUD: `VisibilityEyeUI` tại góc trên trái (lòng trắng 64×32 dùng sprite `UI/Skin/Knob.psd`, con ngươi 20×20 màu `(0.1, 0.1, 0.15)`), chữ `VisibilityText` dời sang phải 80 px; mọi chữ TMP dùng `assets.UiFont` khi có.
  - Match: `AudioSource` (`spatialBlend = 0`) + `MatchStingerPlayer`.
  - `GreyboxBuildResult` thêm `AwarenessIndicatorUI DogIndicator`, `AwarenessIndicatorUI OwnerIndicator`.

- [ ] **Step 1: Viết test hỏng** (dùng `GreyboxSceneTestBase` có sẵn)

```csharp
using DogHeist.Gameplay.Audio;
using DogHeist.UI.Feedback;
using DogHeist.UI.Hud;
using NUnit.Framework;
using Unity.Cinemachine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxFeedbackTests : GreyboxSceneTestBase
    {
        [Test]
        public void BuildInto_AddsIndicatorsBoundToAi()
        {
            var result = Build();

            Assert.AreSame(result.Dog, ReadReference(result.DogIndicator, "_source"));
            Assert.AreSame(result.Owner, ReadReference(result.OwnerIndicator, "_source"));
            Assert.IsNotNull(ReadReference(result.OwnerIndicator, "_style"));
            Assert.IsNotNull(ReadReference(result.Dog, "_indicatorAnchor"));
        }

        [Test]
        public void BuildInto_AddsSoundPlayers()
        {
            var result = Build();

            Assert.IsNotNull(result.Thief.GetComponent<NoiseSoundPlayer>());
            Assert.IsNotNull(result.Dog.GetComponent<NoiseSoundPlayer>());
            Assert.IsNotNull(result.Dog.GetComponent<AwarenessSoundPlayer>());
            Assert.AreSame(result.Owner, ReadReference(result.Owner.GetComponent<AwarenessSoundPlayer>(), "_source"));
            Assert.IsNotNull(ReadReference(result.Match.GetComponent<MatchStingerPlayer>(), "_library"));
        }

        [Test]
        public void BuildInto_AddsEyeAndCameraShake()
        {
            var result = Build();

            var eye = result.Root.GetComponentInChildren<VisibilityEyeUI>(true);
            Assert.IsNotNull(eye);
            Assert.IsNotNull(ReadReference(eye, "_visibility"));
            var shake = result.Root.GetComponentInChildren<SpottedCameraShake>(true);
            Assert.IsNotNull(ReadReference(shake, "_impulse"));
            Assert.IsNotNull(result.Root.GetComponentInChildren<CinemachineImpulseListener>(true));
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxFeedback`
Expected: lỗi biên dịch `'GreyboxBuildResult' does not contain a definition for 'DogIndicator'`.

- [ ] **Step 3: Viết `GreyboxFeedbackFactory` và nối vào các factory, `GreyboxBuildResult`, `GreyboxLevelBuilder`** theo Interfaces.

- [ ] **Step 4: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter Greybox`
Expected: mọi test Greybox (cũ và mới) đạt, `Lỗi 0`.

- [ ] **Step 5: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 6: Commit (hỏi người dùng trước)** — `feat(editor): dựng dấu cảnh giác, con mắt, rung camera và âm thanh vào màn greybox`

---

### Task 8: Tên game, nguồn tải, checklist và chơi thử

**Files:**
- Modify: `ProjectSettings/ProjectSettings.asset` (`companyName: tienanhcapuchino`, `productName: Dog Heist`)
- Create: `docs/audio-sources.md`
- Modify: `docs/testing/M0-smoke-test.md` (thêm phần F), `CLAUDE.md` (Trạng thái hiện tại), `PLAN.md` (tích bốn ô "Phản hồi cho người chơi")

- [ ] **Step 1: Sửa `ProjectSettings.asset`** hai dòng `companyName` và `productName`. Kiểm tra: `grep -n "companyName\|productName" ProjectSettings/ProjectSettings.asset` hiện đúng giá trị mới.

- [ ] **Step 2: Viết `docs/audio-sources.md`** — bảng "Cue → gợi ý file → nguồn → giấy phép" và cách gán (kéo clip vào `Assets/_Project/Audio/Cues/<Cue>.asset`). Nguồn:
  - Bước chân: Kenney *RPG Audio* (https://kenney.nl/assets/rpg-audio, CC0), Kenney *Impact Sounds* (https://kenney.nl/assets/impact-sounds, CC0).
  - Đồ ăn rơi: Kenney *Impact Sounds* (tiếng va chạm nhẹ).
  - Nhạc thắng/thua: Kenney *Music Jingles* (https://kenney.nl/assets/music-jingles, CC0).
  - Chó sủa: OpenGameArt *dog barking mono* (https://opengameart.org/content/dog-barking-mono, CC0), *dog sounds* (https://opengameart.org/content/dog-sounds, CC0).
  - Chó vui, chủ nhà "Hửm?" và "Trộm!": Freesound lọc CC0 (https://freesound.org/search/?q=dog+whine&f=license:%22Creative+Commons+0%22, …`q=huh+male`, …`q=hey+shout`), hoặc người dùng tự thu giọng.
  - Font: Be Vietnam Pro (https://fonts.google.com/specimen/Be+Vietnam+Pro, OFL 1.1), đặt `BeVietnamPro-Regular.ttf` vào `Assets/_Project/Art/Fonts/`.
  - Ghi chú: kiểm tra lại giấy phép từng file trên trang tải trước khi dùng.

- [ ] **Step 3: Thêm phần F vào `docs/testing/M0-smoke-test.md`** đúng năm dòng ở mục "Chơi thử" của spec.

- [ ] **Step 4: Chạy toàn bộ test** — Expected: `Lỗi 0`.

- [ ] **Step 5: Người dùng chạy menu và chơi thử.** Nhờ người dùng tải font và ít nhất vài file âm thanh theo `docs/audio-sources.md`, mở Unity, chạy *DogHeist > Tools > Build Greybox Level*, chơi theo phần F, gửi kết quả. Lỗi phát sinh: dùng superpowers:systematic-debugging.

- [ ] **Step 6: Cập nhật `CLAUDE.md` và `PLAN.md`** sau khi người dùng xác nhận đạt.

- [ ] **Step 7: Commit (hỏi người dùng trước)** — gồm `ProjectSettings/ProjectSettings.asset`, docs, và asset công cụ sinh ra (`Assets/_Project/Audio`, `Assets/_Project/Settings/AwarenessIndicatorStyle.asset`, font asset, scene, prefab) kèm `.meta`. Message: `docs: nguồn âm thanh, checklist phản hồi và đổi tên game thành Dog Heist`.
