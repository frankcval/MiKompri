# Quickstart — 012 Group Invitations Collaboration

## Objetivo

Validar la especificación de invitaciones de grupo de extremo a extremo a nivel de contrato/flujo (sin implementación productiva en esta entrega).

## Precondiciones

1. Usuario autenticado (JWT válido) con perfil de Users existente.
2. Grupo creado con Owner.
3. Usuario target existente con `@username`.

## Flujo principal esperado

1. Owner/Admin busca `@username` objetivo.
2. Owner/Admin crea invitación `Pending` con rol permitido.
3. Target visualiza invitación en inbox in-app.
4. Target acepta:
   - invitación pasa a `Accepted`;
   - se crea `GroupMembership`;
   - target puede consultar contenido shared-list del grupo según 013.

## Flujo alternativo: rechazo

1. Target rechaza invitación `Pending`.
2. Estado cambia a `Declined`.
3. No se crea membresía.

## Flujo alternativo: revocación

1. Owner/Admin revoca invitación `Pending`.
2. Estado cambia a `Revoked`.
3. Target no puede aceptarla.

## Casos de validación crítica

- Duplicado pending para mismo usuario/grupo -> rechazo (`409` esperado).
- Self-invite -> rechazo (`400`).
- Usuario ya miembro -> rechazo (`409`).
- Aceptación repetida -> respuesta idempotente, sin membresías duplicadas.
- Invitación expirada -> no aceptable.
- Aceptar/revocar concurrentes -> estado final consistente.

## Verificación de privacidad

- Payloads y vistas no exponen `UserId`, `oid`, `tid` ni IDs internos.
- Búsqueda/flujo de invitación no usa email para correlación.
