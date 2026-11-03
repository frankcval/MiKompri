# Research011 — decisiones del31/10/2026

Investigación previa a implementación. No instalación, modificación de package.json/lock, nuevos servicios ni configuración de identidad. Se conserva Fluent UI y el entorno010-A funcional. [Baseline](evidence/current-state.md), [auditoría exacta](evidence/icon-audit.json).

## R1 — React Icons frente al alcance preliminar

**Decision:** interpretar literalmente la petición actual como `react-icons`, fijar5.7.0 y usar solo Phosphor (`react-icons/pi`). Sustituye la preferencia exploratoria @fluentui/react-icons de scope.md; no reemplaza @fluentui/react-components.
**Rationale:** familia única con formas suaves, Regular/Fill para estado y vocabulario amplio de listas/acciones. La unidad visual procede de mantener familia/semántica, no de mezclar packs que ofrece la librería. Se reutilizan los controles accesibles Fluent.
**Alternatives:** Fluent Icons encaja mejor con los pictogramas internos de Fluent y era la propuesta preliminar; sigue viable si el usuario prefiere ese paquete, pero no se instala en paralelo. Lucide/Feather son otra opción de React Icons, no elegida para evitar una mezcla y por conveniencia de pares Fill.
**Evidence:** [React Icons oficial](https://github.com/react-icons/react-icons), [catálogo Phosphor](https://react-icons.github.io/react-icons/icons/pi/), [metadata5.7.0](https://registry.npmjs.org/react-icons/5.7.0). npm view consultado31/10 y tarball oficial descargado solo para lectura, SRIsha512 coincidente;31 exports comprobados en pi/index.d.ts y módulo. No se asume que el nombre de Phosphor React directo sea el mismo que el prefijo Pi de React Icons.

## R2 — Paquete, licencias y compatibilidad

5.7.0 es la versión consultada, fijada para reproducibilidad; no usar latest durante implementación sin reauditar. Peer React declarado `*`; el proyecto usa React18.3.1/TypeScript6.0.3/Vite8.3.4. Es compatibilidad declarada, **no build integrado probado**: typecheck/build después de instalar son gates. Root React Icons y familia Phosphor indican MIT; [licencia upstream](https://github.com/phosphor-icons/core/blob/main/LICENSE), notice en design/NOTICE.md para paths derivados.

Importar símbolos concretos, solo de react-icons/pi; no `import *`, import dinámico por string ni registry de todas las familias. Que la librería permita ES imports no demuestra un peso concreto: comprobar chunks/imports y medir gzip después; no prometer0KB ni sustituir budgets. Mantener dependencia separada de logos Google/Microsoft y de los iconos internos de los componentes Fluent.

## R3 — Ayudas que no oculten la acción

**Decision:** reutilizar Tooltip de Fluent y un wrapper solo para los icon-only necesarios. Etiquetas visibles en navegación/primarias/logout/destructivas. Nombre accesible siempre independiente de hover. Ayuda adicional con foco/hover y Escape, sin foco dentro; contenido esencial/error permanece visible. En toque no se exige mantener pulsado ni descubrir hover.
**Evidence:** [Fluent Tooltip](https://fluent2.microsoft.design/components/web/react/core/tooltip/usage), [APG Tooltip](https://www.w3.org/WAI/ARIA/apg/patterns/tooltip/). APG señala que ese patrón está en progreso; no se presenta como consenso normativo definitivo. Se exige conducta probada con teclado/lector además de roles.

## R4 — Tamaño, contraste y percepción de estados

44x44CSSpx es el criterio de producto ya existente y el mínimo elegido para controles, no el tamaño del glyph. [WCAG2.2AA2.5.8](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html) establece24x24 con excepciones; no afirmar que44 sea su mínimo AA. [2.5.5enhanced](https://www.w3.org/WAI/WCAG22/Understanding/target-size-enhanced.html) contempla44. [Contraste de texto](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html):4.5 normal; controles/foco necesarios3. Estados con texto/semántica/forma y no solo color. [Fluent iconography](https://fluent2.microsoft.design/iconography/) mantiene principios de familiaridad/estado, aunque los glyph app elegidos sean Phosphor.

La paleta propuesta tiene11 pares calculados en evidence/contrast.json. Eso no acredita el DOM, semántica, lector ni foco real. Forced-colors y reduce-motion se verifican cuando haya implementación; no oscurecer texto comprado hasta perder contraste.

## R5 — Metáfora de nevera sin nuevo tablero funcional

**Decision:** pizarra verde localizada en cabecera y notas papel para listas. Mantener tipografía/teal de marca, orden DOM y layout responsive existente. Sin fotos/texturas pesadas/drag/pinning/dibujo/manuscrita externa. Las superficies planas de formularios no se convierten todas en post-its.
**Rationale:** el usuario quiere el carácter de los papeles de compra, pero debe seguir pudiendo reconocer productos, marcar y ver precios/contexto en el supermercado. Es una decisión de diseño del brief, no una afirmación estadística sobre tendencias.
**Trade-offs:** un poco más de personalidad y espaciado puede reducir densidad; probar320px/100ítems. La pizarra no es modo oscuro global. Nombres largos y zoom hacen crecer tarjetas/filas; no se recortan para mantener una ilustración.

## R6 — Reutilización MAUI y límites

Catálogo neutral de significados, labels y reglas de tamaño/estado; adaptación futura de SVG/glyphs y permisos/licencias por plataforma. No compartir ReactType ni dependencia con un cliente MAUI inexistente. TP11/identidad y contratos de negocio no cambian;012 invitaciones y010-B permanecen fuera.

No incertidumbres técnicas ocultas: se fija una alternativa para la propuesta; aprobación estética y medición de bundle/rendimiento se realizan en sus fases, no se presentan como pruebas ya ejecutadas.
