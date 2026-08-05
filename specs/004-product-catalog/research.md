# Research: 004 — Product Catalog (MVP-2)

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-08-05

## Objetivo de investigación

Resolver decisiones de diseño para implementar un bounded context `ProductCatalog` independiente, alineado con la arquitectura actual del repositorio y el alcance estricto de MVP-2.

---

## Decision 1: Crear bounded context independiente `MiKompri.ProductCatalog.*`

**Decision**: Implementar cuatro proyectos nuevos (`Api`, `Application`, `Domain`, `Infrastructure`) y sus tres proyectos de test en `test/`.

**Rationale**: El repositorio ya usa separación por bounded context (ShoppingList/Users). Esto mantiene Clean Architecture, bajo acoplamiento y evolución independiente.

**Alternatives considered**:
- Extender `ShoppingList` con módulos de catálogo: descartado por mezclar dominios distintos.
- Extender `Users`: descartado por no corresponder al dominio de identidad.

---

## Decision 2: Persistencia PostgreSQL dedicada para ProductCatalog

**Decision**: Base lógica `MiKompri_ProductCatalog` en la misma instancia PostgreSQL de Docker local.

**Rationale**: Aísla modelo y migraciones del contexto; evita dependencia de esquemas de `ShoppingList` o `Users`.

**Alternatives considered**:
- Compartir `MiKompri_ShoppingList`: descartado por acoplamiento de datos.
- Usar InMemory en runtime: descartado por no representar entorno real.

---

## Decision 3: Modelo de producto con desactivación lógica y unicidad de negocio

**Decision**: `CatalogProduct` será desactivable (soft deactivate) y único por `(NormalizedName, PurchaseUnit)` para registros activos.

**Rationale**: Cumple FR-003/FR-004 y preserva historial incluso tras desactivar.

**Alternatives considered**:
- Borrado físico: descartado por pérdida de trazabilidad histórica.
- Unicidad solo por nombre: descartado porque diferentes unidades (kg, unidad, litro) representan productos operativamente distintos.

---

## Decision 4: Historial de precios inmutable por fecha efectiva

**Decision**: `ProductPriceRecord` será append-only. Se bloquea duplicado por `(CatalogProductId, MarketId, EffectiveDate)`.

**Rationale**: Cumple FR-009/FR-010 y simplifica auditoría y consultas cronológicas (FR-012).

**Alternatives considered**:
- Sobrescribir último precio: descartado por pérdida de historial.
- Permitir múltiples precios mismo día sin discriminador: descartado por ambigüedad funcional.

---

## Decision 5: Auditoría mínima obligatoria

**Decision**: Todas las entidades raíz y de historial incluyen `CreatedAt`/`UpdatedAt`. `CreatedBy`/`UpdatedBy` se activan cuando el contexto de identidad esté disponible en runtime.

**Rationale**: Cumple FR-015 y evita bloquear MVP-2 por integración completa con `Users` (fuera de alcance).

**Alternatives considered**:
- Exigir siempre `CreatedBy`/`UpdatedBy`: descartado por dependencia funcional con otro contexto.
- Sin auditoría temporal: descartado por incumplir FR-015.

---

## Decision 6: Contrato REST v1 con CQRS y validación explícita

**Decision**: Exponer endpoints REST bajo `/api/v1` con controladores delgados que delegan en MediatR (commands/queries) y FluentValidation.

**Rationale**: Replica patrón estable del repositorio y mantiene consistencia entre bounded contexts.

**Alternatives considered**:
- Minimal APIs: descartado para mantener homogeneidad con contextos actuales.
- Validación manual en controladores: descartado por duplicación y menor mantenibilidad.

---

## Decision 7: Integración futura con ShoppingList solo por contrato

**Decision**: MVP-2 no integra runtime con `ShoppingList`; solo garantiza `catalogProductId` estable en respuestas y consultas.

**Rationale**: Respeta alcance y evita refactor de contextos existentes.

**Alternatives considered**:
- Integración directa ahora (FK cruzada o llamadas síncronas): descartado por estar fuera de alcance y elevar riesgo.

---

## Decision 8: Estrategia de pruebas por capas

**Decision**: Mantener 3 niveles obligatorios: Domain, Application y API Integration con xUnit.

**Rationale**: Cumple TP8 y práctica actual del repositorio.

**Alternatives considered**:
- Solo tests de API: descartado por baja cobertura de reglas de dominio.
- Solo unit tests sin integración: descartado por falta de verificación de contratos HTTP.

---

## NEEDS CLARIFICATION resueltos

No quedan marcadores `NEEDS CLARIFICATION` para continuar a diseño y contratos.
