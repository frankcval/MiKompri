# Feature Specification: 011 — Web Iconography & Fridge Notes

**Feature Branch**: `011-web-iconography`
**Created**: 2026-10-31
**Status**: Implementación en validación — diseño ratificado; cierre sujeto a evidencia y gates pendientes.
**Input**: React Icons, una experiencia fluida/intuitiva/actual inspirada en papeles, notas y pizarra de la nevera para listas de compra. Primero especificar y fijar diseño; implementación posterior.

## Contexto y alcance

Base `main`9597af5, PR59 integrado y verificado por ancestry.007/008/009 y010-A ya implementadas; sus tareas/gates pendientes no se completan por011. [Diseño](design.md), [mapa de iconos](icon-map.md), [research](research.md), [auditoría](evidence/current-state.md).

La dirección se denomina **Nevera compartida**: superficies claras, listas como notas legibles, acento de pizarra verde y controles accesibles. Es una metáfora visual; no añade notas libres, pinning, arrastre, dibujo ni un nuevo modelo de tablero. React Icons/Phosphor sustituye la preferencia preliminar por Fluent Icons en scope.md por la petición actual; Fluent UI sigue siendo base de componentes. No reemplazar la arquitectura ni el logo de marca.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Reconocer dónde estoy (Priority: P1)

Como persona que compra desde el móvil, identifico Mis listas, Grupos y Productos y veo la sección seleccionada sin adivinar iconos.

**Why this priority**: la navegación es común a todos los recorridos.
**Independent Test**: recorrer las tres rutas con toque y teclado a320/390/430; icono y etiqueta coherentes, estado seleccionado semántico y visual.
**Acceptance Scenarios**:
1. **Given** Mis listas activo, **When** navego a Productos, **Then** cambia la ruta existente y el icono/etiqueta/aria-current coinciden; sin depender solo de color.
2. **Given** detalle personal/compartido, **Then** Mis listas sigue identificado como sección padre; Mi cuenta conserva acceso propio.
3. **Given** teclado o lector, **Then** enlaces/botones conservan sus nombres y foco; SVG decorativos no añaden ruido al árbol accesible.

### User Story 2 — Entender mis notas de compra (Priority: P1)

Como persona que organiza compras, distingo mis listas activas e historial y entiendo pendientes/estimación/contexto en una superficie que recuerda una nota de nevera.

**Why this priority**: traduce la inspiración en el núcleo del producto, preservando el comportamiento009.
**Independent Test**: usar Mis listas y sus filtros/orden actuales con listas personales/compartidas, nombres largos y precios desconocidos.
**Acceptance Scenarios**:
1. **Given** listas cargadas, **Then** cada nota muestra nombre, contexto, pendientes y estimación parcial con productos sin precio; no inventa datos ni totales.
2. **Given** selecciono Historial/orden/filtro, **Then** usa los mismos query params y reglas existentes; una100%comprada sigue fuera de Activas y accesible en Historial.
3. **Given** vacío/error/carga parcial, **Then** el diseño explica el estado y ofrece la acción existente; no presenta una nevera vacía como resultado válido si faltan datos.

### User Story 3 — Comprar sin fricción (Priority: P1)

Como persona en el supermercado, leo productos y marco la compra con una mano, sin que la decoración o el movimiento interfieran.

**Why this priority**: marcar/comprar es el núcleo irrenunciable.
**Independent Test**: abrir detalle, marcar/desmarcar, añadir/buscar producto y navegar a datos de compra/Gastos con los mismos contratos.
**Acceptance Scenarios**:
1. **Given** un ítem, **When** marco/desmarco, **Then** conserva checkbox/label/foco y lógica de historial; el estado incluye señal visual y texto/semántica, no solo tinta atenuada.
2. **Given** nombre largo, precio desconocido o100ítems, **Then** texto envuelve, controles no se solapan y la barra inferior no tapa contenido ni teclado virtual.
3. **Given** movimiento reducido, **Then** no hay animaciones de notas volando, giros o compra obligatoria animada; la operación sigue comprensible.

### User Story 4 — Usar acciones claras y seguras (Priority: P1)

