# Tasks: Modernización integral web P0–P3

**Input**: [spec](spec.md), [plan](plan.md), [research](research.md), [modelo](data-model.md), [contratos](contracts/api-and-permissions.md), [diseño](design.md), [trazabilidad](traceability.md).
**Estado**: Implementación local y aceptación parcial. Casillas cerradas acreditan la unidad descrita con evidencia; las abiertas incluyen trabajo parcial o verificación pendiente. Ver [validación](evidence/validation.md), [rendimiento](evidence/performance.md) y [piloto](evidence/pilot.md). La feature no está cerrada.
**Tests**: requeridos por FR-025/TP8. Tests de dominio/caso de uso/API antes de cambios de regla, fallando por el comportamiento esperado; no pruebas que solo repliquen estilo o implementación.
**Formato**: Txxx identifica esta feature; usar `008/Txxx` al referenciar desde 007. [P] indica archivos diferentes y ejecución independiente solo después de dependencias.

## Phase 1: Setup — P0 trazabilidad y baseline

- [x] T001 Capturar revisión local, estado de suites/entorno y baseline de carga en `specs/008-web-ux-modernization/quickstart.md`, sin sobrescribir cambios previos ni convertir evidencia histórica en nueva.
- [x] T002 Reconciliar `specs/008-web-ux-modernization/traceability.md` con `specs/007-web-react-client/tasks.md`, asignando cada deuda y ampliación a evidencia/cierre individual, sin duplicar implementación.
- [x] T003 Revisar Node/Vite/lockfile, scripts/navegadores/Docker/tenant y disponibilidad de fixtures en `MiKompri.Web/package.json`, `MiKompri.Web/playwright.config.ts` y `.github/workflows/ci-mikompri-web.yml`; registrar decisiones y prerrequisitos en `specs/008-web-ux-modernization/research.md` sin actualizaciones masivas.

## Phase 2: Foundational — P0 contratos e infraestructura de prueba

- [x] T004 Auditar rutas/DTO/respuestas/scopes/roles, concretar permiso Member sobre gasto propio y compatibilidad de estimación desconocida, en `specs/008-web-ux-modernization/contracts/api-and-permissions.md`; contrastar `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs` y contratos Users/ProductCatalog.
- [x] T005 Preparar dos propietarios, Owner/Admin/Member, no miembro y miembro retirado con recursos aislados en `test/MiKompri.ShoppingList.Api.Tests/` y `MiKompri.Web/tests/e2e/fixtures/`; limpieza solo de fixtures creadas por la prueba, sin datos del usuario.
- [x] T006 Definir escenarios obligatorios y separación UI controlada/integración en `MiKompri.Web/playwright.config.ts` y `specs/008-web-ux-modernization/checklists/acceptance.md`; la aceptación Entra se inicia explícitamente y falla por ausencia de prerrequisitos, sin reemplazar Entra por harness ni cerrarla con la CI de push.

**Checkpoint**: T001–T006 proporcionan entorno/contratos. US1 acredita permisos antes de nuevas escrituras. Prototipos sin escrituras y preparación de pruebas pueden avanzar aisladamente.

## Phase 3: US1 — Operaciones autorizadas (P0)

**Goal**: Autorización servidor para cada operación expuesta, con regresión ItemId/ProductId.
**Independent Test**: Matriz multiusuario/grupo sin UI como frontera.

