# Tareas: Shopping List Core Hardening

**Entrada**: Documentos de diseño en `/specs/002-shopping-list-core/`

**Prerrequisitos**: plan.md (requerido), spec.md (requerido para historias de usuario), research.md, data-model.md, contracts/, quickstart.md

**Pruebas**: Incluidas porque la spec exige escenarios de aceptación, errores esperados y criterios de done verificables.

**Organización**: Las tareas se agrupan por historia de usuario para permitir implementación, validación y entrega incremental.

## Fase 1: Preparación (Infraestructura Compartida)

**Propósito**: Alinear alcance, contratos y cobertura inicial de la fase de hardening.

- [x] T001 Validar alineación de `spec.md`, `plan.md`, `research.md`, `data-model.md` y `contracts/shoppinglist-core-api.md` en `specs/002-shopping-list-core/`
- [x] T002 Crear sección de seguimiento de ejecución del MVP en `specs/002-shopping-list-core/quickstart.md`
- [x] T003 [P] Definir matriz de cobertura FR/RB/ERR/CD en `specs/002-shopping-list-core/quickstart.md`

---

## Fase 2: Fundacional (Prerrequisitos Bloqueantes)

**Propósito**: Dejar listas las bases compartidas antes de implementar historias.

**⚠️ CRÍTICO**: No iniciar historias de usuario hasta cerrar esta fase.

- [x] T004 Revisar y ajustar validaciones base en `MiKompri.ShoppingList.Application/Behavior/ValidationBehavior.cs`
- [x] T005 [P] Revisar mapeo de errores de negocio y validación en `MiKompri.ShoppingList.Api/Middleware/ExceptionHandlingMiddleware.cs`
- [x] T006 [P] Confirmar que `PurchaseListRepository` carga agregado completo para mutaciones en `MiKompri.ShoppingList.Infrastructure/Persistence/Repositories/PurchaseListRepository.cs`
- [x] T007 Revisar consistencia de persistencia con `IUnitOfWork` en `MiKompri.ShoppingList.Infrastructure/Persistence/Repositories/UnitOfWork.cs`
- [x] T008 [P] Añadir/actualizar pruebas base de middleware y errores en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`

**Punto de control**: Reglas transversales de validación/errores/persistencia listas para historias US1-US4.

---

## Fase 3: User Story 1 - Crear y consultar lista de compra (Prioridad: P1) 🎯 MVP

**Objetivo**: Permitir crear una lista válida y consultarla con sus datos principales.

**Prueba Independiente**: Crear lista y consultarla por id, verificando nombre, estado inicial y trazabilidad básica.

### Pruebas (primero)

- [x] T009 [P] [US1] Añadir caso de dominio para nombre inválido en `test/MiKompri.ShoppingList.Domain.Tests/PurchaseListTests.cs`
- [x] T010 [P] [US1] Añadir casos de aplicación para crear lista válida/ inválida en `test/MiKompri.ShoppingList.Application.Tests/CreateShoppingListCommandHandlerTests.cs`
- [x] T011 [P] [US1] Añadir caso de integración API crear + consultar lista en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`

### Implementación

