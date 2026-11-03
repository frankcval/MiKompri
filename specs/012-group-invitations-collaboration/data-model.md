# Data Model — 012 Group Invitations Collaboration

## 1) `GroupInvitation`

### Campos
- `Id` (Guid)
- `GroupId` (Guid)
- `InvitedByUserId` (Guid)
- `TargetUserId` (Guid)
- `Role` (enum: `Member | Admin`)
- `Status` (enum: `Pending | Accepted | Declined | Expired | Revoked`)
- `CreatedAt` (UTC)
- `ExpiresAt` (UTC)
- `RespondedAt` (UTC?, null hasta respuesta)

### Reglas
1. Solo `Pending` puede transicionar.
2. Transiciones válidas:
   - `Pending -> Accepted`
   - `Pending -> Declined`
   - `Pending -> Revoked`
   - `Pending -> Expired` (proceso temporal o validación al leer/operar)
3. Estado terminal no reabre.
4. `RespondedAt` se setea en `Accepted`/`Declined`; en `Revoked/Expired` queda según política (recomendado: setear cuando cambia de estado por operación de negocio).

### Invariantes
- No invitarse a sí mismo (`InvitedByUserId != TargetUserId`).
- No invitar si `TargetUserId` ya es miembro del grupo.
- No más de una invitación `Pending` por `(GroupId, TargetUserId)`.
- No aceptar/rechazar invitación vencida/revocada.

### Índices/constraints propuestos
- Índice por `TargetUserId, Status, ExpiresAt` para inbox del usuario.
- Índice por `GroupId, Status, ExpiresAt` para gestión en grupo.
- Constraint de unicidad parcial para pending por `(GroupId, TargetUserId)`.

## 2) `GroupMembership` (existente)

`GroupMembership` permanece como fuente de verdad de pertenencia al grupo.  
012 solo define que `Accepted` crea membresía si no existe.

### Regla de creación desde invitación
- En aceptar invitación:
  1. verificar `Pending` + no expirado + target coincide con usuario actual;
  2. crear membresía con `Role` de invitación si no existe;
  3. marcar invitación `Accepted`.

### Idempotencia
Si la invitación ya está `Accepted` o la membresía ya existe para el target:
- responder éxito idempotente (sin duplicar `GroupMembership`).

## 3) `User` (existente)

Se utiliza `Username` como clave funcional de búsqueda en UX (`@username`).  
No se expone `UserId`, `oid` o `tid` en UI de invitación.
