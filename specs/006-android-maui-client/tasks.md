---

description: "Task list for MVP-4 Cliente Android con .NET MAUI"
---

# Tasks: MVP-4 Cliente Android con .NET MAUI

**Input**: `specs/006-android-maui-client/` (spec.md, plan.md, research.md, data-model.md, contracts/api-contracts.md, quickstart.md) y `.specify/memory/constitution.md` (TP1-TP11)

**Prerequisites**: plan.md, spec.md

**Tests**: Obligatorios (TP8). Los tests de Domain/Application/API se escriben primero y deben fallar antes de implementar. No hay UI automation en MVP-4.

**Organization**: Por fases y por historia de usuario (US1-US10 de spec.md). Las fases 2-4 son backend (TP11) y bloquean al cliente.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: paralelizable (archivos distintos, sin dependencia de tareas incompletas)
- **[USn]**: historia de usuario de spec.md
- Rutas reales del repositorio; el cliente vive en `MiKompri.Mobile/` y `test/MiKompri.Mobile.Tests/` (nuevos, según plan.md)

## Alcance y límites

- Sin sincronización offline bidireccional, sin OBO, sin audiences separadas nuevas, sin Web/iOS, sin escritura de catálogo desde el cliente, sin UI automation.
- Docker/CI solo para backend (TP4); el cliente MAUI se compila con `dotnet build` (sin contenedor).
- Despliegue actual: imágenes en GHCR; Azure sigue planificado (no se adelanta).
- Preservar el patrón de persistencia de cada contexto (ShoppingList: `IUnitOfWork`; Users: repositorios con `SaveChangesAsync`).

---

## Phase 1: Setup

**Purpose**: Línea base y esqueleto de proyectos.

- [ ] T001 Verificar línea base backend: `dotnet restore MiKompri.sln` y `dotnet test MiKompri.sln --configuration Release` en verde antes de cambios (regresión MVP-0..MVP-3), anotar resultado en `specs/006-android-maui-client/quickstart.md` § Prerrequisitos si hay fallos previos
- [ ] T002 Instalar/verificar workload MAUI Android (`dotnet workload install maui-android`) y documentarlo en `specs/006-android-maui-client/quickstart.md` § Prerrequisitos
- [ ] T003 Crear proyecto `MiKompri.Mobile/MiKompri.Mobile.csproj` (.NET MAUI, `net8.0-android` únicamente) con estructura `Platforms/Android/`, `Services/{Auth,Http,Navigation}/`, `ViewModels/`, `Views/`, `Models/`, `Configuration/`, `Resources/` y añadirlo a `MiKompri.sln`
- [ ] T004 Crear proyecto de tests `test/MiKompri.Mobile.Tests/MiKompri.Mobile.Tests.csproj` (xUnit, sin UI automation) con referencia a `MiKompri.Mobile` y añadirlo a `MiKompri.sln`
- [ ] T005 [P] Añadir paquetes del cliente en `MiKompri.Mobile/MiKompri.Mobile.csproj`: `Microsoft.Identity.Client`, `CommunityToolkit.Mvvm`, `Microsoft.Extensions.Http`
- [ ] T006 [P] Añadir al `test/MiKompri.Mobile.Tests/MiKompri.Mobile.Tests.csproj` las librerías de fakes/mocks consistentes con `test/MiKompri.*.Tests` existentes

---

## Phase 2: Foundational A — Identidad y migración en `Users` (TP11, bloquea todo)

**Purpose**: Modelo `(tid, oid)`, `ExternalUserId` legacy/nullable, migración EF Core, Fase 1 lazy.

**⚠️ CRITICAL**: Ninguna historia del cliente ni endurecimiento de otros contextos antes de completar esta fase.

### Tests primero (Domain)

- [ ] T007 [P] Tests de dominio para `User` (captura `TenantId`/`ObjectId`, `ExternalUserId` nulo, creación de usuario nuevo solo con `(tid, oid)` sin `ExternalUserId`, rechazo de `""` como `ExternalUserId`) en `test/MiKompri.Users.Domain.Tests/` (nuevo `UserIdentityTests.cs`)

