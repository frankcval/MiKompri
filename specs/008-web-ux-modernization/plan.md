# Implementation Plan: Modernización integral web P0–P3

**Branch**: identificador `008-web-ux-modernization`, sin cambio Git | **Date**: 2026-10-28 | **Spec**: [spec.md](spec.md)
**Input**: 008 y deuda trazable de 007. **Estado**: implementación local entregada; aceptación parcial, con integración Entra, verificación manual ampliada y piloto pendientes. [Evidencia](evidence/validation.md).

## Summary

Reutilizar cliente/contextos actuales. Primero permisos/contratos; después compra completa/recuperación; luego componentes/navegación/carga; finalmente integración y comprensión humana. Incrementos demostrables sin reinicializar plataforma.

## Technical Context

**Language/Version**: TypeScript/React 18 según package.json; C#/.NET 8+ según proyectos existentes.
**Primary Dependencies**: Vite, Fluent v9, Router, MSAL, React Query, RHF y Zod existentes; axe de desarrollo incorporado. Runtime/lockfile y actualizaciones acotadas documentados en research.
**Storage**: PostgreSQL por contexto; sessionStorage por pestaña/cuenta/lista para borradores, sin tokens, TTL 24 h, limpiar logout/cambio cuenta.
**Testing**: .NET dominio/aplicación/API, Vitest/RTL y Playwright separado en UI controlada e integración real.
**Target Platform**: web móvil/tablet/escritorio; Chromium/Firefox/WebKit para críticos. Azure objetivo futuro, no despliegue.
**Project Type**: SPA en monorepo y backend modular .NET.
**Performance Goals**: SC-008 en portada, Mis listas y ambos detalles. Perfil 390×844, 4× CPU, 1,6 Mbps/150 ms, fixture 50 productos; 20 navegaciones sin caché por ruta y 20 interacciones por añadir/marcar/abrir gastos. Registrar hardware/versiones, percentiles y fallos. Es aceptación de laboratorio, no métricas de campo.
**Constraints**: TP1–11, autorización servidor, cálculos financieros backend, EUR/cantidades enteras, URLs compatibles.
**Scale/Scope**: ocho historias, 28 FR, nueve SC; hogares/grupos pequeños. Medir coste por grupo antes de nuevo agregador/servicio.

## Constitution Check

Gate de diseño PASS, sin dispensas nuevas; ejecución pendiente de evidencia.

| Principio | Aplicación/verificación |
| --- | --- |
| PP1–3 | Valor por historia y prueba independiente; núcleo CRUD antes de decoración; no cierre con tareas pendientes |
| PP4 | Autoría y auditoría; historia sin nombre no recibe autor inventado |
| PP5 | Verificación primero 390/320/430; prioridad web ya establecida en 007 no cambia framework móvil |
| TP1–3 | .NET, contextos actuales con Users por API, monorepo |
| TP4/5 | Docker reproducible e imágenes validadas; Azure compatible sin despliegue |
| TP6 | Sin cambios al MAUI diferido |
| TP7 | Contratos/OpenAPI; adición compatible o versión nueva |
| TP8 | Tests dominio/aplicación/API, regresiones y CI bloqueante |
| TP9/10 | Decisiones en plan/research, spec/plan/tasks antes de código |
| TP11 | Entra/PKCE existentes, `(tid,oid)` a UserId en Users, audience común, issuer/audience/firma/expiración/scopes y permiso por recurso |

Postdiseño: contratos futuros etiquetados; controles no habilitados hasta evidencia. Sin acceso cruzado a bases, nuevos runtimes ni identidad alternativa. No enmienda implícita del contexto histórico MAUI/web.

## Project Structure

### Documentation (this feature)

```text
specs/008-web-ux-modernization/
  spec.md, plan.md, research.md, data-model.md, design.md
  traceability.md, quickstart.md, tasks.md
  contracts/api-and-permissions.md
  checklists/requirements.md, checklists/acceptance.md
```

