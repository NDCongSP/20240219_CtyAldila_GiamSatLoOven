# Roadmap: Temperature Monitoring Module (Oven/Revo Format)

**Milestone 1**

---

## Phase 1: Set up Winform Data Collector
**Goal:** Xây dựng phần mềm thu thập dữ liệu bằng C# Winform dựa vào JSON Tag Mapping
**Depends on:** None
**Requirements:** CORE-01, CORE-02

**Success Criteria:**
1. Winform load phân tích được dữ liệu `easyDriverTagFileOven_Temp.json` dưới dạng class Objects (Tag Name, Tag Address).
2. Thiết lập kết nối Modbus Master tới Emulator và thu về trị số chuẩn.

---

## Phase 2: Xử Lý Logic Lưu Database và Ghi Log Báo Động (Alarm State)
**Goal:** Cập nhật DB thời gian thực và ghi nhận chính xác 2 điểm thời gian biến thiên (vượt mức và phục hồi).
**Depends on:** 1
**Requirements:** LOGIC-01, LOGIC-02, LOGIC-03, LOGIC-04

**Success Criteria:**
1. Record trong `FT11` được ghi đè/update liên tục theo refresh rate.
2. Bảng `FT13_TemperatureAlarmLog` ghi nhận chuẩn xác `Thời gian Bắt đầu` + `Nhiệt độ Đầu`, và cập nhật `Thời gian Khôi Phục` + `Nhiệt độ Cuối`.

---

## Phase 3: Triển khai Giao Diện Web Dashboard Blazor
**Goal:** Trình bày giao diện Realtime và Log View trực quan cho người dùng cuối trên Web UI.
**Depends on:** 2
**Requirements:** UI-01, UI-02, UI-03

**Success Criteria:**
1. Dashboard hiển thị đủ chỉ số hiện hành (PV, Setpoint, Status) với giao diện chuẩn sáng (Light Theme Dashboard giống Revo).
2. DataGrid liệt kê đầy đủ nhật ký theo các ca báo động kèm cột `Start Time`, `End Time`, `PV at Start`, `PV at End`.
