# Threat model básico — 010-A / futuro 010-B

## Aplicabilidad (30/10/2026)

Se preservan todas las amenazas anteriores. **A** aplica login/JWT, resolución/alta, migración Microsoft administrada, perfil, aislamiento de sesión y D1 antes de negocio productivo. **B** aplica linking/unlinking/cambio de método, segundo método, recuperación mediante otro proveedor, broker/ProofReceipt, fotos y borrado operativo. No exigir pruebas B ni G3 como gate del login A. Las menciones a varios bindings/Graph mutador/broker siguientes son diseño B; A usa una canónica activa y un registro acotado de transición.

En A la transición Microsoft acredita el mismo origen inmutable por federación confiable y cohorte reservada, con guard de provisión y rollback; no usa email ni un flujo de prueba arbitraria de dos proveedores. Recuperación se delega al proveedor original, sin soporte MiKompri que reasigne UserId. [Migración](migration.md) y [alcance diferido](010-b-deferred.md).


Activos: UserId/ownership, métodos, perfil privado, pruebas, tokens, membresías e historia financiera. Actores: usuario legítimo, atacante externo, usuario B autenticado, proveedor comprometido y operador con privilegios. Fronteras: navegador↔CIAM↔social; SPA↔APIs; Users↔Graph; contexto↔Users; datos/telemetría. A/B no eliminan esas fronteras. [Contratos](contracts/account-and-identity.md) y [migración](migration.md).

| Alcance / amenaza | Mitigación exigida | Verificación pendiente |
| --- | --- | --- |
| B — Account takeover por linking a sesión robada | Prueba reciente de origen y candidato, consentimiento, sesión/propósito/nonce ligados; confirmar método sin cambiar cuenta activa SPA | A inicia/B altera, reauth ausente o vieja; negar |
| A / B cuando aplica — Email collision, cambio o reciclaje | No lookup/link por email/nombre/username/avatar/teléfono; identidad inmutable del emisor | Dos proveedores mismo correo y cambios posteriores; UserId sin alteración |
| B — B vincula su identidad al UserId A | Servidor deriva A; no aceptar targetUserId en body/query; doble prueba y candidato libre | Cambiar ID de operación/callback y payload malicioso |
| A / B cuando aplica — Proveedor o directorio comprometido | Dependencia de confianza explícita, aislamiento de tenant, revocar binding/cuenta y tokens según capacidades; auditoría/notificación sin PII | Simular issuer/binding bloqueado, firma/clave no autorizada; no rollback a origen comprometido |
| B — Eliminar último acceso, carreras | Inventario CIAM fresco, prueba alternativa utilizable, lock/version y serialización por User | Dos unlink paralelos, Graph incierto, factor MFA contado como login; nunca cero métodos |
| A / B cuando aplica — Replay de OAuth/prueba/confirmación | State/nonce/verifier un solo uso, TTL/reautenticación, purpose binding e idempotencia con fingerprint | Reusar prueba en otro User/propósito, expirada, payload distinto |
| A / B cuando aplica — Login/link CSRF, sustitución de sesión | PKCE S256, state/nonce, callbacks fijados y sesión de operación, anti-CSRF de cookies | Token/code injection, callback iniciado por B, origen hostil |
| A / B cuando aplica — PKCE o redirect URI abuse | SDK soportado, exact match por entorno/cliente, sin wildcards/open redirect; verificar deep links/firma MAUI futuro | Redirect externo, manipulación returnTo y callback duplicado |
| B — Pérdida del proveedor y recovery débil | D3: alternativa ya vinculada y reciente; unlink prueba la que permanece, no obliga al proveedor perdido. Sin alternativa no hay recovery autoservicio beta ni bypass por perfil/email | Perder Google con/sin alternativa, mismo UserId o denegación explícita; AR-06 |
| A / B cuando aplica — Duplicación de migración/auto-provisión | A: ventana de traslado con nueva provisión pausada, origen inmutable/reserva, destino único y corte atómico Users; no fallback que reabra origen retirado. B: rutas de proof separadas | Callbacks duplicados, migraciones cruzadas y candidato ya asignado |
| A / B cuando aplica — Confusión tokens / tenant / audience | Validar tuple de confianza, algoritmo/lifetime/scope; ID token no bearer de negocio; broker B solo con audience propia | Token Google/Graph/SPA, old issuer+new aud, tenant inesperado, scope ausente |
| A / B cuando aplica — Regresión autorización del dominio | Resolver UserId por Users, roles/membresías locales; no usar roles/grupos CIAM como dominio | Owner/Admin/Member, ajeno, retirado; balances/ownership idénticos tras migrar |
| B — Graph sobreprivilegiado o lost update | Adapter server-side, least privilege probado, no permisos directorio SPA; read-modify-write/locks/reconciliación, mínimo journal | Timeout antes/después de cambio, alteración concurrente, identidad de otro usuario; fail closed |
| A / B cuando aplica — Metadata/JWKS/URL maliciosa | Authorities/hosts fijados, HTTPS, metadatos de emisor conocido; no URLs de usuario para discovery | Issuer/host inyectado, refresh keys, error de metadata |
| A / B cuando aplica — Perfil/PII expuestos o avatar SSRF/XSS | Minimización, autorización propia, DTO público mínimo, escape de texto, sin foto/URL de usuario en A; procesar imágenes/límites y no fetch arbitrario antes de B | Perfil B, upload ajeno/SVG/EXIF/decompression bomb, URL privada; iniciales fallback |
| A / B cuando aplica — Caché/token/draft A visible en B | Claves por identidad/tenant/cliente y generación de sesión; cancelar/ignorar requests A y limpiar logout | Respuesta tardía, sesión multi-tab, reload, cambio de issuer/método |
| A / B cuando aplica — Enumeración/abuso OTP/link/support | Respuestas genéricas, rate limit por actor/origen con privacidad, cuotas/protección CIAM y alarmas de coste | Email existente/inexistente no revela cuenta; saturación sin mensajes sensibles |
| B — Borrado destructivo de historia ajena | Proceso por fases, último Owner, tombstone/retención aprobados; ningún cascade genérico | Gastos compartidos/participantes e integridad con usuario eliminado |
| A / B cuando aplica — JWT válido después de suspender o reactivar | D1 lookup autoritativo por request en tres APIs, sin allow cacheado. Cortes de emisión al suspender/reactivar evitan resurrección; SecurityVersion invalida proofs sin requerir claim nuevo | AR-01–03: exp futura, binding vs cuenta, token emitido durante suspensión/mismo segundo y Users caído; 403/401/503 según contrato |
| B — Reauth aparente/assurance mal interpretada | D3: timestamp firmado con semántica de reto probada y nonce/operación; rechazar iat nuevo, cookies SSO/selector/consent o MFA histórica. OTP primario no es segundo factor; passkey no asumida para social/OTP | AR-05–06: auth_time ausente/viejo/futuro, max_age ignorado, envejecimiento entre pruebas/confirmación y método no soportado |

