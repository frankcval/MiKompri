# Quickstart — 007-web-react-client

## Objetivo
Validar de extremo a extremo los flujos críticos del cliente Web full-stack sin implementar backend nuevo.

## Prerrequisitos
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

### Escenario D — ProductCatalog read-only (P2)
1. Navegar a catálogo.
2. Consultar productos, mercados e historial de precios.
3. Validar estado empty cuando no hay historial.
4. Simular 503 y validar estado de indisponibilidad con reintento.

**Resultado esperado**: consumo read-only estable y sin operaciones de escritura.

## Validación de Calidad Transversal
- Responsive en mobile/tablet/desktop.
- Accesibilidad básica: navegación teclado, foco visible, nombres accesibles en controles clave.
- Estados UI obligatorios: loading, empty, error, 401, 403, 503.

## Comandos de referencia (futura implementación)
- Build/test frontend en CI dedicado.
- E2E Playwright en pipeline (smoke en PR y suite completa en main).

## Resultados de regresión backend (Phase 8)

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
   - Settlement (`/shared-lists` -> `Ver settlement`)
   - ProductCatalog read-only (`/product-catalog`)
6. Confirmar estados UX obligatorios: loading, empty, error, 401, 403, 503.

## Referencias
- [Spec](./spec.md)
- [Plan](./plan.md)
- [Data Model](./data-model.md)
- [Contract](./contracts/frontend-backend-integration.md)
