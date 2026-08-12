# Feature Specification: MVP-3 Listas Compartidas y Reparto de Gastos

**Feature Branch**: `[005-shared-lists-settlement]`

**Created**: 2026-08-06

**Status**: Draft

**Input**: User description: "Crear la especificación 005-shared-lists-settlement para MVP-3: listas compartidas por grupo y reparto de gastos entre miembros."

## Clarifications

### Session 2026-08-06

- Q: ¿Qué matriz de permisos debe regir Owner/Admin/Member en listas compartidas? → A: Opción B — Owner/Admin con operación completa sobre listas y gastos; Member colaborador (puede agregar/editar ítems propios y marcar compra, registrar pago solo de ítems que compra, sin borrar listas ni borrar gastos ajenos).
- Q: En un gasto, ¿el pagador debe ser participante? → A: Opción C — el pagador puede o no participar; si está en participantes, consume cuota; si no, solo adelanta pago.
- Q: ¿Qué identidad de actor es obligatoria al registrar gasto? → A: Opción A — `PaidBy` obligatorio y `PurchasedBy` opcional.
- Q: ¿Cómo debe definirse la propuesta de liquidación para resultados consistentes? → A: Opción B — algoritmo determinista: emparejar mayor deudor con mayor acreedor; en empate, ordenar por `UserId` ascendente.
- Q: ¿Cuántos participantes mínimos debe tener un gasto? → A: Opción A — al menos 1 participante activo.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Crear y operar listas compartidas por grupo (Priority: P1)

Como miembro de un grupo, quiero crear una lista compartida vinculada a mi grupo para que los integrantes colaboren en una misma compra.

**Why this priority**: Es el núcleo del MVP-3. Sin lista compartida por grupo no existe colaboración real en ShoppingList.

**Independent Test**: Se valida creando una lista compartida asociada a un `GroupId`, verificando que solo miembros del grupo pueden verla y modificarla, y confirmando que las listas personales siguen funcionando sin cambios.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado con membresía activa en un grupo, **When** crea una lista compartida asociada a ese grupo, **Then** la lista queda registrada como compartida y vinculada al `GroupId`.
2. **Given** una lista compartida existente, **When** un miembro del grupo la consulta, **Then** puede verla con sus ítems y trazabilidad correspondiente.
3. **Given** una lista compartida existente, **When** un usuario sin membresía activa en ese grupo intenta consultarla o modificarla, **Then** el sistema rechaza la operación por falta de autorización sin exponer información sensible de la lista.
4. **Given** listas personales existentes de usuarios, **When** se habilita la funcionalidad compartida, **Then** las listas personales mantienen su comportamiento actual sin cambios de acceso ni reglas funcionales.

---

### User Story 2 - Registrar gastos colaborativos por ítem con participantes (Priority: P1)

Como miembro del grupo, quiero registrar quién añadió, compró y pagó cada ítem, su precio real y los participantes del gasto para reflejar el gasto real de forma trazable.

**Why this priority**: Sin registro detallado del gasto no se puede calcular reparto ni liquidación entre miembros.

**Independent Test**: Se valida agregando y actualizando ítems en una lista compartida, registrando `AddedBy`, comprador/pagador, precio real y participantes, y comprobando que todos los datos quedan auditables.

**Acceptance Scenarios**:

1. **Given** una lista compartida y un miembro autorizado, **When** agrega un ítem, **Then** el sistema registra automáticamente `AddedBy` con la identidad del miembro.
2. **Given** un ítem en una lista compartida, **When** un miembro registra la compra, **Then** el sistema permite capturar quién compró o pagó y el precio real pagado.
3. **Given** un ítem comprado, **When** se seleccionan los participantes del gasto, **Then** solo se permiten miembros activos del grupo y se persiste la selección para cálculos posteriores.
4. **Given** operaciones relevantes sobre el ítem (alta, edición, compra, pago, cambio de participantes), **When** la operación se confirma, **Then** el sistema conserva trazabilidad de actor, tipo de acción y momento de registro.

---

### User Story 3 - Calcular reparto, balances y propuesta de liquidación (Priority: P2)

Como miembro del grupo, quiero ver cuánto pagó y cuánto le corresponde a cada participante para obtener balances deudores/acreedores y una propuesta simple de liquidación.

**Why this priority**: Convierte los datos de compras en valor práctico para cerrar cuentas del grupo.

