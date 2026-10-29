# 009 — Shopping Experience Completion

Branch: `009-shopping-experience-completion`. Fecha: 2026-10-28.
Estado: alcance aprobado; implementación local verificada. T010/T011 y aceptación global pendientes, con resultados y límites publicados en evidence/.
Base verificada: main/origin/main `77ddbfc`, merge del PR #56. No cierra 007/008.

## Objetivo e historias

Completar preparación, compra y consulta posterior en listas personales y compartidas.

| Historia | Prioridad | Aceptación independiente |
| --- | --- | --- |
| US1 Comprender estimaciones | P1 | Precio unitario 2 × cantidad 3 y otro desconocido: subtotal conocido 6 EUR y un ítem sin estimación; cero conserva su significado |
| US2 Activas e historial | P1 | Lista no vacía comprada al 100% pasa a historial sin cerrarse; desmarcar vuelve a activas; filtros/orden sobreviven recarga |
| US3 Añadir directamente | P1 | Sin texto hay productos, escribir filtra, una fila selecciona; sin coincidencias ofrece alta contextual recuperable |
| US4 Registrar compra al marcar | P1 | Compra marcada independientemente; captura opcional, persistente, sin duplicados; Gastos utiliza el mismo registro |
| US5 Mantener catálogo/mercados | P2 | Buscar/editar productos y crear/editar/desactivar mercados mediante APIs existentes |
| US6 Precios fiables | P2 | Alta manual de precio y consulta por producto/mercado/fecha con vacío y errores diferenciados |
| US7 Borrado comprensible | P2 | Confirmación/cancelación visible, permisos servidor e historia financiera protegida |

## Requisitos funcionales

- FR-001 Mostrar suma de precio unitario estimado × cantidad, cobertura parcial y desconocidos; no confundir con importe real total.
- FR-002 Lista vacía activa; completada = no vacía y todos comprados. Closed/Archived también son históricas, con motivo visible.
- FR-003 Completar no llama close. Desmarcar/añadir pendientes en Active devuelve a activas, conservando registros financieros. Cerradas siguen sin escritura.
- FR-004 Orden inicial creación descendente; opciones creación ascendente, modificación y nombre; desempate ID, filtro/orden URL y fallos parciales visibles.
- FR-005 Borrado con nombre/consecuencias y cancelación; no eliminar registros financieros por borrar lista/ítem. Long-press evaluado, nunca único mecanismo.
- FR-006 Unificar búsqueda/selección; alta contextual nombre/unidad, conservar producto creado si incorporación falla y no repetir alta confirmada.
- FR-007 Marcar ofrece captura opcional sin obligar a Gastos; omitir/cancelar/error no desmarca ni pierde contexto al completar el último ítem.
- FR-008 Un registro canónico de compra/gasto; precio real total positivo de hasta dos decimales, EUR, mercado opcional y fecha de compra opcional separada de auditoría.
- FR-009 Personal: propietario/comprador/pagador resueltos por identidad, sin participantes colaborativos. Compartida: pagador obligatorio, comprador opcional, participantes activos elegidos explícitamente y permisos 005/008.
- FR-010 Idempotencia persistente por operación: mismo payload devuelve mismo ID; cambio de payload con misma operación devuelve 409; compras distintas explícitas siguen permitidas.
- FR-011 Gastos independiente: consulta/corrección/registro conservados; desmarcar no toca saldos. Corrección con versión evita sobrescritura silenciosa; clientes legacy siguen compatibles.
- FR-012 Reutilizar búsqueda, PUT catálogo, mercados y precios existentes. Definir política global de gestión autenticada sin inventar roles de grupo para catálogo.
- FR-013 Historial distingue producto no seleccionado, carga, vacío, error HTTP/red y reintento; usa nombre de mercado/moneda de la respuesta y filtros existentes.
- FR-014 No publicar automáticamente gasto/estimación como observación de precio de catálogo.
- FR-015 Mantener TP11: Users resuelve `(tid,oid)` a UserId interno, sin acceso cruzado a bases.

## Edge cases

Lista vacía, todos desconocidos, cero legado, eliminar último pendiente, añadir desde historial, desmarcar con gastos, Closed/Archived, empates de orden, varios gastos por ítem, doble envío/concurrencia, timeout tras persistir, refetch fallido tras éxito, pérdida de membresía, participantes retirados, producto/mercado desactivado, duplicado precio producto/mercado/día, fecha futura, rango invertido, cambio cuenta y borradores aislados. Renombrar catálogo no reescribe snapshots de listas; cambiar unidad no reinterpreta precios históricos.

## Criterios de éxito y pruebas

Todos los escenarios FR deben tener evidencia identificada. Dominio/aplicación/API con negativos de autorización y persistencia PostgreSQL/Docker; Web unit/component/integration y E2E móvil/teclado. Un reintento financiero no incrementa gasto ni saldo. Ambas entradas leen el mismo ID. Flujo real precio → historial HTTP y UI. Resoluciones 320/390/430 primero, objetivos 44px, foco, lector, zoom 200%, teclado virtual y rendimiento según protocolo 008. No presentar harness como aceptación Entra, ni cerrar tareas anteriores sin evidencia.

- SC-001: 100% de casos definidos de estimación/orden/clasificación reproducen el resultado esperado, incluido vacío, cero y desconocido.
- SC-002: 100% de reintentos financieros probados preservan un único registro por operación y su saldo; versiones antiguas no sobrescriben correcciones.
- SC-003: Compra, omisión/captura opcional, Gastos, historial y desmarcado persisten en recorridos personal/compartido autorizados; se rechazan actores ajenos.
- SC-004: Flujos críticos sin desbordamiento en 320/390/430/768/1280; teclado/foco y objetivos >=44px. La validación automatizada no sustituye lector, zoom 200% o dispositivo real.
- SC-005: Historial verifica alta HTTP real de precio, filtros, mercado inactivo y vacío/error distintos; cambios de catálogo no escriben gastos.
- SC-006: Publicar 20 muestras por ruta bajo el protocolo 008, con LCP p75 <=2500ms, interacción <=200ms y CLS <=0.1; cualquier incumplimiento permanece explícito y bloquea este criterio.
- SC-007: Antes de cierre global, todos los FR/SC tienen evidencia y las suites obligatorias pasan sin omisiones. Identidades controladas no acreditan Entra multiusuario.

## Dependencias y exclusiones

002 core, 003 identidad/membresías, 004 catálogo/mercados/precios, 005 gastos/reparto, 007 sesión/SPA y 008 permisos/compra reversible/precio desconocido/recuperación. Deuda de aceptación heredada permanece en sus specs.
Fuera: comentarios/chat (feature posterior), implementación de fotos, OCR, tickets, pagos, push, multidivisa, fracciones, sincronización automática de precios, nuevas reglas de reparto, reapertura administrativa, implementación 010/011 y despliegue.
