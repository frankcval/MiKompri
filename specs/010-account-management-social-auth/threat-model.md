# Threat model básico — 010

Activos: UserId/ownership, métodos, perfil privado, pruebas, tokens, membresías e historia financiera. Actores: usuario legítimo, atacante externo, usuario B autenticado, proveedor comprometido y operador con privilegios. Fronteras: navegador↔CIAM↔social; SPA↔APIs; Users↔Graph; contexto↔Users; datos/telemetría. A/B no eliminan esas fronteras. [Contratos](contracts/account-and-identity.md) y [migración](migration.md).

| Amenaza | Mitigación exigida | Verificación pendiente |
| --- | --- | --- |
| Account takeover por linking a sesión robada | Prueba reciente de origen y candidato, consentimiento, sesión/propósito/nonce ligados; confirmar método sin cambiar cuenta activa SPA | A inicia/B altera, reauth ausente o vieja; negar |
| Email collision, cambio o reciclaje | No lookup/link por email/nombre/username/avatar/teléfono; identidad inmutable del emisor | Dos proveedores mismo correo y cambios posteriores; UserId sin alteración |
| B vincula su identidad al UserId A | Servidor deriva A; no aceptar targetUserId en body/query; doble prueba y candidato libre | Cambiar ID de operación/callback y payload malicioso |
| Proveedor o directorio comprometido | Dependencia de confianza explícita, aislamiento de tenant, revocar binding/cuenta y tokens según capacidades; auditoría/notificación sin PII | Simular issuer/binding bloqueado, firma/clave no autorizada; no rollback a origen comprometido |
| Eliminar último acceso, carreras | Inventario CIAM fresco, prueba alternativa utilizable, lock/version y serialización por User | Dos unlink paralelos, Graph incierto, factor MFA contado como login; nunca cero métodos |
| Replay de OAuth/prueba/confirmación | State/nonce/verifier un solo uso, TTL/reautenticación, purpose binding e idempotencia con fingerprint | Reusar prueba en otro User/propósito, expirada, payload distinto |
| Login/link CSRF, sustitución de sesión | PKCE S256, state/nonce, callbacks fijados y sesión de operación, anti-CSRF de cookies | Token/code injection, callback iniciado por B, origen hostil |
| PKCE o redirect URI abuse | SDK soportado, exact match por entorno/cliente, sin wildcards/open redirect; verificar deep links/firma MAUI futuro | Redirect externo, manipulación returnTo y callback duplicado |
| Pérdida del proveedor y recovery débil | Método alternativo probado; soporte con criterios aprobados, sin recuperación automática por perfil/email | Perder Google con/sin alternativa, mismo UserId o denegación explícita |
| Duplicación de migración/auto-provisión | Rutas de proof excluidas de auto-provision; clave destino única, journal y commit atómico Users | Callbacks duplicados, migraciones cruzadas y candidato ya asignado |
| Confusión tokens / tenant / audience | Validar tuple de confianza, algoritmo/lifetime/scope; ID token solo broker con audience propia | Token Google/Graph/SPA, old issuer+new aud, tenant inesperado, scope ausente |
| Regresión autorización del dominio | Resolver UserId por Users, roles/membresías locales; no usar roles/grupos CIAM como dominio | Owner/Admin/Member, ajeno, retirado; balances/ownership idénticos tras migrar |
| Graph sobreprivilegiado o lost update | Adapter server-side, least privilege probado, no permisos directorio SPA; read-modify-write/locks/reconciliación, mínimo journal | Timeout antes/después de cambio, alteración concurrente, identidad de otro usuario; fail closed |
| Metadata/JWKS/URL maliciosa | Authorities/hosts fijados, HTTPS, metadatos de emisor conocido; no URLs de usuario para discovery | Issuer/host inyectado, refresh keys, error de metadata |
| Perfil/PII expuestos o avatar SSRF/XSS | Minimización, autorización propia, DTO público mínimo, escape de texto, URLs permitidas; procesar imágenes/límites y no fetch arbitrario | Perfil B, upload ajeno/SVG/EXIF/decompression bomb, URL privada; iniciales fallback |
| Caché/token/draft A visible en B | Claves por identidad/tenant/cliente y generación de sesión; cancelar/ignorar requests A y limpiar logout | Respuesta tardía, sesión multi-tab, reload, cambio de issuer/método |
| Enumeración/abuso OTP/link/support | Respuestas genéricas, rate limit por actor/origen con privacidad, cuotas/protección CIAM y alarmas de coste | Email existente/inexistente no revela cuenta; saturación sin mensajes sensibles |
| Borrado destructivo de historia ajena | Proceso por fases, último Owner, tombstone/retención aprobados; ningún cascade genérico | Gastos compartidos/participantes e integridad con usuario eliminado |

## Riesgos residuales y controles de salida

- No puede probarse identidad humana si se pierden todos los factores: recuperación puede ser imposible. No prometer una garantía basada en email.
- Un IdP comprometido puede autenticar indebidamente; doble prueba no sustituye la confianza del emisor. Operaciones sensibles necesitan control reciente y assurance admitida, no solo token válido.
- JWT emitido puede seguir válido tras logout. Comprobar cuenta/binding y política de revocación en las tres APIs; si falla Users/Graph no conceder nuevas capacidades. No prometer revocación IdP instantánea.
- Locks locales no hacen Graph transaccional. La prueba A debe demostrar todas las fronteras y alternativa segura; si no, revisar B o bloquear habilitación.
- Riesgos humanos (soporte, secretos, roles administradores) requieren responsables y runbook tras aprobación. No se han creado credenciales ni instalado controles en esta ejecución.

Salida requerida: evidencia negativa por amenaza, revisión de seguridad independiente, prueba de migración/rollback, privacy sign-off y ningún gate abierto que permita takeover/último método. Tests de fixtures no sustituyen proveedores reales ni revisión manual.
