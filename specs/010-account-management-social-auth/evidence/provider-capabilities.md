# 010 — Capacidades de proveedores: spike QA

Fecha 2026-10-30; base `b02947f5f624cd0b1326018982967cae5325ab03`. [Plan previo de recursos](spike-resource-plan.md). **PASS** significa ejecución del caso indicado; una fuente Microsoft o un fixture Graph no acreditan un login.

| Caso | Observado | Resultado de producto |
|---|---|---|
| Tenant aislado | ARM lo crea en Europe/ES; organization devuelve CIAM. Separado del tenant AAD previo | PASS de preparación, no de autenticación |
| App API + SPA | Registros separados, sin secretos; scope delegado `access_as_user`, consentimiento solo al recurso QA | PASS de configuración |
| Email OTP | Portal crea `MiKompri010QA`; Graph confirma `EmailOtpSignup-OAUTH` y asociación SPA; solo recoge email. Retorno real, token aceptado por backend y objeto consumidor distinto del administrador | PASS básico; [registro](email-registration.json), [correlación](email-directory-correlation.json) |
| Navegador Web | MSAL Browser instalado 3.30.0, code+PKCE S256, state/nonce y callback real | PASS de Google/email en harness aislado; no cliente MiKompri productivo |
| Google sin Microsoft | Tester sin Microsoft confirmado por titular; token CIAM validado y objeto federated/google.com distinto del OTP | PASS de acceso básico; [token](google-registration.json), [Graph](google-directory-correlation.json), no aceptación de linking/G3 |
| Email: login posterior | Reautenticación retorna mismo tid/oid y auth_time nuevo; renovación silenciosa mantiene auth_time | PASS del retorno estable; [reauth](email-reauthentication.json), [silent](email-silent-refresh.json) |
| Email: error/expiración OTP | No se han ejecutado OTP incorrecto, expirado o reutilizado | INCONCLUSIVE de negativos; no completar aceptación total email |
| MFA/recuperación email | No se configura MFA/SMS ni se acredita recovery | INCONCLUSIVE; OTP primario no es segundo factor |
| Microsoft personal / Entra legado | No ensayados: Google/email básicos preceden estos casos | INCONCLUSIVE; ninguna migración ejecutada |
| Apple / MAUI | Fuera del spike | No ejecutados |

El administrador invitado tiene un UPN interno `#EXT#`; no es un usuario cliente local OTP y el spike no le crea contraseña. Usar un correo QA distinto en el flujo consumidor. No almacenar ese UPN, nombre ni correo en evidence.

El consumidor OTP real tiene `signInType=federated`, issuer literal `mail`, además del UPN. **No** se representa como `emailAddress` de cuenta local con contraseña. Esto cambia la interpretación del probe inicial social→local; ver [agregación](aggregation-spike.md). El oid validado coincide con ese objeto Graph y no con el administrador; no se aprovisiona usuario MiKompri.

Google Cloud: el titular completó login administrativo y resolvió manualmente el bloqueo2SV. Después se creó un único proyecto QA, OAuth External/Testing, cliente web con siete callbacks de Microsoft y proveedor Google en CIAM, seleccionado junto con OTP. [Configuración real saneada](google-qa-configuration.json). El ID del proyecto se regeneró al usar Actualizar y se inventaría fuera del repo; no se crea otro proyecto para corregirlo. JSON de credenciales descargado directamente al destino privado, ACL usuario/SYSTEM; transferencia a Entra en memoria, sin imprimir secreto. No se inicia prueba gratis ni se vincula billing mediante automatización. Tester autorizado y acceso Google ejecutado con token validado. Esto acredita acceso básico; la reautenticación observada solo pidió selección/consentimiento y no acredita G3.

## Research revalidado, separado de ejecución

- [Métodos CIAM](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-authentication-methods-customers): enumera Google, email y federaciones; persiste una frase restrictiva sobre Native Auth.
- [Comparativa de enfoques](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-choose-authentication-approach): admite social en Native Auth con tramo navegador; no demuestra paridad de SDK .NET.
- [Tutorial Android](https://learn.microsoft.com/en-us/entra/identity-platform/tutorial-native-authentication-android-identity-provider-sign-in-sign-up): ejemplifica web flow en Native Auth Android. No se ha ejecutado MAUI ni elegido su SDK.
- [Google CIAM](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-google-federation-customers): requiere cliente OAuth web y secreto del proveedor, consent screen y redirect generado por el tenant. Prepararlo en proyecto QA, sin compartir secreto por chat.

Las discrepancias oficiales siguen registradas. D2 se conserva: Web browser-delegated; MAUI queda pendiente.

## Continuación exacta

1. En la pestaña del harness `http://localhost:55412/`, usar Registro/acceso QA → Crear cuenta; el titular introduce un correo QA distinto del administrador y el OTP solo en CIAM.
2. Retorno/token/oid email ya observados; completar OTP inválido/expirado y [G3](assurance-spike.md). No guardar capturas de buzón ni PII.
3. Proyecto/cliente/proveedor Google ya preparados. El titular ya añadió la cuenta tester y confirmó condición sin Microsoft; no se guarda correo en evidence. El panel Entra no generaba una URL única: se usaron las siete URLs de la guía Microsoft actual, resueltas en `.local/google-redirects.txt` ignorado. No se usan callbacks B2B/B2C de otro producto por similitud de nombre.
4. Acceso Google real acreditado; resolver G3 antes de casos de agregación con doble prueba. Microsoft adicional solo tras básicos completos.

## Estado beta aprobado31/10

La tabla de arriba corresponde al spike30/10; no define el estado del cliente actual. [Aceptación Web normal](real-provider-acceptance.md) acredita Google sin Microsoft, nuevo UserId y tokens de recurso en tres APIs, perfil persistente y Microsoft legacy. Actualización beta: Microsoft→External ID se difiere, no se intenta otra federación y A-AC06/07 no bloquean. Google nuevo Web existente más retorno actual/código idéntico se acepta para beta; repetición con nuevos atributosT059, lector/zoomT053 y matriz manualT060 quedan QA posterior no bloqueante. EmailT056 sigue opcional; G3 y métodos permanecen010-B.
