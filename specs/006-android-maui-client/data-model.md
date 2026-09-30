# Data Model: MVP-4 Cliente Android con .NET MAUI

Este documento describe las entidades relevantes para esta feature: los ajustes al modelo de dominio backend (`Users`, `ShoppingList`) necesarios para TP11, y las entidades/proyecciones del lado del cliente (MAUI). No se introducen nuevos bounded contexts; se documentan extensiones acotadas a entidades ya existentes.

## Backend — Ajustes a entidades existentes

### `User` (bounded context `Users`, `MiKompri.Users.Domain.Users.User`)

Campos actuales (con ajuste):
- `Id: Guid` — `UserId` interno de MiKompri, identidad canónica estable.
- `DisplayName: string`
- `Email: string?`
- `IdentityProvider: string` — hoy usado para el IdP (`"entra"`, etc.).
- `ExternalUserId: string?` — pasa a ser **nullable** desde la Fase 1. Almacena el claim `sub` únicamente para perfiles legacy ya correlacionados por ese medio antes de esta spec. Se trata como **dato legacy de solo lectura** desde la Fase 1 en adelante (no solo tras la Fase 2): no se usa para nuevas correlaciones de usuarios nuevos, y nunca se sustituye por cadena vacía (`""`) cuando no aplica — el valor ausente siempre es `null`.

Campos nuevos (Fase 1):
- `TenantId: string?` (`tid`) — nullable hasta que se resuelva para el perfil.
- `ObjectId: string?` (`oid`) — nullable hasta que se resuelva para el perfil.

Reglas de validación / invariantes:
- No pueden coexistir dos perfiles `User` con el mismo `(TenantId, ObjectId)` no nulo (invariante de unicidad de correlación canónica, implementada como índice único filtrado en EF Core/PostgreSQL que excluye `NULL`).
- No pueden coexistir dos perfiles `User` con el mismo `ExternalUserId` no nulo para el mismo `IdentityProvider` (invariante ya implícita hoy; el índice único correspondiente pasa a ser filtrado para excluir `NULL`, de modo que múltiples perfiles sin `ExternalUserId` no generen conflicto).
- Un usuario nuevo creado directamente por `(tid, oid)` (sin correlación previa por `sub`) NO requiere `ExternalUserId`; dicho campo permanece `null` para ese perfil de forma permanente, salvo que en el futuro se asocie un login legacy adicional (fuera de alcance de este MVP).
- Nunca se crea un `User` nuevo si ya existe una fila correlacionable por `(TenantId, ObjectId)` o, transitoriamente, por `ExternalUserId` (FR-020).

Migración EF Core requerida (ver `plan.md` § "`ExternalUserId` legacy y migración EF Core"):
1. Alterar columna `ExternalUserId` a nullable (si no lo era).
2. Añadir columnas `TenantId`/`ObjectId` (nullable).
3. Reemplazar el índice único existente sobre `ExternalUserId` (si aplica) por un índice único filtrado que excluya `NULL`.
4. Añadir índice único filtrado sobre `(TenantId, ObjectId)` que excluya filas con cualquiera de los dos campos en `NULL`.
5. La migración solo cambia esquema; no ejecuta backfill de datos (el backfill, si se decide, es un proceso operativo aparte, ver criterio de entrada a Fase 2 en `plan.md`).

Transiciones de estado (conceptual, no una máquina de estados formal):
- `Legacy (solo sub)` → `Convivencia (sub + tid/oid, vía autenticación lazy)` [Fase 1, solo si el usuario autentica] → `Canónico (tid/oid, sub legacy)` [Fase 2, tras cumplir criterio de cobertura o backfill].
- `Legacy nunca migrado (solo sub, sin tid/oid)` — estado terminal legítimo para perfiles que nunca vuelven a autenticar durante la Fase 1; no es un error y no bloquea la entrada a Fase 2 si se cumple el criterio de cobertura de perfiles activos.
- `Nuevo (solo tid/oid, sin ExternalUserId)` — usuarios que se registran después de iniciada la Fase 1 no pasan por el estado `Legacy` y nunca reciben un valor de `ExternalUserId`.

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
