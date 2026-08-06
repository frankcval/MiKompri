---

description: "Lista de tareas para la implementación de Product Catalog (MVP-2)"

---

# Tasks: Product Catalog (MVP-2)

**Input**: Documentos de diseño desde `/specs/004-product-catalog/`

**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Incluidos porque la especificación y el quickstart requieren validación por capas (Domain, Application y API).

**Organization**: Las tareas están agrupadas por user story para permitir implementación y validación independiente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias)
- **[Story]**: US1, US2, US3
- Incluir rutas de archivo exactas en cada descripción

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicializar el bounded context ProductCatalog, sus proyectos y su integración básica con la solución.

- [X] T001 Crear la estructura base del bounded context y registrar los nuevos proyectos en `MiKompri.sln`: `MiKompri.ProductCatalog.Api/`, `MiKompri.ProductCatalog.Application/`, `MiKompri.ProductCatalog.Domain/`, `MiKompri.ProductCatalog.Infrastructure/` y `test/MiKompri.ProductCatalog.*.Tests/`
- [X] T002 Configurar los archivos `.csproj` iniciales de `MiKompri.ProductCatalog.Api`, `MiKompri.ProductCatalog.Application`, `MiKompri.ProductCatalog.Domain`, `MiKompri.ProductCatalog.Infrastructure` y los tres proyectos de test con `net8.0`, referencias entre capas y paquetes base de MediatR, FluentValidation, EF Core, Serilog, xUnit, FluentAssertions y Moq
- [X] T003 [P] Crear los archivos de arranque y plantilla del API en `MiKompri.ProductCatalog.Api/Program.cs`, `MiKompri.ProductCatalog.Api/appsettings.json`, `MiKompri.ProductCatalog.Api/appsettings.Development.json`, `MiKompri.ProductCatalog.Api/Properties/launchSettings.json`, `MiKompri.ProductCatalog.Api/Dockerfile` y `MiKompri.ProductCatalog.Api/MiKompri.ProductCatalog.Api.http`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base compartida que debe existir antes de implementar cualquier user story.

**⚠️ CRITICAL**: No iniciar trabajo de user stories hasta completar esta fase.

- [X] T004 [P] Implementar las abstracciones base de dominio y excepciones comunes en `MiKompri.ProductCatalog.Domain/Abstractions/Entity.cs` y archivos relacionados que necesite el contexto
- [X] T005 [P] Implementar la base de Application en `MiKompri.ProductCatalog.Application/DependencyInjection.cs`, `MiKompri.ProductCatalog.Application/Behavior/`, `MiKompri.ProductCatalog.Application/Dtos/` y contratos de puertos compartidos
- [X] T006 [P] Implementar la base de persistencia en `MiKompri.ProductCatalog.Infrastructure/Persistence/ProductCatalogDbContext.cs`, `MiKompri.ProductCatalog.Infrastructure/Persistence/Repositories/`, `MiKompri.ProductCatalog.Infrastructure/Persistence/UnitOfWork.cs`, `MiKompri.ProductCatalog.Infrastructure/Persistence/Configurations/` y `MiKompri.ProductCatalog.Infrastructure/Services/SystemDateTimeProvider.cs`
- [X] T007 [P] Configurar el bootstrap del API y el manejo transversal en `MiKompri.ProductCatalog.Api/Program.cs`, `MiKompri.ProductCatalog.Api/Middleware/ExceptionHandlingMiddleware.cs`, `MiKompri.ProductCatalog.Api/Middleware/RequestLoggingMiddleware.cs` y `MiKompri.ProductCatalog.Api/Extensions/HealthChecksExtensions.cs`

**Checkpoint**: La base técnica del bounded context queda lista para implementar stories de forma independiente.

---

## Phase 3: User Story 1 - Gestionar catálogo reutilizable (Priority: P1) 🎯 MVP

**Goal**: Crear, editar, consultar y desactivar productos reutilizables del catálogo sin depender de mercados ni historial.

**Independent Test**: Se puede validar creando, editando, consultando y desactivando productos; el producto desactivado no aparece en el catálogo activo.

### Tests for User Story 1

- [X] T008 [P] [US1] Crear pruebas de dominio para `CatalogProduct`, `ProductName` y `Money` en `test/MiKompri.ProductCatalog.Domain.Tests/Products/CatalogProductTests.cs`, `test/MiKompri.ProductCatalog.Domain.Tests/Products/ProductNameTests.cs` y `test/MiKompri.ProductCatalog.Domain.Tests/Products/MoneyTests.cs`
- [X] T009 [P] [US1] Crear pruebas de Application para los handlers y validadores de catálogo en `test/MiKompri.ProductCatalog.Application.Tests/Commands/CreateCatalogProduct/`, `UpdateCatalogProduct/`, `DeactivateCatalogProduct/` y `Queries/GetCatalogProducts/`, `GetCatalogProductById/`

