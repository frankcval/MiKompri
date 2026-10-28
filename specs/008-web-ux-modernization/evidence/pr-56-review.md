# Revisión de PR #56 — 2026-10-28

Revisión sobre `007-web-react-client`, HEAD local y remoto `07c07e1c0b4b9b954e50229d6b5a04e033b44c5e`, base del PR `917f031d98be67e73da4910997a5c8b58a06c468` (`main`). Árbol inicialmente limpio. Estado remoto comprobado a las 21:44 Europe/Madrid. Esta revisión solo añade documentación; no cambia código, tests, workflows, migraciones, commits ni casillas de completitud. No se hace commit, push, merge ni despliegue.

Publicación posterior: el usuario solicita incorporar este informe y las notas de tareas en un commit de la misma rama y subirlo al remoto. Los resultados de CI siguientes siguen referidos al SHA auditado `07c07e1`; no certifican el nuevo commit documental.

## Users: causa y verificación

El fallo comunicado pertenece al padre `bd444ac238b90d9c079e6578a0fd9c25a0d2924e`: [Users #44](https://github.com/frankcval/MiKompri/actions/runs/40761761405/job/123075238451). El log confirma `GetMe_WithoutTidOidCorrelation_Returns401`: esperado Unauthorized, recibido NotFound; API 26 correctas/1 fallida, Docker omitido por el fallo previo.

El middleware antiguo continuaba con `_next` si no podía resolver identidad. Sin aprovisionamiento, `HttpCurrentUserService.UserId` devuelve `Guid.Empty`; `GetMyProfileQueryHandler` no encuentra perfil y lanza `KeyNotFoundException`, traducida a 404. Además, resolvía `oid` desde `sub`/NameIdentifier y `tid` desde issuer: podía aprovisionar con una identidad que no incluía el par canónico.

TP11 y 007/FR-005 exigen `(tid, oid)`. Para un principal autenticado sin ese par corresponde rechazo 401 antes de aprovisionar, no continuar hasta una lectura de perfil. Los alias URI de tenantid/objectidentifier son representaciones del mismo claim; `sub`/NameIdentifier no son alias de `oid`.

**La corrección ya está en HEAD**, commit `07c07e1`: exige ambos claims, elimina los fallbacks y lanza UnauthorizedAccessException; el middleware global la traduce a 401 con mensaje genérico y traceId. `sub` sigue separado como metadato/soporte transicional documentado en el handler, sin sustituir `oid`. No se elimina la migración de cuentas legacy ni se cambia esa política en esta revisión.

Los tests actuales cubren ausencia de ambos claims y de cada uno, `tid + sub` insuficiente, `iss + oid` insuficiente, cero usuarios creados al rechazar, y éxito con claims canónicos o alias URI. Se conserva la expectativa 401. No corresponde duplicar la corrección ni relajar tests.

## Pruebas ejecutadas en esta revisión

Windows/PowerShell, SDK .NET 10.0.402, proyectos net8.0, configuración Release. Desde la raíz, por este orden:

```powershell
dotnet test test/MiKompri.Users.Domain.Tests/MiKompri.Users.Domain.Tests.csproj --configuration Release --logger 'console;verbosity=minimal'
dotnet test test/MiKompri.Users.Application.Tests/MiKompri.Users.Application.Tests.csproj --configuration Release --logger 'console;verbosity=minimal'
dotnet test test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj --configuration Release --logger 'console;verbosity=minimal'
```

| Suite | Correctas | Fallidas | Omitidas |
| --- | ---: | ---: | ---: |
| Users.Domain | 12 | 0 | 0 |
| Users.Application | 32 | 0 | 0 |
| Users.Api | 31 | 0 | 0 |

Restore y compilación incluidos. Advertencias de nulabilidad existentes; no impiden el resultado. API usa WebApplicationFactory, autenticación controlada y EF InMemory; no se presenta como Entra real. El job remoto con .NET 8 confirma los mismos 12/32/31 y Docker correcto. No se repiten localmente frontend, ShoppingList o ProductCatalog: no se ha cambiado implementación; su resultado remoto está debajo.

## CI completo del commit auditado

Se consultaron runs por SHA y todos los check-runs de HEAD mediante GitHub API; los statuses clásicos estaban vacíos y no bastaban para describir CI. Se incluyó SonarCloud, que no aparece en la lista de workflows de Actions.

| Comprobación | Estado final | Evidencia |
| --- | --- | --- |
| Web #19 | SUCCESS | [Run 40764713101](https://github.com/frankcval/MiKompri/actions/runs/40764713101): configuración, lint, tipos, tests, tres motores controlados, build y Docker |
| ShoppingList #119 | SUCCESS | [Run 40764713043](https://github.com/frankcval/MiKompri/actions/runs/40764713043): build/test e integración |
| ProductCatalog #29 | SUCCESS | [Run 40764713052](https://github.com/frankcval/MiKompri/actions/runs/40764713052): build/test, Docker e integración |
| Resource authorization/persistence #7 | SUCCESS | [Run 40764713071](https://github.com/frankcval/MiKompri/actions/runs/40764713071): PostgreSQL 16, suites y Docker |
| Users #45 | SUCCESS | [Run 40764713186](https://github.com/frankcval/MiKompri/actions/runs/40764713186): 12/32/31 y Docker |
| SonarCloud Code Analysis | FAILURE | [Check 123086861442](https://github.com/frankcval/MiKompri/runs/123086861442): cobertura nueva 24,6% frente a 80%; Security Rating C frente a A |
| Aceptación Entra | PENDIENTE | Workflow manual separado; ningún éxito de aceptación acreditado para HEAD |

Los cinco workflows pedidos cumplen el objetivo verde. **CI completo no está verde** por SonarCloud. No se ha comprobado que SonarCloud sea required por protección de rama; el fallo real se conserva como bloqueo de recomendación. El éxito técnico de mergeability del PR no acredita aceptación. Estos resultados corresponden al SHA indicado; los cambios documentales locales todavía no tienen CI propio.

## Alcance real del PR

El diff contra la base contiene 298 archivos. El título es «007 web react client» y el cuerpo sigue como `[Copilot is generating a summary...]`: no describen adecuadamente el alcance combinado. Este informe prepara la descripción revisable; no se publica ningún mensaje en el PR.

| Propietario | Contenido que pertenece a ese alcance |
| --- | --- |
| 007 | SPA React/TypeScript/Vite/Fluent, MSAL/PKCE, perfil, grupos mínimos, listas personales/compartidas, gastos/reparto, catálogo y alta autorizada posteriormente, portada, responsive/estados y base de CI web. Prerrequisitos backend expresos: correlación Users `(tid, oid)`, ownership personal, JWT real de ProductCatalog, audience común y CORS. T133 corrige ItemId/ProductId. |
| 007 — migración | `20261023110000_AddCanonicalIdentityCorrelation`: TenantId/ObjectId, índices canónicos y ExternalUserId nullable transicional. No es una ampliación UX de 008. El fix `07c07e1` repara este requisito TP11 transversal. |
| 008 — deuda de 007 | Completar CRUD, miembros/roles, RHF para gastos, permisos por recurso (T132), errores/recuperación, contratos, pruebas y aceptación original pendientes. No todo cambio backend es ajeno a 007: parte completa sus prerrequisitos. |
| 008 — ampliaciones | Compra/descompra explícita, estimación desconocida, gestión compartida dedicada, lectura/historial/corrección de gastos, búsqueda/alta contextual en dos pasos, intención/borradores, Mis listas, Personas, navegación móvil, componentes comunes, lazy loading y WOFF2. |
| 008 — persistencia/CI | `20261028151702_AddKnownPriceState` y snapshot/configuración: IsPriceKnown con default true, conserva ceros previos. Workflow resource-security con PostgreSQL, selección controlada en tres motores y aceptación Entra manual separada; pruebas de preflight. |

La clasificación es por responsabilidad y requisito; varios archivos evolucionan en ambas specs. No se separan ni reestructuran commits ni se presume que se puedan integrar subconjuntos del diff sin comprobar sus dependencias.

## Reconciliación de tareas sin cierres automáticos

Las notas de 007 del 24/27 de octubre son históricas: sus afirmaciones de placeholders, `/mine` inexistente, falta de membresías o falta de RHF en gastos ya no describen todo el árbol actual. La implementación nueva no convierte automáticamente en aceptada cada tarea compuesta.

| Tareas abiertas | Evidencia implementada y desajuste documental | Gate que persiste |
| --- | --- | --- |
| 007/T056/T113 | GroupDetailPage y GroupPeople muestran miembros/roles y enlaces; 008/T019 cerrada | Verificación integrada/roles; no reabrir invitación por GUID, excluida sin contrato público |
| 007/T057/T058/T060/T061/T111/T112 | Clientes, detalle y acciones personales/compartidas existen; `/mine` existe; permisos servidor probados; 008/T007–021 | Matriz completa de errores/recarga y RHF/Zod completo en edición personal; 008/T011/T012 abiertas correctamente |
| 007/T059 | Creación usa RHF/Zod; edición/renombrado en ManageListActions usa estado/draft y validación explícita | No declarar toda la tarea RHF/Zod completada |
| 007/T069/T071/T114 | ExpenseForm usa RHF y Zod; SettlementPage está en pestaña Gastos del detalle compartido; 008/T034/T035 | Recorrido integrado gasto → reparto, roles/retirados y recuperación completa |
| 007/T047/T087/T115 | Perfil y estados compartidos implementados; 008/T029 | Matriz HTTP/red por vista y operación, 008/T030 |
| 007/T089/T117 | Workflow ya exige smoke real; tres motores, artefactos y Docker pasan | La parte de configuración está acreditada; CI controlada no cierra E2E Entra ni aceptación global |
| 007/T093 | Regresión ShoppingList antigua fallida superada por CI actual verde y evidencia 008 de API 68/68 | Candidata a reconciliación individual; el resultado histórico no es un fallo vigente. No equivale a cerrar T118 ni 007 |
| 007/T108–110/T116; 008/T041/T046 | Tema, WOFF2, componentes y pruebas visuales/axe/teclado existen | Contraste completo, lector, zoom 200%, teclado virtual y safe areas reales |
| 007/T043/T053/T054/T066/T075/T083/T102; 008/T016/T022/T031/T036 | Archivos integrados con aserciones sustituyen skips históricos | Completar escenarios y ejecutarlos con Entra/API reales; no basta retirar test.skip |
| 008/T023/T026/T030 | Saga y recuperación parcial implementadas | Duplicados/timeout, compartida y matriz completa de errores aún sin acreditar |
| 008/T043/T049 | Medición y optimización existen; T049 acredita medir | SC-008 incumple: LCP índices/personal/compartida 2512/2584/2692 ms; marcado/añadir 216/344 ms, sobre 2500/200 ms |
| 007/T118; 008/T048/T050/T054 | Suites controladas y protocolo documentados | Dos personas con Entra, piloto 0/10 y cierre FR/SC pendientes |

Hallazgos concretos de cobertura integrada que requieren trabajo además de credenciales:

- `personal-lists.spec.ts` prepara lista/ítem por API, marca/desmarca y edita cantidad en UI; no verifica renombrado, borrado UI ni segunda lista intacta. T016 no está completa.
- `settlement-flow.spec.ts` registra gasto por API, abre historial y compara summary al desmarcar; no ejerce corrección/borrado por interfaz, propuesta esperada ni antiguos miembros. Su título no acredita esos criterios de T036.
- `settlement-authorization.spec.ts` comprueba 403 al pagar por otro y al borrar lista, no acceso denegado a summary/proposal. No cierra 007/T102.
- `auth-session.spec.ts` restaura una sesión exportada y prueba logout; no hace login interactivo inicial/PKCE, cambio de cuenta ni recuperación completa de destino/borrador. T031 sigue abierta.
- El smoke RTL de accesibilidad solo comprueba main/nav. La evidencia axe/teclado adicional de 008 mejora cobertura, pero no completa por sí sola 007/T081 ni revisión manual.

## Decisión de integración

**NOT READY TO MERGE** para el PR combinado tal como está.

Los prerrequisitos TP11 corregidos, permisos/persistencia y las suites de los cinco workflows están técnicamente acreditados y son candidatos a integración como incremento. No requieren repetir la corrección de Users. Eso no autoriza integrar ahora todo el PR ni cerrar las specs: PP2 exige el done de cada MVP y TP8 sus verificaciones.

Bloqueos concretos: SonarCloud rojo (cobertura y seguridad); completar y ejecutar integración Entra de dos cuentas sobre el candidato; completar matrices de errores/CRUD/RHF y escenarios integrados señalados; accesibilidad manual ampliada; presupuesto SC-008 incumplido; piloto de diez personas y aceptación individual FR/SC. La descripción del PR debe reflejar 007+008, backend, ambas migraciones y CI antes de integrarlo. No se relajan gates ni se presentan pruebas controladas como aceptación humana.
