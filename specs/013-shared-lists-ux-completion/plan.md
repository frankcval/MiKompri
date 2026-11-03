# Implementation Plan — 013 Shared Lists UX Completion

**Branch**: `013-shared-lists-ux-completion`  
**Date**: 2026-11-03  
**Spec**: [spec.md](spec.md)  
**Base**: `main` SHA `f0a1643`

## Summary

Completar experiencia de listas compartidas en Web con foco en navegación coherente, jerarquía de contexto, paridad funcional de compra, claridad de gastos y representación por roles, sin mezclar invitaciones (012).

## Technical Context

- **Frontend**: React + TypeScript + Vite + Fluent UI + TanStack Query.
- **Backend consumido**: ShoppingList + Users APIs existentes.
- **Dominio**: no se añaden entidades ni migraciones en esta iniciativa de UX.
- **Testing objetivo**: unit/component/e2e en shared-lists + integración de autorización.

## Constitution & scope checks

- Mantener TP11 y separación de bounded contexts.
- No cambiar auth trust ni reglas de dominio de settlement salvo bug probado.
- No incluir invitaciones/cambios de membresía (012).
- Evitar rutas paralelas para el mismo recurso compartido.

## Workstreams

### W1 — Auditoría y baseline verificable
- Consolidar estado real (código + pruebas + permisos efectivos) y brechas UX demostradas.

### W2 — Navegación y contexto de recurso
- Reglas de entrada/salida, breadcrumb liviano, query params y consistencia back/forward.

### W3 — Listado compartido y creación
- Pulir UX del listado por grupo y crear lista compartida sin fricción.

### W4 — Detalle Compra
- Paridad de claridad con lista personal + permisos por rol visibles y robustos.

### W5 — Detalle Gastos
- Separación explícita estimación vs gasto real + estados y permisos.

### W6 — Detalle Personas
- Presentación útil de miembros/roles con datos disponibles actuales.

### W7 — Calidad transversal
- Responsive 320–1280, accesibilidad operativa, performance acotada al flujo shared-lists.

## Sequencing (recomendado)

1. Baseline + matriz de permisos real.
2. Navegación/contexto.
3. Listado + creación.
4. Detalle compra.
5. Detalle gastos.
6. Detalle personas.
7. Pruebas finales, evidencia y cierre.

## Deliverables de formalización

- Spec, plan, research, data-model.
- Tasks ordenadas por fases/US.
- Checklists de requirements/acceptance.
- Evidencia de auditoría actual.

## Risks

1. **Regresión de permisos en UI** al introducir simplificaciones visuales.
2. **Inconsistencia de navegación** entre origen Grupos y origen Mis listas.
3. **Fragilidad de tests async** en suites largas de frontend.
4. **Confusión semántica** entre estimación de productos y gasto real.

## Mitigaciones

- Matriz de permisos trazada a backend antes de cambios UI.
- Contrato explícito de navegación y estados query.
- Tests component/e2e por escenario crítico, con esperas estables.
- Copy y estructura visual separando claramente Compra vs Gastos.
