# Feature Specification: 013 — Shared Lists UX Completion

**Feature Branch**: `013-shared-lists-ux-completion`  
**Created**: 2026-11-03  
**Status**: Draft (formalización)  
**Base auditada**: `main` SHA `f0a1643`  
**Input**: completar y corregir la experiencia Web de listas compartidas sin mezclar invitaciones (012).

## Contexto y autoridad

013 consolida UX de listas compartidas sobre contratos ya existentes de ShoppingList + Users, con lenguaje visual heredado de 011 y reglas de compra/gastos de 005/009.

Trazabilidad obligatoria:
- 005 Shared Lists Settlement (reglas de negocio y permisos colaborativos).
- 007 Web React Client (base SPA y sesión).
- 008 Web UX Modernization (principios de flujo y resiliencia).
- 009 Shopping Experience Completion (activa/historial, compra reversible, gastos).
- 011 Web Iconography (lenguaje visual y accesibilidad operativa).

Dependencia separada, fuera de alcance:
- `012-group-invitations-collaboration` (onboarding colaborativo por invitaciones, aceptación/rechazo y creación de membresía).

Precondición operativa de 013: la persona ya debe ser miembro del grupo; el ingreso al grupo se resuelve exclusivamente en 012.

No se modifica el estado histórico de specs previas.

## Clarificaciones de alcance (013)

1. 013 trabaja con personas que **ya pertenecen** a grupos existentes.
2. 013 no añade invitaciones ni gestión avanzada de membresía.
3. 013 no altera reglas financieras de 005 salvo bug real demostrado.
4. 013 prioriza UX, consistencia, navegación y cobertura de aceptación.

## Estado actual auditado (resumen)

Fuente detallada: `evidence/current-state-audit.md`.

- Existe flujo funcional de listado compartido, creación por grupo y detalle con tabs Compra/Gastos/Personas.
- Backend aplica autorización real por rol/membresía activa (no UI-only).
- Hay pruebas relevantes, pero falta formalización integral de aceptación UX específica para shared-lists.
- El detalle compartido concentra mucha lógica; navegación contextual y jerarquía de información son mejorables.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Navegación coherente al mismo recurso (Priority: P1)

Como persona usuaria, quiero llegar a la misma lista compartida desde Grupos o Mis listas sin experiencias incoherentes.

**Independent Test**: abrir la misma `sharedListId` desde ambos recorridos y validar igualdad de recurso, tab, estado y navegación back/forward.

**Acceptance Scenarios**
1. **Given** entrada por `/groups/:groupId` o `/lists`, **When** abro una shared list, **Then** llego al mismo recurso `/shared-lists/:listId` con contexto consistente.
2. **Given** `tab` en query, **When** recargo, **Then** se conserva la pestaña y el recurso.
3. **Given** navegación atrás/adelante, **When** alterno tabs y origen, **Then** el estado URL/UI permanece coherente.

---

### User Story 2 — Listado compartido claro y completo (Priority: P1)

Como persona miembro de grupos, quiero entender mis listas compartidas por grupo, estado y estimación sin sobrecarga administrativa.

**Independent Test**: validar vacío/una/múltiples/multi-grupo/parcial/error/carga y nombres largos en `SharedListsPage`.

**Acceptance Scenarios**
1. **Given** grupos cargados, **When** selecciono uno, **Then** veo listas activas/históricas con nombre, contexto de grupo, pendientes y estimación.
2. **Given** URL con `groupId` inválido o no accesible, **When** abre la vista, **Then** se informa claramente y no se filtran datos.
3. **Given** fallo parcial o 403 en listas, **When** se renderiza, **Then** hay mensaje accionable y reintento.

---

### User Story 3 — Crear lista compartida sin fricción (Priority: P1)

Como persona usuaria, quiero crear una shared list indicando grupo y nombre, sin exponer IDs internos.

