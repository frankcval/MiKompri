# ADR-003 — Formato MADR para Architecture Decision Records

**Estado**: Aceptado
**Fecha**: 2026-06-30
**Spec relacionada**: [specs/001-project-baseline/plan.md](../../specs/001-project-baseline/plan.md)

## Contexto

No existe ningún ADR en el proyecto. La constitución (TP9) establece que las decisiones
técnicas significativas DEBEN documentarse como ADR o en el `plan.md` de la spec
correspondiente.

Se necesita establecer un formato estándar que:
- Sea simple de escribir sin herramientas adicionales.
- Se renderice bien en GitHub.
- Sea legible por el equipo y por los agentes de IA (Spec Kit).
- Sea adoptable incrementalmente (no requiere migrar documentación existente).

## Decisión

Adoptar **MADR (Markdown Architectural Decision Records)** en versión simplificada
como formato estándar.

Campos obligatorios:
- `Estado` con ciclo de vida: `Propuesto → Aceptado → Obsoleto | Reemplazado por ADR-{NNN}`
- `Fecha`
- `Spec relacionada` (si aplica)
- `Contexto`
- `Decisión`
- `Alternativas Consideradas` (tabla)
- `Consecuencias` (positivas, trade-offs, tareas derivadas)

Ubicación: `docs/adr/ADR-{NNN}-{titulo-kebab}.md`

La plantilla completa y el índice de ADRs se mantienen en
`specs/001-project-baseline/contracts/adr-template.md`.

## Alternativas Consideradas

| Opción | Ventajas | Desventajas |
|--------|----------|-------------|
| **MADR simplificado** (elegido) | Markdown puro, GitHub-native, ligero, estándar en industria | Menos formal que RFC |
| MADR completo (con YAML front matter) | Procesable por herramientas | Front matter YAML innecesario para el tamaño del proyecto |
| RFC format | Muy detallado, apto para grandes equipos | Excesivo para equipos pequeños; overhead de escritura |
| Formato custom | Total flexibilidad | Sin estándar; inconsistencia entre ADRs |
| Confluence / Notion | Rica en colaboración | Fuera del repo; rompe TP3; requiere acceso a herramienta externa |
| `adr-tools` CLI | Automatiza creación | Dependencia de herramienta de tercero; MADR manual es suficiente |

## Consecuencias

### Positivas
- Cualquier colaborador puede crear un ADR sin herramientas especiales.
- Los ADRs son parte del código fuente y evolucionan con el proyecto.
- Los agentes de Spec Kit pueden leer y referenciar ADRs directamente.

### Negativas / Trade-offs
- Sin indexación automática; el índice en `adr-template.md` DEBE mantenerse manualmente.

### Tareas derivadas
- [ ] Este ADR es auto-referencial: ya define el formato que describe.
- [ ] Mantener actualizado el índice en `specs/001-project-baseline/contracts/adr-template.md`
  al crear cada nuevo ADR.
