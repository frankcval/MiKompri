# MiKompri.Web

Cliente Web SPA de MiKompri (MVP-4), construido con React + TypeScript + Vite, Fluent UI v9, MSAL y TanStack Query. Es el cliente prioritario actual; Android/MAUI está aplazado.

## Estado al 30/10/2026

007 aporta el cliente full-stack; 008 moderniza UX y autorización; 009 completa preparación, compra y consulta. Las tres tienen implementación en `main` (008/009 integradas por PR #56/#57), con aceptación global todavía pendiente. En 009, T009 está cerrada con evidencia técnica y T010/T011 siguen abiertas. [Estado y roadmap](../README.md#-estado-actual-del-proyecto), [tareas 009](../specs/009-shopping-experience-completion/tasks.md) y [evidencia](../specs/009-shopping-experience-completion/evidence/review-corrections.md).

010-A tiene código implementado en su rama: Google mediante External ID, Microsoft legacy conservado, perfil propio, D1 e aislamiento de sesión; [aceptación pendiente](../specs/010-account-management-social-auth/evidence/validation.md). CIAM permanece deshabilitado por defecto en configuración fuente. 010-B y 011 siguen futuros. `006-android-maui-client` es un identificador reservado, sin carpeta de spec ni proyecto MAUI; Web primero no cambia la tecnología Android prevista.

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

«Continuar con Google» abre el acceso alojado de External ID con `prompt=login`; en esa pantalla hay que pulsar «Iniciar sesión con Google». Microsoft conserva `select_account` y su configuración legacy. No se usa el hint Google rechazado en QA y `prompt=login` no acredita G3 ni reautenticación Google reciente.

El selector QA aún ofrece email OTP heredado del spike, pero la Web no lo anuncia: T056 sigue opcional y pendiente. No retirarlo sin evaluar la evidencia y las cuentas QA existentes. El perfil permite personalizar nombre, username, nombres/apellidos, idioma y orden inicial; los datos propios y los borrados explícitos prevalecen en accesos posteriores. Un `unknown` previo no se corrige mediante Graph.

La migración Microsoft federada real y su rollback siguen sin acreditarse por colisión de cuenta en CIAM. El proveedor experimental está retirado del flujo y no se ejecutó cutover. La aceptación manual de lector/zoom también permanece pendiente. [Resultados del candidato](../specs/010-account-management-social-auth/evidence/validation.md), [proveedores reales](../specs/010-account-management-social-auth/evidence/real-provider-acceptance.md) y [migración](../specs/010-account-management-social-auth/evidence/migration-rehearsal.md). Estas limitaciones impiden declarar el cierre global de010-A; no reabren010-B ni cambian007/008/009.

Al terminar este ensayo se restauró `.env.local` original byte por byte. La configuración QA permanece solo en `.env.010a.local` ignorado; npm run dev -- --mode010a permite retomarla voluntariamente mientras los recursos QA sigan vigentes. No usar esos puertos/tenant como configuración de producción ni versionar el archivo. Revisar inventario/retirada QA antes del06/11/2026.
