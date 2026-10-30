# Contratos propuestos — cuenta e identidad 010

**No implementados.** Se reutilizan ProfileController GET/PUT `/api/v1/users/me`, POST `/me/sync`, grupos/membresías y resolución de Users consumida por ShoppingList. [Auditoría](../current-state-audit.md). No hay endpoint existente de account linking, migración, métodos o recuperación. Estos contratos requieren aprobación del ADR y OpenAPI antes de codificar; no implican nuevas rutas operativas hoy.

## Contrato de token y confianza

| Elemento | Regla propuesta |
| --- | --- |
| Emisor aceptado | External ID destino, configuración/metadata OIDC HTTPS fijada por entorno. Legado solo en ventana aprobada. No Google/Apple/Graph ni cualquier tenant derivado de un tid sin validar |
| Audience | Recurso lógico común MiKompri. Tabla explícita por issuer: tenant permitido, JWKS, token version, audience GUID/URI realmente emitida, scope y clientes autorizados. No listas combinables libremente |
| Scope | `access_as_user` delegado del backend (nombre actual a conservar si se registra igual), tokenizado por espacios; ausencia/scope distinto →403 después de autenticar. ID token o token app-only sin contrato →rechazo |
| Claims de correlación | tid y oid completos, coherentes con issuer confiable; namespace server-side. El actor de dominio se obtiene de Users. sub/email/nombre no son claves |
| Validación | Firma/algoritmos admitidos/kid metadata, issuer, audiencia, exp/nbf, skew acotado; no construir issuer sts.windows.net automáticamente para CIAM |
| Aislamiento | Scope no sustituye permisos de recurso. Owner/Admin/Member siguen siendo datos de Users. Cuenta/binding suspendidos →403 en el siguiente control autoritativo tras commit, en tres APIs; 503 si estado inaccesible, sin caché entre requests, según D1 |
| Tokens de prueba | Un ID token validado por el broker con su audience/nonce específicos puede acreditar una operación; nunca se acepta como bearer de las APIs de negocio |

Casos mínimos en Users/ShoppingList/ProductCatalog: token destino válido; legado permitido/retiro; old issuer+new aud y viceversa; tenant no autorizado; firma/exp incorrectas; sin tid/oid; Google directo; Graph aud; SPA ID token; scope ausente/equivocado; cliente no autorizado; binding suspendido. 401 para autenticación inválida, 403 para scope/permisos insuficientes, 503 para resolución de estado obligatoria no disponible (fail closed).

## Estado de acceso — contrato mínimo beta propuesto

**GET `/api/v1/users/me/access-status`**, bearer ya validado de audiencia común, sin UserId destino ni lectura de otro usuario. Users lee el estado autoritativo local sin auto-provisión, sincronización de perfil o llamada a Graph/otra API. Sin recursión HTTP al propio endpoint. Respuesta 200 mínima: UserId resuelto, account/binding activos, SecurityVersion/BindingVersion y corte de emisión aplicable; `Cache-Control: no-store`. No PII ni lista de métodos. Un JWT anterior/al corte se rechaza (401), suspendido conocido 403, backend estado no disponible 503. Unknown no permite negocio ni crea User.

