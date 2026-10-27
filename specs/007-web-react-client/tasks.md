# Tasks: 007-web-react-client

**Input**: Design documents from `/specs/007-web-react-client/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/frontend-backend-integration.md, quickstart.md

**Tests**: Obligatorios por TP8 y por requerimiento explícito de la feature. Cada historia incluye tareas de pruebas unitarias, de componente, integración con MSW y E2E con Playwright.

**Organization**: Tareas agrupadas por historia de usuario para permitir implementación y validación independiente.

## Estado vigente de diseño — revisión documental 2026-10-27

Referencia normativa: [design.md](design.md). Los apartados fechados anteriores son evidencia histórica, no diagnósticos vigentes. La entrega visual se registra en T120–T126; T086/T106 quedan reconciliadas con esa evidencia. Esta revisión documental no ejecuta otra vez las pruebas ni cambia su fecha.

| Tareas | Estado vigente |
| --- | --- |
| T086/T106 y T120–T126 | Completadas en su alcance de layout, portada y coherencia visual; no equivalen al cierre de US1–US5 |
| T088/T108 | Tema, fuente local y foco entregados; pendiente medir contraste completo de combinaciones/estados |
| T107 | Rutas y contexto completados en T129/T130, incluido detalle compartido por URL; validación integrada de recarga/tenant sigue en T117 |
| T109/T110 | Formato común, filas y progreso personal entregados; pendientes composición índice/detalle y acciones/detalle compartido previstos |
| T113/T114 | Selector de grupo y participantes por nombre entregados; membresías completas, RHF y flujo real multiusuario siguen pendientes |
| T115/T116 | Estados y teclado básico comprobados en harness; falta recuperación completa, zoom, teclado virtual, lector de pantalla, contraste y fronteras |
| T117/T118 | Hay smoke local y evidencia visual; CI, siete E2E integrados, regresiones pertinentes y cierre global siguen pendientes |

Los datos ficticios explícitos de portada son la excepción aprobada por FR-032; la prohibición histórica de fixtures en producción se refiere a datos de negocio reales y a bypasses de sesión. No se mezclan con APIs ni identidades del usuario.

## Format: `[ID] [P?] [Story] Description`

## Reparto de gastos con personas por nombre — 2026-10-25

- [X] Alinear con FR-012 y el plan: sustituir «Settlement» visible por **Reparto de gastos**, **Balance por persona** y pagos para saldar cuentas. Pagador/comprador por selector y participantes por casillas, con nombres de miembros de Users; no pedir UserIds ni mostrar UUIDs en balances/transferencias.
- [X] Eliminar IDs de lista/ítem de ejemplo; cargar detalle autorizado, grupo, miembros y perfil del usuario. Elegir ítem real antes de registrar; sin ítems o membresía disponible no hay formulario operativo. Member solo puede escoger su propio pago; Owner/Admin pueden elegir pagador del grupo. Cambiar lista remonta el formulario.
- [X] Corregir el contrato de registro a `paidBy`, `purchasedBy`, `realPaidPrice`, `currency`, `participants`; retirar nota no soportada. Corregir balance a `totalPaid/totalOwed/netBalance`. Validar, impedir envíos duplicados, conservar datos ante error y refrescar detalle/balance/propuesta tras éxito. No recalcular reglas financieras en frontend.
- [X] `npx.cmd vitest run tests/component/features/settlements tests/component/features/shared-lists tests/integration/features/settlements.integration.spec.tsx tests/integration/auth/token-expiration-write.integration.spec.tsx`: **22 passed, 0 failed, 0 skipped**, con nueve casos nuevos en `ExpenseMembers.spec.tsx`. Typecheck/lint/build satisfactorios; continúa la advertencia de bundle >500 kB.
- [X] MCP Playwright con APIs reales: lista/ítem dedicados de prueba, miembros con nombres, registro **201** de 12,50 EUR con un participante, balance actualizado, sin UUIDs ni «Settlement» visibles. Sin desbordamiento a 375/430/1280 px; capturas verificadas. Consola sin errores nuevos, advertencia conocida de React Router. Lista con su ítem/gasto de prueba eliminada (204), ausencia comprobada; producto existente solo leído, no modificado.

Evidencia local `.playwright-mcp/expense-members-{375,430,1280}.png`, `expense-members-console.txt` y `expense-members-network.txt`; contrato sección 11 y FR-012 actualizados. Pendientes: prueba real con varios usuarios (los nombres/transferencias de dos usuarios sí se prueban con respuestas controladas), alta de ítems compartidos desde UI, RHF y cierre completo US3/T069/T071, resolución de nombres de antiguos miembros y soporte de otras divisas. No se marcan superados estos pendientes. Commit y push de esta corrección sin incorporar cambios previos de navegación.

## Grupos por nombre en listas compartidas — 2026-10-25

- [X] Contrastar specs: FR-009/FR-011 de la 007 y FR-001/FR-003 de la 005 vinculan listas a grupos de Users con membresía; `plan.md`, «Pantallas del núcleo», ya exige selector por nombre y prohíbe GUID escrito por la persona. El campo GroupId era una desviación de implementación, no una necesidad del usuario. FR-011 y contrato sección 10 explicitan el criterio.
- [X] Sustituir GroupId por selector de grupos propios desde Users, con carga/error/reintento y enlace Crear grupo si no hay membresías. Sin selección no se consultan listas ni se permite crear; grupo ajeno de URL no se envía a ShoppingList.
- [X] Conservar grupo en `?groupId=` para recarga/Atrás/Adelante; limpiar lista/settlement/borrador al cambiar de grupo o cuenta. Creación en diálogo con nombre de grupo, validación, bloqueo de duplicados, errores conservando nombre e invalidación de la consulta del grupo correcto. Un 403 oculta datos y deshabilita creación.
- [X] `npx.cmd vitest run tests/component/features/shared-lists tests/component/auth tests/integration/features/lists-and-groups.integration.spec.tsx`: **27 passed, 0 failed, 0 skipped**, incluidos nueve casos nuevos en `SharedGroupSelection.spec.tsx`. Typecheck, lint y build satisfactorios; permanece la advertencia de bundle >500 kB.
- [X] MCP Playwright + APIs reales: tres grupos mostrados por nombre, consultas **200** para los tres; selección conservada al recargar y usar historial; creación en grupo seleccionado **201** y lista visible sin recarga manual. Diálogo lleva foco al campo, Escape cierra y devuelve foco al disparador. Sin desbordamiento a 375/430/1280 px tanto con diálogo abierto como cerrado. Sin nuevos errores de consola; advertencia conocida de React Router.

Evidencia local `.playwright-mcp/shared-groups-{375,430,1280}.png`, `shared-groups-dialog-{375,430,1280}.png`, `shared-groups-console.txt` y `shared-groups-network.txt`. Lista de prueba eliminada (204) y ausencia comprobada; no se modificaron los grupos ni listas previos. No se cierra toda US2/US3 ni T107/T060: siguen pendientes detalle compartido/ítems, apertura del detalle tras crear y reconciliación completa de roles (el handler de creación actual comprueba `IsAuthorizedMember`, mientras la 005 diferencia Owner/Admin/Member). Esta corrección acotada al selector/contexto no certifica esos permisos. Commit y push limitados a la corrección y documentación.

## Acceso para crear productos — 2026-10-25

- [X] Confirmar la carencia: existía POST de ProductCatalog, pero el cliente web solo consultaba productos. La restricción inicial de solo lectura dejaba sin recorrido el alta de ítems con catálogo vacío. Ampliación solicitada registrada en `spec.md` y contrato, sección 9.
- [X] Añadir formulario visible **Catálogo → Crear producto**, nombre y unidad de compra, límites 120/30, campos obligatorios, trimming, bloqueo de duplicados pendientes, validaciones 400, conflicto 409, errores conservando valores y confirmación accesible.
- [X] Añadir **Crear producto en el catálogo** al detalle de la lista y **Volver a la lista** al catálogo, conservando el ID con `returnTo` restringido a rutas internas. Refrescar la query compartida para que el producto creado aparezca al volver en el selector de ítems. Crear producto no agrega automáticamente un ítem.
- [X] Verificar `npx.cmd vitest run tests/component/features/product-catalog tests/component/features/shopping-lists tests/integration/features/product-catalog.integration.spec.tsx`: **24 passed, 0 failed, 0 skipped**, incluidos siete casos nuevos de creación/retorno, campos obligatorios, errores, reintento y envíos duplicados. Typecheck, lint y build satisfactorios; persiste la advertencia de bundle >500 kB.
- [X] MCP Playwright con Vite/APIs reales: lista existente → enlace de creación → nombre/unidad → **201 Created** → regreso a la misma lista → nuevo producto seleccionable y botón Agregar ítem habilitado. Sin modificar los ítems de la lista existente. Sin desbordamiento a 375/430/1280 px. Consola sin errores nuevos; advertencia conocida de React Router.

Capturas locales `.playwright-mcp/catalog-create-{375,430,1280}.png` y registros `catalog-create-console.txt`/`catalog-create-network.txt`. Producto de verificación desactivado después (204), comprobada su ausencia del catálogo activo; el formulario permanece visible para que el usuario cree productos reales. Pendientes: administración de mercados/precios, edición/desactivación desde UI y restantes criterios globales de US4; no se cierran por este alta acotada. Commit y push de esta corrección sin incluir cambios previos de navegación.

## Selección de lista y alta de ítems — 2026-10-25

- [X] Diagnosticar el 400: el formulario escribía en `lists[0]` sin selección y enviaba `productId: null`. Reproducido contra la API real: error de conversión `$.productId` a `System.Guid`; la ausencia de selección era un defecto de UX independiente.
- [X] Incorporar `/personal-lists/:listId`, enlaces explícitos con selección visible, detalle con nombre/ítems y regreso al índice. Índice sin editor; ID no presente en las listas de la cuenta no habilita escrituras. Crear una lista abre su propio detalle. La recarga y el historial mantienen el ID.
- [X] Sustituir producto de texto libre por selección de producto activo del catálogo; `ListItemInput.productId` obligatorio. Mostrar carga, error con reintento y catálogo vacío; nunca generar GUIDs ni enviar null para eludir el contrato.
- [X] Validar cantidad entera positiva y precio no negativo, bloquear envíos pendientes/duplicados, preservar valores y mostrar validaciones HTTP ante errores, confirmar el alta y refrescar los ítems de la lista seleccionada. Cambiar lista reinicia el borrador.
- [X] Pruebas: `npx.cmd vitest run tests/component/features/shopping-lists tests/component/auth tests/component/accessibility`: **30 passed, 0 failed, 0 skipped**, incluidos 13 escenarios nuevos en `PersonalListSelection.spec.tsx`. Integraciones de listas/grupos, expiración durante escritura y estados HTTP: **5 passed, 0 failed, 0 skipped**. Typecheck y lint satisfactorios. `npm.cmd run build` satisfactorio tras repetir con permisos para `.vite-temp`; advertencia de bundle >500 kB pendiente de optimización.
- [X] MCP Playwright + Vite/APIs reales: crear lista y agregar producto con cantidad 2/precio 1.25 devuelve **201/201**; request incluye ID de catálogo real e ID de la lista abierta, y el ítem aparece en el detalle. Recarga, Atrás/Adelante, menú activo y ausencia de editor en índice comprobados. Sin desbordamiento a **375/430/1280 px**; teclado llega a Cantidad con foco visible. No hubo nuevos errores de consola ni peticiones fallidas durante el flujo válido; el 400 intencionado del diagnóstico se conserva como evidencia histórica.

Evidencia local en `.playwright-mcp/`: `items-selection-empty-catalog.png`, `items-selection-fixed-{375,430,1280}.png`, `items-selection-keyboard.png`, `items-selection-console.txt` y `items-selection-network.txt`. Para probar el alta se creó un producto y una lista identificados como verificación: lista eliminada (204), producto desactivado (204), comprobada su ausencia del índice y del catálogo activo. El catálogo activo vuelve a estar vacío: para usar el alta normalmente necesita productos reales cargados; la UI lo comunica y bloquea el envío. No se modificaron las listas previas del usuario. Quedan pendientes las restantes operaciones CRUD, auditoría completa de accesibilidad y el cierre global de T107/T059/T060/T061; esta corrección no las certifica. Commit/push limitado al flujo de ítems y su documentación.

## Corrección del 401 de listas personales — 2026-10-25

- [X] Confirmar y corregir la identidad JWT en ShoppingList: `MapInboundClaims = false` conserva los claims Entra `tid`/`oid` usados por provisioning y por el adaptador de Users. Las pruebas con el manejador Bearer real reproducían 401 antes de la corrección.
- [X] Implementar `GET /api/v1/purchaselists/mine` autenticado. Deriva el propietario del usuario resuelto en servidor y devuelve únicamente sus listas con `GroupId == null`, o `200 []` si no existen; ignora selectores ownerId/groupId del cliente.
- [X] Corregir la redirección HTTP → HTTPS en Development: el cambio de puerto eliminaba Authorization y causaba otro 401/CORS. Se mantiene `UseHttpsRedirection` fuera de Development.
- [X] Añadir cinco regresiones con JWT firmado y manejador Bearer real en `test/MiKompri.ShoppingList.Api.Tests/Security/PersonalListsBearerTests.cs`: aislamiento de propietario/listas compartidas, lista vacía, oid ausente, audiencia incorrecta y petición anónima. Simula puertos HTTP/HTTPS, no sigue redirecciones y comprueba CORS del origen Vite.
- [X] Ejecutar `dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release --no-restore --logger "console;verbosity=minimal" --logger "trx;LogFileName=personal-lists-fix.trx" --results-directory .playwright-mcp/test-results`: **64 passed, 0 failed, 0 skipped**. Primer intento Debug bloqueado por permisos al copiar DLL; Release final satisfactorio. Persisten advertencias previas de nulabilidad en build Docker.
- [X] Reconstruir y reiniciar únicamente ShoppingList con `docker compose up -d --build --no-deps mikomprishoppinglistapi`; verificar con MCP Playwright contra Vite y Entra reales: **200 []** a 375, 430 y 1280 px, estado «Sin listas personales todavía», cinco enlaces del menú y sin desbordamiento. En esta repetición: **0 errores de consola y 0 peticiones fallidas**; permanece la advertencia conocida de React Router. No fue necesario iniciar sesión de nuevo.

Evidencia local: `.playwright-mcp/personal-lists-fixed-{375,430,1280}.png`, `personal-lists-fixed-console.txt`, `personal-lists-fixed-network.txt` y `test-results/personal-lists-fix.trx` (rutas desde la raíz). Contrato actualizado en [frontend-backend-integration.md](contracts/frontend-backend-integration.md). Los registros anteriores de fallo se conservan como histórico; este apartado resuelve el 401 observado, sin cerrar T106/T086 ni la feature completa. Pendiente: comprobar en navegador estados con listas pobladas y mutaciones; el aislamiento de usuarios sí queda cubierto por integración automatizada. Commit limitado a esta corrección, sin incluir cambios previos de navegación/diseño y sin push.

## Verificación en navegador real con MCP Playwright — 2026-10-25

**Revisión posterior del 401 (2026-10-25)**: reproducido mediante Playwright. La petición envía Bearer y el JWT no está caducado; contiene audiencia configurada, scope `access_as_user` y claims `tid`/`oid` (no se guardó el token). Respuesta: `401`, JSON `Authentication failed.`, sin `WWW-Authenticate`. Ese cuerpo corresponde a `UserProvisioningMiddleware`, que rechaza identidad sin `tid`/`oid` literales o sin usuario resuelto. `Program.cs` no desactiva el mapeo de claims JWT: posible incompatibilidad entre claims mapeados y las búsquedas literales del middleware; requiere confirmar en runtime antes de atribuir la causa definitiva. También `GET /api/v1/purchaselists` devuelve el mismo 401 con el mismo token. El Swagger del servicio en ejecución y el controlador local NO exponen `/api/v1/purchaselists/mine`, aunque el frontend lo solicita: discrepancia de contrato independiente que debe corregirse tras resolver la identidad. No se ha cambiado la autenticación ni sustituido la ruta por un listado global. Diagnóstico pendiente de cierre; no se considera necesario repetir login por falta de token.

Entorno: Vite en `http://localhost:5173`, navegador controlado por MCP Playwright, sesión existente autenticada (no se introdujeron credenciales). Se realizaron clics reales, Tab/Enter, entrada directa, recarga HTTP e historial del navegador. No se modificó código de la aplicación ni se ejecutó commit/push.

