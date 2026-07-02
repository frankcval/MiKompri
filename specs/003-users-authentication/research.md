# Research: 003 — Users Authentication & Groups

**Spec**: [spec.md](./spec.md) | **Date**: 2026-07-12

---

## Hallazgos de inspección de codebase

### Bounded context Users — estado actual

| Proyecto | Estado |
|----------|--------|
| `MiKompri.Users.Domain` | Scaffolded y parcialmente implementado |
| `MiKompri.Users.Application` | Scaffolded con handlers parciales |
| `MiKompri.Users.Infrastructure` | Scaffolded con repos y DbContext |
| `MiKompri.Users.Api` | Solo bootstrapper mínimo (`WeatherForecastController` presente) |

---

## Inventario detallado de gaps

### Dominio

| Artefacto | Estado | Gap |
|-----------|--------|-----|
| `Entity.cs` | Existente | Solo tiene `Id`; falta `CreatedAt`, `UpdatedAt` |
| `User.cs` | Existente | Tiene `DisplayName`, `Email?`, `IdentityProvider`, `ExternalUserId`; falta timestamps; falta `SyncClaims()` para FR-016 |
| `Group.cs` | Existente | Tiene `Name`, `OwnerId`, `AddMember`, `RemoveMember`, `ChangeMemberRole`, `IsOwner`; `RemoveMember` solo guarda Owner, falta regla Admin-no-puede-eliminar-Admin |
| `GroupMembership.cs` | Existente | Tiene `GroupId`, `UserId`, `Role`; falta `JoinedAt` |
| `GroupRole.cs` | Existente | Solo tiene `Owner = 1`, `Member = 2`; **falta `Admin`** |
| `IUserRepository.cs` | Existente | `GetByIdAsync`, `GetByExternalIdAsync`, `AddAsync`, `UpdateAsync` — completo |
| `IGroupRepository.cs` | Existente | `GetByIdAsync`, `GetByUserIdAsync`, `AddAsync`, `UpdateAsync` — completo |
| `IGroupMembershipRepository.cs` | Existente | Solo reads — sin `AddAsync`/`UpdateAsync`, ok porque Group agrega memberships como aggregate |

### Aplicación

| Artefacto | Estado | Gap |
|-----------|--------|-----|
| `DependencyInjection.cs` | Existente | MediatR, FluentValidation, LoggingBehavior, ValidationBehavior ya registrados |
| `ICurrentUserService.cs` | Existente | `Guid UserId` + `bool IsAuthenticated` — interfaz completa; falta implementación concreta en Api |
| `CreateGroupCommandHandler.cs` | Existente | Solo verifica `IsAuthenticated`, no valida perfil local previo |
| `AddMemberToGroupCommandHandler.cs` | Existente | Solo verifica `IsOwner`; falta lógica Admin-puede-agregar-Member |
| `GetMyGroupsQuery/Handler` | Existente | Funcional |
| `GroupDto.cs` / `GroupMemberDto.cs` | Existente | Falta `UserProfileDto`; `GroupMemberDto` necesita revisión para `Admin` |
| `SyncProfileCommand` | **Faltante** | Para auto-provisioning en primer request |
| `RefreshProfileCommand` | **Faltante** | Para endpoint explícito de refresco |
| `GetMyProfileQuery` | **Faltante** | Para `GET /api/v1/users/me` |
| `UpdateProfileCommand` | **Faltante** | Para `PUT /api/v1/users/me` |
| `RemoveMemberFromGroupCommand` | **Faltante** | Para `DELETE /api/v1/groups/{id}/members/{userId}` |
| `GetGroupMembersQuery` | **Faltante** | Para `GET /api/v1/groups/{id}/members` |

### Infraestructura

| Artefacto | Estado | Gap |
|-----------|--------|-----|
| `UsersDbContext.cs` | Existente | `Users`, `Groups`, `GroupMemberships` — completo |
| `UserRepository.cs` | Existente | Llama `SaveChangesAsync()` internamente — **preservar este patrón** |
| `GroupRepository.cs` | Existente | Llama `SaveChangesAsync()` internamente — **preservar este patrón** |
| `GroupMembershipRepository.cs` | Existente | Solo reads; correcto, Group agrega memberships vía ORM cascade |
| `UserConfiguration.cs` | Existente | Índice único en `(IdentityProvider, ExternalUserId)` ✅; faltan columnas de timestamps |
| `GroupConfiguration.cs` | Existente | Relación Group→Memberships con cascade ✅; `OwnerId` mapeado ✅; faltan timestamps |
| `GroupMembershipConfiguration.cs` | Existente | Índice único `(GroupId, UserId)` ✅; `Role` como string (`HasConversion<string>`) ✅; falta `JoinedAt` |
| Migraciones | **Faltante** | No existe `Migrations/` en `Users.Infrastructure` |
| `IUnitOfWork` para Users | N/A | Los repos salvan directamente — no se introduce UnitOfWork en Users |

### API

