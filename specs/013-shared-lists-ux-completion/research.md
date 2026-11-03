# Research — 013 Shared Lists UX Completion

## Decisión R1 — Mantener un único recurso de detalle compartido

**Decisión**: conservar `/shared-lists/:listId` como recurso único para ambas entradas (`/groups/:groupId` y `/lists`).  
**Motivo**: evita estados paralelos y reduce divergencia de permisos, tabs y caché.  
**Implicación**: la diferencia entre recorridos se resuelve con contexto de navegación (query/volver/breadcrumb), no con rutas duplicadas.

## Decisión R2 — No introducir invitaciones en 013

**Decisión**: 013 no incluye invitar, aceptar/rechazar, links/email ni notificaciones de invitación.  
**Motivo**: separación explícita con `012-group-invitations-collaboration` y reducción de riesgo de scope creep.  
**Implicación**: personas se limita a visualización de miembros y roles existentes.

## Decisión R3 — Reutilizar componentes de compra existentes

**Decisión**: priorizar reutilización de `PurchaseItems`/`ShoppingListView` y patrones de 009/011 para compra compartida.  
**Motivo**: mantener paridad funcional y accesible entre lista personal y compartida.  
**Implicación**: diferencias solo por permisos y contexto colaborativo, no por lógica de compra alternativa.

## Decisión R4 — Matriz de permisos basada en backend real

**Decisión**: la matriz Owner/Admin/Member se deriva de `PurchaseAccessPolicy` y handlers de shared/settlement.  
**Motivo**: evitar supuestos de UI y asegurar trazabilidad contractual.  
**Implicación**: frontend debe representar estados permitidos/no permitidos y manejar 403 sin filtrar reglas propias inventadas.

## Decisión R5 — Gastos: separar semánticamente estimación vs gasto real

**Decisión**: reforzar en UX la diferencia entre:
- estimación de productos (vista compra/listado),
- gasto real registrado (tab gastos/historial).

**Motivo**: evitar interpretaciones erróneas del total en colaboración.  
**Implicación**: copy, headings y estados deben explicitar ambos conceptos sin mezclar cálculos.

## Decisión R6 — Progressive disclosure en detalle compartido

**Decisión**: mantener contenido principal legible y abrir creación/edición/corrección con diálogo o acciones bajo demanda.  
**Motivo**: reducir saturación, especialmente en 320–430px.  
**Implicación**: no mostrar formularios extensos simultáneos si no son necesarios.

## Decisión R7 — Sin cambios backend por defecto

**Decisión**: arrancar 013 sin exigir endpoint nuevo.  
**Motivo**: contratos actuales ya cubren listado, detalle, ítems, purchase state, gastos, settlement y miembros.  
**Implicación**: solo proponer cambio de contrato si emerge bloqueo UX demostrable en implementación.

## Decisión R8 — Evidencia primero, implementación después

**Decisión**: formalizar 013 con evidencia de estado actual, requisitos, aceptación, plan y tareas antes de tocar código productivo.  
**Motivo**: alinear alcance y evitar regresiones sobre features ya integradas.  
**Implicación**: esta entrega no implementa producción ni modifica reglas de negocio.
