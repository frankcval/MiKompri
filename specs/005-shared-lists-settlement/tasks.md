---

description: "Lista de tareas para la implementación de MVP-3 Shared Lists & Settlement"

---

# Tasks: MVP-3 Shared Lists & Settlement

**Input**: Documentos de diseño desde `/specs/005-shared-lists-settlement/`

**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Incluye tareas explícitas de Domain, Application y API/Integration porque TP8 lo exige para esta feature.

**Organization**: Las tareas están agrupadas por user story para permitir implementación y validación independiente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias)
- **[Story]**: US1, US2, US3, US4
- Cada tarea incluye rutas de archivo exactas

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar la estructura de trabajo y la base transversal para MVP-3 dentro del bounded context ShoppingList.

- [X] T001 Crear carpetas base de feature en `MiKompri.ShoppingList.Application/Commands/SharedLists/`
- [X] T002 [P] Crear contratos de salida iniciales para shared lists en `MiKompri.ShoppingList.Application/DTOs/SharedPurchaseListDto.cs`
- [X] T003 [P] Crear modelos HTTP iniciales para shared lists en `MiKompri.ShoppingList.Api/Models/SharedLists/CreateSharedListRequest.cs`
- [X] T004 [P] Alinear documentación de spec/plan/contract/quickstart en `specs/005-shared-lists-settlement/spec.md`
- [X] T005 [P] Preparar estructura de pruebas para shared lists en `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base transversal obligatoria antes de cualquier historia de usuario.

**⚠️ CRITICAL**: No iniciar user stories hasta completar esta fase.

- [X] T006 [P] Extender el aggregate de lista para modo compartido y estado de ciclo de vida en `MiKompri.ShoppingList.Domain/Entities/PurchaseList.cs`
- [X] T007 [P] Añadir trazabilidad de actor colaborativo en ítems (`AddedBy`, `UpdatedBy`) en `MiKompri.ShoppingList.Domain/Entities/ListItem.cs`
- [X] T008 [P] Crear la abstracción de identidad local y autorización de grupo en `MiKompri.ShoppingList.Application/Interfaces/ICurrentUserService.cs`, `MiKompri.ShoppingList.Application/Interfaces/IGroupAuthorizationService.cs` y `MiKompri.ShoppingList.Application/Interfaces/GroupAuthorizationResult.cs`
- [X] T009 [P] Configurar JWT Bearer y extracción de identidad desde `sub` en `MiKompri.ShoppingList.Api/Program.cs`, `MiKompri.ShoppingList.Api/appsettings.json`, `MiKompri.ShoppingList.Api/appsettings.Development.json`, `MiKompri.ShoppingList.Api/Middleware/UserProvisioningMiddleware.cs` y `MiKompri.ShoppingList.Api/Services/HttpCurrentUserService.cs`
- [X] T010 [P] Implementar el adapter de infraestructura para consultar membresía/rol sin acceso directo a la base de datos de Users en `MiKompri.ShoppingList.Infrastructure/Services/UsersGroupAuthorizationAdapter.cs` y registrarlo en `MiKompri.ShoppingList.Infrastructure/InfrastructureDependencyInjection.cs`
- [X] T011 [P] Registrar nuevas entidades compartidas y configuración base en `MiKompri.ShoppingList.Infrastructure/Persistence/ShoppingListDbContext.cs`
- [X] T012 Implementar configuraciones EF Core base para entidades compartidas en `MiKompri.ShoppingList.Infrastructure/Persistence/Configurations/SharedLists/SharedListItemConfiguration.cs`, `MiKompri.ShoppingList.Infrastructure/Persistence/Configurations/SharedLists/ItemExpenseRecordConfiguration.cs`, `MiKompri.ShoppingList.Infrastructure/Persistence/Configurations/SharedLists/ExpenseParticipantConfiguration.cs` y `MiKompri.ShoppingList.Infrastructure/Persistence/Configurations/SharedLists/SharedListAuditEventConfiguration.cs`
- [X] T013 Generar migración de infraestructura para shared lists y settlement en `MiKompri.ShoppingList.Infrastructure/Migrations/` y actualizar `MiKompri.ShoppingList.Infrastructure/Migrations/ShoppingListDbContextModelSnapshot.cs`

**Checkpoint**: Fundación lista; las historias ya pueden desarrollarse por prioridad.

---

## Phase 3: User Story 1 - Crear y operar listas compartidas por grupo (Priority: P1) 🎯 MVP

**Goal**: Permitir crear, consultar y operar listas compartidas asociadas a `GroupId`, manteniendo compatibilidad con listas personales.

**Independent Test**: Crear una shared list con miembro activo, consultarla con otro miembro, rechazar acceso de no miembro y verificar que endpoints personales existentes siguen funcionando.

### Tests for User Story 1

- [X] T014 [P] [US1] Crear pruebas de dominio para ciclo de vida de listas compartidas y compatibilidad con listas personales en `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/PurchaseListSharedModeTests.cs`
- [X] T015 [P] [US1] Crear pruebas de Application para comandos y queries de shared lists en `test/MiKompri.ShoppingList.Application.Tests/SharedLists/CreateSharedListCommandHandlerTests.cs`, `test/MiKompri.ShoppingList.Application.Tests/SharedLists/UpdateSharedListCommandHandlerTests.cs`, `test/MiKompri.ShoppingList.Application.Tests/SharedLists/CloseSharedListCommandHandlerTests.cs`, `test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSharedListByIdQueryHandlerTests.cs` y `test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSharedListsByGroupQueryHandlerTests.cs`
- [X] T016 [P] [US1] Crear pruebas API/integration para creación, consulta, acceso 401/403 y regresión de listas personales en `test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListsApiTests.cs`

### Implementation for User Story 1

- [X] T017 [P] [US1] Implementar comandos de ciclo de vida de shared list en `MiKompri.ShoppingList.Application/Commands/SharedLists/CreateSharedList/CreateSharedListCommand.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/UpdateSharedList/UpdateSharedListCommand.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/CloseSharedList/CloseSharedListCommand.cs`
- [X] T018 [P] [US1] Implementar validadores de shared list en `MiKompri.ShoppingList.Application/Commands/SharedLists/CreateSharedList/CreateSharedListCommandValidator.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/UpdateSharedList/UpdateSharedListCommandValidator.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/CloseSharedList/CloseSharedListCommandValidator.cs`
- [X] T019 [US1] Implementar handlers con validación de membresía/rol contra `IGroupAuthorizationService` en `MiKompri.ShoppingList.Application/Commands/SharedLists/CreateSharedList/CreateSharedListCommandHandler.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/UpdateSharedList/UpdateSharedListCommandHandler.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/CloseSharedList/CloseSharedListCommandHandler.cs`
- [X] T020 [P] [US1] Implementar queries de lectura de shared list en `MiKompri.ShoppingList.Application/Queries/SharedLists/GetSharedListById/GetSharedListByIdQuery.cs`, `MiKompri.ShoppingList.Application/Queries/SharedLists/GetSharedListsByGroup/GetSharedListsByGroupQuery.cs` y sus handlers
- [X] T021 [US1] Extender repositorio para carga/filtrado de shared lists en `MiKompri.ShoppingList.Infrastructure/Persistence/Repositories/PurchaseListRepository.cs` y `MiKompri.ShoppingList.Application/Interfaces/IPurchaseListRepository.cs`
- [X] T022 [US1] Exponer endpoints `/api/v1/shared-lists` y operaciones básicas en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`
- [X] T023 [US1] Mantener compatibilidad de listas personales ajustando mapeos DTO y filtros existentes en `MiKompri.ShoppingList.Application/DTOs/PurchaseListMappings.cs` y `MiKompri.ShoppingList.Application/Queries/GetAllShoppingLists/GetAllShoppingListsQueryHandler.cs`