| Comprobación | Resultado y alcance |
| --- | --- |
| Disponibilidad MCP y Vite | SUPERADO: navegación y capturas mediante MCP Playwright. |
| Menú persistente | SUPERADO: los cinco enlaces permanecen visibles al visitar Perfil, Grupos, Listas personales, Listas compartidas y Catálogo en los tres anchos. |
| Opción activa destacada | SUPERADO: exactamente un enlace con `aria-current="page"`, peso 700 y subrayado, correspondiente a cada ruta; comprobado en 15 combinaciones de ruta/ancho. |
| Entrada directa y recarga de rutas internas | SUPERADO para las cinco rutas: conserva URL, cinco enlaces y destino activo tras `goto` y `reload`. Esto acredita el enrutamiento, no el éxito de cada API. |
| Atrás/Adelante | SUPERADO: Catálogo → Grupos → Perfil; Atrás devuelve a Grupos y Adelante a Perfil con enlace activo correcto. |
| Desbordamiento horizontal | SUPERADO para los estados observados de las cinco páginas a 375, 430 y 1280 px (altura 900): `documentElement.scrollWidth` y `body.scrollWidth` coinciden con el ancho del viewport. Se repitió Listas personales tras terminar la carga y mostrar el error. No acredita listas/tablas con datos adicionales. |
| Teclado y foco del menú | SUPERADO: Tab recorre los cinco enlaces y Enter abre cada destino a 1280 px. `:focus-visible` verdadero con contorno `auto 1px`, confirmado visualmente en captura. También se comprobó el foco inicial con Tab a 375 y 430 px. No equivale a una auditoría de todos los formularios. |
| Ausencia de errores relevantes de consola/peticiones fallidas | NO SUPERADO: respuestas HTTP 401 repetidas de `GET http://localhost:8080/api/v1/purchaselists/mine`, con errores de recurso en consola y mensaje «No fue posible cargar listas personales». Existe además advertencia React Router sobre `v7_startTransition`. |

Evidencia guardada en `../../.playwright-mcp/` desde esta carpeta:

- `verify-{375,430,1280}-{profile,groups,personal-lists,shared-lists,product-catalog}.png`: 15 capturas de página completa.
- `verify-{375,430,1280}-personal-lists-settled.png`: error de listas personales tras finalizar la carga.
- `keyboard-focus-{0,1,2,3,4}.png`: foco de los cinco enlaces a 1280 px; `keyboard-focus-{375,430}.png`: foco en móvil.
- [Consola](../../.playwright-mcp/verification-console.txt) y [peticiones de red](../../.playwright-mcp/verification-network.txt).

