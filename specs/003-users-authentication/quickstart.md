# Quickstart: 003 — Users Authentication & Groups

**Propósito**: Guía de validación end-to-end. No incluye código de implementación — los pasos de ejecución asumen que la feature ya está implementada.

---

## Prerequisitos

| Requisito | Verificación |
|-----------|-------------|
| .NET 8 SDK | `dotnet --version` → `8.x` |
| Docker Desktop corriendo | `docker info` sin error |
| PostgreSQL accesible (vía Docker) | Ver sección de arranque |
| Token JWT válido de proveedor OIDC | Obtener con `curl` o Postman según proveedor configurado |

---

## 1. Arranque del entorno local

```powershell
# Desde la raíz del repositorio
docker compose up -d postgres
```

Las APIs pueden arrancarse desde Visual Studio (F5) o desde terminal:

```powershell
# Terminal 1 — ShoppingList API (si se necesita)
dotnet run --project MiKompri.ShoppingList.Api --launch-profile http

# Terminal 2 — Users API
dotnet run --project MiKompri.Users.Api --launch-profile http
```

> La Users API ejecuta `Database.Migrate()` al arrancar, creando el esquema de `MiKompri_Users` si no existe.

---

## 2. Obtener un token de prueba

Para tests de integración el `WebApplicationFactory` reemplaza la autenticación con un token JWT de prueba generado internamente.

Para pruebas manuales contra la API real, obtener un token del proveedor OIDC configurado (`appsettings.Development.json → Authentication:Authority`).

```powershell
$TOKEN = "eyJ..."   # Pegar token aquí
$HEADERS = @{ Authorization = "Bearer $TOKEN" }
$BASE = "http://localhost:5XXX/api/v1"  # Puerto configurado en launchSettings.json
```

---

## 3. Escenarios de validación

### Escenario 1 — Auto-provisioning y consulta de perfil (FR-002, FR-003)

**Objetivo**: Verificar que el primer request autenticado crea el perfil local.

```powershell
# GET /users/me  — primer request
Invoke-RestMethod -Uri "$BASE/users/me" -Headers $HEADERS -Method GET
```

**Resultado esperado `200 OK`**:
```json
{
  "id": "<uuid>",
  "displayName": "<nombre del claim>",
  "email": "<email del claim o null>",
  "createdAt": "<timestamp>",
  "updatedAt": "<timestamp>"
}
```

```powershell
# Segundo request — no debe duplicar el perfil
Invoke-RestMethod -Uri "$BASE/users/me" -Headers $HEADERS -Method GET
# El id debe ser el mismo
```

**Request sin token → 401**:
```powershell
Invoke-RestMethod -Uri "$BASE/users/me" -Method GET   # sin header
# StatusCode: 401
```

---

### Escenario 2 — Actualización de nombre visible (FR-004)

```powershell
$body = '{ "displayName": "Ana García López" }' | ConvertTo-Json -Compress
Invoke-RestMethod -Uri "$BASE/users/me" -Headers $HEADERS -Method PUT `
  -Body '{"displayName":"Ana García López"}' -ContentType "application/json"
```

**Resultado esperado `200 OK`**: perfil con `displayName` actualizado y `updatedAt` mayor que `createdAt`.

**Nombre vacío → 400**:
```powershell
Invoke-RestMethod -Uri "$BASE/users/me" -Headers $HEADERS -Method PUT `
  -Body '{"displayName":"   "}' -ContentType "application/json"
# StatusCode: 400 con errors[0].field = "DisplayName"
```

---

### Escenario 3 — Sincronización de claims desde IdP (FR-016)

```powershell
# Primera llamada (si el perfil no existía aún) → 201 Created
Invoke-RestMethod -Uri "$BASE/users/me/sync" -Headers $HEADERS -Method POST

# Segunda llamada con mismo token (perfil ya existe) → 200 OK
Invoke-RestMethod -Uri "$BASE/users/me/sync" -Headers $HEADERS -Method POST
```

**Resultado esperado primera llamada `201 Created`**: perfil con `displayName` y/o `email` tomados de los claims del token.

**Resultado esperado segunda llamada `200 OK`**: perfil con los mismos datos (idempotente en claims sin cambios).

---

### Escenario 4 — Crear grupo y verificar Owner automático (FR-005, FR-006)

```powershell
$group = Invoke-RestMethod -Uri "$BASE/groups" -Headers $HEADERS -Method POST `
  -Body '{"name":"Familia García"}' -ContentType "application/json"

$groupId = $group.id

# Verificar membresía
$members = Invoke-RestMethod -Uri "$BASE/groups/$groupId/members" -Headers $HEADERS
$members | Where-Object { $_.role -eq "Owner" }  # debe aparecer el caller
```

**Resultado esperado**: exactamente 1 miembro con rol `"Owner"` igual al perfil del caller.

**Nombre vacío → 400**:
```powershell
Invoke-RestMethod -Uri "$BASE/groups" -Headers $HEADERS -Method POST `
  -Body '{"name":""}' -ContentType "application/json"
# StatusCode: 400
```

---

### Escenario 5 — Gestión de miembros: matriz de privilegios (FR-007, FR-009, Q2)

