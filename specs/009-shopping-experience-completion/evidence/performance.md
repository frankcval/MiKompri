# Rendimiento de laboratorio 009 — 2026-10-29

Serie completa de 80 navegaciones frías, 20 por ruta, sobre build separado del candidato. Chromium, viewport 390×844, CPU ×4, descarga 1.6 Mbps/150ms, fixture de 50 ítems. Servidor loopback con gzip y sin caché, sin suites/compilaciones simultáneas. Auth/API controladas; no mide Entra, latencia entre servicios ni tráfico real. [Datos íntegros](performance.json).

La primera serie incompleta se conserva en `.playwright-mcp/009-performance.log`; detectó la carrera de dos diálogos y no se mezcla con la repetición. Serie final en `.playwright-mcp/009-performance-final.log`. Se espera o cancela la captura opcional antes de Añadir y se excluyen del muestreo de Añadir los eventos de cancelación. Sigue siendo Event Timing de laboratorio, no INP de campo.

| Ruta | LCP p75 008 → 009 | CLS 009 | Bytes p75 009 | LCP <=2500ms |
| --- | --- | --- | --- | --- |
| home | 1876 → 1972 ms | 0.0000 | 238,529 | Sí |
| lists | 2512 → 2600 ms | 0.0106 | 292,646 | NO |
| personal | 2584 → 2764 ms | 0.0001 | 306,495 | NO |
| shared | 2692 → 2712 ms | 0.0001 | 308,550 | NO |

| Acción | p75 009 | <=200ms |
| --- | --- | --- |
| personal/mark | 200 ms | Sí |
| personal/add | 208 ms | NO |
| shared/expenses | 64 ms | Sí |

**SC-006 no acreditado. T010 sigue abierto.** CLS cumple, portada cumple y marcado/Gastos cumplen; índices/detalles y Añadir aún exceden el presupuesto. No se relajan umbrales ni se cierra SC-008 de 008 por esta medición.

Los números anteriores de 008 son referencia histórica, no un experimento emparejado del mismo día: cambiaron capacidad del índice y recorrido de compra, y el estado del escritorio puede influir. No atribuir diferencias pequeñas a una optimización concreta. Hardware de laboratorio existente Windows/i5-1235U/16GB; Node 24.22.1 y Chromium de caché Playwright local.

Siguiente trabajo de rendimiento: perfilar cadena de chunks/fuentes y render de 50 filas antes de cambios, mantener captura opcional/estimaciones/roles y repetir la serie completa sobre el mismo perfil. Lector/zoom/dispositivo/Entra conservan sus verificaciones independientes.

## Serie intermedia de cierre

Tras perfilado, CreateListDialog y ListItemsPanel diferidos, formato de moneda reutilizado y precargas del harness iguales a producción: [80 muestras separadas](performance-intermediate.json), log `.playwright-mcp/009-close-performance.log`. Mismo protocolo, sin mezclar resultados con la serie anterior.

| Ruta | LCP p75 | CLS p75 | Bytes p75 |
| --- | --- | --- | --- |
| home | 2504ms | 0 | 241669 |
| lists | 2728ms | 0.0105 | 251199 |
| personal | 3152ms | 0 | 271836 |
| shared | 2944ms | 0 | 274564 |

Marcado 168ms, Añadir 248ms, Gastos 56ms. No cumple SC-006. Aunque se reducen bytes, la carga no mejora en esta serie y Añadir empeora. Se revierte ListItemsPanel diferido; no se reducen objetivos, no se seleccionan solo muestras favorables ni se afirma una mejora causal. Esta serie precede al último cambio de menú/foco y no certifica ese candidato.
