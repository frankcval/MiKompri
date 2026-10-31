# ADR-005 — React Icons y lenguaje visual de nevera

**Estado:** Aceptado para011: el titular pide avanzar a implementación tras revisar la propuesta (31/10/2026). La autorización procede de esa instrucción, no de la presencia del documento.
**Fecha:**2026-10-31. [Spec](../../specs/011-web-iconography/spec.md), [research](../../specs/011-web-iconography/research.md), [diseño](../../specs/011-web-iconography/design.md).

## Contexto

011 solo tenía alcance exploratorio basado en Fluent Icons. La petición actual pide React Icons y una interfaz inspirada en papeles/pizarra de nevera. La Web ya utiliza Fluent UI9, MiKompri Sans, teal, navegación y contratos007–010. Cambiar la biblioteca de componentes o lógica de compra para lograr el aspecto aumentaría el riesgo sin aportar valor al brief.

## Decisión aceptada

Usar react-icons5.7.0, exclusivamente Phosphor/react-icons/pi, con31 exports auditados y catálogo semántico compartido. Preservar Fluent UI para controles/Tooltip y sus glyphs internos, logos y marca. Regular/Fill indica estado según mapa; texto/ARIA permanece. Guardar catálogo neutral para futura adaptación MAUI, no dependencia React en ese cliente.

Superficies Nevera/Papel/Pizarra y acentos teal/limón, tipografía actual y layout mobile-first, sin texturas remotas, nueva fuente, UI kit, librería de motion, canvas, drag/pinning ni nuevo dominio. Icon-only se limita a acciones secundarias con label/tooltip/target44. Tema mediante tokens y CSS scoped.

## Alternativas y consecuencias

Fluent Icons era la preferencia inicial y ofrece correspondencia directa con el framework. React Icons/Phosphor se elige por la solicitud actual y familia coherente; se asume una dependencia adicional que debe fijarse/medirse, no coexistencia de packs por capricho. Lucide/Feather o Phosphor React directo quedan descartados para evitar dos fuentes de iconos app.

Imports estáticos y chequeo de exports/licencia; tamaños/bundle/reflow/foco/contraste se prueban durante implementación. Referencias estáticas no acreditan accesibilidad real. Se preservan budgets y rollback de capa visual; no auth/BD/env como reversión. Sin cambios constitucionales/TP11 ni despliegue. Aprobación registrada por la instrucción explícita de pasar a la siguiente fase; el cierre de implementación requiere evidencia, no esta aprobación.
