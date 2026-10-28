# Quickstart — 007-web-react-client

## Objetivo
Validar de extremo a extremo los flujos críticos del cliente Web full-stack sin implementar backend nuevo.

## Revisión visual local (sin tenant ni datos reales)

Referencia: [design.md](design.md). Ejecutar desde `MiKompri.Web` en PowerShell:

```powershell
npm.cmd ci
npm.cmd run lint
npm.cmd run typecheck
npx.cmd vitest run --maxWorkers=2
npm.cmd run build
$env:PLAYWRIGHT_CHANNEL='chrome'
npx.cmd playwright test tests/e2e/home tests/e2e/visual --workers=2
```

El canal Chrome requiere Chrome instalado. Omitir `PLAYWRIGHT_CHANNEL` si se utiliza Chromium instalado mediante Playwright. El webServer de Playwright inicia Vite en `127.0.0.1:5173`; esa dirección del harness no cambia los redirect URIs de Entra. Los datos y el contexto MSAL de `tests/visual/` son exclusivos de pruebas; no se crean cuentas ni se escribe en APIs reales.

Desde la raíz, validar la imagen sin publicarla:

```powershell
docker build -f MiKompri.Web/Dockerfile -t mikompri-web:visual-review .
```

### Última evidencia registrada — 2026-10-27

| Control | Resultado registrado |
| --- | --- |
| Lint, TypeScript, build, Docker y diff | Satisfactorios |
| Vitest completo | 81 passed, 0 failed, 0 skipped |
| Playwright completo | 11 passed, 0 failed, 7 skipped; sin reintentos |
| Anchos | 320, 390, 430, 768 y 1280 px |
| Vistas interiores | Perfil, grupos, detalle personal, catálogo/historial, diálogo compartido, reparto y login; 35 capturas |
| Estados controlados | Carga/vacío/401/403/503 en perfil, grupos, listas y catálogo a 390 px |

Informes `.playwright-mcp/visual-vitest.json` y `visual-playwright.json`, capturas `.playwright-mcp/app-{sección}-{ancho}.png` desde la raíz. Ver T123–T126 para comandos y alcance exactos. La revisión documental posterior no vuelve a ejecutar estos controles. Bundle >500 kB, conversión WOFF2 y auditoría ampliada de accesibilidad siguen pendientes.

Los siete E2E omitidos requieren entorno integrado: los escenarios siguientes conservan su función de validación real. El harness no acredita Entra/PKCE, permisos multiusuario, CRUD completo, lectores de pantalla, zoom/teclado virtual ni métricas piloto. No cerrar la 007 con este resultado local ni desplegar por haber construido la imagen.

## Prerrequisitos del entorno integrado
1. APIs backend de MiKompri disponibles (Users, ShoppingList, ProductCatalog).
2. Registro SPA en Microsoft Entra ID configurado para entorno local.
3. Variables `.env` del frontend definidas por ambiente (sin secretos).
4. CORS backend habilitado para el origen local del frontend.

## Escenarios de Validación E2E (MVP)

### Escenario A — Login, sesión y rutas protegidas (P1)
1. Iniciar SPA sin sesión.
2. Ejecutar login con Entra ID.
3. Verificar acceso a ruta protegida.
4. Recargar navegador y confirmar restauración de sesión.
5. Cerrar sesión y confirmar bloqueo de rutas protegidas.

**Resultado esperado**: flujo auth completo funcional con PKCE, sin secretos en cliente.

### Escenario B — Listas personales (P1)
1. Crear lista personal.
2. Agregar/editar/marcar/eliminar ítems.
3. Forzar respuesta 401 y validar redirección de reauth.
4. Forzar 409 y validar mensaje accionable.

**Resultado esperado**: operaciones CRUD personales respetan autorización por identidad.

### Escenario C — Listas compartidas, gastos y settlement (P1/P2)
1. Abrir lista compartida de grupo con membresía válida.
2. Registrar gasto y participantes.
3. Consultar balances y settlement.
4. Repetir con usuario sin membresía y validar 403.

**Resultado esperado**: reglas colaborativas visibles en UI con estados authorized/forbidden correctos.

### Escenario D — Consulta y alta de productos (P2)
1. Navegar a catálogo.
2. Consultar productos, mercados e historial de precios.
3. Validar estado empty cuando no hay historial.
4. Simular 503 y validar estado de indisponibilidad con reintento.
5. Crear producto con nombre y unidad; comprobar validación, conflicto 409, conservación de valores y bloqueo de duplicados pendientes.
6. Desde una lista seleccionada, entrar al catálogo, crear el producto y volver a la misma lista; comprobar que aparece en el selector sin agregar automáticamente un ítem.

**Resultado esperado**: consulta estable y alta de productos mediante el contrato existente; sin edición/desactivación ni gestión de mercados/precios.

## Validación de Calidad Transversal
- Responsive en mobile/tablet/desktop.
- Accesibilidad básica: navegación teclado, foco visible, nombres accesibles en controles clave.
- Estados UI obligatorios: loading, empty, error, 401, 403, 503.

## Integración continua pendiente de cierre
- Build/test frontend en CI dedicado.
- E2E Playwright en pipeline (smoke en PR y suite completa en main).