**Independent Test**: crear/cancelar/error/permisos en escenarios sin grupos, grupo único y múltiples grupos.

**Acceptance Scenarios**
1. **Given** grupo seleccionado, **When** creo lista, **Then** se envía `{name, groupId}` y navega al detalle creado.
2. **Given** no pertenezco a grupos, **When** abro shared lists, **Then** se muestra explicación útil.
3. **Given** 403 al crear, **When** falla, **Then** se conserva borrador y se muestra mensaje de permiso.

---

### User Story 4 — Detalle Compra con paridad funcional (Priority: P1)

Como participante de una lista compartida, quiero preparar y ejecutar compra con la misma claridad que en lista personal, respetando permisos por rol.

**Independent Test**: añadir/editar/eliminar/marcar/desmarcar/100 ítems/precios desconocidos con Owner/Admin/Member.

**Acceptance Scenarios**
1. **Given** lista activa, **When** opero ítems y estado comprado, **Then** se conserva progreso/estimación y foco accesible.
2. **Given** member no autor del ítem, **When** intenta editar/eliminar, **Then** UI y backend niegan de forma coherente.
3. **Given** lista cerrada, **When** intento escribir, **Then** la UI bloquea y comunica estado de solo consulta.

---

### User Story 5 — Gastos comprensibles y separados de estimación (Priority: P1)

Como persona del grupo, quiero registrar y revisar gastos reales sin confundirlos con la estimación de compra.

**Independent Test**: vacío/registros/balance/propuesta/errores/permisos en tab Gastos.

**Acceptance Scenarios**
1. **Given** tab Gastos, **When** consulto balance/propuesta, **Then** se muestran importes reales por persona con claridad.
2. **Given** permisos insuficientes o 403, **When** falla carga o escritura, **Then** se informa sin exponer acciones inválidas.
3. **Given** lista activa/cerrada, **When** registro gasto, **Then** comportamiento respeta reglas vigentes de backend.

---

### User Story 6 — Personas útiles sin funciones falsas (Priority: P2)

Como participante, quiero ver quién forma parte de la lista/grupo y su rol, sin acciones de invitación fuera de alcance.

**Independent Test**: validar presentación Owner/Admin/Member, vacíos y errores en tab Personas.

**Acceptance Scenarios**
1. **Given** miembros disponibles, **When** abro Personas, **Then** veo nombre y rol legible.
2. **Given** sin miembros o error, **When** abre tab, **Then** hay estado vacío/error claro.
3. **Given** alcance 013, **When** reviso tab, **Then** no aparecen acciones de invitar/aceptar/rechazar/cambiar rol.

---

### User Story 7 — Gestión segura de eliminación/cierre (Priority: P2)

Como owner/admin, quiero cerrar o eliminar lista con confirmación y destino consistente.

**Independent Test**: confirmar/cancelar, foco restaurado, navegación post-acción, errores por gastos asociados.

**Acceptance Scenarios**
1. **Given** eliminar lista, **When** confirmo, **Then** navega al destino correcto y restaura foco.
2. **Given** lista con gastos, **When** intento eliminar, **Then** se rechaza y explica alternativa (cerrar/conservar cuentas).
3. **Given** cancelar diálogo, **When** cierro sin confirmar, **Then** no hay escritura y foco vuelve al trigger.

## Edge Cases

- `groupId` en URL no pertenece al usuario.
- Lista compartida borrada/cerrada entre carga y operación.
- Pérdida de membresía mientras la vista está abierta (reload/403).
- Producto con gastos: bloqueo de borrado de ítem.
- Desmarcar comprado tras registrar gastos.
- Múltiples grupos con nombres largos y listas largas.
- Responsive 320 con tablist + acciones + diálogo.
- Historial compartido vacío vs histórico con datos.

## Functional Requirements

