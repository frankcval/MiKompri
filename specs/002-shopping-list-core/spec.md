# Feature Specification: Shopping List Core Hardening

**Feature Branch**: `[002-shopping-list-core]`

**Created**: 2026-06-30

**Status**: Implementada; cierre documental menor pendiente

**Input**: User description: "Refinar y completar el core existente de ShoppingList para uso individual"

**Cierre**: La feature quedó implementada a nivel funcional en las fases 1-7 descritas en `tasks.md`, con evidencia operativa registrada en `quickstart.md`. Quedan pendientes únicamente el cierre documental del gate Docker y una limpieza menor de wording en artefactos.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Crear y consultar lista de compra (Priority: P1)

Como usuario, quiero crear una lista de compra y poder consultarla para empezar a planificar mi compra.

**Why this priority**: Es la base del MVP; sin lista no existe flujo de valor.

**Estado**: ✅ Completada

**Independent Test**: Se valida creando una lista con un nombre válido y consultándola por su identificador, confirmando datos básicos y trazabilidad.

**Acceptance Scenarios**:

1. **Given** que el usuario inicia una nueva gestión, **When** crea una lista con nombre válido, **Then** la lista queda registrada con estado inicial y fecha de creación.
2. **Given** que existe una lista creada, **When** el usuario consulta la lista, **Then** obtiene nombre, ítems actuales, progreso y metadatos de trazabilidad básica.
3. **Given** que el usuario intenta crear una lista con nombre vacío, **When** confirma la operación, **Then** el sistema rechaza la solicitud con un error de validación claro.

---

### User Story 2 - Gestionar ítems de la lista (Priority: P1)

Como usuario, quiero agregar, editar, marcar como comprado y eliminar ítems para mantener la lista al día.

**Why this priority**: Es el uso principal del producto en el día a día.

**Estado**: ✅ Completada

**Independent Test**: Se valida con un flujo completo sobre una lista existente: agregar ítem, editarlo, marcarlo como comprado y eliminarlo, verificando resultado tras cada acción.

**Acceptance Scenarios**:

1. **Given** que existe una lista activa, **When** el usuario agrega un ítem válido, **Then** el ítem se registra en estado pendiente con trazabilidad de creación.
2. **Given** que existe un ítem pendiente, **When** el usuario lo marca como comprado, **Then** el ítem cambia a estado comprado y actualiza su trazabilidad de modificación.
3. **Given** que existe un ítem en la lista, **When** el usuario edita sus datos permitidos, **Then** los cambios se guardan y quedan visibles al consultar la lista.
4. **Given** que existe un ítem en la lista, **When** el usuario elimina el ítem, **Then** el ítem deja de mostrarse y el progreso de la lista se recalcula.

---

### User Story 3 - Consultar progreso de la lista (Priority: P2)

Como usuario, quiero ver el avance de mi lista para saber cuánto me falta por comprar.

**Why this priority**: Aporta visibilidad inmediata del estado de compra una vez existen ítems.

**Estado**: ✅ Completada

**Independent Test**: Se valida creando ítems en distintos estados y consultando la lista para confirmar totales y porcentaje de avance.

**Acceptance Scenarios**:

1. **Given** que una lista tiene ítems pendientes y comprados, **When** el usuario consulta la lista, **Then** ve total de ítems, comprados, pendientes y porcentaje de progreso.
2. **Given** que una lista no tiene ítems, **When** el usuario consulta la lista, **Then** el sistema muestra progreso en 0% sin error.

---

### User Story 4 - Ver trazabilidad básica (Priority: P2)

Como usuario, quiero conocer cuándo se creó y cuándo se modificó por última vez una lista o ítem.

**Why this priority**: Facilita control y seguimiento operativo del listado.

**Estado**: ✅ Completada

**Independent Test**: Se valida creando una lista, actualizando sus datos, operando ítems y consultando después que lista e ítems muestren marca de creación y última modificación. En la lista, `UpdatedAt` refleja tanto cambios directos como mutaciones de sus ítems.

**Acceptance Scenarios**:

1. **Given** que se crea una lista o ítem, **When** el usuario consulta el recurso, **Then** visualiza su fecha/hora de creación.
2. **Given** que se modifica una lista o cualquiera de sus ítems, **When** el usuario consulta el recurso correspondiente, **Then** visualiza una fecha/hora de última modificación actualizada.

---

### Edge Cases

