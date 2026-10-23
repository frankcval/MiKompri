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

## Referencias
- [Spec](./spec.md)
- [Plan](./plan.md)
- [Data Model](./data-model.md)
- [Contract](./contracts/frontend-backend-integration.md)