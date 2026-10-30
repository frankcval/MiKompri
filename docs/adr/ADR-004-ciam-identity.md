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

## Consecuencias

Ventajas: credenciales externas, tokens homogéneos para APIs, dominio independiente del proveedor, perfil personalizado estable. Costes: tenant y federaciones, Graph/operación privilegiada si A lo requiere, QA real, migración/coexistencia/rollback, MFA/recuperación, posibles MAU/SMS y avatar/storage. Ningún coste cero total o despliegue se presume.

Riesgos: limitaciones Graph/linking, colisiones de objetos, frescura, fallback legado por sub, confianza de emisores y PII. Invariantes y pruebas en modelo/migration/threat-model; no automático por email, no fusionar UserIds existentes.

## Aprobación y plan

Proponer [enmienda TP11 1.2.0](../../specs/010-account-management-social-auth/constitution-amendment-proposal.md), sin editar constitución ahora. Revisar A/B, G1–G7, política de recuperación y presupuesto. Autorizar por separado un spike QA sin datos productivos; completar gates y ratificar decisión. Solo después, aprobar implementación incremental y migración ensayada. El estado Propuesto no autoriza Azure, BD, secretos ni cambio de registros actuales.
