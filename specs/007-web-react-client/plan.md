# Implementation Plan: 007-web-react-client

**Continuidad — 2026-10-28**: el [plan 008](../008-web-ux-modernization/plan.md) organiza la evolución P0–P3 sobre esta base. [Trazabilidad](../008-web-ux-modernization/traceability.md) asigna deuda pendiente y ampliaciones a tareas 008 sin duplicar implementación. Los cierres de esta feature exigen evidencia propia; el nuevo plan no altera retrospectivamente resultados ni autoriza despliegues.

**Branch**: `007-web-react-client` | **Date**: 2026-10-23 | **Spec**: [`spec.md`](./spec.md)

**Input**: Feature specification from `/specs/007-web-react-client/spec.md`

## Summary

**Diseño vigente (2026-10-27)**: [design.md](design.md) fija tokens, medidas, rutas y alcance entregado. La revisión del 2026-10-24 se conserva como histórico y backlog; sus medidas no prevalecen sobre esta referencia. Estado de verificación: T120–T126 en [tasks.md](tasks.md), reproducible en [quickstart.md](quickstart.md).

Construir el plan técnico de un cliente Web SPA de MiKompri como MVP-4 (siguiente iniciativa activa), priorizando validación full-stack de identidad, autorización y contratos de los bounded contexts `Users`, `ShoppingList` y `ProductCatalog` sin acoplarse a decisiones específicas de MAUI. La implementación se organizará por features con React + TypeScript strict + Vite + Fluent UI v9, autenticación MSAL (Authorization Code + PKCE), estado servidor con TanStack Query y estrategia de despliegue futura en Azure Static Web Apps.

## Technical Context

**Language/Version**: TypeScript (strict) + React 18 (SPA)

**Primary Dependencies**: React, Vite, Fluent UI v9, React Router, MSAL React/MSAL Browser, TanStack Query, React Hook Form, Zod

**Storage**: N/A (cliente web stateless; persistencia de negocio en APIs backend)

**Testing**: Vitest, React Testing Library, MSW, Playwright

**Target Platform**: Navegadores modernos en desktop/tablet/mobile

**Project Type**: Web SPA frontend en monorepo

**Performance Goals**: Primera navegación autenticada <2 min para usuarios piloto; UX fluida en flujos P1 con caché y refetch controlado

**Constraints**:
- Cumplimiento estricto de TP11 (Entra ID, `(tid, oid)`, `UserId` interno, audience común)
- Sin client secret en frontend
- Sin SSR/Next.js
- Sin Redux salvo necesidad demostrada
- Separación estricta frontend vs bounded contexts backend

**Scale/Scope**:
- Flujos P1: auth/sesión, listas personales/compartidas
- Flujos P2: reparto de gastos y catálogo con consulta y alta de productos (ampliación 2026-10-25)
- Flujos transversales: responsive, accesibilidad básica, estados HTTP, CI frontend

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **TP8 (tests obligatorios)**: PASS. Estrategia definida con unit/component (Vitest/RTL), integración de red (MSW) y E2E (Playwright) para flujos críticos.
- **TP11 (identidad/auth transversal)**: PASS. Se mantiene Entra ID, MSAL + PKCE, `(tid, oid)` y audience backend común como decisiones cerradas; `UserId` interno como identidad de dominio.
- **Bounded-context boundaries**: PASS. El frontend consume contratos HTTP públicos; no hay acceso directo a BDs ni lógica interna de `Users`.

