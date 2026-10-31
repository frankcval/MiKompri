# 010 — Contrato de token observado y controles

Fecha 2026-10-30; base `b02947f`. [Metadata saneada](qa-metadata.json), [controles JWT](jwt-controls.json), [Graph real negativo](graph-token-negative.json).

## Metadata real QA

Discovery se obtiene del subdominio QA `ciamlogin.com`. El **issuer anunciado** usa GUID de tenant: `https://<qa-tid>.ciamlogin.com/<qa-tid>/v2.0`. No equivale al hostname de discovery; comparar contra issuer exacto anunciado y fijado para ese tenant. Ocho claves JWKS observadas; RS256 anunciado.

La metadata no contiene `code_challenge_methods_supported`; no significa falta de PKCE. La petición real iniciada por MSAL incluye `code_challenge_method=S256`, response_type=code, state y nonce. No se guarda la URL completa, challenge, nonce ni códigos de autorización.

El primer intento del validador aislado esperaba issuer con subdominio y se detuvo por mismatch de metadata antes de validar. Se corrigió el harness tras observar el issuer real, sin introducir aliases ni cambiar validadores productivos.

## Contrato esperado frente a claims emitidos

| Claim/control | Regla del harness | Evidencia de access token consumidor CIAM |
|---|---|---|
| iss | Issuer exacto QA anterior | PASS email, coincide con metadata fijada |
| Firma/algoritmo | JWKS del discovery HTTPS, RS256, token firmado | PASS email en backend aislado |
| aud | App API QA, distinta de SPA y Graph | PASS email API; ID token SPA rechazado |
| exp/nbf | Validar lifetime; skew 30s | PASS registro/refresh; límite max_age=0 descrito abajo |
| scp | Token delegado debe contener `access_as_user` exacto | PASS presencia en positivos; negativo real todavía sin aislar |
| tid/oid | Tenant QA exacto y oid GUID no vacío | PASS estable en retornos email, correlación Graph comprobada |
| sub | Solo observación/digest; no identidad de dominio | Digest estable en retornos email capturados |
| auth_time/amr | Solicitados como optional claims; no se presume semántica | auth_time firmado presente; amr no se acredita; G3 sigue parcial |

Ejecución email: [registro real](email-registration.json), [reauth](email-reauthentication.json), [refresh](email-silent-refresh.json) y [refresh posterior](email-post-reauth-silent.json). Google real: [acceso](google-registration.json), [reauth](google-reauthentication.json), [refresh](google-post-reauth-silent.json) y [correlación Graph](google-directory-correlation.json). Los hashes de tid/oid/sub/aud permanecen iguales dentro de los retornos capturados de cada consumidor; los objetos Google y OTP son distintos. No se infiere estabilidad entre aplicaciones/tenants ni linking seguro. En ambos métodos el backend valida access token CIAM y rechaza ID token como bearer401.

Ningún `UserId`, OwnerId o relación MiKompri se modifica. Los fixtures Graph no acreditan correlación productiva ni estabilidad del oid entre logins.

## Ejecuciones

- **PASS control sintético:** 12/12 en harness .NET 8, JWT firmados RSA efímera: válido; issuer Google, audience Graph, ID token con aud SPA, audience incorrecta, scope incorrecto/ausente, tid ausente/incorrecto, oid ausente, expirado y firma incorrecta. No son tokens reales Google/CIAM ni acreditan G1.
- **PASS negativo real:** Azure CLI obtiene access token Microsoft Graph del QA; el validador API QA lo rechaza (`SecurityTokenInvalidSignatureException`). Se acredita exclusión real, sin atribuir el rechazo a una causa distinta de la observada ni afirmar validación positiva del contrato consumidor.
- **FAIL de adecuación de helpers actuales al contrato futuro, control aislado:** los tres helpers JWT existentes aceptan un JWT firmado para su issuer/aud sin scope o con scope distinto y sin tid/oid. Se enlaza su fuente exacta al proyecto del spike, sin editarla. Esto examina helpers, no pipeline completo: middleware/handlers pueden rechazar después. TP11/010 requieren política explícita adicional; no se arregla productivamente en este spike.
- **PASS negativo real:** el ID token devuelto por CIAM al cliente se envía como bearer al endpoint API QA y recibe401 en todos los retornos capturados. El cliente SPA y API tienen audiences distintas.
- **PASS negativo real de scope:** un cliente SPA QA **separado**, con solo diagnostic_only en su consentimiento, obtiene token CIAM válido para el mismo API. La observación server-side confirma firma/issuer/aud/lifetime aceptados, tenant correcto, mismo oid email, diagnostic_only presente, access_as_user ausente (un único scope). El endpoint API exige access_as_user y devuelve403; ID bearer401. [Evidencia](email-wrong-scope-isolated-client.json).
- Los dos intentos anteriores con cliente principal devolvieron scope requerido y se conservan como **INCONCLUSIVE de fixture negativo**, no bypass ni aprobación: [primero](email-scope-bundling.json), [grant temporal](email-wrong-scope.json). Grant principal restaurado. El cliente separado resuelve el aislamiento; no se deduce mínimo de scope emitido a partir del parámetro solicitado.
- **PASS negativo Google directo real:** [resultado](google-direct-token-negative.json), Google Identity Services devuelve un **ID token Google**, firma/issuer/audience del cliente Google QA/expiry validados únicamente por diagnóstico aislado; `/spike/claims` lo rechaza401. El hash de sub coincide con issuerAssignedId de la identidad google.com del consumidor CIAM ensayado. No establece principal de negocio ni valida acceso a una API Google; no confundir ID token Google con access token Google. auth_time ausente en este token; no acredita G3.

Con prompt=login/max_age=0, `exp` del token observado coincidió con auth_time/retorno; la validación positiva está dentro del skew configurado de30s. Una renovación silenciosa posterior conserva auth_time y obtiene expiración futura normal. No se promete que max_age=0 proporcione un bearer utilizable durante300s ni se amplía tolerancia para resolverlo. Falta ratificar la estrategia de prueba efímera G3 y su validez con expiración estricta. `iat` es anterior a auth_time en los ejemplos: tampoco acredita control reciente.

## Reproducción sin exportar tokens

`dotnet build tools/spikes/010-external-id/ExternalIdSpike.csproj -c Release`

`dotnet tools/spikes/010-external-id/bin/Release/net8.0/ExternalIdSpike.dll jwt-controls`

`pwsh -File tools/spikes/010-external-id/run-graph-token-negative.ps1`

El runner recibe token CLI en memoria y stdin del validador, nunca argumento/archivo. `run-auth-harness.ps1` sirve MSAL y valida bearer server-side; solo muestra hashes/claims temporales permitidos. No copiar token desde DevTools ni subir caches/snapshots del navegador. Config pública QA en `.local/qa-inventory.json`, ignorada; se reconstruye del inventario ARM/Graph autorizado si se pierde.

Negativo scope: usar `http://localhost:55412/negative.html` y login/renovación con cliente separado. `/spike/scope-observation` es diagnóstico aislado que exige JWT válido y devuelve únicamente flags/hash de claims; no consulta datos de dominio ni relaja `/spike/claims`, cuya política sigue exigiendo scope. No existe este endpoint en APIs MiKompri.

[Optional claims](https://learn.microsoft.com/en-us/entra/identity-platform/optional-claims) documenta configuración en el recurso emisor de access tokens; solicitar auth_time no demuestra que se emita ni que acredite reautenticación social.
