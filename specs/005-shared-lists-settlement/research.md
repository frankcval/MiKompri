# Research: 005 — Shared Lists & Settlement (MVP-3)

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-08-06

## Decision 1: Matriz de permisos operativa (Owner/Admin/Member)

- **Decision**: Adoptar la matriz de permisos de la opción B validada en clarify.
  - `Owner` y `Admin`: operación completa sobre listas compartidas y gastos (crear/editar/cerrar/eliminar lista; registrar/editar/eliminar gastos/participantes).
  - `Member`: colaboración en ítems propios, marcado de compra y registro de pago solo de ítems que compra; sin borrado de listas ni gastos ajenos.
- **Rationale**: Balancea colaboración real con control de acciones destructivas y permite validaciones claras por rol.
- **Alternatives considered**:
  - Roles idénticos para todos: descartado por riesgo de cambios destructivos por miembros no administradores.
  - Solo Owner administrativo: descartado por fricción operativa cuando el owner no está disponible.

## Decision 2: Regla de pagador en gasto

- **Decision**: `PaidBy` es obligatorio; `PurchasedBy` es opcional. El pagador puede participar o no en el consumo.
- **Rationale**: Garantiza responsable contable de cada gasto y soporta casos donde una persona adelanta pago para otros.
- **Alternatives considered**:
  - Exigir pagador participante: descartado, no cubre compra por terceros.
  - Exigir comprador y pagador obligatorios: descartado por rigidez innecesaria.

## Decision 3: Participación mínima en gasto

- **Decision**: Todo gasto requiere al menos 1 participante activo.
- **Rationale**: Evita gastos huérfanos y división inválida del costo.
- **Alternatives considered**:
  - Permitir 0 participantes: descartado por inconsistencias en cálculo y liquidación.
  - Exigir todos los miembros del grupo: descartado por no reflejar consumo real.

## Decision 4: Reparto y redondeo monetario

- **Decision**: Reparto equitativo entre participantes seleccionados, redondeo a 2 decimales y asignación de residuo al pagador.
- **Rationale**: Conserva exactitud contable (`suma cuotas == precio real`) y simplifica comprensión para usuarios.
- **Alternatives considered**:
  - Distribuir residuo al primer participante: descartado por menor trazabilidad de adelanto de fondos.
  - Reparto ponderado desde MVP-3: descartado por complejidad fuera de alcance.

## Decision 5: Algoritmo de liquidación simplificada

- **Decision**: Liquidación determinista emparejando mayor deudor con mayor acreedor; desempate por `UserId` ascendente.
- **Rationale**: Resultados reproducibles, auditables y consistentes entre ejecuciones.
- **Alternatives considered**:
  - “Mínimo razonable” sin desempate fijo: descartado por no determinismo.
  - Priorización por rol (Owner/Admin/Member): descartado por sesgo no contable.

## Decision 6: Integración con Users sin acceso directo a BD

- **Decision**: ShoppingList validará identidad y membresía por contrato de integración de aplicación (puerto de autorización de grupo), sin referencias de dominio ni acceso a almacenamiento de Users.
- **Rationale**: Cumple TP2 y el requisito FR-019/FR-020; mantiene independencia de bounded contexts.
- **Alternatives considered**:
  - Lectura directa de tablas de Users: descartado por violar constitución.
  - Replicar modelo de membresía en ShoppingList como fuente primaria: descartado por duplicar ownership de datos.

## Decision 7: Manejo de bajas de miembros

- **Decision**: Mantener histórico inmutable de operaciones con identificadores de actor; bloquear participación futura de miembros inactivos/eliminados.
- **Rationale**: Preserva trazabilidad y coherencia histórica de balances sin permitir nuevas operaciones inválidas.
- **Alternatives considered**:
  - Borrado lógico con anonimización total inmediata: descartado por pérdida de trazabilidad colaborativa.
  - Reescritura retroactiva de gastos al quitar miembro: descartado por ruptura contable.

## Decision 8: Relación con ProductCatalog

- **Decision**: Mantener compatibilidad por `ProductId` (catalogProductId) sin sincronización automática de precios ni consultas obligatorias de runtime.
- **Rationale**: Cumple alcance MVP-3 y evita acoplamiento innecesario entre contextos.
- **Alternatives considered**:
  - Resolver precio en vivo desde ProductCatalog al registrar gasto: descartado (no requerido y potencialmente inconsistente con precio real pagado).

## Decision 9: Objetivos no funcionales para planificación

- **Decision**: Definir objetivos iniciales para MVP-3:
  - p95 < 300 ms en operaciones CRUD de listas/gastos.
  - p95 < 500 ms en cálculo de settlement para listas con hasta 500 gastos.
  - Cero exposición de recursos de grupo a no miembros (respuestas de autorización uniformes).
- **Rationale**: Permite diseñar pruebas de aceptación técnicas y evita ambigüedad en la fase de tasks.
- **Alternatives considered**:
  - Dejar NFR abiertos: descartado por riesgo de criterios de done poco verificables.
