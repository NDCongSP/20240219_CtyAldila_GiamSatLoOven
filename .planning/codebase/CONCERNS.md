# CONCERNS.md — Technical Debt, Issues & Risk Areas

## 🔴 Critical Security Issues

### 1. Hardcoded Credentials in `appsettings.json`

**File:** `GiamSat.API/appsettings.json`  
**Risk:** CRITICAL — exposed in version control

```json
"ConnStr": "Server=phucthinhautomation.ddns.net,1433;Database=oven;User Id=dev1;Password=DaPHA5eY@$AWysDW;..."
"Secret": "JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"
```

The production SQL Server password and JWT signing secret are committed to the repository in plaintext. Commented code suggests MD5 encryption was considered (`EncodeMD5.DecryptString(...)`) but not implemented.

**Remediation:** Use environment variables, Azure Key Vault, or `appsettings.Local.json` (git-ignored) for production secrets.

### 2. CORS Policy: `AllowAnyOrigin`

**File:** `Startup.cs` (line ~301)  
**Risk:** HIGH — any domain can call the API

```csharp
builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
```

No origin restriction in production. Combined with JWT auth this is lower risk but still non-ideal.

### 3. Swagger Enabled in Production

**File:** `Startup.cs` (line ~330)

```csharp
if (env.IsDevelopment() || env.IsProduction())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI(...);
```

Developer exception page AND Swagger are exposed in Production. Swagger UI reveals all API endpoints without requiring authentication.

### 4. Hardcoded MDB File Path

**File:** `Startup.cs` (seeding) — line ~441  
**Risk:** HIGH — production deploy failure on different machines

```csharp
ConstringAccessDb = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:\\MyCompany\\8.SourceCode\\...\\Roll3.mdb;"
```

Hardcoded developer machine path in production seed data.

### 5. `EnableSensitiveDataLogging()` Active

**File:** `ApplicationDbContext.cs` (line ~90)  
SQL parameters (including passwords) may appear in logs.

---

## 🟡 Architecture / Design Concerns

### 6. JSON Blob Anti-Pattern (FT01–FT09)

The primary data storage pattern serializes complex objects into `C000`, `C001`, etc. columns using `JsonConvert.SerializeObject()`. This means:
- No SQL-level querying of individual fields
- Schema changes require code-only changes
- Historical blob data can become incompatible
- No DB-level validation

### 7. Non-RESTful Error Responses

**Files:** Multiple controllers  
Many endpoints return `HTTP 200 OK` with `{ Status: "Error", Message: "..." }` in body. This breaks standard REST conventions and makes error handling on the client fragile.

### 8. No Token Refresh Store

**Files:** `AuthenticateController.cs`, `JwtAuthenticationService.cs`  
The refresh token system is incomplete — tokens are issued as new JWTs from old claims, but there's no server-side refresh token registry. The UI-side refresh logic (at <5/10 min remaining) is partially commented out.

### 9. `_context.RolePermissions` vs `_context.RoleToPermissions`

**File:** `AuthenticateController.cs` line ~328  
Uses `_dbContext.RolePermissions` (wrong name) while DbContext exposes `.RoleToPermissions`. This is likely a compilation error or the DbSet was renamed without updating all usages. **This indicates the code may not build cleanly.**

### 10. Large Commented-Out Code Blocks

`Startup.cs` has ~130 lines of commented-out data initialization code that was used for initial dev and never cleaned up. This creates noise and makes maintenance harder.

---

## 🟡 Performance Concerns

### 11. No Caching Layer

Every request hits SQL Server. High-frequency polling from SCADA clients + browser UI with no response caching or in-memory cache. For realtime displays with 1s refresh intervals, this could cause SQL Server load.

### 12. N+1 Query Pattern in `GetAllUsers`

**File:** `AuthenticateController.cs` lines ~270-299  
```csharp
var u = _userManager.Users.ToList(); // Load all users
foreach (var item in u) {
    var user = await _userManager.FindByNameAsync(item.UserName); // N+1!
    var roles = await _userManager.GetRolesAsync(user); // N+1!
}
```
Separate query per user for role lookup — degrades with user count.

### 13. `GiamSatApi.cs` is 600 KB

The auto-generated NSwag API client is a single 600 KB file. This bloats the Blazor WASM download bundle. NSwag can be configured to generate partial classes or split clients.

---

## 🟡 Code Quality Concerns

### 14. No Automated Tests

Zero test coverage. Details in `TESTING.md`.

### 15. Mixed Code Quality — Old vs New

The codebase mixes:
- Old explicit-namespace `new List<string>()` style
- New `var x = new()` style
- Old-style IAsyncEnumerable usage alongside modern Task<> patterns
- Some files use file-scoped namespaces, others block-scoped

### 16. Debug Console.WriteLine in Production Code

**File:** `JwtAuthenticationService.cs` lines ~305-309

```csharp
Console.WriteLine(kvp.Key);
Console.WriteLine(kvp.Value.ToString() ?? string.Empty);
Console.WriteLine(kvp.Value.GetType());
```

JWT claim key/values are logged to console on every `GetAuthenticationState` call.

### 17. Exception Swallowing

**Files:** Multiple UI service methods  
Pattern of `catch (Exception ex) { return null; }` without logging loses failure context.

### 18. Unused Projects

`BlazorApp1`, `BlazorApp2`, `BlazorApp3` appear to be prototype/scratch Blazor apps in the solution that were never cleaned up.

---

## 🟢 Notable Positives

- **Permission system is well designed** — clean separation of Permissions/RoleToPermissions with seeding
- **EF retry policy** — proper transient fault handling configured
- **SCADA-API decoupling** — data tier is separate from UI tier, SCADA apps post independently
- **NSwag client generation** — API contract is explicit and typed
- **Role-based nav menu** — `<AuthorizeView Roles="Admin">` in NavMenu correctly gates admin items

---

## 🔵 Migration / Evolution Risks

### Recent Migration: ASP.NET Identity Integration

Based on git history and last conversation context, the codebase recently migrated from a custom `SecurityRole` system to standard ASP.NET Identity roles + custom `Permissions`/`RoleToPermissions` tables. The migration (`20260416082702_addEntitiesPermission`) was the most recent. Residual inconsistencies may exist in:
- DB context property naming (`RolePermissions` vs `RoleToPermissions`)
- Frontend components still referencing old role models
- `build_error.txt` being 176 KB suggests build errors from this migration work

### Temperature System Added April 2026

`FT10`–`FT13` tables added in migration `20260414151457_addEntityForTemperature`. Integration with SCADA and frontend may be incomplete.
