# Research: Baseline del Proyecto MiKompri

**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Fecha**: 2026-06-30

> Este documento resuelve las cinco decisiones arquitecturales abiertas identificadas
> en el Technical Context del plan. Cada decisión sigue el formato:
> Decision / Rationale / Alternatives considered.

---

## Decisión 1 — Plataforma de despliegue en Azure

**Contexto**: El pipeline CD actual solo publica la imagen Docker a GHCR. No existe
paso de despliegue en Azure. La constitución (TP5) establece Azure como destino
obligatorio. Se necesita elegir el servicio Azure antes de diseñar el pipeline CD.

**Decision**: **Azure Container Apps (ACA)**

**Rationale**:
- Sin gestión de clúster Kubernetes (a diferencia de AKS). Adecuado para el tamaño
  actual del equipo y del proyecto.
- Escala a cero: el costo es cero cuando no hay tráfico, óptimo para etapa temprana.
- Soporte nativo de imágenes Docker desde GHCR mediante credenciales de registry.
- El action `azure/container-apps-deploy-action` integra directamente con GitHub Actions.
- DAPR disponible como opción futura si se necesita comunicación entre bounded contexts
  vía mensajería sin cambiar la infraestructura.
- Azure Database for PostgreSQL Flexible Server como base de datos gestionada en el
  mismo entorno.
- OIDC Workload Identity Federation permite al pipeline CD autenticarse en Azure sin
  secrets de larga duración en GitHub.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Azure App Service for Containers | Pricing por plan (siempre encendido), menos flexible para multi-contenedor, no escala a cero |
| Azure Kubernetes Service (AKS) | Overhead operativo excesivo para 1–2 personas. Apropiado si el proyecto escala a múltiples bounded contexts con SLOs exigentes |
| Azure Container Instances (ACI) | Sin orquestación, sin escala automática, no es producción-grade |
| Azure Functions / Durable | Cambia el modelo de programación; no se alinea con el runtime REST API actual |

---

## Decisión 2 — Estrategia de migraciones EF Core

**Contexto**: No existe carpeta `Migrations/` en `MiKompri.ShoppingList.Infrastructure`.
El Dockerfile actual no incluye ningún paso de migración. La base de datos se crea
probablemente mediante `EnsureCreated()` o manualmente. Esto no es sostenible para
producción ni para equipos colaborativos.

**Decision**: **Migraciones EF Core explícitas en repositorio + aplicación en dos
contextos: `Migrate()` en startup para desarrollo local, y step dedicado en CD para
producción.**

**Rationale**:
- Las migraciones explícitas son el estándar para EF Core en producción. Permiten
  revisar SQL generado, controlar cambios y hacer rollback.
- `Migrate()` en startup es seguro y conveniente para desarrollo local (instancia única,
  bajo riesgo).
- Un step dedicado en CD evita race conditions en despliegues multi-instancia y permite
  fallar el pipeline antes de desplegar código si la migración falla.
- La imagen final de producción no necesita SDK de .NET para aplicar migraciones; se
  puede usar una imagen efímera de SDK o el comando `efbundle` para generar un ejecutable
  de migración standalone.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| `EnsureCreated()` en startup | No aplica migraciones incrementales; recrea la BD en cambios incompatibles. Pérdida de datos en producción |
| Solo `Migrate()` en startup (sin step CD) | Race condition si múltiples instancias arrancan simultáneamente en ACA. Aceptable solo en desarrollo |
| Script SQL manual | Más control pero rompe el DX de EF Core; requiere sincronización manual entre modelo y SQL |
| `efbundle` standalone | Válido, pero añade complejidad. Apropiado cuando el step CD no puede acceder al SDK; diferir a cuando ACA no tenga acceso a SDK images |

---

## Decisión 3 — Formato de ADR: MADR

**Contexto**: No existen ADRs en el proyecto. La constitución (TP9) exige documentar
decisiones técnicas significativas. Se necesita un formato estándar ligero compatible
con GitHub y el flujo de Spec Kit.

**Decision**: **MADR — Markdown Architectural Decision Records (versión simplificada)**

Campos obligatorios:
```
# ADR-{NNN} — {Título}
Estado: Propuesto | Aceptado | Obsoleto | Reemplazado por ADR-{NNN}
Fecha: YYYY-MM-DD
Decisión: ...
Contexto: ...
Alternativas consideradas: tabla con pros/contras
Consecuencias: ...
```

