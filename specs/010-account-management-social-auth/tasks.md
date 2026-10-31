# Tasks: 010 — MVP010-A y futuro010-B

**Input**: [spec](spec.md), [plan](plan.md), [modelo](data-model.md), [migración](migration.md), [contratos](contracts/account-and-identity.md).
**Estado**: beta010-A con alcance aprobado31/10/2026: Google CIAM sin Microsoft + Microsoft legacy. TP11 1.2.0 intacta; gates funcionales acreditados, CI del SHA final verificada al publicar el PR. Migración Microsoft y QA ampliada se difieren explícitamente. T007–T010 acreditan aprobación explícita. [Resultados técnicos](evidence/validation.md) y [recuperación/login real](evidence/authentication-recovery-2026-10-31.md) distinguen código, pruebas y aceptación pendiente. Las demás tareas no se cierran por existencia de archivos ni por diferirse a B.
**Formato**: A=gates beta (BLOCKS 010-A BETA si falta su aceptación); A-OPT=opcional; B=010-B; DEFERRED-MICROSOFT-MIGRATION=traslado futuro; DEFERRED-NON-BLOCKING-QA=QA posterior. Diferir no equivale a completar. Tests antes de reglas según TP8; no ejecución paralela/delegación por una etiqueta.

## 010-A — A0 aprobación y configuración del diseño

- [x] T001 [A] Ratificar auditoría/evidencia/fuentes y alcance Microsoft existente+Google en `current-state-audit.md`, `research.md` y `evidence/010-a-scope-review.md`; harness no equivale a aceptación productiva.
- [x] T006 [A] Aprobar coste/configuración mínima y minimización de perfil/recuperación en proveedor en `research.md` y `privacy-and-deletion.md`; sin SMS/trials/credenciales propios.
- [x] T007 [A] Revisar/aprobar `docs/adr/ADR-004-ciam-identity.md` incremental: CIAM para Google y transición Microsoft, sin exigir prueba de linking A ni elegir Alternative B.
- [x] T008 [A] Aprobar `constitution-amendment-proposal.md` por gobernanza; solo tras autorización explícita aplicar TP11/version/templates. No editar constitución por esta tarea documental.
- [x] T009 [A] Ratificar contrato JWT/tupla, perfil y D1/D2 en `data-model.md`, `contracts/account-and-identity.md`, `architecture-review.md`. D3/broker quedan B.
- [x] T010 [A][US2] Aprobar coexistencia/cohortes y prueba de origen Microsoft inmutable en `migration.md`; mismo UserId, reserva, rollback/no migrados. Sin linking general ni G3 como prerequisito.
- [x] T011 [A] Revisar amenazas de login/provisión/migración/perfil/sesión en `threat-model.md`; takeover/linking/recovery por otro proveedor se revisarán en B antes de ofrecerlos.
- [x] T012 [A][US4] Ratificar perfil básico/username/idioma y preferencias tipadas con finalidad actual. Iniciales; sin foto/storage, DOB/teléfono/dirección ni UI de métodos.

## 010-A — A1 login aislado (no habilita negocio protegido)

- [x] T002 [A][US1] Validar Microsoft existente y Google sin Microsoft con `evidence/provider-capabilities.md`, claims/token del recurso y diferencias de usuario nuevo/existente. Preservar tipos de cuenta ya admitidos; MSA adicional/email no son gate de este ítem.
- [x] T005 [A][US5] Verificar/pinar MSAL Web/metadata/Code+PKCE browser-delegated; conservar la auditoría Native Auth sin convertir SDK MAUI en bloqueo.
- [x] T020 [A][US5] Tests JWT firmados para pares issuer/tenant/audience/scope/clientes/tid/oid de tres APIs en `test/MiKompri.Users.Api.Tests/Security/ExternalIdValidationTests.cs` y equivalentes; negativos Google/Graph/ID/expired/cruces.
- [x] T022 [A][US1] Tras autorización pertinente preparar configuración CIAM/Google y confianza de QA según `quickstart.md`; conservar Microsoft actual. No activar Authentication Context/CA de linking ni modificar defaults para login.
- [x] T023 [A][US1] Tests Web de botones Google prioritario/Microsoft disponible, callback, cancelación, errores y aviso de cuenta Microsoft existente en `MiKompri.Web/tests/unit/auth/external-id.spec.ts` y componentes auth.
- [x] T024 [A][US1] Integrar Web MSAL/configuración conocida por ruta/proveedor en `MiKompri.Web/src/auth/msal/msalConfig.ts`, `src/shared/config/env.ts` y login; secrets fuera de SPA. No UI de vincular/desvincular. Proveedores opcionales no se muestran hasta QA.

