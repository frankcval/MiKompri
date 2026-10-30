# Feature Specification: 010 — Account Management & Social Auth

Estado de verificación posterior a la formalización: [spike QA controlado](evidence/spike-summary.md), autorizado separadamente para D1–D3 y recursos QA mínimos. No implementación productiva ni aceptación de G1/G3 por existir artefactos. FR/SC de esta especificación no se rebajan; constitución y UserId canónico intactos.

**Feature Branch**: `010-account-management-social-auth`

**Created**: 2026-10-30

**Status**: Draft — architecture review; implementación no autorizada

**Input**: CIAM para usuarios finales: Google/email/Microsoft, migración conservando UserId, métodos de acceso y perfil propio. Solo artefactos, sin código/Azure/credenciales/BD.

## Contexto y autoridad

Base `origin/main` `1a05d8e`, PRs #57/#58 integrados. [TP11](../../.specify/memory/constitution.md) permanece vigente; su [enmienda propuesta](constitution-amendment-proposal.md) no se aplica todavía. [Auditoría](current-state-audit.md), [research oficial](research.md), [ADR propuesto](../../docs/adr/ADR-004-ciam-identity.md). Alta en el directorio CIAM y alta de usuario MiKompri son operaciones distintas.

`UserId : Guid` MUST ser canónico e inmutable para OwnerId, GroupMembership, AddedBy, PaidBy, PurchasedBy, auditoría, listas, gastos e historial. IdP autentica; Users correlaciona y posee perfil; cada bounded context autoriza. No sustituir esas referencias por identificadores externos.

## User Scenarios & Testing *(mandatory)*

### US1 — Entrar sin cuenta Microsoft (P1)

Google y email eliminan la barrera de acceso actual. Microsoft es adicional; Apple/otros son evaluación futura.

**Independent Test**: cuenta Google real sin Microsoft → autenticación → lista personal con access token final External ID para MiKompri.

**Acceptance Scenarios**:

1. **Given** A solo tiene Google, **When** elige «Continuar con Google», **Then** Google autentica, External ID emite el token del backend y Users resuelve `(externalTid, externalOid)` a UserId; A utiliza MiKompri sin crear cuenta Microsoft.
2. **Given** usuario nuevo, **When** completa registro/acceso por email, **Then** CIAM gestiona la prueba y credenciales; MiKompri no almacena contraseñas.
3. **Given** Microsoft habilitado, **When** selecciona esa opción, **Then** el flujo distingue cuenta personal MSA de organización Entra; ninguna es obligatoria para Google/email.

### US2 — Migrar conservando mi cuenta (P1)

Protege propiedad e historia. **Independent Test**: fixture UserId=A y relaciones conocidas; migrar y revertir verificando igualdad exacta de IDs/referencias.

1. **Given** `(oldTid, oldOid) → A`, **When** acredita recientemente ambas identidades y confirma migración, **Then** `(externalTid, externalOid) → A`, sin crear otro UserId ni reescribir relaciones de dominio.
2. **Given** no ha migrado, **When** accede durante coexistencia aprobada, **Then** conserva su acceso legado y puede iniciar migración.
3. **Given** mismo email sin prueba o identidad candidata ligada a otro usuario, **When** solicita vincular, **Then** no se relacionan cuentas ni se fusionan sus datos.

### US3 — Añadir, quitar y recuperar métodos (P1)

Evita dependencia de un proveedor sin introducir takeover. **Independent Test**: Google → añadir email/otro método → probarlo → quitar Google; negativos concurrentes y de suplantación.

1. **Given** sesión A, **When** elige vincular, **Then** ambas identidades requieren prueba reciente, transacción ligada a sesión/propósito y consentimiento; resultado conserva UserId.
2. **Given** último método válido, **When** intenta quitarlo —también concurrentemente—, **Then** se rechaza hasta probar otro método primario utilizable.
3. **Given** pierde proveedor pero conserva un método aprobado, **When** recupera acceso, **Then** conserva UserId. Email/nombre por sí solos no recuperan una cuenta sin métodos.

### US4 — Perfil propio y username público (P2)

Separa identidad de acceso y presentación. **Independent Test**: importar «Frank Carlos», personalizar «Frank» y avatar, repetir login/sync.

1. **Given** primer registro, **When** hay claims permitidos, **Then** pueden inicializar perfil con procedencia; ausencia usa fallback neutro.
2. **Given** `@frankcv`, **When** accede con Google/email, **Then** username público no es login ni clave de correlación.
3. **Given** nombre/avatar personalizados, **When** cambian en el proveedor, **Then** no se sobrescriben; reimportar exige elección explícita por campo.

### US5 — Sesión y APIs seguras (P1)

Mantiene límites de dominio. **Independent Test**: matriz JWT en las tres APIs y cambio A→B con consultas pendientes.

