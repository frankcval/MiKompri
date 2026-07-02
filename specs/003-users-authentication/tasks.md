# Tasks: Users Authentication & Groups

**Feature**: `003-users-authentication` | **Branch**: `003-users-authentication`
**Input**: `specs/003-users-authentication/` — spec.md · plan.md · data-model.md · contracts/users-api.md · quickstart.md
**User Stories**: 6 (US1–US6) | **Functional requirements**: FR-001–FR-017 · SR-001–SR-006
**Tests**: Sí — FR-015 y TP8 exigen cobertura de dominio, aplicación e integración

---

## Checklist Format

- **[P]**: puede ejecutarse en paralelo con otras tareas [P] del mismo bloque lógico
- **[USx]**: historia de usuario que cubre la tarea (traceabilidad a spec.md)
- Todos los tasks referencian archivos concretos del repositorio

---

## Phase 1: Configuración inicial e inspección del repositorio

**Propósito**: Eliminar scaffold innecesario, crear proyectos de test, añadir dependencias de autenticación.
**⚠ Gate TP8**: Proyectos de test creados en esta fase antes de codificar lógica.

- [ ] T001 Eliminar `MiKompri.Users.Api/Controllers/WeatherForecastController.cs` y `MiKompri.Users.Api/WeatherForecast.cs` del proyecto (quitar del `.csproj` si están incluidos explícitamente)
- [ ] T002 Crear proyecto xUnit `MiKompri.Users.Domain.Tests` en `test/MiKompri.Users.Domain.Tests/` con `dotnet new xunit`, añadir a `MiKompri.sln` y agregar ProjectReference a `MiKompri.Users.Domain`
- [ ] T003 [P] Crear proyecto xUnit `MiKompri.Users.Application.Tests` en `test/MiKompri.Users.Application.Tests/`, añadir a `MiKompri.sln`, agregar ProjectReferences a `MiKompri.Users.Application` y `MiKompri.Users.Domain`
- [ ] T004 [P] Crear proyecto xUnit `MiKompri.Users.Api.Tests` en `test/MiKompri.Users.Api.Tests/`, añadir a `MiKompri.sln`, agregar ProjectReference a `MiKompri.Users.Api` y paquete `Microsoft.AspNetCore.Mvc.Testing` 8.x
- [ ] T005 Añadir paquete `Microsoft.AspNetCore.Authentication.JwtBearer` 8.x a `MiKompri.Users.Api/MiKompri.Users.Api.csproj`
- [ ] T006 [P] Añadir paquetes `FluentAssertions` 6.x, `NSubstitute` 5.x y `coverlet.collector` a los tres proyectos de test (`*.Domain.Tests`, `*.Application.Tests`, `*.Api.Tests`)

**Checkpoint**: Solución compila con 0 errores tras `dotnet restore MiKompri.sln`.

---

## Phase 2: Modelo de dominio

**Propósito**: Completar agregados de dominio con timestamps, nueva regla de privilegios en Group y SyncClaims en User.
**No tocar**: ningún archivo de `MiKompri.ShoppingList.*`.

