# Modelo de presentación011

Sin cambios de dominio, contratos HTTP, BD, migraciones EF ni persistencia de preferencias. No existe una entidad FridgeBoard o StickyNote nueva; son superficies que muestran listas ya existentes.

| Concepto frontend | Datos | Invariante |
|---|---|---|
| SemanticIcon | clave semántica estática, componente base/seleccionado y glyph size | Exports reales de una familia; ninguna clave recibida del backend produce import dinámico |
| IconTreatment | decorativo/informativo, label visible/accesible y tooltip opcional | SVG no crea target ni nombre duplicado; el control dueño conserva semántica |
| SurfaceToken | rol de color/superficie/borde/foco/tipo/espaciado | No se persiste, no código hex disperso por features |
| ScreenVisualState | loading/ready/empty/error/partial/selected, derivado de estado actual | UI no inventa datos ni oculta error parcial con una superficie vacía |
| ListNoteView | nombre/contexto/pending/known estimate/unknown count/createdAt/to | Proyección de009; query params, orden e historial mantienen reglas existentes |

Transiciones de compra, historial, perfil, grupo/permiso y sesión siguen siendo las actuales. Purchased=false/true continúa en el checkbox; un aspecto de papel no cambia comando, OperationId ni gastos. No nuevos endpoints/DTOs o fotos/notas libres. [Contrato UI](contracts/ui-iconography.md).

Estado adicional de presentación: CatalogView(products/markets), CatalogDialog(open/pending/restoreFocus) y selectedProductId existente. Son estados locales efímeros; no se guardan en backend. GroupSummary proyecta myRole/memberCount ya existentes en el DTO efectivo; los ausentes se omiten, sin suponer rol o count. No modelo EF ni contrato nuevo.
