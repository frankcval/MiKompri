# Aceptación de proveedores — beta010-A

Decisión31/10/2026: Google sin Microsoft vía External ID y Microsoft legacy operativo. Código relevante idéntico entre a9ca37d y candidato6dd53e0; esta ejecución cambia documentación, no autenticación/configuración cloud. [Validación](validation.md), [seguridad/D1](security.md), [checklist](../checklists/acceptance.md).

| Gate beta | Resultado acreditado | Límite conservado |
|---|---|---|
| Google sin Microsoft / External ID | PASS: condición confirmada por titular; recorrido actual localhost→CIAM→GoogleSSO→Web y tres APIs200 | No es G3 ni proveedor social directo contra APIs |
| UserId Google | PASS: coincide con el de la nueva alta Web observada31/10, distinto del canónico Microsoft; no adopción por email | No prueba migración Microsoft |
| APIs/JWT | PASS: access token MiKompri200 en tres APIs; ID token CIAM401 revalidado ahora; Google directo/Graph reales401 acreditados con backend sin cambios y matriz firmada | Tokens solo en memoria, sin nuevo trust ni claims de seguridad inventados |
| Microsoft legacy | PASS: ruta/app/issuer actuales, UserId esperado y tres APIs200; reparación de entorno confirma sync200 y llegada a Mis listas | API anterior8082 tenía GET200/sync401; para beta se mantiene stack QA compatible28082/28080/28083 |
| D1 | PASS:200→403→401→token realmente posterior200; Consumers503 con Users caído | Se conserva evidencia, no se repiten cortes/suspensiones sin cambio relevante |
| Perfil | PASS: dos relogin reales conservan seis campos y luego clear; en login actual conservados username/nombre propios y null explícitos | Unknown/claims ausentes del objeto anterior no se rellenan por Graph |
| Aislamiento Google/Microsoft | PASS beta: ambos sentidos reales sin perfil/UserId/token cruzado; tests de tardías/TanStack/drafts/generación/activa/tenant | Matriz manual exhaustiva multi-tabT060 diferida, sin defecto observado ni PASS manual inventado |
| UX básica | PASS:320/390/430 sin overflow, botones44px; Tab alcanza Google, Microsoft y enlaces. Mensajes/cancelación/loading/return intent en suites | Lector/zoom completosT053 sin ejecutar, QA posterior no bloqueante |
| Microsoft→External ID | DEFERRED, ensayo INCONCLUSIVE por colisión CIAM | A-AC06/07 y T019/T048 abiertos, no gate beta |

## Suficiencia de alta Google para beta

La nueva alta Google en **Web/APIs normales** ya se observó el31/10: al usar Google alojado se creó una fila Users con UserId propio, distinto del Microsoft preservado, y se comprobó issuer google.com en CIAM. No se basa solo en el harness del30/10. Los retornos posteriores y el recorrido actual conservan ese mismo Guid. Negativos Application/PG ejecutados impiden adoptar al Microsoft por email/nombre/sub global y verifican provisión concurrente única.

Una repetición adicional con cuenta nunca registrada tras seleccionar atributos opcionales de nombre **no se ejecutó**. Limitación operativa: no hay una segunda sesión Google QA autorizada/autenticada disponible; las otras cuentas personales ofrecidas por el titular requieren admisión como tester y autenticación manual. No se solicitan correos/credenciales por chat ni se reciclan/borran usuarios para simular nueva alta.

**Decisión de suficiencia beta**, permitida expresamente por el alcance31/10: se acepta alta Web real existente + continuidad actual + código de provisión/JWT/perfil idéntico + pruebas actuales. La repetición de atributos va a T059 DEFERRED NON-BLOCKING QA. No se afirma importación de GivenName/Surname si CIAM no los aporta; unknown se conserva como limitación del registro previo y el perfil propio editable prevalece. Esto no reabre arquitectura ni habilita email/linking.

## Sesión, accesibilidad y opcionales

Los recorridos reales completos en ambos sentidos se detallan abajo. La nueva política beta acepta combinarlos con tests adecuados de sesión sin exigir todos los escenarios manuales multi-tab. T051 y A-AC09 cierran solo ese criterio beta; T060 queda abierto. Del mismo modo, T053 sigue abierto sin fingir lector/zoom probado. Los básicos anteriores sí se observaron.

T056 continúa A-OPT; Web no anuncia email. Selector CIAM QA puede conservar OTP heredado para no afectar consumidores/evidencia del spike. No configura segundo método ni010-B. Invitaciones/grupos quedan para012 independiente.

## Registro histórico íntegro

<details>
<summary>Recorridos anteriores y estados antes de aprobar el alcance beta</summary>

# Proveedores reales — aceptación parcial010-A

31/10/2026. Revisión inicial de46cf0ba y candidato de código a9ca37d. APIs normales QA aisladas, mismas validaciones productivas, sin mocks en los recorridos de esta tabla. [Validación](validation.md), [seguridad/D1](security.md), [migración](migration-rehearsal.md). El historial anterior en authentication-recovery-2026-10-31.md se conserva; esta tabla actualiza los pendientes que ya se pudieron ejecutar.

