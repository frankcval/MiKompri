# Evidencia 009 — 2026-10-28/29

Revisión más reciente: 274/274 .NET sin cambios backend posteriores; 113/113 componentes finales (`009-persistent-dialog-vitest.log`, 195.68s); 6/6 carga lenta/cancelación en tres motores (`009-persistent-dialog-cancel.log`). Matriz completa de 114 escenarios, Docker y rendimiento de este último candidato todavía en verificación. T009 se mantiene abierto hasta completarlos; T010/T011 conservan sus gates.

Base `main`/`origin/main` 77ddbfc (PR #56 integrado), rama `009-shopping-experience-completion`. El alcance y la implementación fueron autorizados en conversación; spec/plan/tasks/modelo/contratos se crearon antes de código. El usuario autoriza el 29 la verificación final, documentación, commit y push. No se modifican casillas de 002–008. 010/011 contienen solo alcance.

## Resultado funcional

Estimación unitaria × cantidad y cobertura desconocida; filtros Activas/Historial y orden por creación/modificación/nombre en los tres índices. Completada no equivale a Closed; desmarcar reactiva sin tocar gastos. Lista vacía activa. Borrado usa confirmación y protege historia financiera.

Selección directa desde resultados de búsqueda, sin segundo desplegable; alta contextual con nombre precompletado y saga existente de creación/incorporación. Catálogo conecta search/PUT, mercados conecta POST/PUT/desactivación, registro manual de precios y filtros de historial existentes. Se distinguen selección pendiente/carga/vacío/error HTTP/red; se utiliza marketName/currency del backend incluso en mercados inactivos. No se publica una compra automáticamente como observación de precio.

Captura opcional tras marcado, sin revertir compra al omitir. Personales y compartidas usan el mismo ItemExpenseRecord que Gastos. Se añaden mercado, fecha real nullable separada de auditoría, versión y recibo de operación duradero. Servidor deriva actores personales; shared mantiene roles y participantes activos. Reintento exacto conserva ID; payload distinto con mismo ID o corrección con versión antigua devuelve 409. Recibo sin cascade impide recrear un gasto borrado por replay. Campos shared son aditivos para clientes legacy; la nueva Web envía operación/versión. Múltiples compras distintas por ítem siguen siendo explícitamente posibles.

## Pruebas acreditadas

Windows/PowerShell, Node 24.22.1, npm 11.19.1, SDK .NET 10.0.402 compilando net8.0, PostgreSQL 16 en contenedor propio `mikompri-qa-009`, separado de servicios/volúmenes del usuario. Factories crean/eliminan bases aleatorias. Autenticación y Users controlados: no equivalen a Entra real.

| Comprobación | Resultado | Salida local ignorada |
| --- | --- | --- |
| Web unit/component/integration | 113/113, 32 archivos, cero omitidas | `.playwright-mcp/009-vitest-final.log` |
| Tipos y lint | Correctos, sin advertencias finales | Invocaciones npm run typecheck/lint 28 y 29 octubre |
| Build Web | Correcto, entrada JS ~371 kB, ~97 kB gzip | `.playwright-mcp/009-build.log` |
| ShoppingList Domain | 42/42 | Salida dotnet test en conversación |
| ShoppingList Application | 35/35 | Salida dotnet test en conversación |
| ShoppingList API PostgreSQL completa | 74/74 | `.playwright-mcp/009-shopping-api-postgres-full.log` |
| ShoppingList API 009 repetida el 29 | 6/6 PostgreSQL, incluida concurrencia | `.playwright-mcp/009-resume-api.log` |
| ProductCatalog Application | 20/20 | `.playwright-mcp/009-product-app.log` |
| ProductCatalog Domain | 21/21 | Salida dotnet test 29 octubre |
| ProductCatalog API PostgreSQL | 7/7 | `.playwright-mcp/009-product-api-postgres-final.log` |
| Modelo EF | Sin cambios pendientes tras migración | dotnet ef migrations has-pending-model-changes |
| Playwright inventario completo, 3 motores | 101/105 inicial; fallos conservados abajo | `.playwright-mcp/009-browser-all.log` |
| Repetición de familias afectadas, 3 motores | 33/33 tras corrección; complementa los otros 72 escenarios ya satisfactorios | `.playwright-mcp/009-resume-browsers.log` |
| Regresión tras resolver captura diferida | 18/18 E2E en tres motores y 17/17 componente personal | `.playwright-mcp/009-resume-capture-regression.log`, `009-resume-personal-tests.log` |
| Captura compartida adicional | 3/3 en tres motores: participantes explícitos, mismo gasto, aviso al volver a marcar y ningún POST adicional | `.playwright-mcp/009-shared-capture.log` |
| Docker ShoppingList | Correcto con SDK oficial 8.0-noble | `.playwright-mcp/009-docker-shopping.log` |
| Docker Web | Correcto | `.playwright-mcp/009-docker-web.log` |
| Docker ProductCatalog | Correcto con SDK oficial 8.0-noble | `.playwright-mcp/009-resume-docker-product.log` |
| Builds finales tras revisión | ShoppingList y Web correctos | `.playwright-mcp/009-resume-docker-{shopping,web}.log` |
| Workflows | 4 archivos correctos con actionlint | Invocación del 29 octubre |
| Runtime Docker/OpenAPI | ShoppingList QA health 200, rutas y campos nuevos enumerados | Consulta pública Swagger sobre contenedor QA, sin credenciales de usuario |

Los 105 escenarios distintos están cubiertos mediante selecciones complementarias; no se afirma un único run 105/105. La repetición verifica las cuatro resoluciones de compra 009 y cinco de portada por motor, selector directo/alta contextual/axe y edición de producto/mercado/historial. Capturas 009 a 390px revisadas visualmente, sin scroll horizontal: `MiKompri.Web/test-results/009-{lists,inline,catalog}-390.png`. Foco/teclado automatizados no sustituyen lector o dispositivo físico.

Los tres casos adicionales de compra compartida amplían el inventario a 108 escenarios distintos cubiertos por selecciones complementarias, con `009-shared-capture-390.png` revisada. Runtime final en contenedores QA: Swagger de POST personal declara 201/400/401/403/409; health ShoppingList 200; Web `/lists` 200 y `/tests/visual/index.html` 404. Ningún harness se publica en la imagen Web. Las suites se ejecutaron sobre código real con red/identidad controlada; no sobre una simulación separada de la UI.

Comandos principales:

```powershell
node --max-old-space-size=768 node_modules/vitest/vitest.mjs run --pool=threads --maxWorkers=1
npm.cmd run typecheck
npm.cmd run lint
npm.cmd run build
# MIKOMPRI_TEST_POSTGRES apunta exclusivamente al PostgreSQL QA aislado.
dotnet test test/MiKompri.ShoppingList.Api.Tests --configuration Release --no-restore
dotnet test test/MiKompri.ProductCatalog.Api.Tests --configuration Release --no-restore
# En MiKompri.Web, con PLAYWRIGHT_BROWSERS_PATH y PLAYWRIGHT_ALL_BROWSERS=1:
npx.cmd playwright test --workers=2 --reporter=line
npx.cmd playwright test tests/e2e/modernization/completion.spec.ts tests/e2e/home/home.spec.ts --workers=2 --reporter=line
```

## Fallos encontrados y tratamiento

- Tests nuevos de dominio inicialmente no compilaban por capacidades ausentes; se conserva como tal, no se afirma una prueba de comportamiento fallida previa.
- Selecciones antiguas de producto y payloads de test se adaptaron al nuevo contrato/UX preservando aserciones de IDs, errores, reintento y doble envío. Primera suite Web: 20 fallos; segunda: uno; final 113/113.
- Vitest con pool forks no produjo resultado y se interrumpió; threads completó. No modificar CI para ocultar el intento.
- Factory ProductCatalog generaba un nombre de DB por contexto: no permitía probar una escritura seguida de lectura. Ahora nombre único por factory y opción PostgreSQL. Nuevos hosts simultáneos expusieron `The logger is already frozen` del bootstrap Serilog estático: suites de API serializadas, como Users/ShoppingList; concurrencia HTTP dentro del host sigue probada.
- Primera selección navegador 009 usaba check/uncheck con estado controlado y persistencia asíncrona: se usa click y se espera el resultado, sin introducir éxito optimista.
- Run de tres motores detectó tres lecturas prematuras del router MemoryRouter en WebKit. El test espera `sort=oldest` antes de reabrir la ruta; conserva comprobación de valor tras recrear navegador/ruta de harness. La SPA real usa BrowserRouter; este test no acredita por sí solo persistencia con Entra.
- Firefox midió un objetivo de portada de 43.999755859375px. Se elevó el mínimo CSS a 45px, sin rebajar el umbral 44px del test.
- SDK Docker 8.0 de ProductCatalog falló con `exec /bin/sh: exec format error`, antes de restore. Se añade argumento SDK_IMAGE igual que ShoppingList, conserva default/runtime y compila con imagen oficial 8.0-noble. Sin reiniciar Docker ni tocar volúmenes del usuario.
- La primera serie de rendimiento quedó incompleta tras 40 navegaciones: la captura opcional diferida podía abrirse después del diálogo Añadir. Se bloquea Añadir mientras se guarda/carga captura y se permite cancelar durante carga. Los 18 casos 009 pasan en tres motores tras este cambio. El script espera/cancela captura y vacía eventos de cancelación antes de medir Añadir; no se mezclan muestras de la serie fallida con la repetición.
- actionlint detectó un byte no UTF-8 heredado en el workflow ShoppingList. Se normalizó su codificación conservando el contenido; los cuatro workflows modificados pasan actionlint sin desactivar validación YAML.

## Pendientes explícitos

Medición de laboratorio completa: [80 muestras y resultados íntegros](performance.md). LCP p75 portada/índices/personal/compartida: 1972/2600/2764/2712 ms. Marcado 200 ms, Añadir 208 ms y Gastos 64 ms; CLS cumple. SC-006/T010 permanecen abiertos por cargas >2500 ms y Añadir >200 ms. No se atribuyen pequeñas diferencias respecto a 008 a una causa concreta ni se cierra su SC-008.

Entra de dos cuentas, lector de pantalla/zoom/teclado virtual/dispositivo físico y aceptación humana conservan gate T011. El piloto posterior con una cuenta real se documenta abajo; no sustituye segunda cuenta ni los otros criterios. CI remoto y SonarCloud requieren comprobar el commit publicado; el merge anterior no certifica su cierre. La validación de scopes de las APIs heredadas no se amplía ni se certifica por las pruebas de recurso de 009. No se declara cerrado 007/008/009.

Preflight integrado ejecutado el 29: faltan `MIKOMPRI_INTEGRATED_URL`, `MIKOMPRI_OWNER_TOKEN`, `MIKOMPRI_MEMBER_TOKEN`, `MIKOMPRI_OWNER_STORAGE` y `MIKOMPRI_MEMBER_STORAGE`. Se registra la ausencia de prerrequisitos, sin ejecutar ni aprobar la suite Entra. No se leen/exportan credenciales del navegador ni se usan datos personales como fixtures.

## Actualización del entorno local — 29 octubre

Se confirma de nuevo PR #56 `merged=true` por GitHub y `origin/main` 77ddbfc tras fetch. El Swagger local anterior de ShoppingList no exponía compras personales: causa del 404 genérico «Este recurso ya no está disponible» en Datos de compra. No significaba que el ítem marcado se hubiera eliminado.

Antes de actualizar se guarda un `pg_dump -Fc` de ShoppingList (15,536 bytes, índice legible con pg_restore) en `.playwright-mcp/backups/mikompri-before-009-shopping.dump`, ignorado por Git. Se conservan imágenes anteriores con tag `before009-20261029`. Build de ambos servicios con SDK 8.0-noble y `docker compose up -d --no-deps mikomprishoppinglistapi mikompriproductcatalogapi`; sin borrar volúmenes. ShoppingList y ProductCatalog saludables, Users/PostgreSQL saludables y conservados. Consulta de `__EFMigrationsHistory` confirma `20261028213746_AddPurchaseDetailsAndOperations`; Swagger 8080 enumera GET/POST/PATCH/DELETE personales. Las suites de API usan exclusivamente bases QA aisladas; el piloto de navegador posterior se limita a una lista sintética identificada.

La revisión de cierre difiere formularios y menú cerrado según [plan](../plan.md). La prueba de creación espera ahora la aparición asíncrona del diálogo, manteniendo las aserciones de validación/reintento/doble envío. Un intento de build de laboratorio apuntó a una ruta de configuración inexistente y se repitió con `tests/visual/vite.performance.config.ts`; no es un build satisfactorio ni una modificación de gates.

## Piloto real con la sesión facilitada por el usuario

El 29 se autorizó utilizar la sesión abierta de `http://localhost:5173/`. Se accedió por navegador sin exportar credenciales ni simular las API. Se creó exclusivamente «QA 009 cierre 2026-10-29», con un producto existente, cantidad 2 y estimación unitaria 1.25 EUR: resumen 2.50 EUR. El marcado abrió Datos de compra sin 404; se registraron 2.30 EUR totales, mercado existente y fecha 2026-10-29. Gastos mostró el mismo importe, fecha y mercado.

La lista completada desapareció de Activas y siguió accesible en Historial. Tras recarga, desmarcar la devolvió a Activas y conservó el gasto. Volver a marcar mostró «ya tiene 1 registro», sin registrar otra compra. Se detectó foco fuera del diálogo durante apertura asíncrona: se corrige foco inicial y se conserva el invocador para restauración, incluyendo cancelación durante carga.

Este piloto acredita una cuenta real y una lista personal; no acredita dos cuentas/roles compartidos, lector de pantalla ni dispositivo físico. Tras la interrupción comunicada por falta de disco, la sesión controlable redirige a `/login`; el usuario vuelve a iniciar sesión manualmente. No se automatiza el login ni se extraen tokens.

Con la sesión recuperada se verifica que intentar borrar la lista con gasto produce conflicto y conserva ambos. La confirmación se amplía para incluir el nombre y explicar la protección de historial también en personales. Se elimina solo el gasto sintético, se cancela primero el borrado de lista comprobando que sigue disponible y después se confirma su eliminación. La lista y gasto QA quedan retirados; el catálogo existente y las otras listas se conservan. Navegador devuelto a `/lists`. Un intento previo de cancelación de diálogo terminó en timeout porque la recarga de desarrollo había cerrado ese diálogo; se reobservó el estado antes de continuar.

## Reanudación tras falta de espacio

El usuario comunica que ha liberado disco. Se verifican C: ~11.1 GB y E: ~1.5 GB libres, servicios saludables, rama y cambios conservados. Aún sin commit/push al reanudar. No se atribuye a disco una causa técnica no demostrada: intentos Vitest anteriores tuvieron timeout, interrupción y fallo de memoria de V8; se preservan logs. Selección diferida revertida según el plan. La primera regresión completa dejó 86/108 correctos y 22 fallos, principalmente de foco; no se presenta como satisfactoria.

Repetición secuencial Web con heap 1536MB y un worker threads: **113/113, 32 archivos, cero omitidas**, 99.71s (`.playwright-mcp/009-resumed-vitest.log`). Typecheck y lint sin advertencias (`009-resumed-{typecheck,lint}-final.log`); preflight CI **6/6** (`009-resumed-ci-config.log`). Nueve suites .NET de la revisión: **274/274**, cero omitidas, con ambas API sobre PostgreSQL QA (`009-close-dotnet.log` y TRX `009-close.trx` por proyecto). No hay cambios backend posteriores a esa ejecución.

La repetición con menú diferido volvió a detectar el primer Tab fuera de «Saltar al contenido» en WebKit y se interrumpió (`009-resumed-browsers.log`); Chromium y Firefox completaron sus 36 escenarios cada uno. Se retira ese experimento conservando el Drawer original. Comprobación específica posterior de portada WebKit **5/5** (`009-resumed-webkit-menu.log`). Se mantiene toda aserción de foco y se prepara regresión completa del candidato, incluida la precarga 700 existente.

Regresión completa posterior: **108/108, cero omitidas**, Chromium/Firefox/WebKit, 2 workers, 6.9 minutos (`.playwright-mcp/009-final-browsers.log`), incluyendo captura compartida y foco de apertura/restauración. Es una ejecución completa satisfactoria anterior a diferir Añadir, no una suma de selecciones. Captura compartida a 390px revisada visualmente de nuevo. `npm run build` (incluye TypeScript) y lint correctos (`009-final-{build,lint}.log`); build de laboratorio separado correcto (`009-final-performance-build.log`).

El perfil de esa revisión encuentra validación de Añadir en la ruta crítica; se difiere el diálogo completo, con fallback modal cancelable y selector síncrono dentro. Componentes **113/113** (`009-lazy-add-vitest-final.log`, 82.88s). Primer test nuevo de objetivo táctil midió 43.38px durante animación de entrada: se espera la geometría >=44px mediante poll, sin rebajar umbral. La regresión siguiente detecta cancelación perdida durante sustitución del fallback de compra por el formulario; se conserva la ventana y el botón en `PurchaseCaptureDialog`, difiriendo solo contenido financiero. No se cambia el test anterior para esperar artificialmente al formulario antes de cancelar.

Pruebas de importación retenida: cancelación de Añadir sin reapertura y control «Ahora no» con foco estable antes/después de cargar. Primer pase 5/6 detecta retorno de foco en WebKit con puntero; se enfoca el checkbox al activar, antes de registrar el invocador. Repetición **6/6** en tres motores (`009-stable-load-cancel-final.log`). Se preservan sus fallos previos en `009-stable-load-cancel.log` y la regresión intermedia interrumpida en `009-release-browsers.log`. La suite final añade seis casos al inventario previo: 114 escenarios.
