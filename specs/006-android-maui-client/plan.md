# Implementation Plan: MVP-4 Cliente Android con .NET MAUI

**Branch**: `006-android-maui-client` | **Date**: 2026-10-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/006-android-maui-client/spec.md`

**Note**: Este plan respeta estrictamente las decisiones ya cerradas en `spec.md` (sesiones de clarificación 2026-09-30, 2026-10-01 y 2026-10-02) y en la Constitución v1.1.0 (TP1-TP11). No reabre ninguna decisión ya aclarada, no amplía el alcance de MVP-4 y no incluye código de implementación.

## Summary

Construir el primer cliente móvil de MiKompri (Android, .NET MAUI) que consume `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api` mediante login con Microsoft Entra ID gestionado por MSAL. En paralelo, se endurece el backend para cumplir TP11: se ejecuta la migración de identidad `sub -> (tid, oid)` en dos fases (convivencia y corte), se deriva el `OwnerId` de listas personales exclusivamente desde el token autenticado, y se consolida una audience lógica común para las tres APIs. El cliente incluye navegación principal (Listas, Grupos, Catálogo, Perfil), gestión mínima de grupos, consumo de listas personales/compartidas con balances y liquidación, y consulta de solo lectura del catálogo de productos. No se introduce sincronización offline bidireccional ni nuevos bounded contexts.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (backend); C# 12 / .NET 8 con target `net8.0-android` (cliente MAUI, conforme a TP6).

**Primary Dependencies**:
- Backend: ASP.NET Core Web API, MediatR (CQRS), EF Core + Npgsql (PostgreSQL), FluentValidation, Serilog, JwtBearer (`Microsoft.AspNetCore.Authentication.JwtBearer`).
- Cliente: .NET MAUI, `Microsoft.Identity.Client` (MSAL.NET) para Android, `HttpClient` tipado por API (`IHttpClientFactory`), MVVM (`CommunityToolkit.Mvvm` o equivalente ya alineado con el stack .NET). `SecureStorage` de MAUI solo se usa como superficie nativa subyacente que MSAL emplea internamente para su caché cifrada; la app no accede a él directamente para tokens (FR-002/FR-016b).

**Storage**: PostgreSQL por bounded context (`Users`, `ShoppingList`, `ProductCatalog`), sin cambios de motor. Cliente: sin almacenamiento propio de tokens (caché MSAL gestionada internamente); solo preferencias no sensibles (URLs de entorno) si se requieren localmente.

**Testing**:
- Backend: xUnit + FluentAssertions/Moq (consistente con `test/MiKompri.*.Tests` existentes), EF Core InMemory para integración de API vía `CustomWebApplicationFactory`, cobertura con Coverlet/OpenCover como en CI actual.
- Cliente: tests unitarios de ViewModels y servicios de aplicación cliente (sin UI automation en este MVP, ver Fuera de alcance).

**Target Platform**: Backend en contenedores Linux (Docker, despliegue objetivo Azure per TP5). Cliente: Android targeting `net8.0-android`, UI de referencia en dispositivos de hasta 430px de ancho (PP5).

**Project Type**: Mobile + API — se añade un cuarto proyecto (cliente MAUI) a la solución existente de bounded contexts backend; no se introduce un quinto framework ni un backend-for-frontend adicional.

**Performance Goals**: Alineados a los Success Criteria de la spec (SC-001 a SC-007): login completo <1 min en red normal; creación de lista + ítem + marcar comprado <2 min; identificación de estado (cargando/error/sin conexión) <10s.

**Constraints**:
- No offline bidireccional (FR-015).
- No refresh tokens gestionados manualmente; toda la gestión de tokens es responsabilidad exclusiva de MSAL vía `AcquireTokenSilent`/interactivo/`RemoveAccount` (FR-002 a FR-004, FR-016a, FR-016b).
- `OwnerId` de listas personales nunca proviene del cliente; se deriva del `UserId` resuelto por el backend a partir del token (FR-026, FR-027).
- Migración de identidad en dos fases sin duplicar `UserId` (FR-020, FR-021).
- Gestión de grupos limitada a listar/crear/ver miembros/añadir-eliminar miembro (sin administración avanzada ni cambio de rol) (FR-016c).
- ProductCatalog es solo lectura en este MVP (FR-016d).

**Scale/Scope**: Alcance de MVP-4 tal como está definido en `spec.md` § Alcance: 1 app Android, 3 APIs backend consumidas, ~10 historias de usuario, sin Web/iOS, sin OCR/push/pagos/recomendaciones.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **TP1 (Backend .NET)**: Cumple — todo el backend ya es .NET 8; ningún cambio de runtime.
- **TP2 (Bounded contexts)**: Cumple — no se crean nuevos bounded contexts; el cliente MAUI consume `Users.Api`, `ShoppingList.Api`, `ProductCatalog.Api` exclusivamente vía sus APIs públicas (REST), sin acceso directo a bases de datos de otro contexto.
- **TP3 (Monorepo)**: Cumple — el proyecto MAUI vive en el mismo repositorio `frankcval/MiKompri`.
- **TP4 (Docker obligatorio)**: Cumple para el backend (build/test en contenedores); el cliente Android MAUI no es dockerizable de forma nativa (limitación de la plataforma AOT/Android), por lo que el alcance de Docker/CI en este plan se limita explícitamente al backend existente (ver sección "Docker/CI"), conforme a lo solicitado por el usuario.
- **TP5 (Azure)**: Sin cambios — el backend sigue desplegándose en Azure; este MVP no introduce nueva infraestructura de despliegue del cliente (distribución fuera de Google Play, per Alcance).
- **TP6 (Cliente Android .NET MAUI)**: Cumple directamente — es el objeto central de esta spec/plan.
- **TP7 (REST + OpenAPI)**: Cumple — no se cambian contratos existentes salvo lo estrictamente necesario para TP11 (ver Fase 1/Fase 2 de migración); Swagger se mantiene disponible en no-productivo.
- **TP8 (Testing obligatorio)**: Se exige cobertura de dominio/aplicación/integración para los cambios de backend (migración de identidad, endurecimiento de `OwnerId`, grupos) y tests de ViewModels/servicios en el cliente. Ver sección "Estrategia de Tests".
- **TP9 (ADR)**: Las decisiones de migración en dos fases y de audience común ya están documentadas en `spec.md` (Clarifications) y se detallan aquí; no se requiere ADR adicional salvo que surja una decisión no cubierta por la spec durante la implementación.
- **TP10 (Spec-first)**: Cumple — `spec.md` existe y está clarificado; este `plan.md` se produce antes de `tasks.md` e implementación.
- **TP11 (Identidad transversal)**: Es el eje central del plan — Entra ID como IdP, migración `sub -> (tid, oid)` en dos fases, `UserId` interno estable, audience lógica común, `OwnerId` derivado del token. Ver sección "Identidad y Migración".

No se detectan violaciones constitucionales que requieran justificación en "Complexity Tracking".

## Project Structure

### Documentation (this feature)

```text
specs/006-android-maui-client/
├── plan.md              # Este archivo
├── research.md          # Fase 0 (decisiones técnicas y alternativas)
├── data-model.md         # Fase 1 (entidades cliente + ajustes de correlación de identidad)
├── quickstart.md         # Fase 1 (guía de validación end-to-end)
├── contracts/            # Fase 1 (contratos HTTP consumidos/ajustados)
└── tasks.md              # Fase 2 (/speckit.tasks - no generado por este comando)
```

### Source Code (repository root)

```text
# Backend existente (bounded contexts ya desplegados, ajustes puntuales para TP11)
MiKompri.Users.Api/
MiKompri.Users.Application/
MiKompri.Users.Domain/
MiKompri.Users.Infrastructure/