### Tests primero (Application)

- [ ] T008 [P] Tests del handler de resolución/sincronización de identidad en `test/MiKompri.Users.Application.Tests/Commands/SyncProfileCommandHandlerTests.cs`: usuario nuevo por `(tid, oid)`; usuario legacy por `sub` que autentica → conserva `UserId` y obtiene `(tid, oid)`; usuario ya migrado; nunca crea duplicado; usuario legacy que no autentica permanece sin `(tid, oid)`

### Tests primero (API)

- [ ] T014a [P] Verificar o crear `CustomWebApplicationFactory.cs` y `TestAuthHandler.cs` en `test/MiKompri.Users.Api.Tests/` con claims `tid`/`oid`/`sub` configurables (mismo patrón que ShoppingList)
- [ ] T015 Tests de integración de migración de identidad Fase 1 (deben fallar antes de T013/T014; depende de T014a) en `test/MiKompri.Users.Api.Tests/IdentityMigrationApiTests.cs` (usando `TestAuthHandler.cs` y `CustomWebApplicationFactory.cs`, con tokens que incluyen `tid`/`oid`/`sub`): (a) usuario legacy (solo `sub`) que autentica conserva el mismo `UserId`; (b) `tid` y `oid` quedan capturados y persistidos correctamente en ese perfil; (c) usuario nuevo se crea correlacionado por `(tid, oid)`; (d) un segundo login del mismo usuario no genera usuarios duplicados (legacy ni nuevo); (e) usuarios nuevos tienen `ExternalUserId = null` (nunca `""`); (f) perfil legacy que no autentica permanece intacto

### Implementación

- [ ] T009 Modificar `MiKompri.Users.Domain/Users/User.cs`: `ExternalUserId` → `string?`, añadir `TenantId: string?` y `ObjectId: string?`, método para asociar `(tid, oid)` a perfil legacy, factoría para usuario nuevo solo por `(tid, oid)`; prohibir `""` (data-model.md)
- [ ] T010 Modificar `MiKompri.Users.Domain/Users/IUserRepository.cs` y `MiKompri.Users.Infrastructure/Persistence/Repositories/UserRepository.cs`: búsqueda por `(TenantId, ObjectId)` además de por `ExternalUserId`/`IdentityProvider`
- [ ] T011 Modificar `MiKompri.Users.Infrastructure/Persistence/Configurations/UserConfiguration.cs`: columna `ExternalUserId` nullable; reemplazar índice único de `ExternalUserId` por índice único filtrado que excluya `NULL`; añadir índice único filtrado sobre `(TenantId, ObjectId)` que excluya `NULL`
- [ ] T012 Generar migración EF Core en `MiKompri.Users.Infrastructure/Persistence/Migrations/` (nullable `ExternalUserId`, columnas `TenantId`/`ObjectId`, índices filtrados) y actualizar `UsersDbContextModelSnapshot.cs`; la migración no ejecuta backfill de datos
- [ ] T013 Modificar `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommand.cs` y `SyncProfileCommandHandler.cs` para incluir `tid`/`oid` y aplicar lógica Fase 1 (lazy): buscar por `(tid, oid)` → si no, por `sub` y asociar `(tid, oid)` al `UserId` existente → si no, crear nuevo solo con `(tid, oid)`
- [ ] T014 Modificar `MiKompri.Users.Api/Services/HttpCurrentUserService.cs` y el punto de sincronización en `MiKompri.Users.Api/Controllers/ProfileController.cs` para extraer claims `tid`/`oid` (y `sub` solo para correlación legacy transitoria)
- [ ] T016 Verificar que `dotnet test test/MiKompri.Users.Domain.Tests`, `Users.Application.Tests` y `Users.Api.Tests` pasan en Release

**Checkpoint**: Fase 1 de migración operativa en `Users`.

---

## Phase 3: Foundational B — Autenticación en ProductCatalog y audience común

