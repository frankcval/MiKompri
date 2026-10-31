# Validación técnica 010-A — 31/10/2026

Código backend `de89da5`, Web/perfil `78b4a23` y corrección posterior AADSTS90023 en esta entrega. [Registro](implementation.md), [recuperación y aceptación humana](authentication-recovery-2026-10-31.md). No es cierre de 010-A.

## Resultados ejecutados

| Suite | Resultado |
|---|---|
| Users Domain / Application / API | 21 / 37 / 90 PASS |
| ShoppingList Domain / Application / API | 43 / 36 / 97 PASS |
| ProductCatalog Domain / Application / API | 22 / 21 / 39 PASS |
| Total .NET | 406 PASS, 0 fallos, 0 omitidos |
| PostgreSQL16 Docker | 4 PASS incluidos en Users API; concurrencia de provisión/username, reserva/cutover/rollback y negativos de origen/destino |
| Web unit/component/integration tras corrección | 138 PASS, 0 fallos/omitidos; auth49 incluidos |
| Web lint / typecheck / build | Exit0 |
| Playwright controlado antes de retirar hint | 120 PASS, 0 fallos/flaky/skips, Chromium/Firefox/WebKit; no sesiones IdP reales |
| Regresión portada tras guard de cuenta | 15 PASS, tres motores, 0 fallos/flaky/skips |
| Playwright tras retirar hint: portada y secciones/login | 60 PASS, Chromium/Firefox/WebKit; 0 fallos/flaky/skips |
| EF pending-model-changes | False, exit0 en las tres APIs |
| Docker APIs | Tres builds PASS con SDK8.0-noble; imagen SDK local predeterminada fallaba exec format antes de ejecutar código |
| Runner administrativo QA | Build PASS; no traslado real ejecutado |
| Docker Web tras retirar hint | Build PASS y configuración pública de compilación comprobada en assets; CI final pendiente |

Los TRX locales `test/*/TestResults/010a-final.trx` acreditan406. Los cuatro tests PostgreSQL no se omitieron: `MIKOMPRI_TEST_POSTGRES` apuntaba al contenedor exclusivo de pruebas. El verificador de federación se sustituye en esos cuatro tests; no son prueba del proveedor Microsoft real. No se alteraron presupuestos de rendimiento, retries ni timeouts de tests para lograr PASS.

## Reproducción

Desde la raíz, con PostgreSQL Docker QA aislado disponible; no usar bases originales/productivas:

```powershell
$env:MIKOMPRI_TEST_POSTGRES='Host=127.0.0.1;Port=55432;Database=postgres;Username=qa'
dotnet test MiKompri.sln --logger 'trx;LogFileName=010a-final.trx'
dotnet build tools/identity-migration
dotnet ef migrations has-pending-model-changes --project MiKompri.Users.Infrastructure --startup-project MiKompri.Users.Api
dotnet ef migrations has-pending-model-changes --project MiKompri.ShoppingList.Infrastructure --startup-project MiKompri.ShoppingList.Api
dotnet ef migrations has-pending-model-changes --project MiKompri.ProductCatalog.Infrastructure --startup-project MiKompri.ProductCatalog.Api
```

Las suites se ejecutaron por proyecto, con los resultados anteriores; el comando de solución permite reproducirlas conjuntamente. La factoría PostgreSQL crea/retira únicamente sus bases de test. La migración de schema Users se ejecutó en QA/tests, no en las bases previas.

Desde `MiKompri.Web`:

```powershell
npm test
npm run test:ci-config
npm run lint
npm run typecheck
npm run build
$env:PLAYWRIGHT_ALL_BROWSERS='1'
npm run test:e2e:smoke -- --workers=2 --reporter=line,html
```

Para reproducir solo el cambio de Google: `npx vitest run tests/unit/auth tests/component/auth tests/integration/auth`. La ejecución final138/138 fijó un tenant externo distinto al de fixtures antes de arrancar Vitest, y setup fijó el tenant sintético correcto. `.env.test` ignorado ya no es requisito oculto de CI. Resultados brutos se guardan en `.local/` ignorado, fuera de `test-results` que Playwright limpia.

## Cobertura de seguridad y límites

JWT: matriz HTTP firmada20 casos por API y17 controles criptográficos; pares issuer/tenant/audience/clientes/scopes, RS256/firma/exp/nbf/tid/oid y negativos Google/Graph/ID/ausencia/cruces. Tokens sintéticos firmados para tests; el negativo con token Google real contra las tres APIs normales sigue pendiente.

D1: pruebas HTTP y manuales QA históricas verificaron200→403 al suspender,401 tras reactivar con token anterior y503 con Users caído. ProductCatalog GET/mutaciones cubiertos; respuesta incompleta/HTTP201/202 no permiten acceso. Sin caché entre requests; resultado/UserId reutilizado dentro de una request. Medición local25 muestras: Users status p50/p95=8.2/12.4ms; catálogo con D1=14.1/24.4ms. No es comparación causal sin D1 ni SLO cloud. Un JWT obtenido tras reactivación conservaba iat anterior al corte; falta positivo real cuando iat sea estrictamente posterior. No se rebajó el corte ni se interpretó iat como G3.

Perfil: campos propios/versiones/conflictos/whitelist, clear y ausencia de sobrescritura por claims; username concurrente único. Sesión: cuenta activa, tenant ajeno, silent token tardío, frontera de generación, cancelación, remount de formulario privado, borradores y namespace por entorno/cliente/tenant/cuenta. Pruebas automatizadas no acreditan lector/zoom manual ni todos los cambios de proveedor reales.

## CI y cierre

Sobre `78b4a23`: Users, ShoppingList, ProductCatalog y Resource terminaron success; [Web falló por configuración de fixtures](https://github.com/frankcval/MiKompri/actions/runs/41194726785), corregida en esta entrega. El estado final debe consultarse para el SHA nuevo. Sonar sigue fuera. No se cierran T045/T055 ni los gates de aceptación basándose en resultados de un SHA anterior.

Bloqueos de aceptación: [traslado Microsoft y rollback](migration-rehearsal.md), negativo Google real contra tres APIs, recorridos actuales Microsoft/cambio de cuenta y perfil con relogin, positivo D1 tras corte, CI final y accesibilidad manual aplicable. Email T056 y 010-B no bloquean el núcleo por sí mismos.
