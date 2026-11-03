# Checklist de requerimientos — 012 Group Invitations Collaboration

> Estado: checklist de **formalización de spec**. Los ítems marcados validan que el requerimiento quedó especificado en `spec.md`/`contracts/*`.

## Funcional

- [x] Owner/Admin puede invitar por `@username`.
- [x] Se crea `GroupInvitation` con estado `Pending`.
- [x] El invitado visualiza invitación in-app.
- [x] El invitado puede `Accept`.
- [x] El invitado puede `Decline`.
- [x] `Accept` crea `GroupMembership` con rol correcto.
- [x] Tras aceptar, el usuario puede acceder a listas compartidas del grupo.
- [x] Owner/Admin puede revocar invitaciones `Pending`.
- [x] Se evita invitar al mismo usuario dos veces en `Pending`.
- [x] Se evita self-invite.
- [x] Se evita invitar usuarios ya miembros.
- [x] `Accept` es idempotente.
- [x] Se define expiración (`ExpiresAt`) y regla de no aceptación si expiró.
- [x] Se define estrategia de concurrencia para cambios de estado.

## Contratos

- [x] Endpoint de búsqueda por username sin exponer IDs internos.
- [x] Endpoint de crear invitación.
- [x] Endpoint de listar mis invitaciones.
- [x] Endpoint de aceptar invitación.
- [x] Endpoint de rechazar invitación.
- [x] Endpoint de revocar invitación.
- [x] Errores y códigos HTTP definidos por caso.

## Seguridad/privacidad

- [x] Usuario actual inferido de JWT en todas las operaciones.
- [x] No exponer `UserId`, `oid`, `tid` ni IDs internos en UI/DTO UX.
- [x] No usar email para correlación de identidad.

## Frontera 012/013

- [x] 012 solo cubre onboarding colaborativo al grupo.
- [x] 013 se limita a UX de shared-lists y no redefine invitaciones.
- [x] Dependencia de 013 hacia 012 documentada.
