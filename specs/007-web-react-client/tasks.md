# Tasks: 007-web-react-client

**Input**: Design documents from `/specs/007-web-react-client/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/frontend-backend-integration.md, quickstart.md

**Tests**: Obligatorios por TP8 y por requerimiento explícito de la feature. Cada historia incluye tareas de pruebas unitarias, de componente, integración con MSW y E2E con Playwright.

**Organization**: Tareas agrupadas por historia de usuario para permitir implementación y validación independiente.

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Crear el proyecto frontend base y preparar estructura técnica común del monorepo.

- [X] T001 Crear directorio `MiKompri.Web/` y archivo base `MiKompri.Web/README.md`
- [X] T002 Inicializar proyecto React + Vite en `MiKompri.Web/package.json`
- [X] T003 Configurar TypeScript strict en `MiKompri.Web/tsconfig.json`
- [X] T004 Configurar build/tooling de Vite en `MiKompri.Web/vite.config.ts`
- [X] T005 [P] Crear punto de entrada web en `MiKompri.Web/src/main.tsx`
- [X] T006 [P] Crear armazón inicial de aplicación en `MiKompri.Web/src/app/App.tsx`
- [X] T007 [P] Definir scripts de pruebas (unit/component/integration/e2e) en `MiKompri.Web/package.json`
- [X] T008 [P] Crear configuración base de Vitest en `MiKompri.Web/vitest.config.ts`
- [X] T009 [P] Crear configuración base de Playwright en `MiKompri.Web/playwright.config.ts`
- [X] T010 [P] Crear plantilla de entorno local/cloud en `MiKompri.Web/.env.example`

**Checkpoint**: Estructura base frontend creada en el monorepo.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Completar prerrequisitos backend y fundaciones frontend antes de historias de usuario.

**⚠️ CRITICAL**: Ninguna historia de usuario inicia hasta completar esta fase.

### Backend prerequisite (tests primero)

- [X] T011 [P] Agregar pruebas de migración de identidad `sub -> (tid, oid)` en `test/MiKompri.Users.Application.Tests/Commands/SyncProfile/SyncProfileCommandHandlerTests.cs`
- [X] T012 [P] Agregar pruebas de middleware Users para claims `tid`/`oid` en `test/MiKompri.Users.Api.Tests/Middleware/UserProvisioningMiddlewareTests.cs`
- [X] T013 [P] Agregar pruebas de autorización de listas personales por usuario autenticado en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`
- [X] T014 [P] Agregar pruebas JWT de ProductCatalog (401 sin token / 200 con token válido) en `test/MiKompri.ProductCatalog.Api.Tests/Auth/ProductCatalogAuthTests.cs`
- [X] T015 [P] Agregar pruebas de audience común entre APIs en `test/MiKompri.Users.Api.Tests/Security/CommonAudienceValidationTests.cs`
- [X] T016 [P] Agregar pruebas CORS para origen SPA local/cloud en `test/MiKompri.ShoppingList.Api.Tests/Security/CorsPolicyTests.cs`

### Backend prerequisite (implementación)

- [X] T017 Actualizar modelo de usuario para identidad canónica y `ExternalUserId` nullable en `MiKompri.Users.Domain/Users/User.cs`
- [X] T018 Ajustar mapeo EF de `ExternalUserId` nullable y nueva unicidad canónica en `MiKompri.Users.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [X] T019 Crear migración de Users para transición `sub -> (tid, oid)` y columna legacy nullable con nombre `AddCanonicalIdentityCorrelation` en `MiKompri.Users.Infrastructure/Migrations/`
- [X] T020 Adaptar comando de sincronización de perfil para `(tid, oid)` + legado en `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommand.cs`
- [X] T021 Adaptar handler de sincronización de perfil para resolver por identidad canónica en `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommandHandler.cs`
- [X] T022 Actualizar middleware de auto-provisioning Users para claims `tid`/`oid` en `MiKompri.Users.Api/Middleware/UserProvisioningMiddleware.cs`
- [X] T023 Alinear validación de audience común en Users API en `MiKompri.Users.Api/Program.cs`
- [X] T024 Implementar autorización de listas personales por identidad autenticada en `MiKompri.ShoppingList.Application/Queries/GetShoppingListsByOwner/GetShoppingListsByOwnerQueryHandler.cs`
- [X] T025 Asegurar derivación de `OwnerId` desde identidad autenticada en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`
- [X] T026 Configurar autenticación JWT real en ProductCatalog API en `MiKompri.ProductCatalog.Api/Program.cs`
- [X] T027 Alinear audience común y validaciones JWT en ShoppingList API en `MiKompri.ShoppingList.Api/Program.cs`
- [X] T028 Configurar CORS explícito para SPA local/cloud en `MiKompri.Users.Api/Program.cs`
- [X] T029 Configurar CORS explícito para SPA local/cloud en `MiKompri.ShoppingList.Api/Program.cs`
- [X] T030 Configurar CORS explícito para SPA local/cloud en `MiKompri.ProductCatalog.Api/Program.cs`

