# Tasks — 012 Group Invitations Collaboration

## Bloque A — Análisis y frontera

- [x] T001 Revisar estado actual de grupos/membresías en Users API/Application/Domain.
- [x] T002 Revisar estado actual de UX grupos/shared-lists en Web.
- [x] T003 Mapear diferencias entre membresía directa actual y flujo invitación objetivo.
- [x] T004 Identificar solapamientos 012 vs 013 y documentarlos.
- [x] T005 Actualizar 013 con dependencia explícita y exclusión de duplicados.

## Bloque B — Dominio de invitaciones

- [x] T006 Definir entidad `GroupInvitation` y atributos obligatorios.
- [x] T007 Definir enum de estados y transiciones válidas.
- [x] T008 Definir reglas de invariantes (self-invite, ya miembro, duplicado pending).
- [x] T009 Definir estrategia de expiración (`ExpiresAt`) y tratamiento de expiradas.
- [x] T010 Definir semántica de `RespondedAt`.

## Bloque C — Contratos API

- [x] T011 Definir contrato de búsqueda por username sin IDs internos.
- [x] T012 Definir contrato de creación de invitación.
- [x] T013 Definir contrato de listado de invitaciones del usuario actual.
- [x] T014 Definir contrato de aceptar invitación (idempotente).
- [x] T015 Definir contrato de rechazar invitación (idempotente).
- [x] T016 Definir contrato de revocar invitación pending.
- [x] T017 Definir códigos HTTP y errores por escenario.

## Bloque D — Permisos, seguridad y concurrencia

- [x] T018 Definir matriz Owner/Admin/Member/Invitado para cada acción.
- [x] T019 Definir origen de actor siempre desde JWT.
- [x] T020 Asegurar política de no exposición (`UserId`, `oid`, `tid`, internos).
- [x] T021 Prohibir correlación por email en contratos.
- [x] T022 Definir control de concurrencia en transiciones de estado.
- [x] T023 Definir deduplicación transaccional en creación pending.

## Bloque E — Validación de experiencia y adopción

- [x] T024 Definir UX mínima de inbox in-app y estados.
- [x] T025 Definir punto de integración con shared-lists tras aceptación.
- [x] T026 Documentar quickstart de validación funcional/manual.
- [x] T027 Completar checklist de requerimientos.
- [x] T028 Completar checklist de aceptación.

## Bloque F — Cierre de especificación

- [x] T029 Consolidar riesgos y mitigaciones.
- [x] T030 Revisar consistencia global entre spec/plan/tasks/contracts.
- [x] T031 Emitir resumen final solicitado (solapamientos, cambios 013, alcance, endpoints, modelo, permisos, riesgos, total tareas).
- [x] T032 Definir matriz determinista de respuestas HTTP por estado previo en accept/decline/revoke.
- [x] T033 Definir casos de validación de concurrencia (accept vs revoke, create duplicado simultáneo).
- [x] T034 Definir casos de validación de idempotencia (retry de accept/decline sobre misma invitación).
- [x] T035 Definir casos de validación de expiración en frontera UTC (`UtcNow >= ExpiresAt`).
- [x] T036 Documentar regla explícita: Member no puede invitar ni revocar.
- [x] T037 Validar trazabilidad de cumplimiento TP11 en `spec.md` y `plan.md` antes de cierre.
