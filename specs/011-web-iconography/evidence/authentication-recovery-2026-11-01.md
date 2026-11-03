# Recuperación de acceso y entrada Google — 01/11/2026

Rama `011-web-iconography`, base `f9cc1b0102fadf7a68b428917d377b25c20ce8a4`. Corrección local posterior al candidato publicado; su CI anterior no acredita este diff. El titular solicita resolver el acceso tras introducir el código, dejar solo Google visible y documentarlo. [ADR-006](../../../docs/adr/ADR-006-temporary-google-only-web-login.md).

## Diagnóstico y corrección

| Hallazgo | Evidencia | Corrección |
| --- | --- | --- |
| APIs QA y PostgreSQL detenidos | `docker ps -a`: los cuatro contenedores `mikompri-010a-*` estaban Exited; no había listeners28082/28080/28083/55432. La Web ya apuntaba a280xx y CIAM estaba habilitado | Arrancar los cuatro contenedores existentes, PostgreSQL primero; tres `/health`200. Volumen persistente `mikompri-010a-postgres-data` conservado |
| Preparación de cuenta podía repetirse después de fallar | El catch reiniciaba `redirected.current`; con nuevas referencias MSAL y render posterior, la prueba roja503→éxito continuaba sin pedir reintento y otra quedaba en «Preparando…» | Mantener el intento terminado; mostrar error y botón «Reintentar acceso». El usuario decide reintentar con su sesión existente. Mensajes401/403/503 conservados |
| Microsoft seguía visible | DOM inicial: dos botones Google/Microsoft | Solo Google en LoginPage; sin fallback Microsoft si CIAM no está configurado. Perfil conserva instrucción genérica de recuperación con proveedor original |
| Riesgo de servidores distintos en el mismo puerto | Se había encontrado Vite propio en127.0.0.1 y Vite del usuario en::1. El proceso propio se retiró tras verificar su comando; se conserva el servidor del usuario | Un único runtime de desarrollo en localhost:5173, callback en el mismo origen. QA privado sin restaurar variables antiguas; pruebas aisladas en5181 |

La indisponibilidad local de APIs está demostrada. No se observó la pantalla original del código que falló: no se atribuye un rechazo OTP a Google/CIAM sin prueba. El flujo alojado todavía tiene email OTP; introducir el correo allí puede seleccionar ese método. Para Google hay que pulsar su botón alojado. No se ha alterado el tenant para evitar códigos, MFA o selección de cuenta.

## Comprobación real con servicios QA

Navegador controlado, Web localhost:5173 y servicios persistentes existentes, sin fixtures en el primer recorrido:

1. Login: único botón «Continuar con Google», cero Microsoft. Abre CIAM; se pulsa «Iniciar sesión con Google».
2. La sesión Google existente permite SSO y vuelve a `/lists`, cabecera «Mis listas». POST de preparación y consultas iniciales200. No se introduce un OTP nuevo ni se simula un token.
3. `/profile` abre Perfil y sigue disponible después de recargar. No se edita ni guarda el perfil.
4. «Cerrar sesión» lleva al selector de cierre CIAM; se confirma la única cuenta QA mostrada. Visitar `/profile` vuelve a `/login`, sin mostrar datos privados. El selector intermedio es interacción alojada, no un fallo de cierre.
5. Google de nuevo, sin nuevo código en este recorrido: vuelve a Perfil respetando la intención protegida. Microsoft sigue ausente de la UI.
6. Con esa sesión real se inyecta **solo una respuesta503 en el navegador** a `/users/me/sync`. Error estable, un único intento. Se pulsa «Reintentar acceso»: POST real a Users200 y llegada a Mis listas; GET Users perfil/grupos y ShoppingList listas personales/compartidas200. Se retira la interceptación en `finally`. La primera503 es inyectada, no una caída real del backend ni una prueba de suspensión D1.
7. Se abre Catálogo con la misma sesión: GET ProductCatalog `/api/v1/catalog-products` y `/api/v1/markets`, ambos200. Se deja el navegador de vuelta en Mis listas y el Vite del usuario activo; el servidor aislado de pruebas5181 ya no escucha.

Estos recorridos no prueban Google en un navegador sin sesión previa, todos los tipos de código/MFA, linking ni migración Microsoft. No se publican cuentas, tokens, correos, OTP, cookies, storageState o URLs OIDC completas. No se cambian datos de dominio administrativamente; el login conserva su sincronización normal de perfil.

## Validación ejecutada

Desde `MiKompri.Web`:

| Comando | Resultado |
| --- | --- |
| `npm test -- tests/component/auth/ProviderLogin.spec.tsx tests/component/auth/LoginContinuation.spec.tsx` antes de corregir | 1PASS/4FAIL; evidencia roja de UI/reintento |
| Mismo comando después | 5/5PASS |
| `npm test -- --reporter=default` | 162/162PASS,48archivos;202.64s. Incluye regresiones de sesión, aislamiento, autorización, perfil, retorno y expiración. Warnings React `act` en navegación lazy conservados |
| `npm run lint` | PASS |
| `npm run typecheck` | PASS |
| `npm run build` | PASS; aviso de chunk inicial>500kB conservado, sin modificar budget |
| `PLAYWRIGHT_ALL_BROWSERS=1`, `npx playwright test --config ../specs/011-web-iconography/.local/playwright-isolated.config.mts tests/e2e/home/login.spec.ts tests/e2e/home/home.spec.ts --output=test-results/011-auth-recovery` | 21/21PASS:7por motor Chromium/Firefox/WebKit;56.7s,2workers,0retries |

Para esta máquina, definir también `PLAYWRIGHT_BROWSERS_PATH=E:/Projects/MiKompri/.playwright-browsers`, donde ya están instalados los motores. La primera ejecución sin esa variable dio21errores de lanzamiento por ejecutable ausente en la caché predeterminada; no ejercitó la aplicación. Se corrigió solo la ubicación de motores y se repitió completa, sin instalar nuevos ni relajar pruebas.

El config aislado reutiliza el config del repo, cambiando solo servidor/URL a127.0.0.1:5181 (`reuseExistingServer:false`) y salida de resultados. No toca el Vite del usuario ni su configuración de autenticación. En otro equipo puede usarse el config normal con5173 libre; instalar sus navegadores según README si no existen. E2E usa harness/fixtures: no reemplaza el recorrido Google real anterior.

Después de la última modificación de texto de recuperación en Perfil: `npm test -- tests/component/auth/ProfileEditing.spec.tsx`1/1PASS, seguido de lint, typecheck y build todosPASS. No se atribuye esa última modificación a la ejecución completa anterior. `git diff --check`PASS. Las comprobaciones reales de Catálogo anteriores también utilizan ese texto final.

## Límites y publicación

No cambios backend, trust/JWT/MSAL, env versionado/privado, migraciones, datos administrados, licencias ni cloud. No se usa un token Google contra las APIs. Se preservan `UserId`, D1 y las tareas abiertas007/008/009/010. T019/T025/AC09 de011 siguen diferidas, sin fingir QA manual.

Microsoft oculto es una limitación explícita para nuevos accesos legacy, no un cutover completado ni una retirada del proveedor. El periodo beta anterior sigue siendo histórico. Publicación del commit solicitada posteriormente por el titular; comprobar CI del SHA publicado antes de integrar. Los resultados de este documento son locales y no acreditan nueva CI remota ni despliegue.
