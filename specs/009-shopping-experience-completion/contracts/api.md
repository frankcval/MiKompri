# Contratos — 009

## Reutilizados

GET catalog-products?search; POST/PUT catalog-products; GET markets?search&includeInactive; POST/PUT markets; PATCH markets/{id}/deactivate; POST product-prices; GET catalog-products/{id}/price-history?marketId&from&to. JWT obligatorio. Política catálogo global: usuario autenticado, sin rol de grupo. No nuevos endpoints equivalentes.

GET purchaselists/mine y shared-lists?groupId ya incluyen fechas/items/progreso/status. purchase-state y DELETE existentes conservados; completitud no llama close. History usa marketName/currency de records y diferencia error de vacío.

## Extensiones financieras aditivas (implementar antes de activar UI)

POST shared-lists/{listId}/items/{itemId}/expenses conserva entrada/salida GUID y añade operationId, marketId, purchasedOn opcionales para clientes legacy. Web envía operationId siempre.
GET shared-lists/{listId}/expenses añade marketId/purchasedOn/version. PATCH gasto añade expectedVersion y metadatos, preservando compatibilidad por omisión; nueva Web envía versión al corregir.

POST purchaselists/{listId}/items/{itemId}/purchases: operationId requerido, realPaidPrice total >0 (2 decimales), currency EUR, marketId/purchasedOn opcionales. Actor/paid/purchased resueltos servidor; no entrada de otro owner/participantes. GET purchaselists/{listId}/purchases devuelve misma proyección canónica autorizada. PATCH/DELETE personal específico: owner, ID de ítem/gasto validado, versión para corrección. Sin nueva tabla de importes.

Compra opcional no cambia IsPurchased. Desmarcar no modifica gastos. Alta replay exacto devuelve mismo ID; operationId distinto permite otro registro explícito. Misma operación/payload distinto, versión antigua o replay de gasto borrado: 409. Actor ajeno nunca obtiene ID por replay. Colisión concurrente 409 permite reintentar misma operación. Escritura confirmada/refetch fallido no repite POST. Datos reales no alimentan catálogo automáticamente.

400 validación, 401 sesión, 403 owner/membresía/rol, 404 recurso autorizado no disponible, 409 conflicto. Lista cerrada mantiene solo lectura. DELETE con registros financieros devuelve 409 para conservar historia, también en personales.

## Hoy y fechas civiles

`PurchasedOn`/`EffectiveDate` siguen como `YYYY-MM-DD`, sin zona almacenada. Para validación de fecha futura, los clientes con fecha local envían el header opcional `X-Client-Utc-Offset-Minutes`: entero entre -840 y 840, positivo al este de UTC (por ejemplo +60). Hoy = fecha del instante del servidor convertido a ese offset actual. Sin header, hoy UTC por compatibilidad; valores vacíos, múltiples o inválidos producen 400 en operaciones que validan fechas. La Web envía el offset al guardar fechas y usa fecha local para defaults y máximos. No se aceptan un reloj ni un «hoy» arbitrarios del cliente. El offset no es identidad y no se persiste ni interviene en la huella de idempotencia.