**Checkpoint**: US1 funcional e independiente.

---

## Phase 4: User Story 2 - Registrar gastos colaborativos por ítem con participantes (Priority: P1)

**Goal**: Registrar `AddedBy`, gasto real, `PaidBy` obligatorio, `PurchasedBy` opcional y participantes activos por ítem.

**Independent Test**: Agregar ítem colaborativo con `AddedBy`, registrar gasto válido y rechazar gasto sin pagador, sin participantes activos, con duplicados o con usuario fuera del grupo.

### Tests for User Story 2

- [X] T024 [P] [US2] Crear pruebas de dominio para reglas de gasto, trazabilidad y participantes en `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ItemExpenseRecordTests.cs`, `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ExpenseParticipantTests.cs` y `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/SharedListAuditEventTests.cs`
- [X] T025 [P] [US2] Crear pruebas de Application para comandos de colaboración y gastos en `test/MiKompri.ShoppingList.Application.Tests/SharedLists/AddSharedItemCommandHandlerTests.cs`, `test/MiKompri.ShoppingList.Application.Tests/SharedLists/RegisterItemExpenseCommandHandlerTests.cs`, `test/MiKompri.ShoppingList.Application.Tests/SharedLists/UpdateItemExpenseCommandHandlerTests.cs` y `test/MiKompri.ShoppingList.Application.Tests/SharedLists/DeleteItemExpenseCommandHandlerTests.cs`
- [X] T026 [P] [US2] Crear pruebas API/integration para gasto colaborativo, validaciones, autorización y casos 401/403 en `test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListExpensesApiTests.cs`