- [ ] T007 Modificar `MiKompri.Users.Domain/Abstractions/Entity.cs`: añadir `public DateTime CreatedAt { get; protected set; }` y `public DateTime UpdatedAt { get; protected set; }`; todos los constructores de agregado que hereden deben inicializar ambas propiedades con `DateTime.UtcNow`
- [ ] T008 [P] Modificar `MiKompri.Users.Domain/Users/GroupRole.cs`: establecer `Owner = 1`, añadir `Admin = 2` (nuevo), cambiar `Member` a `Member = 3` (antes era 2 — seguro: EF usa `HasConversion<string>()`, la BD almacena el nombre del enum, no el valor entero)
- [ ] T009 [P] [US1] Modificar `MiKompri.Users.Domain/Users/User.cs`: inicializar `CreatedAt`/`UpdatedAt` en el constructor existente; añadir método `public void SyncClaims(string? displayName, string? email)` que actualiza `DisplayName` si `displayName != null && displayName != DisplayName`, actualiza `Email` si `email != Email`, y marca `UpdatedAt = DateTime.UtcNow` solo cuando hubo al menos un cambio
- [ ] T010 [P] [US5] Modificar `MiKompri.Users.Domain/Users/GroupMembership.cs`: añadir propiedad `public DateTime JoinedAt { get; private set; }`; asignar `JoinedAt = DateTime.UtcNow` en el método de fábrica `Create()`; inicializar `CreatedAt`/`UpdatedAt` heredados de `Entity`
- [ ] T011 [US5] Modificar `MiKompri.Users.Domain/Users/Group.cs`: inicializar `CreatedAt`/`UpdatedAt` en constructor; añadir método `public GroupRole? GetMemberRole(Guid userId)` que retorna el rol del miembro o `null` si no existe; actualizar firma de `RemoveMember` a `public void RemoveMember(Guid targetUserId, GroupRole requestingRole)` aplicando reglas: (a) target con rol `Owner` → throw `InvalidOperationException("No se puede eliminar al propietario del grupo.")`; (b) `requestingRole == Admin && target.Role == Admin` → throw `InvalidOperationException("Un Admin no puede eliminar a otro Admin.")`; actualizar `UpdatedAt` en `AddMember` y `RemoveMember`

> **Orden**: T007 debe completarse antes de T009, T010, T011. T008 debe completarse antes de T011 (GroupRole.Admin disponible).

**Checkpoint**: `dotnet build MiKompri.Users.Domain` con 0 errores.

---

## Phase 3: Infraestructura y EF Core

**Propósito**: Alinear configuraciones EF con el modelo actualizado; exponer la consulta de grupos por usuario para FR-017.
**Nota**: La migración se ejecuta en Phase 7 una vez que `Program.cs` tenga connection string válida.

- [ ] T012 Modificar `MiKompri.Users.Infrastructure/Persistence/Configurations/UserConfiguration.cs`: añadir en `Configure()` las llamadas `.Property(u => u.CreatedAt).IsRequired()` y `.Property(u => u.UpdatedAt).IsRequired()`
- [ ] T013 [P] Modificar `MiKompri.Users.Infrastructure/Persistence/Configurations/GroupConfiguration.cs`: añadir columnas `CreatedAt` y `UpdatedAt` como requeridas en `Configure()`
- [ ] T014 [P] Modificar `MiKompri.Users.Infrastructure/Persistence/Configurations/GroupMembershipConfiguration.cs`: añadir columnas `JoinedAt`, `CreatedAt` y `UpdatedAt` como requeridas en `Configure()`
- [ ] T015 [P] [US4] Añadir método `Task<IReadOnlyCollection<Group>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)` a `MiKompri.Users.Application/Abstractions/IGroupRepository.cs`; implementar en `MiKompri.Users.Infrastructure/Persistence/Repositories/GroupRepository.cs` con `_context.Groups.Include(g => g.Memberships).Where(g => g.Memberships.Any(m => m.UserId == userId)).ToListAsync(ct)` [FR-017]

**Checkpoint**: `dotnet build MiKompri.Users.Infrastructure` con 0 errores.

---

## Phase 4: Configuración de autenticación / JWT

**Propósito**: Declarar las secciones de appsettings necesarias para JwtBearer antes de tocar Program.cs.

- [ ] T016 Modificar `MiKompri.Users.Api/appsettings.json`: añadir secciones:
  ```json
  "Authentication": { "Authority": "", "Audience": "mikompri-users", "IdentityProvider": "entra" },
  "ConnectionStrings": { "UsersPostgreSQL": "" }
  ```
- [ ] T017 [P] Modificar `MiKompri.Users.Api/appsettings.Development.json`: añadir `ConnectionStrings.UsersPostgreSQL` con valor `Host=localhost;Port=5432;Database=MiKompri_Users;Username=postgres;Password=12345`; añadir `Authentication.Authority` y `Authentication.Audience` con valores de desarrollo (puede ser una URL de IdP local o un placeholder para entorno de pruebas)

---

## Phase 5: Commands, Queries y Validadores (capa Application)

