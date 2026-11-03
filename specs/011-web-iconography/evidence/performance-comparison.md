# Rendimiento final011 — budgets intactos

01/11/2026. Código `0923a0c37012e26f9a998732250f57c04ab0ae31`, huella `cf8d87e788a3b6c75ff45cbe989b2958c1481d4bc8c2abffa04fb85deff3bcf6`. [80muestras finales](performance-final.json), [antes](performance-before.json), [primer011](performance-after.json), [intento anterior al Drawer público](performance-pre-home.json), [diagnóstico](lcp-analysis.md), [build/imports](bundle-final.json).

## Protocolo y conjunto real

Runner measure-performance.mjs original sin cambios respecto a cf83bff: Chromium390x844, CPU4x,1.6Mbps/150ms, caché deshabilitada, modernFixtures(page,50),20muestras/ruta (80). Todas conservadas, sin retries ni selección de rápidas. Sin suites/builds/capturas concurrentes; solo servidores locales inactivos, edición documental/consultas CI ligeras. No es medición de backend ni de campo/INP real.

El fixture histórico produce50ítems personales pero1compartido. Se mantiene para comparabilidad, sin afirmar rendimiento compartido50; la regresión funcional100ítems es otra prueba. No se cambia el dataset para obtener PASS. Baseline previo3bee0ec y primer011cf83bff conservan sus ejecuciones originales, no se presentan como nuevas mediciones de este cierre. [Linaje](performance-lineage.json).

Budgets: LCPp75<=2500ms, interacción<=200ms, CLS<=0.1.

| Ruta | Antes ms | Primer011 ms | Final ms | Δ vs antes ms | CLS final | Interacciones p75 ms | LCP |
|---|---:|---:|---:|---:|---:|---|---|
| home | 2536 | 2668 | 2392 | -144 | 0.0000 | — | PASS |
| lists | 3352 | 2880 | 2660 | -692 | 0.0180 | — | FAIL LCP / deuda previa |
| personal | 3936 | 3196 | 3096 | -840 | 0.0000 | mark 96, add 104 | FAIL LCP / deuda previa |
| shared | 3552 | 3052 | 2964 | -588 | 0.0000 | expenses 32 | FAIL LCP / deuda previa |

**Global LCP budget: FAIL en3rutas. PRE-EXISTING PERFORMANCE DEBT, no bloqueante solo para este PR011 por autorización expresa.** Portada ahora PASS. Todas las rutas mejoran frente a baseline y primer011. CLS máximo0.01804 e interacciones96/104/32ms cumplen. No cierra007/008/009 ni su rendimiento pendiente.

## Transferencia por ruta (bytes p75)

| Ruta | Antes | Primer011 | Final | Δ vs antes |
|---|---:|---:|---:|---:|
| home | 306049 | 327112 | 307053 | +1004 |
| lists | 315530 | 336734 | 316343 | +813 |
| personal | 334448 | 355704 | 339557 | +5109 |
| shared | 335901 | 357091 | 341041 | +5140 |

## Bundle JS/CSS, suma de todos los chunks

| Magnitud | Antes | Primer011/HEAD inicial | Final | Δ vs antes | Δ vs primer011 |
|---|---:|---:|---:|---:|---:|
| Bytes | 1195588 | 1257876 | 1255944 | +60356 | -1932 |
| Gzip por archivo, suma | 357988 | 378590 | 368879 | +10891 | -9711 |

Incluye rutas diferidas; no representa toda la descarga inicial. Coste residual10891bytes gzip frente a antes (3.04%) justificado por22exports estáticos/18significados, wrappers de tooltip/acción y superficies/diálogos de la feature. Una familia5.7.0/MIT, sin nuevas fuentes, motion kit o pack extra. Frente al primer011 se eliminan9711bytes gzip globales y unos16–20KB de transferencia por ruta. No se presume coste cero ni causalidad de cada ms.

## Corrección atribuible y deuda residual

Atribución PARTIAL:011 añadió iconos/Tooltip y presentación al arranque previo. Se retiran nueve exports sin uso, ambos Drawer se cargan bajo demanda y producción/lab comparten agrupación del cierre estático inicial para reducir fan-out y mejorar compresión. No se oculta contenido ni se cambia observer/espera/fixtures del runner. Los intentos Drawer-protegido-solo y renderer-solo permanecen registrados, sin elegirlos como aceptación.

El diagnóstico observa JS/evaluación/render como tramo dominante, fuentes disponibles antes del LCP, páginas diferidas, queries y50filas. La remediación restante requiere investigar bootstrap/MSAL/Fluent, carga de páginas y render, ampliando la revisión de presentación. No se cambia D1/auth ni se introduce SSR, otro UI kit o virtualización aquí. No se afirma que la deuda sea imposible de resolver.

Vite conserva el aviso de chunk inicial>500KB sin comprimir: menor granularidad de caché y trabajo de evaluación siguen siendo riesgos. No se aumenta ni suprime el umbral. No se observa regresión material frente a baseline; se cumple la condición autorizada de deuda previa con CLS/interacciones dentro de budget y payload declarado. Esto no convierte los tres FAIL en PASS.

## Repetición

Desde Web: npm ci; npm run build; node scripts/audit-iconography.mjs; construir tests/visual/vite.performance.config.ts en un directorio lab separado. Servir con scripts/serve-performance.mjs en5180. Ejecutar `node scripts/diagnose-lcp.mjs http://127.0.0.1:5180 final` aparte y, tras cerrar esos navegadores y terminar builds/suites/capturas, `node scripts/measure-performance.mjs http://127.0.0.1:5180 009-011-closure` con PERFORMANCE_SAMPLES=20 y browsers instalados/ruta local adaptada. El label009 conserva la captura opcional original; no significa aceptación nueva de009. No restaurar.env ni auth para medir otro commit.
