# 010 — G3, autenticación reciente

Fecha 2026-10-30; base `b02947f`. D3 aprobado **para el spike**: prueba al confirmar <=300s; operación <=600s. No se rebajan límites ni se equipara OTP primario a MFA.

| Método | Clasificación actual | Motivo |
|---|---|---|
| Google | **NOT RELIABLE FOR G3** para la ruta ensayada | Token CIAM con auth_time nuevo tras solo seleccionar cuenta/consentir, confirmado por titular. prompt=login/max_age=0 no acreditan challenge Google nuevo. No se ensayó otro step-up CIAM |
| Email OTP | **INCONCLUSIVE** | Alta/reauth/refresh, auth_time firmado y OTP nuevo confirmados; faltan negativos, binding de operación y expiración de la estrategia elegida |

## Qué está preparado, sin atribuirle fiabilidad

Harness aislado en `tools/spikes/010-external-id/`, MSAL Browser 3.30.0, Code+PKCE, redirect exacto loopback. Ofrece inicio normal, prompt=login+max_age=0, selector y renovación silenciosa. El servidor registra en memoria el inicio de cada intento, recibe state devuelto por MSAL y muestra timestamps del access token **después** de verificar firma/issuer/aud/expiry/scope/tid/oid. Consume el marcador de intento una vez.

El marcador es instrumentación QA, no AccountOperation/IdentityBinding ni broker productivo. `withinG3TimestampBounds` solo calcula límites D3; `provesG3=false` deliberadamente. auth_time leído del ID token por el cliente se etiqueta como observación del cliente, no prueba server-side. No se exportan JWT/nonce/state/OTP. Token/iat nuevo, selector, consentimiento o MFA previa no acreditan control reciente.

La [solicitud federada real](google-federation-request.json) de inicio normal no contiene `prompt`, `max_age` ni `claims`. Solicita `openid email profile address`; no se persiste dirección ni se amplía el perfil MiKompri. Revisar minimización del scope `address` antes de implementación. Los parámetros observados de esta solicitud inicial no prueban el comportamiento de una futura reautenticación.

## Matriz obligatoria pendiente por método

### Ejecución Google real

[Acceso inicial](google-registration.json): token CIAM validado por backend, scope requerido, ID token como bearer401; [Graph](google-directory-correlation.json) confirma federated/google.com, objeto distinto del consumidor OTP. El titular había confirmado que esta cuenta Google no tiene cuenta Microsoft. No se crea usuario MiKompri.

[Reautenticación](google-reauthentication.json): solicitud CIAM con prompt=login/max_age=0; MSAL añadió login_hint y mostró contraseña sin Google. Para el diagnóstico se retiraron solo login_hint/X-AnchorMailbox de la misma solicitud, conservando PKCE/state/nonce/prompt/max_age; reapareció la opción Google. Este ajuste manual no es una solución productiva ni permite afirmar que el botón actual funcione sin cambios.

El retorno conserva tid/oid/sub/aud y avanza auth_time de1793372385 a1793372920 (inicio server1793372849, observado1793372921). **El titular confirma que solo seleccionó cuenta o aceptó permisos**, sin nueva contraseña/passkey/MFA. Por ello el timestamp firmado de sesión CIAM no demuestra control Google nuevo y la ruta es **NOT RELIABLE FOR G3**, aunque withinG3TimestampBounds=true. El access token de ese retorno expira en1793372921; no depender del skew para prolongarlo.

[Refresh silencioso posterior](google-post-reauth-silent.json) mantiene auth_time1793372920 y los identificadores, con exp1793377788. No aporta nueva prueba. No se cambian los límites300/600s, no se mutan las cuentas reales para linking/unlinking y no se descarta A ni se elige B por este resultado. Una alternativa de desafío verificable CIAM requiere revisión/ensayo separado; continúa INCONCLUSIVE.

### Ejecución parcial email

| Control ejecutado | Resultado observado |
|---|---|
| Registro real y retorno validado | auth_time1793365002, mismo tid/oid/sub/aud en retornos posteriores; ID bearer401 |
| forceRefresh silencioso | auth_time1793365002 permanece, iat cambia, sin marcador de challenge nuevo |
| prompt=login y max_age=0 enviados realmente | auth_time1793365309; inicio server1793365202, retorno1793365309, dentro de300/600s; mismo oid |
| Expiración de ese access token | exp1793365309, igual al retorno; aceptación dentro del skew30s, no evidencia de validez futura de300s |
| forceRefresh después | auth_time1793365309 permanece, exp1793370743 vuelve a futuro; no se transforma en prueba de nuevo challenge |

Archivos [registro](email-registration.json), [silent](email-silent-refresh.json), [reauth](email-reauthentication.json), [posterior](email-post-reauth-silent.json). El titular confirma explícitamente en esta sesión: **pidió e introdujo un OTP nuevo**, no retorno automático. Se registra solo esta observación, sin código/correo. La operación es instrumentación QA, sin linking ni confirmación de una operación sensible real. Faltan OTP erróneo/expirado/replay, selector/SSO y validación de estrategia de expiración sin depender de skew. Por ello email no se clasifica todavía RELIABLE FOR G3.

