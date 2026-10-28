# Matriz de reutilización — contratos e identidad (Web/MAUI/iOS)

## Objetivo
Definir una guía reutilizable para clientes de MiKompri (Web, MAUI, iOS) basada en contratos HTTP públicos y reglas de identidad TP11.

## Reglas base obligatorias
- Identidad externa: claims canónicos `(tid, oid)`.
- Identidad de dominio: `UserId` interno de MiKompri.
- Scope delegado común backend (`VITE_API_SCOPE` en Web; equivalente en móviles).
- Sin secretos embebidos en cliente.

## Matriz de contratos por bounded context

| Bounded Context | Contrato HTTP base | Operaciones clave | Requisito de auth |
|---|---|---|---|
| Users | `/api/v1/users`, `/api/v1/groups` | perfil, grupos, membresías | Bearer JWT válido + audience común |
| ShoppingList | `/api/v1/purchaselists`, `/api/v1/shared-lists` | listas personales/compartidas, ítems, settlement | Bearer JWT + autorización por identidad/membresía |
| ProductCatalog | `/api/v1/catalog-products`, `/api/v1/markets` | lectura de catálogo/mercados/historial | Bearer JWT (read-only en MVP) |

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
- **Web**: usa MSAL + SPA + `staticwebapp.config.json`.
- **MAUI/iOS**: usar equivalente OAuth/OIDC con storage seguro del token y mismas reglas TP11.
