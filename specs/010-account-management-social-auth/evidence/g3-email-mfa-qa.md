# G3 — Preparación QA de Authentication Context + email OTP MFA

2026-10-30; base `010d298`. [Resultado saneado](g3-email-mfa-qa.json). El usuario autorizó expresamente contexto+email OTP MFA para Google, tenant/usuarios QA; SMS y demás opciones fuera. Este registro continúa la [revisión documental](g3-step-up-review.md), sin convertir configuración en aceptación de G3.

**Estado actual:** candidato MFA social **INCONCLUSIVE**, no desafío ejecutado. Configuración G3 y respaldo administrativo preparados; la activación del respaldo se rechaza o limita incluso tras revisar asignación/API. Se detienen nuevas transiciones globales y se revierte. Contexto sin publicar y políticas deshabilitadas; [verificación de restauración](g3-restoration-verification.json). Negativo real: c1 firmado con política deshabilitada. SMS/métodos primarios/identidades/BD productiva intactos. Las secciones siguientes conservan la secuencia, incluidos fallos y correcciones.

## Preparado y verificado

- Tenant CIAM QA comprobado por Graph organization y sesión portal del tenant. CLI no puede leer contextos/métodos (403); lectura de políticas produjo429. No se amplían permisos CLI ni aplicaciones de negocio. Se continúa por portal administrativo existente.
- Métodos en portal: email OTP ya habilitado para todos los usuarios del tenant QA; SMS deshabilitado. No se cambian esos ajustes ni métodos restantes. El portal advierte que algunos métodos no aplican a usuarios externos de autoservicio; habilitado no demuestra MFA social compatible.
- Contexto `c1`, nombre `MiKompri010G3EmailOtpQA`, creado **sin publicar**. Portal confirma creación y el contexto aparece como destino seleccionable de política.
- Política del mismo nombre creada **deshabilitada**: un único consumidor Google QA, previamente acreditado por token/identidad Graph; no incluye administrador ni consumidor OTP. Solo target `c1`, sin all-resources. Grant MFA, sesión Every time para autenticación primaria/secundaria. Opción de frecuencia estricta preview observada, no seleccionada.
- Payload de creación observado mediante petición normal del portal a Graph beta. Relectura de colección HTTP200 confirma nombre/ID/estado/ámbito. Esa proyección no contiene grant/session: no interpretar ausencia como false; detalles de la política en UI confirman MFA y reautenticación por sesión.
- IDs y snapshot público de configuración quedan solo en `.local/g3-qa-inventory.json` y `.local/g3-policy-snapshot.json`, ignorados. Evidence contiene hash del ID de política; no emails, nombres personales, tokens, OTP ni credenciales.

## Requisito adicional observado

El formulario muestra: «Primero debe deshabilitar los valores predeterminados de seguridad antes de habilitar una directiva de acceso condicional». No se ha intentado activar la política ni se afirma rechazo de una petición de activación al servidor.

