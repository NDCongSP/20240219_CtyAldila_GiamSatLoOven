# STRUCTURE.md — Directory Layout & Organization

## Repository Root

```
e:/Project/OVEN/
├── .git/
├── .github/                # CI/CD pipeline configs
├── .gitignore
├── .dockerignore
├── README.md
├── docker-compose.yml
├── info/                   # Reference docs, REVO data files
└── sourceCode/
    ├── GiamSat.API/        # Main solution directory
    ├── DB/                 # Raw SQL scripts
    └── PLC/                # PLC-related files/configs
```

## Solution: GiamSat.API

```
sourceCode/GiamSat.API/
├── GiamSat.API.sln                     # Visual Studio solution
├── GiamSat.API/                        # Backend Web API
├── GiamSat.UI/                         # Frontend Blazor WASM
├── GiamSat.Interface/                  # Alt/older Blazor app
├── GiamSat.Models/                     # Shared models/DTOs
├── GiamSat.APIClient/                  # NSwag generated HTTP client
├── GiamSat.Scada/                      # Core SCADA WinForms
├── Scada.TemperatureMonitoring/        # Temperature SCADA
├── Scada_TrackingTime_AutoRolling/     # REVO rolling tracker
├── Scada.TrackingTime_AutoRolling1/    # Alt rolling tracker
├── Scada_TrackingTIme_Revo/            # REVO time tracker
├── Database/                           # DB migration artifacts
├── ConnectionSchema_*.json             # PLC/OPC tag configs
├── easyDriverTagFile/                  # EasyDriver tag definitions
├── packages/                           # NuGet package cache
├── sql/                                # Additional SQL scripts
├── BlazorApp1/                         # Scratch/prototype Blazor apps
├── BlazorApp2/
├── BlazorApp3/
└── PublishDeploy/                      # Deployment build output
```

## GiamSat.API (Backend)

```
GiamSat.API/
├── GiamSat.API.csproj
├── Program.cs                          # Bootstrap (calls Startup)
├── Startup.cs                          # Full DI + middleware config
├── GlobalVariable.cs                   # App-wide static globals (ConString)
├── EncodeMD5.cs                        # MD5 decrypt for connection string
├── PermissionSeeder.cs                 # Seed permissions + roles on startup
├── Controllers/
│   ├── BaseController.cs               # Base with shared helpers
│   ├── AuthenticateController.cs       # Login, register, user CRUD
│   ├── PermissionsController.cs        # Roles, permissions, RBAC
│   ├── FT01Controller.cs               # Oven config API
│   ├── FT02Controller.cs               # Realtime display API
│   ├── FT03Controller.cs               # Oven datalog API
│   ├── FT04Controller.cs               # Oven profile tracking API
│   ├── FT05Controller.cs               # Oven summary API
│   ├── FT06Controller.cs               # Misc oven API
│   ├── FT07Controller.cs               # REVO config API
│   ├── FT08Controller.cs               # REVO realtime API
│   ├── FT09Controller.cs               # REVO datalog API
│   └── RevoMockController.cs           # REVO data mock/test endpoint
├── Services/
│   ├── SCommon.cs                      # Common service helpers
│   ├── SFT01.cs–SFT09.cs              # Business logic per feature table
│   └── PdfExportRevo.cs               # QuestPDF REVO report generation
├── DbContext/
│   └── ApplicationDbContext.cs         # EF Core: IdentityDbContext + all DbSets
├── Migrations/
│   └── *.cs                           # 15 EF migrations (2024-03 to 2026-04)
├── Properties/
│   └── launchSettings.json
├── Views/
│   └── DataLog/                       # Razor views (if any)
└── appsettings*.json
```

## GiamSat.UI (Frontend Blazor WASM)