### Implementation for User Story 2

- [X] T027 [P] [US2] Crear entidades de gasto colaborativo en `MiKompri.ShoppingList.Domain/Entities/ItemExpenseRecord.cs` y `MiKompri.ShoppingList.Domain/Entities/ExpenseParticipant.cs`
- [X] T028 [P] [US2] Crear entidad de trazabilidad de operaciones en `MiKompri.ShoppingList.Domain/Entities/SharedListAuditEvent.cs`
- [X] T029 [US2] Implementar invariantes de gasto (`PaidBy` obligatorio, mínimo 1 participante activo, sin duplicados, `RealPaidPrice > 0`) en `MiKompri.ShoppingList.Domain/Entities/ItemExpenseRecord.cs`
- [X] T030 [P] [US2] Implementar comandos de colaboración y gastos en `MiKompri.ShoppingList.Application/Commands/SharedLists/AddSharedItem/AddSharedItemCommand.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/RegisterItemExpense/RegisterItemExpenseCommand.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/UpdateItemExpense/UpdateItemExpenseCommand.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/DeleteItemExpense/DeleteItemExpenseCommand.cs`
- [X] T031 [P] [US2] Implementar validadores de comandos de gastos en `MiKompri.ShoppingList.Application/Commands/SharedLists/RegisterItemExpense/RegisterItemExpenseCommandValidator.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/UpdateItemExpense/UpdateItemExpenseCommandValidator.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/DeleteItemExpense/DeleteItemExpenseCommandValidator.cs`
- [X] T032 [US2] Implementar handlers de gastos con control de permisos Owner/Admin/Member en `MiKompri.ShoppingList.Application/Commands/SharedLists/RegisterItemExpense/RegisterItemExpenseCommandHandler.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/UpdateItemExpense/UpdateItemExpenseCommandHandler.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/DeleteItemExpense/DeleteItemExpenseCommandHandler.cs`
- [X] T033 [US2] Persistir gastos, participantes y auditoría en `MiKompri.ShoppingList.Infrastructure/Persistence/ShoppingListDbContext.cs` y `MiKompri.ShoppingList.Infrastructure/Persistence/Configurations/SharedLists/SharedListAuditEventConfiguration.cs`
- [X] T034 [US2] Actualizar carga agregada del repositorio para incluir gastos/participantes en `MiKompri.ShoppingList.Infrastructure/Persistence/Repositories/PurchaseListRepository.cs`
- [X] T035 [US2] Exponer endpoints de ítems y gastos compartidos en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs` y `MiKompri.ShoppingList.Api/Models/SharedLists/RegisterExpenseRequest.cs`

**Checkpoint**: US2 funcional e independiente.

---

## Phase 5: User Story 3 - Calcular reparto, balances y liquidación (Priority: P2)

**Goal**: Calcular `TotalPagado`, `TotalCorrespondiente`, balances netos y propuesta determinista de liquidación.

**Independent Test**: Con múltiples gastos, consultar resumen y propuesta dos veces con mismo input y verificar resultados idénticos, redondeo correcto, conservación contable y el límite `D + A - 1` cuando aplique.

### Tests for User Story 3

- [X] T036 [P] [US3] Crear pruebas de dominio para cálculo de reparto, redondeo y settlement determinista en `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ExpenseSettlementCalculatorTests.cs`, `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ExpenseShareRoundingPolicyTests.cs` y `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/DeterministicSettlementProposalBuilderTests.cs`
- [X] T037 [P] [US3] Crear pruebas de Application para queries de settlement summary/proposal y sus validaciones en `test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSettlementSummaryQueryHandlerTests.cs` y `test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSettlementProposalQueryHandlerTests.cs`
- [X] T038 [P] [US3] Crear pruebas API/integration para resumen, propuesta y reproducibilidad del settlement en `test/MiKompri.ShoppingList.Api.Tests/SharedLists/SettlementApiTests.cs`

### Implementation for User Story 3

- [X] T039 [P] [US3] Implementar servicio de dominio de cálculo de reparto en `MiKompri.ShoppingList.Domain/Services/ExpenseSettlementCalculator.cs`
- [X] T040 [P] [US3] Implementar redondeo y asignación de residuo al pagador en `MiKompri.ShoppingList.Domain/Services/ExpenseShareRoundingPolicy.cs`
- [X] T041 [US3] Implementar algoritmo determinista de liquidación con límite verificable `D + A - 1` en `MiKompri.ShoppingList.Domain/Services/DeterministicSettlementProposalBuilder.cs`
- [X] T042 [P] [US3] Implementar queries de settlement summary/proposal en `MiKompri.ShoppingList.Application/Queries/SharedLists/GetSettlementSummary/GetSettlementSummaryQuery.cs`, `MiKompri.ShoppingList.Application/Queries/SharedLists/GetSettlementProposal/GetSettlementProposalQuery.cs` y handlers
- [X] T043 [US3] Implementar DTOs de balances y transferencias en `MiKompri.ShoppingList.Application/DTOs/SettlementSummaryDto.cs` y `MiKompri.ShoppingList.Application/DTOs/SettlementProposalDto.cs`
- [X] T044 [US3] Exponer endpoints `/api/v1/shared-lists/{id}/settlement/summary` y `/proposal` en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`

