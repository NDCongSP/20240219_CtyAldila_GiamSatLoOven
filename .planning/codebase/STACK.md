# STACK.md — Technology Stack

## Language & Runtime

| Layer | Language | Runtime |
|---|---|---|
| Backend API | C# | .NET 7.0 |
| Frontend (SPA) | C# / Razor | Blazor WebAssembly (.NET 7.0) |
| SCADA Client | C# | .NET Framework 4.x (WinForms) |
| Database Scripts | SQL | SQL Server (T-SQL) |

## Backend — GiamSat.API

**Framework:** ASP.NET Core 7 (`Microsoft.NET.Sdk.Web`)  
**Entry Point:** `sourceCode/GiamSat.API/GiamSat.API/Program.cs` → `Startup.cs`

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 7.0.14 | ASP.NET Identity with EF Core |
| `Microsoft.EntityFrameworkCore.SqlServer` | 7.0.14 | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Design` | 7.0.14 | EF Core Migrations tooling |
| `Microsoft.EntityFrameworkCore.Tools` | 7.0.14 | CLI migration support |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 7.0.14 | JWT authentication |
| `Dapper` | 2.1.24 | Micro-ORM for raw SQL reads |
| `QuestPDF` | 2024.3.10 | PDF report generation |
| `Swashbuckle.AspNetCore` | 6.5.0 | Swagger/OpenAPI docs |
| `Swashbuckle.AspNetCore.Annotations` | 6.5.0 | Swagger annotations |
| `Newtonsoft.Json` | 13.0.3 | JSON serialization |
| `Stl.Fusion.Server` | 5.2.87 | Reactive state streaming (partially used) |

### Configuration Files

- `appsettings.json` — DB connection string, JWT settings
- `appsettings.Development.json` — Dev overrides  
- `appsettings.Development.Local.json` — Local dev machine overrides (git-ignored)

Sample JWT config (from `appsettings.json`):
```json
{
  "ConnectionStrings": {
    "ConnStr": "Server=phucthinhautomation.ddns.net,1433;Database=oven;..."
  },
  "JWT": {
    "ValidAudience": "http://localhost:64665",
    "ValidIssuer": "http://localhost:8687",
    "Secret": "JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"
  }
}
```

## Frontend — GiamSat.UI

**Framework:** Blazor WebAssembly (`Microsoft.NET.Sdk.BlazorWebAssembly`, .NET 7.0)  
**Entry Point:** `sourceCode/GiamSat.API/GiamSat.UI/Program.cs`

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Components.WebAssembly` | 7.0.14 | Blazor WASM host |
| `Radzen.Blazor` | `*` (latest) | UI component library (grids, dialogs, charts) |
| `Blazored.LocalStorage` | 4.4.0 | Browser localStorage access |
| `Microsoft.AspNetCore.Components.Authorization` | 7.0.14 | Authorization primitives |
| `Microsoft.AspNetCore.Components.WebAssembly.Authentication` | 7.0.14 | Access token provider |
| `Toolbelt.Blazor.HttpClientInterceptor` | 10.2.0 | HTTP request/response interception |
| `ClosedXML` | 0.102.2 | Excel file reading |
| `ClosedXML.Report` | 0.2.8 | Excel template-based reporting |
| `Blazicons` | 1.2.20 | Icon set |
| `Blazorise.LoadingIndicator` | 1.3.3 | Loading spinners |
| `Microsoft.AspNetCore.Localization` | 2.2.0 | Culture/locale support |
| `Microsoft.Extensions.Http` | 7.0.0 | `IHttpClientFactory` |

### TypeScript / JavaScript

No custom JavaScript/TypeScript. All interop happens via Blazor's built-in JS interop. Static assets in `wwwroot/`.

## Shared Libraries

### GiamSat.Models

Shared class library containing:
- Domain entities (`Entities/`, e.g. `FT01.cs`–`FT09_RevoDatalog.cs`, `Permissions.cs`, `RoleToPermission.cs`)
- Auth/security models (`Security/PermissionNames.cs`, `UserRoles.cs`)
- API DTOs (`LoginModel.cs`, `LoginResult.cs`, `RegisterModel.cs`, `UpdateModel.cs`, `UserModel.cs`, `IdentityAdminDtos.cs`)
- Route constants (`Conts/ApiRoutes.cs`)
- JWT helper (`JwtHelper.cs`)

### GiamSat.APIClient

Auto-generated NSwag client (~600 KB) from `swagger.json`:
- `ApiClient/GiamSat.APIClient.cs` — Typed HTTP client for all API endpoints
- `ApiClient/nswag.json` — NSwag config

## SCADA / WinForms Applications

These are standalone Windows desktop apps that **read PLC data** and push to the API:

| Project | Purpose |
|---|---|
| `GiamSat.Scada` | Original SCADA WinForms (Oven monitoring) |
| `Scada.TemperatureMonitoring` | Temperature sensor monitoring |
| `Scada_TrackingTime_AutoRolling` | REVO rolling time tracker |
| `Scada.TrackingTime_AutoRolling1` | Alt rolling tracker |
| `Scada_TrackingTIme_Revo` | REVO-specific tracker |

These use `Form1.cs` (main form), `Email.cs` (alert emails), `EncodeMD5.cs` (config encryption), OPC/EasyDriver tag files.

## Database

- **Platform:** Microsoft SQL Server (remote: `phucthinhautomation.ddns.net,1433`)
- **Database name:** `oven`
- **ORM:** EF Core 7 (code-first, migrations under `GiamSat.API/Migrations/`)
- **Identity tables:** Standard `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.
- **Custom tables:** `FT01`–`FT13`, `Permissions`, `RoleToPermissions`
- **SQL Scripts:** `sourceCode/DB/oven_script.sql`, `sourceCode/GiamSat.API/sql/`

## Supported Locales

- `en-US` (default), `es-ES`, `de-DE`  
- Decimal separator forced to `.` at startup in Blazor WASM

## Solution File

`sourceCode/GiamSat.API/GiamSat.API.sln` — references all projects in solution.
