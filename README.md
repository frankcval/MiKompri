# MiKompri

[![CI - MiKompri ShoppingList](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml)
[![CD - MiKompri ShoppingList API](https://github.com/frankcval/MiKompri/actions/workflows/cd-mikompri-shoppinglist.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/cd-mikompri-shoppinglist.yml)

**MiKompri** es una plataforma de gestión colaborativa diseñada para facilitar la organización de compras y usuarios en grupos. El proyecto implementa una arquitectura de microservicios con Clean Architecture y Domain-Driven Design (DDD).

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

**Puerto**: TBD  
**Base de Datos**: MiKompri_Users (PostgreSQL)

#### Modelo de Dominio

**User**: Usuario con soporte para múltiples proveedores de identidad (OAuth/OIDC)
- `DisplayName`: Nombre visible
- `Email`: Correo electrónico
- `IdentityProvider`: Proveedor externo (Keycloak, Auth0, Entra, etc.)
- `ExternalUserId`: ID del usuario en el proveedor externo (claim "sub")

**Group**: Grupos colaborativos
- `Name`: Nombre del grupo
- `OwnerId`: Usuario propietario
- `Memberships`: Colección de membresías

**GroupMembership**: Relación Usuario-Grupo
- `UserId`: ID del usuario
- `GroupId`: ID del grupo
- `Role`: Rol del usuario (Owner, Admin, Member)

#### Estado Actual

🚧 **En Desarrollo** - Rama actual: `30-feature/users/infra-efcore`

- ✅ Modelo de dominio completo
- ✅ Capa de aplicación con comandos:
  - CreateGroup
  - AddMemberToGroup
- ✅ DbContext configurado (UsersDbContext)
- ✅ Repositorios implementados
- ⏳ Migraciones pendientes
- ⏳ API Controllers pendientes
- ⏳ Integración con proveedores OAuth/OIDC pendiente

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

#### Gestión de Usuarios

- Registro de usuarios desde proveedores OAuth/OIDC
- Actualización de perfil
- Gestión de membresías en grupos

#### Gestión de Grupos

- Creación de grupos colaborativos
- Asignación de roles (Owner, Admin, Member)
- Agregar/remover miembros
- Control de acceso basado en roles

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
  }
}
```

### 3. Aplicar Migraciones

#### ShoppingList Database

```bash
cd MiKompri.ShoppingList.Infrastructure
dotnet ef database update --startup-project ../MiKompri.ShoppingList.Api
```

#### Users Database (Cuando estén disponibles)

```bash
cd MiKompri.Users.Infrastructure
dotnet ef database update --startup-project ../MiKompri.Users.Api
```

## 🏃 Ejecución

### Opción 1: Docker Compose (Recomendado)

```bash
docker-compose up -d
```

Esto iniciará:
- ShoppingList API en `http://localhost:8080`
- PostgreSQL en `localhost:5432`

**Swagger UI**: http://localhost:8080/swagger

### Opción 2: Ejecución Local

#### Terminal 1 - ShoppingList API

```bash
cd MiKompri.ShoppingList.Api
dotnet run
```

#### Terminal 2 - Users API (Cuando esté lista)

```bash
cd MiKompri.Users.Api
dotnet run
```

### Health Check

```bash
curl http://localhost:8080/health
```

Respuesta esperada:
```json
{
  "status": "Healthy",
  "checks": [
	{
	  "name": "PostgreSQL",
	  "status": "Healthy"
	}
  ]
}
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
# Tests de Dominio
dotnet test test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release

# Tests de Aplicación
dotnet test test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj --configuration Release

# Tests de Integración
dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release
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

**Workflow**: `.github/workflows/ci-mikompri-shoppinglist.yml`

**Triggers**:
- Push a `main`, `develop`, `feature/*`, `hotfix/*`
- Pull Requests a `main` o `develop`

**Pasos**:
1. ✅ Checkout del código
2. ✅ Caché de paquetes NuGet
3. ✅ Setup .NET 8
4. ✅ Instalación de SonarScanner
5. ✅ Inicio de análisis SonarCloud
6. ✅ Restauración de dependencias
7. ✅ Build del proyecto
8. ✅ Ejecución de tests con cobertura
9. ✅ Finalización de análisis SonarCloud

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

### ✅ Completado

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

#### Users Microservice
- ✅ Modelo de dominio (User, Group, GroupMembership)
- ✅ Capa de aplicación con comandos básicos
- ✅ DbContext y configuraciones
- ✅ Repositorios implementados
- ✅ Proyecto API inicializado

### 🚧 En Progreso

#### Users Microservice (Rama actual: `30-feature/users/infra-efcore`)
- ⏳ Migraciones de Entity Framework Core
- ⏳ Controllers de API
- ⏳ Comandos y Queries faltantes
- ⏳ Validadores
- ⏳ Tests

### ⏳ Próximos Pasos

## 🎯 Próximos Pasos

### Prioridad Alta

1. **Completar Users Microservice**
   - [ ] Crear y aplicar migraciones de EF Core
   - [ ] Implementar todos los Controllers
   - [ ] Agregar comandos faltantes:
	 - `UpdateUser`
	 - `RemoveMemberFromGroup`
	 - `UpdateGroupRole`
	 - `DeleteGroup`
   - [ ] Implementar queries:
	 - `GetUserById`
	 - `GetUsersByGroup`
	 - `GetGroupById`
	 - `GetGroupsByUser`
   - [ ] Agregar validadores FluentValidation
   - [ ] Escribir tests unitarios y de integración
   - [ ] Dockerizar Users API

2. **Autenticación y Autorización**
   - [ ] Integrar OAuth 2.0 / OpenID Connect
   - [ ] Implementar JWT Bearer Authentication
   - [ ] Configurar Identity Provider (Keycloak, Auth0, o Entra)
   - [ ] Agregar políticas de autorización basadas en roles
   - [ ] Proteger endpoints con `[Authorize]`
   - [ ] Implementar refresh tokens

3. **Comunicación entre Microservicios**
   - [ ] Definir estrategia de comunicación (REST, gRPC, o Message Queue)
   - [ ] Validar `OwnerId` y `GroupId` en ShoppingList contra Users API
   - [ ] Implementar circuit breaker pattern (Polly)
   - [ ] Agregar retry policies

### Prioridad Media

4. **API Gateway**
   - [ ] Configurar Ocelot o YARP como API Gateway
   - [ ] Centralizar autenticación
   - [ ] Implementar rate limiting
   - [ ] Agregar logging centralizado

5. **Mejoras de Infraestructura**
   - [ ] Agregar Redis para caché
   - [ ] Implementar Event Sourcing (opcional)
   - [ ] Configurar monitoreo con Prometheus + Grafana
   - [ ] Agregar tracing distribuido (OpenTelemetry)

6. **Funcionalidades Adicionales**
   - [ ] Notificaciones en tiempo real (SignalR)
   - [ ] Compartir lista vía link público
   - [ ] Historial de cambios en listas
   - [ ] Templates de listas predefinidas
   - [ ] Categorización de ítems
   - [ ] Búsqueda y filtrado avanzado

### Prioridad Baja

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
