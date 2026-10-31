# Tasks011 — implementación posterior a revisión de diseño

**Input**: [spec](spec.md), [plan](plan.md), [research](research.md), [diseño](design.md), [icon-map](icon-map.md), [contrato](contracts/ui-iconography.md).
**Estado**: formalización terminada; todas las tareas de implementación siguen abiertas. Las referencias/auditorías documentales no prueban comportamiento productivo. No ejecutar012/010-B ni tocar auth/BD/env.

## Phase1 — revisión y baseline

- [ ] T001 Ratificar propuesta visual y ADR-005 con revisión de `specs/011-web-iconography/design.md`, sus tres referencias y `docs/adr/ADR-005-web-visual-language.md`; registrar cambios pedidos antes de implementar.
- [ ] T002 Capturar baseline real de shell/listado/detalle, imports/chunks y métricas vigentes en `specs/011-web-iconography/evidence/implementation.md`; no afirmar que una captura acredita accesibilidad.
- [ ] T003 Incluir rama011 en `.github/workflows/ci-mikompri-web.yml` para push conservando gates/motores/budgets, sin omitir pruebas ni modificar workflows backend innecesarios.

## Phase2 — foundation compartida

- [ ] T004 Instalar solo react-icons5.7.0 en `MiKompri.Web/package.json` y `package-lock.json`, revalidar export/licencia de `evidence/icon-audit.json` y preservar notice; typecheck/build inicial sin imports de otros packs.
- [ ] T005 Escribir tests funcionales de claves cerradas/exports/tamaño/selección/decoración en `MiKompri.Web/tests/unit/ui/iconRegistry.spec.ts`, fallando antes del registro.
- [ ] T006 Implementar catálogo estático y `MiKompri.Web/src/shared/ui/icons/iconRegistry.ts`, `AppIcon.tsx` según contrato; sin namespace/dynamic imports ni SVG interactivo propio.
- [ ] T007 Escribir tests de acción icon-only (nombre, tooltip/foco/Escape, busy/disabled y semántica link/button) en `MiKompri.Web/tests/component/ui/IconAction.spec.tsx` antes del wrapper.
- [ ] T008 Implementar `MiKompri.Web/src/shared/ui/icons/IconAction.tsx` reutilizando Fluent y targets44; no incluir lógica de sesión/compra/negocio.
- [ ] T009 Introducir tokens definidos en `MiKompri.Web/src/shared/ui/theme.ts` y `AppSurface.module.css`, con foco claro/oscuro, forced-colors y reduced-motion sin selector SVG global.

## Phase3 — US1 navegación

- [ ] T010 [US1] Añadir regresiones nav/selected/detalle-padre/Drawer/foco en `MiKompri.Web/tests/component/auth/PersistentNavigation.spec.tsx` y `tests/e2e/modernization/iconography.spec.ts` antes de cambiar shell.
- [ ] T011 [US1] Aplicar icono+etiqueta y estados en `MiKompri.Web/src/app/layout/MainLayout.tsx`, `MainLayout.module.css` y `shared/ui/AccountMenu.tsx`, conservando marca, tres destinos y Mi cuenta textual.
- [ ] T012 [US1] Validar shell/Drawer/barra inferior a320/390/430/desktop, teclado y safe-area con `tests/e2e/modernization/iconography.spec.ts`; sin navegación oculta focusable ni icon-only principal.

## Phase4 — US2 notas de listas

- [ ] T013 [US2] Añadir tests de proyección/queries/filtro/orden/active-history/error parcial/unknown price en `MiKompri.Web/tests/component/features/shopping-lists/` antes de reskin.
- [ ] T014 [US2] Aplicar cabecera pizarra/notas en `MiKompri.Web/src/features/shopping-lists/pages/ListsPage.tsx`, `components/ListOverview.tsx` y CSS compartido manteniendo orden DOM y datos existentes.
- [ ] T015 [US2] Validar en `tests/e2e/modernization/iconography.spec.ts` nombres largos, vacío/carga/error parcial y todos los query params; conservar creación personal/compartida e historial009.

## Phase5 — US3 papel de trabajo

- [ ] T016 [US3] Escribir regresiones checkbox/foco/mark-unmark/100ítems/precios desconocidos en `MiKompri.Web/tests/component/` y `tests/e2e/modernization/iconography.spec.ts` antes del estilo de detalle.
- [ ] T017 [US3] Aplicar hoja/fila/jerarquía y targets en `MiKompri.Web/src/shared/ui/ShoppingListView.tsx`, `AppSurface.module.css` y páginas de detalle existentes, sin reemplazar semántica de Checkbox.
- [ ] T018 [US3] Ejecutar recorridos compra opcional/Gastos/idempotencia/foco al marcar y reabrir desde historial en `MiKompri.Web/tests/e2e/modernization/`; documentar resultado sin cambiar APIs/operaciones009.

