# TESTING.md — Test Structure & Practices

## Current Testing State

⚠️ **No automated tests exist in this codebase.**

There are no test projects, no `*.Tests.csproj` files, and no test frameworks referenced in any `.csproj`. The solution relies entirely on:

1. Manual testing via Swagger UI (`/swagger`)
2. Manual browser testing via the Blazor WASM frontend
3. `build_error.txt` (`sourceCode/build_error.txt`, 176 KB) — appears to be accumulated build output/error log used informally for debugging

## Manual Testing Approach

### API Testing

- **Swagger UI:** Available at `https://api-host/swagger` in all environments
- Swagger has JWT Bearer auth configured — devs can authenticate and call endpoints interactively
- `RevoMockController.cs` (13 KB) — provides mock REVO data endpoints for UI testing without live PLC data

### UI Testing

- Manual browser testing of each page
- No E2E framework (no Playwright, Cypress, etc.)

### Build Validation

- `sourceCode/build_error.txt` — 176 KB of accumulated build output suggests frequent build validation cycles
- No CI build gate enforcing zero errors before merge

## Seeding & Integration Helpers

While not tests, these provide test data setup:

- **`Startup.cs` `SeedingData()` method** — seeds default users (`admin`, `operator`, `user`), roles (`Admin`, `User`, `Operator`), and initial REVO configs on every startup
- **`PermissionSeeder.SeedAsync()`** — seeds permission definitions and grants all to Admin on every startup
- **`/api/Permissions/seed` endpoint (POST)** — can re-trigger permission seeding on demand
- **`sourceCode/DB/Insert_FT09_TestData.sql`** — SQL script with REVO datalog test data

## Recommended Testing Strategy (Gap)

If tests are to be added, recommended approach:

### Unit Tests
- Project: `GiamSat.API.Tests` (xUnit or NUnit)
- Targets: `Services/SFT*.cs`, `PermissionSeeder`, `EncodeMD5`, `JwtHelper`
- Mock: `ApplicationDbContext` via EF Core InMemory or `NSubstitute`/`Moq`

### Integration Tests
- `WebApplicationFactory<Program>` for API integration tests
- Test auth flows, permission seeding, RBAC enforcement
- Requires separate test database or SQL Server LocalDB

### UI Component Tests
- `bUnit` for Blazor component testing
- Targets: `JwtAuthenticationService`, dialog components, permission-gated `<AuthorizeView>` blocks

### E2E Tests
- Playwright with .NET bindings
- Cover: login flow, REVO dashboard, report generation, user management

## Notes

- The large `RevoReport.razor.cs` (~39 KB) is a prime candidate for unit testing — complex filtering, aggregation logic
- The JSON blob deserialization pattern (FT01–FT09) should be tested with edge cases (null, malformed JSON)
- The token refresh logic in `JwtAuthenticationService.cs` has commented-out code paths that are untested
