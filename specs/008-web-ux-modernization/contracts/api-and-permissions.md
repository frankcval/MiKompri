# Contratos, permisos y recuperación

Estado: implementación local 2026-10-28; ver [validación y límites](../evidence/validation.md). Las rutas de esta sección ya existen en código y están cubiertas por las suites API; no certifica acceso Entra con dos cuentas. Fuente: controladores vigentes y [contrato 007](../../007-web-react-client/contracts/frontend-backend-integration.md).

## Inventario existente a comprobar en P0

Prefijos ShoppingList y Users conservan `/api/v1`. IDs GUID internos, token validado; ownership/actor no desde formulario.

| Operación | Ruta/verbo existente | Entrada/salida observada | Condición de reutilización |
| --- | --- | --- | --- |
| Personales propias | GET /purchaselists/mine | Colección PurchaseListDTO | Filtrar personales servidor, identidad actual |
| Crear personal | POST /purchaselists | name/groupId según request; GUID creado | UI personal no elige otro owner ni transforma a grupo |
| Leer/renombrar/borrar personal | GET/PUT/DELETE /purchaselists/{id} | PUT name/groupId; GET DTO; escritura 204 | Revisar/añadir auth+ownership; prohibir mutar groupId para escalar permisos |
| Añadir personal | POST /purchaselists/{listId}/items | productId/productName/price/quantity; GUID | Auth+ownership y producto válido; distinguir itemId/productId |
| Editar/borrar producto | PUT/DELETE /purchaselists/{listId}/items/{itemId} | PUT productName/price/quantity; 204 | Auditar auth+ownership/rol y pertenencia del item a lista; no asumir seguro para compartidas |
| Marcar legado | POST /purchaselists/{listId}/items/{itemId}/mark-as-purchased | Sin body; 204 | T132; cubrir propietario/grupo/rol; conservar compatibilidad y T133 |
| Grupos y miembros | GET/POST /groups; GET /groups/{groupId}/members | Grupo/colección, nombres y roles desde Users | Rol/vigencia y no filtración de grupos ajenos |
| Perfil | GET /users/me; POST /users/me/sync | Perfil; sync 200/201 según provisioning | Distinguir refetch/sync, mantener correlación TP11 |
| Compartidas | GET/POST /shared-lists; GET /shared-lists/{id} | GET colección con groupId; POST name/groupId/description y GUID | Membresía y matriz 005 |
| Editar/cerrar compartida | PATCH /shared-lists/{id}; PATCH /shared-lists/{id}/close | name/description o sin body; 204 | Owner/Admin y reglas de lista cerrada |
| Añadir compartido | POST /shared-lists/{id}/items | productId/name/estimatedPrice/quantity; GUID | Membresía y AddedBy servidor |
| Registrar gasto | POST /shared-lists/{id}/items/{itemId}/expenses | realPaidPrice/paidBy/purchasedBy/participants/currency; GUID | Personas/autoría/pago según 005 |
| Editar/eliminar gasto | PATCH/DELETE /shared-lists/{id}/items/{itemId}/expenses/{expenseId} | PATCH datos de gasto; 204 | Owner/Admin y permisos de Member según 005; no gasto ajeno |
| Resumen/propuesta | GET /shared-lists/{id}/settlement/{summary\|proposal} | DTO backend | Membresía; no recálculo cliente |
| Auditoría | GET /shared-lists/{id}/audit-events | Eventos | Revisar acceso y datos sensibles |
| Catálogo | GET/POST /catalog-products, GET /catalog-products/{id}/price-history, GET /markets | Producto nombre/unidad; listas/historial | JWT real; alta reutilizable, no edición/mercados nuevos |

DTOs, códigos, errores y scopes se contrastaron con controladores y pruebas de API. No se admiten IDs/nombres de ejemplo en payload productivo. El POST genérico con groupId conserva compatibilidad solo tras comprobar identidad/propietario y membresía activa; el alta de un ajeno se rechaza antes de persistir.

## Contratos aditivos implementados en 008

