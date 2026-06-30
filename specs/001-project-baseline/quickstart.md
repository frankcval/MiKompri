# Quickstart — MiKompri Development Guide

**Spec**: [plan.md](plan.md) | **Fecha**: 2026-06-30

Guía para ejecutar, probar y desarrollar MiKompri localmente y entender el flujo
de trabajo con Spec Kit.

---

## Prerrequisitos

| Herramienta | Versión mínima | Verificar con |
|-------------|----------------|---------------|
| .NET SDK | 8.0 | `dotnet --version` |
| Docker Desktop | 4.x | `docker --version` |
| Docker Compose | 2.x (incluido en Docker Desktop) | `docker compose version` |
| Git | 2.x | `git --version` |
| PowerShell | 5.1+ (o pwsh 7+) | `$PSVersionTable` |

---

## 1. Clonar y configurar el entorno

```bash
git clone https://github.com/frankcval/MiKompri.git
cd MiKompri
```

Crear el override local de Docker Compose (no se commitea):
```bash
# Copiar el override de ejemplo si existiera, o crear uno vacío
# Este archivo puede sobreescribir variables de entorno sin commitear secretos
touch docker-compose.override.yml
```

---

## 2. Levantar el entorno local con Docker Compose

```bash
# Levantar ShoppingList API + PostgreSQL
docker compose up -d

# Verificar que los servicios están healthy
docker compose ps

# Ver logs en tiempo real
docker compose logs -f mikomprishoppinglistapi
```

La API quedará disponible en:
- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health

---

## 3. Ejecutar sin Docker (desarrollo activo)

```bash
# Restaurar dependencias
dotnet restore MiKompri.sln

# Build completo
dotnet build MiKompri.sln --configuration Release

# Levantar solo PostgreSQL con Docker
docker compose up postgres -d

# Ejecutar la API directamente
dotnet run --project MiKompri.ShoppingList.Api
```

La cadena de conexión por defecto en `appsettings.Development.json` debe apuntar a
`localhost:5432`.

---

## 4. Ejecutar los tests

```bash
# Todos los tests de la solución
dotnet test MiKompri.sln --configuration Release

# Solo tests de dominio (ShoppingList)
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj

# Solo tests de aplicación
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj

# Solo tests de integración de API
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj

# Test individual por nombre
dotnet test test\MiKompri.ShoppingList.Domain.Tests\... --filter "FullyQualifiedName~PurchaseListTests"

# Con cobertura
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=opencover `
  /p:CoverletOutput=.\TestResults\coverage.opencover.xml
```

---

## 5. Migraciones EF Core

> ⚠️ **Estado actual**: No existe carpeta `Migrations/` en ShoppingList.Infrastructure.
> Las migraciones se crearán como parte de la resolución de DT-002 (ADR-002).

```powershell
# Crear nueva migración (ejecutar desde la raíz del monorepo)
dotnet ef migrations add <NombreMigración> `
  --project MiKompri.ShoppingList.Infrastructure `
  --startup-project MiKompri.ShoppingList.Api `
  --output-dir Persistence/Migrations

# Aplicar migraciones a la BD local
dotnet ef database update `
  --project MiKompri.ShoppingList.Infrastructure `
  --startup-project MiKompri.ShoppingList.Api

# Verificar el estado de las migraciones
dotnet ef migrations list `
  --project MiKompri.ShoppingList.Infrastructure `
  --startup-project MiKompri.ShoppingList.Api
```

La variable de entorno `ConnectionStrings__PostgreSQL` o la entry en `appsettings.json`
debe estar configurada para que los comandos `ef` funcionen.

---

## 6. Validar el escenario principal (ShoppingList API)

Con el entorno levantado, verificar el flujo básico con los endpoints:

**Crear una lista**:
```bash
curl -X POST http://localhost:8080/api/v1/PurchaseLists \
  -H "Content-Type: application/json" \
  -d '{"name": "Compra semanal", "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"}'
# Esperado: 201 Created con el ID de la lista
```

**Agregar un ítem**:
```bash
LIST_ID="<id-retornado-arriba>"
curl -X POST http://localhost:8080/api/v1/PurchaseLists/$LIST_ID/items \
  -H "Content-Type: application/json" \
  -d '{"productId": "aabb1234-0000-0000-0000-000000000001", "name": "Leche", "price": 1.50, "quantity": 2}'
# Esperado: 201 Created
```

**Consultar la lista**:
```bash
curl http://localhost:8080/api/v1/PurchaseLists/$LIST_ID
# Esperado: 200 OK con la lista y progreso
```

---

## 7. Crear una nueva spec con Spec Kit

El flujo completo para iniciar una nueva feature:

```text
Paso 1 — Especificar (qué y por qué):
  Abrir GitHub Copilot Chat y ejecutar:
  /speckit.specify <descripción de la feature>

Paso 2 — Planificar (cómo y qué decisiones técnicas):
  /speckit.plan

Paso 3 — Generar tareas (ítems ejecutables):
  /speckit.tasks

Paso 4 — Implementar (guiado por tasks.md):
  /speckit.implement

Paso 5 — Validar antes de PR:
  /speckit.checklist
```

Los artefactos se crean en `specs/{NNN}-{feature}/`.

---

## 8. Convención de commits

```
feat: <descripción breve> [refs specs/{NNN}]
fix: <descripción breve>
docs: <descripción breve>
test: <descripción breve>
chore: <descripción breve>
refactor: <descripción breve>
```

---

## 9. Estructura del pipeline CI (referencia rápida)

```text
Trigger: push/PR a main, develop, feature/*, hotfix/*

1. dotnet restore
2. dotnet build --configuration Release
3. dotnet test (3 proyectos: Domain, Application, Api.Tests)
   └── Con cobertura Coverlet (opencover)
4. SonarCloud analysis (begin → build → end)

Badge: [![CI](https://github.com/frankcval/MiKompri/actions/workflows/ci-mikompri-shoppinglist.yml/badge.svg)]
```

---

## 10. Troubleshooting frecuente

| Problema | Causa probable | Solución |
|----------|---------------|----------|
| `Connection refused` al iniciar API | PostgreSQL no está listo | `docker compose up postgres -d` primero; esperar healthcheck |
| Tests de integración fallan en CI | BD InMemory no configurada | Verificar que `CustomWebApplicationFactory` usa EF InMemory |
| `dotnet ef` no encontrado | EF Core Tools no instalado | `dotnet tool install --global dotnet-ef` |
| Swagger no visible en producción | `if (IsDevelopment)` condicional | Solo disponible en Development; usar Swagger en entorno de staging |
| Build falla con error de encoding | Caracteres especiales en comentarios | Ya existen comentarios con `?` en el código; no crítico para build |
