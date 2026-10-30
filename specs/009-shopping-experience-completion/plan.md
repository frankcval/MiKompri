# Plan — 009

## Base, constitución y secuencia

Revisión main/origin/main 77ddbfc y rama limpia antes de documentación. No AGENTS.md encontrado. Alcance aprobado en conversación antes de código. PP1–5: historias independientes, core móvil y trazabilidad; TP1–3: .NET/React y monorepo actual; TP4: integración PostgreSQL/Docker aislada; TP5/6: Azure compatible, MAUI sin cambios; TP7: OpenAPI aditivo; TP8: tests por capas; TP9: decisiones aquí; TP10: spec/plan/tasks previos; TP11: Entra/UserId vigentes. Sin nuevos servicios ni acceso cruzado DB. No dispensas de aceptación 007/008.

1. Especificación y baseline. 2. Estimaciones/orden/historial. 3. Selector unificado. 4. Catálogo/mercados/precios. 5. Backend compra personal/metadatos/idempotencia/concurrencia. 6. Captura contextual y Gastos. 7. Pruebas y evidencia.

## Diseño móvil

Conservar Source Sans 3, primario #006B68, fondo/superficies/texto actuales de theme y AppSurface; sin segundo sistema visual. Alineación izquierda, resumen legible encima de compra, filtros compactos Activas/Historial y selector de orden; fila de producto seleccionable en diálogo con cantidad/precio debajo. Formulario de compra opcional en diálogo, cierre Ahora no. 44px, teclado/foco/restauración, errores accionables. No rediseño ornamental ni iconografía 011. Long-press pospuesto: menú visible ya cubre móvil sin conflicto con scroll ni descubrimiento.

## Decisiones de datos/contrato

Completitud derivada, no nuevo estado/migración de lista. Resúmenes usan DTO ya existente con timestamps/items/progreso, sin N+1 por tarjeta. Estimación unitario × cantidad, desconocido nunca cero implícito.

Mantener ItemExpenseRecord como único registro monetario: compartir infraestructura también para personal, participante implícito propietario solo internamente (sin exponer selector colaborativo), preservando reglas existentes compartidas. Añadir mercado GUID opcional como referencia externa, PurchasedOn nullable (sin inferir del ExpenseDate legado), versión y operación/hash de solicitud. Mercado no crea FK cross-context ni consulta obligatoria de catálogo al registrar: referencia opcional, nombres resueltos por contrato frontend con fallback histórico. Nuevas capturas usan operación obligatoria; contratos shared legacy aceptan operación opcional para compatibilidad, Web siempre la envía.

Persistir recibos de operación separados del gasto para evitar que borrar/corregir el gasto permita repetir una operación ya confirmada. Índice único global por operación; scope actor/lista/ítem y hash canónico validado antes de replay. Alta transaccional en UoW; colisión concurrente se traduce a 409 reintentable con la misma operación (no 500), reintento devuelve ID existente. Versión GUID concurrency token en gasto; PATCH opcional expectedVersion aditivo, Web lo envía. Mantener IDs históricos y asociaciones de reparto.

Catálogo global: política vigente autenticado puede mantener productos/mercados/precios, explícita y documentada; no usar Owner/Admin de grupos. Conservar JWT y configurar validación de scopes como APIs actuales cuando corresponda. Edición duplicada devuelve 409 consistente con alta.

## Validación

Tests específicos antes de reglas, después suites afectadas y no regresión. Reproducción HTTP con WebApplicationFactory real y persistencia PostgreSQL aislada cuando Docker disponible. Playwright controlado en tres motores; integración Entra separada si hay identidades autorizadas. Evidencia registra comando/revisión/resultado/límites, sin completar gates por presencia de archivos.

## Cierre de rendimiento — 2026-10-29

Perfil de tres rutas con el mismo límite móvil: el chunk de validación de formularios (~36kB gzip) se carga aun con diálogos cerrados y retrasa la lectura principal. Diferir CreateListDialogContent y ListItemsPanel hasta apertura conserva formularios/reglas, foco y borradores; mostrar feedback durante carga. Reutilizar Intl.NumberFormat entre filas. El harness no tenía las precargas 400/600 de index.html de producción: alinearlas, sin cambiar fuentes ni presupuestos. Conservar mediciones anteriores y publicar una serie completa nueva; comprobar contenido del selector además de apertura del diálogo para no presentar un shell vacío como operación terminada.