1. **Given** access token CIAM para backend/scope correcto, **When** llama APIs, **Then** validan confianza y aplican los permisos existentes al mismo UserId.
2. **Given** token Google directo, token Graph, ID token SPA, scope incorrecto o issuer/audience cruzados, **When** llama negocio, **Then** se rechaza.
3. **Given** logout/cambio A→B, **When** terminan peticiones A pendientes, **Then** no aparecen datos, borradores, caché o tokens A en B.

### US6 — Privacidad y salida de la cuenta (P2)

**Independent Test**: usuario propietario de grupo con gastos compartidos solicita salida; verificar el plan de datos sin destruir historia ajena.

1. **Given** datos opcionales, **When** usa la app, **Then** no necesita nacimiento, teléfono ni dirección para comprar.
2. **Given** solicita eliminación, **When** existe historia/propiedad compartida, **Then** se distingue bloqueo, perfil/identidades y conservación/anonimización autorizada. Ejecución de borrado no se habilita en el primer incremento sin política aprobada.

### Edge Cases

Cancelación, timeout OAuth/Graph, consentimiento denegado, Google distinto al esperado, email cambiado/reciclado/ausente, Apple relay, candidato existente solo en CIAM, dos UserIds ya provisionados, carreras link/unlink/migración, token viejo, frescura no acreditada, legado sin tid/oid, pérdida de todos los métodos, MFA no equivalente a login primario y rollback con escrituras posteriores.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: MUST ofrecer Google y email sin cuenta Microsoft; Microsoft adicional. Apple/otros se investigan sin activación automática.
- **FR-002**: MUST preferir External ID agregador (A), sujeto a prueba de métodos/linking; B solo se elige por insuficiencia demostrada de A y ADR revisado. Ambas opciones usan tokens CIAM en APIs.
- **FR-003**: MUST preservar UserId y todas sus referencias. Users será propietario exclusivo del mapeo, sin acceso cruzado a su BD.
- **FR-004**: MUST correlacionar tenant/object del emisor validado; MUST NOT usar email, username, teléfono, nombre, avatar o `sub` global para linking/correlación.
- **FR-005**: MUST separar resolución, alta intencional y perfil. Linking/migración no pueden ejecutar aprovisionamiento genérico de la identidad candidata.
- **FR-006**: MUST exigir consentimiento y control reciente verificable según [D3](architecture-review.md#d3--control-reciente-y-step-up-para-beta): origen+candidato para linking/migración; alternativa ya vinculada que permanece para unlink/recovery, sin exigir proveedor perdido. Cada prueba ligada a operación/sesión/propósito, de un solo uso y edad <=300s al confirmar; TTL de operación <=600s no amplía frescura. Sin evidencia CIAM fiable, mutación bloqueada. El servidor deriva el UserId destino; token nuevo/iat/prompt/MFA histórica no prueban reauth.
- **FR-007**: MUST rechazar identidad asociada a otro UserId; fusionar usuarios existentes queda fuera de alcance.
- **FR-008**: MUST bloquear último método y carreras; un estado desconocido CIAM impide unlink. Contar métodos primarios probados, no factores MFA ni un mero email de contacto.
- **FR-009**: MUST registrar operaciones idempotentes, versionadas y auditadas, con reconciliación de fallos; no asumir transacción distribuida Graph/BD.
- **FR-010**: MUST seguir migración expand/validate/cutover/retire y rollback de [migration.md](migration.md), sin cambiar relaciones financieras ni identidad por email.
- **FR-011**: MUST mantener acceso legado restringido durante ventana aprobada; retirada exige plan para no migrados y recuperación. No múltiples CIAM permanentes.
- **FR-012**: MUST delegar contraseñas/OTP de login/reset/MFA al CIAM. Se recomienda email OTP, condicionado al gate MFA/recuperación del research; no credenciales propias.
- **FR-013**: MUST mantener username público normalizado/único y separado del login; DisplayName propio y soporte diseñado para nombres, avatar, idioma/preferencias.
- **FR-014**: MUST conservar personalizaciones ante login/sync; importación inicial, snapshot y reimportación explícita por campo tienen precedencia diferenciada, también para avatar.
- **FR-015**: MUST distinguir email de login/claim/contacto y autoridad/fecha de verificación. Contacto no verificado no recupera acceso; username no revela email.
- **FR-016**: MUST minimizar PII; nacimiento, teléfono/dirección y verificación telefónica se diseñan como extensiones, sin recogerlos/persistirlos por anticipación.
- **FR-017**: MUST autorizar perfil sobre usuario resuelto y versión vigente; no exponer bindings/pruebas/PII privada en DTOs de miembros.
- **FR-018**: MUST usar para Web beta SPA pública MSAL.js Browser/React, Authorization Code + PKCE **browser-delegated**, redirects exactos y limpieza de sesión/caché/borradores; sin secretos SPA ni Native Auth JS SDK en esta entrega. [D2](architecture-review.md#d2--browser-delegated-y-native-authentication).
- **FR-019**: MUST diseñar compatibilidad MAUI: registro propio, PKCE, deep links/redirects y storage seguro. Candidato futuro MSAL.NET con navegador del sistema; Native Authentication no se fija sin versión/SDK/flujo real acreditados. Social native JS/Android puede incluir tramo browser-delegated, sin inferir paridad MAUI. MAUI productivo sigue diferido.
- **FR-020**: MUST validar firma/algoritmo/lifetime/issuer/tenant/audience y scope delegado en cada API; rechazar tokens sociales, Graph e ID tokens como bearer de negocio.
- **FR-021**: MUST mantener audience lógica común. En coexistencia, cada issuer solo acepta su audience autorizada; no un producto cartesiano de listas ni tenants descubiertos desde claims sin validar.
- **FR-022**: MUST conservar autorización Owner/Admin/Member y contratos financieros; grupos/roles CIAM no se convierten en roles del dominio.
- **FR-023**: MUST aplicar [D1 opción 1 beta](architecture-review.md#d1--suspensión-y-access-token-ya-emitido): tras commit de suspensión en Users, el siguiente control autoritativo de cada API rechaza incluso JWT vigente anterior. Estado consultado por request autenticada, sin caché entre peticiones; fail-closed 503 si no disponible, 403 si suspendido. No promete cancelar requests ya autorizadas ni reflejar instantáneamente cambios solo en Azure. Reactivación no revive tokens anteriores al corte. Logout local no equivale a revocación global IdP.
- **FR-024**: MUST proteger replay/CSRF/state/nonce/PKCE/redirects, limitar abuso y errores enumerables; sin tokens/códigos/PII innecesaria en logs.
- **FR-025**: MUST contemplar eliminación, retención, anonimización y recuperación según [privacidad](privacy-and-deletion.md); no habilitar borrado sin política aprobada.
- **FR-026**: MUST respetar TP11 hasta aprobación de ADR/enmienda. Esta formalización no autoriza cambios Azure, BD o producción ni cierra gates anteriores.

### Key Entities

User/Profile, IdentityBinding, AccountOperation/ProofReceipt, MigrationRecord, SecurityAuditEvent, ProfileFieldAuthority y AvatarReference. El [modelo](data-model.md) diferencia esquema actual, A, transición acotada y B condicional.

## Success Criteria *(mandatory)*

- **SC-001**: los 12 casos de [aceptación](checklists/acceptance.md) tienen evidencia del candidato; Google/email reales para alta; ninguna omisión cuenta como éxito.
- **SC-002**: migración autorizada/rollback conservan igualdad exacta de UserId y referencias de listas/grupos/gastos/auditoría, cero nuevos UserIds para migrados y cero huérfanos.
- **SC-003**: todos los negativos de colisión/último método/concurrencia/replay/suplantación y frescura D3 rechazan sin alterar correlación ni perder acceso alternativo; pruebas ausentes/viejas/SSO no acreditado nunca habilitan una mutación.
- **SC-004**: todos los campos personalizados probados conservan valor tras login/sync; reimportación exige selección y versión.
- **SC-005**: tres APIs pasan matriz de confianza/scopes y regresiones Users/ShoppingList/ProductCatalog; ningún token Google directo se acepta. AR-01–03 acreditan suspensión con JWT ya emitido, 503 sin Users, reactivación sin resurrección y coste de ProductCatalog, conforme D1 y sin modificar budgets.
- **SC-006**: logout/cambio A→B no expone datos A incluso con requests pendientes/recarga. Web se valida primero en 320/390/430px, teclado, lector y zoom, con evidencia manual propia.
- **SC-007**: recuperación mantiene UserId; reconciliación probada en cada frontera de fallo CIAM/BD; sin bypass basado en perfil.
- **SC-008**: acceso público requiere ADR/enmienda aprobados, prueba A o decisión B, política de frescura/recuperación, presupuesto operativo y ensayo de migración/rollback. Esta entrega prepara esa revisión, no acredita estos gates.

## Assumptions, Dependencies & Out of Scope

Depende de 003 (Users/correlación), 007 (MSAL/sesión), 008 (autorización) y 009 (UserId en compras/history). Sus pendientes permanecen abiertos. 006 no existe: MAUI no tiene implementación que certificar. Google requiere proyecto del proveedor y CIAM un tenant/suscripción administrables. El spike posterior autorizado ya dispone de recursos QA aislados y acceso Google/email real; esto no autoriza despliegue productivo ni cierra G1/G3. Véase [resumen](evidence/spike-summary.md).

Fuera: contraseñas propias, autenticación desde cero, APIs validando Google/Apple directamente, múltiples CIAM permanentes, ownership por identificadores externos, fusión de UserIds, chat, pagos, cambios funcionales ShoppingList/ProductCatalog, MAUI productivo, PII innecesaria, borrado productivo y activación Apple/otros sin decisión posterior. No se modifican presupuestos ni aceptación 007/008/009.
