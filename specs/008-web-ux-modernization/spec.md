# Feature Specification: Modernización integral de la experiencia web

**Feature Branch**: identificador lógico `008-web-ux-modernization`; sin creación/cambio de rama Git.
**Created**: 2026-10-28
**Status**: Implementación local entregada; aceptación parcial. Integración Entra, revisión manual ampliada, piloto y presupuesto completo conservan sus gates pendientes.
**Input**: «Aprovecha lo existente y mantén las prácticas de Spec Kit; incluye todo el plan recomendado P0 al P3, todo lo necesario».

## Contexto y autoridad

Preparar, compartir y realizar una compra y entender sus gastos con experiencia móvil coherente con la portada. Esta iniciativa coordina deuda de [007](../007-web-react-client/spec.md) y mejoras nuevas; no declara cerrada la feature anterior. Reglas de negocio: [002](../002-shopping-list-core/spec.md), [003](../003-users-authentication/spec.md), [004](../004-product-catalog/spec.md), [005](../005-shared-lists-settlement/spec.md).

Se mantiene [TP11](../../.specify/memory/constitution.md): Entra ID, correlación `(tid, oid)` a UserId interno exclusivamente en Users mediante contratos públicos, nunca correlación global por `sub`. La identidad enviada por el navegador no acredita permisos.

P0–P3 son oleadas: autorización/trazabilidad, tareas completas, modernización y acreditación. Tras preparar el plan, el usuario autorizó ejecutar, comprobar y hacer los cambios necesarios. Se implementó y validó localmente lo documentado en [evidencia](evidence/validation.md); no se autoriza ni se realizó un despliegue cloud.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Operaciones autorizadas (Priority: P0)

Como usuario quiero que solo las personas autorizadas consulten y modifiquen mis compras.
**Why this priority**: No ampliar colaboración sobre permisos no acreditados.
**Independent Test**: Dos propietarios, dos grupos y miembros activos/retirados; comparar lecturas/escrituras con matriz de permisos.
**Acceptance Scenarios**:

1. **Given** una operación protegida, **When** alguien sin sesión o ajeno manipula sus IDs, **Then** no obtiene datos ni escribe.
2. **Given** membresía y rol permitido, **When** marca una compra compartida, **Then** persiste actor/fecha; con membresía retirada se rechaza.
3. **Given** T132 pendiente, **When** se corrige, **Then** pruebas personales y compartidas acreditan autorización; T133 no sustituye esta evidencia.

### User Story 2 — Completar y corregir compra personal (Priority: P1)

Como usuario quiero crear, consultar, renombrar/eliminar listas, añadir, editar/eliminar productos y marcar/desmarcar compras.
**Why this priority**: Completar el núcleo y recuperar errores cotidianos.
**Independent Test**: Operar sobre una de dos listas y verificar persistencia tras recarga sin alterar la otra.
**Acceptance Scenarios**:

1. **Given** lista/producto seleccionado, **When** cambio nombre, cantidad o estimación, **Then** solo cambia ese recurso y persiste.
2. **Given** un borrado, **When** cancelo, **Then** no cambia nada; al confirmar se elimina y vuelve el foco a un destino válido.
3. **Given** producto comprado por error, **When** desmarco, **Then** vuelve a pendiente y se actualiza progreso sin cambiar gastos.
4. **Given** varios productos, **When** se guarda uno, **Then** otro puede operarse independientemente sin duplicar escrituras del primero.

### User Story 3 — Grupo y compra compartida (Priority: P1)

Como miembro quiero ver personas/roles y operar las listas de mi grupo según permisos.
**Why this priority**: Hace visible la colaboración prometida.
**Independent Test**: Owner/Admin/Member y no miembro recorren grupo/lista con resultados según matriz 005.
**Acceptance Scenarios**:

1. **Given** mis grupos, **When** abro uno, **Then** veo miembros/roles por nombre, listas accesibles y creación si está permitida.
2. **Given** un producto compartido, **When** consulto autoría, **Then** veo persona disponible o «Persona no disponible», nunca autor inventado.
3. **Given** una lista/producto compartido, **When** edito/cierro/elimino según permisos, **Then** se preserva historia financiera; una eliminación incompatible se rechaza explicando cómo continuar.
4. **Given** ausencia de contrato público de invitación, **When** veo personas, **Then** no hay invitación ficticia ni campo de identificador interno para invitar.

