# Dirección visual011 — Nevera compartida

Dirección ratificada el31/10/2026 al autorizar el titular la siguiente fase. **Implementación en validación**, sin declarar cierre por las referencias estáticas. La inspiración del usuario es papel, notas y pizarra de la nevera para apuntar compras. La aplicación se siente doméstica y cercana, con claridad de producto actual y controles previsibles.

## Idea y contención

Un único gesto memorable: **cada lista es una nota de compra**, sobre una superficie de nevera clara; una cabecera verde pizarra organiza la sección. La información sigue alineada y horizontal, con jerarquía fuerte y mucho espacio para marcar productos. La nota no se arrastra ni se pinea. Los pequeños imanes son decoración no interactiva y prescindible en alto contraste.

Autocrítica: una interfaz crema+serif+terracota sería genérica y ajena a MiKompri. Se mantiene el teal de marca y MiKompri Sans; no serif/manuscrita nueva ni grano de imagen. Se evita una pared de cards idénticas para todos los formularios: papel solo para listas; paneles planos para catálogo/perfil/errores. Sin cristales borrosos, gradientes de adorno o notas inclinadas que reduzcan lectura.

## Paleta base

| Token conceptual | Hex | Uso |
|---|---|---|
| Nevera | #F2F4EE | Fondo principal, limpio y neutro |
| Papel | #FFFDF6 | Notas/listas, superficie de lectura |
| Tinta | #20312C | Texto y títulos |
| Pizarra | #23483D | Cabecera contextual breve, texto claro |
| Imán/acción | #006B68 | Teal existente, primarias/foco/enlaces |
| Nota limón | #F3E6A3 | Acento ocasional, con tinta; nunca estado único |

Complementarios funcionales: texto secundario#56645E; borde necesario de control#6B7B70; borde decorativo#D6DDD2; peligro#A52C2C; superficie blanca#FFFFFF; selección#DDEBE1. No asignar significado de personal/compartida o éxito/error únicamente al tono del papel. [Contrastes calculados](evidence/contrast.json).

## Tipografía y ritmo

MiKompri Sans existente (derivada de Source Sans3, licencia ya conservada) + fallback Segoe UI/sans-serif. Sin descargas de fuentes nuevas. Títulos28/34px móvil y32/40px escritorio, peso600/700; título nota22/28; producto16/24 con peso500; metadatos14/20 solo secundarios, nunca acciones principales diminutas. Importes tabulares. Alineación izquierda, líneas legibles y wrap; nombres no se cortan para proteger un layout rígido.

Espaciado4/8/12/16/24/32. Margen móvil16 y escritorio32/40; anchura principal existente1440. Papel12px de radio, control8px y pizarra16px: formas con función. Sombra corta0 2px 4px con tinta al8%; no sombra difusa en cada control ni blur caro. Regla fina entre productos para recordar una lista en papel, sin rayado detrás del texto.

## Estructura de pantallas

### Mis listas — móvil primero

```text
marca                     Mi cuenta / menú
┌ Pizarra: Mis listas                     ┐
│ 2 listas activas  [Crear lista personal] │
└─────────────────────────────────────────┘
[Activas] [Historial]   (misma query view)
[Todas] [Personales] [Compartidas]
Ordenar listas [Más recientes]
┌ Nota: Compra de la semana ──────────────┐
│ Personal · 5 pendientes / 8productos    │
│ Estimación parcial22,20€ · 2 sin precio │
└─────────────────────────────────────────┘
┌ Nota: Cena del viernes / Casa ──────────┐
...
[Mis listas]      [Grupos]      [Productos]
```

Preservar tres destinos existentes y Mi cuenta con texto en cabecera (a320 se puede ocultar su glyph decorativo para dejar espacio, no su etiqueta); no inventar navegación a nuevas funciones. Historial sigue siendo una vista local de listas. Menú móvil conserva acciones/rutas; no duplicar focos visibles con barras ocultas por breakpoint. Botones importantes con verbo y texto. Crear compartida permanece enlace/contexto existente, no un nuevo flujo de elección/endpoint.

### Detalle — la lista se convierte en papel de trabajo

Volver, título/contexto, progreso y estimación quedan antes de los productos. Añadir productos sigue visible; cada fila prioriza checkbox44px, nombre y cantidad/precio, y Más opciones44px. Listas de100 productos permanecen lista semántica, sin masonry/posiciones absolutas. Marcar aplica estilo legible y estado checked; no borra texto ni cambia reglas de historial. Compra real opcional y Gastos conservan su circuito y diálogos.

