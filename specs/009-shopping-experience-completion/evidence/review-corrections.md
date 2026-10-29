# Revisión acotada de 009 — 2026-10-29

Rama `009-shopping-experience-completion`, base de revisión `13d119fd0e3d7f74fe88423a280913105a9fb3a0`, PR [#57](https://github.com/frankcval/MiKompri/pull/57). Sin funcionalidades nuevas. Constitución y spec/plan/tasks/evidencia revisados antes de cambiar código. SonarCloud fuera del alcance de esta revisión.

## Causas y correcciones

1. Spec Kit: `feature_directory` seguía en 008. Se usa el mismo formato JSON que `Save-FeatureJson` en `.specify/scripts/powershell/common.ps1`, ruta relativa 009 y UTF-8 sin BOM. `check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks` resuelve 009 y sus artefactos. No se modifica completitud 007/008.
2. T009: la nota de verificación pendiente se redactó antes de la CI del push/PR. El candidato 13d119f ya acredita cinco workflows, incluyendo Users por `pull_request` a main (aunque no por push a 009). Las correcciones de fechas se validan como candidato posterior; no se hereda automáticamente el éxito previo.
3. Fechas: formularios obtenían día UTC y el dominio consultaba directamente el reloj. ProductCatalog repetía validación UTC en aplicación y dominio, por lo que incluso un reloj de aplicación simulado no gobernaba toda la regla. Ahora dominio recibe hoy explícito; aplicación usa proveedor de fecha y API adapta reloj `TimeProvider` más offset del cliente. Web usa fecha local, actualiza hoy a medianoche/al recuperar foco y envía contexto tanto en compras personales como compartidas, correcciones y precios.
4. CI: los cinco workflows se ejecutan por PR #57 sobre el mismo SHA. No es necesario modificar sus triggers ni permisos. Se verificará el nuevo HEAD tras publicar las correcciones.

El primer candidato corregido `487f649` termina con Users, ShoppingList, ProductCatalog y Resource satisfactorios, pero Web deja 119/120 E2E (117/117 unitarios). En WebKit axe mide colores intermedios al alternar `appearance` de los dos botones de Añadir productos: contrastes 1.76/1.01 durante la transición Fluent de background/border/color. El test 009 ahora espera las animaciones finitas del diálogo mediante `Animation.finished` antes de auditar el estado estable. No cambia CSS, reglas axe, aserciones, retries ni timeouts. [Fallo reproducible](https://github.com/frankcval/MiKompri/actions/runs/40961462810), job `123755007517`; los seis escenarios nuevos de fecha sí pasaron.

Después del ajuste del test: 33/33 E2E afectados, tres motores, 1.3m (`009-review-e2e-settled.log`); el caso WebKit repetido cinco veces pasa 5/5, 12.7s (`009-review-webkit-settled-repeat.log`). Lint/typecheck repetidos correctamente (`009-review-settled-{lint,typecheck}.log`). No cambia el código de aplicación que acreditan las 208 pruebas .NET, 117 Web y comprobaciones EF anteriores.

## Semántica de hoy

Fechas elegidas se guardan `YYYY-MM-DD` sin conversión. Header opcional `X-Client-Utc-Offset-Minutes`, entero [-840,840], positivo al este de UTC. El servidor calcula hoy desde su instante y ese offset. No acepta un «hoy» ni un reloj proporcionados por el cliente. Header ausente conserva hoy UTC; header inválido presente devuelve 400. `HttpClient` puede omitir un header vacío: se prueba además el parser con header vacío/múltiple presente en `DefaultHttpContext`.

Ejemplo: reloj servidor 29/10 23:30 UTC y offset +60 → hoy 30/10; el 30 se acepta y el 31 se rechaza. Con UTC 30/10 00:30 y offset -120 → hoy 29/10; el 30 se rechaza como futuro. Hay pruebas HTTP con reloj fijado en ambos bounded contexts, alta/corrección personal/compartida e historial de precios. Los timestamps de auditoría siguen UTC. Contexto de fecha no afecta permisos, correlación TP11 ni huella de idempotencia.

Modelo y migraciones no cambian. EF verifica ambos contextos contra PostgreSQL QA aislado, sin usar datos de la aplicación. No se añaden zonas históricas, calendarios ni atributos de Users.

## Validación local

| Comprobación | Resultado acreditado | Evidencia local ignorada |
| --- | --- | --- |
| ShoppingList Domain/Application/API | 43/43, 36/36, 77/77; API PostgreSQL | `.playwright-mcp/009-review-dotnet-full.log`, TRX `009-review.trx` |
| ProductCatalog Domain/Application/API | 22/22, 21/21, 9/9; API PostgreSQL | Mismo log y TRX por proyecto |
| Fechas Web unitarias | 4/4, incluido cambio local de medianoche y recuperación de pestaña | `009-review-localdate-unit.log` |
| Fecha local en navegador | 6/6, tres motores; offsets +60/-120, precio, compra personal/compartida e historial | `009-review-calendar-e2e.log` |
| Regresión E2E afectada | 33/33, Chromium/Firefox/WebKit, 1.8m; fechas, compra, omisión/captura, Gastos e historial | `009-review-e2e-affected.log` |
| Suite Web completa | 117/117, 33 archivos, cero omitidas; repetición aislada 202.83s | `009-review-vitest-final.log` |
| Lint/typecheck | Correctos, sin advertencias finales | `009-review-release-{lint,typecheck}.log` |
| Build Web | Correcto (TypeScript + Vite) | `009-review-web-build.log` |
| Modelo EF | Ambos sin cambios pendientes | `009-review-ef-model.log` |

La primera suite Web completa dejó 116/117: timeout de 5s en recorrido de catálogo mientras había otras suites. Se conserva `009-review-vitest-full.log`; la repetición aislada no amplía timeout ni elimina aserciones. El primer montaje de pruebas HTTP de fechas envió por error GroupId al crear una lista personal y obtuvo 403; se corrige únicamente el fixture. No se cambia autorización. También se separa header vacío presente del que HttpClient omite.

Comandos principales: `dotnet test test/MiKompri.{ShoppingList,ProductCatalog}.{Domain,Application,Api}.Tests --configuration Release --no-restore` (por proyecto, ambas API con `MIKOMPRI_TEST_POSTGRES` exclusivamente QA); `node --max-old-space-size=1536 node_modules/vitest/vitest.mjs run --pool=threads --maxWorkers=1 --reporter=verbose`; `npm run lint`, `npm run typecheck`, `npm run build`. EF: `dotnet ef migrations has-pending-model-changes --project MiKompri.<contexto>.Infrastructure --startup-project MiKompri.<contexto>.Api --configuration Release --no-build`, conexiones limitadas a bases QA.

E2E desde MiKompri.Web: `npx playwright test tests/e2e/modernization/calendar-dates.spec.ts tests/e2e/modernization/completion.spec.ts --workers=2 --reporter=line`, con `PLAYWRIGHT_ALL_BROWSERS=1`. Son ejecuciones controladas, no aceptación Entra ni accesibilidad manual.

## Archivos por responsabilidad

- `.specify/feature.json`: selección activa.
- ProductCatalog: abstracción `IDateTimeProvider`, `SystemDateTimeProvider`, adaptador API `ClientDateTimeProvider`/registro en Program, handler de precio y `ProductPriceRecord`.
- ShoppingList: `ICurrentDateProvider`, `SystemCurrentDateProvider`, adaptador API `ClientCurrentDateProvider`/Program, handlers de compra personal/gasto compartido y `ItemExpenseRecord`.
- Web: `shared/dates/localDate.ts`, `useLocalToday.ts`, transporte en `shared/api/request.ts`/`expensesApi.ts`, fechas de `ExpenseForm` y `CatalogManagement`.
- Tests afectados de dominio/aplicación, HTTP `LocalCalendarDateTests` de ambos contextos, `localDate.spec.ts` y E2E `calendar-dates.spec.ts`.
- Solo documentación 009: spec, plan, contratos, modelo, tasks y evidencia. Sin cambios en workflows, permisos, identidad ni 007/008/010/011.

## CI de entrada

Todos sobre 13d119f, evento `pull_request`, satisfactorios:

| Workflow | Ejecución |
| --- | --- |
| Users | [40934426515](https://github.com/frankcval/MiKompri/actions/runs/40934426515) |
| ShoppingList | [40934426677](https://github.com/frankcval/MiKompri/actions/runs/40934426677) |
| ProductCatalog | [40934426469](https://github.com/frankcval/MiKompri/actions/runs/40934426469) |
| Web | [40934426493](https://github.com/frankcval/MiKompri/actions/runs/40934426493) |
| Resource authorization and persistence | [40934426477](https://github.com/frankcval/MiKompri/actions/runs/40934426477) |

## Estado de tareas durante la corrección

T009 abierta hasta verificar el nuevo candidato. T010/T011 abiertas, sin cambio de criterios: última serie 13d119f con LCP personal/compartida 2556/2604ms frente a 2500ms; faltan aceptación Entra multiusuario y accesibilidad manual indicadas en tasks/spec. Esta revisión no los acredita ni incluye SonarCloud en su recomendación.

Riesgo residual: consumidores que omiten el offset mantienen semántica UTC y deben enviarlo si sus fechas representan el día local. Offset es contexto declarado, acotado y no persistido; no prueba ubicación ni se usa como autorización.
