# Spec 012 — Group Invitations Collaboration

## Resumen

Esta especificación define el flujo para que una persona ingrese a un grupo mediante invitaciones in-app por `@username`, con control de estados, expiración, idempotencia y concurrencia, sin exponer identificadores internos en la UI.

## Objetivo de negocio

Eliminar el alta directa de miembros por `UserId` como experiencia de producto y reemplazarla por un proceso controlado de invitación/aceptación que habilite colaboración segura en listas compartidas.

## Alcance (in)

1. Invitar usuario por `@username` desde Owner/Admin.
2. Crear `GroupInvitation` en estado `Pending`.
3. Inbox in-app de invitaciones para usuario objetivo.
4. Acciones `Accept` y `Decline`.
5. `Accept` crea `GroupMembership` y habilita acceso posterior a shared-lists.
6. Revocación de invitaciones `Pending` por actor autorizado.
7. Reglas de negocio:
   - no duplicados `Pending`;
   - no self-invite;
   - no invitar miembros existentes.
8. Idempotencia de aceptación.
9. Expiración (`ExpiresAt`) y estrategia de concurrencia.

## Fuera de alcance (out)

- Email invitations.
- Links/QR/SMS/chat/push.
- Eliminación de grupo.
- Transfer ownership.
- Eliminación de miembros.
- 010-B.
- Cambios de autenticación.
- Rediseño global de UX.

## Actores

- Owner de grupo.
- Admin de grupo.
- Member de grupo (sin permisos de invitar/revocar en 012).
- Usuario invitado (target).

## Historias funcionales

### H1 — Invitar por username
Como Owner/Admin quiero invitar por `@username` para incorporar personas sin exponer IDs internos.

**Criterios**
- Validar username objetivo.
- Resolver identidad canónica en backend.
- Crear `Pending` con `ExpiresAt`.

### H2 — Ver invitaciones recibidas
Como usuario invitado quiero ver mis invitaciones pendientes para decidir aceptar o rechazar.

**Criterios**
- Listado in-app por usuario actual.
- Sin exponer IDs internos.

### H3 — Aceptar invitación
Como invitado quiero aceptar para entrar al grupo.

**Criterios**
- Solo target autenticado puede aceptar.
- Crea membresía con rol de invitación.
- Idempotente ante reintentos.

### H4 — Rechazar invitación
Como invitado quiero rechazar cuando no quiero participar.

**Criterios**
- Solo target autenticado.
- Transición `Pending -> Declined`.
- Idempotencia de operación.

### H5 — Revocar invitación
Como Owner/Admin quiero revocar invitaciones pendientes erróneas u obsoletas.

**Criterios**
- Solo estado `Pending`.
- Solo actor autorizado del grupo.
- `Revoked` bloquea aceptación posterior.

## Reglas de negocio

1. No invitar al mismo usuario más de una vez en `Pending` para el mismo grupo.
2. Self-invite prohibido.
3. No invitar usuario ya miembro.
4. Invitaciones expiradas no son aceptables.
5. Estado terminal no revierte.
6. Usuario actual siempre obtenido desde JWT/canonical UserId backend.
7. No usar email para correlacionar identidad.
8. Member no puede invitar ni revocar invitaciones.
9. La búsqueda por username es exacta normalizada (trim, sin `@` inicial, comparación case-insensitive).
10. Política de expiración: `ExpiresAt = CreatedAt + 7 días` en UTC.
11. Una invitación se considera expirada cuando `UtcNow >= ExpiresAt` y no puede aceptarse.
12. Estrategia de concurrencia: unicidad transaccional de `Pending` por `(GroupId, TargetUserId)` y transición de estado con control optimista/versionado.

## Modelo

`GroupInvitation`:
- `Id`
- `GroupId`
- `InvitedByUserId`
- `TargetUserId`
- `Role`
- `Status`
- `CreatedAt`
- `ExpiresAt`
- `RespondedAt`

Estados:
- `Pending`
- `Accepted`
- `Declined`
- `Expired`
- `Revoked`

## API objetivo

- `GET /api/v1/users/search?username={value}`
- `POST /api/v1/groups/{groupId}/invitations`
- `GET /api/v1/groups/invitations/me`
- `POST /api/v1/groups/invitations/{invitationId}/accept`
- `POST /api/v1/groups/invitations/{invitationId}/decline`
- `POST /api/v1/groups/invitations/{invitationId}/revoke`

Ver detalle en `contracts/api.md`.

## Seguridad y privacidad

- Sin exposición de `UserId`, `oid`, `tid`, `tenant`, ni IDs internos en UI.
- Sin correlación por email.
- Autorización por rol de grupo y ownership de invitación.

## Alineación constitucional TP11 (Identidad y Autenticación Transversal)

1. La identidad del usuario actual se obtiene exclusivamente desde JWT validado (issuer, audience, firma, expiración y scopes).
2. La correlación de identidad externa a identidad interna sigue el mapeo canónico `(tid, oid) -> UserId` gestionado únicamente por `Users`.
3. Este feature no altera autenticación ni onboarding global; consume `UserId` canónico ya resuelto por `Users`.
4. Ningún bounded context fuera de `Users` accede directamente a la base de datos de `Users`; la integración se hace por contratos/API públicos.
5. Se prohíbe correlacionar identidad por email, username, nombre o teléfono para identidad canónica.

## Dependencia con 013

012 habilita onboarding colaborativo al grupo. 013 consume resultado (membresía existente) para UX de shared-lists. 013 no define ni implementa lógica de invitaciones.

## Success Criteria

- SC-001: Crear invitación válida devuelve `Pending` con `CreatedAt` y `ExpiresAt` en UTC.
- SC-002: Aceptar invitación crea exactamente una membresía; reintentos no crean duplicados (idempotencia).
- SC-003: Rechazar invitación es idempotente y no crea membresía.
- SC-004: Revocar solo aplica a `Pending`; invitación revocada no puede aceptarse.
- SC-005: No se permite self-invite, invitación a miembro existente ni duplicado `Pending` para `(GroupId, TargetUserId)`.
- SC-006: Ningún contrato de UX expone `UserId`, `oid`, `tid` o IDs internos.
- SC-007: Invitaciones expiradas no son aceptables y devuelven respuesta consistente según contrato.

## Riesgos

1. Carreras entre aceptar/revocar/expirar.
2. Duplicados por concurrencia de creación.
3. Divergencia de implementación respecto a la matriz HTTP idempotente definida.
4. Dependencia de normalización consistente de username.

## Evidencia y artefactos

- Auditoría: `evidence/current-state-audit.md`
- Modelo: `data-model.md`
- Investigación: `research.md`
- Contratos: `contracts/*`
- Checklist: `checklists/*`
