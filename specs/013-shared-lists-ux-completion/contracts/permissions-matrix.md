# Contract — Permissions Matrix (013)

Fuente real: backend ShoppingList (`PurchaseAccessPolicy` y handlers shared/settlement) + membresía Users activa.

## Roles
- **Owner**
- **Admin**
- **Member**

## Matriz

| Acción | Owner | Admin | Member | Evidencia backend |
|---|---|---|---|---|
| Ver lista compartida | Sí | Sí | Sí | `GetSharedListById`, `GetSharedListsByGroup` validan membresía activa |
| Añadir producto | Sí | Sí | Sí | `AddSharedItem` requiere miembro activo |
| Editar producto | Sí (cualquiera) | Sí (cualquiera) | Solo ítems propios | `RequireList(... ownItem ...)` |
| Eliminar producto | Sí (cualquiera) | Sí (cualquiera) | Solo ítems propios y sin gastos | `RemoveSharedItem` + policy + conflicto por gastos |
| Marcar/desmarcar comprado | Sí | Sí | Sí | `SetPurchaseState` con `RequireList(write:true)` |
| Registrar gasto | Sí | Sí | Sí, con límites (pagador/comprador propio) | `RegisterItemExpense` |
| Editar gasto | Sí | Sí | No | `UpdateItemExpense` exige Owner/Admin |
| Eliminar gasto | Sí | Sí | No | `DeleteItemExpense` exige Owner/Admin |
| Renombrar lista | Sí | Sí | No | `UpdateSharedList` exige Owner/Admin |
| Cerrar lista | Sí | Sí | No | `CloseSharedList` exige Owner/Admin |
| Eliminar lista | Sí | Sí | No | `RemoveSharedList` exige manage + conflicto con gastos |

## Estados de lista

- Lista `Closed` bloquea escrituras por policy (`write && status != Active` -> conflicto).
- Consulta sigue permitida para miembros autorizados.

## Reglas de UI derivadas

1. UI no debe prometer acciones que backend negará de forma sistemática.
2. UI debe manejar 403 explícito aun cuando acción esté oculta/deshabilitada.
3. Para Member, edición/borrado de ítem depende también de autoría del ítem.