**Propósito**: Implementar todos los handlers CQRS nuevos y actualizar el handler existente de AddMember.
**Patrón**: repositorios de Users llaman `SaveChangesAsync()` internamente (sin IUnitOfWork — ver research.md AD-001).

- [ ] T018 Crear `MiKompri.Users.Application/Dtos/UserProfileDto.cs` con propiedades: `Guid Id`, `string DisplayName`, `string? Email`, `string IdentityProvider`, `string ExternalUserId`, `DateTime CreatedAt`, `DateTime UpdatedAt`; modificar `MiKompri.Users.Application/Dtos/GroupMemberDto.cs` añadiendo `string DisplayName` y `DateTime JoinedAt`; modificar `MiKompri.Users.Application/Dtos/GroupDto.cs` añadiendo `string MyRole`
- [ ] T019 [P] [US1] Crear `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommand.cs` (record con `string IdentityProvider`, `string ExternalUserId`, `string? DisplayName`, `string? Email`) y `SyncProfileCommandHandler.cs`: buscar usuario por `(IdentityProvider, ExternalUserId)`; si no existe → `new User(...)` + guardar → retornar `(Guid UserId, bool Created: true)`; si existe → llamar `user.SyncClaims(displayName, email)` → guardar solo si hubo cambio → retornar `(UserId, Created: false)` [FR-002, FR-016]
- [ ] T020 [P] [US3] Crear `MiKompri.Users.Application/Commands/UpdateProfile/UpdateProfileCommand.cs` (record con `string DisplayName`), `UpdateProfileCommandHandler.cs` (carga `User` por `ICurrentUserService.UserId`; llama `user.UpdateProfile(displayName)`; guarda; retorna `UserProfileDto`) y `UpdateProfileCommandValidator.cs` (`NotEmpty`, `MaximumLength(100)` en `DisplayName`) [FR-004]
- [ ] T021 [P] [US5] Crear `MiKompri.Users.Application/Commands/RemoveMemberFromGroup/RemoveMemberFromGroupCommand.cs` (record con `Guid GroupId`, `Guid TargetUserId`), `RemoveMemberFromGroupCommandHandler.cs` (carga grupo; `callerRole = group.GetMemberRole(currentUserId)`; si `callerRole` es null o `Member` → throw `InvalidOperationException`; llama `group.RemoveMember(targetUserId, callerRole.Value)`; guarda) y `RemoveMemberFromGroupCommandValidator.cs` (`NotEmpty` en ambos IDs) [FR-009, FR-010]
- [ ] T022 [P] [US2] Crear `MiKompri.Users.Application/Queries/GetMyProfile/GetMyProfileQuery.cs` (record vacío) y `GetMyProfileQueryHandler.cs` (carga `User` por `ICurrentUserService.UserId`; si null → throw `KeyNotFoundException`; mapea a `UserProfileDto`) [FR-003]
- [ ] T023 [P] [US4] Crear `MiKompri.Users.Application/Queries/GetMyGroups/GetMyGroupsQuery.cs` (record vacío) y `GetMyGroupsQueryHandler.cs` (llama `IGroupRepository.GetByUserIdAsync(currentUserId)`; mapea cada grupo a `GroupDto` con `MyRole = grupo.GetMemberRole(currentUserId).ToString()`) [FR-017]
- [ ] T024 [P] [US6] Crear `MiKompri.Users.Application/Queries/GetGroupMembers/GetGroupMembersQuery.cs` (record con `Guid GroupId`) y `GetGroupMembersQueryHandler.cs` (carga grupo con `Include(Memberships)`; `callerRole = group.GetMemberRole(currentUserId)`; si null → throw `InvalidOperationException`; para cada membresía carga `User` por `UserId`; retorna `IReadOnlyCollection<GroupMemberDto>` con `DisplayName` y `JoinedAt`) [FR-011]
- [ ] T025 [US5] Modificar `MiKompri.Users.Application/Commands/AddMemberToGroup/AddMemberToGroupCommandHandler.cs`: reemplazar lógica de autorización de "solo Owner" por matriz completa: (a) `callerRole` null o `Member` → throw `InvalidOperationException`; (b) `callerRole == Admin && request.Role == GroupRole.Admin` → throw `InvalidOperationException("Solo el Owner puede asignar rol Admin.")`; verificar que usuario a agregar existe vía `IUserRepository.GetByIdAsync` antes de llamar `group.AddMember()` [FR-007]