Como usuario de toque/teclado/lector, identifico buscar, añadir, editar y borrar; las ayudas emergentes complementan acciones reconocibles.

**Independent Test**: activar acciones por teclado/toque; verificar nombre, tooltip y confirmación de borrado existente.
**Acceptance Scenarios**:
1. **Given** icon-only, **When** foco/hover, **Then** nombre accesible y tooltip legible; Escape cierra la ayuda sin mover foco. En toque hay contexto textual/menú, sin depender de hover.
2. **Given** Borrar lista, **Then** permanece en su ubicación/confirmación existente con verbo y objeto; no es indistinguible de editar ni se vuelve gesto exclusivo.
3. **Given** error/loading/disabled, **Then** no se usa tooltip como único mensaje de error o requisito; texto visible explica lo necesario.

### User Story 5 — Percibir una interfaz coherente (Priority: P2)

Como usuario habitual, encuentro el mismo lenguaje de iconos, espaciado, colores y foco en listas, catálogo/mercados, grupos y perfil sin reaprender funciones.

**Independent Test**: revisar matriz de rutas actuales en móvil/escritorio y comprobar el mapa semántico y tokens compartidos.
**Acceptance Scenarios**:
1. **Given** mismo significado, **Then** usa el mismo icono/etiqueta; Regular/Fill se reserva a selección/estado definido, no a variaciones decorativas.
2. **Given** login/logout/cambio Google↔Microsoft, **Then** botones010-A, rutas, perfil y aislamiento permanecen; no se restaura configuración antigua ni se cambia trust.
3. **Given** un futuro cliente MAUI, **Then** puede reutilizar significado/etiquetas/tamaños de toque; no requiere dependencias React ni una implementación MAUI en011.

### Edge Cases

320px, zoom200%/400%, tamaños de texto del sistema, traducción/texto largo, listas de100ítems, cero precio vs precio desconocido, total parcial, error parcial de compartidas, todas compradas/desmarcado posterior, hover imposible, Escape con menú/dialog abierto, focus restoration, icono no mapeado, forced-colors, reduced-motion, conexión lenta y usuario autenticado de otro proveedor. Ninguno añade nuevas reglas de negocio.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001** MUST conservar las rutas y tres destinos principales actuales; icono+etiqueta en navegación, aria-current y estado seleccionado diferenciados.
- **FR-002** MUST usar una familia de iconos de aplicación: React Icons/Phosphor, con nombres verificados; no instalar/usar paquetes adicionales por conveniencia.
- **FR-003** MUST centralizar mapa semántico y tamaños/colores por token; no strings de icono del servidor, imports de namespace/dinámicos ni catálogos completos en runtime.
- **FR-004** MUST aplicar Regular/Fill solo según el mapa; controles internos de Fluent y logos de marca/proveedor son excepciones explícitas, no familias app añadidas.
- **FR-005** MUST dar nombre accesible y tooltip a icon-only; SVG decorativos aria-hidden y no focusables. Icono no sustituye el nombre de un botón.
- **FR-006** MUST mantener objetivos táctiles>=44x44CSSpx; glyph20/24px dentro del control, sin comprimirlo por densidad.
- **FR-007** MUST mantener texto visible en acciones primarias, navegación, Cerrar sesión y acciones destructivas/confirmación; tooltips no contienen instrucciones esenciales ni errores exclusivos.
- **FR-008** MUST preservar teclado/foco/restauración, Escape, contraste de texto/control, forced-colors y nombres de estados. No afirmar cumplimiento solo por usar Fluent.
- **FR-009** MUST representar listados como notas y una cabecera de pizarra según design.md, sin texturas pesadas, texto inclinado, manuscrito obligatorio ni adornos que parezcan acciones.
- **FR-010** MUST conservar MiKompri Sans, marca y acento teal existentes; definir tokens de papel/tinta/pizarra/limón, superficies y espaciado compartidos.
- **FR-011** MUST mantener lectura de nombre/contexto/pendientes/estimación parcial/fecha y estado activo/historial; precios sin dato no se convierten en0.
- **FR-012** MUST conservar filtros/orden/query params y navegación profunda de009; representación100%comprada y desmarcado no cambian.
- **FR-013** MUST respetar checkbox/formularios/diálogos nativos o Fluent y todos los contratos de compra, Gastos, producto, mercado e historial; no añadir endpoints ni modificar cálculo de importes.
- **FR-014** MUST limitar movimiento nuevo a feedback de acciones100–160ms y desactivarlo con reduced-motion; no loops decorativos, drag obligatorio ni nueva librería de animación.
- **FR-015** MUST soportar320/390/430/768/1280 y escritorio mayor, reflow/zoom y safe-area/teclado; sin scroll horizontal ni acciones ocultas por barra inferior.
- **FR-016** MUST tratar vacíos/loading/error/parcial como contenido funcional visible; iconos ayudan y no reemplazan mensajes existentes.
- **FR-017** MUST reutilizar Fluent Button/Tooltip/Menu/Dialog/Checkbox y tokens; sin reescritura de componentes accesibles desde cero.
- **FR-018** MUST incluir prueba de exports reales, licencia y manifest del paquete fijado antes de instalarlo durante implementación; medir tree-shaking/bundle, no prometer tamaño por intuición.
- **FR-019** MUST conservar budgets008/009: protocolo20 muestras/ruta, LCPp75<=2500ms, interacción<=200ms, CLS<=0.1. Incumplimientos heredados se documentan; no se rebajan ni se cierran tareas anteriores por esta feature.
- **FR-020** MUST preservar login Google/legacy Microsoft, D1, UserId, cachés/drafts/return intent; ningún cambio de configuración/identidad/BD por iconografía.
- **FR-021** MUST separar significado/presentación en un catálogo reutilizable por MAUI, sin entregar MAUI ni assets de otras marcas como iconos genéricos.
- **FR-022** MUST adoptar diseño por fases con capturas antes/después y reversión solo de capa visual; no restaurar.env.local ni retirar funciones para conseguir un mockup.

