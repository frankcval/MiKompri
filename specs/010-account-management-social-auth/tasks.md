# Tasks: 010 — MVP010-A y futuro010-B

**Input**: [spec](spec.md), [plan](plan.md), [modelo](data-model.md), [migración](migration.md), [contratos](contracts/account-and-identity.md).
**Estado**: alcance revisado30/10/2026; implementación no iniciada por esta ejecución. Todas las tareas permanecen abiertas. Se conservan T001–T055; sus partes de gestión de métodos pasan a B, sin dar por completado nada.
**Formato**: A=obligatorio para el MVP; A-OPT=opcional y no bloqueante; B=diferido. Tests antes de reglas según TP8; no ejecución paralela/delegación por una etiqueta.

## 010-A — A0 aprobación y configuración del diseño

- [ ] T001 [A] Ratificar auditoría/evidencia/fuentes y alcance Microsoft existente+Google en `current-state-audit.md`, `research.md` y `evidence/010-a-scope-review.md`; harness no equivale a aceptación productiva.
- [ ] T006 [A] Aprobar coste/configuración mínima y minimización de perfil/recuperación en proveedor en `research.md` y `privacy-and-deletion.md`; sin SMS/trials/credenciales propios.
- [ ] T007 [A] Revisar/aprobar `docs/adr/ADR-004-ciam-identity.md` incremental: CIAM para Google y transición Microsoft, sin exigir prueba de linking A ni elegir Alternative B.
- [ ] T008 [A] Aprobar `constitution-amendment-proposal.md` por gobernanza; solo tras autorización explícita aplicar TP11/version/templates. No editar constitución por esta tarea documental.
- [ ] T009 [A] Ratificar contrato JWT/tupla, perfil y D1/D2 en `data-model.md`, `contracts/account-and-identity.md`, `architecture-review.md`. D3/broker quedan B.
- [ ] T010 [A][US2] Aprobar coexistencia/cohortes y prueba de origen Microsoft inmutable en `migration.md`; mismo UserId, reserva, rollback/no migrados. Sin linking general ni G3 como prerequisito.
- [ ] T011 [A] Revisar amenazas de login/provisión/migración/perfil/sesión en `threat-model.md`; takeover/linking/recovery por otro proveedor se revisarán en B antes de ofrecerlos.
- [ ] T012 [A][US4] Ratificar perfil básico/username/idioma y preferencias tipadas con finalidad actual. Iniciales; sin foto/storage, DOB/teléfono/dirección ni UI de métodos.

## 010-A — A1 login aislado (no habilita negocio protegido)

- [ ] T002 [A][US1] Validar Microsoft existente y Google sin Microsoft con `evidence/provider-capabilities.md`, claims/token del recurso y diferencias de usuario nuevo/existente. Preservar tipos de cuenta ya admitidos; MSA adicional/email no son gate de este ítem.
- [ ] T005 [A][US5] Verificar/pinar MSAL Web/metadata/Code+PKCE browser-delegated; conservar la auditoría Native Auth sin convertir SDK MAUI en bloqueo.
- [ ] T020 [A][US5] Tests JWT firmados para pares issuer/tenant/audience/scope/clientes/tid/oid de tres APIs en `test/MiKompri.Users.Api.Tests/Security/ExternalIdValidationTests.cs` y equivalentes; negativos Google/Graph/ID/expired/cruces.
- [ ] T022 [A][US1] Tras autorización pertinente preparar configuración CIAM/Google y confianza de QA según `quickstart.md`; conservar Microsoft actual. No activar Authentication Context/CA de linking ni modificar defaults para login.
- [ ] T023 [A][US1] Tests Web de botones Google prioritario/Microsoft disponible, callback, cancelación, errores y aviso de cuenta Microsoft existente en `MiKompri.Web/tests/unit/auth/external-id.spec.ts` y componentes auth.
- [ ] T024 [A][US1] Integrar Web MSAL/configuración conocida por ruta/proveedor en `MiKompri.Web/src/auth/msal/msalConfig.ts`, `src/shared/config/env.ts` y login; secrets fuera de SPA. No UI de vincular/desvincular. Proveedores opcionales no se muestran hasta QA.

## 010-A — A2 identidad y transición Microsoft

- [ ] T013 [A][US2] Tests de UserId inmutable, canonical tuple única y traslado acotado en `test/MiKompri.Users.Domain.Tests/`; AccountOperation/ProofReceipt y pruebas de cambio de métodos se conservan en T027[B].
- [ ] T014 [A][US2] Tests de traslado Microsoft administrado en `test/MiKompri.Users.Application.Tests/Commands/MigrateIdentityTests.cs`: origen federado acreditado, reserva, destino ocupado, idempotencia, email collision y fallback sub solo legado; sin doble prueba G3 genérica.
- [ ] T015 [A][US2] Adaptar resolución/alta en `MiKompri.Users.Domain/Users/` y `MiKompri.Users.Application/Commands/SyncProfile/`: una canónica activa, namespace confiable y registro acotado de migración; no ExternalIdentities[] genérica.
- [ ] T016 [A][US2/US4] Preparar únicamente schema aditivo necesario para perfil/estado y traslado en `MiKompri.Users.Infrastructure/Persistence/`; backfill fiable, perfiles previos personalizados, sin tablas de broker por anticipación.
- [ ] T017 [A][US2] Tests PostgreSQL Docker de índices/provisión concurrente/reserva/cutover en `test/MiKompri.Users.Api.Tests/IdentityMigrationTests.cs`, manifiestos antes/después.
- [ ] T018 [A][US2] Separar resolver/provisión/sync en `MiKompri.Users.Api/Middleware/UserProvisioningMiddleware.cs`; migración reservada/retirada no puede crear User sustituto ni reactivar origen por fallback.
- [ ] T019 [A][US2] Ensayar reintento/timeout/no migrados/rollback de tráfico sin Down ni pérdida de nuevas compras en `evidence/migration-rehearsal.md`; detener cutover si origen no acreditado, conservar Microsoft.
- [ ] T048 [A][US2] Acreditar cohorte Microsoft en QA que conserva exactamente UserId y referencias al pasar a CIAM y al revertir, A-AC06/07. Conservar acceso legado no acredita por sí solo migración terminada; no habilitar cambio de método autoservicio.