> T019–T024 son independientes entre sí: pueden ejecutarse en paralelo. T025 depende de T011 (`Group.GetMemberRole` disponible).

**Checkpoint**: `dotnet build MiKompri.Users.Application` con 0 errores.

---

## Phase 6: Controllers / Endpoints de API

**Propósito**: Exponer los handlers CQRS como endpoints HTTP. Todos los controllers llevan `[Authorize]`.
**Contrato de referencia**: `specs/003-users-authentication/contracts/users-api.md`

- [ ] T026 [US1] [US2] [US3] Crear `MiKompri.Users.Api/Controllers/ProfileController.cs` con `[ApiController][Route("api/v1/users")][Authorize]`: `GET /me` → `GetMyProfileQuery` → `200 UserProfileDto`; `PUT /me` → `UpdateProfileCommand` → `200 UserProfileDto`; `POST /me/sync` → `SyncProfileCommand` → `201 UserProfileDto` si `result.Created == true`, `200 UserProfileDto` si `result.Created == false`
- [ ] T027 [P] [US4] [US5] [US6] Crear `MiKompri.Users.Api/Controllers/GroupsController.cs` con `[ApiController][Route("api/v1/groups")][Authorize]`: `POST /` → `CreateGroupCommand` → `201 GroupDto` + header `Location: /api/v1/groups/{id}/members`; `GET /` → `GetMyGroupsQuery` → `200 GroupDto[]`; `GET /{groupId}/members` → `GetGroupMembersQuery` → `200 GroupMemberDto[]`; `POST /{groupId}/members` → `AddMemberToGroupCommand` → `201 GroupMemberDto`; `DELETE /{groupId}/members/{userId}` → `RemoveMemberFromGroupCommand` → `204 No Content`

**Checkpoint**: `dotnet build MiKompri.Users.Api` con 0 errores.

---

## Phase 7: Autorización — Middleware y pipeline completo

**Propósito**: Auto-provisioning, manejo de excepciones y construcción del pipeline definitivo de Program.cs. Cierra el gate TP4 con la migración.

- [ ] T028 Copiar `MiKompri.ShoppingList.Api/Middleware/ExceptionHandlingMiddleware.cs` a `MiKompri.Users.Api/Middleware/ExceptionHandlingMiddleware.cs` cambiando el namespace a `MiKompri.Users.Api.Middleware`; verificar que mapea `ValidationException`→400, `KeyNotFoundException`→404, `InvalidOperationException`→400, incluye `traceId` en el cuerpo de error
- [ ] T029 [P] Copiar o adaptar `RequestLoggingMiddleware` a `MiKompri.Users.Api/Middleware/RequestLoggingMiddleware.cs` (si ShoppingList usa `app.UseSerilogRequestLogging()`, la tarea consiste en confirmar la referencia al paquete Serilog.AspNetCore en `Users.Api.csproj`)
- [ ] T030 [US1] Crear `MiKompri.Users.Api/Middleware/UserProvisioningMiddleware.cs`: si `context.User.Identity?.IsAuthenticated != true` → `await _next(context); return`; leer claim `"sub"` — si vacío o nulo → `context.Response.StatusCode = 401; return` (SR-005: sin mensaje detallado interno); leer claims `"name"` y `"email"` (nullable); enviar `SyncProfileCommand` vía `ISender`; asignar `context.Items["UserId"] = result.UserId`; `await _next(context)` [FR-002, SR-005]
- [ ] T031 [P] Crear `MiKompri.Users.Api/Services/HttpCurrentUserService.cs` implementando `ICurrentUserService`: `UserId` → `_accessor.HttpContext?.Items["UserId"] is Guid id ? id : Guid.Empty`; `IsAuthenticated` → `_accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false`
- [ ] T032 Reconstruir `MiKompri.Users.Api/Program.cs` completo con este orden de registro de servicios: (1) Serilog bootstrap; (2) `AddControllers()`; (3) `AddUsersApplication()`; (4) `AddUsersInfrastructure(configuration)` con connection string `"UsersPostgreSQL"`; (5) `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => { o.Authority = config["Authentication:Authority"]; o.Audience = config["Authentication:Audience"]; o.TokenValidationParameters = new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true }; })`; (6) `AddSwaggerGen()` con `AddSecurityDefinition("Bearer", ...)` y `AddSecurityRequirement(...)`; (7) `AddHttpContextAccessor()`; (8) `AddScoped<ICurrentUserService, HttpCurrentUserService>()`; (9) `AddHealthChecks().AddDbContextCheck<UsersDbContext>()`; pipeline de middleware (orden crítico): `UseSerilogRequestLogging` → `UseGlobalExceptionHandling()` → `UseCors("MiKompriCors")` → `UseAuthentication()` → `UseAuthorization()` → `UseMiddleware<UserProvisioningMiddleware>()` → `MapControllers()` → `MapHealthChecks("/health")`; añadir auto-migrate en Development (`Database.Migrate()`)
- [ ] T033 Ejecutar migración EF (requiere T032 completado con connection string válida): `dotnet ef migrations add InitialUsers --project MiKompri.Users.Infrastructure --startup-project MiKompri.Users.Api --output-dir Persistence/Migrations`; verificar que se generan los archivos `*_InitialUsers.cs` y `UsersDbContextModelSnapshot.cs` en `MiKompri.Users.Infrastructure/Persistence/Migrations/`

