# ARCHITECTURE.md — System Architecture

## Overview

**GiamSat (Giám Sát)** is an industrial SCADA monitoring platform for Aldila Vietnam's manufacturing facility. It monitors two primary systems:

1. **OVEN system** — 13 industrial ovens with temperature profile management
2. **REVO system** — 9 REVO rolling machines with realtime/datalog monitoring

The system follows a **3-tier distributed architecture**:

```
[PLC / Hardware]
      ↓
[SCADA WinForms clients] ──→ [GiamSat.API (ASP.NET Core)]
                                        ↓
                              [SQL Server Database]
                                        ↑
                        [GiamSat.UI (Blazor WASM)] ←──────→ [Browser]
```

## Layers

### 1. Hardware / PLC Layer

Physical industrial equipment communicating via OPC/EasyDriver protocols. Tag data is polled by SCADA WinForms applications and written to the API or database.

### 2. SCADA Data Acquisition Layer (WinForms)

Five standalone Windows desktop applications (`.exe`):

| Project | Role |
|---|---|
| `GiamSat.Scada` | Core SCADA — oven monitoring, datalog, alarms |
| `Scada.TemperatureMonitoring` | Temperature sensor data collection |
| `Scada_TrackingTime_AutoRolling` | REVO rolling machine tracking |
| `Scada.TrackingTime_AutoRolling1` | Alternate rolling tracker |
| `Scada_TrackingTIme_Revo` | REVO-specific time tracking |

These apps run 24/7 on operator PCs, polling PLCs and posting data to the API.

### 3. Backend API Layer — GiamSat.API

**ASP.NET Core 7 Web API** — the central hub:
- Hosts Identity + JWT authentication
- Serves CRUD endpoints for all FT (Feature Table) entities
- Manages permissions/roles
- Generates PDF reports
- Seeds default data on startup

### 4. Frontend Layer — GiamSat.UI (+ GiamSat.Interface)

**Blazor WebAssembly** — browser-based monitoring dashboard:
- Communicates exclusively via HTTP to the API using the NSwag-generated client
- JWT stored in `localStorage` via `Blazored.LocalStorage`
- Custom `JwtAuthenticationService` extends `AuthenticationStateProvider`

`GiamSat.Interface` appears to be an earlier/alternate Blazor WASM project with similar structure.

## Data Architecture

### Feature Tables (FT)

Data is organized into **numbered feature tables** — each is a single-row (or limited-row) table storing JSON-serialized complex state:

| Table | Summary |
|---|---|
| `FT01` | Oven configuration (config + ovens JSON) |
| `FT02` | Realtime display state |
| `FT03` | Oven datalog entries |
| `FT04` | Oven step/profile tracking |
| `FT05` | Oven summary data |
| `FT06` | Additional oven data |
| `FT07_RevoConfig` | REVO machine configuration (JSON blob of 9 REVO configs) |
| `FT08_RevoRealtime` | REVO realtime data |
| `FT09_RevoDatalog` | REVO production datalog |
| `FT10_TemperatureConfig` | Temperature system config |
| `FT11_TemperatureRealtime` | Temperature realtime |
| `FT12_TemperatureDatalog` | Temperature datalog |
| `FT13_TemperatureAlarmLog` | Temperature alarm log |

JSON blobs in `C000`, `C001`, etc. columns store complex nested objects (e.g. `OvensInfo`, `ConfigModel`, `RevoConfigs`).

### Identity Tables (ASP.NET Identity)

Standard `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, etc.

### Permission Tables (Custom)

| Table | Purpose |
|---|---|
| `Permissions` | Permission registry (`Id`, `Code`, `Module`, `Action`, `Description`, `IsActive`) |
| `RoleToPermissions` | Many-to-many: `RoleId` (FK → `AspNetRoles`) × `PermissionId` (FK → `Permissions`) |

### SQL Views (EF Queryable)

- `RevoReportStepVm` — REVO step-level reporting view
- `RevoReportShaftVm` — REVO shaft-level rollup view
- `RevoReportHourVm` / `RevoGetTotalShaftCountDto` — hourly aggregations

## Key Flows

### Login Flow

```
Browser → POST /api/Authenticate/login
       ← JWT (containing role + permission claims)
Browser stores JWT in localStorage
All subsequent requests: Authorization: Bearer {token}
```

### Permission Resolution

1. On login, server queries `RoleToPermissions` for the user's roles
2. Permission codes baked into JWT as `"Permission"` claims
3. Admin gets the wildcard `"Permission": "*"` claim  
4. API enforces via `PermissionAuthorizationHandler` (custom `IAuthorizationHandler`)
5. UI enforces via `AuthorizeView` + `RequireClaim`

### Realtime Data Flow

```
SCADA WinForms ──(HTTP POST)──→ API FTxx endpoints → SQL Server
Blazor UI ──(HTTP GET)──→ API FTxx endpoints ←── SQL Server
```

No WebSocket/SignalR — polling-based refresh at configurable intervals (`RevoRefreshInterval`, `RealtimeTrendInterval` in UI `appsettings`).

## Authentication Architecture

```
JwtAuthenticationService (AuthenticationStateProvider + IAccessTokenProvider)
  ├─ GetAuthenticationStateAsync() → parse JWT from localStorage
  ├─ LoginAsync() → POST /api/Authenticate/login → cache + notify
  ├─ LogoutAsync() → clear localStorage + navigate /login
  └─ RequestAccessToken() → return cached token (refresh at <10 min remaining)

JwtAuthenticationHeaderHandler (DelegatingHandler)
  └─ Attaches Bearer token to all outbound HTTP calls

HttpInterceptorManager (Toolbelt.Blazor.HttpClientInterceptor)
  └─ Intercepts 401 responses → redirect to /login
```

## Entry Points

| Layer | Entry Point |
|---|---|
| API | `sourceCode/GiamSat.API/GiamSat.API/Program.cs` → `Startup.cs` |
| UI | `sourceCode/GiamSat.API/GiamSat.UI/Program.cs` |
| SCADA | `GiamSat.Scada/Program.cs` → `Form1.cs` |

## Deployment

- `sourceCode/GiamSat.API/PublishDeploy/` — built artifacts for production deployment
- Docker: `docker-compose.yml` at repository root
- CI: `.github/` workflows present