1. **Estado explícito**: PATCH `/api/v1/purchaselists/{listId}/items/{itemId}/purchase-state`, `{ isPurchased: boolean }`, 204. Personal solo owner; compartida matriz 005. Asignación repetida del mismo estado no duplica eventos de cambio. Desmarcar no toca gastos. Endpoint legado sigue funcionando con la misma autorización endurecida. Implementado y conectado tras comprobar casos autorizados y negativos; isPurchased es obligatorio, un body vacío devuelve 400 sin escritura.
2. **Edición/borrado compartido**: preferir rutas dedicadas PATCH/DELETE `/api/v1/shared-lists/{listId}/items/{itemId}`; PATCH `{ quantity, estimatedPrice }` y 204; actualizar nombre solo si el contrato de dominio lo permite sin cambiar identidad del producto. 409 ante ítem con historia financiera incompatible con eliminación. Owner/Admin pueden gestionar; Member solo sus ítems, según 005.
3. **Eliminar lista compartida**: DELETE `/api/v1/shared-lists/{listId}`, 204 para Owner/Admin si no destruye historia; 409 en caso contrario y ofrecer cierre existente. No prometer borrado físico de datos contables.
4. **Lectura para corregir gastos**: GET `/api/v1/shared-lists/{listId}/expenses`, colección paginable de `{ expenseId, itemId, realPaidPrice, currency, paidBy, purchasedBy, participants, createdBy, createdAt, updatedAt }` con nombres/históricos autorizados o referencias resueltas por contrato de Users. Solo miembros autorizados. No directorio público ni acceso cruzado DB. Añadir consulta histórica acotada al grupo cuando contrato actual solo devuelve miembros activos; nombres ausentes tienen fallback.
5. **Proyección de detalle**: exponer autoría/timestamps y estimación nullable aditivamente; no hacer desaparecer ceros legados. Si DTO numérico existente no admite null de forma compatible, añadir indicador/campo de estado desconocido o versión de DTO, con pruebas de consumidores. El campo canónico se decide en T004 con matriz de compatibilidad, antes de implementación T021/T032.

Estado de las propuestas anteriores: 1–3 implementadas con las rutas/códigos indicados; 4 implementada como colección completa no paginada, incluyendo `productName` y `createdBy` obtenido de la auditoría; nombres por el contrato de miembros existente y texto de miembro anterior o no disponible cuando faltan. No se creó un directorio histórico adicional. 5 se resuelve con `IsPriceKnown` persistido (default true para datos anteriores) y precio nullable en DTO; autoría/fechas se conservan. No hay reinterpretación de ceros. La proyección heredada ya admitía precio nullable. Suite API: 68/68 tanto en memoria como PostgreSQL real. OpenAPI se genera desde los controladores actuales.

## Matriz mínima de permisos

| Acción | Personal owner | Compartida Owner/Admin | Compartida Member | Ajeno/inactivo/sin sesión |
| --- | --- | --- | --- | --- |
| Leer lista/ítems/reparto | Sí, reparto no aplica | Sí | Sí | No |
| Renombrar/cerrar/eliminar lista | Sí; cerrar no aplica | Sí con preservación histórica | No | No |
| Añadir producto | Sí | Sí | Sí | No |
| Editar/eliminar producto | Sí | Sí | Solo propio conforme 005 | No |
| Marcar/desmarcar | Sí | Sí | Sí conforme 005 | No |
| Registrar gasto | No aplica | Pagador permitido del grupo | Solo ítems que compra y paga él según 005 | No |
| Editar/eliminar gasto/participantes | No aplica | Conforme 005 | No, conforme handlers actuales; solicitar corrección a Owner/Admin | No |
| Ver personas/roles | No aplica | Sí | Sí | No |

Decisión de alcance: los handlers actuales UpdateItemExpense/DeleteItemExpense permiten únicamente Owner/Admin; se conserva esta restricción también para gastos propios de Member. Member registra pagos propios de ítems que compra, sin permiso nuevo de editar/eliminar gastos. T004 acredita y documenta esta regla con pruebas. No se amplían derechos por inferencia de autoría. Toda autorización se reevalúa al enviar; no confiar en capabilities UI cacheadas.

## Errores y escrituras

Edición personal: omitir `price` conserva la estimación; enviarlo explícitamente como `null` la deja desconocida; `0` continúa siendo un importe conocido. El modelo de entrada registra internamente la presencia del campo (no es un flag controlado por el cliente). Las tres variantes y la edición solo de cantidad tienen regresión API con lectura posterior.

400: campo/razón útil; 401: reauth con draft, sin replay; 403: pérdida de permiso y volver; 404: recurso no disponible, sin revelar datos ajenos; 409: duplicado/cambio o historia incompatible, revisar lectura; 503/red: servicio no disponible, lectura reintentable. No exponer stack traces, claves de configuración ni cuerpo técnico arbitrario.

Un 204 confirmado seguido de refetch fallido es guardado con lectura pendiente. Una respuesta de escritura perdida es resultado incierto: consultar por ID/identidad de producto/gasto antes de permitir nuevo envío. Donde no sea posible determinar el resultado, mostrar incertidumbre y revisión explícita, nunca inventar idempotencia ni repetir cobros/gastos.

Crear producto e incorporar no es transacción: conservar productId y fase confirmada; no eliminar catálogo como rollback. Duplicado existente se ofrece para reutilizar después de validarlo. Identificadores devueltos reales quedan vinculados a la lista/cuenta seleccionada.
