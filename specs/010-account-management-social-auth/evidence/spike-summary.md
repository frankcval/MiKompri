# 010 — Resumen del spike controlado

Fecha: 2026-10-30. Rama `010-account-management-social-auth`; base `b02947f5f624cd0b1326018982967cae5325ab03` más harness/documentación de esta ejecución. D1–D3 aprobados por el usuario **solo para el spike**. No autorización de Phase3–8, ADR completo/enmienda, cutover ni implementación productiva.

Estado: acceso real Google sin Microsoft y email OTP con tokens CIAM validados en API QA. G3 Google falla para la ruta ensayada: auth_time nuevo con selección/consentimiento solamente, confirmado por titular. G3 email y agregación segura siguen abiertos. No convertir fuentes oficiales en PASS. PASS = caso descrito realmente ejecutado; FAIL = fallo observado de la hipótesis descrita; INCONCLUSIVE = falta demostrar la garantía de producto.

## Matriz de decisión

| Pregunta | Resultado | Evidencia | Decisión |
|---|---|---|---|
| Google sin Microsoft | PASS de acceso en harness | [Token validado](google-registration.json), [Graph](google-directory-correlation.json); condición sin Microsoft confirmada por titular | Flujo básico viable, sin crear usuario MiKompri ni acreditar linking/G3 |
| Email | PASS básico; negativos/recovery INCONCLUSIVE | [Registro](email-registration.json), [reauth](email-reauthentication.json), [Graph consumidor](email-directory-correlation.json) | OTP viable para acceso básico en harness; no cerrar aceptación completa |
| Token backend | PASS Google y email, aislado | [Contrato](external-id-token-contract.md), accessTokenValidation.accepted=true | API QA valida JWT CIAM; no se cambian tres APIs MiKompri |
| tid/oid | PASS estable por consumidor en retornos Google/email capturados | Registro/reauth/refresh y Graph: Google=federated/google.com, OTP=federated/mail; objetos distintos | UserId de dominio sigue independiente; no se crea User MiKompri ni migra ownership |
| Audience/scope | PASS positivo, tokens Google/Graph/ID bearer rechazados y scope negativo403 reales | [Contrato](external-id-token-contract.md), [Google directo401](google-direct-token-negative.json), [Graph rechazado](graph-token-negative.json), [scope403](email-wrong-scope-isolated-client.json) | Google directo es ID token GIS validado solo en diagnóstico, sin acceso de negocio; dos intentos previos de scope con cliente principal eran fixtures inconclusos |
| G3 Google | FAIL de ruta ensayada: NOT RELIABLE FOR G3 | [Reauth](google-reauthentication.json): auth_time nuevo, titular confirma solo selección/consentimiento | No habilitar mutaciones; estudiar desafío verificable CIAM mediante revisión, sin rebajar300/600s |
| G3 email | INCONCLUSIVE, resultados favorables parciales | auth_time firmado, refresh lo conserva, OTP nuevo confirmado por titular, reauth lo avanza; exp con max_age=0 requiere cierre | Mantener <=300/600s; no confundir token/iat nuevo con prueba ni basarse en skew para ampliar validez |
| Google→email | INCONCLUSIVE de producto; PASS mecánico OTP-shaped | [Probes Graph](graph-aggregation-controls.json), [análisis](aggregation-spike.md) | mail-federated se añade a google.com ficticio con mismo oid; falta doble prueba/login con ambos. emailAddress/password falla400: caso distinto |
| Email→Google | INCONCLUSIVE de producto; PASS mecánico OTP-shaped | Misma evidencia, dirección inversa | Proveedor Google real ya funciona; falta control reciente fiable y login tras vincular |
| Unlink | PASS mecánico, INCONCLUSIVE de acceso superviviente | Graph retira Google ficticio dejando mail; no login superviviente | Guardia Users y prueba de alternativa antes de mutar |
| Último método | FAIL de delegar protección a PATCH Graph | Permite retirar último mail explícito y deja solo UPN; no prueba acceso recuperable | No delegar la regla en Graph; último método y concurrencia deben controlarse en Users |
| Graph permissions | PASS con token amplio, INCONCLUSIVE de mínimo | [Agregación](aggregation-spike.md); IdentityProviders403; CLI scopes extraAADSTS65002 | Scope/rol mínimo requiere cliente propio QA o portal; no elevar APIs ni cambiar first-party Microsoft |
| Objetos CIAM duplicados | PASS de rechazo de identidad ocupada, INCONCLUSIVE de reconciliación/linking real | HTTP400 por alternativeSecurityIds duplicado | No fusión por email, no recrear/borrar objetos como solución automática |
| Alternative A | INCONCLUSIVE como solución completa; requiere cambios | Acceso básico Google/OTP real y probes bidireccionales positivos; G3 Google falla, linking seguro/permisos abiertos | Conservar candidata A, revisar assurance/guards/reconciliación antes de esquema; B no se implementa ni resuelve por sí sola G3 |
| Latencia D1 | PASS benchmark local; INCONCLUSIVE cloud/garantía D1 | [Benchmark](revocation-dependency-spike.md), [JSON](revocation-benchmark.json) | p95 recorrido extra6,072ms C1/8,241ms C8; no descarta beta ni acredita SLO cloud |
| Concurrencia | FAIL de reemplazo de colección sin coordinación | Dos204, una sola adición final | Serialización/journal/relectura en diseño, no PATCH ciego |
| Timeout | PASS de relectura después de cancelación, INCONCLUSIVE de exactly-once | Cliente cancela a1ms; candidato ausente al releer en ese ensayo | No asumir rollback de Graph ni reintentar a ciegas |

