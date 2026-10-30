# ADR-004 — CIAM External ID, UserId y gestión de métodos

**Estado**: Propuesto — pendiente de architecture review, no aceptado

**Fecha**: 2026-10-30

**Spec**: [010](../../specs/010-account-management-social-auth/spec.md)

**Decisiones y fuentes**: [research A/B](../../specs/010-account-management-social-auth/research.md)

## Contexto

El acceso workforce actual no ofrece alta orientada a consumidores sin Microsoft. UserId Guid ya sostiene propiedad e historia. Users guarda una sola tupla canónica y sincroniza campos de claims sin autoridad por campo. Cambiar tenant sin migración autorizada produciría usuarios nuevos y pérdida de acceso a relaciones existentes.

## Opciones

**A:** External ID gestiona Google/email/Microsoft y varios métodos del mismo objeto; Users correlaciona principalmente `(externalTid, externalOid) → UserId`, perfil y operaciones. Menos estado permanente propio, pero depende de linking y mutaciones CIAM realmente soportadas.

**B:** Users permite varias identidades External ID por UserId. Facilita conservar métodos que CIAM no reúna en un solo objeto, a cambio de más unicidad, estados, revocación, recuperación y datos. Sigue usando un solo CIAM como emisor; no Google JWT directo ni múltiples CIAM permanentes.

## Decisión propuesta

Investigar e intentar **A como objetivo preferente**, con gate de viabilidad antes de implementación del modelo final. Google y email OTP primero; Microsoft personal como adicional y federación del tenant legado para transición; Apple/otros evaluados, no activados. Mantener UserId y todas las referencias de dominio. La existencia de identities en Graph no certifica todo el flujo: capacidades documentadas y límites en [research](../../specs/010-account-management-social-auth/research.md).

Users orquesta doble prueba/consentimiento, idempotencia, perfil y journal; CIAM autentica/gestiona credenciales. La fase de migración admite un puente old/new acotado, no una colección genérica de identidades sociales. Si G1 demuestra que A no satisface un caso obligatorio —en particular Google-only→email o unlink seguro—, registrar evidencia y revisar este ADR para seleccionar B mínimo **antes** de producir esquema/contratos definitivos. No mezclar A y B silenciosamente ni quitar requisitos para conservar A.

Web mantiene MSAL/PKCE y navegador delegado como opción inicial. MAUI futuro usará registro propio y flujos soportados por plataforma; no se infiere que native auth excluya social ni que MSAL.NET tenga toda la paridad Android/JS.

### Precisión beta propuesta tras revisión acotada del 30/10

- **D1: opción 1, suspensión prácticamente inmediata** en el siguiente control autoritativo tras commit Users. Validar estado cuenta/binding por request en tres APIs, sin allow cacheado entre peticiones y sin esperar exp del JWT; Users local, consumidores por contrato mínimo. 403 suspendido, 503 si no puede resolverse estado. No se cancelan retroactivamente peticiones ya autorizadas. ProductCatalog incorpora dependencia operativa Users; medir latencia/disponibilidad sin cambiar budgets. Sin Graph por request, Redis/event bus ni CAE supuesto; tokens/version claims no sustituyen lookup. Cortes server-side impiden revivir tokens antiguos al reactivar.
- **D2: Web = Authorization Code + PKCE + MSAL.js browser-delegated**. Native Auth JS/Android puede incluir social vía tramo web; no se adopta para beta. MAUI/MSAL.NET y Native Auth requieren versión/SDK/retorno comprobados antes de decidir; las discrepancias oficiales se conservan.
- **D3: prueba reciente por operación**, <=300s al confirmar, operación <=600s. Reauth CIAM y auth_time/challenge verificables solo tras G3; no iat/prompt/MFA histórica. Linking y migración prueban origen+candidato; unlink prueba alternativa que permanece; recovery no exige el proveedor perdido ni permite bypass sin alternativa. OTP primario no es MFA; passkey no es actualmente una solución general para OTP/social. Sin evidencia fiable, mutación bloqueada, no aceptación simulada.

