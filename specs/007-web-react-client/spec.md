# Feature Specification: MVP-4 Cliente Web Full-Stack

**Feature Branch**: `007-web-react-client`

**Created**: 2026-10-23

**Status**: Draft

**Continuidad — 2026-10-28**: [008-web-ux-modernization](../008-web-ux-modernization/spec.md) especifica la evolución P0–P3 y coordina el cierre pendiente de esta feature. La [matriz de trazabilidad](../008-web-ux-modernization/traceability.md) separa deuda, ampliaciones y evidencia. No se modifica retrospectivamente el alcance ni se cierra ningún requisito por crear 008. Mis listas/destino por intención y desmarcado son cambios prospectivos; el comportamiento vigente se conserva hasta su entrega acreditada.

**Estado de entrega — 2026-10-27**: implementación parcial, sin cierre global ni autorización de despliegue. Portada y coherencia visual transversal implementadas; [design.md](design.md) es la referencia vigente. T120–T126 registran evidencia acotada; las verificaciones integradas y el backlog restante conservan su estado.

**Actualización de alcance — 2026-10-25**: el usuario solicita completar el acceso para agregar productos al catálogo desde la web, necesario para poder agregar ítems a sus listas. Se incorpora creación de producto con nombre y unidad de compra mediante el contrato backend existente, validación y errores, refresco del catálogo y regreso a la lista de origen. Esta decisión posterior sustituye la restricción histórica de «solo lectura» exclusivamente para el alta de productos; no incorpora edición/desactivación de productos ni administración de mercados o precios. Las menciones originales de solo lectura que siguen se conservan como contexto de la definición inicial.

**Input**: User description: "Crear la especificación 007-web-react-client como nuevo cliente Web full-stack de MiKompri, priorizado antes de MAUI, cumpliendo TP11 y reutilizando contratos backend existentes."

## Clarifications

### Session 2026-10-23

- Q: ¿Cuál es el stack frontend confirmado para esta iniciativa? → A: React + TypeScript + Vite + Fluent UI v9 (con React Router, TanStack Query, React Hook Form, Zod, Vitest, React Testing Library, MSW y Playwright).
- Q: ¿El cliente Web será SSR o SPA? → A: SPA; SSR/Next.js queda fuera de alcance en esta spec.
- Q: ¿Qué librerías y flujo de autenticación se confirman para Web? → A: MSAL React/MSAL Browser con Authorization Code Flow + PKCE y sin client secret en frontend.
- Q: ¿Se reabren en esta spec las decisiones TP11 de `(tid, oid)` y audience backend común? → A: No; son decisiones existentes, obligatorias y cerradas por constitución/proyecto.
- Q: ¿Qué cambios backend quedan definidos como prerrequisitos? → A: Migración de identidad en Users (`sub -> (tid, oid)`), autorización de listas personales en ShoppingList y JWT real en ProductCatalog.
- Q: ¿Qué alcance tiene ProductCatalog en el cliente Web? → A: Solo lectura (productos, mercados e historial de precios).
- Q: ¿Cuál es la gestión mínima de grupos para este MVP? → A: Crear grupo, listar grupos propios, ver membresías e invitar miembros cuando el backend lo permita; sin administración avanzada de roles.
- Q: ¿Qué comportamiento responsive mínimo se exige? → A: UX operable en mobile, tablet y desktop con layout adaptativo y navegación sin bloqueo funcional.
- Q: ¿Qué estados de carga/error/autorización deben estandarizarse? → A: loading, empty, error general y manejo explícito de 401/403/503 en vistas principales.
- Q: ¿Cuál es la estrategia de configuración local vs Azure? → A: Variables por ambiente, redirect URIs separadas local/cloud y configuración sin secretos en el bundle.
- Q: ¿Cuál es el objetivo de despliegue cloud para la SPA? → A: Azure Static Web Apps como objetivo preferente si resulta técnicamente compatible con la arquitectura final.
- Q: ¿Qué criterios verificables se fijan para accesibilidad y E2E? → A: Accesibilidad básica validable (teclado, foco visible, nombres accesibles) y cobertura E2E de flujos P1 críticos.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Acceso seguro y sesión del usuario (Priority: P1)