| Artefacto | Estado | Gap |
|-----------|--------|-----|
| `Program.cs` | Scaffolded | Falta: Serilog, CORS, JWT Bearer auth, `UseAuthentication()`, exception middleware, `UserProvisioningMiddleware` |
| `WeatherForecastController.cs` | Existente | Eliminar |
| `ProfileController` | **Faltante** | `/api/v1/users/me` endpoints |
| `GroupsController` | **Faltante** | `/api/v1/groups` endpoints |
| `HttpCurrentUserService` | **Faltante** | Implementación de `ICurrentUserService` que lee de `HttpContext.Items` |
| `UserProvisioningMiddleware` | **Faltante** | Auto-provisioning de perfil antes de los controllers |
| `Dockerfile` | **Faltante** | No existe Dockerfile para Users.Api |

### Docker

| Artefacto | Estado | Gap |
|-----------|--------|-----|
| `docker-compose.yml` | Existente | Solo tiene ShoppingList API + postgres; falta servicio Users API |
| `docker-compose.override.yml` | Existente | Solo env vars de ShoppingList; falta Users env vars |
| Base de datos Users | **Faltante** | Necesita `MiKompri_Users` en la misma instancia PostgreSQL |

### Tests

| Proyecto | Estado |
|----------|--------|
| `MiKompri.Users.Domain.Tests` | **No existe** — crear |
| `MiKompri.Users.Application.Tests` | **No existe** — crear |
| `MiKompri.Users.Api.Tests` | **No existe** — crear |

---

## Decisiones de arquitectura ratificadas

### Decision 1: Patrón de SaveChanges en Users

- **Decisión**: Los repositorios de Users llaman `SaveChangesAsync()` internamente.
- **Rationale**: Patrón ya establecido en el bounded context. La instrucción de copilot-instructions.md es explícita: *"Preserve the pattern already used in the project you are editing instead of mixing them."*
- **Consecuencia**: No se introduce `IUnitOfWork` en Users.

### Decision 2: Resolución de `ICurrentUserService.UserId`

- **Decisión**: `UserProvisioningMiddleware` ejecuta después de `UseAuthentication()`; extrae el claim `sub`, busca/crea el perfil local y almacena el `Guid` interno en `HttpContext.Items["UserId"]`. `HttpCurrentUserService` lee desde ahí.
- **Rationale**: Evita llamadas a DB en cada operación del servicio; garantiza que `UserId` interno siempre esté disponible cuando el handler lo solicite.
- **Alternativa rechazada**: Resolver internamente en `HttpCurrentUserService` con `IUserRepository` — creaba dependencia circular y llamadas adicionales a DB.

### Decision 3: Ubicación del middleware de excepción

- **Decisión**: Copiar `ExceptionHandlingMiddleware` desde `ShoppingList.Api.Middleware` a `Users.Api.Middleware` con idéntica implementación.
- **Rationale**: No existe una capa compartida entre bounded contexts (TP2). Mover a un proyecto compartido es refactor de alcance mayor — diferir.
- **ADR**: Documentado aquí; revisitar cuando exista un tercer bounded context que comparta esta necesidad.

### Decision 4: Base de datos separada para Users

- **Decisión**: `MiKompri_Users` como base de datos separada en la misma instancia PostgreSQL.
- **Rationale**: Preserva el principio de bounded contexts independientes (TP2). Cada contexto gestiona su esquema.
- **Implementación Docker**: Script de inicialización en `docker-entrypoint-initdb.d/` o EF `Database.EnsureCreated()` / `Database.Migrate()` en `Program.cs`.

### Decision 5: Privilegios de Admin en dominio vs. aplicación

- **Decisión**: La regla "Admin no puede eliminar Admin u Owner" se implementa en el dominio (`Group.RemoveMember`). La verificación de si el caller es Admin u Owner se realiza en la capa de Aplicación (handler).
- **Rationale**: La invariante de qué roles pueden gestionar otros roles es una regla de negocio del agregado `Group`. La identidad del caller (autenticación) es concern de Aplicación.

### Decision 6: GroupRole enum — orden de valores

- **Decisión**: `Owner = 1`, `Admin = 2`, `Member = 3`.
- **Rationale**: No existen migraciones previas, por lo que el reordenamiento es seguro. La configuración EF usa `HasConversion<string>()` para que los valores de DB sean strings legibles, insensibles al int asignado.

---

## Patrones de referencia a seguir

| Patrón | Referencia en ShoppingList |
|--------|--------------------------|
| Serilog en Program.cs | `MiKompri.ShoppingList.Api/Program.cs` |
| Exception middleware | `MiKompri.ShoppingList.Api/Middleware/ExceptionHandlingMiddleware.cs` |
| Estructura de command handler | `CreateShoppingListCommandHandler.cs` |
| EF configuración con timestamps | `MiKompri.ShoppingList.Infrastructure/Migrations/` (snapshot) |
| WebApplicationFactory + InMemory | `test/MiKompri.ShoppingList.Api.Tests/` |
| DependencyInjection extension | `MiKompri.ShoppingList.Application` / `MiKompri.ShoppingList.Infrastructure` |
