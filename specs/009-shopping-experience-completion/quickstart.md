# Validar 009

Requiere frontend y APIs del mismo candidato. Configuración existente Entra/audience/Users preservada; datos y sesiones de prueba aislados. No ejecutar fixtures sobre datos personales.

1. Aplicar la migración `20261028213746_AddPurchaseDetailsAndOperations` en PostgreSQL por Docker/flujo existente. Versiones se inicializan; fecha real y mercado anteriores permanecen null. No inferir datos reales de ExpenseDate/CreatedAt. `dotnet ef migrations has-pending-model-changes` debe quedar limpio.
2. Crear dos listas personales, una compartida y producto/mercado QA. Estimaciones 2 × 3 y otro desconocido: 6 EUR conocidos y uno sin precio; cero conocido permanece conocido.
3. En los tres índices cambiar Activas/Historial y criterios de orden; comprobar URL/Atrás/recarga. Completar lista no vacía: desaparece de activas sin cerrarse. Abrir detalle desde historial y desmarcar: vuelve a activas, gastos intactos. Lista vacía permanece activa.
4. Añadir: sin texto hay productos; escribir filtra; seleccionar fila no exige segundo selector. Sin coincidencias crear con nombre precompletado. Simular incorporación fallida tras alta y confirmar reutilización del producto creado.
5. Marcar y omitir captura: compra permanece. Repetir en otra lista con importe total, mercado y fecha. Personales no muestran participantes; shared exige selección y pagador permitido. Gastos independiente muestra mismo registro.
6. Perder respuesta de alta y reintentar misma operación: mismo ID, un registro/saldo. Cambiar payload con mismo ID: 409. Dos correcciones con versión antigua: la segunda 409. Desmarcar no borra gastos. Borrado de lista/ítem con registros: 409; cancelar confirmación no modifica datos.
7. Catálogo: buscar, editar nombre/unidad (sin convertir historia); mercados crear/editar/desactivar; registrar precio manual y consultar historial por mercado/fechas, incluyendo mercado inactivo. Red fallida no equivale a vacío.
8. Verificar primero 320/390/430, luego 768/1280; teclado, foco, 44px, lector/zoom y dispositivo real. Pruebas controladas y Entra real son ejecuciones separadas.

[Resultados y límites](evidence/validation.md). Tests 009 están en las suites existentes y Playwright controlado `modernization/completion.spec.ts`. No cerrar T011 ni tareas heredadas sin evidencia independiente.
