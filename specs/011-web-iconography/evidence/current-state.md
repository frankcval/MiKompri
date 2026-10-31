# Baseline e investigación011 — 31/10/2026

**Base:** main/origin main9597af5, limpio y0/0 antes de crear rama. Merge commit de PR59 visible; a1cf0a0 de010-A es ancestro de origin/main (exit0). Rama011-web-iconography creada desde esa base, sin upstream main; Spec Kit create-new-feature con Number11/AllowExistingBranch conserva scope existente y guarda feature.json011. setup-plan y setup-tasks resuelven011. No AGENTS.md encontrado en inventario del repo.

## Inventario de implementación actual

| Superficie | Estado real inspeccionado | Cambio propuesto |
|---|---|---|
| package.json | Fluent9.68/React18.3.1/TS6.0.3/Vite8.3.4; React Icons no instalado | Una dependencia exacta futura5.7.0, familia pi |
| shared/ui/theme.ts | Light theme teal#006B68/MiKompri Sans, fondo blanco/frío, radios8 | Mantener marca/fuente y extender tokens Nevera/Papel/Pizarra |
| app/layout/MainLayout.tsx/CSS | Tres destinos y barra inferior/Drawer; menu☰ y cerrar✕; marca✓ | Iconos app semánticos y labels, conservar marca/cuentas/rutas/estructura |
| shared/ui/AccountMenu.tsx | Link Mi cuenta, no colección de métodos | Etiqueta visible e icono opcional, sin cambiar logout/perfil |
| ListsPage/ListOverview | Tarjetas blancas, filtros, view/sort en query, estimación parcial/unknown/history009 | Presentación de notas/pizarra, mismos datos/estados |
| ShoppingListView/AppSurface | Checkbox/progreso/filas/acciones, styles compartidos | Hoja de trabajo legible y targets; componentes/compra actuales |
| Catálogo/mercados/grupos/perfil | Componentes y contratos ya existen | Adaptar tokens/iconos, no nuevas acciones ni endpoints |
| Tests | Vitest, Playwright smoke de tres motores, axe y auth/regresiones existentes | Añadir casos de wrappers/nav/tooltip/visual responsive, no sustituir gates |
| CI Web | Push lista ramas hasta010 y PR a main/develop | En implementación habilitar011 en push sin omitir checks |

007/008/009 conservan gates manuales/performance propios;010-A beta integrado y010-B diferido. No se marcan sus tareas por investigar011.12/invitaciones permanece futura independiente.

## Evidencia ejecutada de diseño/research

- npm view oficial devuelve react-icons5.7.0/MIT. Tarball5.7.0 leído sin instalar, SRIsha512 coincide,31símbolos presentes en index.d.ts/módulo y0ausentes;9072exports del pack auditado no implican importarlos. Manifest icon-audit.json sin credenciales.
- Fuentes primarias actuales en research.md: React Icons/Phosphor/Fluent/W3C. No recomendaciones de packs basadas en nombres recordados ni tendencias presentadas como estudio de mercado.
-11pares de color calculados, todos sobre umbral del uso indicado; contrast.json no equivale a una auditoría de DOM.
- Tres SVG→PNG renderizados con Chromium headless instalado y revisados visualmente. Se ajustan metadatos/navegación inferior a14px y se mantiene Mi cuenta textual en cabecera; no se recortan nombres para simular densidad. Referencias no son pantallas implementadas ni pruebas de interacción.
- Primer comando de render falló por un nivel relativo de ruta en helper privado; corregido el helper y los tres renders terminan exit0. No instalar módulos para resolverlo ni tocar Web. Artefactos y fuentesSVG finales bajo design/.

## Estado de esta fase

Solo documentación/.specify y referencias gráficas estáticas; ningún cambio productivo React/CSS/package/lock/auth/env/API/modeloEF. Helpers/tarball/módulo leído y renders temporales bajo.local ignorado, nunca parte del bundle. Configuración QA010-A funcional conservada; no restauración automática ni llamada de migración/cloud.

La verificación final de enlaces/IDs/JSON/SVG/alcance y Spec Kit se añade al cerrar la formalización. No se ejecutan suites de producto para declarar una implementación que todavía no existe. Todas las32 tareas quedan abiertas.

## Validación final de formalización

- Spec Kit RequireTasks/IncludeTasks y PathsOnly exit0, FEATURE_DIR011; PathsOnly devuelve BRANCH vacío en el script existente, git branch --show-current confirma011 independientemente. feature.json conserva formato de un campo relativo y se normalizan separadores a `/` para portabilidad del repositorio.
-16Markdown comprobados,106enlaces locales sin rotos,22FR/seisSC/cinco historias con IDs únicos y32tareas pendientes consecutivas. Mapa exactamente igual a31exports verificados, sin nombres inventados.
- JSON válidos, tres SVG parseados sin script/foreignObject/event handlers; tres PNG finales renderizados e inspeccionados.11pares de contraste pasan su mínimo previsto; no auditoría de accesibilidad/producto completada.
- git diff --check exit0 y diff contra main sin src/package/lock/constitución ni specs007–010 modificados. No instalación ni test productivo necesario para artefactos de diseño; aceptación futura permanece abierta.
- Helpers/local/npm metadata y módulo del paquete se mantienen ignorados. Solo referencias estáticas y documentación son versionables; el código de la aplicación/identidad/BD/configuración QA permanece.