Users aplica su lectura local a cada recurso protegido; ShoppingList y ProductCatalog llaman este contrato antes del handler (GET incluidos), validan/reutilizan resultado **solo dentro de request** y no sirven allow cacheado tras fallo. Validar estado y `iat > TokensValidFromExclusive` conforme [D1](../architecture-review.md#d1--suspensión-y-access-token-ya-emitido); iat aquí no es prueba de reauth. Deadline propuesto 1s máximo configurable y cero retries automáticos de mutaciones. Health/documentación pública y flujos de alta/proof expresamente aislados no abren recursos privados como excepción. Contrato nuevo justificado porque GET `/me` actual aprovisiona/sincroniza y devuelve perfil; no reutilizar esos efectos para autorizar cada request.

ProductCatalog adquiere una dependencia operativa nueva de Users para todos sus endpoints protegidos; no cambia su política de catálogo global ni introduce roles de grupo. No CAE ni revocación Graph prometida; solicitudes ya autorizadas antes del commit no se cancelan por contrato. Al reactivar, cortes impiden resurrección de tokens anteriores/durante suspensión; pruebas AR-01–03 obligatorias antes de habilitar.

## Perfil existente ampliado

- **GET `/api/v1/users/me`**: conserva `id` como UserId, DisplayName/Email actuales según compatibilidad. Añadir perfil propio, procedencia/verificación de contacto, avatar seguro y `profileVersion`/ETag. No devolver sujetos de proveedor/pruebas en DTO público de miembro; deprecar ExternalUserId legado con política de clientes.
- **PUT `/api/v1/users/me`**: reutilizar edición existente. Legacy solo DisplayName conserva su alcance; campos nuevos ausentes no borran datos. Para mutar nuevos campos/operaciones 010 exigir versión `If-Match` y validación de presencia; documentar retirada del contrato legacy sin versión antes de hacerlo obligatorio para todos. Nunca aceptar tid/oid/UserId/verificationState desde cuerpo.
- **POST `/api/v1/users/me/sync`**: separar resolver identidad de importar perfil. Mantener compatibilidad de sync como lectura/actualización de snapshot; selección explícita de campos y versión para reemplazar personalizaciones. No usar sync para enlazar cuenta. Validar consumidores actuales antes del cambio de respuesta/semántica.
- Contacto cambiado pasa a unverified. Verificación por autoridad demostrada, no booleano del cliente. Verificación propia de contacto/telefónica puede quedar futura; no presentarla como recovery disponible.

Conflictos de username/perfil →409; cuerpo inválido →400; falta de versión cuando es requerida →428; versión antigua →412. Conservar formato de error actual `status/error/traceId/errors` y añadir `code` estable sin identificar a otro usuario.

## Operaciones nuevas mínimas (diseño)

| Ruta propuesta | Entrada / salida | Reglas |
| --- | --- | --- |
| GET `/api/v1/users/me/access-methods` | Inventario mínimo de métodos propios, estado, `canUnlink`, versión | Datos CIAM resueltos/confirmados; unknown no habilita unlink. Ocultar subjects/secretos y distinguir MFA de login |
| POST `/api/v1/users/me/account-operations` | `purpose: Link/Migrate/Unlink/Recover`, proveedor/método elegido, expectedVersion, idempotency key →202 con operationId/status y acción permitida | Sesión propia y acceso actual para link/unlink/migrate; Recover requiere prueba alternativa/control aprobado, nunca email como bearer. Derivar destino en servidor |
| GET `/api/v1/users/me/account-operations/{id}` | Estado propio sin pruebas/tokens | 404 genérico para operación ajena, expirada desconocida o inexistente según política anti-enumeración; no capability URL |
| POST `/api/v1/users/me/account-operations/{id}/confirm` | Consentimiento/expectedVersion; no UserId ni tokens externos →resultado idempotente | Revalidar ProofReceipts exigidos por operación D3, edad <=300s y versiones actuales, estado, candidato/alternativa, último método e inventario antes de commit; 409 colisión, 410 proof expirada, 429 abuso |

Unlink utiliza el identificador opaco del método propio devuelto al usuario; validar ownership e inventario otra vez. No se añade DELETE User habilitado: [estrategia de eliminación](../privacy-and-deletion.md) tiene gate separado.

## Protocolo de doble prueba, no una nueva emisión de tokens MiKompri

1. Users inicia operación autenticada, fija iniciador/binding/propósito/versiones y crea state/nonce/verifier de un solo uso. UI abre la acción autorizada; no acepta redirect de vuelta arbitrario.
2. Broker de prueba en Users (aplicación/redirect dedicados a definir tras aprobación) ejecuta Authorization Code + PKCE con emisores/retos pertinentes según D3 y sesiones de prueba aisladas: origen+candidato para vincular/migrar, alternativa que permanece para unlink/recovery. El callback verifica código, issuer, audience del broker, nonce/state, sesión iniciadora y evidencia de control reciente. Tokens/códigos no se transportan como parámetros de nuestros endpoints de negocio ni quedan en journal/logs. Si se usan cookies efímeras: HttpOnly/Secure/SameSite apropiado, anti-CSRF y origins exactos.
3. Aplicar [D3 y su matriz por operación](../architecture-review.md#d3--control-reciente-y-step-up-para-beta): edad de cada prueba <=300s al confirmar, operación <=600s, no prueba de otra transacción. Solicitar reauth/max_age cuando se soporte y validar ID token del broker con auth_time firmado **y semántica acreditada en G3**, o desafío CIAM equivalente verificable aprobado. auth_time CIAM no implica nueva credencial Google; prompt/iat/selector/consentimiento/MFA histórica no bastan. Fuente de tiempo no es callback/UI. Si falta evidencia, mutación bloqueada. Skew propuesto 30s no amplía edad máxima. Sin Native Auth JS añadido solo para probarlo ni OTP propio.
4. Target CIAM login dentro de operación no crea User nuevo ni sustituye cuenta A activa del SPA. ProofReceipt queda server-side; consentimiento final muestra método/cuenta a asociar sin exponer PII ajena.
5. A: adaptador CIAM resuelve identidades de objetos acreditados y muta Graph solo si el gate de capacidad/propiedad es positivo. No aceptar un Google subject escrito por cliente ni verificar Google JWT en APIs de negocio. B: añade binding CIAM demostrado. Resultado publicado solo después de reconciliar; incertidumbre →Reconciling, sin nuevos permisos.

La recuperación sin sesión previa usa un desafío del mismo broker con método alternativo ya vinculado y respuesta genérica. La forma definitiva del endpoint de inicio anónimo se fija después de G1/G3/anti-abuso; no se diseña un «enviar email y obtener UserId». Cambios de sesión incluyen invalidar consultas/requests tardíos por generación de cuenta, limpiar borradores y caché privada, sin reintentar escrituras financieras automáticamente.

Linking/migración prueban origen+candidato. Unlink exige control reciente mediante alternativa que permanece, consentimiento y checks de último método; no reautenticar obligatoriamente con el método comprometido que se retira. Recovery prueba alternativa ya vinculada y, si incorpora otro acceso, el candidato. Pérdida total sin prueba →sin autoservicio beta, nunca recovery por perfil. OTP primario no se etiqueta MFA; passkey actual no se presupone para OTP/social. No se cierran AC de linking/recovery por dejar el recorrido deshabilitado hasta G3.

## Integración y entornos

Web y futuro MAUI tienen registros independientes. SPA: redirect exacto `/auth/callback`, logout exacto, registro público sin secreto, MSAL authority conocida del CIAM y scope backend. Broker de pruebas tiene callback distinto. MAUI: registro público/PKCE, redirects y firma/paquete Android/deep links probados en su fase; no wildcard ni secreto embebido.

Local/cloud usan configuración separada; no copiar tenant IDs/redirects de producción a QA sin autorización. CORS solo orígenes Web conocidos; CORS no sustituye validación de redirect OAuth. IDs/scopes nuevos deben propagarse coordinadamente a tres APIs y forwarding UsersApi; no introducir OBO ni scopes Graph en tokens para MiKompri. No se modifica hoy ninguna configuración.