- [x] T012 [US1] Ajustar reglas de creación de lista en `MiKompri.ShoppingList.Domain/Entities/PurchaseList.cs`
- [x] T013 [US1] Ajustar validación de comando en `MiKompri.ShoppingList.Application/Commands/CreateShoppingList/CreateShoppingListValidator.cs`
- [x] T014 [US1] Ajustar handler de creación en `MiKompri.ShoppingList.Application/Commands/CreateShoppingList/CreateShoppingListCommandHandler.cs`
- [x] T015 [US1] Ajustar query de detalle de lista en `MiKompri.ShoppingList.Application/Queries/GetShoppingListById/GetShoppingListByIdHandler.cs`
- [x] T016 [US1] Ajustar endpoint de creación/consulta en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`

**Punto de control**: US1 funcional y validable sin depender de US2-US4.

---

## Fase 4: User Story 2 - Gestionar ítems de la lista (Prioridad: P1)

**Objetivo**: Agregar, editar, marcar como comprado y eliminar ítems de una lista.

**Prueba Independiente**: Flujo completo add → update → mark purchased → delete sobre una lista existente.

### Pruebas (primero)

- [x] T017 [P] [US2] Añadir casos de dominio de ítems (duplicado, editar, eliminar, comprado) en `test/MiKompri.ShoppingList.Domain.Tests/PurchaseListTests.cs`
- [x] T018 [P] [US2] Añadir casos de aplicación para `AddItemCommand` en `test/MiKompri.ShoppingList.Application.Tests/AddItemCommandHandlerTests.cs`
- [x] T019 [P] [US2] Añadir casos de aplicación para `UpdateItemShoopingListCommand` en `test/MiKompri.ShoppingList.Application.Tests/UpdateItemShoopingListCommandHandlerTests.cs`
- [x] T020 [P] [US2] Añadir casos de aplicación para `MarkItemAsPurchasedCommand` en `test/MiKompri.ShoppingList.Application.Tests/MarkItemAsPurchasedCommandHandlerTests.cs`
- [x] T021 [P] [US2] Añadir casos de aplicación para `DeleteItemShoppingListCommand` en `test/MiKompri.ShoppingList.Application.Tests/DeleteItemShoppingListCommandHandlerTests.cs`
- [x] T022 [P] [US2] Añadir integración API de ciclo de vida de ítem en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`

### Implementación

- [x] T023 [US2] Ajustar reglas de ítems y duplicidad por producto en `MiKompri.ShoppingList.Domain/Entities/PurchaseList.cs`
- [x] T024 [US2] Ajustar entidad de ítem para datos editables y estado en `MiKompri.ShoppingList.Domain/Entities/ListItem.cs`
- [x] T025 [US2] Ajustar validación/handler de alta de ítem en `MiKompri.ShoppingList.Application/Commands/AddItemToList/AddItemCommandValidator.cs` y `AddItemCommandHandler.cs`
- [x] T026 [US2] Ajustar validación/handler de edición en `MiKompri.ShoppingList.Application/Commands/UpdateItemShoppingList/UpdateShoppingListValidator.cs` y `UpdateItemShoopingListCommandHandler.cs`
- [x] T027 [US2] Ajustar validación/handler de marcado comprado en `MiKompri.ShoppingList.Application/Commands/MarkItemAsPurchased/MarkItemAsPurchasedValidator.cs` y `MarkItemAsPurchasedCommandHandler.cs`
- [x] T028 [US2] Ajustar validación/handler de eliminación en `MiKompri.ShoppingList.Application/Commands/DeleteItemShoppingList/DeleteItemShoppingListValidator.cs` y `DeleteItemShoppingListCommandHandler.cs`
- [x] T029 [US2] Ajustar endpoints de ítems en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`

**Punto de control**: US2 funcional y verificable con errores esperados de lista/ítem no encontrado y duplicados.

---

## Fase 5: User Story 3 - Consultar progreso de la lista (Prioridad: P2)

**Objetivo**: Exponer progreso consistente (total, comprados, pendientes, porcentaje).

**Prueba Independiente**: Consultar lista vacía y lista con ítems mixtos, verificando cálculos.

### Pruebas (primero)

- [x] T030 [P] [US3] Añadir/ajustar pruebas de cálculo de progreso en `test/MiKompri.ShoppingList.Domain.Tests/ListProgressTests.cs`
- [x] T031 [P] [US3] Añadir prueba de aplicación para detalle con progreso en `test/MiKompri.ShoppingList.Application.Tests/GetShoppingListsByOwnerQueryHandlerTests.cs`
- [x] T032 [P] [US3] Añadir integración API de progreso (lista vacía y con datos) en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`

### Implementación

- [x] T033 [US3] Ajustar reglas de progreso en `MiKompri.ShoppingList.Domain/ValueObjects/ListProgress.cs`
- [x] T034 [US3] Ajustar mapeo de progreso en DTOs en `MiKompri.ShoppingList.Application/DTOs/PurchaseListDTO.cs`
- [x] T035 [US3] Ajustar queries de lectura para incluir progreso consistente en `MiKompri.ShoppingList.Application/Queries/GetShoppingListById/GetShoppingListByIdHandler.cs`

**Punto de control**: US3 entrega progreso correcto e independiente de trazabilidad avanzada.

---

