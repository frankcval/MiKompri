# Seguridad 010-A — evidencia parcial

31/10/2026. [Validación](validation.md), [amenazas](../threat-model.md), [traslado](migration-rehearsal.md). No acredita cierre T041 ni controles010-B.

| Amenaza A | Evidencia ejecutada | Pendiente |
|---|---|---|
| Token ajeno/issuer cruzado/Graph/ID/Google | RSA sintético, matriz20 HTTP por API y17 criptográficos; issuer/tenant/audience/client/scope/firma/algoritmo/exp/nbf/tupla | Google directo real contra las tres APIs normales |
| Cuenta suspendida/token resucitado/Users caído | Tests D1, QA real histórica403/401/503; no caché entre requests ni fail-open; respuesta de estado inválida/201/202 rechazada | Positivo real con iat estrictamente posterior al corte; traslado real y manifiesto |
| Colisión de email/sub/perfil | Application niega adopción por esos atributos; PostgreSQL8 provisiones concurrentes→un Guid; reserva/origen retirado niegan alta | Aceptación externa de cohorte reservada |
| Traslado a cuenta equivocada | Verificador Graph7 casos controlados, unicidad/versiones/locks y PostgreSQL; sin email como prueba | Origen Microsoft federado real, cutover/rollback |
| Mass assignment/perfil pisado | API niega IDs/tenant/estado/verificación; If-Match/428/412/409, clear/claims y username concurrente único | Relogin humano personalizado del candidato |
| Datos A visibles en B | Web tenant/cuenta activa, generación/abort, respuesta tardía, remount, caches/borradores por contexto; logout SDK | Matriz real completa de proveedores/multi-tab/recarga |

Se retiró PII del logging de comandos. Solo configuración SPA pública; secretos de federación QA restringidos al CIAM y DPAPI local, con vencimiento/documentación de retiro. `.local` ignorado en Git y Docker. No contraseñas propias ni Graph por request de negocio, broker, nuevos permisos de negocio o colección de identidades genérica.

El error AADSTS90023 se corrige retirando aceleración opcional, sin relajar trust ni introducir tokens Google en APIs. El valor inicial unknown se diagnostica mediante booleanos, sin publicar perfil. El selector QA heredado aún contiene email OTP sin aceptación T056; no promover ese flujo ni anunciar ese método antes de resolverlo. Linking/unlinking/último método/G3/CA/MFA social quedan diferidos; el histórico INCONCLUSIVE sigue intacto.
