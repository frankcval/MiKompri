# Contrato de UI011 — iconos y superficies

Contrato frontend propuesto, no endpoint ni implementación. Propietario: capa shared/ui; backend y persistencia permanecen iguales.

## AppIcon

Entrada: clave cerrada del mapa, tamaño20/24, seleccionado cuando haya par y clase/token de color. Salida: SVG de un export estático de react-icons/pi. No title/boton/tabIndex propio para decoración; aria-hidden=true y focusable=false. Consumidores no pasan JSX arbitrario ni strings remotos. Clave desconocida se detecta en typecheck/tests; no se carga otro pack por fallback silencioso.

Mapas o módulos por ruta pueden separar importes usados, sin namespace global de9072 exports. El helper no necesita proveedor/contexto adicional ni estado de cuenta. No modificar geometría/stroke de Phosphor ni inferir accesibilidad por el nombre del export.

## Acción con icono

Button/Link/MenuItem/summary reales según el comportamiento existente. Primarias/navegación/logout/destructivas conservan texto. Icon-only exige label no vacío, glyph decorativo, target>=44x44 y Tooltip Fluent con contenido de la acción. Estado busy/disabled/pressed/expanded viene del control actual; el wrapper no sustituye lógica ni navega por su cuenta.

Tooltip suplementario: hover y foco, Escape, no foco interno/HTML interactivo ni autoapertura al cargar. Nombre accesible siempre disponible sin tooltip. Relación label/description elegida según función, sin duplicación de lectura. En toque hay etiqueta/contexto/menú; error o condición para continuar es visible, no hover-only.

Menús/diálogos de borrado mantienen confirmación y restauración de foco. No insertar botones dentro de links de tarjeta ni de summary como interactivos anidados; no listeners de borrado en toda la nota. Checkbox/Select/Spinner internos de Fluent conservan semántica y glyphs.

## Superficies/tema

Tema único existente de Fluent, extendido con tokens de design.md en theme.ts/capa compartida. Scope CSS por rol de superficie, no selectores globales para todos los SVG/botones. Foreground/currentColor desde tokens. Focus3px en claro:teal; en pizarra:papel claro (teal sobre verde oscuro no es el par de foco aprobado). Contorno decorativo de papel no sustituye el borde de control/foco contrastado.

320px/zoom/texto largo envuelven y aumentan altura. Fondo/ornamento sin events/foco/aria funcional; safe-area y padding final del shell evitan tapar la última acción. High-contrast usa system colors y selected/labels mantienen significado sin relleno decorativo. Reduced-motion elimina movimiento nuevo, sin retrasar guardado ni animar cada nota al entrar.

## Compatibilidad y rechazo de ampliaciones

No se cambia /lists?filter=...&view=...&sort=..., rutas de detalle/grupos/productos/perfil, comandos API, cookies/tokens/redirects, namespace de sesión ni.env.local. No nueva entidad/tablero ni notas/drag/pinning/compartir/invitar. Cada prueba de11 debe poder usar los mismos fixtures/contratos007–010. Métricas y gates anteriores no se rebajan.
