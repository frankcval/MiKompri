# Research — 012 Group Invitations Collaboration

## R1 — Separar invitación de membresía directa

**Decisión**: introducir `GroupInvitation` como recurso explícito, sin eliminar de inmediato la infraestructura de membresía.  
**Motivo**: encapsular pending/accepted/declined/expired/revoked y trazabilidad.  
**Impacto**: el alta efectiva de `GroupMembership` ocurre al aceptar invitación.

## R2 — Identidad objetivo por `@username` (no UserId visible)

**Decisión**: la UI opera con `username` y display name; backend resuelve `TargetUserId`.  
**Motivo**: evitar exposición de identificadores internos y cumplir requisito de UX.  
**Impacto**: se requiere endpoint de búsqueda/validación de usuario por username.

## R3 — Autoridad de usuario actual siempre desde JWT

**Decisión**: `InvitedByUserId`, acciones de aceptar/rechazar/revocar y permisos derivan del usuario autenticado backend.  
**Motivo**: evitar spoofing de actor en payload.  
**Impacto**: contratos no aceptan IDs de actor provenientes del cliente.

## R4 — Estados de invitación cerrados

**Decisión**: usar estados `Pending | Accepted | Declined | Expired | Revoked`.  
**Motivo**: cubrir ciclo completo y evitar ambigüedades en lógica de negocio y UI.

## R5 — Idempotencia en aceptación/rechazo

**Decisión**: aceptar/rechazar debe ser idempotente por invitación y usuario objetivo.  
**Motivo**: doble click, retry de red y concurrencia de cliente.

## R6 — Concurrencia y unicidad

**Decisión**:
- evitar duplicados pending para mismo `(GroupId, TargetUserId)`;
- transición de estado con control transaccional/optimista.

**Motivo**: prevenir carreras (dos invitaciones simultáneas o aceptación/revocación concurrentes).

## R7 — Expiración explícita

**Decisión**: definir `ExpiresAt` y comportamiento de expirado como no aceptable/rechazable por cliente.  
**Motivo**: higiene de invitaciones y reducción de deuda operativa.

## R8 — 012 vs 013

**Decisión**: 012 gestiona incorporación y estado de invitaciones; 013 no reimplementa ni duplica estos flujos.  
**Motivo**: evitar solapamiento de especificaciones y churn de UX.