**Independent Test**: Se valida con una lista compartida con múltiples gastos y participantes, comprobando que los totales, saldos y propuesta de liquidación sean consistentes con las reglas de cálculo y redondeo.

**Acceptance Scenarios**:

1. **Given** varios ítems comprados con precio real y participantes definidos, **When** se solicita el resumen del reparto, **Then** el sistema calcula por participante el total pagado y el total que le corresponde pagar.
2. **Given** un resumen de reparto calculado, **When** se generan balances, **Then** cada participante queda clasificado con saldo acreedor, deudor o equilibrado.
3. **Given** balances deudores y acreedores, **When** se solicita liquidación simplificada, **Then** el sistema propone un conjunto mínimo razonable de transferencias de deuda para saldar el grupo.
4. **Given** que no existen gastos válidos para repartir, **When** se solicita liquidación, **Then** el sistema devuelve resultado vacío sin error y comunica que no hay deudas pendientes.

---

### User Story 4 - Gestionar cambios de membresía con datos históricos (Priority: P2)

Como owner o admin del grupo, quiero que la salida o eliminación de miembros no rompa los cálculos ni la trazabilidad de operaciones ya registradas.

**Why this priority**: Protege la integridad histórica y evita inconsistencias cuando cambia la composición del grupo.

**Independent Test**: Se valida retirando un miembro con operaciones previas registradas y comprobando que el historial y cálculos históricos siguen siendo consultables, mientras nuevas operaciones respetan la membresía activa.

**Acceptance Scenarios**:

1. **Given** un miembro con operaciones históricas registradas en listas compartidas, **When** su membresía pasa a inactiva o es eliminada, **Then** sus operaciones históricas se conservan para auditoría y cálculo histórico.
2. **Given** un miembro inactivo o eliminado, **When** se intenta asignarlo como nuevo participante, comprador o pagador en gastos nuevos, **Then** el sistema rechaza la operación por no tener membresía activa.
3. **Given** una liquidación que incluye operaciones previas a la baja de un miembro, **When** se recalcula el balance histórico del período afectado, **Then** el resultado mantiene coherencia con los datos registrados en ese período.

---

### Edge Cases

- Intento de crear una lista compartida con un `GroupId` inexistente o sin membresía activa del solicitante.
- Intento de acceso concurrente de dos miembros editando el mismo ítem de gasto con valores distintos.
- Ítem marcado como comprado sin precio real pagado.
- Ítem con precio real positivo pero sin participantes seleccionados.
- Participante duplicado en un mismo gasto.
- Participante que no pertenece al grupo al momento de registrar el gasto.
- Precio real con más de dos decimales.
- Reparto donde el redondeo deja diferencia residual en centavos.
- Miembro eliminado con saldos pendientes aún no liquidados.
- Grupo sin miembros activos suficientes para nuevas operaciones, pero con historial previo existente.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE permitir crear listas compartidas asociadas a un `GroupId` gestionado por el bounded context Users.
- **FR-002**: El sistema DEBE permitir que coexistan listas personales y listas compartidas, sin alterar la experiencia ni reglas actuales de listas personales.
- **FR-003**: El sistema DEBE permitir acceso de lectura y escritura sobre listas compartidas únicamente a usuarios con membresía activa en el grupo vinculado.
- **FR-004**: El sistema DEBE aplicar permisos por rol en listas compartidas con la siguiente matriz mínima: `Owner` y `Admin` pueden crear/editar/cerrar/eliminar listas y registrar/editar/eliminar gastos y participantes; `Member` puede colaborar agregando/editando ítems propios, marcando compra y registrando pago solo de ítems que compra, sin eliminar listas ni eliminar gastos ajenos.
- **FR-005**: El sistema DEBE registrar `AddedBy` al crear cada ítem en una lista compartida.
- **FR-006**: El sistema DEBE permitir registrar por ítem quién compró y/o pagó, y el precio real pagado; la persona pagadora puede ser o no participante del gasto.
- **FR-006a**: El sistema DEBE exigir `PaidBy` como dato obligatorio en todo gasto registrado; `PurchasedBy` es opcional.
- **FR-007**: El sistema DEBE permitir seleccionar qué miembros participan en cada gasto asociado a ítems comprados.
- **FR-007a**: El sistema DEBE exigir al menos 1 participante activo por gasto; si no hay participantes válidos, el gasto se rechaza.
- **FR-008**: El sistema DEBE impedir participantes duplicados en un mismo gasto.
- **FR-009**: El sistema DEBE impedir registrar gastos con precio real menor o igual a cero.
- **FR-010**: El sistema DEBE calcular para cada participante el total efectivamente pagado.
- **FR-011**: El sistema DEBE calcular para cada participante el total que le corresponde pagar según su participación en cada gasto.
- **FR-012**: El sistema DEBE calcular balances netos por participante (saldo acreedor/deudor/equilibrado).
- **FR-013**: El sistema DEBE generar una propuesta de liquidación simplificada que minimice transferencias necesarias para saldar deudas del grupo, usando un algoritmo determinista de emparejamiento (mayor deudor con mayor acreedor; empates por `UserId` ascendente).
- **FR-014**: El sistema DEBE bloquear consultas o modificaciones de listas compartidas por usuarios ajenos al grupo, evitando exposición de datos del grupo.
- **FR-015**: El sistema DEBE conservar trazabilidad de operaciones relevantes en listas compartidas y gastos (actor, acción y fecha/hora).
- **FR-016**: El sistema DEBE preservar operaciones históricas cuando un miembro salga o sea eliminado del grupo.
- **FR-017**: El sistema DEBE impedir que miembros inactivos o eliminados participen en operaciones nuevas.
- **FR-018**: El sistema DEBE mantener compatibilidad con identificadores de ProductCatalog para ítems, sin exigir sincronización automática de precios ni dependencias obligatorias de consulta en tiempo real.
- **FR-019**: ShoppingList DEBE validar identidad autenticada por JWT de un proveedor OIDC externo y validar membresía/rol del grupo a través de contratos entre bounded contexts, sin acceso directo a la base de datos de Users.
- **FR-020**: Users DEBE seguir siendo el propietario de perfiles, grupos, membresías y roles; ShoppingList solo consume esa información por mecanismos de integración definidos en fases posteriores.