### User Story 4 — Añadir sin salir (Priority: P1)

Como usuario quiero buscar un producto o crearlo e incorporarlo dentro de mi lista.
**Why this priority**: Evita abandonar la preparación hacia catálogo.
**Independent Test**: Personal y compartida, producto existente/nuevo y fallos intermedios.
**Acceptance Scenarios**:

1. **Given** una búsqueda, **When** existe producto, **Then** selecciono por nombre/unidad sin recorrer un selector extenso.
2. **Given** producto inexistente, **When** elijo «Crear y añadir», **Then** reviso nombre/unidad, cantidad y precio opcional sin perder contexto.
3. **Given** creación correcta e incorporación fallida, **When** reintento, **Then** reutilizo el producto creado; un resultado incierto se reconcilia antes de escribir otra vez.
4. **Given** duplicado/error, **When** se informa, **Then** conservo valores y puedo revisar el producto existente o reintentar apropiadamente.

### User Story 5 — Continuidad tras acceso/error (Priority: P1)

Como usuario quiero recuperar mi tarea tras autenticarme sin perder datos ni duplicar acciones.
**Why this priority**: Acceso y recuperación son parte del recorrido.
**Independent Test**: Enlace protegido, CTA de portada, expiración en formulario y errores de servicio.
**Acceptance Scenarios**:

1. **Given** detalle protegido con grupo/pestaña, **When** accedo, **Then** vuelvo al destino interno autorizado y su contexto.
2. **Given** «Crear una lista» en portada, **When** termino acceso, **Then** continúo creando; sin intención explícita entro en Mis listas.
3. **Given** borrador y sesión expirada, **When** reautentico la misma cuenta, **Then** puedo revisar/enviar una vez; otra cuenta no ve ese borrador.
4. **Given** validación, conflicto, acceso denegado o indisponibilidad, **When** falla, **Then** recibo explicación/recuperación útil sin detalles internos ni falso éxito.

### User Story 6 — Entender y corregir gastos (Priority: P1)

Como participante quiero registrar/corregir gastos permitidos y comprender quién debe cuánto.
**Why this priority**: Cierra la propuesta colaborativa.
**Independent Test**: Pagadores/participantes distintos, corrección/borrado por rol y consulta de resultado backend.
**Acceptance Scenarios**:

1. **Given** nuevo gasto, **When** preparo, **Then** selecciono producto real y personas por nombre; pagador actual se propone solo si es válido y participantes requieren revisión explícita.
2. **Given** confirmación backend, **When** consulto, **Then** balance/propuesta se actualizan sin cálculo financiero alternativo en cliente.
3. **Given** gasto existente, **When** edito/elimino, **Then** se comprueban permisos, se confirma borrado y queda trazabilidad.
4. **Given** miembros retirados, **When** consulto historia, **Then** sus importes permanecen y no se seleccionan para nuevos gastos.
5. **Given** resumen, **When** leo, **Then** distingo estimación, pagado y deuda, sin confundir propuesta con transferencia real.

### User Story 7 — Interfaz coherente y accesible (Priority: P2)

Como usuario quiero encontrar mis listas y comprar con una mano, teclado o lector, con la lógica mostrada en portada.
**Why this priority**: Convertir capacidades en experiencia reconocible.
**Independent Test**: Mismas tareas en prototipo/producto, móvil primero, tablet/escritorio y estados.
**Acceptance Scenarios**:

1. **Given** usuario recurrente, **When** entra, **Then** encuentra listas y filtros personales/compartidas, grupos, productos y cuenta.
2. **Given** ejemplo público, **When** interactúa, **Then** solo prueba capacidades entregadas, con datos ficticios y sin escrituras reales.
3. **Given** compra, **When** marca, **Then** foco/orden no saltan inesperadamente y puede revisar pendientes/comprados.
4. **Given** tamaños 320/390/430/768/1280 y fronteras, **When** usa menús/formularios, **Then** no hay desbordamiento/oclusión; funcionan teclado, foco, zoom y lector.

### User Story 8 — Acreditar calidad antes de cierre (Priority: P3)

Como responsable quiero evidencia reproducible de funcionamiento, accesibilidad, carga y éxito humano.
**Why this priority**: Apariencia o pruebas controladas no demuestran aceptación integrada.
**Independent Test**: Guía con servicios reproducibles, identidades reales de prueba y piloto, evidencias separadas.
**Acceptance Scenarios**:

