# Implementation Plan: Shopping List Core Hardening

**Branch**: `002-shopping-list-core` | **Date**: 2026-06-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-shopping-list-core/spec.md`

**Status**: Ejecutado y cerrado funcionalmente; quedan pendientes sólo el cierre documental del gate Docker y una limpieza menor de wording.

## Summary

Esta feature endureció y completó el núcleo funcional ya existente de listas de compra para uso
individual: crear/consultar listas, gestionar ítems (agregar, editar, marcar comprado,
eliminar), calcular progreso y mantener trazabilidad básica de creación/modificación.
La implementación se apoyó en la arquitectura existente de `MiKompri.ShoppingList`
(API + Application + Domain + Infrastructure), reforzando validaciones, reglas de
negocio, contratos de error y cobertura de tests.

## Technical Context

**Language/Version**: C# / .NET 8 (`net8.0`)

**Primary Dependencies**:
- ASP.NET Core Web API
- MediatR
- FluentValidation
- Entity Framework Core + Npgsql
- Serilog

**Storage**: PostgreSQL (runtime) + EF Core InMemory (tests de integración)

**Testing**:
- xUnit
- FluentAssertions
- Moq
- WebApplicationFactory
- Validación final ejecutada sobre Domain, Application y API tests de ShoppingList

**Target Platform**: API REST en contenedor Linux

**Project Type**: Backend API por bounded context (ShoppingList)

**Performance Goals**:
- La consulta de detalle de lista concentra datos principales, ítems, progreso y trazabilidad en una única respuesta funcional
- Las mutaciones de lista e ítems quedan reflejadas en la lectura posterior inmediata del recurso consultado

**Constraints**:
- Mantener arquitectura por capas y CQRS existente
- Sin login, invitaciones ni colaboración multiusuario en esta fase
- Sin introducir nuevos bounded contexts ni dependencias transversales

**Scale/Scope**:
- Fase de hardening para gestión individual de listas
- Alcance acotado a `MiKompri.ShoppingList.*` y sus tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Estado | Evidencia |
|-----------|--------|-----------|
| PP1 · Valor de Usuario Primero | ✅ PASS | La feature mejora un flujo usable existente y corrige huecos del core de valor |
| PP2 · Autonomía de MVP | ✅ PASS | El alcance es independiente y desplegable sin depender de Users/Auth |
| PP3 · Listas de Compra como Núcleo | ✅ PASS | La feature fortalece exactamente el núcleo del producto |
| PP4 · Transparencia Colaborativa | ✅ PASS | Se incluye trazabilidad básica de creación/modificación; colaboración multiusuario queda fuera de alcance |
| PP5 · Móvil Primero | ✅ PASS | Contratos REST simples y consumibles por cliente móvil |
| TP1 · Backend en .NET | ✅ PASS | Proyectos objetivo en `net8.0` |
| TP2 · Bounded Contexts | ✅ PASS | Cambios limitados a ShoppingList, sin acoplamiento cruzado |
| TP3 · Monorepo en GitHub | ✅ PASS | Artefactos y código dentro del monorepo |
| TP4 · Docker Obligatorio | ⚠️ NOTE | La feature no modifica Docker/Compose ni Dockerfiles; la validación ejecutada de esta spec quedó registrada en `quickstart.md` con build/tests .NET y el cumplimiento Docker sigue heredado del baseline y del pipeline compartido |
| TP5 · Despliegue Objetivo en Azure | ⚠️ NOTE | Esta feature no empeora el estado actual, pero el gate repo-level sigue abierto en `001-project-baseline` hasta implementar deploy efectivo a Azure |
| TP6 · Cliente Android MAUI | ✅ PASS | Cliente completo fuera de alcance por definición de MVP |
| TP7 · APIs REST con OpenAPI | ✅ PASS | Endpoints REST alineados al contexto actual de ShoppingList |
| TP8 · Testing Obligatorio | ✅ PASS | Se planificaron y ejecutaron tests de dominio, aplicación e integración API |
| TP9 · ADR | ✅ PASS | Sin decisión arquitectónica disruptiva; se documenta en plan de feature |
| TP10 · Spec-First | ✅ PASS | La implementación se ejecutó a partir de `spec.md`, `plan.md` y `tasks.md` |

## Project Structure

### Documentation (this feature)

```text
specs/002-shopping-list-core/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── shoppinglist-core-api.md
└── tasks.md
```

### Source Code (repository root)

```text
MiKompri.ShoppingList.Api/
├── Controllers/
├── Models/
├── Middleware/
└── Program.cs

MiKompri.ShoppingList.Application/
├── Commands/
├── Queries/
├── Behavior/
├── DTOs/
└── Interfaces/

MiKompri.ShoppingList.Domain/
├── Entities/
├── ValueObjects/
└── Abstractions/

MiKompri.ShoppingList.Infrastructure/
└── Persistence/
   ├── ShoppingListDbContext.cs
   ├── Repositories/
   └── Configurations/

test/
├── MiKompri.ShoppingList.Domain.Tests/
├── MiKompri.ShoppingList.Application.Tests/
└── MiKompri.ShoppingList.Api.Tests/
```

**Structure Decision**: Reutilizar la estructura vertical existente del bounded context `ShoppingList` sin crear nuevos proyectos.

## Phase 0 — Research

Ver [research.md](research.md). Decisiones cerradas para:
1. Alcance funcional exacto de la fase de hardening del core.
2. Estrategia de trazabilidad básica sin auth real.
3. Manejo de duplicados y operaciones idempotentes.
4. Estrategia de validación y errores esperados.
5. Cobertura de tests por capa.

## Phase 1 — Design

- **Data model funcional**: [data-model.md](data-model.md)
- **Contratos funcionales**: [contracts/shoppinglist-core-api.md](contracts/shoppinglist-core-api.md)
- **Validación operativa**: [quickstart.md](quickstart.md)

## Phase 2 — Task Planning Approach

`tasks.md` quedó organizado y ejecutado por historia priorizada:
- P1: Crear/consultar lista + CRUD operativo de ítems sobre el core existente
- P2: Progreso y trazabilidad básica
- Transversal: validaciones, errores esperados, evidencia de cierre y cobertura de tests

## Complexity Tracking

No se registran excepciones constitucionales de diseño en esta feature. Se mantiene como
dependencia conocida el gate repo-level de TP5 documentado en `001-project-baseline`.
## Cierre actual de la feature

- ✅ US1 completada: crear y consultar listas.
- ✅ US2 completada: ciclo completo de ítems.
- ✅ US3 completada: progreso de lista.
- ✅ US4 completada: trazabilidad básica.

## Pendientes exclusivamente documentales

- Cierre de la ejecución completa del gate Docker documentado en `evidence/docker-gate.md`.
- Limpieza menor de wording en artefactos de cierre.
