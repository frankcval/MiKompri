# Revisión transversal de UI/UX e iconografía011

01/11/2026. Fuente final0923a0c; huella y build en bundle-final.json. Dirección: Nevera compartida, MiKompri Sans, teal, tinta/papel/pizarra. Revisión de código y del DOM implementado; capturas de harness sintético separadas de QA humana. La guía [Web Interface Guidelines](https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md) se consultó en esta revisión. Sus recomendaciones no amplían el alcance a auth, negocio, nuevas librerías o virtualización.

| Sección | Criterio revisado | Evidencia de conducta |
|---|---|---|
| Portada | Marca/sans/teal preservados; menú y cerrar del mapa, sin rehacer hero/FAQ | home.spec + cancelación de Drawer diferido |
| Acceso | Google/Microsoft textuales, marca/proveedores como excepción, mensajes legibles | auth55 y confirmación humana previa de ambos proveedores |
| Shell | Icono+label, selected/aria-current, padres de detalle, accesos rápidos y Mi cuenta | PersistentNavigation, iconography, enlaces profundos/reload/Atrás/Adelante |
| Mis listas/Historial | Pizarra/notas, contexto visible, unknown prices, filtros/orden/queries | iconography/completion y componentes de listas |
| Listas personales/compartidas | Mismo lenguaje, contexto grupo y creación existente | app-sections y shopping |
| Compra personal/compartida | Papel de trabajo, checkbox nativo, Más opciones y controles44, texto largo | compra/100ítems/mark-unmark/datos de compra/idempotencia |
| Gastos | Panel plano, importes tabulares, participantes/pagador/errores conservados | completion y app-sections; ninguna regla cambiada |
| Personas | Panel legible de rol/nombre, solo datos disponibles | Grupos/detalle/People y regresiones de permisos existentes |
| Grupos/detalle | Papel con banda teal, rol/count reales, CTA/entrada a listas/Personas | groups y GroupSurfaces; Owner/Admin/Member, cero/uno/diez |
| Productos | Búsqueda y filas protagonistas, Historial textual, editar con tooltip | CatalogDisclosure, CatalogCreation, catalog y completion |
| Mercados/historial | Administración por tab/diálogo y filtros solo con producto | catalog/completion/calendar-dates |
| Perfil | Panel de datos, campos/límites intactos, avatar/iniciales y logout textual | profile/auth/app-sections |
| Estados | Vacío/carga/parcial/error/retry textuales, glyph decorativo del mapa | VisualSemantics y app-sections |

## Hallazgos corregidos (archivo:línea)

- src/features/groups/pages/GroupsPage.tsx:49 - formulario permanente y links sin jerarquía → diálogo, tarjetas y metadatos efectivos.
- src/features/product-catalog/pages/ProductCatalogPage.tsx:109 - formularios/filtros simultáneos → contenido primero, tabs y contexto.
- src/features/product-catalog/components/CatalogManagement.tsx:42 - creación/edición siempre abiertas → diálogos reutilizando la lógica.
- src/features/groups/components/GroupSurfaces.module.css:20 - input42px pese a wrapper44 → input44px.
- src/features/groups/pages/GroupsPage.tsx:26 - retorno de foco podía adelantarse al fin de pending → espera de mutación terminada.
- src/app/layout/MainLayout.tsx:26 y src/features/home/HomePage.tsx:83 - Tab nativo de WebKit omitía skip link → tabindex0 explícito, sin cambiar orden ni simular foco en prueba.

## Límites

No se afirma lector humano, zoom real200/400, teclado virtual o dispositivo físico ejecutados. [Diferidos autorizados](manual-qa-deferred.json). Reflow y toque emulado son comprobaciones separadas. No se cambian006/007/008/009/010 ni su aceptación. Group lifecycle and invitations intentionally out of scope for 011.

- src/features/shopping-lists/components/ManageListActions.tsx:20 - la papelera también se usaba en Cerrar lista/Desactivar mercado; se reserva para eliminación y se pasa close explícito en los otros dos contextos. Se mantienen texto, confirmación y operación.

## Correcciones transversales tras inspeccionar las17 vistas

- src/features/shared-lists/pages/SharedListsPage.tsx:56 - panel/link antiguo y metadatos concatenados → nota existente, nombre accesible y descripción de resumen separados; Gastos conserva enlace propio sin anidarlo.
- src/features/settlements/components/PurchaseCapture.tsx:57 - acciones contiguas en Gastos personales → espaciado ui.stack existente, sin cambiar operación ni campos.
- src/features/home/HomePage.tsx:52 - copy de Microsoft obligatorio → acceso a cuenta MiKompri en ayuda y FAQ; no handlers ni configuración de proveedores modificados.

[Manifest de17 vistas/68 capturas](all-sections-screenshots.json): móvil390 en Chromium/Firefox/WebKit y escritorio1280 en Chromium. Se inspeccionan todas las secciones en móvil y comparaciones de escritorio/motores; no se exige pixel-perfect ni se acepta una baseline para ocultar un fallo. Los escenarios automatizados añaden320/430/768 y nombres largos/estados límite. Las barras fijas aparecen en la posición de viewport en capturas fullPage largas; no se ocultan para fabricar una imagen. Eso no acredita un ensayo de teclado virtual/safe-area físico.

Inspección final: confirmados en las capturas regeneradas la nota compartida con metadatos separados, espaciado de Gastos personales, título del historial antes de filtros, acciones secundarias y selección del shell. Comparadas variantes de catálogo/grupos/perfil/compartidas en Firefox y WebKit y escritorio Chromium. La portada conserva hero/FAQ y ejemplo; solo se corrige el texto de acceso a cuenta MiKompri. No se restaura configuración antigua.