**Purpose**: JWT real en `ProductCatalog.Api` y mecanismo `ValidAudiences` homogéneo (sin OBO ni audiences nuevas).

### Tests primero

- [ ] T017 [P] Crear `test/MiKompri.ProductCatalog.Api.Tests/CustomWebApplicationFactory.cs` y `TestAuthHandler.cs` (mismo patrón que `test/MiKompri.ShoppingList.Api.Tests/`) si no existen
- [ ] T017a Inventariar los endpoints reales de `ProductCatalog.Api` (controladores, verbos y rutas) y fijar esa lista como alcance definitivo de T018 y T022
- [ ] T018 Tests (lista según T017a) en `test/MiKompri.ProductCatalog.Api.Tests/CatalogAuthApiTests.cs` para TODOS los endpoints existentes de `ProductCatalog.Api`: `401 Unauthorized` sin token y acceso correcto (no 401/403) con token válido en `GET/POST/PUT api/v1/catalog-products`, `GET api/v1/catalog-products/{id}`, `GET api/v1/catalog-products/{id}/price-history`, `GET/POST/PUT api/v1/markets`, `GET api/v1/markets/{id}` y `POST api/v1/product-prices` (lista final según T017a); los tests de escritura validan solo autenticación, no cambian su comportamiento funcional
- [ ] T019 Adaptar `test/MiKompri.ProductCatalog.Api.Tests/CatalogProductsPriceHistoryApiTests.cs` para autenticarse con el handler de test (regresión MVP-1/2)

### Implementación

- [ ] T020 Modificar `MiKompri.ProductCatalog.Api/Program.cs`: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` con `Authority` común y soporte `ValidAudiences` (mismo patrón que `MiKompri.Users.Api/Program.cs`), `app.UseAuthentication()` antes de `app.UseAuthorization()`
- [ ] T021 Añadir paquete `Microsoft.AspNetCore.Authentication.JwtBearer` en `MiKompri.ProductCatalog.Api/MiKompri.ProductCatalog.Api.csproj` y configuración `Authentication:*` en `MiKompri.ProductCatalog.Api/appsettings.json` y `appsettings.Development.json`
- [ ] T022 Aplicar `[Authorize]` a nivel de controlador en `MiKompri.ProductCatalog.Api/Controllers/CatalogProductsController.cs`, `MarketsController.cs` y `ProductPricesController.cs`, de modo que TODOS los endpoints existentes (lista exacta según T017a; documentación diferida a T079) requieran JWT; no se añaden endpoints nuevos ni roles/políticas adicionales, y el cliente Android sigue siendo read-only (solo GET)
- [ ] T023 [P] Homogeneizar `ValidAudiences` en `MiKompri.ShoppingList.Api/Program.cs` y `MiKompri.ShoppingList.Api/appsettings*.json` reutilizando el patrón de `Users.Api` (audience histórica + audience backend común, transición TP11)
- [ ] T024 [P] Documentar/configurar variables de entorno de audience y authority para las tres APIs en `docker-compose.yml` y `docker-compose.override.yml` (solo backend)
- [ ] T025 Verificar `dotnet test` de `ProductCatalog.Api.Tests` y `ShoppingList.Api.Tests` en Release

**Checkpoint**: Las tres APIs validan JWT con el mismo mecanismo de audiences.

---

## Phase 4: Foundational C — Endurecimiento de listas personales en `ShoppingList` (FR-026/027)

**Purpose**: `[Authorize]` y `OwnerId` derivado de la identidad autenticada.

### Tests primero

- [ ] T026 [P] Tests Application: `CreateShoppingListCommandHandler` usa `ICurrentUserService.UserId` como `OwnerId` y no acepta valor del request, en `test/MiKompri.ShoppingList.Application.Tests/` (actualizar `CreateShoppingListCommandHandlerTests.cs`)
- [ ] T027 [P] Tests Application: queries/commands de consulta, edición y eliminación de listas personales rechazan acceso cuando `OwnerId` ≠ `UserId` autenticado (error de autorización/`KeyNotFoundException` según convención de `ExceptionHandlingMiddleware`), en `test/MiKompri.ShoppingList.Application.Tests/`
- [ ] T028 [P] Tests Domain: `PurchaseList` mantiene `OwnerId` inmutable tras construcción, en `test/MiKompri.ShoppingList.Domain.Tests/PurchaseListTests.cs`
- [ ] T029 Tests API `401` sin token en todos los endpoints de listas personales y `403/404` en acceso cruzado entre dos usuarios (Usuario B sobre lista de A: consultar, editar, eliminar, añadir ítem) en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs` (o nuevo `PersonalListsAuthApiTests.cs`)
- [ ] T030 Test API de no regresión en `test/MiKompri.ShoppingList.Api.Tests/PersonalListsRegressionApiTests.cs`: lista preexistente con `OwnerId` válido; GET/PUT/DELETE del propietario responden 2xx (spec US4 escenario 6)

