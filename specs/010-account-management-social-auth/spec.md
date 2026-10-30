# Feature Specification: 010 — Account Management & Social Auth

**Feature Branch**: `010-account-management-social-auth`
**Created / Revised**: 2026-10-30
**Status**: Approved 010-A — arquitectura y TP11 1.2.0 aprobadas; implementación en curso, aceptación pendiente.
**Input**: conservar Microsoft existente y añadir Google sin cuenta Microsoft. Esta ejecución modifica documentación, sin implementar ni configurar servicios.

## Contexto, evidencia y alcance

[TP11 vigente](../../.specify/memory/constitution.md), [ADR propuesto](../../docs/adr/ADR-004-ciam-identity.md), [auditoría](current-state-audit.md), [research](research.md), [migración](migration.md).

**010-A** entrega login, identidad estable, perfil básico y sesión Web. **010-B** difiere gestión de métodos y sus gates; ver [alcance diferido](010-b-deferred.md). No son dos bounded contexts ni dos sistemas de autenticación.

El [spike](evidence/spike-summary.md) acredita Google sin Microsoft, JWT CIAM, tid/oid y rechazo Google directo en harness; email OTP básico también funciona. No acredita provisión/ownership en la aplicación productiva ni G3/linking seguro. La [restauración QA](evidence/g3-restoration-verification.json) permanece como evidencia. G3 no bloquea010-A porque sus operaciones sensibles no se ofrecen; no se declara resuelto ni se rebaja.

`UserId : Guid` es canónico e inmutable en OwnerId, GroupMembership.UserId, AddedBy, PaidBy, PurchasedBy, listas, gastos, compras, historial y auditoría. Users posee la correlación; cada contexto conserva autorización de negocio.

### Proveedores y producto

| Prioridad | Decisión010-A |
|---|---|
| Microsoft/Entra existente | Obligatorio: mantener ruta/tipos de cuenta actualmente admitidos y mismo UserId durante transición |
| Google | Obligatorio, opción prioritaria visible: Google → External ID → token MiKompri → (tid, oid) → Users → UserId |
| Email OTP | Elegible como opción adicional del mismo010-A, no obligatorio. Mismo CIAM/contrato/alta simple; sin linking ni G3. T056 exige QA básico completo antes de habilitarlo. Si no pasa o retrasa Microsoft+Google, se difiere sin bloquearlos |
| Otros | Diferidos. No prometer MSA personal adicional si el acceso actual no lo admite; no activar proveedores por disponibilidad teórica |

Web muestra «Continuar con Google» primero y «Continuar con Microsoft» siempre disponible. «Continuar con email» solo tras aprobar T056; tampoco anunciar métodos no acreditados en el flujo del IdP. No hay botones de vincular/desvincular/cambiar proveedor. Un usuario existente debe continuar con Microsoft; no prometer deduplicación entre proveedores ni auto-link por email.

## User Scenarios & Testing *(mandatory)*

### US1 — Entrar con Microsoft o Google (P1,010-A)

**Independent Test**: usuario Microsoft actual conserva acceso; Google nuevo sin Microsoft usa la Web y una lista propia con identidad de dominio.

1. **Given** A solo tiene Google, **When** elige Google, **Then** External ID emite el access token MiKompri y Users crea/resuelve su UserId sin cuenta Microsoft.
2. **Given** usuario Microsoft existente, **When** usa Microsoft, **Then** conserva acceso y exactamente su UserId y datos. El botón no lo redirige automáticamente a crear otra cuenta CIAM.
3. **Given** email OTP aprobado y habilitado, **When** una persona nueva usa email, **Then** CIAM gestiona el OTP y se aplica la misma correlación; sin contraseña propia ni linking.

### US2 — Transición Microsoft conservando mi cuenta (P1,010-A)

**Independent Test**: inventario A/ownership → transición controlada Microsoft → mismo A → rollback sin reescribir relaciones.

1. **Given** (oldTid, oldOid) → A, **When** sigue usando la ruta actual, **Then** A no cambia.
2. **Given** cohorte Microsoft autorizada y equivalencia de origen acreditada por identificadores inmutables del proveedor, **When** se cambia al objeto CIAM reservado, **Then** (externalTid, externalOid) → A, sin INSERT de otro User ni inferencias por email.
3. **Given** prueba de origen insuficiente, identidad destino ocupada o reintento incierto, **When** se prepara el traslado, **Then** se detiene el cutover, conserva Microsoft existente y no aprovisiona un sustituto.

La transición administrada del mismo origen Microsoft no es linking general ni pantalla de cambio de proveedor. Su protocolo y ensayo son obligatorios antes de mover una cohorte; no dependen del broker G3 de010-B.

### US3 — Gestionar métodos (P1 de010-B, diferida)

