# ADR-001 — Azure Container Apps como Plataforma de Despliegue

**Estado**: Aceptado
**Fecha**: 2026-06-30
**Spec relacionada**: [specs/001-project-baseline/plan.md](../../specs/001-project-baseline/plan.md)

## Contexto

El pipeline CD actual (`cd-mikompri-shoppinglist.yml`) solo construye la imagen Docker
de ShoppingList API y la publica en GitHub Container Registry (GHCR). No existe ningún
paso de despliegue en Azure.

La constitución técnica (TP5) establece que el despliegue objetivo DEBE ser Microsoft Azure
y que las decisiones de infraestructura DEBEN evaluarse en el contexto de los servicios
Azure disponibles.

Se necesita elegir el servicio Azure que:
- Soporte contenedores Docker directamente desde GHCR.
- Tenga integración con GitHub Actions.
- Sea operacionalmente manejable para un equipo pequeño.
- Sea económicamente viable para etapa temprana (sin carga significativa aún).

## Decisión

Usar **Azure Container Apps (ACA)** como plataforma de despliegue para todas las APIs
REST de MiKompri.

La base de datos de producción será **Azure Database for PostgreSQL Flexible Server**.

La autenticación del pipeline CI/CD con Azure se realizará mediante
**OIDC Workload Identity Federation** (sin secrets de larga duración en GitHub).

## Alternativas Consideradas

| Opción | Ventajas | Desventajas |
|--------|----------|-------------|
| **Azure Container Apps** (elegida) | Sin gestión de Kubernetes, escala a cero, pricing por consumo, DAPR disponible, integración directa GHCR | Menos control granular que AKS |
| Azure App Service for Containers | Familiar, maduro, fácil de configurar | Pricing por plan (siempre encendido), menos flexible para multi-contenedor, sin escala a cero |
| Azure Kubernetes Service (AKS) | Máximo control, portable, production-grade para alta escala | Overhead operativo excesivo para equipo pequeño; coste mensual fijo de cluster |
| Azure Container Instances (ACI) | Muy simple | Sin orquestación, sin escala automática, no es opción production-grade para una API |
| Azure Functions | Integración con Azure, pricing por ejecución | Cambia el modelo de programación REST actual; no apropiado sin refactoring |

## Consecuencias

### Positivas
- Coste cero cuando no hay tráfico (escala a cero).
- El pipeline CD se extiende con `azure/container-apps-deploy-action` sin reescribir la lógica de build.
- DAPR disponible si en el futuro se necesita comunicación entre bounded contexts vía mensajería.
- OIDC Federation elimina la necesidad de rotar secrets de Azure en GitHub.

### Negativas / Trade-offs
- Requiere crear y configurar el entorno Azure (Container Apps Environment, Registry credentials,
  PostgreSQL Flexible Server) antes del primer despliegue.
- ACA tiene menos control sobre networking que AKS; puede ser una limitación si en el futuro
  se necesita VNet injection granular.

### Tareas derivadas
- [ ] Crear `infra/azure/container-apps.bicep` con la definición del entorno ACA.
- [ ] Crear `infra/azure/postgres.bicep` con la definición de PostgreSQL Flexible Server.
- [ ] Extender `cd-mikompri-shoppinglist.yml` con el step de deploy a ACA.
- [ ] Configurar OIDC Workload Identity Federation en Azure y GitHub.
- [ ] Documentar variables de entorno y secretos requeridos por la API en ACA.
