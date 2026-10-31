# Implementation Plan: 011 — Web Iconography & Fridge Notes

**Branch**: `011-web-iconography` | **Date**:2026-10-31 | **Spec**: [spec.md](spec.md)
**Estado**: implementación frontend en validación. Fuente main9597af5/PR59 y formalización3bee0ec. Diseño/ADR-005 ratificados al autorizar el titular la siguiente fase el31/10/2026; evidencias de implementación separadas de las referencias estáticas.

## Summary

Aplicar Nevera compartida a la capa visual existente: React Icons/Phosphor para iconos propios, Fluent UI para componentes, papel en listas y cabecera pizarra. Conservar rutas/reglas de compra/historial/Gastos/permisos/autenticación y contratos. [Diseño](design.md), [mapa](icon-map.md), [research](research.md), [contrato UI](contracts/ui-iconography.md).

## Technical Context

| Campo | Decisión |
|---|---|
| Language/Version | React18.3.1, TypeScript6.0.3, Vite8.3.5 del lock existente; sin upgrade paralelo |
| Primary Dependencies | @fluentui/react-components actual, react-icons5.7.0 fijada (solo pi); MSAL/TanStack no cambian |
| Storage | Ninguno nuevo; sin persistencia de tema/layout, dominio ni schema EF |
| Testing | Vitest/Testing Library, Playwright Chromium/Firefox/WebKit, axe y controles manuales de foco/toque/lector/zoom |
| Target Platform | Web responsive320/390/430/768/1280+, teclado y coarse pointer; MAUI solo semántica futura |
| Performance | Budgets008/009:20 muestras por ruta/protocolo, LCPp75<=2500ms, interacción<=200ms, CLS<=0.1; medir bundle/gzip sin prometer peso |
| Constraints | Mantener fuentes/marca/Fluent, capa visual reversible; no cambio de auth/BD/env ni funcionalidades fuera de alcance |
| Scale/Scope | Shell común, Mis listas/detalles como primer corte; adaptación de iconos/tokens en catálogo/mercados/grupos/perfil y superficies compartidas |

## Constitution Check

PASS para formalización: PP1/PP3 prioriza leer/marcar listas, PP5 mobile-first; TP1/TP2 no backend nuevo ni cruce de contextos; TP3/TP4 conserva monorepo y Docker; TP6 MAUI aplazado con semántica adaptable. TP7 sin endpoints/DTOs nuevos. TP8 tests funcionales primero para wrappers y cambios de interacción; no tests de dominio repetidos para pintura. TP9 ADR-005 documenta dependencia/estilo. TP10 spec/plan/tasks antes de producción. TP11/1.2.0 intacta: no correlación, providers, scopes, caché o identidad nueva.

Revisar de nuevo antes de implementar y tras validar UI. Las referencias/contrastes calculados no son PASS de implementación; la aprobación de dirección visual se registra en ADR-005. No cambian completitud007/008/009 ni010-B/012.

## Phase0 — Research (esta formalización)

Paquete oficial5.7.0 descargado solo para lectura, SRI comprobado y31exports reales verificados; no instalación. Alternativas Fluent Icons/Lucide/Phosphor justificadas en research/ADR. Tokens y11pares contrastados; catálogo y tres referencias SVG/PNG renderizadas y revisadas. [Evidencia](evidence/current-state.md).

## Phase1 — Diseño y contratos (esta formalización)

Propuesta de modelo de presentación sin EF/API; contrato AppIcon/acciones/tokens y [data-model](data-model.md). Documentar estados y excepciones de iconos internos/brand. No comenzar React/CSS productivos en esta fase.

## Phase2 — Implementación por historias

1. Ratificar diseño/ADR y capturas baseline, asegurar CI Web para rama011.
2. Instalar solo react-icons5.7.0 con lock actualizado; no nuevo UI kit. Tests antes de registry/AppIcon/IconAction; entonces implementar módulos compartidos tipados.
3. US1 navegación: MainLayout y AccountMenu con icono+texto, selected y Drawer/foco existentes; sin destinos nuevos.
4. US2 listado: theme/tokens y notas, filtros/orden/historial/query params actuales; pruebas de pendientes/unknown/error parcial.
5. US3 detalle: papel de trabajo, fila legible y targets, conservando checkbox/compra/Gastos/foco/idempotencia de009.
6. US4 ayudas/acciones: wrapper icon-only cuando proceda, tooltip/teclado/toque y borrado con confirmación existente. Texto de acciones importantes permanece.
7. US5 transversal: catálogo/mercados/grupos/perfil/estados; sin renovar endpoints ni auth. Login permanece Google+Microsoft operativo. No rediseño completo de landing.
8. Validación responsive/accesibilidad/rendimiento/bundle y CI HEAD; documentar diferencias respecto a propuesta con evidencias y revisión. No merge/deploy automático.