### Implementation for User Story 1

- [X] T010 [P] [US1] Implementar el aggregate `CatalogProduct` y los value objects `ProductName` y `Money` en `MiKompri.ProductCatalog.Domain/Products/CatalogProduct.cs` y `MiKompri.ProductCatalog.Domain/Products/ValueObjects/`
- [X] T011 [P] [US1] Implementar el contrato de repositorio y el mapeo EF Core del catálogo en `MiKompri.ProductCatalog.Infrastructure/Persistence/Repositories/`, `MiKompri.ProductCatalog.Infrastructure/Persistence/Configurations/` y `MiKompri.ProductCatalog.Infrastructure/Persistence/ProductCatalogDbContext.cs`
- [X] T012 [US1] Implementar comandos, queries, handlers, validadores y DTOs del catálogo en `MiKompri.ProductCatalog.Application/Commands/CreateCatalogProduct/`, `UpdateCatalogProduct/`, `DeactivateCatalogProduct/`, `MiKompri.ProductCatalog.Application/Queries/GetCatalogProducts/`, `GetCatalogProductById/` y `MiKompri.ProductCatalog.Application/Dtos/`
- [X] T013 [US1] Implementar `CatalogProductsController` y los modelos HTTP del catálogo en `MiKompri.ProductCatalog.Api/Controllers/CatalogProductsController.cs` y `MiKompri.ProductCatalog.Api/Models/`

**Checkpoint**: El catálogo reutilizable debe ser funcional y testeable de forma independiente.

---

## Phase 4: User Story 2 - Gestionar mercados y registrar precios por mercado (Priority: P2)

**Goal**: Crear mercados, desactivarlos y registrar precios válidos por producto y mercado con validaciones de negocio.

**Independent Test**: Se puede validar creando mercados y registrando precios para un producto activo en fechas válidas, rechazando duplicados, precios no positivos y fechas futuras.

### Tests for User Story 2

- [X] T014 [P] [US2] Crear pruebas de dominio para `Market` y `ProductPriceRecord` en `test/MiKompri.ProductCatalog.Domain.Tests/Markets/MarketTests.cs` y `test/MiKompri.ProductCatalog.Domain.Tests/Products/ProductPriceRecordTests.cs`
- [X] T015 [P] [US2] Crear pruebas de Application para comandos de mercados y registro de precios en `test/MiKompri.ProductCatalog.Application.Tests/Commands/CreateMarket/`, `UpdateMarket/`, `DeactivateMarket/` y `RegisterProductPrice/`

### Implementation for User Story 2

- [X] T016 [P] [US2] Implementar el aggregate `Market` y la entidad `ProductPriceRecord` con sus invariantes en `MiKompri.ProductCatalog.Domain/Markets/Market.cs` y `MiKompri.ProductCatalog.Domain/Products/ProductPriceRecord.cs`
- [X] T017 [P] [US2] Implementar el mapeo EF Core, los índices y los repositorios necesarios para mercados y precios en `MiKompri.ProductCatalog.Infrastructure/Persistence/Configurations/`, `MiKompri.ProductCatalog.Infrastructure/Persistence/Repositories/` y `MiKompri.ProductCatalog.Infrastructure/Persistence/ProductCatalogDbContext.cs`
- [X] T018 [US2] Implementar comandos, handlers, validadores y DTOs para mercados y registro de precios en `MiKompri.ProductCatalog.Application/Commands/CreateMarket/`, `UpdateMarket/`, `DeactivateMarket/`, `RegisterProductPrice/` y `MiKompri.ProductCatalog.Application/Dtos/`
- [X] T019 [US2] Implementar `MarketsController` y `ProductPricesController` con sus request/response models en `MiKompri.ProductCatalog.Api/Controllers/MarketsController.cs`, `MiKompri.ProductCatalog.Api/Controllers/ProductPricesController.cs` y `MiKompri.ProductCatalog.Api/Models/`

**Checkpoint**: Mercados y precios por mercado deben funcionar sin depender del historial agregado de la story 3.

---

## Phase 5: User Story 3 - Consultar historial de precios y preparar asociación futura con listas (Priority: P3)

**Goal**: Consultar el historial de precios por producto y mercado, con filtros temporales, manteniendo un identificador estable reutilizable para futuras integraciones.

**Independent Test**: Se puede validar consultando historial ordenado cronológicamente, con resultados vacíos cuando no hay datos, y filtrando por mercado y rango de fechas.

### Tests for User Story 3