Comparación completa, garantías/limitaciones, matrices por método/operación y pruebas pendientes en [architecture-review](../../specs/010-account-management-social-auth/architecture-review.md); fuentes actuales R20–R28 en research. D1–D3 siguen **propuestos**, no aprobados ni implementados.

### Actualización posterior: autorización y observaciones del spike QA

El usuario ha aprobado D1–D3 **para el spike controlado**, sin aceptar todavía este ADR completo ni enmienda. [Resumen](../../specs/010-account-management-social-auth/evidence/spike-summary.md), [probes de agregación](../../specs/010-account-management-social-auth/evidence/aggregation-spike.md), [G3](../../specs/010-account-management-social-auth/evidence/assurance-spike.md).

Email OTP real se representa como identidad federated/mail. Los probes CIAM permiten agregar mail a google.com ficticio y viceversa con mismo oid; el rechazo documentado/observado de social-only→emailAddress corresponde a cuenta local password y no descarta esa ruta OTP. No recrear objetos ni alterar UserId para sortearlo. Estas ediciones no acreditan prueba de control ni acceso con Google real.

Graph permite retirar último método explícito, no fusiona identidades ocupadas y dos PATCH concurrentes aceptados dejan una sola adición. Por ello A requiere concretar guards/journal/serialización/reconciliación y cerrar G1/G3/permisos mínimos, no un simple PATCH desde UI. Se conserva A como candidata; no seleccionar/implementar B mientras no se demuestre insuficiencia de la ruta OTP segura. D1 tiene benchmark local parcial; sin acceso-status productivo ni aceptación cloud. Constitución intacta y fases3–8 no iniciadas.

El acceso Google posterior sí funciona sin cuenta Microsoft, según confirmación del titular, token CIAM validado y objeto Graph federated/google.com distinto del OTP. En cambio, la reautenticación prompt=login/max_age=0 (reintentada sin login_hint) produjo auth_time nuevo tras solo selección/consentimiento: **NOT RELIABLE FOR G3 para la ruta ensayada**. No aprobar un broker que use ese timestamp como control Google reciente. Antes de implementar, revisar una prueba/challenge CIAM verificable y su compatibilidad real por método; si no puede acreditarse, las mutaciones afectadas siguen bloqueadas. Cambiar A por B no resuelve automáticamente esta limitación. No se rebajan300/600s ni se mutan identidades reales para forzar una aceptación.

Investigación posterior propone ensayar authentication context CA con desafío CIAM, no reauth Google forzada: [revisión G3](../../specs/010-account-management-social-auth/evidence/g3-step-up-review.md). Soporte documental no es PASS: email MFA social presenta discrepancia con la configuración OTP, `acrs` puede emitirse sin política y Every time tolera MFA previa5min. Primero verificar compatibilidad, política efectiva, bootstrap del factor y timestamp del control real; SMS no se activa ni se impone teléfono, password o passkey. Este candidato no cambia D3 ni acepta ADR/arquitectura final.

## Consecuencias

Ventajas: credenciales externas, tokens homogéneos para APIs, dominio independiente del proveedor, perfil personalizado estable. Costes: tenant y federaciones, Graph/operación privilegiada si A lo requiere, QA real, migración/coexistencia/rollback, MFA/recuperación, posibles MAU/SMS y avatar/storage. Ningún coste cero total o despliegue se presume.

Riesgos: limitaciones Graph/linking, colisiones de objetos, frescura, fallback legado por sub, confianza de emisores y PII. Invariantes y pruebas en modelo/migration/threat-model; no automático por email, no fusionar UserIds existentes.

## Aprobación y plan

Proponer [enmienda TP11 1.2.0](../../specs/010-account-management-social-auth/constitution-amendment-proposal.md), sin editar constitución ahora. Revisar A/B, G1–G7, política de recuperación y presupuesto. Autorizar por separado un spike QA sin datos productivos; completar gates y ratificar decisión. Solo después, aprobar implementación incremental y migración ensayada. El estado Propuesto no autoriza Azure, BD, secretos ni cambio de registros actuales.
