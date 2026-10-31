# Validación010-A — 31/10/2026

Código candidato **a9ca37d4f8130b2a29ad017a075839257942a5f6**, posterior a la auditoría inicial de46cf0ba. Los cambios posteriores de cierre son documentación/evidencia; el informe final identifica el SHA publicado y sus cinco workflows. No usar los resultados anteriores de406 como si fueran los de esta ejecución. No cierre global de010-A ni aceptación de producción.

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

**No se declara READY FOR PR:** faltan ensayo Microsoft federado/rollback con manifiestos y aceptación manual aplicable. Alta nueva para atributos y matriz de sesión completa conservan sus tareas abiertas. T056 y010-B no son bloqueos del núcleo por sí mismos. README raíz/Web actualizados, sin modificar completitud007/008/009.

## Estado local al terminar QA

`.env.local` Web restaurado byte a byte desde respaldo original (hash igual); configuración QA conservada exclusivamente en `.env.010a.local` ignorado, activable voluntariamente con npm run dev -- --mode010a. La Web normal vuelve al entorno anterior; fuentes CIAM siguen deshabilitadas por defecto. Se cierra sesión Google QA antes de restaurar. Harness loopback55412 detenido; build final0warnings/0errores después de liberar su DLL. APIs/volumen QA se conservan separados, Usershealth200/ProvisioningPaused=false, proveedor Microsoft experimental fuera del flujo. Nuevo destino federado preparado queda inventariado para diagnóstico/retirada antes del06/11; ningún usuario MiKompri se migra. Bases originales y datos reales intactos.
