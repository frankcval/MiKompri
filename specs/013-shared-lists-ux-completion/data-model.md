# Data Model — 013 Shared Lists UX Completion

013 no introduce nuevas entidades de dominio ni migraciones. Define un **modelo de presentación UX** sobre contratos existentes.

## 1) SharedListCardVM (listado)

Representa cada tarjeta en `SharedListsPage`.

- `listId: string`
- `name: string`
- `groupId: string`
- `groupName: string`
- `status: 'Active' | 'Closed' | 'Archived' | string`
- `pendingCount: number`
- `purchasedCount: number`
- `estimatedAmount: number`
- `unknownPriceCount: number`
- `historyReason: string | null`
- `createdAt?: string`
- `updatedAt?: string | null`

Origen: `SharedListSummary` + `items` + nombre de grupo desde `getMyGroups`.

## 2) SharedListContextVM (cabecera de detalle)

Contexto rápido del recurso abierto.

- `listId: string`
- `listName: string`
- `groupId: string`
- `groupName?: string`
- `listStatus: string`
- `isActive: boolean`
- `completionText: string` (ej. `3 de 7 comprados · 4 pendientes`)
- `estimatedText: string` (incluye parcial/unknown)
- `backTarget: string` (`/shared-lists?groupId=...` o fallback)

Origen: `getSharedListById` + `getMyGroups` + cálculo local existente (`ShoppingListView`/`ListOverview`).

## 3) RoleCapabilitiesVM (matriz de acciones)

Deriva capacidades de UI según rol y estado de lista.

- `role: 'owner' | 'admin' | 'member' | 'unknown'`
- `canViewList: boolean`
- `canAddItem: boolean`
- `canEditAnyItem: boolean`
- `canEditOwnItemOnly: boolean`
- `canTogglePurchaseState: boolean`
- `canRegisterExpense: boolean`
- `canEditExpense: boolean`
- `canDeleteExpense: boolean`
- `canRenameList: boolean`
- `canCloseList: boolean`
- `canDeleteList: boolean`
- `writeBlockedByClosedStatus: boolean`

Fuente normativa: backend (`PurchaseAccessPolicy` + handlers).

## 4) SharedNavigationState

- `groupId?: string`
- `tab: 'shopping' | 'expenses' | 'people'`
- `source?: 'groups' | 'lists' | 'shared-index'`

Se persiste en URL (query params) para deep-link/reload/back-forward.

## 5) ExpenseSummaryVM (tab Gastos)

No cambia settlement; solo presenta:

- `isLoadingSummary`
- `isLoadingProposal`
- `summaryRows[]` (nombre, pagado, adeudado, saldo)
- `proposalRows[]` (deudor, acreedor, amount)
- `historyRows[]` (registro real por ítem)
- `canManageHistory: boolean`
- `errorState?: 'forbidden' | 'unavailable' | 'generic'`

## 6) PeoplePanelVM

- `members[]: { userId, displayName, roleLabel }`
- `count`
- `emptyState`
- `errorState`

Sin mutaciones de membresía en 013.

## 7) Invariantes UX

1. Misma lista compartida (`listId`) debe verse igual sin importar ruta de entrada.
2. `tab` nunca cambia el recurso; solo la vista del mismo recurso.
3. Estimación (producto) y gasto real (settlement) no se mezclan en una sola métrica.
4. Acciones visibles en UI deben ser subconjunto de acciones permitidas por backend.
5. Estado cerrado bloquea escrituras y lo comunica explícitamente.
