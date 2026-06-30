# MiKompri Copilot Instructions

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.

## Build and test commands

This repository is a .NET 8 solution. The commands below match the solution layout and the CI workflow in `.github\workflows\ci-mikompri-shoppinglist.yml`.

```powershell
dotnet restore MiKompri.sln
dotnet build MiKompri.sln --configuration Release --no-restore
dotnet test MiKompri.sln --configuration Release
```

ShoppingList test projects are the only test projects currently present:

```powershell
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj --configuration Release
```

Run a single test with `--filter`. Useful examples:

```powershell
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj --filter "FullyQualifiedName~PurchaseListTests.Rename_ActualizaNombreYUpdatedAt"
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj --filter "FullyQualifiedName~CreateShoppingListCommandHandlerTests.Handle_Should_Add_List_And_SaveChanges_Returns_ListId"
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj --filter "FullyQualifiedName~PurchaseListsApiTests.Create_Then_GetById_Should_Return_Created_List"
```

Coverage commands used by CI:

```powershell
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=.\TestResults\coverage.opencover.xml
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=.\TestResults\coverage.opencover.xml
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=.\TestResults\coverage.opencover.xml
```

There is no repository-defined lint command. Code quality checks currently run through the normal build/test flow plus SonarCloud in CI.

## High-level architecture

MiKompri is organized by bounded context, with each context split into `Api`, `Application`, `Domain`, and `Infrastructure` projects instead of keeping all APIs or all domains together.

- `MiKompri.ShoppingList.*` is the only fully wired vertical slice today. Requests enter `MiKompri.ShoppingList.Api\Controllers\PurchaseListsController.cs`, are translated into MediatR commands or queries, pass through logging and validation pipeline behaviors from `MiKompri.ShoppingList.Application\Behavior\`, execute handlers in `Commands\` or `Queries\`, then persist through EF Core repositories in `MiKompri.ShoppingList.Infrastructure\Persistence\`.
- `MiKompri.Users.*` follows the same layered split in domain, application, and infrastructure, but the API project is still mostly scaffolded (`WeatherForecastController` is still present). Treat ShoppingList as the reference implementation when extending Users.
- ShoppingList API startup is centralized in `MiKompri.ShoppingList.Api\Program.cs`: Serilog replaces default logging, CORS is open, health checks are mapped through `HealthChecksExtensions`, and global exception handling is added through middleware.
- The ShoppingList persistence model uses PostgreSQL in production (`AddInfrastructure` registers `ShoppingListDbContext` with Npgsql) and swaps to EF Core InMemory only inside integration tests via `CustomWebApplicationFactory`.

## Key conventions

- Keep controllers thin. Existing controllers only map HTTP request models to MediatR requests and return HTTP responses; business rules live below the API layer.
- CQRS types are grouped by feature folder. Each ShoppingList command or query usually has its own folder containing the request record, handler, and validator. Follow that layout rather than creating shared handler folders.
- Commands and queries are usually `record` types implementing `IRequest` or `IRequest<T>`. Search by the exact existing names before renaming anything; there are intentional-or-existing spellings such as `AddAplicaction`, `DeleteShoppinList`, and `UpdateItemShoopingListCommand`.
- ShoppingList domain rules are enforced inside the aggregate root. `PurchaseList` owns its `_items` collection, prevents duplicate `ProductId` values, updates timestamps, and exposes mutation methods like `AddItem`, `UpdateItem`, `DeleteItem`, and `MarkItemAsPurchased`. Repository code loads the aggregate with `Include(...Items)` before mutations so the domain logic runs on a complete aggregate.
- Persistence is not consistent across bounded contexts yet. In ShoppingList, repositories do not save immediately; handlers call `IUnitOfWork.SaveChangesAsync()` after repository work. In Users, repositories currently call `SaveChangesAsync()` directly. Preserve the pattern already used in the project you are editing instead of mixing them.
- Integration tests for ShoppingList use `WebApplicationFactory` and replace the production `DbContextOptions<ShoppingListDbContext>` registration with an InMemory database. If API changes need integration coverage, extend that test harness instead of introducing a separate startup path.
- API error handling for ShoppingList is standardized in middleware, not in controllers. `ExceptionHandlingMiddleware` maps `ValidationException` to `400`, `KeyNotFoundException` to `404`, and `InvalidOperationException` to `400`, and includes a trace/correlation id in the JSON payload.
