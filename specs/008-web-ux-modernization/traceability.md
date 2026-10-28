# Trazabilidad y reconciliación 007 → 008

Fecha: 2026-10-28. Identificadores completos: `007/FR-010`, `008/FR-005`, `008/T012`. Tareas 008 son unidades de ejecución de esta evolución, no una segunda implementación de lo ya hecho. No cerrar requisitos anteriores solo por referenciarlos aquí.

## Deuda, ampliación y evidencia

| Origen | Tipo | Tratamiento en 008 | Cierre/evidencia |
| --- | --- | --- | --- |
| 007/FR-010; T057–061/T112 | Deuda | US2 CRUD y parte de US3 según permiso | T011–022; recarga, lista correcta, negativos |
| 007/FR-009; T056/T113 | Deuda mínima | US3 ver miembros/roles y enlazar listas | T017/019/022; no invita por GUID |
| 007/FR-011; 005/FR-003/004; T111/T132 | Deuda/bloqueo | US1 autorización y US3 operaciones compartidas | T004/007–010/017–022; matriz backend, no selección UI |
| 007/T133 | Entregado histórico | Preservar regresión ItemId distinto de ProductId | T007/013; no reabrir defecto como si no se hubiese corregido |
| 007/FR-012; T069/T071/T114 | Deuda y mejora | US6 RHF, gastos, balances/historia y corrección por rol | T032–036; no recalcular finanzas |
| 007/FR-013/034 | Implementado básico + mejora | US4 alta contextual, búsqueda y saga recuperable | T021/023–026; catálogo actual no se reimplementa |
| 007/FR-007/008/015/016; T047/T115 | Deuda + continuidad nueva | US5 reauth, errores, perfil refetch/sync | T027–031 |
| 007/FR-014/032 | Cambio prospectivo | US5 destino por intención; US7 Mis listas conservando destinos/rutas | T028/031/038/039; conducta anterior sigue vigente hasta entrega |
| 007/FR-033/034; T108–110 | Base entregada + mejora | US7 tokens, lista común, jerarquía y contexto URL | T037–044; no borrar pruebas/capturas históricas |
| 002/FR-005; restricción 007/FR-034 | Ampliación expresa | Desmarcar con estado explícito, sin efectos financieros | T013–016/020; endpoint aditivo y negativos |
| Precio obligatorio por contrato actual | Ampliación expresa | Estimación desconocida distinta de cero, sin reinterpretar legado | T004/021/025; compatibilidad/migración verificadas |
| 007/FR-017/018; T116 | Verificación pendiente | US7/US8 accesibilidad ampliada | T041/044/046; zoom/lector/contraste real |
| 007/FR-023; SC-007; T117/089 | Evidencia/CI pendiente | US8 obligatorios seleccionados, ejecutados y publicados | T006/031/036/047/048; cero skips obligatorios |
| 007/SC-001/002/003/004/005/006/007 | Evidencia pendiente | US8 protocolo piloto/estados/accesibilidad/integración | T046/048/050/054, sin cerrar por harness |
| 007/T093/T118 | Regresión/cierre | US8 y cierre individual de deuda afectada | T048/052/054; resultados nuevos separados de históricos |
| 007 fuera de alcance | Límite conservado | Sin invitaciones, pagos reales, roles, OCR/push/offline completo ni administración catálogo/mercados | T002/037/053/054 |

## Cobertura requisito → tarea

