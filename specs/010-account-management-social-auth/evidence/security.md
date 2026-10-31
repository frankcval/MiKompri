# Seguridad010-A — revisión del candidato a9ca37d

31/10/2026; [amenazas](../threat-model.md), [validación](validation.md), [migración](migration-rehearsal.md). Revisión aplicable a010-A; no acredita controles010-B ni cierre global.

| Amenaza A | Evidencia ejecutada | Límite pendiente |
|---|---|---|
| Token ajeno/issuer cruzado/Google/Graph/ID | Matriz firmada20 HTTP por API y17 criptográficos; además Google directo real, Graph real e ID token CIAM401 en las tres APIs normales | Sin equivalencia con migración ni cambio de trust |
| Cuenta suspendida/token resucitado/Users caído | [Secuencia real saneada](d1-real-cutoff.json),200→403→401→200 estrictamente posterior; consumidores503 con Users caído | AR de traslado canónico real permanece en T042/T048 |
| Email/sub/perfil usados como identidad | Application niega adopción por esos atributos, PostgreSQL8 provisiones concurrentes→un Guid; reserva/origen retirado niegan alta | Cohorte federada real no acreditada, no se adopta mail ante colisión CIAM |
| Traslado al destino incorrecto | Verificador Graph7 controlados, PG7/7 con unicidad/versiones/locks/retry/rollback | Source authentication sí ejecutada, pero CIAM no acredita destino; no se aplica traslado |
| Mass assignment/perfil pisado | API If-Match/428/412/409, whitelist, username único/clear; dos relogin Google conservan campos propios y borrados | Alta Google nueva para importación del nuevo flow pendiente |
| Datos A visibles en B | Web activa/tenant/generación/abort, respuestas tardías/remount/caché/borradores; comparación real de perfiles Google/Microsoft | Matriz real completa/multi-tab no sustituida por suites |

## Tokens reales y reproducción

[Google real contra negocio](google-direct-business-negative.json): abrir diagnóstico loopback55412/google-direct.html, instalar attach-business-negative-probe.js con browser_run_code_unsafe y completar Google. El callback diagnóstico valida firma/cliente/lifetime con metadata real; el helper usa esa misma credencial solo en memoria para GET fijos en localhost28082/api/v1/users/me,28080/api/v1/purchaselists/mine y28083/api/v1/catalog-products, todos401. Lee solo window.qaGoogleBusinessNegative; no guardar request headers/storage. Avisos GIS de origen/COOP observados, sin deshabilitar protecciones; el callback firmado y los statuses sí se acreditaron. No confundir este diagnóstico con soporte Google directo de APIs.

[Graph real](graph-business-negative.json): token delegado QA existente, organization200 acredita su contrato real; tres rutas normales401. ID token de la SPA CIAM obtenido mediante MSAL después del login Google: mismas tres rutas401; el access token del recurso de ese mismo resultado devuelve200. Pruebas solo en memoria, sin exportación del storage ni tokens completos.

## D1 real después del corte

[Registro saneado](d1-real-cutoff.json). Token iat1793461754: tres APIs200. Suspender con User.SetSuspended(true, TimeProvider.System.GetUtcNow()) en base descartable MiKompri010QAUsers: corte1793462157, securityVersion1, tres APIs403. Reactivar mediante el mismo método/reloj: corte1793462173, version2; token anterior401. Petición realmente nueva aún emite iat1793461903 y se rechaza401: emitir después no garantiza iat posterior. Se detiene Users QA y ambos consumidores devuelven503; se reinicia y verificahealth200. Tras espera natural, forceRefresh devuelve iat1793462190 > corte1793462173 y las tres APIs responden200. No se modifica reloj/skew/corte ni configuración JWT.

Operación administrativa local: contexto EF apuntado literalmente a Host127.0.0.1/Port55432/DatabaseMiKompri010QAUsers, selección única de consumidor CIAM por hash SHA256 del UserId, llamada al método de dominio y SaveChangesAsync. Proyecto temporal ignorado .local/qa-status, sin endpoint nuevo ni SQL que rebaje cutoffs. Para repetir: login y GET normales; seleccionar únicamente cuenta QA propia; ejecutar SetSuspended en el orden anterior; conservar el token viejo en memoria y probarlo antes/después; adquirir forceRefresh hasta comprobar iat>corte y repetir los tres GET. No aplicar sobre bases originales, no usar ID token en el positivo. Estado final: cuenta activa/version2, Users saludable; el corte permanece elevado.

Los controles de no caché inter-request, respuesta incompleta/HTTP201/202 y reutilización de UserId dentro de ShoppingList request pasan en suites API actuales. La secuencia real anterior observa denegación al siguiente request; no cancela retroactivamente requests ya autorizadas. T042 conserva abierto el caso de cambio de canónica real, bloqueado por T048; no requiere varios bindings B.

## Secretos, minimización y configuración

Se conserva logging sin PII, configuración SPA pública, namespaces por ruta/tenant/cliente/cuenta; recovery en proveedor original. Credencial Microsoft anterior solo en CIAM/DPAPI con retiro previsto06/11. No contraseñas MiKompri, Graph en negocio, broker, nuevos permisos de negocio ni colección genérica de identidades. No se cambian CA/G3/defaults ni se crean licencias/trials/SMS/credenciales en esta revisión. Los nombres/apellidos del flujo son opcionales; no se añaden teléfono, nacimiento, dirección o foto. El candidato federado nuevo no tiene correo/contraseña/roles.

La revisión de archivos versionables y exclusiones se registra en validation.md; los logs/TRX/traces y .env/.local privados permanecen ignorados y fuera de Docker. No eliminar evidencia histórica válida por nombres de campos como accessTokenValidation. T041 no se cierra por esta matriz: los pendientes de migración/sesiones reales permanecen explícitos.
