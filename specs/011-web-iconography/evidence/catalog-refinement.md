# Catálogo — contenido primero, administración bajo demanda

Refinamiento autorizado el01/11/2026; fuente d384f24. No cambios en API, DTO backend, EF, identidad ni permisos. Se reutilizan CreateCatalogProductForm, CatalogEditor, MarketManagement, PriceHistoryPanel, RegisterPriceForm y sus operaciones/query keys originales.

## Decisiones de UX

- Cabecera y CTA Crear producto; el formulario ya no precede a la búsqueda.
- Tabs locales Productos/Mercados: mismo destino /product-catalog, returnTo intacto. El tab es estado efímero de presentación, sin nuevo historial/contrato. Se conserva la búsqueda y selección al volver entre vistas.
- Filas semánticas de productos: nombre/unidad, Historial textual y edición secundaria con IconAction edit/label contextual/tooltip/44px. Sin interacción anidada ni acciones dependientes de hover.
- Crear/editar producto y crear/editar mercado en Dialog Fluent. Sin campos nuevos; validación, conflicto, error/draft, pending, protección contra duplicados e invalidación siguen en los formularios existentes. Pending impide Cancelar/Escape durante escritura. Foco retorna al trigger al cerrar.
- Historial/filtros mercado/Desde/Hasta/registro de precio solo con selectedProductId válido. Se conserva el cálculo y la semántica de fechas civiles de009. Mercados inactivos permanecen consultables y su historial no se elimina.
- Mercados: lista y Añadir mercado; editar/desactivar originales bajo contexto. No acciones ficticias ni nueva administración.

## Pruebas previas y dirigidas

Dos nuevas pruebas de componente fallaron antes del cambio: administración permanente y edición inline. Después13/13 tests del área pasaron, incluidos creación/error/reintento/duplicate-submit/returnTo. La edición y los diálogos de mercado se comprueban con foco/Cancel/Escape. Nueva prueba E2E en cinco anchos por motor:15/15 PASS. Regresiones existentes de edición/mercados/precios/compra/fechas se mantienen en la pasada completa final, sin rebajar expectativas.

La primera pasada detectó que axe examinaba restos del diálogo durante su animación de salida. Se espera la eliminación del nodo role=dialog, también cuando está aria-hidden; no se desactiva contraste ni se cambia el diseño para la prueba. Las muestras fallidas no se presentan como PASS. Resultados del candidato cerrado y capturas se consolidan en final-review.md.