### Backend hardening adicional (bloqueante TP11)

- [X] T095 [P] Agregar pruebas de audience inválida/ válida en ShoppingList API en `test/MiKompri.ShoppingList.Api.Tests/Security/CommonAudienceValidationTests.cs`
- [X] T096 [P] Agregar pruebas de audience inválida/ válida en ProductCatalog API en `test/MiKompri.ProductCatalog.Api.Tests/Security/CommonAudienceValidationTests.cs`
- [X] T097 [P] Agregar pruebas del middleware de ShoppingList para flujo canónico TP11 (`tid`,`oid`) y rechazo seguro sin correlación en `test/MiKompri.ShoppingList.Api.Tests/Middleware/UserProvisioningMiddlewareTests.cs`
- [X] T098 Adaptar middleware de ShoppingList para no depender exclusivamente de `sub` y alinearlo al flujo canónico TP11 en `MiKompri.ShoppingList.Api/Middleware/UserProvisioningMiddleware.cs`
- [X] T099 Ajustar resolución de identidad en adaptador de Users para ShoppingList conforme a correlación canónica en `MiKompri.ShoppingList.Infrastructure/Services/UsersGroupAuthorizationAdapter.cs`

### Frontend foundation (arquitectura transversal)

- [X] T031 Crear proveedores globales (MSAL, Router, QueryClient, Theme) en `MiKompri.Web/src/app/providers/AppProviders.tsx`
- [X] T032 Configurar MSAL Browser/React y bootstrap de sesión en `MiKompri.Web/src/auth/msal/msalConfig.ts`
- [X] T033 Implementar guard de rutas protegidas en `MiKompri.Web/src/auth/guards/ProtectedRoute.tsx`
- [X] T034 Implementar cliente API común con propagación Bearer token en `MiKompri.Web/src/api/client/httpClient.ts`
- [X] T035 Implementar mapeo centralizado de errores HTTP 400/401/403/404/409/503 en `MiKompri.Web/src/api/errors/httpErrorMapper.ts`
- [X] T036 Configurar QueryClient y políticas base de TanStack Query en `MiKompri.Web/src/app/providers/queryClient.ts`
- [X] T037 Crear router principal de la SPA en `MiKompri.Web/src/app/router/index.tsx`
- [X] T038 Configurar Fluent UI Theme global en `MiKompri.Web/src/shared/ui/theme.ts`
- [X] T039 Configurar lectura de variables de entorno por ambiente en `MiKompri.Web/src/shared/config/env.ts`

**Checkpoint**: Prerrequisitos backend y fundaciones frontend listos para historias.

---

## Phase 3: User Story 1 - Acceso seguro y sesión + perfil (Priority: P1) 🎯 MVP

**Goal**: Login/logout, restauración de sesión, rutas protegidas y perfil autenticado.

**Independent Test**: Usuario inicia sesión con Entra, accede a rutas protegidas, recarga y conserva sesión, cierra sesión y pierde acceso.

### Tests for User Story 1

- [ ] T040 [P] [US1] Crear pruebas unitarias de sesión/auth helpers en `MiKompri.Web/tests/unit/auth/session.spec.ts`
- [ ] T041 [P] [US1] Crear pruebas de componente de `ProtectedRoute` en `MiKompri.Web/tests/component/auth/ProtectedRoute.spec.tsx`
- [ ] T042 [P] [US1] Crear pruebas de integración MSW para `/users/me` y restauración de sesión en `MiKompri.Web/tests/integration/auth/session.integration.spec.tsx`
- [ ] T103 [US1] Crear prueba de integración para expiración de token durante operación de escritura y reautenticación guiada en `MiKompri.Web/tests/integration/auth/token-expiration-write.integration.spec.tsx`
- [ ] T043 [US1] Crear prueba E2E de login/logout/restore en `MiKompri.Web/tests/e2e/auth/auth-session.spec.ts`

### Implementation for User Story 1

