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

Revisión de correcciones, pausa solicitada el 29/10: las pruebas locales del código de fechas están acreditadas (208 .NET, 117 Web, 33 E2E, lint/typecheck/build y EF sin cambios). El candidato 487f649 dejó un fallo axe transitorio en WebKit; f8032ca espera las animaciones antes de la auditoría y pasa 33/33 E2E más 5/5 repeticiones WebKit. En f8032ca Users, ShoppingList, ProductCatalog y Resource ya están verdes; falta conocer el resultado final de Web (run 40963002962, en curso al guardar), reconciliar T009 con ese resultado y verificar los cinco workflows del HEAD final que incluya la documentación. T009 sigue abierta por esas comprobaciones concretas, no por ausencia de pruebas locales. Evidencia y reanudación en [review-corrections](evidence/review-corrections.md). T010/T011 siguen abiertas y no condicionan documentar la verificación técnica de T009.

Definition of Done: todos los FR/SC acreditados, migración compatible, autorización negativa probada, cero duplicación financiera por reintento, Gastos independiente, suites obligatorias satisfactorias y aceptación móvil/integrada completa. Documentar un gate pendiente no lo satisface. No afirmar cierre global mientras T010/T011 u otro criterio siga pendiente.

T001 se acredita por artefactos anteriores a código y revisión main/PR56. T002 por listPresentation y recorrido de historial en tres motores. T003 por selección directa/alta precompletada y los dos tests de recuperación InlineProductCreation (incluidos en 113/113). T004/T005 por CatalogPriceFlow HTTP con PostgreSQL, suites ProductCatalog y E2E de edición/mercados/precios. T006/T007 por Domain 42, Application 35, API 74 PostgreSQL, carrera de cuatro solicitudes, migración/snapshot y Docker. T008 por PurchaseRecovery y recorridos 009 en tres motores. T009 por la matriz de evidencia, tres builds Docker y repetición pertinente tras correcciones. T010/T011 conservan validación completa pendiente; ninguna casilla heredada cambia.