## 010-A — A2 identidad y regresiones conservadas

- [x] T013 [A][US2] Tests de UserId inmutable, canonical tuple única y traslado acotado en `test/MiKompri.Users.Domain.Tests/`; AccountOperation/ProofReceipt y pruebas de cambio de métodos se conservan en T027[B].
- [x] T014 [A][US2] Tests de traslado Microsoft administrado en `test/MiKompri.Users.Application.Tests/Commands/IdentityResolutionTests.cs`, `test/MiKompri.Users.Api.Tests/PostgresIdentityTests.cs` y `FederatedOriginVerifierTests.cs`: origen federado acreditado, reserva, destino ocupado, idempotencia, email collision y fallback sub solo legado; sin doble prueba G3 genérica.
- [x] T015 [A][US2] Adaptar resolución/alta en `MiKompri.Users.Domain/Users/` y `MiKompri.Users.Application/Commands/SyncProfile/`: una canónica activa, namespace confiable y registro acotado de migración; no ExternalIdentities[] genérica.
- [x] T016 [A][US2/US4] Preparar únicamente schema aditivo necesario para perfil/estado y traslado en `MiKompri.Users.Infrastructure/Persistence/`; backfill fiable, perfiles previos personalizados, sin tablas de broker por anticipación.
- [x] T017 [A][US2] Tests PostgreSQL Docker de índices/provisión concurrente/reserva/cutover en `test/MiKompri.Users.Api.Tests/PostgresIdentityTests.cs`; manifiestos de pruebas técnicas, no aceptación del traslado real.
- [x] T018 [A][US2] Separar resolver/provisión/sync en `MiKompri.Users.Api/Middleware/UserProvisioningMiddleware.cs`; migración reservada/retirada no puede crear User sustituto ni reactivar origen por fallback.

## 010-A — A3 APIs protegidas y sesión

- [x] T021 [A][US5] Adaptar `MiKompri.{Users,ShoppingList,ProductCatalog}.Api/Security/*JwtValidation.cs` y Program a confianza explícita por issuer y scopes; sin cambiar permisos de negocio ni aceptar Google directo.
- [x] T025 [A][US5] Tests y limpieza de sesión A→B en `MiKompri.Web/tests/unit/auth/session.spec.ts`, `src/auth/session/`; namespaces Microsoft/CIAM, TanStack/borradores y respuestas tardías.
- [x] T026 [A][US5] Tests primero e integración mínima D1 access-status: lectura Users local; consumidores por UsersApi/bearer, GET catálogo incluido, resultado solo dentro de request. No Graph/Redis/bus/cache inter-request. Gate antes de liberar negocio, no de primer login aislado.
- [x] T042 [A][US5] AR-01–03: JWT vigente→suspender→siguiente control de tres APIs deniega; Users caído503; carreras autorizadas en vuelo y cortes/reactivación no resucitan tokens. Canónica activa beta; el caso original de traslado se conserva en T019/T048 diferidos, sin exigir múltiples bindings B.
- [x] T043 [A][US2] Regresiones Groups/GroupMembership/Owner/Admin/Member, listas/gastos/participantes/compras/idempotencia005/008/009 y referencias UserId.
- [x] T051 [A][US5] A-AC03/04/05/09 y AR-01–03 en tres APIs/Web real: confianza, D1, logout/cambio y browser-delegated. AR-05–06/G3 no bloquean este ítem.

## 010-A — A4 perfil básico

