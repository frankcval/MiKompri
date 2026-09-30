# Contracts: MVP-4 Cliente Android con .NET MAUI

Este documento describe, a nivel de contrato HTTP, las interfaces que el cliente Android consume de las tres APIs backend, y los ajustes de contrato necesarios para cumplir TP11 y el endurecimiento de `OwnerId`. No incluye código de implementación; es una referencia de forma/comportamiento esperado para guiar `tasks.md` y el desarrollo posterior.

## Convención general

- Todas las rutas usan el prefijo ya existente `api/v1/`.
- Todas las operaciones sobre recursos protegidos requieren `Authorization: Bearer <token>` válido (issuer/audience/firma/expiración validados por cada API, TP11).
- Errores de autenticación → `401`. Errores de autorización de negocio (owner/rol no coincide) → `403` (o `404` si se prefiere no revelar existencia del recurso, según convención ya usada en `ExceptionHandlingMiddleware`).

## `Users.Api`

### Perfil propio

| Método | Ruta | Cambio respecto al contrato actual |
|---|---|---|
| `GET` | `/api/v1/users/me` (o equivalente ya existente) | Sin cambios de forma; la resolución de identidad interna ahora puede ejecutar la lógica de Fase 1/Fase 2 de forma transparente al cliente. |
| `PATCH` | `/api/v1/users/me` | Sin cambios de forma (actualización de `DisplayName`). |

### Grupos (ya expuestos, consumidos por primera vez desde un cliente propio)

| Método | Ruta | Notas |
|---|---|---|
| `GET` | `/api/v1/groups` | Devuelve grupos del usuario autenticado con su rol (`GroupDto` ya incluye rol). Sin cambios de contrato. |
| `POST` | `/api/v1/groups` | Crea grupo; el creador queda como `Owner`. Sin cambios de contrato. |
| `GET` | `/api/v1/groups/{groupId}/members` | Lista miembros con rol. Sin cambios de contrato. |
| `POST` | `/api/v1/groups/{groupId}/members` | Añade miembro con rol `Member` o `Admin`, según reglas de Spec 003. Sin cambios de contrato. |
| `DELETE` | `/api/v1/groups/{groupId}/members/{userId}` (o equivalente ya existente) | Elimina miembro según reglas de autorización de Spec 003. Sin cambios de contrato. |

No se añaden endpoints de administración avanzada de grupo ni de cambio de rol (fuera de alcance, FR-016c).

## `ShoppingList.Api`

### Listas personales (cambio de contrato para TP11 / FR-026 / FR-027)

| Método | Ruta | Contrato actual | Contrato ajustado |
|---|---|---|---|
| `POST` | `/api/v1/PurchaseLists` | Requiere `OwnerId` en el body (`CreatePurchaseListRequest`); sin `[Authorize]` explícito. | `OwnerId` **se elimina del body**; requiere `[Authorize]`; el `OwnerId` se deriva de `ICurrentUserService.UserId`. |
| `GET` | `/api/v1/PurchaseLists?ownerId={guid}` | Acepta `ownerId` arbitrario por query string. | Para listas personales, requiere `[Authorize]`; se ignora/rechaza un `ownerId` de query que no coincida con el usuario autenticado, o se elimina el parámetro y se infiere siempre del token. |
| `PATCH`/`PUT` (edición) | `/api/v1/PurchaseLists/{id}` | — | Requiere `[Authorize]`; rechaza con error de autorización si `PurchaseList.OwnerId` ≠ `UserId` autenticado. |
| `DELETE` | `/api/v1/PurchaseLists/{id}` | — | Requiere `[Authorize]`; misma regla de comparación de `OwnerId`. |

### Ítems de lista

Sin cambios de contrato respecto al comportamiento actual; la autorización de pertenencia a la lista (personal o compartida) sigue las reglas ya vigentes, extendidas transitivamente por el endurecimiento de `OwnerId` en listas personales.

### Listas compartidas, gastos, balances y settlement

Sin cambios de contrato — ya están protegidas por `[Authorize]` y por las reglas de pertenencia a grupo definidas en Spec 005. El cliente consume:

- `POST /api/v1/shared-lists`, `GET /api/v1/shared-lists/{id}`, `GET /api/v1/shared-lists?groupId={guid}`, `PATCH /api/v1/shared-lists/{id}`, `PATCH /api/v1/shared-lists/{id}/close`.
- `POST /api/v1/shared-lists/{id}/items`, `POST /api/v1/shared-lists/{id}/items/{itemId}/expenses`, `PATCH`/`DELETE` de expenses.
- `GET /api/v1/shared-lists/{id}/settlement/summary`, `GET /api/v1/shared-lists/{id}/settlement/proposal`.

## `ProductCatalog.Api`

Contratos de consulta ya existentes, consumidos en modo **solo lectura** (FR-016d, FR-011):

| Método | Ruta (indicativa, ver contrato real ya publicado en Swagger) | Notas |
|---|---|---|
| `GET` | `/api/v1/products` (activos) | Sin cambios de contrato. |
| `GET` | `/api/v1/markets` (activos) | Sin cambios de contrato. |
| `GET` | `/api/v1/products/{id}/price-history` | Sin cambios de contrato. |

No se documentan ni se implementan endpoints de escritura para este bounded context en este MVP.

## Audience común (transición TP11)

Durante la Fase 2 de migración de identidad, `ShoppingList.Api` y `ProductCatalog.Api` adoptan el mismo mecanismo `Authentication:ValidAudiences` ya presente en `Users.Api`, de modo que las tres APIs acepten simultáneamente:

1. La audience histórica de cada API (mientras existan tokens/clientes antiguos en circulación).
2. La audience de backend común definida por TP11.

Este es un cambio de configuración (`appsettings` / variables de entorno), no de forma del contrato REST en sí.
