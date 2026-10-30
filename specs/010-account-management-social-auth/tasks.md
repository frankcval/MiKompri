# Tasks: 010 — Account Management & Social Auth

**Input**: [spec](spec.md), [plan](plan.md), [research](research.md), [modelo](data-model.md), [contratos](contracts/account-and-identity.md).

**Estado**: formalización documental; todas las tareas siguientes son trabajo/revisión futura y permanecen abiertas. Tener un artefacto no acredita su aprobación o un test real. No implementar ni configurar Azure/BD con esta ejecución.

**Tests**: TP8; dominio/caso de uso/API antes de modificar reglas, persistencia real para carreras/migración. [USx] remite a historias de spec. Paths de código nuevos son destinos planificados, no archivos existentes. No se delega ni ejecuta trabajo paralelo por marcar [P].

## Phase 1 — Research (gate G1–G6)

- [ ] T001 Ratificar auditoría/fuentes/versiones y límites de documentación en `current-state-audit.md` y `research.md`; registrar nueva fecha antes del spike.
- [ ] T002 [US1] Tras autorización QA, validar Google/email/Microsoft personal y Entra legado en `evidence/provider-capabilities.md`, sin secretos/PII.
- [ ] T003 [US3] Probar matriz A Google-only→email/email→Google, identidades en objetos distintos, Graph permisos y unlink/último método en `evidence/aggregation-spike.md`; no asumir identities==linking completo.
- [ ] T004 [US3] Acreditar G3 por método según D3/AR-05–06 en `architecture-review.md`: auth_time/max_age/challenge verificable, silent SSO Google/refresh, MFA histórica, OTP primario y límites passkey, timestamps/300s al confirmar, state/nonce/PKCE y recovery sin proveedor perdido; `evidence/assurance-spike.md`. Sin garantía, no habilitar mutación.
- [ ] T005 [US5] Revalidar SDK/metadata/claims/scopes: Web beta MSAL.js+Code/PKCE+browser-delegated; MAUI/MSAL.NET candidato browser sin fijar Native Auth antes de versión/SDK/redirect real. Registrar discrepancia oficial en `research.md`; AR-04, no crear app MAUI productiva.
- [ ] T006 Aprobar coste/dependencias/región/operación y finalidad/retención de datos en `research.md` y `privacy-and-deletion.md`.

## Phase 2 — Architecture (bloqueante)

- [ ] T007 Revisar `docs/adr/ADR-004-ciam-identity.md`; seleccionar A tras prueba positiva o B por insuficiencia documentada, sin mezclar modelos.
- [ ] T008 Revisar/aprobar `constitution-amendment-proposal.md`; solo con autorización posterior actualizar constitución/version/templates mediante flujo constitucional.
- [ ] T009 Ratificar `data-model.md`, `contracts/account-and-identity.md` y D1–D3 de `architecture-review.md`: opción 1 beta (estado autoritativo por request sin cache inter-request), dependencia/fail-closed ProductCatalog, cortes/versión, prueba por operación/300s y límites de SDK. Incluir errores, p95/disponibilidad y compatibilidad PUT/sync sin alterar budgets.
- [ ] T010 [US2] Aprobar cohortes/ventana/no migrados/cutover/rollback en `migration.md` antes de alterar confianza.
- [ ] T011 [US3] Revisión de seguridad del `threat-model.md` y procedimiento de recuperación sin métodos; no bypass por atributos.
- [ ] T012 [US4] Aprobar perfil mínimo/username/avatar y publicación en `data-model.md`; mantener DOB/teléfono/dirección sin recogida inicial.

## Phase 3 — Migration (US2, fundamento de identidad)

- [ ] T013 [US2] Escribir tests invariantes de binding/operación en `test/MiKompri.Users.Domain.Tests/IdentityBindingTests.cs` y `AccountOperationTests.cs` antes del dominio.
- [ ] T014 [US2] Escribir tests de migración/proof/provisioning en `test/MiKompri.Users.Application.Tests/Commands/MigrateIdentityTests.cs`, con email collision, fallback sub restringido y retries.
- [ ] T015 [US2] Implementar modelo aprobado en `MiKompri.Users.Domain/Users/` y resolución explícita en `MiKompri.Users.Application/Commands/SyncProfile/SyncProfileCommandHandler.cs`; no reemplazar UserId.
- [ ] T016 [US2] Preparar migración aditiva/índices/backfill reanudable en `MiKompri.Users.Infrastructure/Persistence/`, preservando columnas/tuplas y perfiles previos como personalizados.
- [ ] T017 [US2] Crear pruebas PostgreSQL de migración/carreras/constraints en `test/MiKompri.Users.Api.Tests/IdentityMigrationTests.cs`; usar instancia QA Docker, comparar manifiestos antes/después.
- [ ] T018 [US2] Separar resolver/alta/perfil y excluir candidates de auto-provisioning en `MiKompri.Users.Api/Middleware/UserProvisioningMiddleware.cs` y endpoints de operación aprobados.
- [ ] T019 [US2] Ensayar fallos por frontera y rollback de tráfico sin Down en `evidence/migration-rehearsal.md`; incluir usuarios no migrados y registros sin tupla canónica.