### Implementación

- [ ] T031 Modificar `MiKompri.ShoppingList.Application/Commands/CreateShoppingList/` (command/handler/validator): eliminar `OwnerId` del request y resolverlo desde `ICurrentUserService.UserId`
- [ ] T032 Modificar `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`: eliminar `OwnerId` de `CreatePurchaseListRequest`, añadir `[Authorize]` a `Create`, `GetAll`, `GetById`, edición, eliminación e ítems de listas personales, e ignorar/eliminar el parámetro `ownerId` de query usando siempre el `UserId` del token
- [ ] T033 Modificar handlers de consulta/edición/eliminación de listas personales en `MiKompri.ShoppingList.Application/Commands/` y `Queries/` para comprobar propiedad (`OwnerId == ICurrentUserService.UserId`) antes de operar
- [ ] T034 Verificar `MiKompri.ShoppingList.Api/Services/HttpCurrentUserService.cs` resuelve el `UserId` interno a partir del token (vía `Users.Api`, sin acceso directo a la BD de Users, TP2/TP11) y cubrirlo con test en `test/MiKompri.ShoppingList.Api.Tests/CrossService/`
- [ ] T035 Actualizar `test/MiKompri.ShoppingList.Api.Tests/CustomWebApplicationFactory.cs` y `TestAuthHandler.cs` si se necesita autenticar usuarios distintos
- [ ] T036 Ejecutar `dotnet test` de `ShoppingList.Domain.Tests`, `Application.Tests` y `Api.Tests` en Release

**Checkpoint**: Backend listo para el cliente (Users + ShoppingList + ProductCatalog con identidad TP11).

---

## Phase 5: User Story 1 + 2 — Login Entra ID y token MSAL (P1) 🎯 MVP

**Goal**: Login interactivo, `AcquireTokenSilent` antes de cada llamada, fallback interactivo y logout.

**Independent Test**: quickstart Escenarios 1 y 7.

### Tests

- [ ] T037 [P] [US1] Tests de `AuthService` (fake MSAL): login ok, cancelación/fallo con error reintentable, restauración de sesión sin interacción en `test/MiKompri.Mobile.Tests/Services/AuthServiceTests.cs`
- [ ] T038 [P] [US2] Tests del `DelegatingHandler` de autenticación: orden `AcquireTokenSilent` → fallback interactivo solo ante `MsalUiRequiredException` → reintento; ninguna gestión manual de refresh tokens, en `test/MiKompri.Mobile.Tests/Services/AuthDelegatingHandlerTests.cs`
- [ ] T039 [US2] Tests de logout y no persistencia propia con fake de `IPublicClientApplication` en `test/MiKompri.Mobile.Tests/Services/MsalSessionTests.cs`: (a) logout invoca `RemoveAccount` y limpia el estado de sesión en memoria; (b) la app no usa `SecureStorage` ni persiste tokens/refresh tokens (FR-016b); restauración de sesión y fallback ya cubiertos por T037/T038

### Implementación

