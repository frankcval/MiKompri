# Implementation Plan: Baseline del Proyecto MiKompri

**Branch**: `001-project-baseline` | **Date**: 2026-06-30 | **Spec**: [spec.md](spec.md)

> Este plan no genera código nuevo. Documenta la arquitectura técnica de referencia,
> las estrategias operativas y las convenciones que guiarán el desarrollo continuo de
> MiKompri con Spec-Driven Development.

---

## Summary

MiKompri es una plataforma de listas de compra colaborativas construida con .NET 8 en
un monorepo organizado por bounded contexts. El bounded context `ShoppingList` es
operacional. El bounded context `Users` tiene el dominio definido pero carece de capa
de aplicación y API.

Este plan define la estructura objetivo del monorepo, las estrategias de Docker,
migraciones, testing, CI/CD y despliegue en Azure, y las convenciones de REST/OpenAPI,
ADR y Spec Kit. Tres decisiones arquitecturales abiertas (D-1, D-2, D-3) se resuelven
en [research.md](research.md) y se formalizan como ADR-001, ADR-002 y ADR-003.

---

## Technical Context

**Language/Version**: .NET 8 (TFM `net8.0` en todos los proyectos backend)

**Primary Dependencies**:
- ASP.NET Core Web API — framework HTTP y middleware
- Entity Framework Core 9 + Npgsql 9 — ORM + driver PostgreSQL
- MediatR 12 — CQRS y patrón Mediator
- FluentValidation 12 — validación de Commands y Queries via pipeline behavior
- Serilog 10 — logging estructurado (console + archivo, configurado desde `appsettings.json`)

**Storage**: PostgreSQL 15 (producción y desarrollo local via Docker), EF Core InMemory
(tests de integración de API únicamente)

**Testing**: xUnit + Moq + FluentAssertions + WebApplicationFactory + Coverlet

**Target Platform**: Docker containers (Linux/AMD64) → Azure Container Apps

**Project Type**: Dos APIs REST (multi-bounded-context) + cliente móvil Android futuro (.NET MAUI)

**Performance Goals**: Sub-200ms p95 para operaciones core de ShoppingList (MVP-0).
Sin SLOs de carga definidos hasta MVP-2 cuando existan usuarios reales.

**Constraints**:
- Docker-first: toda validación local y en CI pasa por contenedores (TP4)
- Azure-target: infraestructura diseñada para Azure Container Apps (TP5)
- Spec-first: ninguna feature sin spec.md + plan.md + tasks.md (TP10)
- Sin referencias de dominio cruzadas entre bounded contexts (TP2)

**Scale/Scope**: Grupos de hasta ~50 usuarios, ~100 listas por grupo (objetivo MVP-2).
Sin planificación de escala horizontal hasta validar tracción de producto.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Evaluación Pre-Diseño

| Principio | Estado | Evidencia |
|-----------|--------|-----------|
| PP1 · Valor de Usuario Primero | ✅ PASS | ShoppingList API operacional. MVP-0 entregado |
| PP2 · Autonomía de MVP | ✅ PASS | Cada MVP tiene alcance y criterio de done en spec.md |
| PP3 · Listas de Compra como Núcleo | ✅ PASS | ShoppingList es el primer bounded context desplegado |
| PP4 · Transparencia Colaborativa | ⚠️ WARN | `AddedBy` ausente en `ListItem` (DT-004). No bloquea el plan; bloquea MVP-2 |
| PP5 · Móvil Primero | ⚠️ NOTE | MAUI planificado para MVP-3. APIs REST ya son consumibles desde móvil |
| TP1 · Backend en .NET | ✅ PASS | Todos los proyectos usan `net8.0` |
| TP2 · Bounded Contexts | ✅ PASS | Sin referencias de dominio entre ShoppingList y Users |
| TP3 · Monorepo en GitHub | ✅ PASS | `frankcval/MiKompri`, todo en un repositorio |
| TP4 · Docker Obligatorio | ✅ PASS | Dockerfile y Docker Compose presentes; CI ejecuta tests |
| TP5 · Despliegue en Azure | ⚠️ WARN | CD actual solo publica a GHCR. Resuelto en ADR-001 |
| TP6 · MAUI Android | ⚠️ NOTE | MVP-3. Sin violación activa; no hay proyecto MAUI aún |
| TP7 · OpenAPI/Swagger | ✅ PASS | Swagger disponible en Development para ShoppingList API |
| TP8 · Testing Obligatorio | ✅ PASS | Tres niveles de test en CI con cobertura Coverlet |
| TP9 · ADR | ⚠️ WARN | Primer ADR creado en este plan (ADR-001, 002, 003). Retroactivo |
| TP10 · Spec-First | ✅ PASS | Este es el primer ciclo spec completo del proyecto |

