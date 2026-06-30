# Convenciones REST y OpenAPI — MiKompri

**Spec**: [plan.md](../plan.md) | **Vigente desde**: 2026-06-30

---

## 1. Estructura de URLs

### Patrón base
```
/api/v{N}/{recurso-en-kebab-case-plural}
```

### Ejemplos
| Operación | URL correcta |
|-----------|-------------|
| Listar listas | `GET /api/v1/purchase-lists` |
| Obtener lista por ID | `GET /api/v1/purchase-lists/{id}` |
| Crear lista | `POST /api/v1/purchase-lists` |
| Actualizar lista | `PUT /api/v1/purchase-lists/{id}` |
| Eliminar lista | `DELETE /api/v1/purchase-lists/{id}` |
| Agregar ítem a lista | `POST /api/v1/purchase-lists/{listId}/items` |
| Marcar ítem comprado | `PATCH /api/v1/purchase-lists/{listId}/items/{itemId}/purchase` |

### Reglas de URLs
- **kebab-case** y **plural** para todos los recursos: `purchase-lists`, `list-items`, `user-groups`.
- **Sub-recursos** representan relaciones de composición: `/purchase-lists/{id}/items`.
- **Acciones** como sustantivos en sub-recursos cuando no encajan en CRUD:
  `/purchase-lists/{id}/items/{itemId}/purchase` en lugar de verbos en la URL.
- Los **IDs** van en el path: `/purchase-lists/{id:guid}`.
- Los **filtros opcionales** van como query params: `?ownerId=...&groupId=...&page=1&pageSize=20`.
- **No** incluir el nombre del controlador ni del tipo en la URL:
  ✅ `/api/v1/purchase-lists` · ❌ `/api/v1/PurchaseLists` · ❌ `/api/v1/shoppinglists`

> ⚠️ **Deuda actual**: El endpoint existente usa `/api/v1/PurchaseLists` (PascalCase).
> El cambio a kebab-case es un breaking change. Se diferirá a `v2` para no romper
> clientes existentes. Documentar en ADR cuando se realice el cambio.

---

## 2. Versioning

- El número de versión va en el **path**: `/api/v1/`, `/api/v2/`.
- Incrementar la versión **mayor** solo cuando se introduzcan cambios incompatibles
  (campo eliminado, tipo cambiado, URL modificada).
- Las versiones antiguas DEBEN mantenerse activas durante al menos un ciclo de release
  antes de deprecarse.
- El header `Sunset` DEBE indicar la fecha de deprecación cuando se planifique:
  `Sunset: Sat, 01 Jan 2028 00:00:00 GMT`

---

## 3. Métodos HTTP y Semántica

| Método | Uso | Body request | Body response | Idempotente |
|--------|-----|-------------|---------------|-------------|
| `GET` | Consultar (no modifica estado) | No | Recurso o colección | Sí |
| `POST` | Crear un nuevo recurso | Sí (campos del recurso) | Id o recurso creado | No |
| `PUT` | Reemplazar recurso completo | Sí (todos los campos) | Recurso actualizado o vacío | Sí |
| `PATCH` | Modificación parcial o acción | Sí (campos parciales) | Recurso actualizado o vacío | No (generalmente) |
| `DELETE` | Eliminar recurso | No | Vacío | Sí |

---

## 4. Códigos HTTP de Respuesta

| Código | Cuándo usarlo |
|--------|---------------|
| `200 OK` | GET exitoso, PUT/PATCH exitoso con body de respuesta |
| `201 Created` | POST exitoso — DEBE incluir header `Location` apuntando al recurso creado |
| `204 No Content` | DELETE exitoso o PUT/PATCH sin body de respuesta |
| `400 Bad Request` | Validación fallida, datos de entrada inválidos, regla de dominio violada |
| `401 Unauthorized` | Token ausente o inválido (futuro MVP-1) |
| `403 Forbidden` | Token válido pero sin permisos para la operación (futuro MVP-2) |
| `404 Not Found` | Recurso no encontrado |
| `409 Conflict` | Recurso duplicado (ej: producto ya existe en la lista) |
| `422 Unprocessable Entity` | Datos bien formados pero semánticamente inválidos (alternativa a 400) |
| `500 Internal Server Error` | Error inesperado — nunca exponer stack traces en producción |

---

## 5. Formato de Respuesta de Error (RFC 9457 Problem Details)

Todos los errores DEBEN retornar un body con el formato [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457):

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "El producto ABC ya existe en esta lista de compra.",
  "instance": "/api/v1/purchase-lists/3fa85f64/items",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

| Campo | Obligatorio | Descripción |
|-------|-------------|-------------|
| `type` | Sí | URI que identifica el tipo de error |
| `title` | Sí | Descripción corta legible por humanos |
| `status` | Sí | Código HTTP numérico |
| `detail` | Recomendado | Descripción específica de la ocurrencia |
| `instance` | Recomendado | URI del recurso que causó el error |
| `traceId` | Sí (MiKompri) | Correlation ID para trazabilidad |

> El `ExceptionHandlingMiddleware` actual ya incluye `traceId` en las respuestas de error.
> Validar que el formato sea compatible con Problem Details en la próxima iteración.

---

## 6. Formato de Respuestas Exitosas

### Recurso individual
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Compra semanal",
  "ownerId": "...",
  "groupId": null,
  "progress": {
	"totalItems": 5,
	"purchasedItems": 2,
	"percentage": 40.0
  },
  "items": [...],
  "createdAt": "2026-06-30T12:00:00Z",
  "updatedAt": null
}
```

### Colección (sin paginación, estado actual)
```json
[
  { "id": "...", "name": "Compra semanal", ... },
  { "id": "...", "name": "Compra mensual", ... }
]
```

### Colección paginada (a implementar cuando sea necesario)
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalItems": 47,
  "totalPages": 3
}
```

---

## 7. Convenciones de Fechas y Tipos

- Fechas: **ISO 8601 UTC** → `"2026-06-30T12:00:00Z"`
- IDs: **GUID/UUID** en formato lowercase con guiones → `"3fa85f64-5717-4562-b3fc-2c963f66afa6"`
- Campos opcionales ausentes: `null` (no omitir la clave del JSON)
- Campos numéricos de precio: `decimal` representado como número JSON sin comillas
- Enums: **PascalCase** string → `"Owner"`, `"Admin"`, `"Member"`

---

## 8. Requisitos OpenAPI / Swagger

- El endpoint `/swagger` (UI) y `/swagger/v1/swagger.json` (spec) DEBEN estar disponibles
  en todos los entornos no productivos.
- Todos los endpoints DEBEN tener `summary` y al menos un `response` documentado en los
  atributos XML o mediante `[ProducesResponseType]`.
- La spec OpenAPI generada DEBEN ser válida (sin errores en el parser de Swashbuckle).
- Objetivo futuro: validar el schema OpenAPI en el pipeline CI.
- Los contratos de API documentados en `/specs/{NNN}-{feature}/contracts/` DEBEN
  mantenerse sincronizados con el schema OpenAPI generado.

---

## 9. Autenticación (futuro MVP-1)

- Todos los endpoints DEBEN requerir un **JWT Bearer token** válido tras MVP-1.
- El token DEBE incluir el claim `sub` (usuario) y opcionalmente `groups`.
- El header de autenticación: `Authorization: Bearer {token}`
- Los endpoints públicos (health check, swagger) DEBEN estar explícitamente excluidos
  de la autenticación con `[AllowAnonymous]`.
