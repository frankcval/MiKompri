# Docker Gate Evidence - 002-shopping-list-core

## Fecha

2026-06-30

## Objetivo

Validar que el core de ShoppingList puede construirse y verificarse usando el flujo Docker definido por la constitución del proyecto.

## Artefactos verificados

- `docker-compose.yml`
- `docker-compose.override.yml`
- `MiKompri.ShoppingList.Api/Dockerfile`
- `MiKompri.ShoppingList.Api/appsettings.json`
- `MiKompri.ShoppingList.Api/appsettings.Development.json`

## Validaciones ejecutadas

1. Verificación de herramientas Docker disponibles en el entorno:
   - `docker --version`
   - `docker compose version`
2. Revisión de artefactos Docker y de la configuración de conexión de la API hacia PostgreSQL.
3. Intento de ejecutar el gate mínimo local:
   - `docker compose build mikomprishoppinglistapi`
   - `docker compose up -d postgres`
   - `docker exec mikompri_postgres pg_isready -U postgres -d MiKompri_ShoppingList`
   - `docker compose up -d mikomprishoppinglistapi`
   - `GET http://localhost:8080/health/details`
4. Validaciones complementarias ya ejecutadas fuera de Docker para el cierre funcional de la feature:
   - `dotnet test` de Domain, Application y API para ShoppingList
   - `dotnet build MiKompri.sln --configuration Release --no-restore`

## Resultado observado

- **Docker CLI y Docker Compose están instalados** en el entorno:
  - `Docker version 28.3.2, build 578ccf6`
  - `Docker Compose version v2.38.2-desktop.1`
- **El gate Docker no pudo completarse localmente** porque el daemon de Docker Desktop no estaba disponible al momento de la ejecución.
- Error observado al intentar ejecutar `docker compose build`:

```text
error during connect: Head "http://%2F%2F.%2Fpipe%2FdockerDesktopLinuxEngine/_ping": open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.
```

## Conclusión

El core de ShoppingList quedó **validado funcionalmente** mediante build y tests .NET, y los artefactos Docker necesarios para el gate están presentes en el repositorio.  
Sin embargo, la **evidencia de ejecución completa del gate Docker** queda abierta hasta repetir este flujo con Docker Desktop/daemon activo o automatizarlo en CI.

## Pendientes

- Repetir el gate local con el daemon de Docker activo.
- Automatizar este gate completamente en CI si todavía se ejecuta de forma manual.
