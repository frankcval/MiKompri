# Recuperación de autenticación — 31/10/2026

Rama `010-account-management-social-auth`, base `78b4a23`; corrección posterior en este commit. No acredita cierre de 010-A ni migración Microsoft. [Registro](implementation.md), [recursos QA](010a-qa-resource-plan.md).

## Entorno tras reiniciar

La Web conservaba configuración QA y sus APIs aisladas estaban detenidas. PostgreSQL QA utilizaba tmpfs: sus fixtures desaparecieron al reiniciar. Las bases originales no se modificaron. Se restauró primero `.env.local` original, comprobando igualdad con su respaldo privado, y se verificó salud/configuración de las tres APIs originales.

Después se preparó un nuevo ciclo QA explícito: PostgreSQL16 con volumen Docker dedicado, APIs en loopback28082/28080/28083, namespace de sesión nuevo y reinicio de contenedores QA configurado. Se copió únicamente UserId/tupla canónica del piloto Microsoft desde lectura del origen; no su perfil/historial. Esta semilla permite comprobar conservación del identificador al acceder por legacy, pero no demuestra traslado cross-tenant ni conservación de historia.

Microsoft permanece siempre en su ruta legacy. Se retiró del código la opción experimental de enviarlo por CIAM y del flujo QA únicamente la asociación del proveedor Microsoft experimental. Google y email OTP del spike se preservaron. No se retiraron proveedor/app/credencial QA inventariados, ni se cambiaron CA, defaults, métodos o registros existentes de Microsoft. Email OTP no se anuncia en la Web ni tiene aceptación T056; el selector QA heredado aún lo incluye. Esto requiere resolver configuración y aceptación antes de publicar ese flujo.

## AADSTS90023 al pulsar Google

El titular reprodujo en navegador real: `'google' '' pair is not an external identity provider`. La solicitud MSAL de la aplicación incluía `domainHint: 'google'`. La [documentación oficial](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-authentication-methods-customers), consultada31/10, sí documenta ese valor para acelerar Google. Por tanto no se atribuye el fallo a una errata de formato ni se sustituye por `google.com` de otro producto sin acreditarlo.

Se retira la aceleración opcional. La Web solicita el scope CIAM con `prompt=select_account`, sin `domainHint`/`domain_hint`; el usuario selecciona Google en la página alojada. Es el recorrido descrito en la [guía de Google](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-google-federation-customers). Se añade ayuda visible para explicar esa selección. No hay reintentos silenciosos, nuevos proveedores ni relajación de JWT/tenant/scope. La causa interna del rechazo CIAM no se ha demostrado; el cambio evita la solicitud que lo dispara.

Tests primero: fallaron la expectativa de configuración, la captura de `loginRedirect` y la ayuda ausente. Después de corregir: autenticación49/49; Web completo138/138, lint/typecheck/build exit0. La prueba completa se ejecutó con `VITE_ENTRA_TENANT_ID=ci-must-not-be-used-by-fixtures` para comprobar independencia del entorno. Esto verifica el contrato saliente; no sustituye el login real Google posterior a la corrección, solicitado al titular desde `http://localhost:5173/login`.

El titular confirma después: Google llega a «Mis listas» con el selector alojado. Informa de «Nombre visible: unknown». Lecturas de diagnóstico, sin publicar PII: tanto el User CIAM QA como su fila Users tienen ese valor; Graph no aporta givenName/surname y ProfileVersion=0. El alta copió el nombre disponible, no es una transformación del editor. Se propone utilizar «Guardar perfil» existente y repetir logout/login Google para acreditar personalización propia. No se modifica el perfil por SQL/Graph, no se deduce nombre desde email ni se añade una sincronización que sobrescriba decisiones del usuario. La procedencia de `unknown` dentro de la federación Google/CIAM no se ha demostrado.

**Corrección de atribución:** el titular añade que cada acceso pide OTP. La lectura de `identities` de la única fila CIAM local existente hasta entonces muestra issuer `mail`, no Google. Esto identifica la fila examinada, pero **no demuestra el recorrido más reciente del titular**: posteriormente confirma que sí pulsa Google y elige cuenta. La atribución inicial de ese acceso a Google, y la inferencia posterior de que habría elegido el formulario de email, no están acreditadas. Se solicita solo el dominio que muestra el desafío para distinguir Google de CIAM. La edición propia se guarda; todavía no se atribuye a relogin Google. Llegar a listas no basta para identificar el proveedor.

Recuperado el navegador de pruebas, se observa «Seleccione una cuenta» →«Usar otra cuenta» →formulario email **y botón separado «Iniciar sesión con Google»**. Al pulsar ese botón, SSO Google existente vuelve a localhost/lists sin OTP; Users crea una fila distinta y Graph acredita issuer google.com. Este es el positivo Google del candidato corregido. La Web aclara la secuencia; no se fusionan cuentas mail/Google por email. Nombre: el objeto Google QA también almacena unknown y no aporta nombres. Lectura del portal QA confirma que el flujo solo recopila Email; Display Name/Given Name/Surname no están seleccionados. No se modifica configuración cloud ni perfiles durante este diagnóstico. Falta configurar/probar inicialización de atributos en un alta Google nueva; no usar cambios del IdP para sobrescribir perfiles existentes.

## CI Web

Sobre `78b4a23`, Users/ShoppingList/ProductCatalog/Resource terminaron success. [Web](https://github.com/frankcval/MiKompri/actions/runs/41194726785) falló en tests: fixtures AccountInfo del tenant cero dependían de `.env.test` ignorado, mientras CI usaba otro tenant. Se fija explícitamente el tenant sintético en setup Vitest; no se modifica la validación productiva. Regresiones locales138/138 tras ese cambio. CI del nuevo commit debe verificarse por SHA, sin reutilizar los cuatro PASS anteriores como resultado final.

## Pendiente

- Google real confirmado por navegador de pruebas y asociación Graph, sin OTP. Falta identificar el desafío OTP adicional del titular, confirmar Microsoft y logout entre cuentas sobre este candidato. Empezar desde localhost; recargar la página antigua del error no inicia otra solicitud OAuth.
- Completar perfil personalizado → logout/login real y negativo de token Google directo contra las tres APIs normales.
- Acreditar origen Microsoft federado, traslado y rollback con manifiesto completo. Sin origen acreditado no se hace cutover ni se crea User sustituto.
- Validación final D1 tras corte, CI exacto y accesibilidad manual pendiente. G3/010-B siguen diferidos.

La Web se mantiene temporalmente en QA mientras el titular repite la prueba. Los datos QA son independientes. Tras el ensayo se restaura el respaldo original de `.env.local`; las claves CIAM de ejemplo siguen deshabilitadas por defecto en fuente.

## Actualización del candidato a9ca37d

Las observaciones anteriores sobre select_account y flow solo Email corresponden al diagnóstico previo. Google ahora solicita prompt=login, sin hint; Microsoft conserva select_account. El flujo QA incorpora nombres opcionales pero falta alta nueva real para acreditar importación. No se modifica unknown por Graph.

Se completan negativos Google/Graph/ID CIAM contra tres APIs normales, perfil propio y clear con relogin Google, y D1 positivo con iat posterior al corte; [resultado vigente](real-provider-acceptance.md), [secuencia](d1-real-cutoff.json). Microsoft legacy sigue conservando su Guid y respondiendo200. Migración federada devuelve colisión, se cancela y restaura asociación/provisioning; [estado exacto](migration-rehearsal.md). EmailT056 sigue opcional no habilitado en Web. No se reabren G3/CA.
