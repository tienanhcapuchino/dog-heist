# Kế hoạch phát triển Dog Heist

> Bản làm việc cho Claude Code, chép từ Claude Doc "Kế hoạch phát triển Dog Heist" ngày 2026-10-02.
> Từ nay file này là bản chính: Claude Code tích `[x]` khi xong việc và cập nhật file khi kế hoạch thay đổi.

## Tổng quan

Bản chơi đơn phát hành trên Steam vào khoảng tuần 40 đến 46 (khoảng 10 tháng); chế độ nhiều người chơi ra sau đó dưới dạng bản cập nhật miễn phí, khoảng tuần 62. Mỗi giai đoạn có danh sách việc để tích và một điều kiện "hoàn thành khi" rõ ràng; chưa đạt điều kiện thì chưa sang giai đoạn sau.

Giả định của kế hoạch:

- Claude Code làm gần như toàn bộ phần sản xuất: viết code, dựng model 3D, tìm tài nguyên, làm giao diện, âm thanh và tài liệu.
- Bạn phụ trách thử chơi, nhận xét, duyệt hướng hình ảnh, và các việc cần tài khoản hoặc thanh toán.
- Đồ họa theo phong cách low-poly hoạt hình, dựa trên tài nguyên miễn phí giấy phép CC0 rồi chỉnh sửa bằng script.
- Thứ tự: chơi đơn hoàn chỉnh trước, phát hành lên Steam, rồi bổ sung nhiều người chơi bằng bản cập nhật miễn phí.

Nguyên tắc xuyên suốt: gameplay phải vui khi còn là khối hộp xám thì mới đầu tư vẽ. Đồ họa đẹp không cứu được một vòng chơi nhàm.

**Ai làm gì**

| Việc | Claude Code | Bạn |
| --- | --- | --- |
| Code gameplay, AI, UI, công cụ Editor | Viết, sửa lỗi, viết test | Dán lỗi từ Console Unity khi có |
| Nhân vật, môi trường | Tìm model CC0, chỉnh bằng script Blender, nhập vào Unity | Xem ảnh xem trước, chọn phương án |
| Giao diện | Thiết kế, vẽ biểu tượng SVG, dựng màn hình | Nhận xét dễ đọc, dễ dùng hay không |
| Âm thanh | Tìm âm thanh CC0, tạo tiếng động đơn giản bằng code | Nghe thử, chọn nhạc |
| Thử chơi | Viết checklist test cho từng bản | Chơi, ghi lỗi, mời người khác chơi |
| Tài khoản, thanh toán | Không làm được | Unity, GitHub, Steam, đăng nhập trang tải tài nguyên |

Giới hạn cần biết: Claude không vẽ tay như họa sĩ. Ảnh "concept" sẽ là ảnh render từ Blender hoặc hình SVG, và nhân vật sẽ gọn gàng, đồng bộ nhưng khó độc đáo bằng người vẽ chuyên. Claude cũng không đăng nhập thay bạn vào trang cần tài khoản như Mixamo, Asset Store hay Freesound.

**Lộ trình theo tuần**

| Giai đoạn | Tuần |
| --- | --- |
| 0. Chạy được vòng chơi | 1–2 |
| 1. Gameplay vui | 3–8 |
| 2A. Nhân vật | 9–16 |
| 2B. Môi trường, UI, âm thanh | 13–22 |
| 3. Đủ nội dung | 23–34 |
| 4. Steam, phát hành (mở trang Steam tuần 22) | 22–46, phát hành 40–46 |
| 5. Nhiều người chơi (bản cập nhật) | 47–62 |

## Giai đoạn 0: Chạy được vòng chơi (tuần 1 đến 2)

Mục tiêu: mở project trong Unity, bấm Play và chơi được một ván trộm chó từ đầu đến cuối bằng khối hình đơn giản.

**Cài đặt**

- [x] Cài Unity Hub, Unity 6.3 LTS (kèm Windows Build Support), Visual Studio hoặc Rider
- [x] Cài Git, Git LFS, tạo repo riêng tư trên GitHub
- [x] Tạo project Universal 3D, chép bộ khung code vào theo README
- [x] Cài package AI Navigation, Cinemachine; đặt Active Input Handling
- [x] Mở Console, sửa hết lỗi biên dịch (dán lỗi cho Claude Code)
- [x] Chạy EditMode test, tất cả phải xanh

