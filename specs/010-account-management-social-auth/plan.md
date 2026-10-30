# Implementation Plan: 010 — Account Management & Social Auth

**Branch**: `010-account-management-social-auth` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)

**Estado**: plan propuesto para arquitectura; solo documentación en esta ejecución.

## Summary

Acceso de consumidores sin cuenta Microsoft mediante External ID, Google/email y Microsoft adicional. Preferencia A (CIAM agrega métodos) con gate G1 antes de elegir esquema definitivo; B mínimo solo por insuficiencia acreditada. UserId Guid permanece inmutable. Users orquesta mapeo/perfil/consentimiento, CIAM credenciales y tokens; tres APIs conservan permisos existentes. [Research y decisiones](research.md), [ADR-004](../../docs/adr/ADR-004-ciam-identity.md).

## Technical Context

| Campo | Contexto y cambio propuesto |
| --- | --- |
| Language/Version | C#/.NET 8 y React 18/TypeScript actuales; no upgrade de runtime aprobado aquí |
| Dependencies | ASP.NET JWT, EF/Npgsql, MediatR, MSAL Browser/React actuales; Graph adapter y MSAL compatible External ID a evaluar/pinar tras revisión, no instalados |
| Storage | Users PostgreSQL propio; cambios aditivos de identidad/perfil, pruebas efímeras protegidas y journal. Avatar storage solo si se aprueba su entrega. No DB compartida entre contextos |
| Testing | xUnit Domain/Application/API; JWT firmas/metadata controladas; PostgreSQL Docker para unicidad/migración; Vitest/RTL; Playwright controlado y separado de QA real CIAM |
| Platform | Web móvil/escritorio primero; .NET MAUI futuro sin implementación 006 |
| Performance | Mantener presupuestos existentes 003/007/008/009; no acreditar ni modificar sus gates. Medir coste de resolución de cuenta y dependencias antes de habilitar; no Graph por cada render |
| Constraints | Sin nuevos secretos/clientes/registros/tenant/DB hoy; no passwords propios; no Google JWT en APIs; no cambio de ownership ni permisos de grupo |
| Scope | Seis historias, 26 FR, ocho fases; gates G1–G7 abiertos y conocidos. Configuración/licencias/QA requieren autorización posterior |

## Constitution Check

**Antes y después del diseño:** PASS para formalización documental; **HOLD para implementar/cambiar identidad** hasta aprobar ADR/enmienda y resolver gates aplicables. No se afirma PASS operativo por una propuesta.

| Principios | Evidencia / gate |
| --- | --- |
| PP1–3 | Acceso sin Microsoft y preservación de compras/historia, slices de usuario verificables |
| PP4 | Actor UserId estable y auditoría mínima de linking/migración |
| PP5/TP6 | Web responsive primero; MAUI sigue previsto para Android y diferido |
| TP1–3 | Backend .NET, tres bounded contexts, Users por API, monorepo |
| TP4/5 | Pruebas/imagenes Docker y Azure como destino, sin aprovisionamiento en formalización |
| TP7/8 | Contratos propuestos/OpenAPI posterior, tests de dominio/caso de uso/API y negativos antes de reglas |
| TP9/10 | ADR propuesto y spec/plan/tasks antes de código |
| TP11 | [Mapa de 13 reglas y enmienda 1.2.0 propuesta](constitution-amendment-proposal.md). Identidad canónica/audience común intactas; nueva confianza/migración no autorizadas aún |

## Project Structure

```text
specs/010-account-management-social-auth/
  scope.md, spec.md, plan.md, tasks.md
  research.md, current-state-audit.md, data-model.md
  migration.md, threat-model.md, privacy-and-deletion.md
  constitution-amendment-proposal.md, quickstart.md
  contracts/account-and-identity.md
  checklists/requirements.md, checklists/acceptance.md
  evidence/formalization-review.md
docs/adr/ADR-004-ciam-identity.md        # Propuesto, no aceptado
```