## Recursos y límites

[Plan previo de recursos y limpieza](spike-resource-plan.md), escrito antes de cada clase de creación/cambio.

- CIAM QA Europe/ES y RG exclusivo, API/SPA principal más SPA diagnóstico con consentimiento separado diagnostic_only, flujo email OTP y callbacks loopback. No secretos de SPA/API.
- Administrador humano y un consumidor OTP distintos; tokens/PII no se copian a evidence. Contraseña efímera del fixture local existe solo en memoria/request Graph y el objeto se elimina; no contraseñas propias MiKompri.
- Grant adicional CLI intentado, rechazadoAADSTS65002 y retirado; service principal CLI temporal eliminado. Cliente Graph administrativo propio **no creado**. [Limpieza verificada](qa-cleanup-verification.json). No se cambia política/preautorización Microsoft para eludir el rechazo.
- Tres fixtures por corrida ampliada eliminados permanentemente; fixture del probe inicial también retirado de deletedItems. PostgreSQL de benchmark eliminado, tmpfs sin volumen. No se tocan contenedores/BD actuales.
- Harness loopback55412 disponible para terminar QA; configuración pública ignorada en `.local/`, reconstruible con `get-qa-inventory.ps1`. No acceso a listas/grupos/gastos reales.
- Google: bloqueo2SV resuelto por titular; proyecto/cliente QA creados y proveedor añadido al flujo. Credencial únicamente en archivo privado externo con ACL usuario/SYSTEM y configuración del proveedor CIAM. IDs efectivos en `.local/google-inventory.json`, ignorado; evidence solo hashes. External/Testing confirmado. Opciones Security Bundle deshabilitadas en Testing; no se publica la app. Tester añadido, acceso/reauth Google y refresh ejecutados; G3 de la ruta ensayada no fiable.
- Coste esperado core dentro del agregado gratuito <=50.000MAU; sin SMS/M2M/Go-Local/hosting cloud adicional. Recursos QA quedan para completar el spike, con retiro previsto antes de2026-11-06; no hay tarea de borrado automático. Borrar recurso ARM no equivale a eliminar tenant.

## Validación ejecutada y no ejecutada

