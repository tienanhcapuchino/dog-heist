# Nguồn âm thanh và font tạm (M1)

Âm thanh trong M1 là âm thanh tạm, lấy từ các gói miễn phí giấy phép **CC0** (dùng tự do, kể cả thương mại, không cần ghi công). Code không phụ thuộc file cụ thể nào: mỗi loại tiếng là một `AudioCue`, đổi file chỉ cần kéo clip mới vào.

> Luôn kiểm tra lại giấy phép của từng file trên trang tải trước khi dùng. Chỉ dùng file ghi rõ CC0 (hoặc giấy phép tương đương cho phép dùng thương mại).

## Cách gán file vào game

1. Bỏ file âm thanh (`.wav`, `.ogg` hoặc `.mp3`) vào `Assets/_Project/Audio/SFX/`.
2. Trong Unity, chọn cue cần gán trong `Assets/_Project/Audio/Cues/` (ví dụ `DogBark.asset`).
3. Ở ô **Clips** trong Inspector, kéo một hoặc nhiều file vào. Có từ hai file trở lên thì mỗi lần phát chọn ngẫu nhiên và không lặp lại file vừa phát.
4. Chỉnh **Volume Range** và **Pitch Range** nếu tiếng quá to, quá nhỏ hoặc nghe đều đều.

Cue chưa có file thì game vẫn chạy, chỉ im lặng và Console cảnh báo `[Audio]` một lần cho mỗi cue. Chạy lại menu *Build Greybox Level* không xóa các file đã gán.

## Gợi ý nguồn theo từng cue

| Cue | Khi nào phát | Gợi ý file | Nguồn | Giấy phép |
|---|---|---|---|---|
| `FootstepCrouch` | Trộm lom khom di chuyển | Bước chân nhẹ trên cỏ hoặc đất (`footstep_grass`) | [Kenney RPG Audio](https://kenney.nl/assets/rpg-audio) | CC0 |
| `FootstepWalk` | Trộm đi bộ | Bước chân thường (`footstep`) | [Kenney RPG Audio](https://kenney.nl/assets/rpg-audio), [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | CC0 |
| `FootstepSprint` | Trộm chạy | Bước chân mạnh, nhanh | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | CC0 |
| `DogBark` | Chó sủa | Tiếng sủa ngắn, mono | [OpenGameArt: dog barking mono](https://opengameart.org/content/dog-barking-mono), [OpenGameArt: dog sounds](https://opengameart.org/content/dog-sounds) | CC0 |
| `DogHappy` | Chó chuyển sang thân thiện (ăn, hiền) | Tiếng rên vui hoặc nhai | [OpenGameArt: dog sounds](https://opengameart.org/content/dog-sounds), [Freesound lọc CC0: dog whine](https://freesound.org/search/?q=dog+whine&f=license:%22Creative+Commons+0%22) | CC0 |
| `OwnerHuh` | Chủ nhà tỉnh dậy nghi ngờ | Giọng nam "Hửm?" | [Freesound lọc CC0: huh male](https://freesound.org/search/?q=huh+male&f=license:%22Creative+Commons+0%22), hoặc tự thu giọng | CC0 |
| `OwnerShout` | Chủ nhà phát hiện trộm | Tiếng hô "Ê!" / "Trộm!" | [Freesound lọc CC0: hey shout](https://freesound.org/search/?q=hey+shout&f=license:%22Creative+Commons+0%22), hoặc tự thu giọng | CC0 |
| `LureLand` | Đồ ăn chạm đất lần đầu | Tiếng va chạm nhẹ, mềm | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | CC0 |
| `StingerWin` | Trốn thoát thành công | Đoạn nhạc ngắn vui | [Kenney Music Jingles](https://kenney.nl/assets/music-jingles) | CC0 |
| `StingerLose` | Bị bắt | Đoạn nhạc ngắn buồn cười | [Kenney Music Jingles](https://kenney.nl/assets/music-jingles) | CC0 |

Tự thu giọng cho chủ nhà là cách nhanh và hợp giọng điệu hài hước của game: thu bằng điện thoại, cắt còn dưới 1 giây, xuất `.wav` mono.

## File đang dùng trong project (2026-10-03)

| Cue | File trong `Assets/_Project/Audio/SFX/` | Lấy từ | Ghi chú |
|---|---|---|---|
| `FootstepCrouch`, `FootstepWalk`, `FootstepSprint` | `Footsteps/footstep_grass_000–004.ogg` | Kenney Impact Sounds | Chung clip; âm lượng và pitch khác nhau theo cue |
| `DogBark` | `Dog/dog_bark_1–4.wav` | OpenGameArt *dog sounds* (`Dog Bark*.wav` trong `dog.7z`) | Sủa ngắn 0,2–0,4 giây |
| `DogHappy` | `Dog/dog_whine.wav` | OpenGameArt *dog sounds* (`Sad Dog.wav`) | Tạm: tiếng rên dài 4 giây, nên thay bằng tiếng vui ngắn hơn |
| `LureLand` | `Impact/impactSoft_medium_000–002.ogg` | Kenney Impact Sounds | |
| `StingerWin` | `Jingles/jingles_PIZZI00.ogg` | Kenney Music Jingles | Chọn theo tên, chưa nghe thử; đổi sang `PIZZI02`/`PIZZI03` nếu không hợp |
| `StingerLose` | `Jingles/jingles_PIZZI01.ogg` | Kenney Music Jingles | Như trên |
| `OwnerHuh`, `OwnerShout` | (chưa có) | | Freesound cần đăng nhập để tải; tự tải hoặc tự thu giọng |

Giấy phép gốc của các gói Kenney nằm trong `Audio/SFX/Licenses/`. Gói *dog sounds* trên OpenGameArt ghi CC0, không kèm file giấy phép.

## Font tiếng Việt

| File | Nguồn | Giấy phép |
|---|---|---|
| `BeVietnamPro-Regular.ttf` | [Google Fonts: Be Vietnam Pro](https://fonts.google.com/specimen/Be+Vietnam+Pro) | SIL Open Font License 1.1 |

- Chỉ cần file `BeVietnamPro-Regular.ttf` trong gói tải về, đặt vào `Assets/_Project/Art/Fonts/`.
- Giấy phép OFL cho phép dùng trong game thương mại nhưng phải phát hành kèm giấy phép, nên giữ file `BeVietnamPro-OFL.txt` cạnh file font.
- Chạy menu *Build Greybox Level*: công cụ tự tạo `BeVietnamPro SDF.asset` (font động, LiberationSans làm font dự phòng cho ký tự như ♥) và gán cho HUD và dấu trên đầu AI.
