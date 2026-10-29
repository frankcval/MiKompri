# Tasks — 009

Estado al 2026-10-29: implementación funcional y verificaciones automatizadas según [validation](evidence/validation.md); aceptación completa pendiente. No modificar completitud 002–008.

- [x] T001 Formalizar spec/plan/modelo/contratos, auditoría y alcances futuros.
- [x] T002 Probar e implementar estimaciones/orden/activas/historial en todos los índices y detalles.
- [x] T003 Unificar búsqueda/selección/alta contextual con recuperación de incorporación fallida.
- [x] T004 Reproducir historial HTTP y conectar DTO/estados/filtros/registro manual.
- [x] T005 Conectar edición de producto y gestión de mercados; documentar permisos globales y errores.
- [x] T006 Tests dominio/backend para compra personal, mercado/fecha, idempotencia y concurrencia.
- [x] T007 Implementar contratos backend aditivos, persistencia/migración y protección de historia.
- [x] T008 Captura opcional desde marcado y acceso independiente de Gastos, mismos registros e idempotencia Web.
- [ ] T009 Completar pruebas Web/API/Docker y regresión, documentar resultados y límites.
- [ ] T010 Validar móvil/teclado/errores/recarga/historial y rendimiento; registrar evidencia.
- [ ] T011 Aceptación integrada Entra, lector/zoom/teclado físico y cierre FR/SC con evidencia; no sustituir por harness.

T010: mediciones completas publicadas en [performance](evidence/performance.md); pendientes presupuesto LCP/Añadir y revisión manual completa. T011: piloto personal con una cuenta real realizado; siguen pendientes segunda cuenta/roles, lector, zoom completo y dispositivo. El preflight de la suite integrada sigue sin todos sus recursos; no se presenta como test aprobado.

Revisión de correcciones: el código 13d119f ya obtuvo cinco workflows satisfactorios por PR #57, incluida la matriz Web 114/114 y Docker; el texto anterior se había escrito antes de ese resultado. Las correcciones de fechas introducen otro candidato: T009 sigue abierta hasta acreditar sus tests afectados, lint/typecheck/build, EF sin cambios y los cinco workflows sobre el nuevo HEAD. Evidencia vigente y decisión en [review-corrections](evidence/review-corrections.md). No se exige cerrar T010/T011 para documentar la verificación técnica de T009, ni se presentan como satisfechas.

Definition of Done: todos los FR/SC acreditados, migración compatible, autorización negativa probada, cero duplicación financiera por reintento, Gastos independiente, suites obligatorias satisfactorias y aceptación móvil/integrada completa. Documentar un gate pendiente no lo satisface. No afirmar cierre global mientras T010/T011 u otro criterio siga pendiente.

T001 se acredita por artefactos anteriores a código y revisión main/PR56. T002 por listPresentation y recorrido de historial en tres motores. T003 por selección directa/alta precompletada y los dos tests de recuperación InlineProductCreation (incluidos en 113/113). T004/T005 por CatalogPriceFlow HTTP con PostgreSQL, suites ProductCatalog y E2E de edición/mercados/precios. T006/T007 por Domain 42, Application 35, API 74 PostgreSQL, carrera de cuatro solicitudes, migración/snapshot y Docker. T008 por PurchaseRecovery y recorridos 009 en tres motores. T009 por la matriz de evidencia, tres builds Docker y repetición pertinente tras correcciones. T010/T011 conservan validación completa pendiente; ninguna casilla heredada cambia.