**Dựng màn greybox**

- [x] Nhờ Claude Code viết công cụ Editor tự dựng màn greybox, hoặc làm tay theo 18 bước trong README
- [x] Tạo 3 file config Thief, Dog, Owner
- [x] Bake NavMesh, gắn camera Cinemachine theo trộm
- [x] Dựng HUD tối thiểu: thanh tiếng ồn, gợi ý phím, màn kết quả

**Commit đầu tiên**

- [x] Commit scene, prefab, config cùng file .meta; đẩy lên GitHub
- [x] Cập nhật mục "Trạng thái hiện tại" trong CLAUDE.md

**Hoàn thành khi:** chơi được cả hai kết cục (trốn thoát và bị bắt), thành tích tăng đúng sau mỗi ván, không còn lỗi đỏ trong Console.

## Giai đoạn 1: Làm cho gameplay vui (tuần 3 đến 8)

Mục tiêu: một ván greybox dài 3 đến 5 phút, căng thẳng và buồn cười, người lạ chơi hiểu ngay không cần giải thích.

**Cân bằng thông số** (chỉnh trong file config, không sửa code)

- [ ] Tầm nghe của chó và chủ nhà so với tiếng bước chân
- [ ] Tầm nhìn của chủ nhà trong sáng và trong tối
- [ ] Tốc độ trộm khi bế chó so với tốc độ chủ nhà chạy
- [ ] Số đồ ăn mỗi ván, thời gian chó ăn và thời gian chó hiền

**Phản hồi cho người chơi**

- [ ] Dấu "?" trên đầu AI khi nghi ngờ, dấu "!" khi phát hiện
- [ ] Biểu tượng con mắt cho biết đang lộ hay đang ẩn
- [ ] Âm thanh tạm: bước chân, chó sủa, chủ nhà la (Claude tạo bằng code hoặc lấy từ gói CC0)
- [ ] Rung camera nhẹ khi bị phát hiện

**Thêm chiều sâu (chọn 1 đến 2 thứ, thử xem có vui không)**

- [ ] Lỗ hàng rào chỉ chui được khi lom khom và không bế chó
- [ ] Cổng kêu cót két khi mở
- [ ] Đèn cảm biến bật khi có người đi qua
- [ ] Đồ ném gây tiếng động để đánh lạc hướng chủ nhà

**Thử chơi**

- [ ] Claude đóng bản build Windows và viết checklist test; bạn gửi cho 3 đến 5 người chơi
- [ ] Ngồi xem họ chơi, không chỉ dẫn; ghi lại chỗ họ bối rối, chán hoặc cười
- [ ] Sửa 3 vấn đề lớn nhất, cho chơi lại

**Hoàn thành khi:** đa số người thử muốn chơi thêm ván nữa, và họ tự tìm ra cách dụ chó bằng đồ ăn.

## Giai đoạn 2A: Nhân vật (tuần 9 đến 16)

Mục tiêu: ba nhân vật chính có model 3D, rig và đủ animation để thay khối capsule trong game. Claude Code làm toàn bộ; bạn duyệt ở 3 điểm kiểm tra.

**Chuẩn bị một lần (bạn làm, khoảng 30 phút)**

- [ ] Cài Blender bản mới nhất và thêm vào PATH, để Claude Code chạy được lệnh `blender --background --python`
- [ ] Cho Claude Code biết đường dẫn Unity Editor để nó chạy Unity ở chế độ dòng lệnh (build, chạy test, chụp ảnh scene)
- [ ] Tạo tài khoản các trang tài nguyên cần đăng nhập nếu Claude đề xuất, rồi tự tải file theo danh sách Claude đưa

**Cách Claude làm đồ họa**

1. Tìm model nền có sẵn rig và animation, giấy phép CC0 (Quaternius, Kenney, Poly Pizza), ghi nguồn vào `docs/CREDITS.md`.
2. Viết script Python cho Blender để chỉnh thành nhân vật của game: đổi tỷ lệ, tô theo bảng màu chung, thêm phụ kiện như mũ len, khăn che mặt, đèn pin, dép tổ ong.
3. Render ảnh xem trước 3 góc và ảnh đặt cạnh nhân vật khác, lưu vào `Art/Previews` để bạn xem.
4. Xuất FBX, nhập vào Unity, cấu hình rig (Humanoid cho người, Generic cho chó), tạo Animator Controller và nối với ThiefMotor, DogAI, OwnerAI.
5. Chụp ảnh scene bằng script để tự kiểm tra trước khi giao bạn chơi thử.

