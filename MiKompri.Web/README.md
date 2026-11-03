# MiKompri.Web

Cliente Web SPA de MiKompri (MVP-4), construido con React + TypeScript + Vite, Fluent UI v9, MSAL y TanStack Query. Es el cliente prioritario actual; Android/MAUI está aplazado.

## Estado al 01/11/2026

007 aporta el cliente full-stack; 008 moderniza UX y autorización; 009 completa preparación, compra y consulta. Las tres tienen implementación en `main` (008/009 integradas por PR #56/#57), con aceptación global todavía pendiente. En 009, T009 está cerrada con evidencia técnica y T010/T011 siguen abiertas. [Estado y roadmap](../README.md#-estado-actual-del-proyecto), [tareas 009](../specs/009-shopping-experience-completion/tasks.md) y [evidencia](../specs/009-shopping-experience-completion/evidence/review-corrections.md).

010-A beta está integrada en main por PR59: Google mediante External ID, Microsoft legacy conservado, perfil propio, D1 e aislamiento de sesión; [aceptación beta y riesgos diferidos](../specs/010-account-management-social-auth/evidence/validation.md). 011 está implementada en su rama para revisión; 010-B continúa diferida. La presentación actual ofrece solo Google y oculta Microsoft temporalmente por decisión expresa ([ADR-006](../docs/adr/ADR-006-temporary-google-only-web-login.md)). CIAM permanece deshabilitado por defecto en configuración fuente: hay que configurar sus variables públicas para ofrecer acceso; no existe fallback visible a Microsoft. `006-android-maui-client` es un identificador reservado, sin carpeta de spec ni proyecto MAUI; Web primero no cambia la tecnología Android prevista.

## Requisitos

- Node.js 24 LTS (entorno validado y CI)
- npm 11

## Comandos principales

```powershell
npm ci
npm run dev
npm run lint
npm run build
npm run typecheck
npm run test
npm run test:unit
npm run test:component
npm run test:integration
npm run test:e2e
```

## Estructura actual

```text
src/
├── app/          # Providers, router, layout y entrada
├── auth/         # MSAL, guard y recuperación de sesión/borradores
├── features/     # Perfil, grupos, listas, catálogo y gastos/reparto
├── shared/       # API, configuración, fechas y UI común
├── assets/       # Fuentes y recursos
└── main.tsx
tests/
├── unit/
├── component/
├── integration/
├── e2e/          # Recorridos controlados y suite real; configs Playwright separadas
├── visual/
└── setup/
```

## Variables de entorno

Desde `MiKompri.Web`, copiar `.env.example` a `.env.local` y configurar las URLs públicas de Users, ShoppingList y ProductCatalog, el registro SPA Entra, tenant/authority, redirect y logout URIs, y `VITE_API_SCOPE` del recurso backend común. Reiniciar Vite después de editarlo. `VITE_*` es configuración pública incorporada durante el build, nunca secretos de cliente.

Para la entrada visible Google se requieren además `VITE_CIAM_ENABLED=true` y `VITE_CIAM_CLIENT_ID`, `VITE_CIAM_TENANT_ID`, `VITE_CIAM_AUTHORITY`, `VITE_CIAM_API_SCOPE` válidos. No habilitarla con valores ficticios ni crear recursos automáticamente: usar un entorno autorizado. Sin esa configuración el login indica indisponibilidad, sin mostrar Microsoft.

Levantar antes las APIs y PostgreSQL con el Compose de la raíz; ese Compose no incluye un servicio Web. `npm run dev` sirve la SPA en `http://localhost:5173`. Registrar `/auth/callback` para ese origen en Entra. Usuarios autenticados acceden a `/lists`; `/profile`, `/groups`, `/personal-lists`, `/shared-lists` y `/product-catalog` son rutas protegidas. [Guía CORS](../docs/web-client-cors.md) y [contratos/identidad](../docs/client-contract-reuse-guidelines.md).

## Modernización 008

La entrada autenticada es `/lists`, con filtros personales/compartidas; las rutas anteriores siguen disponibles. Compra reversible, edición, alta de producto dentro de la lista, personas/roles e historial de gastos comparten la navegación nueva. El backend actualizado es necesario: [contratos](../specs/008-web-ux-modernization/contracts/api-and-permissions.md).

## Compra completa 009

Mis listas, índice personal y listas de grupo ofrecen Activas/Historial y orden conservado en URL. El total estimado multiplica precio unitario por cantidad y avisa de precios desconocidos; una lista no vacía completada pasa al historial sin cerrarse. Desmarcar la devuelve a activas y conserva sus gastos.

Añadir productos muestra resultados seleccionables y creación contextual con nombre precompletado. Catálogo permite buscar/editar productos, gestionar mercados y registrar precios manuales para consultar su historial. Esos precios no se crean automáticamente al registrar compras.

Después de marcar se puede omitir la captura o registrar datos reales (importe total obligatorio al guardar; mercado y fecha opcionales; pagador/comprador/participantes según contexto). Gastos permanece accesible por separado, también en personales; se utiliza el mismo registro y una operación persistente para reintentar sin duplicación. Requiere backend/migración 009; contratos shared legacy continúan compatibles. Las fechas de compra/precio son civiles: Web usa día local y envía `X-Client-Utc-Offset-Minutes`; auditoría sigue UTC. [Contratos](../specs/009-shopping-experience-completion/contracts/api.md), [validación](../specs/009-shopping-experience-completion/evidence/validation.md) y [guía QA](../specs/009-shopping-experience-completion/quickstart.md).

`npm test` ejecuta las pruebas locales; `npm run test:e2e` usa fixtures controlados y no certifica Entra. Para tres motores instalar `npx playwright install chromium firefox webkit` y definir `PLAYWRIGHT_ALL_BROWSERS=1`. Se ejecutan como máximo dos workers.

`npm run test:e2e:integrated` exige un entorno real y falla si faltan URL, dos tokens y dos archivos de sesión. Variables: `MIKOMPRI_INTEGRATED_URL`, `MIKOMPRI_OWNER_TOKEN`, `MIKOMPRI_MEMBER_TOKEN`, `MIKOMPRI_OWNER_STORAGE`, `MIKOMPRI_MEMBER_STORAGE`; URLs API opcionales `MIKOMPRI_SHOPPING_API`, `MIKOMPRI_USERS_API`, `MIKOMPRI_CATALOG_API`. Los archivos privados contienen el storageState de Playwright y un objeto `sessionStorage` adicional exportado por la persona después de autenticarse con Entra, porque MSAL usa almacenamiento por pestaña. No guardar estos archivos en Git ni compartir tokens en el chat. Usar un tenant y datos desechables; los grupos creados por las pruebas quedan identificados como QA porque no existe API de borrado de grupos.

Docker incluye fallback SPA, caché de assets, compresión y bloqueo de `/tests/`. El harness solo se usa en desarrollo/pruebas. [Plan P0–P3, evidencia y pendientes](../specs/008-web-ux-modernization/quickstart.md).

El Dockerfile compila la SPA y la sirve con nginx; `.env.local` está excluido del contexto Docker. No asumir que el login queda configurado al construir la imagen ni que cambiar variables del contenedor nginx reconfigura el bundle: la configuración pública debe estar disponible durante su compilación. Azure Static Web Apps es un destino previsto, sin despliegue productivo acreditado por esta documentación.

Las fuentes web derivadas se denominan MiKompri Sans; conservan el diseño original de Source Sans 3, la atribución y la licencia OFL. Las variantes latinas pesan aproximadamente 76 kB en total; otros alfabetos usan las variantes completas por cobertura Unicode. El script opcional `python scripts/build-fonts.py` requiere `fonttools[woff]` solo para regenerarlas. Las licencias también se distribuyen en `public/fonts/`.

## CI y aceptación con Entra

`CI - MiKompri Web` ejecuta en las ramas configuradas y PRs hacia main/develop las pruebas locales, los recorridos controlados de tres navegadores y la compilación Docker. No requiere cuentas Entra. `npm run test:ci-config` verifica que el preflight rechaza configuración incompleta; sus fixtures no autentican usuarios.

`Acceptance - MiKompri Web with Entra` está separado en `.github/workflows/acceptance-mikompri-web-entra.yml` y se inicia manualmente con `workflow_dispatch`. Cuando se invoca exige todos los recursos y falla si faltan; no convierte una prueba omitida en una aceptación. El cierre de 008 exige una ejecución real satisfactoria sobre el commit de entrega, además de la CI normal.

Configurar en **Settings → Secrets and variables → Actions**:

| Tipo | Nombres | Contenido |
| --- | --- | --- |
| Variables | `MIKOMPRI_INTEGRATED_URL` | URL web de QA; también puede indicarse en el input `target_url` al iniciar el workflow |
| Variables | `MIKOMPRI_SHOPPING_API`, `MIKOMPRI_USERS_API`, `MIKOMPRI_CATALOG_API` | URLs base de las tres APIs accesibles desde el runner |
| Secrets | `MIKOMPRI_OWNER_TOKEN`, `MIKOMPRI_MEMBER_TOKEN` | Tokens vigentes de dos cuentas de prueba |
| Secrets | `MIKOMPRI_OWNER_STATE`, `MIKOMPRI_MEMBER_STATE` | JSON de las sesiones exportadas después del login Entra real, incluyendo `sessionStorage` |

`OWNER_STORAGE` y `MEMBER_STORAGE` son archivos temporales generados automáticamente a partir de los secretos `*_STATE`, con permisos restringidos y eliminación al terminar. El preflight se ejecuta antes de instalar dependencias y navegadores. Comprueba presencia de recursos; la autenticación efectiva se verifica en los tests posteriores.

`http://localhost:5173` del equipo de desarrollo no es accesible desde un runner hospedado de GitHub. Para la ejecución hospedada preparar QA accesible; para validar el entorno local usar `npm run test:e2e:integrated` con las sesiones locales y las variables anteriores (los defaults de API local se conservan).

El workflow de aceptación ya está incorporado a `main`; se puede iniciar desde Actions y seleccionar la rama a validar, con los recursos QA anteriores configurados. También se puede ejecutar la suite localmente. Su presencia no configura credenciales ni acredita la aceptación pendiente.

## Acceso y límites actuales de 010-A

«Continuar con Google» es la única opción visible y abre el acceso alojado de External ID con `prompt=login`; en esa pantalla hay que pulsar «Iniciar sesión con Google», en lugar de introducir el correo en el acceso email OTP. Microsoft queda oculto temporalmente; conserva `select_account`, trust y sesiones legacy internamente, sin migrar datos. Un usuario Microsoft sin sesión activa no tiene botón de entrada; cambiar a Google no recupera su UserId por coincidencia de email. [Decisión, límites y reversión](../docs/adr/ADR-006-temporary-google-only-web-login.md). No se usa el hint Google rechazado en QA y `prompt=login` no acredita G3 ni reautenticación Google reciente.

Si la cuenta no se prepara después del acceso, comprobar primero Users y las demás APIs. La pantalla conserva el error y ofrece «Reintentar acceso» con la sesión existente, sin bucle automático ni nuevo código obligatorio. Para el **entorno QA local ya preparado**, recuperar solo sus contenedores existentes:

```powershell
docker start mikompri-010a-postgres
docker start mikompri-010a-users mikompri-010a-shopping mikompri-010a-catalog
# Comprobar /health en localhost:28082, :28080 y :28083: los tres deben responder 200.
# Desde MiKompri.Web, con .env.010a.local privado ya configurado:
npm run dev:010a -- --strictPort
```

No usar este procedimiento para crear o migrar otro entorno. No arrancar simultáneamente servidores distintos para localhost/127.0.0.1 en5173: el callback registrado debe volver al mismo origen y configuración de la Web. Si ya hay un Vite válido en5173, conservarlo; `--strictPort` evita continuar inadvertidamente en otro puerto. [Recuperación verificada](../specs/011-web-iconography/evidence/authentication-recovery-2026-11-01.md).

El selector QA aún ofrece email OTP heredado del spike, pero la Web no lo anuncia: T056 sigue opcional y pendiente. No retirarlo sin evaluar la evidencia y las cuentas QA existentes. El perfil permite personalizar nombre, username, nombres/apellidos, idioma y orden inicial; los datos propios y los borrados explícitos prevalecen en accesos posteriores. Un `unknown` previo no se corrige mediante Graph.

Microsoft→External ID y rollback real quedan DEFERRED MICROSOFT MIGRATION por decisión beta31/10; el ensayo conserva INCONCLUSIVE por colisión CIAM. Proveedor experimental fuera del flujo, sin nuevo cutover. Lector/zoom completos y matriz manual exhaustiva multi-tab quedan DEFERRED NON-BLOCKING QA; los básicos móviles/teclado y ambas direcciones reales sí están acreditados. [Resultados del candidato](../specs/010-account-management-social-auth/evidence/validation.md), [proveedores reales](../specs/010-account-management-social-auth/evidence/real-provider-acceptance.md) y [migración](../specs/010-account-management-social-auth/evidence/migration-rehearsal.md). Estos diferidos no bloquean la beta010-A Google sin Microsoft+legacy operativo. CI final success y revisión de PR son gates de publicación; no reabren010-B ni cambian007/008/009.

Para continuar010-A en este equipo se deja activa la configuración QA que incluye ambos proveedores y sus tres APIs compatibles. Arrancar con `npm run dev:010a`: Vite carga `.env.010a.local` ignorado explícitamente, aunque `.env.local` corresponda a otro entorno. No restaurar automáticamente la configuración antigua al terminar una comprobación: ocultaba Google y conectaba el bootstrap de la Web a la API anterior, cuyo POST de sync devuelve401 con el mismo token Microsoft que permite GET200. Los tres servicios010-A usan28082/28080/28083 y datos QA separados; no muestran las listas de la base anterior.

Los archivos de configuración local siguen ignorados; no contienen secretos de cliente. Este comando requiere el archivo QA ya preparado y los servicios compatibles en ejecución; no crea tenants, registros, credenciales ni bases. No usar esta configuración como producción. Revisar inventario/retirada QA antes del06/11/2026.