Pendiente: resolver la respuesta 401 y repetir consola/red y Listas personales con datos cargados; verificar el layout con estados de datos no presentes en esta sesión. No se probaron mutaciones ni se alteraron datos. T106/T086/T081/T083/T088 permanecen pendientes: esta comprobación acotada no acredita todos sus criterios, menú móvil específico, skip link, contraste, formularios completos ni suites automatizadas. La evidencia nueva complementa la revisión histórica siguiente sin cerrar las tareas globales.

## Revisión de evidencia — 2026-10-24

Esta actualización solo planifica; no implementa ni ejecuta pruebas. La propuesta visual, pantallas y criterios transversales están en `plan.md`, sección «Revisión visual y de cierre». Una casilla pendiente puede representar implementación parcial, no ausencia total de código. Las casillas históricas conservadas no certifican una nueva ejecución ni el cierre de la historia; contrastar su alcance antes de cerrar la 007. No se marca ninguna tarea nueva como completada.

| Tareas reabiertas | Evidencia insuficiente o discrepancia |
| --- | --- |
| T043, T053, T054, T066, T075, T083, T102 | Archivos E2E con `test.skip`; auth solo visita `/` y además omite la suite. Implementar aserciones y ejecutar los escenarios obligatorios, registrando passed/failed/skipped. |
| T047, T087 | Perfil no distingue correctamente estados HTTP; los componentes globales están integrados parcialmente. Verificar estados en cada página, no solo helper. |
| T056 | Grupos crea/lista, pero no presenta membresías. Invitación queda condicionada al contrato público de FR-009. |
| T057, T058 | Clientes de listas parciales: faltan detalle/operaciones; `/purchaselists/mine` no aparece en el controlador revisado. Auditar contrato real antes de conectar. |
| T059, T060, T061 | Personal opera sobre primera lista y no ofrece CRUD completo ni RHF+Zod; compartidas no ofrece detalle de ítems. |
| T069, T071 | Formulario de gastos usa useState + Zod, sin RHF; integración no selecciona ítem real y puede usar placeholder. |
| T081 | Smoke solo comprueba main/nav, sin teclado, foco ni nombres de controles críticos. |
| T086 | Layout solo envuelve `/`; flex-wrap no acredita la experiencia responsive en todas las rutas. |
| T088 | Hay tokens de foco/color, pero sin medición de contraste ni evidencia del foco renderizado. |
| T089 | Workflow usa filtro `@smoke` y permite ausencia de tests; la suite revisada no aporta smoke obligatorio ejecutado. |
| T093 | Quickstart registra regresión fallida (27 errores de 58). Se conserva ese histórico; se reabre el cierre para obtener regresión satisfactoria. |

**Regla de cierre**: un archivo, un scaffold, un `test.skip`, una suite vacía o un comando que termina sin ejecutar escenarios no acreditan validación. Adjuntar comando, fecha, entorno, resultados y capturas/trazas cuando proceda. Los bloqueos de tenant/contrato quedan pendientes con causa explícita. TP8 y los quality gates requieren pruebas satisfactorias, no solo ejecutadas.

## Continuación priorizada: cuatro pasos (todos pendientes)

Las tareas T106–T118 completan y verifican el trabajo de las tareas originales; no sustituyen su cierre. Orden: paso 1 → 2 → 3 → 4. Dentro del paso 3, T111 audita contratos antes de conectar mutaciones. Los nombres de componentes nuevos son orientativos; conservar arquitectura por features.

### Paso 1 — Layout y navegación compartidos

**Corrección parcial del menú (2026-10-24)**: `routes.tsx` ahora anida todas las páginas autenticadas bajo `ProtectedRoute → MainLayout → Outlet`. `App.tsx` deja de montar un layout exclusivo de `/`; las páginas internas dejan de anidar landmarks main. NavLink identifica el destino activo mediante aria-current y estilo visible. `tests/component/auth/PersistentNavigation.spec.tsx` aporta 12 casos con páginas y MSAL controlados: cinco destinos, misma instancia de menú, historial del router, entrada directa, protección sin sesión y espera del bootstrap. Comando `npx.cmd vitest run tests/component/auth tests/component/accessibility`: 16 passed, 0 failed, 0 skipped. Lint y typecheck satisfactorios. Esto no demuestra recarga HTTP ni historial en navegador real con Entra: la inspección de Firefox falló por ventana minimizada. Mantener T106/T086 y verificación en navegador pendientes hasta completar los demás criterios (incluidos menú móvil y accesibilidad); no cerrar el bloque completo con estas pruebas de componente.

- [X] T106 [US5] Convertir `src/app/layout/MainLayout.tsx` y `src/app/router/routes.tsx` en layout padre protegido con Outlet; retirar main anidados en páginas. **Aceptación**: cinco destinos persistentes con enlace activo, un main/h1, skip link y menú móvil accesible; URL directa a cada destino conserva navegación. Pruebas de rutas y teclado en `tests/component/` y `tests/e2e/`. **Cierre acotado 2026-10-27**: T124/T126; no certifica autenticación integrada ni las acciones CRUD pendientes.
- [X] T107 [US2] Definir rutas `/personal-lists/:listId` y `/shared-lists/:listId`, contexto groupId y landing tras login en listas; actualizar `src/app/App.tsx`, router y navegación auth. **Aceptación**: enlace real selecciona lista, recarga y Atrás/Adelante conservan ID/grupo; índice sin selección no permite escribir en la primera lista implícitamente; 404 ofrece regreso seguro.

### Paso 2 — Diseño del núcleo de listas

- [ ] T108 [US5] Aplicar tokens de `plan.md` en `src/shared/ui/theme.ts` e `src/index.css`, usando primero Fluent UI v9; verificar licencia y carga local de Source Sans 3 antes de añadirla. **Aceptación**: escala tipográfica/espaciado coherente, fallback legible, estados Fluent preservados y combinaciones finales con contraste medido; registrar evidencia para T088.
- [ ] T109 [US2] Diseñar índice personal/compartido en `features/shopping-lists/` y `features/shared-lists/`: resúmenes enlazados, selección visible, crear lista y selector de grupo por nombre. **Aceptación**: vacío con acción, nombres largos sin desbordar y selección por URL; una columna móvil/tablet y composición índice/detalle en escritorio cuando quepa; capturas primero 390 px, luego 768/1280 px.
- [ ] T110 [US2] Diseñar detalle y filas en `features/shopping-lists/` reutilizables por compartidas. **Aceptación**: título/contexto/progreso, pendientes/comprados, cantidad/precio y autor disponible; marcar con texto accesible, menú por ítem y formularios Fluent; sin prometer desmarcado sin contrato ni mover foco al marcar. Fixtures representativos exclusivamente en pruebas; no datos simulados en producción.

### Paso 3 — Flujos funcionales pendientes

- [ ] T111 [US2] Auditar clientes `shoppingListsApi.ts`, `sharedListsApi.ts`, `groupsApi.ts` frente a controladores, DTOs y `contracts/frontend-backend-integration.md`; actualizar ese documento al verificar discrepancias. **Aceptación**: matriz operación/ruta/payload/respuesta/permisos con pruebas de contrato y 403 entre usuarios/grupos; resolver `/mine` y confirmar reutilización segura de endpoints genéricos para compartidas. Si falta backend, documentar bloqueo y tarea antes de ampliarlo; no cerrar FR-010/011 mediante acciones ocultas sin explicación.
- [ ] T112 [US2] Completar T057–T061: detalle real, renombrar/eliminar lista personal y agregar/consultar/editar/eliminar/marcar ítems; compartidas según matriz T111. **Aceptación**: RHF+Zod, ID seleccionado en cada escritura, confirmación de borrado, invalidación por lista/grupo e indicador pending; persiste tras recargar y no cambia otra lista. Cubrir éxito, vacío, 400/401/403/404/409/503 aplicables con RTL/MSW y E2E.
- [ ] T113 [US2] Completar membresías en `features/groups/pages/GroupsPage.tsx` y selección de grupos en compartidas. **Aceptación**: nombres/roles desde Users, sin entrada manual de GroupId; creación refresca listas y grupos correctos; invitación solo con contrato público FR-009, sin errores técnicos si no existe.
- [ ] T114 [US3] Completar T069/T071 en `ExpenseForm.tsx`, `SettlementPage.tsx` y detalle compartido: seleccionar ítem real, pagador/participantes por nombre, RHF+Zod; retirar IDs de ejemplo. **Aceptación**: escritura contiene sharedListId/itemId/UserIds reales, propuesta y balances proceden del backend, errores conservan datos y autorización impide operar sobre otro grupo; prueba completa registrar gasto → consultar reparto.

### Paso 4 — Navegador, responsive y accesibilidad

**Intento de verificación real — 2026-10-24**: inventario de herramientas de esta sesión sin MCP Playwright/Chrome DevTools ni herramientas de consola/red/viewport de navegador. El intento previo de Computer Use sobre Firefox fue bloqueado por la herramienta: «browser URL policy enforcement is not yet supported for the current Windows browser». No se elude ese bloqueo mediante otro controlador. `@playwright/test` instalado en el repositorio no equivale a un MCP conectado. `vite.config.ts` fija 5173 y Playwright configura 127.0.0.1:5173, pero no se ha observado la URL de arranque real de Vite; `netstat` no muestra listeners en 5173–5175/4173. No asumir que esa URL está activa ni cambiar hostname sin comprobar redirect URI de Entra.

