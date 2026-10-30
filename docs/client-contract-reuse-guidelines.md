# Matriz de reutilización — contratos e identidad (Web/MAUI/iOS)

## Objetivo
Definir una guía reutilizable para clientes de MiKompri (Web, MAUI, iOS) basada en contratos HTTP públicos y reglas de identidad TP11.

Estado al 30/10/2026: Web es el cliente implementado y prioritario (007/008/009); Android/MAUI permanece aplazado y iOS es una posibilidad futura. La reutilización es de contratos y semántica, no de componentes React. Ver [roadmap](../README.md#-estado-actual-del-proyecto).

## Reglas base obligatorias
- Identidad externa: claims canónicos `(tid, oid)`.
- Identidad de dominio: `UserId` interno de MiKompri.
- Scope delegado común backend (`VITE_API_SCOPE` en Web; equivalente en móviles).
- Sin secretos embebidos en cliente.

## Matriz de contratos por bounded context

| Bounded Context | Contrato HTTP base | Operaciones clave | Requisito de auth |
|---|---|---|---|
| Users | `/api/v1/users`, `/api/v1/groups` | perfil, grupos, membresías | Bearer JWT válido + audience común |
| ShoppingList | `/api/v1/purchaselists`, `/api/v1/shared-lists` | listas personales/compartidas, ítems, compras/gastos, balances y liquidación | Bearer JWT + autorización por identidad/membresía/rol |
| ProductCatalog | `/api/v1/catalog-products`, `/api/v1/markets`, `/api/v1/product-prices` | consulta/búsqueda, edición/desactivación, mercados, registro manual e historial de precios | Bearer JWT; gestión autenticada de catálogo global según contratos, sin roles de grupo inventados |

Referencias vigentes: [008 — API y permisos](../specs/008-web-ux-modernization/contracts/api-and-permissions.md), [009 — Compras y catálogo](../specs/009-shopping-experience-completion/contracts/api.md) y OpenAPI de cada API. El catálogo global no equivale a datos privados de un grupo; esta guía no amplía sus permisos.

## Semántica que deben conservar los clientes

- `UserId` interno canónico; solo Users resuelve `(tid, oid)`. 010 es análisis futuro, no una migración de identidad vigente.
- Compra y Gastos usan el mismo registro financiero. Preservar operación/idempotencia y versión de corrección según el contrato; no crear un gasto adicional por cada reintento o al desmarcar/re-marcar.
- `PurchasedOn` y `EffectiveDate` son fechas civiles `YYYY-MM-DD`. Enviar `X-Client-Utc-Offset-Minutes` actual (entero [-840,840], positivo al este de UTC) cuando representan el día local. El servidor usa su reloj; sin header conserva hoy UTC. Auditoría sigue UTC.
- Completitud de una lista no vacía mueve su presentación a historial sin llamar a close; desmarcar reactiva la presentación y conserva gastos. Closed/Archived mantiene sus restricciones.

## Mapeo de estados HTTP a UX (todos los clientes)

| HTTP | Significado | Estado UI recomendado |
|---|---|---|
| 400 | validación/negocio | ErrorState con mensaje accionable |
| 401 | sesión inválida/expirada | UnauthorizedState + reautenticación |
| 403 | autenticado sin permisos | ForbiddenState |
| 404 | recurso inexistente | Empty/NotFound según flujo |
| 409 | conflicto de negocio | ErrorState conservando contexto |
| 503 | indisponibilidad temporal | ServiceUnavailableState + retry |

## Checklist de portabilidad por cliente
1. Reutilizar contratos y DTOs públicos sin acoplar lógica interna backend.
2. Reutilizar política de errores HTTP -> estado UI.
3. Reutilizar estrategia de token refresh/reauth sin secretos locales.
4. Validar CORS (Web) o networking policy equivalente (móvil).
5. Verificar flujos críticos: auth, listas, settlement, catálogo.

## Notas por plataforma
- **Web**: usa MSAL + SPA; desarrollo con Vite y empaquetado nginx/Docker. `staticwebapp.config.json` prepara rutas para el destino Azure Static Web Apps, sin acreditar despliegue.
- **MAUI futuro**: usar OAuth/OIDC con almacenamiento seguro y registro de cliente propio, preservando TP11. Formalizar su spec antes de implementar; no reutilizar dependencias React.