Rutas futuras de implementación: Users Domain/Users (invariantes), Application/Commands (operaciones/perfil), Infrastructure/Persistence (migración), Infrastructure/Identity (adapter CIAM), Api/Security y Middleware (resolución/frescura); Web auth/session/msal y features/profile. `Security/*JwtValidation.cs`/Program en las tres APIs requieren revisión de trust/scope, sin cambiar negocio de ShoppingList/ProductCatalog. Son destinos propuestos, no archivos creados por esta ejecución.

## Fases, dependencia y entrega incremental

1. **Research:** ratificar fuentes y ensayar en QA después de autorización separada. Matriz A: nuevos Google/email, añadir método, colisiones, unlink/último método, recuperación, Graph/consistencia; Microsoft personal y legado; metadata/claims/assurance; SDKs Web/MAUI. No crear tenant por esta fase documental.
2. **Architecture:** aprobar enmienda/ADR, A o B tras G1, límites de perfil, TTL/frescura, política de recovery/PII/coste. Actualizar contratos y revisar amenazas. Base de dominio/test antes de persistencia y API.
3. **Migration:** expandir/backfill Users, introducir resolución antes de provisioning, journal y bootstrap intencional. Ensayo legacy/CIAM preserva IDs, rollback de tráfico sin Down. Matriz exacta old/new en tres APIs debe estar lista antes de cohorte.
4. **Authentication:** Google + email recomendados; Microsoft adicional, incluyendo legado. MSAL/PKCE, callbacks/return intent y sesión. Slice US1 verificable en QA sin activar migración masiva; usuarios nuevos y existentes tienen rutas separadas para evitar duplicación por operación.
5. **Account linking:** broker doble prueba/consentimiento, adapter A o bindings B aprobados, idempotencia y reconciliación. Añadir antes de permitir quitar; recovery alternativo; no ampliar a merge de UserIds.
6. **Profile:** ownership por campo, migrar perfiles anteriores a personalizados, username/nombres/idioma, contacto/procedencia y avatar seguro según alcance aprobado. Sin DOB/teléfono/dirección iniciales. PUT/sync existentes con transición de contrato documentada.
7. **Security & tests:** contratos JWT/scope, revocación/estado, pruebas adversarias, carreras/replay y regresiones relacionales/permisos/finanzas; matriz de fallos Graph/BD. Sin relajar gates anteriores.
8. **Acceptance:** QA real Google sin Microsoft, email, Microsoft y Entra legado, doce casos, accesibilidad/móvil, rollback, privacidad/operación. MAUI solo se verifica en la fase que le corresponda; no se fabrica aceptación productiva móvil para cerrar Web.

No paralelizar mutaciones de confianza/esquema antes de elegir A/B. Dentro de slices, tests preceden reglas y se integran por PR revisable. Cualquier prueba de proveedores real exige entorno autorizado, configuración/credenciales fuera del repo y evidencia sin PII. No activar switches de exposición hasta aprobación de gates.

## Regresiones y validación

Preservar grupos/membresías/roles y relaciones en fixtures antes/después. Incluir CommonAudienceValidationTests (ahora insuficientes para CIAM), middleware/profile/SyncProfile tests y web session recovery. Añadir PostgreSQL para constraints/backfill y contaminación de cuenta A→B con requests pendientes. Validación real no puede sustituirse por TestAuthHandler.

Build/lint/typecheck/suites, imágenes Docker y CI por SHA serán gates de implementación. La formalización actual se valida con prerrequisitos Spec Kit, enlaces, trazabilidad FR→tasks→aceptación y diff documental; no se ejecuta DB ni se reclama éxito de tests futuros. [Quickstart de revisión/QA](quickstart.md).

## Complexity Tracking

La complejidad justificada es puente de migración y operaciones seguras; no una plataforma genérica de identidad. Adapter Graph/almacén efímero/filas nuevas se mantienen condicionados a pruebas A. B aumenta bindings permanentes solo si demuestra necesidad. Borrado productivo, verificaciones de contacto propias y recopilación PII adicional necesitan decisión posterior. No hay excepción constitucional aceptada por este plan.
