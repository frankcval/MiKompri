# Aceptación011 — revisión para PR y diferidos

Todas las casillas requieren evidencia del DOM/comportamiento real del candidato. Una referencia SVG/PNG o el cálculo de paleta no las completa. [Spec](../spec.md), [plan](../plan.md), [contrato](../contracts/ui-iconography.md).

- [x] AC01/US1: Mis listas/Grupos/Productos y detalle-padre muestran icono+label/selected/aria-current correctos en shell móvil/escritorio; Drawer y Mi cuenta accesibles.
- [x] AC02/US2: notas/pizarra aplicadas según referencias; personal/compartida, filtros/orden/query/active-history mantienen valores y estado100%comprado/desmarcado.
- [x] AC03/US2: nombre/contexto/pendientes/fecha/estimación parcial y unknown price legibles; vacío/loading/error parcial ofrecen texto/acción apropiados.
- [x] AC04/US3: marcar/desmarcar/añadir/buscar/compra opcional/Gastos mantienen semántica/foco/contratos/idempotencia;100ítems y nombres largos utilizables.
- [x] AC05/US4: todo icon-only con label y tooltip, targets>=44x44; foco/hover/Escape y toque sin hover obligatorio, SVG decorativos silenciosos.
- [x] AC06/US4: acciones destructivas y logout conservan texto; borrado con confirmación/foco existente, sin gesto exclusivo/interactivos anidados.
- [x] AC07/US5: mismo mapa/token en catálogo/mercados/grupos/perfil/estados; marcas y glyphs internos Fluent son excepciones registradas.
- [x] AC08/US5: Google/return intent/caché/drafts/perfil/roles mantienen sus regresiones; configuración y sesiones legacy conservadas. Microsoft se oculta de la entrada por decisión posterior ADR-006; no se afirma disponibilidad UI ni cutover acreditado. Sin cambios de trust/env ni tareas de010 reabiertas por estilo.
- [ ] AC09:320/390/430/768/1280, zoom/reflow, teclado/lector, forced-colors y reduced-motion documentados; no overflow ni barra/teclado tapa acciones.
- [x] AC10: contraste de colores en DOM y foco necesario, axe sin nuevas infracciones; no depender solo de color/icono para estado.
- [x] AC11:22exports estáticos/licencia, sin pack/fuentes/motion nuevos;80muestras con budgets intactos, porción011 mitigada sin empeorar baseline. GlobalLCP FAIL3rutas clasificado PRE-EXISTING PERFORMANCE DEBT no bloqueante autorizado, no PASS global; CLS/interacciones PASS. Véase performance-comparison.
- [x] AC12: tests/Web lint/typecheck/build/E2E tres motores/Docker/CI SHA final acreditados, capturas reales comparadas y rollback visual revisado.

No endpoints/migraciones/entidades de tablero nuevas; no cambios de completitud007/008/009 ni funciones diferidas010-B/012. Cierre solo con implementación y evidencia reales, no por cerrar esta formalización.

Evidencia ejecutada: [resultados](../evidence/validation.md), [capturas/comparación](../evidence/implementation.md), [límites manuales](../evidence/accessibility.md), [métricas](../evidence/performance.md). Los checks hechos cubren comportamiento/DOM del candidato, no sesiones Entra simuladas como si fueran reales. AC08 combina regresiones automáticas de sesión/perfil/aislamiento y confirmación manual del titular del acceso Google→logout→Microsoft, llegada a Mis listas y apertura de Perfil (31/10/2026). Esta confirmación no acredita por sí sola inspección manual de cachés/roles. AC05 acredita comportamiento automatizado/targets/toque emulado; AC09 y T019/T025 conservan QA humana no ejecutada como DEFERRED-NON-BLOCKING-QA. AC11 cumple la clasificación autorizada de deuda sin convertir FAIL global en PASS. AC12 acredita código final/CI; el SHA de publicación se verifica antes de PR y se informa en la entrega.

[Revisión final y26gates](../evidence/final-review.md), [resultados actuales](../evidence/validation-results.json). AC09 permanece sin cerrar: no se atribuye lector/zoom/dispositivo real a automatización.

Corrección posterior de acceso01/11: [evidencia](../evidence/authentication-recovery-2026-11-01.md). Google real y regresiones locales acreditados; Microsoft oculto por ADR-006. La casilla AC12 conserva el resultado del candidato publicado anterior: la nueva corrección debe verificar CI de su propio commit antes de integrar; no se reutiliza aquel PASS como si correspondiera al diff actual.
