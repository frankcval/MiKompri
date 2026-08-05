# REST API Contract: Product Catalog (MVP-2)

**Feature**: 004-product-catalog | **Version**: v1 | **Date**: 2026-08-05
**Base URL**: `/api/v1`

## Convenciones

- Timestamps en UTC ISO 8601.
- `catalogProductId` es identificador estable para asociación futura con ítems de listas.
- Respuestas de error alineadas con middleware estándar (`status`, `error`, `traceId`, y `errors[]` cuando aplique).
- Códigos esperados: `200`, `201`, `204`, `400`, `404`, `409`.

---

## Productos de catálogo

### `POST /api/v1/catalog-products`
Crea un producto reutilizable.

**Request**
```json
{
  "name": "Leche Entera",
  "purchaseUnit": "l"
}
```

**Validaciones**
- `name` obligatorio.
- `purchaseUnit` obligatorio.
- Unicidad por `normalizedName + purchaseUnit` para productos activos.

**Responses**
- `201 Created` + body `CatalogProductDto`
- `409 Conflict` si producto activo duplicado
- `400 Bad Request` validación inválida

### `PUT /api/v1/catalog-products/{catalogProductId}`
Actualiza datos descriptivos del producto.

### `PATCH /api/v1/catalog-products/{catalogProductId}/deactivate`
Desactiva el producto sin borrar historial.

### `GET /api/v1/catalog-products`
Lista productos del catálogo.

**Query params opcionales**
- `includeInactive` (bool, default `false`)
- `search` (string)

### `GET /api/v1/catalog-products/{catalogProductId}`
Obtiene detalle de producto.

---

## Mercados

### `POST /api/v1/markets`
Crea mercado/tienda.

**Request**
```json
{
  "name": "Mercado Central",
  "locationHint": "Av. Principal 123"
}
```

### `PUT /api/v1/markets/{marketId}`
Actualiza nombre/ubicación descriptiva.

### `PATCH /api/v1/markets/{marketId}/deactivate`
Desactiva mercado sin borrar historial.

### `GET /api/v1/markets`
Lista mercados (activos por defecto).

---

## Precios e historial

### `POST /api/v1/product-prices`
Registra precio de producto por mercado y fecha efectiva.

**Request**
```json
{
  "catalogProductId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketId": "1b9e6679-7425-40de-944b-e07fc1f90ae7",
  "effectiveDate": "2026-08-05",
  "priceAmount": 1.45
}
```

**Validaciones**
- `priceAmount > 0`
- `effectiveDate` no futura
- producto y mercado deben existir y estar activos
- no duplicado por `catalogProductId + marketId + effectiveDate`
- la moneda del registro se asigna automáticamente según la configuración operativa del MVP-2

**Responses**
- `201 Created`
- `409 Conflict` por duplicado
- `400 Bad Request` por validación
- `404 Not Found` si producto/mercado no existe

### `GET /api/v1/catalog-products/{catalogProductId}/price-history`
Consulta historial de precios del producto usando `catalogProductId` como identificador estable reutilizable.

**Query params opcionales**
- `marketId` (Guid)
- `from` (fecha)
- `to` (fecha)

**Response `200 OK`**
```json
{
  "catalogProductId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "catalogProductName": "Leche Entera",
  "records": [
	{
	  "marketId": "1b9e6679-7425-40de-944b-e07fc1f90ae7",
	  "marketName": "Mercado Central",
	  "effectiveDate": "2026-08-01",
	  "priceAmount": 1.35,
	  "currency": "EUR"
	},
	{
	  "marketId": "1b9e6679-7425-40de-944b-e07fc1f90ae7",
	  "marketName": "Mercado Central",
	  "effectiveDate": "2026-08-05",
	  "priceAmount": 1.45,
	  "currency": "EUR"
	}
  ]
}
```

Reglas:
- Orden cronológico por `effectiveDate` ascendente.
- Si no hay datos, retorna `records: []`.
- Si se envía `marketId`, `from` y/o `to`, los resultados deben respetar esos filtros.

---

## Fuera de alcance contractual (MVP-2)

- No endpoints de listas compartidas/reparto.
- No OCR/tickets.
- No promociones/alertas.
- No scraping/comparación automática.
- No recomendaciones inteligentes.
- No endpoints de cliente MAUI.
- No integración runtime completa con `ShoppingList` o `Users`.
