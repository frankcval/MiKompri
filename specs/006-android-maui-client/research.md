# Research: MVP-4 Cliente Android con .NET MAUI

Este documento consolida las decisiones técnicas necesarias para pasar de `spec.md` (ya clarificada en su totalidad) a un diseño de implementación. No quedan `NEEDS CLARIFICATION` pendientes: todas las incógnitas de producto fueron resueltas en las sesiones de clarificación del spec. Este research se enfoca en decisiones de *implementación técnica* derivadas de esas respuestas.

## 1. Librería de autenticación en el cliente

- **Decision**: Usar `Microsoft.Identity.Client` (MSAL.NET) con el flujo público de aplicación móvil (PKCE), integrado vía el paquete de MAUI/Android correspondiente.
- **Rationale**: Es el único mecanismo aceptado por la spec (FR-001, FR-016a); MSAL.NET es la librería oficial de Microsoft para Entra ID en clientes .NET, con soporte nativo de `AcquireTokenSilent`, caché cifrada y `RemoveAccount`.
- **Alternatives considered**: Implementar un cliente OIDC propio con `IdentityModel` — rechazado porque reimplementaría gestión de refresh tokens, violando FR-016a/FR-016b y aumentando superficie de riesgo de seguridad sin beneficio.

## 2. Estrategia de correlación de identidad en el backend

- **Decision**: Resolución de identidad centralizada en `Users.Api` (ya es el propietario exclusivo de la correlación, TP11), extendiendo `User` con `TenantId`/`ObjectId` opcionales y aplicando la lógica de Fase 1/Fase 2 ya descrita en `plan.md`.
- **Rationale**: La constitución exige que `Users` sea el único propietario de la correlación `(tid, oid) -> UserId`; ningún otro bounded context debe leer/escribir esa correlación directamente.
- **Alternatives considered**: Resolver `(tid, oid)` en cada API de forma independiente — rechazado por violar TP11 y duplicar lógica de correlación, con riesgo de inconsistencias entre bounded contexts.

## 3. Mecanismo de audience común durante la transición

- **Decision**: Reutilizar el patrón `ValidAudiences` (lista separada por comas) ya implementado en `Users.Api`, extendiéndolo a `ShoppingList.Api` y `ProductCatalog.Api` para aceptar temporalmente la audience histórica y la audience común durante la Fase 2.
- **Rationale**: Patrón ya probado en el código existente (`Users.Api/Program.cs`), evita downtime y no introduce una librería o mecanismo nuevo.
- **Alternatives considered**: Corte inmediato de audience sin periodo de convivencia — rechazado por alto riesgo de romper clientes ya emitidos (tokens en vuelo, apps no actualizadas).

## 4. Endurecimiento de `OwnerId` en listas personales

- **Decision**: Eliminar `OwnerId` del payload de entrada en los comandos de listas personales y resolverlo desde `ICurrentUserService.UserId` (interfaz ya existente en `MiKompri.ShoppingList.Application.Interfaces`), aplicando `[Authorize]` a los endpoints correspondientes.
- **Rationale**: `ICurrentUserService` ya existe en el proyecto y ya se usa en el patrón de listas compartidas (que sí tienen `[Authorize]`); reutilizar el mismo mecanismo mantiene consistencia arquitectónica.
- **Alternatives considered**: Validar `OwnerId` enviado por el cliente contra el token (en vez de ignorarlo) — rechazado porque la spec exige explícitamente que el cliente "no pueda enviar arbitrariamente" el owner; aceptar y validar sigue exponiendo superficie de ataque innecesaria frente a simplemente ignorarlo y derivarlo del servidor.

## 5. Arquitectura cliente: separación de capas para reutilización futura

