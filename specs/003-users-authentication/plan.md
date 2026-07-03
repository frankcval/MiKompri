# Implementation Plan: Users Authentication & Groups

**Branch**: `003-users-authentication` | **Date**: 2026-07-12 | **Spec**: [spec.md](./spec.md)

---

## Summary

Completar el bounded context `Users` formalizando el dominio parcialmente scaffolded, añadiendo los endpoints de API, la configuración de JWT Bearer y la infraestructura de Docker. La API de Users valida tokens JWT Bearer emitidos por un proveedor OIDC externo, auto-provisiona perfiles locales en el primer request autenticado y expone endpoints de gestión de grupos con una matriz de privilegios Owner/Admin/Member. No emite tokens propios.

---

## Technical Context

| Campo | Valor |
|-------|-------|
| **Language/Version** | C# 12 / .NET 8 |
| **Primary Dependencies** | ASP.NET Core Web API, MediatR 12, FluentValidation, Serilog, `Microsoft.AspNetCore.Authentication.JwtBearer`, EF Core 8 (Npgsql) |
| **Storage** | PostgreSQL 15 — instancia compartida docker; base de datos `MiKompri_Users` separada de `MiKompri_ShoppingList` |
| **Testing** | xUnit, FluentAssertions, `WebApplicationFactory` + EF InMemory (patrón de `ShoppingList.Api.Tests`) |
| **Target Platform** | Linux container (Docker Compose) |
| **Project Type** | Web API dentro de monorepo por bounded contexts |
| **Performance Goals** | < 200 ms P95 para operaciones de perfil y membresía |
| **Constraints** | ShoppingList no se toca. Repos de Users salvan directamente (sin `IUnitOfWork`). No emitir tokens. Sin paginación en esta iteración. |
| **Scale/Scope** | Sin límite de miembros por grupo; sin paginación |

---

## Constitution Check

*Primera evaluación — antes de iniciar implementación.*

| Principio | Estado | Acción |
|-----------|--------|--------|
| **PP1** Valor de usuario primero — 6 user stories con criterios de aceptación concretos | ✅ | — |
| **PP2** MVP autónomo — deployable independientemente de ShoppingList | ✅ | — |
| **PP3** ShoppingList como núcleo — Users no modifica ShoppingList; `GroupId` prepara integración futura | ✅ | — |
| **TP1** Backend .NET 8 — todos los proyectos apuntan a `net8.0` | ✅ | — |
| **TP2** Bounded contexts — `MiKompri.Users.*` separado de `ShoppingList.*`; sin referencias cruzadas entre dominios | ✅ | — |
| **TP3** Monorepo — todo permanece en `frankcval/MiKompri` | ✅ | — |
| **TP4** Docker obligatorio — **FALTA Dockerfile para Users.Api** | ❌ | Crear `MiKompri.Users.Api/Dockerfile` — gate bloqueante |
| **TP5** Azure objetivo — no aplica esta iteración (diferido explícitamente en spec) | ⏩ | — |
| **TP7** REST + OpenAPI — Swagger con Bearer security definition | ✅ | — |
| **TP8** Testing — crear 3 proyectos de test: Domain, Application, Api | ⚠ | Crear proyectos antes de codificar lógica |
| **TP9** ADR — decisiones de auth boundary y middleware documentadas en research.md | ✅ | — |
| **TP10** Spec-first — spec.md + plan.md antes de código | ✅ | — |

**Gates bloqueantes:**

1. ❌ **TP4** — Crear Dockerfile antes de cualquier prueba de Docker.
2. ⚠ **TP8** — Crear proyectos de test en paralelo con la implementación.

---

## Project Structure

### Documentación

```text
specs/003-users-authentication/
├── plan.md             ← este archivo
├── spec.md
├── research.md         ← Phase 0 output
├── data-model.md       ← Phase 1 output
├── quickstart.md       ← Phase 1 output
├── contracts/
│   └── users-api.md    ← Phase 1 output
├── checklists/
│   └── requirements.md
└── tasks.md            ← pendiente (/speckit.tasks)
```

### Código fuente afectado

