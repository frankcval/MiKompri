# REST API Contract: Users Authentication & Groups

**Feature**: 003-users-authentication | **Version**: v1 | **Date**: 2026-07-12
**Base URL**: `/api/v1`
**Auth**: `Authorization: Bearer <jwt>` en todos los endpoints marcados con 🔒

---

## Convenciones generales

- Todos los timestamps en ISO 8601 UTC: `"2026-07-12T10:30:00Z"`
- Respuestas de error: estructura compartida con ShoppingList (middleware existente)
- `GroupRole` como string: `"Owner"` | `"Admin"` | `"Member"`
- Errores de validación → `400`; No autenticado → `401`; Sin permiso → `403` (mapeado desde `InvalidOperationException`); No encontrado → `404`

### Forma de error estándar

```json
{
  "status": 400,
  "error": "La petición no es válida",
  "traceId": "0HMXXXXXX:00000001",
  "errors": [
	{ "field": "DisplayName", "message": "El nombre no puede estar vacío." }
  ]
}
```

Para errores sin lista de campos (`404`, `403`, `500`):
```json
{
  "status": 404,
  "error": "Grupo no encontrado.",
  "traceId": "0HMXXXXXX:00000002"
}
```

---

## Perfil de usuario (`/users`)

### `GET /api/v1/users/me` 🔒

Devuelve el perfil local del usuario autenticado. Si es su primer request autenticado, el middleware auto-provisiona el perfil antes de que el controller responda.

**Respuesta `200 OK`**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "displayName": "Ana García",
  "email": "ana@example.com",
  "identityProvider": "entra",
  "externalUserId": "auth0|abc123",
  "createdAt": "2026-07-12T10:00:00Z",
  "updatedAt": "2026-07-12T10:00:00Z"
}
```

**Errores**

| Código | Cuándo |
|--------|--------|
| `401` | Sin token o token inválido |

---

### `PUT /api/v1/users/me` 🔒

Actualiza el nombre visible del usuario autenticado. El usuario controla su nombre, independientemente del IdP.

**Request body**

```json
{
  "displayName": "Ana García López"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|-----------|
| `displayName` | `string` | ✅ | No vacío, no solo espacios, máx. 100 caracteres |

**Respuesta `200 OK`** — perfil actualizado (misma forma que `GET /users/me`)

**Errores**

| Código | Cuándo |
|--------|--------|
| `400` | `displayName` vacío o solo espacios |
| `401` | Sin autenticación |

---

### `POST /api/v1/users/me/refresh` 🔒

Refresca el perfil local con los claims actuales del token JWT. El usuario usa este endpoint cuando ha actualizado sus datos en el proveedor de identidad.

**Request body**: vacío (`{}` o sin body)

**Respuesta `200 OK`** — perfil actualizado (misma forma que `GET /users/me`)

**Errores**

| Código | Cuándo |
|--------|--------|
| `401` | Sin autenticación |

---

## Grupos (`/groups`)

### `POST /api/v1/groups` 🔒

Crea un nuevo grupo. El caller se convierte en Owner automáticamente.

**Request body**

```json
{
  "name": "Familia García"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|-----------|
| `name` | `string` | ✅ | No vacío, máx. 100 caracteres |

**Respuesta `201 Created`**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Familia García",
  "createdAt": "2026-07-12T10:05:00Z",
  "updatedAt": "2026-07-12T10:05:00Z"
}
```

Header: `Location: /api/v1/groups/7c9e6679-7425-40de-944b-e07fc1f90ae7/members`

**Errores**

| Código | Cuándo |
|--------|--------|
| `400` | `name` vacío |
| `401` | Sin autenticación |

---

### `GET /api/v1/groups` 🔒

Devuelve los grupos en los que el usuario autenticado es miembro (cualquier rol).

**Respuesta `200 OK`**

```json
[
  {
	"id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
	"name": "Familia García",
	"myRole": "Owner",
	"memberCount": 3,
	"createdAt": "2026-07-12T10:05:00Z"
  }
]
```

---

### `GET /api/v1/groups/{groupId}/members` 🔒

Devuelve la lista de miembros del grupo. Requiere que el caller sea miembro del grupo.

**Path params**: `groupId` (UUID)

**Respuesta `200 OK`**

```json
[
  {
	"userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	"displayName": "Ana García",
	"role": "Owner",
	"joinedAt": "2026-07-12T10:05:00Z"
  },
  {
	"userId": "1b9e6679-1234-40de-944b-e07fc1f90ae7",
	"displayName": "Carlos García",
	"role": "Member",
	"joinedAt": "2026-07-12T11:00:00Z"
  }
]
```

**Errores**

| Código | Cuándo |
|--------|--------|
| `401` | Sin autenticación |
| `403` | Caller no es miembro del grupo |
| `404` | Grupo no encontrado |

---

### `POST /api/v1/groups/{groupId}/members` 🔒

Agrega un nuevo miembro al grupo.

- **Owner** puede especificar rol `"Admin"` o `"Member"`.
- **Admin** solo puede especificar rol `"Member"`.
- **Member** no puede usar este endpoint → `403`.

**Path params**: `groupId` (UUID)

**Request body**

```json
{
  "userId": "1b9e6679-1234-40de-944b-e07fc1f90ae7",
  "role": "Member"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|-----------|
| `userId` | `uuid` | ✅ | Debe existir como perfil local |
| `role` | `string` | ✅ | `"Admin"` o `"Member"` (nunca `"Owner"`) |

**Respuesta `201 Created`**

```json
{
  "userId": "1b9e6679-1234-40de-944b-e07fc1f90ae7",
  "displayName": "Carlos García",
  "role": "Member",
  "joinedAt": "2026-07-12T11:00:00Z"
}
```

**Errores**

| Código | Cuándo |
|--------|--------|
| `400` | `userId` o `role` inválidos; usuario ya miembro |
| `401` | Sin autenticación |
| `403` | Caller es Member; o Admin intentando asignar rol Admin |
| `404` | Grupo o usuario no encontrado |

---

### `DELETE /api/v1/groups/{groupId}/members/{userId}` 🔒

Elimina a un miembro del grupo.

- **Owner** puede eliminar a cualquier miembro excepto el último Owner.
- **Admin** solo puede eliminar a miembros con rol Member.
- **Member** no puede usar este endpoint → `403`.

**Path params**: `groupId` (UUID), `userId` (UUID)

**Respuesta `204 No Content`**

**Errores**

| Código | Cuándo |
|--------|--------|
| `401` | Sin autenticación |
| `403` | Caller sin permiso (Member; Admin intentando eliminar Admin u Owner) |
| `404` | Grupo o miembro no encontrado |
| `400` | Intento de eliminar al único Owner del grupo |

---

## Swagger / OpenAPI

Registrar Bearer en `AddSwaggerGen`:

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
	Name         = "Authorization",
	Type         = SecuritySchemeType.Http,
	Scheme       = "bearer",
	BearerFormat = "JWT",
	In           = ParameterLocation.Header
});
c.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });
```

El endpoint de documentación debe estar disponible en `/swagger` para entornos no productivos (TP7).
