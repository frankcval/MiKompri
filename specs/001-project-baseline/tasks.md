# Tareas: Baseline del Proyecto MiKompri

**Entrada**: Documentos de diseño en `/specs/001-project-baseline/`

**Prerrequisitos**: plan.md (requerido), spec.md (requerido para historias de usuario), research.md, data-model.md, contracts/, quickstart.md, .specify/memory/constitution.md

**Pruebas**: Esta spec es documental; no agrega pruebas de código nuevas. Sí incluye tareas para ejecutar y evidenciar tests existentes de ShoppingList y definir la suite mínima de Users (SC-003, TP8).

**Organización**: Las tareas se agrupan por historia de usuario para permitir implementación y validación independiente por historia.

## Fase 1: Preparación (Infraestructura Compartida)

**Propósito**: Inicializar artefactos de baseline y evidencias de trabajo.

- [ ] T001 Crear estructura de evidencias del baseline en specs/001-project-baseline/evidence/.gitkeep
- [ ] T002 Crear índice de seguimiento de deuda técnica en docs/backlog/technical-debt.md
- [ ] T003 [P] Crear plantilla de issue para deuda técnica en .github/ISSUE_TEMPLATE/technical-debt.yml
- [ ] T004 [P] Añadir sección "Spec-Driven Development" y enlace a specs/001-project-baseline/ en README.md

---

## Fase 2: Fundacional (Prerrequisitos Bloqueantes)

**Propósito**: Alinear estándares transversales que bloquean el avance consistente de todas las historias.

**⚠️ CRÍTICO**: No se puede iniciar trabajo de historias de usuario hasta completar esta fase

- [ ] T005 Crear índice de ADRs y convenciones de numeración en docs/adr/README.md
- [ ] T006 [P] Revisar y actualizar ADR-001 (Azure Container Apps) en docs/adr/ADR-001-azure-deployment.md
- [ ] T007 [P] Revisar y actualizar ADR-002 (EF Core migrations) en docs/adr/ADR-002-migrations-strategy.md
- [ ] T008 [P] Revisar y actualizar ADR-003 (formato MADR) en docs/adr/ADR-003-adr-format.md
- [ ] T009 Consolidar reglas REST/OpenAPI vigentes y diferencias v1/v2 en specs/001-project-baseline/contracts/rest-conventions.md
- [ ] T010 Definir criterio de cierre del gate TP5 y plan incremental de implementación Azure (Bicep + CD + hitos MVP-1) en infra/azure/README.md

**Punto de control**: Base fundacional lista; la implementación por historias puede comenzar en paralelo (con criterio explícito para cerrar TP5).

---

## Fase 3: User Story 1 - Gestión Personal de Listas de Compra (Prioridad: P1) 🎯 MVP

**Objetivo**: Dejar verificable y trazable el estado operacional de ShoppingList (MVP-0).

**Prueba Independiente**: Ejecutar tests Domain/Application/API de ShoppingList y validar flujo create/add/purchase/delete en `/api/v1/PurchaseLists`.

### Implementación de User Story 1

- [ ] T011 [US1] Ejecutar pruebas de dominio de ShoppingList y registrar resultado en specs/001-project-baseline/evidence/us1-domain-tests.md
- [ ] T012 [US1] Ejecutar pruebas de aplicación de ShoppingList y registrar resultado en specs/001-project-baseline/evidence/us1-application-tests.md
- [ ] T013 [US1] Ejecutar pruebas de API de ShoppingList y registrar resultado en specs/001-project-baseline/evidence/us1-api-tests.md
- [ ] T014 [P] [US1] Documentar recorrido E2E de lista personal (create/add/purchase/delete) en specs/001-project-baseline/evidence/us1-e2e-scenario.md
- [ ] T015 [P] [US1] Inventariar endpoints y contratos activos de PurchaseLists en specs/001-project-baseline/evidence/us1-endpoints-inventory.md
- [ ] T016 [US1] Actualizar estado de cumplimiento FR-001..FR-010 y SC-003 en specs/001-project-baseline/spec.md

**Punto de control**: User Story 1 funcional y verificable de forma independiente con evidencia documentada.

---

## Fase 4: User Story 2 - Listas Colaborativas por Grupo (Prioridad: P2)

**Objetivo**: Cerrar brechas documentales y backlog ejecutable para colaboración por grupos.

**Prueba Independiente**: Validar que existe evidencia del filtro por `groupId` y backlog priorizado para `AddedBy`, membresías y permisos.

### Implementación de User Story 2

- [ ] T017 [US2] Documentar validación manual del filtro `groupId` en specs/001-project-baseline/evidence/us2-group-filter-validation.md
- [ ] T018 [P] [US2] Especificar impacto del campo `AddedBy` en dominio y API en specs/001-project-baseline/evidence/us2-addedby-impact.md
- [ ] T019 [P] [US2] Añadir tareas de DT-004 y DT-001 al backlog técnico en docs/backlog/technical-debt.md
- [ ] T020 [US2] Crear propuesta de criterios de autorización por grupo en specs/001-project-baseline/evidence/us2-authorization-rules.md
- [ ] T021 [P] [US2] Definir contrato objetivo de colaboración para v2 en specs/001-project-baseline/contracts/rest-conventions.md
- [ ] T022 [US2] Actualizar estado de cobertura de User Story 2 en specs/001-project-baseline/spec.md

**Punto de control**: User Story 2 tiene plan ejecutable, criterios claros y evidencia del estado parcial actual.

---

## Fase 5: User Story 3 - Gestión de Usuarios y Grupos (Prioridad: P3)

**Objetivo**: Establecer plan técnico ejecutable para convertir Users en bounded context operacional.

**Prueba Independiente**: Verificar que existe roadmap de implementación Users (Application/API/Tests/CI) con contratos y definición de done.

