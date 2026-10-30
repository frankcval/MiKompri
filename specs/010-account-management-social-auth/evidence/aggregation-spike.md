# 010 — Alternative A: probes reales Graph

Fecha 2026-10-30; base `b02947f`. [Resultados completos saneados](graph-aggregation-controls.json), [sondeo inicial independiente](graph-identity-probe.json). Fixture = objeto CIAM real con identificador federado ficticio; **no cuenta Google autenticada**. Cuenta local de laboratorio con password efímero CIAM = **no email OTP demostrado**.

## Resultados ejecutados

| Operación Graph en QA real | Resultado | Implicación |
|---|---|---|
| Crear y leer federado ficticio | PASS: identities contiene federated + userPrincipalName | Se observa colección real, sin probar control del proveedor |
| Federado-only → añadir emailAddress conservando oid | **FAIL** de ruta de cuenta con contraseña: HTTP400 Request_BadRequest | No demuestra fallo Google→OTP: el usuario OTP real usa federated/mail |
| Local → añadir federado desocupado | PASS mecánico: PATCH aceptado, mismo objeto, local y federado presentes | No acredita email OTP→Google real, doble prueba ni capacidad de login con ambos |
| google.com ficticio → añadir federated/mail ficticio | PASS mecánico en CIAM real; ambos presentes, mismo oid | Posible representación para Google→email OTP; falta login/control real en ambos métodos |
| federated/mail ficticio → añadir google.com ficticio | PASS mecánico, mismo oid | Posible representación para OTP→Google; no linking seguro acreditado |
| Quitar Google manteniendo mail | PASS mecánico | No se prueba acceso con mail superviviente |
| Quitar último mail dejando solo UPN | Graph acepta | La guardia last-method no puede delegarse en PATCH Graph |
| Quitar federado conservando local | PASS mecánico | Falta autenticar con método superviviente y validar G3 |
| Añadir federado ya propiedad de otro objeto | HTTP400, conflicto alternativeSecurityIds | No se fusionan automáticamente; no transferir ni borrar cuentas sin estrategia revisada |
| Quitar último método explícito dejando UPN | Graph acepta; lectura posterior solo userPrincipalName | **FAIL de la suposición «Graph protege último método»**. UPN no demuestra acceso válido; Users deberá aplicar guardia y prueba superviviente. No se ha ensayado recovery real |
| Dos PATCH concurrentes desde misma instantánea | Ambos204; queda una de dos adiciones | **FAIL de composición atómica sin coordinación**; colección completa es last-writer-wins en este ensayo |
| Cancelación cliente a1ms y relectura | Outcome desconocido al cancelar; read posterior funciona, candidato ausente en esta ejecución | PASS de observación/relectura; no prueba rollback, exactly-once ni garantías bajo fallo posterior a commit |

Los tres fixtures del runner se eliminan permanentemente en finally. El primer sondeo anterior fue soft-deleted y después retirado de deletedItems; no quedan credenciales de fixture. No se crea ni migra usuario MiKompri.

## Permisos: observado frente a mínimo documentado

El token CLI QA con Global Administrator y scopes amplios preautorizados (incluyen User.ReadWrite.All y Directory.AccessAsUser.All) puede crear/leer/editar/eliminar estos fixtures y listar flujos. **No demuestra least privilege**. identity/identityProviders devuelve403 con ese token.

La ruta de grant delegado adicional para CLI se probó y falló AADSTS65002: cliente first-party exige preautorización de Microsoft. Grant retirado. No debilitar políticas ni usar app production como sustituto. Portal administrativo QA permite crear flujo OTP y asociar SPA. Tercer cliente administrativo propio quedó solo previsto, no creado.

Se crea posteriormente un tercer **cliente consumidor diagnóstico**, no administrativo, solo para el negativo de scope. Vincularlo mediante PATCH de conditions falla400 por formato; usando la operación existente [Add includeApplication](https://learn.microsoft.com/en-us/graph/api/authenticationconditionsapplications-post-includeapplications?view=graph-rest-1.0), POST del subrecurso, sí se asocia con el token CLI amplio. No atribuir ese400 a falta de permisos. La lectura de identityProviders sigue siendo un403 distinto. El service principal CLI temporal ya se ha retirado, junto con el grant fallido; la sesión CLI preautorizada sigue pudiendo la operación observada.

[Graph Update user](https://learn.microsoft.com/en-us/graph/api/user-update?view=graph-rest-1.0) documenta User.ManageIdentities.All, reemplazo de colección y restricción al añadir local a social-only. Se confirma esta última **en CIAM real**, no solo en B2C por inferencia. [Create flow](https://learn.microsoft.com/en-us/graph/api/identitycontainer-post-authenticationeventsflows?view=graph-rest-1.0) documenta EventListener.ReadWrite.All y rol External ID User Flow Administrator. Una app propia con permisos mínimos y sus consentimientos sigue pendiente de ensayar; no afirmar mínimo a partir del token administrativo amplio.

## Decisión

**ALTERNATIVE A REQUIRES CHANGES.** La recomendación A no puede pasar a implementación como simple edición de identities. Revisión ADR requerida para distinguir **OTP federated/mail** de emailAddress/password, aplicar guards/serialización/reconciliación y resolver objetos separados. Los probes de ambas direcciones OTP/Google son positivos mecánicamente, **no** prueban que ambas autenticaciones reales resuelvan el objeto tras linking ni acreditan G3. El rechazo a añadir password no descarta la beta OTP. No recrear objetos a partir del mensaje de error: cambia oid y no constituye migración segura.

No se declara que todas las rutas A sean inviables por fallar un PATCH, ni se desarrolla B preventivamente. Si la revisión demuestra que ningún flujo A satisface el requisito, seleccionar B mínimo mediante ADR antes de esquema. No retirar el requisito Google-only→email ni auto-vincular por email para conservar A.

## Reproducción

Con inventario QA local y sesión CLI autorizada:

`pwsh -File tools/spikes/010-external-id/run-graph-probes.ps1`

Rechaza otro dominio/tenant no CIAM, crea solo tres fixtures nuevos, no autentica ni envía correos y elimina exactamente sus IDs. La segunda corrida amplía la primera con federated/mail tras observar el consumidor OTP real; el JSON conserva el resultado de esa corrida ampliada y el sondeo inicial está separado. G1/G3, linking entre cuentas Google/email reales, defensa A/B, recuperación y permisos mínimos permanecen abiertos.
