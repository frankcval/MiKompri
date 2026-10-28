# Rendimiento de laboratorio — 2026-10-28

La reducción del bundle no equivale a cumplir SC-008. Este informe conserva resultados favorables y desfavorables. No son percentiles de tráfico real ni métricas del acceso interactivo Entra.

## Protocolo reproducible

- Origen baseline: `git archive f362713 MiKompri.Web`, extracción y `npm ci` en carpeta ignorada aislada. Sin revertir archivos del árbol de trabajo. Lockfile original; sin upgrades del baseline.
- Actual: árbol local de 008. Build optimizado independiente con `tests/visual/vite.performance.config.ts`; el harness no se incluye en `dist/` de producción.
- Servidores estáticos loopback separados, gzip igual en ambos, sin caché HTTP. APIs y autenticación controladas; no se mide latencia real entre servicios ni aprovisionamiento.
- Chromium, viewport 390 × 844, caché fría y contexto nuevo por muestra; CDP CPU ×4, descarga 1,6 Mbps, subida 750 kbps, latencia 150 ms. Fixture de 50 productos por detalle. Suites de navegador y compilaciones finalizadas antes de medir.
- Veinte muestras independientes por ruta y versión. Una muestra de marcado/apertura de añadir en cada detalle personal y apertura de Gastos en cada compartida. Baseline y actual se ejecutan secuencialmente.
- LCP y desplazamiento acumulado observados antes de interacción; Event Timing observa duración de eventos por acción. Un valor 0 de evento significa que no se observó entrada ≥16 ms, no latencia física cero. Estas duraciones son un indicador de laboratorio, no INP de campo.
- p75: valor ordenado en posición `ceil(n × 0,75)`, sin retirar muestras lentas. Recursos transferidos medidos por Resource Timing en el mismo origen.

| Caso | Baseline | Actual | Comparabilidad |
| --- | --- | --- | --- |
| Portada | `/` | `/` | Mismo objetivo; comparte componente de compra en actual |
| Índice | `/personal-lists` | `/lists` | Actual agrega compartidas: distinta capacidad, no atribuir toda diferencia a optimización |
| Personal | `/personal-lists/{fixtureId}` | Misma ruta | 50 productos y acciones equivalentes; actual añade gestión |
| Compartida | `/shared-lists/{fixtureId}` | Misma ruta | 50 productos; actual añade roles/gestión/historial |

Comando desde `MiKompri.Web`, con los dos servidores y la caché Playwright configurados:

```powershell
$env:PERFORMANCE_SAMPLES='20'
node scripts/measure-performance.mjs http://127.0.0.1:5176 baseline
node scripts/measure-performance.mjs http://127.0.0.1:5175 current
```

Datos crudos: `.playwright-mcp/008-performance-baseline.json` y `008-performance-current.json`, conservados localmente y sin datos personales. El resumen final se publica al terminar la serie, junto con el estado explícito de cada presupuesto (LCP ≤2.500 ms, interacción ≤200 ms y CLS ≤0,1).

## Optimizaciones entregadas

Entrada JS: aproximadamente 1.064 kB → 369 kB antes de gzip. El resto se difiere por ruta; no se elimina del producto. Tres fuentes TTF (~704 kB) → WOFF2 (~225 kB), con licencia original. Consultas de grupos y perfil de la compartida se inician en paralelo con el detalle para reducir cascadas y la inserción tardía de permisos. Nginx usa gzip y caché inmutable para assets; las rutas SPA tienen fallback y el harness está bloqueado.

**Estado**: serie final completa. Los resultados y límites figuran a continuación; no se usan los preflight tomados durante otras suites.

## Resultados finales: 20 muestras por ruta y versión

Equipo: Intel Core i5-1235U (10 núcleos/12 procesadores lógicos), 16 GB RAM, Windows. Las suites y compilaciones se finalizaron antes de cada serie; permanecieron los servicios locales y procesos normales del escritorio. Una serie anterior interrumpida para corregir contratos se descarta completa, sin mezclar sus observaciones con la serie final.

[Datos baseline](performance-baseline.json) · [Datos actuales](performance-current.json). 160 navegaciones frías y 60 acciones por versión (20 por acción: marcar, añadir, Gastos).

| Ruta | LCP p75 anterior → actual | Bytes p75 anterior → actual | CLS acumulado anterior → actual | Carga ≤2,5 s |
| --- | --- | --- | --- | --- |
| home | 2524 → 1876 ms | 915,292 → 232,457 B | 0.0000 → 0.0000 | Cumple |
| lists | 2384 → 2512 ms | 915,292 → 288,087 B | 0.0000 → 0.0201 | NO cumple |
| personal | 2564 → 2584 ms | 915,292 → 296,685 B | 0.0022 → 0.0001 | NO cumple |
| shared | 2564 → 2692 ms | 915,292 → 307,003 B | 0.0011 → 0.0001 | NO cumple |

| Acción | Duración Event Timing p75 anterior → actual | Umbral ≤200 ms |
| --- | --- | --- |
| personal/mark | 136 → 216 ms | NO cumple |
| personal/add | 232 → 344 ms | NO cumple |
| shared/expenses | 152 → 64 ms | Cumple |

**SC-008: NO acreditado.** Persisten presupuestos incumplidos; T043/A022 permanecen abiertos.

El desplazamiento observado es la suma de cambios sin entrada reciente durante la carga inicial; es una medida conservadora, no el cálculo de ventanas de sesión de Core Web Vitals. No se presenta Event Timing de acciones aisladas como INP de usuarios reales. La comparación del índice tiene distinta capacidad y se informa separada.

Siguiente trabajo si hay incumplimiento: perfilar tareas largas y render de 50 filas, revisar cascadas de módulos/fuentes y reservar espacio de permisos/contexto para evitar desplazamientos. Mantener funcionalidades y repetir el mismo protocolo; no rebajar umbrales para darlo por aprobado.

## Iteración de optimización conservada

La [primera serie actual](performance-current-initial.json), también de veinte muestras por ruta, detectó LCP de 2.120/2.836/2.860/3.288 ms y acciones de marcado/añadir de 304/248 ms. Se conserva completa. Después se difirieron los controles secundarios por producto y la pestaña Gastos, y se sirvieron variantes latinas de ~76 kB con cobertura completa diferida. La comparación principal usa baseline y versión final; no mezcla observaciones de distintas implementaciones. La mejora de portada y transferencia no compensa por sí sola los presupuestos restantes.

Compilación final: entrada JS ~452 kB (~124 kB gzip), más chunks por ruta. El cambio en partición de módulos explica que la entrada sea mayor que los ~369 kB de la primera iteración; la transferencia total observada por ruta es la autoridad para comparar la carga. Chromium 156.0.8078.4, revisión Playwright 1248.