## Riesgos residuales y controles de salida

- No puede probarse identidad humana si se pierden todos los factores: recuperación puede ser imposible. No prometer una garantía basada en email.
- Un IdP comprometido puede autenticar indebidamente; ninguna doble prueba sustituye la confianza del emisor. A mantiene confianza acotada/estado D1; las operaciones sensibles B necesitan control reciente y assurance admitida.
- JWT emitido puede seguir válido tras logout. [D1](architecture-review.md#d1--suspensión-y-access-token-ya-emitido) bloquea acceso local tras commit/siguiente control y falla 503 sin Users, pero no cancela requests ya autorizadas ni refleja automáticamente cambios solo en Azure. Graph no se consulta en requests de negocio; su caída bloquea mutaciones de métodos que lo necesitan, no añade un lookup Graph a cada GET. No prometer CAE/revocación IdP instantánea.
- Locks locales no hacen Graph transaccional. La prueba de agregación de la alternativa arquitectónica A debe demostrar esas fronteras antes de ofrecer linking en 010-B; si no, revisar alternativa B o bloquear esa capacidad, no login básico.
- Riesgos humanos (soporte, secretos, roles administradores) requieren responsables y runbook tras aprobación. No se han creado credenciales ni instalado controles en esta ejecución.

Salida 010-A: negativos de amenazas aplicables A, revisión de seguridad, migración/rollback, privacidad y D1 antes de negocio productivo. No se publica gestión de métodos ni recuperación alternativa, de modo que G3/último método se conservan como gates B. Email OTP opcional requiere sus negativos básicos antes de habilitarse. Tests de fixtures no sustituyen proveedores reales ni revisión manual.
