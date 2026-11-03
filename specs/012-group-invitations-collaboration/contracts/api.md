# Contratos API — 012 Group Invitations Collaboration

Base path: `/api/v1/groups`

## 1) Buscar usuario por username

### GET `/api/v1/users/search?username={value}`

Uso: búsqueda exacta normalizada de `@username` para invitar (trim, sin `@` inicial, comparación case-insensitive).  
No devuelve identificadores internos.

**200 OK**
```json
{
  "username": "ana",
  "displayName": "Ana Pérez",
  "isActive": true
}
```

**404 Not Found**
- username no existente o no elegible.

**Reglas**
- No retornar `id`, `oid`, `tid`, `tenant`, etc.
- Sanitizar prefijo `@` en input.
- Si el username normalizado no corresponde exactamente a un usuario elegible, devolver `404`.

---

## 2) Crear invitación

### POST `/api/v1/groups/{groupId}/invitations`

**Body**
```json
{
  "targetUsername": "@ana",
  "role": "Member"
}
```

**201 Created**
```json
{
  "invitationId": "GUID",
  "groupId": "GUID",
  "groupName": "Casa",
  "target": {
	"username": "ana",
	"displayName": "Ana Pérez"
  },
  "role": "Member",
  "status": "Pending",
  "createdAt": "2026-11-03T12:00:00Z",
  "expiresAt": "2026-11-10T12:00:00Z"
}
```

**Errores esperados**
- `400` self-invite, role inválido, username inválido.
- `403` caller sin privilegio en grupo (solo Owner/Admin invitan).
- `404` grupo no accesible o usuario no encontrado (según política anti-enumeración).
- `409` ya existe invitación pending o usuario ya miembro.

---

## 3) Listar mis invitaciones

### GET `/api/v1/groups/invitations/me?status=Pending|Accepted|Declined|Expired|Revoked`

**200 OK**
```json
[
  {
	"invitationId": "GUID",
	"groupId": "GUID",
	"groupName": "Casa",
	"invitedBy": {
	  "username": "carlos",
	  "displayName": "Carlos"
	},
	"role": "Member",
	"status": "Pending",
	"createdAt": "2026-11-03T12:00:00Z",
	"expiresAt": "2026-11-10T12:00:00Z"
  }
]
```

---

## 4) Aceptar invitación

### POST `/api/v1/groups/invitations/{invitationId}/accept`

Sin body. Usuario actual del JWT.

**200 OK** (idempotente)
```json
{
  "invitationId": "GUID",
  "status": "Accepted",
  "membership": {
	"groupId": "GUID",
	"role": "Member",
	"joinedAt": "2026-11-03T12:05:00Z"
  }
}
```

**Matriz de respuesta determinista (accept)**
- `200` si estado actual es `Pending` (transiciona a `Accepted` y crea membresía) o `Accepted` (idempotente).
- `409` si estado actual es `Declined`, `Revoked` o `Expired`.
- `403` si la invitación existe pero el target no coincide con el usuario autenticado.
- `404` si la invitación no existe o no es visible por política anti-enumeración.

---

## 5) Rechazar invitación

### POST `/api/v1/groups/invitations/{invitationId}/decline`

Sin body.

**200 OK** (idempotente)
```json
{
  "invitationId": "GUID",
  "status": "Declined",
  "respondedAt": "2026-11-03T12:05:00Z"
}
```

**Matriz de respuesta determinista (decline)**
- `200` si estado actual es `Pending` (transiciona a `Declined`) o `Declined` (idempotente).
- `409` si estado actual es `Accepted`, `Revoked` o `Expired`.
- `403` si la invitación existe pero el target no coincide con el usuario autenticado.
- `404` si la invitación no existe o no es visible por política anti-enumeración.

---

## 6) Revocar invitación pending

### POST `/api/v1/groups/invitations/{invitationId}/revoke`

Sin body.

**200 OK**
```json
{
  "invitationId": "GUID",
  "status": "Revoked"
}
```

**Matriz de respuesta determinista (revoke)**
- `200` si estado actual es `Pending` (transiciona a `Revoked`) o `Revoked` (idempotente).
- `409` si estado actual es `Accepted`, `Declined` o `Expired`.
- `403` si el actor no tiene permisos de revocación en el grupo.
- `404` si la invitación no existe o no es visible por política anti-enumeración.

**Reglas**
- Solo inviter con permisos vigentes o Owner/Admin del grupo.
- Solo sobre `Pending`.
- No afecta membresías ya creadas.

---

## Reglas de expiración y tiempo

1. `ExpiresAt` se calcula como `CreatedAt + 7 días` en UTC.
2. Una invitación está expirada cuando `UtcNow >= ExpiresAt`.
3. Estados `Expired` devuelven `409` en `accept`, `decline` y `revoke`.

## Reglas de seguridad y privacidad de contratos

1. Actor siempre inferido desde JWT.
2. No exponer identificadores internos de identidad en UI/DTO públicos.
3. No usar email para correlacionar identidad.
4. Auditar `traceId` y errores consistentes con middleware actual.
