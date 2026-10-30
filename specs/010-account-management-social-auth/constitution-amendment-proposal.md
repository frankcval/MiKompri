# Propuesta de enmienda constitucional — TP11

> Actualización2026-10-30: aprobación explícita de010-A/ADR/TP11 y transición; TP11 **1.2.0 aplicada**. **010-A READY FOR IMPLEMENTATION**. El resto conserva la propuesta/revisión previa; [registro actual](evidence/implementation.md).

**Estado:** propuesta para aprobación; la constitución vigente NO se modifica.
**Versión vigente:** 1.1.0. **Propuesta:** **1.2.0 MINOR**: ampliar CIAM/Google y transición conservando garantías. No retirar invariantes ni introducir una ruptura que justifique MAJOR.

## Mapa de reglas TP11

Enumeración para trazabilidad de las reglas actuales, sin sustituir su texto normativo:

| Regla vigente | Se conserva / evolución propuesta |
| --- | --- |
| 1. Microsoft Entra como proveedor oficial inicial | Conservar compatibilidad inicial; permitir External ID como CIAM para Google y otros métodos aprobados incrementalmente |
| 2. Registro independiente por cliente | Conservar Web y futuro MAUI; configuración por entorno |
| 3. sub no es identidad global | Conservar sin excepciones |
| 4. Tupla tid/oid estable por tenant | Conservar con issuer/namespace de confianza explícito; cambio de tenant exige transición autorizada |
| 5. UserId Guid canónico | Conservar inmutable, incluso al cambiar tupla externa |
| 6. Users único correlador | Conservar; no resolver identidades sociales en cada bounded context |
| 7. Referencias de dominio a UserId | Conservar ownership/membresías/AddedBy/PaidBy/PurchasedBy/historia |
| 8. Sin dependencias cross-database | Conservar contratos entre contextos |
| 9. Recurso/audience común para APIs | Conservar recurso lógico común; durante transición, matriz estricta por issuer con audience de ese emisor compartida por las tres APIs |
| 10. Firma/issuer/audience/exp/scopes validados | Conservar; rechazar token de proveedor social directo |
| 11. Permisos de negocio en cada contexto | Conservar; claims/roles CIAM no reemplazan memberships |
| 12. Nuevos clientes no duplican usuarios | Conservar; migración origen/destino reservada y UserId inmutable |
| 13. OBO si se separan audiences | Conservar condicional; 010-A no introduce separación entre las tres APIs |

## Texto normativo propuesto para revisión

- MiKompri MUST conservar `UserId : Guid` como identidad canónica inmutable. Users MUST ser autoridad de correlación. Ninguna relación de dominio se migra a identificadores externos.
- Microsoft/Entra existente MUST seguir disponible durante una transición controlada. Microsoft Entra External ID MAY actuar como CIAM central para usuarios Google sin cuenta Microsoft y métodos adicionales aprobados; las APIs MUST validar tokens MiKompri del emisor permitido, no tokens Google/Apple/etc. directos.
- La primera entrega MAY conservar una única tupla canónica activa por UserId. NO se exige una colección genérica de múltiples identidades ni capacidades de linking/unlinking/recuperación entre proveedores para ofrecer login Microsoft + Google.
- Cada emisor/tenant/audience/scope/cliente MUST pertenecer a una combinación de confianza explícita. La coexistencia legado/CIAM MUST ser acotada, inventariada y reversible; no confiar en combinaciones cruzadas de listas independientes.
- Una transición de identidad MUST acreditar origen/destino por identificadores inmutables y autoridad confiable, reservar unicidad, preservar exactamente UserId y referencias, bloquear auto-provisión duplicada y ensayar rollback. Email, nombre, username, teléfono, avatar o sub global MUST NOT servir de vinculación automática.
- Si una entrega posterior ofrece vincular/desvincular métodos o recuperar mediante otro proveedor, MUST acreditar control reciente aplicable, consentimiento, protección contra replay/carreras y no eliminar último acceso. Esas obligaciones condicionales NO obligan a incluir esa funcionalidad en el primer incremento.
- Las credenciales MUST permanecer en el IdP cuando este pueda gestionarlas; MiKompri MUST NOT implementar contraseñas propias para este alcance. Claims externos MAY inicializar perfil; MUST NOT sobrescribir personalizaciones posteriores.
- D1 de estado Users autoritativo MUST aplicarse antes de habilitar acceso productivo protegido. El primer ensayo aislado de login no equivale a aceptar negocio productivo ni exige completar de antemano toda esa infraestructura.

Email OTP es una decisión opcional de producto, no obligación constitucional. Native Authentication, CA/G3, MFA avanzada y Alternative B no se fijan como requisitos de 010-A.

## Aplicación posterior

Aprobar [ADR-004](../../docs/adr/ADR-004-ciam-identity.md), esta propuesta y transición acotada según gobernanza §4.1. Una ejecución posterior autorizada actualizará constitución, versión, sync report y referencias pertinentes de Spec Kit. No aprobar esta enmienda implícitamente por existir documentación o QA. No modificar ahora constitución, registros, permisos, secretos o BD.