- [x] T007 [US1] Añadir casos negativos y positivos personales, compartidos, scopes y recursos cruzados en `test/MiKompri.ShoppingList.Api.Tests/` y tests de handlers en `test/MiKompri.ShoppingList.Application.Tests/`; incluir actor/ítem de otra lista y miembro retirado.
- [x] T008 [US1] Endurecer lectura/escrituras genéricas en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs` y `MiKompri.ShoppingList.Application/Commands/`, aplicando identidad/propiedad/membresía/rol mediante contratos Users; impedir cambio de groupId/owner por payload.
- [x] T009 [US1] Cubrir y corregir acceso a detalle/listado en `MiKompri.ShoppingList.Application/Queries/`, autoría y permisos de gasto en `MiKompri.ShoppingList.Application/Commands/SharedLists/`, sin acceso a DB de Users.
- [x] T010 [US1] Registrar regresiones dominio/aplicación/API, OpenAPI y pruebas de matriz en `specs/008-web-ux-modernization/quickstart.md`; enlazar evidencia a 007/T132 y conservar 007/T133 como corrección distinta.

**Checkpoint**: SC-001 y FR-002/003/027 acreditados para operaciones existentes; nuevos endpoints heredan pruebas de autorización antes de activación. Sin este gate no se habilita marcado compartido.

## Phase 4: US2 — Compra personal completa (P1)

**Goal**: CRUD y compra reversible real sin bloquear otras filas.
**Independent Test**: Una de dos listas, recarga y estado conservado.

- [ ] T011 [US2] Añadir tests de renombrado/borrado/edición y fallos 400/401/403/404/409/503 en `MiKompri.Web/tests/integration/features/lists-and-groups.integration.spec.tsx` y `MiKompri.Web/tests/component/features/shopping-lists/`; verificar lista/ítem correctos y cancelación.
- [ ] T012 [US2] Implementar clientes y controles CRUD en `MiKompri.Web/src/features/shopping-lists/api/shoppingListsApi.ts`, `pages/PersonalListsPage.tsx` y `components/`, con RHF/Zod, confirmación y foco tras borrado.
- [x] T013 [US2] Escribir pruebas de asignación compra/descompra, repetición idempotente, autorización y gastos intactos en `test/MiKompri.ShoppingList.Domain.Tests/`, `test/MiKompri.ShoppingList.Application.Tests/` y `test/MiKompri.ShoppingList.Api.Tests/`.
- [x] T014 [US2] Implementar contrato propuesto purchase-state en `MiKompri.ShoppingList.Domain/Entities/PurchaseList.cs`, `MiKompri.ShoppingList.Application/Commands/` y `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`; mantener POST legado endurecido y publicar OpenAPI.
- [x] T015 [US2] Conectar marcado/desmarcado y pending por itemId en `MiKompri.Web/src/features/shopping-lists/components/PurchaseItems.tsx`, `pages/PersonalListsPage.tsx` y API; conservar fila/foco, reconciliar resultado incierto y no repetir escritura por refetch fallido.
- [ ] T016 [US2] Completar `MiKompri.Web/tests/e2e/lists/personal-lists.spec.ts` con CRUD real, desmarcado, recarga y segunda lista intacta; registrar SC-002 en `specs/008-web-ux-modernization/quickstart.md`.

## Phase 5: US3 — Grupos y compra compartida (P1)

**Goal**: Miembros/roles, listas vinculadas y operaciones según permisos.
**Independent Test**: Grupo preparado con cuatro perfiles; lectura, escritura permitida/rechazada e historia.

- [x] T017 [US3] Cubrir contratos/nombres/roles, cierre/borrado y edición de producto propio/ajeno en `test/MiKompri.ShoppingList.Api.Tests/` y `MiKompri.Web/tests/component/features/shared-lists/`, incluyendo retirados y fallback histórico.
- [x] T018 [US3] Implementar endpoints compartidos propuestos y autoría según contrato T004 en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`, `MiKompri.ShoppingList.Application/Commands/SharedLists/`, `Queries/SharedLists/` y DTOs; bloquear borrado incompatible con gastos, conservar auditoría.
- [x] T019 [US3] Incorporar detalle de grupo y miembros en `MiKompri.Web/src/features/groups/pages/GroupDetailPage.tsx`, `pages/GroupsPage.tsx`, `api/groupsApi.ts` y `MiKompri.Web/src/app/router/routes.tsx`; nombres/roles y enlaces a listas, sin invitaciones ni GUID manual.
- [x] T020 [US3] Conectar edición/cierre/borrado compartido y purchase-state en `MiKompri.Web/src/features/shared-lists/api/sharedListsApi.ts` y `pages/SharedListDetailPage.tsx`, con autoría, confirmación y estados por rol; no activar sin T010/T014/T018.
- [x] T021 [US3] Exponer/proyectar estimación desconocida y autoría compatible en `MiKompri.ShoppingList.Application/DTOs/`, `MiKompri.ShoppingList.Api/Models/`, `MiKompri.ShoppingList.Infrastructure/` y clientes `MiKompri.Web/src/features/{shopping-lists,shared-lists}/api/`; contemplar alta/edición personal y compartida, migración solo si T004 demuestra necesaria, con tests de ceros legados.
- [ ] T022 [US3] Completar `MiKompri.Web/tests/e2e/lists/shared-lists-authorization.spec.ts` con dos cuentas, roles, baja de membresía y persistencia; actualizar evidencia FR-008/009 en `specs/008-web-ux-modernization/quickstart.md`.

