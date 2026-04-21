# Machine Temperature Monitoring (Oven/Revo Style)

## Cốt lõi giá trị (Core Value)
Giám sát thông số nhiệt độ của máy theo thời gian thực và tự động ghi nhận các sự kiện cảnh báo biến thiên nhiệt độ (vượt ngưỡng hoặc phục hồi), nhằm đảm bảo chất lượng và an toàn hệ thống xưởng.

## Dự án là gì? (What This Is)
Dự án nhằm mục đích thiết kế và phát triển một phân hệ (module) giám sát nhiệt độ dựa trên các thiết kế đã áp dụng thành công trên hệ thống Lò Oven và Revo. Module bao gồm:
1. Một ứng dụng WinForm bertạo vai trò như "Trạm trung chuyển dữ liệu" (Data Collector): Đọc file thiết bị gốc JSON (`easyDriverTagFileOven_Temp.json`), kết nối với thiết bị/giả lập để nhận dữ liệu, và lưu vào database (các bảng từ FT10 tới FT13).
2. Một API Web / Dashboard xây dựng trên nền tảng Blazor (nằm trong dự án GiamSat.UI) cho phép hiển thị các báo cáo thời gian thực, với biểu đồ, bảng log báo động.

Trong quá trình phát triển (môi trường Develop), dữ liệu từ phần cứng (máy thực tế) sẽ được mô phỏng bởi phần mềm Modbus/Giả lập Server, sau đó API sẽ cung cấp tín hiệu cho giao diện theo luồng tương tự như Oven/Revo gốc.

## Đối tượng sử dụng (Target Audience)
- Quản đốc nhà máy, Kỹ thuật viên (người cần xem số liệu nhiệt độ để kiểm soát chất lượng).
- Kỹ sư bảo trì, IT (kiểm tra logs để xem tình trạng trục trặc thiết bị).

## Ràng buộc kỹ thuật (Constraints)
- Phải dùng lại/tái sử dụng các Models đang có của Database: `FT10_TemperatureConfig`, `FT11_TemperatureRealtime`, `FT12_TemperatureDatalog`, và `FT13_TemperatureAlarmLog`.
- Parse tags các thông số thiết bị từ file cứng: `easyDriverTagFileOven_Temp.json`.
- Action lưu log hệ thống cần bắt trọn 2 thời điểm: Thời điểm bắt đầu vượt ngưỡng nhiệt độ an toàn (CAO hơn và THẤP hơn) và Thời điểm kết thúc (khi nhiệt độ phục hồi bình thường), lúc lưu cần ghi cùng cả nhiệt độ đầu và giới hạn cuối đó.

## Yêu cầu (Requirements)

### Đã xác thực (Validated)
- ✓ [Hạ tầng Backend] Database GiamSat_API đã sẵn sàng chứa Schema `FT10` - `FT13` và AppContext dùng kèm.
- ✓ [Cấu hình Thiết Bị] Dữ liệu tham chiếu chuẩn Tag Modbus đã được cấp thông qua JSON Easy Driver Tag.

### Đang thực thi (Active)
- [ ] Xây dựng Background Data Collector (Winform Service) kết nối Modbus, parse file `.json` để liên kết thông số PV, SV và ghi status về DB bằng vòng lặp.
- [ ] Ghi Log Datalog định kỳ vào bảng `FT12_TemperatureDatalog`.
- [ ] Trực quan Update Realtime vào bảng `FT11_TemperatureRealtime`.
- [ ] Xử lý Báo Động (Alarm State): Detect biến độ của PV lệch khỏi vùng (SV_Low, SV_High). Khi nhiệt độ chạm hoặc vượt trần/đáy, hệ thống ghi log lưu thời gian bắt đầu. Khi nhiệt độ trở lại vùng an toàn (Normal), tiếp tục cập nhật và lưu lại Record với thời gian kết thúc với nhiệt độ đầu/cuối của ca báo động vào bảng `FT13_TemperatureAlarmLog`.
- [ ] Tạo API Backend (Controller mới) trong `GiamSat.API` để feed báo cáo cho Dashboard Blazor.
- [ ] Tạo Dashboard UI (Blazor Razor Pages / Component) tuân thủ theme màu chuẩn của hệ thống, cho phép xem đồ thị Realtime và Bảng Alarm Logs chi tiết.

### Nằm ngoài phạm vi (Out of Scope)
- Không can thiệp sửa đổi các quy trình và logic core hiện có của hệ thống Oven/Revo cũ.

## Quyết định cốt lõi (Key Decisions)

| Quyết định | Lý do (Rationale) | Kết quả (Outcome) |
|------------|-------------------|-------------------|
| Dùng WinForm làm nền tảng Service kết nối thiết bị | Tương thích và dễ bảo trì kế thừa từ Lò Oven/Revo cũ đã làm | Pending |
| Cơ chế Alarm log Record kép (Begin, End time/temperature) | Yêu cầu log sự kiện đầy đủ thông tin để report khoảng thời gian sự cố chính xác | Pending |


---
*Lần cập nhật cuối: Hôm nay (Sau khi khởi tạo dự án)*

## Tiến trình phát triển bản kế hoạch (Evolution)

Bản tài liệu này sẽ lớn dần và tiến hóa ở các giai đoạn milestone chính.

**Sau mỗi Phase** (sử dụng `/gsd-transition`):
1. Yêu cầu có rớt đài không? → Chuyển xuống "Out of Scope" và ghi lý do
2. Yêu cầu đã hiện thực xong? → Vứt lên "Validated" cùng mã Phase liên kết
3. Nảy sinh Yêu cầu mới mẻ? → Nhét vào "Active"
4. Còn quyết định nào chốt lại? → Đổ nội dung vào bảng Key Decisions
5. "What This Is" vẫn đang chính xác chứ? → Nắn lại cho chuẩn

**Sau một Cột Mốc Milestone lớn** (sử dụng `/gsd-complete-milestone`):
1. Review tổng quát cấu trúc và sự thay đổi
2. Goal Core Value lúc này vẫn hợp lý không? 
3. Xem lại đám Out Of Scope
4. Làm mới lại bối cảnh (Context) dựa vào trạng thái hiện đại