> T028 y T029 pueden ejecutarse en paralelo. T030 depende de T028 (patrón de error definido). T031 es independiente. T032 requiere T028–T031 completos más T026–T027 (controllers) más T016–T017 (appsettings). T033 requiere T032.

**Checkpoint**: `dotnet run --project MiKompri.Users.Api` arranca sin excepciones; `GET /health` devuelve 200.

---

## Phase 8: Tests

**Propósito**: Cobertura de dominio, aplicación e integración para FR-015 y TP8. Tests escritos en paralelo con los incrementos de implementación.

### 8.1 Tests de dominio (`MiKompri.Users.Domain.Tests`)

- [ ] T034 [P] [US5] Crear `test/MiKompri.Users.Domain.Tests/GroupTests.cs`: (1) `Constructor_RegistersOwnerMembership`; (2) `AddMember_Duplicate_ThrowsInvalidOperationException`; (3) `RemoveMember_WhenTargetIsOwner_Throws`; (4) `RemoveMember_AdminRemovesMember_Succeeds`; (5) `RemoveMember_AdminRemovesAdmin_Throws`; (6) `RemoveMember_AdminRemovesOwner_Throws`
- [ ] T035 [P] [US1] [US3] Crear `test/MiKompri.Users.Domain.Tests/UserTests.cs`: (1) `Constructor_WithoutName_DisplayNameIsEmpty` (CHK016 provisioning); (2) `UpdateProfile_EmptyName_ThrowsInvalidOperationException` (CHK016 actualización); (3) `SyncClaims_WithDifferentValues_UpdatesAndSetsUpdatedAt`; (4) `SyncClaims_WithSameValues_DoesNotChangeUpdatedAt`

### 8.2 Tests de aplicación (`MiKompri.Users.Application.Tests`)

*Mocks mediante NSubstitute para `IUserRepository`, `IGroupRepository`, `ICurrentUserService`.*