Añadir segundo método, link/unlink, último método, cambio de proveedor y recuperación por alternativa requieren la [especificación diferida](010-b-deferred.md). Sin UI/endpoints en010-A. Su aceptación no se marca completada por ocultarlos.

### US4 — Perfil propio (P2,010-A)

**Independent Test**: importar «Frank Carlos», editar a «Frank» y repetir login/sync conservando «Frank».

Username opcional público, DisplayName, GivenName/FamilyName opcionales, PreferredLanguage y preferencias funcionales tipadas acordadas. Username no es login. Iniciales como avatar/fallback; foto propia, almacenamiento y reimportación explícita desde proveedor se difieren. Claims inicializan una vez campos permitidos; personalizaciones y borrados voluntarios prevalecen, incluidos perfiles anteriores tratados como propios.

### US5 — Sesión y APIs (P1,010-A)

**Independent Test**: JWT válido en tres APIs; Google/Graph/ID token y scope/issuer/audience incorrectos rechazados; logout/cambio A→B sin datos A.

D1 aplica antes de liberar acceso productivo protegido. Un harness aislado de login puede ejecutarse antes de integrar D1; no equivale a habilitar negocio.

### US6 — Privacidad (010-A) y eliminación (010-B)

No solicitar nacimiento, teléfono/dirección ni crear columnas por utilidad futura. Recuperación de acceso se delega al proveedor original; si no se recupera allí, MiKompri no sustituye la identidad ni promete acceso por otro proveedor. Estrategia de eliminación conservada en [privacidad](privacy-and-deletion.md); endpoint/purga diferidos.

### Edge Cases

Google cancelado/consentimiento denegado, primer registro concurrente, mismo email en cuentas distintas, claims ausentes/cambiados, usuario Microsoft con canonical tuple incompleta, fallback sub limitado al legado, destino reservado/ocupado, timeout/rollback con nuevas compras, issuer/cuenta cambiados y respuesta tardía de la sesión anterior. G3, último método y carreras de Graph identities pertenecen a010-B.

## Requirements *(mandatory)*

### Functional Requirements

Los IDs se conservan para trazabilidad; A/B indica aplicación efectiva actual.

