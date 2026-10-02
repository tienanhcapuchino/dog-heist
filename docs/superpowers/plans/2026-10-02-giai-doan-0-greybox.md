# Giai đoạn 0: Chạy được vòng chơi greybox — Kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mở project trong Unity, chọn menu `DogHeist > Tools > Build Greybox Level`, bấm Play và chơi trọn một ván trộm chó (trốn thoát hoặc bị bắt) trên màn khối hộp xám dựng đúng theo bản đồ `ban_do_man_choi_trom_cho.png`.

**Architecture:** Thêm assembly `DogHeist.Editor` (chỉ chạy trong Editor, là "lá" cuối cùng nên được tham chiếu mọi assembly khác). Số liệu màn chơi nằm trong `GreyboxLayout` (thuần, test được không cần scene). Các factory nhỏ dựng môi trường, nhân vật, camera, HUD dưới **một** GameObject gốc `[Greybox]`; tham chiếu được gán qua `SerializedObject` y như kéo thả trong Inspector. Chạy lại công cụ chỉ thay gốc `[Greybox]`, không đụng config người dùng đã chỉnh.

**Tech Stack:** Unity 6.3 LTS (6000.3.x), URP, C# 9, Input System, AI Navigation 2.x (`NavMeshSurface`), Cinemachine 3.x (`CinemachineCamera`), uGUI + TextMeshPro, Unity Test Framework (NUnit, EditMode), PowerShell 5.1.

**Spec:** `PLAN.md` (mục "Giai đoạn 0"), `CLAUDE.md`, `docs/GDD.md`, `docs/ARCHITECTURE.md`, `README.md` (18 bước dựng greybox), ảnh `ban_do_man_choi_trom_cho.png` (bố cục sân).

## Global Constraints

- Trả lời người dùng bằng tiếng Việt; comment và tài liệu tiếng Việt; tên class, method, biến tiếng Anh.
- Tối đa **C# 9**: không file-scoped namespace, không record struct, không raw string, **không dùng `init`** (Unity thiếu `IsExternalInit`).
- Allman, `PascalCase` cho type/method/property, `_camelCase` field private, `s_camelCase` field private static, interface bắt đầu bằng `I`; asmdef thụt lề 4 dấu cách như các file hiện có.
- Chiều phụ thuộc: `UI → Gameplay → Core`, `AI → Gameplay`; `Editor` được tham chiếu tất cả, **không assembly runtime nào được tham chiếu `DogHeist.Editor`**.
- Namespace của assembly Editor là `DogHeist.EditorTools` (không dùng `DogHeist.Editor` để tránh che mất class `UnityEditor.Editor`).
- Không `GameObject.Find`; không `catch {}` rỗng; thông báo lỗi có tiền tố (`[Greybox]`, `[Tests]`).
- Khi có `using System;` thì dùng `UnityEngine.Random`/`UnityEngine.Object` đầy đủ hoặc alias `Object = UnityEngine.Object`.
- Thông số cân bằng chỉ nằm trong `ThiefConfig`, `DogConfig`, `OwnerConfig`; công cụ greybox **không bao giờ ghi đè** config đã tồn tại.
- Không sửa code gameplay/AI/UI hiện có trong kế hoạch này (trừ khi Task 1 phát hiện lỗi biên dịch).
- **Chưa dùng git** (người dùng sẽ tự khởi tạo sau): bỏ qua mọi bước "Commit" trong kế hoạch. Khi đã có repo, mỗi commit kết thúc bằng dòng `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Claude Code không mở được Unity Editor: xác minh bằng `tools/run-unity-tests.ps1` (Unity phải đang **đóng**) hoặc nhờ người dùng chạy *Window > General > Test Runner > EditMode > Run All* rồi dán kết quả.

## Review Focus

1. **Chạy công cụ hai lần** (hoặc sau khi người dùng tự thêm vật vào scene): phải còn đúng một gốc `[Greybox]`, vật người dùng thêm ngoài gốc vẫn còn. → test ở Task 5.
2. **Config đã chỉnh tay** (ví dụ `WalkSpeed = 9`): chạy lại công cụ không được reset về mặc định. → test ở Task 4.
3. **Project thiếu điều kiện** (chưa import TMP Essentials, không có URP): công cụ phải dừng và nói rõ thiếu gì, không tạo scene hồng hoặc chữ trống. → test ở Task 6.
4. **Bố cục vô lý**: bụi cây nằm trong vùng sáng (không bao giờ ẩn được), trộm xuất phát trong vùng sáng, điểm tuần tra nằm trong tường nhà. → test ở Task 3.
5. **Nút Chơi lại**: scene chưa có trong Build Profiles thì `MatchManager.RestartMatch` báo lỗi; chạy công cụ nhiều lần không được thêm trùng scene. → test ở Task 6.

---

## Bố cục màn chơi (từ bản đồ)

Quy ước: +X là phía đông (bên phải ảnh), +Z là phía bắc (phía trên ảnh), mặt đất y = 0, khoảng 50 px = 1 m.

| Vật | Tọa độ (m) | Ghi chú |
|---|---|---|
| Sân (hàng rào) | X −14..14, Z −11..11 | Rào cao 1.5, dày 0.2 |
| Cổng | Rào nam, tâm X = −1, rộng 3 | Dẫn ra ngõ xóm |
| Lỗ hàng rào | Rào tây, tâm Z = 3.4, rộng 2 | Đường lẻn vào |
| Nhà chủ | tâm (6.9, 2, 6.2), kích thước 11.2 × 4 × 7.2 | Góc đông bắc |
| Đèn hiên | (1.3, 3.2, 1.3) chiếu về (−2, 0, −5) | Góc tây nam của nhà |
| Vùng sáng | tâm (−1.5, 1, −3.5), kích thước 6 × 2 × 9 | Phủ lối từ chuồng ra cổng |
| Chuồng chó | tâm (−8.5, 0.6, −4.8), kích thước 3 × 1.2 × 2 | |
| Chỗ chó ở (Home) | (−8.5, 0, −2.8) | Trước chuồng |
| Võng chủ nhà (Bed) | (−0.5, 0, 1.6) | Cách chó ~9.1 m để tiếng sủa (16 × 0.6 = 9.6) đánh thức được |
| Bụi cây | (−10.5, 7.8) Ø2.1; (−8.4, 8.5) Ø1.8; (11.3, −2.5) Ø1.9; (11.6, −4.6) Ø1.6 | Chỗ nấp |
| Tuần tra | (8.9, 1.3) → (8.9, −7.8) → (−4.5, −7.8) → (−6, 5) | Theo đường chấm trên ảnh |
| Trộm xuất phát | (−22, 0, 3.4), nhìn về phía đông | Ngoài lỗ hàng rào |
| Vùng thoát (ngõ) | tâm (−3, 1, −14.5), kích thước 34 × 2 × 3 | Chỉ thắng khi đang bế chó |
| Mặt đất | 60 × 60, tường vô hình quanh mép | |

"Camera" trên ảnh thuộc màn Biệt thự ở Giai đoạn 3; màn 1 chỉ có đèn hiên.

## Cấu trúc file

| File | Trách nhiệm |
|---|---|
| `tools/run-unity-tests.ps1` | Chạy Unity batchmode: kiểm tra biên dịch hoặc chạy EditMode test, in kết quả gọn |
| `Assets/_Project/Scripts/Editor/DogHeist.Editor.asmdef` | Assembly chỉ cho Editor |
| `Assets/_Project/Scripts/Editor/AssemblyInfo.cs` | Cho test thấy `internal` |
| `Assets/_Project/Scripts/Editor/Greybox/SerializedWiring.cs` | Gán field `[SerializeField] private`, báo lỗi khi sai tên/kiểu |
| `.../Greybox/GreyboxLayout.cs`, `FenceSegment.cs`, `BushSpec.cs` | Số liệu bố cục và phép chia hàng rào |
| `.../Greybox/GreyboxAssets.cs` | Tạo/nạp config, material, prefab đồ ăn, layer `Characters` |
| `.../Greybox/GreyboxPrimitives.cs` | Hàm tạo khối, trigger, GameObject rỗng, đặt layer |
| `.../Greybox/GreyboxEnvironmentFactory.cs`, `GreyboxEnvironment.cs` | Đất, rào, nhà, chuồng, bụi, đèn, vùng sáng, vùng thoát, NavMeshSurface |
| `.../Greybox/GreyboxCharacterFactory.cs` | Trộm, chó, chủ nhà và nối tham chiếu |
| `.../Greybox/GreyboxCameraFactory.cs` | Main Camera + Cinemachine bám theo trộm |
| `.../Greybox/GreyboxHudFactory.cs`, `GreyboxHud.cs` | Canvas, thanh tiếng ồn, chữ trạng thái, màn kết quả, EventSystem |
| `.../Greybox/GreyboxBuildSettings.cs` | Thêm scene vào danh sách build, không trùng |
| `.../Greybox/GreyboxLevelBuilder.cs`, `GreyboxBuildOptions.cs`, `GreyboxBuildResult.cs` | Menu, điều phối dựng, bake NavMesh, lưu scene |
| `Assets/_Project/Tests/EditMode/*Greybox*Tests.cs`, `SerializedWiringTests.cs`, `GreyboxSceneTestBase.cs` | Test |
| `docs/testing/M0-smoke-test.md` | Checklist chơi thử cho người dùng |

---

### Task 0: Tạo project Unity và git (người dùng làm, Claude hướng dẫn)

**Files:**
- Create (do Unity sinh): `Packages/`, `ProjectSettings/`, `Assets/Settings/` (URP assets của template)

Thư mục `dog-heist` hiện chỉ có `Assets/_Project`. Cách gọn nhất là để chính thư mục này thành project Unity.

- [ ] **Step 1: Cài công cụ.** Unity Hub → cài Unity **6.3 LTS** kèm *Windows Build Support (IL2CPP)*; cài Visual Studio 2022 (workload *Game development with Unity*) hoặc Rider; cài Git và Git LFS.

- [ ] **Step 2: Tạo project mẫu ở chỗ khác.** Unity Hub → *New project* → template **Universal 3D** → tên `DogHeistTemplate`, vị trí `C:\TienAnhData\Game_Trom_Cho\_template`. Đợi Unity mở xong rồi **đóng Unity**.

- [ ] **Step 3: Chép khung project vào `dog-heist`.** Chép `Packages`, `ProjectSettings` và toàn bộ `Assets` của `_template\DogHeistTemplate` vào `dog-heist` (gộp với `Assets/_Project` có sẵn, không có file trùng). Xóa `Assets/TutorialInfo` và `Assets/Readme.asset` nếu có. Lý do: giữ repo ở đúng thư mục hiện tại và giữ URP assets trong `Assets/Settings` (thiếu chúng thì mọi thứ màu hồng).

- [ ] **Step 4: Mở project.** Unity Hub → *Add > Add project from disk* → chọn `dog-heist` → mở.

- [ ] **Step 5: Cài package.** *Window > Package Manager > Unity Registry*: cài **AI Navigation** và **Cinemachine** (bản 3.x). Kiểm tra **Input System** và **Test Framework** đã có. Khi Unity hỏi bật backend Input System mới thì chọn *Yes*. Vào *Edit > Project Settings > Player > Other Settings*, đặt **Active Input Handling** = *Input System Package (New)* hoặc *Both*.

- [ ] **Step 6: Import TMP Essentials.** *Window > TextMeshPro > Import TMP Essential Resources* → *Import*.

- [ ] **Step 7: (Để sau) Khởi tạo git.** Người dùng tự làm khi sẵn sàng: `git init`, `git lfs install`, commit lần đầu. Không chặn các task khác.

- [ ] **Step 8: Gửi Claude đường dẫn Unity.exe** (ví dụ `C:\Program Files\Unity\Hub\Editor\6000.3.xf1\Editor\Unity.exe`) để Claude tự chạy test ở Task 1. Đặt biến môi trường người dùng `UNITY_EDITOR` bằng đường dẫn đó (*System Properties > Environment Variables*).

---

### Task 1: Script chạy Unity dòng lệnh, sửa lỗi biên dịch, test gốc xanh

**Files:**
- Create: `tools/run-unity-tests.ps1`
- Modify: file C# nào có lỗi biên dịch (chưa biết trước)

**Interfaces:**
- Produces: lệnh `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 [-Filter <tên>] [-CompileOnly]`; mã thoát 0 = đạt, 1 = lỗi biên dịch hoặc test hỏng, 2 = thiếu Unity.exe, 3 = Unity đang mở project.

- [ ] **Step 1: Viết script**

```powershell
# Chạy Unity ở chế độ dòng lệnh để kiểm tra biên dịch hoặc chạy EditMode test mà không cần mở Editor.
# Cách dùng (Unity Editor phải đang ĐÓNG project này):
#   powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1
#   powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter Greybox
#   powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -CompileOnly
param(
    [string]$UnityPath = $env:UNITY_EDITOR,
    [string]$Filter = "",
    [switch]$CompileOnly
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($UnityPath) -or -not (Test-Path $UnityPath))
{
    Write-Host "[Tests] Không tìm thấy Unity.exe. Đặt biến môi trường UNITY_EDITOR hoặc truyền -UnityPath."
    exit 2
}

if (Test-Path (Join-Path $projectPath "Temp/UnityLockfile"))
{
    Write-Host "[Tests] Unity Editor đang mở project này. Đóng Unity rồi chạy lại."
    Write-Host "        Nếu chắc chắn Unity đã đóng (ví dụ sau khi bị treo), xóa file Temp/UnityLockfile."
    exit 3
}

$outDir = Join-Path $projectPath "Logs/TestRuns"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$logFile = Join-Path $outDir "unity.log"
$resultsFile = Join-Path $outDir "editmode-results.xml"
if (Test-Path $resultsFile)
{
    Remove-Item $resultsFile
}

$arguments = @("-batchmode", "-nographics", "-projectPath", "`"$projectPath`"", "-logFile", "`"$logFile`"")
if ($CompileOnly)
{
    $arguments += "-quit"
}
else
{
    $arguments += @("-runTests", "-testPlatform", "EditMode", "-testResults", "`"$resultsFile`"")
    if ($Filter)
    {
        $arguments += @("-testFilter", $Filter)
    }
}

$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow

$compileErrors = @()
if (Test-Path $logFile)
{
    $compileErrors = Select-String -Path $logFile -Pattern "error CS\d+"
}

if ($compileErrors.Count -gt 0)
{
    Write-Host "[Tests] Lỗi biên dịch:"
    $compileErrors | ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique | ForEach-Object { Write-Host "  $_" }
    exit 1
}

if ($CompileOnly)
{
    Write-Host "[Tests] Biên dịch không lỗi (Unity exit code $($process.ExitCode))."
    exit $process.ExitCode
}

if (-not (Test-Path $resultsFile))
{
    Write-Host "[Tests] Không có file kết quả. Xem log: $logFile"
    exit 1
}

[xml]$results = Get-Content -Path $resultsFile -Encoding UTF8
$run = $results.'test-run'
Write-Host "[Tests] Tổng $($run.total) | Đạt $($run.passed) | Lỗi $($run.failed) | Bỏ qua $($run.skipped)"
foreach ($case in $results.SelectNodes("//test-case[@result='Failed']"))
{
    Write-Host "  FAIL $($case.fullname)"
    Write-Host "       $($case.SelectSingleNode('failure/message').InnerText)"
}

if ([int]$run.failed -gt 0)
{
    exit 1
}

exit 0
```

- [ ] **Step 2: Lưu script dạng UTF-8 có BOM** (PowerShell 5.1 đọc file không BOM theo mã ANSI và làm hỏng chữ tiếng Việt):

```powershell
$path = "tools/run-unity-tests.ps1"; $text = [IO.File]::ReadAllText($path); [IO.File]::WriteAllText($path, $text, (New-Object Text.UTF8Encoding $true))
```

- [ ] **Step 3: Kiểm tra biên dịch.** Đóng Unity, chạy:

```bash
powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -CompileOnly
```

Expected: `[Tests] Biên dịch không lỗi`. Nếu có lỗi `error CS...`: dùng skill **superpowers:systematic-debugging**, sửa từng lỗi theo quy ước (không đổi kiến trúc), chạy lại tới khi sạch. Nếu người dùng không cung cấp Unity.exe, nhờ họ mở Unity và dán nội dung Console.

- [ ] **Step 4: Chạy test gốc.**

```bash
powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1
```

Expected: tất cả test có sẵn (`StateMachineTests`, `StatsServiceTests`, `JsonFileStatsStorageTests`, `NoiseEventTests`) đạt, `Lỗi 0`.

- [ ] **Step 5: Commit**

```bash
git add tools/run-unity-tests.ps1 Assets/_Project/Scripts
git commit -m "build: thêm script chạy Unity dòng lệnh và sửa lỗi biên dịch lần đầu"
```

---

### Task 2: Assembly Editor và `SerializedWiring`

**Files:**
- Create: `Assets/_Project/Scripts/Editor/DogHeist.Editor.asmdef`
- Create: `Assets/_Project/Scripts/Editor/AssemblyInfo.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/SerializedWiring.cs`
- Modify: `Assets/_Project/Tests/EditMode/DogHeist.Tests.EditMode.asmdef`
- Test: `Assets/_Project/Tests/EditMode/SerializedWiringTests.cs`

**Interfaces:**
- Produces: `internal static class SerializedWiring` trong namespace `DogHeist.EditorTools.Greybox`:
  - `void Assign(UnityEngine.Object target, string propertyPath, UnityEngine.Object value)` — ném `ArgumentException` (thông điệp bắt đầu bằng `[Greybox]`) khi field không tồn tại, không phải tham chiếu, hoặc sai kiểu.
  - `void AssignArray(UnityEngine.Object target, string propertyPath, IReadOnlyList<UnityEngine.Object> values)`

- [ ] **Step 1: Cập nhật asmdef test** để tham chiếu thêm các assembly sẽ dùng:

```json
{
    "name": "DogHeist.Tests.EditMode",
    "rootNamespace": "DogHeist.Tests.EditMode",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "DogHeist.Core",
        "DogHeist.Gameplay",
        "DogHeist.AI",
        "DogHeist.UI",
        "DogHeist.Editor",
        "Unity.AI.Navigation",
        "Unity.Cinemachine"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Viết test hỏng**

```csharp
using System;
using DogHeist.AI.Owner;
using DogHeist.EditorTools.Greybox;
using DogHeist.Gameplay.Thief;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DogHeist.Tests.EditMode
{
    public sealed class SerializedWiringTests
    {
        private GameObject _host;

        [SetUp]
        public void CreateHost() => _host = new GameObject("WiringTestHost");

        [TearDown]
        public void DestroyHost() => Object.DestroyImmediate(_host);

        [Test]
        public void Assign_SetsPrivateSerializedField()
        {
            var motor = _host.AddComponent<ThiefMotor>();
            var config = ScriptableObject.CreateInstance<ThiefConfig>();

            SerializedWiring.Assign(motor, "_config", config);

            Assert.AreSame(config, motor.Config);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Assign_UnknownField_Throws()
        {
            var motor = _host.AddComponent<ThiefMotor>();

            var error = Assert.Throws<ArgumentException>(() => SerializedWiring.Assign(motor, "_doesNotExist", null));
            StringAssert.StartsWith("[Greybox]", error.Message);
        }

        [Test]
        public void Assign_WrongType_Throws()
        {
            var motor = _host.AddComponent<ThiefMotor>();

            Assert.Throws<ArgumentException>(() => SerializedWiring.Assign(motor, "_config", _host.transform));
        }

        [Test]
        public void AssignArray_SetsAllElements()
        {
            var owner = _host.AddComponent<OwnerAI>();
            var first = new GameObject("WaypointA").transform;
            var second = new GameObject("WaypointB").transform;

            SerializedWiring.AssignArray(owner, "_patrolWaypoints", new Object[] { first, second });

            var property = new SerializedObject(owner).FindProperty("_patrolWaypoints");
            Assert.AreEqual(2, property.arraySize);
            Assert.AreSame(second, property.GetArrayElementAtIndex(1).objectReferenceValue);
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }
    }
}
```

- [ ] **Step 3: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter SerializedWiring`
Expected: lỗi biên dịch `error CS0246: The type or namespace name 'DogHeist' ... EditorTools` (chưa có assembly Editor).

- [ ] **Step 4: Tạo asmdef Editor**

```json
{
    "name": "DogHeist.Editor",
    "rootNamespace": "DogHeist.EditorTools",
    "references": [
        "DogHeist.Core",
        "DogHeist.Gameplay",
        "DogHeist.AI",
        "DogHeist.UI",
        "Unity.AI.Navigation",
        "Unity.Cinemachine",
        "Unity.InputSystem",
        "Unity.TextMeshPro",
        "UnityEngine.UI"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

`AssemblyInfo.cs`:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DogHeist.Tests.EditMode")]
```

- [ ] **Step 5: Viết `SerializedWiring`**

```csharp
using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Gán tham chiếu vào field [SerializeField] private bằng SerializedObject, giống thao tác kéo thả trong Inspector.
    /// Ném lỗi ngay khi field không tồn tại hoặc sai kiểu, để phát hiện sớm khi field trong code gameplay bị đổi tên.
    /// </summary>
    internal static class SerializedWiring
    {
        public static void Assign(Object target, string propertyPath, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = FindReferenceProperty(serialized, target, propertyPath);

            property.objectReferenceValue = value;
            EnsureAccepted(property, target, propertyPath, value);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AssignArray(Object target, string propertyPath, IReadOnlyList<Object> values)
        {
            var serialized = new SerializedObject(target);
            var property = FindProperty(serialized, target, propertyPath);
            if (!property.isArray)
            {
                throw new ArgumentException($"[Greybox] {target.GetType().Name}.{propertyPath} không phải mảng.", nameof(propertyPath));
            }

            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.objectReferenceValue = values[i];
                EnsureAccepted(element, target, $"{propertyPath}[{i}]", values[i]);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty FindReferenceProperty(SerializedObject serialized, Object target, string propertyPath)
        {
            var property = FindProperty(serialized, target, propertyPath);
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new ArgumentException($"[Greybox] {target.GetType().Name}.{propertyPath} không phải field tham chiếu.", nameof(propertyPath));
            }

            return property;
        }

        private static SerializedProperty FindProperty(SerializedObject serialized, Object target, string propertyPath)
        {
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                throw new ArgumentException(
                    $"[Greybox] {target.GetType().Name} không có field '{propertyPath}'. Field có bị đổi tên không?",
                    nameof(propertyPath));
            }

            return property;
        }

        // Unity âm thầm gán null khi kiểu không khớp, nên phải kiểm tra lại sau khi gán.
        private static void EnsureAccepted(SerializedProperty property, Object target, string propertyPath, Object value)
        {
            if (value != null && property.objectReferenceValue != value)
            {
                throw new ArgumentException(
                    $"[Greybox] {target.GetType().Name}.{propertyPath} không nhận kiểu {value.GetType().Name}.",
                    nameof(value));
            }
        }
    }
}
```

- [ ] **Step 6: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter SerializedWiring`
Expected: `Đạt 4 | Lỗi 0`.

- [ ] **Step 7: Commit**

```bash
git add Assets/_Project/Scripts/Editor Assets/_Project/Tests/EditMode
git commit -m "feat(editor): thêm assembly DogHeist.Editor và SerializedWiring"
```

---

### Task 3: `GreyboxLayout` — số liệu bố cục và phép chia hàng rào

**Files:**
- Create: `Assets/_Project/Scripts/Editor/Greybox/FenceSegment.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/BushSpec.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxLayout.cs`
- Test: `Assets/_Project/Tests/EditMode/GreyboxLayoutTests.cs`

**Interfaces:**
- Produces:
  - `internal readonly struct FenceSegment { Vector3 Center; Vector3 Size; bool ContainsXZ(Vector3 point); }`
  - `internal readonly struct BushSpec { Vector3 Position; float Diameter; }`
  - `internal static class GreyboxLayout` với các hằng/field trong bảng "Bố cục màn chơi", cộng: `IReadOnlyList<FenceSegment> BuildFenceSegments()`, `List<(float From, float To)> SplitAroundGap(float min, float max, float gapCenter, float gapWidth)`, `bool IsInsideYard(Vector3)`, `bool IsOnGround(Vector3)`, `bool IsInsideBoxXZ(Vector3 point, Vector3 center, Vector3 size)`.

- [ ] **Step 1: Viết test hỏng**

```csharp
using System.Linq;
using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxLayoutTests
    {
        [Test]
        public void SplitAroundGap_NoGap_ReturnsWholeEdge()
        {
            var pieces = GreyboxLayout.SplitAroundGap(-5f, 5f, 0f, 0f);

            Assert.AreEqual(1, pieces.Count);
            Assert.AreEqual((-5f, 5f), pieces[0]);
        }

        [Test]
        public void SplitAroundGap_MiddleGap_ReturnsTwoPiecesAroundGap()
        {
            var pieces = GreyboxLayout.SplitAroundGap(-5f, 5f, 1f, 2f);

            Assert.AreEqual(2, pieces.Count);
            Assert.AreEqual((-5f, 0f), pieces[0]);
            Assert.AreEqual((2f, 5f), pieces[1]);
        }

        [Test]
        public void SplitAroundGap_GapWiderThanEdge_ReturnsNothing()
        {
            Assert.IsEmpty(GreyboxLayout.SplitAroundGap(-1f, 1f, 0f, 10f));
        }

        [Test]
        public void FenceSegments_LeaveGateOpen()
        {
            var gate = new Vector3(GreyboxLayout.GateCenterX, 0f, GreyboxLayout.YardMinZ);

            Assert.IsFalse(GreyboxLayout.BuildFenceSegments().Any(segment => segment.ContainsXZ(gate)));
        }

        [Test]
        public void FenceSegments_LeaveHoleOpen()
        {
            var hole = new Vector3(GreyboxLayout.YardMinX, 0f, GreyboxLayout.FenceHoleCenterZ);

            Assert.IsFalse(GreyboxLayout.BuildFenceSegments().Any(segment => segment.ContainsXZ(hole)));
        }

        [TestCase(10f, -11f)]
        [TestCase(-14f, -5f)]
        [TestCase(0f, 11f)]
        [TestCase(14f, 0f)]
        public void FenceSegments_CloseYardElsewhere(float x, float z)
        {
            var point = new Vector3(x, 0f, z);

            Assert.IsTrue(GreyboxLayout.BuildFenceSegments().Any(segment => segment.ContainsXZ(point)));
        }

        [Test]
        public void ThiefSpawn_IsOutsideYardButOnGround()
        {
            Assert.IsFalse(GreyboxLayout.IsInsideYard(GreyboxLayout.ThiefSpawn));
            Assert.IsTrue(GreyboxLayout.IsOnGround(GreyboxLayout.ThiefSpawn));
        }

        [Test]
        public void EscapeZone_IsOutsideYardInFrontOfGate()
        {
            Assert.Less(GreyboxLayout.EscapeZoneCenter.z, GreyboxLayout.YardMinZ);
            var inFrontOfGate = new Vector3(GreyboxLayout.GateCenterX, 0f, GreyboxLayout.EscapeZoneCenter.z);
            Assert.IsTrue(GreyboxLayout.IsInsideBoxXZ(inFrontOfGate, GreyboxLayout.EscapeZoneCenter, GreyboxLayout.EscapeZoneSize));
        }

        [Test]
        public void KeyPoints_AreInsideYardAndNotInsideBuildings()
        {
            var points = GreyboxLayout.PatrolWaypoints
                .Append(GreyboxLayout.DogHome)
                .Append(GreyboxLayout.OwnerBed);

            foreach (var point in points)
            {
                Assert.IsTrue(GreyboxLayout.IsInsideYard(point), $"{point} nằm ngoài sân");
                Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(point, GreyboxLayout.HouseCenter, GreyboxLayout.HouseSize), $"{point} nằm trong nhà");
                Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(point, GreyboxLayout.KennelCenter, GreyboxLayout.KennelSize), $"{point} nằm trong chuồng");
            }
        }

        [Test]
        public void Bushes_AreNotLit()
        {
            foreach (var bush in GreyboxLayout.Bushes)
            {
                Assert.IsFalse(
                    GreyboxLayout.IsInsideBoxXZ(bush.Position, GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize),
                    $"Bụi cây tại {bush.Position} nằm trong vùng sáng nên không bao giờ ẩn được");
            }
        }

        [Test]
        public void ThiefSpawnAndDogHome_AreNotLit()
        {
            Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(GreyboxLayout.ThiefSpawn, GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize));
            Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(GreyboxLayout.DogHome, GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize));
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxLayout`
Expected: lỗi biên dịch `The name 'GreyboxLayout' does not exist`.

- [ ] **Step 3: Viết hai struct**

`FenceSegment.cs`:

```csharp
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Một đoạn hàng rào hình hộp: tâm và kích thước tính theo mét.</summary>
    internal readonly struct FenceSegment
    {
        public FenceSegment(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        public Vector3 Center { get; }

        public Vector3 Size { get; }

        public bool ContainsXZ(Vector3 point) => GreyboxLayout.IsInsideBoxXZ(point, Center, Size);
    }
}
```

`BushSpec.cs`:

```csharp
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Vị trí (trên mặt đất) và đường kính của một bụi cây.</summary>
    internal readonly struct BushSpec
    {
        public BushSpec(Vector3 position, float diameter)
        {
            Position = position;
            Diameter = diameter;
        }

        public Vector3 Position { get; }

        public float Diameter { get; }
    }
}
```

- [ ] **Step 4: Viết `GreyboxLayout`**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Tọa độ màn greybox Level01, đo từ ban_do_man_choi_trom_cho.png (khoảng 50 px = 1 m).
    /// Quy ước: +X là phía đông (bên phải ảnh), +Z là phía bắc (phía trên ảnh), mặt đất ở y = 0.
    /// Chỉ chứa số liệu và phép tính thuần nên test được mà không cần scene.
    /// </summary>
    internal static class GreyboxLayout
    {
        public const float YardMinX = -14f;
        public const float YardMaxX = 14f;
        public const float YardMinZ = -11f;
        public const float YardMaxZ = 11f;

        public const float FenceHeight = 1.5f;
        public const float FenceThickness = 0.2f;

        // Cổng ở hàng rào phía nam, lỗ hổng ở hàng rào phía tây.
        public const float GateCenterX = -1f;
        public const float GateWidth = 3f;
        public const float FenceHoleCenterZ = 3.4f;
        public const float FenceHoleWidth = 2f;

        public const float GroundSize = 60f;
        public const float BoundaryHeight = 3f;

        public static readonly Vector3 ThiefSpawn = new Vector3(-22f, 0f, 3.4f);

        public static readonly Vector3 HouseCenter = new Vector3(6.9f, 2f, 6.2f);
        public static readonly Vector3 HouseSize = new Vector3(11.2f, 4f, 7.2f);

        public static readonly Vector3 KennelCenter = new Vector3(-8.5f, 0.6f, -4.8f);
        public static readonly Vector3 KennelSize = new Vector3(3f, 1.2f, 2f);

        public static readonly Vector3 DogHome = new Vector3(-8.5f, 0f, -2.8f);

        // Đủ gần chuồng để tiếng sủa (16 m x độ thính lúc ngủ 0.6) đánh thức được chủ nhà.
        public static readonly Vector3 OwnerBed = new Vector3(-0.5f, 0f, 1.6f);

        public static readonly Vector3 PorchLightPosition = new Vector3(1.3f, 3.2f, 1.3f);
        public static readonly Vector3 PorchLightTarget = new Vector3(-2f, 0f, -5f);

        public static readonly Vector3 LightZoneCenter = new Vector3(-1.5f, 1f, -3.5f);
        public static readonly Vector3 LightZoneSize = new Vector3(6f, 2f, 9f);

        public static readonly Vector3 EscapeZoneCenter = new Vector3(-3f, 1f, -14.5f);
        public static readonly Vector3 EscapeZoneSize = new Vector3(34f, 2f, 3f);

        public static readonly BushSpec[] Bushes =
        {
            new BushSpec(new Vector3(-10.5f, 0f, 7.8f), 2.1f),
            new BushSpec(new Vector3(-8.4f, 0f, 8.5f), 1.8f),
            new BushSpec(new Vector3(11.3f, 0f, -2.5f), 1.9f),
            new BushSpec(new Vector3(11.6f, 0f, -4.6f), 1.6f)
        };

        public static readonly Vector3[] PatrolWaypoints =
        {
            new Vector3(8.9f, 0f, 1.3f),
            new Vector3(8.9f, 0f, -7.8f),
            new Vector3(-4.5f, 0f, -7.8f),
            new Vector3(-6f, 0f, 5f)
        };

        public static IReadOnlyList<FenceSegment> BuildFenceSegments()
        {
            var segments = new List<FenceSegment>();
            AddEdgeAlongX(segments, YardMaxZ, 0f, 0f);
            AddEdgeAlongX(segments, YardMinZ, GateCenterX, GateWidth);
            AddEdgeAlongZ(segments, YardMaxX, 0f, 0f);
            AddEdgeAlongZ(segments, YardMinX, FenceHoleCenterZ, FenceHoleWidth);
            return segments;
        }

        /// <summary>Chia đoạn [min, max] thành các phần nằm ngoài khoảng trống. gapWidth = 0 nghĩa là không có khoảng trống.</summary>
        public static List<(float From, float To)> SplitAroundGap(float min, float max, float gapCenter, float gapWidth)
        {
            var pieces = new List<(float From, float To)>();
            if (gapWidth <= 0f)
            {
                pieces.Add((min, max));
                return pieces;
            }

            var gapStart = Mathf.Clamp(gapCenter - gapWidth * 0.5f, min, max);
            var gapEnd = Mathf.Clamp(gapCenter + gapWidth * 0.5f, min, max);

            if (gapStart > min)
            {
                pieces.Add((min, gapStart));
            }

            if (gapEnd < max)
            {
                pieces.Add((gapEnd, max));
            }

            return pieces;
        }

        public static bool IsInsideYard(Vector3 point) =>
            point.x > YardMinX && point.x < YardMaxX && point.z > YardMinZ && point.z < YardMaxZ;

        public static bool IsOnGround(Vector3 point) =>
            Mathf.Abs(point.x) < GroundSize * 0.5f && Mathf.Abs(point.z) < GroundSize * 0.5f;

        public static bool IsInsideBoxXZ(Vector3 point, Vector3 center, Vector3 size) =>
            Mathf.Abs(point.x - center.x) <= size.x * 0.5f && Mathf.Abs(point.z - center.z) <= size.z * 0.5f;

        private static void AddEdgeAlongX(List<FenceSegment> segments, float z, float gapCenter, float gapWidth)
        {
            foreach (var (from, to) in SplitAroundGap(YardMinX, YardMaxX, gapCenter, gapWidth))
            {
                segments.Add(new FenceSegment(
                    new Vector3((from + to) * 0.5f, FenceHeight * 0.5f, z),
                    new Vector3(to - from, FenceHeight, FenceThickness)));
            }
        }

        private static void AddEdgeAlongZ(List<FenceSegment> segments, float x, float gapCenter, float gapWidth)
        {
            foreach (var (from, to) in SplitAroundGap(YardMinZ, YardMaxZ, gapCenter, gapWidth))
            {
                segments.Add(new FenceSegment(
                    new Vector3(x, FenceHeight * 0.5f, (from + to) * 0.5f),
                    new Vector3(FenceThickness, FenceHeight, to - from)));
            }
        }
    }
}
```

- [ ] **Step 5: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxLayout`
Expected: `Lỗi 0` (14 test case).

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Editor/Greybox Assets/_Project/Tests/EditMode/GreyboxLayoutTests.cs
git commit -m "feat(editor): thêm số liệu bố cục màn greybox theo bản đồ"
```

---

### Task 4: `GreyboxAssets` — config, material, prefab đồ ăn, layer

**Files:**
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxAssets.cs`
- Test: `Assets/_Project/Tests/EditMode/GreyboxAssetsTests.cs`

**Interfaces:**
- Consumes: không.
- Produces: `internal sealed class GreyboxAssets` với:
  - `const string DefaultAssetRoot = "Assets/_Project"`, `const string CharactersLayerName = "Characters"`
  - `static GreyboxAssets LoadOrCreate(string assetRoot)`
  - `static int EnsureLayer(string layerName)`, `static void EnsureFolder(string folderPath)`
  - Property: `ThiefConfig ThiefConfig`, `DogConfig DogConfig`, `OwnerConfig OwnerConfig`, `FoodLure LurePrefab`, `int CharactersLayer`, `Material GroundMaterial, FenceMaterial, BuildingMaterial, BushMaterial, ThiefMaterial, DogMaterial, OwnerMaterial, LureMaterial`.
  - Đường dẫn asset: `{root}/Settings/Configs/ThiefConfig.asset` (và `DogConfig.asset`, `OwnerConfig.asset`), `{root}/Prefabs/Items/FoodLure.prefab`, `{root}/Art/Materials/Greybox/<Tên>.mat`.

- [ ] **Step 1: Viết test hỏng**

```csharp
using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxAssetsTests
    {
        private const string TempRoot = "Assets/_Project/Tests/_GreyboxAssetsTemp";

        [TearDown]
        public void DeleteTempAssets() => AssetDatabase.DeleteAsset(TempRoot);

        [Test]
        public void LoadOrCreate_CreatesConfigsPrefabAndMaterials()
        {
            var assets = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<ScriptableObject>($"{TempRoot}/Settings/Configs/ThiefConfig.asset"));
            Assert.IsNotNull(assets.DogConfig);
            Assert.IsNotNull(assets.OwnerConfig);
            Assert.IsNotNull(assets.LurePrefab);
            Assert.IsNotNull(assets.LurePrefab.GetComponent<Rigidbody>());
            Assert.IsNotNull(assets.GroundMaterial.shader);
        }

        [Test]
        public void LoadOrCreate_DoesNotOverwriteExistingConfig()
        {
            var first = GreyboxAssets.LoadOrCreate(TempRoot);
            var serialized = new SerializedObject(first.ThiefConfig);
            serialized.FindProperty("<WalkSpeed>k__BackingField").floatValue = 9f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            var second = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.AreSame(first.ThiefConfig, second.ThiefConfig);
            Assert.AreEqual(9f, second.ThiefConfig.WalkSpeed);
        }

        [Test]
        public void EnsureLayer_ReturnsSameIndexOnSecondCall()
        {
            var first = GreyboxAssets.EnsureLayer(GreyboxAssets.CharactersLayerName);
            var second = GreyboxAssets.EnsureLayer(GreyboxAssets.CharactersLayerName);

            Assert.AreEqual(first, second);
            Assert.AreEqual(first, LayerMask.NameToLayer(GreyboxAssets.CharactersLayerName));
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxAssets`
Expected: lỗi biên dịch `The name 'GreyboxAssets' does not exist`.

- [ ] **Step 3: Viết `GreyboxAssets`**

```csharp
using System;
using System.IO;
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Items;
using DogHeist.Gameplay.Thief;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Nạp hoặc tạo các asset mà màn greybox cần. Asset đã tồn tại thì giữ nguyên,
    /// để thông số người dùng đã cân bằng trong config không bị reset khi dựng lại màn.
    /// </summary>
    internal sealed class GreyboxAssets
    {
        public const string DefaultAssetRoot = "Assets/_Project";
        public const string CharactersLayerName = "Characters";

        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string FallbackShaderName = "Standard";
        private const int FirstUserLayer = 8;
        private const int LayerCount = 32;

        public ThiefConfig ThiefConfig { get; private set; }

        public DogConfig DogConfig { get; private set; }

        public OwnerConfig OwnerConfig { get; private set; }

        public FoodLure LurePrefab { get; private set; }

        public int CharactersLayer { get; private set; }

        public Material GroundMaterial { get; private set; }

        public Material FenceMaterial { get; private set; }

        public Material BuildingMaterial { get; private set; }

        public Material BushMaterial { get; private set; }

        public Material ThiefMaterial { get; private set; }

        public Material DogMaterial { get; private set; }

        public Material OwnerMaterial { get; private set; }

        public Material LureMaterial { get; private set; }

        public static GreyboxAssets LoadOrCreate(string assetRoot)
        {
            var configFolder = $"{assetRoot}/Settings/Configs";
            var materialFolder = $"{assetRoot}/Art/Materials/Greybox";
            var itemFolder = $"{assetRoot}/Prefabs/Items";
            EnsureFolder(configFolder);
            EnsureFolder(materialFolder);
            EnsureFolder(itemFolder);

            var assets = new GreyboxAssets
            {
                CharactersLayer = EnsureLayer(CharactersLayerName),
                ThiefConfig = LoadOrCreateConfig<ThiefConfig>($"{configFolder}/ThiefConfig.asset"),
                DogConfig = LoadOrCreateConfig<DogConfig>($"{configFolder}/DogConfig.asset"),
                OwnerConfig = LoadOrCreateConfig<OwnerConfig>($"{configFolder}/OwnerConfig.asset"),
                GroundMaterial = LoadOrCreateMaterial($"{materialFolder}/Ground.mat", new Color(0.20f, 0.32f, 0.20f)),
                FenceMaterial = LoadOrCreateMaterial($"{materialFolder}/Fence.mat", new Color(0.45f, 0.35f, 0.25f)),
                BuildingMaterial = LoadOrCreateMaterial($"{materialFolder}/Building.mat", new Color(0.75f, 0.72f, 0.65f)),
                BushMaterial = LoadOrCreateMaterial($"{materialFolder}/Bush.mat", new Color(0.15f, 0.45f, 0.18f)),
                ThiefMaterial = LoadOrCreateMaterial($"{materialFolder}/Thief.mat", new Color(0.15f, 0.20f, 0.45f)),
                DogMaterial = LoadOrCreateMaterial($"{materialFolder}/Dog.mat", new Color(0.95f, 0.75f, 0.30f)),
                OwnerMaterial = LoadOrCreateMaterial($"{materialFolder}/Owner.mat", new Color(0.80f, 0.25f, 0.20f)),
                LureMaterial = LoadOrCreateMaterial($"{materialFolder}/Lure.mat", new Color(0.60f, 0.35f, 0.15f))
            };

            assets.LurePrefab = LoadOrCreateLurePrefab($"{itemFolder}/FoodLure.prefab", assets.LureMaterial);
            AssetDatabase.SaveAssets();
            return assets;
        }

        public static int EnsureLayer(string layerName)
        {
            var existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0)
            {
                return existing;
            }

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (var i = FirstUserLayer; i < LayerCount; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(layer.stringValue))
                {
                    continue;
                }

                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }

            throw new InvalidOperationException(
                $"[Greybox] Không còn chỗ trống cho layer '{layerName}' (layer 8–31 đã dùng hết). Xóa bớt layer trong Tags and Layers.");
        }

        public static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
            {
                throw new ArgumentException($"[Greybox] Đường dẫn thư mục không hợp lệ: {folderPath}", nameof(folderPath));
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        private static T LoadOrCreateConfig<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static Material LoadOrCreateMaterial(string path, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogWarning("[Greybox] Không tìm thấy shader URP Lit, dùng shader Standard. Project có tạo từ template Universal 3D không?");
                shader = Shader.Find(FallbackShaderName);
            }

            var material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static FoodLure LoadOrCreateLurePrefab(string path, Material material)
        {
            var existing = AssetDatabase.LoadAssetAtPath<FoodLure>(path);
            if (existing != null)
            {
                return existing;
            }

            var temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            try
            {
                temporary.name = "FoodLure";
                temporary.transform.localScale = Vector3.one * 0.3f;
                temporary.GetComponent<Renderer>().sharedMaterial = material;
                var body = temporary.AddComponent<Rigidbody>();
                body.mass = 0.2f;
                body.collisionDetectionMode = CollisionDetectionMode.Continuous;
                temporary.AddComponent<FoodLure>();

                var prefab = PrefabUtility.SaveAsPrefabAsset(temporary, path);
                return prefab.GetComponent<FoodLure>();
            }
            finally
            {
                Object.DestroyImmediate(temporary);
            }
        }
    }
}
```

- [ ] **Step 4: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxAssets`
Expected: `Đạt 3 | Lỗi 0`.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/Editor/Greybox/GreyboxAssets.cs Assets/_Project/Tests/EditMode/GreyboxAssetsTests.cs
git commit -m "feat(editor): tạo config, material, prefab đồ ăn và layer cho greybox"
```

---

### Task 5: Dựng môi trường và nhân vật (`GreyboxLevelBuilder.BuildInto`)

**Files:**
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxPrimitives.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxEnvironment.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxEnvironmentFactory.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxCharacterFactory.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxBuildOptions.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxBuildResult.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxLevelBuilder.cs`
- Test: `Assets/_Project/Tests/EditMode/GreyboxSceneTestBase.cs`
- Test: `Assets/_Project/Tests/EditMode/GreyboxLevelBuilderTests.cs`

**Interfaces:**
- Consumes: `GreyboxLayout` (Task 3), `GreyboxAssets.LoadOrCreate`, `.CharactersLayer`, các config/material/prefab (Task 4), `SerializedWiring.Assign/AssignArray` (Task 2).
- Field gameplay được gán (tên phải khớp code hiện có): `ThiefMotor._config`, `ThiefMotor._cameraTransform` (Task 6), `ThiefVisibility._motor`, `ThiefInteractor._motor/_carryAnchor/_throwOrigin/_lurePrefab`, `DogAI._config/_hearing/_thief/_home`, `OwnerAI._config/_hearing/_vision/_thief/_bed/_patrolWaypoints`, `VisionSensor._eye`, `MatchManager._thief`.
- Produces:
  - `internal static class GreyboxLevelBuilder { const string RootName = "[Greybox]"; static GreyboxBuildResult BuildInto(Scene scene, GreyboxBuildOptions options); }`
  - `internal sealed class GreyboxBuildOptions { string AssetRoot (mặc định GreyboxAssets.DefaultAssetRoot); }`
  - `internal sealed class GreyboxBuildResult { GameObject Root; ThiefMotor Thief; DogAI Dog; OwnerAI Owner; MatchManager Match; NavMeshSurface NavMesh; }` (Task 6 thêm `Camera MainCamera`, `GreyboxHud Hud`)
  - `internal sealed class GreyboxEnvironment { Transform DogHome; Transform OwnerBed; Transform[] PatrolWaypoints; NavMeshSurface NavMesh; }`
  - `GreyboxPrimitives.Create / CreateEmpty / CreateTrigger / SetLayerRecursively` (chữ ký ở Step 3).

- [ ] **Step 1: Viết lớp test nền và test hỏng**

`GreyboxSceneTestBase.cs`:

```csharp
using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DogHeist.Tests.EditMode
{
    /// <summary>
    /// Mỗi test dựng màn vào một scene trống mới (Test Runner tự khôi phục scene của người dùng sau khi chạy xong).
    /// Asset tạo ra nằm trong thư mục tạm và bị xóa sau mỗi test.
    /// </summary>
    public abstract class GreyboxSceneTestBase
    {
        protected const string TempAssetRoot = "Assets/_Project/Tests/_GreyboxSceneTemp";

        protected Scene Scene { get; private set; }

        [SetUp]
        public void CreateEmptyScene() =>
            Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        [TearDown]
        public void DeleteTempAssets() => AssetDatabase.DeleteAsset(TempAssetRoot);

        internal GreyboxBuildResult Build() =>
            GreyboxLevelBuilder.BuildInto(Scene, new GreyboxBuildOptions { AssetRoot = TempAssetRoot });

        protected static Object ReadReference(Object target, string propertyPath) =>
            new SerializedObject(target).FindProperty(propertyPath).objectReferenceValue;
    }
}
```

`GreyboxLevelBuilderTests.cs`:

```csharp
using System.Linq;
using DogHeist.EditorTools.Greybox;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Stealth;
using DogHeist.Gameplay.Thief;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxLevelBuilderTests : GreyboxSceneTestBase
    {
        [Test]
        public void BuildInto_WiresThief()
        {
            var result = Build();
            var interactor = result.Thief.GetComponent<ThiefInteractor>();

            Assert.IsNotNull(result.Thief.Config);
            Assert.IsNotNull(ReadReference(interactor, "_lurePrefab"));
            Assert.IsNotNull(ReadReference(interactor, "_carryAnchor"));
            Assert.IsNotNull(ReadReference(interactor, "_throwOrigin"));
            Assert.AreSame(result.Thief, ReadReference(result.Thief.GetComponent<ThiefVisibility>(), "_motor"));
        }

        [Test]
        public void BuildInto_WiresDogAndOwnerToThief()
        {
            var result = Build();
            var visibility = result.Thief.GetComponent<ThiefVisibility>();

            Assert.AreSame(visibility, ReadReference(result.Dog, "_thief"));
            Assert.IsNotNull(ReadReference(result.Dog, "_hearing"));
            Assert.IsNotNull(ReadReference(result.Dog, "_home"));
            Assert.AreSame(visibility, ReadReference(result.Owner, "_thief"));
            Assert.IsNotNull(ReadReference(result.Owner, "_vision"));
            Assert.IsNotNull(ReadReference(result.Owner, "_bed"));
            var waypoints = new SerializedObject(result.Owner).FindProperty("_patrolWaypoints");
            Assert.AreEqual(GreyboxLayout.PatrolWaypoints.Length, waypoints.arraySize);
        }

        [Test]
        public void BuildInto_MatchManagerReferencesThief()
        {
            var result = Build();

            Assert.AreSame(result.Thief, ReadReference(result.Match, "_thief"));
        }

        [Test]
        public void BuildInto_PutsCharactersAndChildrenOnCharactersLayer()
        {
            var result = Build();
            var layer = LayerMask.NameToLayer(GreyboxAssets.CharactersLayerName);

            foreach (var character in new Component[] { result.Thief, result.Dog, result.Owner })
            {
                Assert.IsTrue(character.GetComponentsInChildren<Transform>().All(t => t.gameObject.layer == layer), character.name);
            }
        }

        [Test]
        public void BuildInto_CreatesFencesHidingSpotsAndZones()
        {
            var result = Build();

            var fenceCount = result.Root.GetComponentsInChildren<Transform>().Count(t => t.name.StartsWith("Fence"));
            Assert.AreEqual(GreyboxLayout.BuildFenceSegments().Count, fenceCount);
            Assert.AreEqual(GreyboxLayout.Bushes.Length, result.Root.GetComponentsInChildren<HidingSpot>().Length);
            Assert.IsTrue(result.Root.GetComponentInChildren<LightZone>().GetComponent<Collider>().isTrigger);
            Assert.IsTrue(result.Root.GetComponentInChildren<EscapeZone>().GetComponent<Collider>().isTrigger);
        }

        [Test]
        public void BuildInto_NavMeshCollectsEnvironmentOnlyAndExcludesCharacters()
        {
            var result = Build();
            var layer = LayerMask.NameToLayer(GreyboxAssets.CharactersLayerName);

            Assert.AreEqual(CollectObjects.Children, result.NavMesh.collectObjects);
            Assert.AreEqual(0, result.NavMesh.layerMask.value & (1 << layer));
        }

        [Test]
        public void BuildInto_CalledTwice_KeepsSingleRootAndUserObjects()
        {
            var userObject = new GameObject("UserAddedProp");

            Build();
            Build();

            Assert.AreEqual(1, Scene.GetRootGameObjects().Count(go => go.name == GreyboxLevelBuilder.RootName));
            Assert.IsTrue(userObject != null);
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxLevelBuilder`
Expected: lỗi biên dịch `The name 'GreyboxLevelBuilder' does not exist`.

- [ ] **Step 3: Viết `GreyboxPrimitives`**

```csharp
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các hàm tạo khối hình đơn giản cho màn greybox. Gốc [Greybox] đặt ở (0,0,0) nên tọa độ local trùng tọa độ thế giới.</summary>
    internal static class GreyboxPrimitives
    {
        public static GameObject Create(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider = true)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = localScale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                Object.DestroyImmediate(gameObject.GetComponent<Collider>());
            }

            return gameObject;
        }

        public static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            return gameObject;
        }

        public static GameObject CreateTrigger(string name, Transform parent, Vector3 center, Vector3 size)
        {
            var gameObject = CreateEmpty(name, parent, center);
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            return gameObject;
        }

        public static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }
    }
}
```

- [ ] **Step 4: Viết `GreyboxEnvironment` và `GreyboxEnvironmentFactory`**

`GreyboxEnvironment.cs`:

```csharp
using Unity.AI.Navigation;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các điểm mốc trong môi trường mà nhân vật AI cần tham chiếu.</summary>
    internal sealed class GreyboxEnvironment
    {
        public Transform DogHome { get; set; }

        public Transform OwnerBed { get; set; }

        public Transform[] PatrolWaypoints { get; set; }

        public NavMeshSurface NavMesh { get; set; }
    }
}
```

`GreyboxEnvironmentFactory.cs`:

```csharp
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Stealth;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Dựng mặt đất, hàng rào, nhà, chuồng chó, bụi cây, đèn, vùng sáng, vùng thoát và các điểm mốc.
    /// NavMeshSurface gắn lên chính "Environment" và chỉ thu thập con của nó, nên nhân vật không bị tính là vật cản.
    /// </summary>
    internal static class GreyboxEnvironmentFactory
    {
        private const float PlaneSize = 10f; // Plane mặc định của Unity rộng 10 x 10 m.
        private const float BoundaryThickness = 0.5f;

        public static GreyboxEnvironment Build(Transform root, GreyboxAssets assets)
        {
            var environment = GreyboxPrimitives.CreateEmpty("Environment", root, Vector3.zero).transform;

            var groundScale = GreyboxLayout.GroundSize / PlaneSize;
            GreyboxPrimitives.Create(PrimitiveType.Plane, "Ground", environment, Vector3.zero,
                new Vector3(groundScale, 1f, groundScale), assets.GroundMaterial);

            BuildFences(environment, assets);
            BuildBoundary(environment, assets);
            GreyboxPrimitives.Create(PrimitiveType.Cube, "House", environment,
                GreyboxLayout.HouseCenter, GreyboxLayout.HouseSize, assets.BuildingMaterial);
            GreyboxPrimitives.Create(PrimitiveType.Cube, "Kennel", environment,
                GreyboxLayout.KennelCenter, GreyboxLayout.KennelSize, assets.BuildingMaterial);
            BuildBushes(environment, assets);
            BuildLights(environment);

            GreyboxPrimitives.CreateTrigger("LightZone", environment,
                GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize).AddComponent<LightZone>();
            GreyboxPrimitives.CreateTrigger("EscapeZone", environment,
                GreyboxLayout.EscapeZoneCenter, GreyboxLayout.EscapeZoneSize).AddComponent<EscapeZone>();

            var surface = environment.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;

            return new GreyboxEnvironment
            {
                DogHome = GreyboxPrimitives.CreateEmpty("DogHome", root, GreyboxLayout.DogHome).transform,
                OwnerBed = GreyboxPrimitives.CreateEmpty("OwnerBed", root, GreyboxLayout.OwnerBed).transform,
                PatrolWaypoints = BuildWaypoints(root),
                NavMesh = surface
            };
        }

        private static void BuildFences(Transform parent, GreyboxAssets assets)
        {
            var segments = GreyboxLayout.BuildFenceSegments();
            for (var i = 0; i < segments.Count; i++)
            {
                GreyboxPrimitives.Create(PrimitiveType.Cube, $"Fence{i:00}", parent,
                    segments[i].Center, segments[i].Size, assets.FenceMaterial);
            }
        }

        // Tường vô hình quanh mép mặt đất để người chơi không rơi khỏi màn.
        private static void BuildBoundary(Transform parent, GreyboxAssets assets)
        {
            var half = GreyboxLayout.GroundSize * 0.5f;
            var height = GreyboxLayout.BoundaryHeight;
            var y = height * 0.5f;
            var specs = new[]
            {
                (new Vector3(0f, y, half), new Vector3(GreyboxLayout.GroundSize, height, BoundaryThickness)),
                (new Vector3(0f, y, -half), new Vector3(GreyboxLayout.GroundSize, height, BoundaryThickness)),
                (new Vector3(half, y, 0f), new Vector3(BoundaryThickness, height, GreyboxLayout.GroundSize)),
                (new Vector3(-half, y, 0f), new Vector3(BoundaryThickness, height, GreyboxLayout.GroundSize))
            };

            for (var i = 0; i < specs.Length; i++)
            {
                var (center, size) = specs[i];
                var wall = GreyboxPrimitives.Create(PrimitiveType.Cube, $"Boundary{i}", parent, center, size, assets.FenceMaterial);
                Object.DestroyImmediate(wall.GetComponent<MeshRenderer>());
            }
        }

        // Bụi cây: khối cầu chỉ để nhìn (không chặn người) cộng trigger HidingSpot để nấp.
        private static void BuildBushes(Transform parent, GreyboxAssets assets)
        {
            for (var i = 0; i < GreyboxLayout.Bushes.Length; i++)
            {
                var bush = GreyboxLayout.Bushes[i];
                var position = bush.Position + Vector3.up * (bush.Diameter * 0.4f);
                var gameObject = GreyboxPrimitives.Create(PrimitiveType.Sphere, $"Bush{i}", parent, position,
                    Vector3.one * bush.Diameter, assets.BushMaterial, keepCollider: false);

                var trigger = gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                gameObject.AddComponent<HidingSpot>();
            }
        }

        private static void BuildLights(Transform parent)
        {
            var moon = GreyboxPrimitives.CreateEmpty("Moonlight", parent, new Vector3(0f, 10f, 0f));
            moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var moonLight = moon.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.6f, 0.7f, 1f);
            moonLight.intensity = 0.25f;
            moonLight.shadows = LightShadows.Soft;

            var porch = GreyboxPrimitives.CreateEmpty("PorchLight", parent, GreyboxLayout.PorchLightPosition);
            porch.transform.LookAt(GreyboxLayout.PorchLightTarget);
            var porchLight = porch.AddComponent<Light>();
            porchLight.type = LightType.Spot;
            porchLight.color = new Color(1f, 0.85f, 0.6f);
            porchLight.intensity = 4f;
            porchLight.range = 14f;
            porchLight.spotAngle = 70f;
            porchLight.shadows = LightShadows.Soft;
        }

        private static Transform[] BuildWaypoints(Transform root)
        {
            var group = GreyboxPrimitives.CreateEmpty("PatrolWaypoints", root, Vector3.zero).transform;
            var waypoints = new Transform[GreyboxLayout.PatrolWaypoints.Length];
            for (var i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = GreyboxPrimitives.CreateEmpty($"Waypoint{i}", group, GreyboxLayout.PatrolWaypoints[i]).transform;
            }

            return waypoints;
        }
    }
}
```

- [ ] **Step 5: Viết `GreyboxCharacterFactory`**

```csharp
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.AI.Sensors;
using DogHeist.Gameplay.Controls;
using DogHeist.Gameplay.Dog;
using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.AI;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Tạo trộm, chó, chủ nhà bằng khối capsule và nối tham chiếu như bước 9–15 trong README.
    /// Gốc mỗi nhân vật đặt ở chân (y = 0), thân capsule là object con.
    /// </summary>
    internal static class GreyboxCharacterFactory
    {
        public static ThiefMotor CreateThief(Transform parent, GreyboxAssets assets)
        {
            var thief = GreyboxPrimitives.CreateEmpty("Thief", parent, GreyboxLayout.ThiefSpawn);
            thief.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Nhìn về phía lỗ hàng rào.

            var controller = thief.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            GreyboxPrimitives.Create(PrimitiveType.Capsule, "Body", thief.transform, new Vector3(0f, 0.9f, 0f),
                new Vector3(0.7f, 0.9f, 0.7f), assets.ThiefMaterial, keepCollider: false);

            thief.AddComponent<LocalPlayerInput>();
            var motor = thief.AddComponent<ThiefMotor>();
            var visibility = thief.AddComponent<ThiefVisibility>();
            var interactor = thief.AddComponent<ThiefInteractor>();
            var carryAnchor = GreyboxPrimitives.CreateEmpty("CarryAnchor", thief.transform, new Vector3(0f, 1.1f, 0.5f));
            var throwOrigin = GreyboxPrimitives.CreateEmpty("ThrowOrigin", thief.transform, new Vector3(0f, 1.4f, 0.7f));

            SerializedWiring.Assign(motor, "_config", assets.ThiefConfig);
            SerializedWiring.Assign(visibility, "_motor", motor);
            SerializedWiring.Assign(interactor, "_motor", motor);
            SerializedWiring.Assign(interactor, "_carryAnchor", carryAnchor.transform);
            SerializedWiring.Assign(interactor, "_throwOrigin", throwOrigin.transform);
            SerializedWiring.Assign(interactor, "_lurePrefab", assets.LurePrefab);

            GreyboxPrimitives.SetLayerRecursively(thief, assets.CharactersLayer);
            return motor;
        }

        public static DogAI CreateDog(Transform parent, GreyboxAssets assets, ThiefVisibility thief, Transform home)
        {
            var dog = GreyboxPrimitives.CreateEmpty("Dog", parent, home.localPosition);

            var agent = dog.AddComponent<NavMeshAgent>();
            agent.radius = 0.3f;
            agent.height = 0.6f;
            // Giữ collider của thân để trộm tìm thấy chó khi bấm E.
            GreyboxPrimitives.Create(PrimitiveType.Capsule, "Body", dog.transform, new Vector3(0f, 0.3f, 0f),
                new Vector3(0.5f, 0.3f, 0.5f), assets.DogMaterial);

            dog.AddComponent<CarryableDog>();
            var hearing = dog.AddComponent<HearingSensor>();
            var ai = dog.AddComponent<DogAI>();

            SerializedWiring.Assign(ai, "_config", assets.DogConfig);
            SerializedWiring.Assign(ai, "_hearing", hearing);
            SerializedWiring.Assign(ai, "_thief", thief);
            SerializedWiring.Assign(ai, "_home", home);

            GreyboxPrimitives.SetLayerRecursively(dog, assets.CharactersLayer);
            return ai;
        }

        public static OwnerAI CreateOwner(
            Transform parent,
            GreyboxAssets assets,
            ThiefVisibility thief,
            Transform bed,
            Transform[] patrolWaypoints)
        {
            var owner = GreyboxPrimitives.CreateEmpty("Owner", parent, bed.localPosition);
            owner.transform.rotation = Quaternion.Euler(0f, 225f, 0f); // Nhìn ra sân về phía chuồng chó.

            var agent = owner.AddComponent<NavMeshAgent>();
            agent.radius = 0.4f;
            agent.height = 1.9f;
            GreyboxPrimitives.Create(PrimitiveType.Capsule, "Body", owner.transform, new Vector3(0f, 0.95f, 0f),
                new Vector3(0.8f, 0.95f, 0.8f), assets.OwnerMaterial);
            var eye = GreyboxPrimitives.CreateEmpty("Eye", owner.transform, new Vector3(0f, 1.6f, 0f));

            var hearing = owner.AddComponent<HearingSensor>();
            var vision = owner.AddComponent<VisionSensor>();
            var ai = owner.AddComponent<OwnerAI>();

            SerializedWiring.Assign(vision, "_eye", eye.transform);
            SerializedWiring.Assign(ai, "_config", assets.OwnerConfig);
            SerializedWiring.Assign(ai, "_hearing", hearing);
            SerializedWiring.Assign(ai, "_vision", vision);
            SerializedWiring.Assign(ai, "_thief", thief);
            SerializedWiring.Assign(ai, "_bed", bed);
            SerializedWiring.AssignArray(ai, "_patrolWaypoints", patrolWaypoints);

            GreyboxPrimitives.SetLayerRecursively(owner, assets.CharactersLayer);
            return ai;
        }
    }
}
```

- [ ] **Step 6: Viết `GreyboxBuildOptions`, `GreyboxBuildResult`, `GreyboxLevelBuilder`**

`GreyboxBuildOptions.cs`:

```csharp
namespace DogHeist.EditorTools.Greybox
{
    internal sealed class GreyboxBuildOptions
    {
        /// <summary>Thư mục chứa config, material, prefab. Test dùng thư mục tạm để không đụng asset thật.</summary>
        public string AssetRoot { get; set; } = GreyboxAssets.DefaultAssetRoot;
    }
}
```

`GreyboxBuildResult.cs`:

```csharp
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using Unity.AI.Navigation;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các object chính vừa dựng, để menu và test dùng tiếp.</summary>
    internal sealed class GreyboxBuildResult
    {
        public GameObject Root { get; set; }

        public ThiefMotor Thief { get; set; }

        public DogAI Dog { get; set; }

        public OwnerAI Owner { get; set; }

        public MatchManager Match { get; set; }

        public NavMeshSurface NavMesh { get; set; }
    }
}
```

`GreyboxLevelBuilder.cs`:

```csharp
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Dựng màn greybox Level01 thay cho 18 bước làm tay trong README.
    /// Mọi thứ sinh ra nằm dưới một gốc [Greybox]; dựng lại chỉ thay gốc này, giữ nguyên vật người dùng tự thêm.
    /// </summary>
    internal static class GreyboxLevelBuilder
    {
        public const string RootName = "[Greybox]";

        public static GreyboxBuildResult BuildInto(Scene scene, GreyboxBuildOptions options)
        {
            RemoveExistingRoot(scene);
            var assets = GreyboxAssets.LoadOrCreate(options.AssetRoot);

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            var environment = GreyboxEnvironmentFactory.Build(root.transform, assets);
            var characters = GreyboxPrimitives.CreateEmpty("Characters", root.transform, Vector3.zero).transform;
            var thief = GreyboxCharacterFactory.CreateThief(characters, assets);
            var visibility = thief.GetComponent<ThiefVisibility>();
            var dog = GreyboxCharacterFactory.CreateDog(characters, assets, visibility, environment.DogHome);
            var owner = GreyboxCharacterFactory.CreateOwner(characters, assets, visibility,
                environment.OwnerBed, environment.PatrolWaypoints);

            var match = GreyboxPrimitives.CreateEmpty("Match", root.transform, Vector3.zero).AddComponent<MatchManager>();
            SerializedWiring.Assign(match, "_thief", thief);

            environment.NavMesh.layerMask = ~(1 << assets.CharactersLayer);

            return new GreyboxBuildResult
            {
                Root = root,
                Thief = thief,
                Dog = dog,
                Owner = owner,
                Match = match,
                NavMesh = environment.NavMesh
            };
        }

        private static void RemoveExistingRoot(Scene scene)
        {
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                if (gameObject.name == RootName)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }
    }
}
```

- [ ] **Step 7: Chạy test, xác nhận đạt**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter Greybox`
Expected: mọi test Greybox (Task 3, 4, 5) đạt, `Lỗi 0`.

- [ ] **Step 8: Commit**

```bash
git add Assets/_Project/Scripts/Editor/Greybox Assets/_Project/Tests/EditMode
git commit -m "feat(editor): dựng môi trường và nhân vật greybox, nối tham chiếu tự động"
```

---

### Task 6: Camera, HUD, menu, bake NavMesh, lưu scene

**Files:**
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxCameraFactory.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxHud.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxHudFactory.cs`
- Create: `Assets/_Project/Scripts/Editor/Greybox/GreyboxBuildSettings.cs`
- Modify: `Assets/_Project/Scripts/Editor/Greybox/GreyboxBuildResult.cs` (thêm `MainCamera`, `Hud`)
- Modify: `Assets/_Project/Scripts/Editor/Greybox/GreyboxLevelBuilder.cs` (gọi camera/HUD, thêm menu, kiểm tra điều kiện, bake, lưu)
- Test: `Assets/_Project/Tests/EditMode/GreyboxPresentationTests.cs`

**Interfaces:**
- Consumes: `GreyboxBuildResult.Thief/Match/NavMesh/Root` (Task 5), `SerializedWiring`, `GreyboxPrimitives.CreateEmpty`, `GreyboxAssets.EnsureFolder`.
- Field UI được gán: `NoiseMeterUI._thief/_fill`, `ThiefStatusUI._interactor/_visibility/_promptText/_lureText/_visibilityText`, `ResultScreenUI._panel/_titleText/_detailsText/_restartButton/_matchManager`.
- Produces:
  - `GreyboxCameraFactory.Create(Transform parent, Transform target) → Camera`
  - `internal sealed class GreyboxHud { Canvas Canvas; NoiseMeterUI NoiseMeter; ThiefStatusUI Status; ResultScreenUI ResultScreen; }`
  - `GreyboxHudFactory.Create(Transform parent, ThiefMotor thief, MatchManager match) → GreyboxHud`
  - `GreyboxBuildSettings.EnsureSceneInBuild(string scenePath)`
  - `GreyboxLevelBuilder.ScenePath = "Assets/_Project/Scenes/Level01_Neighborhood.unity"`, `GreyboxLevelBuilder.FindMissingPrerequisites() → List<string>`, menu `DogHeist/Tools/Build Greybox Level`.

- [ ] **Step 1: Viết test hỏng**

```csharp
using System.Linq;
using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxPresentationTests : GreyboxSceneTestBase
    {
        private EditorBuildSettingsScene[] _originalBuildScenes;

        [SetUp]
        public void RememberBuildScenes() => _originalBuildScenes = EditorBuildSettings.scenes;

        [TearDown]
        public void RestoreBuildScenes() => EditorBuildSettings.scenes = _originalBuildScenes;

        [Test]
        public void BuildInto_CreatesSingleMainCameraFollowingThief()
        {
            var result = Build();

            Assert.AreEqual(1, result.Root.GetComponentsInChildren<Camera>().Count(c => c.CompareTag("MainCamera")));
            Assert.IsNotNull(result.MainCamera.GetComponent<CinemachineBrain>());
            var follow = result.Root.GetComponentInChildren<CinemachineCamera>();
            Assert.AreSame(result.Thief.transform, follow.Follow);
            Assert.AreSame(result.MainCamera.transform, ReadReference(result.Thief, "_cameraTransform"));
        }

        [Test]
        public void BuildInto_WiresHud()
        {
            var result = Build();
            var hud = result.Hud;

            Assert.AreSame(result.Thief, ReadReference(hud.NoiseMeter, "_thief"));
            Assert.IsNotNull(ReadReference(hud.NoiseMeter, "_fill"));
            Assert.IsNotNull(ReadReference(hud.Status, "_interactor"));
            Assert.IsNotNull(ReadReference(hud.Status, "_promptText"));
            Assert.AreSame(result.Match, ReadReference(hud.ResultScreen, "_matchManager"));
            Assert.IsNotNull(ReadReference(hud.ResultScreen, "_panel"));
            Assert.IsNotNull(ReadReference(hud.ResultScreen, "_restartButton"));
            Assert.AreNotSame(hud.ResultScreen.gameObject, ReadReference(hud.ResultScreen, "_panel"),
                "ResultScreenUI phải gắn lên Canvas, không gắn lên chính panel");
            Assert.IsNotNull(result.Root.GetComponentInChildren<EventSystem>());
        }

        [Test]
        public void EnsureSceneInBuild_CalledTwice_AddsSceneOnce()
        {
            GreyboxBuildSettings.EnsureSceneInBuild(GreyboxLevelBuilder.ScenePath);
            GreyboxBuildSettings.EnsureSceneInBuild(GreyboxLevelBuilder.ScenePath);

            var matches = EditorBuildSettings.scenes.Where(s => s.path == GreyboxLevelBuilder.ScenePath).ToArray();
            Assert.AreEqual(1, matches.Length);
            Assert.IsTrue(matches[0].enabled);
        }

        [Test]
        public void FindMissingPrerequisites_ProjectIsSetUp_ReturnsNothing()
        {
            // Test này đồng thời kiểm tra Task 0 đã làm đủ (URP, TMP Essentials).
            CollectionAssert.IsEmpty(GreyboxLevelBuilder.FindMissingPrerequisites());
        }
    }
}
```

- [ ] **Step 2: Chạy test, xác nhận hỏng**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter GreyboxPresentation`
Expected: lỗi biên dịch `'GreyboxBuildResult' does not contain a definition for 'MainCamera'`.

- [ ] **Step 3: Viết `GreyboxCameraFactory`**

```csharp
using Unity.Cinemachine;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Main Camera có CinemachineBrain, cộng một CinemachineCamera quay quanh trộm (chuột hoặc cần phải để xoay).
    /// Tương đương bước 16 trong README.
    /// </summary>
    internal static class GreyboxCameraFactory
    {
        private const float OrbitRadius = 6f;
        private static readonly Vector3 LookOffset = new Vector3(0f, 1.2f, 0f);
        private static readonly Color NightSky = new Color(0.03f, 0.04f, 0.08f);

        public static Camera Create(Transform parent, Transform target)
        {
            var startPosition = target.position + new Vector3(-OrbitRadius, 3f, 0f);

            var mainCameraObject = GreyboxPrimitives.CreateEmpty("Main Camera", parent, startPosition);
            mainCameraObject.tag = "MainCamera";
            var camera = mainCameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = NightSky;
            mainCameraObject.AddComponent<AudioListener>();
            mainCameraObject.AddComponent<CinemachineBrain>();

            var freeLookObject = GreyboxPrimitives.CreateEmpty("FreeLook Camera", parent, startPosition);
            var freeLook = freeLookObject.AddComponent<CinemachineCamera>();
            freeLook.Follow = target;
            var orbit = freeLookObject.AddComponent<CinemachineOrbitalFollow>();
            orbit.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbit.Radius = OrbitRadius;
            var composer = freeLookObject.AddComponent<CinemachineRotationComposer>();
            composer.TargetOffset = LookOffset;
            freeLookObject.AddComponent<CinemachineInputAxisController>();

            return camera;
        }
    }
}
```

- [ ] **Step 4: Viết `GreyboxHud` và `GreyboxHudFactory`**

`GreyboxHud.cs`:

```csharp
using DogHeist.UI.Hud;
using DogHeist.UI.Screens;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    internal sealed class GreyboxHud
    {
        public Canvas Canvas { get; set; }

        public NoiseMeterUI NoiseMeter { get; set; }

        public ThiefStatusUI Status { get; set; }

        public ResultScreenUI ResultScreen { get; set; }
    }
}
```

`GreyboxHudFactory.cs`:

```csharp
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using DogHeist.UI.Hud;
using DogHeist.UI.Screens;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// HUD tối thiểu (bước 18 trong README): thanh tiếng ồn, chữ trạng thái, màn kết quả có nút Chơi lại.
    /// </summary>
    internal static class GreyboxHudFactory
    {
        private const string BuiltinUiSprite = "UI/Skin/UISprite.psd";
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 BottomLeft = new Vector2(0f, 0f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        public static GreyboxHud Create(Transform parent, ThiefMotor thief, MatchManager match)
        {
            var canvasObject = GreyboxPrimitives.CreateEmpty("HUD Canvas", parent, Vector3.zero);
            canvasObject.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var hud = new GreyboxHud
            {
                Canvas = canvas,
                NoiseMeter = CreateNoiseMeter(canvas.transform, thief),
                Status = CreateStatus(canvasObject, thief),
                ResultScreen = CreateResultScreen(canvasObject, match)
            };

            var eventSystem = GreyboxPrimitives.CreateEmpty("EventSystem", parent, Vector3.zero);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            return hud;
        }

        private static NoiseMeterUI CreateNoiseMeter(Transform canvas, ThiefMotor thief)
        {
            var background = CreateImage("NoiseMeter", canvas, BottomLeft, new Vector2(40f, 40f),
                new Vector2(360f, 28f), new Color(0f, 0f, 0f, 0.6f));

            var fill = CreateImage("Fill", background.transform, BottomLeft, Vector2.zero, Vector2.zero, Color.white);
            Stretch(fill.rectTransform);
            // Image Type = Filled chỉ hoạt động khi có sprite, nên dùng sprite có sẵn của Unity.
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinUiSprite);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            var meter = background.gameObject.AddComponent<NoiseMeterUI>();
            SerializedWiring.Assign(meter, "_thief", thief);
            SerializedWiring.Assign(meter, "_fill", fill);
            return meter;
        }

        private static ThiefStatusUI CreateStatus(GameObject canvasObject, ThiefMotor thief)
        {
            var canvas = canvasObject.transform;
            var prompt = CreateText("PromptText", canvas, BottomCenter, new Vector2(0f, 120f), new Vector2(1000f, 50f), 32f, TextAlignmentOptions.Center);
            var lure = CreateText("LureText", canvas, TopLeft, new Vector2(40f, -40f), new Vector2(600f, 40f), 28f, TextAlignmentOptions.Left);
            var visibilityText = CreateText("VisibilityText", canvas, TopLeft, new Vector2(40f, -90f), new Vector2(600f, 40f), 28f, TextAlignmentOptions.Left);

            var status = canvasObject.AddComponent<ThiefStatusUI>();
            SerializedWiring.Assign(status, "_interactor", thief.GetComponent<ThiefInteractor>());
            SerializedWiring.Assign(status, "_visibility", thief.GetComponent<ThiefVisibility>());
            SerializedWiring.Assign(status, "_promptText", prompt);
            SerializedWiring.Assign(status, "_lureText", lure);
            SerializedWiring.Assign(status, "_visibilityText", visibilityText);
            return status;
        }

        // ResultScreenUI gắn lên Canvas (luôn bật), không gắn lên panel bị ẩn, để còn nhận sự kiện MatchEnded.
        private static ResultScreenUI CreateResultScreen(GameObject canvasObject, MatchManager match)
        {
            var panel = CreateImage("ResultPanel", canvasObject.transform, Center, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.75f));
            Stretch(panel.rectTransform);

            var title = CreateText("Title", panel.transform, TopCenter, new Vector2(0f, -160f), new Vector2(1400f, 90f), 64f, TextAlignmentOptions.Center);
            var details = CreateText("Details", panel.transform, Center, Vector2.zero, new Vector2(900f, 360f), 32f, TextAlignmentOptions.Center);
            var button = CreateButton("RestartButton", panel.transform, BottomCenter, new Vector2(0f, 160f), new Vector2(320f, 80f), "Chơi lại");
            panel.gameObject.SetActive(false);

            var resultScreen = canvasObject.AddComponent<ResultScreenUI>();
            SerializedWiring.Assign(resultScreen, "_panel", panel.gameObject);
            SerializedWiring.Assign(resultScreen, "_titleText", title);
            SerializedWiring.Assign(resultScreen, "_detailsText", details);
            SerializedWiring.Assign(resultScreen, "_restartButton", button);
            SerializedWiring.Assign(resultScreen, "_matchManager", match);
            return resultScreen;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = parent.gameObject.layer;
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var image = CreateRect(name, parent, anchor, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            float fontSize, TextAlignmentOptions alignment)
        {
            var text = CreateRect(name, parent, anchor, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = string.Empty;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, string label)
        {
            var image = CreateImage(name, parent, anchor, position, size, new Color(0.95f, 0.75f, 0.3f));
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText("Label", image.transform, Center, Vector2.zero, Vector2.zero, 32f, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            text.color = Color.black;
            text.text = label;
            return button;
        }
    }
}
```

- [ ] **Step 5: Viết `GreyboxBuildSettings`**

```csharp
using System.Collections.Generic;
using UnityEditor;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Đưa scene vào danh sách build (cần cho nút Chơi lại), không thêm trùng.</summary>
    internal static class GreyboxBuildSettings
    {
        public static void EnsureSceneInBuild(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
```

- [ ] **Step 6: Thay toàn bộ `GreyboxBuildResult.cs`** (thêm hai property):

```csharp
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using Unity.AI.Navigation;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các object chính vừa dựng, để menu và test dùng tiếp.</summary>
    internal sealed class GreyboxBuildResult
    {
        public GameObject Root { get; set; }

        public ThiefMotor Thief { get; set; }

        public DogAI Dog { get; set; }

        public OwnerAI Owner { get; set; }

        public MatchManager Match { get; set; }

        public NavMeshSurface NavMesh { get; set; }

        public Camera MainCamera { get; set; }

        public GreyboxHud Hud { get; set; }
    }
}
```

- [ ] **Step 7: Thay toàn bộ `GreyboxLevelBuilder.cs`**

```csharp
using System.Collections.Generic;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Dựng màn greybox Level01 thay cho 18 bước làm tay trong README.
    /// Mọi thứ sinh ra nằm dưới một gốc [Greybox]; dựng lại chỉ thay gốc này, giữ nguyên vật người dùng tự thêm.
    /// </summary>
    internal static class GreyboxLevelBuilder
    {
        public const string RootName = "[Greybox]";
        public const string ScenePath = "Assets/_Project/Scenes/Level01_Neighborhood.unity";

        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string NavMeshDataPath = "Assets/_Project/Scenes/Level01_Neighborhood_NavMesh.asset";
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string TmpSettingsResource = "TMP Settings";
        private static readonly Color NightAmbient = new Color(0.08f, 0.10f, 0.16f);

        [MenuItem("DogHeist/Tools/Build Greybox Level")]
        private static void BuildFromMenu()
        {
            var missing = FindMissingPrerequisites();
            if (missing.Count > 0)
            {
                Debug.LogError("[Greybox] Chưa dựng được màn vì project còn thiếu:\n- " + string.Join("\n- ", missing));
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = OpenOrCreateScene();
            SceneManager.SetActiveScene(scene);
            var result = BuildInto(scene, new GreyboxBuildOptions());
            ApplyNightAmbient();
            BakeNavMesh(result.NavMesh);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            GreyboxBuildSettings.EnsureSceneInBuild(ScenePath);
            Selection.activeGameObject = result.Thief.gameObject;
            Debug.Log($"[Greybox] Đã dựng xong {ScenePath}. Bấm Play để chơi thử.");
        }

        public static List<string> FindMissingPrerequisites()
        {
            var missing = new List<string>();
            if (Resources.Load<TMP_Settings>(TmpSettingsResource) == null)
            {
                missing.Add("TMP Essential Resources (Window > TextMeshPro > Import TMP Essential Resources).");
            }

            if (GraphicsSettings.defaultRenderPipeline == null || Shader.Find(LitShaderName) == null)
            {
                missing.Add("Universal Render Pipeline: project cần tạo từ template Universal 3D (xem Task 0 của kế hoạch).");
            }

            return missing;
        }

        public static GreyboxBuildResult BuildInto(Scene scene, GreyboxBuildOptions options)
        {
            RemoveExistingRoot(scene);
            var assets = GreyboxAssets.LoadOrCreate(options.AssetRoot);

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            var environment = GreyboxEnvironmentFactory.Build(root.transform, assets);
            var characters = GreyboxPrimitives.CreateEmpty("Characters", root.transform, Vector3.zero).transform;
            var thief = GreyboxCharacterFactory.CreateThief(characters, assets);
            var visibility = thief.GetComponent<ThiefVisibility>();
            var dog = GreyboxCharacterFactory.CreateDog(characters, assets, visibility, environment.DogHome);
            var owner = GreyboxCharacterFactory.CreateOwner(characters, assets, visibility,
                environment.OwnerBed, environment.PatrolWaypoints);

            var match = GreyboxPrimitives.CreateEmpty("Match", root.transform, Vector3.zero).AddComponent<MatchManager>();
            SerializedWiring.Assign(match, "_thief", thief);

            var camera = GreyboxCameraFactory.Create(root.transform, thief.transform);
            SerializedWiring.Assign(thief, "_cameraTransform", camera.transform);
            var hud = GreyboxHudFactory.Create(root.transform, thief, match);

            environment.NavMesh.layerMask = ~(1 << assets.CharactersLayer);

            return new GreyboxBuildResult
            {
                Root = root,
                Thief = thief,
                Dog = dog,
                Owner = owner,
                Match = match,
                NavMesh = environment.NavMesh,
                MainCamera = camera,
                Hud = hud
            };
        }

        private static Scene OpenOrCreateScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GreyboxAssets.EnsureFolder(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            return scene;
        }

        private static void ApplyNightAmbient()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = NightAmbient;
            RenderSettings.skybox = null;
        }

        // Lưu dữ liệu NavMesh thành asset để scene giữ được kết quả bake sau khi đóng mở lại.
        private static void BakeNavMesh(NavMeshSurface surface)
        {
            surface.BuildNavMesh();
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshDataPath) != null)
            {
                AssetDatabase.DeleteAsset(NavMeshDataPath);
            }

            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshDataPath);
        }

        private static void RemoveExistingRoot(Scene scene)
        {
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                if (gameObject.name == RootName)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }
    }
}
```

- [ ] **Step 8: Chạy toàn bộ test**

Run: `powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1`
Expected: toàn bộ test (cũ và mới) đạt, `Lỗi 0`. Nếu `FindMissingPrerequisites_ProjectIsSetUp_ReturnsNothing` hỏng, thông điệp chỉ ra bước Task 0 còn thiếu.

- [ ] **Step 9: Chạy công cụ thật.** Nhờ người dùng mở Unity, chọn `DogHeist > Tools > Build Greybox Level`, xác nhận Console có dòng `[Greybox] Đã dựng xong ...` và không có lỗi đỏ; gửi ảnh chụp cửa sổ Scene nhìn từ trên xuống.

- [ ] **Step 10: Commit** (gồm scene, NavMesh, config, prefab, material vừa sinh cùng file `.meta`)

```bash
git add Assets/_Project ProjectSettings/TagManager.asset ProjectSettings/EditorBuildSettings.asset
git commit -m "feat(editor): thêm camera, HUD, menu Build Greybox Level và scene Level01"
```

---

### Task 7: Chơi thử, cập nhật tài liệu, đóng Giai đoạn 0

**Files:**
- Create: `docs/testing/M0-smoke-test.md`
- Modify: `README.md` (mục "Dựng màn chơi greybox đầu tiên")
- Modify: `docs/ARCHITECTURE.md` (sơ đồ assembly)
- Modify: `CLAUDE.md` (mục "Trạng thái hiện tại", "Việc tiếp theo")
- Modify: `PLAN.md` (tích các ô Giai đoạn 0)

- [ ] **Step 1: Viết checklist chơi thử** `docs/testing/M0-smoke-test.md`:

```markdown
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

## Nếu gặp vấn đề
- Chữ tiếng Việt hiện ô vuông: tạo font hỗ trợ tiếng Việt theo ghi chú cuối README.
- Chó sủa mà chủ nhà không dậy: tăng `BarkNoiseRadius` trong `DogConfig` (ví dụ 20), không sửa code.
- Chuột không xoay camera: chọn `FreeLook Camera`, ở component *Cinemachine Input Axis Controller* bấm menu ba chấm > *Reset*.
- Nhân vật AI đứng im: chọn `[Greybox]/Environment`, ở *NavMeshSurface* bấm *Bake*, hoặc chạy lại menu Build Greybox Level.
```

- [ ] **Step 2: Người dùng chơi theo checklist** và gửi kết quả (ảnh chụp, lỗi Console). Lỗi phát sinh: dùng **superpowers:systematic-debugging**; chỉnh số trong config, không viết cứng vào code.

- [ ] **Step 3: Cập nhật README.** Thêm ngay dưới tiêu đề "Dựng màn chơi greybox đầu tiên":

```markdown
**Cách nhanh (khuyên dùng):** chọn menu *DogHeist > Tools > Build Greybox Level*. Công cụ tự tạo scene `Level01_Neighborhood`, 3 file config, prefab đồ ăn, layer Characters, bake NavMesh, gắn camera và HUD, rồi thêm scene vào Build Profiles. Chạy lại bất cứ lúc nào: chỉ object `[Greybox]` được dựng lại, config đã chỉnh được giữ nguyên.

Các bước dưới đây là cách làm tay, để hiểu công cụ làm gì.
```

- [ ] **Step 4: Cập nhật sơ đồ trong `docs/ARCHITECTURE.md`:**

```
DogHeist.UI ─────► DogHeist.Gameplay ─────► DogHeist.Core
DogHeist.AI ─────► DogHeist.Gameplay
DogHeist.Editor ─► tất cả (chỉ chạy trong Editor, không assembly nào tham chiếu ngược)
DogHeist.Tests ──► DogHeist.Core, Gameplay, AI, UI, Editor
```

Thêm một dòng mô tả: "**Editor**: công cụ trong Unity Editor (dựng màn greybox). Không được đưa vào bản build game."

- [ ] **Step 5: Cập nhật `CLAUDE.md`** mục "Trạng thái hiện tại": code đã chạy trong Unity 6.3, có công cụ `DogHeist > Tools > Build Greybox Level`, chạy test bằng `tools/run-unity-tests.ps1` (Unity phải đóng); mục "Việc tiếp theo" chuyển sang Giai đoạn 1. Thêm `Editor → tất cả` vào quy tắc chiều phụ thuộc.

- [ ] **Step 6: Tích các ô Giai đoạn 0 trong `PLAN.md`** đã đạt điều kiện "chơi được cả hai kết cục, thành tích tăng đúng, không lỗi đỏ". Để trống các ô liên quan đến git và GitHub ("Cài Git...", "Commit scene...", "đẩy lên GitHub") cho tới khi người dùng tự khởi tạo repo.

- [ ] **Step 7: Commit**

```bash
git add README.md CLAUDE.md PLAN.md docs
git commit -m "docs: checklist chơi thử M0 và cập nhật tài liệu sau khi có công cụ greybox"
```

---

## Các kế hoạch tiếp theo (viết sau khi Giai đoạn 0 đạt)

Mỗi mục là một kế hoạch riêng, tự chạy và test được. Mục nào chưa rõ thiết kế thì brainstorm trước (skill `superpowers:brainstorming`).

1. **G1a: Phản hồi người chơi.** Dấu "?" và "!" trên đầu AI: thêm interface `IAwarenessSource` (mức Unaware/Suspicious/Alerted) trong Gameplay, `DogAI` và `OwnerAI` cài đặt, UI chỉ đọc interface (giữ đúng chiều `UI → Gameplay`). Biểu tượng con mắt, rung camera nhẹ, âm thanh tạm sinh bằng code.
2. **G1b: Chiều sâu.** Chọn 1–2 thứ sau khi chơi thử: lỗ rào chỉ chui được khi lom khom và không bế chó, cổng cót két, đèn cảm biến, đồ ném gây tiếng.
3. **G1c: Cân bằng và thử chơi.** Bảng thông số mục tiêu cho ván 3–5 phút, script đóng bản build Windows bằng dòng lệnh, checklist cho 3–5 người chơi.
4. **G2A, G2B, G3, G4, G5:** mỗi giai đoạn brainstorm rồi viết kế hoạch riêng. Sơ đồ vòng lặp hai vai (`vong_lap_gameplay_hai_vai.png`: Chuẩn bị → Tuần tra → Phát hiện → Bắt được trộm) là đầu vào cho vai chủ nhà ở G3 và đối kháng ở G5.