- [x] T034 [A][US4] Tests de inicialización/autoridad, personalización/clear, username normalizado/único y edición por versión en `test/MiKompri.Users.Domain.Tests/ProfileAuthorityTests.cs` y `UsernameTests.cs`.
- [x] T035 [A][US4] Ampliar User/perfil mínimo en `MiKompri.Users.Domain/Users/User.cs` y Application/UpdateProfile; sin foto, datos personales futuros ni almacén genérico de preferencias.
- [x] T036 [A][US4] Tests de regresión login/GET/sync no sobrescribe DisplayName/nombres/idioma propios en `test/MiKompri.Users.Api.Tests/ProfileApiTests.cs` y Application/SyncProfile.
- [x] T037 [A][US4] Reutilizar GET/PUT/me y POST/me/sync en `MiKompri.Users.Api/Controllers/ProfileController.cs` y DTOs; compatibilidad legacy/If-Match documentada, sin aceptar IDs/verificationState del cliente.
- [x] T038 [A][US4] Tests/UI de perfil propio, username distinto de login e iniciales en `MiKompri.Web/tests/component/profile/` y `src/features/profile/`. Reimportación explícita futura T058[B].
- [x] T050 [A][US4] A-AC10 con proveedor real: personalizar nombre/campos A y repetir login/sync sin sobrescritura. Avatar personalizado solo cuando se implemente T039[B].

## 010-A — A5 validación y cierre

- [x] T041 [A] Evidencia negativa de amenazas aplicables A en `evidence/security.md`; matriz separada de métodos B y revisión de seguridad.
- [x] T044 [A] Suites Domain/Application/API/PostgreSQL, lint/typecheck/build/Web unit/component/integration/E2E directamente afectados; registrar SHA/comandos/resultados en `evidence/validation.md`.
- [x] T045 [A] Verificar modelo/migraciones EF, Docker y workflows Users/ShoppingList/ProductCatalog/Web/Resource del HEAD candidato; no dar por verde CI de otro SHA.
- [x] T046 [A] Revisar issuer/audience/scopes/CORS/redirects/configuración por entorno, mínimo privilegio de transición y recuperación en proveedor; sin secretos/PII en evidencia ni Graph en negocio.
- [x] T047 [A][US1] Microsoft existente y Google nuevo sin Microsoft en aplicación normal, A-AC01/02/08, `evidence/real-provider-acceptance.md`. Harness es evidencia parcial; email no bloquea.
- [x] T055 [A] Revisión final del alcance beta31/10: ocho A-AC01–05/08–10, doce gates de producto, ADR/TP11/coexistencia y CI exacta final. A-AC06/07 y T019/T048 diferidos; ninguna autorización de despliegue/merge/cutover implícita. A-AC11 solo si T056 se habilita. No exigir tareas B ni cerrar007/008/009.

## Microsoft External ID migration — diferida, no gate beta

Decisión explícita31/10: conservar criterios originales sin completarlos. A-AC06/A-AC07 = DEFERRED — Microsoft External ID migration. El ensayo sigue INCONCLUSIVE por colisión CIAM; no PASS/FAIL. Tests, runner, journal/reservation e infraestructura permanecen. No ejecutar preparación de destino/reserva/cutover/rollback/manifiestos/retiro legacy durante beta.

- [ ] T019 [DEFERRED-MICROSOFT-MIGRATION][US2] Ensayar reintento/timeout/no migrados/rollback de tráfico sin Down ni pérdida de nuevas compras en `evidence/migration-rehearsal.md`; detener cutover si origen no acreditado, conservar Microsoft.
- [ ] T048 [DEFERRED-MICROSOFT-MIGRATION][US2] Acreditar cohorte Microsoft en QA que conserva exactamente UserId y referencias al pasar a CIAM y al revertir, A-AC06/07. Conservar acceso legado no acredita por sí solo migración terminada; no habilitar cambio de método autoservicio.

## QA ampliada — DEFERRED NON-BLOCKING QA