### Reglas de autorización

- **AR-001**: Antes de cualquier operación sobre una lista compartida, el sistema DEBE verificar que el usuario tiene membresía activa en el grupo correspondiente.
- **AR-002**: La autorización de la acción específica DEBE respetar la matriz definida en **FR-004**.
- **AR-003**: Si la membresía es inexistente, está inactiva o la acción no está permitida por rol, el sistema DEBE responder `403 Forbidden` sin revelar si el recurso existe.
- **AR-004**: Las operaciones sobre listas personales siguen usando sus reglas actuales y no dependen de la autorización por grupo.

### Reglas de cálculo y redondeo

- **CR-001**: Todo gasto válido debe incluir `PaidBy`; `PurchasedBy` es opcional y no afecta por sí mismo el reparto.
- **CR-002**: El costo distribuible de cada gasto es el precio real pagado del ítem.
- **CR-003**: Todo gasto debe tener al menos 1 participante activo; sin participantes válidos no se calcula reparto.
- **CR-004**: El costo distribuible se divide en partes iguales entre los participantes seleccionados de ese gasto, salvo que en una futura iteración se definan ponderaciones explícitas.
- **CR-005**: Los importes individuales se redondean a 2 decimales con redondeo comercial estándar.
- **CR-006**: Si el redondeo genera diferencia residual de centavos respecto al total del gasto, el residuo se asigna al participante pagador del gasto para mantener conservación contable.
- **CR-007**: El pagador de un gasto puede ser o no participante del consumo de ese gasto; si está incluido en participantes, también asume su cuota correspondiente, y si no, su cuota de consumo en ese gasto es 0.
- **CR-008**: `TotalPagado(participante)` es la suma de importes registrados como pagados por ese participante.
- **CR-009**: `TotalCorrespondiente(participante)` es la suma de sus cuotas en todos los gastos donde participa.
- **CR-010**: `BalanceNeto = TotalPagado - TotalCorrespondiente`; positivo = acreedor, negativo = deudor, cero = equilibrado.
- **CR-011**: La propuesta de liquidación simplificada DEBE generar, para `D` deudores y `A` acreedores, como máximo `D + A - 1` transferencias cuando no existan restricciones adicionales, manteniendo exactitud monetaria de saldos finales.
- **CR-012**: La generación de liquidación debe ser determinista: con los mismos balances de entrada, el sistema debe producir el mismo conjunto ordenado de transferencias; primero empareja el mayor saldo deudor con el mayor saldo acreedor disponible y, en empate de importe, se prioriza `UserId` ascendente.