- Build Release del proyecto aislado: correcto, cero warnings/errores. .NET8.0.32 en ejecución; SDK10.0.402. Sin cambios de solución/proyectos productivos.
- [Validación y hashes de fuente](spike-validation.json): sintaxis JavaScript de dos páginas del harness y seis scripts PowerShell correcta; prerrequisitos Spec Kit resuelven010 y55 tareas únicas permanecen abiertas; enlaces locales comprobados. No se publican bin/obj/configuración privada.
- JWT RSA sintéticos:12/12; [resultados](jwt-controls.json). Helpers actuales sin política explícita scope/tupla no satisfacen por sí solos el contrato futuro; pipeline completo no ensayado por esos controles.
- Límites temporales offline:10/10; [resultados](g3-timing-controls.json). Incluyen300/301s,600/601s, futuro30/31s, challenge anterior30/31s y faltantes. Aritmética, **no prueba del proveedor ni implementación de operación sensible**.
- Graph QA real: colección/colisiones/last-method/concurrencia/cancelación, con fixtures identificadores ficticios y borrado en finally; los probes no hacen login Google. El acceso Google real se documenta por separado.
- Flujo email real Code+PKCE, token validado, ID bearer401, refresh/reauth y confirmación humana de OTP nuevo. Graph real como bearer API rechazado; token CIAM de cliente diagnóstico válido pero sin scope requerido rechazado403. No se guardan tokens completos.
- Google real sin Microsoft: token CIAM validado, objeto google.com distinto del OTP, identificadores estables en acceso/reauth/refresh. Reauth con selección/consentimiento solamente y auth_time nuevo: ruta no fiable para G3. Token Google directo GIS válido para su cliente rechazado401; hash de sujeto coincide con la identidad federada CIAM.
- Benchmark real PostgreSQL/EF/HTTP:8.000 muestras medidas y800 warmup,10.000 usuarios sintéticos; comandos y limitaciones en documento D1.
- No aceptación productiva, CI de nuevas funcionalidades, migración EF, BD MiKompri, account linking real, Apple/MAUI, recovery ni prueba de suspensión productiva. Google sí ejecutado en harness; no se cierra ninguna tarea por archivos existentes.

## Pendientes concretos y orden

1. Acceso básico Google sin Microsoft acreditado. Conservar cuenta QA y configuración aislada para revisar assurance; no vincular objetos reales sin doble prueba fiable.
2. Revisar G3 Google: la ruta prompt=login/max_age=0 con selección/consentimiento solamente no demuestra control nuevo. Resolver también el efecto login_hint observado y expiración del retorno; no habilitar mutación. Email: errores/expiración/replay OTP, binding de operación, límites al confirmar y recuperación.
3. Linking/unlink real en ambas direcciones con doble prueba; demostrar acceso con ambos métodos al mismo objeto, colisión entre objetos y alternativa superviviente. Probar permisos mínimos; no elevar aplicaciones de negocio.
4. Negativo scope real ya acreditado con cliente separado; conservar aislamiento y restauración del grant principal. No volver a presentar intentos con token que sí llevaba permiso como negativos aprobados.
5. Revisar ADR/A con esos resultados. D1 consulta final, fallos y red/cloud permanecen para fase autorizada posterior, sin cambiar budgets. No empezar Phase3–8.

Dictamen actual del spike: **ALTERNATIVE A REQUIRES CHANGES**. El acceso básico OTP/Google funciona; la ruta Google ensayada no acredita G3 y faltan agregación segura y garantías propias. B no resuelve por sí sola la frescura del proveedor. No se autoriza implementación por este documento.

Continuación documental: [revisión G3 y ensayo candidato](g3-step-up-review.md), fuentes revalidadas sin configurar ni ejecutar nuevas pruebas. Se propone contexto CA+desafío CIAM, email OTP social primero si compatible; SMS diferido a decisión de finalidad/coste. `acrs` sin política y tolerancia MFA5min no acreditan D3. No cambia ningún PASS/FAIL anterior ni se cierra tarea; el nuevo candidato sigue INCONCLUSIVE.