### Source Code (repository root)

```text
MiKompri.Web/src/{app,auth,features,shared}/
MiKompri.Web/tests/{unit,component,integration,e2e}/
MiKompri.ShoppingList.{Api,Application,Domain,Infrastructure}/
MiKompri.Users.{Api,Application,Domain,Infrastructure}/
MiKompri.ProductCatalog.{Api,Application,Domain,Infrastructure}/
test/MiKompri.*.Tests/
.github/workflows/ci-mikompri-web.yml
```

**Structure Decision**: extender features. Propuestos ShoppingListView, ProductSearch, InlineProductCreation, AccountMenu, ListsPage y GroupDetailPage, sin segundo shell/kit visual.

## Phase 0 — Research

Código local/contratos, historial T132/T133, CI y deuda revisados en [research](research.md). [Trazabilidad](traceability.md) coordina sin duplicar cierre: al verificar 007 se anota evidencia 008/Txxx conservando aceptación antigua.

## Phase 1 — Design

[Modelo](data-model.md), [contratos](contracts/api-and-permissions.md), [UX](design.md) y [quickstart](quickstart.md). Rutas propuestas no son públicas hasta implementación/OpenAPI/tests. Auditar verbos/DTO/códigos antes de conectar controles.

## Oleadas y gates

| Oleada | Entrega | Gate |
| --- | --- | --- |
| P0 | Trazabilidad y US1 permisos | Propiedad/roles/membresía en servidor, pruebas negativas y contrato |
| P1 | US2–6 compra, grupos, alta, sesión/errores y gastos | Persistencia, fallos/roles e integración vertical con dos personas |
| P2 | US7 prototipo, navegación, componentes/carga | URL/foco y antes/después comprobados |
| P3 | US8 CI, integración, accesibilidad, rendimiento/piloto | SC-001–009, cero skips obligatorios y cierre individual 007/008 |

Las historias usan recursos preparados para prueba independiente, con dependencias de integración en tasks. P3 acepta finalmente, pero tests/accesibilidad ocurren en cada incremento.

## Decisiones operativas

- Mis listas consulta personales/grupos y compartidas con caché por cuenta/grupo y máximo cuatro solicitudes concurrentes; fallo parcial visible, nunca falso vacío. Medir antes de agregador nuevo.
- Detalle personal con lectura específica segura si evita descargar todas las listas e ítems.
- Pending por itemId; sin éxito anticipado. Refetch fallido tras escritura confirmada dice «Guardado; no se pudo actualizar la vista» y solo reintenta lectura.
- Concurrencia: auditar soporte actual; 409 relee/revisa, no fusión silenciosa. No prometer ETag/idempotency keys no existentes.
- Desmarcar por asignación de estado aditiva; no tocar gastos. Estimación desconocida aditiva/null compatible; cero legado conserva significado.
- Draft sessionStorage sin credenciales ni resultados financieros completos, TTL 24 h; revalidar membresía/productos y limpiar tras éxito/descarte/logout/cambio cuenta.
- Edición de gasto requiere expenseId y proyección de valores reales; incorporar lectura si falta, nunca reconstruir desde saldos.
- GET /me refresca y POST /me/sync sincroniza, sin nueva edición de perfil.
- Axe-core como dependencia de desarrollo a verificar al implementar. Storybook/iconos opcionales con decisión documentada si aportan valor; no gates. WOFF2/carga por rutas medidos, licencias preservadas.
- No hay script update-agent-context en esta distribución: contexto preservado aquí y con puntero 008; no regenerar instrucciones Copilot con instalador.

## Complexity Tracking

Sin violaciones nuevas propuestas. No dispensar TP8/TP11 ni sustituir validación humana por harness. Tenant/participantes son prerrequisitos de ejecución, no dudas de diseño ni motivo para simular éxito.