**Resultado gate pre-Phase 0**: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/007-web-react-client/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── frontend-backend-integration.md
└── tasks.md
```

### Source Code (repository root)

```text
MiKompri.Web/
├── src/
│   ├── app/
│   │   ├── providers/
│   │   ├── router/
│   │   └── layout/
│   ├── auth/
│   │   ├── msal/
│   │   ├── guards/
│   │   └── session/
│   ├── api/
│   │   ├── client/
│   │   ├── interceptors/
│   │   └── errors/
│   ├── features/
│   │   ├── profile/
│   │   ├── groups/
│   │   ├── shopping-lists/
│   │   ├── shared-lists/
│   │   ├── settlements/
│   │   └── product-catalog/
│   └── shared/
│       ├── ui/
│       ├── forms/
│       ├── hooks/
│       ├── config/
│       └── accessibility/
├── tests/
│   ├── unit/
│   ├── component/
│   ├── integration/
│   └── e2e/
└── .env.*
```

**Structure Decision**:
Se adopta arquitectura por features para minimizar acoplamiento entre dominios UI, facilitar escalado por módulos funcionales y mantener trazabilidad con bounded contexts backend (`Users`, `ShoppingList`, `ProductCatalog`). Se evita una estructura global tipo Controllers/Services/Models y se preserva reutilización conceptual futura para MAUI/iOS.

## Phase 0: Research

1. Confirmar estrategia de autenticación SPA con MSAL React/MSAL Browser + Authorization Code + PKCE y session restore segura.
2. Definir patrón de API client común con propagación Bearer token, manejo de expiración y estrategia de retries para 503.
3. Definir patrón de TanStack Query (query keys por feature, invalidación, staleTime, refetchOnWindowFocus).
4. Definir patrón de formularios con React Hook Form + Zod (errores de validación 400/409 y mensajes UX).
5. Definir arquitectura de errores HTTP centralizados (400/401/403/404/409/503) y estados de pantalla.
6. Definir estrategia de configuración por ambientes (`.env.local`, `.env.test`, `.env.production`) sin secretos en bundle.
7. Definir guía responsive (mobile-first) y criterios de accesibilidad verificables.
8. Definir estrategia CI frontend y despliegue futuro SWA con separación de responsabilidades backend.

## Phase 1: Design & Contracts

### Diseño técnico

- **Auth + Protected Routes**: guardas de ruta y bootstrap de sesión con MSAL; fallback para 401/403.
- **API client común**: capa única para HTTP con inyección de token, correlación de errores y normalización de respuestas.
- **Server state**: TanStack Query como fuente de verdad para datos remotos; sin Redux inicial.
- **Formularios**: React Hook Form + Zod por feature; mapeo de errores backend a errores de campo/globales.
- **UI/UX transversal**: Fluent UI theme central, componentes compartidos de loading/empty/error/unauthorized, responsive y accesibilidad.
- **Separación BCs**: módulos de features mapeados a contratos de `Users`, `ShoppingList`, `ProductCatalog` sin mezclar reglas internas backend.

### Cambios backend prerequisite (bloqueantes)

1. `Users`: migración de correlación de identidad `sub -> (tid, oid)` + soporte transicional `ExternalUserId` nullable.
2. `ShoppingList`: autorización efectiva de listas personales por identidad autenticada; `OwnerId` derivado del usuario autenticado.
3. `ProductCatalog`: autenticación JWT real alineada a audience común y validaciones TP11.
4. Plataforma: configuración CORS para origen SPA local/cloud y headers Authorization.

### CI/CD frontend

- Workflow dedicado de frontend: install, lint/typecheck, unit/component/integration tests, build, `docker build` (cumplimiento TP4) y publish artifacts.
- E2E Playwright en stage de validación (smoke en PR y suite completa en main/release).

### Estrategia Azure Static Web Apps (futuro)

- Objetivo preferente: Azure Static Web Apps si se mantiene compatibilidad técnica final.
- Despliegue recomendado: `azd` (IaC + reproducibilidad), no obligatorio para MVP local.
- Configuración prevista: `navigationFallback` SPA, variables por ambiente, redirect URIs local/cloud y cero secretos en frontend.

### Re-check Constitución (post-design)

- TP8: PASS (matriz de pruebas definida por niveles).
- TP11: PASS (decisiones de identidad cerradas y no reabiertas).
- Boundaries BC: PASS (contratos HTTP, sin acceso directo a datos entre contextos).

**Resultado gate post-Phase 1**: PASS.

## Complexity Tracking

Sin violaciones constitucionales previstas.

## Histórico: revisión visual y de cierre — 2026-10-24

**Estado a esa fecha**: propuesta documentada; implementación y validación pendientes. Las medidas y diagnósticos siguientes son históricos; para el estado actual consultar [design.md](design.md) y T120–T126. Los requisitos funcionales todavía no entregados continúan en T107–T118. Los PASS anteriores corresponden al diseño técnico original y no certifican la entrega completa. Se mantienen PP5, TP8, TP10 y TP11.

### Dirección visual: la compra en común

MiKompri sirve a familias, parejas y grupos pequeños que consultan y marcan compras mientras se desplazan. La pieza protagonista será una lista de compra legible, con pendientes, progreso y autoría cuando el contrato la exponga. El primer contenido autenticado será el acceso a las listas; perfil seguirá disponible en navegación.

Se aplica `.agents/skills/frontend-design/SKILL.md`: tokens, composición y revisión contra el uso real antes de escribir código. Se aplica `.agents/skills/web-design-guidelines/SKILL.md` a navegación, formularios y verificación. Reglas consultadas el 2026-10-24: https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md. En conflictos prevalecen spec, constitución y Fluent UI; la interfaz española usa sentence case.

| Token base | Valor | Uso |
| --- | --- | --- |
| Fondo | `#F5F7FA` | Espacio exterior y separación de zonas |
| Superficie | `#FFFFFF` | Lista de compra y formularios |
| Texto | `#172B3A` | Nombres, títulos e importes |
| Texto secundario | `#526575` | Contexto del grupo y metadatos |
| Marca | `#006B68` | Acción principal, selección y progreso |
| Separador | `#D8E1E8` | Filas y límites de superficies |

