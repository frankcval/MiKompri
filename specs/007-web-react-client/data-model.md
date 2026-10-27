# Data Model — 007-web-react-client

## 1) WebSession
- **Purpose**: Representar estado de autenticación del cliente SPA.
- **Fields**:
  - `isAuthenticated` (boolean)
  - `accessToken` (string, in-memory)
  - `expiresAt` (datetime)
  - `activeAccount` (object ref)
- **Validation Rules**:
  - No persistir secretos ni tokens en código fuente.
  - Renovación/expiración gestionada por flujo MSAL.
- **State Transitions**:
  - `anonymous -> authenticating -> authenticated -> expired -> anonymous`

## 2) UserProfileView
- **Purpose**: Proyección de perfil de usuario autenticado.
- **Fields**:
  - `userId` (guid interno MiKompri)
  - `displayName` (string)
  - `email` (string|null)
  - `lastSyncAt` (datetime)
- **Relationships**:
  - Pertenece a una identidad externa correlacionada por `(tid, oid)` en backend Users.

## 3) GroupSummary / GroupMembershipView
- **Purpose**: Gestión mínima de grupos y membresías visibles en UI.
- **Fields**:
  - `groupId` (guid)
  - `groupName` (string)
  - `role` (Owner|Admin|Member)
  - `memberStatus` (Active|Inactive)
- **Validation Rules**:
  - Operaciones permitidas dependen de rol/membresía retornada por backend.

## 4) PurchaseListView (personal/shared)
- **Purpose**: Representar listas e ítems para operaciones de compra.
- **Fields**:
  - `listId` (guid)
  - `name` (string)
  - `ownerId` (guid)
  - `groupId` (guid|null)
  - `items` (ListItemView[])
  - `progress` (number)
- **Validation Rules**:
  - `ownerId` siempre deriva de identidad autenticada en backend para listas personales.
  - El progreso visual se deriva de los ítems cargados; no se inventa una propiedad remota ni se precargan todos los detalles para mostrar resúmenes.

## 5) ListItemView
- **Purpose**: Ítem operativo de lista.
- **Fields**:
  - `itemId` (guid)
  - `productId` (guid|null)
  - `name` (string)
  - `quantity` (number)
  - `isPurchased` (boolean)
  - `addedBy` (guid|null)
  - `paidBy` (guid|null)
- **Validation Rules**:
  - Reglas de duplicados y negocio resueltas por backend.

## 6) SharedExpenseView / SettlementView
- **Purpose**: Mostrar reparto, balances y propuesta de settlement.
- **Fields**:
  - `expenseId` (guid)
  - `participants` (guid[])
  - `amount` (decimal)
  - `balances` (participant balance[])
  - `transfers` (from,to,amount[])
- **Validation Rules**:
  - Solo participantes/miembros activos según backend.

## 7) CatalogReadModel
- **Purpose**: Consulta de ProductCatalog, separada del alta de producto habilitada el 2026-10-25.
- **Fields**:
  - `products` (id,name,unit,status)
  - `markets` (id,name,status)
  - `priceHistory` (productId,marketId,effectiveDate,price)
- **Validation Rules**:
  - Alta con `{ name, purchaseUnit }` mediante el contrato existente; no modifica mercados/precios ni permite editar/desactivar productos desde UI.

## 8) UiFeedbackState
- **Purpose**: Estado UX transversal por vista.
- **Fields**:
  - `state` (loading|empty|error|unauthorized|forbidden|serviceUnavailable|ready)
  - `message` (string)
  - `retryAction` (optional callback)
- **Validation Rules**:
  - Mapeo explícito mínimo para HTTP 400/401/403/404/409/503.

## 9) Ejemplo local de portada

- Fixture independiente de las entidades remotas: nombre, cantidad, precio ilustrativo y marca de compra.
- Estado React temporal, restablecible y explícitamente ficticio; nunca se envía a las APIs ni se mezcla con queries de una cuenta.
- El harness de revisión interior vive en `tests/visual/`, fuera de la entrada y bundle productivos. No representa una sesión, invitación ni pago real.
- Presentación común y alcance entregado: [design.md](design.md).

## 10) Contexto de navegación de compra

- Lista personal: ID de ruta validado contra `/mine` de la cuenta; ningún editor ni mutación se vincula a la primera lista implícita.
- Lista compartida: ID de ruta consultado por contrato protegido; grupo de regreso derivado del detalle autorizado. `tab=shopping|expenses` identifica la vista de la misma lista y conserva historial.
- Creación e incorporación: borradores temporales de formulario RHF, validados con Zod; se preservan ante error y se reinician tras éxito/cancelación o cambio de cuenta/lista.
- Marcado personal: pendiente por ítem, éxito confirmado por servidor e invalidación de lectura; sin desmarcado ni estado optimista que simule persistencia.
- Regreso desde catálogo: solo detalle interno con UUID y pestaña permitida; alta de producto no equivale a alta de ítem.
- No son entidades nuevas de backend; no alteran identidad, permisos, payloads o cálculos del reparto.