Como usuario de MiKompri, quiero iniciar sesión, restaurar mi sesión y cerrar sesión de forma segura para acceder a mis datos sin fricción y con control de identidad.

**Why this priority**: Sin autenticación y sesión estable no existe acceso funcional al resto del producto.

**Independent Test**: Se valida iniciando sesión con identidad válida, restaurando sesión tras recargar navegador y cerrando sesión, verificando estados autenticado/no autenticado y respuestas ante 401/403.

**Acceptance Scenarios**:

1. **Given** un usuario sin sesión activa, **When** inicia sesión con identidad válida de Microsoft Entra ID, **Then** la aplicación crea sesión local del cliente y habilita la navegación autenticada.
2. **Given** un usuario con sesión activa no expirada, **When** recarga la aplicación, **Then** la sesión se restaura sin requerir nuevo login.
3. **Given** un usuario autenticado, **When** cierra sesión, **Then** la sesión local se limpia y se bloquea el acceso a vistas protegidas.
4. **Given** un token inválido, expirado o con audience no válida, **When** se intenta acceder a recursos protegidos, **Then** la aplicación muestra estado no autorizado y guía de reautenticación.

---

### User Story 2 - Operación completa de listas personales y compartidas (Priority: P1)

Como usuario autenticado, quiero gestionar listas personales y compartidas con sus ítems para usar MiKompri como aplicación real de compras colaborativas.

**Why this priority**: Es el primer flujo full-stack de negocio que valida identidad, autorización y contratos de ShoppingList + Users.

**Independent Test**: Se valida creando, consultando y actualizando listas e ítems en contexto personal y de grupo, con usuarios autorizados y no autorizados.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado, **When** crea una lista personal y agrega ítems, **Then** la lista queda asociada a su identidad autenticada y se refleja en la vista personal.
2. **Given** un usuario miembro de un grupo, **When** crea o consulta una lista compartida del grupo, **Then** puede operar según su rol y membresía vigente.
3. **Given** un usuario sin membresía en el grupo, **When** intenta acceder a una lista compartida ajena, **Then** la aplicación muestra estado unauthorized sin exponer datos sensibles.
4. **Given** una respuesta de validación de negocio (400/409), **When** el usuario envía datos inválidos o en conflicto, **Then** la aplicación muestra mensajes accionables y conserva el contexto del formulario.

---

### User Story 3 - Reparto de gastos, balances y settlement (Priority: P2)

Como miembro de un grupo, quiero registrar gastos por ítem y consultar balances/settlement para cerrar cuentas colaborativas con transparencia.

**Why this priority**: Completa la propuesta de valor colaborativa de MVP-3 en un cliente usable por usuarios finales.

**Independent Test**: Se valida registrando gastos en listas compartidas y consultando balances y propuesta de settlement con distintos participantes.

**Acceptance Scenarios**:

1. **Given** una lista compartida con ítems comprados, **When** se registra quién pagó y participantes del gasto, **Then** el sistema refleja trazabilidad y reparto según contratos vigentes.
2. **Given** gastos registrados en una lista compartida, **When** se consulta el balance del grupo, **Then** se muestran totales pagados, adeudados y diferencia por participante.
3. **Given** balances con deudores y acreedores, **When** se consulta settlement, **Then** se presenta la propuesta de liquidación definida por backend.
4. **Given** una lista sin gastos válidos, **When** se consulta settlement, **Then** se muestra estado vacío claro sin error técnico.

---

### User Story 4 - Consulta y alta de productos del catálogo (Priority: P2)

Como usuario autenticado, quiero consultar productos, mercados e historial de precios y crear productos con nombre y unidad para poder agregarlos a mis listas. La edición/desactivación de productos y la gestión de mercados/precios quedan fuera de alcance.