Estos seis colores definen la dirección, no acreditan contraste. Crear escala BrandVariants y mapear roles mediante el tema Fluent; errores, advertencias y estados de éxito usan tokens semánticos Fluent. El separador decorativo no sustituye el contraste requerido en controles. Medir todas las combinaciones finales: texto normal >=4.5:1, texto grande y elementos gráficos/controles esenciales >=3:1.

- **Tipografía**: Source Sans 3, por su claridad en nombres de productos, cantidades e importes; una familia para toda la app, fallback Segoe UI/system-ui. Antes de incorporar la fuente, comprobar licencia y servir WOFF2 local con `font-display: swap`; no depender de CDN. Escala 14/16/20/24/32 px, pesos 400/600/700; cuerpo e inputs 16 px, interlineado 1.5, títulos 1.2. Título móvil 24 px, escritorio 32 px. Importes con cifras tabulares e `Intl.NumberFormat`; fechas con `Intl.DateTimeFormat`. Texto alineado a la izquierda, prosa <=75 caracteres por línea.
- **Espaciado**: base 4 px; tokens 4/8/12/16/24/32/48. Márgenes móviles 16 px, tablet 24 px, escritorio 32 px. Filas con mínimo 56 px y acciones táctiles de al menos 44x44 px. Formularios de una columna en móvil; cantidades/precios pueden compartir fila solo si caben sus etiquetas y errores.
- **Superficies**: una superficie continua para ítems con divisores; tarjetas reservadas a resúmenes de listas. Radio 8 px en superficies, 4 px en controles cuando sea compatible con Fluent. Sombras solo para capas superpuestas. Iconos del ecosistema Fluent, con texto o nombre accesible. El producto se reconoce por la lista y su progreso, sin ilustraciones decorativas necesarias.
- **Movimiento**: respuesta breve al guardar/marcar; respetar `prefers-reduced-motion`. Mantener estable el foco y la posición del ítem tras marcarlo; no desplazarlo automáticamente fuera de pantalla.

**Autocrítica previa a construir**: se descarta una portada de métricas y una cuadrícula de tarjetas para cada ítem por su poca utilidad durante la compra. Se elige una lista continua, acción de compra prominente y contexto del grupo junto al título. El verde azulado se usa de forma contenida; la diferenciación principal procede del flujo de compra y de la jerarquía del contenido. Revisar la propuesta mediante capturas con nombres largos, listas vacías y compra avanzada antes de extenderla a todas las vistas.

### Layout y navegación compartidos (FR-014, FR-017, FR-018)

- Un layout padre protegido con `Outlet` envuelve listas, detalle, grupos, catálogo y perfil; login/callback son públicos. Un solo landmark `main`, un `h1` de pantalla y enlace «Saltar al contenido». La marca no es otro `h1`.
- Escritorio >=1024 px: navegación lateral de 208 px; contenido flexible hasta 1280 px. Enlaces «Listas personales», «Listas compartidas», «Grupos», «Catálogo» y «Perfil»; selección con texto/peso, indicador y `aria-current`, no solo color. Perfil y cierre de sesión accesibles desde todas las pantallas protegidas.
- Móvil <768 px: cabecera compacta «MiKompri» + botón «Menú» que abre Drawer Fluent con los mismos cinco destinos. Al abrir, foco dentro; Escape cierra y devuelve foco al disparador. Tablet 768–1023 px conserva menú compacto y una sola columna. No introducir barra fija que tape los ítems o el teclado.
- `/` y el acceso tras login conducen al índice de listas personales; conservar `/personal-lists`, `/shared-lists`, `/groups`, `/product-catalog` y `/profile`. Nuevos detalles `/personal-lists/:listId` y `/shared-lists/:listId`; grupo en query `groupId` para restaurar contexto. Recarga, Atrás/Adelante y URL directa conservan selección; nunca elegir silenciosamente la primera lista para escribir.