El perfil también identifica el Drawer móvil cerrado en la cadena inicial de todas las rutas, incluida portada (~53kB gzip compartidos de superficies/foco). Cargar su contenido al pedir el menú, conservar el componente montado tras la primera apertura y mantener restauración de foco/cancelación durante carga. Repetir regresión de navegación/teclado y medir una serie independiente, sin sustituir las anteriores.

La serie intermedia con selección diferida reduce bytes, pero no acredita el presupuesto y empeora Añadir (248ms). Se revierte esa división de ListItemsPanel, conservando selección directa, reglas y borradores. Mantener los umbrales originales y publicar también el intento; no atribuir las diferencias exclusivamente a la división de código. La captura recuerda el elemento invocador antes del guardado y restaura foco también al cancelar el fallback de carga; el diálogo recibe foco al montarse, aun con consultas pendientes.

Se retira también el menú diferido tras regresión de primer Tab en WebKit; las cinco resoluciones de portada vuelven a pasar con el Drawer original. El perfil registra la fuente 700 de títulos/marca solicitada a 2143–2361ms y terminada a 2686–3000ms, aunque siempre aparece sobre el pliegue. Precargar también esa variante existente en producción y harness; sin cambiar tipografía, peso, contenido ni presupuesto. Validar de nuevo los tres motores y medir el candidato final separado.

El perfil con precarga 700 obtiene 2360ms en índice y 2696/2708ms en detalles (exploratorio, no p75). En detalles, la validación de Añadir (~36kB gzip) termina a 2435ms antes de comenzar la lectura de lista. Diferir el diálogo Añadir completo hasta petición, con feedback y cancelación de apertura durante carga. Mantener ListItemsPanel síncrono dentro de ese módulo, sin segunda espera al recibir catálogo. Conservar foco, borradores, selección directa y bloqueo durante captura. Verificar nuevamente componentes y navegadores antes de medir 20 muestras por ruta; no usar el perfil de tres navegaciones como gate aprobado.

La carga diferida expone una carrera previa en captura: sustituir el botón del fallback por el botón del formulario puede perder un clic durante el cambio. `PurchaseCaptureDialog` mantiene ventana, foco y «Ahora no» estables; solo difiere el contenido financiero. Los padres conservan el invocador y el diálogo bloquea cierre durante escritura. Prueba explícita con módulo retenido, foco en cancelar antes/después de cargar y retorno al checkbox; otra prueba retiene Añadir y verifica cancelación sin reapertura tardía, modalidad y objetivo 44px. No esperar artificialmente al formulario para ocultar la carrera en los tests anteriores.

La serie de entrega 13d119f mejora acciones a 80/80/32ms, pero detalles quedan en 2556/2604ms. Se evaluó `content-visibility:auto` y se retiró; no forma parte del código entregado. El perfil exploratorio no acreditó el presupuesto. T010 permanece abierto y sus criterios no cambian.

## Correcciones de revisión — fechas y trazabilidad

Alcance limitado a feature activa Spec Kit, reconciliación T009, fechas civiles y CI del candidato. T010/T011, catálogo global, identidad/permisos, 010/011 y aceptación humana no cambian. Users no se dispara por push a 009, pero sí por PR hacia main: ejecución 40934426515 satisfactoria sobre 13d119f. No modificar sus triggers si el PR acredita el nuevo HEAD.

`PurchasedOn` y `EffectiveDate` son fechas civiles elegidas por el usuario, sin conversión al guardarlas. La Web calcula fecha/max/default con componentes locales y comunica `X-Client-Utc-Offset-Minutes` (minutos al este de UTC, offset actual). Las API obtienen el instante de `TimeProvider`, calculan hoy con ese offset validado [-840,840] y usan UTC si se omite, por compatibilidad. Header inválido devuelve 400; no afecta identidad/autorización ni se persiste. No se implementan zonas históricas ni calendarios.

El dominio recibe `today` explícito para validar DateOnly, sin leer reloj ni depender de una región. ProductCatalog reutiliza su abstracción de fecha; ShoppingList incorpora la equivalente y un default UTC para consumidores de aplicación existentes. Los adaptadores HTTP residen en API. Auditoría continúa en UTC. No cambia EF ni requiere migración. Pruebas: UTC/local a ambos lados del cambio de día, mañana rechazado, metadata en alta/corrección personal/compartida, precios e historial; reloj fijado en API y navegador.
