# Tasks — 009

Estado al 2026-10-30: implementación funcional y verificación técnica T009 acreditadas según [validation](evidence/validation.md); aceptación completa pendiente en T010/T011. No modificar completitud 002–008.

- [x] T001 Formalizar spec/plan/modelo/contratos, auditoría y alcances futuros.
- [x] T002 Probar e implementar estimaciones/orden/activas/historial en todos los índices y detalles.
- [x] T003 Unificar búsqueda/selección/alta contextual con recuperación de incorporación fallida.
- [x] T004 Reproducir historial HTTP y conectar DTO/estados/filtros/registro manual.
- [x] T005 Conectar edición de producto y gestión de mercados; documentar permisos globales y errores.
- [x] T006 Tests dominio/backend para compra personal, mercado/fecha, idempotencia y concurrencia.
- [x] T007 Implementar contratos backend aditivos, persistencia/migración y protección de historia.
- [x] T008 Captura opcional desde marcado y acceso independiente de Gastos, mismos registros e idempotencia Web.
- [x] T009 Completar pruebas Web/API/Docker y regresión, documentar resultados y límites. Evidencia: [cierre técnico del 30/10](evidence/review-corrections.md#verificación-y-reconciliación--2026-10-30).
- [ ] T010 Validar móvil/teclado/errores/recarga/historial y rendimiento; registrar evidencia.
- [ ] T011 Aceptación integrada Entra, lector/zoom/teclado físico y cierre FR/SC con evidencia; no sustituir por harness.

T010: mediciones completas publicadas en [performance](evidence/performance.md); pendientes presupuesto LCP/Añadir y revisión manual completa. T011: piloto personal con una cuenta real realizado; siguen pendientes segunda cuenta/roles, lector, zoom completo y dispositivo. El preflight de la suite integrada sigue sin todos sus recursos; no se presenta como test aprobado.

Reconciliación del 30/10: revisados resultados reales, no solo existencia de tests. Pruebas locales: 208 .NET, 117 Web, 33 E2E, lint/typecheck/build y ambos modelos EF sin cambios. El ajuste del test axe de f8032ca pasa además cinco repeticiones WebKit. Al reanudar, tanto f8032ca como el HEAD guardado f81b911 tienen los cinco workflows satisfactorios; los logs de f81b911 confirman Web 117/117 y 120/120 E2E, API relacionales 77/77 y 9/9, builds Docker e integración. T009 se cierra con [ejecuciones y comandos reproducibles](evidence/review-corrections.md#verificación-y-reconciliación--2026-10-30). El commit posterior de cierre solo cambia documentación y su CI se comprueba sobre su propio SHA. T010/T011 siguen abiertas; no se acredita aceptación global.

Definition of Done: todos los FR/SC acreditados, migración compatible, autorización negativa probada, cero duplicación financiera por reintento, Gastos independiente, suites obligatorias satisfactorias y aceptación móvil/integrada completa. Documentar un gate pendiente no lo satisface. No afirmar cierre global mientras T010/T011 u otro criterio siga pendiente.

T001 se acredita por artefactos anteriores a código y revisión main/PR56. T002 por listPresentation y recorrido de historial en tres motores. T003 por selección directa/alta precompletada y los dos tests de recuperación InlineProductCreation (incluidos en 113/113). T004/T005 por CatalogPriceFlow HTTP con PostgreSQL, suites ProductCatalog y E2E de edición/mercados/precios. T006/T007 por Domain 42, Application 35, API 74 PostgreSQL, carrera de cuatro solicitudes, migración/snapshot y Docker. T008 por PurchaseRecovery y recorridos 009 en tres motores. T009 por la matriz de evidencia, tres builds Docker y repetición pertinente tras correcciones. T010/T011 conservan validación completa pendiente; ninguna casilla heredada cambia.