| Comprobación solicitada en navegador real | Resultado de este intento |
| --- | --- |
| Menú persistente en las cinco páginas autenticadas | No ejecutada; pendiente de MCP compatible y sesión manual |
| Opción activa destacada | No ejecutada; pruebas de componente previas no sustituyen inspección visual |
| Recarga de ruta interna | No ejecutada; pendiente de navegador y servidor local confirmado |
| Atrás/Adelante del navegador | No ejecutada; solo existe evidencia previa del historial en MemoryRouter |
| Viewports 375, 430 y 1280 px sin overflow horizontal | No ejecutada; medir scrollWidth/clientWidth y guardar capturas de cada página/ancho |
| Menú con teclado y foco visible | No ejecutada; recorrer Tab/Shift+Tab/Enter e inspeccionar foco renderizado |
| Consola y peticiones sin fallos relevantes | No ejecutada; sin acceso a eventos de consola/red no puede declararse superada |
| Capturas de navegador real | No obtenidas; no crear imágenes sustitutivas como evidencia |

Desbloqueo: conectar MCP Playwright en el cliente local de Codex y abrir nueva sesión para comprobar herramientas disponibles; iniciar Vite y utilizar exactamente la URL que anuncie. Si se requiere login, el usuario lo realiza manualmente en el navegador controlado; no leer ni solicitar contraseñas, tokens o almacenamiento de sesión. Después repetir la matriz, capturar cada ancho y registrar errores por ruta/estado HTTP sin cabeceras de autorización ni credenciales. T116/T117 y demás tareas de validación permanecen pendientes.

**Actualización de disponibilidad (2026-10-24)**: `codex mcp list` confirma `playwright` enabled con `npx.cmd -y @playwright/mcp@latest`. Sin embargo, el catálogo de herramientas de esta conversación todavía no expone herramientas Playwright; configuración habilitada no acredita conexión operativa en esta sesión. Vite iniciado tras resolver EPERM mediante ejecución autorizada fuera del sandbox: salida real `VITE v8.3.5`, puerto 5173 ocupado y URL anunciada `http://localhost:5174/`. No se ha abierto mediante MCP ni iniciado sesión; verificaciones y capturas continúan pendientes. Antes de continuar tras recargar el cliente, confirmar que el servidor siga activo y que Entra admita el origen/redirect URI del puerto anunciado; no modificar registro ni credenciales automáticamente.

- [ ] T115 [US5] Completar T047/T087 en páginas principales y estados compartidos. **Aceptación**: matriz por pantalla de loading/empty/error/401/403/503 con acción apropiada; 400/404/409 aplicables; reintento de lectura, reauth sin duplicar escritura, campos conservados y anuncios asíncronos. Verificar navegación hacia estados y recuperación en navegador.
- [ ] T116 [US5] Completar T081/T083/T085/T086/T088 mediante revisión real. **Aceptación**: matriz 390x844 primero, 320x740, 430x932, 768x1024, 1280x800 y fronteras 767/768/1023/1024; capturas, zoom 200%, teclado virtual y sin overflow/solapamientos. Recorrido teclado completo, retorno de foco en Dialog/Drawer, lector de pantalla, contraste 4.5:1 texto normal y 3:1 texto grande/controles esenciales; objetivos táctiles >=44x44. Utilidades existentes deben estar integradas donde se necesiten, no solo definidas.
- [ ] T117 Completar E2E reabiertos y T089 en `tests/e2e/`, `playwright.config.ts` y `.github/workflows/ci-mikompri-web.yml`. **Aceptación**: smoke contiene escenarios reales seleccionados por CI y falla si faltan; navegador/dependencias preparados, resultados y trazas publicados; cero skips obligatorios. UI con sesión controlada solo en harness de pruebas y validación Entra/PKCE real por separado; sin tenant, auth integrado permanece pendiente.
- [ ] T118 Registrar resultados finales en `quickstart.md` y reconciliar casillas con evidencia. **Aceptación**: lint/typecheck/unit/component/integration/E2E/build/docker build y regresiones backend pertinentes satisfactorios; resolver T093 fallido histórico. Registrar comandos, fecha, entorno, passed/failed/skipped, capturas y limitaciones; SC-001/002 solo se cierran con medición de usuarios piloto. No declarar 007 completada mientras haya gates pendientes.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Crear el proyecto frontend base y preparar estructura técnica común del monorepo.

- [X] T001 Crear directorio `MiKompri.Web/` y archivo base `MiKompri.Web/README.md`
- [X] T002 Inicializar proyecto React + Vite en `MiKompri.Web/package.json`
- [X] T003 Configurar TypeScript strict en `MiKompri.Web/tsconfig.json`
- [X] T004 Configurar build/tooling de Vite en `MiKompri.Web/vite.config.ts`
- [X] T005 [P] Crear punto de entrada web en `MiKompri.Web/src/main.tsx`
- [X] T006 [P] Crear armazón inicial de aplicación en `MiKompri.Web/src/app/App.tsx`
- [X] T007 [P] Definir scripts de pruebas (unit/component/integration/e2e) en `MiKompri.Web/package.json`
- [X] T008 [P] Crear configuración base de Vitest en `MiKompri.Web/vitest.config.ts`
- [X] T009 [P] Crear configuración base de Playwright en `MiKompri.Web/playwright.config.ts`
- [X] T010 [P] Crear plantilla de entorno local/cloud en `MiKompri.Web/.env.example`

**Checkpoint**: Estructura base frontend creada en el monorepo.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Completar prerrequisitos backend y fundaciones frontend antes de historias de usuario.

**⚠️ CRITICAL**: Ninguna historia de usuario inicia hasta completar esta fase.

### Backend prerequisite (tests primero)

- [X] T011 [P] Agregar pruebas de migración de identidad `sub -> (tid, oid)` en `test/MiKompri.Users.Application.Tests/Commands/SyncProfile/SyncProfileCommandHandlerTests.cs`
- [X] T012 [P] Agregar pruebas de middleware Users para claims `tid`/`oid` en `test/MiKompri.Users.Api.Tests/Middleware/UserProvisioningMiddlewareTests.cs`
- [X] T013 [P] Agregar pruebas de autorización de listas personales por usuario autenticado en `test/MiKompri.ShoppingList.Api.Tests/PurchaseListsApiTests.cs`
- [X] T014 [P] Agregar pruebas JWT de ProductCatalog (401 sin token / 200 con token válido) en `test/MiKompri.ProductCatalog.Api.Tests/Auth/ProductCatalogAuthTests.cs`
- [X] T015 [P] Agregar pruebas de audience común entre APIs en `test/MiKompri.Users.Api.Tests/Security/CommonAudienceValidationTests.cs`
- [X] T016 [P] Agregar pruebas CORS para origen SPA local/cloud en `test/MiKompri.ShoppingList.Api.Tests/Security/CorsPolicyTests.cs`

### Backend prerequisite (implementación)

