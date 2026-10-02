# Tài liệu thiết kế game (bản MVP)

## Ý tưởng

Game lén lút 3D, giọng điệu hài hước kiểu hoạt hình. Người chơi là một tên trộm chó vụng về phải đột nhập vào sân nhà, dụ chó và mang chó đi mà không bị chủ nhà tóm. Về lâu dài, người chơi có thể chọn vai trộm hoặc vai chủ nhà, và đấu với nhau qua mạng.

## Trụ cột thiết kế

- **Căng thẳng nhưng vui:** mỗi tiếng bước chân đều có thể làm chó sủa, nhưng thất bại phải buồn cười chứ không ức chế.
- **Đọc được tình huống:** người chơi luôn biết mình đang ồn tới mức nào và có đang bị lộ hay không.
- **Nhiều cách giải:** cùng một sân nhà có thể lẻn qua lỗ hàng rào, đánh lạc hướng bằng đồ ăn, hoặc liều chạy thẳng.

## Phạm vi bản MVP

Có trong bản đầu: một màn chơi (sân nhà ở xóm), vai trộm, chủ nhà và chó do AI điều khiển, hai món đồ ăn dụ chó mỗi ván, bảng thành tích lưu trên máy.

Chưa làm trong bản đầu: vai chủ nhà, chơi nhiều người, nhiều màn, mua sắm đồ nghề, đồ họa và âm thanh hoàn chỉnh.

## Vòng lặp một ván

1. Trinh sát từ ngoài hàng rào, quan sát chủ nhà và chó.
2. Lẻn vào qua cổng hoặc lỗ hàng rào, tận dụng bóng tối và bụi cây.
3. Ném đồ ăn để dụ chó. Chó ăn xong sẽ hiền và cho bế.
4. Bế chó (di chuyển chậm hơn, ồn hơn) và mang ra vùng thoát ngoài cổng.
5. Ván kết thúc khi trốn thoát hoặc bị chủ nhà bắt.

## Cơ chế chính

**Tiếng ồn.** Lom khom gần như không gây tiếng, đi bộ gây tiếng vừa, chạy gây tiếng lớn, bế chó cộng thêm tiếng ồn. Chó nghe thấy sẽ sủa; tiếng sủa rất to và đánh thức chủ nhà.

**Độ lộ diện.** Đứng trong vùng sáng thì bị nhìn thấy từ xa. Trong bóng tối, chủ nhà phải lại gần mới thấy. Lom khom trong bụi cây ở chỗ tối thì gần như vô hình.

**Chó.** Lang thang quanh chuồng, sủa khi phát hiện trộm. Đồ ăn luôn thắng: kể cả đang sủa, chó vẫn bỏ đi ăn. Đang ăn hoặc vừa ăn xong thì cho bế.

**Chủ nhà.** Ngủ (nghe kém), thức dậy khi có tiếng động lớn, đi xem chỗ có tiếng, đi tuần một lúc rồi quay lại ngủ. Nhìn thấy trộm thì đuổi; tới đủ gần là bắt được.

## Thành tích

- Số lần trộm thành công không bị bắt (chỉ số chính).
- Số lần trộm hoàn hảo: thành công mà chủ nhà không hề nhìn thấy.
- Chuỗi thắng hiện tại và chuỗi thắng cao nhất.
- Số lần bị bắt.
- Số lần bắt được trộm (dành cho vai chủ nhà ở bản sau).

## Giọng điệu

Hài hước, hoạt hình, không bạo lực. Chủ nhà bắt trộm bằng cách tóm lại, không đánh đập. Chó không bao giờ bị làm hại, chỉ bị dụ bằng đồ ăn. Trộm chó là vấn đề có thật, nên game giữ giọng vui nhộn và tránh mô tả các hành vi gây hại ngoài đời.