**Veredicto**: ✅ Sin gates de ERROR. Tres WARNs activos, todos gestionados en este plan.

### Evaluación Post-Diseño

Los artefactos generados (research.md, data-model.md, contracts/, quickstart.md)
no introducen nuevas violaciones. Los WARNs se resuelven mediante:
- **PP4 / DT-004**: Registrado en tasks.md; obligatorio antes de MVP-2.
- **TP5**: ADR-001 define Azure Container Apps. Implementación en el CD de MVP-1.
- **TP9**: ADR-001, ADR-002 y ADR-003 creados en `docs/adr/`.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-project-baseline/
├── spec.md              # Baseline spec — estado del proyecto
├── plan.md              # Este archivo
├── research.md          # Phase 0 — cinco decisiones resueltas
├── data-model.md        # Phase 1 — estructura objetivo del monorepo
├── quickstart.md        # Phase 1 — guía de desarrollo local y Spec Kit
├── contracts/
│   ├── rest-conventions.md   # Convenciones REST y OpenAPI
│   └── adr-template.md       # Plantilla MADR y convenciones de ADR
└── tasks.md             # Phase 2 — creado por /speckit.tasks
```

### Source Code (repository root)

Ver [data-model.md](data-model.md) para el árbol completo con estado actual vs objetivo.
El resumen por capa:

```text
MiKompri/
├── MiKompri.{BC}.Api/              ← Controllers, Middleware, Models, Program.cs, Dockerfile
├── MiKompri.{BC}.Application/     ← Commands, Queries, DTOs, Behaviors, Validators
├── MiKompri.{BC}.Domain/          ← Entities, ValueObjects, Abstractions
├── MiKompri.{BC}.Infrastructure/  ← DbContext, Repositories, Migrations, Configurations
├── test/                          ← {BC}.{Layer}.Tests por bounded context y capa
├── docs/adr/                      ← ADRs numerados secuencialmente  [NUEVO]
├── infra/azure/                   ← Bicep/Terraform para ACA + PostgreSQL  [PENDIENTE]
├── MiKompri.Mobile/               ← .NET MAUI Android (MVP-3)  [PENDIENTE]
├── specs/                         ← Especificaciones Spec Kit
└── .specify/                      ← Config y templates de Spec Kit
```

**Structure Decision**: Se mantiene la estructura flat de bounded contexts en la raíz
del monorepo (sin agrupar bajo `src/`). Mover los proyectos requeriría reescribir el
`.sln` y todas las referencias de `.csproj` sin un beneficio proporcional en esta etapa.
Se añaden solo los directorios nuevos: `docs/adr/`, `infra/azure/`, `MiKompri.Mobile/`.

---

## Estrategia Docker y Docker Compose

**Principio**: Todo el backend local DEBE levantarse con `docker compose up`.

**Organización de archivos**:

| Archivo | Propósito |
|---------|-----------|
| `docker-compose.yml` | Topología de servicios: imágenes, redes, healthchecks, dependencias. Sin valores secretos |
| `docker-compose.override.yml` | Variables de entorno para desarrollo local (contraseñas, puertos). En `.gitignore` |

**Servicios objetivo en compose**:

| Servicio | Imagen | Puerto | Perfil |
|----------|--------|--------|--------|
| `postgres` | `postgres:15` | 5432 | `backend`, `migrate` |
| `mikompri-shoppinglist-api` | Local build / GHCR | 8080 | `backend` |
| `mikompri-users-api` | Local build / GHCR | 8081 | `backend` (MVP-1) |

**Reglas**:
1. Los servicios DEBEN usar `depends_on` con `condition: service_healthy`.
2. Las contraseñas de desarrollo DEBEN vivir en `docker-compose.override.yml`, no en
   `docker-compose.yml`.
3. El `Dockerfile` de cada API usa multi-stage build: `base → build → publish → final`.
   La imagen final es `aspnet:8.0` (no SDK). Ver Dockerfile existente como referencia.

---

## Estrategia de Migraciones

Ver [ADR-002](../../docs/adr/ADR-002-migrations-strategy.md). Resumen operativo:

**Desarrollo local**: `Database.Migrate()` al arrancar (condicionado a `AUTO_MIGRATE=true`
o entorno Development). No requiere paso manual.

**Producción (CI/CD)**: Step dedicado en el pipeline CD antes del deploy:

```yaml
- name: Apply EF Core Migrations
  run: |
    dotnet tool install --global dotnet-ef
    dotnet ef database update \
      --project MiKompri.ShoppingList.Infrastructure \
      --startup-project MiKompri.ShoppingList.Api
  env:
    ConnectionStrings__PostgreSQL: ${{ secrets.PROD_CONNECTION_STRING }}