## Phase 6: US4 — Crear y añadir en contexto (P1)

**Goal**: Buscar/crear/incorporar sin navegación a catálogo ni duplicados por reintento.
**Independent Test**: Probar ambos tipos de lista y fallo en cada paso.

- [ ] T023 [US4] Añadir pruebas de búsqueda, duplicados, creación correcta con incorporación fallida y timeout incierto en `MiKompri.Web/tests/component/features/shopping-lists/` y `tests/integration/features/product-catalog.integration.spec.tsx`.
- [x] T024 [US4] Crear `MiKompri.Web/src/features/shopping-lists/components/ProductSearch.tsx` y `InlineProductCreation.tsx` reutilizando alta/catalogApi y validación nombre/unidad; autocompletado accesible, sin dependencia nueva obligatoria.
- [x] T025 [US4] Integrar saga de dos pasos en `MiKompri.Web/src/features/shopping-lists/components/AddProductsDialog.tsx` y `ListItemsPanel.tsx`, conservando productId/fase, cuenta/lista, cantidad y estimación desconocida conforme T021; no rollback borrando catálogo.
- [ ] T026 [US4] Añadir E2E de crear/incorporar/reintentar en `MiKompri.Web/tests/e2e/lists/inline-product.spec.ts`, con lista personal/compartida y ninguna creación duplicada; registrar éxito parcial y recuperación en `specs/008-web-ux-modernization/quickstart.md`.

## Phase 7: US5 — Acceso y recuperación (P1)

**Goal**: Destino seguro, borradores aislados y estados accionables.
**Independent Test**: Callback, cuenta distinta, expiración, errores y recarga con contexto.

- [x] T027 [US5] Escribir casos de open redirect, rutas/params permitidos, TTL, cambio de cuenta/logout y no replay en `MiKompri.Web/tests/unit/auth/` y `tests/integration/auth/`; incluir fallos de token silencioso anteriores a la escritura.
- [x] T028 [US5] Implementar intención y draft en `MiKompri.Web/src/auth/session/returnIntent.ts`, `draftStore.ts`, `authSessionService.ts`, `guards/ProtectedRoute.tsx` y `features/profile/pages/LoginPage.tsx`; sessionStorage/24 h y restauración revisable por misma cuenta.
- [x] T029 [US5] Integrar borradores y recuperación en `MiKompri.Web/src/features/shopping-lists/components/`, `features/settlements/components/ExpenseForm.tsx` y `features/profile/pages/ProfilePage.tsx`; añadir refetch y sync explícitos mediante `features/profile/api/profileApi.ts`, con errores sin borrar datos.
- [ ] T030 [US5] Unificar mensajes/códigos y recuperación en `MiKompri.Web/src/shared/ui/states/` y clientes API de features; matriz por vista 400/401/403/404/409/503/red, sin cuerpos técnicos ni estados de éxito falsos.
- [ ] T031 [US5] Completar `MiKompri.Web/tests/e2e/auth/auth-session.spec.ts` y `tests/e2e/recovery/session-errors.spec.ts` con Entra real, destino/CTA, logout, cambio cuenta, draft, Atrás/recarga y contexto autorizado.

## Phase 8: US6 — Gastos comprensibles y corregibles (P1)

**Goal**: Registro/edición/borrado por rol y balances/historia comprensibles.
**Independent Test**: Recursos preparados, distintos pagadores y reparto con céntimos/retirados.

- [x] T032 [US6] Cubrir autoría, lectura de gastos/expenseId, permisos de gasto propio/ajeno, bajas, redondeo, corrección/borrado y datos financieros intactos al desmarcar en `test/MiKompri.ShoppingList.Application.Tests/`, `test/MiKompri.ShoppingList.Api.Tests/` y tests Users afectados.
- [x] T033 [US6] Implementar lectura de gastos/expenseId y nombres históricos según propuesta en `MiKompri.ShoppingList.Application/Queries/SharedLists/`, DTOs y `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`; si Users requiere extensión histórica acotada, añadirla en `MiKompri.Users.Api/Controllers/GroupsController.cs` y `MiKompri.Users.Application/Queries/`, sin DB cruzada ni directorio público.
- [x] T034 [US6] Extender clientes y RHF/Zod en `MiKompri.Web/src/features/settlements/api/expensesApi.ts` y `components/ExpenseForm.tsx`; edición/eliminación, pagador permitido y confirmación explícita de participantes, borrador intacto tras error.
- [x] T035 [US6] Presentar estimación/pagado/balance y «Debes/Te deben» en `MiKompri.Web/src/features/settlements/pages/SettlementPage.tsx`, con historial, corrección contextual y propuestas sin pagos; consultar/refrescar backend sin recalcular finanzas.
- [ ] T036 [US6] Completar `MiKompri.Web/tests/e2e/settlements/settlement-flow.spec.ts` y `settlement-authorization.spec.ts`, con dos cuentas, corrección/borrado, antiguos miembros y reparto esperado; registrar evidencia en `specs/008-web-ux-modernization/quickstart.md`.