- **Decision**: Tres módulos independientes en el cliente: `Auth` (adaptador MSAL específico de Android), `Http` (contratos tipados de API, agnósticos de plataforma) e `Identity` (modelo `(tid, oid)` + `UserId`, agnóstico de mecanismo de autenticación).
- **Rationale**: FR-016 exige que futuros clientes (Web, iOS) reutilicen contratos e identidad sin reutilizar la implementación de autenticación móvil; esta separación en capas permite sustituir solo `Auth` por un adaptador OIDC de navegador en Web sin tocar `Http` ni `Identity`.
- **Alternatives considered**: Cliente monolítico con MSAL acoplado directamente a las llamadas HTTP — rechazado porque impediría la reutilización exigida por FR-016 sin una reescritura significativa.

## 6. Manejo de estados de UI (loading/error/offline)

- **Decision**: Máquina de estados simple por pantalla (`Idle → Loading → Success | Error | Offline`), expuesta como propiedad observable en cada ViewModel, usando `Microsoft.Maui.Networking.Connectivity` para detectar el estado de red antes de ejecutar operaciones de escritura.
- **Rationale**: Cumple FR-013 de forma uniforme y testeable (SC-005: identificar el estado en <10s); `Connectivity` es la API estándar de MAUI, sin dependencias adicionales.
- **Alternatives considered**: Manejo de estado ad-hoc por pantalla sin convención común — rechazado por el riesgo de inconsistencia de UX y de cobertura de test dispersa.

## 7. Testing del cliente sin UI automation

- **Decision**: Cobertura de tests limitada a ViewModels y servicios (Auth/Http) mediante mocks/fakes; sin Appium ni UI Tests en MVP-4.
- **Rationale**: La spec no exige UI automation y prioriza un MVP acotado (Alcance); TP8 exige tests de dominio/aplicación/API, no necesariamente UI automation de cliente, por lo que esta decisión no viola la constitución.
- **Alternatives considered**: Incluir UI Tests con Appium desde este MVP — rechazado por ampliar el esfuerzo de MVP-4 sin un requisito explícito que lo justifique (evitar scope creep, según instrucción del usuario).

## 8. Alcance de Docker/CI

- **Decision**: Docker/CI obligatorio solo para los tres backends (`Users`, `ShoppingList`, `ProductCatalog`); el cliente Android se compila vía `dotnet build`/`publish` con el workload MAUI, sin contenedor.
- **Rationale**: TP4 exige Docker para servicios desplegables; el cliente Android no es un servicio desplegable en contenedor, por lo que la obligación no aplica técnicamente a ese artefacto. Instrucción explícita del usuario de limitar Docker/CI al backend existente.
- **Alternatives considered**: Forzar un contenedor de build para el cliente MAUI (por ejemplo, para estandarizar el entorno de compilación) — rechazado por complejidad desproporcionada frente al beneficio en este MVP; puede reconsiderarse en una spec futura si se requiere reproducibilidad estricta del entorno de build Android.

## 9. Autenticación real en `ProductCatalog.Api`

- **Decision**: Incorporar `AddAuthentication().AddJwtBearer(...)` en `MiKompri.ProductCatalog.Api/Program.cs`, con la misma `Authority` y soporte de `ValidAudiences` ya usado en `Users.Api`/`ShoppingList.Api`, más `UseAuthentication()` (hoy ausente) antes de `UseAuthorization()`, y `[Authorize]` en TODOS los endpoints existentes (consulta y escritura); el cliente solo consume los de consulta.
- **Rationale**: Hoy `ProductCatalog.Api` no valida JWT en absoluto (confirmado en `Program.cs`), lo cual es inconsistente con TP11 y con el resto del backend; el cliente Android consumiría un endpoint desprotegido si no se corrige, contradiciendo FR-017/FR-022/FR-023.
- **Alternatives considered**: Dejar `ProductCatalog.Api` sin autenticación por tratarse de datos "públicos" de solo lectura — rechazado porque TP11 exige que las tres APIs se traten como un único recurso lógico OAuth con controles de autenticación consistentes, y porque la spec (FR-023a) exige explícitamente esta protección.

## Resumen de incógnitas resueltas

Todas las decisiones anteriores derivan directamente de requisitos ya cerrados en `spec.md`; no quedan `NEEDS CLARIFICATION` abiertos para este plan.