| Recorrido | Resultado observado | Límite |
|---|---|---|
| Google sin Microsoft | PASS: cuenta QA previamente confirmada por titular sin Microsoft; login normal con prompt=login y botón Google alojado, Users/me200, ShoppingList/mine200 y catálogo200 | SSO/selector no es G3; alta nueva para validar nuevos atributos del flow aún pendiente |
| JWT reales | PASS: access token CIAM200 en tres APIs; ID token CIAM401 en las mismas rutas. Token Google real con firma/cliente válidos401 en tres APIs; Graph real válido para organization200 pero401 en negocio | No se guardan tokens ni se alteró trust; negativos sintéticos adicionales en suites |
| Microsoft legacy, base46cf | PASS: ruta original, tres APIs200 (también mercados), UserId igual a semilla original y distinto de Google; perfil Google ausente | No es traslado a CIAM. Revalidación del candidato actual se registra al final |
| UserId Google | Mismo hash de UserId entre accesos; distinto del Microsoft legado | No adopción por email; no prueba migración |
| Perfil Google en a9ca37d | PASS: UI guarda DisplayName, username, givenName, familyName, español y orden name. Logout CIAM→Google login→los seis campos permanecen. Después clear explícito de givenName/familyName/idioma→guardar→logout/login Google→los tres siguen vacíos y DisplayName/username/orden se conservan | Sin cambios Graph de perfil, sin nombres/correos personales en evidencia. Logout CIAM requiere seleccionar cuenta y termina en pantalla «Cerró la sesión»; no vuelve automáticamente a localhost, se navega manualmente al acceso |
| Nombre inicial unknown | Flow QA ahora recopila Email y nombres opcionales; relectura/layout verificados | Falta nueva cuenta Google que nunca haya entrado en CIAM para acreditar importación. Los objetos previos unknown no se rellenan ni maquillan administrativamente |
| D1 real | PASS secuencia200→suspender403→reactivar401 con token viejo→token realmente posterior200 en tres APIs; Users caído503 en consumidores | iat anterior pese a nueva solicitud se rechazó; se esperó naturalmente sin bajar cutoff. No es G3 ni migración |
| Microsoft federado | INCONCLUSIVE: source Microsoft correcto y desafío aprobado, CIAM devuelve colisión de cuenta incluso con destino federado nuevo preparado | No Reserve/Apply/Rollback ni cambio de UserId; asociación experimental retirada, provisioning restaurado |
| Aislamiento completo | Parcial: Google→logout→Microsoft en base46cf, sin perfil cruzado; suites prueban cuenta/tenant/generación, caché/borradores/respuestas tardías | Matriz real completa con ambos sentidos/multi-tab y candidato actual no se infiere de tests |

## Perfil inicial y correo opcional

El titular ve unknown en una cuenta anterior y confirma que su personalización se guarda. No puede atribuirse un desafío CIAM previo a primario/MFA/verificación solo por el dominio de la URL. En el recorrido aquí observado Google usa SSO sin OTP; no significa que todos los usuarios/escenarios estén exentos de otros controles. Se corrige el shortcut de cuenta cacheada mostrando el acceso explícito; no se toca CA/MFA.

Email OTP se mantiene en selector alojado porque retirarlo afectaría el consumidor/evidencia del spike. No hay botón email en Web, no se habilita T056 ni se acepta como alternativa vinculada a Google. T056 sigue A-OPT;010-B queda fuera.

Pendientes indispensables: traslado Microsoft real con manifiestos/rollback y accesibilidad manual aplicable; cualquier matriz real de sesión aún incompleta conserva su tarea abierta. Sin cierre global de010-A ni despliegue productivo.

## Continuidad legacy y ambos sentidos — a9ca37d

Google logout→selección de cuenta CIAM→pantalla «Cerró la sesión»; regreso manual al login local→Microsoft select_account→perfil y tres APIs200. Hash del UserId Microsoft exactamente igual a la semilla esperada, perfil Google ausente. MSAL puede enumerar el mismo metadato de cuenta desde clientes distintos; no es un token de ese cliente: cero cuentas del tenant CIAM en la sesión Microsoft, ruta Microsoft seleccionada y peticiones con su access token.

Microsoft logout→selección de cuenta→logoutredirect→regreso al login local→Google/prompt=login→Google SSO→perfil. Mismo hash UserId Google anterior, tres APIs200, personalización/clear conservados; cero cuentas del tenant Microsoft en la sesión Google. No se observa perfil de la otra cuenta. Ambos sentidos quedan ejecutados sobre el candidato actual. Respuestas tardías, drafts/TanStack y generación están cubiertos por suites; la matriz real completa multi-tab/requests demoradas no se ejecutó y T051/A-AC09 siguen abiertos.

</details>