## Phase 9: US7 — Modernización visual y navegación (P2)

**Goal**: Mis listas, continuidad con portada y compra móvil directa sin pérdida de accesibilidad.
**Independent Test**: Fixtures representativas, todas las rutas/estados y navegación teclado/URL.

- [x] T037 [US7] Preparar prototipo navegable y revisión crítica en `design/prototypes/mikompri-modernization/` y `specs/008-web-ux-modernization/design.md`; móvil 390 primero, validar barra/Drawer, agrupación estable, jerarquía, vocabulario y estados antes de código productivo.
- [x] T038 [US7] Implementar `MiKompri.Web/src/features/shopping-lists/pages/ListsPage.tsx` y rutas en `src/app/router/routes.tsx`, con filtros URL, consultas limitadas/cacheadas y fallos parciales visibles; conservar entradas antiguas personal/compartida.
- [x] T039 [US7] Adaptar `MiKompri.Web/src/app/App.tsx`, `layout/MainLayout.tsx`, `layout/MainLayout.module.css` y `shared/ui/AccountMenu.tsx` a Mis listas/grupos/productos/cuenta; implementar variante móvil acreditada por T037, safe areas/teclado y destino por intención de T028.
- [x] T040 [US7] Extraer `MiKompri.Web/src/shared/ui/ShoppingListView.tsx` y adaptar PurchaseItems/HomePage a lista común con capacidades; datos demo ficticios aislados, sin acciones aún no entregadas ni APIs reales.
- [ ] T041 [US7] Consolidar tokens/espaciado/tipografía/estados en `MiKompri.Web/src/shared/ui/theme.ts`, `AppSurface.module.css` y páginas de features; contraste medido, objetivos 44 px, foco, nombre/cantidad/importe legibles y grupos estables.
- [x] T042 [US7] Añadir Personas y acciones contextuales en `MiKompri.Web/src/features/shared-lists/pages/SharedListDetailPage.tsx` con tab/grupo en URL; mantener retorno desde catálogo y gastos y pruebas de enlaces/Atrás/recarga.
- [ ] T043 [US7] Medir y optimizar rutas/bundle/fuentes en `MiKompri.Web/src/app/router/`, `src/index.css`, `src/assets/fonts/` y `vite.config.ts`; WOFF2/licencias, lazy loading y presupuesto SC-008 antes/después; sin dependencia nueva si no mejora resultados.
- [x] T044 [US7] Verificar capturas/teclado/URL/estados en `MiKompri.Web/tests/e2e/visual/`, `tests/e2e/home/` y `tests/e2e/responsive/`, sin convertir harness en evidencia de permisos/Entra; actualizar `specs/008-web-ux-modernization/design.md` con decisiones finales.

## Phase 10: US8 — Calidad integrada y resultados (P3)

**Goal**: Evidencia humana y técnica completa, sin skips obligatorios.
**Independent Test**: Guía reproducible con entorno aislado y piloto representativo.

