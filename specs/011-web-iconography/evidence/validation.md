# Validación011 — candidato de implementación

31/10/2026, rama `011-web-iconography`, base documental `3bee0ec`. [Build y huellas del candidato](bundle-after.json), [implementación](implementation.md), [accesibilidad y límites](accessibility.md). No se atribuye una ejecución local al CI remoto ni al commit base limpio.

## Comandos reproducibles

Desde `MiKompri.Web`, Node24 y dependencias del lock:

```powershell
npm ci
npm run test:ci-config
npm run lint
npm run typecheck
npx vitest run --reporter verbose --maxWorkers 2
$env:PLAYWRIGHT_ALL_BROWSERS='1'
$env:PLAYWRIGHT_BROWSERS_PATH='E:\Projects\MiKompri\.playwright-browsers'
npx playwright test --grep '@smoke' --reporter line --output test-results/011-verified
npm run build
node scripts/audit-iconography.mjs
```

La ruta de browsers se adapta al entorno; no es una credencial. En CI se instalan los motores según workflow. No exportar sesión/tokens para reproducir los tests controlados.

Desde raíz:

```powershell
docker build -t mikompri.web:011-review -f MiKompri.Web/Dockerfile .
docker run --rm --network none --entrypoint sh mikompri.web:011-review -c 'test -f /usr/share/nginx/html/index.html && test ! -d /usr/share/nginx/html/tests && test ! -f /usr/share/nginx/html/.env.local && nginx -t'
git diff --check
```

## Resultados

| Control | Resultado |
|---|---|
| Vitest completo | PASS150/150 en46archivos en la ejecución final JSON; incluye50 casos bajo directorios auth, providers/sesión/perfil/aislamiento y regresiones de compra |
| Requisitos configuración de CI | PASS6/6; no se ejecutan gates Entra reales sin su configuración |
| Lint | PASS |
| Typecheck | PASS |
| Build producción | PASS;31exports concretos verificados, MIT, dependencia exacta5.7.0 |
| E2E smoke Chromium/Firefox/WebKit | PASS144/144,48 por motor,7.7min; pasada completa posterior a la corrección de foco y sin solapamiento de artefactos |
| Docker | PASS build y nginx -t; runtime sin tests ni `.env.local`. Imagen de revisión local, no despliegue |
| Rendimiento | 80muestras antes y80después; FAIL LCP4rutas (2668/2880/3196/3052ms), PASS CLS/interacciones del candidato; [comparación](performance.md), sin relajar gates |
| CI remoto | Pendiente de verificar el commit que publica este candidato. Añadida rama011 al trigger Web; commit/push no equivale a CI aprobado |

Una primera ejecución completa Vitest se interrumpió sin resultado final; no se usa como PASS. La ejecución detallada posterior es la acreditada. La primera pasada E2E detectó el falso cruce item-9/item-99 del fixture; se corrigió y el resultado final debe corresponder a la nueva pasada completa, no a las pruebas fallidas/interrumpidas.

## Límites y control de alcance

Las pruebas de navegador usan harness/API simuladas y datos sintéticos; no acreditan nuevas sesiones Google/Microsoft reales. Sus botones/rutas y las regresiones auth están cubiertos, sin cambios de configuración ni de implementación de autenticación. El titular confirmó después el recorrido humano del candidato: véase la confirmación manual al final de este documento. T024 queda cerrada combinando esa evidencia con las regresiones automáticas, sin atribuir autenticación real al harness.

No hay cambios en backend, contratos HTTP, EF/modelo/migraciones, identidad, autorizaciones ni configuración cloud. No procede ejecutar migraciones o modificar datos para011. No se marca ninguna tarea anterior007/008/009/010 como hecha. CI final, accesibilidad humana y presupuestos no se dan por completados sin resultado.

## Incidencias de validación intermedias

La primera pasada completa terminó140 PASS/4 FAIL: dos fallos reales de restauración de foco en confirmación (Firefox/WebKit) y dos ENOENT de artefactos WebKit al solaparse una ejecución dirigida sobre el mismo directorio de resultados. Esa pasada no es un PASS. Se evita repetir el solapamiento y la siguiente pasada completa usa output propio.

ConfirmAction ahora conserva una referencia al disparador y restaura su foco después de cerrar/cancelar/Escape, siempre que siga montado. La prueba separa Escape del Drawer de Escape de un Tooltip abierto, enfocando un enlace con texto antes de cerrar el Drawer. Reejecución dirigida de ese recorrido:3/3 PASS en los tres motores. La pasada completa posterior se registra aparte.


[Resumen estructurado final](validation-results.json). Nueva pasada completa Vitest también150/150; no se reutiliza el resultado previo a la corrección para identificar el candidato final.

Estado final local: implementación y regresiones automáticas acreditadas; no cierre de011. Pendientes concretos: LCP<=2500ms en las cuatro rutas, lector/zoom/toque/teclado virtual, más verificación de CI del commit publicado antes de PR. Ningún merge/despliegue realizado.

## Confirmación manual de acceso — 31/10/2026

Se solicitó al titular probar en localhost:5173 el acceso con Google, cerrar sesión y entrar con Microsoft, comprobando llegada a «Mis listas» y apertura de Perfil con ambos. Respuesta del titular: **«Ambos funcionan»**.

Resultado: **PASS reportado por el usuario** para ese recorrido del candidato011; T024 y AC08 cerradas junto con50 regresiones automáticas de autenticación/sesión/perfil/aislamiento ya ejecutadas. No se recopilaron correos, OTP, contraseñas, tokens ni storageState. No se presenta como un nuevo ensayo instrumentado ni como prueba manual de todas las propiedades de autorización, cachés o drafts.

La confirmación no cierra accesibilidad manual, LCP ni CI remoto, ni modifica tareas de007/008/009/010.
