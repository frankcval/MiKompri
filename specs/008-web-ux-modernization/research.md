# Research y decisiones — 2026-10-28

Investigación inicial y decisiones actualizadas tras implementación local. Versiones concretas validadas con el lockfile; no se afirma que sean las últimas disponibles.

| ID | Decisión | Razón | Alternativa descartada |
| --- | --- | --- | --- |
| D01 | 008 enlaza deuda 007 | Separa mejoras/cierre original | Reescribir historia o duplicar cierre |
| D02 | Scripts/plantillas Spec Kit existentes | Configuración ya preparada | Reinstalar/reinicializar |
| D03 | Autorización P0 antes de nuevas escrituras | Handler genérico no acredita recurso | Ocultar controles como frontera de seguridad |
| D04 | Desmarcar por estado explícito sin efectos financieros | Recuperación idempotente | Toggle ciego/borrar gastos |
| D05 | Mantener rutas y recuperar intención | Continuidad de tarea | Romper enlaces/destino fijo obligatorio |
| D06 | Crear e incorporar como dos pasos recuperables | Contextos independientes | Transacción distribuida o rollback borrando catálogo |
| D07 | EUR/enteros y estimación desconocida | Mantener reglas y evitar cero ficticio | Fracciones/multidivisa no solicitadas |
| D08 | Draft por pestaña/cuenta/lista, TTL 24 h | Reauth sin mezclar cuentas | Storage global o memoria que se pierde en redirect |
| D09 | Sin invitaciones ni administración de miembros | Ver miembros cumple alcance actual | Pedir GUID no equivale a invitar |
| D10 | Lectura de gastos e historia por contrato | Necesita expenseId/valores | Inferir gastos a partir de balances |
| D11 | Fluent, Source Sans 3 y lista compartida con demo | Coherencia y menor mantenimiento | Migrar kit visual sin beneficio acreditado |
| D12 | Harness, integración y piloto separados | Evidencia responde a preguntas distintas | Contar screenshots como éxito humano |
| D13 | Carga con perfil reproducible | Comparación verificable | Score aislado sin condiciones |

## Fuentes

[Constitución](../../.specify/memory/constitution.md), [plantillas](../../.specify/templates/), [agentes](../../.github/agents/), [007/spec](../007-web-react-client/spec.md), [tasks](../007-web-react-client/tasks.md), [design](../007-web-react-client/design.md), [contrato](../007-web-react-client/contracts/frontend-backend-integration.md), [handler](../../MiKompri.ShoppingList.Application/Commands/MarkItemAsPurchased/MarkItemAsPurchasedCommandHandler.cs), [controlador](../../MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs), [grupos](../../MiKompri.Users.Api/Controllers/GroupsController.cs), [perfil](../../MiKompri.Users.Api/Controllers/ProfileController.cs), [cliente](../../MiKompri.Web/src/), [CI](../../.github/workflows/ci-mikompri-web.yml) y [análisis previo](../../docs/analisis-requisitos-ux-ui-2026-10-28.md).

## Verificaciones técnicas con tarea

1. Auditar genéricos editar/borrar además de marcado; no solo checkbox compartido.
2. Proyección compartida cliente omite estimación/autor/gastos; comprobar DTO real y extender compatible donde falte.
3. CI: comprobar compatibilidad Node/Vite, instalación de navegadores y smoke sin `pass-with-no-tests` para obligatorios.
4. Tenant/roles/antiguos miembros requieren recursos de prueba; ausencia mantiene gate pendiente.
5. T133 ya documenta corrección ItemId/ProductId: protegerla con regresión y mantener T132 independiente.

Sin dudas de diseño pendientes para elaborar tareas; verificaciones de contrato son trabajo explícito, no contratos presumidos existentes.

## Decisiones de implementación

- Se reutilizaron Spec Kit, Fluent UI, React Query, RHF y Zod existentes. No se reinstaló Spec Kit ni se incorporó un segundo kit visual. El prototipo está en `design/prototypes/mikompri-modernization/`.
- Node 24 en local/CI/Docker; Vitest 5.0.5 y React Router 7.18.5 tras detectar vulnerabilidades con `npm audit`. Las actualizaciones se limitaron a esas familias y se repitieron tipos, compilación y pruebas. Auditoría final: cero vulnerabilidades.
- Se añadió `@axe-core/playwright` 4.14 para verificación automatizada; no hizo falta Storybook ni un servicio externo. Centinelas Tabster y límites de axe en [validación](evidence/validation.md).
- Source Sans 3 permanece local y conserva su licencia: WOFF2 aproximadamente 225 kB entre tres pesos frente a 704 kB TTF. Conversión con fonttools en carpeta QA ignorada, sin dependencia runtime. TTF se conserva como fuente original.
- Rutas diferidas y componentes compartidos reducen la entrada JS de ~1.064 kB a ~369 kB; el resto se descarga por ruta. Esto no acredita por sí mismo los presupuestos LCP/interacción/CLS.
- Precio desconocido utiliza columna booleana con default true y proyección nullable. Migración y ceros existentes probados en PostgreSQL 16.
- Se conservaron las reglas de edición de gastos Owner/Admin. Member no recibe nuevos permisos por ser autor. Los participantes y pagador/comprador se comprueban contra membresía activa al escribir.
- Recursos externos opcionales para el siguiente ciclo: [Playwright](https://playwright.dev/docs/intro), [axe-core](https://github.com/dequelabs/axe-core), [Fluent UI](https://react.fluentui.dev/) y prototipos Figma si el equipo ya trabaja allí. No se instalaron plugins ni servicios de pago.