- [X] T017 Actualizar modelo de usuario para identidad canónica y `ExternalUserId` nullable en `MiKompri.Users.Domain/Users/User.cs`
- [X] T018 Ajustar mapeo EF de `ExternalUserId` nullable y nueva unicidad canónica en `MiKompri.Users.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [X] T019 Crear migración de Users para transición `sub -> (tid, oid)` y columna legacy nullable con nombre `AddCanonicalIdentityCorrelation` en `MiKompri.Users.Infrastructure/Migrations/`
- [X] T020 Adaptar comando de sincronización de perfil para `(tid, oid)` + legado en `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommand.cs`
- [X] T021 Adaptar handler de sincronización de perfil para resolver por identidad canónica en `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommandHandler.cs`
- [X] T022 Actualizar middleware de auto-provisioning Users para claims `tid`/`oid` en `MiKompri.Users.Api/Middleware/UserProvisioningMiddleware.cs`
- [X] T023 Alinear validación de audience común en Users API en `MiKompri.Users.Api/Program.cs`
- [X] T024 Implementar autorización de listas personales por identidad autenticada en `MiKompri.ShoppingList.Application/Queries/GetShoppingListsByOwner/GetShoppingListsByOwnerQueryHandler.cs`
- [X] T025 Asegurar derivación de `OwnerId` desde identidad autenticada en `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs`
- [X] T026 Configurar autenticación JWT real en ProductCatalog API en `MiKompri.ProductCatalog.Api/Program.cs`
- [X] T027 Alinear audience común y validaciones JWT en ShoppingList API en `MiKompri.ShoppingList.Api/Program.cs`
- [X] T028 Configurar CORS explícito para SPA local/cloud en `MiKompri.Users.Api/Program.cs`
- [X] T029 Configurar CORS explícito para SPA local/cloud en `MiKompri.ShoppingList.Api/Program.cs`
- [X] T030 Configurar CORS explícito para SPA local/cloud en `MiKompri.ProductCatalog.Api/Program.cs`

### Backend hardening adicional (bloqueante TP11)

- [X] T095 [P] Agregar pruebas de audience inválida/ válida en ShoppingList API en `test/MiKompri.ShoppingList.Api.Tests/Security/CommonAudienceValidationTests.cs`
- [X] T096 [P] Agregar pruebas de audience inválida/ válida en ProductCatalog API en `test/MiKompri.ProductCatalog.Api.Tests/Security/CommonAudienceValidationTests.cs`
- [X] T097 [P] Agregar pruebas del middleware de ShoppingList para flujo canónico TP11 (`tid`,`oid`) y rechazo seguro sin correlación en `test/MiKompri.ShoppingList.Api.Tests/Middleware/UserProvisioningMiddlewareTests.cs`
- [X] T098 Adaptar middleware de ShoppingList para no depender exclusivamente de `sub` y alinearlo al flujo canónico TP11 en `MiKompri.ShoppingList.Api/Middleware/UserProvisioningMiddleware.cs`
- [X] T099 Ajustar resolución de identidad en adaptador de Users para ShoppingList conforme a correlación canónica en `MiKompri.ShoppingList.Infrastructure/Services/UsersGroupAuthorizationAdapter.cs`

### Frontend foundation (arquitectura transversal)

- [X] T031 Crear proveedores globales (MSAL, Router, QueryClient, Theme) en `MiKompri.Web/src/app/providers/AppProviders.tsx`
- [X] T032 Configurar MSAL Browser/React y bootstrap de sesión en `MiKompri.Web/src/auth/msal/msalConfig.ts`
- [X] T033 Implementar guard de rutas protegidas en `MiKompri.Web/src/auth/guards/ProtectedRoute.tsx`
- [X] T034 Implementar cliente API común con propagación Bearer token en `MiKompri.Web/src/api/client/httpClient.ts`
- [X] T035 Implementar mapeo centralizado de errores HTTP 400/401/403/404/409/503 en `MiKompri.Web/src/api/errors/httpErrorMapper.ts`
- [X] T036 Configurar QueryClient y políticas base de TanStack Query en `MiKompri.Web/src/app/providers/queryClient.ts`
- [X] T037 Crear router principal de la SPA en `MiKompri.Web/src/app/router/index.tsx`
- [X] T038 Configurar Fluent UI Theme global en `MiKompri.Web/src/shared/ui/theme.ts`
- [X] T039 Configurar lectura de variables de entorno por ambiente en `MiKompri.Web/src/shared/config/env.ts`

**Checkpoint**: Prerrequisitos backend y fundaciones frontend listos para historias.

---

## Phase 3: User Story 1 - Acceso seguro y sesión + perfil (Priority: P1) 🎯 MVP

**Goal**: Login/logout, restauración de sesión, rutas protegidas y perfil autenticado.

**Independent Test**: Usuario inicia sesión con Entra, accede a rutas protegidas, recarga y conserva sesión, cierra sesión y pierde acceso.

### Tests for User Story 1

- [X] T040 [P] [US1] Crear pruebas unitarias de sesión/auth helpers en `MiKompri.Web/tests/unit/auth/session.spec.ts`
- [X] T041 [P] [US1] Crear pruebas de componente de `ProtectedRoute` en `MiKompri.Web/tests/component/auth/ProtectedRoute.spec.tsx`
- [X] T042 [P] [US1] Crear pruebas de integración MSW para `/users/me` y restauración de sesión en `MiKompri.Web/tests/integration/auth/session.integration.spec.tsx`
- [X] T103 [US1] Crear prueba de integración para expiración de token durante operación de escritura y reautenticación guiada en `MiKompri.Web/tests/integration/auth/token-expiration-write.integration.spec.tsx`
- [ ] T043 [US1] Crear prueba E2E de login/logout/restore en `MiKompri.Web/tests/e2e/auth/auth-session.spec.ts`

### Implementation for User Story 1

- [X] T044 [P] [US1] Implementar flujo de login/logout en `MiKompri.Web/src/auth/session/authSessionService.ts`
- [X] T045 [P] [US1] Implementar pantalla de login y callback en `MiKompri.Web/src/features/profile/pages/LoginPage.tsx`
- [X] T046 [US1] Implementar consulta de perfil autenticado con TanStack Query en `MiKompri.Web/src/features/profile/api/profileApi.ts`
- [ ] T047 [US1] Implementar vista de perfil con estados loading/error/empty/unauthorized en `MiKompri.Web/src/features/profile/pages/ProfilePage.tsx`
- [X] T048 [US1] Registrar rutas protegidas de perfil en `MiKompri.Web/src/app/router/routes.tsx`

**Checkpoint**: US1 pendiente de cierre verificado; véase revisión de evidencia del 2026-10-24.

---

## Phase 4: User Story 2 - Gestión básica de grupos + listas personales/compartidas + ítems (Priority: P1)

**Goal**: Operación completa de grupos mínimos y listas (personal/shared) con ítems.

**Independent Test**: Usuario crea grupo, gestiona listas personales/compartidas e ítems; no miembro recibe 403 en listas de grupo.

### Tests for User Story 2

- [X] T049 [P] [US2] Crear pruebas unitarias de validación de formularios (RHF + Zod) en `MiKompri.Web/tests/unit/features/groups/groupSchemas.spec.ts`
- [X] T050 [P] [US2] Crear pruebas de componente de lista personal en `MiKompri.Web/tests/component/features/shopping-lists/PersonalListPage.spec.tsx`
- [X] T051 [P] [US2] Crear pruebas de componente de lista compartida en `MiKompri.Web/tests/component/features/shared-lists/SharedListPage.spec.tsx`
- [X] T052 [P] [US2] Crear pruebas MSW para grupos/listas/ítems y errores 400/403/409 en `MiKompri.Web/tests/integration/features/lists-and-groups.integration.spec.tsx`
- [ ] T053 [US2] Crear prueba E2E de CRUD de lista personal e ítems en `MiKompri.Web/tests/e2e/lists/personal-lists.spec.ts`
- [ ] T054 [US2] Crear prueba E2E de acceso autorizado/no autorizado a lista compartida en `MiKompri.Web/tests/e2e/lists/shared-lists-authorization.spec.ts`

### Implementation for User Story 2

- [X] T055 [P] [US2] Implementar API de grupos (crear/listar/membresías/invitar) en `MiKompri.Web/src/features/groups/api/groupsApi.ts`
- [ ] T056 [P] [US2] Implementar páginas de grupos y membresías en `MiKompri.Web/src/features/groups/pages/GroupsPage.tsx`
- [ ] T057 [P] [US2] Implementar API de listas personales e ítems en `MiKompri.Web/src/features/shopping-lists/api/shoppingListsApi.ts`
- [ ] T058 [P] [US2] Implementar API de listas compartidas e ítems en `MiKompri.Web/src/features/shared-lists/api/sharedListsApi.ts`
- [ ] T059 [US2] Implementar página de listas personales con formularios RHF+Zod en `MiKompri.Web/src/features/shopping-lists/pages/PersonalListsPage.tsx`
- [ ] T060 [US2] Implementar página de listas compartidas con validación de estados 403 en `MiKompri.Web/src/features/shared-lists/pages/SharedListsPage.tsx`
- [ ] T061 [US2] Implementar componentes de ítems y acciones CRUD en `MiKompri.Web/src/features/shopping-lists/components/ListItemsPanel.tsx`
- [X] T062 [US2] Registrar rutas de grupos/listas en `MiKompri.Web/src/app/router/routes.tsx`

**Checkpoint**: US2 pendiente de cierre verificado; véase revisión de evidencia del 2026-10-24.

---

## Phase 5: User Story 3 - Gastos compartidos, balances y settlement (Priority: P2)

**Goal**: Registrar gastos por ítem y visualizar balances/settlement en listas compartidas.

**Independent Test**: Miembro válido registra gasto, consulta balances y settlement; usuario sin permiso recibe 403.

### Tests for User Story 3

- [X] T063 [P] [US3] Crear pruebas unitarias de reglas de formulario de gastos en `MiKompri.Web/tests/unit/features/settlements/expenseSchemas.spec.ts`
- [X] T064 [P] [US3] Crear pruebas de componente de balances/settlement en `MiKompri.Web/tests/component/features/settlements/SettlementPanel.spec.tsx`
- [X] T065 [P] [US3] Crear pruebas MSW de gastos, balances y settlement (incluye 503) en `MiKompri.Web/tests/integration/features/settlements.integration.spec.tsx`
- [ ] T066 [US3] Crear prueba E2E de flujo registrar gasto + consultar settlement en `MiKompri.Web/tests/e2e/settlements/settlement-flow.spec.ts`
- [ ] T102 [US3] Crear prueba E2E de acceso no autorizado (403) a settlement para miembro sin permisos en `MiKompri.Web/tests/e2e/settlements/settlement-authorization.spec.ts`

### Implementation for User Story 3

- [X] T067 [P] [US3] Implementar API de gastos compartidos en `MiKompri.Web/src/features/settlements/api/expensesApi.ts`
- [X] T068 [P] [US3] Implementar API de balances/settlement en `MiKompri.Web/src/features/settlements/api/settlementApi.ts`
- [ ] T069 [US3] Implementar formulario de gasto compartido con RHF+Zod en `MiKompri.Web/src/features/settlements/components/ExpenseForm.tsx`
- [X] T070 [US3] Implementar vista de balances y transferencias propuestas en `MiKompri.Web/src/features/settlements/pages/SettlementPage.tsx`
- [ ] T071 [US3] Integrar settlement en navegación de listas compartidas en `MiKompri.Web/src/features/shared-lists/pages/SharedListsPage.tsx`

**Checkpoint**: US3 pendiente de cierre verificado; véase revisión de evidencia del 2026-10-24.

---

## Phase 6: User Story 4 - Consulta y alta de productos (Priority: P2)

**Goal**: Consulta de productos, mercados e historial y alta de productos con nombre/unidad, según ampliación del 2026-10-25. Sin edición/desactivación ni administración de mercados/precios.

**Independent Test**: Usuario autenticado consulta catálogo e historial; la UI maneja empty y 503.

### Tests for User Story 4

- [X] T072 [P] [US4] Crear pruebas unitarias de transformadores de catálogo en `MiKompri.Web/tests/unit/features/product-catalog/catalogMappers.spec.ts`
- [X] T073 [P] [US4] Crear pruebas de componente de listado de catálogo en `MiKompri.Web/tests/component/features/product-catalog/ProductCatalogPage.spec.tsx`
- [X] T074 [P] [US4] Crear pruebas MSW para catálogo read-only e historial (empty/503) en `MiKompri.Web/tests/integration/features/product-catalog.integration.spec.tsx`
- [ ] T075 [US4] Crear prueba E2E de navegación y consulta de ProductCatalog en `MiKompri.Web/tests/e2e/product-catalog/product-catalog-readonly.spec.ts`

### Implementation for User Story 4

- [X] T076 [P] [US4] Implementar API read-only de ProductCatalog en `MiKompri.Web/src/features/product-catalog/api/productCatalogApi.ts`
- [X] T077 [US4] Implementar consulta de productos/mercados en `MiKompri.Web/src/features/product-catalog/pages/ProductCatalogPage.tsx`; el alta añadida después se documenta en «Acceso para crear productos» y FR-013.
- [X] T078 [US4] Implementar vista de historial de precios por producto/mercado en `MiKompri.Web/src/features/product-catalog/components/PriceHistoryPanel.tsx`
- [X] T079 [US4] Registrar ruta de ProductCatalog en `MiKompri.Web/src/app/router/routes.tsx`

**Checkpoint**: US4 pendiente de cierre verificado; véase revisión de evidencia del 2026-10-24.

---

## Phase 7: User Story 5 - Responsive, accesibilidad y estados UX globales (Priority: P3)

**Goal**: Consolidar diseño responsive, accesibilidad básica y estados globales de carga/error/autorización.

**Independent Test**: Flujos P1 operables en mobile/tablet/desktop con navegación por teclado y feedback consistente de estados.

### Tests for User Story 5

- [X] T080 [P] [US5] Crear pruebas de componente para estados UI globales en `MiKompri.Web/tests/component/shared/UiStateViews.spec.tsx`
- [ ] T081 [P] [US5] Crear pruebas de accesibilidad básica en componentes clave con RTL en `MiKompri.Web/tests/component/accessibility/accessibility-smoke.spec.tsx`
- [X] T082 [P] [US5] Crear pruebas MSW de mapeo de errores HTTP 401/403/503 en `MiKompri.Web/tests/integration/shared/http-error-states.integration.spec.tsx`
- [ ] T083 [US5] Crear prueba E2E responsive (mobile/tablet/desktop) de flujos P1 en `MiKompri.Web/tests/e2e/responsive/responsive-smoke.spec.ts`

### Implementation for User Story 5

- [X] T084 [P] [US5] Implementar componentes globales `LoadingState`, `EmptyState`, `ErrorState`, `UnauthorizedState`, `ForbiddenState`, `ServiceUnavailableState` en `MiKompri.Web/src/shared/ui/states/`
- [X] T085 [P] [US5] Implementar utilidades de accesibilidad (focus management, nombres accesibles) en `MiKompri.Web/src/shared/accessibility/a11y.ts`
- [X] T086 [US5] Aplicar layout responsive base en `MiKompri.Web/src/app/layout/MainLayout.tsx`. Evidencia T124/T126, cinco anchos y menú compacto/lateral; la auditoría ampliada sigue en T116.
- [ ] T087 [US5] Integrar estados UX globales en páginas de features en `MiKompri.Web/src/shared/ui/states/withViewState.tsx`
- [ ] T088 [US5] Ajustar tema Fluent UI para contraste y foco visible en `MiKompri.Web/src/shared/ui/theme.ts`

**Checkpoint**: US5 pendiente de cierre verificado; véase revisión de evidencia del 2026-10-24.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Cierre transversal de calidad, CI y preparación de despliegue.

- [ ] T089 [P] Configurar workflow CI frontend (install + lint + typecheck + tests unit/component/integration + e2e smoke + build + `docker build`) en `.github/workflows/ci-mikompri-web.yml`
- [X] T104 [P] Crear Dockerfile del frontend para build reproducible en CI en `MiKompri.Web/Dockerfile`
- [X] T090 [P] Configurar estrategia de preparación Azure Static Web Apps (sin despliegue) en `MiKompri.Web/staticwebapp.config.json`
- [X] T091 [P] Documentar variables de entorno local/cloud y redirect URIs en `MiKompri.Web/.env.example`
- [X] T092 [P] Agregar guía de CORS para frontend y APIs en `docs/web-client-cors.md`
- [ ] T093 Ejecutar regresión API de ShoppingList MVP-0/MVP-3 con `dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release` y registrar resultado en `specs/007-web-react-client/quickstart.md`
- [X] T094 Actualizar quickstart de validación cruzada frontend/backend en `specs/007-web-react-client/quickstart.md`
- [X] T100 Ejecutar regresión API de Users MVP-1 con `dotnet test test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj --configuration Release` y registrar resultado en `specs/007-web-react-client/quickstart.md`
- [X] T101 Ejecutar regresión API de ProductCatalog MVP-2 con `dotnet test test/MiKompri.ProductCatalog.Api.Tests/MiKompri.ProductCatalog.Api.Tests.csproj --configuration Release` y registrar resultado en `specs/007-web-react-client/quickstart.md`
- [X] T105 [P] Documentar matriz de alineación de contratos/identidad reutilizable para clientes Web/MAUI/iOS en `docs/client-contract-reuse-guidelines.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: inicia inmediatamente.
- **Phase 2 (Foundational)**: depende de Phase 1 y bloquea todas las historias.
- **Phases 3-7 (User Stories)**: dependen de Phase 2; se ejecutan por prioridad (P1 → P2 → P3).
- **Phase 8 (Polish)**: depende de completar historias requeridas.