```
GiamSat.UI/
├── GiamSat.UI.csproj
├── Program.cs                          # WASM host setup, DI, auth, culture
├── App.razor                           # Root router component
├── _Imports.razor                      # Global using directives
├── Extensions.cs                       # Service extension helpers
├── Authorization/
│   ├── JwtAuthenticationService.cs     # AuthStateProvider + IAccessTokenProvider
│   ├── JwtAuthenticationHeaderHandler.cs # DelegatingHandler for JWT
│   ├── HttpInterceptorManager.cs       # 401 redirect interceptor
│   ├── IHttpInterceptorManager.cs
│   ├── AccessTokenProviderAccessor.cs
│   └── ExtAccessTokenProvider.cs
├── Pages/
│   ├── Index.razor[.cs]                # Oven home (dashboard)
│   ├── Login.razor[.cs]               # Login page
│   ├── OvenConfig.razor[.cs]          # Oven configuration
│   ├── OvenConfiguration.razor[.cs]   # Oven configuration alt
│   ├── OvenHome.razor[.cs/.css]       # Oven home
│   ├── OvenReport.razor               # Oven reporting page
│   ├── OvenSettings.razor             # Oven settings
│   ├── Report.razor[.cs]              # Main report page
│   ├── RealtimeTrend.razor[.cs]       # Realtime trend chart
│   ├── RevoHome.razor[.cs/.css]       # REVO home page
│   ├── RevoReport.razor[.cs]          # REVO report (largest — 39KB codebehind)
│   ├── RolesPermissions.razor         # Admin: role/permission management
│   ├── Settings.razor[.cs]            # App settings page
│   ├── UsersManager.razor[.cs]        # Admin: user management
│   └── ...
├── Components/
│   ├── DialogCardPageAddNewUser.razor[.cs]    # Add user dialog
│   ├── DialogCardPageAddStep.razor[.cs]       # Add oven step dialog
│   ├── DialogCardPageEditProfile.razor[.cs]   # Edit profile dialog
│   ├── DialogCardPageOvenDetail.razor[.cs]    # Oven detail dialog
│   ├── DialogCardPageUserInfo.razor[.cs]      # User info dialog
│   ├── DialogRevoConfig.razor[.cs]            # REVO config dialog
│   ├── DialogRevoDetail.razor[.cs/.css]       # REVO detail dialog
│   ├── RedirectToLogin.razor                  # Auth redirect helper
│   ├── RevoCard.razor[.css]                   # REVO machine card widget
│   └── ...
├── Shared/
│   ├── MainLayout.razor[.cs/.css]     # App shell with sidebar
│   ├── NavMenu.razor[.cs/.css]        # Navigation panel (Radzen)
│   ├── LoginLayout.razor[.cs]         # Login-only layout
│   └── SurveyPrompt.razor
├── Model/                             # UI-local view models
├── StaticClass/                       # Static constants/helpers
│   └── StorageConts.cs                # localStorage key constants
├── XLS/                               # Excel export helpers
├── wwwroot/
│   └── (static assets: CSS, images, icons)
└── TemplateReport.xlsx                # Excel report template
```

## GiamSat.Models (Shared Library)

```
GiamSat.Models/
├── GiamSat.Models.csproj
├── Entities/
│   ├── FT01.cs–FT06.cs               # Oven feature table entities
│   ├── FT07_RevoConfig.cs             # REVO config entity
│   ├── FT08_RevoRealtime.cs           # REVO realtime entity
│   ├── FT09_RevoDatalog.cs            # REVO datalog entity
│   ├── FT10_TemperatureConfig.cs      # Temperature entities
│   ├── FT11_TemperatureRealtime.cs
│   ├── FT12_TemperatureDatalog.cs
│   ├── FT13_TemperatureAlarmLog.cs
│   ├── Permissions.cs                 # Permission entity
│   └── RoleToPermission.cs            # Role↔Permission join entity
├── Security/
│   └── PermissionNames.cs             # Permission code constants (Revo.*, Oven.*)
├── Conts/
│   └── ApiRoutes.cs                   # Route constant strings
├── Enums/                             # App enumerations
├── Authorize/                         # Authorization models
├── NotTable/                          # Non-DB models (view models)
├── Result/                            # Result wrapper types
├── Services/                          # Service interfaces
├── LoginModel.cs                      # Login request DTO
├── LoginResult.cs                     # Login response DTO
├── RegisterModel.cs                   # User registration DTO
├── UpdateModel.cs                     # Password update DTO
├── UserModel.cs                       # User info DTO
├── UserRoles.cs                       # Role name constants
├── IdentityAdminDtos.cs               # Admin management DTOs
├── RefreshTokenModel.cs               # Token refresh DTO
├── Response.cs                        # Generic API response
├── BreadCrumbModel.cs                 # UI breadcrumb model
└── JwtHelper.cs                       # JWT decode utilities
```

## Key File Locations

| Purpose | Path |
|---|---|
| DB context | `GiamSat.API/GiamSat.API/DbContext/ApplicationDbContext.cs` |
| Service registration | `GiamSat.API/GiamSat.API/Startup.cs` |
| Permission seeder | `GiamSat.API/GiamSat.API/PermissionSeeder.cs` |
| JWT auth (UI) | `GiamSat.UI/Authorization/JwtAuthenticationService.cs` |
| Nav menu | `GiamSat.UI/Shared/NavMenu.razor` |
| REVO report | `GiamSat.UI/Pages/RevoReport.razor.cs` |
| API client | `GiamSat.APIClient/ApiClient/GiamSatApi.cs` |
| NSwag config | `GiamSat.APIClient/ApiClient/nswag.json` |
| EF Migrations | `GiamSat.API/GiamSat.API/Migrations/` |
| SQL scripts | `sourceCode/DB/` and `GiamSat.API/sql/` |
| PLC tag schemas | `GiamSat.API/ConnectionSchema_*.json` |

## Naming Conventions

- **Projects:** `GiamSat.{Layer}` (e.g., `.API`, `.UI`, `.Models`)
- **Controllers:** `{Entity}Controller.cs`
- **Services:** `S{Entity}.cs` with interface `IS{Entity}`
- **Entities:** `FT{NN}` for feature tables, descriptive names for new entities
- **Pages:** PascalCase `.razor` + optional `.razor.cs` code-behind
- **Components:** `DialogCardPage{Name}.razor` pattern for dialogs
- **Constants:** `ApiRoutes`, `PermissionNames`, `UserRoles`, `StorageConts`