**Why this priority**: Permite validar consumo cross-context de ProductCatalog con autenticación real y UX de consulta.

**Independent Test**: Se valida consultando catálogo, mercados e historial por producto, incluyendo casos de resultados vacíos y errores de conectividad.

**Acceptance Scenarios**:

1. **Given** usuario autenticado, **When** navega a catálogo, **Then** puede ver productos activos y su información principal en modo lectura.
2. **Given** un producto con historial, **When** solicita el historial de precios, **Then** obtiene la secuencia de precios por mercado y fecha.
3. **Given** un producto sin historial, **When** consulta precios históricos, **Then** la aplicación muestra estado empty sin fallo.
4. **Given** indisponibilidad temporal de backend (503), **When** se consulta catálogo, **Then** la aplicación comunica indisponibilidad y ofrece reintento.

---

### User Story 5 - Navegación, estados de interfaz y experiencia responsive accesible (Priority: P3)

Como usuario de escritorio, tablet o móvil, quiero una navegación clara con estados de carga, vacío, error y acceso denegado para completar tareas de forma consistente.

**Why this priority**: Sostiene la usabilidad transversal del cliente Web y reduce fricción operativa.

**Independent Test**: Se valida recorriendo navegación principal y verificando comportamiento visual/funcional en resoluciones desktop/tablet/mobile, incluyendo accesibilidad básica.

**Acceptance Scenarios**:

1. **Given** secciones principales de la aplicación, **When** el usuario navega entre ellas, **Then** conserva contexto y feedback de estado en cada vista.
2. **Given** datos en carga, vacíos o con error, **When** se renderizan pantallas, **Then** la UI muestra estados explícitos para loading, empty y error con acción de reintento cuando aplique.
3. **Given** interacción por teclado y lector de pantalla básico, **When** se usan formularios y navegación principal, **Then** el flujo es operable, con foco visible y nombres accesibles en controles interactivos clave.
4. **Given** ancho de pantalla móvil (<768px), tablet (768–1023px) o desktop (>=1024px), **When** se usan flujos clave, **Then** la interfaz mantiene legibilidad, navegación y acciones principales sin solapamientos ni pérdida de funcionalidad.

---

### Aceptación de portada y coherencia visual (US1/US5)

Los criterios siguientes complementan US1/US5 para FR-032/033:

- Visitante en `/`: ve la portada y un ejemplo temporal identificado como ficticio; no se hacen escrituras reales desde el ejemplo.
- Usuario autenticado en `/` o tras login/callback: llega a `/personal-lists`. Crear lista utiliza el formulario existente y exige sesión real; durante bootstrap se muestra carga.
- Al recorrer acceso, perfil, grupos, listas, catálogo, historial y reparto, se mantienen el tema, la jerarquía y los controles definidos en [design.md](design.md), incluso en carga/vacío/error.
- A 320/390/430/768/1280 px, contenido y acciones permanecen legibles sin scroll horizontal. Drawer, diálogo y formularios son operables por teclado con foco visible y retorno al disparador al cerrar.
- El menú autenticado conserva los cinco destinos y la protección de rutas. No se exponen «Vista», invitaciones o pagos simulados como funciones del producto.

### Edge Cases

- Token expira durante una operación de escritura en listas o gastos.
- Usuario pierde membresía de grupo mientras tiene una vista compartida abierta.
- Backend responde 401, 403, 404 o 409 para acciones iniciadas desde datos previamente visibles.
- ProductCatalog responde vacío por filtros o por inexistencia de historial.
- Error 503 intermitente durante polling o refetch de datos.
- Navegación directa a URL protegida sin sesión previa.
- Conflictos de edición concurrente en listas compartidas entre dos usuarios.

## Requirements *(mandatory)*

### Functional Requirements

### Requisitos de plataforma y seguridad (TP11)