```text
MiKompri.Users.Domain/
├── Abstractions/
│   └── Entity.cs                          ← MODIFICAR: añadir CreatedAt, UpdatedAt
└── Users/
	├── GroupRole.cs                        ← MODIFICAR: añadir Admin = 2
	├── GroupMembership.cs                  ← MODIFICAR: añadir JoinedAt; timestamps heredados
	├── Group.cs                            ← MODIFICAR: RemoveMember con privilegios; GetMemberRole; UpdatedAt
	└── User.cs                             ← MODIFICAR: SyncClaims(); timestamps heredados

MiKompri.Users.Application/
├── Abstractions/
│   └── ICurrentUserService.cs             (existente — sin cambios)
├── Behavior/                              (existente — sin cambios)
├── Commands/
│   ├── AddMemberToGroup/                  ← MODIFICAR: handler — actualizar lógica Admin
│   ├── CreateGroup/                       (existente — sin cambios)
│   ├── RemoveMemberFromGroup/             ← CREAR: command + handler + validator
│   ├── SyncProfile/                       ← CREAR: command + handler (middleware + POST /me/sync; devuelve bool creado)
│   └── UpdateProfile/                     ← CREAR: command + handler + validator
├── Queries/
│   ├── GetMyGroups/                       ← CREAR: query + handler (FR-017: lista grupos del caller)
│   ├── GetMyProfile/                      ← CREAR: query + handler
│   └── GetGroupMembers/                   ← CREAR: query + handler
└── Dtos/
	├── GroupDto.cs                        ← MODIFICAR: añadir MyRole
	├── GroupMemberDto.cs                  ← MODIFICAR: añadir DisplayName, JoinedAt
	└── UserProfileDto.cs                  ← CREAR

MiKompri.Users.Infrastructure/
├── Persistence/
│   ├── UsersDbContext.cs                  (existente — sin cambios)
│   ├── Configurations/
│   │   ├── UserConfiguration.cs          ← MODIFICAR: añadir timestamps
│   │   ├── GroupConfiguration.cs         ← MODIFICAR: añadir timestamps
│   │   └── GroupMembershipConfiguration.cs ← MODIFICAR: añadir JoinedAt + timestamps
│   ├── Repositories/                     (existente — sin cambios)
│   └── Migrations/                       ← CREAR via dotnet ef migrations add

MiKompri.Users.Api/
├── Controllers/
│   ├── WeatherForecastController.cs      ← ELIMINAR
│   ├── ProfileController.cs             ← CREAR
│   └── GroupsController.cs              ← CREAR
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs    ← CREAR (copiar de ShoppingList.Api)
│   ├── RequestLoggingMiddleware.cs       ← CREAR (copiar de ShoppingList.Api)
│   └── UserProvisioningMiddleware.cs     ← CREAR (auto-provisioning)
├── Services/
│   └── HttpCurrentUserService.cs        ← CREAR
├── Program.cs                           ← REEMPLAZAR completamente
├── appsettings.json                     ← MODIFICAR: añadir Authentication section
├── appsettings.Development.json         ← MODIFICAR: connection string + auth dev
├── Dockerfile                           ← CREAR (gate TP4)
└── WeatherForecast.cs                   ← ELIMINAR

test/
├── MiKompri.Users.Domain.Tests/         ← CREAR proyecto xUnit
├── MiKompri.Users.Application.Tests/    ← CREAR proyecto xUnit
└── MiKompri.Users.Api.Tests/            ← CREAR proyecto xUnit

docker-compose.yml                       ← MODIFICAR: añadir mikompriusersapi
docker-compose.override.yml              ← MODIFICAR: añadir env vars Users + corregir typo
```

---

## Phase 0 — Archivos a inspeccionar antes de codificar

> Completado durante la planificación. Ver [research.md](./research.md) para hallazgos detallados y las 6 decisiones de arquitectura ratificadas.

| Archivo clave | Hallazgo relevante |
|---------------|-------------------|
| `Users.Domain/Abstractions/Entity.cs` | Sin timestamps — base de todos los cambios de dominio |
| `Users.Domain/Users/GroupRole.cs` | Falta `Admin` — primer cambio a aplicar |
| `Users.Domain/Users/Group.cs` | `RemoveMember` solo guarda Owner; `AddMember` sin restricción de rol |
| `Users.Infrastructure/.../GroupRepository.cs` | `SaveChangesAsync()` dentro del repo — preservar patrón en Users |
| `ShoppingList.Api/Middleware/ExceptionHandlingMiddleware.cs` | Referencia para error shape y middleware pipeline |
| `docker-compose.yml` + `override.yml` | Solo tiene ShoppingList; typo en connection string de override |