MiKompri.ShoppingList.Api/
MiKompri.ShoppingList.Application/
MiKompri.ShoppingList.Domain/
MiKompri.ShoppingList.Infrastructure/

MiKompri.ProductCatalog.Api/
MiKompri.ProductCatalog.Application/
MiKompri.ProductCatalog.Domain/
MiKompri.ProductCatalog.Infrastructure/

test/
├── MiKompri.Users.Domain.Tests/
├── MiKompri.Users.Application.Tests/
├── MiKompri.Users.Api.Tests/
├── MiKompri.ShoppingList.Domain.Tests/
├── MiKompri.ShoppingList.Application.Tests/
├── MiKompri.ShoppingList.Api.Tests/
├── MiKompri.ProductCatalog.Domain.Tests/
├── MiKompri.ProductCatalog.Application.Tests/
└── MiKompri.ProductCatalog.Api.Tests/

# Nuevo: cliente móvil (Mobile + API, opción 3)
MiKompri.Mobile/                         # Proyecto .NET MAUI (net8.0-android)
├── Platforms/Android/
├── App.xaml / App.xaml.cs
├── Services/
│   ├── Auth/                            # Adaptador MSAL (IAuthService), sin lógica propia de refresh tokens
│   ├── Http/                            # Clientes tipados: UsersApiClient, ShoppingListApiClient, ProductCatalogApiClient
│   └── Navigation/
├── ViewModels/
│   ├── Login/
│   ├── Profile/
│   ├── PersonalLists/
│   ├── Items/
│   ├── Groups/
│   ├── SharedLists/
│   ├── Settlement/
│   └── Catalog/
├── Views/                               # XAML correspondientes a cada ViewModel
├── Models/                              # DTOs de cliente (proyección de contratos de API)
├── Configuration/                       # appsettings por entorno (URLs base de las 3 APIs)
└── Resources/