## Phase6 — US4 ayudas y acciones

- [ ] T019 [US4] Cubrir hover/foco/Escape/lector/coarse pointer y texto esencial visible en `tests/component/ui/IconAction.spec.tsx` y `tests/e2e/modernization/iconography.spec.ts`; no tooltip como único error.
- [ ] T020 [US4] Sustituir glyphs app de menú/cerrar/acciones secundarias por mapa en `MiKompri.Web/src/app/layout/MainLayout.tsx` y componentes afectados; preservar texto de primarias/logout/destructivas y excepciones Fluent/brand.
- [ ] T021 [US4] Verificar borrado con confirmación/nombre/contexto/restauración de foco y ausencia de interactivos anidados en `tests/e2e/modernization/iconography.spec.ts`; no long-press exclusivo ni borrado desde toda la nota.

## Phase7 — US5 coherencia transversal

- [ ] T022 [US5] Añadir cobertura de semántica/labels y estados en `MiKompri.Web/tests/component/features/product-catalog/`, grupos/perfil y estados compartidos antes de adaptar sus controles.
- [ ] T023 [US5] Adaptar iconos/tokens de catálogo/mercados/grupos/perfil/estados en componentes existentes de `MiKompri.Web/src/features/` y `src/shared/ui/states/`; no nuevos destinos, permisos, forms o endpoints.
- [ ] T024 [US5] Ejecutar regresiones auth49 y ambas rutas Google/Microsoft del candidato con `MiKompri.Web/tests/unit/auth`, `tests/component/auth`, `tests/integration/auth` y QA normal cuando corresponda; no restaurar configuración antigua ni modificar providers.

## Phase8 — validación final y evidencia

- [ ] T025 Validar320/390/430/768/1280, zoom/reflow, teclado/lector, forced-colors, reduced-motion, targets y touch/teclado virtual en `specs/011-web-iconography/evidence/accessibility.md`; declarar pendientes reales sin fingir PASS.
- [ ] T026 Comparar capturas implementadas y estados límite con `specs/011-web-iconography/design/` en Chromium/Firefox/WebKit, registrar revisión en `evidence/implementation.md`; ninguna baseline nueva se acepta para ocultar regresión.
- [ ] T027 Medir imports/chunks/gzip y protocolo20muestras por ruta/budgets008/009 en `specs/011-web-iconography/evidence/performance.md`; documentar incumplimientos heredados/impacto sin ampliar umbrales o retries.
- [ ] T028 Ejecutar suites Web afectadas/completas, lint/typecheck/build y E2E smoke en tres motores; guardar comandos/SHA/resultados en `specs/011-web-iconography/evidence/validation.md`, sin tratar mocks como aceptación real.
- [ ] T029 Construir Docker Web con configuración pública de compilación aplicable y verificar que `MiKompri.Web/Dockerfile` no incluya artifacts/tests/env privados; sin cambios de BD/cloud/backend por la capa visual.
- [ ] T030 Reconciliar `specs/011-web-iconography/checklists/acceptance.md`, `tasks.md` y README según evidencia; no cerrar007/008/009 ni diferidos010 por presencia de iconos.
- [ ] T031 Verificar CI Web/checks aplicables del SHA final y diff sin APIs/modeloEF/auth/env no autorizados, documentar en `specs/011-web-iconography/evidence/validation.md`.
- [ ] T032 Revisar DoD/ADR/diseño final con `specs/011-web-iconography/spec.md` y preparar PR si procede, sin merge/deploy automático ni implementación de012/010-B.

## Dependencies & Execution Order

T001→baseline/CI→paquete→tests foundation→wrappers/tokens→US1→US2→US3→US4/US5→validación. Las pruebas de cada corte preceden cambios; no agentes ni ejecución paralela implícita. US1 aporta navegación por sí misma, US2 mejora el listado, US3 la compra; US4 aporta ayudas/acciones y US5 coherencia transversal.

## Definition of Done

Spec/diseño revisados y todas las aceptaciones aplicables con evidencia del candidato implementado; fuente/familia/licencia/imports correctos; accesibilidad/estados/responsive/rendimiento/CI verificados. Las32 tareas no están hechas en esta formalización. No marcar un requisito por existir un test o una captura estática.