- **FR-001**: El cliente frontend MUST usar React + TypeScript + Vite + Fluent UI v9 como stack base oficial de esta iniciativa.
- **FR-002**: El cliente Web MUST implementarse como SPA; SSR/Next.js MUST permanecer fuera de alcance en esta spec.
- **FR-003**: El cliente Web MUST autenticarse contra Microsoft Entra ID con un registro de aplicación independiente del resto de clientes.
- **FR-004**: La autenticación del cliente Web MUST usar MSAL React/MSAL Browser con Authorization Code Flow + PKCE y MUST NOT exponer client secret en frontend.
- **FR-005**: La identidad externa canónica `(tid, oid)` y la audience lógica común de backend MUST tratarse como decisiones existentes y cerradas; esta spec MUST NOT reabrirlas.
- **FR-006**: La aplicación MUST consumir `UserId` interno de MiKompri para operaciones de dominio y trazabilidad.

### Requisitos funcionales de producto

- **FR-007**: El sistema MUST permitir login, logout y restauración de sesión del usuario autenticado.
- **FR-008**: El sistema MUST mostrar y refrescar el perfil del usuario autenticado usando los contratos vigentes de Users.
- **FR-009**: El sistema MUST permitir gestión mínima de grupos: crear grupo, listar grupos del usuario y ver membresías. La invitación de miembros MUST habilitarse únicamente cuando exista contrato backend público versionado para invitaciones; en ausencia de dicho contrato, la UI MUST ocultar/deshabilitar la acción de invitar sin error técnico.
- **FR-010**: El sistema MUST permitir crear, consultar, actualizar y eliminar listas personales e ítems según reglas actuales de ShoppingList.
- **FR-011**: El sistema MUST permitir crear y operar listas compartidas de grupo respetando autorización por rol/membresía. El grupo MUST seleccionarse por nombre entre los grupos del usuario obtenidos de Users; `GroupId` es un dato interno del contrato, nunca un campo de texto que deba completar la persona. La selección se conserva en la URL para recarga e historial. Sin grupos se ofrece acceso a crear uno; un grupo ajeno/no disponible no habilita consultas ni creación desde la UI.
- **FR-012**: El sistema MUST permitir registrar gastos colaborativos, consultar balances y consultar settlement según contratos actuales. La interfaz denomina este flujo «Reparto de gastos»; pagador, comprador opcional y participantes se eligen por nombre desde miembros del grupo, sin introducir UserIds. El registro exige seleccionar un ítem real de la lista y conserva los datos ante errores; se prohíben IDs de ejemplo y campos ajenos al contrato backend.
- **FR-013**: El sistema MUST permitir consultar productos, mercados e historial y crear productos con nombre/unidad mediante el contrato existente, según la ampliación del 2026-10-25. La UI MUST validar, conservar valores ante error, bloquear duplicados pendientes y refrescar catálogo/selectores tras el alta. Edición/desactivación de productos y gestión de mercados/precios quedan fuera de alcance.
- **FR-014**: El sistema MUST implementar navegación principal para perfil, grupos, listas personales, listas compartidas y catálogo.
- **FR-015**: El sistema MUST manejar explícitamente estados loading, empty, error general, 401, 403 y 503 en vistas principales.
- **FR-016**: El sistema MUST manejar errores HTTP 400/401/403/404/409/503 con mensajes accionables y sin filtrar detalles sensibles.
- **FR-017**: El sistema MUST soportar comportamiento responsive en mobile (<768px), tablet (768–1023px) y desktop (>=1024px), manteniendo operables los flujos P1.
- **FR-018**: El sistema MUST cumplir accesibilidad básica verificable: navegación por teclado, foco visible y etiquetas/nombres accesibles en controles clave.
- **FR-032**: La raíz MUST mostrar una portada pública a visitantes y conducir a usuarios autenticados al índice personal. Crear lista MUST usar el flujo real protegido; el ejemplo local MUST estar identificado y separado de datos reales. El selector «Vista» y las simulaciones de autenticación, invitación y pago MUST quedar fuera del producto.
- **FR-033**: Todas las secciones web MUST compartir la dirección visual de portada: Source Sans 3, primario #006B68, superficies claras, espaciado base 4 px, objetivos táctiles mínimos 44 × 44 px, jerarquía y foco visible. La adaptación MUST preservar rutas, permisos, identidad, contratos y valores de formularios. [design.md](design.md) concreta la referencia visual vigente.
- **FR-034**: Los índices MUST priorizar seleccionar o crear una lista mediante diálogo breve. El detalle MUST conservar nombre, contexto, progreso y acción contextual de añadir productos; la selección permanece en URL. Compartidas MUST mantener Compra/Gastos en el mismo detalle y la pestaña en query. Se usan contratos reales, sin marcado compartido mientras no se acredite su autorización específica (T132), ni simulación de éxitos, desmarcado, invitaciones o pagos.

