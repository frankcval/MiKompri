# MiKompri.Web

Cliente Web SPA de MiKompri (MVP-4) construido con React + TypeScript + Vite.

## Requisitos

- Node.js 24 LTS (entorno validado y CI)
- npm 11

## Comandos principales

```powershell
npm install
npm run dev
npm run build
npm run typecheck
npm run test
npm run test:unit
npm run test:component
npm run test:integration
npm run test:e2e
```

## Estructura base

```text
src/
├── app/
│   └── App.tsx
└── main.tsx
tests/
└── setup/
    └── vitest.setup.ts
```

## Variables de entorno

Usar `.env.example` como plantilla para variables locales/cloud del frontend.

## Modernización 008

La entrada autenticada es `/lists`, con filtros personales/compartidas; las rutas anteriores siguen disponibles. Compra reversible, edición, alta de producto dentro de la lista, personas/roles e historial de gastos comparten la navegación nueva. El backend actualizado es necesario: [contratos](../specs/008-web-ux-modernization/contracts/api-and-permissions.md).

`npm test` ejecuta las pruebas locales; `npm run test:e2e` usa fixtures controlados y no certifica Entra. Para tres motores instalar `npx playwright install chromium firefox webkit` y definir `PLAYWRIGHT_ALL_BROWSERS=1`. Se ejecutan como máximo dos workers.

`npm run test:e2e:integrated` exige un entorno real y falla si faltan URL, dos tokens y dos archivos de sesión. Variables: `MIKOMPRI_INTEGRATED_URL`, `MIKOMPRI_OWNER_TOKEN`, `MIKOMPRI_MEMBER_TOKEN`, `MIKOMPRI_OWNER_STORAGE`, `MIKOMPRI_MEMBER_STORAGE`; URLs API opcionales `MIKOMPRI_SHOPPING_API`, `MIKOMPRI_USERS_API`, `MIKOMPRI_CATALOG_API`. Los archivos privados contienen el storageState de Playwright y un objeto `sessionStorage` adicional exportado por la persona después de autenticarse con Entra, porque MSAL usa almacenamiento por pestaña. No guardar estos archivos en Git ni compartir tokens en el chat. Usar un tenant y datos desechables; los grupos creados por las pruebas quedan identificados como QA porque no existe API de borrado de grupos.

Docker incluye fallback SPA, caché de assets, compresión y bloqueo de `/tests/`. El harness solo se usa en desarrollo/pruebas. [Plan P0–P3, evidencia y pendientes](../specs/008-web-ux-modernization/quickstart.md).

Las fuentes web derivadas se denominan MiKompri Sans; conservan el diseño original de Source Sans 3, la atribución y la licencia OFL. Las variantes latinas pesan aproximadamente 76 kB en total; otros alfabetos usan las variantes completas por cobertura Unicode. El script opcional `python scripts/build-fonts.py` requiere `fonttools[woff]` solo para regenerarlas. Las licencias también se distribuyen en `public/fonts/`.

## CI y aceptación con Entra

`CI - MiKompri Web` ejecuta en cada push/PR las pruebas locales, los recorridos controlados de tres navegadores y la compilación Docker. No requiere cuentas Entra. `npm run test:ci-config` verifica que el preflight rechaza configuración incompleta; sus fixtures no autentican usuarios.

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

[GitHub requiere que el workflow manual exista en la rama predeterminada](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow). El archivo nuevo debe incorporarse a `main` para que aparezca **Run workflow** en Actions; después se puede seleccionar la rama a validar. Hasta entonces la misma suite se puede ejecutar localmente. Este cambio no configura credenciales ni acredita la aceptación pendiente.
