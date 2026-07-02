# Data Model: Shopping List Core Hardening

**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Fecha**: 2026-06-30

## Entidades clave

### 1) Lista de compra

Representa la unidad principal de planificación de compra del usuario.

**Atributos funcionales**:
- `ListId` (identificador único)
- `Name` (nombre visible de la lista)
- `Items` (colección de ítems)
- `CreatedAt` (fecha/hora de creación)
- `UpdatedAt` (fecha/hora de última modificación)

**Reglas**:
- Nombre obligatorio no vacío.
- Debe permitir consulta aun cuando no tenga ítems.

### 2) Ítem de compra

Representa un producto a comprar dentro de una lista.

**Atributos funcionales**:
- `ItemId` (identificador único)
- `ListId` (relación con lista)
- `ProductId` (referencia de producto)
- `Name` (descripción del ítem)
- `Quantity` (cantidad esperada)
- `IsPurchased` (estado pendiente/comprado)
- `CreatedAt` (fecha/hora de creación)
- `UpdatedAt` (fecha/hora de última modificación)

**Reglas**:
- No puede duplicar `ProductId` dentro de la misma lista.
- Puede editarse en campos permitidos.
- El marcado como comprado debe ser idempotente.

## Relaciones

- Una **Lista de compra** tiene **0..N Ítems de compra**.
- Un **Ítem de compra** pertenece a **1 Lista de compra**.

## Vistas/DTO funcionales esperadas

### Vista de lista
- Datos de lista
- Colección de ítems
- Resumen de progreso (total, comprados, pendientes, porcentaje)
- Trazabilidad básica

### Vista de ítem
- Datos editables del ítem
- Estado de compra
- Trazabilidad básica

## Reglas de cálculo

### Progreso de lista
- `TotalItems = cantidad de ítems`
- `PurchasedItems = cantidad de ítems marcados comprados`
- `PendingItems = TotalItems - PurchasedItems`
- `ProgressPercent = 0` si `TotalItems = 0`
- En otro caso: `PurchasedItems / TotalItems * 100`