| Requisito 008 | Historia | Tareas | Comprobación principal |
| --- | --- | --- | --- |
| FR-001 | Transversal | T001,T002,T052,T054 | Deuda y ampliación sin doble cierre |
| FR-002 | US1 | T004,T007,T008,T009,T010 | Acceso por recurso y negativos |
| FR-003 | US1/3 | T005,T007,T009,T017,T018,T022 | Roles, baja y auditoría |
| FR-004 | US2 | T011,T012,T016 | CRUD personal/recarga |
| FR-005 | US2/3 | T011,T012,T017,T018,T020 | Producto correcto/permiso |
| FR-006 | US2/3 | T013,T014,T015,T020 | Estado reversible/gastos intactos |
| FR-007 | US2 | T013,T015,T016 | Pending individual y resultado incierto |
| FR-008 | US3 | T017,T018,T019,T020,T022 | Personas/listas/permisos |
| FR-009 | US3/6 | T017,T021,T032,T033,T035 | Autoría e historia retirada |
| FR-010 | US4 | T004,T021,T023,T024,T025 | Buscar/crear/estimación |
| FR-011 | US4 | T023,T025,T026 | Dos pasos recuperables |
| FR-012 | US5 | T027,T028,T031 | Destino interno seguro |
| FR-013 | US5 | T027,T028,T029,T031 | Aislamiento/TTL/no replay |
| FR-014 | US5/8 | T029,T030,T031,T046,T050 | Matriz de estado y comprensión |
| FR-015 | US5 | T029,T031 | Refetch frente a sync |
| FR-016 | US6 | T033,T034,T036 | Personas reales/revisión |
| FR-017 | US6 | T032,T033,T034,T036 | Corrección/roles/historia |
| FR-018 | US6 | T035,T036,T050 | Deuda sin pago real |
| FR-019 | US7 | T037,T038,T039,T044 | Mis listas y rutas antiguas |
| FR-020 | US7 | T037,T040,T044 | Demo aislada/capacidades |
| FR-021 | US7 | T037,T040,T041,T044 | Jerarquía/foco estable |
| FR-022 | US5/7 | T031,T038,T039,T042,T044 | URL/recarga/Atrás/safe areas |
| FR-023 | US7/8 | T041,T044,T045,T046 | Móvil/lector/teclado/contraste |
| FR-024 | US7/8 | T001,T043,T049 | Perfil comparable/recursos diferidos |
| FR-025 | US8 | T003,T006,T047,T048 | Suites reales/CI |
| FR-026 | US8 | T050,T051,T054 | Piloto y no falsos cierres |
| FR-027 | US1 | T004,T007,T008,T009,T010 | TP11 |
| FR-028 | Transversal | T002,T037,T053,T054 | Alcance sin simulaciones |

## Resultados medibles → tareas y método

| Criterio | Tareas | Evidencia exigida |
| --- | --- | --- |
| SC-001 | T007–010,T017,T022,T033,T048 | Matriz completa y resultados no autorizados |
| SC-002 | T011–016,T022,T026,T036,T048 | Lectura tras recarga y otra lista intacta |
| SC-003 | T031,T050 | Piloto ≥10, acceso sin ayuda y tiempo |
| SC-004 | T022,T026,T036,T048,T050 | Recorrido compartido completo, ≥9/10 |
| SC-005 | T035,T050 | Preguntas de deuda/propuesta, ≥9/10 |
| SC-006 | T041,T044,T045,T046 | Matriz accesibilidad y cero bloqueantes |
| SC-007 | T030,T046,T048,T050 | Casos de error ejecutados, revisión humana ≥95% |
| SC-008 | T001,T043,T049 | Datos crudos, perfil, p75 por ruta/acción |
| SC-009 | T002,T006,T047,T048,T052,T054 | Inventario completo, cero skips obligatorios |

Las 54 tareas están asociadas a FR/SC directamente o como preparación/cierre. En P3 se añade revisión exacta, fecha, comando y resultado por fila, manteniendo el estado pendiente donde falte evidencia. Documentación consistente no equivale a cumplimiento implementado.

## Estado implementado

Ver [validación](evidence/validation.md) y [tareas con estado individual](tasks.md). Los contratos aditivos ya están implementados localmente. Se conservan los límites históricos, la separación de pruebas controladas/Entra y los gates de piloto, accesibilidad ampliada y rendimiento. La navegación móvil incluye barra inferior con safe areas, Drawer y menú de cuenta; se oculta al enfocar campos. Las acciones de gestión se compactaron para dar prioridad a productos, con nombres accesibles completos y confirmación de eliminación.