### User Story Dependencies

- **US1 (P1)**: depende solo de Foundational.
- **US2 (P1)**: depende de Foundational; puede avanzar en paralelo con cierre de US1 si hay equipo separado.
- **US3 (P2)**: depende de US2 (requiere listas compartidas e ítems operativos).
- **US4 (P2)**: depende solo de Foundational (consulta y alta de productos desacopladas del reparto).
- **US5 (P3)**: depende de US1-US4 para aplicar ajustes transversales finales.

### Backend Prerequisite Dependencies

- `sub -> (tid, oid)` + `ExternalUserId` nullable + audience común deben completarse antes de cerrar US1.
- personal ShoppingList auth debe completarse antes de cerrar US2.
- ProductCatalog JWT debe completarse antes de cerrar US4.

## Parallel Execution Examples

### US1

- Ejecutar en paralelo `T040`, `T041`, `T042` mientras se implementa `T044` y `T045`.

### US2

- Ejecutar en paralelo `T055`, `T057`, `T058` (APIs por feature) y `T050`, `T051`, `T052` (tests).

### US3

- Ejecutar en paralelo `T067` y `T068` antes de consolidar `T069` y `T070`.

### US4

- Ejecutar en paralelo `T072`, `T073`, `T074` junto con `T076`.

### US5

- Ejecutar en paralelo `T080`, `T081`, `T082` junto con `T084` y `T085`.

## Implementation Strategy

### MVP First (US1 + US2)

1. Completar Setup + Foundational (incluye prerrequisitos backend obligatorios).
2. Entregar US1 (auth/sesión/perfil) como primer incremento funcional.
3. Entregar US2 (grupos + listas + ítems) para cerrar el núcleo de uso diario.

### Incremental Delivery

4. Agregar US3 (gastos/balances/settlement).
5. Agregar US4 (consulta de catálogo y alta de productos).
6. Cerrar con US5 (responsive/accesibilidad/estados UX) y Phase 8.

### Quality Gates

- Todas las tareas de test de cada historia completadas y en verde.
- Regresión backend MVP-0 a MVP-3 ejecutada antes de finalizar.
- CI frontend activo y estable antes de preparar despliegue futuro SWA.

## Portada React — 2026-10-27

- [X] T120 Implementar FR-032 con componentes Fluent, estilos acotados, tema/fuentes locales, ejemplo separado y acciones reales.
- [X] T121 Verificar visitante/autenticado, rutas protegidas, menú compartido, teclado/foco y formularios existentes.
- [X] T122 Comprobar 320/390/430/768/1280 px y ejecutar controles existentes; registrar evidencia y límites sin despliegue.

### Evidencia T120–T122 (2026-10-27)

- [X] T120: portada por componentes React/TypeScript y Fluent UI v9, CSS Module, fuente Source Sans 3 local/licenciada y tema #006B68. Ejemplo en `features/home/exampleList.ts`, sin consultas ni mutaciones; no hay selector Vista, invitaciones o pagos ficticios. No se copian HTML ni listeners del prototipo.
- [X] T121: visitante ve portada; usuario autenticado o con cuentas MSAL llega a `/personal-lists`; bootstrap muestra carga. Crear lista pasa por ProtectedRoute y login real y llega al formulario personal existente. Login/callback usan el índice personal como destino, conforme al plan. Se conserva el layout y los cinco destinos del menú compartido; los cambios locales previos no se sobrescriben. Pruebas específicas de portada, navegación persistente, guard, listas/formularios, catálogo, unitarias e integración: **59 passed, 0 failed** (`npx.cmd vitest run tests/component/features/home tests/component/auth tests/component/features/shopping-lists tests/component/features/product-catalog tests/unit tests/integration --maxWorkers=2 --reporter=json --outputFile=../.playwright-mcp/home-targeted-vitest.json`).
- [X] T122: Playwright en Chrome instalado, **5 passed** con 320/390/430/768/1280 px: sin desbordamiento, un main/h1, salto al contenido por Tab/Enter, marcado por Espacio, progreso/restablecimiento, Drawer con foco dentro, Escape/retorno al disparador, enlace móvil/FAQ y CTA → login. Objetivos de CTA >=44×44 y foco visible comprobados. Se corrigió el retorno de foco con `useRestoreFocusTarget` de Fluent tras detectarlo en navegador. Capturas revisadas de móvil y escritorio: `.playwright-mcp/home-{320,390,430,768,1280}.png` (copias de `MiKompri.Web/test-results/`). Comando: `$env:PLAYWRIGHT_CHANNEL='chrome'; npx.cmd playwright test tests/e2e/home --workers=2`. Chromium headless no está instalado; el canal opcional permite usar Chrome sin cambiar el valor por defecto en CI.
- Lint, typecheck, build de producción y `git diff --check`: satisfactorios. `docker build -f MiKompri.Web/Dockerfile -t mikompri-web:home-review .`: satisfactorio, imagen solo local, sin ejecución/despliegue/publicación. Vitest excluye ahora `tests/e2e/**`: antes intentaba ejecutar también archivos Playwright y fallaba al cargarlos.

