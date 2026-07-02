# ADR-002 — Estrategia de Migraciones EF Core

**Estado**: Aceptado
**Fecha**: 2026-06-30
**Spec relacionada**: [specs/001-project-baseline/plan.md](../../specs/001-project-baseline/plan.md)

## Contexto

No existe carpeta `Migrations/` en `MiKompri.ShoppingList.Infrastructure` ni en
`MiKompri.Users.Infrastructure`. El `Dockerfile` actual no incluye ningún paso de
aplicación de migraciones.

Se asume que la base de datos se crea actualmente mediante `EnsureCreated()` en algún
punto del startup o manualmente. Esto no es sostenible para producción porque:

1. `EnsureCreated()` no aplica migraciones incrementales; si el schema cambia,
   es necesario destruir y recrear la BD (pérdida de datos).
2. Sin migraciones versionadas en el repo, los cambios de esquema no son auditables
   ni reproducibles entre entornos.
3. El pipeline CD no puede aplicar cambios de schema de forma controlada antes del deploy.

## Decisión

Adoptar **migraciones EF Core explícitas** versionadas en el repositorio con la siguiente
estrategia por entorno:

**Desarrollo local**: `Migrate()` en el startup de la API (dentro de `Program.cs`),
condicionado a desarrollo o a una variable de entorno `AUTO_MIGRATE=true`. Esto aplica
migraciones pendientes automáticamente al levantar el contenedor.

**Producción / CI-CD**: Step dedicado en el pipeline CD que ejecuta las migraciones
**antes** de desplegar la nueva versión de la API. Esto evita race conditions en
despliegues multi-instancia y permite fallar el pipeline si la migración falla antes
de que el tráfico llegue a la nueva versión.

## Alternativas Consideradas

| Opción | Ventajas | Desventajas |
|--------|----------|-------------|
| **Migrate() en startup + step CD** (elegida) | Desarrollo simple (automático), producción segura (step explícito) | Requiere implementar `--migrate-only` flag o lógica condicional en startup |
| Solo `EnsureCreated()` | Trivial de implementar | No aplica migraciones; destruye datos en cambios. Descartado para producción |
| Solo `Migrate()` en startup (sin step CD) | Automático en todos los entornos | Race condition en despliegues multi-instancia; no hay visibilidad en el pipeline |
| Script SQL manual | Control total del SQL | Rompe DX de EF Core; requiere sincronización manual; no compatible con el ciclo de Spec Kit |
| `efbundle` standalone | Sin SDK en la imagen de producción | Añade complejidad al pipeline; diferir hasta que la imagen de SDK efímera no sea viable |

## Consecuencias

### Positivas
- Los cambios de schema quedan auditados en el control de versiones junto con el código.
- El pipeline CD puede detectar y fallar ante migraciones problemáticas antes del despliegue.
- En desarrollo, el entorno se auto-configura sin pasos manuales.

### Negativas / Trade-offs
- Requiere crear las migraciones iniciales (retroactivas) para el schema actual de
  ShoppingList y Users.
- El step de migración en CD necesita acceso a la BD de producción, lo que implica
  configurar las credenciales en el pipeline.

### Tareas derivadas
- [ ] Crear migración inicial de ShoppingList: `dotnet ef migrations add InitialCreate ...`
- [ ] Crear migración inicial de Users (cuando se implemente la Application layer).
- [ ] Añadir lógica de `Migrate()` condicional en `Program.cs` de ShoppingList API.
- [ ] Añadir step de migración en `cd-mikompri-shoppinglist.yml` antes del deploy.
- [ ] Documentar los comandos de migración en `specs/001-project-baseline/quickstart.md`.