Việc gì Claude không làm được trong quy trình này (ví dụ một trang bắt đăng nhập), Claude sẽ dừng lại và đưa bạn danh sách cụ thể để làm tay.

**Điểm kiểm tra của bạn**

- [ ] Duyệt phong cách: Claude đưa 2 đến 3 phương án render (màu, tỷ lệ, có viền hay không), bạn chọn một
- [ ] Duyệt từng nhân vật qua ảnh xem trước trước khi Claude làm animation
- [ ] Chơi thử bản có nhân vật thật: đứng xa có nhận ra trộm đang lom khom không, có biết chó đang cảnh giác hay đang hiền không

**Ba nhân vật**

| Nhân vật | Ý tưởng ngoại hình | Yêu cầu gameplay |
| --- | --- | --- |
| Trộm | Gầy, đội mũ len, khăn che nửa mặt, áo khoác quá rộng, túi đồ ăn đeo hông | Dáng lom khom phải khác rõ dáng đứng khi nhìn từ xa |
| Chủ nhà | Ông chú bụng to, áo ba lỗ, quần đùi, dép tổ ong, cầm đèn pin | Hướng nhìn phải đọc được ngay qua đèn pin |
| Chó | Chó ta lông vàng, tai vểnh, đuôi cong | Trạng thái đọc được qua tư thế: cảnh giác, sủa, ăn, vẫy đuôi |

**Danh sách animation**

| Nhân vật | Bắt buộc | Thêm nếu kịp |
| --- | --- | --- |
| Trộm | Đứng, đi, chạy, đi lom khom, đứng lom khom, bế chó khi đi, ném, bị tóm | Chui rào, vấp ngã, ăn mừng |
| Chủ nhà | Ngủ trên võng, thức dậy, đi, chạy, nhìn quanh, tóm | Ngáp, gãi bụng, la mắng |
| Chó | Đứng, đi, chạy, sủa, ăn, vẫy đuôi, nằm trong lòng khi được bế | Ngửi đất, nằm ngủ, liếm mặt trộm |

Động tác chung lấy từ gói CC0; động tác đặc thù như bế chó và nằm võng là phần khó nhất, Claude sẽ ghép từ tư thế có sẵn và dùng IK (Animation Rigging) để tay ôm đúng con chó.

**Hoàn thành khi:** cả ba capsule đã được thay, mọi trạng thái AI có animation tương ứng, game vẫn chạy ít nhất 60 khung hình mỗi giây.

## Giai đoạn 2B: Môi trường, giao diện, âm thanh (tuần 13 đến 22)

Mục tiêu: một màn chơi hoàn chỉnh trông như game thật. Màn này dùng cho trailer, ảnh Steam và bản demo. Làm song song với nhân vật từ tuần 13.

**Môi trường (Claude làm)**

- [ ] Lập danh sách vật thể của màn xóm: hàng rào (thẳng, góc, cổng, đoạn thủng), nhà cấp 4, hiên có võng, chuồng chó, bụi cây, cây chuối, đèn đường, thùng phuy, xe máy, dây phơi đồ
- [ ] Lấy bộ vật thể CC0 kiểu khu dân cư làm nền, dựng thêm vật đặc trưng Việt Nam bằng script Blender
- [ ] Dựng theo mô-đun: mỗi đoạn hàng rào dài đúng 2 mét để ghép khít
- [ ] Dùng chung bảng màu với nhân vật
- [ ] Ánh sáng đêm trong URP: ánh trăng xanh nhạt, đèn hiên vàng ấm; bake ánh sáng tĩnh, chỉ đèn bật tắt được mới để realtime
- [ ] Post-processing nhẹ: vignette, chỉnh màu, bloom cho đèn
- [ ] Đặt lại các LightZone khớp vùng sáng thật

**Giao diện (Claude làm)**

1. Viết style guide: font Be Vietnam Pro (đủ dấu tiếng Việt), 4 đến 5 màu UI lấy từ bảng màu game, kiểu nút, kiểu biểu tượng.
2. Vẽ biểu tượng bằng SVG (con mắt, đồ ăn, cục xương, dấu ? và !), xuất PNG cùng độ dày nét.
3. Render ảnh mẫu từng màn hình để bạn duyệt trước khi dựng.
4. Dựng bằng uGUI và TextMeshPro qua script Editor, lưu thành prefab.
5. Tự kiểm tra ở 1280x720, 1920x1080 và 2560x1440 bằng ảnh chụp tự động.

