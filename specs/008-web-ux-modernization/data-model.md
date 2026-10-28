# Modelo y transiciones

Modelo implementado localmente: GUID internos y actor desde identidad validada en servidor. La estimación desconocida utiliza la migración aditiva AddKnownPriceState, verificada en PostgreSQL 16; el resto reutiliza las entidades existentes.

| Entidad/vista | Campos | Autoridad/validación |
| --- | --- | --- |
| PurchaseList | id, name, ownerId o groupId, abierta/cerrada, timestamps | ShoppingList; personal por owner, compartida por rol/membresía |
| ListItem | id distinto de productId, nombre, quantity, estimación, isPurchased, AddedBy/UpdatedBy, fechas | ShoppingList; entero positivo, duplicados conforme 002 |
| Estimación | amount conocido o desconocido, EUR | IsPriceKnown persistido; proyección null para desconocido; default true conserva ceros legados |
| Group/Member | id/name, userId/displayName/email, rol/vigencia | Users; no directorio global/invitaciones nuevas |
| Expense | expenseId, listId/itemId, realPaidPrice, paidBy, purchasedBy nullable, participants, currency, auditoría | Importe >0, participantes únicos y al menos uno activo al escribir; conservar historia |
| Settlement | paid/owed/net y transfers | Backend financiero; UI solo formatea |
| ReturnIntent | destino validado, groupId/tab/filtros, create, fecha | Lista de rutas internas, TTL 24 h; sin tokens/URL externa |
| Draft | partición homeAccountId local/listId/tipo, valores, productId creado, fase, updatedAt | sessionStorage/24 h; limpiar al completar/descartar/logout/cambio cuenta; revalidar membresía |
| Evidence | requisito/tarea, revisión, entorno, fecha, comando/protocolo, resultados/límites | QA sin tokens/datos privados en artefactos publicados |

## Transiciones

- Pendiente → guardando → comprado confirmado; fallo conserva pendiente/error. Comprado → desmarcando → pendiente; fallo conserva comprado. Pending por itemId.
- Desmarcar no altera gastos/balance; si hay gasto, explicar separación. Progreso sobre estado confirmado.
- Búsqueda → existente o creando → disponible → incorporando → incorporado. Fallo de incorporación conserva productId. Timeout de creación reconcilia duplicado por nombre/unidad antes de volver a escribir.
- Borrador → enviando → confirmado → refetch; refetch fallido no repite escritura. 409 conserva borrador separado y pide revisar lectura actualizada.
- Intención/borrador → acceso → misma cuenta → restauración revisable. Cambio cuenta/logout descarta partición. Callback sin destino válido abre Mis listas.
- Borrar/cerrar: confirmar → enviando → confirmado; error mantiene contexto. No cascada sobre historia financiera: 409 explicativo ante incompatibilidad.
- Gastos: personas vigentes verificadas al enviar; baja conserva historia, impide selecciones nuevas; no inventar nombres para autores históricos.

## Concurrencia/migración

No presuponer ETag, idempotency keys ni transacción entre contextos. Auditar comportamiento actual P0; estado nuevo se asigna explícitamente. Documentar compatibilidad y migración reversible si cambia nullabilidad/proyección, conservando ceros históricos.
