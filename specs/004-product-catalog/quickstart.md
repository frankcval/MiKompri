# Quickstart: 004 — Product Catalog (MVP-2)

**Propósito**: validar la implementación de MVP-2 end-to-end sin incluir código de implementación.

## Prerrequisitos

- .NET SDK 8.x
- Docker Desktop en ejecución
- Puertos libres para APIs y PostgreSQL
- Solución restaurable: `MiKompri.sln`

## 1) Arranque de infraestructura

```powershell
# Desde raíz del repositorio
docker compose up -d postgres
```

Validar PostgreSQL activo:
```powershell
docker compose ps
```

## 2) Build y tests base del repositorio

```powershell
dotnet restore MiKompri.sln
dotnet build MiKompri.sln --configuration Release --no-restore
dotnet test MiKompri.sln --configuration Release
```

Esperado: build y tests en verde.

## 3) Ejecutar ProductCatalog API

```powershell
dotnet run --project MiKompri.ProductCatalog.Api --launch-profile http
```

Verificar:
- Swagger en `/swagger` (entorno no productivo)
- Health en `/health`

## 4) Escenarios de validación funcional

> Referencia de payloads y códigos: [contracts/product-catalog-api.md](./contracts/product-catalog-api.md)

### Escenario A — Catálogo reutilizable
1. Crear producto (`POST /catalog-products`) -> `201`.
2. Consultar lista (`GET /catalog-products`) -> incluye nuevo producto.
3. Actualizar producto (`PUT /catalog-products/{id}`) -> `200`.
4. Desactivar (`PATCH /catalog-products/{id}/deactivate`) -> `204`.
5. Reconsultar activos -> no aparece el desactivado.

### Escenario B — Mercados
1. Crear mercado (`POST /markets`) -> `201`.
2. Actualizar mercado (`PUT /markets/{id}`) -> `200`.
3. Desactivar mercado (`PATCH /markets/{id}/deactivate`) -> `204`.

### Escenario C — Registro de precios
1. Registrar precio válido (`POST /product-prices`) -> `201`; la moneda se completa internamente según la configuración del MVP-2.
2. Repetir mismo producto+mercado+fecha -> `409`.
3. Registrar precio <= 0 -> `400`.
4. Registrar fecha futura -> `400`.

### Escenario D — Historial
1. Consultar historial (`GET /api/v1/catalog-products/{catalogProductId}/price-history`) -> `200` ordenado por fecha.
2. Confirmar que la respuesta expone `catalogProductId` como identificador estable reutilizable.
3. Consultar producto sin historial -> `200` con `records: []`.
4. Filtrar por mercado y rango de fechas (`marketId`, `from`, `to`) -> resultados acotados.

## 5) Validación de no alcance

Confirmar que MVP-2 no expone:
- Flujos de listas compartidas/reparto.
- OCR/tickets.
- Promociones o scraping.
- Recomendaciones inteligentes.
- Integración completa con `Users` y `ShoppingList`.

## 6) Tests de ProductCatalog (nuevos proyectos)

```powershell
dotnet test test\MiKompri.ProductCatalog.Domain.Tests\MiKompri.ProductCatalog.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.ProductCatalog.Application.Tests\MiKompri.ProductCatalog.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.ProductCatalog.Api.Tests\MiKompri.ProductCatalog.Api.Tests.csproj --configuration Release
```

Esperado:
- Dominio: invariantes (unicidad lógica, desactivación, precio válido).
- Aplicación: handlers/validators CQRS.
- API: contratos HTTP y errores esperados.

## 7) Docker

```powershell
docker compose up --build -d
docker compose ps
docker compose logs mikompriproductcatalogapi --tail=50
```

Esperado: servicio `mikompriproductcatalogapi` en estado `healthy`.