- [ ] T040 [US1] Definir abstracción `IAuthService` en `MiKompri.Mobile/Services/Auth/IAuthService.cs` (agnóstica de plataforma, FR-016)
- [ ] T041 [US1] Implementar adaptador MSAL Android en `MiKompri.Mobile/Services/Auth/MsalAuthService.cs` (PKCE, una cuenta por instalación, caché cifrada de MSAL; sin copias de tokens en `SecureStorage`)
- [ ] T042 [US1] Configurar `Platforms/Android/AndroidManifest.xml`, `MsalActivity.cs` y `MainActivity.cs` para el redirect de MSAL en `MiKompri.Mobile/Platforms/Android/`
- [ ] T043 [US2] Implementar `AuthDelegatingHandler` en `MiKompri.Mobile/Services/Http/AuthDelegatingHandler.cs` (`AcquireTokenSilent` antes de cada llamada; interactivo solo si falla)
- [ ] T044 [US1] Implementar módulo de identidad en `MiKompri.Mobile/Services/Identity/` (estado de sesión sin tratar `tid`/`oid` como datos de negocio del cliente)
- [ ] T045 [US1] Crear `LoginViewModel` y `LoginPage` en `MiKompri.Mobile/ViewModels/Login/` y `MiKompri.Mobile/Views/` con error claro y reintento
- [ ] T046 [US2] Implementar logout (`RemoveAccount`) en `MsalAuthService` y acción de cerrar sesión en la UI de perfil/ajustes
- [ ] T047 [US1] Manejo de errores de autorización (audience/scope) distinguible de conectividad en la capa Http

**Checkpoint**: Usuario puede iniciar/restaurar/cerrar sesión y llamar APIs con token válido.

---

## Phase 6: User Story 9 + 10 — Navegación, estados y configuración por entorno (P1/P3)

**Goal**: Shell principal, estados Loading/Success/Error/Offline, URLs por entorno.

**Independent Test**: quickstart Escenario 6; cambio de entorno por build.

### Tests

- [ ] T048 [P] [US9] Tests de `ViewModelBase`/estado de UI (`Idle→Loading→Success|Error|Offline`) y mapeo de errores HTTP (401/403/4xx/5xx vs. conectividad) en `test/MiKompri.Mobile.Tests/ViewModels/ViewModelBaseTests.cs`
- [ ] T049 [P] [US10] Tests de carga de configuración por entorno (desarrollo/staging/producción) en `test/MiKompri.Mobile.Tests/Configuration/EnvironmentConfigTests.cs`

### Implementación

- [ ] T050 [US10] Crear `MiKompri.Mobile/Configuration/` con `appsettings.{Development,Staging,Production}.json` (URLs base de Users, ShoppingList, ProductCatalog, authority, client id, scopes) y lector de configuración
- [ ] T051 [US9] Crear `ViewModelBase` con estado de UI y detección offline (`Connectivity`) en `MiKompri.Mobile/ViewModels/ViewModelBase.cs`; las escrituras sin conexión no se intentan ni se reintentan automáticamente (FR-015)
- [ ] T052 [US9] Configurar `AppShell.xaml` con navegación a Listas, Grupos, Catálogo y Perfil en `MiKompri.Mobile/` y `Services/Navigation/`
- [ ] T053 [US9] Registrar clientes tipados `UsersApiClient`, `ShoppingListApiClient`, `ProductCatalogApiClient` con `IHttpClientFactory` y `AuthDelegatingHandler` en `MiKompri.Mobile/MauiProgram.cs` y `Services/Http/`
- [ ] T054 [P] [US9] Crear componentes XAML reutilizables de loading/error/offline en `MiKompri.Mobile/Views/Controls/`

**Checkpoint**: Base de cliente (auth, HTTP, navegación, estados, entornos) lista para las historias funcionales.

---

## Phase 7: User Story 3 — Perfil propio (P2)

**Independent Test**: ver y editar nombre visible tras login.

