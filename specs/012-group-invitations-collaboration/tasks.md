# Tasks — 012 Group Invitations Collaboration

## Bloque A — Análisis y frontera

- [ ] T001 Revisar estado actual de grupos/membresías en Users API/Application/Domain.
- [ ] T002 Revisar estado actual de UX grupos/shared-lists en Web.
- [ ] T003 Mapear diferencias entre membresía directa actual y flujo invitación objetivo.
- [ ] T004 Identificar solapamientos 012 vs 013 y documentarlos.
- [ ] T005 Actualizar 013 con dependencia explícita y exclusión de duplicados.

## Bloque B — Dominio de invitaciones

- [ ] T006 Definir entidad `GroupInvitation` y atributos obligatorios.
- [ ] T007 Definir enum de estados y transiciones válidas.
- [ ] T008 Definir reglas de invariantes (self-invite, ya miembro, duplicado pending).
- [ ] T009 Definir estrategia de expiración (`ExpiresAt`) y tratamiento de expiradas.
- [ ] T010 Definir semántica de `RespondedAt`.

## Bloque C — Contratos API

- [ ] T011 Definir contrato de búsqueda por username sin IDs internos.
- [ ] T012 Definir contrato de creación de invitación.
- [ ] T013 Definir contrato de listado de invitaciones del usuario actual.
- [ ] T014 Definir contrato de aceptar invitación (idempotente).
- [ ] T015 Definir contrato de rechazar invitación (idempotente).
- [ ] T016 Definir contrato de revocar invitación pending.
- [ ] T017 Definir códigos HTTP y errores por escenario.

## Bloque D — Permisos, seguridad y concurrencia

- [ ] T018 Definir matriz Owner/Admin/Member/Invitado para cada acción.
- [ ] T019 Definir origen de actor siempre desde JWT.
- [ ] T020 Asegurar política de no exposición (`UserId`, `oid`, `tid`, internos).
- [ ] T021 Prohibir correlación por email en contratos.
- [ ] T022 Definir control de concurrencia en transiciones de estado.
- [ ] T023 Definir deduplicación transaccional en creación pending.

## Bloque E — Validación de experiencia y adopción

- [ ] T024 Definir UX mínima de inbox in-app y estados.
- [ ] T025 Definir punto de integración con shared-lists tras aceptación.
- [ ] T026 Documentar quickstart de validación funcional/manual.
- [ ] T027 Completar checklist de requerimientos.
- [ ] T028 Completar checklist de aceptación.

## Bloque F — Cierre de especificación

- [ ] T029 Consolidar riesgos y mitigaciones.
- [ ] T030 Revisar consistencia global entre spec/plan/tasks/contracts.
- [ ] T031 Emitir resumen final solicitado (solapamientos, cambios 013, alcance, endpoints, modelo, permisos, riesgos, total tareas).
- [ ] T032 Definir matriz determinista de respuestas HTTP por estado previo en accept/decline/revoke.
- [ ] T033 Definir casos de validación de concurrencia (accept vs revoke, create duplicado simultáneo).
- [ ] T034 Definir casos de validación de idempotencia (retry de accept/decline sobre misma invitación).
- [ ] T035 Definir casos de validación de expiración en frontera UTC (`UtcNow >= ExpiresAt`).
- [ ] T036 Documentar regla explícita: Member no puede invitar ni revocar.
- [ ] T037 Validar trazabilidad de cumplimiento TP11 en `spec.md` y `plan.md` antes de cierre.