1. **Given** candidato, **When** verifico obligatorios, **Then** no se aprueban por omisión o ausencia de tests.
2. **Given** piloto, **When** analizo, **Then** registro muestra, intentos, tiempos, ayudas y errores sin adjudicar éxitos no medidos.
3. **Given** pantalla frecuente, **When** comparo carga antes/después bajo mismo perfil, **Then** la mejora se mide sin ocultar funcionalidad.

### Edge Cases

- Doble envío, respuesta perdida tras escritura, refetch fallido tras éxito, recarga durante petición.
- Cambio de cuenta, varias pestañas, destino externo/manipulado, callback sin intención, logout con borrador.
- Retirada de membresía/rol, edición concurrente y recurso eliminado por otra persona.
- Catálogo vacío/inactivo, duplicado nombre/unidad, producto creado sin incorporación.
- Nombres largos/ausentes, precio desconocido/cero, decimal español y cantidades fraccionarias no admitidas.
- Lista cerrada, producto con gastos, redondeo, antiguos participantes y entrada directa a Gastos.
- Teclado virtual, safe areas, zoom 200%, movimiento reducido y foco tras borrado.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Cada deuda 007 y cambio nuevo MUST tener propietario documental, tareas y aceptación trazables, sin cierre por sustitución documental.
- **FR-002**: Toda lectura/escritura de listas, productos de lista y gastos MUST validar identidad y autorización por recurso en servidor, independientemente de UI.
- **FR-003**: Colaboración MUST respetar rol/membresía/autoría de 005, preservar actor/fecha y rechazar manipulación de IDs entre grupos.
- **FR-004**: El usuario MUST crear, consultar, renombrar y eliminar listas personales con confirmación y recuperación.
- **FR-005**: El usuario MUST añadir, consultar, editar cantidad/estimación y eliminar productos conforme a permisos/reglas vigentes.
- **FR-006**: El usuario MUST marcar/desmarcar productos personales/compartidos autorizados; desmarcar no altera gastos. Ampliación expresa del alcance anterior.
- **FR-007**: Operaciones por producto MUST tener pending independiente; progreso/éxito MUST corresponder a resultados acreditados, reconciliando respuestas inciertas.
- **FR-008**: Grupos MUST mostrar miembros/roles por nombre y enlaces a listas; gestión compartida MUST aplicar 005 y explicar restricciones por historia financiera.
- **FR-009**: Autoría/historia MUST mostrarse cuando estén disponibles; retirados MUST conservar historia y excluirse de nuevas selecciones.
- **FR-010**: Añadir MUST buscar productos o crearlos por nombre/unidad dentro de la lista, conservando cantidad/estimación; desconocido MUST distinguirse de cero real.
- **FR-011**: Crear en catálogo e incorporar MUST ser pasos recuperables independientes: reutilizar creado, resolver duplicados y confirmar éxito completo solo al incorporar.
- **FR-012**: Acceso MUST conservar destino interno permitido, grupo/pestaña e intención de creación; descartar destinos externos/ajenos/inválidos.
- **FR-013**: Borradores MUST aislarse por cuenta/lista y sobrevivir reautenticación en esa pestaña, sin credenciales ni reenvío automático; limpiar en logout/cambio de cuenta.
- **FR-014**: Vistas MUST distinguir carga/vacío/error/acceso restringido y recuperación para 400/401/403/404/409/503/red, conservar valores y evitar detalles sensibles.
- **FR-015**: Perfil MUST permitir releer datos y sincronizar explícitamente con proveedor, diferenciando ambas acciones; sin ampliar edición de perfil.
- **FR-016**: Gastos MUST seleccionar producto/personas válidas por nombre, conservar datos ante fallo y revisar participantes explícitamente antes de enviar.
- **FR-017**: Gastos MUST permitir corregir/eliminar según rol, con confirmación/trazabilidad y cálculos/redondeo/propuestas de la autoridad existente.
- **FR-018**: Reparto MUST distinguir estimación/pagado/balance, expresar deuda por persona y aclarar que no ejecuta pagos.
- **FR-019**: Entrada autenticada MUST priorizar Mis listas con filtros personales/compartidas y acceso a grupos/productos/cuenta; URLs antiguas MUST funcionar.
- **FR-020**: Portada/interior MUST compartir lenguaje y componentes de compra; ejemplo ficticio MUST permanecer aislado y limitarse a funciones entregadas.
- **FR-021**: Presentación MUST priorizar productos/cantidades/acciones/progreso y no desplazar inesperadamente fila/foco al marcar.
- **FR-022**: Navegación/filtros/selección/grupo/pestaña MUST recuperarse en enlaces, recarga y Atrás/Adelante; barras fijas no ocultarán controles.
- **FR-023**: Flujos MUST funcionar primero en móvil, con teclado/lector, foco, zoom 200%, contraste medido y objetivos 44 × 44 px.
- **FR-024**: Mejora de carga MUST medirse antes/después con misma configuración, sin eliminar funciones y permitiendo diferir recursos no necesarios al inicio.
- **FR-025**: Cierre MUST exigir pruebas de dominio/aplicación/API afectados, UI e integración real separada de harness, sin skips obligatorios.
- **FR-026**: Cierre MUST registrar piloto/métricas; calidad documental/tests MUST NOT presentarse como satisfacción humana.
- **FR-027**: Solución MUST preservar TP11; dominio/permisos no se inferirán de nombres ni IDs externos suministrados por cliente.
- **FR-028**: Invitaciones, pagos, administración de roles, edición/desactivación de catálogo, mercados/precios, OCR, push y offline completo MUST quedar fuera; sin simulaciones.