- [ ] T044 [P] [US1] Implementar flujo de login/logout en `MiKompri.Web/src/auth/session/authSessionService.ts`
- [ ] T045 [P] [US1] Implementar pantalla de login y callback en `MiKompri.Web/src/features/profile/pages/LoginPage.tsx`
- [ ] T046 [US1] Implementar consulta de perfil autenticado con TanStack Query en `MiKompri.Web/src/features/profile/api/profileApi.ts`
- [ ] T047 [US1] Implementar vista de perfil con estados loading/error/empty/unauthorized en `MiKompri.Web/src/features/profile/pages/ProfilePage.tsx`
- [ ] T048 [US1] Registrar rutas protegidas de perfil en `MiKompri.Web/src/app/router/routes.tsx`

**Checkpoint**: US1 funcional y testeable independientemente.

---

## Phase 4: User Story 2 - Gestión básica de grupos + listas personales/compartidas + ítems (Priority: P1)

**Goal**: Operación completa de grupos mínimos y listas (personal/shared) con ítems.

**Independent Test**: Usuario crea grupo, gestiona listas personales/compartidas e ítems; no miembro recibe 403 en listas de grupo.

### Tests for User Story 2

- [ ] T049 [P] [US2] Crear pruebas unitarias de validación de formularios (RHF + Zod) en `MiKompri.Web/tests/unit/features/groups/groupSchemas.spec.ts`
- [ ] T050 [P] [US2] Crear pruebas de componente de lista personal en `MiKompri.Web/tests/component/features/shopping-lists/PersonalListPage.spec.tsx`
- [ ] T051 [P] [US2] Crear pruebas de componente de lista compartida en `MiKompri.Web/tests/component/features/shared-lists/SharedListPage.spec.tsx`
- [ ] T052 [P] [US2] Crear pruebas MSW para grupos/listas/ítems y errores 400/403/409 en `MiKompri.Web/tests/integration/features/lists-and-groups.integration.spec.tsx`
- [ ] T053 [US2] Crear prueba E2E de CRUD de lista personal e ítems en `MiKompri.Web/tests/e2e/lists/personal-lists.spec.ts`
- [ ] T054 [US2] Crear prueba E2E de acceso autorizado/no autorizado a lista compartida en `MiKompri.Web/tests/e2e/lists/shared-lists-authorization.spec.ts`

### Implementation for User Story 2

- [ ] T055 [P] [US2] Implementar API de grupos (crear/listar/membresías/invitar) en `MiKompri.Web/src/features/groups/api/groupsApi.ts`
- [ ] T056 [P] [US2] Implementar páginas de grupos y membresías en `MiKompri.Web/src/features/groups/pages/GroupsPage.tsx`
- [ ] T057 [P] [US2] Implementar API de listas personales e ítems en `MiKompri.Web/src/features/shopping-lists/api/shoppingListsApi.ts`
- [ ] T058 [P] [US2] Implementar API de listas compartidas e ítems en `MiKompri.Web/src/features/shared-lists/api/sharedListsApi.ts`
- [ ] T059 [US2] Implementar página de listas personales con formularios RHF+Zod en `MiKompri.Web/src/features/shopping-lists/pages/PersonalListsPage.tsx`
- [ ] T060 [US2] Implementar página de listas compartidas con validación de estados 403 en `MiKompri.Web/src/features/shared-lists/pages/SharedListsPage.tsx`
- [ ] T061 [US2] Implementar componentes de ítems y acciones CRUD en `MiKompri.Web/src/features/shopping-lists/components/ListItemsPanel.tsx`
- [ ] T062 [US2] Registrar rutas de grupos/listas en `MiKompri.Web/src/app/router/routes.tsx`

**Checkpoint**: US2 funcional e independiente, manteniendo US1 estable.

---

## Phase 5: User Story 3 - Gastos compartidos, balances y settlement (Priority: P2)

**Goal**: Registrar gastos por ítem y visualizar balances/settlement en listas compartidas.

**Independent Test**: Miembro válido registra gasto, consulta balances y settlement; usuario sin permiso recibe 403.

### Tests for User Story 3

- [ ] T063 [P] [US3] Crear pruebas unitarias de reglas de formulario de gastos en `MiKompri.Web/tests/unit/features/settlements/expenseSchemas.spec.ts`
- [ ] T064 [P] [US3] Crear pruebas de componente de balances/settlement en `MiKompri.Web/tests/component/features/settlements/SettlementPanel.spec.tsx`
- [ ] T065 [P] [US3] Crear pruebas MSW de gastos, balances y settlement (incluye 503) en `MiKompri.Web/tests/integration/features/settlements.integration.spec.tsx`
- [ ] T066 [US3] Crear prueba E2E de flujo registrar gasto + consultar settlement en `MiKompri.Web/tests/e2e/settlements/settlement-flow.spec.ts`
- [ ] T102 [US3] Crear prueba E2E de acceso no autorizado (403) a settlement para miembro sin permisos en `MiKompri.Web/tests/e2e/settlements/settlement-authorization.spec.ts`

