# Contract — Shared Lists Navigation (013)

## Objetivo

Garantizar que cualquier entrada a listas compartidas converge al mismo recurso y estado URL.

## Rutas canónicas

- Índice compartido: `/shared-lists`
- Detalle compartido: `/shared-lists/:listId`

## Query params soportadas

### Índice
- `groupId=<guid>`: grupo seleccionado.
- `view=active|history`: filtro de vista.
- `sort=newest|oldest|updated|name`: orden.

### Detalle
- `groupId=<guid>`: contexto para retorno.
- `tab=shopping|expenses|people`: pestaña activa.

## Entrada esperada

- Desde Grupos: `/groups/:groupId` -> `/shared-lists?groupId=...` -> `/shared-lists/:listId?groupId=...`
- Desde Mis listas: `/lists` -> `/shared-lists/:listId?groupId=...`

## Reglas

1. `listId` identifica el recurso; `tab` solo cambia la vista del mismo recurso.
2. Back/forward no debe cambiar de recurso inadvertidamente.
3. Reload debe conservar `tab` y, cuando exista, `groupId`.
4. El destino "volver" prioriza `groupId`; fallback `/shared-lists`.
5. No se crean rutas paralelas tipo `/groups/:groupId/shared-lists/:listId` para evitar doble estado.
