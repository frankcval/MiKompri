# Aceptación011 — implementación y gates pendientes

Todas las casillas requieren evidencia del DOM/comportamiento real del candidato. Una referencia SVG/PNG o el cálculo de paleta no las completa. [Spec](../spec.md), [plan](../plan.md), [contrato](../contracts/ui-iconography.md).

- [x] AC01/US1: Mis listas/Grupos/Productos y detalle-padre muestran icono+label/selected/aria-current correctos en shell móvil/escritorio; Drawer y Mi cuenta accesibles.
- [x] AC02/US2: notas/pizarra aplicadas según referencias; personal/compartida, filtros/orden/query/active-history mantienen valores y estado100%comprado/desmarcado.
- [x] AC03/US2: nombre/contexto/pendientes/fecha/estimación parcial y unknown price legibles; vacío/loading/error parcial ofrecen texto/acción apropiados.
- [x] AC04/US3: marcar/desmarcar/añadir/buscar/compra opcional/Gastos mantienen semántica/foco/contratos/idempotencia;100ítems y nombres largos utilizables.
- [ ] AC05/US4: todo icon-only con label y tooltip, targets>=44x44; foco/hover/Escape y toque sin hover obligatorio, SVG decorativos silenciosos.
- [x] AC06/US4: acciones destructivas y logout conservan texto; borrado con confirmación/foco existente, sin gesto exclusivo/interactivos anidados.
- [x] AC07/US5: mismo mapa/token en catálogo/mercados/grupos/perfil/estados; marcas y glyphs internos Fluent son excepciones registradas.
- [x] AC08/US5: Google/Microsoft/return intent/caché/drafts/perfil/roles siguen operativos, sin cambios de trust/env ni tareas de010 reabiertas por estilo.
- [ ] AC09:320/390/430/768/1280, zoom/reflow, teclado/lector, forced-colors y reduced-motion documentados; no overflow ni barra/teclado tapa acciones.
- [x] AC10: contraste de colores en DOM y foco necesario, axe sin nuevas infracciones; no depender solo de color/icono para estado.
- [ ] AC11: imports estáticos/exports reales/licencia, sin pack extra ni librería de motion/fuente nueva; bundle/20muestras por ruta cumplen budgets vigentes, incumplimientos explícitos.
- [ ] AC12: tests/Web lint/typecheck/build/E2E tres motores/Docker/CI SHA final acreditados, capturas reales comparadas y rollback visual revisado.

No endpoints/migraciones/entidades de tablero nuevas; no cambios de completitud007/008/009 ni funciones diferidas010-B/012. Cierre solo con implementación y evidencia reales, no por cerrar esta formalización.

Evidencia ejecutada: [resultados](../evidence/validation.md), [capturas/comparación](../evidence/implementation.md), [límites manuales](../evidence/accessibility.md), [métricas](../evidence/performance.md). Los checks hechos cubren comportamiento/DOM del candidato, no sesiones Entra simuladas como si fueran reales. AC08 combina regresiones automáticas de sesión/perfil/aislamiento y confirmación manual del titular del acceso Google→logout→Microsoft, llegada a Mis listas y apertura de Perfil (31/10/2026). Esta confirmación no acredita por sí sola inspección manual de cachés/roles. AC05 conserva cierre manual táctil; AC09 lector/zoom/dispositivo; AC11 budgets y AC12 CI final permanecen abiertos.
