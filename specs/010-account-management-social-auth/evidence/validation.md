# Validación010-A — 31/10/2026

Código candidato **a9ca37d4f8130b2a29ad017a075839257942a5f6**, posterior a la auditoría inicial de46cf0ba. Los cambios posteriores de cierre son documentación/evidencia; el informe final identifica el SHA publicado y sus cinco workflows. No usar los resultados anteriores de406 como si fueran los de esta ejecución. La decisión beta31/10 permite cierre funcional con los gates actualizados; no aceptación/despliegue de producción. Las condiciones de migración/QA ampliada previas se sustituyen por los diferidos explícitos al final.

## Ejecuciones del candidato

| Suite/control | Resultado observado |
|---|---|
| Users Domain/Application/API | 21/37/93 PASS |
| ShoppingList Domain/Application/API | 43/36/97 PASS |
| ProductCatalog Domain/Application/API | 22/21/39 PASS |
| Total solución .NET | **409 PASS**,0 fallos,0 omitidos; nueve TRX010a-closure-final |
| PostgreSQL16 aislado | **7 PASS**, incluidos en Users API; cuatro previos + reserva idempotente, fallo de origen antes de aplicar/retry y rollback que conserva nuevas escrituras/suspensión |
| Web unit/component/integration | Primera ejecución137PASS+1timeout5000ms en CatalogCreation; segunda ejecución completa **138/138PASS**, sin cambiar timeout/retry ni código de catálogo |
| Auth Web |49/49PASS incluidos; test primero falla con select_account para Google y pasa con prompt=login |
| Preflight CI config |6/6PASS |
| Lint/typecheck/build |Exit0 |
| Playwright Chromium/Firefox/WebKit | Completa: **119/120PASS**, un timeout del heading Balance por persona en contexto430px WebKit. Repetición aislada del mismo caso **1/1PASS**. Sin aumentar budgets ni retries. No se presenta la primera ejecución como120/120 |
| CI Web del código a9ca37d |success, incluido gate obligatorio de tres navegadores, build y Docker; [run41217508141](https://github.com/frankcval/MiKompri/actions/runs/41217508141) |
| EF pending-model-changes |Tres APIs exit0/No changes; sin nuevas migraciones/modelo EF en esta continuación |
| Docker Users/ShoppingList/ProductCatalog/Web |Cuatro builds exit0; imágenes locales010a-closure, sin sustituir contenedores originales |
| Runner administrativo QA |Build exit0; no traslado real ejecutado |
| Harness QA Release |0warnings/0errores. Un rebuild intermedio falló porque el proceso diagnóstico mantenía su DLL abierta; se detuvo únicamente ese proceso, después buildPASS. No defecto productivo |

Los warnings nullable backend preexistentes se conservan: no afirmar que toda la solución compila sin warnings. El fallo inicial Web de catálogo y el timeout aislado WebKit se conservan como variabilidad local; no se ajustan criterios de rendimiento ni se corrigen funcionalidades008/009 para ocultarlos. La CI del código candidato completó sus gates sin esos fallos.

## Reproducción local

Desde raíz, PostgreSQL QA Docker16 disponible en55432; no usar las bases anteriores/productivas:

```powershell
$env:MIKOMPRI_TEST_POSTGRES='Host=127.0.0.1;Port=55432;Database=postgres;Username=qa'
dotnet test MiKompri.sln --logger 'trx;LogFileName=010a-closure-final.trx'
dotnet build tools/identity-migration
dotnet build tools/spikes/010-external-id/ExternalIdSpike.csproj --configuration Release
dotnet ef migrations has-pending-model-changes --project MiKompri.Users.Infrastructure --startup-project MiKompri.Users.Api
dotnet ef migrations has-pending-model-changes --project MiKompri.ShoppingList.Infrastructure --startup-project MiKompri.ShoppingList.Api
dotnet ef migrations has-pending-model-changes --project MiKompri.ProductCatalog.Infrastructure --startup-project MiKompri.ProductCatalog.Api
```

El verificador Graph de los siete tests PostgreSQL se sustituye; persistencia/índices/transacciones son reales, no equivalencia del proveedor Microsoft. La factoría crea/retira solo sus bases de prueba. Ninguna migración se ejecutó en las bases originales.

Desde MiKompri.Web:

```powershell
npm test -- --reporter=json --outputFile=../tools/spikes/010-external-id/.local/closure-web-rerun.json
npm run test:ci-config
npm run lint
npm run typecheck
npm run build
$env:PLAYWRIGHT_BROWSERS_PATH='../.playwright-browsers'
$env:PLAYWRIGHT_ALL_BROWSERS='1'
npm run test:e2e:smoke -- --workers=2 --reporter=json
# Repetición que resolvió el único fallo observado:
npm run test:e2e:smoke -- --project=webkit --grep 'preparar y comprar con contexto 430px' --workers=1 --reporter=json
```

Reportes privados closure-web*.json,closure-e2e*.json,TRX y logs quedan ignorados. Vitest fija tenant sintético explícito, sin depender de.env.test privado. Los escenarios controlados no autentican proveedores reales. No ejecutar build de harness sobre la DLL que esté sirviendo el diagnóstico en Windows.

Docker, desde raíz: docker build -t mikompri.<context>.api:010a-closure --build-arg SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0-noble -f MiKompri.<Context>.Api/Dockerfile . para Users/ShoppingList/ProductCatalog; docker build -t mikompri.web:010a-closure -f MiKompri.Web/Dockerfile . . El SDK local predeterminado había fallado exec format antes de ejecutar código; no se cambia Dockerfile ni ese argumento en CI. CI Linux usa su configuración normal.

## Resultados reales y límites

- [Tres APIs y perfil](real-provider-acceptance.md): Google sin Microsoft, Microsoft legacy, access token CIAM200; ID token CIAM401. Dos relogin Google conservan nombre/username/nombres/idioma/orden y posteriormente clear explícito. Sin backfill por Graph.
- [Seguridad](security.md): token Google directo y Graph reales401 en tres APIs normales, [D1 real200→403→401→200](d1-real-cutoff.json), consumidores503 con Users caído. Ningún corte/reloj/skew se reduce. La cuenta termina activa y Users saludable.
- [Migración Microsoft](migration-rehearsal.md): **INCONCLUSIVE**, colisión CIAM después de origen autorizado. No Reserve/Apply/Rollback, no nuevo User sustituto, no manifestación ficticia de conservación de referencias. Proveedor experimental retirado del flujo y provisioningfalse comprobados.
- UI real de login a320/390/430: scrollWidth igual a viewport, botones Google/Microsoft de44px de alto, primerTab enfoca enlace visible. Google primero y selector explicado. Suites cubren errores/loading/cancelación/return intent; lector real y zoom manual **no acreditados**. T053 sigue abierta.
- D1 local previo25 muestras: Users p50/p95=8.2/12.4ms; catálogo conD1=14.1/24.4ms. Histórica local, sin SLO cloud ni comparación causal; no cambia budgets.

## Revisión de seguridad/artefactos

Escaneo de67 archivos versionables010/ADR y los nuevos JSON: sin JWT completos ni email personal detectado. No archivos.env.local/.local/TestResults/test-results/Playwright privados versionados. gitignore y dockerignore excluyen esos artefactos; los logs/TRX/traces locales no se publican. Revisión del diff de código confirma solo configuración SPA pública, ninguna credencial o nuevo secreto. No se elimina evidencia antigua por nombres de campos. Enlaces locales de Markdown modificados sin rotos,58 tareas/58IDs únicos, git diff --check y Spec Kit -Json -RequireTasks -IncludeTasks exit0/feature010.

## CI remota por SHA

Los cinco workflows de **a9ca37d4f8130b2a29ad017a075839257942a5f6** finalizaron success:

| Workflow | Ejecución |
|---|---|
| Users |[41217508075](https://github.com/frankcval/MiKompri/actions/runs/41217508075)|
| ShoppingList |[41217508156](https://github.com/frankcval/MiKompri/actions/runs/41217508156)|
| ProductCatalog |[41217508089](https://github.com/frankcval/MiKompri/actions/runs/41217508089)|
| Resource authorization/persistence |[41217508002](https://github.com/frankcval/MiKompri/actions/runs/41217508002)|
| Web |[41217508141](https://github.com/frankcval/MiKompri/actions/runs/41217508141)|

Después de publicar el commit documental se reconsulta su SHA exacto; su estado se entrega con el cierre al titular. Los PASS anteriores no certifican automáticamente ese HEAD. Sonar fuera de alcance.

**Decisión vigente beta31/10:** ensayo Microsoft/rollback/manifiestos = DEFERRED MICROSOFT MIGRATION, no gate. Lector/zoom completos, repetición de alta para atributos y matriz manual exhaustiva = DEFERRED NON-BLOCKING QA. Sus tareas permanecen abiertas sin fingir PASS. T056 y010-B no bloquean. Gates funcionales beta acreditados; READY depende de cinco workflows success del SHA exacto final y PR. README raíz/Web actualizados, sin modificar completitud007/008/009.

## Estado local al terminar QA

`.env.local` Web restaurado byte a byte desde respaldo original (hash igual); configuración QA conservada exclusivamente en `.env.010a.local` ignorado, activable voluntariamente con npm run dev -- --mode010a. La Web normal vuelve al entorno anterior; fuentes CIAM siguen deshabilitadas por defecto. Se cierra sesión Google QA antes de restaurar. Harness loopback55412 detenido; build final0warnings/0errores después de liberar su DLL. APIs/volumen QA se conservan separados, Usershealth200/ProvisioningPaused=false, proveedor Microsoft experimental fuera del flujo. Nuevo destino federado preparado queda inventariado para diagnóstico/retirada antes del06/11; ningún usuario MiKompri se migra. Bases originales y datos reales intactos.

## Corrección posterior del entorno local

La restauración descrita arriba se revierte tras reproducir la regresión comunicada por el titular: Google oculto y sync Microsoft401 en la API anterior, frente a200 en la API010-A con el mismo token. Ahora se deja el entorno QA compatible activo y ambos botones disponibles; Microsoft vuelve a listas y Google da sync/perfil/listas/catálogo200. npm run dev:010a carga el archivo QA privado explícitamente. Auth49/49 y lint/typecheck/build pasan; sin cambios de código productivo, trust, schema ni traslados en las bases anteriores. [Diagnóstico y estado vigente](authentication-recovery-2026-10-31.md#regresión-de-entorno-al-finalizar-qa-y-corrección--31102026). No se reaplica la restauración automática que causó el fallo.

## Cierre beta aprobado31/10 y trazabilidad del candidato

Nueva frontera: Google sin Microsoft mediante CIAM, Microsoft legacy conservado, UserId/JWT/APIs/perfil/sesión/D1 y cero secretos. A-AC06/07/T019/T048 no bloquean; INCONCLUSIVE de migración no se reinterpreta. T053/059/060 son QA posterior, emailT056 opcional,010-B e invitaciones012 fuera.

La auditoría remota base6dd53e0080bd7547ce167752883e56f6430e9ebc coincide con local (0/0) y main...HEAD=0/12. CI base Users41220960524,ShoppingList41220960483,ProductCatalog41220960535,Resource41220960494,Web41220960500 finalizó success. **No se usa esa base como CI del commit documental final**: se consulta el SHA publicado exacto y se registra junto al PR/informe. PR listo solo después de cinco success; sin merge/deploy.

Esta ejecución modifica solo Markdown. Árboles git de runtime idénticos entre validadoa9ca37d y6dd53e0, y comprobados de nuevo frente al final; pruebas previas409/PG7/Web138/auth49, Docker y EF siguen siendo de ese código. La CI final ejecuta las suites de nuevo. Por ello no se repite la mutación destructiva D1 ni se simulan nuevos cutoffs.

| Árbol relevante | Git tree SHA compartido a9ca37d/6dd53e0 |
|---|---|
| MiKompri.Identity | `a4913e961f4f17baf63bf49b6dce79d9488faa4b` |
| MiKompri.Users.Api | `03edf285154779beadd23653f1df408e262121fa` |
| MiKompri.Users.Application | `2018f805e5a1d1dbe7d6b1757d7e14fd5223350a` |
| MiKompri.Users.Domain | `ef4847c25a95c9dfcf3777168f8c84485280daea` |
| MiKompri.Users.Infrastructure | `e0cf8e38eff764ac1f3fb02b2f33dba6b1eef96a` |
| MiKompri.ShoppingList.Api | `81ec23f8b36a31fa3c7886eb9ea4ce80348659c6` |
| MiKompri.ShoppingList.Application | `47d658b9f93d9b9a71eba92f1be6d8c244b5e80c` |
| MiKompri.ShoppingList.Domain | `9501cbc9d729ccd69a63b9d2e535fbf672d6c822` |
| MiKompri.ShoppingList.Infrastructure | `1bb6f40766ce392d240d88385f61a915d8d551d6` |
| MiKompri.ProductCatalog.Api | `e4f6b2cc962c7a1acc36624dc3988be86d58e7ea` |
| MiKompri.ProductCatalog.Application | `9125208723ebd611fa79fd326d1e2158c1b49309` |
| MiKompri.ProductCatalog.Domain | `1710a195d87502228dab0b4011ba75982ffc7fcf` |
| MiKompri.ProductCatalog.Infrastructure | `8102c70ba10a10dfe6ef49a330a67dee93f66b6e` |
| MiKompri.Web/src | `a7d9201a45cabdd2c5d3b7194b5d407fff60ccd8` |

Revalidación normal actual: Google→selector CIAM→GoogleSSO→Web, tres APIs200; ID token CIAM401 en tres APIs; mismo Google UserId, distinto de Microsoft, personalización/clear conservados. Microsoft legacy y ambos cambios reales acreditados en el mismo árbol; los controles tardíos/TanStack/drafts/activa/tenant están ejecutados en suites. Alta Google adicional con otro tester no disponible se difiere justificadamenteT059; alta nueva Web existente suficiente beta según real-provider-acceptance.md.

UX básico actual:320/390/430 sin overflow; dos botones44px y Tab alcanza Google/Microsoft/enlaces. Errores/cancelación/loading/return intent en tests. T053 lector/zoom y T060 manual multi-tab no ejecutados, riesgos no bloqueantes aceptados; no modificar gates007/008/009.

Revisión final ejecutada: Spec Kit -Json -RequireTasks -IncludeTasks y -PathsOnly exit0/feature010;60 tareas únicas (58 originales conservadas),38 A cerradas,26FR/ochoSC/seis historias;249 enlaces locales/anchors sin fallos;69 archivos010/ADR/README sin JWT completos, credencial Google/clave privada ni email personal, cero artefactos privados versionados; git diff --check exit0. TP11, código productivo, APIs/BD/configuración de identidad y entorno QA funcional permanecen. No restauración automática de.env.local. Historial de pruebas/fallos previos se conserva. [Checklist beta](../checklists/acceptance.md), [tareas](../tasks.md), [decisión](010-a-scope-review.md).
