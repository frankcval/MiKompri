# Implementation Plan: MVP 1 - Shopping List Core

**Branch**: `002-shopping-list-core` | **Date**: 2026-06-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-shopping-list-core/spec.md`

## Summary

Implementar el núcleo funcional de listas de compra para uso individual: crear/consultar listas, gestionar ítems (agregar, editar, marcar comprado, eliminar), calcular progreso y mantener trazabilidad básica de creación/modificación. La implementación se apoya en la arquitectura existente de `MiKompri.ShoppingList` (API + Application + Domain + Infrastructure), reforzando validaciones, reglas de negocio y cobertura de tests.

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

**Target Platform**: API REST en contenedor Linux

**Project Type**: Backend API por bounded context (ShoppingList)

**Performance Goals**:
- Operaciones CRUD de lista/ítems percibidas como inmediatas por el usuario en flujo normal
- Consulta de lista con progreso en una única lectura funcional

**Constraints**:
- Mantener arquitectura por capas y CQRS existente
- Sin login, invitaciones ni colaboración multiusuario en este MVP
- Sin introducir nuevos bounded contexts ni dependencias transversales

**Scale/Scope**:
- MVP para gestión individual de listas
- Alcance acotado a `MiKompri.ShoppingList.*` y sus tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Estado | Evidencia |
|-----------|--------|-----------|
| PP1 · Valor de Usuario Primero | ✅ PASS | El MVP entrega flujo usable end-to-end de lista de compra |
| PP2 · Autonomía de MVP | ✅ PASS | El alcance es independiente y desplegable sin funcionalidades futuras |
| PP3 · Listas de Compra como Núcleo | ✅ PASS | La feature implementa exactamente el núcleo del producto |
| PP4 · Transparencia Colaborativa | ✅ PASS | Se incluye trazabilidad básica de creación/modificación; colaboración multiusuario queda fuera de alcance |
| PP5 · Móvil Primero | ✅ PASS | Contratos REST simples y consumibles por cliente móvil |
| TP1 · Backend en .NET | ✅ PASS | Proyectos objetivo en `net8.0` |
| TP2 · Bounded Contexts | ✅ PASS | Cambios limitados a ShoppingList, sin acoplamiento cruzado |
| TP3 · Monorepo en GitHub | ✅ PASS | Artefactos y código dentro del monorepo |
| TP4 · Docker Obligatorio | ✅ PASS | No rompe flujo Docker/Compose existente |
| TP5 · Despliegue Objetivo en Azure | ✅ PASS | Sin cambios que bloqueen despliegue en Azure |
| TP6 · Cliente Android MAUI | ✅ PASS | Cliente completo fuera de alcance por definición de MVP |
| TP7 · APIs REST con OpenAPI | ✅ PASS | Endpoints REST alineados al contexto actual de ShoppingList |
| TP8 · Testing Obligatorio | ✅ PASS | Se planifican tests de dominio, aplicación e integración API |
| TP9 · ADR | ✅ PASS | Sin decisión arquitectónica disruptiva; se documenta en plan de feature |
| TP10 · Spec-First | ✅ PASS | Se parte de `spec.md` y este `plan.md` antes de implementación |

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
└── tasks.md            # Lo crea /speckit.tasks
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
1. Alcance funcional exacto del MVP 1.
2. Estrategia de trazabilidad básica sin auth real.
3. Manejo de duplicados y operaciones idempotentes.
4. Estrategia de validación y errores esperados.
5. Cobertura de tests por capa.

## Phase 1 — Design

- **Data model funcional**: [data-model.md](data-model.md)
- **Contratos funcionales**: [contracts/shoppinglist-core-api.md](contracts/shoppinglist-core-api.md)
- **Validación operativa**: [quickstart.md](quickstart.md)

## Phase 2 — Task Planning Approach

`/speckit.tasks` deberá generar tareas agrupadas por historia priorizada:
- P1: Crear/consultar lista + CRUD operativo de ítems
- P2: Progreso y trazabilidad básica
- Transversal: validaciones, errores esperados y cobertura de tests

## Complexity Tracking

No se registran excepciones constitucionales ni complejidad adicional a justificar en esta feature.
