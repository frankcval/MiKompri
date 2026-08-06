# MiKompri

[![CI - MiKompri ShoppingList](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml)
[![CI - MiKompri Users](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-users.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-users.yml)
[![CD - MiKompri ShoppingList API](https://github.com/frankcval/MiKompri/actions/workflows/cd-mikompri-shoppinglist.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/cd-mikompri-shoppinglist.yml)

**MiKompri** es una plataforma de gestión colaborativa diseñada para facilitar la organización de compras y usuarios en grupos. El proyecto implementa una arquitectura modular por bounded contexts, preparada para evolucionar hacia microservicios con Clean Architecture y Domain-Driven Design (DDD).

## 📋 Tabla de Contenidos

- [Descripción General](#-descripción-general)
- [Arquitectura](#-arquitectura)
- [Tecnologías](#-tecnologías)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Microservicios](#-microservicios)
- [Características Principales](#-características-principales)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Configuración](#-instalación-y-configuración)
- [Ejecución](#-ejecución)
- [Testing](#-testing)
- [CI/CD](#-cicd)
- [Estado Actual del Proyecto](#-estado-actual-del-proyecto)
- [Próximos Pasos](#-próximos-pasos)
- [Contribución](#-contribución)

## 🎯 Descripción General

MiKompri es una solución empresarial para la gestión colaborativa de listas de compras, diseñada para permitir a múltiples usuarios trabajar en conjunto dentro de grupos organizados. El sistema está construido siguiendo principios SOLID y patrones de diseño modernos.

### Casos de Uso Principales

- **Gestión de Listas de Compras**: Creación, actualización, eliminación y consulta de listas
- **Gestión de Ítems**: Agregar, modificar, marcar como comprados y eliminar ítems
- **Gestión de Usuarios**: Registro, perfiles y autenticación con proveedores externos
- **Gestión de Grupos**: Creación de grupos, membresías y roles (Owner, Admin, Member)
- **Colaboración**: Listas compartidas entre miembros de un grupo

## 🏗️ Arquitectura

El proyecto implementa **Clean Architecture** dividida en capas:

```
┌─────────────────────────────────────────┐
│           API Layer (Controllers)        │
├─────────────────────────────────────────┤
│      Application Layer (Use Cases)      │
│     - Commands (CQRS Write)              │
│     - Queries (CQRS Read)                │
│     - DTOs                               │
│     - Validators (FluentValidation)      │
│     - Behaviors (MediatR Pipeline)       │
├─────────────────────────────────────────┤
│         Domain Layer (Entities)          │
│     - Aggregates                         │
│     - Value Objects                      │
│     - Domain Services                    │
│     - Repository Interfaces              │
├─────────────────────────────────────────┤
│    Infrastructure Layer (Data Access)    │
│     - EF Core DbContext                  │
│     - Repositories                       │
│     - Unit of Work                       │
│     - Configurations                     │
└─────────────────────────────────────────┘
```

### Patrones Implementados

- **CQRS** (Command Query Responsibility Segregation)
- **Mediator Pattern** con MediatR
- **Repository Pattern**
- **Unit of Work Pattern**
- **Aggregate Root Pattern**
- **Value Objects**
- **Dependency Injection**
- **Pipeline Behavior** para validación y logging

## 🛠️ Tecnologías

### Backend

- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - APIs HTTP y middleware
- **Entity Framework Core 9** - ORM para acceso a datos
- **Npgsql EF Core Provider 9** - Integración de EF Core con PostgreSQL
- **PostgreSQL** - Base de datos principal
- **MediatR 12** - Implementación del patrón Mediator/CQRS
- **FluentValidation 12** - Validación de comandos y queries registrada en DI y ejecutada por pipeline behaviors de MediatR
- **Serilog 10** - Logging estructurado
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI para documentación de endpoints

### Testing

- **xUnit** - Framework de testing
- **Moq** - Mocking framework
- **FluentAssertions** - Assertions más expresivas
- **EF Core InMemory 9** - Base en memoria para pruebas aisladas
- **Coverlet** - Cobertura de código
- **WebApplicationFactory** - Testing de integración

### DevOps

- **Docker** & **Docker Compose** - Containerización
- **GitHub Actions** - CI/CD
- **SonarCloud** - Análisis de calidad de código
- **GitHub Container Registry (GHCR)** - Registro de imágenes Docker

## 📁 Estructura del Proyecto

```
MiKompri/
├── MiKompri.ShoppingList.Api/              # API de Listas de Compras
│   ├── Controllers/                        # Endpoints REST
│   ├── Middleware/                         # Global exception handling, logging
│   ├── Models/                             # DTOs de request/response
│   └── Program.cs                          # Configuración de la aplicación
│
├── MiKompri.ShoppingList.Application/      # Capa de Aplicación
│   ├── Commands/                           # Comandos CQRS (Write)
│   ├── Queries/                            # Queries CQRS (Read)
│   ├── DTOs/                               # Data Transfer Objects
│   ├── Interfaces/                         # Contratos de repositorios
│   ├── Behavior/                           # MediatR Pipeline Behaviors
│   └── DependencyInjection.cs             # Registro de servicios
│
├── MiKompri.ShoppingList.Domain/           # Capa de Dominio
│   ├── Entities/                           # Entidades (PurchaseList, ListItem)
│   ├── ValueObjects/                       # Value Objects (ListProgress)
│   └── Abstractions/                       # Interfaces base (Entity, IAggregateRoot)
│
├── MiKompri.ShoppingList.Infrastructure/   # Capa de Infraestructura
│   ├── Persistence/
│   │   ├── Configurations/                # EF Core Configurations
│   │   ├── Repositories/                  # Implementaciones de repositorios
│   │   ├── ShoppingListDbContext.cs       # DbContext
│   │   └── Migrations/                    # Migraciones de BD
│   └── InfrastructureDependencyInjection.cs
│
├── MiKompri.Users.Api/                     # API de Usuarios
├── MiKompri.Users.Application/             # Capa de Aplicación - Usuarios
├── MiKompri.Users.Domain/                  # Capa de Dominio - Usuarios
│   └── Users/                              # Agregados (User, Group, GroupMembership)
├── MiKompri.Users.Infrastructure/          # Capa de Infraestructura - Usuarios
│   └── Persistence/
│       ├── UsersDbContext.cs              # DbContext para usuarios
│       └── Repositories/                  # Repositorios de usuarios/grupos
│
├── test/                                   # Pruebas
│   ├── MiKompri.ShoppingList.Api.Tests/   # Tests de integración API
│   ├── MiKompri.ShoppingList.Application.Tests/  # Tests unitarios de casos de uso
│   └── MiKompri.ShoppingList.Domain.Tests/       # Tests de dominio
│
├── docker-compose.yml                      # Orquestación de contenedores
├── .github/workflows/                      # Pipelines CI/CD
└── README.md                               # Este archivo
```

## 🔧 Microservicios

### 1. ShoppingList API

**Puerto**: 8080  
**Base de Datos**: MiKompri_ShoppingList (PostgreSQL)

#### Endpoints Principales

```
POST   /api/v1/PurchaseLists              # Crear lista
GET    /api/v1/PurchaseLists              # Obtener listas (filtros: ownerId, groupId)
GET    /api/v1/PurchaseLists/{id}         # Obtener lista por ID
PUT    /api/v1/PurchaseLists/{id}         # Actualizar lista
DELETE /api/v1/PurchaseLists/{id}         # Eliminar lista

POST   /api/v1/PurchaseLists/{id}/items   # Agregar ítem
PUT    /api/v1/PurchaseLists/{listId}/items/{itemId}  # Actualizar ítem
DELETE /api/v1/PurchaseLists/{listId}/items/{itemId}  # Eliminar ítem
PATCH  /api/v1/PurchaseLists/{listId}/items/{itemId}/purchase  # Marcar como comprado

GET    /health                            # Health check
```

#### Características

- ✅ CQRS con MediatR
- ✅ Validación con FluentValidation
- ✅ Logging estructurado con Serilog
- ✅ Global Exception Handling
- ✅ CORS configurado
- ✅ Health Checks
- ✅ Swagger/OpenAPI
- ✅ Unit of Work Pattern
- ✅ Migración de base de datos

### 2. Users API

**Puerto**: 8082  
**Base de Datos**: MiKompri_Users (PostgreSQL)

#### Capacidades implementadas en MVP-1

- ✅ Validación JWT Bearer contra un proveedor OIDC externo configurado
- ✅ Auto-provisioning del perfil local en el primer request autenticado
- ✅ Sincronización explícita del perfil local desde los claims del token
- ✅ Perfil local editable (`DisplayName`) y consulta de perfil propio
- ✅ Grupos colaborativos con roles `Owner`, `Admin` y `Member`
- ✅ Gestión de membresías con reglas de autorización por rol
- ✅ Migraciones, Docker, health checks, Swagger/OpenAPI y CI para Users

#### Endpoints principales

```text
GET    /api/v1/users/me
PUT    /api/v1/users/me
POST   /api/v1/users/me/sync

GET    /api/v1/groups
POST   /api/v1/groups
GET    /api/v1/groups/{groupId}/members
POST   /api/v1/groups/{groupId}/members
DELETE /api/v1/groups/{groupId}/members/{userId}

GET    /health
GET    /swagger
```

#### Modelo de Dominio

**User**: Perfil local sincronizado desde un proveedor OIDC externo.
- `DisplayName`: Nombre visible local
- `Email`: Correo electrónico sincronizado desde claims
- `IdentityProvider`: Proveedor externo configurado
- `ExternalUserId`: Claim `sub` del token JWT

**Group**: Grupo colaborativo con referencia canónica `GroupId`.
- `Name`: Nombre del grupo
- `OwnerId`: Usuario propietario inicial
- `Memberships`: Colección de membresías activas

**GroupMembership**: Relación Usuario-Grupo.
- `UserId`: ID del usuario local
- `GroupId`: ID del grupo
- `Role`: Rol del usuario (`Owner`, `Admin`, `Member`)

## ✨ Características Principales

### ShoppingList Microservice

#### Gestión de Listas

- Creación de listas personales o compartidas con grupos
- Actualización de nombre y descripción
- Seguimiento de progreso (ítems totales vs comprados)
- Timestamps de creación y actualización
- Soft delete

#### Gestión de Ítems

- Agregar ítems con nombre, cantidad y unidad
- Actualizar propiedades de ítems
- Marcar/desmarcar como comprado
- Eliminar ítems
- Ordenamiento automático

#### Características Técnicas

- **Validación robusta**: Reglas de negocio validadas antes de ejecución
- **Logging detallado**: Requests, comandos, queries y excepciones
- **Manejo global de excepciones**: Respuestas estandarizadas
- **Health checks**: Monitoreo de BD y estado de la API
- **Cobertura de tests**: Unit tests, integration tests

### Users Microservice

#### Identidad y autenticación

- Validación JWT de un proveedor OIDC externo
- Auto-provisioning del perfil local usando el claim `sub`
- Sincronización manual del perfil desde claims (`name`, `email`)
- Actualización local del nombre visible

#### Gestión de grupos

- Creación de grupos colaborativos
- Listado de grupos del usuario autenticado
- Consulta de miembros por grupo
- Altas y bajas de membresías con matriz de permisos `Owner/Admin/Member`

## 📋 Requisitos Previos

- **.NET 8 SDK**: [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker** & **Docker Compose**: [Descargar](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 15** (opcional, si no usas Docker)
- **Visual Studio 2022** o **Visual Studio Code** (recomendado)
- **Git**

## 🚀 Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone https://github.com/frankcval/MiKompri.git
cd MiKompri
```

### 2. Configurar Variables de Entorno

#### ShoppingList API

Editar `MiKompri.ShoppingList.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
	"PostgreSQL": "Host=localhost;Port=5432;Database=MiKompri_ShoppingList;Username=postgres;Password=TU_PASSWORD"
  }
}
```

#### Users API

Editar `MiKompri.Users.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
	"UsersPostgreSQL": "Host=localhost;Port=5432;Database=MiKompri_Users;Username=postgres;Password=TU_PASSWORD"
  },
  "Authentication": {
	"Authority": "https://login.microsoftonline.com/<tenant>/v2.0",
	"Audience": "mikompri-users",
	"IdentityProvider": "entra"
  }
}
```

### 3. Aplicar Migraciones

#### ShoppingList Database

```bash
cd MiKompri.ShoppingList.Infrastructure
dotnet ef database update --startup-project ../MiKompri.ShoppingList.Api
```

#### Users Database

```bash
cd MiKompri.Users.Infrastructure
dotnet ef database update --startup-project ../MiKompri.Users.Api
```

> La Users API también aplica `Database.Migrate()` al arrancar en entorno Development.

## 🏃 Ejecución

### Opción 1: Docker Compose (Recomendado)

```bash
docker compose up -d
```

Esto iniciará:
- ShoppingList API en `http://localhost:8080`
- Users API en `http://localhost:8082`
- PostgreSQL en `localhost:5432`

**Swagger UI**:
- ShoppingList: http://localhost:8080/swagger
- Users: http://localhost:8082/swagger

### Opción 2: Ejecución Local

#### Terminal 1 - ShoppingList API

```bash
cd MiKompri.ShoppingList.Api
dotnet run
```

#### Terminal 2 - Users API

```bash
cd MiKompri.Users.Api
dotnet run
```

### Health Check

```bash
curl http://localhost:8080/health
curl http://localhost:8082/health
```

Respuesta esperada:
```text
Healthy
{ "status": "Healthy" }
```

## 🧪 Testing

### Ejecutar Todos los Tests

```bash
dotnet test MiKompri.sln --configuration Release
```

### Ejecutar Tests con Cobertura

```bash
dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml
dotnet test test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml
dotnet test test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml
```

### Tests por Proyecto

```bash
# ShoppingList
dotnet test test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release
dotnet test test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj --configuration Release
dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release

# Users
dotnet test test/MiKompri.Users.Domain.Tests/MiKompri.Users.Domain.Tests.csproj --configuration Release
dotnet test test/MiKompri.Users.Application.Tests/MiKompri.Users.Application.Tests.csproj --configuration Release
dotnet test test/MiKompri.Users.Api.Tests/MiKompri.Users.Api.Tests.csproj --configuration Release
```

### Ejecutar un Test Individual

```bash
dotnet test test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj --filter "FullyQualifiedName~PurchaseListTests.Rename_ActualizaNombreYUpdatedAt"
dotnet test test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj --filter "FullyQualifiedName~CreateShoppingListCommandHandlerTests.Handle_Should_Add_List_And_SaveChanges_Returns_ListId"
dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --filter "FullyQualifiedName~PurchaseListsApiTests.Create_Then_GetById_Should_Return_Created_List"
```

### Cobertura Actual

El proyecto tiene una cobertura significativa de tests:
- ✅ Entidades de dominio (PurchaseList, ListItem)
- ✅ Value Objects (ListProgress)
- ✅ Command Handlers
- ✅ Query Handlers
- ✅ Validators
- ✅ API Endpoints (Integration Tests)

## 🔄 CI/CD

### Integración Continua (CI)

**Workflows**:
- `.github/workflows/ci-mikompri-shoppinglist.yml`
- `.github/workflows/ci-mikompri-users.yml`

**Cobertura actual**:
- **ShoppingList CI**: restore, build, tests con cobertura, SonarCloud y verificación de Docker Compose
- **Users CI**: restore, build, tests de dominio/aplicación/API y build de `MiKompri.Users.Api/Dockerfile`

### Entrega Continua (CD)

**Workflow**: `.github/workflows/cd-mikompri-shoppinglist.yml`

**Triggers**:
- Tags con formato `v*.*.*` (ej: `v1.0.0`)

**Pasos**:
1. ✅ Build de imagen Docker
2. ✅ Push a GitHub Container Registry (GHCR)
3. ✅ Tag con versión y `latest`

**Imagen Docker**: `ghcr.io/frankcval/mikompri-shoppinglist-api`

### Crear Release

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

## 📊 Estado Actual del Proyecto

El estado del proyecto se organiza por MVPs. Para la trazabilidad detallada entre MVPs y
specs de GitHub Spec Kit, consultar [`specs/001-project-baseline/spec.md`](specs/001-project-baseline/spec.md).

### MVP-0 — Listas de Compra Personales ✅ Completado

*(Specs: `001-project-baseline`, `002-shopping-list-core`)*

#### ShoppingList Microservice
- ✅ Arquitectura Clean Architecture completa
- ✅ CQRS con MediatR implementado
- ✅ Todas las operaciones CRUD de listas e ítems
- ✅ Validaciones con FluentValidation
- ✅ Repositorio y Unit of Work
- ✅ EF Core con PostgreSQL
- ✅ Migraciones de base de datos
- ✅ Logging con Serilog
- ✅ Global exception handling
- ✅ Health checks
- ✅ Tests unitarios y de integración
- ✅ Dockerización completa
- ✅ CI/CD con GitHub Actions
- ✅ Swagger/OpenAPI documentation

### MVP-1 — Usuarios, Autenticación e Identidad ✅ Completado

*(Spec: `003-users-authentication`)*

#### Users Microservice
- ✅ Validación JWT Bearer de un proveedor OIDC externo
- ✅ Perfil local del usuario y auto-provisioning por `sub`
- ✅ Sincronización explícita desde claims (`POST /api/v1/users/me/sync`)
- ✅ Actualización de `DisplayName` y consulta de perfil propio
- ✅ Grupos, membresías y roles `Owner`, `Admin`, `Member`
- ✅ Endpoints, migraciones, pruebas, Docker y CI
- ✅ Swagger/OpenAPI con configuración Bearer

### MVP-2 — Catálogo de Productos y Precios ✅ Completado

*(Spec: `004-product-catalog`)*

Ver detalle en [`specs/004-product-catalog/spec.md`](specs/004-product-catalog/spec.md).

---

## 🎯 Próximos Pasos

El roadmap del proyecto se organiza por MVPs. Con **MVP-2** completado, el siguiente paso es definir la Spec 005.

### Prioridad Media — MVP-3 y posteriores (Pendiente)

4. **Catálogo de Productos, Mercados e Historial de Precios** *(MVP-2)* ✅ Completado
   - [x] Spec `004-product-catalog` implementada y cerrada

5. **Listas Compartidas y Reparto de Gastos** *(MVP-3)*
   - [ ] Definir spec `005-shared-lists-settlement` *(siguiente paso)*
   - [ ] Gestión de grupos y listas compartidas con permisos

6. **Cliente Android con .NET MAUI** *(MVP-4)*
   - [ ] Definir spec `006-android-maui-client`

### Prioridad Baja — Calidad y DevOps (Transversal)

7. **Mejoras de Calidad**
   - [ ] Aumentar cobertura de tests a >80%
   - [ ] Implementar mutation testing (Stryker)
   - [ ] Agregar tests de carga (k6 o JMeter)
   - [ ] Documentación técnica detallada
   - [ ] Agregar ejemplos de uso en README

8. **DevOps**
   - [ ] Configurar staging environment
   - [ ] Implementar canary deployments
   - [ ] Agregar rollback automático
   - [ ] Crear Helm charts para Kubernetes

## 🤝 Contribución

### Flujo de Trabajo

1. **Fork** el repositorio
2. Crear una rama feature: `git checkout -b feature/nueva-funcionalidad`
3. Commit de cambios: `git commit -m 'feat: agregar nueva funcionalidad'`
4. Push a la rama: `git push origin feature/nueva-funcionalidad`
5. Crear un **Pull Request** hacia `develop`

### Convenciones de Commits

Seguimos [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: nueva funcionalidad
fix: corrección de bug
docs: cambios en documentación
style: cambios de formato (no afectan código)
refactor: refactorización de código
test: agregar o modificar tests
chore: tareas de mantenimiento
```

### Guía de Estilo

- Usar C# 12 features cuando sea apropiado
- Seguir principios SOLID
- Mantener alta cobertura de tests
- Documentar métodos públicos con XML comments
- Usar `record` para DTOs y Commands
- Preferir `async/await` para operaciones I/O
- Usar `CancellationToken` en métodos async

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver archivo `LICENSE` para más detalles.

## 👥 Autores

- **Frank Cruz** - [@frankcval](https://github.com/frankcval)

## 📞 Contacto

Para preguntas o sugerencias, por favor abrir un [issue](https://github.com/frankcval/MiKompri/issues).

---

⭐ Si este proyecto te resulta útil, considera darle una estrella en GitHub!
