# Research: Shopping List Core Hardening

**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Fecha**: 2026-06-30

## Decisión 1 — Alcance funcional de la fase de hardening

**Decision**: Incluir solo operaciones core de lista de compra individual: crear/consultar lista, agregar/editar/marcar/eliminar ítems, progreso y trazabilidad básica.

**Rationale**: Coincide con el valor mínimo usable definido en spec y evita dependencia de módulos no maduros.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Incluir login/usuarios en esta fase | Aumenta alcance y rompe enfoque en núcleo |
| Incluir colaboración multiusuario | Requiere reglas de permisos y trazabilidad avanzada no prioritaria |

## Decisión 2 — Trazabilidad básica sin autenticación real

**Decision**: Mantener trazabilidad temporal mínima (creación y última modificación) en listas e ítems dentro de esta fase.

**Rationale**: Cumple necesidad operativa actual y mantiene coherencia con el alcance sin registro/login.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Auditoría histórica completa | Sobredimensionado para esta fase |
| Sin trazabilidad | Incumple requisito funcional FR-008 |

## Decisión 3 — Duplicados e idempotencia

**Decision**: Impedir ítems duplicados por producto en la misma lista y tratar el marcado como comprado como operación idempotente.

**Rationale**: Reduce inconsistencias y evita efectos secundarios en reintentos.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Permitir duplicados y consolidar luego | Complejiza UX y cálculo de progreso |
| Marcar comprado como error si ya comprado | Penaliza reintentos legítimos |

## Decisión 4 — Errores esperados y validación

**Decision**: Estandarizar errores funcionales del MVP (`ERR-001` a `ERR-005`) con mensajes claros y validaciones previas a persistencia.

**Rationale**: Facilita pruebas, consistencia de API y experiencia de consumo.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Errores genéricos sin tipología | Difícil de probar y operar |
| Validación parcial solo en capa API | Riesgo de reglas duplicadas/inconsistentes |

## Decisión 5 — Estrategia de pruebas

**Decision**: Cubrir MVP en tres niveles: dominio (reglas), aplicación (casos de uso), integración API (flujos).

**Rationale**: Está alineado con constitución TP8 y estructura actual del repositorio.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Solo pruebas de integración | Diagnóstico lento y frágil |
| Solo unit tests | No valida contratos HTTP ni composición real |
