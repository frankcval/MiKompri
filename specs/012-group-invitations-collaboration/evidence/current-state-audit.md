# Auditoría de estado actual — 012 Group Invitations Collaboration

**Fecha**: 2026-11-03  
**Base**: `main` remoto (`f0a1643`)  
**Objetivo**: identificar qué existe hoy para incorporación a grupos y qué falta para invitaciones colaborativas.

## 1) Estado actual backend (Users)

### Endpoints actuales de grupos
- `POST /api/v1/groups` crea grupo.
- `GET /api/v1/groups` lista grupos del usuario.
- `GET /api/v1/groups/{groupId}/members` lista membresías del grupo.
- `POST /api/v1/groups/{groupId}/members` añade miembro **directamente** con `UserId` y `Role`.
- `DELETE /api/v1/groups/{groupId}/members/{userId}` elimina membresía.

### Hallazgos
1. **No existe entidad `GroupInvitation`** en dominio/persistencia.
2. La incorporación al grupo se hace hoy por alta directa de membresía (`AddMemberToGroupCommand`).
3. El contrato actual exige `UserId` en API (`AddGroupMemberRequest`), no `@username`.
4. Hay soporte de username en Users (`User.Username` + índice único + `GetByUsernameAsync`), pero no endpoint público de búsqueda para invitaciones.
5. No hay estados de invitación, expiración, revocación ni idempotencia de aceptación.
6. No hay notificación in-app de invitaciones.

## 2) Estado actual frontend (Web)

### Capacidad vigente
- UI de grupos permite crear grupo y visualizar miembros/roles.
- UI de shared-lists consume membresías para permisos y personas.

### Hallazgos
1. No existe flujo de "invitar por username".
2. No existe inbox de invitaciones pendientes/aceptadas/rechazadas.
3. No existe aceptación/rechazo en cliente.
4. No existe revocación de invitaciones pendientes.

## 3) Seguridad e identidad

- El backend usa usuario actual desde contexto autenticado/JWT (`ICurrentUserService`).
- No hay necesidad de exponer `oid`, `tid` o `UserId` en UX de invitaciones.
- 010 define identidad canónica y se mantiene vigente; 012 no cambia autenticación.

## 4) Solapamientos detectados con 013

- 013 trata UX de shared-lists (Compra/Gastos/Historial/Personas) una vez que la membresía existe.
- 012 debe cubrir cómo una persona **entra** al grupo y por tanto habilita acceso a listas compartidas.
- Zona de frontera: pestaña Personas en 013 permanece de lectura; acciones de invitación/aceptación se mueven a 012.

## 5) Conclusión de auditoría

El repositorio no tiene todavía el ciclo de invitaciones. El modelo actual de membresía directa por `UserId` es funcional para tests internos, pero no cumple el flujo de producto requerido (invitar por `@username`, pending, aceptar/rechazar, revocar, expiración y concurrencia). 012 cubre precisamente ese hueco.