**Checkpoint**: US3 funcional e independiente.

---

## Phase 6: User Story 4 - Gestionar cambios de membresía con datos históricos (Priority: P2)

**Goal**: Conservar historial de operaciones y bloquear miembros inactivos/eliminados para operaciones nuevas.

**Independent Test**: Dar de baja miembro con operaciones previas, mantener consulta histórica y rechazar su uso en nuevos gastos.

### Tests for User Story 4

- [X] T045 [P] [US4] Crear pruebas de dominio para preservación de histórico y estados de membresía en `test/MiKompri.ShoppingList.Domain.Tests/SharedLists/SharedListHistoryTests.cs`
- [X] T046 [P] [US4] Crear pruebas de Application para política de membresía activa, auditoría y validaciones en `test/MiKompri.ShoppingList.Application.Tests/SharedLists/ActiveMembershipPolicyTests.cs` y `test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSharedListAuditEventsQueryHandlerTests.cs`
- [X] T047 [P] [US4] Crear pruebas API/integration para auditoría y rechazo de miembros inactivos en `test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListAuditApiTests.cs`

### Implementation for User Story 4

- [X] T048 [P] [US4] Implementar política de membresía activa para operaciones nuevas en `MiKompri.ShoppingList.Application/Services/ActiveMembershipPolicy.cs`
- [X] T049 [US4] Aplicar validación de membresía activa en comandos de ítems/gastos en `MiKompri.ShoppingList.Application/Commands/SharedLists/AddSharedItem/AddSharedItemCommandHandler.cs` y `MiKompri.ShoppingList.Application/Commands/SharedLists/RegisterItemExpense/RegisterItemExpenseCommandHandler.cs`
- [X] T050 [US4] Implementar consulta de auditoría de operaciones compartidas en `MiKompri.ShoppingList.Application/Queries/SharedLists/GetSharedListAuditEvents/GetSharedListAuditEventsQuery.cs` y handler
- [X] T051 [US4] Exponer endpoint de auditoría `GET /api/v1/shared-lists/{id}/audit-events` en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`
- [X] T052 [US4] Garantizar preservación de histórico ante baja de miembro en mapeos y consultas de settlement en `MiKompri.ShoppingList.Application/Queries/SharedLists/GetSettlementSummary/GetSettlementSummaryQueryHandler.cs`

**Checkpoint**: US4 funcional e independiente.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Cierre integral, hardening y validación transversal.

- [X] T053 [P] Crear pruebas específicas de autenticación/autorización runtime en `test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListAuthApiTests.cs` cubriendo JWT Bearer, claim `sub`, `401 Unauthorized` y `403 Forbidden` para operaciones compartidas
- [X] T054 [P] Asegurar regresión de listas personales existentes en `test/MiKompri.ShoppingList.Domain.Tests/PurchaseListTests.cs`, `test/MiKompri.ShoppingList.Application.Tests/CreateShoppingListCommandHandlerTests.cs`, `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs` y su `CustomWebApplicationFactory` correspondiente
- [X] T055 [P] Actualizar documentación funcional de feature en `specs/005-shared-lists-settlement/contracts/shared-lists-settlement-api.md` y `specs/005-shared-lists-settlement/quickstart.md` con nombres finales de payloads, protocolo de validación SC-003 y protocolo humano SC-006
- [X] T056 [P] Ajustar documentación Swagger y seguridad de endpoints compartidos en `MiKompri.ShoppingList.Api/Program.cs` y `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`
- [X] T057 Verificar no regresión de despliegue: `docker compose config`, `dotnet build MiKompri.sln --configuration Release --no-restore` y confirmación de que no se cambia la plataforma de despliegue ni el modelo CD
- [X] T058 Ejecutar regresión completa de ShoppingList, Users y ProductCatalog para validar que la Spec 005 no rompe contextos existentes: `test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj`, `test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj`, `test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj`, `test/MiKompri.Users.Domain.Tests/MiKompri.Users.Domain.Tests.csproj`, `test/MiKompri.Users.Application.Tests/MiKompri.Users.Application.Tests.csproj`, `test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj`, `test/MiKompri.ProductCatalog.Domain.Tests/MiKompri.ProductCatalog.Domain.Tests.csproj`, `test/MiKompri.ProductCatalog.Application.Tests/MiKompri.ProductCatalog.Application.Tests.csproj` y `test/MiKompri.ProductCatalog.Api.Tests/MiKompri.ProductCatalog.Api.Tests.csproj`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: inicia inmediatamente.
- **Phase 2 (Foundational)**: depende de Phase 1 y bloquea todas las user stories.
- **Phase 3 (US1)**: depende de Phase 2.
- **Phase 4 (US2)**: depende de Phase 2 y del baseline de shared lists de US1.
- **Phase 5 (US3)**: depende de US2 (requiere datos de gastos/participantes).
- **Phase 6 (US4)**: depende de US2 y se integra con consultas de US3.
- **Phase 7 (Polish)**: depende de historias objetivo completadas.

### User Story Dependencies

- **US1 (P1)**: primera entrega MVP; sin dependencia de otras historias.
- **US2 (P1)**: requiere shared lists operativas (US1).
- **US3 (P2)**: requiere datos de gastos (US2).
- **US4 (P2)**: requiere trazabilidad y gastos colaborativos (US2); consume cálculos de US3 para histórico.

### Parallel Opportunities

- En Setup: T002, T003, T004 y T005 pueden ejecutarse en paralelo.
- En Foundational: T006, T007, T008, T009, T010 y T011 pueden ejecutarse en paralelo.
- En US1: T014, T015, T016, T017, T018 y T020 pueden ejecutarse en paralelo.
- En US2: T024, T025, T026, T027, T028, T030, T031 pueden ejecutarse en paralelo.
- En US3: T036, T037, T038, T039, T040 y T042 pueden ejecutarse en paralelo.
- En US4: T045, T046, T047, T048 y T050 pueden ejecutarse en paralelo.
- En Polish: T053, T054, T055 y T056 pueden ejecutarse en paralelo.

---

## Parallel Example: User Story 1

```bash
Task: "T014 [US1] Crear pruebas de dominio para ciclo de vida de listas compartidas y compatibilidad con listas personales en test/MiKompri.ShoppingList.Domain.Tests/SharedLists/PurchaseListSharedModeTests.cs"
Task: "T015 [US1] Crear pruebas de Application para comandos y queries de shared lists en test/MiKompri.ShoppingList.Application.Tests/SharedLists/CreateSharedListCommandHandlerTests.cs, test/MiKompri.ShoppingList.Application.Tests/SharedLists/UpdateSharedListCommandHandlerTests.cs, test/MiKompri.ShoppingList.Application.Tests/SharedLists/CloseSharedListCommandHandlerTests.cs, test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSharedListByIdQueryHandlerTests.cs y test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSharedListsByGroupQueryHandlerTests.cs"
Task: "T016 [US1] Crear pruebas API/integration para creación, consulta, acceso 401/403 y regresión de listas personales en test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListsApiTests.cs"
```

## Parallel Example: User Story 2

```bash
Task: "T024 [US2] Crear pruebas de dominio para reglas de gasto, trazabilidad y participantes en test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ItemExpenseRecordTests.cs, test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ExpenseParticipantTests.cs y test/MiKompri.ShoppingList.Domain.Tests/SharedLists/SharedListAuditEventTests.cs"
Task: "T025 [US2] Crear pruebas de Application para comandos de colaboración y gastos en test/MiKompri.ShoppingList.Application.Tests/SharedLists/AddSharedItemCommandHandlerTests.cs, test/MiKompri.ShoppingList.Application.Tests/SharedLists/RegisterItemExpenseCommandHandlerTests.cs, test/MiKompri.ShoppingList.Application.Tests/SharedLists/UpdateItemExpenseCommandHandlerTests.cs y test/MiKompri.ShoppingList.Application.Tests/SharedLists/DeleteItemExpenseCommandHandlerTests.cs"
Task: "T026 [US2] Crear pruebas API/integration para gasto colaborativo, validaciones, autorización y casos 401/403 en test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListExpensesApiTests.cs"
```

## Parallel Example: User Story 3

```bash
Task: "T036 [US3] Crear pruebas de dominio para cálculo de reparto, redondeo y settlement determinista en test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ExpenseSettlementCalculatorTests.cs, test/MiKompri.ShoppingList.Domain.Tests/SharedLists/ExpenseShareRoundingPolicyTests.cs y test/MiKompri.ShoppingList.Domain.Tests/SharedLists/DeterministicSettlementProposalBuilderTests.cs"
Task: "T037 [US3] Crear pruebas de Application para queries de settlement summary/proposal y sus validaciones en test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSettlementSummaryQueryHandlerTests.cs y test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSettlementProposalQueryHandlerTests.cs"
Task: "T038 [US3] Crear pruebas API/integration para resumen, propuesta y reproducibilidad del settlement en test/MiKompri.ShoppingList.Api.Tests/SharedLists/SettlementApiTests.cs"
```

## Parallel Example: User Story 4

```bash
Task: "T045 [US4] Crear pruebas de dominio para preservación de histórico y estados de membresía en test/MiKompri.ShoppingList.Domain.Tests/SharedLists/SharedListHistoryTests.cs"
Task: "T046 [US4] Crear pruebas de Application para política de membresía activa, auditoría y validaciones en test/MiKompri.ShoppingList.Application.Tests/SharedLists/ActiveMembershipPolicyTests.cs y test/MiKompri.ShoppingList.Application.Tests/SharedLists/GetSharedListAuditEventsQueryHandlerTests.cs"
Task: "T047 [US4] Crear pruebas API/integration para auditoría y rechazo de miembros inactivos en test/MiKompri.ShoppingList.Api.Tests/SharedLists/SharedListAuditApiTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Setup.
2. Completar Phase 2: Foundational.
3. Completar Phase 3: US1.
4. Validar el test independiente de US1 (acceso por membresía + compatibilidad de listas personales).
5. Hacer demo interna del MVP colaborativo básico.

### Incremental Delivery

1. Setup + Foundational → base lista.
2. Añadir US1 → validar y estabilizar.
3. Añadir US2 → validar registro de gasto y permisos.
4. Añadir US3 → validar balances y liquidación determinista.
5. Añadir US4 → validar histórico con baja de miembros.

### Parallel Team Strategy

1. Equipo A/B completa Setup + Foundational.
2. Después:
   - Dev A: US1
   - Dev B: US2
   - Dev C: US3
   - Dev D: US4 (cuando US2 esté habilitada)
3. Cierre conjunto en Phase 7.

---

## Notes

- `[P]` indica tareas sin conflicto de archivo y sin dependencia directa.
- `[US#]` asegura trazabilidad por historia de usuario.
- Mantener separación de bounded contexts: ninguna tarea debe introducir acceso directo de ShoppingList a la BD de Users.
- Las tareas están orientadas a ejecución inmediata por LLM/equipo sin contexto adicional.