- [ ] T036 [US1] Crear `test/MiKompri.Users.Application.Tests/Commands/SyncProfileCommandHandlerTests.cs`: (1) usuario no existe → crea + `Created=true`; (2) usuario existe + claims distintos → `SyncClaims` invocado + `Created=false`; (3) usuario existe + claims idénticos → repositorio no guarda + `Created=false`
- [ ] T037 [P] [US2] Crear `test/MiKompri.Users.Application.Tests/Queries/GetMyProfileQueryHandlerTests.cs`: (1) usuario existe → devuelve `UserProfileDto`; (2) usuario no existe → lanza `KeyNotFoundException`
- [ ] T038 [P] [US3] Crear `test/MiKompri.Users.Application.Tests/Commands/UpdateProfileCommandHandlerTests.cs`: (1) displayName válido → guarda y devuelve DTO; (2) displayName vacío → `ValidationException`
- [ ] T039 [P] [US4] Crear `test/MiKompri.Users.Application.Tests/Commands/CreateGroupCommandHandlerTests.cs`: (1) no autenticado → throws; (2) nombre válido → devuelve `GroupId`; (3) Owner registrado automáticamente en membresías
- [ ] T040 [P] [US5] Crear `test/MiKompri.Users.Application.Tests/Commands/AddMemberToGroupCommandHandlerTests.cs`: Owner+Admin → ok; Owner+Member → ok; Admin+Member → ok; Admin intenta +Admin → throws; Member intenta → throws; usuario target no existe → throws; duplicado → throws
- [ ] T041 [P] [US5] Crear `test/MiKompri.Users.Application.Tests/Commands/RemoveMemberFromGroupCommandHandlerTests.cs`: Owner elimina Member → ok; Owner elimina Admin → ok; Admin elimina Member → ok; Admin elimina Admin → throws; Admin elimina Owner → throws; último Owner → throws; target no es miembro → `KeyNotFoundException`
- [ ] T042 [P] [US4] Crear `test/MiKompri.Users.Application.Tests/Queries/GetMyGroupsQueryHandlerTests.cs`: (1) caller con grupos → devuelve lista con `MyRole`; (2) caller sin grupos → devuelve colección vacía (sin throws)
- [ ] T043 [P] [US6] Crear `test/MiKompri.Users.Application.Tests/Queries/GetGroupMembersQueryHandlerTests.cs`: (1) caller es miembro → devuelve lista con `DisplayName` y `JoinedAt`; (2) caller no es miembro → throws `InvalidOperationException`

### 8.3 Tests de integración API (`MiKompri.Users.Api.Tests`)

- [ ] T044 Crear `test/MiKompri.Users.Api.Tests/CustomWebApplicationFactory.cs`: reemplazar opciones de `UsersDbContext` por EF InMemory; registrar `TestAuthHandler` como esquema Bearer; exponer método helper `CreateAuthenticatedClient(string sub, string? name = null, string? email = null)` [Patrón: `MiKompri.ShoppingList.Api.Tests/CustomWebApplicationFactory.cs`]; crear `TestAuthHandler.cs` que genera `ClaimsIdentity` con los claims recibidos
- [ ] T045 [P] [US1] [US2] [US3] Crear `test/MiKompri.Users.Api.Tests/ProfileApiTests.cs`: (1) `GET /api/v1/users/me` sin auth → 401; (2) `GET /api/v1/users/me` con auth → 200 + perfil auto-provisionado; (3) segundo `GET` mismo token → 200, mismo `id`, sin duplicar; (4) `PUT /api/v1/users/me` con displayName válido → 200; (5) `PUT` con displayName vacío → 400 con `errors[0].field = "DisplayName"`; (6) `POST /api/v1/users/me/sync` primera llamada → 201; (7) segunda llamada mismo token → 200
- [ ] T046 [P] [US4] [US5] [US6] Crear `test/MiKompri.Users.Api.Tests/GroupsApiTests.cs`: (1) `POST /api/v1/groups` nombre válido → 201 + `Location` header; (2) `POST` nombre vacío → 400; (3) `GET /api/v1/groups` con grupos → 200 con `myRole`; (4) `GET /api/v1/groups` sin grupos → 200 array vacío; (5) `GET /{id}/members` como Owner → 200 con miembros; (6) `GET /{id}/members` sin membresía → 403; (7) Owner agrega Member → 201; (8) Admin intenta agregar Admin → 403; (9) Admin elimina Admin → 403; (10) duplicado → 400; (11) eliminar único Owner → 400

> T044 debe completarse antes de T045 y T046.

**Checkpoint**: `dotnet test MiKompri.sln --configuration Release` → 0 fallos.

---

## Phase 9: Docker y desarrollo local

**Propósito**: Cumplir gate TP4 (Dockerfile obligatorio). Configurar Docker Compose para arranque local completo.

