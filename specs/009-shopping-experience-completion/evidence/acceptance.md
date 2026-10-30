# Aceptación 009

Estado: técnica controlada acreditada según [validation](validation.md); aceptación global abierta. No se sustituye una verificación humana/integrada con pruebas de código.

| Criterio | Evidencia disponible | Estado/límite |
| --- | --- | --- |
| SC-001 estimación/orden/clasificación | listPresentation: desconocido/cero, cantidades, vacío, completa, desmarcada, Closed y orden estable; E2E filtros/URL/historial | Acreditado en datos controlados |
| SC-002 idempotencia/concurrencia | PurchaseOperationService, PurchaseCompletionApiTests en PostgreSQL, carrera de cuatro solicitudes, stale version y replay tras borrado; PurchaseRecovery Web | Acreditado en pruebas; no idempotencia prometida a clientes shared legacy que omitan operación |
| SC-003 compra/Gastos/historial/autorización | Recorrido personal 009 y piloto real con la cuenta facilitada; captura shared y participantes explícitos 3/3 motores; suites API; desmarcar conserva saldos/registro, volver a marcar no duplica; deriva actor personal, rechaza ajenos y Member no autorizado | Personal real acreditado; integración Entra multiusuario pendiente |
| SC-004 móvil/accesibilidad | Recorridos 320/390/430/768/1280 en tres motores, teclado/foco/axe y capturas revisadas; mínimo táctil de portada corregido sin relajar test | Lector, zoom completo y dispositivo físico pendientes |
| SC-005 catálogo/historial | CatalogPriceFlow HTTP con PostgreSQL, validación rango, producto/mercado/precio/duplicado/inactivo; E2E edición y mercado; PriceHistoryStates distingue selección/red/vacío | Acreditado técnicamente; causa original de consulta backend no demostrada |
| SC-006 rendimiento | [80 muestras completas](performance.md), primera serie incompleta conservada y repetición separada | Abierto: LCP índices/detalles 2600/2764/2712ms y Añadir 208ms; no se bajan umbrales |
| SC-007 cierre | Spec/plan/tasks/contratos/modelo/gaps/futuros y evidencia; Docker/CI local validados | Abierto: T010/T011 y gates originales 007/008 no cerrados |

## Trazabilidad funcional

FR-001/002 → listPresentation, ListOverview, ShoppingListView. FR-003/004 → tres índices, purchase-state existente, orden y URL. FR-005 → ConfirmAction/DELETE y bloqueos de historia de 008 extendidos a compras personales; long-press evaluado y pospuesto. FR-006 → ListItemsPanel/InlineProductCreation y pruebas de recuperación. FR-007 → PurchaseCapture, cancelación/foco y bloqueo de dos diálogos durante carga. FR-008/009 → ItemExpenseRecord, PersonalPurchaseHandler, handlers shared y ExpenseForm. FR-010/011 → recibo/hash/versión, UoW, API y Gastos independiente. FR-012/013 → CatalogManagement, ProductCatalogPage, PriceHistoryPanel y consultas existentes. FR-014 → separación explícita de registro de compra y observación de precio. FR-015 → correlación/contratos Users existentes conservados; ninguna lectura cruzada DB nueva.

El funcionamiento de las políticas por recurso está probado con identidades controladas; este informe no certifica toda la estrategia OAuth/scopes heredada ni modifica retrospectivamente su aceptación.