## Histórico de regresión backend (Phase 8)

Estos resultados no son una ejecución nueva de la revisión visual. El fallo ShoppingList de 27/58 se conserva como histórico; la corrección del 2026-10-25 registró después **64 passed, 0 failed, 0 skipped** en tasks y contrato sección 7. Reconciliar el gate global con la regresión pertinente antes de cerrar T093/T118; no presentar el fallo antiguo como diagnóstico actual.

### ShoppingList API (T093)
- Comando ejecutado:
  - `dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release`
- Resultado:
  - Falló.
  - Resumen: 58 tests, 31 ok, 27 con error.
  - Errores representativos: respuestas `401 Unauthorized` donde se esperaba `201 Created`; `JsonException` por respuestas vacías en tests que esperan payload JSON.

### Users API (T100)
- Comando ejecutado:
  - `dotnet test test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj --configuration Release`
- Resultado:
  - Exitoso.
  - Resumen: 26 tests, 26 ok, 0 errores.

### ProductCatalog API (T101)
- Comando ejecutado:
  - `dotnet test test/MiKompri.ProductCatalog.Api.Tests/MiKompri.ProductCatalog.Api.Tests.csproj --configuration Release`
- Resultado:
  - Exitoso.
  - Resumen: 5 tests, 5 ok, 0 errores.

## Validación cruzada frontend/backend (T094)
1. Levantar APIs (`Users`, `ShoppingList`, `ProductCatalog`) con configuración JWT/audience común TP11.
2. Configurar `MiKompri.Web/.env` desde `.env.example` para local o cloud.
3. Ejecutar frontend:
   - `npm ci`
   - `npm run build`
4. Ejecutar pruebas frontend relevantes:
   - `npx vitest run tests/unit`
   - `npx vitest run tests/component`
   - `npx vitest run tests/integration`
5. Verificar flujos cruzados:
   - Auth y perfil (`/profile`)
   - Listas personales/compartidas (`/personal-lists`, `/shared-lists`)
   - Reparto de gastos (`/shared-lists` → `Ver reparto de gastos`)
   - Catálogo: consulta y alta de productos (`/product-catalog`)
6. Confirmar estados UX obligatorios: loading, empty, error, 401, 403, 503.

## Referencias
- [Spec](./spec.md)
- [Diseño vigente](./design.md)
- [Plan](./plan.md)
- [Data Model](./data-model.md)
- [Contract](./contracts/frontend-backend-integration.md)

## Estado de UX privada — 2026-10-27

El recorrido vigente está en design.md, «Recorrido privado por lista», con trazabilidad FR-034 y T128–T132. Índices sin formularios permanentes, creación breve, incorporación por diálogo y detalle privado; compartidas conserva Compra/Gastos en `?tab=shopping|expenses`. Regreso desde catálogo acepta rutas internas personales/compartidas y, en compartidas, conserva la pestaña permitida. Las comprobaciones usan el mismo harness de tests, separado de producción.

Evidencia de esta entrega en `.playwright-mcp/private-ux-vitest-final.json` y `private-ux-playwright-final.json`; comandos: `npx.cmd vitest run --maxWorkers=2 --reporter=json --outputFile=../.playwright-mcp/private-ux-vitest-final.json` y `$env:PLAYWRIGHT_CHANNEL='chrome'; npx.cmd playwright test --workers=2 --reporter=json`. Los resultados definitivos se registran en T131; no reutilizar los recuentos de T126 como si acreditaran este nuevo flujo.

Límite concreto: marcado personal mediante endpoint genérico existente, restringido por selección de /mine en UI; esto no acredita autorización servidor. No se ofrece marcado compartido hasta resolver el prerrequisito T132. No hay desmarcado ni pagos simulados y no se ha ampliado el backend. La validación de tenant real, roles multiusuario y los siete E2E integrados continúa pendiente.

### Resultado definitivo del recorrido privado

Vitest completo: **96 passed, 0 failed, 0 skipped**. Playwright completo: **16 passed, 0 failed, 7 skipped, 0 flaky**, sin reintentos. Lint, TypeScript, build y Docker local satisfactorios; imagen `mikompri-web:private-ux-review`, sin ejecución o publicación. Diez capturas nuevas `app-private-shopping-{ancho}.png` / `app-private-shared-{ancho}.png` a 320/390/430/768/1280 px en `.playwright-mcp/`, además de las capturas por sección. T128–T131 explican alcance, adaptador non-modal exclusivo de JSDOM y verificación modal real en Chrome; T132 y los gates integrados siguen abiertos.

### Regresión de marcado tras incorporación — 2026-10-27

Se corrige la búsqueda por ProductId en `PurchaseList.MarkItemAsPurchased`: el contrato siempre recibe ItemId. Nuevas regresiones de identidad y flujo HTTP completo con persistencia. Suites ShoppingList Release: dominio 38/38, aplicación 33/33, API 65/65, sin omitidas. TRX: `.playwright-mcp/test-results/mark-item-regression.trx`. Ver T133 para reproducción previa y límites; no cierra la autorización T132 ni altera la configuración Entra.