### Pantallas del núcleo (US2)

En el índice personal, título «Mis listas», acción «Crear lista» y resúmenes enlazados con nombre y progreso si el contrato lo proporciona. En compartidas, título «Listas compartidas», selector de grupos propios por nombre y acción «Crear lista». El grupo se obtiene de Users, no de un GUID escrito por la persona. Sin grupos, ofrecer «Crear grupo»; sin listas, explicar y ofrecer crear la primera. La creación abre Dialog Fluent con nombre y validación; al guardar abre el detalle del identificador devuelto e invalida la consulta correcta.

En móvil, seleccionar un resumen navega al detalle y «Volver a mis listas» recupera contexto. En escritorio, columna de resúmenes de 280 px junto al detalle flexible (mínimo 440 px); si el espacio real no alcanza se apilan. El índice sin selección muestra una invitación explícita a elegir lista. No precargar detalles de todas las listas para completar sus resúmenes.

```text
Móvil: índice                    Móvil: detalle
MiKompri                 Menú    MiKompri                 Menú
Mis listas       Crear lista     Volver a mis listas
Compra semanal      3 de 8       Compra semanal       Acciones
Compras del sábado  0 de 4       Personal / nombre del grupo
                                 3 de 8 comprados
                                 Agregar producto
                                 Pendientes (5)
                                 [Marcar] Leche   2 ud   Más
                                 Comprados (3)

Escritorio
Navegación | Mis listas  Crear lista | Compra semanal   Acciones
           | Compra semanal         | Grupo Casa · 3 de 8
           | Compras del sábado     | Agregar producto
           |                        | Pendientes / Comprados
           |                        | Filas de ítems y acciones
```

El detalle contiene título, contexto personal/grupo, progreso textual y visual, acción «Agregar producto» y secciones «Pendientes»/«Comprados». Cada fila muestra nombre, cantidad, precio estimado identificado como tal y autor si está disponible. Nombre largo puede ocupar varias líneas. «Marcar comprado» es una acción explícita de una dirección: no prometer desmarcar sin contrato. La fila comprada mantiene nombre legible y estado textual; color o tachado solos no bastan.

Acciones personales: renombrar/eliminar lista; agregar, editar, eliminar y marcar ítems. Menú «Más acciones para [producto]» para acciones secundarias; eliminar exige confirmación, indicando el nombre. Edición/alta con RHF + Zod, errores junto al campo, conservación de valores ante fallo y prevención de envíos duplicados mientras están pendientes.

En compartidas, mostrar las acciones autorizadas por los contratos y rol/membresía vigentes: consultar detalle, agregar ítems, actualizar/cerrar lista y operar ítems solo tras verificar autorización de los endpoints genéricos. No inventar endpoints ni permisos. «Registrar gasto» parte de un ítem real y permite seleccionar pagador/participantes por nombre desde membresías; internamente usa UserId. «Reparto de gastos» muestra balances y propuesta backend, sin recalcular reglas en React. No usar IDs de ejemplo. Si falta contrato o autorización, registrar el bloqueo concreto; no simular éxito ni cerrar FR-011.

### Contraste con implementación y contratos

| Requisito | Evidencia actual | Trabajo pendiente |
| --- | --- | --- |
| FR-014 navegación | `src/app/App.tsx` usa MainLayout solo en `/`; páginas hermanas en `src/app/router/routes.tsx` | Layout padre, navegación activa y acceso directo a detalles |
| FR-010 listas personales | `PersonalListsPage.tsx` crea/lista; selecciona `data[0]`; `ListItemsPanel.tsx` solo agrega | Selección, detalle, renombrado/borrado, lectura y CRUD de ítems, RHF+Zod |
| FR-010 contratos | `shoppingListsApi.ts` consulta `/purchaselists/mine`; el controlador revisado expone GET raíz con filtros, no `/mine` | Verificar contrato HTTP real y autorización con UserId interno antes de conectar el índice; documentar discrepancias en contracts, sin permitir listar datos ajenos |
| FR-011 compartidas | `SharedListsPage.tsx` pide GroupId, lista/crea y abre settlement; cliente tiene alta de ítems sin UI | Selector de grupos, invalidación tras crear, detalle/ítems y comprobación de permisos por operación |
| FR-009 grupos | `GroupsPage.tsx` crea/lista; API cliente tiene consulta de miembros | Vista de membresías; invitación solo con contrato público conforme FR-009 |
| FR-012 gastos | `SettlementPage.tsx` tiene IDs de ejemplo y recibe lista pero no ítem desde compartidas; `ExpenseForm.tsx` pide UserIds | Selección de ítem real, pagador/participantes por nombre, RHF+Zod y errores accionables |
| FR-015/016 estados | Estados compartidos existen; integración parcial y perfil comunica indisponibilidad genérica | Cubrir todas las vistas principales, reintento seguro, 401 reauth, 403 sin datos, 404 regreso, 409 conservar formulario/refrescar, 503 reintento |
| FR-017/018 y SC-005/006/007 | E2E omitidos; accesibilidad smoke solo comprueba landmarks | Navegador real, responsive, teclado, foco, nombres, contraste y evidencia por escenario |

