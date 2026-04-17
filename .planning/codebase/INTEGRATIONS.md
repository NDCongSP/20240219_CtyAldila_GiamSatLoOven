# INTEGRATIONS.md — External Services & APIs

## Primary Database — Microsoft SQL Server

- **Host:** `phucthinhautomation.ddns.net:1433` (DDNS-based remote SQL Server)
- **Database:** `oven`
- **Auth:** SQL auth (`dev1` / password)
- **SSL:** `TrustServerCertificate=True`
- **Retry policy:** Configured in `Startup.cs` — max 5 retries, 10s max delay, transient failure handling
- **Connection string location:** `appsettings.json` → `ConnectionStrings:ConnStr`

> ⚠️ The connection string (including password) is stored in plain text in `appsettings.json`. There is a commented-out MD5 decrypt path suggesting encryption was considered but not implemented.

## Authentication — ASP.NET Core Identity + JWT

- **Identity store:** SQL Server via `IdentityDbContext<IdentityUser>`  
- **Token format:** JWT Bearer
- **Token expiry:** 60 minutes (`DateTime.Now.AddMinutes(60)` in `AuthenticateController.cs`)
- **Issuer/Audience:** configured in `appsettings.json` (JWT section)
- **Token refresh:** Minimal — endpoint exists (`/api/Authenticate/refreshtoken`) but client-side logic checks expiry at 5/10 minute threshold and issues a new token from old JWT claims without a true refresh token store
- **Permission claims:** Embedded in JWT as `"Permission"` claim type entries

### Authentication Endpoints (`/api/Authenticate/`)

| Route | Method | Description |
|---|---|---|
| `login` | POST | Issue JWT for valid credentials |
| `refreshtoken` | POST | Re-issue JWT from existing valid JWT |
| `register` | POST | Create user with specified roles |
| `register-admin` | POST | Create admin user |
| `updatePass` | POST | Change password (old + new) |
| `checkuser` | POST | Verify credentials without issuing token |
| `GetAllUsers` | GET | List all users with roles |
| `DeleteUser` | POST | Remove non-admin user |

## RBAC — Permissions System

Custom permission system layered on top of ASP.NET Identity roles:
- **Permissions table** → `sourceCode/GiamSat.API/GiamSat.API/DbContext/ApplicationDbContext.cs`
- **RoleToPermissions table** → maps `AspNetRoles.Id` → `Permissions.Id`
- **Seeded at startup** via `PermissionSeeder.SeedAsync()`
- **Exposed via** `/api/Permissions/` (CRUD for roles, permissions, role–permission links)
- **Baked into JWT** at login time as `Permission` claims
- **Backend policy enforcement:** `PermissionAuthorizationHandler` (singleton) + `PermissionRequirement`
- **Frontend policy enforcement:** `RequireClaim(PermissionNames.Prefix, code)` in `Program.cs`

### Permission Modules

| Module | Actions |
|---|---|
| `Revo` | View, Create, Edit, Delete, Export, Approve |
| `Oven` | View, Create, Edit, Delete, Export, Approve |

## OPC / EasyDriver (PLC Integration)

- **Tool:** EasyDriver (SCADA tag server)
- **Tag files:** `sourceCode/GiamSat.API/easyDriverTagFile/` and `sourceCode/GiamSat.API/ConnectionSchema_*.json`
- **Integration:** WinForms SCADA apps (`GiamSat.Scada`, `Scada_*`) poll EasyDriver/OPC tags and push data to the API or database directly
- **Connection schemas:** JSON files describing PLC channel/device/tag topology for:
  - `ConnectionSchema_Oven.json` — full oven config
  - `ConnectionSchema_revo.json` — REVO device
  - `ConnectionSchema_AutoRolling.json` — auto-rolling process
  - `ConnectionSchema_Oven and Temperaturejson.json` — combined

## Access DB / REVO Data Source (Legacy)

- Some REVO SCADA apps read from an **MS Access `.mdb` file** (Roll3.mdb via JET OLEDB)
- Connection string in `Startup.cs` seeding data:  
  `Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:\...\Roll3.mdb`
- This is a **production hardcoded path** — a known concern

## Report Generation

- **PDF:** `QuestPDF` library → `sourceCode/GiamSat.API/GiamSat.API/Services/PdfExportRevo.cs`
- **Excel:** `ClosedXML` + `ClosedXML.Report` in UI → template-based Excel export
  - Template: `sourceCode/GiamSat.API/GiamSat.UI/TemplateReport.xlsx`

## Email Notifications

- `GiamSat.Scada/Email.cs` — SMTP email alerting from SCADA WinForms app
- Not integrated with the API layer directly; triggered from SCADA on alarm conditions

## NSwag (API Client Generation)

- `sourceCode/GiamSat.API/GiamSat.APIClient/ApiClient/nswag.json` — config
- `GiamSatApi.cs` — ~600KB auto-generated typed client from `swagger.json`
- Regenerated via `nswag run` when API changes

## CORS Configuration

- Policy `"AllowAll"`: `AllowAnyOrigin()` + `AllowAnyMethod()` + `AllowAnyHeader()`
- Applied globally in `Configure()` — **not restricted in production**

## Swagger / OpenAPI

- Enabled in all environments (Development AND Production)
- Available at `/swagger`
- Includes JWT Bearer security definition