- [x] T045 [US8] Evaluar versión/compatibilidad y añadir `@axe-core/playwright` en `MiKompri.Web/package.json`/lockfile, documentando decisión en `specs/008-web-ux-modernization/research.md`; Storybook/iconos solo si la evaluación demuestra necesidad, no requisito de cierre.
- [ ] T046 [US8] Ejecutar axe y revisión manual de foco, lector, zoom 200%, contraste 4.5:1 texto/3:1 grandes y controles, movimiento reducido, teclado virtual y safe areas en `MiKompri.Web/tests/e2e/accessibility/`; matriz completa en `specs/008-web-ux-modernization/checklists/acceptance.md`.
- [x] T047 [US8] Corregir runtime, navegadores, selección smoke obligatorio y publicación de informes/trazas en `.github/workflows/ci-mikompri-web.yml` y `MiKompri.Web/playwright.config.ts`; CI de push separada del workflow manual `acceptance-mikompri-web-entra.yml`, cero skips como evidencia de aceptación y Docker build, sin despliegue.
- [ ] T048 [US8] Ejecutar suites frontend, .NET afectadas y recorrido integrado de dos personas en `MiKompri.Web/tests/e2e/` con Docker/identidades de prueba; registrar comandos/revisión/entorno/resultados/límites en `specs/008-web-ux-modernization/quickstart.md`, sin secretos.
- [x] T049 [US8] Medir SC-008 con perfil fijo y reportar 20 muestras por ruta/acción en `specs/008-web-ux-modernization/evidence/performance.md`; comparar baseline, rutas frías, interacción y fallos, sin presentarlo como p75 de campo.
- [ ] T050 [US8] Ejecutar piloto de al menos 10 personas, tareas/tiempos/ayudas y cuestionario de reparto en `specs/008-web-ux-modernization/evidence/pilot.md`; medir SC-003/004/005/007 y criterios heredados 007, con datos anonimizados y sin grabación no consentida.
- [x] T051 [US8] Corregir defectos hallados en features y tests correspondientes de `MiKompri.Web/` y backend afectado; registrar incidencia, arreglo y repetición pertinente en `specs/008-web-ux-modernization/checklists/acceptance.md`, sin relajar aserciones ni umbrales para aprobar.

## Phase 11: Polish y cierre transversal

- [x] T052 Reconciliar evidencia y casillas de `specs/007-web-react-client/{spec,plan,tasks,design,quickstart}.md` con `specs/008-web-ux-modernization/traceability.md`, enlazando deuda individual y cambios prospectivos, sin borrar límites históricos.
- [x] T053 Actualizar `README.md`, `MiKompri.Web/README.md` y `specs/008-web-ux-modernization/{contracts/api-and-permissions,design,quickstart}.md` a capacidades realmente entregadas, sin describir propuestas como contratos disponibles.
- [ ] T054 Revisar cumplimiento FR/SC y constitución, enlaces, formato y evidencia en `specs/008-web-ux-modernization/checklists/requirements.md` y `acceptance.md`; cerrar solo con todos los gates satisfechos, sin despliegue ni aprobación implícita por documentación.

## Dependencies & Execution Order

- T001 → T002/T003 → T004 → T005/T006 → US1 T007–T010; tests antes de cambios de regla.
- US2 depende de T010; T013 → T014 → T015 → T016. US3 puede probarse con recursos preparados, pero compra reversible requiere T014. T017 → T018 → T020; T021 requiere decisión T004.
- US4 depende de contratos T004 y estimación T021; T023 → T024/T025 → T026. Funciona con recursos de listas preparados, sin exigir navegación nueva.
- US5 depende de fundamentos y permisos; T027 → T028 → T029/T030 → T031. Puede verificarse con formulario existente; no exige US4 completa.
- US6 usa lista/miembros preparados y T004/T010; T032 tests → T033 lectura → T034/T035 → T036. No requiere nueva navegación.
- US7 prototipo T037 puede comenzar tras fundamentos sin escribir en APIs; implementación T038–T044 integra US2–US6. T040 solo activa capacidades ya acreditadas.
- US8 tests/entorno se preparan desde P0; aceptación final T048–T051 requiere historias afectadas. Cierre T052–T054 después de evidencia, no al terminar estilos.

## Parallel Opportunities

Oportunidades de organización futura, no instrucciones para lanzar agentes. Sin [P] en tareas que comparten archivos. Tras T010, tests US2 y pruebas auth US5 pueden prepararse en archivos distintos; US6 backend puede investigarse mientras se prototipa US7. Sin escrituras concurrentes en contratos/traceability/quickstart.

Ejemplos por historia: US1 tests personales/compartidos en archivos separados; US2 dominio purchase-state y UI CRUD; US3 fixtures de roles y GroupDetail; US4 tests de saga y prototipo de búsqueda; US5 tests de returnIntent y matriz de errores; US6 lectura backend e investigación de textos; US7 fuentes/licencia y prototipo; US8 evaluación lector y preparación de piloto. Todas respetan dependencias y evitan conflictos de archivo.

## Implementation Strategy / Definition of Done

