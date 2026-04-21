## v1 Requirements

### Core (Data Collector)
- [ ] **CORE-01**: Background WinForm application parse được thông số hệ thống giám sát từ file đầu vào `easyDriverTagFileOven_Temp.json`.
- [ ] **CORE-02**: Tự động kết nối Modbus RTU / Server Giả lập qua vòng lặp quét (scan lool) để lấy về thông số thực (PV, SV).

### Logic (Data Processing)
- [ ] **LOGIC-01**: Đồng bộ số liệu thời gian thực (Realtime) vào bảng `FT11_TemperatureRealtime` liên tục.
- [ ] **LOGIC-02**: Ghi lưu Datalog định kỳ (Datalogging) vào bảng `FT12_TemperatureDatalog` để làm cơ sở dữ liệu vẽ biểu đồ.
- [ ] **LOGIC-03**: Nhận biết điều kiện vượt ngưỡng (PV > SV_High hoặc PV < SV_Low) và Insert dòng mới vào bảng `FT13_TemperatureAlarmLog` để đánh dấu thời gian Bắt Đầu sự cố (kèm Nhiệt độ Bắt Đầu).
- [ ] **LOGIC-04**: Tracking khi nhiệt độ khôi phục trở lại trong vùng an toàn (Normal), tiến hành Update lại Record đang mở của `FT13_TemperatureAlarmLog` để lưu Thời gian Kết Thúc (kèm Nhiệt độ Kết Thúc).

### UI (Dashboard)
- [ ] **UI-01**: Cung cấp API Controller để fetch dữ liệu từ FT10, FT11, FT12, FT13 đưa ra Frontend.
- [ ] **UI-02**: Tạo Dashboard Blazor tái sử dụng UI/UX màu và theme của Revo, thể hiện biểu đồ xu hướng.
- [ ] **UI-03**: Tạo View/Table danh sách lịch sử biến thiên báo động với thời điểm bắt đầu, thời điểm hoàn tất phục hồi tương ứng.

## Out of Scope
- Chỉnh sửa hệ thống cũ Oven/Revo: Yêu cầu này chỉ focus vào 1 web module/winform riêng và cấy chung vào solution, không đụng tới mã thực thi lõi của dây chuyền cũ.
