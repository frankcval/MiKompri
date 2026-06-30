# Data Model: Estructura del Monorepo MiKompri

**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Fecha**: 2026-06-30

> En este plan, el "modelo de datos" es la estructura del monorepo: la organización
> de directorios y proyectos que guiará el desarrollo continuo.

---

## Estado Actual vs Estado Objetivo

### Estado Actual (baseline)

```text
MiKompri/
├── MiKompri.sln
│
├── ── Bounded Context: ShoppingList ──────────────────
├── MiKompri.ShoppingList.Api/
├── MiKompri.ShoppingList.Application/
├── MiKompri.ShoppingList.Domain/
├── MiKompri.ShoppingList.Infrastructure/
│
├── ── Bounded Context: Users ─────────────────────────
├── MiKompri.Users.Api/
├── MiKompri.Users.Application/
├── MiKompri.Users.Domain/
├── MiKompri.Users.Infrastructure/
│
├── ── Tests ────────────────────────────────────────
├── test/
│   ├── MiKompri.ShoppingList.Api.Tests/
│   ├── MiKompri.ShoppingList.Application.Tests/
│   └── MiKompri.ShoppingList.Domain.Tests/
│
├── ── CI/CD ────────────────────────────────────────
├── .github/
│   ├── workflows/
│   │   ├── ci-mikompri-shoppinglist.yml
│   │   └── cd-mikompri-shoppinglist.yml
│   ├── agents/         # Spec Kit agents
│   └── prompts/        # Spec Kit prompts
│
├── ── Spec Kit ─────────────────────────────────────
├── .specify/           # Config y scripts de Spec Kit
├── specs/              # Especificaciones de features
│   └── 001-project-baseline/
│
├── ── Docker ───────────────────────────────────────
├── docker-compose.yml
├── docker-compose.override.yml
│
└── README.md
```

**Elementos faltantes** respecto al estado objetivo:
- `docs/adr/` — ADRs del proyecto (a crear con este plan)
- `infra/azure/` — Infraestructura como código para Azure (futuro MVP-1 CD)
- `MiKompri.Mobile/` — Proyecto MAUI Android (futuro MVP-3)
- `test/MiKompri.Users.*.Tests/` — Tests del bounded context Users (futuro MVP-1)

---

## Estado Objetivo (tras este plan y futuras iteraciones)

