# Tasks — 013 Shared Lists UX Completion

**Input**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, evidencia auditada en `evidence/current-state-audit.md`.

**Regla de alcance**: 013 no implementa invitaciones ni redefinición de 012.

## Formato
- `[ID] [P?] [Área] Descripción`
- `[P]` = paralelizable (archivos distintos / bajo acoplamiento)

---

## Phase 1 — Baseline y contratos de UX

- [ ] T001 [P] [Baseline] Consolidar matriz de permisos Owner/Admin/Member desde backend en `contracts/permissions-matrix.md`.
- [ ] T002 [P] [Baseline] Documentar contrato de navegación shared-lists (`source`, `groupId`, `tab`, `back`) en `contracts/navigation.md`.
- [ ] T003 [Baseline] Registrar baseline visual/funcional de `/shared-lists` y `/shared-lists/:id` en `evidence/current-state-audit.md` (actualizar con capturas y rutas probadas).
- [ ] T004 [Baseline] Definir checklist de no-regresión respecto a 005/009/011 en `checklists/requirements.md`.

---

## Phase 2 — Tests primero (shared lists UX)

- [ ] T005 [P] [Tests-List] Añadir pruebas component para listado compartido (vacío, una, múltiples, multi-grupo, parcial) en `MiKompri.Web/tests/component/features/shared-lists/`.
- [ ] T006 [P] [Tests-Create] Añadir pruebas component para creación compartida (grupo único, múltiples, sin grupos, cancelar, 403) en `MiKompri.Web/tests/component/features/shared-lists/`.
- [ ] T007 [P] [Tests-Detail-Navigation] Añadir pruebas de deep-link/reload/back-forward para tabs y origen de navegación en `SharedListDetail.spec.tsx`.
- [ ] T008 [P] [Tests-Detail-Shopping] Añadir pruebas component de compra compartida por rol (owner/admin/member autor/no autor) en `SharedListDetail.spec.tsx`.
- [ ] T009 [P] [Tests-Detail-Expenses] Añadir pruebas component de tab gastos (vacío, registros, error, permisos) en `tests/component/features/settlements/`.
- [ ] T010 [P] [Tests-Detail-People] Añadir pruebas component de tab personas (roles, vacío, error) en `tests/component/features/shared-lists/`.
- [ ] T011 [P] [Tests-E2E] Añadir escenarios e2e shared-lists end-to-end (Group→List y Lists→List) en `tests/e2e/modernization/`.
- [ ] T012 [P] [Tests-E2E] Añadir cobertura responsive 320/390/430/768/1280 para shared-list listing/detail en `tests/e2e/modernization/`.

---

## Phase 3 — Navegación y contexto

- [ ] T013 [Nav] Implementar contrato de navegación unificada para shared detail en `src/features/shared-lists/pages/SharedListDetailPage.tsx`.
- [ ] T014 [Nav] Mejorar contexto de cabecera (lista/grupo/estado/progreso/estimación) en detalle compartido.
- [ ] T015 [Nav] Ajustar estrategia de "volver" coherente con origen y query params.
- [ ] T016 [Nav] Verificar que selected navigation en shell sigue consistente para `/shared-lists/*`.

---

## Phase 4 — Listado compartido y creación

- [ ] T017 [List] Refinar tarjetas de `SharedListsPage` para coherencia visual y jerarquía informativa con 011.
- [ ] T018 [List] Resolver estados de vacío/error/parcial con copy accionable y no administrativo.
- [ ] T019 [Create] Ajustar flujo crear lista compartida (grupo→nombre→confirmación) sin exponer IDs.
- [ ] T020 [Create] Asegurar persistencia/control de selección de grupo sin contaminación entre grupos.

---

## Phase 5 — Detalle Compra

- [ ] T021 [Shopping] Mejorar legibilidad de filas de compra compartida (producto/cantidad/estimación/autoría/estado).
- [ ] T022 [Shopping] Alinear acciones secundarias y disclosure con patrón de 011 (sin saturación).
- [ ] T023 [Shopping] Reforzar mensajes de permisos/403 y estado de lista cerrada.
- [ ] T024 [Shopping] Validar edge cases: 100 ítems, nombres largos, precios desconocidos, desmarcado posterior.

---

## Phase 6 — Detalle Gastos

- [ ] T025 [Expenses] Clarificar semántica visual entre estimación de compra y gasto real.
- [ ] T026 [Expenses] Mejorar estados de balance/propuesta/historial (loading/error/empty) en tab gastos.
- [ ] T027 [Expenses] Reforzar comportamiento por rol para registrar/corregir/eliminar gastos según backend.

---

## Phase 7 — Detalle Personas

- [ ] T028 [People] Mejorar presentación de miembros y roles sin introducir funciones de invitación (dependencia 012 para onboarding).
- [ ] T029 [People] Asegurar copy y estados vacíos/error acordes a alcance 013 y sin duplicar acciones de 012 (invitar/aceptar/rechazar/revocar).

---

## Phase 8 — Accesibilidad, responsive y rendimiento

- [ ] T030 [A11y] Validar teclado/foco/Escape/dialog restore/tabs y nombres accesibles en shared flows.
- [ ] T031 [Responsive] Validar layouts 320/390/430/768/1280 en listado + detalle + tabs.
- [ ] T032 [Perf] Revisar queries duplicadas/rerenders/waterfalls de shared-lists y documentar impacto.

---

## Phase 9 — Evidencia y cierre de 013

- [ ] T033 [Docs] Actualizar `evidence/` con resultados de validación y límites reales.
- [ ] T034 [Docs] Completar `checklists/acceptance.md` con evidencia por criterio.
- [ ] T035 [Docs] Redactar resumen final de alcance cumplido, out-of-scope y riesgos residuales.

## Conteo

Total tareas: **35**.
