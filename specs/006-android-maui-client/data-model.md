# Data Model: MVP-4 Cliente Android con .NET MAUI

Este documento describe las entidades relevantes para esta feature: los ajustes al modelo de dominio backend (`Users`, `ShoppingList`) necesarios para TP11, y las entidades/proyecciones del lado del cliente (MAUI). No se introducen nuevos bounded contexts; se documentan extensiones acotadas a entidades ya existentes.

## Backend — Ajustes a entidades existentes

### `User` (bounded context `Users`, `MiKompri.Users.Domain.Users.User`)

Campos actuales (sin cambios):
- `Id: Guid` — `UserId` interno de MiKompri, identidad canónica estable.
- `DisplayName: string`
- `Email: string?`
- `IdentityProvider: string` — hoy usado para el IdP (`"entra"`, etc.).
- `ExternalUserId: string` — hoy almacena el claim `sub`. Pasa a tratarse como **dato legacy de solo lectura** tras completar la Fase 2 de migración (FR-021).

Campos nuevos (Fase 1):
- `TenantId: string?` (`tid`) — nullable hasta que se resuelva para el perfil.
- `ObjectId: string?` (`oid`) — nullable hasta que se resuelva para el perfil.

Reglas de validación / invariantes:
- No pueden coexistir dos perfiles `User` con el mismo `(TenantId, ObjectId)` no nulo (invariante de unicidad de correlación canónica).
- No pueden coexistre dos perfiles `User` con el mismo `ExternalUserId` no vacío para el mismo `IdentityProvider` (invariante ya implícita hoy).
- Nunca se crea un `User` nuevo si ya existe una fila correlacionable por `(TenantId, ObjectId)` o, transitoriamente, por `ExternalUserId` (FR-020).

Transiciones de estado (conceptual, no una máquina de estados formal):
- `Legacy (solo sub)` → `Convivencia (sub + tid/oid)` [Fase 1] → `Canónico (tid/oid, sub legacy)` [Fase 2].
- `Nuevo (solo tid/oid)` — usuarios que se registran después de iniciada la Fase 1 no pasan por el estado `Legacy`.

### `PurchaseList` (bounded context `ShoppingList`, `MiKompri.ShoppingList.Domain.Entities.PurchaseList`)

Sin cambios de estructura; `OwnerId: Guid` ya existe y ya es inmutable tras construcción (no hay método `ChangeOwner`). El cambio es exclusivamente en la **capa de aplicación/API**: de dónde proviene el valor de `OwnerId` al construir/consultar la entidad (se resuelve desde `ICurrentUserService.UserId`, nunca desde el payload del cliente), no en el modelo de dominio en sí.

### `Group` / `GroupMembership` (bounded context `Users`)

Sin cambios de estructura para esta spec. El cliente únicamente consume los contratos ya existentes de `GroupsController` (crear grupo, listar grupos propios, ver miembros, añadir/eliminar miembro), respetando la matriz de roles Owner/Admin/Member ya definida en Spec 003.

## Cliente (MAUI) — Modelos y proyecciones

Estas entidades viven únicamente en `MiKompri.Mobile` y son proyecciones de lectura/escritura de los contratos de API; no son entidades de dominio backend.

### `SesionCliente` (ya descrita en `spec.md` § Key Entities)

- `AccountIdentifier` — referencia opaca a la cuenta cacheada por MSAL (no el token en sí).
- `EstadoSesion`: `Activa | Expirada | Cerrada` (estado conceptual derivado de MSAL, no persistido por la app).

### `PerfilUsuarioCliente`

- `UserId: Guid` (no editable).
- `DisplayName: string` (editable).
- `Email: string?` (solo lectura, sincronizado por `Users.Api`).

### `ListaPersonalCliente` / `ItemListaCliente`

- Proyección de `PurchaseListDTO` / ítems ya expuestos por `ShoppingList.Api`. El cliente nunca construye ni envía `OwnerId` al crear una lista.

### `GrupoCliente` / `MiembroGrupoCliente`

- Proyección de `GroupDto` / `GroupMemberDto` ya expuestos por `Users.Api`, incluyendo `Rol: Owner | Admin | Member` para mostrar en UI.

### `ProductoCatalogoCliente` / `MercadoCliente` / `HistorialPrecioCliente`

- Proyecciones de solo lectura de los DTOs ya expuestos por `ProductCatalog.Api`; no existen comandos de escritura del lado del cliente para estas entidades (FR-016d).

## Relaciones clave

- `PerfilUsuarioCliente.UserId` corresponde siempre al `UserId` interno resuelto por el backend a partir de `(tid, oid)` o, durante la Fase 1, por `sub` — el cliente nunca conoce ni maneja `tid`/`oid` directamente; solo obtiene tokens de MSAL y consume `Users.Api` para su perfil.
- `ListaPersonalCliente.OwnerId` (mostrado en UI, si aplica) siempre coincide con `PerfilUsuarioCliente.UserId` del usuario autenticado; cualquier discrepancia debe tratarse como error de autorización del backend, nunca resuelta localmente en el cliente.
