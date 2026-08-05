# Implementation Plan: Product Catalog (MVP-2)

**Branch**: `main` | **Date**: 2026-08-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-product-catalog/spec.md`

## Summary

Diseñar e implementar un bounded context independiente `ProductCatalog` en .NET 8 con Clean Architecture, DDD y CQRS para gestionar productos reutilizables, mercados y precios históricos por mercado. Se mantiene el patrón arquitectónico actual del repositorio (`Api`, `Application`, `Domain`, `Infrastructure`, tests por capa), sin refactorizar `ShoppingList` ni `Users`, y sin salir del alcance de MVP-2.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (`net8.0`)

**Primary Dependencies**: ASP.NET Core Web API, MediatR 12, FluentValidation 12, EF Core 9 + Npgsql, Serilog

**Storage**: PostgreSQL 15 en base dedicada `MiKompri_ProductCatalog`

**Testing**: xUnit, FluentAssertions, Moq, WebApplicationFactory + EF InMemory para pruebas de API

**Target Platform**: Linux containers (Docker / Docker Compose)

**Project Type**: Bounded context backend independiente dentro del monorepo

**Performance Goals**: p95 < 250 ms en operaciones CRUD; p95 < 400 ms en consulta histórica con filtros

**Constraints**: No modificar `ShoppingList`/`Users`; fuera de alcance: shared lists, OCR, promociones, scraping, recomendaciones, cliente MAUI e integración completa entre contextos

**Scale/Scope**: MVP-2: catálogo de productos, mercados, precios por mercado, historial y contrato de asociación futura con listas

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

| Principio | Estado | Evidencia |
|-----------|--------|-----------|
| PP1 Valor de Usuario Primero | ✅ PASS | Flujo central: registrar y consultar precios reales de compra |
| PP2 Autonomía de MVP | ✅ PASS | MVP-2 desplegable sin dependencias funcionales de MVP-3+ |
| PP3 Núcleo ShoppingList | ✅ PASS | No se refactoriza ShoppingList; solo se prepara `catalogProductId` estable |
| TP1 Backend .NET 8 | ✅ PASS | Nuevos proyectos previstos en `net8.0` |
| TP2 Bounded Contexts | ✅ PASS | `ProductCatalog` aislado, sin acceso DB cruzado |
| TP3 Monorepo | ✅ PASS | Todo dentro de `frankcval/MiKompri` |
| TP4 Docker obligatorio | ✅ PASS | Se planifica Dockerfile + compose + validación |
| TP5 Azure objetivo | ⚠ NOTE | Sin cambios de infraestructura de despliegue en esta feature |
| TP7 REST + OpenAPI | ✅ PASS | Contrato API v1 documentado |
| TP8 Testing obligatorio | ✅ PASS | Plan de tests Domain/Application/API |
| TP9 Decisiones documentadas | ✅ PASS | Decisiones en `research.md` y este `plan.md` |
| TP10 Spec-first | ✅ PASS | Diseño parte de `spec.md` antes de implementación |

**Gate pre-diseño**: ✅ Aprobado.

## Project Structure

### Documentation (this feature)

```text
specs/004-product-catalog/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── product-catalog-api.md
└── tasks.md
```

### Source Code (repository root)

```text
MiKompri.ProductCatalog.Api/
├── Controllers/
│   ├── CatalogProductsController.cs
│   ├── MarketsController.cs
│   └── ProductPricesController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Extensions/
│   └── HealthChecksExtensions.cs
├── Program.cs
└── Dockerfile

MiKompri.ProductCatalog.Application/
├── DependencyInjection.cs
├── Behavior/
├── Commands/
│   ├── CreateCatalogProduct/
│   ├── UpdateCatalogProduct/
│   ├── DeactivateCatalogProduct/
│   ├── CreateMarket/
│   ├── UpdateMarket/
│   ├── DeactivateMarket/
│   └── RegisterProductPrice/
├── Queries/
│   ├── GetCatalogProducts/
│   ├── GetCatalogProductById/
│   ├── GetMarkets/
│   └── GetProductPriceHistory/
└── Dtos/

MiKompri.ProductCatalog.Domain/
├── Abstractions/
│   └── Entity.cs
├── Products/
│   ├── CatalogProduct.cs
│   ├── ProductPriceRecord.cs
│   └── ValueObjects/
│       ├── ProductName.cs
│       └── Money.cs
└── Markets/
    └── Market.cs

MiKompri.ProductCatalog.Infrastructure/
├── DependencyInjection.cs
├── Persistence/
│   ├── ProductCatalogDbContext.cs
│   ├── Configurations/
│   ├── Repositories/
│   ├── UnitOfWork.cs
│   └── Migrations/
└── Services/
    └── SystemDateTimeProvider.cs

test/
├── MiKompri.ProductCatalog.Domain.Tests/
├── MiKompri.ProductCatalog.Application.Tests/
└── MiKompri.ProductCatalog.Api.Tests/
```

**Structure Decision**: Se crea un nuevo bounded context completo, replicando convenciones actuales del repositorio y preservando aislamiento entre contextos.

## Phase 0 — Research

Ver [research.md](./research.md). Se resolvieron decisiones de unicidad, modelo histórico, auditoría mínima, estrategia de integración futura y pruebas.

## Phase 1 — Design & Contracts

- Modelo de dominio y reglas: [data-model.md](./data-model.md)
- Contratos externos: [contracts/product-catalog-api.md](./contracts/product-catalog-api.md)
- Validación E2E: [quickstart.md](./quickstart.md)

### Agent Context Update

Se intentó ejecutar script de actualización de contexto del agente.
Resultado: `NO_AGENT_CONTEXT_SCRIPT` (no existe `.specify/scripts/powershell/update-agent-context.ps1`).

## Constitution Check (Post-Design)

| Principio | Estado | Validación |
|-----------|--------|------------|
| PP1 | ✅ PASS | Diseño centrado en valor de compra real |
| PP2 | ✅ PASS | MVP-2 autónomo |
| PP3 | ✅ PASS | Sin cambios en ShoppingList |
| TP1 | ✅ PASS | Stack .NET 8 |
| TP2 | ✅ PASS | Contexto ProductCatalog independiente |
| TP4 | ✅ PASS | Flujo Docker definido |
| TP7 | ✅ PASS | Contrato REST v1 definido |
| TP8 | ✅ PASS | Estrategia de pruebas por capas |
| TP9 | ✅ PASS | Decisiones documentadas |
| TP10 | ✅ PASS | Flujo spec-first respetado |

**Gate post-diseño**: ✅ Aprobado.

## Complexity Tracking

No hay excepciones constitucionales que justificar en esta feature.