La documentación [Security defaults](https://learn.microsoft.com/en-us/entra/fundamentals/security-defaults) también requiere deshabilitarlos para reemplazarlos por Acceso Condicional y mantener protección con las nuevas políticas. Es un ajuste del tenant completo, incluido su administrador; no es limitar una política al consumidor Google. El [plan previo autorizado](spike-resource-plan.md) preserva defaults y políticas existentes. Por eso no se modifica ese ajuste dentro de la autorización acotada actual.

**Bloqueo concreto de ejecución:** resolver y autorizar la transición temporal de protección del tenant QA, con cobertura administrativa y rollback revisados antes de deshabilitar nada. No proponer simplemente apagar defaults y dejar al administrador fuera de toda protección. Si la transición requiere métodos, credenciales, licencias o coste adicionales, detener y presentar el impacto; SMS sigue excluido.

## Estado de aceptación y continuación

| Comprobación | Resultado |
|---|---|
| Contexto y política específicos preparados | PASS de configuración deshabilitada; no autenticación |
| Aislamiento de destinatario y estado | PASS de relectura: consumidor Google1, contexto1, disabled |
| Compatibilidad Google+email OTP MFA | INCONCLUSIVE; no desafío ejecutado |
| Bootstrap, tiempo del factor y tolerancia MFA | INCONCLUSIVE; no nuevo recibo/evidencia de control |
| G3 de la ruta Google anterior | NOT RELIABLE FOR G3; sin cambios |
| Nuevo candidato de G3 | INCONCLUSIVE, pendiente de requisito de activación |

Ninguna tarea T004/G1/G3 ni AC de linking se cierra. No se modifica harness, código productivo, BD, flujo primario, usuarios/identities, credenciales, SMS o constitución. No se vinculan métodos reales ni se pide OTP al titular antes de disponer de la política efectiva.

Continuación propuesta: revisar sustitución temporal segura de defaults exclusivamente en QA y su reversión; con autorización suficiente y protección administrativa preparada, activar política/contexto y ejecutar la matriz del documento G3. Sin esa ampliación, mantener política deshabilitada/contexto sin publicar. Limpieza: retirar solo esta política/contexto al terminar, según inventario, antes del retiro QA previsto2026-11-06; no hay borrado automático.

**ALTERNATIVE A REQUIRES CHANGES**.

## Continuación autorizada de transición QA

El titular autoriza posteriormente la transición segura solo QA; [ampliación previa de recursos](spike-resource-plan.md). La sesión delegada del portal permite lecturas Graph adicionales sin nuevos grants ni app/credenciales: se verifica tid QA antes de usarlas y no se imprimen tokens. Security defaults devuelve200/isEnabled=true. Un único administrador humano Global Administrator; el otro rol tiene un service principal. Token administrativo actual contiene MFA, pero métodos locales solo password: eso no demuestra enrolamiento local ni un reto administrativo nuevo. No inscribir métodos nuevos para sortearlo.

Se crea `MiKompri010G3BaselineQA-AdminMfa` deshabilitada, para ese administrador, todos los recursos y clientes, grant MFA. POST201; GET directo inmediato404; la relectura de colección200 sí encuentra y confirma configuración/estado. Conservar esa diferencia y no repetir POST por un404. La activación devuelve429/TooManyRequests; relectura confirma disabled. No se modifica Security defaults ni se declara protección CA efectiva. Respetar throttling antes de reintentar y mantener reversión preparada.

El harness aislado añade solicitud de `acrs=c1` y flags de claims validados; siempre `provesG3=false`. Prompt select_account evita el login_hint automático observado en MSAL local3.30.0; no acredita frescura. [Control real con política G3 deshabilitada](g3-context-with-disabled-policy.json): token CIAM aceptado, c1 presente y sin claim amr; timestamps dentro de límites. Esto demuestra que el contexto solo no es prueba MFA/G3. No atribuir un desafío humano sin observación.

Build Release0warnings/0errores, controles JWT12/12 y temporales10/10, sintaxis JavaScript y runner PowerShell correctos después de instrumentar. No código productivo ni cambio de identidad/BD. Queda pendiente activación segura y desafío email MFA real; no cerrar G3 por esta preparación.

### Intento de transición con reversión

Activar el respaldo con defaults aún activo devuelve400: «Security Defaults is enabled in the tenant. You must disable Security defaults before enabling a Conditional Access policy.» Con snapshot y autorización preparados, se intenta el orden compatible: defaults off204 →activar respaldo. Esta activación también devuelve400 y no se continúa con G3. La causa de ese segundo400 no se guardó completa en esa respuesta, por lo que no se atribuye a licencia ni a propagación.

La reversión despublica contexto204, deshabilita política G3 y respaldo204, y restaura defaults: primer intento429/Retry-After60, segundo204. En ese primer rollback la espera técnica se limitó a30s, inferior al Retry-After; corregido para intentos siguientes usando la espera completa en tramos<=30s. Lecturas posteriores confirman ambas políticas deshabilitadas, pero defaults/contexto devuelven429. El204 de restauración es acuse, no se presenta como relectura final acreditada.

Se prepara diagnóstico acotado que conserva el mensaje de error y solo reintenta conflicto explícito de defaults; si hay429 respeta Retry-After completo. Su preflight devuelve429/60s antes de modificar nada. Esperar la ventana completa y verificar restauración antes de nuevo cambio. Lectura de auditoría200 no aporta causa del400; subscribedSkus200 vacío tampoco prueba que las capacidades CIAM de CA requieran comprar P1. No activar trials/compras ni inferir esa causa.

Diagnóstico posterior: defaults off204 →activación de respaldo429/60s →reversión completa204; no se llega a habilitar G3. [Secuencia saneada](g3-transition-attempts.json). Tras ventana de espera, [relectura de restauración](g3-restoration-verification.json): defaults200/true, colección políticas200 y ambas disabled, contexto isAvailable=false, acceso administrativo de lectura correcto. El GET de contexto se reporta201; conservar la anomalía HTTP en evidencia, sin interpretarla como nueva escritura. Se elimina la referencia administrativa temporal mantenida solo en memoria del proceso Playwright, sin archivo/token exportado.

Se revisa además la forma de asignación documentada para CIAM (All con exclusiones). El inventario sigue siendo exactamente administrador/Google/OTP. Se intenta ajustar solo el respaldo deshabilitado, preservando su destinatario efectivo y los demás controles; Graph devuelve429/Retry-After60. No se modifica defaults ni se confirma cambio de asignación. Es una hipótesis de compatibilidad, no causa probada del400. Evitar nuevas alternancias de defaults mientras ni siquiera pueda actualizarse la política deshabilitada; respetar espera y diagnosticar por esa operación reversible antes de otra transición.

Tras la espera completa, ambas actualizaciones devuelven204: All con dos exclusiones por política; relectura200 verifica un único destinatario efectivo, respectivamente administrador y Google. G3 conserva target c1, respaldo conserva MFA/todos los recursos y ambas siguen disabled. La UI del portal muestra ambas políticas deshabilitadas; su GET beta200 confirma asignación y fecha de respuesta actual. No afirmar que esa revisión resuelva el400, cuya causa posterior a defaults off sigue sin acreditarse.

Otro preflight se detiene por429 antes de modificar defaults. Una lectura mínima con no-cache devuelve200/defaults=true; lectura de contexto sigue reportando201 con isAvailable=false. Se usa la API beta observada del portal para el último intento, con esa instantánea reciente y scopes verificados: defaults off204 →respaldo429/60s →reversión. Rollback de contexto/G3204, rollback de respaldo429 y después204 tras esperar60s completos, defaults on204. No se alcanza la activación de G3. No repetir más transiciones globales para obtener un PASS; detener mutaciones y cerrar relectura protegida.

El bloqueo demostrado es la imposibilidad de completar una activación administrativa verificada en estos intentos por rechazo/throttling, no un fallo demostrado de email MFA social. La siguiente ejecución debe diagnosticar el servicio/entitlement y operación de activación de CA en este tenant antes de retirar de nuevo defaults; no activar SMS, password, licencias/trials, otro tenant o B automáticamente. T004/G1/G3 y aceptación permanecen abiertas.

## Cierre de restauración

[Lecturas finales independientes](g3-restoration-verification.json): defaults200/isEnabled=true con fecha de respuesta actual; GET nativo del portal Graph beta200 confirma ambas políticas disabled y All con dos exclusiones. Contexto isAvailable=false, conservando el201 anómalo del GET como diagnóstico. No hubo mutaciones cloud entre esas lecturas. Se confirma retirada de la referencia administrativa temporal en memoria. El429 intermedio de la colección no se usa como prueba de estado: se cierra con la lectura nativa200 posterior.

Quedan dos políticas QA deshabilitadas y contexto sin publicar para revisión/limpieza prevista; no hay protección administrativa sustituida activa. Ningún método SMS/otro factor, flujo primario, identidad, licencia o dato productivo cambiado. El harness y evidencia son los únicos cambios locales de código/documentación. Resultado final del candidato: **INCONCLUSIVE**, sin OTP MFA social ejecutado. **ALTERNATIVE A REQUIRES CHANGES**.