---

## Phase 1 — Dominio

### 1.1 Entity base — añadir timestamps

- **Archivo**: `MiKompri.Users.Domain/Abstractions/Entity.cs`
- **Cambio**: Añadir `public DateTime CreatedAt { get; protected set; }` y `UpdatedAt`.
- **Impacto**: Todos los agregados heredan; sus constructores asignan `DateTime.UtcNow`.

### 1.2 GroupRole — añadir Admin

- **Archivo**: `MiKompri.Users.Domain/Users/GroupRole.cs`
- **Cambio**: `Owner = 1`, `Admin = 2` (nuevo), `Member = 3` (antes era 2).
- **Seguridad**: `GroupMembershipConfiguration` usa `HasConversion<string>()` — BD almacena strings. Sin migración previa. Cambio seguro.

### 1.3 GroupMembership — añadir JoinedAt

- **Archivo**: `MiKompri.Users.Domain/Users/GroupMembership.cs`
- **Cambio**: `JoinedAt = DateTime.UtcNow` en `Create()`. Heredar `CreatedAt`/`UpdatedAt` de Entity.

### 1.4 Group — actualizar lógica de privilegios en RemoveMember

- **Archivo**: `MiKompri.Users.Domain/Users/Group.cs`
- **Cambios**:
  - `RemoveMember(Guid targetUserId, GroupRole requestingRole)` — nueva firma. El dominio aplica: Owner no puede ser eliminado, Admin no puede eliminar Admin (Q2 spec).
  - `GetMemberRole(Guid userId) → GroupRole?` — helper para handlers.
  - `UpdatedAt` actualizado en `AddMember` y `RemoveMember`.
- **Detalle**: Ver [data-model.md](./data-model.md) sección Group.

> **Principio**: La invariante de qué roles pueden gestionar a qué otros roles es regla de dominio. La identidad del caller y su pertenencia al grupo son concern de Application.

### 1.5 User — añadir SyncClaims

- **Archivo**: `MiKompri.Users.Domain/Users/User.cs`
- **Cambio**: `SyncClaims(string? displayName, string? email)` — actualiza solo si los valores difieren y marca `UpdatedAt`. Usado por FR-016 (refresco explícito desde token).

---

## Phase 2 — Aplicación

### 2.1 Nuevos Commands y Queries

| Artefacto | Tipo | Validator | Descripción |
|-----------|------|-----------|-------------|
| `SyncProfileCommand` | Command | — | Middleware AND `POST /me/sync`. Crea el perfil si no existe (devuelve `created = true`) o actualiza claims si ya existía (`created = false`). El controller usa `created` para retornar `201 Created` o `200 OK` según corresponda. |
| `UpdateProfileCommand` | Command | ✅ DisplayName required, max 100 | `PUT /users/me` → llama `user.UpdateProfile()`. |
| `RemoveMemberFromGroupCommand` | Command | ✅ GroupId, TargetUserId required | Verifica rol caller; llama `group.RemoveMember(target, callerRole)`. |
| `GetMyProfileQuery` | Query | — | Devuelve `UserProfileDto`. |
| `GetMyGroupsQuery` | Query | — | FR-017: devuelve los grupos donde el caller tiene membresía activa (`IReadOnlyCollection<GroupDto>` con `MyRole`). |
| `GetGroupMembersQuery` | Query | — | Verifica que caller es miembro; devuelve `IReadOnlyCollection<GroupMemberDto>`. |

### 2.2 Actualizar AddMemberToGroupCommandHandler

**Cambio**: Ampliar de "solo Owner puede agregar" a la matriz completa (Q2 del clarification session).

```
1. Cargar grupo con membresías (IGroupRepository.GetByIdAsync ya hace Include)
2. callerRole = group.GetMemberRole(currentUserId)
3. Si callerRole es null o Member → InvalidOperationException (403)
4. Si callerRole es Admin y request.Role == Admin → InvalidOperationException (403)
5. Verificar que el usuario a agregar existe (IUserRepository.GetByIdAsync)
6. group.AddMember(request.MemberUserId, request.Role)
7. await _groupRepository.UpdateAsync(group, cancellationToken)
```

