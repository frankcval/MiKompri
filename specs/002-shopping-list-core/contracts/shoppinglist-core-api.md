# Contract: Shopping List Core API (MVP 1)

**Spec**: [../spec.md](../spec.md) | **Plan**: [../plan.md](../plan.md)

> Contrato funcional de alto nivel para guiar implementación y pruebas del MVP 1.

## Recursos

- `ShoppingList`
- `ShoppingListItem`

## Operaciones del MVP

### 1. Crear lista
- **Resultado esperado**: lista creada con identificador y trazabilidad de creación.
- **Errores esperados**: nombre inválido.

### 2. Consultar lista
- **Resultado esperado**: datos de lista + ítems + progreso + trazabilidad.
- **Errores esperados**: lista no encontrada.

### 3. Agregar ítem
- **Resultado esperado**: ítem agregado en estado pendiente.
- **Errores esperados**: lista no encontrada, ítem duplicado, datos inválidos.

### 4. Editar ítem
- **Resultado esperado**: ítem actualizado y trazabilidad de modificación actualizada.
- **Errores esperados**: lista/ítem no encontrado, datos inválidos.

### 5. Marcar ítem como comprado
- **Resultado esperado**: estado del ítem en comprado (idempotente).
- **Errores esperados**: lista/ítem no encontrado.

### 6. Eliminar ítem
- **Resultado esperado**: ítem removido de la lista y progreso recalculado.
- **Errores esperados**: lista/ítem no encontrado.

### 7. Consultar progreso
- **Resultado esperado**: total/comprados/pendientes/porcentaje consistentes.
- **Regla especial**: lista vacía retorna 0%.

## Mapeo de errores funcionales

- `ERR-001`: validación de nombre de lista.
- `ERR-002`: lista no encontrada.
- `ERR-003`: ítem no encontrado.
- `ERR-004`: ítem duplicado por producto en la misma lista.
- `ERR-005`: datos inválidos en alta/edición.