```text
MiKompri/
│
├── MiKompri.sln                       # Solución .NET — todos los proyectos
├── README.md
│
├── ── BOUNDED CONTEXT: ShoppingList ──────────────────────────────────────────
│   ├── MiKompri.ShoppingList.Api/            ✅ Operacional
│   │   ├── Controllers/                      # PurchaseListsController
│   │   ├── Middleware/                       # GlobalException, RequestLogging, HealthChecks
│   │   ├── Models/                           # Request DTOs
│   │   ├── Program.cs
│   │   └── Dockerfile                        # Multi-stage build (base / build / publish / final)
│   │
│   ├── MiKompri.ShoppingList.Application/    ✅ Operacional
│   │   ├── Commands/{Feature}/               # Command + Handler + Validator por carpeta
│   │   ├── Queries/{Feature}/                # Query + Handler por carpeta
│   │   ├── DTOs/
│   │   ├── Interfaces/                       # IPurchaseListRepository, IUnitOfWork
│   │   ├── Behavior/                         # LoggingBehavior, ValidationBehavior
│   │   └── DependencyInjection.cs
│   │
│   ├── MiKompri.ShoppingList.Domain/         ✅ Operacional
│   │   ├── Abstractions/                     # Entity, IAggregateRoot, ValueObject
│   │   ├── Entities/                         # PurchaseList (AggregateRoot), ListItem
│   │   └── ValueObjects/                     # ListProgress
│   │
│   └── MiKompri.ShoppingList.Infrastructure/ ✅ Operacional
│       ├── Persistence/
│       │   ├── Configurations/               # EF Core IEntityTypeConfiguration
│       │   ├── Repositories/                 # PurchaseListRepository, UnitOfWork
│       │   ├── Migrations/                   # ⚠ PENDIENTE: crear migraciones explícitas (ADR-002)
│       │   └── ShoppingListDbContext.cs
│       └── InfrastructureDependencyInjection.cs
│
├── ── BOUNDED CONTEXT: Users ─────────────────────────────────────────────────
│   ├── MiKompri.Users.Api/                   ⚠ Solo scaffolding (WeatherForecastController)
│   │   └── Dockerfile                        # ⚠ PENDIENTE: crear para MVP-1
│   │
│   ├── MiKompri.Users.Application/           🔴 Sin implementar
│   │   ├── Commands/{Feature}/               # A implementar en MVP-1
│   │   ├── Queries/{Feature}/
│   │   └── DependencyInjection.cs
│   │
│   ├── MiKompri.Users.Domain/                ✅ Definido
│   │   ├── Abstractions/
│   │   └── Users/                            # User, Group, GroupMembership, GroupRole
│   │
│   └── MiKompri.Users.Infrastructure/        ⚠ Parcial (sin migraciones)
│       └── Persistence/
│           ├── Configurations/
│           ├── Repositories/
│           ├── Migrations/                   # ⚠ PENDIENTE: crear para MVP-1
│           └── UsersDbContext.cs
│
├── ── CLIENTE MÓVIL ──────────────────────────────────────────────────────────
│   └── MiKompri.Mobile/                      🔴 PENDIENTE (MVP-3)
│       └── [.NET MAUI project structure]
│
├── ── TESTS ──────────────────────────────────────────────────────────────────
│   └── test/
│       ├── MiKompri.ShoppingList.Domain.Tests/        ✅
│       ├── MiKompri.ShoppingList.Application.Tests/   ✅
│       ├── MiKompri.ShoppingList.Api.Tests/            ✅
│       ├── MiKompri.Users.Domain.Tests/               🔴 PENDIENTE (MVP-1)
│       ├── MiKompri.Users.Application.Tests/          🔴 PENDIENTE (MVP-1)
│       └── MiKompri.Users.Api.Tests/                  🔴 PENDIENTE (MVP-1)
│
├── ── DOCUMENTACIÓN ──────────────────────────────────────────────────────────
│   └── docs/
│       └── adr/
│           ├── ADR-001-azure-deployment.md            ← Creado en este plan
│           ├── ADR-002-migrations-strategy.md         ← Creado en este plan
│           └── ADR-003-adr-format.md                  ← Creado en este plan
│
├── ── INFRAESTRUCTURA COMO CÓDIGO ────────────────────────────────────────────
│   └── infra/
│       └── azure/                                     🔴 PENDIENTE (MVP-1 CD)
│           ├── container-apps.bicep
│           └── postgres.bicep
│
├── ── SPECS Y SPEC KIT ──────────────────────────────────────────────────────
│   ├── specs/
│   │   ├── 001-project-baseline/          ← Este ciclo de spec
│   │   └── {NNN}-{feature}/               ← Futuras specs
│   └── .specify/
│       ├── memory/constitution.md
│       ├── templates/
│       └── scripts/
│
├── ── CI/CD ──────────────────────────────────────────────────────────────────
│   └── .github/
│       ├── workflows/
│       │   ├── ci-mikompri-shoppinglist.yml           ✅
│       │   ├── cd-mikompri-shoppinglist.yml           ✅ (solo GHCR, falta Azure deploy)
│       │   ├── ci-mikompri-users.yml                  🔴 PENDIENTE (MVP-1)
│       │   └── cd-mikompri-users.yml                  🔴 PENDIENTE (MVP-1)
│       ├── agents/                                    ✅ Spec Kit agents
│       └── prompts/                                   ✅ Spec Kit prompts
│
└── ── DOCKER ─────────────────────────────────────────────────────────────────
	├── docker-compose.yml                             ⚠ Solo ShoppingList + postgres
	└── docker-compose.override.yml                   ✅ Overrides locales

```

---

## Capas por Bounded Context (regla de nomenclatura)

Cada bounded context `{BC}` DEBE seguir esta estructura de proyectos:

| Proyecto | Responsabilidad | Dependencias |
|----------|----------------|--------------|
| `MiKompri.{BC}.Domain` | Entidades, value objects, interfaces de repositorios | Ninguna (independiente) |
| `MiKompri.{BC}.Application` | Commands, Queries, DTOs, Behaviors, Validators | → Domain |
| `MiKompri.{BC}.Infrastructure` | DbContext, Repositories, Migrations, servicios externos | → Domain, → Application |
| `MiKompri.{BC}.Api` | Controllers, Middleware, Models de request, Program.cs, Dockerfile | → Application |

**Regla de dependencias** (flecha → indica "puede referenciar"):
```
Api → Application → Domain
Infrastructure → Domain
Infrastructure → Application (solo para interfaces de repositorios)
```

**Prohibido**:
- `Domain.{BC1}` → cualquier proyecto de `{BC2}`
- `Application.{BC1}` → cualquier proyecto de `{BC2}`
- `Infrastructure.{BC1}` → `Infrastructure.{BC2}` (no acceso directo a BD cruzado)

---

## Directorios de Soporte: Propósito y Reglas

| Directorio | Propósito | Reglas |
|------------|-----------|--------|
| `docs/adr/` | Decisiones arquitecturales | Numeración secuencial ADR-{NNN}. Ver [contracts/adr-template.md](contracts/adr-template.md) |
| `infra/azure/` | Bicep/Terraform para Azure | Un archivo por recurso. Parametrizado por entorno |
| `specs/{NNN}-{name}/` | Especificaciones de features | Tres artefactos obligatorios: spec.md, plan.md, tasks.md |
| `test/` | Proyectos de test | Un proyecto por capa por bounded context. Naming: `{BC}.{Layer}.Tests` |
