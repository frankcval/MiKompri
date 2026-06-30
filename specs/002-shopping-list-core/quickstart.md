# Quickstart — Shopping List Core Hardening

**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Fecha**: 2026-06-30

## Objetivo

Validar de extremo a extremo el flujo core existente: lista + ítems + progreso +
trazabilidad básica, confirmando el cierre de huecos de consistencia de esta fase
de hardening.

## Seguimiento de ejecución

| Tarea | Estado | Evidencia |
|-------|--------|-----------|
| T001 | ✅ Completada | La spec define 4 historias (US1-US4), 13 FR, 6 RB, 5 ERR y 6 CD; el plan mantiene el alcance en `MiKompri.ShoppingList.*`; `research.md` fija las decisiones de hardening; `data-model.md` cubre lista, ítem y progreso; `contracts/shoppinglist-core-api.md` refleja las operaciones y errores funcionales esperados. |
| T002 | ✅ Completada | Este archivo incorpora el seguimiento operativo de la fase, los criterios de salida y la evidencia mínima a registrar. |
| T003 | ✅ Completada | Se añade una matriz FR/RB/ERR/CD con trazabilidad hacia historias, tareas y validación esperada. |
| T030-T035 | ✅ Completadas | `ListProgress` ahora expone total/comprados/pendientes/porcentaje; `PurchaseListDTO` y los queries de lectura mapean progreso consistente; los tests de dominio, aplicación y API cubren lista vacía y lista con ítems mixtos. |

## Alineación de artefactos

| Artefacto | Qué define | Alineación confirmada |
|-----------|------------|-----------------------|
| `spec.md` | Alcance funcional, historias, requisitos, reglas, errores y done | El alcance se limita al hardening del core individual y deja fuera auth, Users, colaboración y MAUI completo. |
| `plan.md` | Enfoque técnico, restricciones, estructura y constitution check | Reutiliza la arquitectura existente de ShoppingList y mantiene el gate repo-level de TP5 solo como nota transversal. |
| `research.md` | Decisiones funcionales clave | Confirma trazabilidad temporal básica, duplicados por `ProductId`, marcado idempotente y cobertura por capas. |
| `data-model.md` | Entidades, relaciones y cálculo de progreso | Refuerza que la trazabilidad de esta fase es `CreatedAt`/`UpdatedAt` y que el progreso se calcula sobre ítems comprados/total. |
| `contracts/shoppinglist-core-api.md` | Operaciones funcionales y mapeo de errores | Cubre crear lista, consultar lista, CRUD operativo de ítems, progreso y ERR-001..ERR-005. |
| `tasks.md` | Ejecución por fases e historias | Las tareas cubren US1-US4 y enlazan pruebas, implementación y cierre transversal sin ampliar alcance. |

### Resultado de la validación

- No hay bloqueos de alcance entre `spec.md`, `plan.md`, `research.md`, `data-model.md`,
  `contracts/shoppinglist-core-api.md` y `tasks.md`.
- La trazabilidad básica queda acotada a timestamps (`CreatedAt`/`UpdatedAt`) hasta una
  feature posterior de autenticación/Users.
- El trabajo de la fase se mantiene en `MiKompri.ShoppingList.*` y sus tests.

## Prerrequisitos

- .NET SDK 8
- Docker Desktop + Docker Compose
- Solución restaurada

## 1) Restaurar y compilar

```powershell
dotnet restore MiKompri.sln
dotnet build MiKompri.sln --configuration Release --no-restore
```

## 2) Ejecutar tests del bounded context ShoppingList

```powershell
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj --configuration Release
```

## 3) Verificar flujo funcional mínimo

1. Crear una lista de compra.
2. Consultar la lista creada.
3. Agregar un ítem.
4. Editar el ítem.
5. Marcar el ítem como comprado.
6. Consultar progreso (debe actualizarse).
7. Eliminar el ítem.
8. Confirmar progreso en 0% cuando no hay ítems.

## 4) Verificar casos negativos

- Crear lista con nombre inválido → error de validación.
- Consultar lista inexistente → error de no encontrado.
- Editar/eliminar ítem inexistente → error de no encontrado.
- Agregar ítem duplicado en misma lista → error de conflicto de regla de negocio.