- [ ] T055 [P] [US3] Tests de `ProfileViewModel` (carga, edición válida, nombre vacío sin llamada a API) en `test/MiKompri.Mobile.Tests/ViewModels/ProfileViewModelTests.cs`
- [ ] T056 [US3] Implementar métodos de perfil en `MiKompri.Mobile/Services/Http/UsersApiClient.cs` (`ProfileController`: obtener/actualizar) y modelos en `MiKompri.Mobile/Models/`
- [ ] T057 [US3] Crear `ProfileViewModel` y `ProfilePage` en `MiKompri.Mobile/ViewModels/Profile/` y `Views/`

---

## Phase 8: User Story 4 + 5 — Listas personales e ítems (P1)

**Independent Test**: quickstart Escenario 3; crear lista, añadir ítem, marcar comprado, eliminar.

- [ ] T058 [P] [US4] Tests de `PersonalListsViewModel`: crear/editar/eliminar; la creación nunca envía `OwnerId`; errores sin pérdida de datos del formulario, en `test/MiKompri.Mobile.Tests/ViewModels/PersonalListsViewModelTests.cs`
- [ ] T059 [P] [US5] Tests de `ListItemsViewModel`: añadir, editar, marcar comprado, eliminar ítem, en `test/MiKompri.Mobile.Tests/ViewModels/ListItemsViewModelTests.cs`
- [ ] T060 [US4] Implementar métodos de listas personales e ítems en `MiKompri.Mobile/Services/Http/ShoppingListApiClient.cs` (sin `OwnerId` en el contrato) y modelos en `Models/`
- [ ] T061 [US4] Crear `PersonalListsViewModel`, `PersonalListDetailViewModel` y vistas en `MiKompri.Mobile/ViewModels/PersonalLists/` y `Views/`
- [ ] T062 [US5] Crear `ListItemsViewModel` y vistas en `MiKompri.Mobile/ViewModels/Items/` y `Views/`

---

## Phase 9: User Story 5b — Gestión mínima de grupos (P2)

**Independent Test**: quickstart Escenario 4.

- [ ] T063 [P] [US5b] Tests de `GroupsViewModel`: listar con rol, crear grupo, ver miembros, añadir/eliminar miembro, UI deshabilita acciones según rol pero el backend decide (FR-025), en `test/MiKompri.Mobile.Tests/ViewModels/GroupsViewModelTests.cs`
- [ ] T064 [P] [US5b] Añadir los tests que falten de cobertura API mínima de grupos (listar, crear, ver miembros, añadir, eliminar, matriz Owner/Admin/Member) en `test/MiKompri.Users.Api.Tests/GroupsApiTests.cs` (listar en el PR los casos ya cubiertos)
- [ ] T065 [US5b] Implementar métodos de grupos en `MiKompri.Mobile/Services/Http/UsersApiClient.cs` consumiendo `MiKompri.Users.Api/Controllers/GroupsController.cs` (sin cambio de rol ni administración avanzada)
- [ ] T066 [US5b] Crear `GroupsViewModel`, `GroupMembersViewModel` y vistas en `MiKompri.Mobile/ViewModels/Groups/` y `Views/`

---

## Phase 10: User Story 6 + 7 — Listas compartidas, gastos, balances y settlement (P2)

**Independent Test**: crear lista compartida en grupo, registrar gasto con comprador/pagador/participantes, consultar balances y propuesta de liquidación.

- [ ] T067 [P] [US6] Tests de `SharedListsViewModel` y `SharedListExpensesViewModel` (diferenciar personales/compartidas, registrar gasto con comprador, pagador, precio real y participantes) en `test/MiKompri.Mobile.Tests/ViewModels/SharedListsViewModelTests.cs`
- [ ] T068 [P] [US7] Tests de `SettlementViewModel` (balances por participante y propuesta de liquidación) en `test/MiKompri.Mobile.Tests/ViewModels/SettlementViewModelTests.cs`
- [ ] T069 [US6] Implementar métodos de listas compartidas, ítems y gastos en `MiKompri.Mobile/Services/Http/ShoppingListApiClient.cs` según contratos de `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs` (spec 005) y modelos en `Models/`
- [ ] T070 [US6] Crear `SharedListsViewModel`, `SharedListDetailViewModel`, `SharedListExpensesViewModel` y vistas en `MiKompri.Mobile/ViewModels/SharedLists/` y `Views/`
- [ ] T071 [US7] Implementar consulta de summary/proposal de settlement en `ShoppingListApiClient.cs` y crear `SettlementViewModel` y vistas en `MiKompri.Mobile/ViewModels/Settlement/` y `Views/`