- [ ] T053 [DEFERRED-NON-BLOCKING-QA] Web320/390/430, teclado/lector/zoom, errores/return intent y budgets existentes; evidencia manual real cuando corresponda. Evidencia básica320/390/430/teclado/targets/errores/loading/cancelación sí acreditada para beta; lector/zoom completos siguen sin probar.
- [ ] T059 [DEFERRED-NON-BLOCKING-QA][US1] Repetir alta Google con otro tester nunca registrado tras activar atributos opcionales de nombre; registrar claims realmente disponibles, sin Graph/backfill ni usar email para adoptar usuarios. Evidencia de alta Google Web existente suficiente para beta, decisión en real-provider-acceptance.md.
- [ ] T060 [DEFERRED-NON-BLOCKING-QA][US5] Matriz manual exhaustiva multi-tab y respuestas demoradas. Ambos cambios reales y cobertura automatizada adecuada bastan para beta; no afirmar que esta matriz manual ya se ejecutó.

## 010-A — opcional, nunca bloquea Microsoft + Google

- [ ] T056 [A-OPT][US1] QA email OTP básico en mismo CIAM/contrato: nuevo usuario sin Microsoft, alta/login posterior, OTP incorrecto/expirado/reutilizado, tuple/UserId estable y sesión. Si pasa sin nuevo modelo/linking/G3 y se aprueba, habilitar opción email y A-AC11; si falla o retrasa el núcleo, dejar deshabilitado y trasladar entrega a B sin marcarla completa.

## 010-B — diferidas; fuera del DoD de010-A

No borrar ni cerrar por diferir. [Requisitos y gates](010-b-deferred.md). La clasificación A como correlación CIAM no certifica agregación de métodos. Toda reanudación sensible requiere revisión y autorización correspondiente.

- [ ] T003 [B][US3] Matriz completa Alternative A Google-only→email/email→Google, colisiones de objetos, permisos Graph y acceso real tras cambios en `evidence/aggregation-spike.md`.
- [ ] T004 [B][US3] G3/D3/AR-05–06 por método y operación: control<=300s, TTL<=600s, state/nonce/PKCE, SSO/refresh/MFA histórica, bootstrap, contexto/política efectivos. Conservar `evidence/assurance-spike.md`, `g3-step-up-review.md` y `g3-email-mfa-qa.md`; no más transiciones CA por esta revisión.
- [ ] T027 [B][US3] Tests AccountOperation/ProofReceipt para link/unlink/cambio de proveedor/migración autoservicio: partes requeridas, sesión/propósito/expiry/replay/consentimiento/versiones y edad al confirmar.
- [ ] T028 [B][US3] Tests API de gestión de métodos A/B, IDOR, último método, concurrencia y errores genéricos en `test/MiKompri.Users.Api.Tests/AccountMethodsTests.cs`.
- [ ] T029 [B][US3] Broker/adaptador de pruebas y handlers AccountOperations tras G3 y ADR; no construirlo para login/traslado Microsoft acotado A.
- [ ] T030 [B][US3] Adapter de mutación CIAM A o múltiples bindings B solo si se elige por insuficiencia demostrada, con mínimo privilegio/serialización/reconciliación. Nunca ambos preventivamente.
- [ ] T031 [B][US3] Timeout antes/después de mutación CIAM/BD, identidad ocupada y colección Graph concurrente en `test/MiKompri.Users.Api.Tests/AccountOperationRecoveryTests.cs`.
- [ ] T032 [B][US3] Tests/UI de gestionar/vincular/probar alternativa/desvincular en `MiKompri.Web/tests/component/profile/AccountMethods.spec.tsx` y perfil.
- [ ] T033 [B][US3] Recuperación con alternativa ya vinculada, mismo UserId; sin prueba no autoservicio, `evidence/recovery.md`.
- [ ] T039 [B][US4] Avatar propio/proveedor si se aprueba: storage/ownership/formatos/EXIF/SSRF y fallback antes de activar. Iniciales A no acreditan avatar propio.
- [ ] T040 [B][US6] Diseñar/probar eliminación y conservación/último Owner; endpoint/jobs solo con aprobación. Minimización A se mantiene en T006/T012/T046.
- [ ] T049 [B][US3] Aceptación original AC-05/06/07/10: segundo método, linking explícito, email collision, último método y B→A con proveedor real.
- [ ] T052 [B][US3] Aceptación original AC-12: recuperación mediante método alternativo conserva UserId, pérdida total sin bypass.
- [ ] T054 [B/futuro006] MAUI: SDK/versión/deep links reales; sin extrapolar tutorial Kotlin/JS a MSAL.NET ni bloquear A Web.
- [ ] T057 [B/opcional] Proveedores adicionales y tipos Microsoft no admitidos actualmente; Apple/Facebook/passkeys/SMS/MFA avanzada solo tras necesidad, QA y autorización nueva. Recuperación sigue en proveedor hasta diseñar alternativa segura.
- [ ] T058 [B/opcional][US4] UI de reimportación explícita por campo/procedencia y personalizaciones de avatar, con versión/consentimiento. No necesaria para inicialización única A.