- [ ] T047 Crear `MiKompri.Users.Api/Dockerfile` multi-stage — referencia: `MiKompri.ShoppingList.Api/Dockerfile`: stage `base` = `mcr.microsoft.com/dotnet/aspnet:8.0`; stage `build` = `mcr.microsoft.com/dotnet/sdk:8.0` → copiar archivos `.csproj` de `MiKompri.Users.*`, `dotnet restore`, copiar fuentes, `dotnet build -c Release`; stage `publish` = `dotnet publish -c Release -o /app/publish`; stage `final` = copiar desde publish, `EXPOSE 8080`, `ENTRYPOINT ["dotnet", "MiKompri.Users.Api.dll"]` **[Gate TP4]**
- [ ] T048 Modificar `docker-compose.yml`: añadir servicio `mikompriusersapi` con `build.context: .`, `build.dockerfile: MiKompri.Users.Api/Dockerfile`, `image: mikompri.users.api`, `depends_on: postgres`, `ports: "8082:8080"`, `healthcheck: test: ["CMD", "curl", "-f", "http://localhost:8080/health"], interval: 30s, timeout: 10s, retries: 3, start_period: 40s`
- [ ] T049 [P] Modificar `docker-compose.override.yml`: (a) corregir typo `PostgreSQ L=` → `PostgreSQL=` en la entrada existente del servicio ShoppingList; (b) añadir sección `mikompriusersapi` con env vars: `ASPNETCORE_ENVIRONMENT=Development`, `ASPNETCORE_HTTP_PORTS=8080`, `ConnectionStrings__UsersPostgreSQL=Host=postgres;Port=5432;Database=MiKompri_Users;Username=postgres;Password=12345`, `Authentication__Authority=<idp-dev-url>`, `Authentication__Audience=mikompri-users`, `Authentication__IdentityProvider=entra`
- [ ] T050 [P] Crear `docker/init-users-db.sql` con script idempotente: `SELECT 'CREATE DATABASE "MiKompri_Users"' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'MiKompri_Users')\gexec`; añadir mount en el servicio `postgres` de `docker-compose.yml`: `volumes: - ./docker/init-users-db.sql:/docker-entrypoint-initdb.d/02-init-users-db.sql`

**Checkpoint**: `docker compose up --build -d` → `mikompriusersapi` en estado `healthy`.

---

## Phase 10: Documentación y verificación

**Propósito**: Confirmar que el build, los tests y el entorno local cumplen todos los criterios de aceptación del quickstart.

- [ ] T051 Ejecutar `dotnet build MiKompri.sln --configuration Release --no-restore` → 0 errores de compilación
- [ ] T052 [P] Ejecutar `dotnet test test/MiKompri.Users.Domain.Tests/MiKompri.Users.Domain.Tests.csproj --configuration Release` → todos los tests pasan; verificar que `UserTests` cubre los escenarios CHK016 (provisioning vs actualización)
- [ ] T053 [P] Ejecutar `dotnet test test/MiKompri.Users.Application.Tests/MiKompri.Users.Application.Tests.csproj --configuration Release` → todos los tests pasan
- [ ] T054 Ejecutar `dotnet test test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj --configuration Release` → todos los tests pasan (`ProfileApiTests` + `GroupsApiTests`)
- [ ] T055 Verificar entorno Docker: `docker compose up --build -d`; `Invoke-RestMethod http://localhost:8082/health` → `{"status":"Healthy"}`; abrir `http://localhost:8082/swagger` → UI de Swagger con definición Bearer visible
- [ ] T056 Ejecutar escenarios 1–8 de `specs/003-users-authentication/quickstart.md` con entorno Docker levantado; anotar en quickstart.md cualquier desvío respecto a los resultados esperados

---

## Dependency Graph