```

**Comandos de gestión** (ver quickstart.md § 5 para detalle completo):

```powershell
# Crear migración nueva
dotnet ef migrations add <Nombre> `
  --project MiKompri.ShoppingList.Infrastructure `
  --startup-project MiKompri.ShoppingList.Api `
  --output-dir Persistence/Migrations

# Listar migraciones pendientes
dotnet ef migrations list `
  --project MiKompri.ShoppingList.Infrastructure `
  --startup-project MiKompri.ShoppingList.Api
```

---

## Estrategia de Testing

**Tres niveles obligatorios** (TP8):

| Nivel | Qué prueba | Framework | Aislamiento |
|-------|-----------|-----------|-------------|
| Domain Unit | Reglas de negocio en aggregate roots y value objects | xUnit + FluentAssertions | Sin dependencias externas |
| Application Unit | Command/Query handlers con repositorios mock | xUnit + Moq + FluentAssertions | Mock de IPurchaseListRepository e IUnitOfWork |
| API Integration | Endpoints REST end-to-end | WebApplicationFactory + EF InMemory | CustomWebApplicationFactory reemplaza DbContext |

**Convenciones a mantener**:
- Los tests de integración usan `CustomWebApplicationFactory` que reemplaza
  `DbContextOptions<ShoppingListDbContext>` por EF Core InMemory.
- Los tests de integración son herméticamente aislados: cada test DEBE limpiar su estado
  o usar un `Guid` único para las entidades creadas.
- Los tests DEBEN ejecutarse en menos de 60 segundos totales para mantener el ciclo rápido.
- Todo nuevo bounded context DEBE tener los tres niveles de tests antes de merge a `main`.

**Cobertura**:
- Meta orientativa: ≥ 80 % en la capa Domain de cada bounded context.
- El umbral obligatorio en CI se definirá en la spec de MVP-1 tras medir la línea base
  actual (TODO(COVERAGE_THRESHOLD) de la constitución).

---

## Estrategia de CI/CD con GitHub Actions

**Pipeline CI** (`ci-mikompri-{bc}.yml`) — por bounded context:

```text
Trigger: push/PR a main, develop, feature/*, hotfix/*
Steps:
  1. dotnet restore
  2. dotnet build --configuration Release
  3. dotnet test (Domain + Application + Api.Tests) con cobertura Coverlet
  4. SonarCloud analysis (begin → build → end)
  [Futuro] 5. Validate OpenAPI schema
```

**Pipeline CD** (`cd-mikompri-{bc}.yml`) — por bounded context:

```text
Trigger: tags v*.*.*
Steps:
  1. docker build + push a GHCR         ← existente
  [NUEVO] 2. Apply EF Core Migrations   ← ADR-002
  [NUEVO] 3. Deploy a Azure Container Apps ← ADR-001
```

**Convenciones de branching**:

| Tipo | Prefijo | Ejemplo |
|------|---------|---------|
| Feature | `feature/` | `feature/002-user-authentication` |
| Hotfix | `hotfix/` | `hotfix/fix-null-owner-id` |
| Docs / Spec | `chore/` | `chore/spec-kit-adoption` |
| Release | `release/` | `release/v0.2.0` |

**Convenciones de tags para CD**:
- `v{MAJOR}.{MINOR}.{PATCH}` → despliega a staging en ACA.
- El despliegue a producción usa entornos protegidos de GitHub Actions con aprobación manual.

---

## Estrategia de Despliegue en Azure

Ver [ADR-001](../../docs/adr/ADR-001-azure-deployment.md).

**Arquitectura objetivo**:

```text
GitHub Actions CD
       │
       ▼
GHCR ──► Azure Container Apps Environment
              ├── ShoppingList API container  (puerto 80/443, ingress externo)
              └── Users API container         (puerto 80/443, ingress externo, MVP-1)
                         │
                         ▼
              Azure Database for PostgreSQL Flexible Server
                  ├── DB: MiKompri_ShoppingList
                  └── DB: MiKompri_Users
```