## 5) Matriz de cobertura FR / RB / ERR / CD

| ID | Cobertura prevista | Validación esperada |
|----|--------------------|---------------------|
| FR-001 | US1 · T010-T016 | Crear lista con nombre válido y obtener `201 Created`. |
| FR-002 | US1 · T011, T015-T016 | Consultar lista existente con nombre, ítems, progreso y trazabilidad. |
| FR-003 | US2 · T018, T025, T029 | Agregar ítem válido a lista existente. |
| FR-004 | US2 · T019, T026, T029 | Editar ítem existente y reflejar cambios en lectura posterior. |
| FR-005 | US2 · T020, T027, T029 | Marcar ítem como comprado con resultado idempotente. |
| FR-006 | US2 · T021, T028-T029 | Eliminar ítem y recalcular progreso. |
| FR-007 | US3 · T030-T035 | Exponer total, comprados, pendientes y porcentaje correctos. |
| FR-008 | US4 · T036-T042 | Mantener `CreatedAt` y `UpdatedAt` en lista e ítems. |
| FR-009 | Fase 2 + US1/US2 · T004, T013, T025-T028 | Validar datos obligatorios antes de persistir. |
| FR-010 | Fase 2 + US1 · T005, T008, T011, T015-T016 | Responder error claro cuando la lista no exista. |
| FR-011 | Fase 2 + US2 · T005, T008, T022, T029 | Responder error claro cuando el ítem no exista. |
| FR-012 | US2 · T017, T023, T025 | Impedir duplicados de `ProductId` en una misma lista. |
| FR-013 | Fase 1 + notas de alcance · T001-T003 | Mantener fuera de alcance auth, colaboración, MAUI completo y extras. |
| RB-001 | US1 · T009, T012-T013 | Lista sin nombre debe fallar. |
| RB-002 | US2 · T017, T023-T024 | Todo ítem debe pertenecer a una única lista. |
| RB-003 | US2 · T017, T023, T025 | No puede haber dos ítems con el mismo `ProductId` en la misma lista. |
| RB-004 | US3 · T030, T033-T035 | Lista vacía => 0%; lista con ítems mixtos => porcentaje correcto. |
| RB-005 | US2 + US3 · T017, T020, T027, T030 | Reintentar marcado no rompe estado ni progreso. |
| RB-006 | US4 · T036-T042 | Toda mutación actualiza la trazabilidad correspondiente. |
| ERR-001 | US1 · T010-T013 | Error de validación por nombre inválido. |
| ERR-002 | US1 · T011, T015-T016 | Error por lista inexistente. |
| ERR-003 | US2 · T019-T022, T026-T029 | Error por ítem inexistente. |
| ERR-004 | US2 · T017-T018, T023, T025 | Error por duplicidad de ítem. |
| ERR-005 | US2 · T018-T019, T025-T029 | Error por datos inválidos en alta/edición. |
| CD-001 | `spec.md` + T001 | Todas las historias mantienen escenarios verificables. |
| CD-002 | T003 + T043-T044 | Los FR-001..FR-013 quedan trazados y con validación documentada. |
| CD-003 | T017-T021, T030, T036-T037 | Las RB-001..RB-006 quedan cubiertas por pruebas de comportamiento. |
| CD-004 | T010-T011, T018-T022, T043 | Los ERR-001..ERR-005 quedan cubiertos en pruebas negativas. |
| CD-005 | US2-US3 · T022, T030-T035 | El progreso se recalcula tras add, mark, edit y delete. |
| CD-006 | US4 · T036-T042 | La trazabilidad básica queda presente y consistente en lista e ítems. |

## 6) Evidencia mínima a registrar al ejecutar la fase

- Resultado de `dotnet restore MiKompri.sln`
- Resultado de `dotnet build MiKompri.sln --configuration Release --no-restore`
- Resultado de los tres proyectos de test de ShoppingList
- Notas de validación manual del flujo create → get → add → update → purchase → delete
- Notas de validación negativa para ERR-001..ERR-005

## 7) Criterio de salida de la fase

- Flujos P1 y P2 trazados y verificables.
- Reglas de negocio y errores esperados mapeados a pruebas.
- No se amplía alcance fuera de esta fase de hardening.