### 2.3 Nuevos DTOs

| DTO | Campos |
|-----|--------|
| `UserProfileDto` (CREAR) | `Id`, `DisplayName`, `Email`, `IdentityProvider`, `ExternalUserId`, `CreatedAt`, `UpdatedAt` |
| `GroupMemberDto` (MODIFICAR) | Añadir `DisplayName`, `JoinedAt` |
| `GroupDto` (MODIFICAR) | Añadir `MyRole` (rol del caller) |

### 2.4 Flujo de resolución de ICurrentUserService

```
Request HTTP autenticado
  → UseAuthentication()             — ASP.NET Core valida JWT Bearer
  → UserProvisioningMiddleware
	  1. Lee claim "sub" → externalUserId
	  2. Envía SyncProfileCommand → crea/encuentra User → devuelve Guid
	  3. context.Items["UserId"] = guid
  → Controller / Handler
	  ICurrentUserService.UserId lee context.Items["UserId"]
```

---

## Phase 3 — Infraestructura

### 3.1 Configuraciones EF — añadir columnas

| Archivo | Columnas a añadir |
|---------|------------------|
| `UserConfiguration.cs` | `CreatedAt` (required), `UpdatedAt` (required) |
| `GroupConfiguration.cs` | `CreatedAt` (required), `UpdatedAt` (required) |
| `GroupMembershipConfiguration.cs` | `JoinedAt` (required), `CreatedAt` (required), `UpdatedAt` (required) |

Índices existentes no se modifican.

### 3.2 Migración inicial

```powershell
dotnet ef migrations add InitialUsers `
  --project MiKompri.Users.Infrastructure `
  --startup-project MiKompri.Users.Api `
  --output-dir Persistence/Migrations
```

Ejecutar DESPUÉS de que `Program.cs` tenga `UsersDbContext` registrado con connection string válida.

### 3.3 Auto-migración en arranque (solo Development)

```csharp
if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.Migrate();
}
```

---

## Phase 4 — API

### 4.1 Program.cs — reconstrucción completa

Referencia: `MiKompri.ShoppingList.Api/Program.cs`.

Pipeline de registro de servicios:

```
1.  Serilog bootstrap + host.UseSerilog(...)
2.  CORS abierta — política "MiKompriCors"
3.  AddControllers()
4.  AddUsersApplication()
5.  AddUsersInfrastructure(configuration)
6.  AddAuthentication(JwtBearerDefaults).AddJwtBearer(options =>
		options.Authority  = config["Authentication:Authority"]
		options.Audience   = config["Authentication:Audience"]
		ValidateIssuer, ValidateAudience, ValidateLifetime = true)
7.  AddSwaggerGen() con Bearer security definition
8.  AddHttpContextAccessor()
9.  AddScoped<ICurrentUserService, HttpCurrentUserService>()
10. AddHealthChecks().AddDbContextCheck<UsersDbContext>()
```

Pipeline de middleware (orden crítico):

```
RequestLoggingMiddleware
UseGlobalExceptionHandling()
UseCors()
UseAuthentication()          ← antes de UseAuthorization
UseAuthorization()
UserProvisioningMiddleware   ← después de UseAuthentication
MapControllers()
MapHealthChecks("/health")
```

### 4.2 UserProvisioningMiddleware

```
Si context.User.Identity.IsAuthenticated == false
	→ await _next(context); return

Lee "sub"   → externalUserId
Lee "name"  → displayName (nullable)
Lee "email" → email (nullable)
Lee config["Authentication:IdentityProvider"] → identityProvider

userId = await _sender.Send(new SyncProfileCommand(
	identityProvider, externalUserId, displayName, email))

context.Items["UserId"] = userId
await _next(context)
```

### 4.3 HttpCurrentUserService

```csharp
public Guid UserId =>
	_accessor.HttpContext?.Items["UserId"] is Guid id ? id : Guid.Empty;

public bool IsAuthenticated =>
	_accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
