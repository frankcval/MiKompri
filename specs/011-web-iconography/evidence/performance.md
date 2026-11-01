# Rendimiento011 — comparación sin rebajar gates

> Registro histórico de la primera implementación (31/10). La revisión posterior y el estado publicable se registran en [final-review.md](final-review.md); estos resultados no certifican el candidato posterior.

31/10/2026. Baseline documental3bee0ec y candidato identificado por [huella de fuentes/lock](bundle-after.json). Mismos budgets008/009: LCPp75<=2500ms, interacción<=200ms, CLS<=0.1. Este análisis no cierra tareas de specs anteriores.

## Protocolo

Runner existente `MiKompri.Web/scripts/measure-performance.mjs`, Chromium390x844, CPU4x, red1.6Mbps/150ms, caché deshabilitada, modernFixtures(page,50),20navegaciones por ruta (80total). Las respuestas API son fixtures, no red/backend real. No ejecutar builds, suites ni otro navegador de QA simultáneamente a la medición. No se amplía budget/retries ni se escogen solo muestras rápidas.

El build previo se conservó inmutable antes de instalar React Icons y modificar fuentes; servidor5176. El candidato usa otro directorio/servidor, sin sobrescribir el baseline. Prefijo `009-` en labels mantiene el manejo de la compra opcional que ya incluye el runner; no significa cambio o aceptación de009. No usar label literal `baseline`, que selecciona una ruta antigua distinta.

Desde Web, para repetir el candidato:

```powershell
npx vite build --config tests/visual/vite.performance.config.ts --outDir ../specs/011-web-iconography/.local/after-final-lab --emptyOutDir false
```

Desde raíz, en otra terminal:

```powershell
node MiKompri.Web/scripts/serve-performance.mjs specs/011-web-iconography/.local/after-final-lab 5178
```

Desde Web:

```powershell
node scripts/measure-performance.mjs http://127.0.0.1:5178 009-011-after
```

Configurar la ruta local de browsers según quickstart. Resultado crudo en `.playwright-mcp/008-performance-009-011-after.json`; copia saneada en evidence. Para repetir un build anterior se necesita checkout separado de3bee0ec y el mismo protocolo; no restaurar auth/env del workspace ni sustituir sus APIs.

## Baseline observado

[80muestras previas](performance-before.json), sin builds/suites concurrentes:

| Ruta | n | LCPp75 ms | CLSp75 | Bytes transferidos p75 | Interacciones p75 ms |
|---|---|---|---|---|---|
| home | 20 | 2536 | 0.0000 | 306049 | {} |
| lists | 20 | 3352 | 0.0105 | 315530 | {} |
| personal | 20 | 3936 | 0.0000 | 334448 | {"mark": 264, "add": 304} |
| shared | 20 | 3552 | 0.0000 | 335901 | {"expenses": 96} |

**FAIL previo** de LCP en las cuatro rutas y de interacción personal. Es evidencia de incumplimiento heredado, no una exención para el candidato. El estado final de presupuestos se decide tras comparar las muestras posteriores.

## JS/CSS de producción

[Inventario previo](bundle-before.json), [inventario posterior](bundle-after.json). Reproducir con `npm run build` y `node scripts/audit-iconography.mjs` desde Web. Verifica imports concretos de la única familia y exports de la dependencia instalada, con integridad del lock. Los tests renderizan todos los significados.

| Magnitud | Antes | Candidato | Delta |
|---|---|---|---|
| Bytes JS/CSS de todos los chunks | 1195588 | 1257876 | +62288 |
| Gzip por archivo, suma | 357988 | 378590 | +20602 |

La suma abarca todos los chunks/rutas, no la descarga inicial de una página. No incluye fuentes porque no se modifican. Gzip calculado con el mismo algoritmo por archivo; no equivale a ahorro/incremento sobre cada petición. No se incorpora el catálogo completo de9072exports al registro app:31imports/23claves; el tamaño de red se mide por ruta, no se infiere solo del tree-shaking.

## Candidato posterior

[80muestras posteriores](performance-after.json), tomadas tras terminar suites/builds/capturas. Sin otra prueba/build/navegador QA concurrente.

| Ruta | LCP antes ms | LCP después ms | Delta ms | CLS después | Interacción después p75 ms | Presupuesto |
|---|---|---|---|---|---|---|
| home | 2536 | 2668 | +132 | 0.0000 | {} | FAIL LCP |
| lists | 3352 | 2880 | -472 | 0.0180 | {} | FAIL LCP |
| personal | 3936 | 3196 | -740 | 0.0000 | {"mark": 136, "add": 96} | FAIL LCP |
| shared | 3552 | 3052 | -500 | 0.0000 | {"expenses": 32} | FAIL LCP |

**FAIL de LCP en las cuatro rutas**. Interacciones personal136/96ms y Gastos32ms sí cumplen200ms; CLS máximo0.0180 cumple0.1. Antes las interacciones personales264/304ms fallaban. El listado/detalles tienen valores LCP inferiores en esta pasada; portada aumenta132ms. Esto es una comparación de laboratorio, no prueba causal ni aceptación de rendimiento global.

La transferencia p75 aumenta21063/21204/21256/21190bytes respectivamente para portada/listas/personal/compartida, aproximadamente21KB por ruta del laboratorio. El registro/Tooltip compartidos añaden peso también a portada: no se presume coste cero por icono ni se presenta el mejor valor de otra ruta para ocultarlo. Una pasada por candidato no aísla causas de variación temporal del host.

T027 completa la **medición y documentación**, no el cumplimiento de presupuestos. **SC006/AC11 y cierre de011 siguen abiertos**. Siguiente trabajo acotado: perfilar carga crítica/chunks y waterfall de las rutas actuales antes de proponer optimizaciones; conservar estas muestras/budgets y comprobar la intervención con el mismo protocolo. No ampliar funcionalidad ni cerrar007/008/009 como efecto colateral.

Aclaración del conjunto histórico: modernFixtures(page,50) genera50ítems personales y1compartido. Esto se aplica a las mediciones anteriores y posteriores; no se cambia el fixture/runner para esta revisión y no se afirma aceptación de rendimiento compartido con50ítems. La prueba funcional100ítems no sustituye esa medición.

## Estado actual — 01/11/2026

[Comparación final](performance-comparison.md): portada2392PASS; listas2660/personal3096/compartida2964FAIL LCP como deuda previa no bloqueante autorizada para011. No PASS global ni cambio de budgets. CLS/interacciones cumplen; todas las rutas mejoran frente a baseline. Payload, atribución y límites explícitos.