### Requisitos de arquitectura, configuración y pruebas

- **FR-019**: El frontend MUST organizarse por features dentro del monorepo MiKompri y MUST NOT depender de una estructura global centrada solo en Controllers/Services/Models.
- **FR-020**: La especificación funcional del cliente Web MUST ser reusable conceptualmente por futuros clientes MAUI e iOS en identidad, contratos y reglas de negocio.
- **FR-021**: El cliente Web MUST definir configuración por ambientes local y cloud mediante variables de entorno públicas de frontend, con redirect URI local/cloud y sin secretos en el bundle.
- **FR-022**: La solución MUST contemplar política CORS explícita para orígenes SPA local/cloud y una estrategia CI/CD entre local y Azure que incluya, como mínimo, lint/typecheck, tests y `docker build` conforme a TP4.
- **FR-023**: El cliente Web MUST incluir cobertura de pruebas unitarias/de componentes, pruebas de integración con mocks de red y pruebas E2E para flujos P1 críticos.

### Prerrequisitos backend bloqueantes para esta iniciativa

- **FR-024**: Antes de salida productiva del cliente Web, Users MUST completar migración de correlación `sub -> (tid, oid)` y ajustes de contratos dependientes.
- **FR-025**: Users MUST mantener `ExternalUserId` como legado nullable durante transición para compatibilidad de datos históricos.
- **FR-026**: Backend MUST mantener audience lógica común y validación homogénea entre APIs de MiKompri.
- **FR-027**: ProductCatalog MUST operar con autenticación JWT real alineada a TP11.
- **FR-028**: ShoppingList MUST aplicar autorización de listas personales basada en identidad autenticada.
- **FR-029**: `OwnerId` de listas personales y recursos derivados MUST resolverse desde la identidad autenticada, no desde entrada manipulable del cliente.

### Requisitos de despliegue futuro en Azure (sin implementación en este MVP local)

- **FR-030**: La solución MUST quedar preparada para despliegue futuro de SPA en Azure, con Azure Static Web Apps como objetivo preferente si resulta técnicamente compatible con la arquitectura final.
- **FR-031**: La especificación MUST contemplar preparación de despliegue en Azure Static Web Apps (ruteo SPA, artefactos y workflow de entrega) sin secretos en artefactos del frontend.

### Key Entities *(include if feature involves data)*