| Màn hình | Nội dung chính |
| --- | --- |
| Menu chính | Chơi, thành tích, cài đặt, thoát; nền là cảnh sân nhà ban đêm |
| HUD | Thanh tiếng ồn, biểu tượng mắt, số đồ ăn, gợi ý phím, mục tiêu hiện tại |
| Chỉ báo trên đầu AI | Dấu "?" vàng khi nghi ngờ, "!" đỏ khi phát hiện, vòng đếm ngược khi chó đang ăn |
| Tạm dừng | Tiếp tục, chơi lại, cài đặt, về menu |
| Kết quả | Thắng hoặc thua, thời gian, thành tích, chơi lại |
| Thành tích | Toàn bộ chỉ số đã lưu |
| Cài đặt | Độ phân giải, chế độ cửa sổ, chất lượng đồ họa, âm lượng, độ nhạy chuột, đổi phím, ngôn ngữ |

- [ ] Chuyển input sang file .inputactions để làm phần đổi phím
- [ ] Lưu cài đặt ra file riêng, tách khỏi file thành tích
- [ ] Điều hướng menu được bằng tay cầm
- [ ] Phụ đề cho âm thanh quan trọng ("Chó đang sủa!")

**Âm thanh (Claude làm)**

- [ ] Tiếng động: bước chân trên đất, cỏ, gạch; chó sủa, gầm gừ, ăn, rên vui; chủ nhà ngáy, la, chạy dép lê; cổng cót két; đồ ăn rơi
- [ ] Âm thanh nền: dế kêu, gió, xe máy chạy xa, chó hàng xóm
- [ ] Nhạc: một bản lén lút nhẹ, một bản dồn dập khi bị đuổi, chuyển mượt giữa hai bản
- [ ] Nguồn: gói âm thanh CC0, tiếng động đơn giản tạo bằng code; file ở trang cần đăng nhập thì Claude đưa danh sách để bạn tải
- [ ] Ghi nguồn và giấy phép mọi file vào `docs/CREDITS.md`

**Điểm kiểm tra của bạn:** duyệt ảnh mẫu giao diện, nghe thử và chọn nhạc, chơi thử bản hoàn chỉnh trên ít nhất hai độ phân giải.

**Hoàn thành khi:** chụp màn hình bất kỳ lúc nào trong ván cũng đủ đẹp để đưa lên trang Steam.

## Giai đoạn 3: Đủ nội dung cho bản phát hành (tuần 23 đến 34)

Mục tiêu: đủ nội dung cho 3 đến 5 giờ chơi: ba màn, màn hướng dẫn, và vai chủ nhà đấu với trộm do máy điều khiển.

**Thêm màn chơi** (mỗi màn có một cơ chế mới)

| Màn | Bối cảnh | Cơ chế mới |
| --- | --- | --- |
| 1. Xóm nhỏ | Nhà cấp 4, một chó, chủ nhà ngủ võng | Cơ chế gốc: tiếng ồn, bóng tối, đồ ăn |
| 2. Biệt thự | Tường cao, cổng điện, hai chó | Camera an ninh quay qua lại, đèn cảm biến |
| 3. Trang trại ven sông | Sân rộng, ao, chuồng gà | Đàn ngỗng kêu ầm khi bị đến gần, đường thoát bằng thuyền |

- [ ] Claude dựng màn greybox trước, bạn chơi thử, đạt mới làm đồ họa thật cho màn đó
- [ ] Màn hướng dẫn 3 phút dạy lom khom, ném đồ ăn, bế chó
- [ ] Huy chương mỗi màn: thoát được, không bị phát hiện, dưới thời gian mục tiêu
- [ ] Màn chọn màn chơi, lưu tiến độ

**Vai chủ nhà (chơi đơn)**

- [ ] Claude viết `OwnerMotor` cho người chơi điều khiển chủ nhà, dùng lại `ICharacterInput`
- [ ] AI trộm: trinh sát, chọn đường vào, dụ chó, chạy khi bị phát hiện
- [ ] Đồ nghề chủ nhà: đèn pin, khóa cổng, rải sỏi gây tiếng động, gọi hàng xóm
- [ ] Thành tích số lần bắt được trộm (đã có sẵn trong code)

