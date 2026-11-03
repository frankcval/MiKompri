# Permissions Matrix — 012 Group Invitations Collaboration

## Roles relevantes
- `Owner`
- `Admin`
- `Member`
- `Invitado` (usuario objetivo aún no miembro)

## Acciones

| Acción | Owner | Admin | Member | Invitado objetivo | Regla |
|---|---:|---:|---:|---:|---|
| Buscar usuario por `@username` para invitar | ✅ | ✅ | ❌ | N/A | La búsqueda para invitar requiere contexto de grupo y permisos de invitación |
| Crear invitación `Pending` con rol `Member` | ✅ | ✅ | ❌ | N/A | Member no gestiona incorporación |
| Crear invitación `Pending` con rol `Admin` | ✅ | ❌ | ❌ | N/A | Solo Owner puede invitar con rol `Admin`; Admin solo puede invitar con rol `Member`. |
| Ver invitaciones emitidas de mi grupo | ✅ | ✅ | ❌ | N/A | Gestión operativa |
| Revocar invitación `Pending` | ✅ | ✅ | ❌ | ❌ | Solo actor con permisos de grupo |
| Ver mis invitaciones recibidas | N/A | N/A | N/A | ✅ | Inbox personal |
| Aceptar invitación propia | N/A | N/A | N/A | ✅ | Solo target autenticado |
| Rechazar invitación propia | N/A | N/A | N/A | ✅ | Solo target autenticado |

## Reglas transversales

1. Self-invite prohibido.
2. Usuario ya miembro no puede recibir invitación pending.
3. Aceptar/rechazar idempotente bajo retries.
4. Revocar solo `Pending`.
5. Expirada no aceptable.
