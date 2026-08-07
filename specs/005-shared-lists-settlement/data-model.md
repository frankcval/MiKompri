# Data Model: 005 — Shared Lists & Settlement (MVP-3)

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-08-06

## Entidades principales

### 1) SharedPurchaseList (Aggregate Root)

Representa una lista colaborativa asociada a un grupo de Users.

**Campos**
- `Id` (Guid)
- `Name` (string, requerido, máx. 120)
- `Description` (string?, opcional, máx. 500)
- `GroupId` (Guid, requerido)
- `CreatedAt` (DateTime UTC)
- `UpdatedAt` (DateTime UTC)
- `CreatedBy` (Guid, requerido)
- `UpdatedBy` (Guid, requerido)
- `Status` (enum: `Active`, `Closed`, `Archived`)

**Reglas de validación**
- `GroupId` obligatorio para listas compartidas.
- Solo miembros activos pueden operar sobre la lista (validación en capa aplicación vía Users contract).
- No reemplaza comportamiento de listas personales existentes.

**Transiciones de estado**
- `Active -> Closed` (lista finalizada para nuevas operaciones de gasto).
- `Closed -> Active` (reapertura administrativa).
- `Closed -> Archived` (histórico de solo lectura).

---

### 2) SharedListItem (Entity)

Ítem colaborativo dentro de `SharedPurchaseList`.

**Campos**
- `Id` (Guid)
- `SharedPurchaseListId` (Guid)
- `ProductId` (Guid, compatible con ProductCatalog)
- `Name` (string, requerido, máx. 150)
- `Quantity` (decimal(10,2), > 0)
- `EstimatedPrice` (decimal(12,2), opcional)
- `IsPurchased` (bool)
- `AddedBy` (Guid, requerido)
- `CreatedAt` (DateTime UTC)
- `UpdatedAt` (DateTime UTC)
- `UpdatedBy` (Guid, requerido)

**Reglas de validación**
- `AddedBy` obligatorio y trazable (PP4).
- `Quantity > 0`.
- Un ítem comprado puede o no tener gasto asociado en el mismo instante, pero el gasto válido exige reglas de `ItemExpenseRecord`.

**Transiciones de estado**
- `Pending -> Purchased`.
- `Purchased -> Pending` (si se revierte operación, conservando auditoría).

---

### 3) ItemExpenseRecord (Entity)

Registro contable de gasto real para un ítem comprado.

**Campos**
- `Id` (Guid)
- `SharedListItemId` (Guid)
- `PaidBy` (Guid, requerido)
- `PurchasedBy` (Guid?, opcional)
- `RealPaidPrice` (decimal(12,2), > 0)
- `Currency` (string, requerido, longitud 3)
- `ExpenseDate` (DateTime UTC)
- `CreatedAt` (DateTime UTC)
- `UpdatedAt` (DateTime UTC)
- `CreatedBy` (Guid, requerido)
- `UpdatedBy` (Guid, requerido)

**Reglas de validación**
- `PaidBy` obligatorio.
- `RealPaidPrice > 0`.
- Pagador puede ser o no participante del gasto.
- Solo miembros activos pueden registrarse como pagador/comprador para operaciones nuevas.

**Transiciones de estado**
- `Draft -> Confirmed` (cuando cumple participantes mínimos y validaciones).
- `Confirmed -> Corrected` (ajuste posterior auditado).

---

### 4) ExpenseParticipant (Entity)

Participación de un miembro en el consumo de un gasto.

**Campos**
- `Id` (Guid)
- `ItemExpenseRecordId` (Guid)
- `ParticipantUserId` (Guid)
- `ShareAmount` (decimal(12,2), calculado)
- `CreatedAt` (DateTime UTC)
- `CreatedBy` (Guid, requerido)

**Reglas de validación**
- Mínimo 1 participante activo por gasto.
- No duplicados por `(ItemExpenseRecordId, ParticipantUserId)`.
- Participantes deben pertenecer al grupo en membresía activa al momento de registrar/modificar gasto.

---

### 5) ParticipantBalance (Read Model)

Vista agregada por participante para reparto.

**Campos**
- `UserId` (Guid)
- `TotalPaid` (decimal(14,2))
- `TotalOwed` (decimal(14,2))
- `NetBalance` (decimal(14,2))
- `BalanceType` (enum: `Creditor`, `Debtor`, `Settled`)

**Reglas de cálculo**
- `TotalPaid = sum(ItemExpenseRecord.RealPaidPrice where PaidBy == UserId)`.
- `TotalOwed = sum(ExpenseParticipant.ShareAmount where ParticipantUserId == UserId)`.
- `NetBalance = TotalPaid - TotalOwed`.

---

### 6) SettlementProposal (Read Model)

Resultado de liquidación simplificada.

**Campos**
- `GroupId` (Guid)
- `GeneratedAt` (DateTime UTC)
- `Transfers` (colección de `SettlementTransfer`)

**SettlementTransfer**
- `FromUserId` (Guid, deudor)
- `ToUserId` (Guid, acreedor)
- `Amount` (decimal(12,2), > 0)

**Reglas de cálculo**
- Emparejar mayor deudor con mayor acreedor.
- Empates por `UserId` ascendente.
- Repetir hasta saldos netos en cero con precisión de 2 decimales.

---

### 7) SharedListAuditEvent (Entity)

Trazabilidad de operaciones relevantes para colaboración y auditoría.

**Campos**
- `Id` (Guid)
- `SharedPurchaseListId` (Guid)
- `ActorUserId` (Guid)
- `ActionType` (enum: `ListCreated`, `ItemAdded`, `ItemUpdated`, `ItemPurchased`, `ExpenseRecorded`, `ParticipantsChanged`, `ExpenseUpdated`, `ExpenseDeleted`, `ListClosed`, etc.)
- `TargetEntityType` (string)
- `TargetEntityId` (Guid)
- `OccurredAt` (DateTime UTC)
- `Metadata` (json/string, opcional)

**Reglas**
- No se elimina en operaciones de negocio normales.
- Debe preservar referencia de actor incluso si su membresía luego queda inactiva.

---

## Relaciones

- `SharedPurchaseList (1) ---- (N) SharedListItem`
- `SharedListItem (1) ---- (0..N) ItemExpenseRecord`
- `ItemExpenseRecord (1) ---- (1..N) ExpenseParticipant`
- `SharedPurchaseList (1) ---- (N) SharedListAuditEvent`

No existen relaciones FK hacia tablas de Users o ProductCatalog en este MVP; solo referencias por identificador canónico.

---

## Reglas de redondeo y consistencia

1. Reparto base por gasto: `RealPaidPrice / N participantes`.
2. Redondeo comercial a 2 decimales para cada cuota.
3. Residuo de centavos se asigna a `PaidBy`.
4. Conservación contable: suma de cuotas por gasto == `RealPaidPrice`.
5. Determinismo: cálculos de settlement reproducibles con mismo input.

---

## Mapeo FR -> Modelo

- FR-001..FR-004 -> `SharedPurchaseList`
- FR-005 -> `SharedListItem.AddedBy`
- FR-006, FR-006a, FR-009 -> `ItemExpenseRecord`
- FR-007, FR-007a, FR-008 -> `ExpenseParticipant`
- FR-010..FR-013 -> `ParticipantBalance`, `SettlementProposal`
- FR-015..FR-017 -> `SharedListAuditEvent` + reglas de membresía activa
- FR-018 -> `SharedListItem.ProductId` compatible
- FR-019..FR-020 -> referencias por `GroupId`/`UserId` sin acceso directo a BD de Users
