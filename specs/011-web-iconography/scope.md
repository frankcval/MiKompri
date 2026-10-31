# 011 — Iconografía y lenguaje visual de nevera

Formalización31/10/2026 en `011-web-iconography`, desde main9597af5 con PR59 integrado. [Spec](spec.md), [plan](plan.md), [diseño](design.md), [mapa](icon-map.md), [tasks](tasks.md).

Petición actual: React Icons, fluido/intuitivo/actual, inspirado en papel/notas y pizarra de la nevera. Propuesta Nevera compartida: papel claro, pizarra verde localizada, teal de marca y acento limón; MiKompri Sans existente, targets44 y acciones claras. React Icons5.7.0/Phosphor tiene31exports verificados; sin instalar todavía. Fluent UI sigue como base de componentes.

El alcance preliminar prefería @fluentui/react-icons. Se sustituye esa elección por la solicitud actual interpretada como react-icons y familia única pi; [comparación](research.md), [ADR005 propuesto](../../docs/adr/ADR-005-web-visual-language.md). No mezclar ambos paquetes app ni cambiar el framework. El significado/labels/estado se podrá adaptar a MAUI después, sin entregar MAUI.

Se preservan rutas, filtros/orden/historial, compra/Gastos, permisos, precios, identidad y configuración010-A. La metáfora no añade notas libres, arrastrar/pinear listas, dibujo o nuevo modelo de tablero.012/invitaciones,010-B, fotos/OCR, chat, themes persistidos y rediseño completo de landing fuera. No cambiar tareas/gates anteriores.

Esta ejecución entrega documentación, auditoría y tres referencias visuales estáticas; no React/CSS productivo, instalación, nuevos endpoints/BD/cloud ni implementación. Diseño para revisar antes de la siguiente fase.
