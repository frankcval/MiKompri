# Plan — 012 Group Invitations Collaboration

## Objetivo de ejecución

Diseñar y validar la especificación funcional/técnica para invitaciones de grupo con frontera clara respecto a 013, sin implementar código productivo.

## Fases

### Fase 1 — Baseline y límites
1. Auditar estado actual de grupos/membresías.
2. Confirmar que no existe flujo de invitaciones.
3. Identificar solapamientos con 013.

### Fase 2 — Diseño de dominio y contratos
4. Definir `GroupInvitation` y máquina de estados.
5. Definir invariantes, expiración e idempotencia.
6. Definir endpoints y matriz determinista de errores/respuestas HTTP.
7. Definir matriz de permisos (incluyendo restricción explícita de Member).

### Fase 3 — UX funcional mínima (sin rediseño)
8. Definir búsqueda por username y flujo in-app de inbox.
9. Definir acciones accept/decline/revoke y estados visuales.
10. Definir comportamiento de expiración.

### Fase 4 — Calidad, riesgos y dependencia con 013
11. Consolidar riesgos de concurrencia y mitigaciones.
12. Validar alineación constitucional TP11 y trazabilidad de identidad canónica.
13. Documentar dependencia 013 -> 012 y retirar duplicados de 013.
14. Cerrar checklists y quickstart.

## Entregables

- `spec.md`
- `research.md`
- `data-model.md`
- `contracts/api.md`
- `contracts/permissions-matrix.md`
- `checklists/requirements.md`
- `checklists/acceptance.md`
- `tasks.md`
- `quickstart.md`
- `evidence/current-state-audit.md`

## Criterios de salida

- Alcance 012 acotado y verificable.
- Solapamiento con 013 explícito y minimizado.
- API/modelo/permisos/riesgos definidos.
- Alineación explícita y verificable con TP11 documentada en spec/plan.
- Ready for review sin cambios productivos.
