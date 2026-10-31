# Implementación011 — 31/10/2026

Dirección visual/ADR-005 ratificados por la instrucción del titular «pasa a la siguiente fase», confirmada al pedir continuar. Base documental `3bee0ec`, sobre main `9597af5`/PR59. El candidato se identifica también por `sourceSha256` y lock en [auditoría del build](bundle-after.json); ese informe distingue árbol de trabajo de commit base.

## Cambios aplicados

- React Icons5.7.0 exacto, una familia `react-icons/pi`,31 imports concretos y23 significados tipados. AppIcon silencioso; variantes Fill para selección. Logos y controles internos Fluent conservados.
- IconAction usa Button/Tooltip Fluent; IconActionLink mantiene un enlace real. Nombre independiente de tooltip,44px, busy/disabled, foco/hover/Escape. No lógica de negocio/sesión en wrappers.
- Tokens Nevera/Papel/Tinta/Pizarra/Teal/Limón, espaciado y radios compartidos. Formularios planos, notas solo en listas, foco claro en pizarra. No fuente, textura raster, librería de movimiento ni tema oscuro nuevos.
- Shell con tres destinos icono+texto, Mi cuenta textual, Drawer y safe-area existentes. Link de navegación declara sección padre también en detalles; NavLink retiraba aria-current de un destino cuyo pathname no coincidía, aunque recibiera el atributo.
- Notas de listas con contexto personal/grupo, pendientes, estimación y fecha disponible. Activas/Historial son enlaces de la misma query `view`, conservando filtro/orden/ruta; sort sigue siendo Select. No cambio de clasificación009 ni agregación/precios.
- Detalle con cabecera pizarra y hoja de productos. Checkbox y progreso siguen siendo Fluent; nombre/estado/importe/desconocido siguen textuales. Más opciones usa summary nativo, glyph decorativo y Tooltip; Escape cierra el desplegable y devuelve foco sin interferir con diálogos portaled.
- Renombrar/editar/añadir/borrar/cerrar sesión conservan texto. Borrado diferenciado con verbo, glyph y contorno; confirmación y restauración existentes conservadas. Iconos/tokens comunes en catálogo, mercados, grupos, perfil y estados.

No se modifica producción backend, DTOs, permisos, EF, auth/MSAL, trust, `.env.local`, cloud ni datos. El cambio de fixture de100ítems compara segmentos completos de ID: `item-9` no debe capturar `item-99`. Era un falso resultado del simulador; no se modifica compra productiva.

## Pruebas y comparación

Tests de registro/wrapper fallaron por módulos ausentes antes de implementarlos; luego5/5 PASS. Navegación/controles de vista dieron2 fallos esperados antes del cambio y22/22 PASS después, salvo sección padre que identificó y permitió corregir NavLink. La regresión de100ítems y las de semántica/tooltip suplementario amplían el conjunto final. Los logs intermedios no sustituyen [validación final](validation.md).

Baseline visual real: [listas móvil](before-lists-mobile.png), [detalle móvil](before-detail-mobile.png), [listas escritorio](before-lists-desktop.png). Se tomó del build de laboratorio inmutable anterior a los cambios, con datos sintéticos; no de una sesión privada.

Las referencias estáticas en `design/` expresan jerarquía y metáfora. La implementación conserva los textos y flujos existentes: «Crear lista», enlaces para compartidas/grupos y tabs Compra/Gastos. No copia datos ficticios ni inventa un tablero funcional. Se capturaron y revisaron las nueve pantallas del build final: [manifest](screenshots.json).

| Motor | Listas móvil390 | Detalle móvil390 | Listas escritorio1280 |
|---|---|---|---|
| Chromium | [PNG](after-chromium-lists-mobile.png) | [PNG](after-chromium-detail-mobile.png) | [PNG](after-chromium-lists-desktop.png) |
| Firefox | [PNG](after-firefox-lists-mobile.png) | [PNG](after-firefox-detail-mobile.png) | [PNG](after-firefox-lists-desktop.png) |
| WebKit | [PNG](after-webkit-lists-mobile.png) | [PNG](after-webkit-detail-mobile.png) | [PNG](after-webkit-lists-desktop.png) |

Sin overflow en las nueve. Revisión visual: pizarra localizada, notas papel/pequeño imán decorativo, filas con reglas, controles legibles y etiquetas de navegación presentes; sin discrepancias de geometría apreciables entre motores, con diferencias normales de rasterización de fuente. Se mantiene más información/controles que en la referencia, por lo que no se afirma igualdad píxel a píxel: descripción de cabecera, tabs Compra/Gastos, renombrar/borrar y accesos compartidas/grupos son funcionalidad existente. Se conservó el orden previo de filtros y las etiquetas para no retirar funciones al copiar la ilustración. Los fixtures no incluyen createdAt; la fecha sigue apareciendo cuando el contrato la proporciona.

La vista móvil de notas sitúa la primera tarjeta antes que el baseline y mantiene padding final para poder desplazar contenido por encima de la barra fija. Las capturas full-page muestran la barra en el límite del viewport original: no implican que sea un elemento en mitad del documento. Las referencias no son evidencia de comportamiento.

## Reversión

Revertir exclusivamente commits/componentes/CSS/dependencia de011 y su workflow de rama. No restaurar archivos `.env*`, imágenes/volúmenes QA, auth010-A ni bases de datos como parte de una reversión visual. Ningún despliegue ni merge se ejecuta en esta fase.

Comprobación local adicional de la página QA `/login`: Google y Microsoft visibles simultáneamente, sin intentar autenticación ni leer/exportar sesión. Esto acredita disponibilidad de botones, no un login real nuevo.

El31/10/2026 el titular confirma después el recorrido real Google→logout→Microsoft con acceso a Mis listas y Perfil: «Ambos funcionan». [Registro y límites](validation.md#confirmación-manual-de-acceso--31102026). T024/AC08 quedan acreditadas con la confirmación y regresiones automáticas; permanecen los demás gates.
