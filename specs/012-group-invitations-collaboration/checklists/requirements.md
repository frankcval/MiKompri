# Checklist de requerimientos — 012 Group Invitations Collaboration

## Funcional

- [ ] Owner/Admin puede invitar por `@username`.
- [ ] Se crea `GroupInvitation` con estado `Pending`.
- [ ] El invitado visualiza invitación in-app.
- [ ] El invitado puede `Accept`.
- [ ] El invitado puede `Decline`.
- [ ] `Accept` crea `GroupMembership` con rol correcto.
- [ ] Tras aceptar, el usuario puede acceder a listas compartidas del grupo.
- [ ] Owner/Admin puede revocar invitaciones `Pending`.
- [ ] Se evita invitar al mismo usuario dos veces en `Pending`.
- [ ] Se evita self-invite.
- [ ] Se evita invitar usuarios ya miembros.
- [ ] `Accept` es idempotente.
- [ ] Se define expiración (`ExpiresAt`) y regla de no aceptación si expiró.
- [ ] Se define estrategia de concurrencia para cambios de estado.

## Contratos

- [ ] Endpoint de búsqueda por username sin exponer IDs internos.
- [ ] Endpoint de crear invitación.
- [ ] Endpoint de listar mis invitaciones.
- [ ] Endpoint de aceptar invitación.
- [ ] Endpoint de rechazar invitación.
- [ ] Endpoint de revocar invitación.
- [ ] Errores y códigos HTTP definidos por caso.

## Seguridad/privacidad

- [ ] Usuario actual inferido de JWT en todas las operaciones.
- [ ] No exponer `UserId`, `oid`, `tid` ni IDs internos en UI/DTO UX.
- [ ] No usar email para correlación de identidad.

## Frontera 012/013

- [ ] 012 solo cubre onboarding colaborativo al grupo.
- [ ] 013 se limita a UX de shared-lists y no redefine invitaciones.
- [ ] Dependencia de 013 hacia 012 documentada.