Referencias de contratos: `MiKompri.ShoppingList.Api/Controllers/PurchaseListsController.cs` expone detalle, actualización, cierre y gastos de compartidas; CRUD y marcado en `/purchaselists/{id}`. La existencia de una ruta no acredita autorización correcta: verificar pruebas antes de reutilizarla para compartidas. Campos de progreso e ítems existen en `MiKompri.ShoppingList.Application/DTOs/PurchaseListDTO.cs`; comprobar payload real y trazabilidad antes de mostrarlos. No ampliar contratos backend silenciosamente.

### Implementación posterior en cuatro pasos

1. **Layout y navegación**: T106–T107 y reapertura T086. Padre protegido, destinos persistentes, landing de listas y rutas de detalle. Salida: navegar por enlaces y URL directa sin perder menú; un main/h1; teclado y foco comprobados.
2. **Diseño del núcleo**: T108–T110 y T088. Tema/tipografía/espaciado, índice y detalle con fixtures representativos, composición móvil antes de escritorio. Salida: revisar capturas a 390 px antes de 768/1280 px, incluidos vacío, nombres largos, pendientes/comprados y selección por URL. Fixtures solo en pruebas, nunca datos ficticios de producción.
3. **Flujos pendientes**: T111–T114 y tareas originales reabiertas. Auditar contratos primero; conectar CRUD, grupos y gastos a identidad real; errores y formularios homogéneos. Salida: pruebas de comportamiento por operación y autorización, sin elección implícita ni IDs de ejemplo.
4. **Verificación**: T115–T118 y E2E reabiertos. Salida: evidencia de navegador, responsive, accesibilidad y regresión/CI. Un paso no se cierra por existencia de archivos ni por pruebas omitidas.

### Criterios de aceptación y evidencia

- Validar primero a 390x844 (PP5), después 320x740, 430x932, 768x1024 y 1280x800. Sin scroll horizontal de página, texto/acciones cortados ni superposición; teclado virtual y zoom 200% no impiden guardar/cancelar. Verificar bordes 767/768 y 1023/1024 px. Capturas son evidencia visual, no sustituyen interacción.
- Recorrido por teclado: menú, selección, crear, agregar, editar, marcar y confirmar/cancelar borrado; sin trampas, foco visible no tapado y retorno al disparador tras Dialog/Drawer. Al borrar el elemento enfocado, mover foco a destino lógico. Anunciar resultados/errores asíncronos con región viva apropiada. Lector de pantalla: nombres y cambios de estado comprensibles.
- Estados loading/empty/error/401/403/503 en cada vista principal; 400/404/409 en operaciones aplicables. Conservar contexto y valores, no revelar datos ajenos ni repetir escrituras automáticamente tras reauth.
- Fixtures MSW deterministas para dos listas del mismo usuario, otra identidad/grupo, listas vacías, ítems largos y fallos; probar que una escritura usa el ID elegido y queda persistida tras recarga. E2E de UI con autenticación controlada solo en harness de pruebas; validación real de Entra/PKCE y APIs por separado en entorno integrado. Ningún bypass llega al bundle productivo.
- Evidencia por tarea: commit revisado, comando/fecha/entorno, escenarios ejecutados, número passed/failed/skipped y capturas/trazas cuando aplique. Cero skips en escenarios obligatorios; ausencia de tenant deja la validación integrada pendiente. SC-001/002 requieren usuarios piloto y porcentajes medidos; tests automáticos no los acreditan.
- La regresión ShoppingList registrada en quickstart falló (27/58); es evidencia histórica, no resultado actual. Repetir y resolver antes de cierre. No declarar la 007 completada con regresiones fallidas o cobertura E2E ausente.

## Integración de portada — 2026-10-27