- **FR-001 [A]** MUST conservar Microsoft existente y ofrecer Google sin Microsoft; email opcional solo tras T056.
- **FR-002 [A/B]** MUST usar un único External ID para nuevos accesos.010-A prefiere correlación simple por tupla, sin colección genérica ExternalIdentity. G1 de agregación/linking no bloquea A; B arquitectónica solo tras insuficiencia demostrada y otro ADR.
- **FR-003 [A]** MUST preservar UserId y referencias de dominio; correlación exclusivamente en Users, sin cross-DB.
- **FR-004 [A/B]** MUST usar issuer/tenant/oid validados; nunca email, nombre, username, teléfono, avatar o sub global para correlacionar personas.
- **FR-005 [A]** MUST separar resolución, nueva alta intencional y perfil. Una migración Microsoft reservada no ejecuta auto-provisión genérica; una identidad desconocida no puede apropiarse de A.
- **FR-006 [B]** Pruebas recientes D3 para linking/unlinking/cambio de método/migración autoservicio entre proveedores: <=300s al confirmar, operación<=600s, sesión/propósito/replay. Diferido; no prerrequisito de login A ni del traslado Microsoft administrado de FR-010.
- **FR-007 [A/B]** MUST rechazar identidad destino ligada a otro UserId. No account merging.
- **FR-008 [B]** MUST impedir unlink del último método y carreras antes de ofrecerlo.010-A no tiene unlink.
- **FR-009 [A/B]** A: traslado por cohorte versionado/idempotente y auditado; B: broker/AccountOperation/ProofReceipt y reconciliación de cambios de métodos. No construir estos últimos para login.
- **FR-010 [A]** MUST diseñar/ensayar [transición](migration.md) con origen Microsoft inmutable acreditado, reserva de destino, mismo UserId y rollback. Si falta equivalencia, conservar ruta actual; no automatizar traslado ni crear un nuevo User.
- **FR-011 [A]** MUST mantener ruta legado durante coexistencia acotada y retirar solo cohortes aprobadas. Objetivo final Microsoft/Google → CIAM; excepción temporal de issuer legado explícita, no múltiples CIAM permanentes.
- **FR-012 [A]** MUST delegar credenciales/reset/recuperación al proveedor/CIAM. Sin passwords ni MFA propios. Email OTP básico no necesita linking/G3 para nueva cuenta.
- **FR-013 [A]** MUST soportar perfil básico propio, username separado de login, nombres opcionales, idioma soportado y preferencias tipadas con finalidad actual; iniciales, sin storage de avatar en A.
- **FR-014 [A/B]** A: claims inicializan y no sobrescriben personalizaciones posteriores. B: avatar propio/proveedor y UI de reimportación explícita por campo, si se aprueban.
- **FR-015 [A]** MUST distinguir email de login y dato de contacto existente/procedencia; no tomar claim email/upn como prueba de identidad/verificación ni como recovery propio.
- **FR-016 [A]** MUST minimizar PII; sin nacimiento, teléfono/dirección o campos vacíos preventivos.
- **FR-017 [A]** MUST autorizar perfil por UserId resuelto y versión; cuerpo no acepta IDs externos, autoridad/verificación ni permisos.
- **FR-018 [A]** MUST usar Web MSAL + Authorization Code/PKCE browser-delegated, redirects exactos, SPA sin secretos y aislamiento de caché/token/requests por sesión/issuer.
- **FR-019 [B/futuro]** MAUI conserva compatibilidad de dominio/contratos; SDK/native auth/deep links reales se validarán en006. No bloquea A Web.
- **FR-020 [A]** MUST validar issuer, audience, firma/algoritmos, expiration, scope delegado, tid y oid en Users/ShoppingList/ProductCatalog. Google directo, Graph e ID tokens no son bearer de negocio.
- **FR-021 [A]** MUST mantener recurso lógico backend común. Durante coexistencia: pares issuer→tenant→audience/scopes/clientes explícitos, nunca producto cartesiano ni tenant descubierto del JWT.
- **FR-022 [A]** MUST preservar Owner/Admin/Member, membresías y contratos de compras/gastos; no roles sociales/directorio como permisos de negocio.
- **FR-023 [A protegido]** MUST aplicar [D1](architecture-review.md#d1--suspensión-y-access-token-ya-emitido) al liberar negocio: estado Users autoritativo por request, sin cache inter-request, fallo503/denegación403 y cortes anti-resurrección. No bloquea primer login aislado. Sin Graph por request, Redis, bus o CAE.
- **FR-024 [A/B]** A: PKCE/state/nonce/CSRF/callbacks y errores/logs seguros. B añade anti-replay y pruebas de operaciones sensibles.
- **FR-025 [A/B]** A mantiene minimización/diseño de conservación; borrado operativo y recuperación avanzada diferidos.
- **FR-026 [A]** MUST respetar TP11 y aprobación explícita de ADR/enmienda antes de cambiar identidad productiva. Esta revisión no modifica constitución, código, Azure o BD.

### Key Entities

A: User/perfil y su tupla canónica actual, autoridad de campos y registro acotado de traslado Microsoft. D1 añade estado/cortes mínimos. B: AccountOperation/ProofReceipt, inventario de métodos y, solo con decisión nueva, múltiples bindings. [Modelo](data-model.md).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001 [A]** Diez [aceptaciones A obligatorias](checklists/acceptance.md) acreditadas en candidato real. A-AC11 solo si email se habilita; los AC diferidos no bloquean A ni se dan por completados.
- **SC-002 [A]** Igualdad exacta de UserId/referencias Microsoft antes/después de transición/rollback; cero duplicados para una migración autorizada y cero ownership huérfano.
- **SC-003 [A]** Negativos de identidad/issuer/audience/scope/cuenta ajena y conflictos de migración rechazan. Pruebas de último método/G3 quedan en B.
- **SC-004 [A]** Campos propios e iniciales derivadas de DisplayName propio sobreviven login/sync sin sobrescritura externa.
- **SC-005 [A protegido]** Tres APIs, D1 y regresiones pasan, manteniendo budgets existentes. El benchmark local no acredita cloud.
- **SC-006 [A]** Logout/cambio con requests pendientes no mezcla tokens/datos; Web móvil primero, teclado/lector/zoom.
- **SC-007 [A/B]** A comunica recuperación en proveedor original y no ofrece reasignación de identidad. Recuperación con alternativa es aceptación B.
- **SC-008 [A]** ADR/TP11 y transición aprobados antes de nueva confianza; despliegue/cutover autorizados separadamente. No depende de G1 de linking ni G3.

## Assumptions, Dependencies & Out of Scope

Depende de002/listas,003/correlación y perfil,004/catálogo,005/gastos,007/MSAL,008/autorización y009/compras con UserId. No cambia sus tareas ni gates.006/MAUI permanece diferida.

Fuera de010-A: segundo método, link/unlink, cambio de proveedor, merging, recovery entre proveedores, G3/Authentication Context/CA de linking, SMS/passkeys/Apple/Facebook, Alternative B, passwords/MFA propios, almacenamiento/avatar, eliminación operativa, chat/pagos y funciones de ShoppingList/ProductCatalog. Investigación y tareas futuras se conservan. [Decisión incremental](evidence/010-a-scope-review.md).