## Dependencies & Execution Order — beta31/10

Auditoría/aprobación → Google CIAM+Microsoft legacy → correlación/alta y JWT → D1 → perfil/sesión → ocho aceptaciones beta → CI SHA final → PR para revisión. Microsoft→External ID se programa después; no bloquea Google ni el cierre beta.010-B y012 independientes.

## Revisión individual de las nueve tareas A antes abiertas

| Tarea original | Resolución y evidencia suficiente beta |
|---|---|
| T002 | Cerrada: Microsoft legacy y Google sin Microsoft en Web normal, access token CIAM/legacy y UserId estable; alta Google ya ejecutada. Repetición opcional con nuevos atributos en T059, sin fingir claims ausentes |
| T019 | DEFERRED MICROSOFT MIGRATION; criterio original de retry/timeout/manifiestos/rollback real no completado |
| T041 | Cerrada para A: JWT/Google directo/Graph/ID reales rechazados y matriz negativa Domain/Application/API/PG; amenazas de traslado real pasan a T019/T048, linking/recovery a010-B |
| T042 | Cerrada para A: D1 real200→403→401→200 con token estrictamente posterior, consumidores503 al caer Users; no se rebajan cortes. Canónica trasladada se probará en la migración futura |
| T047 | Cerrada: Google nuevo Web acreditado y relogin actual + Microsoft normal sin regresión; se acepta evidencia existente de nueva alta según decisión beta. No equivale a migración Microsoft |
| T048 | DEFERRED MICROSOFT MIGRATION; falta traslado/rollback y preservación real de manifiestos, INCONCLUSIVE conservado |
| T051 | Cerrada beta: JWT/D1 reales y Google↔Microsoft sin mezcla; tests cubren requests tardías/TanStack/drafts/generación/cuenta/tenant. Matriz manual exhaustiva T060 no bloqueante |
| T053 | DEFERRED NON-BLOCKING QA; básicos UX pasan, lector/zoom manual completos no acreditados |
| T055 | Cerrada por nuevo alcance aprobado y evidencia beta, sin exigir migración Microsoft/QA ampliada. CI final y estado del PR se verifican en publicación; no deploy/merge |

T056 = A-OPT. Todas las tareas B =010-B y siguen abiertas. T059/T060 son QA no bloqueante; ningún ID original eliminado. No quedan tareas A funcionales abiertas/BLOCKS 010-A BETA si los cinco workflows del SHA final terminan success. README y evidencia se publican junto al candidato.

Evidencia: [validación](evidence/validation.md), [proveedores](evidence/real-provider-acceptance.md), [seguridad](evidence/security.md), [D1 real](evidence/d1-real-cutoff.json), [alcance](evidence/010-a-scope-review.md), [traslado INCONCLUSIVE](evidence/migration-rehearsal.md).

## Registro histórico anterior a la decisión beta31/10

El texto siguiente conserva las reconciliaciones previas; sus estados abiertos/bloqueos de migración y QA **no definen el DoD vigente**. La tabla y casillas anteriores aplican la decisión actual sin convertir resultados INCONCLUSIVE en PASS.

