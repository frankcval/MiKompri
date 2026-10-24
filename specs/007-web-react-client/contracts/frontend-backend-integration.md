# Contract — Frontend/Backend Integration (007-web-react-client)

## Scope
Contrato de integración del cliente Web SPA con APIs existentes de MiKompri (`Users`, `ShoppingList`, `ProductCatalog`) manteniendo separación de bounded contexts.

## 1. Authentication Contract
- **Provider**: Microsoft Entra ID.
- **Flow**: Authorization Code + PKCE.
- **Client**: SPA registrada de forma independiente.
- **Token Use**: Bearer token por request a backend.
- **Non-Negotiable Rules**:
  - Sin client secret en frontend.
  - `(tid, oid)` y audience común de backend son decisiones cerradas.

## 2. Request Contract
- Header requerido: `Authorization: Bearer <access_token>`
- Header recomendado: `Content-Type: application/json`
- CORS backend debe permitir origen SPA por ambiente (local/cloud) y método/headers necesarios.

## 3. Response/Error Contract
- **200/201/204**: operación exitosa
- **400**: validación/negocio (mostrar mensaje accionable y errores de campo si aplica)
- **401**: sesión inválida/expirada (forzar reauth)
- **403**: autenticado sin permisos (mostrar estado forbidden)
- **404**: recurso no encontrado (estado not found)
- **409**: conflicto de negocio (mostrar mensaje y preservar contexto de formulario)
- **503**: servicio no disponible (mostrar estado y opción de reintento)

## 4. Bounded Context Separation
- `Users`: identidad/perfil/grupos/membresías.
- `ShoppingList`: listas personales/compartidas, ítems, gastos, balances, settlement.
- `ProductCatalog`: consulta read-only de productos/mercados/historial.
- El frontend no accede a bases de datos ni lógica interna cross-context.

## 5. Backend Prerequisites (Blocking)
1. Users migra correlación `sub -> (tid, oid)`.
2. Users mantiene `ExternalUserId` nullable en transición.
3. Backend mantiene audience común y validaciones homogéneas JWT.
4. ShoppingList autoriza listas personales por identidad autenticada y deriva `OwnerId`.
5. ProductCatalog aplica autenticación JWT real.

## 6. Deployment Compatibility (Future Azure SWA)
- Configuración por variables de entorno de frontend.
- Redirect URIs separados para local y cloud.
- Sin secretos en bundle.
- Compatible con despliegue en Azure Static Web Apps (preferente) cuando se habilite la fase cloud.

## 7. Personal Lists Read Contract (verified 2026-10-25)

- `GET /api/v1/purchaselists/mine` requires the shared API Bearer token. ShoppingList preserves original Entra `tid`/`oid` claims (`MapInboundClaims = false`) and resolves the internal user through Users API.
- Ownership comes from the resolved authenticated user, never a client-supplied `ownerId`. Only that user's personal lists (`groupId == null`) are returned; shared lists and other owners' lists are excluded.
- Response: `200` with an array of `PurchaseListDTO` objects; `[]` is the successful empty state. Missing/invalid authentication or missing canonical identity returns `401`.
- Local Development supports the configured HTTP base URL `http://localhost:8080` directly. Redirecting this request to HTTPS on port 8081 changes origin and strips the Bearer header in browsers. HTTPS redirection remains enabled outside Development.
- Regression: `PersonalListsBearerTests` uses signed JWTs with the real Bearer handler, explicit HTTPS port, disabled automatic redirects, and allowed-origin CORS checks. Full ShoppingList API suite: 64 passed, 0 failed, 0 skipped. Browser: Vite + existing Entra session returns `200 []` at 375/430/1280 px with no new console errors or failed requests. Populated-list UI and mutations remain outside this verification.

## 8. Personal List Selection and Item Creation (verified 2026-10-25)

- `/personal-lists` is an index with no item editor. A list link opens `/personal-lists/:listId`; URL, reload and browser history preserve that selection. Only a list returned by the authenticated account's `/mine` response enables the editor. Creating a list opens its own returned ID.
- `POST /api/v1/purchaselists/{listId}/items` requires `{ productId, productName, quantity, price }`: `productId` is a non-empty catalog UUID, `productName` is the selected product's display name, quantity is a positive integer and price is non-negative. `productId: null` fails model binding with HTTP 400 even when the list ID is correct.
- Products come from `GET /api/v1/catalog-products`. An empty/unavailable catalog must not offer an invalid write. The UI displays an explanation or retry, preserves values on write failure, prevents pending duplicates and refreshes the selected list after success.
- Browser verification used a dedicated product/list fixture: real HTTP 201 for list and item, quantity 2 and price 1.25, item visible after refresh. The test list was deleted and product deactivated afterwards (204 each); pre-existing lists were not changed. Actual catalog provisioning remains necessary for normal use when no active products exist.
