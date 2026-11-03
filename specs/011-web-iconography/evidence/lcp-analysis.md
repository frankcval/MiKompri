# Investigación LCP011

Base remota cf83bff, CI inicial PASS en [auditoría](head-review.json). Las cifras de diagnóstico son una observación por ruta, no p75 ni sustitución del runner. [Waterfall HEAD](lcp-head-diagnostic.json), [waterfall final](lcp-final-diagnostic.json). Recursos, timestamps, fuentes, primer render y long tasks quedan en JSON sin headers/tokens. Se espera el mismo encabezado de datos que el runner antes de capturar; no se ocultan elementos ni se cambian observers de aceptación.

| Ruta | Elemento observado | Causa probable | Atribuible a011 |
|---|---|---|---|
| home | P de descripción del hero | Entrega/evaluación de dependencias iniciales, incluidos React/MSAL/Fluent; fuentes ya disponibles antes del LCP, sin queries de negocio | PARTIAL: coste de iconos/Tooltip añadido sobre carga previa; no evidencia de que la familia sea causa única |
| lists | P de descripción Mis listas | Arranque común y waterfall de página diferida; consultas Users/listas/grupos después de carga JS | PARTIAL: fracción común añadida; waterfall/bootstrap ya existían |
| personal | H1 Compra de la semana | Página diferida, lectura de lista y render50filas; long tasks mayores que portada | PARTIAL: Tooltip por opciones suma presentación, bootstrap/imports/50filas son previos |
| shared | H1 provisional Lista compartida o texto añadido por persona, según pasada | Arranque/página/consultas de lista y miembros; la entrada LCP de un nodo eliminado puede tener element=null | PARTIAL; coordinación de queries/render es previa, adornos comunes nuevos |

Para shared, un nodo provisional puede seguir siendo el mayor aunque después se cambie su texto. Un element=null se conserva como desconocido, sin inventar selector; la observación final con elemento válido y la secuencia completa están en JSON. No se cambia el título/loading para mejorar la métrica. Los tests y las80muestras usan los mismos contenidos y condiciones.

## Hallazgos y cambios

1. El registro importaba31exports aunque nueve no tenían uso productivo. Se conservan22, sin otro pack/version. Native Checkbox/Select conservan glyphs del framework.
2. Diferir solo el Drawer protegido no bajó la unión de recursos iniciales. Se conserva el [diagnóstico de ese intento](lcp-drawer-only-diagnostic.json), sin presentarlo como mejora aceptada.
3. El mapa de módulos mostró React DOM junto a utilidades de Dialog en un chunk compartido, y una inspección posterior confirmó otro consumidor inicial real: el Drawer público de HomePage. Se difieren ambos mediante el mismo componente Fluent, con cancelación/restauración probadas y navegación pública intacta.
4. Se agrupa únicamente el cierre de dependencias estáticas iniciales; no se arrastran deliberadamente todas las pantallas diferidas. [Opción oficial de Rolldown](https://rolldown.rs/reference/TypeAlias.CodeSplittingGroup#tags). Producción y laboratorio importan vite.chunks.ts. No se modifica measure-performance.mjs, throttling, viewport, caché, fixtures50 ni muestras20/ruta.
5. Separar solo el renderer tampoco acreditó una ganancia suficiente; [diagnóstico intermedio](lcp-renderer-only-diagnostic.json). Se conserva la solución de cierre inicial compartida, no múltiples reglas de vendor por librería.

La agrupación reduce fan-out/duplicación de compresión, a cambio de un chunk inicial grande y menor granularidad de caché. Vite advierte >500KB sin comprimir; se conserva el aviso y su umbral, no se silencia. El peso gzip/transfer se evalúa en la tabla final, no mediante el nombre de un chunk. Credenciales/tokens y lógica MSAL/JWT no cambian.

## Aceptación y deuda

La clasificación final depende de80muestras nuevas del candidato cerrado y de CLS/interacciones, no de los mejores tiempos de los diagnósticos. Se conservan baseline, primer011, intento anterior al Drawer público y final. Cualquier FAIL global se informa como tal. Solo puede no bloquear una deuda previa no empeorada materialmente, con payload justificado e interacciones/CLS dentro de budget, según autorización expresa del titular.

Se descartan cambios en inicialización/autenticación MSAL, contratos, virtualización de compra, sustitución de UI kit o SSR para perseguir el budget en esta revisión de presentación. Requieren evaluación y alcance adicionales; no se presume que la deuda sea imposible de corregir.

## Resultado del cierre

[80muestras finales y clasificación](performance-comparison.md):2392/2660/3096/2964ms. Portada PASS; tres FAIL LCP globales clasificados como deuda previa autorizada no bloqueante para011, con mitigación de la porción011, sin empeorar baseline y CLS/interacciones PASS. Diagnósticos son observaciones separadas, no muestras p75 ni sustitutos del runner.
