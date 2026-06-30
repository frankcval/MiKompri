# Plantilla y Convenciones de ADR — MiKompri

**Spec**: [plan.md](../plan.md) | **Vigente desde**: 2026-06-30
**Formato**: MADR (Markdown Architectural Decision Records, versión simplificada)

---

## Plantilla ADR

Copiar este bloque para crear un nuevo ADR. Archivo en: `docs/adr/ADR-{NNN}-{titulo-kebab}.md`

```markdown
# ADR-{NNN} — {Título de la Decisión}

**Estado**: Propuesto | Aceptado | Obsoleto | Reemplazado por ADR-{NNN}
**Fecha**: YYYY-MM-DD
**Spec relacionada**: [specs/{NNN}-{feature}/plan.md](../../specs/{NNN}-{feature}/plan.md)

## Contexto

Descripción del problema o situación que requiere una decisión. Incluir:
- Por qué la decisión es necesaria ahora.
- Restricciones vigentes (de la constitución, del producto, técnicas).
- Dependencias o impactos en otros componentes.

## Decisión

Descripción clara y concisa de lo que se decidió.
Usar DEBE, DEBERÍA, PUEDE donde aplique (RFC 2119).

## Alternativas Consideradas

| Opción | Ventajas | Desventajas |
|--------|----------|-------------|
| Opción A (elegida) | ... | ... |
| Opción B | ... | ... |
| Opción C | ... | ... |

## Consecuencias

### Positivas
- ...

### Negativas / Trade-offs
- ...

### Tareas derivadas
- [ ] Tarea concreta generada por esta decisión (referenciar en tasks.md si aplica)
```

---

## Reglas de Nomenclatura

| Campo | Regla | Ejemplo |
|-------|-------|---------|
| Número | Secuencial con 3 dígitos, cero relleno | `ADR-001`, `ADR-012` |
| Título en nombre de archivo | kebab-case, descriptivo | `azure-deployment` |
| Nombre completo del archivo | `ADR-{NNN}-{titulo-kebab}.md` | `ADR-001-azure-deployment.md` |
| Ubicación | `docs/adr/` desde la raíz del monorepo | `docs/adr/ADR-001-azure-deployment.md` |

---

## Estados del Ciclo de Vida

```
Propuesto → Aceptado → (Obsoleto | Reemplazado por ADR-{NNN})
```

| Estado | Significado |
|--------|-------------|
| `Propuesto` | En revisión, no aprobado aún |
| `Aceptado` | Aprobado y en vigor |
| `Obsoleto` | Ya no aplica; la decisión fue superada sin un reemplazo específico |
| `Reemplazado por ADR-{NNN}` | La decisión fue revisada y reemplazada por un nuevo ADR |

---

## Cuándo Crear un ADR

✅ **Crear ADR cuando**:
- Se elige una librería, framework o herramienta con al menos una alternativa relevante.
- Se cambia o define un patrón arquitectural (CQRS, Repository, Event Sourcing, etc.).
- Se decide una estrategia de infraestructura o despliegue.
- Se define la estrategia de autenticación o autorización.
- Se introduce comunicación entre bounded contexts.
- Se cambia el esquema de base de datos con impacto en contratos externos.
- Se adopta o cambia una política de seguridad transversal.

❌ **No crear ADR para**:
- Decisiones de estilo de código (usar linter o `.editorconfig`).
- Cambios de implementación sin impacto en contratos ni arquitectura.
- Elección de una librería de utilidad sin alternativas relevantes.
- Decisiones reversibles de bajo impacto.

---

## Referenciando ADRs desde plan.md

En el `plan.md` de una spec, las decisiones técnicas significativas DEBEN referenciar
su ADR correspondiente:

```markdown
### Decisión — Plataforma de despliegue

Ver [ADR-001 — Azure Deployment](../../docs/adr/ADR-001-azure-deployment.md).

**Decisión**: Azure Container Apps ...
```

---

## Índice de ADRs Activos

| ADR | Título | Estado | Fecha |
|-----|--------|--------|-------|
| [ADR-001](../../docs/adr/ADR-001-azure-deployment.md) | Azure Container Apps como plataforma de despliegue | Aceptado | 2026-06-30 |
| [ADR-002](../../docs/adr/ADR-002-migrations-strategy.md) | Estrategia de migraciones EF Core | Aceptado | 2026-06-30 |
| [ADR-003](../../docs/adr/ADR-003-adr-format.md) | Formato MADR para ADRs | Aceptado | 2026-06-30 |
