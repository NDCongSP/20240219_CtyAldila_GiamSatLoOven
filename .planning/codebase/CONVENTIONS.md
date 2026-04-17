# CONVENTIONS.md — Code Style & Patterns

## Language & Style

- **C# version:** Implicit (matches .NET 7 defaults — C# 11)
- **Nullable:** Enabled in `GiamSat.UI.csproj` (`<Nullable>enable</Nullable>`); not explicitly set in API project
- **Implicit usings:** Enabled in UI project
- **File encoding:** UTF-8 with BOM (Windows CRLF line endings throughout)
- **Indentation:** Tabs in API/Models; spaces in UI (mixed)

## Naming Conventions

### Classes & Files

| Pattern | Example |
|---|---|
| Controller | `{Entity}Controller.cs` → `FT07Controller.cs`, `PermissionsController.cs` |
| Service (impl) | `S{Entity}.cs` → `SFT01.cs`, `SFT09.cs` |
| Service (interface) | `IS{Entity}` → `ISFT01`, `ISFT09` |
| Entity | Descriptive or `FT{NN}` → `FT01`, `FT07_RevoConfig`, `Permissions` |
| Page | PascalCase → `OvenHome.razor`, `RevoReport.razor` |
| Dialog component | `DialogCardPage{Name}.razor` → `DialogCardPageAddNewUser.razor` |
| DTO | `{Entity}Dto` or `{Entity}Model` → `IdentityRoleDto`, `LoginModel` |
| Static constants class | `{Domain}Names` / `{Domain}Routes` → `PermissionNames`, `ApiRoutes` |

### Variables & Methods

- PascalCase for public methods, properties, and class-level fields
- camelCase for local variables and private parameters
- Private fields: `_camelCase` with underscore prefix (e.g., `_userManager`, `_dbContext`)
- Async methods suffixed with `Async` (mostly consistent)

### Razor Components/Pages

- Code-behind via partial class `.razor.cs` files
- Component parameters: `[Parameter]` attribute on public properties
- Services injected with `[Inject]` attribute in `.razor` files or constructor injection in `.razor.cs`

## Architecture Patterns

### Controller Pattern (API)

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = UserRoles.Admin)]  // Role-based guard at controller level
public class PermissionsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    // ... other dependencies

    public PermissionsController(ApplicationDbContext dbContext, ...) { ... }

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<IdentityRoleDto>>> GetRoles() { ... }
}
```

### Service Pattern (API)

Interface + implementation registered via `services.AddScoped<ISFTxx, SFTxx>()`:

```csharp
// Interface in GiamSat.Models/Services/
public interface ISFT09 { ... }

// Implementation in GiamSat.API/Services/
public class SFT09 : ISFT09
{
    private readonly ApplicationDbContext _context;
    public SFT09(ApplicationDbContext context) { _context = context; }
}
```

### Blazor Code-Behind Pattern

```csharp
// OvenHome.razor
@page "/"
@inject JwtAuthenticationService AuthService
@inject IFT01Client FT01Client

// OvenHome.razor.cs
public partial class OvenHome
{
    [Inject] private DialogService DialogService { get; set; }
    
    protected override async Task OnInitializedAsync() { ... }
}
```

### Authorization Patterns

**Backend (API)**
- Role-based: `[Authorize(Roles = UserRoles.Admin)]` on controllers/actions
- Permission-based: `[Authorize(Policy = PermissionNames.Revo.View)]` with custom handler
- `PermissionAuthorizationHandler` — checks `"Permission"` claims from JWT

**Frontend (UI)**
- Role check: `<AuthorizeView Roles="Admin">` in Razor
- Permission claim: `<AuthorizeView Policy="Revo.View">`
- Programmatic: `AuthenticationState.User.IsInRole("Admin")`

### HTTP Client Pattern

NSwag-generated typed clients (`IGiamSatApiClient` etc.) accessed via DI:

```csharp
builder.Services.AutoRegisterInterfaces<IApiService>(); // auto-registers all API interfaces

// Usage in component:
[Inject] IAuthenticateClient AuthClient { get; set; }
var result = await AuthClient.LoginAsync(model);
```

## Data Patterns

### JSON Blob Storage

Complex objects are JSON-serialized into `C000`, `C001`, `C002` columns:

```csharp
// Store
entity.C000 = JsonConvert.SerializeObject(myComplexObject);

// Retrieve  
var result = JsonConvert.DeserializeObject<MyComplexType>(entity.C000);
```

This is the dominant pattern for `FT01`–`FT09` tables.

### EF Core Patterns

- DbContext inherits `IdentityDbContext<IdentityUser>` for Identity table integration
- Entity configuration inline in `OnModelCreating()` (not via separate `IEntityTypeConfiguration<T>`)
- EF migrations managed via `dotnet ef migrations add` 
- `EnableSensitiveDataLogging()` enabled (not suitable for production as-is)
- Retry on failure: max 5 retries, 10s max delay (good resilience pattern)

### Error Handling

Inconsistent — no global exception middleware. Current patterns:

```csharp
// API — direct status codes
return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "..." });

// UI — try/catch returning null or error Response
try {
    var res = await _tokenClient.SomeMethodAsync(model);
    return res;
} catch (Exception ex) {
    return new Response() { Status = "Error Exception", Message = ex.Message };
}
```

Notable: API often returns `200 OK` even for errors (with `Status: "Error"` in body). This is a non-standard REST pattern.

### LocalStorage Keys

Defined in `StorageConts.cs`:

```csharp
public static class StorageConts {
    public const string AuthToken = "authToken";
    public const string RefreshToken = "refreshToken";
    public const string Permission = "permissions";
}
```

## UI Component Conventions (Radzen)

- **Grid:** `<RadzenDataGrid>` with `@ref` and manual `Reload()`
- **Dialogs:** `DialogService.OpenAsync<TComponent>(...)` with typed params
- **Notifications:** `NotificationService.Notify(...)`
- **Context menu:** `ContextMenuService.Open(...)`
- **Navigation:** `<RadzenPanelMenu>` in `NavMenu.razor`
- **Loading state:** `Blazorise.LoadingIndicator` or manual `isLoading` bool pattern

## Comments & Documentation

- Code comments predominantly in **Vietnamese** (operational domain)
- XML doc comments sparse — mostly on API response types
- Many large commented-out blocks of old/unused code (seeding data, initialization attempts)
- Vietnamese inline comments document business logic (e.g., `// nếu chạy mới thì bỏ comment`)

## Configuration Access Pattern

```csharp
// API
var value = Configuration["JWT:Secret"];
GlobalVariable.ConString = Configuration.GetConnectionString("ConnStr");

// UI
var value = config["AppSettings:ApiBaseUrl"];
GlobalVariable.RevoRefreshInterval = int.TryParse(config["AppSettings:RevoRefreshInterval"], out int v) ? v : 10000;
```
