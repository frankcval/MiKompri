# Mapa semántico011 — React Icons/Phosphor

Todos los exports de esta tabla están comprobados en react-icons5.7.0, `react-icons/pi`; [manifest](evidence/icon-audit.json),31 símbolos,0 ausentes. No instalar hasta la fase de implementación. Nombre semántico estable distinto del componente externo; no crear rutas o acciones para justificar un icono.

## Navegación y estados

| Semántica | Regular/base | Fill/seleccionado | Label | Ubicación |
|---|---|---|---|---|
| lists | PiClipboardText | PiClipboardTextFill | Mis listas | /lists y detalle personal/compartido |
| history | PiClockCounterClockwise | PiClockCounterClockwiseFill | Historial | vista local view=history; no destino global nuevo |
| groups | PiUsersThree | PiUsersThreeFill | Grupos | /groups |
| products | PiBasket | PiBasketFill | Productos | /product-catalog |
| markets | PiStorefront | PiStorefrontFill | Mercados | sección existente de catálogo |
| account | PiUserCircle | PiUserCircleFill | Mi cuenta | /profile |
| personal-context | PiUser | PiUserFill | Personal | contexto con texto; no es acción |
| purchased-status | PiCheckCircle | PiCheckCircleFill | Comprado / N de M comprados | badge opcional; checkbox Fluent conserva su glyph nativo |

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
| close | PiX | Cerrar {diálogo o menú} |
| sort | PiSortAscending | Ordenar listas |
| filter | PiFunnelSimple | Filtrar listas |
| receipt | PiReceipt | Gastos / Datos de la compra según acción existente |
| price | PiTag | Precio / Historial de precios según contexto |
| information | PiInfo | Información de {campo} |
| error | PiWarningCircle | Texto de error visible |
| menu | PiList | Abrir menú |
| logout | PiSignOut | Cerrar sesión |

Misma palabra y acción en toda la Web; no usar receipt como sinónimo de precio unitario ni trash para cerrar/cancelar. Borrar/logout conservan texto; no icon-only primario. Glyph20/24px, target44/48px. Estado y label dinámicos toman el nombre del recurso existente, sin inyectar HTML.

## Excepciones y accesibilidad

Marca MiKompri y logos de proveedores no se sustituyen; no añadir SiGoogle/Microsoft genérico ni rediseñar branding. Glyphs internos Checkbox/Select/Spinner/Menu/Drawer de Fluent permanecen del framework; no reimplementar esos controles solo para lograr una familia única. SVG app decorativos aria-hidden/focusable=false. Icon-only usa botón/link/summary real con nombre y tooltip independiente; no div clicable. Historial y Mercados no pasan a ser destinos globales nuevos.

MAUI reutiliza las claves semánticas, labels y significado Regular/Fill. Adaptará su implementación/asset, sin React ni copiar indiscriminadamente todo el paquete.