test/
└── MiKompri.Mobile.Tests/               # Tests de ViewModels/servicios (sin UI automation en MVP-4)
```

**Structure Decision**: Se adopta la variante "Mobile + API" del template: el backend existente permanece intacto en su organización por bounded context (`Api`/`Application`/`Domain`/`Infrastructure`), y se añade un único proyecto cliente `MiKompri.Mobile` (.NET MAUI, `net8.0-android`) más su proyecto de tests `MiKompri.Mobile.Tests`, ambos integrados en `MiKompri.sln`. No se crean nuevos bounded contexts backend; los únicos cambios de backend son ajustes acotados dentro de `Users` (migración de identidad) y `ShoppingList` (endurecimiento de `OwnerId`), documentados a continuación.

---

## Identidad y Migración `sub -> (tid, oid)` (TP11)

Esta sección traduce las decisiones ya cerradas en `spec.md` (FR-017 a FR-027) a un plan técnico concreto, sin reabrir ninguna de ellas.

### Estado actual relevante

- `MiKompri.Users.Domain.Users.User` persiste `IdentityProvider` + `ExternalUserId` (`sub`) como única correlación externa hoy.
- `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api` validan JWT vía `JwtBearerDefaults` con `Authority`/`Audience` configurados por `appsettings`; `Users.Api` ya soporta `ValidAudiences` múltiples (mecanismo reutilizable para la transición de audience).
- `PurchaseList.OwnerId` es un `Guid` de dominio ya presente; el endpoint `POST /api/v1/PurchaseLists` actualmente recibe `OwnerId` en el `CreatePurchaseListRequest` (payload del cliente) — este es el punto concreto a endurecer para FR-026/FR-027.

### Fase 1 — Convivencia (sin cambiar correlación canónica)

1. Extender `User` (Domain) con campos opcionales `TenantId` (`tid`) y `ObjectId` (`oid`), nulos por defecto, sin alterar `ExternalUserId`.
2. En el punto de resolución de identidad de `Users.Api` (ejecutado en cualquier request autenticado), al recibir un token con claims `tid`/`oid`:
   - Si el usuario ya existe (correlacionado por `sub`) y no tiene `tid`/`oid` persistidos, persistirlos contra ese mismo `UserId` (actualización, no creación).
   - Si el usuario no existe por `sub` ni por `(tid, oid)`, crear un perfil nuevo correlacionado directamente por `(tid, oid)` (caso de usuario nuevo, no legacy).
   - Nunca crear un `UserId` nuevo si ya existe correlación por `sub` o por `(tid, oid)` (FR-020).
3. La audience/issuer de validación de tokens permanece igual a la configuración actual durante la Fase 1 (sin adoptar todavía la audience común).
4. Todas las operaciones de negocio (Users, ShoppingList, ProductCatalog) continúan usando el `UserId` interno ya asignado (FR-021); `sub` sigue siendo legible pero no se usa como clave para nuevas resoluciones.

### Fase 2 — Corte (adopción de correlación canónica)

1. Una vez verificado que los perfiles activos relevantes tienen `tid`/`oid` resueltos (criterio operativo, verificable vía consulta/reporte, no automatizado por esta spec), cambiar la resolución de identidad en `Users.Api` para que la búsqueda primaria sea por `(tid, oid)`, usando `sub` solo como fallback de solo lectura para perfiles aún no migrados.
2. Adoptar la audience de backend común (TP11) como mecanismo principal de validación en `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api`, reutilizando el mecanismo `ValidAudiences` ya existente en `Users.Api` para una transición sin downtime (aceptar temporalmente audience antigua + audience común, luego retirar la antigua).
3. Marcar `ExternalUserId`/`sub` como dato legacy de solo lectura en el dominio (sin eliminarlo, para trazabilidad histórica).

### Riesgo y mitigación

- **Riesgo**: perfiles que nunca vuelven a autenticarse quedan sin `tid`/`oid` resuelto indefinidamente. **Mitigación**: fuera de alcance de este MVP definir un job de backfill masivo; se documenta como seguimiento futuro (no bloquea Fase 2 para usuarios activos).
- **Riesgo**: cambio de audience rompe clientes ya emitidos. **Mitigación**: uso de `ValidAudiences` para aceptar ambas audiences durante la transición, igual que el patrón ya usado entre `ShoppingList` y `Users`.

---

## Endurecimiento de Listas Personales (`OwnerId`)

1. En `ShoppingList.Api`, el `CreateShoppingListCommand` (y los comandos de edición/eliminación/consulta de listas personales) deja de aceptar `OwnerId` desde el payload del cliente; el `OwnerId` se obtiene exclusivamente de `ICurrentUserService.UserId` (interfaz ya existente en `MiKompri.ShoppingList.Application.Interfaces`), resuelto por el backend a partir del token autenticado.
2. Todos los endpoints de listas personales (`GetAll` por `ownerId`, `Create`, actualización, eliminación) pasan a requerir `[Authorize]` (hoy `Create`/`GetAll` no lo tienen explícitamente, a diferencia de los endpoints de listas compartidas que sí lo usan) — alineado con FR-027.
3. Las consultas por `ownerId` dejan de aceptar un `ownerId` arbitrario por query string para listas personales; se reemplaza por el `UserId` resuelto del token, rechazando con 403/404 (según convención ya usada en `ExceptionHandlingMiddleware`) cualquier intento de acceso a listas de otro `OwnerId` (FR-026).
4. Compatibilidad: las listas personales ya existentes no requieren migración de datos (su `OwnerId` ya es válido); solo cambia la fuente de verdad para nuevas operaciones (spec `Acceptance Scenario 6`).

---

## Gestión Básica de Grupos (cliente)

El backend (`Users.Api` / `GroupsController`) ya expone `POST /api/v1/groups`, `GET /api/v1/groups`, `GET /api/v1/groups/{id}/members`, `POST /api/v1/groups/{id}/members` y `DELETE` de miembro, todos bajo `[Authorize]` y con la matriz Owner/Admin/Member de Spec 003. El cliente MAUI únicamente añade una capa de consumo (`GroupsApiClient` + ViewModels/Views) para:

- Listar grupos propios con rol mostrado (`GET /api/v1/groups`).
- Crear grupo (`POST /api/v1/groups`).
- Ver miembros (`GET /api/v1/groups/{id}/members`).
- Añadir/eliminar miembro respetando permisos ya validados por el backend (la UI deshabilita acciones no permitidas para el rol actual, pero la autorización real sigue siendo responsabilidad exclusiva del backend, FR-025).

No se implementa cambio de rol ni administración avanzada de grupo (fuera de alcance, FR-016c), porque el backend actual no expone esas capacidades.

---

## ProductCatalog (solo lectura)

El cliente consume únicamente los endpoints de consulta ya existentes en `ProductCatalog.Api` (productos activos, mercados activos, historial de precios). No se añade ningún endpoint de creación/edición al backend ni pantallas de escritura al cliente (FR-016d, FR-011).

---

## Arquitectura de Navegación, Servicios HTTP y Estados

- **Navegación**: Shell de .NET MAUI con pestañas/rutas para Listas, Grupos, Catálogo y Perfil (FR-012), usando navegación declarativa estándar de MAUI Shell.
- **Servicios HTTP**: un cliente tipado por bounded context (`UsersApiClient`, `ShoppingListApiClient`, `ProductCatalogApiClient`) registrado vía `IHttpClientFactory`, cada uno con `BaseAddress` configurable por entorno (FR-014) y un `DelegatingHandler` común que invoca `AcquireTokenSilent` de MSAL antes de adjuntar el `Authorization: Bearer` (FR-003), delegando a login interactivo solo si falla.
- **Manejo de loading/error/offline**: patrón MVVM con un estado de UI explícito por pantalla (`Idle/Loading/Success/Error/Offline`), consumido de forma uniforme por las Views (FR-013). La detección de "sin conexión" usa `Microsoft.Maui.Networking.Connectivity` antes de intentar llamadas de escritura; los errores HTTP (401/403/4xx/5xx) se mapean a mensajes de error distinguibles de fallos de conectividad (Edge Cases de la spec).
- **Capas de identidad reutilizables (FR-016)**: se separan en `Auth/` (adaptador MSAL específico de Android), `Http/` (contratos de API, reutilizables por futuros clientes) y un módulo de identidad canónica (`(tid, oid)` + `UserId`) que no depende del mecanismo de autenticación concreto, preparando la reutilización futura por Web/iOS mediante adaptadores propios.

---

## Estrategia de Tests (TP8)

### Backend

- **Dominio** (`MiKompri.Users.Domain.Tests`, `MiKompri.ShoppingList.Domain.Tests`): tests para la extensión de `User` (captura de `tid`/`oid` sin duplicar perfil, comportamiento de Fase 1/Fase 2) y para `PurchaseList` si se introduce alguna regla de dominio adicional ligada a `OwnerId` inmutable.
- **Aplicación** (`MiKompri.Users.Application.Tests`, `MiKompri.ShoppingList.Application.Tests`): tests de los handlers de resolución de identidad (casos: usuario nuevo, usuario legacy por `sub`, usuario ya migrado por `(tid, oid)`) y de los comandos de listas personales (verificar que `OwnerId` se toma de `ICurrentUserService` y no del request).
- **Integración de API** (`MiKompri.Users.Api.Tests`, `MiKompri.ShoppingList.Api.Tests`): tests end-to-end contra `CustomWebApplicationFactory` cubriendo: (a) rechazo 401 sin token en endpoints de listas personales, (b) rechazo de acceso cruzado entre `OwnerId` distinto al autenticado, (c) flujo de migración Fase 1 con token simulado que incluye `tid`/`oid`, (d) endpoints de grupos ya cubiertos si no existen, añadir cobertura mínima de listar/crear/ver miembros/añadir-eliminar.
- Estos tests se ejecutan con los comandos ya definidos en `.github/copilot-instructions.md` (`dotnet test ... --configuration Release`) y se integran en el pipeline de cobertura existente (Coverlet/OpenCover) sin introducir un nuevo formato.

### Cliente (`MiKompri.Mobile.Tests`)

- Tests unitarios de ViewModels (por ejemplo, `LoginViewModel`, `PersonalListsViewModel`, `GroupsViewModel`) mockeando los clientes HTTP y el adaptador MSAL (`IAuthService`), verificando transiciones de estado (Loading/Success/Error/Offline) y que ninguna llamada de creación de lista envía un `OwnerId` explícito.
- Tests del `DelegatingHandler` de autenticación verificando el orden `AcquireTokenSilent` → fallback interactivo, sin acceder a APIs reales (usando fakes del SDK de MSAL).
- No se incluyen pruebas de UI automation (Appium/UI Tests) en MVP-4: queda fuera de alcance explícito para mantener el MVP acotado; se documenta como posible extensión futura.

---

## Docker/CI

- **Backend**: sin cambios estructurales al pipeline existente (`ci-mikompri-shoppinglist.yml`, `ci-mikompri-productcatalog.yml` y el equivalente de `Users`); se añaden/ajustan pasos de test para cubrir los nuevos casos de identidad y autorización de listas personales dentro de los jobs ya existentes (`dotnet test` + cobertura Coverlet/OpenCover), conforme a TP4/TP8.
- **Cliente Android**: TP4 (Docker obligatorio) no aplica al build/empaquetado del cliente MAUI para Android, dado que el SDK de Android y el proceso de firma/empaquetado no son equivalentes a un servicio backend desplegable en contenedor; el build del cliente se ejecuta mediante `dotnet build`/`dotnet publish` con el workload MAUI en el runner de CI (sin `docker build` para este artefacto). Esta exclusión queda documentada aquí como la justificación requerida por TP9, sin requerir ADR adicional al no tratarse de un backend desplegable.
- No se introduce despliegue a tiendas de aplicaciones (Google Play) en este MVP (Alcance ya definido), por lo que no se agrega step de publicación de store al CI.

## Complexity Tracking

> No hay violaciones constitucionales que requieran justificación. La única desviación de TP4 (Docker) está documentada y justificada en la sección "Docker/CI" anterior, no como violación sino como no-aplicabilidad técnica al tipo de artefacto (cliente móvil Android vs. servicio backend).

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |
