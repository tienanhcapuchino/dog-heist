# Dog Heist (tên tạm)

Game lén lút 3D góc nhìn thứ ba cho PC, làm bằng Unity và C#. Người chơi vào vai **trộm chó**: lẻn vào sân nhà, dụ chó bằng đồ ăn, bế chó và mang ra khỏi cổng mà không bị chủ nhà bắt.

Bản đầu tiên chỉ có **chế độ chơi đơn, vai trộm**, đấu với chủ nhà và chó do AI điều khiển. Kiến trúc đã được chia lớp sẵn để sau này thêm vai chủ nhà và chế độ đối kháng nhiều người (xem `docs/ARCHITECTURE.md`).

## Yêu cầu

- Unity **6.3 LTS** (cài qua Unity Hub, tick thêm module *Windows Build Support*)
- Các package (cài qua *Window > Package Manager*):
  - **Input System** (thường có sẵn trong template)
  - **AI Navigation** (để bake NavMesh cho chó và chủ nhà)
  - **Cinemachine** (camera góc nhìn thứ ba)
  - **Test Framework** (có sẵn)
- Git và **Git LFS** (`git lfs install` một lần trên máy)

## Cài đặt lần đầu

Repo này chỉ chứa code, tài liệu và cấu hình git. Phần project Unity (ProjectSettings, Packages) do Unity tự sinh, nên làm như sau:

1. Mở Unity Hub, tạo project mới bằng template **Universal 3D**, phiên bản 6.3 LTS.
2. Đóng Unity. Chép toàn bộ nội dung repo này vào thư mục gốc của project vừa tạo (gộp chung với thư mục `Assets` có sẵn).
3. Mở lại project, cài các package ở phần Yêu cầu.
4. Vào *Edit > Project Settings > Player > Other Settings*, đặt **Active Input Handling** là *Input System Package (New)* hoặc *Both*.
5. Tạo 3 file cấu hình trong `Assets/_Project/Settings/Configs`: chuột phải, chọn *Create > DogHeist > Configs*, lần lượt tạo **Thief Config**, **Dog Config**, **Owner Config**.
6. Khởi tạo git trong thư mục project: `git init`, `git lfs install`, rồi commit lần đầu.

## Cấu trúc thư mục

```
.
├── README.md
├── .gitignore / .gitattributes / .editorconfig
├── docs/
│   ├── GDD.md               Tài liệu thiết kế game (bản MVP)
│   ├── ARCHITECTURE.md      Kiến trúc code và kế hoạch lên multiplayer
│   └── ROADMAP.md           Các mốc phát triển
└── Assets/_Project/         Mọi thứ của game nằm ở đây, tách khỏi asset bên thứ ba
    ├── Art/                 Materials, Models, Textures
    ├── Audio/               Music, SFX
    ├── Prefabs/             Characters, Environment, Items, UI
    ├── Scenes/              Level01_Neighborhood...
    ├── Settings/Configs/    Các file ThiefConfig, DogConfig, OwnerConfig
    ├── Scripts/
    │   ├── Core/            [DogHeist.Core] Logic thuần, không phụ thuộc gameplay
    │   │   ├── FSM/             Máy trạng thái dùng chung
    │   │   ├── Match/           PlayerRole, MatchOutcome, MatchResult
    │   │   └── Stats/           Thành tích và lưu file JSON
    │   ├── Gameplay/        [DogHeist.Gameplay] Luật chơi và nhân vật người chơi
    │   │   ├── Controls/        ICharacterInput, LocalPlayerInput
    │   │   ├── Noise/           Hệ thống tiếng ồn
    │   │   ├── Stealth/         Vùng sáng, chỗ nấp
    │   │   ├── Interaction/     IInteractable
    │   │   ├── Items/           Đồ ăn dụ chó
    │   │   ├── Thief/           Di chuyển, độ lộ diện, tương tác của trộm
    │   │   ├── Dog/             CarryableDog (phần "bế được" của con chó)
    │   │   └── Match/           MatchManager, MatchEvents, EscapeZone
    │   ├── AI/              [DogHeist.AI] Chó và chủ nhà do máy điều khiển
    │   │   ├── Sensors/         HearingSensor, VisionSensor
    │   │   ├── Dog/             DogAI và các trạng thái
    │   │   └── Owner/           OwnerAI và các trạng thái
    │   └── UI/              [DogHeist.UI] HUD và màn hình kết quả
    └── Tests/EditMode/      [DogHeist.Tests.EditMode] Unit test
```

Tên trong ngoặc vuông là **Assembly Definition**. Mỗi assembly chỉ được tham chiếu theo chiều mũi tên, giúp Unity biên dịch nhanh hơn và giữ code không bị rối:

```
UI ──► Gameplay ──► Core
AI ──► Gameplay
```

## Dựng màn chơi greybox đầu tiên

**Cách nhanh (khuyên dùng):** chọn menu *DogHeist > Tools > Build Greybox Level*. Công cụ tự tạo scene `Level01_Neighborhood`, 3 file config, prefab đồ ăn, layer Characters, bake NavMesh, gắn camera và HUD, rồi thêm scene vào Build Profiles. Chạy lại bất cứ lúc nào: chỉ object `[Greybox]` được dựng lại, config đã chỉnh được giữ nguyên.

Các bước dưới đây là cách làm tay, để hiểu công cụ làm gì.

Dùng khối hình đơn giản (cube, capsule) để thử gameplay trước, chưa cần đồ họa thật.

**Môi trường**

1. Tạo scene `Assets/_Project/Scenes/Level01_Neighborhood` và thêm vào *File > Build Profiles > Scene List* (cần cho nút chơi lại).
2. Tạo mặt đất (Plane, scale 4), hàng rào bằng các Cube quanh sân, chừa một khoảng trống làm **cổng** và một **lỗ hổng** ở hàng rào bên hông.
3. Tạo nhà chủ (Cube lớn), chuồng chó (Cube nhỏ), vài bụi cây.
4. Tạo layer **Characters** và đặt trộm, chó, chủ nhà vào layer này.
5. Tạo GameObject rỗng tên `NavMesh`, thêm component **NavMeshSurface**, bỏ chọn layer Characters ở mục *Include Layers*, rồi bấm **Bake**.

**Vùng ẩn nấp và vùng sáng**

6. Mỗi bụi cây: thêm một Box Collider bật *Is Trigger* và component **HidingSpot**.
7. Đèn hiên: thêm Spot Light, thêm một GameObject có Box Collider (*Is Trigger*) phủ vùng sáng, gắn **LightZone**.
8. Ngoài cổng: tạo GameObject có Box Collider (*Is Trigger*), gắn **EscapeZone**.

**Trộm (người chơi)**

9. Tạo Capsule tên `Thief`, xóa Capsule Collider, thêm **CharacterController**, **LocalPlayerInput**, **ThiefMotor**, **ThiefVisibility**, **ThiefInteractor**. Gán ThiefConfig cho ThiefMotor.
10. Tạo 2 GameObject con: `CarryAnchor` ở vị trí (0, 1.1, 0.5) và `ThrowOrigin` ở (0, 1.4, 0.7), gán vào ThiefInteractor.
11. Tạo prefab đồ ăn: Sphere scale 0.3, thêm **Rigidbody** và **FoodLure**, lưu vào `Prefabs/Items`, gán vào ô *Lure Prefab* của ThiefInteractor.

**Chó**

12. Tạo Capsule nhỏ tên `Dog`, thêm **NavMeshAgent** (radius 0.3, height 0.6), **CarryableDog**, **HearingSensor**, **DogAI**.
13. Trong DogAI: gán DogConfig, HearingSensor, ThiefVisibility của trộm, và một GameObject rỗng đặt ở chuồng chó làm *Home*.

**Chủ nhà**

14. Tạo Capsule tên `Owner`, thêm **NavMeshAgent**, **HearingSensor**, **VisionSensor**, **OwnerAI**. Tạo con `Eye` ở độ cao 1.6 gán vào VisionSensor.
15. Trong OwnerAI: gán OwnerConfig, hai sensor, ThiefVisibility của trộm, một điểm `Bed` (ví dụ võng ngoài hiên) và 3 đến 4 điểm tuần tra quanh sân.

**Camera, trận đấu, giao diện**

16. *GameObject > Cinemachine > Targeted Cameras > FreeLook Camera*, đặt *Tracking Target* là Thief.
17. Tạo GameObject `Match`, gắn **MatchManager**, kéo Thief vào ô *Thief*.
18. Tạo Canvas. Thêm Image (Image Type = *Filled*, Horizontal) gắn **NoiseMeterUI**; thêm vài TextMeshPro gắn **ThiefStatusUI**. Tạo panel kết quả có tiêu đề, nội dung và nút *Chơi lại*, rồi gắn **ResultScreenUI** lên **Canvas** (không gắn lên panel).

Bấm Play và thử trộm con chó đầu tiên.

> Font mặc định của TextMeshPro có thể thiếu dấu tiếng Việt. Tải một font hỗ trợ tiếng Việt (ví dụ Be Vietnam Pro hoặc Roboto trên Google Fonts), rồi dùng *Window > TextMeshPro > Font Asset Creator* với *Atlas Population Mode = Dynamic*.

## Điều khiển

| Hành động | Bàn phím chuột | Tay cầm |
|---|---|---|
| Di chuyển | WASD | Cần trái |
| Chạy | Shift trái | Bấm cần trái |
| Lom khom (giữ) | Ctrl trái | B / Circle |
| Bế hoặc thả chó | E | A / Cross |
| Ném đồ ăn | Q hoặc chuột trái | RT / R2 |

## Chạy test

*Window > General > Test Runner*, chọn tab **EditMode**, bấm **Run All**. Các test kiểm tra máy trạng thái, luật tính thành tích, lưu và đọc file thành tích, và hệ thống tiếng ồn.

## Quy ước code

- Theo chuẩn C# của .NET: `PascalCase` cho class, method, property; `_camelCase` cho field private; interface bắt đầu bằng `I`; ngoặc nhọn xuống dòng. File `.editorconfig` giúp IDE tự áp dụng.
- Dùng `[SerializeField] private` thay vì field `public` chỉ để hiện trong Inspector.
- Mọi con số cân bằng game đặt trong ScriptableObject config, không viết cứng trong code.
- Logic không cần Unity (luật thành tích, máy trạng thái) đặt trong Core để viết test được.
- Không dùng `GameObject.Find`; tham chiếu được kéo thả trong Inspector hoặc truyền qua sự kiện.

## Quy trình làm việc với git

- Nhánh `main` luôn chạy được. Mỗi tính năng làm trên nhánh riêng, ví dụ `feature/dog-bark-animation`.
- `main` được bảo vệ bằng ruleset: không push thẳng, không force push, mọi thay đổi đi qua Pull Request và chỉ chủ repo (`@tienanhcapuchino`) được merge. File `.github/CODEOWNERS` giúp GitHub tự động gán chủ repo làm người review cho mỗi PR.
- Commit nhỏ, mô tả rõ việc đã làm.
- Không commit thư mục `Library`, `Temp`, `Logs` (đã có trong `.gitignore`). Luôn commit file `.meta` đi kèm asset.