**Rationale**:
- Formato en Markdown puro; se renderiza directamente en GitHub sin herramientas extra.
- Ligero: no requiere front matter YAML ni herramientas de generación.
- Estándar en la industria con amplia documentación y ejemplos.
- Compatible con el flujo de Spec Kit (referenciable desde `plan.md`).
- Más corto que un RFC; más estructurado que un comentario en el código.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| RFC format (propuesta + discusión larga) | Demasiado pesado para un equipo pequeño |
| Formato custom ad-hoc | Sin estándar; cada ADR se vería diferente |
| Confluence / Notion | Fuera del repositorio; rompe TP3 (monorepo como fuente de verdad) |
| adr-tools (CLI) | Añade dependencia de herramienta; MADR manual es suficiente |

---

## Decisión 4 — Docker Compose multi-contexto

**Contexto**: El `docker-compose.yml` raíz actualmente solo levanta `ShoppingList API`
y `postgres`. Al implementar `Users API` se necesita decidir cómo organizar los servicios
en Docker Compose evitando duplicación y manteniendo la simplicidad.

**Decision**: **Un único `docker-compose.yml` en la raíz con todos los servicios, y
`docker-compose.override.yml` para variables locales. Uso de Docker Compose profiles
para levantar subconjuntos de servicios.**

```bash
# Solo ShoppingList (estado actual)
docker compose up shoppinglist-api postgres

# Todo el backend
docker compose --profile backend up

# Con cliente de migración
docker compose --profile migrate up
```

**Rationale**:
- Un único compose en la raíz es consistente con la estructura de monorepo.
- Los profiles permiten levantar solo los servicios necesarios sin múltiples archivos.
- El override file centraliza los secretos de desarrollo (sin commitear).
- Alternativa de múltiples compose files requiere `-f` flags y más coordinación.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Un compose por bounded context | Duplicación de configuración de postgres; más difícil de levantar el sistema completo |
| `include:` en compose (Docker Compose v2.20+) | Válido técnicamente pero añade complejidad innecesaria en esta etapa |
| Solo variables de entorno sin override file | Secretos pueden quedar en el historial de shell |

---

## Decisión 5 — Comunicación entre Bounded Contexts

**Contexto**: `ShoppingList` necesita validar `OwnerId` y `GroupId` contra el contexto
`Users` (deuda DT-001). Se necesita definir cómo se comunican los contextos sin violar
TP2 (sin acceso directo a BD ni referencias de dominio internas).

**Decision**: **REST API-First para MVP-1 y MVP-2. Sin DAPR ni mensajería asíncrona
hasta que el volumen de operaciones lo justifique.**

En MVP-1: ShoppingList API valida el JWT del usuario autenticado directamente (el token
contiene el UserId). No es necesaria una llamada HTTP a Users API para conocer la identidad
del usuario en cada request.

En MVP-2 (grupos): ShoppingList API consultará Users API via HTTP para validar pertenencia
al grupo antes de retornar listas de un grupo. Esto introduce un call sincrónico entre
contextos, aceptable para el volumen esperado.

**Rationale**:
- REST API sincrónica es el patrón más simple y directo para el volumen actual.
- La autenticación JWT elimina la necesidad de llamar a Users API en cada operación
  de ShoppingList para conocer la identidad del usuario.
- Mensajería asíncrona (Azure Service Bus, RabbitMQ, DAPR pub/sub) añade complejidad
  operativa que no está justificada hasta MVP-3+.

**Alternatives considered**:

| Alternativa | Por qué descartada |
|-------------|-------------------|
| Acceso directo a BD de Users desde ShoppingList | Viola TP2 explícitamente. No considerada |
| DAPR pub/sub desde MVP-1 | Complejidad prematura. Diferir hasta que exista demanda real de desacoplamiento asíncrono |
| Shared kernel / shared domain types | Introduce acoplamiento entre proyectos. Viola TP2 |
| Event sourcing + CQRS cross-context | Excesivo para el tamaño actual; reservado para futura revisión architectural si el producto escala |

---

## Resumen de Decisiones

| # | Decisión | Resultado |
|---|----------|-----------|
| D-1 | Plataforma Azure | Azure Container Apps |
| D-2 | Migraciones EF Core | Explícitas + step CD |
| D-3 | Formato ADR | MADR simplificado |
| D-4 | Docker Compose multi-contexto | Profiles en compose raíz |
| D-5 | Comunicación entre contextos | REST API (JWT en MVP-1, HTTP call en MVP-2) |

Todas las decisiones están resueltas. No quedan ítems NEEDS CLARIFICATION para el plan.