FR-032 amplía la raíz para visitantes; el acceso autenticado y posterior al login conduce a /personal-lists. HomePage por componentes: cabecera con Drawer Fluent, hero, ejemplo local, pasos, escenarios, FAQ y cierre; CSS Module y estado React. Sin listeners globales. Tema compartido #006B68, Source Sans 3 local con licencia, superficies #FFFFFF/#FBFCF9, texto #172B3A/#526575 y divisores #D8E1E8. Espaciado de 4 px y controles 44 px. Texto y lista en dos columnas desde 1024 px, una columna móvil. La lista es protagonista, sin métricas ficticias. Se conservan los cinco destinos del menú compartido. Se sirven los TTF licenciados disponibles con font-display swap en vez de añadir conversión al build. Sin cambios de Entra, permisos o contratos. Validación de sesión/navegación y cinco anchos, teclado, Drawer, ejemplo y llegada al login.

## Extensión visual por sección — 2026-10-27 (FR-033)

| Sección | Estado observado | Adaptación |
| --- | --- | --- |
| Navegación | Cabecera provisional y enlaces en línea, sin menú compacto | Marca de portada, lateral a 1024 px, Drawer en móvil/tablet, selección y salto al contenido |
| Acceso | Main sin composición, botones juntos | Superficie de acceso centrada, introducción, botones apilables y estados existentes |
| Perfil | Tarjeta sin h1; carga/error fuera de una jerarquía estable | Cabecera persistente, datos en definición, superficie y acciones coherentes |
| Grupos | Formulario y tarjetas con estilos inline básicos | Cabecera contextual, formulario y listado con superficies y vacío/error claros |
| Listas personales | Página apilada, ítems como texto, selección poco destacada | Cabecera, selector destacado, filas continuas y progreso de ítems reales, formularios uniformes |
| Compartidas | Selector, creación modal y reparto con formatos independientes | Misma cabecera, contexto de grupo, diálogo y filas; mantener selección/roles |
| Catálogo | Filas sin divisores, MarketId libre y UUID visibles | Superficies de producto/mercados, selector por nombre, fechas/importes legibles en historial |
| Reparto | Formulario, fieldset y balances sin jerarquía común | Superficies de registro y resumen, controles táctiles y filas de balances/importes |
| Estados | Texto inline sin separación | Paneles compartidos con estados semánticos, separación y tipografía común |

Paleta: #006B68 marca, #FBFCF9 lienzo de portada, #FFFFFF superficie, #172B3A texto, #526575 secundario, #D8E1E8 borde. Fuente única Source Sans 3 local; escala 16/20/24/32/40, pasos 4/8/12/16/24/32/48. Usar tokens Fluent en componentes y CSS Modules compartidos para layout, paneles y formularios; nunca reglas globales indiscriminadas. Composición: cabecera/introducción, contenido de lectura y formularios; la información de compra protagoniza la página. Sin convertir todo en tarjetas decorativas. Mantener estado y lógica de negocio; los cambios de selector de mercado solo traducen IDs del contrato a nombres legibles. Harness visual solo en tests con MSAL/red controlados, fuera de entrada/bundle de producción. Validar cinco anchos, menú/teclado, diálogos, navegación y formularios; registrar fallos previos y límites de tenant real.

## Plan de UX privada — 2026-10-27

Implementar FR-034 conforme al recorrido documentado en design.md: componentes compartidos CreateListDialog, AddProductsDialog y PurchaseItems; índice personal y detalle separados por URL, ruta shared-lists/:listId y pestañas controladas por query. RHF+Zod para creación e incorporación; TanStack Query conserva datos/invalidaciones por cuenta/lista/grupo. Retorno desde catálogo restringido a detalles personales/compartidos internos. Conservar portada, MainLayout, MSAL y contratos. Revisar las Web Interface Guidelines obtenidas del repositorio oficial de Vercel el 2026-10-27: controles semánticos, nombres, teclado, foco, URL y estados anunciados; prevalecen spec, Fluent y sentence case español. Sin cambios backend ni despliegue.

## Corrección de identidad en marcado — 2026-10-27

La implementación del agregado usaba ProductId aunque el endpoint y comando declaraban ItemId. Cambiar únicamente la búsqueda de MarkItemAsPurchased a ListItem.Id; mantener sin cambios React y los contratos HTTP. La regresión debe atravesar POST de alta, consulta de /mine, POST de marcado y nueva consulta para comprobar persistencia, además de rechazo de IDs de catálogo y no afectación de otro ítem con identificador cruzado. T133 registra resultados; T132 de autorización no se cierra con esta corrección.