## Project Structure

Documentación: spec/plan/tasks/research/design/icon-map/data-model/quickstart, contracts/ui-iconography, checklists/requirements y acceptance, evidence/icon-audit y contrast/current-state, tres referencias SVG/PNG en design; ADR005 en docs/adr.

Lugares previstos de implementación (todavía sin crear/modificar):
- MiKompri.Web/src/shared/ui/icons/{iconRegistry.ts,AppIcon.tsx,IconAction.tsx}, theme.ts y AppSurface.module.css.
- src/app/layout/MainLayout.tsx y MainLayout.module.css; shared/ui/AccountMenu.tsx.
- features/shopping-lists/pages/ListsPage.tsx, components/ListOverview.tsx; shared/ui/ShoppingListView.tsx y páginas de detalle existentes.
- Componentes existentes de catálogo/mercados/grupos/perfil y states/; solo presentación.
- tests/unit/ui/iconRegistry.spec.ts; tests/component/ui/IconAction.spec.tsx; tests/e2e/modernization/iconography.spec.ts, tests/visual y fixtures existentes.
- .github/workflows/ci-mikompri-web.yml: incluir011 en push, conservar pruebas/gates, nunca omitir escenarios.

## Validation & rollout

[Quickstart](quickstart.md) define pasos. Hacer un corte vertical antes de propagar tokens a todo: menú/navegación → listado/detalle → pantallas relacionadas. Capturas baseline/diseño/resultado etiquetadas, datos ficticios para evidencia y QA real acotada sin tokens. No snapshots masivos que solo copien DOM ni tests de biblioteca; probar conducta/contratos/errores.

Medir imports/chunks y rutas con100ítems, precios desconocidos, texto largo y estados parciales. No fijar un incremento gzip imaginario: registrar baseline/post y justificar solo glyphs requeridos; budgets heredados siguen siendo gate. Si baseline ya incumple, registrar causa/impacto y trabajo necesario sin rebajar umbral ni declarar cerradas tareas previas.

Revertir solo commits de la capa visual011; mantener datos, APIs, fixtures privados, configuración QA010-A y ambos proveedores. No ejecutar migraciones ni cambiar cloud para una regresión de iconos.

## Complexity Tracking

| Elección | Motivo | Complejidad excluida |
|---|---|---|
| Un pack/registry semántico | Consistencia y nombres reales | Mezclar familias/lookup dinámico/importar todo |
| Fluent existente | Controles accesibles reutilizados | Otro UI kit/menú/tooltip desde cero |
| Tokens y papel/pizarra CSS | Identidad visual sin peso externo | Canvas/texturas remotas/manuscrita/nueva animación |
| Proyección de listas actual | Mismo producto/referencias | Nuevo FridgeBoard, notas, drag/pinning, endpoints |

## Dependencies & Definition of Done

Spec/diseño revisados → foundation → US1/US2/US3 → US4/US5 → a11y/métricas/regresiones/CI/evidencia. Cada historia es comprobable por separado; wrappers comunes no requieren cambiar dominio.

Hecho solo con acceptance.md acreditado en implementación final. Todas las tareas de implementación siguen abiertas; esta formalización no declara funcionalidad implementada ni su CI verde. Actualizar versiones/exports solo con nueva auditoría; respetar licencia. Sin cambiosTP11 ni bloqueos010-B/012, sin instalaciones en esta ejecución.

## Revisión final autorizada sobre cf83bff

1. Auditar HEAD/main/CI y contratos efectivos de Grupos antes de código.
2. Mantener tokens/sans/teal; espacios de grupo en papel con banda teal y símbolo groups, diferentes de notas de compra. Crear grupo pasa a diálogo Fluent compacto; validación/carga/error/query/foco permanecen. Detalle conecta una entrada a listas y panel Personas. Sin acciones ficticias.
3. Diagnóstico independiente de LCP por ruta (elemento/waterfall/JS/CSS/fuentes/query/render) antes de optimizar; runner y budgets intactos. Revisar iconos realmente usados y carga crítica.
4. Validar el candidato final con suites,3motores/Docker y80muestras después, sin carga concurrente; atribuir porción011 vs deuda previa.
5. QA humana ausente se registra diferida no bloqueante, sin casilla de ejecución ficticia. Clasificación de deuda según autorización de spec; CI exacto tras commit/push, PR si gates aplicables acreditados. No merge/deploy.