### Dependencias entre bounded contexts

- **DEP-001**: `Users` provee identidad, perfiles, grupos, membresías y roles como fuente de verdad.
- **DEP-002**: `ShoppingList` depende de validación de identidad y membresía por contrato de integración con `Users`, sin acoplamiento a su almacenamiento interno.
- **DEP-003**: `ProductCatalog` provee identificadores de productos reutilizables; `ShoppingList` conserva compatibilidad de referencia, sin dependencia obligatoria para registrar precios reales en gastos.
- **DEP-004**: Las decisiones técnicas de consulta, caché, propagación o sincronización de membresías se difieren a `/speckit.clarify` y `/speckit.plan`.

### Key Entities *(include if feature involves data)*

- **SharedPurchaseList**: Lista de compra colaborativa asociada a un `GroupId`, con metadatos de creación/actualización y estado operativo de la lista.
- **SharedListItem**: Ítem de lista compartida con referencia de producto, cantidad, estado de compra, `AddedBy` y trazabilidad de cambios.
- **ItemExpenseRecord**: Registro de gasto por ítem comprado con precio real pagado, persona compradora/pagadora y fecha del registro.
- **ExpenseParticipant**: Relación entre un gasto y cada miembro participante con su cuota correspondiente del gasto.
- **ParticipantBalance**: Resultado agregado por miembro con total pagado, total correspondiente y balance neto.
- **SettlementProposal**: Conjunto de movimientos sugeridos para saldar balances entre deudores y acreedores con mínimo de transacciones.
- **SharedListAuditEvent**: Evento trazable de acción relevante sobre listas e ítems compartidos (quién, qué, cuándo).

### Out of Scope

- Crear o administrar grupos desde ShoppingList.
- Acceso directo entre bases de datos de bounded contexts.
- Cliente Android .NET MAUI.
- Presupuestos mensuales.
- OCR de tickets.
- Alertas de precios.
- Scraping.
- Recomendaciones inteligentes.
- Pagos bancarios reales.
- Transferencias monetarias reales.
- Notificaciones push.
- Sincronización automática de precios con ProductCatalog.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de listas compartidas creadas queda asociada a un `GroupId` válido y accesible solo por miembros activos del grupo.
- **SC-002**: El 100% de intentos de acceso de usuarios ajenos o inactivos sobre listas compartidas es rechazado.
- **SC-003**: El equipo puede ejecutar un flujo reproducible de compra colaborativa (crear lista, añadir ítems, registrar gastos y obtener liquidación) con evidencia registrada del resultado final y del tiempo total empleado, documentada en `quickstart.md`.
- **SC-004**: El 100% de gastos registrados conserva trazabilidad mínima de actor y momento en operaciones relevantes.
- **SC-005**: El 100% de cálculos de reparto y balances coincide con los resultados esperados en pruebas de aceptación con casos de redondeo.
- **SC-006**: Existe un protocolo de validación humana documentado en `quickstart.md` para comprobar comprensión del reparto y la liquidación; la validación se considera satisfactoria si al menos el 90% de los participantes interpreta correctamente quién debe cuánto y a quién.
- **SC-007**: El 100% de operaciones históricas de miembros dados de baja sigue disponible para auditoría y cálculo histórico del período correspondiente.

## Assumptions

- La autenticación de usuarios continúa delegada a un proveedor OIDC externo que emite JWT válidos para MiKompri.
- Users mantiene la propiedad exclusiva de perfiles, grupos, membresías y roles; ShoppingList solo consume identidad y autorización derivada por contratos entre contextos.
- La moneda operativa para reparto es única por lista en esta iteración.
- El reparto de cada gasto es equitativo entre participantes seleccionados, salvo futuras extensiones de reparto ponderado.
- La salida de un miembro no elimina su historial; solo bloquea su participación en operaciones nuevas.
- Los identificadores de producto se mantienen compatibles con ProductCatalog, pero el precio real de gasto se registra manualmente en ShoppingList.
- La validación de SC-003 y SC-006 se documenta en `quickstart.md` con un protocolo reproducible y una plantilla de evidencia.
- La estrategia técnica de integración entre Users y ShoppingList se decidirá en fases posteriores, sin comprometer en esta spec un mecanismo específico.
