# Quickstart: 004 — Product Catalog (MVP-2)

**Propósito**: validar la implementación de MVP-2 end-to-end sin incluir código de implementación.

## Prerrequisitos

- .NET SDK 8.x
- Docker Desktop en ejecución
- Puertos libres: `8083` (ProductCatalog API), `8080` (ShoppingList), `8082` (Users), `5432` (PostgreSQL)
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
dotnet test MiKompri.sln --configuration Release --no-build
```

Resultado verificado: **0 errores, 0 omitidos** en todos los proyectos de test.

## 3) Ejecutar ProductCatalog API (modo local)

```powershell
dotnet run --project MiKompri.ProductCatalog.Api --launch-profile http
```

Verificar:
- Swagger en `http://localhost:5XXX/swagger` (puerto según `launchSettings.json`, entorno Development)
- Health en `http://localhost:5XXX/health`

Con Docker (puerto fijo):
- Swagger en `http://localhost:8083/swagger`
- Health en `http://localhost:8083/health`

## 4) Escenarios de validación funcional

> Referencia de payloads y códigos: [contracts/product-catalog-api.md](./contracts/product-catalog-api.md)
>
> Base URL con Docker: `http://localhost:8083/api/v1`

### Escenario A — Catálogo reutilizable
1. Crear producto (`POST /api/v1/catalog-products`) → `201 Created` con `id`.
2. Consultar lista (`GET /api/v1/catalog-products`) → incluye el nuevo producto.
3. Actualizar producto (`PUT /api/v1/catalog-products/{id}`) → `200 OK`.
4. Desactivar (`PATCH /api/v1/catalog-products/{id}/deactivate`) → `204 No Content`.
5. Reconsultar activos → el producto desactivado no aparece.

### Escenario B — Mercados
1. Crear mercado (`POST /api/v1/markets`) → `201 Created` con `id`.
2. Actualizar mercado (`PUT /api/v1/markets/{id}`) → `200 OK`.
3. Desactivar mercado (`PATCH /api/v1/markets/{id}/deactivate`) → `204 No Content`.

### Escenario C — Registro de precios y conflictos
1. Registrar precio válido (`POST /api/v1/product-prices`) → `201 Created`; la moneda se completa desde la configuración `ProductCatalog:Currency` (por defecto `"EUR"`).
2. Repetir la misma combinación producto+mercado+fecha → **`409 Conflict`**.
3. Registrar precio ≤ 0 → `400 Bad Request`.
4. Registrar con fecha futura → `400 Bad Request`.
5. Intentar crear dos productos con nombre normalizado y unidad idénticos → **`409 Conflict`**.

### Escenario D — Historial
1. Consultar historial (`GET /api/v1/catalog-products/{catalogProductId}/price-history`) → `200 OK` ordenado por fecha.
2. Confirmar que la respuesta expone `catalogProductId` como identificador estable reutilizable.
3. Consultar producto sin historial → `200 OK` con `records: []`.
4. Filtrar por mercado y rango de fechas (`marketId`, `from`, `to`) → resultados acotados.

## 5) Validación de no alcance

Confirmar que MVP-2 **no** expone:
- Flujos de listas compartidas/reparto.
- OCR/tickets.
- Promociones o scraping.
- Recomendaciones inteligentes.
- Integración runtime con `Users` y `ShoppingList` (queda fuera de MVP-2; la autenticación transversal se definirá en una spec posterior).

## 6) Tests de ProductCatalog

```powershell
dotnet test test\MiKompri.ProductCatalog.Domain.Tests\MiKompri.ProductCatalog.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.ProductCatalog.Application.Tests\MiKompri.ProductCatalog.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.ProductCatalog.Api.Tests\MiKompri.ProductCatalog.Api.Tests.csproj --configuration Release
```

Cobertura verificada:
- **Dominio**: invariantes de `CatalogProduct`, `Market`, `ProductPriceRecord`, `Money`, `ProductName`.
- **Aplicación**: handlers/validators CQRS para todas las user stories; casos de `ConflictException` para duplicados.
- **API**: contrato HTTP de historial de precios; respuesta `200 OK` con datos correctos.

## 7) Docker

```powershell
docker compose config          # valida sintaxis del compose
docker compose up --build -d   # arranca todos los servicios
docker compose ps              # verifica estado
docker compose logs mikompriproductcatalogapi --tail=50
```

Verificar:
- `mikompriproductcatalogapi` en estado `healthy` o `running`.
- `GET http://localhost:8083/health` devuelve `200 OK`.
- `GET http://localhost:8083/swagger` devuelve la UI de Swagger.

```powershell
docker compose down            # limpieza
```

## 8) Registro de validación (rama fix/complete-spec-004-v1)

| Verificación | Resultado |
|---|---|
| `dotnet build` Release | ✅ 0 errores |
| Tests totales (todos los proyectos) | ✅ 0 errores, 0 omitidos |
| ProductCatalog Domain Tests | ✅ verde |
| ProductCatalog Application Tests (incl. ConflictException) | ✅ verde |
| ProductCatalog Api Tests | ✅ verde |
| ShoppingList tests | ✅ sin regresión |
| Users tests | ✅ sin regresión |
| `docker compose config` | pendiente de validar en entorno Linux/CI |
| HTTP 409 en duplicado de producto | ✅ cubierto por test automatizado |
| HTTP 409 en duplicado de precio | ✅ cubierto por test automatizado |
| Moneda configurable (`ProductCatalog:Currency`) | ✅ Options Pattern implementado |
| Sin autenticación añadida a ProductCatalog | ✅ confirmado |
| Spec 005 no creada | ✅ confirmado |

> **Nota**: Las pruebas Docker (health check runtime, Swagger en contenedor, migraciones automáticas)
> deben ejecutarse en un entorno con Docker disponible (Linux/CI). Los tests automatizados cubren
> el comportamiento funcional de forma independiente.