```
Phase 1: Setup (T001–T006)
  ├─► Phase 2: Domain (T007–T011)
  │     ├─► Phase 3: Infrastructure (T012–T015)
  │     └─► Phase 5: Application (T018–T025)
  │             └── T023 GetMyGroups depende también de T015 (IGroupRepository.GetByUserIdAsync)
  ├─► Phase 4: Auth config (T016–T017)
  └─► Phase 6: Controllers (T026–T027) ← depende de Phase 5 completa
		└─► Phase 7: Middleware + Program.cs (T028–T033)
			  └── T032 Program.cs depende de Phase 4 + Phase 5 + Phase 6 + T028–T031
			  └── T033 Migración depende de T032
			  └─► Phase 9: Docker (T047–T050)
			  └─► Phase 10: Verification (T051–T056)
Phase 8: Tests:
  ├── T034–T035 Domain tests → Phase 2 completa
  ├── T036–T043 Application tests → Phase 5 completa
  └── T044–T046 Integration tests → Phase 6 + Phase 7 completas
```

---

## Parallel Execution Examples

**Bloque A** — después de T001 (eliminación scaffold):
```
T002 Domain.Tests  ║  T003 Application.Tests  ║  T004 Api.Tests  ║  T005 JwtBearer pkg
```

**Bloque B** — después de T007 (Entity base):
```
T008 GroupRole  ║  T009 User.SyncClaims  ║  T010 GroupMembership
```
*(T011 Group.RemoveMember espera T008 GroupRole.Admin)*

**Bloque C** — después de Phase 2 completa:
```
T012 UserConfig  ║  T013 GroupConfig  ║  T014 GroupMembershipConfig  ║  T015 IGroupRepository.GetByUserIdAsync
```

**Bloque D** — después de Phase 3 + Phase 4:
```
T019 SyncProfile  ║  T020 UpdateProfile  ║  T021 RemoveMember  ║  T022 GetMyProfile  ║  T023 GetMyGroups  ║  T024 GetGroupMembers
```
*(T025 AddMember update espera T011)*

**Bloque E** — después de Phase 5:
```
T026 ProfileController  ║  T027 GroupsController
```

**Bloque F** — inicio de Phase 7:
```
T028 ExceptionMiddleware  ║  T029 RequestLoggingMiddleware  ║  T031 HttpCurrentUserService
```
*(T030 UserProvisioningMiddleware y T032 Program.cs esperan T028–T031 completos)*

**Bloque G** — tests en paralelo dentro de Phase 8:
```
T034 GroupTests    ║  T035 UserTests
T036 SyncProfile   ║  T037 GetMyProfile  ║  T038 UpdateProfile
T039 CreateGroup   ║  T040 AddMember     ║  T041 RemoveMember
T042 GetMyGroups   ║  T043 GetGroupMembers
T045 ProfileApi    ║  T046 GroupsApi
```

---

## Implementation Strategy

| Incremento | Tareas | US verificables |
|---|---|---|
| **MVP** | T001–T011, T016–T017, T018–T019, T022, T028–T032, T026 (solo GET+POST /me y /me/sync) | US1 + US2 |
| **Incremento 2** | T020, T015, T023, T027 (POST + GET /groups), T039, T042 | US3 + US4 |
| **Incremento 3** | T021, T024, T025, T027 (resto de /groups/{id}/members), T040–T041, T043 | US5 + US6 |
| **Docker** | T033, T047–T050 | Entorno local completo |
| **Verificación** | T051–T056 | Todos |

---

## Summary

- **Total de tareas**: 56 (T001–T056)
- **Tareas paralelas [P]**: 27
- **Distribución por fase**: Setup 6 · Dominio 5 · Infraestructura 4 · Auth 2 · Application 8 · Controllers 2 · Middleware 6 · Tests 13 · Docker 4 · Verificación 6
- **MVP scope**: T001–T011 + T016–T019 + T022 + T026 (parcial) + T028–T032 → US1 + US2 funcionando sin Docker
- **Gates de constitución**: TP4 cubierto por T047 (Dockerfile); TP8 cubierto por T002–T004 + T034–T046
- **Sin modificaciones a ShoppingList**: ninguna tarea toca archivos de `MiKompri.ShoppingList.*` excepto T049 (corrección de typo en docker-compose.override.yml en la entrada existente de ShoppingList — mínimo indispensable)
- **Format validation**: todos los tasks siguen el formato `- [ ] TXXX [P?] [USx?] Descripción con ruta de archivo`