### Implementación de User Story 3

- [ ] T023 [US3] Documentar inventario real de Users Domain/Infrastructure en specs/001-project-baseline/evidence/us3-users-inventory.md
- [ ] T024 [P] [US3] Definir casos de uso mínimos de Users Application (RegisterOrSyncUser, CreateGroup, AddMember, RemoveMember) en specs/001-project-baseline/evidence/us3-application-roadmap.md
- [ ] T025 [P] [US3] Definir contrato REST mínimo de FR-016 con endpoints concretos y payloads en specs/001-project-baseline/evidence/us3-api-roadmap.md
- [ ] T026 [P] [US3] Definir suite mínima de pruebas automatizadas de Users (Domain/Application/API) con casos y filtros `dotnet test` en specs/001-project-baseline/evidence/us3-test-strategy.md
- [ ] T027 [US3] Definir blueprint ejecutable de CI/CD para Users (ci-mikompri-users.yml y cd-mikompri-users.yml) en .github/workflows/README-users-ci-cd.md
- [ ] T028 [US3] Actualizar estado de cumplimiento FR-011..FR-016 y SC-006 en specs/001-project-baseline/spec.md

**Punto de control**: User Story 3 queda lista para iniciar implementación de MVP-1 sin ambigüedades.

---

## Fase 6: Pulido y Aspectos Transversales

**Propósito**: Cierre transversal de consistencia, trazabilidad y gobernanza.

- [ ] T029 [P] Normalizar referencias cruzadas entre spec.md, plan.md, research.md y tasks.md en specs/001-project-baseline/
- [ ] T030 [P] Actualizar quickstart con comandos/flujo final del baseline en specs/001-project-baseline/quickstart.md
- [ ] T031 Consolidar resumen ejecutivo del baseline y siguientes MVPs en specs/001-project-baseline/README.md
- [ ] T032 Ejecutar validación final de formato checklist y dependencias en specs/001-project-baseline/tasks.md

---

## Dependencias y Orden de Ejecución

### Dependencias entre Fases

- **Preparación (Fase 1)**: Sin dependencias.
- **Fundacional (Fase 2)**: Depende de Preparación y bloquea historias.
- **Historias de Usuario (Fase 3+)**: Inician tras completar Fundacional.
- **Pulido (Fase 6)**: Requiere cierre de historias seleccionadas.

### Dependencias entre Historias de Usuario

- **US1 (P1)**: Comienza tras Fase 2; independiente de US2 y US3.
- **US2 (P2)**: Comienza tras Fase 2; depende de evidencia de estado actual de US1 para definir gaps.
- **US3 (P3)**: Comienza tras Fase 2; puede avanzar en paralelo con US2.

### Dentro de Cada User Story

- Primero evidencia/estado actual.
- Después definición de backlog/contratos/criterios.
- Finalmente actualización de spec de cumplimiento.

### Oportunidades de Paralelización

- Fase 1: T003 y T004 en paralelo.
- Fase 2: T006, T007 y T008 en paralelo; T010 depende de la salida de estos artefactos.
- US1: T014 y T015 en paralelo tras ejecutar T011-T013.
- US2: T018, T019 y T021 en paralelo tras T017.
- US3: T024, T025 y T026 en paralelo tras T023.
- Fase 6: T029 y T030 en paralelo.

---

## Ejemplo en Paralelo: User Story 1

```bash
# Después de pruebas base de US1:
Tarea: "T014 [US1] Documentar recorrido E2E en specs/001-project-baseline/evidence/us1-e2e-scenario.md"
Tarea: "T015 [US1] Inventariar endpoints en specs/001-project-baseline/evidence/us1-endpoints-inventory.md"
```

## Ejemplo en Paralelo: User Story 2

```bash
# Tras validar filtro groupId:
Tarea: "T018 [US2] Especificar impacto AddedBy en specs/001-project-baseline/evidence/us2-addedby-impact.md"
Tarea: "T019 [US2] Añadir DT-004/DT-001 en docs/backlog/technical-debt.md"
Tarea: "T021 [US2] Definir contrato colaboración v2 en specs/001-project-baseline/contracts/rest-conventions.md"
```

## Ejemplo en Paralelo: User Story 3

```bash
# Tras inventario Users:
Tarea: "T024 [US3] Backlog Application en specs/001-project-baseline/evidence/us3-application-roadmap.md"
Tarea: "T025 [US3] Backlog API en specs/001-project-baseline/evidence/us3-api-roadmap.md"
Tarea: "T026 [US3] Estrategia de tests en specs/001-project-baseline/evidence/us3-test-strategy.md"
```

---

## Estrategia de Implementación

### MVP Primero (Solo User Story 1)

1. Completar Fase 1 y Fase 2.
2. Completar US1 (T011-T016).
3. Validar evidencia de tests y flujo E2E.
4. Publicar baseline como referencia estable.

### Entrega Incremental

1. Preparación + Fundacional para fijar estándares.
2. US1 para garantizar continuidad del núcleo (ShoppingList).
3. US2 para preparar colaboración por grupos.
4. US3 para dejar listo el arranque de MVP-1 (autenticación e identidad).
5. Pulido para cierre documental y gobernanza.

### Estrategia de Equipo en Paralelo

1. Equipo A: Preparación + Fundacional.
2. Equipo B: US1 cuando Fase 2 termine.
3. Equipo C: US2 y US3 en paralelo tras Fase 2.
4. Cierre conjunto en Fase 6.

---

## Notas

- Todas las tareas cumplen formato checklist `- [ ] T### [P?] [US?] Descripción con ruta`.
- Las tareas [P] evitan conflicto directo de archivo.
- Cada historia mantiene criterio de prueba independiente y entregable verificable.