### Key Entities *(include if feature involves data)*

- **Lista/Producto**: identidad, contexto, nombre, estado, cantidad, estimación y autoría.
- **Membresía/Permiso**: persona interna, grupo, rol y vigencia, autoridad del dominio.
- **Gasto/Reparto**: producto, personas, importe real, historia y propuesta que no ejecuta pagos.
- **Intención/Borrador**: destino permitido y valores de tarea aislados por cuenta/lista.
- **Evidencia**: requisito, entorno, fecha, resultado, omisiones, límites y medición humana.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de casos no autorizados de la matriz impide exposición/escritura y no altera recursos.
- **SC-002**: El 100% de recorridos crear/corregir/eliminar/marcar/desmarcar persiste tras recarga sin alterar otra lista.
- **SC-003**: Al menos 9 de 10 pilotos acceden y llegan a su tarea en menos de 2 minutos sin ayuda; intervención externa cuenta como no completado.
- **SC-004**: Al menos 9 de 10 pilotos completan sin asistencia crear lista compartida, añadir producto nuevo, registrar gasto y consultar reparto.
- **SC-005**: Al menos 9 de 10 pilotos identifican quién debe cuánto/a quién y distinguen propuesta de transferencia real.
- **SC-006**: El 100% de flujos críticos funciona por teclado, tamaños definidos y zoom 200%, con cero defectos bloqueantes de lector/foco/contraste.
- **SC-007**: El 100% de escenarios obligatorios de estado/recuperación se ejecuta; al menos 95% de mensajes evaluados comunica causa y siguiente acción correctas.
- **SC-008**: En perfil móvil reproducible del plan, p75 de carga principal ≤2,5 s, respuesta a interacción ≤200 ms y desplazamiento acumulado ≤0,1, con todos los resultados publicados.
- **SC-009**: El 100% de requisitos tiene aceptación/evidencia antes de cierre; no hay skips obligatorios ni tests vacíos presentados como cobertura.

## Assumptions

- Se mantiene stack/autenticación 007; no reinstalar Spec Kit ni enmendar constitución.
- Identidades/servicios de prueba son prerrequisitos de ejecución; sin ellos P3 sigue pendiente, sin simular autenticación de producto.
- EUR y cantidades enteras positivas; no fracciones/multidivisa nuevas.
- Desmarcar solo cambia compra; gastos se corrigen explícitamente.
- Contratos nuevos se implementan/verifican antes de activar controles, diferenciados de existentes en [contratos](contracts/api-and-permissions.md).
- Piloto de al menos diez personas representativas, conservando resultados previos y repitiendo cuando cambios afecten resultados.

## Dependencies and scope changes

Destino por intención y Mis listas sustituyen prospectivamente 007 FR-014/032 al entregar US5/US7; hasta entonces rige su comportamiento actual. Desmarcado y precio desconocido amplían 002/007 con contrato compatible, sin reinterpretar ceros existentes. [Trazabilidad](traceability.md) separa deuda/ampliación/verificación. 007 mantiene gates hasta evidencia individual; 008 no la declara completada.