### Implementation for User Story 3

- [ ] T067 [P] [US3] Implementar API de gastos compartidos en `MiKompri.Web/src/features/settlements/api/expensesApi.ts`
- [ ] T068 [P] [US3] Implementar API de balances/settlement en `MiKompri.Web/src/features/settlements/api/settlementApi.ts`
- [ ] T069 [US3] Implementar formulario de gasto compartido con RHF+Zod en `MiKompri.Web/src/features/settlements/components/ExpenseForm.tsx`
- [ ] T070 [US3] Implementar vista de balances y transferencias propuestas en `MiKompri.Web/src/features/settlements/pages/SettlementPage.tsx`
- [ ] T071 [US3] Integrar settlement en navegación de listas compartidas en `MiKompri.Web/src/features/shared-lists/pages/SharedListsPage.tsx`

**Checkpoint**: US3 funcional e independiente, sin romper US1/US2.

---

## Phase 6: User Story 4 - ProductCatalog read-only (Priority: P2)

**Goal**: Consulta de productos, mercados e historial de precios en modo solo lectura.

**Independent Test**: Usuario autenticado consulta catálogo e historial; la UI maneja empty y 503.

### Tests for User Story 4

- [ ] T072 [P] [US4] Crear pruebas unitarias de transformadores de catálogo en `MiKompri.Web/tests/unit/features/product-catalog/catalogMappers.spec.ts`
- [ ] T073 [P] [US4] Crear pruebas de componente de listado de catálogo en `MiKompri.Web/tests/component/features/product-catalog/ProductCatalogPage.spec.tsx`
- [ ] T074 [P] [US4] Crear pruebas MSW para catálogo read-only e historial (empty/503) en `MiKompri.Web/tests/integration/features/product-catalog.integration.spec.tsx`
- [ ] T075 [US4] Crear prueba E2E de navegación y consulta de ProductCatalog en `MiKompri.Web/tests/e2e/product-catalog/product-catalog-readonly.spec.ts`

### Implementation for User Story 4

- [ ] T076 [P] [US4] Implementar API read-only de ProductCatalog en `MiKompri.Web/src/features/product-catalog/api/productCatalogApi.ts`
- [ ] T077 [US4] Implementar página read-only de productos/mercados en `MiKompri.Web/src/features/product-catalog/pages/ProductCatalogPage.tsx`
- [ ] T078 [US4] Implementar vista de historial de precios por producto/mercado en `MiKompri.Web/src/features/product-catalog/components/PriceHistoryPanel.tsx`
- [ ] T079 [US4] Registrar ruta de ProductCatalog en `MiKompri.Web/src/app/router/routes.tsx`

**Checkpoint**: US4 funcional e independiente.

---

## Phase 7: User Story 5 - Responsive, accesibilidad y estados UX globales (Priority: P3)

**Goal**: Consolidar diseño responsive, accesibilidad básica y estados globales de carga/error/autorización.

**Independent Test**: Flujos P1 operables en mobile/tablet/desktop con navegación por teclado y feedback consistente de estados.

### Tests for User Story 5

- [ ] T080 [P] [US5] Crear pruebas de componente para estados UI globales en `MiKompri.Web/tests/component/shared/UiStateViews.spec.tsx`
- [ ] T081 [P] [US5] Crear pruebas de accesibilidad básica en componentes clave con RTL en `MiKompri.Web/tests/component/accessibility/accessibility-smoke.spec.tsx`
- [ ] T082 [P] [US5] Crear pruebas MSW de mapeo de errores HTTP 401/403/503 en `MiKompri.Web/tests/integration/shared/http-error-states.integration.spec.tsx`
- [ ] T083 [US5] Crear prueba E2E responsive (mobile/tablet/desktop) de flujos P1 en `MiKompri.Web/tests/e2e/responsive/responsive-smoke.spec.ts`

### Implementation for User Story 5

- [ ] T084 [P] [US5] Implementar componentes globales `LoadingState`, `EmptyState`, `ErrorState`, `UnauthorizedState`, `ForbiddenState`, `ServiceUnavailableState` en `MiKompri.Web/src/shared/ui/states/`
- [ ] T085 [P] [US5] Implementar utilidades de accesibilidad (focus management, nombres accesibles) en `MiKompri.Web/src/shared/accessibility/a11y.ts`
- [ ] T086 [US5] Aplicar layout responsive base en `MiKompri.Web/src/app/layout/MainLayout.tsx`
- [ ] T087 [US5] Integrar estados UX globales en páginas de features en `MiKompri.Web/src/shared/ui/states/withViewState.tsx`
- [ ] T088 [US5] Ajustar tema Fluent UI para contraste y foco visible en `MiKompri.Web/src/shared/ui/theme.ts`