## Fase 6: User Story 4 - Ver trazabilidad básica (Prioridad: P2)

**Objetivo**: Mostrar creación y última modificación en lista e ítems.

**Prueba Independiente**: Crear y modificar recursos, luego consultar timestamps de creación/actualización.

### Pruebas (primero)

- [x] T036 [P] [US4] Añadir casos de dominio de actualización de timestamps en `test/MiKompri.ShoppingList.Domain.Tests/PurchaseListTests.cs`
- [x] T037 [P] [US4] Añadir casos de aplicación para trazabilidad en `test/MiKompri.ShoppingList.Application.Tests/UpdateShoppingListCommandHandlerTests.cs`
- [x] T038 [P] [US4] Añadir integración API para trazabilidad en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`

### Implementación

- [x] T039 [US4] Ajustar campos de trazabilidad y mutaciones en `MiKompri.ShoppingList.Domain/Entities/PurchaseList.cs`
- [x] T040 [US4] Ajustar trazabilidad de ítem en `MiKompri.ShoppingList.Domain/Entities/ListItem.cs`
- [x] T041 [US4] Ajustar mapeo de trazabilidad en DTOs en `MiKompri.ShoppingList.Application/DTOs/ListItemDto.cs` y `PurchaseListDTO.cs`
- [x] T042 [US4] Ajustar respuesta de consulta para exponer trazabilidad en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`

**Punto de control**: US4 funcional y verificable de forma independiente.

---

## Fase 7: Pulido y Cierre Transversal

**Propósito**: Cerrar cumplimiento de errores esperados, criterios de done y validación final.

- [x] T043 [P] Verificar mapeo completo ERR-001..ERR-005 en `specs/002-shopping-list-core/spec.md` y `specs/002-shopping-list-core/contracts/shoppinglist-core-api.md`
- [x] T044 [P] Actualizar pasos de validación final en `specs/002-shopping-list-core/quickstart.md`
- [x] T045 Ejecutar pruebas del bounded context ShoppingList y registrar salida en `specs/002-shopping-list-core/quickstart.md`
- [x] T046 Ejecutar build de solución y registrar resultado de cierre del MVP en `specs/002-shopping-list-core/quickstart.md`

---

## Dependencias y Orden de Ejecución

### Dependencias entre Fases

- **Fase 1 (Preparación)**: sin dependencias.
- **Fase 2 (Fundacional)**: depende de Fase 1 y bloquea historias.
- **Fases 3-6 (US1-US4)**: dependen de Fase 2.
- **Fase 7 (Pulido)**: depende de historias seleccionadas cerradas.

### Dependencias entre Historias

- **US1 (P1)**: puede iniciar justo tras Fase 2.
- **US2 (P1)**: puede iniciar tras Fase 2; se apoya en lista creada de US1 para pruebas E2E.
- **US3 (P2)**: depende de operaciones de ítems (US2) para validar progreso real.
- **US4 (P2)**: puede avanzar en paralelo con US3 tras cerrar operaciones base de US1-US2.

### Reglas dentro de cada historia

- Primero tareas de pruebas (deben fallar inicialmente).
- Luego implementación en Domain → Application → API.
- Cierre de historia solo cuando su prueba independiente pasa.

### Oportunidades de paralelización

- T003, T005, T006, T008 en paralelo.
- En US1: T009-T011 en paralelo.
- En US2: T017-T022 en paralelo por archivo de test.
- En US3: T030-T032 en paralelo.
- En US4: T036-T038 en paralelo.

---

## Estrategia de implementación

### MVP primero (US1 + US2)

1. Completar Fase 1 y 2.
2. Completar US1.
3. Completar US2.
4. Validar flujo principal de valor (crear lista y gestionar ítems).

### Entrega incremental

1. Base compartida (Fase 1-2).
2. US1 (crear/consultar).
3. US2 (gestión de ítems).
4. US3 (progreso).
5. US4 (trazabilidad).
6. Cierre transversal.

---

## Notas

- Tareas `[P]` están diseñadas para no pisar el mismo archivo.
- Cada tarea referencia ruta concreta de archivo.
- La planificación respeta el alcance excluido en FR-013 (sin login, invitaciones, colaboración multiusuario, MAUI completo, pagos y estadísticas avanzadas).