**Hoàn thiện**

- [ ] Hai ngôn ngữ tiếng Việt và tiếng Anh (gói Unity Localization)
- [ ] Tối ưu để chạy 60 khung hình mỗi giây trên máy tầm trung, ví dụ card GTX 1650
- [ ] Một đợt săn lỗi: bạn chơi lại mọi màn theo checklist của Claude, ghi lỗi vào GitHub Issues

**Hoàn thành khi:** người chơi mới tự qua màn hướng dẫn không cần hỏi, chơi trọn ba màn và vai chủ nhà mà không gặp lỗi nghiêm trọng.

## Giai đoạn 4: Steam và phát hành bản chơi đơn (bắt đầu từ tuần 22, phát hành khoảng tuần 40 đến 46)

Mục tiêu: trang Steam lên sớm để gom lượt wishlist trong lúc vẫn làm game, rồi phát hành bản chơi đơn có đủ nội dung.

**Mở trang Steam sớm (ngay sau giai đoạn 2B)**

- [ ] Bạn: đăng ký Steamworks, đóng phí 100 USD cho mỗi game, khai thông tin thuế và ngân hàng
- [ ] Claude: viết mô tả game tiếng Việt và tiếng Anh, danh sách tính năng, thẻ phân loại
- [ ] Claude: dựng cảnh và render ảnh bìa (capsule) theo đúng các kích thước Steam yêu cầu
- [ ] Claude: viết script đặt camera và chụp tự động ít nhất 5 ảnh màn hình đẹp
- [ ] Claude dựng sẵn các cảnh quay trailer 60 đến 90 giây; bạn chơi các đoạn cần người điều khiển để Claude ghi hình bằng Unity Recorder
- [ ] Bạn: duyệt mọi thứ rồi bấm công bố trang "Coming Soon"

**Gom người quan tâm**

- [ ] Đăng devlog ngắn mỗi tuần (TikTok, YouTube Shorts, Facebook nhóm game): Claude viết kịch bản và chọn đoạn quay, bạn đăng
- [ ] Lập server Discord cho người chơi thử
- [ ] Claude lập danh sách streamer và YouTuber chơi game lén lút, viết email giới thiệu; bạn gửi
- [ ] Làm bản demo màn 1 và đăng ký tham gia Steam Next Fest gần nhất

**Chuẩn bị phát hành**

- [ ] Claude tích hợp Steamworks: thành tích Steam (achievements), bảng xếp hạng, lưu đám mây
- [ ] Claude viết checklist phát hành; bạn chơi thử bản build cuối trên máy khác máy làm việc
- [ ] Bạn: chọn giá bán, chọn ngày phát hành, gửi bản build cho Steam duyệt
- [ ] Lưu ý quy định Steam: phải chờ 30 ngày sau khi đóng phí mới được phát hành, và trang Coming Soon phải hiện ít nhất 2 tuần (kiểm tra lại trong tài liệu Steamworks lúc làm)

**Sau phát hành**

- [ ] Tuần đầu: bạn đọc mọi đánh giá và báo lỗi, Claude sửa và ra bản vá
- [ ] Tháng đầu: một bản cập nhật nhỏ theo góp ý người chơi
- [ ] Ghi rõ trên trang Steam rằng chế độ nhiều người chơi đang được làm, rồi chuyển sang giai đoạn 5

**Hoàn thành khi:** game đã lên Steam, không có lỗi khiến người chơi không chơi tiếp được, và bạn có kế hoạch cho bản cập nhật đầu tiên.

## Giai đoạn 5: Đối kháng nhiều người, bản cập nhật sau phát hành (tuần 47 đến 62)

Mục tiêu: một chủ nhà đấu với 1 đến 3 trộm qua mạng, mời bạn bè qua Steam là vào chơi được, phát hành dưới dạng bản cập nhật miễn phí cho người đã mua. Đây là phần kỹ thuật khó nhất của dự án; bộ khung code đã chừa sẵn chỗ để chuyển đổi (xem `docs/ARCHITECTURE.md`).

**Chọn hạ tầng mạng (bạn quyết, Claude so sánh trước)**

