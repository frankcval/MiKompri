# Tasks011 — implementación posterior a revisión de diseño

**Input**: [spec](spec.md), [plan](plan.md), [research](research.md), [diseño](design.md), [icon-map](icon-map.md), [contrato](contracts/ui-iconography.md).
**Estado**: código0923a0c validado para PR; QA humana diferida y deuda globalLCP declaradas, sin cerrar tareas anteriores. [Implementación](evidence/implementation.md), [validación final](evidence/final-review.md), [accesibilidad](evidence/accessibility.md), [rendimiento final](evidence/performance-comparison.md). Las referencias estáticas no prueban comportamiento. No ejecutar012/010-B ni tocar auth/BD/env.

## Phase1 — revisión y baseline

- [x] T001 Ratificar propuesta visual y ADR-005 con revisión de `specs/011-web-iconography/design.md`, sus tres referencias y `docs/adr/ADR-005-web-visual-language.md`; registrar cambios pedidos antes de implementar.
- [x] T002 Capturar baseline real de shell/listado/detalle, imports/chunks y métricas vigentes en `specs/011-web-iconography/evidence/implementation.md`; no afirmar que una captura acredita accesibilidad.
- [x] T003 Incluir rama011 en `.github/workflows/ci-mikompri-web.yml` para push conservando gates/motores/budgets, sin omitir pruebas ni modificar workflows backend innecesarios.

## Phase2 — foundation compartida

- [x] T004 Instalar solo react-icons5.7.0 en `MiKompri.Web/package.json` y `package-lock.json`, revalidar export/licencia de `evidence/icon-audit.json` y preservar notice; typecheck/build inicial sin imports de otros packs.
- [x] T005 Escribir tests funcionales de claves cerradas/exports/tamaño/selección/decoración en `MiKompri.Web/tests/unit/ui/iconRegistry.spec.tsx`, fallando antes del registro.
- [x] T006 Implementar catálogo estático y `MiKompri.Web/src/shared/ui/icons/iconRegistry.ts`, `AppIcon.tsx` según contrato; sin namespace/dynamic imports ni SVG interactivo propio.
- [x] T007 Escribir tests de acción icon-only (nombre, tooltip/foco/Escape, busy/disabled y semántica link/button) en `MiKompri.Web/tests/component/ui/IconAction.spec.tsx` antes del wrapper.
- [x] T008 Implementar `MiKompri.Web/src/shared/ui/icons/IconAction.tsx` reutilizando Fluent y targets44; no incluir lógica de sesión/compra/negocio.
- [x] T009 Introducir tokens definidos en `MiKompri.Web/src/shared/ui/theme.ts` y `AppSurface.module.css`, con foco claro/oscuro, forced-colors y reduced-motion sin selector SVG global.

## Phase3 — US1 navegación

- [x] T010 [US1] Añadir regresiones nav/selected/detalle-padre/Drawer/foco en `MiKompri.Web/tests/component/auth/PersistentNavigation.spec.tsx` y `tests/e2e/modernization/iconography.spec.ts` antes de cambiar shell.
- [x] T011 [US1] Aplicar icono+etiqueta y estados en `MiKompri.Web/src/app/layout/MainLayout.tsx`, `MainLayout.module.css` y `shared/ui/AccountMenu.tsx`, conservando marca, tres destinos y Mi cuenta textual.
- [x] T012 [US1] Validar shell/Drawer/barra inferior a320/390/430/desktop, teclado y safe-area con `tests/e2e/modernization/iconography.spec.ts`; sin navegación oculta focusable ni icon-only principal.

## Phase4 — US2 notas de listas

- [x] T013 [US2] Añadir tests de proyección/queries/filtro/orden/active-history/error parcial/unknown price en `MiKompri.Web/tests/component/features/shopping-lists/` antes de reskin.
- [x] T014 [US2] Aplicar cabecera pizarra/notas en `MiKompri.Web/src/features/shopping-lists/pages/ListsPage.tsx`, `components/ListOverview.tsx` y CSS compartido manteniendo orden DOM y datos existentes.
- [x] T015 [US2] Validar en `tests/e2e/modernization/iconography.spec.ts` nombres largos, vacío/carga/error parcial y todos los query params; conservar creación personal/compartida e historial009.

## Phase5 — US3 papel de trabajo

- [x] T016 [US3] Escribir regresiones checkbox/foco/mark-unmark/100ítems/precios desconocidos en `MiKompri.Web/tests/component/` y `tests/e2e/modernization/iconography.spec.ts` antes del estilo de detalle.
- [x] T017 [US3] Aplicar hoja/fila/jerarquía y targets en `MiKompri.Web/src/shared/ui/ShoppingListView.tsx`, `AppSurface.module.css` y páginas de detalle existentes, sin reemplazar semántica de Checkbox.
- [x] T018 [US3] Ejecutar recorridos compra opcional/Gastos/idempotencia/foco al marcar y reabrir desde historial en `MiKompri.Web/tests/e2e/modernization/`; documentar resultado sin cambiar APIs/operaciones009.

