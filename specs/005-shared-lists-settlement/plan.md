# Implementation Plan: MVP-3 Shared Lists & Settlement

**Branch**: `main` | **Date**: 2026-08-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-shared-lists-settlement/spec.md`

## Summary

Evolucionar el bounded context `ShoppingList` para soportar listas compartidas por `GroupId`, autorización por membresía/rol (Owner/Admin/Member), trazabilidad de colaboración (`AddedBy`, operaciones relevantes), registro de gasto real por ítem, cálculo de reparto y balances, y propuesta de liquidación simplificada determinista. Se mantiene separación estricta entre bounded contexts: `Users` continúa como source of truth de identidad, grupos, membresías y roles, sin acceso directo a su base de datos.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (`net8.0`)

**Primary Dependencies**: ASP.NET Core Web API, MediatR 12, FluentValidation 12, EF Core 9 + Npgsql, Serilog, JWT Bearer authentication

**Storage**: PostgreSQL para `ShoppingList` con nuevas estructuras para gastos, participantes y auditoría en el mismo bounded context

**Testing**: xUnit, FluentAssertions, Moq, WebApplicationFactory + EF InMemory para API tests

**Target Platform**: Linux containers (Docker / Docker Compose)

**Project Type**: Bounded context backend (`ShoppingList`) dentro de monorepo modular

**Performance Goals**:
- p95 < 300 ms en CRUD de listas compartidas, ítems y gastos
- p95 < 500 ms en cálculo de settlement para listas con hasta 500 gastos

**Constraints**:
- Sin acceso directo a BD de Users (TP2 + FR-019/FR-020)
- JWT emitido por proveedor OIDC externo
- `PaidBy` obligatorio, `PurchasedBy` opcional
- Mínimo 1 participante activo por gasto
- Liquidación determinista con conservación monetaria
- Compatibilidad con `ProductId` de ProductCatalog sin sincronización automática de precios

**Scale/Scope**:
- MVP-3 enfocado en colaboración real de grupos pequeños (familias/hogares)
- Listas personales existentes deben seguir funcionando sin regresiones

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

| Principio | Estado | Evidencia |
|-----------|--------|-----------|
| PP1 Valor de usuario primero | ✅ PASS | Historias priorizadas centradas en colaboración real + reparto claro |
| PP2 Autonomía de MVP | ✅ PASS | MVP-3 aporta valor desplegable sin depender de MVP-4+ |
| PP3 Núcleo ShoppingList | ✅ PASS | Se evoluciona ShoppingList sin romper listas personales |
| PP4 Transparencia colaborativa | ✅ PASS | `AddedBy` + auditoría de operaciones relevantes |
| TP1 Backend .NET 8 | ✅ PASS | Stack actual del repo |
| TP2 Bounded contexts | ✅ PASS | Integración con Users por contrato, sin DB cruzada |
| TP3 Monorepo | ✅ PASS | Artefactos dentro de `frankcval/MiKompri` |
| TP4 Docker obligatorio | ✅ PASS | Validación planificada sobre compose + tests |
| TP5 Azure objetivo | ⚠ NOTE | Sin cambio de plataforma en esta feature; compatibilidad se mantiene |
| TP7 REST + OpenAPI | ✅ PASS | Contrato API v1 documentado |
| TP8 Testing obligatorio | ✅ PASS | Estrategia de pruebas por capas incluida en tasks.md |
| TP9 Decisiones documentadas | ✅ PASS | Decisiones en `research.md` + este plan |
| TP10 Spec-first | ✅ PASS | `spec.md` + clarify completados antes de plan |

**Gate pre-diseño**: ✅ Aprobado.

## Project Structure

### Documentation (this feature)

```text
specs/005-shared-lists-settlement/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── shared-lists-settlement-api.md
└── tasks.md
```

### Source Code (repository root)

```text
MiKompri.ShoppingList.Api/
├── Controllers/
├── Middleware/
├── Models/
├── Services/
└── Program.cs

MiKompri.ShoppingList.Application/
├── Abstractions/
├── Behavior/
├── Commands/
├── DTOs/
├── Interfaces/
└── Queries/