### Escritorio

Barra lateral existente224px, nav icono+texto; contenido con pizarra horizontal y dos/tres columnas de notas según espacio real. DOM en el orden de sort; no masonry que cambie lectura. El detalle utiliza una hoja amplia legible, no una nueva vista dividida. Referencias son pantallas distintas, no nueva navegación multipanel.

### Catálogo/mercados, grupos y perfil

Reutilizar tokens y acciones/iconos, manteniendo formularios y paneles planos. Mercados sigue dentro del catálogo actual. Permisos Owner/Admin/Member y errores permanecen. Perfil conserva inputs, versión, clear y acceso Google/Microsoft; logos externos y marca no se sustituyen por Phosphor. Landing solo recibe tokens comunes cuando corresponda, sin rehacer contenido/hero.

## Iconos y tooltips

React Icons5.7.0/Phosphor (`react-icons/pi`), Regular base/Fill seleccionado según [mapa](icon-map.md). Glyph20 para acciones y24 para navegación, target44/48px. `currentColor`, sin sobrescribir geometry/stroke de un icono generado. Botones Fluent con texto+icono; wrapper compartido para los icon-only necesarios. Logos e iconos internos de Checkbox/Select/Spinner/Menu de Fluent son excepciones explícitas.

Tooltip es una ayuda pequeña con vocabulario de la acción, no un post-it persistente. Foco/hover, Escape, no robo de foco ni contenido interactivo. Aria-label existe independientemente del tooltip; relación description para explicación adicional, label solo si corresponde sin duplicar nombres. En móvil, acción identificable por etiqueta/menú; ningún dato vital depende del hover. Borrar y Cerrar sesión conservan texto.

## Estados y accesibilidad

Estilo seleccionado: Fill + texto/peso/indicador + aria-current, no solo color. Error: texto con causa/recuperación, icono secundario, sin tooltip como único aviso. Loading: Spinner/estado existente, sin saltos de layout; vacío: mensaje y acción real. Selección/checkbox/disabled/pressed mantienen semántica. Focus ring3px, forced-colors con system colors; reduced-motion desactiva movimiento nuevo. Reflow/zoom hasta400%, targets>=44x44, safe-area y teclado virtual verificados después de implementar.

Movimiento100–160ms solo para respuestas a acciones, sin librería extra ni entradas automáticas en cascada. La compra no se retrasa por una animación y no hay confeti ni vibración añadidos.

## Referencias visuales de revisión

- [Mis listas390px](design/mobile-lists.svg) / [PNG](design/mobile-lists.png).
- [Detalle390px](design/mobile-shopping.svg) / [PNG](design/mobile-shopping.png).
- [Mis listas escritorio1440px](design/desktop-lists.svg) / [PNG](design/desktop-lists.png).

Datos ficticios y SVG estáticos, sin interacción, servidor ni credenciales. Ilustran intención/jerarquía, no acreditan accesibilidad, rendimiento ni implementación. Las medidas definitivas se validan en DOM real y Fluent; importes/estados de ejemplos deben ser coherentes. [Licencias de iconos](design/NOTICE.md).

## Secuencia visual y reversión

Primero registro/acciones/navegación, después Mis listas/detalle y finalmente adaptación común de catálogo/grupos/perfil. Comparar capturas y pruebas antes de ampliar. Revertir cambios de CSS/tokens/componentes de011 si es necesario; nunca auth/BD/env como efecto colateral. La implementación actual se compara con estas referencias; las verificaciones pendientes figuran en tasks.md y evidencia.

## Refinamiento Grupos autorizado

Mantener papel#FFFDF6, tinta#20312C y teal#006B68 con la misma sans/ritmo. Cada grupo es un espacio compartido: icono groups sobre acento suave, nombre22px, rol/personas secundarios cuando llegan en el DTO y acción textual Ver grupo. Sin imán de lista ni rayado de compra; banda lateral y paneles planos distinguen colaboración de productos. Móvil una columna; desktop grid según espacio, nombres envueltos. Una sola tarjeta conserva tamaño intencional.

Cabecera con CTA Crear grupo; diálogo Fluent reutiliza el formulario sencillo y feedback existente. Empty state explica crear un primer grupo sin anunciar invitaciones. Detalle: nombre/contexto real → entrada Listas compartidas → Personas del grupo; no acciones duplicadas ni lifecycle ficticio. La revisión visual compara lists/groups/catalog sin igualar sus estructuras.
