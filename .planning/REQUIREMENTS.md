## v1 Requirements

### Core (Data Collector & Device Connector)
- [ ] **CORE-01**: Background WinForm application parse được thông số hệ thống giám sát từ file đầu vào `easyDriverTagFileOven_Temp.json`.
- [ ] **CORE-02**: Sử dụng thuộc tính `public EasyDriverConnector EasyDriverConnector { get; set; }` để tương tác trực tiếp với thiết bị (nhận dữ liệu gửi qua và ghi/cập nhật lệnh hướng xuống thiết bị).
- [ ] **CORE-03**: Khớp nối dữ liệu đọc từ `EasyDriverConnector` dựa trên cấu hình map định nghĩa sẵn trong Database (`FT10`) để xử lý luồng realtime.

### Logic (Data Processing)
- [ ] **LOGIC-01**: Đồng bộ số liệu thời gian thực (Realtime) vào bảng `FT11_TemperatureRealtime` liên tục.
- [ ] **LOGIC-02**: Ghi lưu Datalog định kỳ (Datalogging) vào bảng `FT12_TemperatureDatalog` để làm cơ sở dữ liệu vẽ biểu đồ.
- [ ] **LOGIC-03**: Nhận biết điều kiện vượt ngưỡng (PV > SV_High hoặc PV < SV_Low) và Insert dòng mới vào bảng `FT13_TemperatureAlarmLog` để đánh dấu thời gian Bắt Đầu sự cố (kèm Nhiệt độ Bắt Đầu).
- [ ] **LOGIC-04**: Tracking khi nhiệt độ khôi phục trở lại trong vùng an toàn (Normal), tiến hành Update lại Record đang mở của `FT13_TemperatureAlarmLog` để lưu Thời gian Kết Thúc (kèm Nhiệt độ Kết Thúc).

### UI (Dashboard & Config)
- [ ] **UI-01**: Xây dựng màn hình Config Page cho phép User thao tác CRUD danh sách cấu hình các vị trí đo nhiệt độ (`TemperatureConfigsModel`), lưu vào bảng `FT10_TemperatureConfig`.
- [ ] **UI-02**: Cung cấp API Controller để fetch dữ liệu từ FT10, FT11, FT12, FT13 đưa ra Frontend.
- [ ] **UI-03**: Tạo Dashboard Blazor (tái sử dụng UI màn Revo) hiển thị biểu đồ và chỉ số thời gian thực từ các vị trí đã thiết lập trong bản đồ của `FT10`.
- [ ] **UI-04**: Xây dựng thêm một **Trang Báo Cáo (Report Page)** chuyên dụng chuyên liệt kê, bộ lọc phân tích (có DataGrid) danh sách lịch sử biến thiên báo động (các Alarm Alerts đã log vào `FT13`) biểu hiện lúc bắt đầu và kết thúc sự cố.

## Out of Scope
- Chỉnh sửa hệ thống cũ Oven/Revo: Yêu cầu này chỉ focus vào 1 web module/winform riêng và cấy chung vào solution, không đụng tới mã thực thi lõi của dây chuyền cũ.