**Límites y pendientes**: la regresión Vitest completa (`npx.cmd vitest run --maxWorkers=2 --reporter=json --outputFile=../.playwright-mcp/home-vitest.json`) arroja **78 passed, 3 failed**. Los tres fallos pertenecen a `SharedGroupSelection.spec.tsx` (nombre accesible de Dialog, acceso a Cancelar y conservación del nombre tras 403). Tanto ese archivo como `SharedListsPage.tsx` están sin cambios respecto a HEAD y no importan la portada/tema; no se certifica ese flujo ni se alteran sus tests para ocultar el fallo. Los siete E2E antiguos siguen omitidos por sus prerrequisitos de tenant/backend; no se valida login real, CRUD remoto o flujo multiusuario nuevo. Las validaciones de formularios reutilizados se cubren en componentes/integración; no se ha creado ningún dato real. Backend y Entra permanecen sin cambios, sin repetir suites .NET para este cambio de cliente. No es una auditoría completa con lector de pantalla ni todos los navegadores. Persiste la advertencia de bundle >500 kB y React Router v7. Docker/npm ci informa de cinco vulnerabilidades de dependencias (3 moderadas, 2 críticas); no se actualiza el lockfile dentro de esta integración. Los TTF suman ~704 kB sin comprimir; posible conversión a WOFF2 futura. No se declara cerrada la spec 007 ni los gates globales pendientes.

## Coherencia visual transversal — 2026-10-27

- [X] T123 Analizar cada sección y aplicar sistema compartido de superficies, tipografía, controles y estados alineado con portada.
- [X] T124 Extender navegación autenticada responsive y acceso/perfil, preservando protección y destinos.
- [X] T125 Adaptar listas, grupos, catálogo/historial y reparto con formularios y diálogos coherentes, sin modificar contratos ni permisos.
- [X] T126 Validar cinco anchos, teclado/foco y navegación/formularios; ejecutar controles y documentar evidencia por sección y límites.

### Evidencia T123–T126 (2026-10-27)

- [X] T123: análisis por sección en `plan.md`, «Extensión visual por sección». `PageHeader` y `AppSurface.module.css` centralizan cabeceras, superficies, formularios, filas, selección, estados y áreas táctiles. El tema Fluent comparte Source Sans 3, marca teal, texto y superficies. Las reglas se acotan mediante CSS Modules y clases de componente Fluent; se conservan semántica y estados de deshabilitado.
- [X] T124: MainLayout con marca, salto al contenido, lateral desde 1024 px y Drawer móvil/tablet. Conserva los cinco destinos y sus rutas protegidas; selección visible con `aria-current`. Escape devuelve el foco mediante `useRestoreFocusTarget`. Login y callback usan composición de acceso coherente; perfil mantiene h1 durante carga/error y presenta sus datos en una lista de definición. Login permanece conectado a MSAL, sin nueva configuración ni simulaciones en producción.
- [X] T125: grupos, formularios de listas/productos/gastos, selector de grupo y diálogo comparten formato. Las listas personales muestran filas continuas y progreso calculado exclusivamente sobre ítems reales ya cargados. Se eliminan bordes anidados innecesarios. Catálogo muestra nombres de mercado en el selector e historial, fechas españolas e importes EUR; los IDs siguen siendo valores internos de los contratos existentes. Balances y pagos propuestos conservan sus cálculos backend. No se añaden endpoints, invitaciones, transferencias reales ni permisos. No se incorporan datos ficticios a producción.
- [X] T126: `npm.cmd run lint` y `npm.cmd run typecheck`: satisfactorios, sin advertencias de lint al cierre. `npm.cmd run build`: satisfactorio. `docker build -f MiKompri.Web/Dockerfile -t mikompri-web:visual-review .`: satisfactorio, solo imagen local. `git diff --check`: satisfactorio. Ningún despliegue, publicación o cambio de visibilidad.

**Regresión automatizada**: `npx.cmd vitest run --maxWorkers=2 --reporter=json --outputFile=../.playwright-mcp/visual-vitest.json`: **81 passed, 0 failed, 0 skipped**. Incluye sesión, guard, persistencia de navegación, selección/escritura en lista, formulario de producto, grupos, reparto/autorización y errores HTTP. La aserción de fila de `PersonalListSelection.spec.tsx` se adapta a nombre y cantidad ahora separados semánticamente (`listitem` + `toHaveTextContent`); mantiene el criterio de selección y envío del ID. Los tres fallos de compartidas registrados en la entrega anterior no se reprodujeron en las dos regresiones completas satisfactorias; sus aserciones no se han relajado ni omitido. No se atribuye su desaparición a una corrección funcional concreta.

**Navegador**: `$env:PLAYWRIGHT_CHANNEL='chrome'; npx.cmd playwright test --workers=2 --reporter=json > ../.playwright-mcp/visual-playwright.json`: **11 passed, 0 failed, 7 skipped**, sin reintentos ni flakiness en el informe final. Cinco escenarios recorren perfil, grupos, detalle personal, catálogo/historial, diálogo compartido, reparto y login a **320/390/430/768/1280 px**, comprobando un main/h1 y ausencia de desbordamiento. Validan salto al contenido por Tab/Enter, foco dentro de Drawer, Escape/retorno al disparador, navegación por sus enlaces, cierre del diálogo con teclado, selección de producto/cantidad, filtro de mercado y elegibilidad del formulario de gasto. Otros cinco escenarios preservan la portada. El escenario restante comprueba carga, vacío y errores **401/403/503** de perfil, grupos, listas personales/compartidas y catálogo a 390 px. Detectadas y resueltas colisiones de especificidad de navegación y controles Fluent durante la revisión.

**Evidencia visual**: 35 capturas en `.playwright-mcp/app-{profile,groups,personal,catalog,shared-dialog,settlement,login}-{320,390,430,768,1280}.png`, copiadas desde `MiKompri.Web/test-results/`. Revisión visual de móvil y escritorio incluyendo nombres largos, datos de perfil, progreso y formulario de reparto. Informes JSON en `.playwright-mcp/visual-vitest.json` y `visual-playwright.json`.

**Límites**: las capturas interiores usan `tests/visual/` con contexto MSAL y red controlados exclusivamente para pruebas, sin cuentas reales ni escrituras en APIs. El harness importa las mismas rutas y componentes; se comprueba que `visual-test-token` y los datos del harness no aparecen en `dist/`. No sustituye la validación de Entra/PKCE o de datos reales multiusuario. Los siete E2E antiguos de entorno integrado siguen omitidos explícitamente por sus prerrequisitos; no se declara cerrada toda la spec 007. No se ejecutan regresiones .NET por este cambio de presentación ni se modifican contratos backend, scopes, permisos, lockfile o configuración Entra. La revisión cubre Chrome y accesibilidad de teclado básica; no certifica lectores de pantalla, todos los navegadores ni toda la matriz de roles. Persiste la advertencia de bundle >500 kB y las advertencias de futura migración de React Router; las observaciones previas de dependencias/fuentes continúan pendientes.

## Alineación documental de diseño — 2026-10-27

- [X] T127 Revisar spec, plan, tareas, modelo, research, contrato, quickstart y checklist; consolidar el diseño vigente en `design.md`, marcar la propuesta de 2026-10-24 como histórico, incorporar FR-032/033 en los requisitos y reconciliar T086/T106 con T124/T126. Mantener abiertos los gates de contraste, CRUD, membresías, RHF, roles, accesibilidad ampliada y validación integrada.

Se corrigen las referencias vigentes de catálogo estrictamente read-only para reflejar el alta ya autorizada, sin ampliar el backend ni modificar las specs propietarias 002–005. Quickstart separa revisión visual controlada de entorno integrado y conserva las regresiones antiguas como histórico. Checklist deja de afirmar que todos los criterios de éxito están satisfechos. Comprobación documental: nueve Markdown revisados, cero enlaces relativos rotos, cero IDs de requisito duplicados y `git diff --check` satisfactorio. Los informes existentes confirman 81 pruebas Vitest correctas y 11 E2E correctos/7 omitidos; no se han vuelto a ejecutar pruebas de aplicación en esta revisión de documentación. Sin cambios de código, configuración, despliegue o visibilidad en este paso.

## Recorrido privado del prototipo — 2026-10-27

- [X] T128 Auditar contratos y documentar composición/recorridos FR-034; distinguir marcado personal y limitación de autorización compartida.
- [X] T129 Implementar creación breve RHF+Zod e índice/detalle personal con progreso, incorporación contextual y marcado real sin desmarcado.
- [X] T130 Implementar detalle compartido protegido, alta real y Compra/Gastos con tab/contexto en URL; retorno al detalle desde catálogo.
- [X] T131 Validar formularios, errores, duplicados, foco, historial/recarga, navegación y cinco anchos; ejecutar controles y registrar límites.

