# Revisión de documentación y roadmap — 30/10/2026

## Base y alcance

Revisión documental desde `main`/`origin/main` `8522f58`, merge del [PR #57](https://github.com/frankcval/MiKompri/pull/57). Rama nueva `docs/update-web-roadmap`. El PR está integrado; esto no equivale al cierre global de la aceptación de 009. Se revisaron README raíz/Web, constitución TP6/TP11, roadmap baseline, tareas/evidencias 007/008/009, scopes 010/011, scripts Web, configuración de rutas, Docker y workflows.

El objetivo es reflejar el estado implementado y la prioridad Web antes de MAUI. No hay código, cambios de permisos/identidad, nuevos requisitos funcionales ni cambios de completitud en tasks. La constitución y los criterios de rendimiento/aceptación permanecen vigentes.

## Análisis de discrepancias

| Hallazgo | Evidencia contrastada | Corrección documental |
| --- | --- | --- |
| README y baseline presentan 007 como draft/siguiente implementación | Cliente `MiKompri.Web`, tareas 007/008/009 y PRs #56/#57 integrados | MVP-4 implementado por 007/008/009, con aceptación global abierta; retirar el paso de volver a iniciar 007 |
| Web no figura en tecnologías, estructura, requisitos, arranque ni CI del README principal | package.json, router, Dockerfile, workflows Web/Resource | Añadir stack, Node/npm, configuración pública, ejecución Vite, pruebas y separación de backend Compose/SPA |
| Se mezclan números de spec con MVP y entregas con aceptación | Baseline, tareas 007/008 y 009/T009–T011 | Tabla MVP↔specs actualizada; T009 acreditada, T010/T011 abiertas; 010/011 solo alcance futuro |
| MAUI se describe como spec existente aunque está aplazado | No existe `specs/006-android-maui-client` ni proyecto MAUI en la solución | Identificador 006 reservado, sin artefactos/implementación, aplazado sin fecha; MAUI sigue previsto para Android |
| Guía de reutilización llama read-only a ProductCatalog | API y contratos existentes; Web 009 conecta edición, mercados y registro/historial de precios | Documentar capacidades actuales y contratos de compra/idempotencia/fechas sin cambiar política de catálogo global |
| Guía CORS omite contexto de fecha y no distingue orígenes por defecto | Tres Program.cs con Cors:AllowedOrigins, WithOrigins y AllowAnyHeader | Añadir localhost/127.0.0.1 y header X-Client-Utc-Offset-Minutes; TLS/cloud requieren configuración explícita |
| README Web muestra solo el esqueleto inicial y considera el workflow Entra aún sin integrar | Árbol src/tests, configuración integrada y workflow en main | Estructura real, arranque/configuración y estado actual del workflow, sin atribuirle aceptación humana |
| El análisis UX del 28 puede leerse como diagnóstico vigente | Implementaciones posteriores 008/009 | Conservarlo como documento histórico y enlazar estado y evidencia actuales |

También se corrige en el README la ruta reversible `purchase-state` y el ejemplo de audience para usar el recurso OAuth backend común de TP11. No se modifica ninguna API ni registro Entra.

## Estado que debe comunicar la documentación

- MVP-0–3: capacidades backend entregadas; no se reinicia ni se recertifica su aceptación en esta revisión.
- MVP-4: Web implementada e integrada mediante 007/008/009. 009/T009 cerrada con evidencia técnica; T010/T011 y gates globales 007/008 conservan sus pendientes.
- La última serie de entrega registrada mantiene LCP personal/compartida 2556/2604ms frente a 2500ms; aceptación Entra multiusuario y lector/zoom/teclado/dispositivo requieren sus evidencias propias. No se cambian límites ni se sustituyen por CI controlada.
- MVP-5: Android/MAUI aplazado para después de estabilizar experiencia y contratos Web. No se elimina TP6 ni se introduce otro framework móvil.
- 010/011: análisis de alcance pendiente de formalización/aprobación antes de implementar. Presupuesto, OCR/fotos, promociones, comunidad y chat no entran por defecto en esas iniciativas.
- Azure/SWA: destino previsto; no se declara un despliegue productivo por existir configuración o una imagen Docker.

La fuente actual para quien entra al repositorio es el [README](../README.md#-estado-actual-del-proyecto); para ejecución Web, su [README específico](../MiKompri.Web/README.md). Los gates y pruebas pertenecen a [tasks 009](../specs/009-shopping-experience-completion/tasks.md) y [evidencia técnica](../specs/009-shopping-experience-completion/evidence/review-corrections.md), no a una nueva casilla de este documento. El roadmap baseline conserva trazabilidad y señala qué diagnóstico es histórico.

## Validación de esta actualización

Revisión de enlaces locales y anchors Markdown, rutas/scripts citados contra el repositorio y `git diff --check`. Comprobación del diff limitado a documentación, sin cambios en constitución, feature activa, tasks, contratos ejecutables, código ni workflows. No se repiten suites funcionales, builds ni mediciones por cambios de texto; las cifras anteriores se atribuyen a su revisión original, no a una ejecución nueva.

Resultado: siete archivos Markdown revisados, 90 enlaces locales (incluidos anchors) comprobados, cero errores; `git diff --check` correcto. Los scripts npm citados se contrastan con package.json y la separación de suites con ambas configuraciones Playwright.
