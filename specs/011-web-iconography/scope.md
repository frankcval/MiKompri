# 011 — Web Iconography (solo alcance)

Sin implementación ni instalación. Preferir @fluentui/react-icons, comprobar exports reales de versión fijada antes de nombres. Fuente: https://github.com/microsoft/fluentui-system-icons/tree/main/packages/react-icons . Variantes Regular/Filled y bundleIcon disponibles según documentación oficial; no inventar componentes.

Mapa semántico futuro: navegación listas/historial/grupos/catálogo/mercados/cuenta; acciones añadir/buscar/editar/borrar/marcar/volver. Regular base, Filled seleccionado cuando exista par. Icon-only exige nombre accesible y tooltip, objetivo >=44x44; destructivas con texto/confirmación diferenciados. Nunca solo color/icono para estado. Preservar teclado/foco/contraste/alto contraste. Reutilizar semántica en MAUI con adaptación de assets, no dependencias React. Catálogo/mapa y pruebas se formalizan en 011, sin ampliar 009.