- [ ] T132 Prerrequisito backend identificado en T128: acreditar autenticación y autorización de propietario/membresía/rol en el endpoint genérico de marcado antes de habilitarlo para listas compartidas. Añadir pruebas negativas entre usuarios/grupos y reconciliar con 005 FR-004. Este cambio de UX no modifica el backend ni presenta su guard de selección como frontera de seguridad.

### Evidencia T128–T131 — UX privada (2026-10-27)

- [X] T128: sin AGENTS.md encontrado en el árbol del repositorio. Revisadas instrucciones aplicables, constitución, spec/plan/tasks/design y contratos 007, reglas 005 de membresía/roles/reparto, prototipo y handlers/DTOs reales. Composición antes de código en design.md y plan.md. Contrato sección 12 distingue alta compartida (handler con membresía/AddedBy) del marcado genérico sin comprobación de propiedad/rol; limitación concreta en T132. No se modificaron backend, permisos, configuración Entra o contratos remotos.
- [X] T129: índice personal con acción Crear lista y selección explícita, creación Dialog RHF+Zod, apertura del ID devuelto, detalle con nombre, progreso y filas de compra. Añadir productos abre diálogo y carga el catálogo únicamente cuando se necesita; altas con producto real, cantidad/precio válidos, bloqueo de duplicados y valores conservados ante error. Marcado personal por POST existente, restringido al ítem pendiente de la lista encontrada en /mine; sin desmarcado ni éxito optimista. Se conserva el nodo y foco del botón al pasar a Comprado. Borradores y avisos aislados por cuenta/lista. El guard UI no acredita autorización del servidor (T132).
- [X] T130: `/shared-lists/:listId` protegida, detalle autorizado por backend y grupo por nombre. Alta real con `{ productId, name, quantity, estimatedPrice }`, sin usar el endpoint personal. Compra/Gastos de la misma lista, con `tab` en query y enlace de regreso al grupo real del detalle. Gastos integra el flujo existente de registro/balance/propuesta sin recalcular cuentas. El catálogo admite regreso interno a detalle personal/compartido y conserva la pestaña permitida; destinos externos o arbitrarios se rechazan. Mensaje explícito sobre marcado compartido no disponible, sin checkbox ni éxito ficticio. T107 se cierra en el alcance de rutas/contexto; CRUD completo, composición lateral opcional y matriz de roles siguen pendientes.
- [X] T131: lint y TypeScript satisfactorios, sin advertencias de lint. Build de producción y Docker local `mikompri-web:private-ux-review` satisfactorios. `git diff --check` sin errores de whitespace. Se comprueba que el token/identidad del harness y su adaptador JSDOM no aparecen en dist. Sin despliegue, publicación o cambio de visibilidad; portada conservada.

**Regresión final**: `npx.cmd vitest run --maxWorkers=2 --reporter=json --outputFile=../.playwright-mcp/private-ux-vitest-final.json`: **96 passed, 0 failed, 0 skipped**. Se conservan/adaptan las aserciones a índices separados y diálogos, en lugar de exigir formularios permanentes. Nuevos casos de creación (espacios, error/reintento, doble envío/cierre pendiente), marcado por lista/ítem y 401/403/503 sin falso éxito, detalle compartido directo, pestaña/historial, altas con payload compartido y 401/403/404/503 sin exponer escrituras. Prueba de regreso a Gastos desde catálogo. La selección/invalidación conserva el ID y no altera la otra lista de la fixture.

**Navegador final**: `$env:PLAYWRIGHT_CHANNEL='chrome'; npx.cmd playwright test --workers=2 --reporter=json > ../.playwright-mcp/private-ux-playwright-final.json`: **16 passed, 0 failed, 7 skipped, 0 flaky**, sin reintentos. Incluye portada, revisión por sección/estados y cinco recorridos de crear → detalle → añadir → marcar por teclado → progreso confirmado, con retorno de foco y sin scroll horizontal a **320/390/430/768/1280 px**. Entrada directa a Gastos, cambio a Compra, mismo ID y regreso al grupo. Las pruebas de componente verifican Atrás/Adelante del router; el harness de navegador usa MemoryRouter y no sustituye la recarga HTTP e historial con autenticación real. Las comprobaciones de main/h1 al abrir un modal cuentan la estructura DOM de fondo, que debe quedar fuera del árbol accesible durante el modal.

**Capturas e informes**: `.playwright-mcp/app-private-shopping-{320,390,430,768,1280}.png` y `app-private-shared-{320,390,430,768,1280}.png`, copiadas de `MiKompri.Web/test-results/`; además de capturas de las otras secciones. Revisadas visualmente móvil y escritorio. JSON finales `private-ux-vitest-final.json` y `private-ux-playwright-final.json`. Correcciones detectadas durante QA: conexión de Fluent/RHF mediante Controller y refs nativos, bloqueo independiente de duplicados en creación, nombres accesibles de diálogos y conservación de pestaña al regresar del catálogo.

**Límites de prueba**: JSDOM no representa de forma fiable el modalizador de Tabster; `tests/setup/jsdom-dialogs.ts` adapta SOLO en pruebas de componente el Dialog real a non-modal para verificar formularios, estado y eventos. No se atribuye a esos tests cobertura de trap/restore de foco: Playwright usa el Dialog modal real sin este adaptador y comprueba Escape/retorno de foco. El setup JSDOM aporta un viewport de body para evitar el falso iframe oculto; no se usa como evidencia de geometría. La red de las pruebas de componente sigue el patrón existente de dobles fetch; no se añadió MSW ni dependencias nuevas. Datos y MSAL controlados solo en el harness; no se crearon listas o productos reales en esta validación. Las siete pruebas integradas antiguas permanecen omitidas por prerrequisitos de tenant/backend. No se certifican Entra/PKCE, permisos multiusuario completos, marcado compartido, autoría histórica, todas las operaciones CRUD, zoom/lector de pantalla o métricas piloto. Sigue T132 abierto y el gate ampliado de la 007; no se ejecutan regresiones .NET al no modificar backend.

**Revisión de guías (hallazgo abierto)**: `MiKompri.ShoppingList.Application/Commands/MarkItemAsPurchased/MarkItemAsPurchasedCommandHandler.cs:19` — falta validación de propietario/membresía/rol; T132. UI revisada con las Web Interface Guidelines oficiales de Vercel (https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md): navegación mediante enlaces, acciones mediante botones, formularios etiquetados, estado asíncrono anunciado y pestañas en URL. La paleta/contraste ampliado y los controles de lector de pantalla permanecen en T116. Persiste advertencia de bundle >500 kB (~1.04 MB JS antes de gzip), optimización WOFF2 y advertencias futuras de React Router.

## Corrección del marcado tras añadir — 2026-10-27

- [X] T133 Reproducir crear → añadir → leer → marcar → leer con ItemId distinto de ProductId; corregir el dominio que busca ProductId a pesar del contrato `{itemId}`. Cubrir identidad cruzada, progreso persistido e idempotencia, ejecutar regresiones backend y validar imagen/servicio local sin tocar datos del usuario. No sustituir el ID en React para acomodar el fallo ni cerrar T132 de autorización por esta corrección.

### Evidencia de corrección T133 — 2026-10-27

Causa confirmada: el cliente y el comando enviaban ItemId, pero `PurchaseList.MarkItemAsPurchased` comparaba `ListItem.ProductId`. Tres pruebas de identidad fallaron antes de la corrección (incluido el mensaje exacto comunicado); ahora la búsqueda usa `ListItem.Id`. No se altera React, la ruta HTTP, la base de datos ni los permisos. Las pruebas antiguas que pasaban ProductId se ajustan para usar el ID real del ítem y dejar de ocultar la discrepancia.

Regresión Release con `--no-restore`: Domain **38 passed**, Application **33 passed**, API **65 passed**, cero fallos/omitidas en las tres suites. Comandos: `dotnet test test/MiKompri.ShoppingList.{Domain,Application,Api}.Tests/MiKompri.ShoppingList.{Domain,Application,Api}.Tests.csproj --configuration Release --no-restore` ejecutados por proyecto. Nueva `PersonalPurchaseFlowTests` atraviesa HTTP crear → añadir → leer /mine → marcar con ItemId devuelto → releer estado/progreso (100%) → repetir marcado (204). Domain cubre que un ProductId cruzado con el Id de otro ítem no marca el producto equivocado; Application comprueba que un ID de catálogo rechazado no guarda cambios.

Evidencia API `.playwright-mcp/test-results/mark-item-regression.trx`. WebApplicationFactory y EF InMemory ejecutan el pipeline/API real con autenticación y Users controlados de pruebas; no es una validación nueva de Entra ni una escritura sobre datos personales del usuario. Persisten avisos previos de nulabilidad. El marcado compartido y el hardening T132 siguen pendientes: corregir la identidad no certifica autorización. No se repite la suite React al no modificar el cliente; la prueba nueva de API cubre la discrepancia que los dobles de red anteriores no detectaban.

Aplicación local de T133: `docker compose up -d --build --no-deps mikomprishoppinglistapi` completado; solo se recreó ShoppingList. Contenedor `abbf27c8b907` en estado **running/healthy**; GET `http://localhost:8080/health` devuelve **200**. Sin migraciones, eliminación de volúmenes, cambios de datos del usuario o despliegue cloud. La corrección sirve también para los ítems añadidos antes del arreglo: sus IDs no cambian.

Comandos exactos de las regresiones:

```powershell
dotnet test test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release --no-restore --logger 'console;verbosity=minimal'
dotnet test test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj --configuration Release --no-restore --logger 'console;verbosity=minimal'
dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release --no-restore --logger 'console;verbosity=minimal' --logger 'trx;LogFileName=mark-item-regression.trx' --results-directory .playwright-mcp/test-results
```
