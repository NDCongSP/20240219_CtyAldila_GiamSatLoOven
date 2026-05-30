# CLAUDE.md — Project Intelligence File
> Đọc file này **trước tiên** mỗi khi bắt đầu làm việc với project.  
> Dành cho: Claude Code · Claude Cowork · Cursor · Copilot  
> Cập nhật lần cuối: xem `## CHANGELOG`

---

## 📌 MỤC LỤC

1. [Project Overview](#1-project-overview)
2. [Architecture Manual](#2-architecture-manual)
3. [Coding Standards & Comment Rules](#3-coding-standards--comment-rules)
4. [Session Memory & Context Linking](#4-session-memory--context-linking)
5. [Changelog — Edit Log](#5-changelog--edit-log)
6. [Unit Test Guidelines](#6-unit-test-guidelines)
7. [Performance Optimization Rules](#7-performance-optimization-rules)
8. [How to Use This File](#8-how-to-use-this-file)

---

## 1. PROJECT OVERVIEW

```yaml
project_name:     "GiamSat.API — Hệ thống Giám Sát Lò Oven"
version:          "1.0.0"
language:         "C# (.NET 7.0 SDK 7.0.410)"
framework:        "ASP.NET Core 7 Web API + Blazor Server + WinForms SCADA"
package_manager:  "NuGet / dotnet CLI"
primary_author:   "Cong.Nguyen <ndcong08cddv02@gmail.com>"
repo:             "D:\\MyCompany\\8.SourceCode\\3.Projects\\20240219_CtyAldila_GiamSatLoOven\\sourceCode\\GiamSat.API"
env:              "development"   # development | staging | production
started:          "2024-02-19"
client:           "Công ty Aldila"
```

### Mục tiêu
Hệ thống giám sát & điều khiển lò nung (Oven) tại nhà máy Aldila. Theo dõi realtime nhiệt độ, trạng thái, hồ sơ nung (Profile) của 13 lò, đồng thời quản lý dữ liệu máy Revo (cuộn sợi), auto-sanding và temperature monitoring. Cung cấp REST API cho UI Blazor và các SCADA client WinForms.

### Ràng buộc quan trọng
- [x] Mọi API response phải bọc trong `Result<T>` (xem `GiamSat.Models/Result/`)
- [x] Không dùng OFFSET lớn trong query — dùng cursor hoặc lọc theo ngày
- [x] JWT Secret và Connection String **không** commit vào source — đặt trong `appsettings.json` (cần gitignore prod config)
- [x] FT-table pattern: cột dữ liệu phức tạp lưu dưới dạng JSON string (C000, C001...) — deserialize bằng `JsonConvert.DeserializeObject<T>()`
- [x] Tất cả DB context timeout đặt 60 phút trong Service constructor
- [x] CORS mở toàn bộ (`AllowAll`) — chỉ dùng cho nội bộ nhà máy, KHÔNG expose ra internet

---

## 2. ARCHITECTURE MANUAL

### 2.1 Cấu trúc Solution (GiamSat.API.sln)

```
GiamSat.API.sln
├── GiamSat.API/               # ASP.NET Core 7 Web API — backend chính
│   ├── Controllers/           # REST Controllers kế thừa BaseController<TId,T>
│   ├── DbContext/             # ApplicationDbContext (IdentityDbContext)
│   ├── Hubs/                  # SignalR Hubs (LogsHub tại /hubs/logs)
│   ├── Logging/               # SignalRLogSink — đẩy log realtime lên UI
│   ├── Middleware/            # CorrelationIdMiddleware — gắn X-Correlation-Id mỗi request
│   ├── Migrations/            # EF Core migrations
│   ├── Services/              # Business logic: SFT01..SFT16, SCommon, PdfExportRevo
│   ├── GlobalVariable.cs      # Biến global: ConString
│   ├── PermissionSeeder.cs    # Seed permissions vào DB khi khởi động
│   ├── Program.cs             # Entry point, cấu hình Serilog bootstrap
│   └── Startup.cs             # DI, middleware pipeline, JWT, CORS, Swagger
│
├── GiamSat.Models/            # Shared library (target: net7.0 + net48)
│   ├── Entities/              # EF Core entities: FT01..FT16_xxx, RoleToPermission
│   ├── NotTable/              # Models không map DB: ConfigModel, OvenInfoModel, v.v.
│   ├── Services/              # Interfaces: ISFT01..ISFT16, IRepository<TId,T>
│   ├── Result/                # Result<T> wrapper cho mọi API response
│   ├── Enums/                 # EnumProfileStepType, EnumMachineType, EnumSandingMode...
│   ├── Security/              # AppPermissions, AppModule, AppAction
│   └── Conts/                 # ApiRoutes constants
│
├── GiamSat.APIClient/         # RestEase HTTP client — dùng trong Blazor UI & SCADA
├── GiamSat.UI/                # Blazor Server UI
├── GiamSat.Scada/             # WinForms SCADA desktop client
├── Scada_TrackingTIme_Revo/   # WinForms — theo dõi thời gian cuộn Revo
├── Scada.TrackingTime_AutoRolling1/ # WinForms — auto-rolling
├── Scada.TemperatureMonitoring/     # WinForms — giám sát nhiệt độ
├── sql/                       # Scripts SQL (views, stored procedures)
├── Database/                  # Backup DB
└── ConnectionSchema_*.json    # Schema kết nối cho từng loại máy/thiết bị
```

### 2.2 Luồng dữ liệu (Data Flow)

```
[Blazor UI / WinForms SCADA]
         │ HTTP / RestEase client
         ▼
[GiamSat.APIClient]
         │ HTTP REST calls
         ▼
[GiamSat.API — Controllers]     ← JWT Bearer auth + Permission policy check
         │ DI inject (Scoped)
         ▼
[Services: SFT01..SFT16]        ← toàn bộ business logic nằm ở đây
         │ EF Core / Dapper
         ▼
[ApplicationDbContext]           ← SQL Server (phucthinhautomation.ddns.net,1433 / db: oven)
         │
         ▼
[SQL Server — Database "oven"]

[Serilog] ──► Console + File all-.log (daily) + error-.log (90d) + structured-.json + SignalR sink
[SignalR LogsHub (/hubs/logs)] ──► Blazor UI realtime log stream
```

### 2.3 Quy tắc phân tầng (Layer Rules)

| Layer            | Được phép dùng                             | Không được dùng               |
|------------------|--------------------------------------------|-------------------------------|
| Controllers      | Services (qua SCommon), Result<T>          | ApplicationDbContext trực tiếp |
| Services (SFTxx) | ApplicationDbContext, Dapper, Models       | Controllers, HttpContext trực tiếp (dùng IHttpContextAccessor) |
| Models           | System, EF Core annotations, RestEase      | Controllers, Services, DbContext |
| APIClient        | GiamSat.Models, RestEase                   | Mọi layer API-side             |

### 2.4 Entities & FT-Table Pattern

Mỗi bảng FT có cột `C000`, `C001`... lưu JSON serialize của complex object. **KHÔNG** thêm cột mới bừa bãi — nếu cần thêm dữ liệu, dùng cột JSON mới hoặc bảng FT mới.

| Table                    | C000                     | C001             | Mô tả                           |
|--------------------------|--------------------------|------------------|---------------------------------|
| FT01                     | ConfigModel (JSON)       | OvensInfo (JSON) | Cấu hình hệ thống + danh sách lò|
| FT02                     | RealtimeDisplays (JSON)  | —                | Dữ liệu realtime hiển thị       |
| FT03                     | —                        | —                | (xác nhận với dev)              |
| FT04                     | —                        | —                | (xác nhận với dev)              |
| FT05                     | —                        | —                | (xác nhận với dev)              |
| FT06                     | —                        | —                | (xác nhận với dev)              |
| FT07_RevoConfig          | RevoConfigs (JSON)       | —                | Cấu hình 9 máy Revo             |
| FT08_RevoRealtime        | —                        | —                | Realtime máy Revo               |
| FT09_RevoDatalog         | —                        | —                | Datalog máy Revo                |
| FT10_TemperatureConfig   | —                        | —                | Cấu hình nhiệt độ               |
| FT11_TemperatureRealtime | —                        | —                | Realtime nhiệt độ               |
| FT12_TemperatureDatalog  | —                        | —                | Datalog nhiệt độ                |
| FT13_TemperatureAlarmLog | —                        | —                | Alarm log nhiệt độ              |
| FT14_TipOdFreq           | —                        | —                | Tip / OD / Frequency            |
| FT15_SandingRealtime     | —                        | —                | Realtime auto-sanding           |
| FT16_SandingLogData      | —                        | —                | Log data auto-sanding           |

**SQL Views** (HasNoKey — không dùng Migration):
- `RevoReportHourVm` → DTO: `RevoGetTotalShaftCountDto`
- `RevoReportStepVm` → DTO: `RevoReportStepVm`

### 2.5 Quyết định kiến trúc (ADR)

| ID    | Ngày       | Quyết định                                        | Lý do                                            | Trạng thái |
|-------|------------|---------------------------------------------------|--------------------------------------------------|------------|
| ADR-1 | 2024-02-19 | FT-table pattern: JSON columns (C000, C001...)    | Linh hoạt thay đổi schema không cần migration    | Accepted   |
| ADR-2 | 2024-02-19 | SignalR đọc JWT từ query `?access_token=`         | WebSocket không truyền được Authorization header | Accepted   |
| ADR-3 | 2024-02-19 | BaseController<TId,T> generic cho CRUD chuẩn      | Giảm boilerplate, chuẩn hóa API endpoint         | Accepted   |
| ADR-4 | 2024-02-19 | Serilog + SignalR sink cho realtime log stream    | Operator xem log trực tiếp trên UI               | Accepted   |
| ADR-5 | 2024-02-19 | Background Task.Run() cho DB seeder trong Configure() | Tránh API treo nếu DB chậm/timeout           | Accepted   |
| ADR-6 | 2024-02-19 | CORS AllowAll — môi trường nội bộ nhà máy         | Simplified config, không expose ra internet      | Accepted   |
| ADR-7 | 2024-02-19 | Dapper bên cạnh EF Core                           | Query báo cáo phức tạp dùng Dapper nhanh hơn     | Accepted   |

---

## 3. CODING STANDARDS & COMMENT RULES

### 3.1 Cấu trúc comment bắt buộc (C# XML Docs)

#### Class / File header
```csharp
/// <summary>
/// Mô tả ngắn chức năng của class này.
/// </summary>
/// <remarks>
/// File:     GiamSat.API/Services/SFT01.cs
/// Author:   Cong.Nguyen
/// Created:  YYYY-MM-DD
/// Modified: YYYY-MM-DD — mô tả thay đổi ngắn
/// </remarks>
```

#### Method / Property
```csharp
/// <summary>
/// Lấy toàn bộ bản ghi cấu hình lò từ database.
/// </summary>
/// <returns>Result chứa List FT01, hoặc Fail nếu có exception.</returns>
public async Task<Result<List<FT01>>> GetAll() { ... }
```

#### Inline comment — chỉ khi logic KHÔNG tự giải thích được
```csharp
// ✅ Đúng: giải thích tại sao
_context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
// Timeout 60 phút — query báo cáo Revo có thể scan hàng triệu bản ghi

// ❌ Sai: lặp lại code
_context.Database.SetCommandTimeout(...); // set timeout
```

#### TODO / FIXME / HACK
```csharp
// TODO(ndcong, YYYY-MM-DD): Tách SFT07 thành SFT07_Config và SFT07_Realtime
// FIXME(ndcong, YYYY-MM-DD): Race condition khi 2 SCADA client ghi FT02 cùng lúc
// HACK(ndcong, YYYY-MM-DD): Workaround — đọc .mdb Revo qua OleDb thay vì API chuẩn
// PERF(ndcong, YYYY-MM-DD): Bottleneck ở query FT09 — xem CHANGELOG#PERF-001
```

### 3.2 Naming conventions (C#)

| Loại                    | Convention         | Ví dụ                                |
|-------------------------|--------------------|--------------------------------------|
| Class / Interface       | PascalCase         | `SFT01`, `ISFT01`, `FT07_RevoConfig` |
| Method / Property       | PascalCase         | `GetAll()`, `InsertAsync()`          |
| Private field           | `_camelCase`       | `_context`, `_httpContextAccessor`   |
| Local variable / param  | camelCase          | `revoConfig`, `model`                |
| Constant / static field | PascalCase         | `HubPath`, `ApiRoutes.GetById`       |
| Enum value              | PascalCase         | `EnumProfileStepType.RampTime`       |
| File                    | PascalCase.cs      | `SFT01.cs`, `FT07_RevoConfig.cs`     |
| Interface               | Prefix `I`         | `ISFT01`, `IRepository<TId,T>`       |

### 3.3 Code style

```
- Indent: 4 spaces (không dùng tab) — Visual Studio default
- Braces: Allman style (mở ngoặc xuống dòng mới)
- async/await: bắt buộc, không dùng .Result / .Wait() (gây deadlock ASP.NET Core)
- var: dùng khi type đã rõ từ context
- Nullable: dùng ? cho nullable reference type khi cần
- Region: dùng #region để nhóm logic lớn (theo convention hiện tại của codebase)
```

### 3.4 Các pattern bắt buộc

**Result wrapper** — mọi Service method phải wrap trong try/catch → Result:
```csharp
public async Task<Result<FT01>> GetById(Guid id)
{
    try
    {
        var res = await _context.FT01.FindAsync(id);
        return await Result<FT01>.SuccessAsync(res);
    }
    catch (Exception ex)
    {
        return await Result<FT01>.FailAsync(ex.Message);
    }
}
```

**FT JSON column** — serialize/deserialize dữ liệu:
```csharp
// Ghi vào DB
entity.C000 = JsonConvert.SerializeObject(configModel);

// Đọc từ DB
var config = JsonConvert.DeserializeObject<ConfigModel>(entity.C000);
```

**Thêm Service mới** — quy trình chuẩn 4 bước:
```
1. Tạo Interface ISFTxx trong GiamSat.Models/Services/
2. Tạo Service SFTxx trong GiamSat.API/Services/ (implement ISFTxx)
3. Tạo Controller FTxxController trong GiamSat.API/Controllers/
4. Đăng ký DI trong Startup.cs: services.AddScoped<ISFTxx, SFTxx>();
```

---

## 4. SESSION MEMORY & CONTEXT LINKING

### 4.1 Active Context — Việc đang làm

```yaml
# Cập nhật phần này MỖI KHI kết thúc session làm việc
active_context:
  current_task:     "DONE — Fix logic phân bổ row: totalRows = rpmSteps×2, điền data theo ShaftNum tăng dần"
  related_files:
    - "GiamSat.API/Services/SFT14_CalcData.cs"  # FIX: totalRows=rpmValues.Count*2; mỗi RPM 2 row (rowIdx/2); dừng khi đủ totalRows
  blocked_by:       ""
  next_step:
    - Seeding data trong bảng FT16 theo part 
  last_session:     "2026-05-31"
  open_questions:
    - "FT03, FT04, FT05, FT06 chứa dữ liệu gì? (DataLog / Alarm / Profile / Control PLC?)"
    - "Production appsettings có khác với appsettings.json không? Đang deploy ở đâu?"
```

### 4.2 Quyết định đã chốt (Decision Log)

| ID      | Ngày       | Quyết định                                            | Ai quyết | File liên quan                     |
|---------|------------|-------------------------------------------------------|----------|------------------------------------|
| DEC-001 | 2024-02-19 | JWT Secret trong appsettings.json (môi trường nội bộ) | Dev      | `appsettings.json`, `Startup.cs`   |
| DEC-002 | 2024-02-19 | CORS AllowAll cho môi trường nội bộ nhà máy           | Dev      | `Startup.cs`                       |
| DEC-003 | 2024-02-19 | Seeder chạy background Task.Run — không block startup | Dev      | `Startup.cs#Configure()`           |
| DEC-004 | 2024-02-19 | DB timeout 60 phút trong mọi Service constructor      | Dev      | `SFT01.cs` và các SFTxx khác       |
| DEC-005 | 2024-02-19 | Permission-based auth + Role-based (Admin/Operator/User) | Dev   | `PermissionSeeder.cs`, `Startup.cs`|
| DEC-006 | 2024-02-19 | EF Core retry on failure: maxRetry=5, delay=10s       | Dev      | `Startup.cs#ConfigureServices()`   |

### 4.3 Hướng dẫn Claude đọc context

Khi bắt đầu session mới, Claude PHẢI:
1. Đọc `active_context` → biết đang làm gì
2. Đọc `CHANGELOG` gần nhất → biết đã thay đổi gì
3. Đọc `Decision Log` → tránh đề xuất lại phương án đã bác bỏ
4. **Không** hỏi lại những gì đã ghi trong file này

**Prompt mẫu để bắt đầu session:**
```
Đọc CLAUDE.md và tiếp tục từ active_context.
Task hiện tại: [mô tả]. File cần làm việc: [list file].
```

---

## 5. CHANGELOG — EDIT LOG

> Ghi lại **mọi thay đổi đáng kể** theo thứ tự ngược (mới nhất lên đầu).  
> Format: `[YYYY-MM-DD] [TYPE] [File/Module] — Mô tả`  
> Types: `FEAT` · `FIX` · `REFACTOR` · `PERF` · `TEST` · `DOCS` · `CHORE` · `BREAK`

---

### [2026-05-31] — Session: Fix logic phân bổ row theo bước nhảy RPM

```
[FIX]  SFT14_CalcData.cs  — Đổi cách tính số row: totalRows = rpmValues.Count × 2 (2 row/bước nhảy)
                             Vd: From=100, To=500, Step=100 → 5 bước → 10 rows
                             RPM gán theo rowIdx/2: row 0-1 → rpm[0], row 2-3 → rpm[1], ...
                             Dừng điền khi đạt totalRows (không phụ thuộc vào số record Fre1 trong DB)
```

---

### [2026-05-31] — Session: Fix schema FreMeasurementRecord khớp DB thực tế

```
[FIX]  FreMeasurementRecord.cs  — Đổi kiểu Id từ long (Int64) → int (Int32)
                                   ShaftNum: int → int? (DB cho phép null)
                                   BSL, Weight, Reading, UL, LL: double? → string? (DB lưu nvarchar(50))
                                   Thêm IsCalib int? (có trong DB nhưng thiếu trong entity)
[FIX]  SFT14_CalcData.cs        — Thêm ParseReading(string?) để parse nvarchar→double (InvariantCulture)
                                   Lọc ShaftNum != null trước khi query, xử lý .HasValue trong loop/dictionary
                                   Nguyên nhân: bảng DatalogFrequency trong external DB dùng int, không phải bigint
                                   Lỗi runtime: "Unable to cast object of type 'System.Int32' to type 'System.Int64'"
```

---

### [2026-05-30] — Session: Tab 2 AutoSanding — bỏ StiffnessZ, StiffnessY từ FT16, Part searchable

```
[FIX]  AutoSandingConfigModel.cs   — Bỏ property StiffnessZ khỏi AutoSandingTestRow (không dùng nữa)
[FIX]  FT14CalcDataClient.cs       — Bỏ StiffnessZ khỏi DTO AutoSandingTestRow trong APIClient
[FEAT] SFT14_CalcData.cs           — Inject ApplicationDbContext; query FT16.SpineB (SandingMode=Test, Part+Work)
                                     và khớp StiffnessY theo ShaftNum với Fre1 records từ external DB
[FIX]  AutoSandingConfig.razor     — Part dropdown: thêm AllowFiltering=true + CaseInsensitive để search được
                                     Bảng dữ liệu: bỏ cột "Stiffness Z" (header + data cell)
[FIX]  AutoSandingConfig.razor.cs  — Fake data: bỏ StiffnessZ; mapping OnLoadDataFromDB: bỏ StiffnessZ
```

---

### [2026-05-29] — Session: Fix lỗi tạo role và seed permission cho phân hệ Sanding

```
[FIX]  IdentityAdminDtos.cs              — Đổi kiểu dữ liệu của thuộc tính Id và Claims trong class IdentityRoleDto thành nullable (string? và List<string>?) để tránh lỗi Model Validation 400 Bad Request từ API.
[FEAT] AppModule.cs                      — Thêm Sanding_Config và Sanding_Report vào enum AppModule.
[FEAT] AppPermissions.cs                 — Thêm các hằng số phân quyền mới cho Sanding (Sanding_Config_*, Sanding_Report_View).
[FEAT] PermissionSeeder.cs               — Thêm seeding data cho phân hệ Sanding (Sanding_Config.View/Create/Edit/Delete và Sanding_Report.View).
[FEAT] NavMenu.razor & NavMenu.razor.cs  — Phân quyền hiển thị menu Auto Sanding dựa trên quyền Sanding.
[FEAT] AutoSandingConfig.razor           — Enforce quyền Sanding_Config_View thông qua thuộc tính [Authorize].
[FIX]  PermissionDialog.razor            — Sửa kiểu parameter Model từ APIClient.Permissions thành GiamSat.Models.Permissions để tránh lỗi runtime cast exception khi mở dialog từ PermissionsManager.razor.
```

---

### [2026-05-28] — Session: Tab 2 AutoSanding — Load data từ external DB

```
[FEAT]  FreMeasurementRecord.cs          — Entity mới map bảng "FreMeasurement" trong external SQL Server DB
                                           Cột quan trọng: Station, ShaftNum, Part, WorkOrder, Reading
                                           Station = "Auto Fre No.1" → Fre1 | "Auto Fre No.2" → Fre2
[FEAT]  FreMeasurementDbContext.cs        — DbContext riêng kết nối external DB qua ConnStrExternal
[FEAT]  ISFT14_CalcData.cs               — Interface: GetCalcDataAsync(part, work, offsets, motorFrom/To/Step)
[FEAT]  SFT14_CalcData.cs                — Service: query external DB, khớp Fre1/Fre2 theo ShaftNum,
                                           phân bổ RPM: ceil(count / rpmCount) shafts per RPM level
[FEAT]  FT14Controller.cs               — Endpoint mới: GET /api/FT14/calcdata (query params)
[CHORE] appsettings.json                 — Thêm ConnStrExternal (placeholder — user cần điền thực)
[CHORE] Startup.cs                       — DI: AddDbContext<FreMeasurementDbContext> + AddScoped<ISFT14_CalcData>
[FEAT]  FT14CalcDataClient.cs            — File mới trong GiamSat.APIClient: IFT14CalcDataClient + FT14CalcDataClient
                                           (tạo file riêng thay vì edit GiamSatApi.cs 15640 dòng — Edit tool không reliable)
[CHORE] _Imports.razor                   — Thêm @inject IFT14CalcDataClient _ft14CalcDataClient
[FEAT]  AutoSandingConfig.razor.cs       — Thêm fields: _work, _offsetFre1/2, _offsetSpine, _formular, motor from/to/step
                                           Thêm method OnLoadDataFromDB() gọi _ft14CalcDataClient.GetCalcDataAsync()
[FEAT]  AutoSandingConfig.razor          — UI Tab 2: form Work/Offset/Formular/Motor (3 rows) + Load Data từ DB button
```

---

### [2026-05-26] — Session: Fix FT14 data không hiển thị

```
[FIX]  AutoSandingConfig.razor.cs  — LoadData(): đổi filter Actived==true → Actived!=false
                                      (records có Actived=NULL trong DB bị lọc mất → không hiện data)
                                      Thêm error notification khi result.Succeeded==false
[FIX]  DialogAutoSandingConfig.razor.cs — Thêm 2 field bị thiếu khi clone model:
                                          Length (mất khi Edit → lưu lại 0/null)
                                          UpdateddAt (mất timestamp update)
[FIX]  SFT14.cs — GetAll(): đổi ToList() đồng bộ → await ToListAsync() + AsNoTracking()
```

---

### [2026-05-26] — Session: Tái sinh GiamSatApi.cs qua NSwag

```
[CHORE] swagger.json        — Regenerate từ running API (http://localhost:64665) — 111 endpoints,
                              bao gồm /api/TemperatureConfig, /api/TemperatureData/*, /api/FT15, /api/FT16
[CHORE] GiamSatApi.cs       — Tái sinh tự động bởi NSwag (14.0.7) — 15640 dòng:
                              ITemperatureConfigClient, ITemperatureDataClient, IFT15Client, IFT16Client
[CHORE] GiamSat.APIClient.csproj — Bật lại NSwag target (tắt tạm trong DEC-007, nay không cần merge thủ công nữa)
```

---

### [2026-05-26] — Session: Thêm service layer temperature monitoring

```
[FEAT]  ISFT10.cs          — Interface: GetConfigs, SaveConfigs (FT10 JSON config)
[FEAT]  SFT10.cs            — Service: implement ISFT10, timeout 60 phút, Result<T> wrapper
[FEAT]  ISFT11.cs          — Interface: GetRealtime, GetAlarmLogs, GetDataLogs, SyncRealtime, SyncDatalog
[FEAT]  SFT11.cs            — Service: implement ISFT11, alarm dual-state logic chuyển từ controller
[REFACTOR] TemperatureConfigController.cs — inject ISFT10 thay vì ApplicationDbContext trực tiếp
[REFACTOR] TemperatureDataController.cs   — inject ISFT11 thay vì ApplicationDbContext trực tiếp
[CHORE] ApiRoutes.cs       — Thêm FT10 (api/TemperatureConfig) và FT11 (api/TemperatureData) routes
[CHORE] Startup.cs         — Đăng ký AddScoped<ISFT10, SFT10> và AddScoped<ISFT11, SFT11>
```

---

### [2026-05-26] — Session: Merge main → revert1

```
[CHORE] git merge          — Merge nhánh main (temperature monitoring) vào revert1 (auto-sanding).
                             Resolved 7 conflicts thủ công:
                             • NavMenu.razor: giữ cả Auto Sanding + Giám sát nhiệt độ
                             • GiamSatApi.cs: merge tay (FT14/15/16 client + sanding data classes chèn vào bản main)
                             • Logs.razor, LogsTable.razor, PermissionDialog.razor: lấy HEAD (revert1)
                             • index.html: lấy timestamp mới hơn từ main
                             • swagger.json: lấy main (có temperature endpoints)
[CHORE] GiamSat.APIClient.csproj — Tắt NSwag auto-gen target (tránh ghi đè GiamSatApi.cs đã merge thủ công).
                                    Ghi chú: bật lại sau khi có swagger.json đầy đủ cả 2 feature.
[FIX]   JwtAuthenticationService.cs — Sửa signature ResetPassword dùng alias đúng (ResetPasswordModel).
[DOCS]  CLAUDE.md           — Thêm DEC-007, cập nhật active_context sau merge.
```

[DEC-007] 2026-05-26 | GiamSatApi.cs merge thủ công + NSwag tắt tạm | Claude Code
→ Lý do: revert1 có FT14-16, main có temperature types — NSwag chỉ gen từ 1 swagger.json nên không thể tự merge.
→ Action: sau khi chạy API đủ cả 2 feature, tái sinh swagger.json → bật lại NSwag.

---

### [2026-05-25] — Session: Khởi tạo CLAUDE.md thực tế

```
[DOCS]  CLAUDE.md  — Viết lại toàn bộ từ template v1.0 sang v2.0 với thông tin thực tế:
                     language C#/.NET 7, ASP.NET Core 7, multi-project solution,
                     FT-table JSON pattern, JWT, Serilog + SignalR sink, EF Core + Dapper,
                     QuestPDF, BaseController<TId,T>, Result<T>, Permission-based auth.
```

---

## 6. UNIT TEST GUIDELINES

> ⚠️ **Hiện tại chưa có test project trong solution.** Phần này là hướng dẫn khi thêm vào.

### 6.1 Khuyến nghị test framework

```xml
<!-- Thêm project mới: GiamSat.Tests -->
<PackageReference Include="xunit" Version="2.6.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="7.0.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="7.0.*" />
```

### 6.2 Cấu trúc test file

```csharp
/// <summary>
/// Unit tests cho SFT01 — service cấu hình lò.
/// </summary>
public class SFT01Tests
{
    [Fact]
    public async Task GetAll_ShouldReturnSuccess_WhenDataExists()
    {
        // ARRANGE — setup InMemory DB với seed data

        // ACT
        var result = await _service.GetAll();

        // ASSERT
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task GetById_ShouldReturnFail_WhenIdNotFound()
    {
        var result = await _service.GetById(Guid.NewGuid());
        Assert.False(result.Succeeded);
    }
}
```

### 6.3 Test Coverage Targets (khi có test project)

| Layer        | Target | Ghi chú                                          |
|--------------|--------|--------------------------------------------------|
| Services/    | 80%    | Happy path + exception → Result.Fail             |
| Controllers/ | 70%    | Integration test qua WebApplicationFactory       |
| Models/      | 90%    | JSON serialize/deserialize round-trip            |

### 6.4 Lệnh chạy test

```bash
dotnet test                                          # Chạy tất cả
dotnet test --collect:"XPlat Code Coverage"          # Coverage report
dotnet test --filter "FullyQualifiedName~SFT01Tests" # Chỉ 1 class
```

---

## 7. PERFORMANCE OPTIMIZATION RULES

### 7.1 Nguyên tắc chung

```
RULE-PERF-01: Đo trước khi tối ưu — dùng SQL Profiler / Serilog timing, không đoán mò
RULE-PERF-02: Ghi PERF comment + ID trước khi thay đổi liên quan đến performance
RULE-PERF-03: Mỗi tối ưu phải có benchmark trước/sau trong CHANGELOG
RULE-PERF-04: Dùng Dapper cho query báo cáo phức tạp, EF Core cho CRUD đơn giản
```

### 7.2 Backend / .NET Performance Checklist

```markdown
- [ ] EF Core queries: dùng AsNoTracking() cho read-only (GetAll, report)
- [ ] N+1 queries: dùng Include() hoặc JOIN thay vì loop
- [ ] JSON columns: cache deserialized object, không deserialize nhiều lần trong 1 request
- [ ] Pagination: KHÔNG dùng Skip(OFFSET) lớn — dùng WHERE Id > lastId (keyset)
- [ ] async/await: dùng toàn bộ, không .Result / .Wait()
- [ ] SignalR: chỉ broadcast khi data thực sự thay đổi
- [ ] DB connection pool: không tạo ApplicationDbContext thủ công bên ngoài DI
- [ ] Logging: không log level Information trong hot path (datalog polling vài giây 1 lần)
```

### 7.3 SQL Server Checklist

```markdown
- [ ] Index trên cột lọc thường dùng: CreatedAt, OvenId, MachineId, Actived
- [ ] FT09_RevoDatalog: bảng log lớn nhất — cần index trên cột thời gian
- [ ] View RevoReportHourVm, RevoReportStepVm: kiểm tra execution plan định kỳ
- [ ] Retry logic: đã có EnableRetryOnFailure(maxRetry:5, delay:10s) trong EF Core
```

### 7.4 Performance Budget

| Metric                      | Target   | Critical Threshold       |
|-----------------------------|----------|--------------------------|
| API p95 latency (CRUD)      | < 200ms  | > 1s → kiểm tra query    |
| API p95 latency (Report)    | < 3s     | > 10s → dùng background job |
| SignalR log broadcast lag   | < 500ms  | > 2s → kiểm tra sink     |
| App startup time            | < 10s    | > 30s → kiểm tra DB seeder |
| EF Core query (simple CRUD) | < 50ms   | > 500ms → thêm index     |

---

## 8. HOW TO USE THIS FILE

### 8.1 Dành cho Claude (AI Assistant)

```
Khi đọc file này, Claude phải:

1. LUÔN đọc toàn bộ file trước khi viết bất kỳ dòng code nào
2. TUÂN THỦ naming convention C# (PascalCase method, _camelCase private field)
3. TUÂN THỦ Result<T> pattern — mọi service method wrap trong try/catch
4. CẬP NHẬT active_context sau mỗi session
5. THÊM entry vào CHANGELOG mỗi khi sửa/thêm/xoá code đáng kể
6. THAM CHIẾU Decision Log trước khi đề xuất kiến trúc/công nghệ mới
7. KHÔNG lặp lại câu hỏi đã có câu trả lời trong file này
8. KHÔNG tạo ApplicationDbContext thủ công — luôn dùng DI injection
9. KHÔNG dùng .Result hay .Wait() trên Task — gây deadlock ASP.NET Core
10. KHI thêm Service mới: Interface → Service → Controller → DI → (Migration nếu cần)
```

### 8.2 Dành cho Developer

```bash
# Mỗi khi bắt đầu ngày làm việc
git pull origin main
dotnet build GiamSat.API.sln

# Chạy API local
cd GiamSat.API
dotnet run
# → Swagger UI: http://localhost:<port>/swagger

# Trước khi commit
dotnet build --no-incremental    # build sạch
# Thêm CHANGELOG entry vào CLAUDE.md

# EF Core migration
cd GiamSat.API
dotnet ef migrations add <TenMigration>
dotnet ef database update
```

### 8.3 Template prompt để dùng với Claude Code / Cowork

```
# Bắt đầu task mới:
"Đọc CLAUDE.md. Task: [mô tả task].
Các file liên quan: [list files].
Sau khi xong, cập nhật active_context và CHANGELOG."

# Debug / fix:
"Đọc CLAUDE.md section 4 (context) và file [X].
Bug: [mô tả]. Expected: [hành vi đúng].
Ghi FIX vào CHANGELOG sau khi sửa xong."

# Thêm tính năng mới (FT table):
"Đọc CLAUDE.md section 2.4 (FT-table pattern).
Thêm entity FTxx cho [mục đích].
Tạo theo thứ tự: Entity → Interface ISFTxx → Service SFTxx → Controller FTxxController → DI Startup.cs → Migration."

# Code review:
"Đọc CLAUDE.md coding standards (section 3).
Review file [X]: naming convention, Result<T> pattern, XML comment format.
Liệt kê vi phạm theo format: [Line] [Rule] [Gợi ý sửa]."
```

### 8.4 Maintenance

| Việc cần làm                          | Tần suất               | Người chịu trách nhiệm |
|---------------------------------------|------------------------|------------------------|
| Cập nhật `active_context`             | Mỗi session            | Dev đang làm việc      |
| Thêm entry `CHANGELOG`                | Mỗi commit đáng kể     | Dev đang làm việc      |
| Cập nhật bảng Entities (section 2.4)  | Khi thêm FT table mới  | Dev thêm entity        |
| Cập nhật ADR khi có quyết định mới    | Khi phát sinh          | Người quyết định       |
| Audit performance                     | Mỗi tháng              | Dev                    |

---

> **Lưu ý:** File này là nguồn sự thật duy nhất (*single source of truth*) cho AI assistant làm việc với project.  
> Khi có mâu thuẫn giữa code và CLAUDE.md → **ưu tiên CLAUDE.md**, sau đó sửa code cho nhất quán.

---
*CLAUDE.md · Cập nhật bởi Claude Sonnet 4.6 · 2026-05-25 · Phiên bản: 2.0.0 (thực tế hoá từ template v1.0.0)*
