# Accesibilidad011 — alcance de la evidencia

31/10/2026. Candidato y comandos en [validación](validation.md), tokens calculados en [contraste](contrast.json). No equivale a certificación ni aceptación manual completa.

| Comprobación | Evidencia y límite |
|---|---|
| Iconos decorativos/nombres independientes | Tests AppIcon/IconAction/VisualSemantics; SVG aria-hidden y no focusable, labels antes de tooltip, link/button reales |
| Teclado/tooltip | Tests foco/hover/Escape/Enter, nombre suplementario sin sustituir label; navegador verifica Drawer/restauración y summary/Escape |
| Targets | Navegación medida >=44x44; wrappers min44; summary, checkboxes, botones y enlaces mantienen44 en CSS y escenarios responsive |
| Destructivas | Texto+glyph+contorno, confirmación con nombre de lista y cancelación con foco restaurado; no borrado desde tarjeta ni gesto exclusivo |
|320/390/430/768/1280 | Escenarios de navegación/notas/detalle y regresiones previas, comprobación scrollWidth; nombres largos/100productos/estimación parcial |
| Forced-colors/reduced-motion | Emulación combinada con100productos y compra/foco; conserva texto/estado nativo. No sustituye ensayo humano del sistema operativo |
| Contraste | Parejas aprobadas conservadas por token, controles con borde necesario, texto papel sobre pizarra y foco claro; axe sobre DOM real en suite final |
| Estados | Vacío/loading/parcial/error y retry siguen textuales; el glyph nunca es la única explicación |
| Touch | Acciones accesibles mediante clic, menú y texto sin hover obligatorio; falta comprobación táctil humana en dispositivo real |
| Zoom/reflow | Viewport320 y textos largos cubren reflow automatizado. Zoom real200%/400% y tamaño de texto del sistema pendientes; no confundir emulación con zoom humano |
| Lector | Pendiente ensayo NVDA/TalkBack/VoiceOver; nombres/roles/axe no acreditan anuncio, orden ni experiencia de lector real |
| Teclado virtual/safe-area | Código existente preservado y padding/barra revisados; pendiente teclado virtual y safe-area de dispositivo físico |

Las excepciones axe de sentinelas Tabster conservan exactamente el filtro existente, solo `aria-hidden-focus` en `<i data-tabster-dummy=...>`. No se deshabilitan reglas, colores ni componentes para obtener PASS.

T019/T025 y AC09 permanecen abiertos por las comprobaciones humanas indicadas. Las tareas pendientes de007/008/009 no se cierran mediante esta evidencia.
