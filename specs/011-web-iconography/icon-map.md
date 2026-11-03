# Mapa semántico011 — React Icons/Phosphor

Todos los exports de esta tabla están comprobados en react-icons5.7.0, `react-icons/pi`; [manifest](evidence/icon-audit.json),31 símbolos,0 ausentes. La auditoría original comprobó31; el runtime final importa solo22exports usados. Los nueve restantes se retiran del bundle, sin cambiar familia/versión. Nombre semántico estable distinto del componente externo; no crear rutas o acciones para justificar un icono.

## Navegación y estados

| Semántica | Regular/base | Fill/seleccionado | Label | Ubicación |
|---|---|---|---|---|
| lists | PiClipboardText | PiClipboardTextFill | Mis listas | /lists y detalle personal/compartido |
| history | PiClockCounterClockwise | PiClockCounterClockwiseFill | Historial | vista local view=history; no destino global nuevo |
| groups | PiUsersThree | PiUsersThreeFill | Grupos | /groups |
| products | PiBasket | PiBasketFill | Productos | /product-catalog |
| markets | PiStorefront | mismo base | Mercados | sección existente de catálogo |
| account | PiUserCircle | mismo base | Mi cuenta | /profile |
| personal-context | PiUser | mismo base | Personal | contexto con texto; no es acción |

Fill solo si representa selección/estado activo definido; label/peso/indicador/aria-current o checked transmiten estado además del color. Iconos de contexto meramente decorativos, no botones. Se puede no renderizar el glyph si el texto existente explica mejor el concepto.

## Acciones y feedback

| Semántica | Export verificado | Label/regla |
|---|---|---|
| add | PiPlus | Añadir productos / Crear lista personal |
| search | PiMagnifyingGlass | Buscar productos |
| edit | PiPencilSimple | Editar {nombre} |
| delete | PiTrash | Borrar lista {nombre} |
| more | PiDotsThree | Más opciones de {nombre} |
| back | PiArrowLeft | Volver a mis listas |
| close | PiX | Cerrar {diálogo, menú o lista} / Desactivar {mercado}, siempre con texto en cambios de estado |
| information | PiInfo | Información de {campo} |
| error | PiWarningCircle | Texto de error visible |
| menu | PiList | Abrir menú |
| logout | PiSignOut | Cerrar sesión |

Misma palabra y acción en toda la Web; no usar receipt como sinónimo de precio unitario ni trash para cerrar/cancelar. Borrar/logout conservan texto; no icon-only primario. Glyph20/24px, target44/48px. Estado y label dinámicos toman el nombre del recurso existente, sin inyectar HTML.

## Excepciones y accesibilidad

Marca MiKompri y logos de proveedores no se sustituyen; no añadir SiGoogle/Microsoft genérico ni rediseñar branding. Glyphs internos Checkbox/Select/Spinner/Menu/Drawer de Fluent permanecen del framework; no reimplementar esos controles solo para lograr una familia única. SVG app decorativos aria-hidden/focusable=false. Icon-only usa botón/link/summary real con nombre y tooltip independiente; no div clicable. Historial y Mercados no pasan a ser destinos globales nuevos.

MAUI reutiliza las claves semánticas, labels y significado Regular/Fill. Adaptará su implementación/asset, sin React ni copiar indiscriminadamente todo el paquete.

Optimización final: no se importan purchased-status, sort, filter, receipt ni price sin consumidores productivos; checkbox/Select y texto actuales conservan su semántica. No Fill de account/markets/personal-context porque no hay selección productiva que lo use. Back se utiliza en el detalle de grupo. Los exports retirados siguen acreditados en el manifest histórico, no se presentan como cargados.