<details>
<summary>Histórico de tareas anterior al alcance beta aprobado</summary>

## Dependencies & Execution Order

A0 aprobación →tests de A1 →resolución/migración A2 y trust/API →D1 antes de negocio →perfil/sesión →regresiones/aceptación/cierre.
Google puede desarrollarse sin esperar email/G3 y Microsoft conserva ruta anterior hasta cutover acreditado. No cerrar transición por haber creado un objeto CIAM.

B depende de nueva revisión y de sus propios G1/G3; A no depende de B. T056 puede omitirse del release A sin bloquearlo. Los tests de cada slice preceden reglas y su evidencia debe corresponder al candidato.

## Trazabilidad de trabajo conservado

- T002/T023/T047 separan Microsoft+Google obligatorio de email T056 y proveedores adicionales T057.
- Parte broker/proofs de T013/T014/T018 pasa a T027–033[B]; A mantiene canónica, provisión y traslado Microsoft con origen acreditado.
- T038 conserva edición de perfil A; reimportación pasa a T058[B]. T050 prueba campos A y conserva avatar futuro en T039.
- T040[Tarea futura] no elimina minimización A; T055 deja de exigir AC de B. T001–T055 siguen presentes una sola vez, sin marcar completitud.

Evidencia anterior: acceso Google/email en harness, JWT/scopes y regresiones offline; G3 sin fiabilidad acreditada, c1 sin política no prueba MFA y restauración QA verificada. Nada de eso implementa el MVP. [Revisión de alcance](evidence/010-a-scope-review.md).

## Evidencia de implementación

T007/T008/T009/T010: aprobación explícita del titular y enmienda aplicada, [registro A0](evidence/implementation.md). No cierra implementación ni aceptación funcional.

## Evidencia de cierre técnico — 31/10/2026

Las marcas técnicas anteriores se basan en ejecuciones registradas en [validation.md](evidence/validation.md), no en presencia de archivos. No cierran sus casos reales de aceptación:

| Tareas | Evidencia ejecutada |
|---|---|
| T005/022/023/024 | MSAL fijado, QA aislado y rutas; auth49/49, Web138/138. Google real confirmado después de corregir AADSTS90023. Microsoft legacy conservado; T002/T047 siguen abiertos para aceptación completa del candidato |
| T012/034/035/036/037/038 | Perfil mínimo aprobado implementado, Domain21/Application37/API90, Web perfil; clear/claims/versiones/whitelist/username. T050 mantiene pendiente relogin real personalizado |
| T013/015/016/017/018 | Invariantes/resolve/provision/journal y PostgreSQL4/4 sobre schema aditivo. Tests reales en `test/MiKompri.Users.Api.Tests/PostgresIdentityTests.cs`, sin cambios EF pendientes. T014/019/048 siguen abiertos; la prueba externa sustituida no acredita traslado real |
| T020/021/026 | JWT20 casos HTTP por API y17 criptográficos; D1 local/remoto, fallo cerrado y una consulta por petición. T042/T051 siguen abiertos por aceptación completa D1/token social real |
| T025 | Sesión/tenant/cuenta/respuestas tardías/formularios/borradores,49 tests auth; aceptación A→B completa del candidato sigue en T051 |
| T043 | Regresiones backend406/406 y controladas Web120/120 previas a hint, más60/60 portada/secciones tras hint. Esto no acredita manifiestos reales de migración |
| T044 | Suites .NET406, Web138, lint/typecheck/build, E2E60 tras último cambio Web,0 fallos. CI final se conserva en T045 abierto |

T002/014/019/041/042/045/047/048/050/051/053/055 permanecen abiertas. T056 no se habilita. Todas las tareas B permanecen abiertas; no se modifican gates007/008/009. Ver [recuperación](evidence/authentication-recovery-2026-10-31.md) y [ensayo de traslado](evidence/migration-rehearsal.md).

## Reconciliación individual del candidato a9ca37d — 31/10

Esta tabla actualiza el estado histórico anterior; no convierte preparación cloud o tests sustituidos en aceptación real.

