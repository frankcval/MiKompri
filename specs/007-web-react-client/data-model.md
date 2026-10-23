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
- **Purpose**: Consulta read-only de ProductCatalog.
- **Fields**:
  - `products` (id,name,unit,status)
  - `markets` (id,name,status)
  - `priceHistory` (productId,marketId,effectiveDate,price)
- **Validation Rules**:
  - Sin comandos de escritura en MVP web.

## 8) UiFeedbackState
- **Purpose**: Estado UX transversal por vista.
- **Fields**:
  - `state` (loading|empty|error|unauthorized|forbidden|serviceUnavailable|ready)
  - `message` (string)
  - `retryAction` (optional callback)
- **Validation Rules**:
  - Mapeo explícito mínimo para HTTP 400/401/403/404/409/503.