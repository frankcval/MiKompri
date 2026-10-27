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
- `ProductCatalog`: consulta de productos/mercados/historial y alta de productos según sección 9; sin gestión de mercados/precios ni edición/desactivación desde UI.
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

### Presentación web vigente (2026-10-27)

[design.md](../design.md) gobierna la presentación y navegación. La portada pública no concede acceso a las APIs; las rutas de negocio conservan MSAL/ProtectedRoute. Los selectores muestran nombres de grupos, miembros, productos y mercados, mientras envían los IDs del contrato. Formatear fechas, importes y progreso no cambia payloads, roles ni cálculos de reparto. Los ejemplos de portada y el harness de pruebas nunca son respuestas backend reales. Esta alineación documental no modifica endpoints, scopes ni configuración Entra.

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

## 9. Catalog Product Creation (scope extended 2026-10-25)

- The Web catalog now includes a **Crear producto** form. `POST /api/v1/catalog-products` sends Bearer auth and `{ name, purchaseUnit }`; trimmed non-empty name (max 120) and unit (max 30) are required. The existing API returns the created product DTO with HTTP 201; duplicate active name/unit returns 409.
- Creation invalidates the account's shared `catalog-products` query so the product appears both in the catalog and in item selectors. Errors preserve inputs, pending requests block duplicate submission, and success is announced.
- A selected personal list links to `/product-catalog?returnTo=/personal-lists/{listId}`. Only internal personal-list destinations are accepted; **Volver a la lista** restores the selected list. No item is automatically added when a product is created.
- This extends the original read-only catalog scope only to product creation, per the user's request. Markets, price history and existing catalog viewing remain unchanged. Product editing/deactivation UI is outside this change.

## 10. Shared List Group Context (aligned 2026-10-25)

- Per FR-009/FR-011 and the plan's core-screen definition, the group picker displays names from authenticated `GET /api/v1/groups` (Users). The user never types a UUID. No selection means no ShoppingList query or creation action; no memberships offers a link to `/groups`.
- `/shared-lists?groupId={id}` persists group context for reload and history. The URL value must resolve to a group returned for the current account before enabling `GET /api/v1/shared-lists?groupId={id}` or creation. This client check complements server membership enforcement and is not a replacement for it.
- Creating uses a dialog labelled with the selected group name, posts `{ name, groupId }` to `/api/v1/shared-lists`, and refreshes only that group's/account's list query. The group ID comes from the resolved group object, never free text. Errors retain the draft; pending submissions are blocked. Changing group/account clears local list/settlement selection and creation draft.
- Backend authorization remains authoritative; 403 shows a permission state and hides list/settlement data. This change does not certify the complete Owner/Admin/Member permission matrix or implement shared-item detail routes.

## 11. Expense Participants and User-Facing Language (aligned 2026-10-25)

- User-facing labels are **Reparto de gastos**, **Balance por persona** and **Pagos propuestos para saldar cuentas**. Internal settlement endpoint names remain unchanged.
- Read the authorized shared-list detail at `GET /api/v1/shared-lists/{listId}` to obtain its real group and items; resolve active members through `GET /api/v1/groups/{groupId}/members` and caller identity through `/api/v1/users/me`. Do not invent a list/item ID. A list with no items cannot register expenses.
- Payer and optional purchaser use member-name selects; participants use named checkboxes. UserIds are values internal to the request. Owner/Admin can select a payer from the group; Member is limited to self for payments, consistent with the existing expense handler. Server membership/role validation remains authoritative. Unknown or unavailable membership disables the flow.
- `POST /api/v1/shared-lists/{listId}/items/{itemId}/expenses` sends `{ paidBy, purchasedBy, realPaidPrice, currency: "EUR", participants }`, not the previous incompatible `amount/paidByUserId/participantUserIds/note`. Purchaser may be null. Require positive amount, real item, eligible payer and at least one member participant. Preserve values on failure; block duplicate pending requests.
- Summary balances use the actual `totalPaid`, `totalOwed`, `netBalance`, `type` fields, not a nonexistent `amount`. Transfers use backend `fromUserId/toUserId/amount`, rendered with member names; unavailable historic members get a nontechnical fallback rather than raw UUIDs. React formats amounts and does not recompute shares or settlement rules.
- Real-browser verification registered EUR 12.50 on a dedicated list/item with one member (201), then refreshed the balance. Multiple-person labels/transfers, Member restrictions and 403 failures were verified in component tests; a multi-user real-tenant flow remains pending. Shared-item creation UI, historic-member identity resolution and currencies other than EUR remain outside this correction.

## 12. Private list workflow (2026-10-27)

- Personal index `/personal-lists` opens a short creation dialog. POST `/api/v1/purchaselists` still sends `{ name }`; only the returned ID is opened. The detail resolves its write target exclusively from the account's `/mine` response. Item creation remains `{ productId, productName, quantity, price }` with a real active catalog product.
- POST `/api/v1/purchaselists/{listId}/items/{itemId}/mark-as-purchased` has no body and returns 204. The client calls it only for a pending item in a selected personal list, blocks duplicate writes, refreshes that account's query after success and never offers unmarking. This client restriction is not server authorization: the current generic handler lacks ownership/membership/role checks. Backend hardening remains an explicit prerequisite; it is not modified or certified by this UX work.
- Shared detail `/shared-lists/:listId` uses GET `/api/v1/shared-lists/{id}`. The server's read handler is authoritative for membership. The return link derives groupId from the returned detail, not from untrusted query data. `?tab=shopping|expenses` selects Compra/Gastos; both tabs retain the same list ID. Named group context comes from Users, never a typed UUID.
- Shared item creation uses POST `/api/v1/shared-lists/{id}/items` with `{ productId, name, quantity, estimatedPrice }`. The public handler verifies membership and records AddedBy. It is not routed through personal item creation. The generic mark endpoint is deliberately not used for shared lists because its authorization is not verified for the roles required by spec 005 FR-004. The UI explains this limitation instead of simulating a checked item.
- Gastos embeds the existing settlement flow for that shared-list ID and obtains real items/members. No financial computation, transfer, invitation or payment is added. The existing role and historical-member limitations remain.
- Catalog return destinations accept only internal personal/shared detail paths with UUIDs. Returning restores the selected list; creating a catalog product does not silently add an item or replace the list context. Creation and item forms use RHF + Zod, preserve values on failure and are separate from prototype fixtures.

### Identidad del ítem al marcar compra — corrección 2026-10-27

El endpoint `/api/v1/purchaselists/{listId}/items/{itemId}/mark-as-purchased` recibe el `Id` del ítem devuelto al añadirlo o consultarlo, no su `ProductId` de catálogo. El agregado buscaba erróneamente por ProductId, provocando «El item no existe en la lista» pese a una selección correcta del cliente. Se corrige solo el marcado para buscar `ListItem.Id`; sin cambio de ruta/payload, workaround en React, migración ni ampliación de permisos. T132 sigue abierto. La regresión HTTP crea lista, añade producto, lee /mine, marca con el ID devuelto y vuelve a leer estado/progreso; se repite el marcado para comprobar idempotencia.
