# Auditoría de estado actual — 013 Shared Lists UX Completion

**Fecha**: 2026-11-03  
**Base auditada**: `main` remoto, SHA `f0a1643`  
**Alcance de esta auditoría**: contraste código real Web + API + pruebas, sin implementar cambios productivos.

## 1) Rutas, navegación y entradas al recurso

### Rutas reales
- `GET /shared-lists` y `GET /shared-lists/:listId` en `MiKompri.Web/src/app/router/routes.tsx`.
- Entrada desde Grupos: `GroupDetailPage` enlaza a `/shared-lists?groupId={groupId}`.
- Entrada desde Mis listas: `ListsPage` enlaza a `/shared-lists/{listId}?groupId={groupId}` para listas compartidas.

### Hallazgos
- Ambos caminos llegan al mismo recurso (positivo).
- No existe breadcrumb formal (solo enlace "Volver a listas compartidas" en detalle).
- El estado de tab en detalle se gobierna por query param `tab=shopping|expenses|people` (positivo para deep-link y back/forward).

## 2) SharedListsPage (listado + creación)

**Archivo**: `src/features/shared-lists/pages/SharedListsPage.tsx`.

### Comportamiento actual verificado
- Carga grupos del usuario y exige selección de grupo para consultar listas.
- Crea lista compartida con `CreateListDialog` enviando `{ name, groupId }` a `POST /api/v1/shared-lists`.
- Maneja estados: loading, error, sin grupos, grupo inválido en URL, vacío de listas, vista sin resultados (active/history), error 403 y error general de listas.
- Lista visible reutiliza `ListOverview` + `visibleLists` (coherencia de estimación/pendientes con 009/011).

### Hallazgos de UX
- Correcto no pedir `GroupId` manual.
- El bloque de selección + listado funciona, pero el contexto colaborativo del grupo no se sintetiza en una cabecera compacta por lista (depende de componentes genéricos).
- Existe enlace secundario "Ver reparto de gastos de {lista}" por cada tarjeta (útil, pero visualmente duplicado con la entrada principal).

## 3) SharedListDetailPage (Compra/Gastos/Personas)

**Archivo**: `src/features/shared-lists/pages/SharedListDetailPage.tsx`.

### Comportamiento actual verificado
- Carga detalle de lista, grupos del usuario, miembros del grupo y perfil actual.
- Determina rol efectivo y deriva `manager = owner|admin`.
- Tabs por query param: `shopping` (default), `expenses`, `people`.
- Compra:
  - usa `PurchaseItems`/`ShoppingListView`;
  - permite marcar/desmarcar comprado vía `PATCH /api/v1/purchaselists/{listId}/items/{itemId}/purchase-state`;
  - usa `AddProductsDialog` para alta contextual de ítems;
  - abre captura opcional de compra (`PurchaseCaptureDialog`);
  - edición/eliminación de ítem condicionada por manager o autor del ítem.
- Gastos:
  - reutiliza `SettlementPage` (lazy);
  - muestra registro de gasto, balance, propuesta de pagos e historial de gastos.
- Personas:
  - reutiliza `GroupPeople` del contexto grupos.
- Gestión de lista para manager en activa: renombrar, cerrar, eliminar.

### Hallazgos de UX
- El detalle concentra mucha lógica y acciones en un solo componente (difícil mantener consistencia de UX fina).
- El contexto de navegación es mínimo (enlace de vuelta), sin breadcrumb completo del flujo.
- La pestaña Personas es correcta en alcance (solo datos existentes), pero básica para contexto de lista compartida.
- El flujo Compra/Gastos es funcional, pero la jerarquía de información puede optimizarse para "qué lista / qué grupo / qué estado / qué progreso" en primer vistazo.

## 4) Paridad personal vs compartida

### Positivos
- Reutilización real de piezas de compra (`PurchaseItems`, `ShoppingListView`, `ListOverview`).
- Reglas de historial/activa comparten la misma base de presentación (`listPresentation.ts`).
- Estimación parcial y productos sin precio ya están resueltos y visibles.

### Brechas observables
- Cobertura de pruebas frontend en shared-lists todavía menos densa que personal para todos los edge cases de UX (sobre todo matriz detallada por rol en UI y escenarios responsive por estado).
- Falta una formalización propia de UX shared-lists equivalente a la madurez documental de 011.

## 5) Permisos y backend (hechos verificables)

### Fuente de verdad
- `PurchaseAccessPolicy` + handlers de shared lists/settlement en ShoppingList.Application.
- `UsersGroupAuthorizationAdapter` para membresía activa y rol (Owner/Admin/Member).

### Reglas efectivas observadas
- Lectura de lista compartida: miembro activo.
- Crear lista compartida: miembro activo del grupo destino.
- Gestionar lista (renombrar/cerrar/eliminar lista): Owner/Admin.
- Añadir ítems / marcar comprado / registrar gasto: miembro activo.
- Editar/eliminar ítems compartidos:
  - Owner/Admin: permitido.
  - Member: solo ítems añadidos por sí mismo.
- Actualizar/eliminar gastos: Owner/Admin.
- Register expense con Member: restringe pagador/comprador a sí mismo.
- Lista cerrada: bloqueo de escrituras en policy (`ResourceConflictException`).

### Nota
- Seguridad no depende solo de UI; backend aplica autorización por policy y handlers (positivo).

## 6) Estados, responsive y accesibilidad

### Evidencia de tests y código
- Existen pruebas component/e2e específicas (`SharedGroupSelection`, `SharedListDetail`, `shared-lists-authorization`, flujos modernization/settlement).
- Navegación persistente y tabs ya validan parte de deep-link/reload/back-forward.
- UI global mantiene objetivos 44x44 y patrones de foco heredados de 011 (`AppSurface.module.css`, layout principal).

### Brecha para 013
- Falta un paquete de aceptación específico de shared-lists UX con matriz completa:
  - listado (vacío/1/múltiples/multi-grupo/parcial);
  - creación por grupo;
  - compra completa con 100 ítems y precios desconocidos;
  - gastos por permisos y errores;
  - personas por rol sin funciones falsas;
  - navegación completa Grupo→Lista y Mis listas→Lista;
  - responsive 320/390/430/768/1280 para todos los estados.

## 7) Relación con specs anteriores

- 005 define reglas de listas compartidas y settlement (base de negocio vigente).
- 007/008/009 aportan cliente web, modernización UX y cierre de experiencia de compra.
- 010 regula identidad/sesión; no se altera en 013.
- 011 aporta lenguaje visual/iconografía; 013 debe respetarlo en shared lists.
- 012 (invitaciones) es iniciativa separada: explícitamente fuera de alcance en 013.

## 8) Conclusión de auditoría

El sistema actual de listas compartidas es funcional y autorizado en backend, con base visual coherente post-011. El hueco principal es de **completitud UX formal y cobertura de aceptación específica** para shared lists: navegación contextual, jerarquía colaborativa, consistencia de estados y matriz de escenarios owner/admin/member con evidencia homogénea.