### Key Entities

Solo presentación: SemanticIcon, IconTreatment, SurfaceToken y ScreenVisualState. No nuevas entidades de dominio, columnas, preferencias persistidas, migraciones EF ni APIs. [Modelo](data-model.md).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**100% de iconos app de navegación/acciones del alcance aparecen en icon-map.md con significado, export real, estado y etiqueta;0 imports de familias no aprobadas ni emoji funcionales nuevos.
- **SC-002**100% de icon-only tienen nombre accesible, tooltip y target>=44x44; foco/hover/Escape y uso táctil verificados,0 infracciones axe nuevas.
- **SC-003** Las cinco historias pasan en320/390/430 y escritorio; rutas/queries/checkbox/destructivas/auth mantienen comportamiento y regresiones pasan.
- **SC-004** Parejas de color de texto>=4.5:1 y contornos/foco necesarios>=3:1; pruebas forced-colors/zoom/teclado/reduced-motion documentadas, sin fingir lector manual.
- **SC-005** Capturas implementadas se comparan con las tres referencias aprobadas, mostrando papel/pizarra/jerarquía y sin nuevos gestos o funciones; estados límite documentados.
- **SC-006** Bundle/imports y métricas cumplen protocolo/budgets existentes; lint/typecheck/build y suites Web/CI del HEAD pasan. Modelo backend permanece sin cambios.

## Assumptions, Dependencies & Out of Scope

React Icons se interpreta literalmente como react-icons; una sola familia Phosphor y Fluent UI existente para controles. La instrucción del titular de pasar a la siguiente fase ratifica la dirección visual y ADR-005 el31/10/2026; la aceptación de la implementación se verifica por separado en evidencia. MAUI reutiliza semántica después.

Depende de007(shell/Fluent/MSAL),008(UX/roles/foco/budgets),009(listas/historial/precios/compra) y010-A(auth/perfil/sesión). No cambia sus tareas ni reabre010-B.012/invitaciones, chat, notas libres, arrastrar/pinear listas, OCR/fotos, offline nuevo, temas personalizables/dark-mode global, fuente nueva, rebranding/logos, rediseño completo de landing, pagos, nuevos proveedores/backend/migraciones y despliegue quedan fuera.