```

### 4.4 ProfileController — `[Route("api/v1/users")] [Authorize]`

| Verbo | Ruta | MediatR | Response |
|-------|------|---------|----------|
| GET | `/me` | `GetMyProfileQuery` | `200 UserProfileDto` |
| PUT | `/me` | `UpdateProfileCommand` | `200 UserProfileDto` |
| POST | `/me/sync` | `SyncProfileCommand` | `201 UserProfileDto` si crea; `200 UserProfileDto` si actualiza |

### 4.5 GroupsController — `[Route("api/v1/groups")] [Authorize]`

| Verbo | Ruta | MediatR | Response |
|-------|------|---------|----------|
| POST | `/` | `CreateGroupCommand` | `201` + `Location` header |
| GET | `/` | `GetMyGroupsQuery` | `200 GroupDto[]` — FR-017: solo grupos del caller |
| GET | `/{groupId}/members` | `GetGroupMembersQuery` | `200 GroupMemberDto[]` |
| POST | `/{groupId}/members` | `AddMemberToGroupCommand` | `201 GroupMemberDto` |
| DELETE | `/{groupId}/members/{userId}` | `RemoveMemberFromGroupCommand` | `204` |

### 4.6 Dockerfile — gate TP4

Multi-stage build. Stage `base`: `aspnet:8.0`. Stage `build`: `sdk:8.0`, restaurar y compilar proyectos Users. Stage `publish`: `dotnet publish -c Release`. Stage `final`: `ENTRYPOINT ["dotnet", "MiKompri.Users.Api.dll"]`.

Referencia de estructura: `MiKompri.ShoppingList.Api/Dockerfile`.

---

## Phase 5 — Docker

### 5.1 docker-compose.yml — añadir servicio

```yaml
  mikompriusersapi:
	image: mikompri.users.api
	build:
	  context: .
	  dockerfile: MiKompri.Users.Api/Dockerfile
	depends_on:
	  - postgres
	ports:
	  - "8082:8080"
	healthcheck:
	  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
	  interval: 30s
	  timeout: 10s
	  retries: 3
	  start_period: 40s
```

### 5.2 docker-compose.override.yml — añadir env vars + corregir typo

Añadir para `mikompriusersapi`:

```yaml
	environment:
	  - ASPNETCORE_ENVIRONMENT=Development
	  - ASPNETCORE_HTTP_PORTS=8080
	  - ConnectionStrings__UsersPostgreSQL=Host=postgres;Port=5432;Database=MiKompri_Users;Username=postgres;Password=12345
	  - Authentication__Authority=https://<idp-dev>/
	  - Authentication__Audience=mikompri-users
	  - Authentication__IdentityProvider=entra
	ports:
	  - "8082:8080"
	  - "8083:8081"