## Phase6 — US4 ayudas y acciones

- [ ] T019 [US4] Cubrir hover/foco/Escape/lector/coarse pointer y texto esencial visible en `tests/component/ui/IconAction.spec.tsx` y `tests/e2e/modernization/iconography.spec.ts`; no tooltip como único error.
- [x] T020 [US4] Sustituir glyphs app de menú/cerrar/acciones secundarias por mapa en `MiKompri.Web/src/app/layout/MainLayout.tsx` y componentes afectados; preservar texto de primarias/logout/destructivas y excepciones Fluent/brand.
- [x] T021 [US4] Verificar borrado con confirmación/nombre/contexto/restauración de foco y ausencia de interactivos anidados en `tests/e2e/modernization/iconography.spec.ts`; no long-press exclusivo ni borrado desde toda la nota.

## Phase7 — US5 coherencia transversal

- [x] T022 [US5] Añadir cobertura de semántica/labels y estados en `MiKompri.Web/tests/component/features/product-catalog/`, grupos/perfil y estados compartidos antes de adaptar sus controles.
- [x] T023 [US5] Adaptar iconos/tokens de catálogo/mercados/grupos/perfil/estados en componentes existentes de `MiKompri.Web/src/features/` y `src/shared/ui/states/`; no nuevos destinos, permisos, forms o endpoints.
- [x] T024 [US5] Ejecutar regresiones de autenticación y ambas rutas Google/Microsoft del candidato con `MiKompri.Web/tests/unit/auth`, `tests/component/auth`, `tests/integration/auth` y QA normal cuando corresponda; no restaurar configuración antigua ni modificar providers.

## Phase8 — validación final y evidencia

- [ ] T025 Validar320/390/430/768/1280, zoom/reflow, teclado/lector, forced-colors, reduced-motion, targets y touch/teclado virtual en `specs/011-web-iconography/evidence/accessibility.md`; declarar pendientes reales sin fingir PASS.
- [x] T026 Comparar capturas implementadas y estados límite con `specs/011-web-iconography/design/` en Chromium/Firefox/WebKit, registrar revisión en `evidence/implementation.md`; ninguna baseline nueva se acepta para ocultar regresión.
- [x] T027 Medir imports/chunks/gzip y protocolo20muestras por ruta/budgets008/009 en `specs/011-web-iconography/evidence/performance.md`; documentar incumplimientos heredados/impacto sin ampliar umbrales o retries.
- [x] T028 Ejecutar suites Web afectadas/completas, lint/typecheck/build y E2E smoke en tres motores; guardar comandos/SHA/resultados en `specs/011-web-iconography/evidence/validation.md`, sin tratar mocks como aceptación real.
- [x] T029 Construir Docker Web con configuración pública de compilación aplicable y verificar que `MiKompri.Web/Dockerfile` no incluya artifacts/tests/env privados; sin cambios de BD/cloud/backend por la capa visual.
- [x] T030 Reconciliar `specs/011-web-iconography/checklists/acceptance.md`, `tasks.md` y README según evidencia; no cerrar007/008/009 ni diferidos010 por presencia de iconos.
- [x] T031 Verificar CI Web/checks aplicables del SHA final y diff sin APIs/modeloEF/auth/env no autorizados, documentar en `specs/011-web-iconography/evidence/validation.md`.
- [x] T032 Revisar DoD/ADR/diseño final con `specs/011-web-iconography/spec.md` y preparar PR si procede, sin merge/deploy automático ni implementación de012/010-B.

## Dependencies & Execution Order

T001→baseline/CI→paquete→tests foundation→wrappers/tokens→US1→US2→US3→US4/US5→validación. Las pruebas de cada corte preceden cambios; no agentes ni ejecución paralela implícita. US1 aporta navegación por sí misma, US2 mejora el listado, US3 la compra; US4 aporta ayudas/acciones y US5 coherencia transversal.

## Definition of Done

Spec/diseño revisados y todas las aceptaciones aplicables con evidencia del candidato implementado; fuente/familia/licencia/imports correctos; accesibilidad/estados/responsive/rendimiento/CI verificados. T019/T025 siguen abiertas como DEFERRED-NON-BLOCKING-QA por autorización expresa, sin bloquear PR ni implicar ejecución; otras tareas abiertas bloquearían su aceptación aplicable; no se cierran por haberse aplicado el diseño. No marcar un requisito por existir un test o una captura estática.

## Evidencia del primer corte cf83bff (histórica)

