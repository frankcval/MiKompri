# Implementation Plan: 010-A — Microsoft existente + Google

**Branch**: `010-account-management-social-auth` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)
**Estado**: arquitectura 010-A aprobada; implementación autorizada, sin deploy ni merge.

## Summary

Microsoft actual sigue disponible; Google entra por un External ID y recibe token del backend. Users conserva UserId y una tupla canónica activa; perfil propio, sesión Web y confianza JWT comunes. [ADR-004](../../docs/adr/ADR-004-ciam-identity.md), [evidencia aplicada](evidence/010-a-scope-review.md).

Se elimina del camino crítico A el broker de métodos, G3, Authentication Context, CA de linking y la viabilidad de agregar métodos dentro de un objeto. Eso es [010-B](010-b-deferred.md). Ninguna evidencia anterior se borra ni se presenta como PASS distinto.

Email OTP es un adicional elegible de A: login básico probado en harness y mismo contrato, sin modelo nuevo. Se habilita solo si T056 acredita alta/login/errores/expiración/replay/contrato/sesión sin retrasar el núcleo. En caso contrario permanece diferido y no anunciado; no se bloquea Google.

## Technical Context

| Campo | Alcance mínimo |
|---|---|
| Stack | .NET8, React/TypeScript, EF/Npgsql y MSAL actuales; sin upgrade gratuito de stack |
| Identidad | Tupla issuer confiable + tid/oid → User.Id. Reutilizar User/índices existentes y ampliar solo cuando tests de transición/perfil lo exijan |
| Perfil | Username, DisplayName, nombres opcionales, PreferredLanguage/defaults y preferencias tipadas aprobadas; autoridad User prevalece. Iniciales; sin storage/fetch de foto |
| Persistencia | Users PostgreSQL propio; registro de migración acotado, no ExternalIdentities[] genérica ni almacén ProofReceipt en A |
| Clientes | Web Code+PKCE+MSAL browser-delegated. Mantener configuración Microsoft actual y CIAM conocida para Google; separar cachés/sesión. MAUI futuro, sin gate Native Auth para A |
| Confianza APIs | Una matriz compartida por las tres APIs: issuer/tenant/audience/scopes/clientes fijados por entorno; forwarding ShoppingList→Users compatible |
| Infraestructura | CIAM QA del spike reutilizado bajo autorización 010-A y [plan acotado](evidence/010a-qa-resource-plan.md). Sin configuración/migración productiva |
| Rendimiento | Conservar budgets003/007/008/009; medir D1 para acceso protegido. Sin Graph en requests de negocio, Redis, bus ni CAE |

## Constitution Check

PASS de aprobación arquitectónica explícita del titular para 010-A; TP11 1.2.0 aplicada y templates propagados. La evidencia funcional sigue pendiente.

| Principios | Aplicación |
|---|---|
| PP1/PP2 | MVP autónomo Microsoft+Google, sin espera por gestión de métodos B |
| TP1/TP2/TP3 | Backend .NET, Users dueño de identidad, contratos entre contextos, monorepo |
| TP4/TP5/TP7/TP8 | Docker/PostgreSQL, Azure, OpenAPI y tests antes de reglas, sin ejecución cloud por esta revisión |
| PP5/TP6 | Web móvil primero según evolución actual; MAUI sigue previsto y aplazado |
| TP9/TP10 | ADR/spec/plan/tasks antes de producción |
| TP11 | UserId, tuplas, issuer/audience común y permisos intactos; [enmienda propuesta](constitution-amendment-proposal.md) permite evolución incremental sin obligar linking |

## Project Structure

Documentación: spec/plan/tasks, [modelo](data-model.md), [migración](migration.md), [contratos](contracts/account-and-identity.md), research/architecture-review, checklists y evidencia histórica; 010-b-deferred delimita futuro.

Código A implementado en la rama: Users Domain/Users, Application/Commands/SyncProfile y UpdateProfile, Infrastructure/Persistence y migración acotada; Users Api middleware/ProfileController; Security/Program de tres APIs; Web auth/msal/session y perfil. `MiKompri.Identity` comparte únicamente validación técnica JWT/D1; no dominio ni acceso cross-DB. `tools/identity-migration` es un runner QA administrativo, sin endpoint público. [Validación y límites](evidence/validation.md). El harness previo sigue siendo evidencia histórica distinta.

