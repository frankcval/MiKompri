# MiKompri

> **Estado al 31/10/2026:** Web es el cliente prioritario y ya está implementado mediante las specs [007](specs/007-web-react-client/spec.md), [008](specs/008-web-ux-modernization/spec.md) y [009](specs/009-shopping-experience-completion/spec.md). El PR [#57](https://github.com/frankcval/MiKompri/pull/57) está integrado en `main`. En 009, T009 está cerrada con evidencia técnica; T010/T011 y la aceptación global siguen pendientes. El cliente Android con .NET MAUI está aplazado. Ver [estado y próximos pasos](#-estado-actual-del-proyecto).

[![CI - MiKompri ShoppingList](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml)
[![CI - MiKompri Users](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-users.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-users.yml)
[![CI - MiKompri ProductCatalog](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-productcatalog.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-productcatalog.yml)
[![CI - MiKompri Web](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-web.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-web.yml)
[![CI - Resource authorization and persistence](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-resource-security.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-resource-security.yml)
[![CD - MiKompri ShoppingList API](https://github.com/frankcval/MiKompri/actions/workflows/cd-mikompri-shoppinglist.yml/badge.svg)](https://github.com/frankcval/MiKompri/actions/workflows/cd-mikompri-shoppinglist.yml)

**MiKompri** permite organizar compras personales y compartidas, mantener un catálogo con precios por mercado y registrar gastos. Incluye tres APIs .NET (`ShoppingList`, `Users` y `ProductCatalog`) y una SPA React operativa en `MiKompri.Web`. La arquitectura modular utiliza Clean Architecture y Domain-Driven Design (DDD).

Los MVP-0 a MVP-3 corresponden a las capacidades backend entregadas. El trabajo actual se concentra en completar la aceptación del MVP-4 Web, con implementación e historial de validación en 007/008/009. MVP-5 Android/MAUI sigue planificado y diferido, sin proyecto MAUI ni artefactos de spec 006 en el repositorio. Los identificadores de MVP y spec no son equivalentes; consultar la [trazabilidad](specs/001-project-baseline/spec.md#tabla-de-trazabilidad-mvp--specs).

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
- **Identidad y Perfil**: Microsoft Entra, perfil local y grupos. 010-A añade Google mediante External ID, perfil propio y aislamiento de sesión en su rama; aceptación pendiente, sin despliegue productivo
- **Gestión de Grupos**: Creación de grupos, membresías y roles (Owner, Admin, Member)
- **Gestión de Catálogo de Productos**: Alta, actualización y desactivación de productos reutilizables
- **Gestión de Mercados y Precios**: Registro de precios por mercado y consulta de historial
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

### Relación entre Bounded Contexts (estado actual)

```text
┌───────────────────────────────┐        ┌───────────────────────────────┐
│      Users (MVP-1)            │        │   ProductCatalog (MVP-2)      │
│  identidad, perfiles, grupos  │        │ productos, mercados, precios  │
└───────────────┬───────────────┘        └───────────────┬───────────────┘
                │                                        │
                │ referencia por IDs                     │ catálogo reusable
                │ (ownerId, groupId, userId)            │ (catalogProductId)
                ▼                                        ▼
                 ┌─────────────────────────────────────┐
                 │      ShoppingList (MVP-0)           │
                 │ listas e ítems colaborativos        │
                 └─────────────────────────────────────┘
```

- La integración entre bounded contexts se realiza mediante contratos públicos y referencias canónicas. ShoppingList consume Users.Api en runtime para resolver identidad, membresía y roles, sin acceso directo a su base de datos.
- `catalogProductId` queda como identificador estable para asociaciones con ShoppingList y futuras extensiones.

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

### Cliente Web

- **React 18 + TypeScript + Vite** - SPA con rutas protegidas
- **Fluent UI v9** - Componentes y tema compartido
- **MSAL** - Microsoft Entra mediante Authorization Code + PKCE
- **TanStack Query** - Estado de servidor y recuperación de consultas
- **React Hook Form + Zod** - Formularios en los flujos que los han adoptado
- **Vitest + Testing Library + Playwright/axe** - Tests de componentes, integración y navegador; no sustituyen aceptación humana

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
├── MiKompri.ProductCatalog.Api/            # API de Catálogo de Productos
├── MiKompri.ProductCatalog.Application/    # Capa de Aplicación - ProductCatalog
├── MiKompri.ProductCatalog.Domain/         # Capa de Dominio - ProductCatalog
│   ├── Products/                           # CatalogProduct, ProductPriceRecord, ValueObjects
│   └── Markets/                            # Market
├── MiKompri.ProductCatalog.Infrastructure/ # Capa de Infraestructura - ProductCatalog
│   └── Persistence/
│       ├── ProductCatalogDbContext.cs     # DbContext para catálogo
│       ├── Configurations/                # Mapeos EF Core
│       └── Repositories/                  # Repositorios de catálogo/mercados/precios
│
├── MiKompri.Web/                           # Cliente React, tests Web y Dockerfile
├── specs/                                 # Specs, planes, tareas, contratos y evidencia
├── docs/                                  # Guías transversales y ADRs
├── design/prototypes/                     # Prototipos y evidencia de diseño
│
├── test/                                   # Pruebas backend
│   ├── MiKompri.ShoppingList.Api.Tests/   # Tests de integración API
│   ├── MiKompri.ShoppingList.Application.Tests/  # Tests unitarios de casos de uso
│   ├── MiKompri.ShoppingList.Domain.Tests/       # Tests de dominio
│   ├── MiKompri.Users.Api.Tests/
│   ├── MiKompri.Users.Application.Tests/
│   ├── MiKompri.Users.Domain.Tests/
│   ├── MiKompri.ProductCatalog.Api.Tests/
│   ├── MiKompri.ProductCatalog.Application.Tests/
│   └── MiKompri.ProductCatalog.Domain.Tests/
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
PATCH  /api/v1/PurchaseLists/{listId}/items/{itemId}/purchase-state  # Marcar/desmarcar

GET    /health                            # Health check
```

#### Características

Las rutas compartidas, Gastos y compras personales se detallan en los contratos [005](specs/005-shared-lists-settlement/contracts/shared-lists-settlement-api.md), [008](specs/008-web-ux-modernization/contracts/api-and-permissions.md) y [009](specs/009-shopping-experience-completion/contracts/api.md). OpenAPI es la referencia de endpoints y DTOs del backend ejecutado.

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
- `ExternalIdentityKey`: clave canónica del proveedor externo en MiKompri; para Microsoft Entra se usa la tupla `(tid, oid)`

**Group**: Grupo colaborativo con referencia canónica `GroupId`.
- `Name`: Nombre del grupo
- `OwnerId`: Usuario propietario inicial
- `Memberships`: Colección de membresías activas

**GroupMembership**: Relación Usuario-Grupo.
- `UserId`: ID del usuario local
- `GroupId`: ID del grupo
- `Role`: Rol del usuario (`Owner`, `Admin`, `Member`)

### 3. ProductCatalog API

**Puerto**: 8083  
**Base de Datos**: MiKompri_ProductCatalog (PostgreSQL)

#### Capacidades implementadas en MVP-2

- ✅ Gestión de productos reutilizables del catálogo (CRUD lógico con desactivación)
- ✅ Gestión de mercados (alta, edición, desactivación)
- ✅ Registro de precios por producto/mercado/fecha efectiva
- ✅ Historial de precios con filtros (`marketId`, `from`, `to`)
- ✅ Detección de duplicados con respuesta `409 Conflict`
- ✅ Moneda operativa configurable vía `ProductCatalog:Currency`

#### Endpoints principales

```text
POST   /api/v1/catalog-products
PUT    /api/v1/catalog-products/{catalogProductId}
PATCH  /api/v1/catalog-products/{catalogProductId}/deactivate
GET    /api/v1/catalog-products
GET    /api/v1/catalog-products/{catalogProductId}
GET    /api/v1/catalog-products/{catalogProductId}/price-history

POST   /api/v1/markets
PUT    /api/v1/markets/{marketId}
PATCH  /api/v1/markets/{marketId}/deactivate
GET    /api/v1/markets

POST   /api/v1/product-prices

GET    /health
GET    /swagger
```

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
- Auto-provisioning del perfil local usando la identidad canónica del proveedor externo y resolución al `UserId` interno
- Sincronización manual del perfil desde claims (`name`, `email`)
- Actualización local del nombre visible

#### Gestión de grupos

- Creación de grupos colaborativos
- Listado de grupos del usuario autenticado
- Consulta de miembros por grupo
- Altas y bajas de membresías con matriz de permisos `Owner/Admin/Member`

### ProductCatalog Microservice

#### Catálogo y mercados

- Gestión de productos reutilizables con desactivación lógica
- Gestión de mercados activos/inactivos para trazabilidad de precios
- Validación de unicidad de productos y mercados activos

#### Precios e historial

- Registro de precio por `catalogProductId + marketId + effectiveDate`
- Rechazo de duplicados con `409 Conflict`
- Consulta de historial cronológico con filtros por mercado y fechas
- Moneda configurable vía `ProductCatalog:Currency`

## 📋 Requisitos Previos

- **.NET 8 SDK**: [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker** & **Docker Compose**: [Descargar](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 15** (opcional, si no usas Docker)
- **Visual Studio 2022** o **Visual Studio Code** (recomendado)
- **Git**
- **Node.js 24 LTS y npm 11** para `MiKompri.Web` (versiones usadas por CI y la guía Web)
- **Registro SPA de Microsoft Entra** con redirect URI y scope delegado del backend para usar la aplicación autenticada

## 🚀 Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone https://github.com/frankcval/MiKompri.git
cd MiKompri
```

### 2. Configurar Variables de Entorno

#### ShoppingList API

Configurar la conexión mediante variables de entorno, User Secrets u override local. Los ejemplos corresponden a las claves de `appsettings.json`; no versionar credenciales reales.

ShoppingList:

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
	"Audience": "api://<backend-app-client-id>",
	"IdentityProvider": "entra"
  }
}
```

#### ProductCatalog API

Editar `MiKompri.ProductCatalog.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
	"PostgreSQL": "Host=localhost;Port=5432;Database=MiKompri_ProductCatalog;Username=postgres;Password=TU_PASSWORD"
  },
  "ProductCatalog": {
	"Currency": "EUR"
  }
}
```

Las tres APIs deben compartir la configuración de issuer/audience y scopes del recurso OAuth backend según TP11. La SPA tiene un registro de cliente separado; `Users` resuelve `(tid, oid)` a `UserId` interno. Ver [guía de contratos e identidad](docs/client-contract-reuse-guidelines.md).

#### Cliente Web

Copiar `MiKompri.Web/.env.example` a `.env.local` y configurar URLs de las tres APIs, registro SPA, tenant/authority, redirect URIs y `VITE_API_SCOPE`. Las variables `VITE_*` son públicas y se incorporan al bundle: no contienen secretos. Ver [configuración Web](MiKompri.Web/README.md) y [CORS](docs/web-client-cors.md).

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

#### ProductCatalog Database

```bash
cd MiKompri.ProductCatalog.Infrastructure
dotnet ef database update --startup-project ../MiKompri.ProductCatalog.Api
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
- ProductCatalog API en `http://localhost:8083`
- PostgreSQL en `localhost:5432`

El Compose actual levanta backend y base de datos; **no incluye la SPA**. Iniciar Web en otra terminal, después de configurar `.env.local`:

```powershell
cd MiKompri.Web
npm ci
npm run dev
```

Abrir `http://localhost:5173`. La entrada autenticada es `/lists`. Cambiar variables `VITE_*` requiere reiniciar Vite; una imagen Web necesita reconstruirse con su configuración pública. El [README Web](MiKompri.Web/README.md) documenta scripts, Docker y aceptación Entra.

**Swagger UI**:
- ShoppingList: http://localhost:8080/swagger
- Users: http://localhost:8082/swagger
- ProductCatalog: http://localhost:8083/swagger

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

#### Terminal 3 - ProductCatalog API

```bash
cd MiKompri.ProductCatalog.Api
dotnet run
```

### Health Check

```bash
curl http://localhost:8080/health
curl http://localhost:8082/health
curl http://localhost:8083/health
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

# ProductCatalog
dotnet test test/MiKompri.ProductCatalog.Domain.Tests/MiKompri.ProductCatalog.Domain.Tests.csproj --configuration Release
dotnet test test/MiKompri.ProductCatalog.Application.Tests/MiKompri.ProductCatalog.Application.Tests.csproj --configuration Release
dotnet test test/MiKompri.ProductCatalog.Api.Tests/MiKompri.ProductCatalog.Api.Tests.csproj --configuration Release
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

### Validación Web y persistencia

Desde `MiKompri.Web`:

```powershell
npm run lint
npm run typecheck
npm test
npx playwright install chromium firefox webkit
$env:PLAYWRIGHT_ALL_BROWSERS = '1'
npm run test:e2e:smoke
npm run build
```

Los recorridos controlados ejecutan la UI con fixtures en tres motores; la aceptación con dos cuentas Entra usa una suite separada y recursos QA reales. Para las pruebas API relacionales, configurar `MIKOMPRI_TEST_POSTGRES` exclusivamente contra una instancia QA desechable según la [guía 009](specs/009-shopping-experience-completion/quickstart.md).

La [revisión técnica 009](specs/009-shopping-experience-completion/evidence/review-corrections.md) registra 208 tests .NET afectados, 117 Web y 120 E2E satisfactorios en el candidato revisado. Son resultados de esa revisión, no una certificación automática de cualquier HEAD ni aceptación Entra/manual.

## 🔄 CI/CD

### Integración Continua (CI)

**Workflows**:
- `.github/workflows/ci-mikompri-shoppinglist.yml`
- `.github/workflows/ci-mikompri-users.yml`
- `.github/workflows/ci-mikompri-productcatalog.yml`
- `.github/workflows/ci-mikompri-web.yml`
- `.github/workflows/ci-mikompri-resource-security.yml`

**Cobertura actual**:
- **ShoppingList CI**: restore, build, tests con cobertura, SonarCloud y verificación de Docker Compose
- **Users CI**: restore, build, tests de dominio/aplicación/API y build de `MiKompri.Users.Api/Dockerfile`
- **ProductCatalog CI**: restore, build Release, tests de dominio/aplicación/API, validación de `docker compose config` y build de `MiKompri.ProductCatalog.Api/Dockerfile`
- **Web CI**: configuración de aceptación, lint, typecheck, Vitest, escenarios controlados en Chromium/Firefox/WebKit, build y Docker
- **Resource authorization and persistence**: suites ShoppingList y API ProductCatalog contra PostgreSQL 16, más build Docker

La aceptación real está separada en [`acceptance-mikompri-web-entra.yml`](.github/workflows/acceptance-mikompri-web-entra.yml), de ejecución manual y con dos cuentas/sesiones QA. CI controlada verde no cierra ese gate. Azure Static Web Apps es el destino previsto del frontend; el repositorio no acredita por ello un despliegue productivo.

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

Estado al **30/10/2026**, con PR #56 (008) y PR #57 (009) integrados en `main`. «Implementado» y «aceptación completa» son estados distintos. El detalle de tareas y evidencia en cada spec prevalece sobre este resumen; no se cierran tareas por actualizar el roadmap.

| MVP | Capacidad | Estado actual | Specs / evidencia |
| --- | --- | --- | --- |
| MVP-0 | Listas personales y core ShoppingList | Base backend entregada | [001](specs/001-project-baseline/spec.md), [002](specs/002-shopping-list-core/spec.md) |
| MVP-1 | Usuarios, identidad y grupos | Base backend entregada; TP11 vigente | [003](specs/003-users-authentication/spec.md) |
| MVP-2 | Catálogo, mercados y precios | Backend entregado; gestión conectada en Web por 009 | [004](specs/004-product-catalog/spec.md), [contratos 009](specs/009-shopping-experience-completion/contracts/api.md) |
| MVP-3 | Listas compartidas y reparto | Backend entregado y consumido por Web | [005](specs/005-shared-lists-settlement/spec.md) |
| MVP-4 | Cliente Web full-stack | Implementado e integrado; aceptación global abierta | [007 tareas](specs/007-web-react-client/tasks.md), [008 tareas](specs/008-web-ux-modernization/tasks.md), [009 tareas](specs/009-shopping-experience-completion/tasks.md) |
| MVP-5 | Android con .NET MAUI | Planificado y aplazado, sin fecha; identificador 006 reservado, sin artefactos ni proyecto | [Roadmap baseline](specs/001-project-baseline/spec.md#mvp-5--cliente-android-con-net-maui-estado--planned--deferred) |

La numeración de MVP no coincide con la de specs. 008 y 009 amplían el MVP-4 y reutilizan las capacidades backend; no son nuevos MVP-8/MVP-9. El roadmap de MVP-6 en adelante es futuro y no implica implementación ni un calendario comprometido.

### Trabajo Web ya realizado

- **007 — Cliente React:** SPA con MSAL, rutas protegidas, perfil, grupos, listas, gastos/reparto y catálogo; base de navegación y arquitectura por features. Conserva tareas compuestas y aceptación pendiente: [tareas](specs/007-web-react-client/tasks.md).
- **008 — Modernización UX:** Mis listas, compra reversible, CRUD personal/compartido, personas/roles, corrección de gastos y autorización por recurso. [Validación](specs/008-web-ux-modernization/evidence/validation.md) y [aceptación](specs/008-web-ux-modernization/checklists/acceptance.md).
- **009 — Experiencia de compra:** estimaciones con precios desconocidos, orden, Activas/Historial, selección directa/alta contextual, edición de productos, mercados e historial de precios. Captura opcional de importe real, mercado y fecha desde el marcado; Gastos conserva acceso independiente y el mismo registro financiero, con idempotencia y control de concurrencia. Fechas civiles con contexto local y auditoría UTC. [Spec](specs/009-shopping-experience-completion/spec.md), [contratos](specs/009-shopping-experience-completion/contracts/api.md) y [revisión técnica](specs/009-shopping-experience-completion/evidence/review-corrections.md).

### Qué sigue pendiente

En **009**, T009 está cerrada con evidencia de tests, CI y Docker. **T010/T011 siguen abiertas**: la última medición documentada de LCP personal/compartida es 2556/2604 ms frente al límite de 2500 ms; faltan aceptación Entra con dos cuentas/roles y validación manual de lector, zoom, teclado/dispositivo. No se acredita cierre global de 007/008/009 ni despliegue productivo por haber integrado sus PRs.

Las fuentes de verdad son [tasks 009](specs/009-shopping-experience-completion/tasks.md), [rendimiento](specs/009-shopping-experience-completion/evidence/performance.md), [aceptación](specs/009-shopping-experience-completion/evidence/acceptance.md) y [evidencia de revisión](specs/009-shopping-experience-completion/evidence/review-corrections.md). No cambiar umbrales ni sustituir gates humanos por fixtures.

## 🎯 Próximos Pasos

1. Completar los gates Web pendientes de rendimiento, aceptación integrada y accesibilidad con evidencia sobre el candidato validado.
2. Reconciliar individualmente las tareas abiertas de 007/008 según sus criterios y evidencia, sin cerrarlas por la presencia de una implementación posterior.
3. Revisar el PR de [010-A beta — Google CIAM + Microsoft legacy](specs/010-account-management-social-auth/spec.md): arquitectura aprobada, TP11 1.2.0 aplicada y código en su rama; [validación y límites](specs/010-account-management-social-auth/evidence/validation.md). La migración Microsoft real queda diferida y no bloquea beta; linking/G3 quedan en010-B. QA ampliada de lector/zoom y multi-tab se conserva como no bloqueante. [011 — Web Iconography](specs/011-web-iconography/scope.md) conserva solo análisis de alcance.
4. Retomar **MVP-5 Android/MAUI** después de estabilizar la experiencia Web y sus contratos, con alcance y plan propios. MAUI sigue siendo la tecnología prevista para Android; Web primero es una secuencia de entrega, no su cancelación ni sustitución por otro framework móvil.
5. Mantener presupuesto, sugerencias, fotos/OCR, promociones y comunidad como [iniciativas futuras del roadmap](specs/001-project-baseline/spec.md#mvps-definidos), sin incorporarlas a 009/010/011 por defecto. Comentarios/chat también quedan para una feature posterior.

El [análisis documental del 30/10](docs/documentation-review-2026-10-30.md) explica las discrepancias corregidas. Los análisis y planes fechados conservan su contexto histórico; para el estado actual usar esta sección y las tareas de cada spec.

## 🤝 Contribución

### Flujo de Trabajo

1. **Fork** el repositorio
2. Crear una rama feature: `git checkout -b feature/nueva-funcionalidad`
3. Commit de cambios: `git commit -m 'feat: agregar nueva funcionalidad'`
4. Push a la rama: `git push origin feature/nueva-funcionalidad`
5. Crear un **Pull Request** hacia la rama base acordada para el cambio (esta actualización documental parte de `main`)

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

### Identidad010-A

Arquitectura aprobada en [ADR-004](docs/adr/ADR-004-ciam-identity.md), [TP11 v1.2.0](.specify/memory/constitution.md). Código de 010-A implementado en `010-account-management-social-auth`: Google mediante External ID, Microsoft legacy conservado, `UserId` interno inmutable, JWT explícito/D1 en las tres APIs, perfil propio e aislamiento de sesión. Google real accede a Users/ShoppingList/ProductCatalog; los tokens Google directos, Graph e ID de CIAM son rechazados. [Validación y límites](specs/010-account-management-social-auth/evidence/validation.md).

**Alcance beta aprobado31/10:** Google sin Microsoft vía External ID y Microsoft legacy operativo. Los gates funcionales están acreditados; la CI del SHA final se verifica al publicar el PR. Microsoft→External ID deja de ser requisito beta: A-AC06/07 y T019/T048 permanecen diferidos, con [ensayo INCONCLUSIVE](specs/010-account-management-social-auth/evidence/migration-rehearsal.md) conservado. No se cambia identidad ni se ejecuta cutover. Lector/zoom completos y matriz manual exhaustiva multi-tab quedan como QA posterior no bloqueante. No hay merge ni despliegue productivo.

La ruta Google abre el acceso alojado de CIAM; allí hay que pulsar «Iniciar sesión con Google». No usa `domain_hint=google`, rechazado por este tenant. El selector QA conserva email OTP del spike; T056 es opcional y no habilita una opción email en la Web. Los perfiles ya creados con `unknown` se editan desde Perfil; no se rellenan administrativamente ni se sobrescriben al iniciar sesión. Los atributos de nombre del flujo QA se han preparado, pero una alta Google nueva debe confirmar su importación.

010-B (linking/unlinking, segundo método, G3, recuperación entre proveedores, avatar storage y otros proveedores) continúa diferida. MAUI productivo sigue aplazado; Web es el cliente prioritario.

Para probar010-A localmente se mantiene el entorno QA con Microsoft y Google; desde `MiKompri.Web`, usar `npm run dev:010a` con `.env.010a.local` privado ya preparado. Las APIs compatibles usan28082/28080/28083 y datos QA separados. Restaurar la configuración previa de la Web ocultaba Google y conectaba su preparación de cuenta a una API anterior que responde401 al sync Microsoft. [Arranque y límites](MiKompri.Web/README.md#acceso-y-límites-actuales-de-010-a). No se actualizan ni migran las bases anteriores por cambiar este modo.

Grupos/invitaciones (buscar, invitar, aceptar/rechazar, notificar y enlaces para no registrados) se tratarán en la iniciativa independiente prevista `012-group-invitations-collaboration`; no bloquean autenticación010-A.
