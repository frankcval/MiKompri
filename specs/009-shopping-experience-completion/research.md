# Auditoría y límites — 009

Fuente: análisis aprobado en conversación, código de main 77ddbfc y specs 002/003/004/005/007/008, contratos y evidencia PR56/validation/performance/pilot 008. PR56 integrado no equivale a aceptación global. No cerrar tareas previas. Contratos antiguos 003/005 con sub no sustituyen TP11 y correlación vigente (tid,oid) → UserId.

Historial: ruta coincide; backend filtra y ordena, Web omite marketName/currency en tipos, usa mercados activos y fija EUR; red sin status se presenta como vacío y sin producto no distingue selección pendiente. No se ofrece POST product-prices desde Web. No afirmar que una consulta backend falla sin reproducción runtime. Tests anteriores de history: Application 2 y controlador simulado 1 pasaron; no equivalen a HTTP/persistencia. E2E previo solo comprueba encabezados. 009 añadirá prueba HTTP con precio real y estados UI.

Long-press: no añadir por ahora; acción visible confirmada satisface móvil y teclado, evitando conflicto scroll/selección. Fotos: investigación únicamente; referencia y metadatos en ProductCatalog, objetos Azure compatibles, URLs HTTPS controladas/firmadas, JPEG/PNG/WebP, validación de firma/tamaño/dimensiones, saneamiento/EXIF y permisos; no URL arbitraria ni SVG no saneado. Requiere alcance separado para infraestructura/ciclo de vida.
