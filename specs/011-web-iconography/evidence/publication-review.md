# Revisión del CI de publicación

01/11/2026. El [run41330104483](https://github.com/frankcval/MiKompri/actions/runs/41330104483) de fc3e2ca terminó160 tests PASS y191/192 navegadores; falla WebKit en la carga diferida de Datos de compra. No se abre PR con ese resultado.

## Evidencia y corrección mínima

El trace saneado del artefacto13093885014 muestra el formulario y el input de importe presentes en DOM, pero el Dialog tiene aria-hidden=true y Tabster inactive/not-focused. No es un formulario ausente por contrato/API ni error de registro de compra. El snapshot accesible omite por eso el modal. [Resumen sin trazas/tokens](publication-review.json). El ZIP original permanece solo en.local ignorado; no se versionan URLs de descarga firmadas ni sesiones.

La secuencia de prueba establecía foco con focus() antes de garantizar que terminara la entrada/activación del modal. La causa de timing se infiere de esa secuencia y el estado observado; no se atribuye a un cambio backend. [Playwright documenta](https://playwright.dev/docs/actionability) que focus no realiza comprobaciones de interactuabilidad; click sí espera visibilidad/estabilidad/eventos/habilitación. [trial](https://playwright.dev/docs/api/class-locator#locator-click-option-trial) permite comprobarlo sin ejecutar la cancelación.

Se añade un trial click sobre Ahora no antes de focus y de liberar el import. Permanecen las aserciones de input visible, foco conservado, cancelación y retorno al checkbox. No includeHidden/force, sleeps, cambios de timeouts, retries, producción, reglas de compra ni budgets.

Resultado dirigido:3/3 en tres motores. Diagnóstico de estabilidad:15/15 (cinco ejecuciones independientes por motor, todas incluidas, sin reintentar fallos). No sustituye la suite final192 del nuevo SHA, que se verifica antes de abrir PR. Repetir el caso con Playwright en tres motores usando --grep 'purchase cancellation keeps'; la prueba sigue bajo la configuración normal de CI, workers2/retries0.

Para evitar afectar la sesión del titular, las ejecuciones locales usaron un servidor aislado5181 con configuración temporal derivada de la original; al terminar queda retirado. Se cerró únicamente el servidor QA IPv4 iniciado por el agente. La sesión Vite del titular en localhost/IPv6 permanece intacta, igual que.env/cloud/BD.

El código productivo conserva la huella cf8d87e788a3b6c75ff45cbe989b2958c1481d4bc8c2abffa04fb85deff3bcf6 de0923a0c. Se mantienen válidos build/Docker/capturas/80muestras de ese código; no se repiten métricas para un ajuste exclusivo de test/documentación. El CI del nuevo commit de publicación se registra en PR/entrega después del push.