| Tarea A revisada | Resultado y evidencia |
|---|---|
| T001 | Cerrada: auditoría de base remota46cf, comparación main, Spec Kit010, fuentes vigentes y límites A/B revisados en implementation.md y research.md; CI del nuevo HEAD se verifica por separado |
| T006 | Cerrada para alcance aprobado: reutilización QA sin compras/trials/SMS; pricing core/MAU y add-ons revalidados, sin prometer factura cero; perfil mínimo y recovery en proveedor. research.md, privacy-and-deletion.md y plan QA |
| T011 | Cerrada la revisión de amenazas A; threat-model.md y security.md conservan pruebas pendientes sin certificar aceptación |
| T014 | Cerrada técnicamente: Application37, verificador Graph7 controlados y PostgreSQL7 reales, negativos origen/reserva/destino/sub/email/idempotencia/retry. No es T019/T048 ni federación real |
| T019 | Abierta: no hay manifiesto completo ni compras posteriores a un traslado real/rollback; pruebas técnicas de retry/rollback ampliadas, migration-rehearsal.md |
| T048 | Abierta: CIAM devuelve colisión y no acredita login al destino federado; no se cambia UserId ni se ejecuta cutover/rollback |
| T055 | Abierta: depende de aceptación real Microsoft y revisión final/manual; no hay autorización de despliegue ni merge |
| T056 y B | Todas abiertas; email opcional no habilitado en Web, ninguna implementación B ni nuevo ensayo G3/CA |

T002/T041/T042/T045/T046/T047/T050/T051/T053 se contrastan con los resultados detallados del candidato en validation.md, real-provider-acceptance.md y security.md. Las casillas se actualizan únicamente cuando su criterio entero quede acreditado; la presencia de esta tabla no las cierra.

T050 cerrada por recorrido real completo de campos A y clear con dos logout/login Google en a9ca37d, registrado en real-provider-acceptance.md. No incluye foto ni acredita importación de nombres de una cuenta nueva. La continuidad inicial unknown se documenta sin backfill administrativo.

| Otras tareas revisadas | Estado actual y comprobación concreta restante |
|---|---|
| T002 | Abierta: ambos proveedores y sus access tokens funcionan; falta alta Google nueva en el flow actualizado para comparar claims iniciales/nuevo-existente, sin sobrescribir perfiles previos |
| T041 | Abierta: negativos reales Google/Graph/ID y matriz técnica ejecutados; revisar negativos de cohorte externa y matriz real de sesión al completar aceptación, security.md |
| T042 | Abierta parcialmente: secuencia real D1 completa y503 acreditados; traslado de canónica real aún bloqueado por T048, no se exige bindings B |
| T045 | Cerrada sobre código a9ca37d: EF sin cambios, cuatro Docker, runner/harness, cinco workflows success con URLs en validation.md. Commit documental posterior requiere nueva comprobación de SHA, entregada al cierre |
| T046 | Cerrada: matriz issuer/tenant/aud/scope/clientes/redirects y CORS contrastada con configuración y tokens reales; mínimo privilegio del piloto, recuperación original,67 archivos sin JWT/email personal y cero artefactos privados versionados. security.md y validation.md |
| T047 | Abierta parcialmente: Microsoft actual conserva Guid y Google sin Microsoft funciona; nueva alta del candidato/flow y A-AC08 aún requieren comprobación actual explícita |
| T050 | Cerrada: personalizaciones y clear de campos A sobreviven a dos relogin Google reales, sin Graph/backfill |
| T051 | Abierta parcialmente: JWT/D1 y ambas direcciones reales pasan; falta matriz real completa multi-tab/respuestas tardías y caso de token Microsoft después del traslado |
| T053 | Abierta:320/390/430 sin overflow, botones44px/Tab observados y suites de errores/loading/cancelación; faltan lector/zoom y aceptación manual completa |

T044 conserva los resultados ejecutados, incluidos timeout inicial Web y119/120 E2E seguido de1/1 en repetición; no se afirma cero fallos en la primera ejecución. CI del código actual pasó el gate controlado de tres navegadores. No cambia criterio de rendimiento.

</details>