Ruta actual: Microsoft legacy, Google por selector CIAM alojado, sin domain_hint tras AADSTS90023 reproducido. Se retiró el desvío experimental Microsoft→CIAM; no se expone cutover hasta acreditar origen y rollback. [Recuperación QA](evidence/authentication-recovery-2026-10-31.md).

## Entregas y orden

1. **A0 — aprobación:** T001/006–012 revisan ADR, TP11, confianza temporal y protocolo de traslado Microsoft. No requieren G3, pruebas de unlink ni Graph write para identities.
2. **A1 — login aislado:** T002/005/020/022–024. Microsoft actual + Google CIAM, token del recurso, sujeto estable y negativos. Puede comprobarse antes de integrar D1; no habilita listas privadas ni crea aceptación productiva.
3. **A2 — identidad y preservación:** T013–019 y T043/048. Mantener ruta actual mientras se prepara traslado por cohorte; reconocer origen Microsoft por identificador inmutable acreditado, reservar tupla destino y cambiar canónica del mismo UserId. Sin linking general ni G3. Si no se acredita origen, no mover esa cohorte; Google puede desarrollarse en paralelo, sin afirmar migración terminada.
4. **A3 — acceso protegido:** T021/026/041/042/051 aplican D1 antes de negocio: Users local autoritativo; ShoppingList/ProductCatalog por contrato mínimo, incluyendo GET. Sin allow cacheado, 403 suspendido/503 dependencia caída; conservar cortes y no resucitar tokens. No permitir producto protegido como una supuesta excepción de login.
5. **A4 — perfil y sesión:** T025/034–038/050/053. Inicialización única, perfiles anteriores personalizados, actualización autorizada/versionada, username distinto de login; logout/cambio de issuer/cuenta con requests tardías.
6. **A5 — aceptación/cierre:** T044–048/051/055, regresiones reales y CI del SHA candidato. Microsoft existente y Google nuevo en Web/APIs normales; traslado Microsoft y rollback acreditados antes de cerrar su cohorte. Email T056 solo si aprobado. No implementar B al cerrar A.

## Transición de emisores

Objetivo convergente: Microsoft/Google → External ID → APIs. Inicio seguro: Microsoft conserva su issuer/ruta actual; Google usa CIAM. Excepción legacy temporal por población/cohorte, un solo CIAM. Configuración por emisor evita aceptar old issuer con new audience y viceversa.

Una cuenta no tiene dos métodos de login gestionables en Users: su tupla canónica activa es única. El registro de traslado guarda origen/destino para validación/rollback, no autentica ambos indefinidamente ni crea catálogo de proveedores. Retiro legado solo tras criterios de [migración](migration.md).

## Tests y aceptación

Unitarios dominio/aplicación para correlación y perfil; JWT firmados en APIs; PostgreSQL para índices, provisión concurrente y reserva/cutover/rollback; regresiones propiedad/compras/gastos; Web MSAL/callback, aislamiento y móvil. Validar10 casos A y caso email solo si se habilita. No convertir evidencia harness en esos resultados.

D1 y sus AR-01–03 son gate de acceso protegido, no de primer login aislado. G1 de agregación, G3/AR-05–06 y AC de segundo método/último método/recovery no bloquean A; siguen abiertos en B.

## Complexity Tracking

| Necesidad | Solución mínima | Diferido |
|---|---|---|
| Nuevas cuentas Google | Resolver tupla CIAM en User existente/nuevo | Colección genérica de identidades |
| Microsoft existente | Ruta conservada + traslado por cohorte con origen acreditado y reserva | Broker autoservicio entre proveedores |
| Suspensión productiva | D1 por request, estado/cortes de cuenta y canónica actual | Redis/bus/CAE y MFA propia |
| Perfil | Campos propios, autoridad/versiones, iniciales | Avatar storage y reimportador UI |
| Recuperación | Proveedor original; sin sustitución propia | Linking/unlinking/recovery avanzada |

## Dependencies & Definition of Done

[Tasks](tasks.md) distingue A obligatorio, A opcional y B diferido; conserva IDs anteriores. A termina con aceptación real, confianza aprobada, UserId/ownership preservados y D1 productivo, no con placeholders ni ocultando features pendientes. B permanece pendiente tras cerrar A.

**Readiness documental actual:** **010-A READY FOR IMPLEMENTATION**. ADR/TP11/transición aprobados expresamente el 2026-10-30. No equivale a READY FOR PR: implementación/QA siguen pendientes. G3/CA/email opcional no bloquean.