Primer incremento: fundamentos + US1, utilizable como endurecimiento del producto actual. Segundo: US2 completa, después US3–US6 por historia, cada una probada con recursos preparados. Tercer incremento: US7 moderniza capacidades ya reales. P3 acredita aceptación con integración y personas.

Una tarea se marca solo con aceptación y evidencia, no por existencia de un archivo. Una historia requiere éxito/fallo, autorización aplicable, persistencia y foco/contexto. Una feature requiere SC-001–009, cero omisiones obligatorias, gates constitucionales y documentación consistente. Un entorno no disponible deja el gate pendiente. Deploy, publicación, mensajes a terceros e instalación de servicios externos no forman parte de este plan documental.


## Estado de ejecución y deuda de aceptación — 2026-10-28

Las tareas cerradas de permisos/persistencia remiten a 40 pruebas de dominio, 33 de aplicación y 68 de API (también PostgreSQL); las de cliente a las 107 pruebas locales, build/lint y escenarios controlados. OpenAPI local enumera los endpoints nuevos. Ver comandos y límites en evidencia. T005 prepara identidades controladas; no exporta ni inventa cuentas Entra.

| Tareas abiertas | Trabajo realizado | Falta para cerrar la unidad completa |
| --- | --- | --- |
| T001/T049 (cerradas) | Baseline y dos series actuales de 20 muestras por ruta publicadas | SC-008 no cumple; T043/A022 permanecen abiertos |
| T011/T012 | CRUD personal, validación, confirmación y recuperación implementados | Ampliar matriz de fallos por operación; formularios de edición usan validación explícita y drafts, sin migración completa a RHF |
| T016/T022 | Escenarios reales escritos y separados | Ejecutarlos con dos sesiones Entra reales; no se cuenta el harness |
| T023/T026 | Alta contextual y reintento parcial probados en componente; E2E personal en cinco tamaños | Completar toda la matriz de duplicado/timeout de creación y E2E compartida/recuperación real |
| T030 | Normalización HTTP y estados de recuperación implementados | Completar matriz de fallos por cada vista/operación |
| T031/T036 | Pruebas reales de auth/gastos reemplazan placeholders y preflight exige recursos | Acceso inicial, cambio de cuenta, restauración/recuperación completa, antiguos miembros y flujo real con dos cuentas |
| T041/T046 | Tokens, controles de 44 px, axe, tres motores y foco/teclado automatizado | Lector, zoom 200 %, teclado virtual/safe areas físicos y revisión ampliada manual |
| T043 | Rutas lazy, WOFF2, bundle medido | Acreditar presupuesto de carga/interacción, además de reducción de tamaño |
| T044/T051 (cerradas) | 72 recorridos y 15 casos de estados pasan en tres motores; gastos repetidos tras el último ajuste | Evidencia de dos selecciones complementarias, sin presentar los intentos fallidos como aprobados |
| T048 | Suites locales y PostgreSQL aislado pasan | Dos personas/servicios con Entra real; el build completo Docker local ya pasa con SDK 8.0-noble |
| T050 | Protocolo preparado | Piloto humano de diez personas; 0 participantes a la fecha |
| T054 | Revisión documental y alcance conservados | Todos los gates de aceptación; no se cierra por documentación |

T052 conserva las casillas históricas de 007 salvo T132, cerrada con evidencia específica del hardening. T053 describe capacidades locales reales y limitaciones de despliegue/aceptación. T047 cuenta con ejecución remota satisfactoria del job controlado; el job Entra de aquella ejecución falló por recursos ausentes y no acredita aceptación.

Revisión de programación CI: Entra se mueve a un workflow manual independiente hasta disponer de QA y sesiones. Su ejecución real satisfactoria sigue siendo obligatoria para T031/T036/T048 y el cierre de la feature. La CI normal en verde no cierra esos gates. El preflight mantiene salida no cero ante configuración incompleta; seis pruebas de configuración cubren ausencia, ficheros, variables de APIs y recursos completos sin autenticar usuarios.

T027 se completa con tests de destino permitido, consumo único/StrictMode, caducidad de 24 h, aislamiento de cuentas, limpieza al salir, token silencioso que requiere interacción y 401 sin repetición automática. T044/T051: 72 recorridos (24 por motor) satisfactorios y 15 casos de carga/vacío/errores (cinco por motor) tras separar el test agregado; los tres recorridos de gastos se repitieron tras el último ajuste de formulario. Total: 87 escenarios distintos cubiertos por ejecuciones complementarias, cero omitidos en esas selecciones.