**Checkpoint**: US5 funcional e independiente.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Cierre transversal de calidad, CI y preparación de despliegue.

- [ ] T089 [P] Configurar workflow CI frontend (install + lint + typecheck + tests unit/component/integration + e2e smoke + build + `docker build`) en `.github/workflows/ci-mikompri-web.yml`
- [ ] T104 [P] Crear Dockerfile del frontend para build reproducible en CI en `MiKompri.Web/Dockerfile`
- [ ] T090 [P] Configurar estrategia de preparación Azure Static Web Apps (sin despliegue) en `MiKompri.Web/staticwebapp.config.json`
- [ ] T091 [P] Documentar variables de entorno local/cloud y redirect URIs en `MiKompri.Web/.env.example`
- [ ] T092 [P] Agregar guía de CORS para frontend y APIs en `docs/web-client-cors.md`
- [ ] T093 Ejecutar regresión API de ShoppingList MVP-0/MVP-3 con `dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release` y registrar resultado en `specs/007-web-react-client/quickstart.md`
- [ ] T094 Actualizar quickstart de validación cruzada frontend/backend en `specs/007-web-react-client/quickstart.md`
- [ ] T100 Ejecutar regresión API de Users MVP-1 con `dotnet test test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj --configuration Release` y registrar resultado en `specs/007-web-react-client/quickstart.md`
- [ ] T101 Ejecutar regresión API de ProductCatalog MVP-2 con `dotnet test test/MiKompri.ProductCatalog.Api.Tests/MiKompri.ProductCatalog.Api.Tests.csproj --configuration Release` y registrar resultado en `specs/007-web-react-client/quickstart.md`
- [ ] T105 [P] Documentar matriz de alineación de contratos/identidad reutilizable para clientes Web/MAUI/iOS en `docs/client-contract-reuse-guidelines.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: inicia inmediatamente.
- **Phase 2 (Foundational)**: depende de Phase 1 y bloquea todas las historias.
- **Phases 3-7 (User Stories)**: dependen de Phase 2; se ejecutan por prioridad (P1 → P2 → P3).
- **Phase 8 (Polish)**: depende de completar historias requeridas.

### User Story Dependencies

- **US1 (P1)**: depende solo de Foundational.
- **US2 (P1)**: depende de Foundational; puede avanzar en paralelo con cierre de US1 si hay equipo separado.
- **US3 (P2)**: depende de US2 (requiere listas compartidas e ítems operativos).
- **US4 (P2)**: depende solo de Foundational (read-only desacoplado de settlement).
- **US5 (P3)**: depende de US1-US4 para aplicar ajustes transversales finales.

### Backend Prerequisite Dependencies

- `sub -> (tid, oid)` + `ExternalUserId` nullable + audience común deben completarse antes de cerrar US1.
- personal ShoppingList auth debe completarse antes de cerrar US2.
- ProductCatalog JWT debe completarse antes de cerrar US4.

## Parallel Execution Examples

### US1

- Ejecutar en paralelo `T040`, `T041`, `T042` mientras se implementa `T044` y `T045`.

### US2

- Ejecutar en paralelo `T055`, `T057`, `T058` (APIs por feature) y `T050`, `T051`, `T052` (tests).

### US3

- Ejecutar en paralelo `T067` y `T068` antes de consolidar `T069` y `T070`.

### US4

- Ejecutar en paralelo `T072`, `T073`, `T074` junto con `T076`.

### US5

- Ejecutar en paralelo `T080`, `T081`, `T082` junto con `T084` y `T085`.

## Implementation Strategy

### MVP First (US1 + US2)

1. Completar Setup + Foundational (incluye prerrequisitos backend obligatorios).
2. Entregar US1 (auth/sesión/perfil) como primer incremento funcional.
3. Entregar US2 (grupos + listas + ítems) para cerrar el núcleo de uso diario.

### Incremental Delivery

4. Agregar US3 (gastos/balances/settlement).
5. Agregar US4 (ProductCatalog read-only).
6. Cerrar con US5 (responsive/accesibilidad/estados UX) y Phase 8.

### Quality Gates

- Todas las tareas de test de cada historia completadas y en verde.
- Regresión backend MVP-0 a MVP-3 ejecutada antes de finalizar.
- CI frontend activo y estable antes de preparar despliegue futuro SWA.