## 010-A — A3 APIs protegidas y sesión

- [ ] T021 [A][US5] Adaptar `MiKompri.{Users,ShoppingList,ProductCatalog}.Api/Security/*JwtValidation.cs` y Program a confianza explícita por issuer y scopes; sin cambiar permisos de negocio ni aceptar Google directo.
- [ ] T025 [A][US5] Tests y limpieza de sesión A→B en `MiKompri.Web/tests/unit/auth/session.spec.ts`, `src/auth/session/`; namespaces Microsoft/CIAM, TanStack/borradores y respuestas tardías.
- [ ] T026 [A][US5] Tests primero e integración mínima D1 access-status: lectura Users local; consumidores por UsersApi/bearer, GET catálogo incluido, resultado solo dentro de request. No Graph/Redis/bus/cache inter-request. Gate antes de liberar negocio, no de primer login aislado.
- [ ] T042 [A][US5] AR-01–03: JWT vigente→suspender→siguiente control de tres APIs deniega; Users caído503; carreras autorizadas en vuelo y cortes/reactivación no resucitan tokens. Probar canónica activa/traslado, sin exigir múltiples bindings B.
- [ ] T043 [A][US2] Regresiones Groups/GroupMembership/Owner/Admin/Member, listas/gastos/participantes/compras/idempotencia005/008/009 y referencias UserId.
- [ ] T051 [A][US5] A-AC03/04/05/09 y AR-01–03 en tres APIs/Web real: confianza, D1, logout/cambio y browser-delegated. AR-05–06/G3 no bloquean este ítem.

## 010-A — A4 perfil básico

- [ ] T034 [A][US4] Tests de inicialización/autoridad, personalización/clear, username normalizado/único y edición por versión en `test/MiKompri.Users.Domain.Tests/ProfileAuthorityTests.cs` y `UsernameTests.cs`.
- [ ] T035 [A][US4] Ampliar User/perfil mínimo en `MiKompri.Users.Domain/Users/User.cs` y Application/UpdateProfile; sin foto, datos personales futuros ni almacén genérico de preferencias.
- [ ] T036 [A][US4] Tests de regresión login/GET/sync no sobrescribe DisplayName/nombres/idioma propios en `test/MiKompri.Users.Api.Tests/ProfileApiTests.cs` y Application/SyncProfile.
- [ ] T037 [A][US4] Reutilizar GET/PUT/me y POST/me/sync en `MiKompri.Users.Api/Controllers/ProfileController.cs` y DTOs; compatibilidad legacy/If-Match documentada, sin aceptar IDs/verificationState del cliente.
- [ ] T038 [A][US4] Tests/UI de perfil propio, username distinto de login e iniciales en `MiKompri.Web/tests/component/profile/` y `src/features/profile/`. Reimportación explícita futura T058[B].
- [ ] T050 [A][US4] A-AC10 con proveedor real: personalizar nombre/campos A y repetir login/sync sin sobrescritura. Avatar personalizado solo cuando se implemente T039[B].

## 010-A — A5 validación y cierre

- [ ] T041 [A] Evidencia negativa de amenazas aplicables A en `evidence/security.md`; matriz separada de métodos B y revisión de seguridad.
- [ ] T044 [A] Suites Domain/Application/API/PostgreSQL, lint/typecheck/build/Web unit/component/integration/E2E directamente afectados; registrar SHA/comandos/resultados en `evidence/validation.md`.
- [ ] T045 [A] Verificar modelo/migraciones EF, Docker y workflows Users/ShoppingList/ProductCatalog/Web/Resource del HEAD candidato; no dar por verde CI de otro SHA.
- [ ] T046 [A] Revisar issuer/audience/scopes/CORS/redirects/configuración por entorno, mínimo privilegio de transición y recuperación en proveedor; sin secretos/PII en evidencia ni Graph en negocio.
- [ ] T047 [A][US1] Microsoft existente y Google nuevo sin Microsoft en aplicación normal, A-AC01/02/08, `evidence/real-provider-acceptance.md`. Harness es evidencia parcial; email no bloquea.
- [ ] T053 [A] Web320/390/430, teclado/lector/zoom, errores/return intent y budgets existentes; evidencia manual real cuando corresponda.
- [ ] T055 [A] Revisión final FR/SC y10 A-AC, aprobación ADR/TP11, coexistencia/migración/rollback y autorización de despliegue. A-AC11 solo si T056 se habilita. No exigir tareas B ni cerrar007/008/009.

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
