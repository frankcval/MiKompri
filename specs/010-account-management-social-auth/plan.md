# Implementation Plan: 010 — Account Management & Social Auth

**Branch**: `010-account-management-social-auth` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)

**Estado**: plan propuesto para arquitectura; solo documentación en esta ejecución.

## Summary

Acceso de consumidores sin cuenta Microsoft mediante External ID, Google/email y Microsoft adicional. Preferencia A (CIAM agrega métodos) con gate G1 antes de elegir esquema definitivo; B mínimo solo por insuficiencia acreditada. UserId Guid permanece inmutable. Users orquesta mapeo/perfil/consentimiento, CIAM credenciales y tokens; tres APIs conservan permisos existentes. [Research y decisiones](research.md), [ADR-004](../../docs/adr/ADR-004-ciam-identity.md).

## Decisiones beta revisadas — D1/D2/D3

[Comparación y contrato de garantías](architecture-review.md): **opción 1**, lookup autoritativo Users por cada request autenticada, sin caché de autorización entre peticiones; Users local y consumidores por contrato mínimo. ProductCatalog adquiere dependencia de red/estado Users, también en GET; no se cambia su autorización de catálogo global. Estado suspendido 403, no resuelto por fallo 503; sin retries/replay de escrituras. Cota de dependencia propuesta 1s máximo configurable, no un SLO nuevo: medir contra budgets existentes antes de habilitar. Si fallan, revisar explícitamente garantía vs híbrido, no esconder un TTL. No Graph por request, Redis, bus de revocación, nuevos claims dinámicos o CAE como requisito beta. Reutilizar resultado únicamente dentro de request y verificar cortes para no revivir JWT anteriores al reactivar.

Web adopta **Code+PKCE+MSAL.js Browser/React+browser-delegated**; Native Auth JS no se implementa. MAUI futuro: candidato MSAL.NET/browser del sistema, sin fijar Native Auth hasta prueba de SDK/versión/redirects. D3 requiere evidencia CIAM fiable de control <=300s al confirmar; operación <=600s y matriz diferenciada por link/migrate/unlink/recovery. Solo abrir mutaciones cuyo G3 esté acreditado; OTP primario no es MFA, passkeys no son fallback universal y proveedor perdido no se exige como prueba en recovery. No se considera completada la aceptación de métodos por dejar sus mutaciones bloqueadas.

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
  architecture-review.md