- Consulta de una lista inexistente.
- Edición de un ítem inexistente en una lista existente.
- Eliminación de un ítem inexistente.
- Intento de agregar ítem duplicado para el mismo producto dentro de una misma lista.
- Reintento de marcado como comprado sobre un ítem ya comprado.
- Cálculo de progreso con lista vacía.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir crear una lista de compra con nombre válido.
- **FR-002**: El sistema MUST permitir consultar una lista de compra existente.
- **FR-003**: El sistema MUST permitir agregar ítems válidos a una lista existente.
- **FR-004**: El sistema MUST permitir editar ítems existentes de una lista.
- **FR-005**: El sistema MUST permitir marcar un ítem como comprado.
- **FR-006**: El sistema MUST permitir eliminar un ítem de la lista.
- **FR-007**: El sistema MUST exponer el progreso de la lista (total, comprados, pendientes, porcentaje) dentro de la consulta de detalle de la lista.
- **FR-008**: El sistema MUST mantener trazabilidad básica de creación y modificación en listas e ítems; la lista actualiza su marca de modificación tanto en cambios propios como en mutaciones de sus ítems.
- **FR-009**: El sistema MUST validar datos obligatorios en todas las operaciones.
- **FR-010**: El sistema MUST responder con error claro cuando la lista no exista.
- **FR-011**: El sistema MUST responder con error claro cuando el ítem objetivo no exista.
- **FR-012**: El sistema MUST impedir ítems duplicados del mismo producto dentro de una misma lista.
- **FR-013**: El sistema MUST mantener fuera de alcance de esta feature: registro de usuarios, login, invitaciones, compartir listas, notificaciones, cliente MAUI completo, pagos y estadísticas avanzadas.

### Reglas de negocio

- **RB-001**: Toda lista debe tener un nombre no vacío.
- **RB-002**: Todo ítem pertenece a una única lista.
- **RB-003**: Una lista no puede contener dos ítems para el mismo producto.
- **RB-004**: El progreso se calcula con la proporción de ítems comprados sobre ítems totales; si no hay ítems, el progreso es 0%.
- **RB-005**: Marcar un ítem ya comprado no debe alterar indebidamente el estado global de la lista.
- **RB-006**: Toda creación y modificación debe actualizar la trazabilidad correspondiente.

### Errores esperados

- **ERR-001**: Error de validación por nombre de lista inválido.
- **ERR-002**: Error por lista inexistente.
- **ERR-003**: Error por ítem inexistente.
- **ERR-004**: Error por duplicidad de ítem de producto en la misma lista.
- **ERR-005**: Error por datos de entrada inválidos en edición o alta de ítems.

### Criterios de done

- **CD-001**: Todas las historias de usuario de esta spec tienen al menos un escenario de aceptación verificable.
- **CD-002**: Los requisitos FR-001 a FR-013 tienen validación funcional documentada.
- **CD-003**: Las reglas RB-001 a RB-006 están cubiertas en pruebas de comportamiento.
- **CD-004**: Los errores ERR-001 a ERR-005 están cubiertos en pruebas de casos negativos.
- **CD-005**: El progreso de la lista se actualiza correctamente tras agregar, marcar, editar y eliminar ítems.
- **CD-006**: La trazabilidad básica está presente y consistente en lista e ítems.

### Key Entities *(include if feature involves data)*

- **Lista de compra**: Conjunto de ítems de compra gestionados por un usuario; incluye nombre, estado, progreso y trazabilidad básica.
- **Ítem de compra**: Unidad de producto dentro de una lista; incluye referencia de producto, estado (pendiente/comprado), datos editables y trazabilidad básica.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de las tareas T001-T046 queda marcado como completado y alineado con los artefactos de la feature.
- **SC-002**: El 100% de operaciones válidas de gestión de ítems refleja cambios correctos al consultar la lista inmediatamente después.
- **SC-003**: El 100% de casos de prueba de progreso devuelve el valor esperado según estado real de ítems.
- **SC-004**: El 100% de errores esperados devuelve mensajes claros y consistentes con la regla violada.
- **SC-005**: El 100% de listas e ítems creados o modificados conserva campos de trazabilidad básica correctos.

## Assumptions

- Esta feature se ejecuta sobre el modo de uso individual ya existente (sin colaboración multiusuario).
- La trazabilidad básica de esta fase se limita a `CreatedAt` y `UpdatedAt`; la identidad
  del actor (`CreatedBy`/`UpdatedBy`) queda diferida al trabajo posterior de autenticación
  y evolución del bounded context `Users`.
- Existe un identificador de producto utilizable para detectar duplicados dentro de la lista.
- La trazabilidad básica se limita a creación y última modificación, sin auditoría histórica completa.
- El cliente MAUI completo está fuera de alcance y no condiciona la validación funcional de esta fase de hardening.

## Pendientes de cierre documental

- Cerrar la ejecución completa del gate Docker documentado en `evidence/docker-gate.md`.
- Limpieza menor de wording en artefactos relacionados con el cierre final.