- [X] T020 [P] [US3] Crear pruebas de Application y API para la consulta de historial de precios en `test/MiKompri.ProductCatalog.Application.Tests/Queries/GetProductPriceHistory/` y `test/MiKompri.ProductCatalog.Api.Tests/`

### Implementation for User Story 3

- [X] T021 [P] [US3] Implementar `GetProductPriceHistory` con filtros por mercado y rango de fechas en `MiKompri.ProductCatalog.Application/Queries/GetProductPriceHistory/` y `MiKompri.ProductCatalog.Application/Dtos/`
- [X] T022 [US3] Implementar el endpoint de historial en `MiKompri.ProductCatalog.Api/Controllers/CatalogProductsController.cs` y asegurar que las respuestas expongan `catalogProductId` como identificador estable reutilizable
- [X] T023 [P] [US3] Actualizar el contrato REST y la guía de validación en `specs/004-product-catalog/contracts/product-catalog-api.md` y `specs/004-product-catalog/quickstart.md` para reflejar el historial y el identificador estable

**Checkpoint**: El historial de precios debe ser consultable sin romper la independencia del bounded context.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cierre técnico, validación y ajustes que afectan a varias stories.

- [X] T024 [P] Generar la migración inicial y refrescar el snapshot EF Core del contexto en `MiKompri.ProductCatalog.Infrastructure/Persistence/Migrations/` y verificar que el modelo coincide con `MiKompri.ProductCatalog.Infrastructure/Persistence/ProductCatalogDbContext.cs`
- [X] T025 Validar el despliegue local y la compilación completa con `MiKompri.ProductCatalog.Api/Dockerfile`, `docker-compose.yml`, `MiKompri.sln` y los proyectos de test `test/MiKompri.ProductCatalog.*.Tests/`, corrigiendo cualquier error restante en `MiKompri.ProductCatalog.*`
- [x] T026 Benchmark de rendimiento SC-001/SC-002 documentado como deuda técnica DT-PC-001 (script de medición disponible en `MiKompri.ProductCatalog.Api/Performance/Measure-ProductCatalogPerformance.ps1`; validación formal de SLA queda fuera del MVP-2)
- [x] T027 Integrar `MiKompri.ProductCatalog.Api` y su base de datos en `docker-compose.yml` y `docker-compose.override.yml` con healthcheck y variables de entorno coherentes con `quickstart.md`; moneda configurable añadida vía `ProductCatalog__Currency`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias; puede comenzar inmediatamente.
- **Foundational (Phase 2)**: Depende de Setup y bloquea todas las user stories.
- **User Stories (Phase 3+)**: Dependen de Foundational.
  - US1 puede ejecutarse primero como MVP.
  - US2 depende del núcleo del dominio y persistencia, pero debe seguir siendo testeable por separado.
  - US3 depende de la disponibilidad del modelo de precios y de las consultas de lectura.
- **Polish (Phase 6)**: Depende de que terminen las stories que se quieran entregar.

### User Story Dependencies

- **US1 (P1)**: Independiente tras Foundational.
- **US2 (P2)**: Independiente tras Foundational, reutiliza el catálogo activo de US1 para registrar precios.
- **US3 (P3)**: Independiente tras Foundational, reutiliza datos de catálogo y precios para la consulta histórica.

### Within Each User Story

- Las pruebas, si existen, deben escribirse primero y fallar antes de implementar.
- El modelo de dominio antes que servicios y handlers.
- Handlers y validadores antes que controladores.
- Implementación principal antes de integración API.
- Cerrar una story antes de avanzar a la siguiente prioridad.

### Parallel Opportunities

- Las tareas marcadas con `[P]` pueden ejecutarse en paralelo si no comparten archivos.
- En Setup, T003 puede avanzar en paralelo con T002 una vez creado el esqueleto de proyectos.
- En Foundational, T004, T005, T006 y T007 pueden repartirse entre personas distintas.
- Dentro de cada story, las pruebas y los modelos de dominio suelen poder ejecutarse en paralelo si tocan archivos distintos.
- US1, US2 y US3 pueden trabajarse en paralelo después de completar Foundational, aunque la entrega recomendada sigue el orden de prioridad.

---

## Parallel Example: User Story 1