docs/adr/ADR-004-ciam-identity.md        # Propuesto, no aceptado
```

Rutas futuras de implementación: Users Domain/Users (invariantes), Application/Commands (operaciones/perfil), Infrastructure/Persistence (migración), Infrastructure/Identity (adapter CIAM), Api/Security y Middleware (resolución/frescura); Web auth/session/msal y features/profile. `Security/*JwtValidation.cs`/Program en las tres APIs requieren revisión de trust/scope, sin cambiar negocio de ShoppingList/ProductCatalog. Son destinos propuestos, no archivos creados por esta ejecución.

## Fases, dependencia y entrega incremental

1. **Research:** ratificar fuentes y ensayar en QA después de autorización separada. Matriz A: nuevos Google/email, añadir método, colisiones, unlink/último método, recuperación, Graph/consistencia; Microsoft personal y legado; auth_time/max_age frente a SSO/refresh, assurance, límites passkey/OTP; SDKs Web/MAUI. Validar impacto de D1, especialmente ProductCatalog. No crear tenant por esta fase documental.
2. **Architecture:** aprobar enmienda/ADR, A o B tras G1, límites de perfil, TTL/frescura, política de recovery/PII/coste. Actualizar contratos y revisar amenazas. Base de dominio/test antes de persistencia y API.
3. **Migration:** expandir/backfill Users, introducir resolución antes de provisioning, journal y bootstrap intencional. Ensayo legacy/CIAM preserva IDs, rollback de tráfico sin Down. Matriz exacta old/new en tres APIs debe estar lista antes de cohorte.
4. **Authentication:** Google + email recomendados; Microsoft adicional, incluyendo legado. MSAL/PKCE, callbacks/return intent y sesión. Slice US1 verificable en QA sin activar migración masiva; usuarios nuevos y existentes tienen rutas separadas para evitar duplicación por operación.
5. **Account linking:** broker con pruebas/consentimiento según operación D3, adapter A o bindings B aprobados, idempotencia y reconciliación. Añadir antes de permitir quitar; unlink usa alternativa que permanece y recovery no exige proveedor perdido. No habilitar mutación sin G3; no ampliar a merge de UserIds.
6. **Profile:** ownership por campo, migrar perfiles anteriores a personalizados, username/nombres/idioma, contacto/procedencia y avatar seguro según alcance aprobado. Sin DOB/teléfono/dirección iniciales. PUT/sync existentes con transición de contrato documentada.
7. **Security & tests:** contratos JWT/scope, revocación/estado, pruebas adversarias, carreras/replay y regresiones relacionales/permisos/finanzas; matriz de fallos Graph/BD. Sin relajar gates anteriores.
8. **Acceptance:** QA real Google sin Microsoft, email, Microsoft y Entra legado, doce casos, accesibilidad/móvil, rollback, privacidad/operación. MAUI solo se verifica en la fase que le corresponda; no se fabrica aceptación productiva móvil para cerrar Web.

No paralelizar mutaciones de confianza/esquema antes de elegir A/B. Dentro de slices, tests preceden reglas y se integran por PR revisable. Cualquier prueba de proveedores real exige entorno autorizado, configuración/credenciales fuera del repo y evidencia sin PII. No activar switches de exposición hasta aprobación de gates.

## Regresiones y validación

Preservar grupos/membresías/roles y relaciones en fixtures antes/después. Incluir CommonAudienceValidationTests (ahora insuficientes para CIAM), middleware/profile/SyncProfile tests y web session recovery. Añadir PostgreSQL para constraints/backfill y contaminación de cuenta A→B con requests pendientes. Validación real no puede sustituirse por TestAuthHandler.

Build/lint/typecheck/suites, imágenes Docker y CI por SHA serán gates de implementación. La formalización actual se valida con prerrequisitos Spec Kit, enlaces, trazabilidad FR→tasks→aceptación y diff documental; no se ejecuta DB ni se reclama éxito de tests futuros. [Quickstart de revisión/QA](quickstart.md).

La revisión D1–D3 añade pruebas pendientes AR-01–06 al checklist: tokens emitidos antes/durante suspensión, reactivación/precisión de segundos, fallo Users, requests en vuelo, dos bindings, ProductCatalog GET y p95, fresh challenge vs silent SSO, pruebas expiradas a confirmación y proveedor perdido. No son tests ejecutados por editar documentación.

## Complexity Tracking

### Spike QA autorizado, separado de implementación

Se ha autorizado D1–D3 solo para el spike y preparación QA mínima, con [plan de recursos previo](evidence/spike-resource-plan.md) y [evidencia](evidence/spike-summary.md). Harness en `tools/spikes/010-external-id/`, fuera de solución/APIs/Web productivos; no migraciones EF ni escritura en BD MiKompri. Config pública local ignorada; secretos/tokens fuera de evidencia.

El adapter futuro debe distinguir email OTP observado como federated/mail de emailAddress/password; no fijar representación Graph a partir de etiquetas de UI. La colección permite ambos métodos ficticios bajo mismo oid, pero último método/concurrencia requieren guardia propia, y falta linking con Google real/G3. No pasar a Phase3–8 por tener fuentes/probes. D1 permanece hipótesis operativa apoyada por benchmark local, no garantía medida en cloud.

Continuación: Google sin Microsoft ya accede al harness con token CIAM validado; token Google directo rechazado401. La reautenticación Google ensayada emite auth_time nuevo tras solo selección/consentimiento, por lo que es NOT RELIABLE FOR G3. Priorizar revisión de desafío verificable CIAM, comportamiento login_hint y expiración del retorno antes de mutar/linkar objetos reales. Esta observación no autoriza políticas nuevas ni rebaja300/600s; no iniciar Phase3–8 ni B. [Evidencia](evidence/assurance-spike.md).

Siguiente ensayo diseñado, no ejecutado: [contexto CA y desafío CIAM](evidence/g3-step-up-review.md), reutilizando QA/harness y como máximo un contexto/política propios tras autorizar configuración. Primero verificar compatibilidad email OTP secundario para Google sin reemplazar OTP primario; SMS requiere decisión aparte. Probar política ausente/deshabilitada, MFA previa bajo tolerancia5min, bootstrap seguro y tiempo firmado del factor ligado a operación. `acrs`+token reciente no son PASS. D3 se conserva; si la ruta no satisface su frescura, volver a arquitectura antes de implementar.

La complejidad justificada es puente de migración y operaciones seguras; no una plataforma genérica de identidad. Adapter Graph/almacén efímero/filas nuevas se mantienen condicionados a pruebas A. B aumenta bindings permanentes solo si demuestra necesidad. Borrado productivo, verificaciones de contacto propias y recopilación PII adicional necesitan decisión posterior. No hay excepción constitucional aceptada por este plan.