---

## Phase 11: User Story 8 — Catálogo read-only (P3)

**Independent Test**: quickstart Escenario 5.

- [ ] T072 [P] [US8] Tests de `CatalogViewModel`/`PriceHistoryViewModel` (productos activos, mercados activos, historial cronológico con mercado; sin acciones de escritura) en `test/MiKompri.Mobile.Tests/ViewModels/CatalogViewModelTests.cs`
- [ ] T073 [US8] Implementar consultas de solo lectura en `MiKompri.Mobile/Services/Http/ProductCatalogApiClient.cs` (catalog-products, markets, price-history)
- [ ] T074 [US8] Crear `CatalogViewModel`, `MarketsViewModel`, `PriceHistoryViewModel` y vistas en `MiKompri.Mobile/ViewModels/Catalog/` y `Views/` sin acciones de creación/edición (FR-016d)

---

## Phase 12: Polish, regresión y CI (solo backend donde aplique)

- [ ] T075 Regresión completa MVP-0..MVP-3: `dotnet test MiKompri.sln --configuration Release` (Domain, Application, Api de Users, ShoppingList y ProductCatalog) y revisar que no hay cambios de comportamiento fuera de lo descrito en contracts/api-contracts.md
- [ ] T076 Test de verificación SC-007: búsqueda técnica de usos de `sub`/`ExternalUserId` para correlación fuera de la ruta legacy transitoria (código backend y cliente) y registrar resultado en `specs/006-android-maui-client/quickstart.md`
- [ ] T077 [P] Ajustar `.github/workflows/ci-mikompri-shoppinglist.yml` y `.github/workflows/ci-mikompri-productcatalog.yml` para ejecutar los nuevos tests; revisar el workflow/CI de Users si existe o documentar que se cubre por `dotnet test MiKompri.sln`. Sin pipeline de contenedor para el cliente MAUI
- [ ] T078 [P] Añadir job de CI en `.github/workflows/` (p. ej. `ci-mikompri-mobile.yml`) que, en este orden: instale/verifique el workload `maui-android` (mismo comando que T002; `dotnet workload install maui-android`, más JDK/Android SDK si el runner lo requiere), compile `MiKompri.Mobile` (`dotnet build -f net8.0-android`) y ejecute `test/MiKompri.Mobile.Tests`, sin `docker build` para este artefacto (TP4 no aplica al cliente)
- [ ] T079 Actualizar `specs/006-android-maui-client/spec.md` (FR-023a), `plan.md`, `contracts/api-contracts.md` y `quickstart.md` para reflejar que TODOS los endpoints de `ProductCatalog.Api` (incluidos los de escritura) requieren JWT, que las rutas reales son `api/v1/catalog-products`, `api/v1/markets` y `api/v1/product-prices`, y que el cliente solo consume GET; la autorización por roles para escritura queda fuera de alcance
- [ ] T080 [P] Documento operativo del criterio de entrada a Fase 2 (consulta/reporte de cobertura de perfiles activos con `(tid, oid)`; backfill administrativo como alternativa) en `specs/006-android-maui-client/quickstart.md`
- [ ] T081 Implementar y testear la **capacidad** de Fase 2 (no su activación): resolución primaria por `(tid, oid)` con `sub` como fallback de solo lectura en `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommandHandler.cs`, controlada por `Identity:CanonicalCorrelation` (`Legacy` por defecto, `TidOid`); tests con ambos valores (mismo `UserId`, sin duplicados) en `test/MiKompri.Users.Application.Tests/` y `test/MiKompri.Users.Api.Tests/` con el flag activado y desactivado (sin duplicados, mismo `UserId`). Depende de T013 y T023
- [ ] T081b Implementar y testear el cambio de audience principal de Fase 2 mediante `Authentication:PrimaryAudience` en `Program.cs` de las tres APIs (la convivencia de `ValidAudiences` ya la configura T023); tests: token con audience común aceptado y token con audience histórica aceptado durante la transición. Depende de T023 y T081
- [ ] T082 Ejecutar la validación completa de `specs/006-android-maui-client/quickstart.md` (Escenarios 1-7) en emulador/dispositivo con backend vía `docker-compose.yml`, midiendo y registrando SC-001 (login y perfil < 1 min), SC-003 (flujo de lista < 2 min) y SC-005 (estado visible < 10 s)
- [ ] T083 [P] Actualizar `README.md` con el estado de MVP-4 (cliente Android, identidad `(tid, oid)`, despliegue actual en GHCR y Azure planificado)
- [ ] T084 Activación operativa del corte de Fase 2 (**post-MVP, no bloquea el cierre técnico de MVP-4**): activar el flag de T081 en el entorno real solo cuando se cumpla el criterio documentado en T080 (cobertura suficiente de perfiles activos con `(tid, oid)`) o tras ejecutar un backfill administrativo; registrar evidencia (reporte de cobertura/backfill) en `specs/006-android-maui-client/quickstart.md`