```

> ⚠ **Corregir typo en la entry existente de ShoppingList**: `PostgreSQ L=` → `PostgreSQL=`.

### 5.3 Base de datos MiKompri_Users

La API ejecuta `Database.Migrate()` en arranque — EF crea la BD si no existe (Npgsql lo soporta). Si el entorno Docker requiere que la BD exista antes, añadir script de init:

```sql
-- docker/init-users-db.sql
SELECT 'CREATE DATABASE "MiKompri_Users"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'MiKompri_Users')\gexec
```

Montar en `postgres` → `/docker-entrypoint-initdb.d/`.

---

## Phase 6 — Tests

### 6.1 Crear proyectos

```powershell
dotnet new xunit -n MiKompri.Users.Domain.Tests      -o test/MiKompri.Users.Domain.Tests
dotnet new xunit -n MiKompri.Users.Application.Tests -o test/MiKompri.Users.Application.Tests
dotnet new xunit -n MiKompri.Users.Api.Tests         -o test/MiKompri.Users.Api.Tests
# Añadir al sln y agregar referencias a proyectos + paquetes (FluentAssertions, NSubstitute/Moq, coverlet)
```

### 6.2 Tests de Dominio (obligatorios)

| Clase | Escenarios |
|-------|-----------|
| `GroupTests` | Crear → Owner auto-creado en membresías; AddMember duplicado → throws; RemoveMember Owner → throws; Admin elimina Member → ok; Admin elimina Admin → throws; Admin elimina Owner → throws |
| `UserTests` | Constructor sin name → `DisplayName` vacío permitido (CHK016-provisioning); `UpdateProfile` vacío → throws (CHK016-actualización); `SyncClaims` solo actualiza si hay cambios; `SyncClaims` actualiza `UpdatedAt` |

### 6.3 Tests de Aplicación (obligatorios)

Mocks para `IUserRepository`, `IGroupRepository`, `ICurrentUserService`.

| Clase | Escenarios |
|-------|-----------|
| `SyncProfileCommandHandlerTests` | No existe → crea + devuelve `created=true` (middleware); ya existe + claims distintos → `SyncClaims` + devuelve `created=false` (POST /me/sync); ya existe + claims iguales → no-op + devuelve `created=false` |
| `GetMyProfileQueryHandlerTests` | Existe → devuelve DTO; no existe → `KeyNotFoundException` |
| `UpdateProfileCommandHandlerTests` | Válido → guarda; vacío → `ValidationException` |
| `CreateGroupCommandHandlerTests` | No auth → throws; válido → devuelve `GroupId` |
| `AddMemberToGroupCommandHandlerTests` | Owner+Admin → ok; Owner+Member → ok; Admin+Member → ok; Admin+Admin → throws; Member → throws; duplicado → throws |
| `RemoveMemberFromGroupCommandHandlerTests` | Owner elimina cualquiera → ok; Admin elimina Member → ok; Admin elimina Admin → throws; Admin elimina Owner → throws; último Owner → throws; no miembro → `KeyNotFoundException` |
| `GetMyGroupsQueryHandlerTests` | Caller con grupos → devuelve lista con MyRole; caller sin grupos → devuelve colección vacía (no throws) |
| `GetGroupMembersQueryHandlerTests` | Caller miembro → devuelve lista; caller no miembro → throws |

### 6.4 Tests de Integración API (obligatorios)

`WebApplicationFactory` con `UsersDbContext` InMemory + `TestAuthHandler` (sin conexión al IdP real).

| Clase | Escenarios mínimos |
|-------|--------------------|
| `ProfileApiTests` | `GET /me` sin auth → 401; con auth → 200 + auto-provisioning; `PUT /me` válido → 200; vacío → 400; `POST /me/sync` primera vez → 201; segunda vez mismo token → 200 |
| `GroupsApiTests` | `POST /groups` → 201; `GET /` caller con grupos → 200 (FR-017); `GET /` caller sin grupos → 200 array vacío; `GET /{id}/members` como miembro → 200; `GET /{id}/members` caller no miembro → 403; Owner agrega Member → 201; Admin agrega Admin → 403; Admin elimina Admin → 403; duplicado → 400 |

---

## Risks & Constraints

| Riesgo | Severidad | Mitigación |
|--------|-----------|-----------|
| JWT Bearer sin IdP real en tests de integración | **ALTA** | `TestAuthHandler` con clave simétrica de prueba en `WebApplicationFactory`; no conecta al IdP |
| `ExceptionHandlingMiddleware` en namespace de ShoppingList — no compartible directamente | **MEDIA** | Copiar a `MiKompri.Users.Api.Middleware`; diferir consolidación a refactor futuro |
| `UserProvisioningMiddleware` agrega consulta DB en cada request autenticado | **MEDIA** | Índice único existente en `(IdentityProvider, ExternalUserId)` → latencia < 5 ms esperada |
| `GroupRole enum`: `Member` cambia de `2` a `3` | **BAJA** | No hay migración previa; EF almacena strings — cambio seguro |
| Dockerfile de Users.Api no existe | **ALTA** | Primera tarea de implementación — gate TP4 bloqueante |
| Typo `PostgreSQ L=` en `docker-compose.override.yml` | **MEDIA** | Corregir al editar el archivo (Phase 5) |

---

## Constitution Check Post-Diseño

| Principio | Estado | Comentario |
|-----------|--------|-----------|
| TP4 Docker | **PENDIENTE** | Dockerfile en tasks.md como tarea de prioridad 0 |
| TP8 Testing | ✅ | 3 proyectos de test con cobertura dominio, aplicación e integración |
| TP9 ADR | ✅ | research.md documenta 6 decisiones de arquitectura |
| Resto | ✅ | Sin cambios respecto a primera evaluación |

---

**Próximo paso**: `/speckit.tasks` — generar `tasks.md` con ítems ejecutables ordenados por fase y prioridad.