```bash
# Ejecutar en paralelo las pruebas de dominio y Application de US1:
Task: "Crear pruebas de dominio para CatalogProduct, ProductName y Money en test/MiKompri.ProductCatalog.Domain.Tests/Products/CatalogProductTests.cs, test/MiKompri.ProductCatalog.Domain.Tests/Products/ProductNameTests.cs y test/MiKompri.ProductCatalog.Domain.Tests/Products/MoneyTests.cs"
Task: "Crear pruebas de Application para los handlers y validadores de catálogo en test/MiKompri.ProductCatalog.Application.Tests/Commands/CreateCatalogProduct/, UpdateCatalogProduct/, DeactivateCatalogProduct/ y Queries/GetCatalogProducts/, GetCatalogProductById/"

# Ejecutar en paralelo el modelo de dominio y el mapeo de infraestructura de US1:
Task: "Implementar el aggregate CatalogProduct y los value objects ProductName y Money en MiKompri.ProductCatalog.Domain/Products/CatalogProduct.cs y MiKompri.ProductCatalog.Domain/Products/ValueObjects/"
Task: "Implementar el contrato de repositorio y el mapeo EF Core del catálogo en MiKompri.ProductCatalog.Infrastructure/Persistence/Repositories/, MiKompri.ProductCatalog.Infrastructure/Persistence/Configurations/ y MiKompri.ProductCatalog.Infrastructure/Persistence/ProductCatalogDbContext.cs"
```

---

## Parallel Example: User Story 2

```bash
# Ejecutar en paralelo las pruebas y el modelo de dominio de US2:
Task: "Crear pruebas de dominio para Market y ProductPriceRecord en test/MiKompri.ProductCatalog.Domain.Tests/Markets/MarketTests.cs y test/MiKompri.ProductCatalog.Domain.Tests/Products/ProductPriceRecordTests.cs"
Task: "Implementar el aggregate Market y la entidad ProductPriceRecord con sus invariantes en MiKompri.ProductCatalog.Domain/Markets/Market.cs y MiKompri.ProductCatalog.Domain/Products/ProductPriceRecord.cs"
```

---

## Parallel Example: User Story 3

```bash
# Ejecutar en paralelo la consulta y la documentación de US3:
Task: "Implementar GetProductPriceHistory con filtros por mercado y rango de fechas en MiKompri.ProductCatalog.Application/Queries/GetProductPriceHistory/ y MiKompri.ProductCatalog.Application/Dtos/"
Task: "Actualizar el contrato REST y la guía de validación en specs/004-product-catalog/contracts/product-catalog-api.md y specs/004-product-catalog/quickstart.md para reflejar el historial y el identificador estable"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Setup.
2. Completar Phase 2: Foundational.
3. Completar Phase 3: User Story 1.
4. **STOP and VALIDATE**: Ejecutar pruebas de US1 y verificar el flujo de catálogo aislado.
5. Si está listo, pasar a demo/despliegue local.

### Incremental Delivery

1. Setup + Foundational → base lista.
2. US1 → MVP funcional del catálogo.
3. US2 → mercados y precios por mercado.
4. US3 → historial y contrato estable para futuras integraciones.
5. Validar que cada story aporta valor sin romper las anteriores.

### Parallel Team Strategy

Con varios desarrolladores:

1. El equipo completa Setup + Foundational de forma coordinada.
2. Después:
   - Persona A: US1
   - Persona B: US2
   - Persona C: US3
3. Cada story se completa y valida de forma independiente.

---

## Phase 7: Cierre MVP-2 (fix/complete-spec-004-v1)

**Purpose**: Correcciones necesarias para cerrar formalmente el MVP-2.

- [x] T028 Crear `ConflictException` en `MiKompri.ProductCatalog.Domain/Exceptions/ConflictException.cs` y configurar el middleware para devolver HTTP 409
- [x] T029 Lanzar `ConflictException` en `CreateCatalogProductCommandHandler` y `RegisterProductPriceCommandHandler` en lugar de `InvalidOperationException` para casos de duplicado
- [x] T030 Añadir `ProductCatalogOptions` con Options Pattern; eliminar moneda fija `"EUR"` del handler; leer moneda desde configuración `ProductCatalog:Currency`
- [x] T031 Actualizar tests de Application para inyectar `IOptions<ProductCatalogOptions>` y añadir casos de test para `ConflictException`
- [x] T032 Crear `.github/workflows/ci-mikompri-productcatalog.yml` con restore, build Release, tests Domain/Application/API, docker compose config y docker build
- [x] T033 Actualizar `tasks.md`, `quickstart.md` y `spec.md` para reflejar el estado real verificado del MVP-2
- [x] T034 Marcar Spec 004 como `Completed` y actualizar `README.md` y `specs/001-project-baseline/spec.md`

---

## Notes

- `[P]` = tareas en paralelo, diferentes archivos y sin dependencias.
- `[US#]` = traza la tarea a una user story concreta.
- Cada user story debe ser completada y testeada de forma independiente.
- Verificar que las pruebas fallen antes de implementar cuando se trabaje en modo TDD.
- Evitar tareas vagas o dependencias cruzadas que rompan la independencia.