Requiere dos usuarios con tokens distintos (UserA = Owner, UserB = Member, UserC = candidato).

```powershell
# Owner agrega Admin (debe funcionar)
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members" -Headers $HEADERS_OWNER -Method POST `
  -Body "{`"userId`":`"$userBId`",`"role`":`"Admin`"}" -ContentType "application/json"

# Owner agrega Member
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members" -Headers $HEADERS_OWNER -Method POST `
  -Body "{`"userId`":`"$userCId`",`"role`":`"Member`"}" -ContentType "application/json"

# Admin intenta agregar Admin → 403
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members" -Headers $HEADERS_ADMIN -Method POST `
  -Body "{`"userId`":`"<otro-uuid>`",`"role`":`"Admin`"}" -ContentType "application/json"
# StatusCode: 403

# Admin elimina Member → 204
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members/$userCId" -Headers $HEADERS_ADMIN -Method DELETE
# StatusCode: 204

# Admin intenta eliminar Admin → 403
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members/$userBId" -Headers $HEADERS_ADMIN -Method DELETE
# StatusCode: 403

# Member intenta agregar → 403
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members" -Headers $HEADERS_MEMBER -Method POST `
  -Body "{`"userId`":`"<uuid>`",`"role`":`"Member`"}" -ContentType "application/json"
# StatusCode: 403
```

---

### Escenario 6 — No se puede eliminar el único Owner (FR-010)

```powershell
# Intentar eliminar al Owner siendo el único (el grupo creado en Escenario 4)
# Primero remover todos los admins/members hasta que solo quede el Owner
$ownerId = ($members | Where-Object { $_.role -eq "Owner" }).userId
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members/$ownerId" `
  -Headers $HEADERS_OWNER -Method DELETE
# StatusCode: 400 — "No se puede eliminar al propietario del grupo."
```

---

### Escenario 7 — Membresía duplicada rechazada (FR-008)

```powershell
# Agregar un usuario que ya es miembro
Invoke-RestMethod -Uri "$BASE/groups/$groupId/members" -Headers $HEADERS_OWNER -Method POST `
  -Body "{`"userId`":`"$userBId`",`"role`":`"Member`"}" -ContentType "application/json"
# StatusCode: 400 — "El usuario ya pertenece al grupo."
```

---

### Escenario 8 — Listar grupos del usuario autenticado (FR-017)

```powershell
# GET /groups — sólo grupos del caller
$myGroups = Invoke-RestMethod -Uri "$BASE/groups" -Headers $HEADERS_OWNER -Method GET
$myGroups   # array con los grupos donde el caller es miembro
```

**Resultado esperado `200 OK`**:
```json
[
  {
    "id": "<uuid>",
    "name": "Familia García",
    "myRole": "Owner",
    "memberCount": 2,
    "createdAt": "<timestamp>"
  }
]
```

```powershell
# Un usuario sin membresía en ningún grupo devuelve array vacío, no 404
$myGroups = Invoke-RestMethod -Uri "$BASE/groups" -Headers $HEADERS_NEW_USER -Method GET
# [] (array vacío)
```

---

## 4. Ejecutar tests automatizados

```powershell
# Todos los tests de la solución
dotnet test MiKompri.sln --configuration Release

# Solo tests del bounded context Users
dotnet test test\MiKompri.Users.Domain.Tests\MiKompri.Users.Domain.Tests.csproj
dotnet test test\MiKompri.Users.Application.Tests\MiKompri.Users.Application.Tests.csproj
dotnet test test\MiKompri.Users.Api.Tests\MiKompri.Users.Api.Tests.csproj
```

**Resultado esperado**: 0 fallos. Cobertura de dominio objetivo: ≥ 80%.

---

## 5. Verificar Docker Compose

```powershell
docker compose up --build -d
docker compose ps  # mikompriusersapi debe estar "Up (healthy)"
docker compose logs mikompriusersapi --tail=50
```

Verificar health check:
```powershell
Invoke-RestMethod -Uri "http://localhost:8082/health"
# { "status": "Healthy" }
```

---

## Registro de validación (Phase 10)

- **Fecha**: 2026-07-03
- **Entorno**: Docker Compose local (`mikompriusersapi` en `http://localhost:8082`, PostgreSQL en contenedor)
- **Condiciones verificadas**:
  - Build solución Release OK (`dotnet build MiKompri.sln --configuration Release --no-restore`)
  - Tests Users OK (Domain/Application/API)
  - Health endpoint OK (`/health` => `{ "status": "Healthy" }`)
  - Swagger UI y esquema OpenAPI con seguridad Bearer visibles (`/swagger`, `/swagger/v1/swagger.json`)
  - Cobertura funcional de escenarios 1–8 validada por suite automatizada de `MiKompri.Users.Api.Tests` (`ProfileApiTests` + `GroupsApiTests`) y smoke manual en entorno Docker para autenticación y disponibilidad.

## Referencias

- Contrato REST: [contracts/users-api.md](./contracts/users-api.md)
- Modelo de datos: [data-model.md](./data-model.md)
- Plan técnico: [plan.md](./plan.md)
