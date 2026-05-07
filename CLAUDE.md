# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run the API
dotnet run --project BlogAPI/BlogAPI.Web.csproj

# Run all tests
dotnet test

# Run only unit tests
dotnet test BlogAPI.Application.UnitTests/BlogAPI.Application.UnitTests.csproj

# Run only integration tests
dotnet test BlogAPI.IntegrationTests/BlogAPI.IntegrationTests.csproj

# Run a single test class
dotnet test --filter "ClassName=PostServiceTests"

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project BlogAPI.Infrastructure --startup-project BlogAPI

# Apply migrations
dotnet ef database update --project BlogAPI.Infrastructure --startup-project BlogAPI
```

Integration tests require Docker (Testcontainers spins up a PostgreSQL container automatically).

## Architecture

Clean Architecture with four layers:

- **BlogAPI.Domain** — Entities, domain interfaces (repository/service contracts), and `Error`/`Result` types. No dependencies on other layers.
- **BlogAPI.Application** — Services, DTOs, FluentValidation validators, mappers, and query filter/sorting implementations. Depends only on Domain.
- **BlogAPI.Infrastructure** — EF Core `AppDbContext`, repository implementations, ASP.NET Core Identity (`ApplicationUser`), JWT token service, and DI registration. Uses PostgreSQL (Npgsql).
- **BlogAPI** (Web) — Controllers, middleware, `ResultExtensions`, and DI wiring. Targets net10.0.

### Result pattern

All service methods return `Result` or `Result<T>` (defined in `BlogAPI.Domain/Abstractions/`). Controllers call `result.ToProblemDetails()` (in `BlogAPI.Web/Extensions/ResultExtensions.cs`) to convert failures to RFC 7807 problem details. `Error` types: `NotFound`, `Validation`, `Conflict`, `Forbidden`, `Unauthorized`, `Internal`.

### Query pipeline

List endpoints use a composable pipeline:
1. A `IQueryFilter<T>` implementation filters the `IQueryable<T>`
2. A `IQuerySorting<T>` implementation orders it
3. `QueryExtensions.ApplyFiltering()` / `ApplySorting()` (C# 14 extension members) apply them
4. `IPagedListFactory.CreateAsync()` materializes a `PagedList<T>`

### Identity and auth

`ApplicationUser` (ASP.NET Core Identity) is separate from the domain `UserProfile` entity. They are linked via `UserProfile.ApplicationUserId`. `IUserContext` (implemented by `UserContext`) exposes the current user's `UserId` and `UserProfileId` from JWT claims. `IUserManager` wraps `UserManager<ApplicationUser>` to keep Infrastructure details out of Application.

### Validation

FluentValidation validators live in `BlogAPI.Application/Validators/`. The extension method `ValidationResult.ToValidationFailure<T>()` converts FluentValidation results into `Result<T>.Failure(ValidationErrors.ValidationError, subErrors)`.

### Testing

- **Unit tests** (`BlogAPI.Application.UnitTests`): Use NSubstitute for mocking, MockQueryable.NSubstitute for queryable mocking.
- **Integration tests** (`BlogAPI.IntegrationTests`): Use `WebApplicationFactory<Program>` + Testcontainers PostgreSQL. Tests inherit `BaseIntegrationTest` which seeds data via `TestDataSeeder` and provides `AuthenticateAsync()` to set the Bearer token on the shared `HttpClient`.