- Netcode for GameObjects cho phần đồng bộ trận đấu (đã chọn trong kiến trúc).
- Kết nối giữa người chơi: dùng mạng Steam (miễn phí, mời bạn qua Steam) hoặc Unity Relay và Lobby (có hạn mức miễn phí, không phụ thuộc Steam). Claude viết bản so sánh chi phí và độ khó trước khi bắt tay.

**Các bước (Claude làm theo thứ tự, mỗi bước bạn test xong mới sang bước sau)**

1. Hai người cùng thấy nhau di chuyển trong một sân, chạy hai cửa sổ game trên cùng một máy.
2. Chuyển `MatchManager`, tiếng ồn và AI chó sang chỉ chạy trên máy chủ ván.
3. Thay tham chiếu một trộm trong AI bằng danh sách trộm.
4. Vai chủ nhà do người điều khiển, dùng `OwnerMotor` từ giai đoạn 3.
5. Sảnh chờ: tạo phòng, mời bạn, chọn vai, sẵn sàng.
6. Xử lý rớt mạng, người thoát giữa ván, máy chủ ván thoát.
7. Bảng thành tích riêng cho chế độ online.

**Việc của bạn**

- [ ] Rủ 2 đến 3 người bạn, mỗi tuần một buổi test online 30 phút
- [ ] Thử trên hai mạng khác nhau (ví dụ wifi nhà và 4G phát từ điện thoại) để thấy độ trễ thật
- [ ] Ghi lại cảm giác: có lúc nào thấy nhân vật giật, bị bắt dù đã chạy xa không

**Hoàn thành khi:** 4 người chơi 10 ván liên tiếp không bị lỗi đồng bộ hay văng game, ai cũng thấy kết quả ván giống nhau, và bản cập nhật đã lên Steam.

## Công cụ, chi phí, rủi ro và cách làm việc với Claude Code

Chi phí bắt buộc chỉ khoảng 100 USD phí Steam cộng gói Claude có Claude Code; phần lớn công cụ còn lại miễn phí.

**Công cụ và chi phí**

| Công cụ | Dùng để | Chi phí |
| --- | --- | --- |
| Unity 6.3 LTS (Personal) | Làm game | Miễn phí dưới ngưỡng doanh thu Unity quy định |
| Blender | Claude dựng và chỉnh model bằng script | Miễn phí |
| Git, GitHub, Git LFS | Lưu code và asset | Miễn phí, dung lượng LFS có giới hạn |
| Gói tài nguyên CC0 (Quaternius, Kenney, Poly Pizza) | Model, âm thanh nền | Miễn phí |
| Steamworks | Bán game trên Steam | 100 USD mỗi game |
| Gói Claude có Claude Code | Claude làm phần sản xuất | Theo gói bạn đăng ký |
| Họa sĩ thuê ngoài (không bắt buộc) | Ảnh bìa Steam, logo | Tùy người, nên dành ngân sách |

**Rủi ro và cách xử lý**

| Rủi ro | Dấu hiệu | Cách xử lý |
| --- | --- | --- |
| Phạm vi phình to | Thêm tính năng mới trước khi xong giai đoạn | Chỉ làm việc có trong kế hoạch; ý tưởng mới ghi vào mục "để sau" |
| Đồ họa CC0 trông chung chung | Người xem không nhận ra game có gì riêng | Đầu tư vào chi tiết Việt Nam (dép tổ ong, võng, xe máy); thuê họa sĩ cho ảnh bìa |
| Claude không nhìn thấy Unity Editor | Lỗi chỉ thấy khi chơi | Claude chụp ảnh tự động; bạn gửi ảnh chụp và nội dung Console |
| Multiplayer kéo dài | Giai đoạn 5 trễ quá 4 tuần | Ra trước chế độ 1 chủ nhà đấu 1 trộm, báo tiến độ đều đặn trên trang Steam |
| Vi phạm giấy phép tài nguyên | File không rõ nguồn trong project | Chỉ dùng CC0 hoặc đã mua; mọi file ghi trong `docs/CREDITS.md` |
| Chủ đề trộm chó gây phản cảm | Bình luận tiêu cực về nội dung | Giữ giọng hài, chó không bao giờ bị hại, chủ nhà luôn có cơ hội thắng |
| Mất động lực | Nhiều tuần không chơi thử | Mốc nhỏ mỗi 1 đến 2 tuần, đăng devlog để có người theo dõi |

## Ý tưởng để sau

(Ghi ý tưởng mới vào đây thay vì làm ngay.)