---

## Dependencies & Execution Order

### Dependencias críticas

1. **Phase 1** → sin dependencias.
2. **Phase 2 (Users/identidad)** depende de Phase 1 (T001). Bloquea Phase 4 (T034), Phase 5 y todo el cliente.
3. **Phase 3 (ProductCatalog JWT + audience)** depende de T001; puede ejecutarse en paralelo a Phase 2 (contextos distintos), salvo T023/T024 que tocan configuración compartida.
4. **Phase 4 (ShoppingList)** depende de Phase 2 (resolución de `UserId`) y de T023.
5. **Phases 5-6 (cliente base)** dependen de T003-T006 y de Phases 2-4 para pruebas end-to-end; el desarrollo contra fakes puede comenzar tras Phase 1.
6. **Phases 7-11 (historias)** dependen de Phases 5-6 (auth, Http, navegación, estados).
7. **Phase 12** depende de las historias deseadas. **T081/T081b** (capacidad de Fase 2 + tests) sí forma parte del cierre técnico y depende de T013/T023; **T080** documenta el criterio; **T084** (activación operativa real) depende de T080, T081 y del despliegue de Fase 1, y queda fuera del cierre técnico del MVP.

### Orden entre historias

- US1/US2 (P1) → US9/US10 → US4/US5 (P1) → US3, US5b, US6/US7 (P2) → US8 (P3).
- US6/US7 dependen de US5b (grupos).

### Oportunidades de paralelismo

- Phase 1: T005, T006.
- Phase 2: T007, T008 y T014a (tests); T015 antes de T013/T014 (debe fallar primero).
- Phase 3: T017, T023, T024.
- Phase 4: T026, T027, T028.
- Cliente: tests [P] de cada historia antes de su implementación; historias US3, US5b, US8 pueden avanzar en paralelo entre sí tras Phase 6 (ficheros distintos), salvo `UsersApiClient.cs` compartido por US3 y US5b (T056 y T065 no son paralelos entre sí).
- Phase 12: T077, T078, T080, T083.
- Backend (Phases 2-4) y scaffolding del cliente (Phases 5-6 contra fakes) pueden solaparse entre equipos.

### Estrategia de implementación

- **MVP mínimo entregable**: Phases 1-6 + Phase 8 (login, token, navegación, listas personales seguras) verificable con quickstart Escenarios 1, 3, 6, 7.
- Incrementos: Phase 7 (perfil) → Phase 9 (grupos) → Phase 10 (compartidas/settlement) → Phase 11 (catálogo).
- Escribir tests primero y verificar que fallan antes de implementar (TP8); avanzar por fases completas.