MiKompri.ShoppingList.Domain/
├── Abtractions/
├── Entities/
├── Services/
└── ValueObjects/

MiKompri.ShoppingList.Infrastructure/
├── Persistence/
│   ├── ShoppingListDbContext.cs
│   ├── Configurations/
│   │   └── SharedLists/
│   ├── Repositories/
│   └── Migrations/
└── Services/

MiKompri.Users.Application/
└── (solo contrato de integración consumido; sin acceso a infraestructura de Users)

test/
├── MiKompri.ShoppingList.Domain.Tests/
├── MiKompri.ShoppingList.Application.Tests/
└── MiKompri.ShoppingList.Api.Tests/
```

**Structure Decision**: Se implementa íntegramente sobre `ShoppingList.*` reutilizando patrones existentes CQRS + aggregate + UoW. La verificación de membresía/rol se diseña como dependencia de aplicación por contrato, preservando ownership de `Users`.

## AuthN/AuthZ Runtime Plan

- `MiKompri.ShoppingList.Api/Program.cs` configurará JWT Bearer con el mismo tipo de proveedor OIDC externo que ya usa `Users`.
- `MiKompri.ShoppingList.Api/Services/HttpCurrentUserService.cs` leerá la identidad del claim `sub` y expondrá el `UserId` del caller.
- `MiKompri.ShoppingList.Application/Interfaces/IGroupAuthorizationService.cs` será el puerto para consultar membresía/rol sin acoplar `ShoppingList` a la base de datos de `Users`.
- `MiKompri.ShoppingList.Infrastructure/Services/UsersGroupAuthorizationAdapter.cs` implementará ese puerto mediante el mecanismo de integración acordado en research/clarify.
- Las operaciones compartidas usarán ese contrato para responder `401` cuando no exista identidad válida y `403` cuando exista identidad pero falte membresía o permiso.

## Phase 0 — Research

Resultado consolidado en [research.md](./research.md):

1. Matriz de permisos validada (Opción B).
2. Reglas exactas de gasto/reparto (`PaidBy` obligatorio, pagador puede no participar, mínimo 1 participante).
3. Algoritmo de liquidación determinista con desempate por `UserId`.
4. Estrategia de integración con Users por contrato sin DB directa.
5. Estrategia de histórico para bajas de miembros.
6. Compatibilidad con ProductCatalog por identificador estable.

No quedan marcadores `NEEDS CLARIFICATION` en el diseño funcional.

## Phase 1 — Design & Contracts

- Modelo de dominio y relaciones: [data-model.md](./data-model.md)
- Contrato externo de API: [contracts/shared-lists-settlement-api.md](./contracts/shared-lists-settlement-api.md)
- Guía de validación E2E: [quickstart.md](./quickstart.md)

### Agent Context Update

Se intentó ejecutar script de actualización de contexto del agente.
Resultado: `NO_AGENT_CONTEXT_SCRIPT` (no existe `.specify/scripts/powershell/update-agent-context.ps1`).

## Constitution Check (Post-Design)

| Principio | Estado | Validación |
|-----------|--------|------------|
| PP1 | ✅ PASS | Diseño centrado en colaboración y reparto entendible |
| PP2 | ✅ PASS | MVP-3 entregable como incremento funcional autónomo |
| PP3 | ✅ PASS | Core de listas se extiende sin sustituir listas personales |
| PP4 | ✅ PASS | `AddedBy` + eventos auditables definidos en modelo/contrato |
| TP1 | ✅ PASS | .NET 8 mantenido |
| TP2 | ✅ PASS | Contrato de integración con Users, sin acoplamiento de datos |
| TP4 | ✅ PASS | Validación por compose y tests prevista en quickstart |
| TP7 | ✅ PASS | Contrato REST v1 definido |
| TP8 | ✅ PASS | Estrategia de pruebas por capas definida en tasks.md |
| TP9 | ✅ PASS | Decisiones documentadas en research + plan |
| TP10 | ✅ PASS | Flujo spec-first respetado |

**Gate post-diseño**: ✅ Aprobado.

## Complexity Tracking

No hay excepciones constitucionales que justificar en esta feature.