- **WebSession**: Estado autenticado del usuario en el cliente, incluyendo expiración y capacidad de restauración.
- **AuthenticatedUserProfile**: Vista del perfil local de MiKompri asociado a identidad externa canónica y `UserId` interno.
- **GroupSummary**: Representación de grupos y membresías del usuario para autorización funcional en listas compartidas.
- **PurchaseListView**: Proyección de lista personal o compartida con sus ítems, estado de compra y permisos disponibles.
- **SharedExpenseView**: Representación de gasto colaborativo por ítem con pagador, participantes, balance y settlement.
- **CatalogReadModel**: Proyección de consulta de productos, mercados e historial; el alta de producto usa un comando separado con nombre y unidad.
- **UiFeedbackState**: Estado de interfaz estándar para loading, empty, error y unauthorized.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Al menos 90% de usuarios piloto completan login y llegada al panel principal en menos de 2 minutos en primer uso.
- **SC-002**: Al menos 85% de usuarios piloto completan sin asistencia el flujo extremo a extremo "crear lista compartida + registrar gasto + consultar settlement".
- **SC-003**: El 100% de vistas principales muestran estados explícitos para loading, empty, error general, 401, 403 y 503 en pruebas de aceptación.
- **SC-004**: El 95% de respuestas HTTP 400/401/403/404/409/503 en flujos críticos se traducen a mensajes comprensibles y accionables para usuario final.
- **SC-005**: Los flujos P1 son operables sin bloqueos críticos en mobile (<768px), tablet (768–1023px) y desktop (>=1024px) durante validación funcional.
- **SC-006**: En revisión de accesibilidad básica, el 100% de vistas P1 cumple navegación por teclado, foco visible y nombre accesible en controles interactivos clave.
- **SC-007**: Existe cobertura E2E automatizada para el 100% de flujos P1 definidos (login/sesión, listas personales/compartidas y acceso autorizado/no autorizado).
- **SC-008**: La especificación deja documentados todos los prerrequisitos backend de identidad/seguridad y estos quedan trazables para planificación de implementación.

## Assumptions

- La iniciativa mantiene el estado de `006-android-maui-client` como planned/deferred y usa su alcance funcional como referencia, sin reutilizar decisiones específicas de tecnología móvil.
- El cliente Web se implementará como SPA en el monorepo MiKompri con stack objetivo aprobado: React, TypeScript, Vite, Fluent UI v9, React Router, MSAL React/Browser, TanStack Query, React Hook Form, Zod, Vitest, React Testing Library, MSW y Playwright.
- Los contratos backend de Users, ShoppingList y ProductCatalog seguirán siendo la fuente de verdad para reglas de negocio y autorización.
- El MVP local no requiere despliegue Azure inmediato, pero sí diseño compatible con despliegue cloud sin secretos en frontend.

## Dependencies

- `specs/001-project-baseline/spec.md` (prioridad de roadmap y orden de iniciativas).
- `specs/003-users-authentication/spec.md` (identidad, perfil, grupos y autorización por membresía).
- `specs/004-product-catalog/spec.md` (contratos de consulta y alta de productos; no se amplía el backend por el diseño web).
- `specs/005-shared-lists-settlement/spec.md` (reglas funcionales de listas compartidas, gastos, balances y settlement).
- `specs/006-android-maui-client/` (referencia funcional/arquitectónica cuando esté disponible; en este repositorio no existe aún artefacto consumible).
- `README.md` (estado del proyecto y dirección estratégica vigente).
- `.specify/memory/constitution.md` (cumplimiento obligatorio TP11).

## Out of Scope

- Implementación del cliente MAUI.
- Implementación del cliente iOS.
- SSR o migración a Next.js.
- OCR, scraping, notificaciones push, pagos bancarios, recomendaciones inteligentes.
- Soporte offline completo con sincronización.
- Administración avanzada de usuarios más allá de contratos backend actuales.
- Cambio de roles no soportado por backend actual.
- Implementación real de infraestructura Azure en esta fase de especificación.

## Referencias de diseño y evidencia

- [Diseño vigente](design.md): dirección visual, composición por sección y límites de entrega.
- [Plan](plan.md): decisiones técnicas e histórico separado.
- [Tareas](tasks.md): trazabilidad T086/T106–T126 y pendientes de cierre.
- [Quickstart](quickstart.md): validación visual controlada e integrada, con sus resultados separados.

## UX privada por lista — 2026-10-27

FR-034 amplía US2/US3. Aceptación: crear abre el ID devuelto; añadir conserva la lista elegida y los valores ante fallo; marcado personal solo cambia tras éxito y mantiene foco; gastos corresponde al mismo ID compartido; pestaña y grupo sobreviven a navegación por URL e historial; el regreso del catálogo conserva la lista y la pestaña permitida. El marcado compartido no se habilita mientras T132 esté pendiente. Composición y límites en [design.md](design.md).
