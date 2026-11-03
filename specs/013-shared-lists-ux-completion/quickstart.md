# Quickstart — 013 Shared Lists UX Completion

## Objetivo de esta guía

Ejecutar y validar el trabajo de 013 sin invadir 012 ni alterar reglas de negocio previas.

## 1) Preparación

Desde raíz:

```powershell
git checkout main
git pull --ff-only origin main
```

Verificar spec-kit/documentación requerida:

```powershell
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
git diff --check
```

## 2) Foco funcional a auditar/validar

- `/shared-lists`
- `/shared-lists/:listId?tab=shopping|expenses|people`
- Entrada desde `/groups/:groupId` → `/shared-lists?groupId=...`
- Entrada desde `/lists` (tarjetas compartidas) → `/shared-lists/:listId?...`

## 3) Ejecución de pruebas Web recomendadas

Desde `MiKompri.Web`:

```powershell
npm ci
npm run test:component
npm run test:e2e:smoke -- --grep "shared|groups|shopping|settlement"
npm run lint
npm run typecheck
npm run build
```

## 4) Matriz mínima de validación manual/automatizada

- Listado: vacío / una / múltiples / distintos grupos / error parcial.
- Creación: sin grupos / grupo único / múltiples / cancelar / error 403.
- Compra: añadir / editar / eliminar / marcar / desmarcar / 100 ítems / unknown prices.
- Gastos: vacío / con datos / permiso denegado / error de servicio.
- Personas: owner/admin/member y estados vacíos/error.
- Navegación: deep-link, reload, back/forward y dos orígenes.
- Responsive: 320/390/430/768/1280.

## 5) Restricciones de alcance

No implementar en 013:
- invitaciones;
- aceptación/rechazo;
- links o emails de invitación;
- administración de membresía/roles de grupo;
- cambios de trust/auth global (010);
- reglas de settlement nuevas (005).

## 6) Cierre documental esperado

Actualizar:
- `evidence/` con resultados y límites;
- `checklists/requirements.md`;
- `checklists/acceptance.md`.

El cierre debe declarar explícitamente qué quedó fuera por pertenecer a 012.
