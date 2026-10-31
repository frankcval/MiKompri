# Contratos mínimos — 010-A

Contrato 010-A implementado en la rama; aceptación real pendiente. [Auditoría inicial](../current-state-audit.md), [migración](../migration.md), [validación](../evidence/validation.md). Se reutilizan perfil/sync y grupos; las APIs no validan Google directamente.

## Contrato de token y confianza

| Elemento | Regla A |
|---|---|
| Microsoft actual | Conservar issuer/tenant/audience/clientes legítimos durante transición aprobada |
| Google | External ID emite access token para recurso MiKompri; tuple CIAM se resuelve en Users |
| Validación | Firma/algoritmos/JWKS fijados, issuer, audience, exp/nbf/skew, scope delegado, tid y oid. No generar issuer sts para CIAM |
| Audience/scopes | Recurso lógico común en tres APIs. Matriz por issuer: solo audience/scopes/clientes correspondientes; no combinaciones cruzadas |
| Identidad | Namespace confiable+tid/oid → UserId; sin email/sub global ni identificador Google en relaciones |
| Negativos | Google directo, Graph, ID bearer, wrong issuer/aud/scope/tenant, expirado, falta de tupla y actor B deben denegarse |
| D1 | Estado local autoritativo antes de negocio, sin Graph ni caché inter-request; 401 JWT/corte, 403 scope/suspensión, 503 estado inaccesible |
| G3 | Fuera del contrato login A: acrs/auth_time/MFA histórica no autorizan linking. No pedir c1 para entrar ni usar las políticas de spike |

## APIs existentes de perfil y provisión

- GET `/api/v1/users/me`: UserId/perfil propio. Compatibilidad de DTOs documentada; no filtrar subjects/identidades en miembros.
- PUT `/api/v1/users/me`: ampliar capacidades existentes con whitelist de campos A. Mantener alcance de edición legacy; campos ausentes no se borran. Versión/If-Match para campos nuevos; 428 si requerida, 412 conflicto y 409 username ocupado. No aceptar UserId/tid/oid/autoridad/verificación.
- POST `/api/v1/users/me/sync`: conservar integración de bootstrap/resolución, separando actualización de perfil. Alta nueva solo en modo autorizado; nunca crear candidato de migración. Claims no sobrescriben personalizaciones ni disparan traslado.
- Grupos/membresías/roles y contratos de compras/gastos no cambian.

No añadir endpoint para cada proveedor. Username de perfil no es alias de login CIAM. Avatar iniciales; no contratos upload/storage ni métodos de cuenta A.

## Estado de acceso D1

GET `/api/v1/users/me/access-status`, bearer ya validado, lectura Users local sin aprovisionar/sync/Graph ni recursión. HTTP200 y Cache-Control:no-store; JSON obligatorio: `userId` (Guid no vacío), `isSuspended` (bool), `securityVersion` (entero>=0), `tokensValidFromExclusive` (Unix seconds o null). No PII/métodos. HTTP distinto de200, cuerpo inválido/incompleto o dependencia inaccesible fallan cerrado503, salvo401/403 propagados. Tupla desconocida401, sin alta automática.

ShoppingList/ProductCatalog lo consultan antes del handler protegido, GET incluido; solo reutilizan dentro de request. Users aplica lectura local. Deadline configurable1s por defecto, sin retries; medir budgets existentes. API inaccesible→503, suspendido→403, iat ausente o anterior/igual al corte→401 cuando existe corte. No resucitar tokens al reactivar. No volver catálogo público ni cambiar catálogo global/grupos para eludir dependencia.

Este contrato se justifica porque GET/me actual crea/sincroniza perfil; no puede usarse así para autorizar cada request. No bloquea primer token validado en harness, sí salida productiva protegida.

## Traslado Microsoft administrado — DEFERRED, no gate beta010-A

Contrato interno/runbook de cohorte, no nueva API pública de linking: fuente inmutable acreditada, UserId derivado por Users, destino reservado, versión/estado/idempotencia y rollback según migración. No cuerpo HTTP que elija UserId destino. Estado incierto o candidato desconocido bajo piloto→denegar negocio y no insertar otro User.

Antes de retirar legacy se verifican pares old/new en tres APIs y forwarding UsersApi, mismo UserId y cortes D1. El contrato no presume que dos objetos CIAM con mismo email sean la misma persona.

## Sesión Web y configuración

Google prioritario y Microsoft existente disponible; email solo si aprobado. Code+PKCE+MSAL browser-delegated; registro SPA público/redirect exacto y autoridad conocida por ruta. Separar tokens/caché según cliente/issuer/cuenta y generación de sesión; cancelar/ignorar respuestas tardías, limpiar drafts y return intent al cambiar. CORS no sustituye redirect/CSRF.

Recuperación: enlace/ayuda hacia proveedor original, nunca endpoint MiKompri de «reemplazar identidad perdida». Logout local no promete revocación de todos los JWT del IdP.

## Contratos diferidos010-B

Se conserva el diseño, **no se implementa ni habilita en A**:

| Diseño anterior | Situación |
|---|---|
| GET `/api/v1/users/me/access-methods` | Inventario y canUnlink futuros, no lista necesaria para login |
| POST `/api/v1/users/me/account-operations` | Broker de link/unlink/cambio de proveedor/recovery autoservicio, diferido |
| GET `/api/v1/users/me/account-operations/{id}` | Operación propia y estado, diferido |
| POST `/api/v1/users/me/account-operations/{id}/confirm` | Doble prueba/último método/replay/versiones, diferido |
| ProofReceipt/Authentication Context/CA de linking | Gate G3 y revisión futura; no en middleware de login A |
| Reimportar perfil/foto/eliminar cuenta | Contratos futuros tras alcance/seguridad aprobados |

[010-B](../010-b-deferred.md) conserva finalidad y reglas D3; [architecture-review](../architecture-review.md) y evidencias conservan investigación exacta. Un ID token de broker futuro no será bearer de negocio. No schema/broker provisional en A.