T001: instrucción explícita de siguiente fase/ADR005. T002: tres capturas previas, build/gzip y80muestras previas. T003: diff del trigger Web. T004–009: auditoría instalada,31exports renderizados, tests red/green de foundation y tokens. T010–018/T020–023: regresiones ejecutadas de navegación/queries/compra/historial/errores/100ítems y capturas del build final, no presencia de archivos. T026: nueve capturas reales inspeccionadas en los tres motores. T028:150/150 Vitest y144/144 smoke en7.7min, más lint/typecheck/build. T029: Docker final y nginx/runtime sin datos QA/env, aviso de licencia incluido. T030: estado documental reconciliado sin tocar tareas anteriores. Referencias detalladas en evidencia.

T024:50 regresiones auth PASS y recorrido manual Google→logout→Microsoft confirmado por el titular el31/10/2026: «Ambos funcionan», en respuesta a la comprobación de llegada a Mis listas y apertura de Perfil. Evidencia reportada por el usuario, sin captura de tokens ni credenciales; no implica auditoría manual adicional de caché/drafts.

Estado vigente: [revisión final](evidence/final-review.md), [CI código final](evidence/ci-source-final.json), [métricas](evidence/performance-comparison.md). T019/T025: DEFERRED-NON-BLOCKING-QA (lector/zoom/dispositivo), casillas sin cerrar. LCP global sigue FAIL3rutas/deuda previa, no bloqueante solo011 por autorización, sin cambiar budgets. No se cierran007/008/009/010. La publicación se contrasta por SHA/run antes de PR, además del CI del código0923a0c ya acreditado.

## Cierre autorizado desde HEAD remoto cf83bff

- [x] T033 Refinar listado/creación/detalle de Grupos con myRole/memberCount ya efectivos; tests vacío/uno/diez/texto largo/errores/creación/foco/responsive sin lifecycle ni backend.
- [x] T034 Diagnosticar LCP de las cuatro rutas antes de optimizar, con elemento/waterfall/recursos/render y atribución011 documentados.
- [x] T035 Reducir porción atribuible a011 y oportunidades seguras de carga crítica; medir80muestras mismo protocolo, imports/gzip y clasificación A/B/C sin manipular budgets.
- [x] T036 Verificar candidato final: navegación/icon-actions/teclado/zoom/toque/QA diferida explícita, suites completas/tres motores/Docker/CI exacto; reconciliar gates y preparar PR solo si READY.

- [x] T037 Refinar catálogo con creación/edición producto/mercado en diálogo, tabs locales y filtros de historial contextual, sin cambiar contratos; tests de conducta antes de implementar.
- [x] T038 Revisar visualmente todas las secciones existentes y estados en tres motores; documentar coherencia, correcciones y límites.

## Evidencia de cierre actual

Corrección posterior autorizada,01/11/2026 ([ADR-006](../../docs/adr/ADR-006-temporary-google-only-web-login.md)):

- [x] T039 Diagnosticar acceso sin cambiar trust: cuatro contenedores QA detenidos; recuperados y tres health200. Entorno privado compatible conservado, sin migraciones ni nuevos recursos.
- [x] T040 Ocultar Microsoft de la entrada y probar Google/configuración ausente; detener reintentos automáticos de preparación y ofrecer reintento explícito con sesión existente. Pruebas rojas1/5 y verdes5/5, más162/162Web y21/21E2E ejecutados; no se acredita por presencia de tests.
- [x] T041 Comprobar Google real→Mis listas→Perfil→recarga→logout→ruta protegida→nuevo Google con retorno; comprobar además503 inyectado→reintento→Users200 con sesión real. Documentar excepción UI legacy, límites y recuperación en README/ADR/evidencia. Sin cierre de tareas previas ni publicación implícita.

Resultados y límites de este diff en [recuperación de acceso](evidence/authentication-recovery-2026-11-01.md). T024 conserva el resultado histórico de ambos proveedores; el nuevo acceso visible solo Google sustituye su política de presentación, sin afirmar un nuevo ensayo Microsoft ni cutover. CI citada a continuación corresponde al candidato anterior.

T031: run41327776290/0923a0c y diff; SHA de publicación contrastado después del commit en PR/entrega. T032: gates/DoD/reversión y PR preparado tras CI. T033: Groups red/green, Owner/Admin/Member, datos reales/creación/foco/22capturas. T034: cuatro elementos LCP/waterfalls/JS/fuentes/render, sin cambiar runner. T035:22exports, ambos Drawer diferidos, compresión inicial y80muestras;3FAIL LCP/deuda previa explícita. T036:160local/192CI, Docker/HTTP y QA humana diferida. T037:13tests del área y15E2E nuevos, error/retry/pending/draft/returnTo conservados y CI completo. T038:17vistas/68capturas, inspección por secciones/motores/escritorio y correcciones de notas compartidas/Gastos/copy sin negocio.

Publicación: el CI de fc3e2ca tuvo191/192 por foco temprano del test de compra WebKit. [Revisión](evidence/publication-review.md): prueba reforzada sin cambio productivo,3/3 dirigido y15/15 de estabilidad. T031/T032 exigen comprobar el siguiente SHA completo antes de PR; el resultado se publica en PR/entrega. T019/T025 no cambian.
