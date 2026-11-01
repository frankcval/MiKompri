# ADR-006 — Google como única opción visible de acceso Web

**Estado:** aceptado temporalmente por instrucción expresa del titular del 01/11/2026: «solo deja Google, Microsoft ocúltalo por ahora». No autoriza despliegue ni cambios cloud.
**Fecha:** 2026-11-01.
**Spec relacionada:** [011](../../specs/011-web-iconography/spec.md), corrección de acceso autorizada después de la revisión visual.

## Contexto

Tras la revisión visual de 011, el titular reporta que introducir el código no completa el acceso. La Web QA apunta a las APIs correctas, pero sus tres contenedores y PostgreSQL estaban detenidos. Además, LoginPage podía repetir la preparación de cuenta al renderizar después de un fallo, sustituyendo el error por «Preparando tu cuenta…». El código de email en CIAM no demuestra que se haya elegido Google: el flujo QA conserva también la identidad email OTP del spike.

## Decisión

- Mostrar únicamente «Continuar con Google» en el login Web. Si CIAM no está configurado, mostrar indisponibilidad explícita, sin ofrecer Microsoft como alternativa automática.
- Mantener el acceso alojado CIAM existente: elegir «Iniciar sesión con Google» allí. No reintroducir `domain_hint=google`, que ya falló en QA; no alterar factores ni el flujo alojado para ocultar email OTP.
- Conservar configuración Microsoft legacy, validación JWT, sesiones existentes, D1 y la correlación canónica `UserId`. Ocultar el botón no elimina el proveedor ni migra sus usuarios. No vincular por email ni inferir que una cuenta Google posee los datos de una cuenta Microsoft.
- Ante un fallo de preparación, conservar el error y permitir «Reintentar acceso» con la sesión existente. Sin reintentos automáticos en cada render ni nueva autenticación obligatoria ante indisponibilidad de Users. Los errores 401/403/503 conservan su significado; no se elude autorización.
- Recuperar solo los contenedores QA existentes, con su volumen persistente; sin crear credenciales, recursos, migraciones o modificar datos administrativamente.

## Impacto y límites de TP11

Es una excepción temporal, autorizada por el titular, a la **disponibilidad de la entrada Microsoft en la UI** prevista por TP11/ADR-004 hasta acreditar cutover. Un usuario Microsoft sin sesión activa no tiene ahora botón para entrar: se acepta esa limitación de producto, sin declarar migración ni cutover acreditados. Las sesiones Microsoft existentes mantienen su ruta; una reautenticación de sesión legacy puede seguir utilizando Microsoft internamente.

No cambia la identidad canónica, trust, issuer/audience/scopes, propiedad de datos, D1 ni la constitución. No supone aprobación de una retirada permanente de Microsoft ni de 010-B. Cualquier retirada definitiva requiere la revisión y enmienda constitucional correspondientes.

## Alternativas Consideradas

| Opción | Ventaja | Coste o límite |
| --- | --- | --- |
| Solo Google visible, conservar trust legacy (elegida) | Cumple la decisión sin migrar identidades | Nuevos accesos Microsoft no tienen botón |
| Mantener ambos botones | Conserva disponibilidad UI legacy | Contradice la instrucción actual |
| Retirar Microsoft de APIs o convertir cuentas | Reduce rutas de identidad | Requiere transición aprobada; riesgo de bloqueo/duplicados |

## Consecuencias y reversión

Conservar ambos botones contradice la decisión actual del titular. Eliminar Microsoft de APIs/MSAL o convertir automáticamente cuentas a Google introduciría riesgo de bloqueo, duplicados y pérdida de acceso a datos; no se hace. Tampoco se modifica Acceso Condicional ni se deshabilita MFA para evitar un código.

Para restaurar la opción Microsoft, revertir únicamente su presentación y pruebas, revisar el estado operativo de sus APIs y acreditar el acceso legacy real. Para retirar definitivamente el acceso, aprobar primero una estrategia de transición con preservación de UserId; esta decisión no la sustituye. Revisar esta excepción antes de promover la beta o ampliar su cohorte; no tiene eliminación ni reactivación automática por fecha.

## Verificación

[Evidencia y comandos reproducibles](../../specs/011-web-iconography/evidence/authentication-recovery-2026-11-01.md). La comprobación Google real y las pruebas con fallos inyectados se identifican por separado. El resultado previo de ambos proveedores del 31/10 se conserva como evidencia histórica.