**Infraestructura como código**: `infra/azure/` con Bicep (pendiente de crear).
**Autenticación CD → Azure**: OIDC Workload Identity Federation (sin secrets de larga duración).
**Secretos en ACA**: Connection strings y keys gestionados como Container Apps Secrets.

---

## Convenciones REST y OpenAPI

Ver [contracts/rest-conventions.md](contracts/rest-conventions.md).

**Puntos críticos**:
- URLs en **kebab-case plural**: `/api/v1/purchase-lists` (target; actual es PascalCase).
- Versioning en path: `/api/v{N}/`.
- Errores en formato **RFC 9457 Problem Details** con `traceId`.
- Swagger disponible en entornos no productivos en `/swagger`.
- Autenticación JWT Bearer desde MVP-1.

---

## Convenciones para ADR

Ver [contracts/adr-template.md](contracts/adr-template.md) y [ADR-003](../../docs/adr/ADR-003-adr-format.md).

**Resumen**:
- Formato: MADR simplificado.
- Ubicación: `docs/adr/ADR-{NNN}-{titulo-kebab}.md`.
- Numeración secuencial de tres dígitos.
- Ciclo de vida: `Propuesto → Aceptado → Obsoleto | Reemplazado`.

---

## Convenciones para Futuras Specs

**Nomenclatura de directorio**: `specs/{NNN}-{nombre-kebab}/`

**Artefactos obligatorios** por spec (TP10):

| Artefacto | Comando | Propósito |
|-----------|---------|-----------|
| `spec.md` | `/speckit.specify` | Qué y por qué. User stories, requisitos, criterios de éxito |
| `plan.md` | `/speckit.plan` | Cómo. Diseño técnico, ADRs, estructura de código |
| `tasks.md` | `/speckit.tasks` | Ítems ejecutables con criterios de done |

**Flujo completo**:
```
/speckit.specify → /speckit.plan → /speckit.tasks → /speckit.implement → /speckit.checklist
```

**Branch por spec**: `feature/{NNN}-{feature-name}` en el repositorio.

**Próxima spec recomendada**: `002-user-authentication` (MVP-1)
```
/speckit.specify Implementar autenticación OAuth/OIDC. Los usuarios se autentican con
un IdP externo. La API valida JWT tokens. Al primer acceso se crea automáticamente el
perfil del usuario en el bounded context Users. OwnerId en PurchaseList se resuelve
desde el token de autenticación.
```

---

## Riesgos Técnicos

| ID | Riesgo | Prob. | Impacto | Mitigación |
|----|--------|-------|---------|------------|
| R-001 | Sin migraciones EF Core → pérdida de datos al cambiar schema | Alta | Alto | ADR-002: crear migraciones explícitas antes del próximo cambio de modelo |
| R-002 | Endpoints públicos sin autenticación en entorno desplegado | Alta | Alto | Bloquear acceso externo o implementar auth en MVP-1 antes de compartir la URL |
| R-003 | CORS completamente abierto (`AllowAnyOrigin`) | Media | Medio | Restringir a dominios conocidos en MVP-1 al añadir autenticación |
| R-004 | `OwnerId`/`GroupId` son Guids sin validación cruzada con Users | Alta | Medio | Resolver en MVP-1 al integrar JWT con identidad del usuario |
| R-005 | Users Application + API sin implementar → bloquea MVP-2 | Alta | Alto | Priorizar Users Application + API en MVP-1 |
| R-006 | Sin tests para bounded context Users | Alta | Medio | Obligatorio antes de merge de cualquier feature de Users |
| R-007 | `ListItem` sin campo `AddedBy` → viola PP4 | Alta | Medio | Añadir en la misma spec que implemente colaboración (MVP-2) |
| R-008 | Docker Compose sin credenciales seguras (password hardcoded) | Media | Bajo | Mover a `docker-compose.override.yml` inmediatamente |

---

## Complexity Tracking

No hay violaciones de constitución que requieran justificación formal. Los WARNs
registrados en Constitution Check están gestionados en este plan:
- **PP4**: DT-004 registrado, obligatorio antes de MVP-2.
- **TP5**: ADR-001 define la plataforma Azure; implementación en CD de MVP-1.
- **TP9**: ADR-001, ADR-002 y ADR-003 creados en `docs/adr/`.
