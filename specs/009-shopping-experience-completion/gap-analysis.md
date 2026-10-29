# Gaps de entrada a 009

Inventario sobre main 77ddbfc, anterior a los cambios 009. Revisados constitución, specs/planes/tareas/contratos 002/003/004/005/007/008, código y tests. PR56 integrado no cierra gates de 007/008. El resultado posterior y sus límites están en [evidencia](evidence/validation.md).

| Necesidad | Existe backend | Existe frontend | Bug/deuda/nueva feature | Cambio necesario | Riesgo |
| --- | --- | --- | --- | --- | --- |
| Total estimado | Datos cantidad/precio nullable | Solo por ítem | Feature de presentación | Sumar precio unitario × cantidad y cobertura | Medio: no confundir gasto real |
| Productos sin precio | IsPriceKnown/DTO nullable, cero legado conservado | Sí por ítem | Completar UX | Contador y subtotal parcial | Bajo |
| Orden de listas | Fechas/nombre/progreso en DTO | No selector | Feature frontend | Orden estable/URL | Bajo |
| Activas/historial | Estado operativo y progreso | Solo personal/shared | Feature frontend | Clasificar sin nuevo estado persistido | Medio |
| Completada al 100% | Datos suficientes | Sigue en índice | Feature frontend | No vacía y todos comprados → historial | Medio |
| Desmarcar después | Purchase-state explícito | Sí | Definir semántica | Active vuelve a activas, gastos intactos; Closed no escribe | Medio |
| Borrado confirmado | DELETE con permisos/protección financiera | Sí en detalle | Existente/deuda de aceptación | Reutilizar, mejorar descubrimiento y pruebas | Medio |
| Long-press | No requiere API | No | Evaluación opcional | Pospuesto; acción visible permanece | Bajo: scroll/descubrimiento |
| Historial de precios | GET filtros y POST observaciones | GET parcial, sin alta | Frontend/contrato; incidente runtime no confirmado | DTO/states/filtros/registro y reproducción HTTP | Medio |
| Compra desde marcado | Gastos shared | Marcado y Gastos separados | Integración nueva | Formulario opcional canónico | Alto |
| Gastos independiente | Sí | Sí | Preservar | Mantener entrada/historial/corrección/reparto | Medio |
| Evitar duplicados | Varios gastos por ítem sin idempotencia de alta | Bloqueo pendiente insuficiente | Deuda backend/frontend | Recibo persistente de operación y versión | Alto |
| Datos reales y personal/shared | Importe/actores/participantes shared; falta mercado/fecha elegida/personal | Parcial | Ampliación backend | Registro canónico con metadatos y propietario personal | Alto |
| Mercados | CRUD/búsqueda/desactivación | Listado/filtro | Conexión existente | Gestión mínima y selección por nombre | Medio: catálogo global |
| Editar productos | PUT existente | Alta/consulta | Conexión existente | Nombre/unidad, sin reescribir snapshots | Medio |
| Foto | No modelo/almacenamiento | No | Investigación futura | Modelo/URL/formatos/fallback/seguridad, sin implementación | Alto |
| Buscar productos | search existente | Filtro local en Añadir | Deuda de integración | Búsqueda de catálogo y selección directa | Bajo/medio |
| Añadir unificado | Consulta/alta existentes | Búsqueda seguida de desplegable | Evolución UX | Filas seleccionables y alta precompletada recuperable | Medio |
| Comentarios/chat | Sin contrato auditado | No | Feature posterior | Exclusión explícita | Alto si se incorpora |

No se proponen equivalentes nuevos de búsqueda/edición/mercados/precios. Los únicos endpoints nuevos son compras personales, donde no existía capacidad pública equivalente. Contratos shared evolucionan de forma aditiva. [Plan](plan.md), [contratos](contracts/api.md), [modelo](data-model.md).