### Navegación y contexto
- **FR-001** Shared list debe resolverse por `listId` único desde cualquier origen.
- **FR-002** El detalle debe exponer contexto mínimo: nombre de lista, grupo, estado, progreso y estimación.
- **FR-003** `tab` se persiste en URL y soporta deep-link/reload/back/forward.
- **FR-004** El enlace "volver" debe ser coherente con `groupId` disponible y fallback seguro.

### Listado y creación
- **FR-005** Listado por grupo sin introducir tabla administrativa.
- **FR-006** Tarjetas compartidas muestran nombre, pendientes, estimación y motivo histórico cuando aplique.
- **FR-007** Crear shared list no expone ni exige IDs manuales al usuario.
- **FR-008** Estados: loading/empty/error/parcial deben ser explícitos y accionables.

### Compra compartida
- **FR-009** Mantener semántica de checkbox, progreso y estimación parcial en shared detail.
- **FR-010** Acciones de ítem respetan permisos backend (owner/admin/member autor).
- **FR-011** Lista cerrada bloquea escrituras y mantiene consulta.

### Gastos
- **FR-012** Diferenciar visual y semánticamente estimación vs gasto real.
- **FR-013** Reutilizar settlement vigente (summary/proposal/history) sin reglas financieras nuevas.
- **FR-014** Errores de gastos deben distinguir falta de permiso, indisponibilidad y error genérico.

### Personas
- **FR-015** Mostrar miembros y rol con presentación legible.
- **FR-016** No introducir acciones de invitación ni administración de membresía.

### Eliminación y cierre
- **FR-017** Confirmaciones deben incluir nombre de lista y restauración de foco.
- **FR-018** Borrado y cierre respetan restricciones por gastos/historial existentes.

### Responsive y accesibilidad
- **FR-019** Mobile-first 320/390/430 como baseline; luego 768/1280.
- **FR-020** Targets táctiles mínimo 44x44 en controles interactivos.
- **FR-021** Teclado/foco/Escape/dialog restoration/tab semantics obligatorios.
- **FR-022** Mantener cumplimiento de reduced-motion, forced-colors y reflow sin declarar lector real no ejecutado.

### Rendimiento
- **FR-023** No introducir queries duplicadas ni waterfalls evitables en shared flows.
- **FR-024** Mantener deuda LCP global separada; 013 no reescribe budgets previos.

## Non-Goals (Out of Scope)

- Invitaciones y colaboración de onboarding (012): invitar, aceptar/rechazar, links/email, notificaciones.
- Cambios de reglas de settlement de 005 sin bug demostrado.
- Nuevos dominios o endpoints no imprescindibles.
- Rework global del sistema de autenticación (010) o navegación principal fuera de shared-lists.

## Testing Requirements

La implementación de 013 deberá cubrir como mínimo:

1. **Listado**: vacío, una, múltiples, distintos grupos, error parcial.  
2. **Creación**: seleccionar grupo, crear, cancelar, error, permisos.  
3. **Compra**: añadir, editar, eliminar, marcar, desmarcar, 100 ítems, precios desconocidos.  
4. **Gastos**: vacío, registros, balances, errores, permisos.  
5. **Personas**: Owner/Admin/Member, presentación, sin funciones falsas.  
6. **Navegación**: deep-link, reload, back/forward, Group→Shared List, Lists→Shared List.  
7. **Responsive**: 320/390/430/768/1280.

## Success Criteria

- **SC-001** Flujo Grupo→Shared List y Mis listas→Shared List converge al mismo recurso con estado coherente.
- **SC-002** Matriz de permisos Owner/Admin/Member queda reflejada en UI y validada con 403 real.
- **SC-003** Compra compartida mantiene paridad de claridad con lista personal sin duplicar lógicas innecesarias.
- **SC-004** Gastos comunica gasto real y settlement sin confundir con estimación de productos.
- **SC-005** Personas presenta miembros/roles con estados útiles, sin invadir alcance 012.
- **SC-006** Cobertura de pruebas y evidencia de responsive/a11y alcanza los escenarios mínimos definidos.
