# Data Model: 004 — Product Catalog (MVP-2)

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-08-05

## Entidades principales

### 1) CatalogProduct (Aggregate Root)

Representa un producto reutilizable del catálogo.

**Campos**
- `Id` (Guid)
- `Name` (string, requerido, máx. 120)
- `NormalizedName` (string, requerido, máx. 120)
- `PurchaseUnit` (string, requerido, máx. 30) — ej. `unit`, `kg`, `l`
- `IsActive` (bool)
- `CreatedAt` (DateTime UTC)
- `UpdatedAt` (DateTime UTC)
- `CreatedBy` (Guid?, opcional según disponibilidad de identidad)
- `UpdatedBy` (Guid?, opcional según disponibilidad de identidad)

**Reglas de validación**
- Nombre no vacío ni solo espacios.
- Unidad de compra obligatoria.
- Unicidad de negocio para activos: `(NormalizedName, PurchaseUnit)`.

**Transiciones de estado**
- `Active -> Inactive` mediante comando de desactivación.
- No se elimina físicamente en MVP-2.

---

### 2) Market (Aggregate Root)

Representa tienda/mercado reutilizable para captura de precios.

**Campos**
- `Id` (Guid)
- `Name` (string, requerido, máx. 120)
- `LocationHint` (string?, opcional, máx. 180)
- `IsActive` (bool)
- `CreatedAt` (DateTime UTC)
- `UpdatedAt` (DateTime UTC)
- `CreatedBy` (Guid?, opcional)
- `UpdatedBy` (Guid?, opcional)

**Reglas de validación**
- Nombre obligatorio.
- Desactivación permitida aunque existan precios históricos asociados.

**Transiciones de estado**
- `Active -> Inactive` mediante comando de desactivación.

---

### 3) ProductPriceRecord (Entity de historial)

Representa un precio de producto observado en un mercado y fecha efectiva.

**Campos**
- `Id` (Guid)
- `CatalogProductId` (Guid, requerido)
- `MarketId` (Guid, requerido)
- `EffectiveDate` (DateOnly o DateTime UTC normalizado a fecha)
- `PriceAmount` (decimal(12,2), > 0)
- `Currency` (string, requerido, longitud 3; moneda operativa única del MVP-2, asignada por la aplicación y no enviada por el cliente)
- `CreatedAt` (DateTime UTC)
- `UpdatedAt` (DateTime UTC)
- `CreatedBy` (Guid?, opcional)
- `UpdatedBy` (Guid?, opcional)

**Reglas de validación**
- `PriceAmount > 0`.
- `EffectiveDate <= fecha actual`.
- No duplicado por `(CatalogProductId, MarketId, EffectiveDate)`.
- Solo se permite registrar precio si producto y mercado existen.
- Si producto o mercado está inactivo: se rechaza registro nuevo.
- La moneda se fija por configuración del MVP-2 y se completa internamente; el cliente no la proporciona.
- La moneda se fija por configuración del MVP-2 y se completa internamente; el cliente no la proporciona.

**Transiciones de estado**
- Entidad inmutable funcionalmente (append-only); no actualización destructiva en MVP-2.

---

## Relaciones

- `CatalogProduct (1) ---- (N) ProductPriceRecord`
- `Market (1) ---- (N) ProductPriceRecord`

No hay relaciones directas con tablas de `ShoppingList` ni `Users` en MVP-2.

---

## Índices y restricciones recomendadas (EF Core / PostgreSQL)

1. `UX_CatalogProducts_NormalizedName_PurchaseUnit_Active`
   - Unicidad parcial para activos (si se implementa con filtro), o validación de dominio + índice alternativo.

2. `UX_ProductPriceRecords_Product_Market_EffectiveDate`
   - Único para evitar duplicados de historial.

3. Índices de consulta:
   - `IX_ProductPriceRecords_CatalogProductId`
   - `IX_ProductPriceRecords_MarketId`
   - `IX_ProductPriceRecords_EffectiveDate`

---

## DTOs de aplicación (visión de salida)

- `CatalogProductDto`: `Id`, `Name`, `PurchaseUnit`, `IsActive`, `CreatedAt`, `UpdatedAt`
- `MarketDto`: `Id`, `Name`, `LocationHint`, `IsActive`, `CreatedAt`, `UpdatedAt`
- `ProductPriceRecordDto`: `Id`, `CatalogProductId`, `MarketId`, `MarketName`, `EffectiveDate`, `PriceAmount`, `Currency`
- `ProductPriceHistoryDto`: `CatalogProductId`, `CatalogProductName`, `Records[]` ordenado por fecha

---

## Mapeo FR -> Modelo

- FR-001..FR-004 -> `CatalogProduct`
- FR-005..FR-006 -> `Market`
- FR-007..FR-012 -> `ProductPriceRecord`
- FR-013 -> `CatalogProduct.Id` estable y expuesto en contrato
- FR-015 -> campos de auditoría en entidades