## Phase 4 — Authentication (US1/US5)

- [ ] T020 [US5] Tests firmados de issuer/tenant/audience/scope/cliente/claims en `test/MiKompri.Users.Api.Tests/Security/ExternalIdValidationTests.cs` y equivalentes ShoppingList/ProductCatalog; Google/Graph/ID tokens deben fallar.
- [ ] T021 [US5] Adaptar `MiKompri.{Users,ShoppingList,ProductCatalog}.Api/Security/*JwtValidation.cs` y sus Program con confianza explícita por emisor y scopes, sin cambiar permisos de negocio.
- [ ] T022 [US1] Tras autorización específica, preparar configuración CIAM/proveedores/cliente y recursos QA según `quickstart.md`; no versionar credenciales ni tocar registros productivos sin autorización.
- [ ] T023 [US1] Escribir tests Web de inicio Google/email/Microsoft, cancelación y errores en `MiKompri.Web/tests/unit/auth/external-id.spec.ts` y `tests/component/profile/`.
- [ ] T024 [US1] Adaptar `MiKompri.Web/src/auth/msal/msalConfig.ts`, `src/shared/config/env.ts` y entrada de login con MSAL/PKCE soportados, manteniendo secret-free SPA y opciones claras.
- [ ] T025 [US5] Probar y adaptar aislamiento/limpieza A→B con requests pendientes en `MiKompri.Web/tests/unit/auth/session.spec.ts` y `src/auth/session/`, incluyendo caché TanStack y borradores.
- [ ] T026 [US5] Tests primero y posterior integración del contrato mínimo Users access-status: lectura Users local, consumidores por bearer/UsersApi y reutilización solo request; ProductCatalog GET incluido, sin Graph ni loop/caché entre requests. AR-01–03 en `test/MiKompri.ShoppingList.Api.Tests/` y `test/MiKompri.ProductCatalog.Api.Tests/`; medir p95/SQL/RPC y fallos, sin accesos cross-DB.

## Phase 5 — Account linking, unlinking & recovery (US3)

- [ ] T027 [US3] Tests de pruebas por operación D3: origen+candidato link/migrate, alternativa superviviente unlink/recovery, sesión/propósito/expiry/replay/consentimiento/versión y edad al confirmar; `test/MiKompri.Users.Application.Tests/Commands/AccountOperationTests.cs` antes de handlers.
- [ ] T028 [US3] Tests API de actor A/B, IDOR, último método, concurrencia/indisponibilidad y errores genéricos en `test/MiKompri.Users.Api.Tests/AccountMethodsTests.cs`.
- [ ] T029 [US3] Implementar broker/adaptador de pruebas y handlers en `MiKompri.Users.Api/` y `MiKompri.Users.Application/Commands/AccountOperations/` según contrato aprobado, sin tokens en logs/body de negocio.
- [ ] T030 [US3] Implementar adapter CIAM A o binding B elegido en `MiKompri.Users.Infrastructure/Identity/`, con permisos mínimos, locks, idempotencia y reconciliación; no implementar ambos preventivamente.
- [ ] T031 [US3] Probar timeout antes/después de cada mutación CIAM/BD, identidad ocupada y colección Graph concurrente en `test/MiKompri.Users.Api.Tests/AccountOperationRecoveryTests.cs`.
- [ ] T032 [US3] Tests y UI gestionar/vincular/probar alternativa/desvincular en `MiKompri.Web/tests/component/profile/AccountMethods.spec.tsx` y `src/features/profile/`, restauración de foco/errores sin cambiar cuenta activa.
- [ ] T033 [US3] Implementar/ensayar recuperación con alternativa acreditada; negar sin pruebas conforme política aprobada, `evidence/recovery.md`. No crear un UserId nuevo al recuperar.

## Phase 6 — Profile (US4/US6)

