# Roadmap: Temperature Monitoring Module (Oven/Revo Format)

**Milestone 1**

---

## Phase 1: Setup Device Connector & Database Mapping
**Goal:** Tổ chức kết nối thiết bị với `EasyDriverConnector` và CRUD cấu hình đo nhiệt qua DB.
**Depends on:** None
**Requirements:** CORE-01, CORE-02, UI-01

**Success Criteria:**
1. Khởi tạo đối tượng `EasyDriverConnector` quét và map dữ liệu chuẩn xác từ JSON đầu vào.
2. Xây dựng hoàn chỉnh UI Configuration Page (Blazor) và API (Backend) cho thao tác Thêm/Sửa/Xóa cấu hình vị trí đo ứng với Entity `FT10_TemperatureConfig` (`TemperatureConfigsModel`).

---

## Phase 2: Hệ thống Logic Thu nhận Dữ liệu & Báo động Kép (Engine)
**Goal:** Đọc từ bộ Core đẩy xuống DB thời gian thực, lưu trữ lịch sử báo động (Alarm/Datalog).
**Depends on:** 1
**Requirements:** CORE-03, LOGIC-01, LOGIC-02, LOGIC-03, LOGIC-04

**Success Criteria:**
1. Record trong bảng `FT11` (Realtime) & `FT12` (Datalog) được update liên tục tương thích với các điểm đo từ file cấu hình `FT10`.
2. Hệ thống kiểm soát Cảnh Báo (`FT13`) nắm bắt chính xác mức nhiệt vượt ngưỡng (bắt đầu) -> hồi phục (kết thúc) kèm temperature.

### Phase 4: Thiết lập Test Flow EasyDriver

**Goal:** [To be planned]
**Requirements**: TBD
**Depends on:** Phase 3
**Plans:** 0 plans

Plans:
- [ ] TBD (run /gsd-plan-phase 4 to break down)

---

## Phase 3: Dashboard Realtime & Lịch sử
**Goal:** Trình diễn số liệu lên giao diện UI cho người người dùng trích xuất.
**Depends on:** 2
**Requirements:** UI-02, UI-03, UI-04

**Success Criteria:**
1. Dashboard render thành công các khu vực biểu đồ, chỉ số đọc ra từ `FT10` (Map) và `FT11` (Status) bằng tín hiệu Realtime sạch.
2. Giao diện trang Báo Cáo chuyên dụng liệt kê đầy đủ nhật ký báo động (DataGrid/Table) có khả năng trình bày rõ Start Log/End Log khi alarm xảy ra.
