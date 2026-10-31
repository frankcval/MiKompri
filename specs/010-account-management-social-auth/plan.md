# Implementation Plan: 010-A beta — Google CIAM + Microsoft legacy

**Branch**: `010-account-management-social-auth` | **Revised**: 2026-10-31 | **Spec**: [spec.md](spec.md)
**Decisión aprobada**: Google sin cuenta Microsoft es el objetivo prioritario; Microsoft actual sigue operativo. Microsoft→External ID se difiere y no es gate beta. Sin merge, deploy ni nueva migración por esta revisión.

## Summary

Google → un único External ID → access token MiKompri → Users → UserId interno → tres APIs. Microsoft conserva issuer/registro/configuración actuales y exactamente su UserId. Perfil propio y D1 por request permanecen obligatorios. [ADR-004](../../docs/adr/ADR-004-ciam-identity.md), [decisión/evidencia](evidence/010-a-scope-review.md).

## Technical Context

| Campo | Alcance beta |
|---|---|
| Stack | .NET8, React/TypeScript, EF/Npgsql y MSAL existentes |
| Identidad | Issuer confiable + tid/oid → UserId Guid inmutable, correlación solo Users; sin adopción por email/sub global |
| Perfil | Username/DisplayName/nombres opcionales/idioma/orden tipado; claims inicializan según disponibilidad y no pisan datos/clear propios; iniciales |
| Persistencia | Schema aditivo existente. Journal/reservation, tests y runner de migración conservados sin nuevo cutover ni eliminación |
| Web | Code+PKCE+MSAL.js browser-delegated; Google prioritario y Microsoft disponible, sesión por issuer/cliente/tenant/cuenta |
| APIs | Matriz explícita issuer/tenant/audience/scopes/clientes; un recurso backend lógico, no producto cartesiano ni Google/Graph/ID como bearer |
| D1 | Estado autoritativo Users por request, GET catálogo incluido;403 suspendido/401 token viejo/503 Users caído; sin caché inter-request/Graph/Redis/bus |
| Entorno | QA local funcional conservado; npm run dev:010a carga configuración privada. No restaurar automáticamente un backend antiguo incompatible |
| Rendimiento | Budgets003/007/008/009 intactos; mediciones D1 locales no son SLO cloud |

## Constitution Check

TP11 **1.2.0 sigue aplicada**. No enmienda/reversión: permite coexistencia temporal explícita; los requisitos de origen inmutable/reserva/UserId/referencias/rollback se aplican cuando se ejecute la migración futura. No obligan a migrar antes de beta. Users conserva dominio y cada contexto sus permisos Owner/Admin/Member.

PP1/PP2: beta autónoma Google+legacy. TP1–TP5/TP7/TP8: .NET, contextos, monorepo, Docker/PostgreSQL, OpenAPI y tests ejecutados. PP5/TP6: Web mobile-first, MAUI aplazado. TP9/TP10: Spec Kit/ADR/evidencia trazables; no cerrar007/008/009.

## Project Structure

[Modelo](data-model.md), [contratos](contracts/account-and-identity.md), [migración futura](migration.md), [tasks](tasks.md), [aceptación](checklists/acceptance.md), research/architecture-review y evidencia histórica conservados.

Código ya implementado: Users Domain/Application/Infrastructure/API, JWT/Program de tres APIs, MiKompri.Identity técnico compartido y Web auth/msal/session/perfil. tools/identity-migration y journal IdentityMigrations siguen preparados y probados técnicamente; no se retiran por diferir migración. No nuevas funcionalidades de010-B.

## Entregas y orden

1. A0: aprobación ADR/TP11/coexistencia y decisión beta31/10. Auditoría, coste/minimización/amenazas.
2. A1: Google sin Microsoft y Microsoft actual; token del recurso, alta/resolución canónica y negativos. EmailT056 opcional no anunciado.
3. A2 beta: UserId inmutable, alta intencional única, resolución separada y guard contra apropiación; regresiones de ownership. Infraestructura Microsoft futura se conserva.
4. A3: tres APIs protegidas y D1 completo antes de negocio. Estado/revocación de cuenta actual; caso de canónica trasladada se verifica en migración posterior.
5. A4: perfil propio, clear y ambos cambios reales Google↔Microsoft; cobertura automatizada de caché/drafts/requests tardías/generación/cuenta/tenant. UX básica320/390/430/teclado/targets/errores/loading/cancelación.
6. A5: ocho A-AC obligatorios y doce gates de producto, escaneo de secretos/PII y CI exacta final; T055 evalúa este alcance. Preparar/actualizar PR sin merge ni despliegue.

## Transición futura de emisores

Coexistencia actual aprobada como rollout transitorio: Google usa un CIAM y Microsoft mantiene legacy. Sin fecha de retiro inventada ni múltiples CIAM permanentes. El retiro legacy depende de una iniciativa posterior autorizada con A-AC06/07 y T019/T048, no del cierre beta.

[Runbook](migration.md) y [ensayo INCONCLUSIVE](evidence/migration-rehearsal.md) conservados. Federar Microsoft, preparar destino, reservar, cutover/rollback reales y comparar manifiestos son DEFERRED MICROSOFT MIGRATION. No intentar repararlos durante esta entrega ni reemplazar equivalencia por email/linking.

## Tests y aceptación

Se reutilizan resultados ejecutados si los árboles de código relevante son idénticos: .NET409 (PG7), Web138/auth49, JWT negativos, D1 real200→403→401→200 con iat posterior y Users caído503, perfil real con clear/relogin y ambos sentidos sin mezcla. La CI del SHA publicado debe volver a terminar success en los cinco workflows; no usar solo un SHA anterior. [Validación](evidence/validation.md).

Alta Google nueva ya acreditada en Web normal; una repetición con otra cuenta nunca registrada y nuevos atributos se difiere si requiere otro tester/autenticación del titular no disponible. La decisión de suficiencia se documenta, sin inventar importación de nombres. Unknown/claims ausentes se conservan como limitación, no backfill administrativo.

D1 no se repite con suspensiones destructivas si su código/configuración no cambió. Matriz manual multi-tab exhaustiva, lector/zoom completos e importación opcional de nuevos atributos son QA posterior no bloqueante; pruebas automatizadas y controles básicos deben pasar.

## Dependencies & Definition of Done

Obligatorias: A-AC01–05/08–10 y doce gates de producto aprobados. A-AC06/07 conservan estado DEFERRED — Microsoft External ID migration, nunca PASS. T019/T048 y T053/059/060 permanecen abiertos en sus categorías, sin bloquear beta. EmailT056 sigue A-OPT. Todo010-B permanece abierto.

Sin linking/unlinking/segundo método/cambio de proveedor/G3/Authentication Context/recovery entre proveedores/MFA avanzada/SMS/passkeys/Apple/Facebook/avatar storage/MAUI. Grupos/invitaciones se tratarán en012-group-invitations-collaboration, fuera de010.

Cierre beta habilita revisión de PR; no autoriza merge, configuración productiva, despliegue ni migración Microsoft.