- [ ] T034 [US4] Tests de autoridad/importación/personalización/clear/username único y normalizado en `test/MiKompri.Users.Domain.Tests/ProfileAuthorityTests.cs` y `UsernameTests.cs`.
- [ ] T035 [US4] Ampliar `MiKompri.Users.Domain/Users/User.cs`, modelo/persistencia de perfil y `Commands/UpdateProfile`, preservando UserId y procedencia existente.
- [ ] T036 [US4] Tests regresión login/GET/sync no sobrescribe DisplayName/email contacto/avatar propios en `test/MiKompri.Users.Api.Tests/ProfileApiTests.cs` y Application/SyncProfile tests.
- [ ] T037 [US4] Adaptar GET/PUT/sync/DTOs en `MiKompri.Users.Api/Controllers/ProfileController.cs` y `MiKompri.Users.Application/Dtos/`, con versiones, campos permitidos y transición legacy aprobada.
- [ ] T038 [US4] Tests/UI de perfil/username/login diferenciado, reimportación por campo y personalización en `MiKompri.Web/tests/component/profile/` y `src/features/profile/`.
- [ ] T039 [US4] Si se aprueba avatar propio inicial, pruebas de ownership/formatos/límites/EXIF/SSRF antes de almacenamiento y pipeline de imágenes Users; registrar contrato y coste en `data-model.md`. Si se difiere, explicitar alcance sin fingir avatar propio implementado.
- [ ] T040 [US6] Validar minimización y estados de eliminación/último Owner/historia en `privacy-and-deletion.md` y tests de política; no activar purga ni recolectar campos futuros.

## Phase 7 — Security & tests (transversal)

- [ ] T041 Recorrer cada amenaza de `threat-model.md` y vincular evidencia negativa/test concreto en `evidence/security.md`; revisión independiente.
- [ ] T042 Probar D1/AR-01–03: token emitido →commit suspensión →siguiente control rechazado en tres APIs, 503 sin Users, carrera de request en vuelo, binding alternativo y reactivación sin resucitar tokens anteriores/durante suspensión/mismo segundo. Añadir rates/anti-enumeración y logs sin secretos/PII; sin tests que supongan CAE o Graph por request.
- [ ] T043 [US2] Ejecutar regresiones Groups/GroupMembership, Owner/Admin/Member, listas/gastos/participantes/idempotencia de 005/008/009 y verificar referencias UserId antes/después.
- [ ] T044 Ejecutar suites Domain/Application/API con PostgreSQL, lint/typecheck/build/Web unit/component/integration y E2E controlados; registrar comandos/SHA/fallos en `evidence/validation.md`.
- [ ] T045 Validar migraciones pendientes/modelos EF, imágenes Docker y workflows Users/ShoppingList/ProductCatalog/Web/Resource del HEAD candidato; sin confundir CI de otro SHA.
- [ ] T046 Revisar permisos/rotación de credenciales CIAM/proveedores, CORS/redirects exactos, privacidad/retención y runbooks sin secretos; no exceder alcance de token/backend común.

## Phase 8 — Acceptance (gates de cierre)

- [ ] T047 [US1] Google real sin Microsoft y email real: AC-01/02; Microsoft adicional documentado, `evidence/real-provider-acceptance.md`.
- [ ] T048 [US2] Entra existente migra al mismo UserId y conserva listas/grupos/gastos; AC-03/04 y rollback con evidencia de cohorte QA.
- [ ] T049 [US3] Añadir método, email collision sin linking, último método y ataque B→A; AC-05/06/07/10 sobre proveedor/tenant reales además de negativos controlados.
- [ ] T050 [US4] Login real tras personalizar nombre/avatar no sobrescribe; AC-08, incluyendo clear y reimportación explícita por versión.
- [ ] T051 [US5] AC-09/11 y AR-01–04: tokens External ID/Google, suspensión/reactivación/fallo Users, ProductCatalog GET, cambio/logout y flujo Web browser-delegated, tres APIs y Web real.
- [ ] T052 [US3] AC-12: recuperación conserva UserId y método alternativo; documentar pérdida total sin bypass.
- [ ] T053 Validar 320/390/430, teclado/lector/zoom, errores/return intent y rendimiento sin modificar presupuestos existentes; evidencia humana independiente de mocks.
- [ ] T054 Revalidar diseño/versión MSAL.NET/SDK/redirect/deep links MAUI cuando se formalice 006; no fijar Native Authentication desde tutorial Kotlin/JS ni presentar maqueta como aceptación productiva. MAUI sigue fuera del slice Web.
- [ ] T055 Revisión de checklist, FR/SC, gates G1–G7, ADR/enmienda, costes y plan de retiro/no migrados. Autorizar despliegue/cutover por separado; no cerrar 007/008/009 por entregar 010.

## Dependencias y Definition of Done

T001–006 → T007–012 → T013–019 → T020–026 → T027–033 → T034–040 → T041–046 → T047–055. Las historias tienen pruebas independientes en QA, pero comparten arquitectura/trust aprobados. No mutar esquemas ni registrar credenciales antes de los gates. Avatar, borrado operativo, PII adicional y MAUI no se convierten en trabajo obligatorio por figurar como análisis.

Done de implementación futura: FR/SC acreditados, doce AC con evidencia real/negativa, migración/rollback sin cambio UserId ni duplicación financiera, ADR/enmienda aprobados, políticas de seguridad/privacidad operables y CI exacta verde. Done de esta ejecución: artefactos revisables y consistentes, research actual y gates explícitos; **ninguna tarea de implementación se marca completa**.
