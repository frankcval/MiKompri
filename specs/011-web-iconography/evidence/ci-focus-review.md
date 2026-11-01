# Corrección del foco y revisión de gates del PR60

01/11/2026, sobre `0e88e9afb472a051499100a3940bc423d0da993f`. Petición posterior del titular: corregir los fallos encontrados, sin añadir funciones/diseño ni hacer merge.

## Fallo observado

[CI Web del PR](https://github.com/frankcval/MiKompri/actions/runs/41351251813):162tests PASS;197/198E2E. En WebKit, `iconography.spec.ts:85` espera foco en «Opciones de Tomates» tras Escape y obtiene inactive. La traza del artefacto13101578207 conserva el `<details open>`; no es un error de login, datos ni infraestructura caída. El ZIP permanece en.local ignorado, sin versionar URLs firmadas.

La secuencia de prueba avanzaba al contenido siguiente tan pronto desaparecía el diálogo. `focus()` no espera visibilidad/estabilidad/eventos como [click/trial](https://playwright.dev/docs/actionability); Fluent puede estar tomando/restaurando foco durante la transición. Localmente también aparecen fallos anteriores en la misma secuencia (entrada al Drawer y toma de foco del enlace); no se presentan como reproducciones exactas del fallo CI posterior.

Además, ambos menús (público/autenticado) tenían un `requestAnimationFrame(() => trigger.focus())` incondicional: si el usuario elegía otro control antes de ejecutarse, esa devolución podía deshacer la elección. Una unidad que retiene el frame, retira el control del overlay y enfoca el siguiente control reproduce determinísticamente ese robo de foco con el callback anterior. Esto acredita la carrera del callback; no demuestra por sí solo cuál de los restauradores de Fluent intervino en aquella traza remota.

## Corrección acotada

- Compartir `restoreFocusAfterClose`: devolver al disparador si el foco se perdió al cerrar o sigue en el control anterior; respetar un nuevo destino explícito. Comprobar que el disparador siga montado. Usarlo en los dos menús que tenían el mismo callback.
- Antes de las acciones de teclado del test, esperar interactuabilidad con `click({trial:true})` en el disparador, el enlace del Drawer y el summary de opciones. No ejecuta la acción ni reemplaza teclado por clicks. Afirmar foco antes de Enter; conservar Escape, cierre, foco posterior, confirmación y cero DELETE al cancelar.
- Sin sleeps, force, includeHidden, retries, aumento de timeouts, nuevas funciones, diseño, cambios de login/JWT/BD/cloud ni presupuestos.

## Pruebas durante el diagnóstico

| Ejecución | Resultado |
| --- | --- |
| Caso original, WebKit5repeticiones | 3PASS/2FAIL, ambos en entrada del Drawer; diagnóstico, no PASS |
| Instrumentación temporal5repeticiones | 3PASS/2FAIL de entrada; siguiente serie15/15PASS antes de modificar código: intermitencia conservada, no prueba de corrección |
| Unidad con callback incondicional anterior | 1PASS/1FAIL: falla respeto al nuevo foco |
| Unidades corregidas + navegación + portada | 27/27PASS |
| Solo protección del callback, E2E5por motor | 13PASS/2FAIL en otras fases de toma de foco; insuficiente |
| Protección y espera de interactuabilidad, E2E5por motor | 15/15PASS,51.9s; todas las repeticiones cuentan,0retries |

Suite E2E completa posterior: **198/198PASS**,10.2min,2workers,0retries,66escenarios por motor. Comando desde `MiKompri.Web`, con motores ya instalados en `PLAYWRIGHT_BROWSERS_PATH=E:/Projects/MiKompri/.playwright-browsers` y `PLAYWRIGHT_ALL_BROWSERS=1`: `npx playwright test --config ../specs/011-web-iconography/.local/playwright-isolated.config.mts --grep '@smoke' --workers=2 --output=test-results/011-pr60-fixed`. El config temporal deriva del original, cambiando solo servidor/URL a5181 y salida; no afecta Vite del titular en5173.

`npm test -- --maxWorkers=2`:163PASS/1FAIL,49archivos,242.60s. Falló el timeout global5000ms del primer recorrido de `CatalogCreation.spec.tsx`, sin cambios en ese área. Esta pasada coincidió con ejecución de navegadores y no se declara PASS. Se repite el archivo afectado de forma aislada, sin ampliar timeout ni cambiar sus aserciones; el CI final debe acreditar de nuevo la suite completa del SHA publicado.

La primera pasada aislada del archivo mantiene7PASS/1timeout (5112ms), por lo que no se atribuye exclusivamente a la concurrencia. Un diagnóstico temporal del caso con marcas de tiempo pasa1/1 (otros7no ejecutados) bajo el mismo timeout. Tras retirar la instrumentación, la confirmación del **archivo original sin modificaciones pasa8/8**,19.37s, mismo comando y timeout. No se altera Catálogo ni se añade retry/timeout para obtener verde. Esto acredita los casos afectados, pero no convierte la pasada completa anterior en164/164; la suite completa debe pasar en el CI final.

Lint/typecheck/build y `git diff --check`PASS. El aviso de chunk inicial>500kB permanece. No se repiten las métricas de rendimiento históricas como si pertenecieran a este código; el cambio se limita a callbacks de cierre y sincronización del test, sin nuevo diseño/imports de iconos/contratos. Los resultados originales permanecen explícitos, sin reescribirlos como PASS.

## SonarCloud

Check adicional del mismo SHA: Quality Gate FAIL por cobertura nueva0% (mínimo80%) y seguridadB (mínimoA). No se reduce el umbral ni se excluye código para obtener verde. Las anotaciones accesibles mediante GitHub muestran advertencias de mantenibilidad/accesibilidad y scripts, pero no permiten identificar con certeza el hallazgo concreto que determina B.

El proyecto no es accesible sin sesión en SonarCloud: API pública devuelve «Project doesn't exist» y búsqueda vacía; no se interpretan como ausencia de problemas. No hay SONAR_TOKEN local configurado. Se solicita al titular iniciar sesión para leer el detalle privado, sin pedir tokens ni crear credenciales.

La configuración existente de Web no ejecuta cobertura ni genera LCOV; los scanners .NET de ShoppingList/ProductCatalog apuntan al mismo proyecto Sonar y solo importan OpenCover. Es una carencia de integración comprobada, no evidencia de que los tests no cubran código. [Sonar documenta](https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/test-coverage/javascript-typescript-test-coverage/) la generación previa e importación LCOV. Resolver correctamente el gate requiere revisar el análisis efectivo y el hallazgo B; no se introducen pipelines, exclusiones o remediaciones de seguridad a ciegas.

SonarCloud permanece pendiente de diagnóstico completo/acceso al informe. No afirmar READY FOR MERGE ni cerrar el gate mientras siga fallando. Tampoco implica cerrar QA humana o deuda de rendimiento previas.