### Pasos restantes

1. Contexto limpio: inicio server-side, interacción real observada por titular, callback, auth_time firmado, hora de verificación. Comparar `AuthenticatedAt >= ChallengeStartedAt-30s`, no futuro >30s, edad <=300s al confirmar y operación <=600s.
2. Repetir con cookie CIAM existente; Google además con cookie Google y con selector de cuenta. Distinguir credencial/challenge real de SSO automático. Un nuevo auth_time tras solo SSO no basta para G3 social.
3. `prompt=login` + `max_age=0`: verificar parámetros enviados y comportamiento efectivo, no solo solicitud. Registrar si hay OTP nuevo, selección o challenge del proveedor. Repetir en cada método.
4. Renovar access token sin interacción; comprobar iat/auth_time, sin reclamar prueba nueva. Si cambia auth_time sin challenge, documentar límite y negar uso como evidencia de control nuevo.
5. Confirmar después de 300s y después de 600s: negar. No extender TTL por skew; probar timestamps ausentes/viejos/futuros y state/nonce/PKCE inválidos en controles separados del proveedor real.
6. OTP incorrecto/expirado/reutilizado y recuperación sin proveedor disponible; método alternativo superviviente debe probarse en unlink. Sin alternativa no hay bypass beta por email/nombre.

Para clasificar **RELIABLE FOR G3** hace falta nueva prueba de control verificable por backend y comportamiento consistente bajo SSO/negativos. **NOT RELIABLE FOR G3** requiere fallo observado de esa garantía; falta de ejecución = **INCONCLUSIVE**. La clasificación Google procede del retorno firmado y de la observación humana del desafío, no de editar este archivo.

Claims upstream pendientes de demostrar: la [documentación OIDC vigente](https://developers.google.com/identity/openid-connect/openid-connect) sí contempla auth_time/amr opcionales con solicitud claims y habilitación en settings. Falta demostrar que la federación CIAM elegida los obtiene/conserva y que su timestamp corresponde a control Google, no solo a nueva sesión CIAM. Activar2SV para administrar Google Cloud no es evidencia de G3 del consumidor ni nueva obligación de MFA de MiKompri.

La preparación posterior ya crea el cliente Google y lo añade al flujo QA. Settings observado en Testing mantiene deshabilitadas ambas opciones de claims avanzados; exige app publicada/verificada, según [Security Bundle](https://developers.google.com/identity/siwg/security-bundle). Esa guía distingue sesión Google y aplicación, y no admite Google Account reauth requests. QA permanece en Testing: no se publica para desbloquear claims ni se supone que prompt/max_age CIAM equivalgan a nuevo desafío Google. El acceso federado posterior sí se ejecutó: auth_time nuevo con solo selección/consentimiento. Ese resultado real, separado de esta documentación, determina NOT RELIABLE FOR G3 para la ruta ensayada.

[Controles temporales offline](g3-timing-controls.json):10/10 sobre límites300/301s,600/601s,skew30/31s y faltantes; no sustituyen observaciones del proveedor, binding de operación/sesión ni pruebas de OTP real erróneo/expirado/replay.

## Revisión siguiente completada solo como research

[Alternativas de step-up y ensayo propuesto](g3-step-up-review.md): contexto CA+desafío CIAM es candidato; no hay nuevo PASS. Email MFA social requiere resolver discrepancia de configuración, Every time reutiliza MFA hasta5min y `acrs` puede aparecer sin política protectora. Preservar tiempo real del factor, bootstrap y D3; no usar esos indicadores solos ni activar SMS/password/passkey como salida automática. Google observado sigue NOT RELIABLE FOR G3, candidato nuevo INCONCLUSIVE y email INCONCLUSIVE.

## Ensayo adicional autorizado posteriormente

[Preparación, transición y estado QA](g3-email-mfa-qa.md), [registro estructurado](g3-email-mfa-qa.json). Se crean contexto y política Google específicos, más respaldo administrativo tras autorización adicional de transición de defaults. El [control con política deshabilitada](g3-context-with-disabled-policy.json) devuelve c1 firmado sin claim amr: contexto no prueba MFA/G3. Intento de transición rechazado al activar respaldo; reversión ejecutada, con throttling y relectura final por verificar. No OTP MFA social nuevo ni G3 fiable acreditados; las clasificaciones anteriores no cambian.

Cierre posterior: [restauración verificada](g3-restoration-verification.json), defaults activo, ambas políticas deshabilitadas por GET nativo del portal200 y contexto sin publicar. Asignación revisada All/exclusiones conserva destinatarios efectivos, pero activación sigue limitada. Candidato email MFA social INCONCLUSIVE; no nuevo PASS/FAIL de fiabilidad del método por no llegar al desafío.
